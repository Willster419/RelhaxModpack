using System;
using System.Windows.Forms;

namespace RelhaxModpack
{
    //the loading window to show on startup
    public partial class PleaseWait : Form
    {
        private int startX = -1;
        private int startY = -1;
        public PleaseWait()
        {
            InitializeComponent();
        }
        public PleaseWait(int x, int y)
        {
            startX = x;
            startY = y;
            InitializeComponent();
        }
        private void PleaseWait_Load(object sender, EventArgs e)
        {
            if (startY != -1 && startX != -1)
                this.Location = new System.Drawing.Point(startX + 10, startY);
            label1.Text = Translations.getTranslatedString(label1.Name);
            Settings.setUIColor(this);
        }
    }
}
