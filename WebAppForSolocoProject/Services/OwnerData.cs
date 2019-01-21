using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using WebAppForSolocoProject.Models;
using WebAppForSolocoProject.Utilities;
using System.Diagnostics;

namespace WebAppForSolocoProject.Services
{
    public class OwnerData 
    {
        public IEnumerable<Owner> GetOwners()
        {
            List<Owner> Owners = new List<Owner>();
            Owners = ReadFromConfig();
            Owners = CheckForChildOwners(Owners);
            RemoveStringThatIsNotPath(Owners);
            foreach (var owner in Owners)
            {
                owner.Paths = ConvertStringToPaths(owner.Paths);
                owner.SourceFolders = ConvertStringToPaths(owner.SourceFolders);
            }
            return Owners.OrderBy(o => o.Name);
        }

        private List<Owner> ReadFromConfig()
        {
            List<Owner> owners = new List<Owner>();
            string config = ConfigurationManager.AppSettings["..."].ToString();
            string[] configList = Utility.SplitCSL(@"\r?\n", config);
            for (int i = 0; i < configList.Length; i++)
            {
                Owner owner = new Owner()
                {
                    Paths = new List<string>(),
                    SourceFolders = new List<string>(),
                    QualityFolders = new List<string>()
                };
                if (configList[i].Contains("Importers") && configList[i - 1][2] != '.')
                {
                    owner.Name = configList[i - 1].Substring(2);
                    i++;
                    while (!string.IsNullOrWhiteSpace(configList[i]))
                    {
                        owner.Paths.Add(configList[i]);
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
                foreach (var path in owner.Paths)
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
                                Paths = owner.Paths,
                                SourceFolders = owner.SourceFolders,
                                QualityFolders = owner.QualityFolders
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
                foreach (var path in owner.Paths)
                {
                    if (path.Contains("FTP3rdparty") && owner.Paths.Any(p=>p.Contains(path)))
                    {
                        updatedPaths.Add(path);
                        if(path.Contains("SourceFolder"))
                        {
                            owner.SourceFolders.Add(path);
                            if (path.Contains("SD") && !owner.QualityFolders.Any(q=>q.Contains("SD")))
                                owner.QualityFolders.Add("SD");
                            if (path.Contains("HD") && !owner.QualityFolders.Any(q => q.Contains("HD")))
                                owner.QualityFolders.Add("HD");
                        }
                    }
                }
                owner.Paths = updatedPaths;
            }
        }

        private List<string> ConvertStringToPaths(List<string> strings)
        {
                List<string> updatedPaths = new List<string>();

                foreach (var path in strings)
                {
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
                }
                return updatedPaths;
        }

        private string TrimToPath(string path)
        {
            int idx = path.IndexOf('=');
            string pathToAdd = (idx == -1) ? path : path.Substring(idx + 1);
            idx = pathToAdd.LastIndexOf(@"FTP3rdparty\");
            return pathToAdd.Substring(idx + 12);
        }

    }
}
