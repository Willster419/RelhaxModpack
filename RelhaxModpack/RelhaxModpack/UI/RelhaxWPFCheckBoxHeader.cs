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
    public class RelhaxWPFCheckBoxHeader : RelhaxWPFCheckBox
    {
        static RelhaxWPFCheckBoxHeader()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RelhaxWPFCheckBoxHeader), new FrameworkPropertyMetadata(typeof(RelhaxWPFCheckBoxHeader)));
        }

        public RelhaxWPFCheckBoxHeader() : base()
        {

        }

        public override void OnCheckedChanged(bool Checked)
        {
            base.OnCheckedChanged(Checked);

            //Package.TabIndex.OnCheckedChanged(Checked);
        }
    }
}
