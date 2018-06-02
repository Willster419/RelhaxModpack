using System;
using System.Windows.Forms;

namespace RelhaxModpack.Forms
{
    public partial class FontSettingsVerify : RelhaxForum
    {
        private int scalingTimeout = 7;
        public int startX, startY;

        public FontSettingsVerify()
        {
            InitializeComponent();
        }

        private void FontSettingsVerify_Load(object sender, EventArgs e)
        {
            timer1.Start();
            Location = new System.Drawing.Point(startX, startY);
            RevertingTimeoutText.Text = string.Format(Translations.GetTranslatedString(RevertingTimeoutText.Name), scalingTimeout--);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            RevertingTimeoutText.Text = string.Format(Translations.GetTranslatedString(RevertingTimeoutText.Name), scalingTimeout--);
            if (scalingTimeout < 0)
            {
                timer1.Stop();
                DialogResult = DialogResult.No;
            }
        }

        private void NoButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
        }

        private void YesButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
        }

        private void FontSettingsVerify_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(DialogResult != DialogResult.Yes)
                DialogResult = DialogResult.No;
        }
    }
}
