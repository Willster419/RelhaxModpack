using RelhaxModpack.Automation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// Interaction logic for AutomationListBoxItem.xaml
    /// </summary>
    public partial class AutomationListBoxItem : ListBoxItem
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
        /// Create an instance of the AutomationListBoxItem class.
        /// </summary>
        public AutomationListBoxItem()
        {
            InitializeComponent();
        }

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
