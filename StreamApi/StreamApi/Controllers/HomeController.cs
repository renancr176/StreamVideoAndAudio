using Microsoft.AspNetCore.Mvc;
using StreamApi.Enums;
using StreamApi.Extensions;
using StreamApi.Models;
using StreamApi.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace StreamApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly IMediaService _mediaService;

        public HomeController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        [HttpGet("Video")]
        [SwaggerResponse(200, Type = typeof(PlayListModel))]
        [SwaggerResponse(204)]
        public async Task<IActionResult> GetVideoAsync()
        {
            var filePath = _mediaService.GetMedia(MediaTypeEnum.Video);

            if (filePath == null)
                return NoContent();

            var processFolder = await _mediaService.GetFilesFolderToStreamAsync(MediaTypeEnum.Video, filePath);

            if (processFolder == null || !System.IO.File.Exists($"{processFolder}{MediaService.ProcessFinishedControlFileName}"))
                return NoContent();

            var files = Directory.GetFiles(processFolder).ToList();

            if (files.Any(file => file.EndsWith(".m3u8")))
            {
                var playListFile = files.First(file => file.EndsWith(".m3u8"));

                return Ok(new PlayListModel()
                {
                    Name = Path.GetFileName(playListFile).Replace(Path.GetExtension(playListFile), "").Replace("_", " ").Trim(),
                    File = new FileModel()
                    {
                        Name = Path.GetFileName(playListFile),
                        ContentType = "application/x-mpegURL",
                        Base64Data = playListFile.FileToBase64()
                    },
                    TsFiles = files.Where(filePath => filePath.EndsWith(".ts"))
                        .OrderBy(filePath => filePath)
                        .Select(filePath => filePath.Replace(MediaService.ProcessFolder, ""))
                });
            }

            return NoContent();
        }

        [HttpGet("Audio")]
        [SwaggerResponse(200, Type = typeof(PlayListModel))]
        [SwaggerResponse(204)]
        public async Task<IActionResult> GetMusicsAsync()
        {
            var filePath = _mediaService.GetMedia(MediaTypeEnum.Audio);

            if (filePath == null)
                return NoContent();

            var processFolder = await _mediaService.GetFilesFolderToStreamAsync(MediaTypeEnum.Video, filePath);

            if (processFolder == null || !System.IO.File.Exists($"{processFolder}{MediaService.ProcessFinishedControlFileName}"))
                return NoContent();

            var files = Directory.GetFiles(processFolder);

            if (files.Any(file => file.EndsWith(".m3u8")))
            {
                var playListFile = files.First(file => file.EndsWith(".m3u8"));

                return Ok(new PlayListModel()
                {
                    Name = Path.GetFileName(playListFile).Replace(Path.GetExtension(playListFile), "").Replace("_", " ").Trim(),
                    File = new FileModel()
                    {
                        Name = Path.GetFileName(playListFile),
                        ContentType = "application/x-mpegURL",
                        Base64Data = playListFile.FileToBase64()
                    },
                    TsFiles = files.Where(filePath => filePath.EndsWith(".ts"))
                        .OrderBy(filePath => filePath)
                        .Select(filePath => filePath.Replace(MediaService.ProcessFolder, ""))
                });
            }

            return NoContent();
        }

        [HttpGet("StreamFile")]
        [SwaggerResponse(200, Type = typeof(FileModel))]
        [SwaggerResponse(400)]
        public async Task<IActionResult> GetStreamFile([FromQuery] string tsFile)
        {
            var filePath = $"{MediaService.ProcessFolder}{tsFile}";

            if (!System.IO.File.Exists(filePath))
                return BadRequest();

            return Ok(new FileModel()
            {
                Name = Path.GetFileName(filePath),
                ContentType = filePath.GetContentTypeFromFile(),
                Base64Data = filePath.FileToBase64()
            });
        }
    }
}
