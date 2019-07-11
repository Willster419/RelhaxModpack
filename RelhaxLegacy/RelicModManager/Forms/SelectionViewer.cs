using System;
using System.Windows.Forms;
using System.Net;
using System.Drawing;

namespace RelhaxModpack
{
    public partial class SelectionViewer : RelhaxForum
    {
        private int x, y;
        private WebClient client = new WebClient();
        private string url = "";
        public string SelectedXML = "";
        private string SelectedDocument = "";
        public SelectionViewer(int xx, int yy, string urll)
        {
            InitializeComponent();
            //parse in the new ints location of where to display the application
            x = xx;
            y = yy;
            url = urll;
        }
        private void SelectionViewer_Load(object sender, EventArgs e)
        {
            this.Location = new Point(x, y);
            SelectConfigLabel.Text = Translations.GetTranslatedString(SelectConfigLabel.Name);
            SelectButton.Text = Translations.GetTranslatedString("select");
            CancelCloseButton.Text = Translations.GetTranslatedString("cancel");

            SelectionRadioButton b = new SelectionRadioButton
            {
                XMLURL = "localFile",
                Text = Translations.GetTranslatedString("localFile"),
                AutoSize = true
            };
            b.Location = new Point(6, GetYLocation(SelectConfigPanel.Controls));
            SelectConfigPanel.Controls.Add(b);

            foreach (var node in XMLUtils.developerSelections)
            {
                SelectionRadioButton bb = new SelectionRadioButton
                {
                    XMLURL = node.internalName,
                    Text = node.displayName,
                    AutoSize = true
                };
                bb.Location = new Point(6, GetYLocation(SelectConfigPanel.Controls));
                // add ToolTip to the develeopersSelections
                ToolTip rbToolTip = new ToolTip
                {
                    // Set up the delays for the ToolTip.
                    AutoPopDelay = 5000,
                    InitialDelay = 1000,
                    ReshowDelay = 500,
                    // Force the ToolTip text to be displayed whether or not the form is active.
                    ShowAlways = true
                };
                // create Date and Time with local syntax
                Utils.ConvertDateToLocalCultureFormat(node.date, out string cultureDate);
                // Set up the ToolTip text for the Button and Checkbox.
                rbToolTip.SetToolTip(bb, string.Format(Translations.GetTranslatedString("createdAt"), cultureDate));
                SelectConfigPanel.Controls.Add(bb);
            }
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            SelectedDocument = this.GetSelectedXMLDoc();
            if(SelectedDocument.Equals("-1"))
            {
                //error
                Logging.Manager("ERROR: Failed to parse XML File");
                SelectedXML = SelectedDocument;
            }
            else if (SelectedDocument.Equals("localFile"))
            {
                //local file dialog box
                SelectedXML = SelectedDocument;
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                SelectedXML = string.Format("{0},{1}", Settings.ModInfoDatFile, SelectedDocument);
                this.DialogResult = DialogResult.OK;
            }
            this.Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private string GetSelectedXMLDoc()
        {
            foreach(Control c in SelectConfigPanel.Controls)
            {
                if (c is SelectionRadioButton serb)
                {
                    if (serb.Checked)
                    {
                        return serb.XMLURL;
                    }
                }
            }
            return "-1";
        }

        //method for finding the location of which to put a control
        private int GetYLocation(Control.ControlCollection ctrl)
        {
            //initial padding
            int y = 2;
            //only look for the dropDown menu options or checkboxes
            foreach (SelectionRadioButton c in ctrl)
            {
                y += c.Size.Height;
                //spacing
                y += 2;
            }
            return y;
        }
    }
}
