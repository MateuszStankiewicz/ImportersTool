using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebAppForSolocoProject.Models;
using static WebAppForSolocoProject.Services.OwnerData;

namespace WebAppForSolocoProject.ViewModels
{
    public class HomeCreateVM
    {
        public string SelectedOwnerName{ get; set;}
        public string SelectedFolder { get; set; }
        public IEnumerable<Owner> OwnersList { get; set; }
        public IEnumerable<Quality> FolderList { get; set; }
        public string BasePath { get; set; }
        public List<string> Logs { get; set; }
        public string PathToSaveFile { get; set; }
        public Owner SelectedOwner { get; set; }
        public bool SelectFilesBtnClicked { get; set; }
        public List<IFormFile> Files { get; set; }
    }
}
