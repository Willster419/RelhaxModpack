using System;
using System.Windows.Forms;

namespace RelhaxModpack
{
    //the loading window to show on startup
    public partial class PleaseWait : Form
    {
        public PleaseWait()
        {
            InitializeComponent();
        }

        private void PleaseWait_Load(object sender, EventArgs e)
        {
            label1.Text = Translations.getTranslatedString(label1.Name);
            Settings.setUIColor(this);
        }
    }
}
