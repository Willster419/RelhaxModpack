using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.DatabaseComponents
{
    public class PatchUpdate
    {
        public string PatchesToUpdate { get; set; }
        public string XPath { get; set; }
        public string Search { get; set; }
        public bool SearchReturnFirst { get; set; }
        public string Replace { get; set; }
    }
}
