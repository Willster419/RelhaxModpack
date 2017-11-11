using System;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Xml;
using System.IO;

namespace RelhaxModpack
{
    public partial class PatchTester : Form
    {
        private string XVMMode = "";
        private string JSONMode = "edit";
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
            this.AutoScaleMode = Settings.AppScalingMode;
            this.Font = Settings.AppFont;
            if (Settings.AppScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new System.Drawing.SizeF(Settings.ScaleSize, Settings.ScaleSize));
            }
            regexFileDialog.InitialDirectory = Application.StartupPath;
            xmlFileDialog.InitialDirectory = Application.StartupPath;
            jsonFileDialog.InitialDirectory = Application.StartupPath;
            xvmFileDialog.InitialDirectory = Application.StartupPath;
            xmlAddModeButton.Checked = true;
            JSONEditArrayEdit.Checked = true;
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
            PatchUtils.RegxPatch(regexFilePathBox.Text, regexSearchBox.Text, regexReplaceBox.Text, "", "", i, true, xvmFilePathBox.Text);
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
            PatchUtils.XMLPatch(xmlFilePathBox.Text, xmlPathBox.Text, temp, xmlSearchBox.Text, xmlReplaceBox.Text, "", "", true, xvmFilePathBox.Text);
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
            PatchUtils.JSONPatch(jsonFilePathBox.Text, jsonPathBox.Text, jsonSearchBox.Text, jsonReplaceBox.Text, JSONMode, "", "", true, xvmFilePathBox.Text);
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

        private void xvmRegressionTesting_Click(object sender, EventArgs e)
        {
            //edit example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "login.saveLastServer", ".*", "nope", "edit", "", "", true);
            //advanced edit example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.normal.fields.flag.Enabled", ".*", "nope", "edit", "", "", true);
            //very advnaced edit example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.normal.extraFields[2]endIndex.Enabled", ".*", "nope", "edit", "", "", true);
            //very very advnaced edit example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.normal.extraFields[img://gui/maps/icons/library/proficiency/class_icons_{{v.mastery}}.png]endIndex.Enabled", ".*", "nope", "edit", "", "", true);
            //very very very advanced edit example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "battleLabels.formats[totalHP]endIndex.Enabled", ".*", "nope", "edit", "", "", true);

            //add example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "login.saveLastServer", "", "    \"isAwesome\": yup", "add", "", "", true);
            //advanced add example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "login.pingServers", "", "      \"isAwesome\": yup", "add", "", "", true);
            //very advanced add example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "login.pingServers.fontStyle.serverColor", "", "        \"isAwesome\": yup", "add", "", "", true);

            //array clear example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.sorting_criteria", "", "", "array_clear", "", "", true);

            //array add example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.types_order[4]endIndex", "", " \"SCUMBAG\"", "array_add", "", "", true);
            //advanced array add example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "battleLabels.formats[0]endIndex", "", " \"SCUMBAG\"", "array_add", "", "", true);
            //very advanced array add example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "battleLabels.formats[-1]endIndex", "", "  ${ \"battleLabelsTemplates.xc\":\"def.teamRating\"}", "array_add", "", "", true);

            //array edit example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.types_order[2]endIndex", ".*", "\"MEMER\"", "array_edit", "", "", true);

            //array remove example
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.types_order[0]endIndex", ".*", "", "array_remove", "", "", true);
            //advanced array remove example 2
            PatchUtils.XVMPatch(Application.StartupPath + "\\TempPatchWork\\xvm.xc", "hangar.carousel.types_order[mediumTank]endIndex", ".*", "", "array_remove", "", "", true);

            //pmod test example
            PatchUtils.PMODPatch(Application.StartupPath + "\\TempPatchWork\\pmod\\_multiple.json", "zoomIndicator.enable", ".*", "nope", "edit", "", "", true);
            
            //test xvm folder provider
            string testXvmBootLoc = PatchUtils.GetXVMBootLoc(null, Application.StartupPath + "\\TempPatchWork\\xvm.xc");
            MessageBox.Show(testXvmBootLoc);
        }

