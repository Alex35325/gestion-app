using System;

namespace GestionApp.Services;

/// <summary>
/// Matches the web app's uid()/todayStr() so ids stay in the same shape
/// whether a row was created from the website or from this desktop app.
/// </summary>
public static class Ids
{
    private static readonly Random Rng = new();
    private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyz";

    public static string NewId()
    {
        var ms = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return ToBase36(ms) + "-" + RandomBase36(7);
    }

    public static long NowMs() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public static string TodayStr() => DateTime.Now.ToString("yyyy-MM-dd");

    private static string ToBase36(long value)
    {
        if (value == 0) return "0";
        var chars = new System.Text.StringBuilder();
        while (value > 0)
        {
            chars.Insert(0, Alphabet[(int)(value % 36)]);
            value /= 36;
        }
        return chars.ToString();
    }

    private static string RandomBase36(int length)
    {
        var chars = new char[length];
        for (var i = 0; i < length; i++)
            chars[i] = Alphabet[Rng.Next(Alphabet.Length)];
        return new string(chars);
    }
}
