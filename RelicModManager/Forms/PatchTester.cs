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
            regexPatchPathCB.SelectedIndex = 0;
            xmlPatchPathCB.SelectedIndex = 0;
            jsonPatchPathCB.SelectedIndex = 0;
            xvmPatchPathCB.SelectedIndex = 0;

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
            //PatchUtils.RegxPatch(regexFilePathBox.Text, regexSearchBox.Text, regexReplaceBox.Text, "", "", i, true, xvmFilePathBox.Text);
            PatchUtils.RegxPatch(new Patch() { completePath = regexFilePathBox.Text, search = regexSearchBox.Text, replace = regexReplaceBox.Text }, i, true, xvmFilePathBox.Text);
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
            //PatchUtils.XMLPatch(xmlFilePathBox.Text, xmlPathBox.Text, temp, xmlSearchBox.Text, xmlReplaceBox.Text, "", "", true, xvmFilePathBox.Text);
            PatchUtils.XMLPatch(new Patch() { completePath = xmlFilePathBox.Text, path = xmlPathBox.Text, mode = temp, search = xmlSearchBox.Text, replace = xmlReplaceBox.Text }, true, xvmFilePathBox.Text);
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
            //PatchUtils.JSONPatch(jsonFilePathBox.Text, jsonPathBox.Text, jsonSearchBox.Text, jsonReplaceBox.Text, JSONMode, "", "", true, xvmFilePathBox.Text);
            PatchUtils.JSONPatch(new Patch() { completePath = jsonFilePathBox.Text, path = jsonPathBox.Text, search = jsonSearchBox.Text, replace = jsonReplaceBox.Text, mode = JSONMode }, true, xvmFilePathBox.Text);
        }

        private void regexMakePatchButton_Click(object sender, EventArgs e)
        {
            string fileName = Path.GetFileName(regexFilePathBox.Text) + "_patch.xml";
            if (File.Exists(fileName))
                if (MessageBox.Show("patch file already exists, overwrite?", "Patch already exists", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            XmlDocument doc = new XmlDocument();
            XmlElement patchHolder = doc.CreateElement("patchs");
            doc.AppendChild(patchHolder);
            XmlElement patch = doc.CreateElement("patch");
            patchHolder.AppendChild(patch);

            XmlElement type = doc.CreateElement("type");
            type.InnerText = "regex";
            patch.AppendChild(type);

            XmlElement patchPath = doc.CreateElement("patchPath");
            patchPath.InnerText = regexPatchPathCB.SelectedItem.ToString();
            patch.AppendChild(patchPath);

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
            if (File.Exists(fileName))
                if (MessageBox.Show("patch file already exists, overwrite?", "Patch already exists", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
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

            XmlElement patchPath = doc.CreateElement("patchPath");
            patchPath.InnerText = xmlPatchPathCB.SelectedItem.ToString();
            patch.AppendChild(patchPath);

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
            if (File.Exists(fileName))
                if (MessageBox.Show("patch file already exists, overwrite?", "Patch already exists", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            XmlDocument doc = new XmlDocument();
            XmlElement patchHolder = doc.CreateElement("patchs");
            doc.AppendChild(patchHolder);
            XmlElement patch = doc.CreateElement("patch");
            patchHolder.AppendChild(patch);

            XmlElement type = doc.CreateElement("type");
            type.InnerText = "json";
            patch.AppendChild(type);

            XmlElement mode = doc.CreateElement("mode");
            mode.InnerText = JSONMode;
            patch.AppendChild(mode);

            XmlElement patchPath = doc.CreateElement("patchPath");
            patchPath.InnerText = jsonPatchPathCB.SelectedItem.ToString();
            patch.AppendChild(patchPath);

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
            string workingPath = Path.Combine(Application.StartupPath, "TempPatchWork", "xvm.xc");
            //edit example
            Patch p = new Patch()
            {
                completePath = workingPath,
                path = "login.saveLastServer",
                search = ".*",
                replace = "nope",
                mode = "edit"
            };
            PatchUtils.XVMPatch(p, true);

            //advanced edit example
            p.path = "hangar.carousel.normal.fields.flag.Enabled";
            p.search = ".*";
            p.replace = "nope";
            p.mode = "edit";
            PatchUtils.XVMPatch(p, true);

            //very advnaced edit example
            p.path = "hangar.carousel.normal.extraFields[2]endIndex.Enabled";
            p.search = ".*";
            p.replace = "nope";
            p.mode = "edit";
            PatchUtils.XVMPatch(p, true);

            //very very advnaced edit example
            p.path = "hangar.carousel.normal.extraFields[img://gui/maps/icons/library/proficiency/class_icons_{{v.mastery}}.png]endIndex.Enabled";
            p.search = ".*";
            p.replace = "nope";
            p.mode = "edit";
            PatchUtils.XVMPatch(p, true);

            //very very very advanced edit example
            p.path = "battleLabels.formats[totalHP]endIndex.Enabled";
            p.search = ".*";
            p.replace = "nope";
            p.mode = "edit";
            PatchUtils.XVMPatch(p, true);

            //add example
            p.path = "login.saveLastServer";
            p.search = "";
            p.replace = "    \"isAwesome\": yup";
            p.mode = "add";
            PatchUtils.XVMPatch(p, true);

            //advanced add example
            p.path = "login.pingServers";
            p.replace = "      \"isAwesome\": yup";
            p.mode = "add";
            PatchUtils.XVMPatch(p, true);

            //very advanced add example
            p.path = "login.pingServers.fontStyle.serverColor";
            p.replace = "        \"isAwesome\": yup";
            p.mode = "add";
            PatchUtils.XVMPatch(p, true);

            //array clear example
            p.path = "hangar.carousel.sorting_criteria";
            p.replace = "nope";
            p.mode = "array_clear";
            PatchUtils.XVMPatch(p, true);

            //array add example
            p.path = "hangar.carousel.types_order[4]endIndex";
            p.replace = " \"SCUMBAG\"";
            p.mode = "array_add";
            PatchUtils.XVMPatch(p, true);

            //advanced array add example
            p.path = "battleLabels.formats[0]endIndex";
            p.replace = " \"SCUMBAG\"";
            p.mode = "array_add";
            PatchUtils.XVMPatch(p, true);

            //very advanced array add example
            p.path = "battleLabels.formats[-1]endIndex";
            p.replace = "  ${ \"battleLabelsTemplates.xc\":\"def.teamRating\"}";
            p.mode = "array_add";
            PatchUtils.XVMPatch(p, true);

            //array edit example
            p.path = "hangar.carousel.types_order[2]endIndex";
            p.search = ".*";
            p.replace = "\"MEMER\"";
            p.mode = "array_edit";
            PatchUtils.XVMPatch(p, true);

            //array remove example
            p.path = "hangar.carousel.types_order[0]endIndex";
            p.search = ".*";
            p.mode = "array_remove";
            PatchUtils.XVMPatch(p, true);

            //advanced array remove example 2
            p.path = "hangar.carousel.types_order[mediumTank]endIndex";
            p.search = ".*";
            p.mode = "array_remove";
            PatchUtils.XVMPatch(p, true);

            //pmod test example
            workingPath = Path.Combine(Application.StartupPath, "TempPatchWork", "pmod", "_multiple.json");
            p.completePath = workingPath;
            p.path = "zoomIndicator.enable";
            p.search = ".*";
            p.replace = "nope";
            p.mode = "edit";
            PatchUtils.PMODPatch(p, true);
            
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
                //PatchUtils.XVMPatch(xvmFilePathBox.Text, xvmPathBox.Text, xvmSearchBox.Text, newReg, XVMMode, "", "", true, xvmFilePathBox.Text);
                PatchUtils.XVMPatch(new Patch() { completePath = xvmFilePathBox.Text, path = xvmPathBox.Text, search = xvmSearchBox.Text, replace = newReg, mode = XVMMode }, true, xvmFilePathBox.Text);
            }
            else if (PMODPatchRB.Checked)
            {
                //PatchUtils.PMODPatch(xvmFilePathBox.Text, xvmPathBox.Text, xvmSearchBox.Text, newReg, XVMMode, "", "", true, xvmFilePathBox.Text);
                PatchUtils.PMODPatch(new Patch() { completePath = xvmFilePathBox.Text, path = xvmPathBox.Text, search = xvmSearchBox.Text, replace = newReg, mode = XVMMode }, true, xvmFilePathBox.Text);
            }
            else
            {
                //do nothing
            }
        }

        private void xvmMakePatchButton_Click(object sender, EventArgs e)
        {
            string fileName = Path.GetFileName(xvmFilePathBox.Text) + "_patch.xml";
            if (File.Exists(fileName))
                if (MessageBox.Show("patch file already exists, overwrite?", "Patch already exists", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
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

            XmlElement patchPath = doc.CreateElement("patchPath");
            patchPath.InnerText = xvmPatchPathCB.SelectedItem.ToString();
            patch.AppendChild(patchPath);

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
            Patch p = new Patch()
            {
                completePath = Path.Combine(Application.StartupPath, "RelHaxUserMods", "HangMan_add.json"),
                path = "$",
                search = "",
                replace = "awesome/false",
                mode = "add",
            };
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //add test 2: repeat of basic add. should do nothing
            p.completePath = p.completePath;
            p.path = "$";
            p.search = "";
            p.replace = "awesome/false";
            p.mode = "add";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //add test 3: same path as basic add, but different falue to insert. should update the value
            p.completePath = p.completePath;
            p.path = "$";
            p.search = "";
            p.replace = "awesome/true";
            p.mode = "add";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //add test 4: add of a new object as well as the path. should create object paths to value
            p.completePath = p.completePath;
            p.path = "$";
            p.search = "";
            p.replace = "memes/awesome/true";
            p.mode = "add";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //add test 5: add of a new property to part object path that already exists. should add the value without overwriting the path
            p.completePath = p.completePath;
            p.path = "$";
            p.search = "";
            p.replace = "memes/dank/true";
            p.mode = "add";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //add test 6: add of a new blank object
            p.completePath = p.completePath;
            p.path = "$";
            p.search = "";
            p.replace = "memelist[array]";
            p.mode = "add";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //add test 7: add of a new blank array
            p.completePath = p.completePath;
            p.path = "$";
            p.search = "";
            p.replace = "objectname[object]";
            p.mode = "add";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //add test 8: add of new property with slash escape
            p.completePath = p.completePath;
            p.path = "$";
            p.search = "";
            p.replace = "memeville/spaces[sl]hangar_premium_v2";
            p.mode = "add";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);



            //edit test 1: edit attempt of path that does not exist. should note it log and abort
            p.completePath = Path.Combine(Application.StartupPath, "RelHaxUserMods", "HangMan_edit.json");
            p.path = "$.fakePath";
            p.search = "";
            p.replace = "null";
            p.mode = "edit";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //edit test 2: edit attempt of object. should note in log and abort
            p.completePath = p.completePath;
            p.path = "$.nations";
            p.search = "";
            p.replace = "null";
            p.mode = "edit";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //edit test 3: edit attempt of simple path. should change the one value
            p.completePath = p.completePath;
            p.path = "$.mode";
            p.search = "normal";
            p.replace = "epic";
            p.mode = "edit";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //edit test 4: edit attempt of simple path. should change the one value
            p.completePath = p.completePath;
            p.path = "$.mode";
            p.search = "epic";
            p.replace = "epic";
            p.mode = "edit";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //edit test 5: edit of array of values. should change the last value in the array
            p.completePath = p.completePath;
            p.path = "$.ignorelist[*]";
            p.search = "ttest";
            p.replace = "test";
            p.mode = "arrayEdit";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //edit test 6: edit of array of objects. should parse advaned jsonpath and edit the value of 421 or above to be 420
            p.completePath = p.completePath;
            p.path = @"$.screensavers..starttime";
            p.search = @"^[4-9][2-9][0-9]$";
            p.replace = "420";
            p.mode = "arrayEdit";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //edit test 7: edit of array of objects. should parse advaned jsonpath and edit the value of 419 or below to be 420
            p.completePath = p.completePath;
            p.path = @"$.screensavers..starttime";
            p.search = @"^([0123]?[0-9]?[0-9]|4[01][0-9]|41[0-9])$";
            p.replace = "420";
            p.mode = "arrayEdit";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //edit test 8: edit array of objects. should parse very advanced jsonpath and edit values less than 420 to be 420
            p.completePath = p.completePath;
            p.path = @"$.screensavers[?(@.starttime < 420)].starttime";
            p.search = ".*";
            p.replace = "420";
            p.mode = "arrayEdit";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);


            //file, path, search, replace, mode, "", "", true, xvmFilePathBox.Text
            //remove test 1: basic remove test with property
            p.completePath = Path.Combine(Application.StartupPath, "RelHaxUserMods", "HangMan_remove.json");
            p.path = "$.game_greeting2";
            p.search = ".*";
            p.replace = "";
            p.mode = "remove";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //remove test 2: basic remove test with property
            p.completePath = p.completePath;
            p.path = "$.screensaver";
            p.search = ".*";
            p.replace = "";
            p.mode = "remove";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //remove test 3: basic remove test with property
            p.completePath = p.completePath;
            p.path = "$.ignorelist";
            p.search = ".*";
            p.replace = "";
            p.mode = "remove";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);


            //arrayAdd test 1: basic add of jValue at index 0
            p.completePath = Path.Combine(Application.StartupPath, "RelHaxUserMods", "HangMan_arrayAdd.json");
            p.path = ".ignorelist";
            p.search = ".*";
            p.replace = "spaces[sl]urmom[index=0]";
            p.mode = "arrayAdd";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //arrayAdd test 2: basic add of jValue at index -1 (last)
            p.completePath = p.completePath;
            p.path = "$.ignorelist";
            p.search = ".*";
            p.replace = "spaces[sl]urmom2[index=-1]";
            p.mode = "arrayAdd";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //arrayAdd test 3: attempt add of object to array of JValue, should fail
            p.completePath = p.completePath;
            p.path = "$.ignorelist";
            p.search = ".*";
            p.replace = "enable/true[index=0]";
            p.mode = "arrayAdd";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //arrayAdd test 4: attempt add of jValue to array of object, should fail
            p.completePath = p.completePath;
            p.path = "$.screensavers";
            p.search = ".*";
            p.replace = "spaces[sl]urmom[index=0]";
            p.mode = "arrayAdd";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //arrayAdd test 5: basic add of object at end
            p.completePath = p.completePath;
            p.path = "$.screensavers";
            p.search = ".*";
            p.replace = "enable/true[index=0]";
            p.mode = "arrayAdd";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //file, path, search, replace, mode, "", "", true, xvmFilePathBox.Text
            //arrayRemove test 1: basic remove of jValue "test"
            p.completePath = Path.Combine(Application.StartupPath, "RelHaxUserMods", "HangMan_arrayRemove.json");
            p.path = "$.ignorelist";
            p.search = "test";
            p.replace = "";
            p.mode = "arrayRemove";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //arrayRemove test 2: basic remove of jValue "test3" (does not exist)
            p.completePath = p.completePath;
            p.path = "$.ignorelist";
            p.search = "test3";
            p.replace = "";
            p.mode = "arrayRemove";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //arrayRemove test 3: basic remove of jObject "enable:true"
            p.completePath = p.completePath;
            p.path = "$.screensavers";
            p.search = ".*";
            p.replace = "";
            p.mode = "arrayRemove";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);


            //arrayClear test 1: basic clear of jValue "test"
            p.completePath = Path.Combine(Application.StartupPath, "RelHaxUserMods", "HangMan_arrayClear.json");
            p.path = "$.ignorelist";
            p.search = "test";
            p.replace = "";
            p.mode = "arrayClear";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //arrayClear test 2: basic clear of jValue "test" (does not exist)
            p.completePath = p.completePath;
            p.path = "$.ignorelist";
            p.search = "test";
            p.replace = "";
            p.mode = "arrayClear";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

            //arrayClear test 3: basic clear of object ".*" (all)
            p.completePath = p.completePath;
            p.path = "$.screensavers";
            p.search = ".*";
            p.replace = "";
            p.mode = "arrayClear";
            PatchUtils.JSONPatch(p, true, xvmFilePathBox.Text);

        }
    }
}
