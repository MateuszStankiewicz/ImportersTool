using System;
using System.Collections.Generic;
using System.Linq;
using WebAppForSolocoProject.Models;
using WebAppForSolocoProject.Utilities;


namespace WebAppForSolocoProject.Services
{
    public class OwnerData 
    {
        public string BasePath { get; set; }
        public string SelectedOwner { get; set; }
        public string SelectedFolder { get; set; }

        public IEnumerable<Owner> GetOwners()
        {
            List<Owner> Owners = new List<Owner>();
            Owners = ReadFromConfig();
            Owners = CheckForChildOwners(Owners);
            RemoveStringThatIsNotPath(Owners);
            ConvertConfigToFolderPathPair(Owners);
            return Owners.OrderBy(o => o.Name);
        }

        private List<Owner> ReadFromConfig()
        {
            List<Owner> owners = new List<Owner>();
            string[] configList = ManageConfigFile.ParseAppSettingsToStringArray();

            for (int i = 0; i < configList.Length; i++)
            {
                if (configList[i].Contains("Importers") && configList[i - 1][2] != '.')
                {
                    Owner owner = new Owner()
                    {
                        Config = new List<string>(),
                        QualityFolders = new List<Quality>()
                    };
                    owner.Name = configList[i - 1].Substring(2);
                    i++;

                    while (!string.IsNullOrWhiteSpace(configList[i])&&!configList[i].Contains("ImageChecker"))
                    {
                        owner.Config.Add(configList[i]);
                        i++;
                    }
                    owners.Add(owner);
                }
            }
            return owners;
        }

        private List<Owner> CheckForChildOwners(List<Owner> owners)
        {
            List<Owner> updatedOwners = new List<Owner>();
            foreach (var owner in owners)
            {
                bool haveChildOwners = false;
                foreach (var path in owner.Config)
                {
                    if(path.Contains("ChildOwners"))
                    {
                        haveChildOwners = true;
                        string[] childOwners = new string[0];
                        int idx = path.IndexOf('=');
                        childOwners = Utility.SplitCSL(",", path.Substring(idx+1));
                        foreach (var child in childOwners)
                        {
                            updatedOwners.Add(new Owner()
                            {
                                Name = child,
                                Config = owner.Config,
                                QualityFolders = owner.QualityFolders,
                                ChildOwnerOf = owner.Name
                            });
                        }
                    }
                }
                if (!haveChildOwners)
                    updatedOwners.Add(owner);
            }
            return updatedOwners;
        }

        private void RemoveStringThatIsNotPath(List<Owner> owners)
        {            
            foreach (var owner in owners)
            {                
                List<string> updatedPaths = new List<string>();
                foreach (var path in owner.Config)
                {
                    if (path.Contains("FTP3rdparty"))
                    {
                        updatedPaths.Add(path);
                        if(path.Contains("SourceFolder"))
                        {
                            foreach (Quality quality in Enum.GetValues(typeof(Quality)))
                            {
                                if(path.Contains(quality.ToString()) && !owner.QualityFolders.Any(q=>q==quality))
                                    owner.QualityFolders.Add(quality);
                            }
                        }
                    }
                }
                owner.Config = updatedPaths;
            }
        }

        private void ConvertConfigToFolderPathPair(List<Owner> owners)
        {
            foreach (var owner in owners)
            {
                Dictionary<string, List<string>> folderPathPairs = new Dictionary<string, List<string>>();
                foreach (var path in owner.Config)
                {
                    string key = TrimToFolder(path);
                    List<string> paths = ConvertStringToPaths(path);
                    if(!folderPathPairs.ContainsKey(key))
                    {
                        folderPathPairs.Add(key, paths);
                    }
                }
                owner.FolderPathPairs = folderPathPairs;
            }
        }

        private string TrimToFolder(string path)
        {
            int idx = path.IndexOf('=');
            return path.Substring(6,idx-6);
        }

        private List<string> ConvertStringToPaths(string path)
        {
            List<string> updatedPaths = new List<string>();
            string[] paths = new string[0];
            if (path.Contains(";"))
            {
                paths = Utility.SplitCSL(";", path);
                foreach (var splitPath in paths)
                {
                    updatedPaths.Add(TrimToPath(splitPath));
                }
            }
            else
            {
                updatedPaths.Add(TrimToPath(path));
            }
            return updatedPaths;
        }

        private string TrimToPath(string path)
        {
            int idx = path.LastIndexOf(@"FTP3rdparty\");
            path =  path.Substring(idx + 12);
            idx = path.IndexOf('\\');
            return path.Substring(idx+1);
        }

        public enum Quality
        {
            SD,
            HD
        }

    }
}
