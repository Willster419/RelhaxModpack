using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public partial class loadingGifPreview : Form
    {
        int x, y;
        public loadingGifPreview(int newX, int newY)
        {
            InitializeComponent();
            x = newX;
            y = newY;
        }
        //use load to move the location of the form
        private void GifPreview_Load(object sender, EventArgs e)
        {
            Settings.appendToLog("GifPreview: opening at x: " + x + ", y: " +y);
            this.Location = new Point(x,y);
            Settings.setUIColor(this);
            gifPreviewBox.Image = Settings.getLoadingImage();
        }
    }
}
