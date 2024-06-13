using Bogus;
using Microsoft.AspNetCore.Mvc;
using StreamApi.Enums;
using StreamApi.Extensions;
using StreamApi.Models;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace StreamApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private string MediaFolder => Path.GetFullPath($".{Path.DirectorySeparatorChar}Medias{Path.DirectorySeparatorChar}");

        private FileModel GetMedia(MediaType mediaType)
        {
            var path = $"{MediaFolder}Videos";
            switch (mediaType)
            {
                case MediaType.Audio:
                    path = $"{MediaFolder}Audios";
                    break;
            }

            var fileList = Directory.GetFiles(path);

            if (fileList.Any())
            {
                var faker = new Faker();
                var model = new FileModel();
                model.Name = faker.Random.ArrayElement(fileList);
                model.ContentType = model.Name.GetContentTypeFromFile();

                return model;
            }

            return null;
        }

        private async Task WriteItemsAsync(
            MediaType mediaType,
            ChannelWriter<FileModel> writer,
            CancellationToken cancellationToken)
        {
            Exception localException = null;
            try
            {
                var file = GetMedia(MediaType.Video);

                foreach (var mediaPart in file.Name.FileToBase64().SplitInParts(4000))
                {
                    await writer.WriteAsync(new FileModel()
                    {
                        Name = Path.GetFileName(file.Name),
                        ContentType = file.ContentType,
                        Base64Data = mediaPart
                    }, cancellationToken);

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }
            }
            catch (Exception ex)
            {
                localException = ex;
            }
            finally
            {
                writer.Complete(localException);
            }
        }

        [HttpGet("Video")]
        public async Task<ChannelReader<FileModel>> GetVideoAsync(CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<FileModel>();

            WriteItemsAsync(MediaType.Video, channel.Writer, cancellationToken);

            return channel;
        }

        [HttpGet("Audio")]
        public async Task<ChannelReader<FileModel>> GetMusicsAsync(CancellationToken cancellationToken)
        {
            var channel = Channel.CreateUnbounded<FileModel>();

            WriteItemsAsync(MediaType.Audio, channel.Writer, cancellationToken);

            return channel;
        }
    }
}
