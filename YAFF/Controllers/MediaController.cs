using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YAFF.Models;

namespace YAFF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : Controller
    {
        private readonly ILogger<MediaController> _logger;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly string[] _validFileExtensions = 
        {
            ".mp3",
            ".wav",
            ".flac",
            ".ogg",
            ".aiff"
        };
        //https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-3.1
        //https://www.colincrawley.com/audio-file-size-calculator/
        private readonly int _maxUploadSize = 30000000;//IIS should automatically return an invalid request for >28.6MB...can tweak this later in web.config
        public MediaController(ILogger<MediaController> logger, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }
        //TODO robustify logging
        [HttpGet]
        [Route("Download")]
        public async Task<ActionResult> Download(string fileGuid, string fileName)
        {
            //TODO not really any reason to make this async
            var outPath = GetOutputPath(fileGuid);

            if (new FileExtensionContentTypeProvider().TryGetContentType(outPath, out string contentType))
            {
                byte[] data;
                using (var stream = System.IO.File.OpenRead(outPath))
                {
                    //there's probably some way to do this without storing everything in memory
                    data = new byte[stream.Length];
                    await stream.ReadAsync(data);
                }
                var result = File(data, contentType, fileName);
                //clean up file
                System.IO.File.Delete(outPath);
                return result;
            }
            else
            {
                //something's fishy with the contentType
                System.IO.File.Delete(outPath);
                return StatusCode(500);
            }
        }
        [HttpPost]
        [Route("PostFile")]
        public async Task<IActionResult> PostFile([FromForm] MediaModel media)
        {
            //https://stackoverflow.com/questions/18142992/creating-temporary-files-in-wwroot-folder-asp-net-mvc3

            //validate contents
            //TODO check for weird file names...i've read core 3.1 should be dealing with this automatically now but validate this
            //TODO maybe return more than status codes lol

            //prevent skulduggery w paths in filename
            media.InName = Path.GetFileName(media.InName);
            media.OutName = Path.GetFileName(media.OutName);
            //check for empty file or giant file
            if (media.Data.Length == 0 || media.Data.Length > _maxUploadSize)
            {
                LogErrorDebugInfo(media,true);
                _logger.LogError("Upload size error!");
                return StatusCode(400);
            }
            //check file extensions
            if (!_validFileExtensions.Contains(Path.GetExtension(media.InName)) || !_validFileExtensions.Contains(Path.GetExtension(media.OutName)))
            {
                LogErrorDebugInfo(media, true);
                _logger.LogError("File extension error!");
                return StatusCode(400);
            }

            //store files in wwwroot so we can use ffmpeg easily...no database io needed (?)
            var wrp = _hostEnvironment.WebRootPath;
            var uploadPath = Path.Combine(wrp, "uploads");
            var inPath = Path.Combine(uploadPath, media.InName);

            using (var stream = System.IO.File.Create(inPath))
            {
                //await stream.WriteAsync(media.Data);//for byte[]
                await media.Data.CopyToAsync(stream);//for IFF
            }

            //write output to guid file
            var outExt = Path.GetExtension(media.OutName);
            var guid = Guid.NewGuid().ToString() + outExt;
            var outPath = GetOutputPath(guid);
            //do ffmpeg thing
            var success = RunFFMpeg(inPath, outPath);
            //clean up original uploaded file
            System.IO.File.Delete(inPath);
            if (!success)
            {
                LogErrorDebugInfo(media, true);
                _logger.LogError("Error executing FFMPEG!");
                System.IO.File.Delete(outPath);
                return StatusCode(500);
            }
            
            //now...return info needed for download request
            return new JsonResult(new { FileGuid = guid, FileName = $"{media.OutName}" });
        }
        private bool RunFFMpeg(string inPath, string outPath)
        {
            //TODO--add a progress bar specifically for this part since it takes the longest
            //https://stackoverflow.com/questions/747982/can-ffmpeg-show-a-progress-bar
            //https://stackoverflow.com/questions/11441517/ffmpeg-progress-bar-encoding-percentage-in-php
            //TODO--is it stupid to make this async? https://www.pluralsight.com/guides/advanced-tips-using-task-run-async-wait
            //duhhhh
            //https://stackoverflow.com/questions/29680391/ffmpeg-command-line-write-output-to-a-text-file
            //var progressName = Path.GetFileNameWithoutExtension(outPath);
            //var progressPath = GetOutputPath(progressName + ".txt");
            //var progressPath = "output.txt";
            //System.IO.File.Create(progressPath);
            //_logger.LogDebug(progressPath);

            var proStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                //Arguments = $@"-i ""{inPath}"" ""{outPath}"" > ""{progressPath}""",//TODO come on...alternative is write stream output to file in real time
                Arguments = $@"-i ""{inPath}"" ""{outPath}""",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            using var pro = new Process
            {
                StartInfo = proStartInfo
            };
            //TODO this is either not working or everything is being written to StandardError
            pro.OutputDataReceived += new DataReceivedEventHandler((sender, e) =>
            {
                if (!String.IsNullOrEmpty(e.Data))
                {
                    _logger.LogDebug(e.Data);
                }
            });
            pro.Start();
            //capture errors
            var proErrorStream = pro.StandardError;
            var proErrors = proErrorStream.ReadToEnd();//think this might be capturing all output actually

            pro.WaitForExit();
            //System.IO.File.Delete(progressPath);
            
            //any exit code other than 0 means there was an error
            if (pro.ExitCode != 0)
            {
                _logger.LogError($"FFMpeg has exited with error code {pro.ExitCode}! Args: {proStartInfo.Arguments}");
                _logger.LogInformation(proErrors);
            }
            return pro.ExitCode == 0;//fret not, dispose is still called
        }
        private string GetOutputPath(string fileName)
        {
            var wrp = _hostEnvironment.WebRootPath;
            var outputPath = Path.Combine(wrp, "output");
            return Path.Combine(outputPath, fileName);
        }
        private void LogErrorDebugInfo(MediaModel media, bool isError = false)
        {
            var msg = $"Error occurred for file conversion {media.InName}->{media.OutName}. Input file length was {media.Data.Length}";
            if (isError)
            {
                _logger.LogError(msg);
            }
            else
            {
                _logger.LogDebug(msg);
            }
        }
        //TODO--just to be sure, delete all files on session end
    }
}
