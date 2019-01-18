using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebAppForSolocoProject.Models;

namespace WebAppForSolocoProject.ViewModels
{
    public class HomeCreateVM
    {
        public string SelectedOwner{ get; set;}
        public string SelectedFolder { get; set; }
        public IEnumerable<Owner> OwnersList { get; set; }
        public IEnumerable<string> FolderList { get; set; }
        [Required]
        public string BasePath { get; set; }
        public List<string> Logs { get; set; }
        public List<string> Files { get; set; }
        public string PathToSaveFile { get; set; }
    }
}
