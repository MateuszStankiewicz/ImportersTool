using System.Collections.Generic;

namespace WebAppForSolocoProject.Models
{
    public class Owner
    {
        public string Name { get; set; }
        public List<string> Paths { get; set; } = new List<string>();

        public override string ToString()
        {
            return Name.ToString();
        }
    }
}
