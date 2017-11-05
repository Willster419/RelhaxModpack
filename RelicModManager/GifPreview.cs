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
            this.AutoScaleMode = Settings.AppScalingMode;
            this.Font = Settings.AppFont;
            if (Settings.AppScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new SizeF(Settings.ScaleSize, Settings.ScaleSize));
            }
            Utils.AppendToLog("GifPreview: opening at x: " + x + ", y: " + y);
            this.Location = new Point(x, y);
            Settings.setUIColor(this);
            this.Text = Translations.getTranslatedString("loadingGifpreview");
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
