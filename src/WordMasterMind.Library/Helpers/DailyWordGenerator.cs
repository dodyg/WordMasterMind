using WordMasterMind.Library.Models;

namespace WordMasterMind.Library.Helpers;

/// <summary>
///     The idea is to seed a PRNG with a known value and then use that to generate a known sequence of words from the
///     dictionary
///     or possibly to otherwise create a hash function that turns a given calendar date into a specific word from the
///     dictionary
/// </summary>
public static class DailyWordGenerator
{
    /// <summary>
    ///     The Official WordMasterMind Word of the Day seed*
    ///     *) probably should be changed to a random value
    /// </summary>
    public const int Seed = 0xBEEF;

    /// <summary>
    ///     WordMasterMind's Birthday, Puzzle number 1 is this day
    /// </summary>
    public static readonly DateTime WordGeneratorEpoch = new(
        year: 2022,
        month: 1,
        day: 22);

    public static int PuzzleNumber(DateTime? date = null)
    {
        date = date ?? DateTime.Now;
        return (int)Math.Floor(d: date.Value.Subtract(value: WordGeneratorEpoch).TotalDays) + 1;
    }

    public static string WordOfTheDay(int length = Constants.StandardLength, DateTime? date = null,
        LiteralDictionary? dictionary = null)
    {
        dictionary = dictionary ?? new LiteralDictionary();
        var rnd = new Random(Seed: Seed);
        // calculate the days since the creation of the game

        // skip that many entries in the randomizer seed to get to that day's index
        var wordIndex = 0;
        for (var skips = 0; skips < PuzzleNumber(date: date); skips++)
            wordIndex = rnd.Next(
                minValue: 0,
                maxValue: dictionary.WordCountForLength(length: length) - 1);

        return dictionary.WordAtIndex(
            length: length,
            wordIndex: wordIndex);
    }
}