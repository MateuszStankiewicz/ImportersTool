using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using WebAppForSolocoProject.Models;
using System.Configuration;
using WebAppForSolocoProject.Services;
using WebAppForSolocoProject.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;

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
            model.ownersList = ownerData.GetAllOwners();
            model.basePath = ConfigurationManager.AppSettings["basePath"].ToString();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(HomeCreateVM model)
        {
            Owner owner = ownerData.GetOwner(model.selectedOwner);
            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var path in owner.Paths)
                    {
                        Directory.CreateDirectory(model.basePath+"\\"+model.selectedOwner+"\\"+path);
                    } 
                }
                catch (UnauthorizedAccessException)
                {
                    return BadRequest("You don't have access in selected directory. Please change your base path.");
                }
            }

            return View("Created", owner);

        }
    }
}
