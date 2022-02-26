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
    /// <summary>
    /// Allows for display (ToString) of an AutomationSequence for the automation selection list, and allows for color change of the display text based on the output code of the sequence's run.
    /// </summary>
    public class AutomationListBoxItem : ListBoxItem
    {
        /// <summary>
        /// The default color of the foreground brush when this component is loaded.
        /// </summary>
        public Brush DefaultForgroundBrush { get; set; }

        /// <summary>
        /// The automation sequence.
        /// </summary>
        public AutomationSequence AutomationSequence { get; set; }

        /// <summary>
        /// Display the AutomationSequence ToString return value.
        /// </summary>
        /// <returns>The value of the AutomationSequence.ToString() method.</returns>
        /// <seealso cref="AutomationSequence"/>
        public override string ToString()
        {
            return AutomationSequence.ToString();
        }
    }
}
