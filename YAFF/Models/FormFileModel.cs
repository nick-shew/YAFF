using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YAFF.Models
{
    public class FormFileModel
    {
        public long ID { get; set; }
        public long Fk { get; set; }//fk for output info
        public IFormFile FormFile { get; set; }
        //maybe add some metadata fields here so we can delete the actual file later and still have record keeping
    }
}
