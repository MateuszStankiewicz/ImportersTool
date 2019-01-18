﻿using System.Collections.Generic;

namespace WebAppForSolocoProject.Models
{
    public class Owner
    {
        public string Name { get; set; }
        public List<string> Paths { get; set; }
        public List<string> SourceFolders { get; set; }
        public List<string> QualityFolders { get; set; }
    }
}
