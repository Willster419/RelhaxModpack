using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

namespace RelhaxModpack
{
    public partial class PatchTester : Form
    {
        private string xvmMode = "";
        public PatchTester()
        {
            InitializeComponent();
        }

        private void xmlAddModeButton_CheckedChanged(object sender, EventArgs e)
        {
            xmlSearchBox.Enabled = false;
            xmlReplaceBox.Enabled = true;
        }

        private void xmlEditModsButton_CheckedChanged(object sender, EventArgs e)
        {
            xmlSearchBox.Enabled = true;
            xmlReplaceBox.Enabled = true;
        }

        private void xmlRemoveModeButton_CheckedChanged(object sender, EventArgs e)
        {
            xmlSearchBox.Enabled = true;
            xmlReplaceBox.Enabled = false;
        }

        private void PatchTester_Load(object sender, EventArgs e)
        {
            //font scaling
            this.AutoScaleMode = Settings.appScalingMode;
            this.Font = Settings.appFont;
            if (Settings.appScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new System.Drawing.SizeF(Settings.scaleSize, Settings.scaleSize));
            }
            regexFileDialog.InitialDirectory = Application.StartupPath;
            xmlFileDialog.InitialDirectory = Application.StartupPath;
            jsonFileDialog.InitialDirectory = Application.StartupPath;
            xvmFileDialog.InitialDirectory = Application.StartupPath;
            xmlAddModeButton.Checked = true;
        }

        private void regexLoadFileButton_Click(object sender, EventArgs e)
        {
            DialogResult result = regexFileDialog.ShowDialog();
            if (result == DialogResult.Cancel)
                return;
            regexFilePathBox.Text = regexFileDialog.FileName;
        }

        private void xmlLoadFileButton_Click(object sender, EventArgs e)
        {
            DialogResult result = xmlFileDialog.ShowDialog();
            if (result == DialogResult.Cancel)
                return;
            xmlFilePathBox.Text = xmlFileDialog.FileName;
        }

        private void regexPatchButton_Click(object sender, EventArgs e)
        {
            int i = 0;
            try
            {
                i = int.Parse(regexLineBox.Text);
            }
            catch (FormatException)
            {
                i = 0;
            }
            //convert actual newlines to "newlines"
            string newReg = Regex.Replace(regexReplaceBox.Text, @"\n", "newline");
            Utils.RegxPatch(regexFilePathBox.Text, regexSearchBox.Text, regexReplaceBox.Text, "", "", i, true, xvmFilePathBox.Text);
        }

        private void xmlPatchButton_Click(object sender, EventArgs e)
        {
            string temp = null;
            if (xmlAddModeButton.Checked)
            {
                temp = "add";
            }
            else if (xmlEditModsButton.Checked)
            {
                temp = "edit";
            }
            else if (xmlRemoveModeButton.Checked)
            {
                temp = "remove";
            }
            Utils.xmlPatch(xmlFilePathBox.Text, xmlPathBox.Text, temp, xmlSearchBox.Text, xmlReplaceBox.Text, "", "", true, xvmFilePathBox.Text);
        }

        private void jsonLoadFileButton_Click(object sender, EventArgs e)
        {
            DialogResult result = jsonFileDialog.ShowDialog();
            if (result == DialogResult.Cancel)
                return;
            jsonFilePathBox.Text = jsonFileDialog.FileName;
        }

        private void jsonPatchButton_Click(object sender, EventArgs e)
        {
            Utils.jsonPatch(jsonFilePathBox.Text, jsonPathBox.Text, jsonReplaceBox.Text, "edit", "", "", true, xvmFilePathBox.Text);
        }

