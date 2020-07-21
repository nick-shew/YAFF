using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YAFF.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace YAFF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly ILogger<FileController> _logger;
        private readonly MediaContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        public FileController(ILogger<FileController> logger, MediaContext context, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _context = context;
            _hostEnvironment = hostEnvironment;
        }
        // GET: api/<FileController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET api/<FileController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FormFileModel>> Get(long id)
        {
            var file = await _context.FileItems.FindAsync(id);

            if (file == null)
            {
                return NotFound();
            }

            return file;
        }
        /*
         * DO NOT DELETE
         */
        //POST api/<FileController>
        [HttpPost]
        public async Task<ActionResult> PostFileToWebRoot([FromForm(Name = "file")] IFormFile file)
        {
            //https://github.com/aspnet/Mvc/issues/8311
            //TODO validate file types and return BadRequest
            //save to wwwroot/uploads
            var wrp = _hostEnvironment.WebRootPath;
            //wrp = wrp.Substring(1);//trim leading forward slash?
            var uploadPath = Path.Combine(wrp, "uploads");
            var inPath = Path.Combine(uploadPath, file.FileName);
            using (var fs = new FileStream(inPath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }
            //get corresponding output info
            var outPath = Path.Combine();
            //RunFFMpeg(inPath,);

            return Ok("File upload complete");
        }
        //[HttpPost]
        //public async Task<ActionResult<FormFileModel>> PostFileToDb(long fk, [FromForm(Name = "file")] IFormFile file)
        //{
        //    //https://docs.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads?view=aspnetcore-3.1
        //    //so...this should put a FormFileModel in the db. not gonna use model binding partially because my brain is small
        //    //TODO validate file types and return BadRequest
        //    var item = new FormFileModel()
        //    {
        //        //will this infer new pk?
        //        Fk = fk,
        //        FormFile = file
        //    };
        //    _context.FileItems.Add(item);
        //    await _context.SaveChangesAsync();
        //    return CreatedAtAction(nameof(Get), new { id = item.ID }, item);
        //}
        //https://stackoverflow.com/questions/5826649/returning-a-file-to-view-download-in-asp-net-mvc
        //public ActionResult Download()
        //{
        //    var outFile = new File();
        //    var cd = new System.Net.Mime.ContentDisposition
        //    {
        //        FileName = 
        //    }
        //}

        // PUT api/<FileController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<FileController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        private void RunFFMpeg(string inPath, string outPath)
        {
            //TODO
            //HEY...probably have client do another GET request to trigger this
            //locate file
            //run ffmpeg on file
            var proStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",//update w real path
                Arguments = $@"-i ""{inPath}"" ""{outPath}"""//this will be fun
            };
            var pro = new Process
            {
                StartInfo = proStartInfo
            };
            pro.Start();
            pro.Close();
            //identify output file
            //return file :o or error if necessary
        }
        private bool isValidFileType(IFormFile file)
        {
            //TODO...make sure post+put methods do this
            return true;
        }
    }
}
