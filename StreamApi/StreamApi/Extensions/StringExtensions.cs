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

    public static string NormalizeFileName(this string fileName)
    {
        // Not allowed character - Replacement
        var notAllowedCharacters = new Dictionary<string, string>()
        {
            {"\\", "-"},
            {"/", "-"},
            {":", "-"},
            {"*", ""},
            {"?", ""},
            {"\"", ""},
            {"<", ""},
            {">", ""},
            {"|", ""},
        };

        foreach (var item in notAllowedCharacters)
        {
            fileName = fileName.Replace(item.Key, item.Value);
        }

        return fileName;
    }

    public static string? FileToBase64(this string filePath)
    {
        if (File.Exists(filePath))
        {
            return Convert.ToBase64String(File.ReadAllBytes(filePath));
        }

        return null;
    }
}