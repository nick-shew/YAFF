using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace YAFF.Models
{
    public class FileModel
    {
        public int ID { get; set; }
        public string MimeType { get; set; }
        public byte[] Data { get; set; }
        public int Length { get; set; }
        public string MD5Hash { get; set; }
        public string UploadFileName { get; set; }
    }
}