        private void xvm_modeToggle(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            XVMMode = rb.Text;
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
                PatchUtils.XVMPatch(xvmFilePathBox.Text, xvmPathBox.Text, xvmSearchBox.Text, newReg, XVMMode, "", "", true, xvmFilePathBox.Text);
            }
            else if (PMODPatchRB.Checked)
            {
                PatchUtils.PMODPatch(xvmFilePathBox.Text, xvmPathBox.Text, xvmSearchBox.Text, newReg, XVMMode, "", "", true, xvmFilePathBox.Text);
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
            mode.InnerText = XVMMode;
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

        private void PatchTester_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logging.Manager("|------------------------------------------------------------------------------------------------|");
        }

        private void JSONMode_CheckedChanged(object sender, EventArgs e)
        {
            if(JSONEditArrayEdit.Checked)
            {
                JSONMode = "edit";
            }
            else if (JSONArrayAdd.Checked)
            {
                JSONMode = "arrayAdd";
            }
            else if (JSONArrayRemove.Checked)
            {
                JSONMode = "arrayRemove";
            }
            else if (JSONAdd.Checked)
            {
                JSONMode = "add";
            }
            else if (JSONRemove.Checked)
            {
                JSONMode = "remove";
            }
            else if (JSONArrayClear.Checked)
            {
                JSONMode = "arrayClear";
            }
        }

