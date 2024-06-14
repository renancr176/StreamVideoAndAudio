using Quartz;
using StreamApi.Enums;
using StreamApi.Services;

namespace StreamApi.Scheduler.Jobs;

[DisallowConcurrentExecution]
public class SplitFilesToStreamJob : IJob
{
    private readonly IMediaService _mediaService;

    public SplitFilesToStreamJob(IMediaService mediaService)
    {
        _mediaService = mediaService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            var tasks = new List<Task>();
            foreach (var mediaType in Enum.GetValues<MediaTypeEnum>())
            {
                var filesPath = Directory.GetFiles(_mediaService.GetMediaTypeFolder(mediaType)).ToList();

                foreach (var filePath in filesPath)
                {
                    var processFolder = _mediaService.GetProcessFolder(filePath);
                    if (!Directory.Exists(processFolder)
                    || (!File.Exists($"{processFolder}{MediaService.ProcessingControlFileName}") && !File.Exists($"{processFolder}{MediaService.ProcessFinishedControlFileName}")))
                    {
                        tasks.Add(_mediaService.GetFilesFolderToStreamAsync(mediaType, filePath));
                    }
                }
            }

            Task.WaitAll(tasks.ToArray());
        }
        catch (Exception e)
        {
        }
    }
}