using System.Text.Json;
using WordMasterMind.Library.Enumerations;

namespace WordMasterMind.Library.Models;

public partial class LiteralDictionary
{
    /// <summary>
    ///     This constructor creates a list of words from a JSON file with an array of strings containing the words
    ///     It will get passed through FillDictionary and then the standard constructor
    /// </summary>
    /// <param name="pathToDictionaryJson"></param>
    public static LiteralDictionary NewFromJson(string pathToDictionaryJson)
    {
        return new LiteralDictionary(
            words: JsonSerializer
                .Deserialize<string[]>(
                    json: File.ReadAllText(path: pathToDictionaryJson),
                    options: JsonSerializerOptions) ?? throw new InvalidOperationException());
    }

    public static LiteralDictionary NewFromSource(LiteralDictionarySource source)
    {
        return source.FileType switch
        {
            LiteralDictionaryFileType.TextWithNewLines => new LiteralDictionary(
                words: File.ReadLines(path: source.FileName)),
            LiteralDictionaryFileType.JsonStringArray => NewFromJson(pathToDictionaryJson: source.FileName),
            LiteralDictionaryFileType.Binary => Deserialize(inputFilename: source.FileName),
            _ => throw new Exception(message: "Unknown file type")
        };
    }
}