using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.Database
{
    public class PatchUpdate
    {
        public string PatchesToUpdate { get; set; }
        public string XPath { get; set; }
        public string Search { get; set; }
        public bool SearchReturnFirst { get; set; }
        public string Replace { get; set; }

        public string PatchUpdateInformation
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                builder.Append(Environment.NewLine);
                builder.AppendFormat("{0}{1}: {2}{3}", Settings.LogSpacingLineup, nameof(PatchesToUpdate), CommonUtils.EmptyNullStringCheck(PatchesToUpdate), Environment.NewLine);
                builder.AppendFormat("{0}{1}: {2}{3}", Settings.LogSpacingLineup, nameof(XPath), CommonUtils.EmptyNullStringCheck(XPath), Environment.NewLine);
                builder.AppendFormat("{0}{1}: {2}{3}", Settings.LogSpacingLineup, nameof(Search), CommonUtils.EmptyNullStringCheck(Search), Environment.NewLine);
                builder.AppendFormat("{0}{1}: {2}{3}", Settings.LogSpacingLineup, nameof(SearchReturnFirst), CommonUtils.EmptyNullStringCheck(SearchReturnFirst.ToString()), Environment.NewLine);
                builder.AppendFormat("{0}{1}: {2}{3}", Settings.LogSpacingLineup, nameof(Replace), CommonUtils.EmptyNullStringCheck(Replace), Environment.NewLine);

                return builder.ToString();
            }
        }
    }
}
