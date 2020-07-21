using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YAFF.Models;

namespace YAFF.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //TODO...refactor to use Media and OutputItem less interchangeably
    public class MediaController : Controller
    {
        private readonly ILogger<MediaController> _logger;
        private readonly MediaContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;
        public MediaController(ILogger<MediaController> logger, MediaContext context, IWebHostEnvironment hostEnvironment)
        {
            _logger = logger;
            _context = context;
            _hostEnvironment = hostEnvironment;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<OutputModel>> GetOutputInfo(long id)
        {
            var mediaItem = await _context.MediaItems.FindAsync(id);

            if (mediaItem == null)
            {
                return NotFound();
            }

            return mediaItem;
        }
        // POST for initial record creation
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<ActionResult<OutputModel>> PostOutputModel(OutputModel item)
        {
            _logger.LogDebug("Posting outputmodel");
            _context.MediaItems.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOutputInfo), new { id = item.ID }, item);
        }
        //public async Task<ActionResult<OutputModel>> PostFile(IFormFile item)
        //{
        //    //TODO validate file types and return BadRequest
        //    //save to wwwroot/media
        //    var uploadPath = Path.Combine(_hostEnvironment.WebRootPath);
        //    var fullPath = Path.Combine(uploadPath, item.FileName);
        //    using (var fs = new FileStream(fullPath, FileMode.Create))
        //    {
        //        await item.CopyToAsync(fs);
        //    }
        //    //do we want to upload this to DB?
        //    _context.FileItems.Add(item);
        //    await _context.SaveChangesAsync();
        //    return CreatedAtAction(nameof(GetOutputInfo), new { id = item.ID }, item);
        //}
        
        //[HttpPost]
        //public IActionResult PostFile()
        //https://stackoverflow.com/questions/49756601/how-to-upload-file-from-ajax-to-asp-net-core-controller
        //{
        //    var file = HttpContext.Request.Form.Files[0];
        //    return Ok();
        //}

        [HttpPut("{id}")]
        public async Task<IActionResult> PutOutputModel(long id, OutputModel todoItem)
        {
            if (id != todoItem.ID)
            {
                return BadRequest();
            }

            _context.Entry(todoItem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OutputItemExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // GET: MediaController/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: MediaController/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: MediaController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: MediaController/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        private bool OutputItemExists(long id) =>
             _context.MediaItems.Any(e => e.ID == id);
    }
}