        private void JSONRegressionTesting_Click(object sender, EventArgs e)
        {
            /*
             * ALL REGRESSION TESTS PASSED AS OF 10/29/17
             */
            //file, path, search, replace, mode, "", "", true, xvmFilePathBox.Text

            
            //add test 1: basic add
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_add.json", "$","", "awesome/false", "add", "", "", true, xvmFilePathBox.Text);

            //add test 2: repeat of basic add. should do nothing
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_add.json", "$", "", "awesome/false", "add", "", "", true, xvmFilePathBox.Text);

            //add test 3: same path as basic add, but different falue to insert. should update the value
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_add.json", "$", "", "awesome/true", "add", "", "", true, xvmFilePathBox.Text);

            //add test 4: add of a new object as well as the path. should create object paths to value
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_add.json", "$", "", "memes/awesome/true", "add", "", "", true, xvmFilePathBox.Text);

            //add test 5: add of a new property to part object path that already exists. should add the value without overwriting the path
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_add.json", "$", "", "memes/dank/true", "add", "", "", true, xvmFilePathBox.Text);

            //add test 6: add of a new blank object
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_add.json", "$", "", "memelist[array]", "add", "", "", true, xvmFilePathBox.Text);

            //add test 7: add of a new blank array
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_add.json", "$", "", "objectname[object]", "add", "", "", true, xvmFilePathBox.Text);

            //add test 8: add of new property with slash escape
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_add.json", "$.nations", "", "memeville/spaces[sl]hangar_premium_v2", "add", "", "", true, xvmFilePathBox.Text);
            

            
            //edit test 1: edit attempt of path that does not exist. should note it log and abort
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_edit.json", "$.fakePath", "", "null", "edit", "", "", true, xvmFilePathBox.Text);

            //edit test 2: edit attempt of object. should note in log and abort
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_edit.json", "$.nations", "", "null", "edit", "", "", true, xvmFilePathBox.Text);

            //edit test 3: edit attempt of simple path. should change the one value
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_edit.json", "$.mode", "normal", "epic", "edit", "", "", true, xvmFilePathBox.Text);

            //edit test 4: edit attempt of simple path. should change the one value
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_edit.json", "$.mode", "epic", "epic", "edit", "", "", true, xvmFilePathBox.Text);

            //edit test 5: edit of array of values. should change the last value in the array
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_edit.json", @"$.ignorelist[*]", "ttest", "test", "edit", "", "", true, xvmFilePathBox.Text);

            //edit test 6: edit of array of objects. should parse advaned jsonpath and edit the value of 421 or above to be 420
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_edit.json", @"$.screensavers..starttime", @"^[4-9][2-9][0-9]$", "420", "edit", "", "", true, xvmFilePathBox.Text);

            //edit test 7: edit of array of objects. should parse advaned jsonpath and edit the value of 419 or below to be 420
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_edit.json", @"$.screensavers..starttime", @"^([0123]?[0-9]?[0-9]|4[01][0-9]|41[0-9])$", "420", "edit", "", "", true, xvmFilePathBox.Text);

            //edit test 8: edit array of objects. should parse very advanced jsonpath and edit values less than 420 to be 420
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_edit.json", @"$.screensavers[?(@.starttime < 420)].starttime", @".*", "420", "edit", "", "", true, xvmFilePathBox.Text);
            

            //file, path, search, replace, mode, "", "", true, xvmFilePathBox.Text
            //remove test 1: basic remove test with property
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_remove.json", @"$.game_greeting2", @".*", "", "remove", "", "", true, xvmFilePathBox.Text);

            //remove test 2: basic remove test with property
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_remove.json", @"$.screensaver", @".*", "", "remove", "", "", true, xvmFilePathBox.Text);

            //remove test 3: basic remove test with property
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_remove.json", @"$.ignorelist", @".*", "", "remove", "", "", true, xvmFilePathBox.Text);


            //arrayAdd test 1: basic add of jValue at index 0
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_arrayAdd.json", @"$.ignorelist", @".*", "spaces[sl]urmom[index=0]", "arrayAdd", "", "", true, xvmFilePathBox.Text);

            //arrayAdd test 2: basic add of jValue at index -1 (last)
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_arrayAdd.json", @"$.ignorelist", @".*", "spaces[sl]urmom2[index=-1]", "arrayAdd", "", "", true, xvmFilePathBox.Text);

            //arrayAdd test 3: attempt add of object to array of JValue, should fail
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_arrayAdd.json", @"$.ignorelist", @".*", "enable/true[index=0]", "arrayAdd", "", "", true, xvmFilePathBox.Text);

            //arrayAdd test 4: attempt add of jValue to array of object, should fail
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_arrayAdd.json", @"$.screensavers", @".*", "spaces[sl]urmom[index=0]", "arrayAdd", "", "", true, xvmFilePathBox.Text);

            //arrayAdd test 5: basic add of object at end
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_arrayAdd.json", @"$.screensavers", @".*", "enable/true[index=0]", "arrayAdd", "", "", true, xvmFilePathBox.Text);

            //file, path, search, replace, mode, "", "", true, xvmFilePathBox.Text
            //arrayRemove test 1: basic remove of jValue "test"
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_arrayRemove.json", @"$.ignorelist", @"test", "", "arrayRemove", "", "", true, xvmFilePathBox.Text);

            //arrayRemove test 2: basic remove of jValue "test3" (does not exist)
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_arrayRemove.json", @"$.ignorelist", @"test3", "", "arrayRemove", "", "", true, xvmFilePathBox.Text);

            //arrayRemove test 3: basic remove of jObject "enable:true"
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_arrayRemove.json", @"$.screensavers", @".*", "", "arrayRemove", "", "", true, xvmFilePathBox.Text);


            //arrayClear test 1: basic clear of jValue "test"
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_arrayClear.json", @"$.ignorelist", @"test", "", "arrayClear", "", "", true, xvmFilePathBox.Text);

            //arrayClear test 2: basic clear of jValue "test" (does not exist)
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_arrayClear.json", @"$.ignorelist", @"test", "", "arrayClear", "", "", true, xvmFilePathBox.Text);

            //arrayClear test 3: basic clear of object ".*" (all)
            PatchUtils.JSONPatch(Application.StartupPath + "\\RelHaxUserMods\\HangMan_arrayClear.json", @"$.screensavers", @".*", "", "arrayClear", "", "", true, xvmFilePathBox.Text);

        }
    }
}
