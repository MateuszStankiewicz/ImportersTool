using System.Collections.Generic;
using static WebAppForSolocoProject.Services.OwnerData;

namespace WebAppForSolocoProject.Models
{
    public class Owner
    {
        public string Name { get; set; }
        public List<string> Paths { get; set; }
        public List<string> SourceFolders { get; set; }
        public List<Quality> QualityFolders { get; set; }
    }
}
