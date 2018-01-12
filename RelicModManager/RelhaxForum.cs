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
            if (Settings.AppScalingMode == AutoScaleMode.Dpi)
            {
                AutoScaleDimensions = new SizeF(96F, 96F);// for design in 96 DPI
                AutoScaleMode = Settings.AppScalingMode;
                Scale(new SizeF(Settings.ScaleSize, Settings.ScaleSize));
            }
            else if (Settings.AppScalingMode == AutoScaleMode.Font)
            {
                AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F); // for design in 96 DPI
            }
            else
            {

            }
            Font = Settings.AppFont;
            //set the UI colors
            Settings.setUIColor(this);
            ResumeLayout(false);
        }
    }
}
