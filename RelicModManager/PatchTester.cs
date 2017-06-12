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
            regexFileDialog.InitialDirectory = Application.StartupPath;
            xmlFileDialog.InitialDirectory = Application.StartupPath;
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
            Utils.RegxPatch(regexFilePathBox.Text, regexSearchBox.Text, regexReplaceBox.Text, "", "", i, true);
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
            Utils.xmlPatch(xmlFilePathBox.Text, xmlPathBox.Text, temp, xmlSearchBox.Text, xmlReplaceBox.Text, "","",true);
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
            Utils.jsonPatch(jsonFilePathBox.Text, jsonPathBox.Text, jsonReplaceBox.Text, "edit","","", true);
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
            //Utils.jsonPatch(jsonFilePathBox.Text, jsonPathBox.Text, jsonReplaceBox.Text, "edit","","", true);
            //[[ \\t]*\"src\"[ \\t]*:[ \\t]*\".*\"]endIndex
            //[4]endIndex
            //edit example
            //Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "login.saveLastServer", ".*", "false", "edit", "", "", true);
            //add example
            //Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "login.pingServers.fontStyle", "", "    \"isAwesome\": true", "add", "", "", true);
            //array clear example
            //Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "battleLabels.formats", "", "", "array_clear", "", "", true);
            //array add example
            //Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.types_order[4]endIndex", "", "\"SCUMBAG\"", "array_add", "", "", true);
            //array edit example
            //Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.types_order[4]endIndex", "SCUMBAG", "DICKWAD", "array_edit", "", "", true);
            //array remove example
            //Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.types_order[4]endIndex", "\"DICKWAD\",", "", "array_remove", "", "", true);
            //array remove example 2
            //Utils.xvmPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.types_order[\"FAGGOT\"]endIndex", ".*", "", "array_remove", "", "", true);
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
            //Utils.jsonPatch(jsonFilePathBox.Text, jsonPathBox.Text, jsonReplaceBox.Text, "edit", "", "", true);
            Utils.xvmPatch(xvmFilePathBox.Text, xvmPathBox.Text, xvmSearchBox.Text, xvmReplaceBox.Text, xvmMode, "", "", true);
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
