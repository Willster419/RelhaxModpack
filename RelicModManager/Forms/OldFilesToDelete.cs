using System;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public partial class OldFilesToDelete : RelhaxForum
    {
        public bool result;
        private bool resultChosen = false;
        public OldFilesToDelete()
        {
            InitializeComponent();
            this.Text = Translations.GetTranslatedString("foundOldFilesHeader");
        }

        private void OldFilesToDelete_Load(object sender, EventArgs e)
        {
            deleteFilesHeader.Text = Translations.GetTranslatedString("foundOldFilesDelete1");
            deleteFilesQuestion.Text = Translations.GetTranslatedString("foundOldFilesDelete2");
            noDeleteButton.Text = Translations.GetTranslatedString("no");
            yesDeleteButton.Text = Translations.GetTranslatedString("yes");
            if (Program.autoInstall)
                yesDeleteButton_Click(null, null);
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
    }
}
