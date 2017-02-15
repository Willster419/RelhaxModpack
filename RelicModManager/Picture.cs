using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelicModManager
{
    public class Picture
    {
        //two-part variable. specifies wether it's part of a mod, or a mod's config
        //and the mod/config name
        public string name { get; set; }
        public string URL { get; set; }
        //constructor to setup the picture with the "name" and
        //the URL where the picture is located
        public Picture(string newName, string newURL)
        {
            name = newName;
            URL = newURL;
        }
    }
}
