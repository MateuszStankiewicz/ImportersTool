using System;
using Microsoft.AspNetCore.Mvc;
using WebAppForSolocoProject.Models;
using System.Configuration;
using WebAppForSolocoProject.Services;
using WebAppForSolocoProject.ViewModels;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace WebAppForSolocoProject.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration configuration;
        private OwnerData ownerData;

        public HomeController(IConfiguration configuration, OwnerData ownerData)
        {
            this.configuration = configuration;
            this.ownerData = ownerData;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new HomeCreateVM();
            model.ownersList = ownerData.GetOwners();
            model.basePath = ConfigurationManager.AppSettings["basePath"].ToString();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(HomeCreateVM model)
        {
            model.ownersList = ownerData.GetOwners();
            Owner owner = ownerData.GetOwners().FirstOrDefault(o => o.Name == model.SelectedOwner);
            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var path in owner.Paths)
                    {
                        string pathToCreate = model.basePath + "\\" + model.SelectedOwner + "\\" + path;
                        if(Directory.Exists(pathToCreate))
                        {
                            pathToCreate += " - directory already exist.";
                        }
                        else
                        {
                            Directory.CreateDirectory(pathToCreate);
                            pathToCreate += " - directory succesfully created.";
                        }
                        model.CreatedPaths.Add(pathToCreate);
                    }
                    return View("Create", model);
                }
                catch (UnauthorizedAccessException)
                {
                    return BadRequest("You don't have access in selected directory. Please change your base path.");
                }
            }    
            return View("Create",model);
        }
    }
}
