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
            deleteFilesHeader.Text = Translations.getTranslatedString("foundOldFilesDelete1");
            deleteFilesQuestion.Text = Translations.getTranslatedString("foundOldFilesDelete2");
            noDeleteButton.Text = "no";
            yesDeleteButton.Text = "yes";
            this.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
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
