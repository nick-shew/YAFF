using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YAFF.Models
{
    public class MediaModel
    {
        [FromForm(Name="inName")]
        public string InName { get; set; }
        [FromForm(Name = "outName")]
        public string OutName { get; set; }
        [FromForm(Name = "data")]
        public IFormFile Data { get; set; }
    }
}
