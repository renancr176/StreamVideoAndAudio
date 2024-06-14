using StreamApi.Enums;

namespace StreamApi.Services;

public interface IMediaService
{
    string GetMediaTypeFolder(MediaTypeEnum mediaType);
    string? GetMedia(MediaTypeEnum mediaType);
    string GetProcessFolder(string filePath);
    Task<string?> GetFilesFolderToStreamAsync(MediaTypeEnum mediaType, string? filePath);
}