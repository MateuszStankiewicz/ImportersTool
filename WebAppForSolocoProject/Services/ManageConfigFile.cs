using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppForSolocoProject.Utilities
{
    public static class ManageConfigFile
    {
        public static string[] ParseAppSettingsToStringArray()
        {
            string config = ConfigurationManager.AppSettings["..."].ToString();
            return Utility.SplitCSL(@"\r?\n", config);
        }

        public static async Task<string[]> ParseAppConfigToStringArrayAsync()
        {
            return await File.ReadAllLinesAsync(Path.Combine(Directory.GetCurrentDirectory(), "app.config"));
        }
    }
}
