using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YAFF.Models;

namespace YAFF.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        

        public HomeController(ILogger<HomeController> logger, MediaContext context)
        {
            _logger = logger;
        }
        //[HttpPost]
        //public async Task<IActionResult> SubmitMedia(int id, IFormFile file)
        //{
        //    //TODO
        //    //https://stackoverflow.com/questions/3877448/asp-net-mvc-passing-model-together-with-files-back-to-controller
        //    //https://stackoverflow.com/questions/1653469/how-can-i-upload-a-file-and-save-it-to-a-stream-for-further-preview-using-c/1653508#1653508
        //    //https://stackoverflow.com/questions/51021182/httppostedfilebase-in-asp-net-core-2-0/51021836
            
        //    var filePath = Path.GetTempFileName();
        //    //TODO now alter OutputModel with id to point to the file's location, or bind id to file somehow
        //    if (file.Length > 0)
        //    {
        //        using(var stream = new FileStream(filePath, FileMode.Create))
        //        {
        //            await file.CopyToAsync(stream);
        //        }
        //    }

        //    return Ok(filePath);
        //}
        

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
