using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace YAFF.Models
{
    public class MediaContext:DbContext
    {
        public MediaContext(DbContextOptions<MediaContext> options):base(options)
        {

        }
        public DbSet<OutputModel> MediaItems { get; set; }
        public DbSet<FormFileModel> FileItems { get; set; }
    }
}
