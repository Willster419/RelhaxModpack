using System;
using System.Drawing;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public partial class LoadingGifPreview : Form
    {
        int x, y;
        public LoadingGifPreview(int newX, int newY)
        {
            InitializeComponent();
            x = newX;
            y = newY;
        }
        //use load to move the location of the form
        private void GifPreview_Load(object sender, EventArgs e)
        {
            //font scaling
            this.AutoScaleMode = Settings.appScalingMode;
            this.Font = Settings.appFont;
            if (Settings.appScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new SizeF(Settings.scaleSize, Settings.scaleSize));
            }
            Utils.appendToLog("GifPreview: opening at x: " + x + ", y: " + y);
            this.Location = new Point(x, y);
            Settings.setUIColor(this);
            SetLoadingImage();
        }

        private void LoadingGifPreview_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        //sets the loading image
        public void SetLoadingImage()
        {
            gifPreviewBox.Image = Settings.getLoadingImage();
        }
    }
}
