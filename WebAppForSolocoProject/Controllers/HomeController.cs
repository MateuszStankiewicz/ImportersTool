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
            await UpdateModel(model);
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
                        foreach (var key in model.SelectedOwner.FolderPathPairs.Keys)
                        {
                            foreach (var path in model.SelectedOwner.FolderPathPairs.GetValueOrDefault(key))
                            {
                                string pathToCreate = Path.Combine(model.BasePath, model.SelectedOwnerName, path);
                                model.Logs.Add(CreateDirectory(pathToCreate));
                            }
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

            List<string> sourceFolders = model.SelectedOwner.FolderPathPairs.GetValueOrDefault("SourceFolder");
            if (sourceFolders == null)
            {
                model = await CreateDirectories(model);
                model.Logs.Add("Selected owner does not need initialize files.");
            }
            else
            {
                if (model.SelectedFolder != null)
                    model.PathToSaveFile = sourceFolders.FirstOrDefault(s => s.Contains(model.SelectedFolder));
                else
                    model.PathToSaveFile = sourceFolders.FirstOrDefault();

                var files = new TaskCompletionSource<List<string>>();

                Thread thread = new Thread(() => BrowseFiles(files));
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                model.Files = await files.Task;

                if(model.Files.Count()!=0)
                    model = await CreateDirectories(model);

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
                openFileDialog.Filter = Utility.importerFileExtensions;
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

        public async Task<IActionResult> UpdateConfig(HomeCreateVM model)
        {
            await UpdateModel(model);
            var appConfigFile = await ManageConfigFile.ParseAppConfigToStringArrayAsync();
            var updateConfig = RewriteConfig(model, appConfigFile);
            string path = await SaveToFile(updateConfig);

            if (path != null)
            {
                model.Logs.Add("Config succesfully saved in "+ path);
            }

            return View("Create", model);
        }

        private List<string> RewriteConfig(HomeCreateVM model, string[] configList)
        {
            List<string> updateConfig = new List<string>();

            foreach (var line in configList)
            {
                if (line.Contains("OwnersEnabled"))
                {
                    updateConfig.Add(". OwnersEnabled=" + model.SelectedOwnerName);
                    continue;
                }
                if (IsOwnerConfig(line,model))
                {
                    foreach (var key in model.SelectedOwner.FolderPathPairs.Keys)
                    {
                        if (line.Contains(key) && line.Contains("FTP3rdparty"))
                        {
                            string path = string.Empty;
                            int idx = line.IndexOf('=');
                            foreach (var value in model.SelectedOwner.FolderPathPairs.GetValueOrDefault(key))
                            {
                                path += Path.Combine(model.BasePath, model.SelectedOwnerName, value)+";";
                            }
                            path = line.Substring(0, idx + 1) + path.Substring(0, path.Length - 1);
                            updateConfig.Add(path);
                            break;
                        }
                    }
                }
                else
                {
                    updateConfig.Add(line);
                }
            }
            return updateConfig;
        }

        private bool IsOwnerConfig(string line,HomeCreateVM model)
        {
            foreach (var config in model.SelectedOwner.Config)
            {
                if (line.Equals(config))
                    return true;
            }
            return false;
        }

        private async Task<string> SaveToFile(List<string> updateConfig)
        {
            var t = new TaskCompletionSource<string>();
            Thread thread = new Thread(() => SaveFileDialog(t));
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            string path = await t.Task;

            if (path != null)
            {
                await System.IO.File.WriteAllLinesAsync(path, updateConfig);
            }

            return path;
        }

        private void SaveFileDialog(TaskCompletionSource<string> file)
        {
            string path = null;

            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.FileName = "app";
                saveFileDialog.DefaultExt = "config";
                saveFileDialog.Filter = Utility.configFileExtensions;
                saveFileDialog.InitialDirectory =
            Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    path = saveFileDialog.FileName;
                }
            }
            file.SetResult(path);
        }
    }
}
