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
using System.Windows.Forms;
using System.Threading;

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
        public IActionResult Create(string button)
        {
            var model = new HomeCreateVM()
            {
                Logs = new List<string>()
            };
            model.OwnersList = ownerData.GetOwners().OrderBy(o => o.Name);
            model.FolderList = model.OwnersList.First().QualityFolders;
            model.BasePath = ConfigurationManager.AppSettings["basePath"].ToString();
            return View(model);
        }

        [HttpPost]
        //[ValidateAntiForgeryToken]
        public IActionResult Create(HomeCreateVM model)
        {
            UpdateModel(model);
            Owner owner = ownerData.GetOwners().FirstOrDefault(o => o.Name == model.SelectedOwner);
            if (ModelState.IsValid)
            {
                try
                {
                    foreach (var path in owner.Paths)
                    {
                        string pathToCreate = Path.Combine(model.BasePath, model.SelectedOwner, path);
                        //string pathToCreate = model.BasePath + "\\" + model.SelectedOwner + "\\" + path;
                        if (Directory.Exists(pathToCreate))
                        {
                            pathToCreate += " - directory already exist.";
                        }
                        else
                        {
                            Directory.CreateDirectory(pathToCreate);
                            pathToCreate += " - directory succesfully created.";
                        }
                        model.Logs.Add(pathToCreate);
                    }
                    return View("Create", model);
                }
                catch (UnauthorizedAccessException)
                {
                    model.Logs.Add("You don't have access in selected directory. Please change your base path.");
                }
            }
            return View("Create",model);
        }

        public IActionResult ChangeBasePath(HomeCreateVM model)
        {
            UpdateModel(model);

            Thread thread = new Thread(new ThreadStart(ThreadMethod));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            void ThreadMethod()
            {
                var folder = new FolderBrowserDialog();
                DialogResult result = folder.ShowDialog();
                if (result == DialogResult.OK)
                {
                    model.BasePath = folder.SelectedPath;
                }
            }

            return View("Create", model);

        }

        public IActionResult GetFolders(HomeCreateVM model)
        {
            var updateModel = new HomeCreateVM();
            updateModel.BasePath = model.BasePath;
            if (model.Logs != null)
                updateModel.Logs = model.Logs;
            else
                updateModel.Logs = new List<string>();
            updateModel.Files = updateModel.Files;
            updateModel.OwnersList = ownerData.GetOwners().OrderBy(o => o.Name);

            updateModel.FolderList = updateModel.OwnersList.FirstOrDefault(o => o.Name == model.SelectedOwner).QualityFolders;
            return View("Create", updateModel);
        }

        public IActionResult SelectFiles(HomeCreateVM model)
        {
            UpdateModel(model);

            if (ModelState.IsValid)
            {
                if(model.PathToSaveFile == null)
                {
                    model.Logs.Add("Selected owner does not need initialize files.");
                }
                else if(!Directory.Exists(Path.Combine(model.BasePath, model.SelectedOwner, model.PathToSaveFile)))
                {
                    model.Logs.Add("You must first create directories for selected Base Path and Owner.");
                    model.Logs.Add("Click 'Create' button");
                }
                else
                {
                    Thread thread = new Thread(new ThreadStart(ThreadMethod));
                    thread.SetApartmentState(ApartmentState.STA);
                    thread.Start();
                    thread.Join();

                    void ThreadMethod()
                    {
                        using (OpenFileDialog openFileDialog = new OpenFileDialog())
                        {
                            model.Files = new List<string>();
                            openFileDialog.Filter = "Importer Files(*.jpg;*.xml;*.xlsx;*.srt)|*.JPG;*.XML;*.XLSX;*.SRT|All files (*.*)|*.*";
                            openFileDialog.Multiselect = true;

                            if (openFileDialog.ShowDialog() == DialogResult.OK)
                            {
                                foreach (string path in openFileDialog.FileNames)
                                {
                                    model.Files.Add(path);
                                    System.IO.File.Copy(path, Path.Combine(model.BasePath, model.SelectedOwner, model.PathToSaveFile, Path.GetFileName(path)), true);
                                    model.Logs.Add(path + " - file save in " + Path.Combine(model.BasePath, model.SelectedOwner, model.PathToSaveFile));
                                }
                            }
                        }
                    }
                }
            }
            return View("Create", model);
        }

           


        private void UpdateModel(HomeCreateVM model)
        {
            if (model.Logs == null)
                model.Logs = new List<string>();
            model.OwnersList = ownerData.GetOwners().OrderBy(o => o.Name);
            Owner selectedOwner = model.OwnersList.FirstOrDefault(o => o.Name == model.SelectedOwner);
            model.FolderList = selectedOwner.QualityFolders;
            if(selectedOwner.SourceFolders!=null)
            {
                if (model.SelectedFolder != null)
                    model.PathToSaveFile = selectedOwner.SourceFolders.FirstOrDefault(s => s.Contains(model.SelectedFolder));
                else
                    model.PathToSaveFile = selectedOwner.SourceFolders.FirstOrDefault();
            }
        }
    }
}
