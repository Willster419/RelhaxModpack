using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RelhaxModpack.UIComponents;

namespace RelhaxModpack.Forms
{
    public partial class ExportSelectWoTVersion : RelhaxForum
    {

        public List<ExportModeRadioButton> SupportedWoTVersions;

        public ExportModeRadioButton selectedVersion;

        public ExportSelectWoTVersion()
        {
            InitializeComponent();
        }

        private void ExportSelectWoTVersion_Load(object sender, EventArgs e)
        {
            //load translations
            Name = Translations.getTranslatedString("ExportModeCB");
            ExportWindowDesctiption.Text = Translations.getTranslatedString(ExportWindowDesctiption.Name);
            CancelButton.Text = Translations.getTranslatedString("cancel");
            SelectButton.Text = Translations.getTranslatedString("select");
            //load panel with stuff
            foreach(ExportModeRadioButton rb in SupportedWoTVersions)
            {
                int newYLocation = WoTVersionsHolder.Controls.Count * (rb.Size.Height);
                rb.Location = new Point(3, newYLocation);
                WoTVersionsHolder.Controls.Add(rb);
            }
            //check the latest version
            SupportedWoTVersions[SupportedWoTVersions.Count - 1].Checked = true;
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            foreach(Control c in SupportedWoTVersions)
            {
                ExportModeRadioButton rb = (ExportModeRadioButton)c;
                if(rb.Checked)
                {
                    selectedVersion = rb;
                    break;
                }
            }
            DialogResult = DialogResult.OK;
        }
    }
}
