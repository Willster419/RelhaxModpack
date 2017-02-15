using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelicModManager
{
    //a patch is an instruction of how to modify a text file
    //generally a mod config file
    class Patch
    {
        //the type of patch, xml or regex (direct text replacement)
        public string type { get; set; }
        //if xml, the mode that the xml patcher should use
        //add xml node, remove xml node, edit xml node
        public string mode { get; set; }
        //the path to the file, relative to res_mods
        public string file { get; set; }
        //if xml, the xml xpath to the node
        public string path { get; set; }
        //if regex, the optional specific lines in the text file
        //to make the modifications
        public string[] lines { get; set; }
        //the node inner text (xml) or regex criteria to search for
        public string search { get; set; }
        //the text to replace the found search text with
        public string replace { get; set; }
        public Patch() { }
    }
}
