using System;
using System.Drawing;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public class RelhaxForum : Form
    {
        protected override void OnLoad(EventArgs e)
        {
            SuspendLayout();
            base.OnLoad(e);
            //font scaling
            //AutoScaleDimensions = new SizeF(96F, 96F);
            //AutoScaleMode = Settings.AppScalingMode;
            //Font = Settings.AppFont;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F); // for design in 96 DPI
            this.AutoScaleMode = AutoScaleMode.Inherit;
            /*
            if (Settings.AppScalingMode == AutoScaleMode.Dpi)
            {
                this.Scale(new SizeF(Settings.ScaleSize, Settings.ScaleSize));
            }
            */
            //set the UI colors
            Settings.setUIColor(this);
            ResumeLayout(false);
        }
    }
}
