using System;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public partial class OldFilesToDelete : Form
    {
        public bool result;
        private bool resultChosen = false;
        public OldFilesToDelete()
        {
            InitializeComponent();
        }

        private void OldFilesToDelete_Load(object sender, EventArgs e)
        {
            this.Font = Settings.getFont();
            this.AutoScaleMode = Settings.getAutoScaleMode();
            deleteFilesHeader.Text = Translations.getTranslatedString("foundOldFilesDelete1");
            deleteFilesQuestion.Text = Translations.getTranslatedString("foundOldFilesDelete2");
            noDeleteButton.Text = "no";
            yesDeleteButton.Text = "yes";
            this.Font = Settings.getFont();
            OldFilesToDelete_SizeChanged(null, null);
        }

        private void OldFilesToDelete_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!resultChosen)
                result = false;
        }

        private void yesDeleteButton_Click(object sender, EventArgs e)
        {
            result = true;
            resultChosen = true;
            this.Close();
        }

        private void noDeleteButton_Click(object sender, EventArgs e)
        {
            result = false;
            resultChosen = true;
            this.Close();
        }

        private void OldFilesToDelete_SizeChanged(object sender, EventArgs e)
        {
            filesList.Size = new System.Drawing.Size(this.Size.Width - filesList.Location.X - 20, this.Size.Height - filesList.Location.Y - 40 - yesDeleteButton.Size.Height - 6);
            noDeleteButton.Location = new System.Drawing.Point(filesList.Location.X, filesList.Location.Y + filesList.Size.Height + 6);
            yesDeleteButton.Location = new System.Drawing.Point(this.Size.Width - yesDeleteButton.Size.Width - 20, filesList.Location.Y + filesList.Size.Height + 6);
            deleteFilesQuestion.Location = new System.Drawing.Point((this.Size.Width/2) - (deleteFilesQuestion.Size.Width/2) , filesList.Location.Y + filesList.Size.Height + 6);
        }
    }
}
