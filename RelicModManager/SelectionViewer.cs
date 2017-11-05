using System;
using System.Windows.Forms;
using System.Net;
using System.Drawing;
using System.Xml;

namespace RelhaxModpack
{
    public partial class SelectionViewer : Form
    {
        private int x, y;
        private WebClient client = new WebClient();
        private string url = "";
        private int difference;
        public string SelectedXML = "";
        private string SelectedDocument = "";
        private const int titleBar = 23;//set origionally for 23
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
            //setting UI color
            Settings.setUIColor(this);
            //font scaling
            this.AutoScaleMode = Settings.AppScalingMode;
            this.Font = Settings.AppFont;
            if (Settings.AppScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new SizeF(Settings.ScaleSize, Settings.ScaleSize));
            }
            //title bar height
            //get the size of the title bar window
            Rectangle screenRektangle = RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRektangle.Top - this.Top;
            //largest possible is 46
            //mine (programmed for) is 23
            if (titleHeight > titleBar)
            {
                difference = titleHeight - titleBar;
            }
            //this.DialogResult = DialogResult.Cancel;
            SelectConfigLabel.Text = Translations.getTranslatedString(SelectConfigLabel.Name);
            SelectButton.Text = Translations.getTranslatedString("select");
            CancelCloseButton.Text = Translations.getTranslatedString("cancel");

            SelectionRadioButton b = new SelectionRadioButton
            {
                XMLURL = "localFile",
                Text = Translations.getTranslatedString("localFile")
            };
            b.Location = new Point(6, (SelectConfigPanel.Controls.Count * (b.Size.Height-3)));
            SelectConfigPanel.Controls.Add(b);

            foreach (var node in MainWindow.developerSelections)
            {
                SelectionRadioButton bb = new SelectionRadioButton
                {
                    XMLURL = node.internalName,
                    Text = node.displayName
                };
                bb.Location = new Point(6, (SelectConfigPanel.Controls.Count * (b.Size.Height-3)));
                // add ToolTip to the develeopersSelections
                ToolTip rbToolTip = new ToolTip();
                // Set up the delays for the ToolTip.
                rbToolTip.AutoPopDelay = 5000;
                rbToolTip.InitialDelay = 1000;
                rbToolTip.ReshowDelay = 500;
                // Force the ToolTip text to be displayed whether or not the form is active.
                rbToolTip.ShowAlways = true;
                // create Date and Time with local syntax
                string cultureDate = "";
                Utils.ConvertDateToLocalCultureFormat(node.date, out cultureDate);
                // Set up the ToolTip text for the Button and Checkbox.
                rbToolTip.SetToolTip(bb, string.Format(Translations.getTranslatedString("createdAt"), cultureDate));
                SelectConfigPanel.Controls.Add(bb);
            }
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            SelectedDocument = this.getSelectedXMLDoc();
            if(SelectedDocument.Equals("-1"))
            {
                //error
                Utils.AppendToLog("ERROR: Failed to parse XML File");
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

        private string getSelectedXMLDoc()
        {
            foreach(Control c in SelectConfigPanel.Controls)
            {
                if(c is SelectionRadioButton)
                {
                    SelectionRadioButton serb = (SelectionRadioButton)c;
                    if(serb.Checked)
                    {
                        return serb.XMLURL;
                    }
                }
            }
            return "-1";
        }
    }
}
