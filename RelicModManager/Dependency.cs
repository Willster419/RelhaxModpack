using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    //a dependency is a zip file like mod that is required for any of the mods to work
    //i.e. and sound mods require the sound memory to be increased
    public class Dependency
    {
        public string dependencyZipFile { get; set; }
        public string dependencyZipCRC { get; set; }
        public bool enabled { get; set; }
        public Dependency() { }
    }
}
