using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WebAppForSolocoProject.Utilities
{
    public class Utility
    {
        public static string[] SplitCSL(string re, string csl)
        {
            if (csl == null) return null;
            if (csl == "") return new string[0];
            return Regex.Split(csl, re);
        }
    }
}
