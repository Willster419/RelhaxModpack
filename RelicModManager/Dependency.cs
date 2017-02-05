using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelicModManager
{
    public class Dependency
    {
        public string dependencyZipFile { get; set; }
        public string dependencyZipCRC { get; set; }
        public bool enabled { get; set; }
        public Dependency() { }
    }
}