        private void regexMakePatchButton_Click(object sender, EventArgs e)
        {
            string fileName = Path.GetFileName(regexFilePathBox.Text) + "_patch.xml";
            XmlDocument doc = new XmlDocument();
            XmlElement patchHolder = doc.CreateElement("patchs");
            doc.AppendChild(patchHolder);
            XmlElement patch = doc.CreateElement("patch");
            patchHolder.AppendChild(patch);

            XmlElement type = doc.CreateElement("type");
            type.InnerText = "regex";
            patch.AppendChild(type);

            XmlElement file = doc.CreateElement("file");
            file.InnerText = regexFilePathBox.Text;
            patch.AppendChild(file);

            XmlElement line = doc.CreateElement("line");
            line.InnerText = regexLineBox.Text;
            patch.AppendChild(line);

            XmlElement search = doc.CreateElement("search");
            search.InnerText = regexSearchBox.Text;
            patch.AppendChild(search);

            XmlElement replace = doc.CreateElement("replace");
            replace.InnerText = Regex.Replace(regexReplaceBox.Text, @"\n", "newline");
            patch.AppendChild(replace);
            doc.Save(fileName);
        }

        private void xmlMakePatchButton_Click(object sender, EventArgs e)
        {
            string fileName = Path.GetFileName(xmlFilePathBox.Text) + "_patch.xml";
            XmlDocument doc = new XmlDocument();
            XmlElement patchHolder = doc.CreateElement("patchs");
            doc.AppendChild(patchHolder);
            XmlElement patch = doc.CreateElement("patch");
            patchHolder.AppendChild(patch);

            XmlElement type = doc.CreateElement("type");
            type.InnerText = "xml";
            patch.AppendChild(type);

            XmlElement mode = doc.CreateElement("mode");
            if (xmlAddModeButton.Checked)
            {
                mode.InnerText = "add";
            }
            else if (xmlEditModsButton.Checked)
            {
                mode.InnerText = "edit";
            }
            else if (xmlRemoveModeButton.Checked)
            {
                mode.InnerText = "remove";
            }
            patch.AppendChild(mode);


            XmlElement file = doc.CreateElement("file");
            file.InnerText = xmlFilePathBox.Text;
            patch.AppendChild(file);

            XmlElement line = doc.CreateElement("path");
            line.InnerText = xmlPathBox.Text;
            patch.AppendChild(line);

            XmlElement search = doc.CreateElement("search");
            search.InnerText = xmlSearchBox.Text;
            patch.AppendChild(search);

            XmlElement replace = doc.CreateElement("replace");
            replace.InnerText = xmlReplaceBox.Text;
            patch.AppendChild(replace);

            doc.Save(fileName);
        }

        private void jsonMakePatchButton_Click(object sender, EventArgs e)
        {
            string fileName = Path.GetFileName(jsonFilePathBox.Text) + "_patch.xml";
            XmlDocument doc = new XmlDocument();
            XmlElement patchHolder = doc.CreateElement("patchs");
            doc.AppendChild(patchHolder);
            XmlElement patch = doc.CreateElement("patch");
            patchHolder.AppendChild(patch);

            XmlElement type = doc.CreateElement("type");
            type.InnerText = "json";
            patch.AppendChild(type);

            XmlElement mode = doc.CreateElement("mode");
            mode.InnerText = "edit";
            patch.AppendChild(mode);

            XmlElement file = doc.CreateElement("file");
            file.InnerText = jsonFilePathBox.Text;
            patch.AppendChild(file);

            XmlElement line = doc.CreateElement("path");
            line.InnerText = jsonPathBox.Text;
            patch.AppendChild(line);

            XmlElement search = doc.CreateElement("search");
            search.InnerText = jsonSearchBox.Text;
            patch.AppendChild(search);

            XmlElement replace = doc.CreateElement("replace");
            replace.InnerText = jsonReplaceBox.Text;
            patch.AppendChild(replace);

            doc.Save(fileName);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //edit example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "login.saveLastServer", ".*", "nope", "edit", "", "", true);
            //advanced edit example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.normal.fields.flag.enabled", ".*", "nope", "edit", "", "", true);
            //very advnaced edit example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.normal.extraFields[2]endIndex.enabled", ".*", "nope", "edit", "", "", true);
            //very very advnaced edit example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.normal.extraFields[img://gui/maps/icons/library/proficiency/class_icons_{{v.mastery}}.png]endIndex.enabled", ".*", "nope", "edit", "", "", true);
            //very very very advanced edit example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "battleLabels.formats[totalHP]endIndex.enabled", ".*", "nope", "edit", "", "", true);

            //add example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "login.saveLastServer", "", "    \"isAwesome\": yup", "add", "", "", true);
            //advanced add example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "login.pingServers", "", "      \"isAwesome\": yup", "add", "", "", true);
            //very advanced add example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "login.pingServers.fontStyle.serverColor", "", "        \"isAwesome\": yup", "add", "", "", true);

            //array clear example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.sorting_criteria", "", "", "array_clear", "", "", true);

            //array add example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.types_order[4]endIndex", "", " \"SCUMBAG\"", "array_add", "", "", true);
            //advanced array add example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "battleLabels.formats[0]endIndex", "", " \"SCUMBAG\"", "array_add", "", "", true);
             //very advanced array add example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "battleLabels.formats[-1]endIndex", "", "  ${ \"battleLabelsTemplates.xc\":\"def.teamRating\"}", "array_add", "", "", true);
            
            //array edit example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.types_order[2]endIndex", ".*", "\"MEMER\"", "array_edit", "", "", true);

            //array remove example
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.types_order[0]endIndex", ".*", "", "array_remove", "", "", true);
            //advanced array remove example 2
            Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.types_order[mediumTank]endIndex", ".*", "", "array_remove", "", "", true);
            
            //pmod test example
            Utils.pmodPatch(Application.StartupPath + "\\TempPatchWork\\pmod\\_multiple.json", "zoomIndicator.enable", ".*", "nope", "edit", "", "", true);
            
            //test xvm folder provider
            string testXvmBootLoc = Utils.getXVMBootLoc(null, Application.StartupPath + "\\TempPatchWork\\xvm.xc");
            MessageBox.Show(testXvmBootLoc);
        }

