using System;
using Microsoft.AspNetCore.Mvc;
using WebAppForSolocoProject.Models;
using System.Configuration;
using WebAppForSolocoProject.Services;
using WebAppForSolocoProject.ViewModels;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;

namespace WebAppForSolocoProject.Controllers
{
    public class HomeController : Controller
    {
        private IOwnerData ownerData;
        private IConfiguration configuration;

        public HomeController(IOwnerData ownerData, IConfiguration configuration)
        {
            this.ownerData = ownerData;
            this.configuration = configuration;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new HomeCreateVM();
            model.ownersList = ownerData.Owners;
            model.basePath = ConfigurationManager.AppSettings["basePath"].ToString();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(HomeCreateVM model)
        {
            Owner owner = ownerData.Owners.FirstOrDefault(o => o.Name == model.SelectedOwner);
            if (ModelState.IsValid)
            {
                try
                {
                    HomeCreatedVM createdVM = new HomeCreatedVM();
                    foreach (var path in owner.Paths)
                    {
                        string pathToCreate = model.basePath + "\\" + model.SelectedOwner + "\\" + path;
                        Directory.CreateDirectory(pathToCreate);
                        createdVM.Paths.Add(pathToCreate);
                    }
                    return View("Created", createdVM);
                }
                catch (UnauthorizedAccessException)
                {
                    return BadRequest("You don't have access in selected directory. Please change your base path.");
                }
            }
            return BadRequest();
        }
    }
}
