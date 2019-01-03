using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebAppForSolocoProject.Models;

namespace WebAppForSolocoProject.ViewModels
{
    public class HomeCreateVM
    {
        public string SelectedOwner{ get; set;}

        public IEnumerable<Owner> ownersList { get; set; } = new List<Owner>();

        public string basePath { get; set; }
    }
}
