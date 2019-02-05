using System.Collections.Generic;
using static WebAppForSolocoProject.Services.OwnerData;

namespace WebAppForSolocoProject.Models
{
    public class Owner
    {
        public string Name { get; set; }

        public List<string> Config { get; set; }

        public List<Quality> QualityFolders { get; set; }

        public Dictionary<string,List<string>> FolderPathPairs { get; set; }

        public string ChildOwnerOf { get; set; }
    }
}
