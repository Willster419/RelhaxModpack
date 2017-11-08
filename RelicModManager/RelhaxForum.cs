using System;
using System.Drawing;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public class RelhaxForum : Form
    {
        private const int TitleBar = 23;//set origionaly for 23
        private int TitleBarDifference = 0;
        private int TitleHeight = 0;
        protected override void OnLoad(EventArgs e)
        {
            //font scaling
            AutoScaleMode = Settings.AppScalingMode;
            Font = Settings.AppFont;
            if (Settings.AppScalingMode == AutoScaleMode.Dpi)
            {
                this.Scale(new SizeF(Settings.ScaleSize, Settings.ScaleSize));
            }
            //title bar calculation
            Rectangle screenRektangle = RectangleToScreen(ClientRectangle);
            TitleHeight = screenRektangle.Top - Top;
            if (TitleHeight > TitleBar)
            {
                TitleBarDifference = TitleHeight - TitleBar;
            }
            Settings.setUIColor(this);
            base.OnLoad(e);
        }
    }
}
