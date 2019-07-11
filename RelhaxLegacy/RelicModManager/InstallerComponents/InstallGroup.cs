using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack.InstallerComponents
{
    //a class used for creating groups of categories that must be extracted one after the other
    public class InstallGroup
    {
        //the list of categories
        public List<Category> Categories { get; set; }
        public InstallGroup()
        {
            Categories = new List<Category>();
        }
    }
}
