using RelhaxModpack.Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace RelhaxModpack.UI
{
    public class AutomationListBoxItem : ListBoxItem
    {
        public Brush DefaultForgroundBrush { get; set; }

        public AutomationSequence AutomationSequence { get; set; }

        public override string ToString()
        {
            return AutomationSequence.ToString();
        }
    }
}
