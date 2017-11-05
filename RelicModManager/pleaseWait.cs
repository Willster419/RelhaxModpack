using System;
using System.Windows.Forms;

namespace RelhaxModpack
{
    //the loading window to show on startup
    public partial class PleaseWait : Form
    {
        private int startX = -1;
        private int startY = -1;
        private int PGMAX = 0;
        public float progres_max;
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
            //font scaling
            this.AutoScaleMode = Settings.AppScalingMode;
            this.Font = Settings.AppFont;
            if (Settings.AppScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new System.Drawing.SizeF(Settings.ScaleSize, Settings.ScaleSize));
            }
            if (startY != -1 && startX != -1)
                this.Location = new System.Drawing.Point(startX + 10, startY);
            label1.Text = Translations.getTranslatedString(label1.Name);
            Settings.setUIColor(this);
            //align the text box to the middle of the forum
            label1.Location = new System.Drawing.Point((this.Size.Width / 2) - (label1.Size.Width/2), label1.Location.Y);
            PGMAX = forgroundPanel.Size.Width;
            forgroundPanel.BackColor = System.Drawing.Color.Blue;
            forgroundPanel.ForeColor = System.Drawing.Color.Blue;
            forgroundPanel.Size = new System.Drawing.Size(0, forgroundPanel.Size.Height);
        }
        public void SetProgress(int progres_value)
        {
            float tempVal = (float)progres_value;
            float percent = tempVal / progres_max;
            //percent = percent * 100;
            if (0 <= percent && percent <= 100)
            {
                int temp = (int) (PGMAX * percent);
                forgroundPanel.Size = new System.Drawing.Size(temp, forgroundPanel.Size.Height);
            }
        }
    }
}
