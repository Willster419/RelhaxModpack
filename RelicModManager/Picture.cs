using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelicModManager
{
    public class Picture
    {
        public string name { get; set; }
        public string URL { get; set; }
        public Picture()
        {

        }
        public Picture(string newName, string newURL)
        {
            name = newName;
            URL = newURL;
        }
    }
}
