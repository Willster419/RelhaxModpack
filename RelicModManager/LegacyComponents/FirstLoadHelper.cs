using System;
using System.Drawing;
using System.Windows.Forms;

namespace RelhaxModpack
{
    //The window will show on the applications first load. Its text will change
    //based on where the mouse is hovering
    public partial class FirstLoadHelper : Form
    {
        int x, y;
        public FirstLoadHelper(int xx, int yy)
        {
            InitializeComponent();
            //parse in the new ints location of where to display the application
            x = xx;
            y = yy;
        }

        //use the ints to set a new start location
        private void FirstLoadHelper_Load(object sender, EventArgs e)
        {
            //log info
            Utils.AppendToLog("FirstLoadHelper: startup location is x: " + x + ", y: " + y);
            this.Location = new Point(x, y);
            //setting UI color
            Settings.setUIColor(this);
            //font scaling
            this.AutoScaleMode = Settings.AppScalingMode;
            helperText.Font = Settings.AppFont;
            this.Font = Settings.AppFont;
            if (Settings.AppScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new SizeF(Settings.ScaleSize, Settings.ScaleSize));
            }
        }
    }
}
