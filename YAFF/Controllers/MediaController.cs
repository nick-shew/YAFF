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
        [HttpGet]
        [Route("Download")]
        public ActionResult Download(string fileGuid, string fileName)
        {
            _logger.LogDebug($"Download request received for {fileName}, GUID: {fileGuid}");
            var outPath = GetOutputPath(fileGuid);

            if (new FileExtensionContentTypeProvider().TryGetContentType(outPath, out string contentType))
            {
                byte[] data;
                using (var stream = System.IO.File.OpenRead(outPath))
                {
                    //there's probably some way to do this without storing everything in memory
                    data = new byte[stream.Length];
                    stream.Read(data);
                }
                var result = File(data, contentType, fileName);
                //clean up file
                System.IO.File.Delete(outPath);
                return result;
            }
            else
            {
                //something's fishy with the contentType
                _logger.LogError($"Couldn't infer content type for {fileName}, aborting...");
                System.IO.File.Delete(outPath);
                return StatusCode(500);
            }
        }
        [HttpPost]
        [Route("PostFile")]
        public IActionResult PostFile([FromForm] MediaModel media)
        {
            //validate contents
            //TODO handle international file names...i've read core 3.1 should be dealing with this automatically now but validate this
            //TODO check actual MIME type
            _logger.LogDebug($"Beginning file conversion for {media.InName} -> {media.OutName}");

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

            //prevent submission spamming
            if (System.IO.File.Exists(inPath))
            {
                LogErrorDebugInfo(media, true);
                _logger.LogError("File is already being converted!");
                return StatusCode(400);
            }

            using (var stream = System.IO.File.Create(inPath))
            {
                //await stream.WriteAsync(media.Data);//for byte[]
                media.Data.CopyTo(stream);//for IFF
            }

            //write output to guid file
            var outExt = Path.GetExtension(media.OutName);
            var guid = Guid.NewGuid().ToString() + outExt;
            var outPath = GetOutputPath(guid);
            //do ffmpeg thing
            var success = RunFFMpeg(inPath, outPath);
            //regardless of success, clean up original uploaded file
            System.IO.File.Delete(inPath);
            if (!success)
            {
                LogErrorDebugInfo(media, true);
                _logger.LogError("Error executing FFMPEG!");
                System.IO.File.Delete(outPath);
                return StatusCode(500);
            }
            _logger.LogDebug($"File conversion succeeded! Returning {media.OutName} with GUID: {guid}");
            //now...return info needed for download request
            return new JsonResult(new { FileGuid = guid, FileName = $"{media.OutName}" });
        }
        private bool RunFFMpeg(string inPath, string outPath)
        {
            //no longer async as per https://www.pluralsight.com/guides/advanced-tips-using-task-run-async-wait

            var proStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $@"-i ""{inPath}"" ""{outPath}""",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            using var pro = new Process
            {
                StartInfo = proStartInfo
            };

            pro.Start();
            //capture errors
            var proErrorStream = pro.StandardError;
            var proErrors = proErrorStream.ReadToEnd();//think this might be capturing all output actually

            pro.WaitForExit();
            
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
