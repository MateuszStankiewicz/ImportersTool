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
using Microsoft.AspNetCore.Http;

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
                model.SelectFilesBtnClicked = false;
                model.OwnersList = ownerData.GetOwners();
                model.SelectedOwner = model.OwnersList.First();
                model.FolderList = model.SelectedOwner.QualityFolders;
                model.BasePath = ConfigurationManager.AppSettings["basePath"].ToString();
            });
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Run(HomeCreateVM model)
        {
            await UpdateModel(model);
            if(System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "original_app.config")))
            {
                DeleteAppConfig("app.config");
                RenameAppConfig("original_app.config", "app.config");
            }
            else
            {
                model.Logs.Add("You need first Updade Config.");
            }
            return View("Create", model);
        }

        public async Task<HomeCreateVM> CreateDirectories(HomeCreateVM model)
        {
            await Task.Run(() =>
            {
                foreach (var key in model.SelectedOwner.FolderPathPairs.Keys)
                {
                    foreach (var path in model.SelectedOwner.FolderPathPairs.GetValueOrDefault(key))
                    {
                        model.Logs.Add(CreateDirectory(Path.Combine(model.BasePath, model.SelectedOwnerName, path)));
                    }
                }
            });
            return model;
        }

        private string CreateDirectory(string pathToCreate)
        {
            return Directory.Exists(pathToCreate) 
                ? pathToCreate + " - directory already exist." 
                : Directory.CreateDirectory(pathToCreate).ToString() + " - directory succesfully created.";
        }

        public async Task<IActionResult> ChangeBasePath(HomeCreateVM model)
        {
            await UpdateModel(model);
            var path = new TaskCompletionSource<string>();

            Thread thread = new Thread(()=>BrowseFolder(path,model));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            model.BasePath = await path.Task;
            return View("Create", model);
        }

        private void BrowseFolder(TaskCompletionSource<string> path,HomeCreateVM model)
        {
            var folder = new FolderBrowserDialog();
            folder.ShowNewFolderButton = true;
            DialogResult result = folder.ShowDialog();
            if (result == DialogResult.OK)
            {
                path.SetResult(folder.SelectedPath);
            }
            else
            {
                path.SetResult(model.BasePath);
            }
        }

        public async Task<IActionResult> GetFolders(HomeCreateVM model)
        {
            await UpdateModel(model);
            if (!model.SelectedOwner.FolderPathPairs.Keys.Contains("SourceFolder"))
            {
                model.Logs.Add("Selected owner does not need initialize files. Just click 'Copy Files'.");
            }
            return View("Create", model);
        }

        public async Task<IActionResult> CopyFiles(HomeCreateVM model)
        {
            await UpdateModel(model);

            if (model.BasePath == null)
            {
                model.Logs.Add("You must provide a base path!");
                return View("Create", model);
            }
            try
            {
                List<string> sourceFolders = model.SelectedOwner.FolderPathPairs.GetValueOrDefault("SourceFolder");
                if (sourceFolders == null)
                {
                    model.SelectFilesBtnClicked = true;
                    model = await CreateDirectories(model);
                }
                else
                {
                    model.PathToSaveFile = model.SelectedFolder != null
                        ? sourceFolders.FirstOrDefault(s => s.Contains(model.SelectedFolder))
                        : model.PathToSaveFile = sourceFolders.FirstOrDefault();

                    if (model.Files != null)
                    {
                        model.SelectFilesBtnClicked = true;
                        model = await CreateDirectories(model);
                        await CopySelectdFiles(model);
                    }
                    else
                    {
                        model.Logs.Add("You need first choose starting files.");
                    }
                }
                ownerData.BasePath = model.BasePath;
                ownerData.SelectedOwner = model.SelectedOwnerName;
                ownerData.SelectedFolder = model.SelectedFolder;
            }
            catch (UnauthorizedAccessException)
            {
                model.Logs.Add("You don't have access in selected directory. Please change your base path.");
                return View("Create", model);
            }
            return View("Create", model);
        }

        private async Task CopySelectdFiles(HomeCreateVM model)
        {
            foreach (IFormFile file in model.Files)
            {
                var filePath = Path.Combine(model.BasePath, model.SelectedOwnerName, model.PathToSaveFile,file.FileName);

                if (file.Length>0)
                {
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {                    
                        await file.CopyToAsync(stream);
                    }
                }
                model.Logs.Add(file.FileName + " - file save in " + Path.Combine(model.BasePath, model.SelectedOwnerName, model.PathToSaveFile));
            }
        }

        private async Task UpdateModel(HomeCreateVM model)
        {
            if (model.Logs == null)
                model.Logs = new List<string>();
            await Task.Run(() =>
            {
                model.BasePath = (model.BasePath == null) ? ownerData.BasePath : model.BasePath;
                model.OwnersList = ownerData.GetOwners();
                model.SelectedOwnerName = (model.SelectedOwnerName == null) ? ownerData.SelectedOwner : model.SelectedOwnerName;
                model.SelectedOwner = model.OwnersList.FirstOrDefault(o => o.Name == model.SelectedOwnerName);
                model.SelectedFolder = (model.SelectedFolder == null) ? ownerData.SelectedFolder : model.SelectedFolder;
                model.FolderList = model.SelectedOwner.QualityFolders;
            });
        }

        public async Task<IActionResult> UpdateConfig(HomeCreateVM model)
        {
            await UpdateModel(model);

            var appConfigFile = await ManageConfigFile.ParseAppConfigToStringArrayAsync();
            var updateConfig = RewriteConfig(model, appConfigFile);
            RenameAppConfig("app.config", "original_app.config");
            await SaveUpdatedAppConfig(updateConfig);
            model.Logs.Add("Config succesfully updated.");
            model.SelectFilesBtnClicked = true;
            ownerData.SelectedFolder = null;

            return View("Create", model);
        }

        private async Task SaveUpdatedAppConfig(List<string> updateConfig)
        {
            await System.IO.File.WriteAllLinesAsync(Path.Combine(Directory.GetCurrentDirectory(), "app.config"), updateConfig);
        }

        private void RenameAppConfig(string oldName, string newName)
        {
            System.IO.File.Move(Path.Combine(Directory.GetCurrentDirectory(), oldName), Path.Combine(Directory.GetCurrentDirectory(), newName));
        }

        private void DeleteAppConfig(string fileName)
        {
            System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), fileName));
        }

        private List<string> RewriteConfig(HomeCreateVM model, string[] configList)
        {
            List<string> updateConfig = new List<string>();

            for (int i = 0; i < configList.Length; i++)
            {
                if (configList[i].Contains("OwnersEnabled"))
                {
                    updateConfig.Add(". OwnersEnabled=" + model.SelectedOwnerName);
                }
                else if (IsOwnerConfig(configList[i], model))
                {
                    updateConfig.Add(configList[i++]);
                    updateConfig.Add(TrimToImporterOnly(configList[i++]));

                    while (!string.IsNullOrWhiteSpace(configList[i]))
                    {
                        if (configList[i].Contains("FTP3rdparty"))
                        {
                            foreach (var key in model.SelectedOwner.FolderPathPairs.Keys)
                            {
                                if (configList[i].Contains(key))
                                {
                                    string path = string.Empty;
                                    foreach (var value in model.SelectedOwner.FolderPathPairs.GetValueOrDefault(key))
                                    {
                                        path += Path.Combine(model.BasePath, model.SelectedOwnerName, value) + ";";
                                    }
                                    path = (". . . " + key + "=" + path.Substring(0, path.Length - 1));
                                    updateConfig.Add(path);
                                    i++;
                                    break;
                                }
                            }
                        }
                        else if (configList[i].Contains(". . ImageChecker"))
                        {
                            while (!string.IsNullOrWhiteSpace(configList[i]))
                            {
                                updateConfig.Add("#" + configList[i++]);
                            }
                        }
                        else
                        {
                            updateConfig.Add(configList[i++]);
                        }
                        if(string.IsNullOrWhiteSpace(configList[i]))
                            updateConfig.Add(configList[i]);
                    }
                }
                else
                {
                    updateConfig.Add(configList[i]);
                }
            }
            return updateConfig;
        }

        private string TrimToImporterOnly(string line)
        {
            if (line.Contains(','))
            {
                int idx = line.IndexOf(',');
                return line.Substring(0, idx);
            }
            return line;
        }

        private bool IsOwnerConfig(string line,HomeCreateVM model)
        {
            if(line.Equals(". "+model.SelectedOwnerName)||line.Equals(". " + model.SelectedOwner.ChildOwnerOf))
            {
                return true;
            }
            return false;
        }

    }
}
