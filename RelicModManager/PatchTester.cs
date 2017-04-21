using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;

namespace RelhaxModpack
{
    public partial class PatchTester : Form
    {
        private MainWindow mw;
        public PatchTester()
        {
            InitializeComponent();
            mw = new MainWindow();
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
            mw.RegxPatch(regexFilePathBox.Text, regexSearchBox.Text, regexReplaceBox.Text, i,true);
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
            mw.xmlPatch(xmlFilePathBox.Text, xmlPathBox.Text, temp, xmlSearchBox.Text, xmlReplaceBox.Text,true);
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
            mw.jsonPatch(jsonFilePathBox.Text, jsonPathBox.Text, jsonReplaceBox.Text,"edit",true);
        }

        private void regexMakePatchButton_Click(object sender, EventArgs e)
        {

        }

        private void xmlMakePatchButton_Click(object sender, EventArgs e)
        {

        }

        private void jsonMakePatchButton_Click(object sender, EventArgs e)
        {

        }
    }
}
