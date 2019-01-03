using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using WebAppForSolocoProject.Models;
using WebAppForSolocoProject.Utilities;

namespace WebAppForSolocoProject.Services
{
    public class OwnerData : IOwnerData
    {
        public IEnumerable<Owner> Owners {
            get
            {
                return GetOwnersFromConfig();
            }
        }

        private void CheckForChildOwners(List<Owner> owners)
        {
            for(int i=0; i<owners.Count(); i++)
            {

                for(int j=0; j<owners[i].Paths.Count(); j++)
                {
                    string path = owners[i].Paths[j];
                    if (path.Contains("ChildOwners"))
                    {
                        string[] childOwners = new string[0];
                        int idx = path.IndexOf('=');
                        childOwners = Utility.SplitCSL(",", path.Substring(idx+1));
                        owners[i].Paths.Remove(path);
                        for(int k=0; k<childOwners.Length; k++)
                        {
                            owners.Add(new Owner { Name = childOwners[k], Paths = owners[i].Paths });
                        }
                        owners.Remove(owners[i]);
                        i--;
                    }
                }
            }
        }

        private IEnumerable<Owner> GetOwnersFromConfig()
        {
            List<Owner> Owners = new List<Owner>();
            string config = ConfigurationManager.AppSettings["..."].ToString();
            string[] configList = Utility.SplitCSL(@"\r?\n", config);
            for (int i = 0; i < configList.Length; i++)
            {
                Owner lastOwner = new Owner() { Paths = new List<string>() };
                if (configList[i].Contains("Importers") && configList[i - 1][2] != '.')
                {
                    lastOwner.Name = configList[i - 1].Substring(2);
                    i++;
                    while (!string.IsNullOrWhiteSpace(configList[i]))
                    {
                        if (configList[i].Contains("ChildOwners") || configList[i].Contains("FTP3rdparty"))
                            lastOwner.Paths.Add(configList[i]);
                        i++;
                    }
                    Owners.Add(lastOwner);
                }
            }
            CheckForChildOwners(Owners);
            ConvertStringToPaths(Owners);
            return Owners;
        }

        private void ConvertStringToPaths(List<Owner> owners)
        {
            foreach (var owner in owners)
            {
                List<string> updatedPaths = new List<string>();

                foreach (var path in owner.Paths)
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
                owner.Paths = updatedPaths;
            }
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
