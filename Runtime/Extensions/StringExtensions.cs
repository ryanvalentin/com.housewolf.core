using System;

public static class StringExtensions
{
    /// <summary>
    /// Creates a reproducible numeric hash from a string (assuming no difference in casing/culture).
    /// Credit: https://stackoverflow.com/a/5155015
    /// </summary>
    public static int ToNumericHash(this string text)
    {
        if (String.IsNullOrWhiteSpace(text))
            return 0;

        unchecked
        {
            int hash = 23;
            foreach (char c in text)
            {
                hash = hash * 31 + c;
            }
            return hash;
        }
    }
}
