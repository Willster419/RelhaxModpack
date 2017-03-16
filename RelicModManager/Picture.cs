using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
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
        //sorts the mods
        public static int ComparePictures(Picture x, Picture y)
        {
            //name looks like this
            //Mod:name
            //Config:name
            //seperate all 4 things
            string xType = x.name.Split(':')[0];
            string yType = y.name.Split(':')[0];
            string xName = x.name.Split(':')[1];
            string yName = y.name.Split(':')[1];
            //check the mod vs config first
            int typeResult = xType.CompareTo(yType);
            if (typeResult == 0)
            {
                //eithor both mod or both config, sort alphabetically
                return xName.CompareTo(yName);
            }
            //mod first, then config
            return typeResult*-1;
        }
    }
}
