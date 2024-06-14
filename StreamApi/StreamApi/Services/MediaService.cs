using Bogus;
using StreamApi.Enums;
using StreamApi.Extensions;
using System.IO;
using Xabe.FFmpeg;

namespace StreamApi.Services;

public class MediaService : IMediaService
{
    public MediaService()
    {
    }

    public static string ProcessingControlFileName => "_processing.txt";
    public static string ProcessFinishedControlFileName => "_process_finished.txt";
    public static string ProcessFolder => $"{Path.GetTempPath()}MediaProcessingFolder{Path.DirectorySeparatorChar}";

    #region Privates

    private string MediaFolder => Path.GetFullPath($".{Path.DirectorySeparatorChar}Medias{Path.DirectorySeparatorChar}");
    
    private async Task SplitMediaAsync(string processFolder, string filePath)
    {
        try
        {
            FFmpeg.SetExecutablesPath(FFmpegDownloaderBase.FFmpegPath);

            var mediaInfo = await FFmpeg.GetMediaInfo(filePath);
            
            using (StreamWriter sw = File.CreateText($"{processFolder}{ProcessingControlFileName}")) {}

            var start = DateTime.Now;

            var fileName = $"{Path.GetFileName(filePath).Replace(Path.GetExtension(filePath), "").Replace(" ", "_")}_";

            await FFmpeg.Conversions.New()
                .AddStream(mediaInfo.Streams)
                // -codec (Copy the codec)
                // -start_number (Start at second)
                // -hls_time (Define duration of each segment in seconds)
                // -hls_list_size (Keep all segments in the playlist)
                // -f hls (Sets the output format to HLS)
                .AddParameter($"-codec: copy -start_number 0 -hls_time 10 -hls_list_size 0 -f hls")
                .SetOutput($"{processFolder}{fileName}.m3u8")
                .Start();

            var end = DateTime.Now;
            using (StreamWriter sw = File.CreateText($"{processFolder}{ProcessFinishedControlFileName}"))
            {
                await sw.WriteLineAsync($"Process started at {start.ToString("G")}");
                await sw.WriteLineAsync($"Process ended at {end.ToString("G")}");
                await sw.WriteLineAsync($"Process took {(end - start).Seconds} seconds");
            }

            if (File.Exists($"{processFolder}{ProcessingControlFileName}"))
                File.Delete($"{processFolder}{ProcessingControlFileName}");
        }
        catch (Exception e)
        {
            if (File.Exists($"{processFolder}{ProcessingControlFileName}"))
                File.Delete($"{processFolder}{ProcessingControlFileName}");

            if (File.Exists($"{processFolder}{ProcessFinishedControlFileName}"))
                File.Delete($"{processFolder}{ProcessFinishedControlFileName}");
        }
    }

    #endregion

    public string GetMediaTypeFolder(MediaTypeEnum mediaType)
    {
        switch (mediaType)
        {
            case MediaTypeEnum.Video:
                return $"{MediaFolder}Videos{Path.DirectorySeparatorChar}";
            case MediaTypeEnum.Audio:
                return $"{MediaFolder}Audios{Path.DirectorySeparatorChar}";
            default:
                throw new NotImplementedException("Media format not supported");
                break;
        }
    }

    public string? GetMedia(MediaTypeEnum mediaType)
    {
        var path = GetMediaTypeFolder(mediaType);
        var fileList = Directory.GetFiles(path);

        if (fileList.Any())
        {
            var faker = new Faker();
            return faker.Random.ArrayElement(fileList);
        }

        return null;
    }

    public string GetProcessFolder(string filePath)
    {
        return $"{ProcessFolder}{Path.GetFileName(filePath).Replace(Path.GetExtension(filePath), "").NormalizeFileName()}{Path.DirectorySeparatorChar}".Replace(" ", "_");
    }

    public async Task<string?> GetFilesFolderToStreamAsync(MediaTypeEnum mediaType, string? filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            return null;
        }

        var processFolder = GetProcessFolder(filePath);

        if (!Directory.Exists(processFolder))
        {
            Directory.CreateDirectory(processFolder);
        }
        else
        {
            if (File.Exists($"{processFolder}{ProcessFinishedControlFileName}")
                || File.Exists($"{processFolder}{ProcessingControlFileName}"))
                return processFolder;
        }

        switch (mediaType)
        {
            case MediaTypeEnum.Video:
            case MediaTypeEnum.Audio:
                SplitMediaAsync(processFolder, filePath);
                break;
            default:
                throw new NotImplementedException("Media format not supported");
                break;
        }

        return processFolder;
    }
}