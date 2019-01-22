using System;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using WebAppForSolocoProject.Services;
using WebAppForSolocoProject.ViewModels;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using WebAppForSolocoProject.Utilities;

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
        public async Task<IActionResult> Create()
        {
            var model = new HomeCreateVM()
            {
                Logs = new List<string>()
            };
            await Task.Run(() =>
            {
                model.OwnersList = ownerData.GetOwners();
                model.FolderList = model.OwnersList.First().QualityFolders;
                model.BasePath = ConfigurationManager.AppSettings["basePath"].ToString();
            });
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Run(HomeCreateVM model)
        {
            return View("Create", model);
        }

        public async Task<HomeCreateVM> CreateDirectories(HomeCreateVM model)
        {
            if (model.BasePath == null)
            {
                model.Logs.Add("You must provide a base path!");
            }
            else
            { 
                try
                {
                    await Task.Run(() =>
                    {
                        foreach (var path in model.SelectedOwner.Paths)
                        {
                            string pathToCreate = Path.Combine(model.BasePath, model.SelectedOwnerName, path);
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
                    });
                }
                catch (UnauthorizedAccessException)
                {
                    model.Logs.Add("You don't have access in selected directory. Please change your base path.");
                }
            }
            return model;
        }

        public async Task<IActionResult> ChangeBasePath(HomeCreateVM model)
        {
            await UpdateModel(model);

            var path = new TaskCompletionSource<string>();

            Thread thread = new Thread(()=>BrowseFolder(path));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            if(path.Task!=null)
                model.BasePath = await path.Task;

            return View("Create", model);
        }

        private void BrowseFolder(TaskCompletionSource<string> path)
        {
            var folder = new FolderBrowserDialog();
            DialogResult result = folder.ShowDialog();
            if (result == DialogResult.OK)
            {
                path.SetResult(folder.SelectedPath);
            }
            else
            {
                path.SetResult(null);
            }
        }

        public async Task<IActionResult> GetFolders(HomeCreateVM model)
        {
            await UpdateModel(model);

            return View("Create", model);
        }

        public async Task<IActionResult> SelectFiles(HomeCreateVM model)
        {
            await UpdateModel(model);

                if (model.SelectedFolder != null)
                    model.PathToSaveFile = model.SelectedOwner.SourceFolders.FirstOrDefault(s => s.Contains(model.SelectedFolder));
                else
                    model.PathToSaveFile = model.SelectedOwner.SourceFolders.FirstOrDefault();

            model = await CreateDirectories(model);

            if(model.PathToSaveFile == null)
            {
                model.Logs.Add("Selected owner does not need initialize files.");
            }
            else
            {
                var files = new TaskCompletionSource<List<string>>();

                Thread thread = new Thread(() => BrowseFiles(files));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                model.Files = await files.Task;

                foreach (string file in model.Files)
                {
                    System.IO.File.Copy(file, Path.Combine(model.BasePath, model.SelectedOwnerName, model.PathToSaveFile, Path.GetFileName(file)), true);
                    model.Logs.Add(file + " - file save in " + Path.Combine(model.BasePath, model.SelectedOwnerName, model.PathToSaveFile));
                }
            }
            return View("Create", model);
        }

        private void BrowseFiles(TaskCompletionSource<List<string>> files)
        {
            var paths = new List<string>();

            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = Utility.fileExtensions;
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in openFileDialog.FileNames)
                    {
                        paths.Add(file);
                    }
                }
            }
            files.SetResult(paths);
        }

        private async Task UpdateModel(HomeCreateVM model)
        {
            if (model.Logs == null)
                model.Logs = new List<string>();
            await Task.Run(() =>
            {
                model.OwnersList = ownerData.GetOwners();
                model.SelectedOwner = model.OwnersList.FirstOrDefault(o => o.Name == model.SelectedOwnerName);
                model.FolderList = model.SelectedOwner.QualityFolders;
            });
        }

    }
}
