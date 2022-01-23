using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Text.Json;

namespace WordMasterMind.Models;

public class ScrabbleDictionary
{
    private readonly ImmutableDictionary<int, IEnumerable<string>> _wordsByLength;

    public ScrabbleDictionary(string pathToDictionaryJson) : this(
        words: JsonSerializer.Deserialize<string[]>(json: string.Join(separator: "\n",
            value: File.ReadAllLines(path: pathToDictionaryJson))) ?? throw new InvalidOperationException())
    {
    }

    public ScrabbleDictionary(Dictionary<int, IEnumerable<string>>? dictionary = null)
    {
        if (dictionary is not null)
        {
            this._wordsByLength = dictionary.ToImmutableDictionary();
        }
        else
        {
            var result = Task.Run(function: async () => await LoadDictionaryFromWebJson());
            result.Wait();
            this._wordsByLength = result.Result.ToImmutableDictionary();
            if (this._wordsByLength.Count == 0) throw new Exception(message: "Dictionary could not be loaded");
        }
    }

    public ScrabbleDictionary(IEnumerable<string> words) : this(dictionary: FillDictionary(words: words))
    {
    }

    private static Dictionary<int, IEnumerable<string>> FillDictionary(in IEnumerable<string> words)
    {
        var dictionary = new Dictionary<int, IEnumerable<string>>();
        foreach (var word in words)
        {
            var wordLength = word.Length;
            if (dictionary.ContainsKey(key: wordLength))
                dictionary[key: wordLength] = dictionary[key: wordLength].Append(element: word.ToUpperInvariant());
            else
                dictionary.Add(key: wordLength,
                    value: new[] {word.ToUpperInvariant()});
        }

        return dictionary;
    }

    private static async Task<Dictionary<int, IEnumerable<string>>> LoadDictionaryFromWebJson()
    {
        var dictionaryWords =
            await new HttpClient().GetFromJsonAsync<string[]>(requestUri: "/scrabble-dictionary.json");
        if (dictionaryWords is null || !dictionaryWords.Any())
            throw new Exception(message: "Dictionary could not be retrieved");

        return FillDictionary(words: dictionaryWords);
    }


    public bool IsWord(string word)
    {
        var length = word.Length;
        return this._wordsByLength.ContainsKey(key: length) &&
               this._wordsByLength[key: length].Contains(value: word.ToUpperInvariant());
    }

    public string GetRandomWord(int minLength, int maxLength)
    {
        if (minLength > maxLength || maxLength < minLength)
            throw new ArgumentException(message: "minLength must be less than or equal to maxLength");

        var random = new Random();
        var maxTries = 1000;
        var triedIndexes = new List<int>();
        while (maxTries-- > 0)
        {
            var length = random.Next(minValue: minLength,
                maxValue: maxLength);
            IEnumerable<string> wordsForLength;
            if (!this._wordsByLength.ContainsKey(key: length) ||
                !(wordsForLength = this._wordsByLength[key: length]).Any()) continue;
            var forLength = wordsForLength as string[] ?? wordsForLength.ToArray();

            while (true)
            {
                var indexToTry = random.Next(minValue: 0,
                    maxValue: forLength.Length);
                // if we've already used this word, try another index
                if (triedIndexes.Contains(item: indexToTry))
                {
                    // check if words of this size expired
                    if (triedIndexes.Count == forLength.Length) break;
                    // otherwise keep trying
                    continue;
                }

                // add to the list of tried index
                triedIndexes.Add(item: indexToTry);
                // return the word that was not in the dictionary
                return forLength
                    .ElementAt(index: indexToTry);
            }
        }

        throw new Exception(message: "Dictionary doesn't seem to have any words of the requested parameters");
    }

    public string FindWord(in char[] knownCharacters, int maxIterations = 1000, IEnumerable<string>? skipWords = null)
    {
        var skipWordsArray = skipWords is null ? null : skipWords.ToArray();
        while (maxIterations-- > 0)
        {
            var word = this.GetRandomWord(
                minLength: knownCharacters.Length,
                maxLength: knownCharacters.Length);
            if (skipWordsArray is not null && skipWordsArray.Contains(value: word)) continue;

            var allMatch = true;
            for (var i = 0; i < knownCharacters.Length; i++)
            {
                if (knownCharacters[i] == '\0' || knownCharacters[i] == ' ') continue;

                if (word[index: i] != knownCharacters[i])
                {
                    allMatch = false;
                    break;
                }
            }

            if (allMatch)
                return word;
        }

        throw new Exception(message: "Number of iterations exceeded without finding a matching word");
    }
}