using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAppForSolocoProject.Models;

namespace WebAppForSolocoProject.ViewModels
{
    public class HomeCreateVM
    {
        public string selectedOwner{ get; set;}

        public IEnumerable<Owner> ownersList { get; set; } = new List<Owner>();

        public string basePath { get; set; }
    }
}