        private void xvm_modeToggle(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            xvmMode = rb.Text;
        }

        private void xvmLoadFileButton_Click(object sender, EventArgs e)
        {
            DialogResult result = xvmFileDialog.ShowDialog();
            if (result == DialogResult.Cancel)
                return;
            xvmFilePathBox.Text = xvmFileDialog.FileName;
        }

        private void xvmPathButton_Click(object sender, EventArgs e)
        {
            //convert actual newlines to "newlines"
            string newReg = Regex.Replace(xvmReplaceBox.Text, @"\n", "newline");
            if (xvmPatchRB.Checked)
            {
                Utils.xvmPatch(xvmFilePathBox.Text, xvmPathBox.Text, xvmSearchBox.Text, newReg, xvmMode, "", "", true, xvmFilePathBox.Text);
            }
            else if (PMODPatchRB.Checked)
            {
                Utils.pmodPatch(xvmFilePathBox.Text, xvmPathBox.Text, xvmSearchBox.Text, newReg, xvmMode, "", "", true, xvmFilePathBox.Text);
            }
            else
            {
                //do nothing
            }
        }

        private void xvmMakePatchButton_Click(object sender, EventArgs e)
        {
            string fileName = Path.GetFileName(xvmFilePathBox.Text) + "_patch.xml";
            XmlDocument doc = new XmlDocument();
            XmlElement patchHolder = doc.CreateElement("patchs");
            doc.AppendChild(patchHolder);
            XmlElement patch = doc.CreateElement("patch");
            patchHolder.AppendChild(patch);

            XmlElement type = doc.CreateElement("type");
            type.InnerText = "xvm";
            patch.AppendChild(type);

            XmlElement mode = doc.CreateElement("mode");
            mode.InnerText = xvmMode;
            patch.AppendChild(mode);

            XmlElement file = doc.CreateElement("file");
            file.InnerText = xvmFilePathBox.Text;
            patch.AppendChild(file);

            XmlElement line = doc.CreateElement("path");
            line.InnerText = xvmPathBox.Text;
            patch.AppendChild(line);

            XmlElement search = doc.CreateElement("search");
            search.InnerText = xvmSearchBox.Text;
            patch.AppendChild(search);

            XmlElement replace = doc.CreateElement("replace");
            replace.InnerText = xvmReplaceBox.Text;
            patch.AppendChild(replace);

            doc.Save(fileName);
        }
    }
}
