using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using WebAppForSolocoProject.Models;

namespace WebAppForSolocoProject.ViewModels
{
    public class HomeCreateVM
    {
        public string SelectedOwner{ get; set;}

        public IEnumerable<Owner> ownersList { get; set; } = new List<Owner>();

        [Required]
        public string basePath { get; set; }

        public List<string> CreatedPaths { get; set; } = new List<string>();
    }
}
