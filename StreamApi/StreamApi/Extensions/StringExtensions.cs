using Microsoft.AspNetCore.StaticFiles;

namespace StreamApi.Extensions;

public static class StringExtensions
{
    public static string? GetContentTypeFromFile(this string filePath)
    {
        var provider = new FileExtensionContentTypeProvider();

        string? contentType = null;

        provider.TryGetContentType(filePath, out contentType);

        return contentType;
    }

    public static string? FileToBase64(this string filePath)
    {
        if (File.Exists(filePath))
        {
            return Convert.ToBase64String(File.ReadAllBytes(filePath));
        }

        return null;
    }

    public static IEnumerable<String> SplitInParts(this string s, int partLength)
    {
        if (s == null)
            throw new ArgumentNullException(nameof(s));
        if (partLength <= 0)
            throw new ArgumentException("Part length has to be positive.", nameof(partLength));

        for (var i = 0; i < s.Length; i += partLength)
            yield return s.Substring(i, Math.Min(partLength, s.Length - i));
    }
}