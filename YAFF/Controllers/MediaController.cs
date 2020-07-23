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
        public MediaController(ILogger<MediaController> logger, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _hostEnvironment = hostEnvironment;
        }
        [HttpGet]
        [Route("Download")]
        public async Task<ActionResult> Download(string fileGuid, string fileName)
        {
            var outPath = getOutputPath(fileGuid);

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
        public async Task<IActionResult> PostFile([FromForm]MediaModel media)
        {
            //https://stackoverflow.com/questions/18142992/creating-temporary-files-in-wwroot-folder-asp-net-mvc3
            //TODO add progress indicator

            //validate contents
            //TODO check for large file size
            //TODO check for weird file names
            //prevent skulduggery w paths in filename
            media.InName = Path.GetFileName(media.InName);
            media.OutName = Path.GetFileName(media.OutName);
            //check for empty file
            if (media.Data.Length == 0)
            {
                return StatusCode(400);
            }

            //store files in wwwroot so we can use ffmpeg
            //TODO is this kosher?
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
            var outPath = getOutputPath(guid);
            //do ffmpeg thing
            var success = await RunFFMpeg(inPath, outPath);
            if (!success)
            {
                return StatusCode(500);
            }
            //clean up original file
            System.IO.File.Delete(inPath);
            //now...return info needed for download request
            return new JsonResult(new { FileGuid = guid, FileName = $"{media.OutName}" });
        }
        private async Task<bool> RunFFMpeg(string inPath, string outPath)
        {
            //run ffmpeg on file
            var proStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $@"-i ""{inPath}"" ""{outPath}"""
            };
            var pro = new Process
            {
                StartInfo = proStartInfo
            };
            await Task.Run(()=>pro.Start());
            pro.WaitForExit();
            //any exit code other than 0 means there was an error
            return pro.ExitCode == 0;
        }
        private string getOutputPath(string fileName)
        {
            var wrp = _hostEnvironment.WebRootPath;
            var outputPath = Path.Combine(wrp, "output");
           return Path.Combine(outputPath, fileName);
        }
        //TODO--delete all files on session end
    }
}
