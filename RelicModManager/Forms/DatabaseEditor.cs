using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;

namespace RelhaxModpack
{
    //an enum to control the form editor mode
    public enum EditorMode
    {
        GlobalDependnecy = 0,
        Dependency = 1,
        LogicalDependency = 2,
        DBO = 3,
    };
    public partial class DatabaseEditor : Form
    {
        //basic lists
        private List<Category> ParsedCategoryList;
        private List<Dependency> GlobalDependencies;
        private List<Dependency> Dependencies;
        private List<LogicalDependency> LogicalDependencies;
        //other stuff
        private string DatabaseLocation = "";
        private byte[] DatabaseLocationMD5Hash = new byte[] { 0x0 };
        private byte[] LoadedDatabaseMD5Hash = new byte[] { 0x0 };
        private Dependency SelectedGlobalDependency;
        private Dependency SelectedDependency;
        private LogicalDependency SelectedLogicalDependency;
        private SelectablePackage SelectedDatabaseObject;
        private Category SelectedCategory;
        private int currentSelectedIndex = -1;
        private StringBuilder InUseSB;
        private List<SelectablePackage> ListThatContainsPackage;
        private bool UnsavedModifications = false;
        private int ObjectDependenciesListOriginalHeight = 0;
        List<string> allPackageNames = new List<string>();

        private EditorMode DatabaseEditorMode;

        public DatabaseEditor()
        {
            InitializeComponent();
            ObjectDependenciesListOriginalHeight = ObjectDependenciesList.Height;
            AndOrLogicComboBox.DataSource = new List <String>{ LogicalDependency.GetAndOrString(LogicalDependency.AndOrFlag.AND), LogicalDependency.GetAndOrString(LogicalDependency.AndOrFlag.OR)};
            SetExtendedToolTipUserDatas();
            Settings.LoadSettings();
            if (!Utils.TinyManagerUpdateCheck())
                this.Close();
        }

        private void SetExtendedToolTipUserDatas()
        {
            Utils.BuildMacroHash(true);
            string txt = this.ObjectUserdatasToolTip.GetToolTip(this.ObjectUserdatasTB);
            foreach (DictionaryEntry macro in Utils.macroList)
            {
                txt += "\n{" + macro.Key + "} = " + macro.Value;
            }
            this.ObjectUserdatasToolTip.SetToolTip(this.ObjectUserdatasTB, txt);
        }

        private void DatabaseEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (UnsavedModifications)
            {
                if (MessageBox.Show("You have unsaved changes, return to editor?", "unsaved changes", MessageBoxButtons.YesNo, MessageBoxIcon.Stop) == DialogResult.Yes)
                {
                    e.Cancel = true;
                    return;
                }
            }
            Logging.Manager("|------------------------------------------------------------------------------------------------|");
        }
        //hook into the database editor loading
        private void DatabaseEditor_Load(object sender, EventArgs e)
        {
            ResetUI(true,false);
            if(Program.editorAutoLoad)
            {
                if(!File.Exists(Program.editorDatabaseFile))
                {
                    Logging.Manager("file does not exist, loading in regular mode: " + Program.editorDatabaseFile);
                    Program.editorDatabaseFile = "";
                    Program.editorAutoLoad = false;
                }
                Logging.Manager("Auto load xml file: " + Program.editorDatabaseFile);
                DatabaseLocation = Path.Combine(Application.StartupPath, Program.editorDatabaseFile);
                LoadDatabase();
            }
        }
        //loads the database depending on the mode of the radiobuttons
        private void DisplayDatabase(bool resetUI = true)
        {
            SearchBox.Items.Clear();
            allPackageNames.Clear();
            if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
            {
                DatabaseTreeView.Nodes.Clear();
                foreach (Dependency d in GlobalDependencies)
                {
                    SearchBox.Items.Add(d.PackageName);
                    allPackageNames.Add(d.PackageName);
                    DatabaseTreeView.Nodes.Add(new DatabaseTreeNode(d, (int)DatabaseEditorMode));
                }
            }
            else if (DatabaseEditorMode == EditorMode.Dependency)
            {
                DatabaseTreeView.Nodes.Clear();
                foreach (Dependency d in Dependencies)
                {
                    SearchBox.Items.Add(d.PackageName);
                    allPackageNames.Add(d.PackageName);
                    DatabaseTreeView.Nodes.Add(new DatabaseTreeNode(d, (int)DatabaseEditorMode));
                }
            }
            else if (DatabaseEditorMode == EditorMode.LogicalDependency)
            {
                DatabaseTreeView.Nodes.Clear();
                foreach (LogicalDependency d in LogicalDependencies)
                {
                    SearchBox.Items.Add(d.PackageName);
                    allPackageNames.Add(d.PackageName);
                    DatabaseTreeView.Nodes.Add(new DatabaseTreeNode(d, (int)DatabaseEditorMode));
                }
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                DatabaseTreeView.Nodes.Clear();
                foreach (Category cat in ParsedCategoryList)
                {
                    SearchBox.Items.Add(cat.Name);
                    allPackageNames.Add(cat.Name);
                    DatabaseTreeNode catNode = new DatabaseTreeNode(cat, 4);
                    DatabaseTreeView.Nodes.Add(catNode);
                    foreach (SelectablePackage m in cat.Packages)
                    {
                        SearchBox.Items.Add(m.PackageName);
                        allPackageNames.Add(m.PackageName);
                        DatabaseTreeNode modNode = new DatabaseTreeNode(m, (int)DatabaseEditorMode);
                        catNode.Nodes.Add(modNode);
                        if (SelectedDatabaseObject != null && SelectedDatabaseObject.PackageName.Equals(m.PackageName))
                            modNode.EnsureVisible();
                        DisplayDatabaseConfigs(modNode, m.Packages);
                    }
                }

            }
            if (resetUI)
                ResetUI(true,false);
        }
        private void ResetUI(bool clearSelects, bool hardReset)
        {
            if(clearSelects)
            {
                SelectedGlobalDependency = null;
                SelectedDependency = null;
                SelectedLogicalDependency = null;
                SelectedDatabaseObject = null;
                SelectedCategory = null;
            }
            //main panel
            ObjectNameTB.Enabled = false;
            ObjectNameTB.Text = "";

            ObjectPackageNameTB.Enabled = false;
            ObjectPackageNameTB.Text = "";

            ObjectStartAddressTB.Enabled = false;
            ObjectStartAddressTB.Text = "";

            ObjectEndAddressTB.Enabled = false;
            ObjectEndAddressTB.Text = "";

            ObjectZipFileTB.Enabled = false;
            ObjectZipFileTB.Text = "";

            ObjectDevURLTB.Enabled = false;
            ObjectDevURLTB.Text = "";

            ObjectVersionTB.Enabled = false;
            ObjectVersionTB.Text = "";

            ObjectLevelLabel.Text = "";
            ObjectLastUpdatedLabel.Text = "";

            ObjectTypeComboBox.Enabled = false;
            ObjectTypeComboBox.SelectedIndex = 0;

            ObjectLogAtInstallCB.Enabled = false;
            ObjectLogAtInstallCB.Checked = false;

            ObjectEnabledCheckBox.Enabled = false;
            ObjectEnabledCheckBox.Checked = false;

            ObjectVisibleCheckBox.Enabled = false;
            ObjectVisibleCheckBox.Checked = false;

            ObjectAppendExtractionCB.Enabled = false;
            ObjectAppendExtractionCB.Checked = false;

            DownloadZipfileButton.Enabled = false;

            DescriptionTabPage.Enabled = false;
            DependenciesTabPage.Enabled = false;
            MediaTabPage.Enabled = false;
            UserDatasTabPage.Enabled = false;
            if (hardReset)
            {
                // Description tab
                ObjectDescTB.Text = "";
                ObjectUpdateNotesTB.Text = "";

                // Dependencies tab
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesLabel.Text = "dependencies (click to edit)";
                CurrentDependenciesCB.DataSource = null;
                CurrentDependenciesCB.Items.Clear();
                CurrentDependenciesCB.SelectedIndex = -1;
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                CurrentLogicalDependenciesCB.DataSource = null;
                CurrentLogicalDependenciesCB.Items.Clear();
                CurrentLogicalDependenciesCB.SelectedIndex = -1;
                LogicalDependnecyNegateFlagCB.CheckedChanged -= LogicalDependnecyNegateFlagCB_CheckedChanged;
                LogicalDependnecyNegateFlagCB.Checked = false;
                LogicalDependnecyNegateFlagCB.CheckedChanged += LogicalDependnecyNegateFlagCB_CheckedChanged;

                // Pictures tab
                ObjectPicturesList.DataSource = null;
                ObjectPicturesList.Items.Clear();
                PicturesTypeCBox.SelectedIndex = -1;
                PictureURLTB.Text = "";

                // Userdata tab
                ObjectUserdatasList.DataSource = null;
                ObjectUserdatasList.Items.Clear();
                ObjectUserdatasTB.Text = "";
            }
        }

        private void DisplayDatabaseConfigs(DatabaseTreeNode parrent, List<SelectablePackage> configs)
        {
            foreach (SelectablePackage c in configs)
            {
                SearchBox.Items.Add(c.PackageName);
                allPackageNames.Add(c.PackageName);
                DatabaseTreeNode ConfigParrent = new DatabaseTreeNode(c, (int)DatabaseEditorMode);
                parrent.Nodes.Add(ConfigParrent);
                if (SelectedDatabaseObject != null && SelectedDatabaseObject.PackageName.Equals(c.PackageName))
                    ConfigParrent.EnsureVisible();
                DisplayDatabaseConfigs(ConfigParrent, c.Packages);
            }
        }
        //show the load database dialog and load the database
        private void LoadDatabaseButton_Click(object sender, EventArgs e)
        {
            string workingDirectory = Path.Combine(string.IsNullOrEmpty(Settings.CustomModInfoPath) ? Application.StartupPath : Settings.CustomModInfoPath);
            if (Directory.Exists(workingDirectory))
            {
                OpenDatabaseDialog.InitialDirectory = workingDirectory;
            }
            if (OpenDatabaseDialog.ShowDialog() == DialogResult.Cancel)
                return;
            if (!File.Exists(OpenDatabaseDialog.FileName))
                return;
            DatabaseLocation = OpenDatabaseDialog.FileName;
            LoadDatabase();
        }
        //method for actually loading the database
        private void LoadDatabase()
        {
            //Save this key data for later queries and comparisons 
            LoadedDatabaseMD5Hash = GetDatabaseMD5Hash(DatabaseLocation);
            DatabaseLocationMD5Hash = GetDatabaseLocationMD5Hash(DatabaseLocation);
            //Xpath for the folder version: //modInfoAlpha.xml/@version
            Settings.TanksVersion = XMLUtils.GetXMLElementAttributeFromFile(DatabaseLocation, "//modInfoAlpha.xml/@version");
            //Xpath for the onlineFolder version: //modInfoAlpha.xml/@onlineFolder
            Settings.TanksOnlineFolderVersion = XMLUtils.GetXMLElementAttributeFromFile(DatabaseLocation, "//modInfoAlpha.xml/@onlineFolder");
            Text = String.Format("DatabaseEditor      GameVersion: {0}    OnlineFolder: {1}", Settings.TanksVersion, Settings.TanksOnlineFolderVersion);
            GlobalDependencies = new List<Dependency>();
            Dependencies = new List<Dependency>();
            LogicalDependencies = new List<LogicalDependency>();
            ParsedCategoryList = new List<Category>();
            XMLUtils.CreateModStructure(DatabaseLocation, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCategoryList,true);
            if(GlobalDependencyRB.Checked)
            {
                DatabaseEditorMode = EditorMode.GlobalDependnecy;
            }
            else if (DependencyRB.Checked)
            {
                DatabaseEditorMode = EditorMode.Dependency;
            }
            else if (LogicalDependencyRB.Checked)
            {
                DatabaseEditorMode = EditorMode.LogicalDependency;
            }
            else
            {
                DatabaseEditorMode = EditorMode.DBO;
            }
            DisplayDatabase();
        }
        //calculate the MD5 of the selected file
        private byte[] GetDatabaseMD5Hash(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    using (var md5 = System.Security.Cryptography.MD5.Create())
                    {
                        using (var stream = File.OpenRead(filename))
                        {
                            return md5.ComputeHash(stream);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("GetDatabaseMD5Hash", "filename: " + filename, ex);
                    MessageBox.Show("File access error occurs.\n\nFor your safty, try to safe your work and restart the DatabaseEditor again.", "CRITICAL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return new byte[] { 0x0 };
        }
        //calculate the MD5 of the selected filepath
        private byte[] GetDatabaseLocationMD5Hash(string filename)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                //now calculate the value, but first lower all string elements to avoid wrong results for the non "case sensitive" windows system
                return md5.ComputeHash(Encoding.UTF8.GetBytes(filename.ToLower()));
            }
        }
        //show the save database dialog and save the database
        private void SaveDatabaseButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded", "CRITICAL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (SaveDatabaseDialog.ShowDialog() == DialogResult.Cancel)
                return;
            DatabaseLocation = SaveDatabaseDialog.FileName;
            //Is the current selected database file, the same file that was selected at loading? If "yes", go to the next check
            if (Utils.CompareByteArray(DatabaseLocationMD5Hash, GetDatabaseLocationMD5Hash(DatabaseLocation)))
            {
                //If the currently selected database file does not have the same MD5 value as at the time of loading, display the warning message. 
                if (!Utils.CompareByteArray(LoadedDatabaseMD5Hash, GetDatabaseMD5Hash(DatabaseLocation)))
                {
                    if (MessageBox.Show("The database file has already been changed since the last loading!\n\nContinue SAVING and OVERWRITE all changes of the file?", "CRITICAL", MessageBoxButtons.OKCancel, MessageBoxIcon.Stop) == DialogResult.Cancel) return;
                    Logging.Manager(string.Format("Database file '{0}' was overwritten by user against MD5 check-result-warning (MD5 file overwritten: {1})", DatabaseLocation, Utils.ConvertByteArrayToString(GetDatabaseMD5Hash(DatabaseLocation))));
                }
            }
            XMLUtils.SaveDatabase(DatabaseLocation, Settings.TanksVersion, Settings.TanksOnlineFolderVersion, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCategoryList);
            UnsavedModifications = false;
            //Save this key data for later queries and comparisons 
            LoadedDatabaseMD5Hash = GetDatabaseMD5Hash(DatabaseLocation);
            DatabaseLocationMD5Hash = GetDatabaseLocationMD5Hash(DatabaseLocation);
        }

        private void CheckModInfoChanged()
        {
            byte[] fileHash = GetDatabaseMD5Hash(DatabaseLocation);
            if (!Utils.CompareByteArray(fileHash, new byte[] { 0x0 }) &&  !Utils.CompareByteArray(LoadedDatabaseMD5Hash, fileHash))
            {
                DialogResult result = MessageBox.Show("File location: " + Path.GetDirectoryName(DatabaseLocation) + "\n\nThe last loaded " + Path.GetFileName(DatabaseLocation) + " was modified from another program. Maybe a merge or pull with github!?\n\nThe last changes are NOT at this DatabaseEditor session.\n\nOK (recommended):\nreload the modified " + Path.GetFileName(DatabaseLocation) + " file and discard all changes since last load/save at this DatabaseEditor session?\n\nCANCEL (NOT recommended):\nignore the changes at the " + Path.GetFileName(DatabaseLocation) + " file?\nWARNING! You maybe delete work of another member!" , "CRITICAL", MessageBoxButtons.OKCancel, MessageBoxIcon.Stop);
                if (result == DialogResult.Cancel)
                {
                    LoadedDatabaseMD5Hash = fileHash;
                }
                if (result == DialogResult.OK)
                {
                    LoadDatabase();
                }
            }
            else
            {
                UnsavedModifications = true;
            }
        }

        //Apply all changes from the form
        private void ApplyChangesButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            if (MessageBox.Show("Confirm you wish to apply changes", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            int duplicates = 0;
            if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
            {
                if (!SelectedGlobalDependency.PackageName.Equals(ObjectPackageNameTB.Text))
                    duplicates++;
            }
            else if (DatabaseEditorMode == EditorMode.Dependency)
            {
                if (!SelectedDependency.PackageName.Equals(ObjectPackageNameTB.Text))
                    duplicates++;
            }
            else if (DatabaseEditorMode == EditorMode.LogicalDependency)
            {
                if (!SelectedLogicalDependency.PackageName.Equals(ObjectPackageNameTB.Text))
                    duplicates++;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                if (SelectedCategory == null)
                {
                    if (!SelectedDatabaseObject.PackageName.Equals(ObjectPackageNameTB.Text))
                        duplicates++;
                }
            }
            if (DuplicatePackageName(this.GlobalDependencies,this.Dependencies,this.LogicalDependencies,this.ParsedCategoryList, ObjectPackageNameTB.Text,duplicates))
            {
                MessageBox.Show(string.Format("Package name: {0}, already exists", ObjectPackageNameTB.Text));
                return;
            }
            if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
            {
                int index = GlobalDependencies.IndexOf(SelectedGlobalDependency);
                SelectedGlobalDependency.PackageName = ObjectPackageNameTB.Text;
                SelectedGlobalDependency.StartAddress = ObjectStartAddressTB.Text;
                SelectedGlobalDependency.EndAddress = ObjectEndAddressTB.Text;
                SelectedGlobalDependency.DevURL = ObjectDevURLTB.Text;
                if (!SelectedGlobalDependency.ZipFile.Equals(ObjectZipFileTB.Text))
                {
                    SelectedGlobalDependency.CRC = "f";
                    SelectedGlobalDependency.ZipFile = ObjectZipFileTB.Text;
                    SelectedGlobalDependency.Timestamp = Utils.GetCurrentUniversalFiletimeTimestamp();
                    ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(SelectedGlobalDependency.Timestamp));
                }
                SelectedGlobalDependency.Enabled = ObjectEnabledCheckBox.Checked;
                SelectedGlobalDependency.AppendExtraction = ObjectAppendExtractionCB.Checked;
                SelectedGlobalDependency.LogAtInstall = ObjectLogAtInstallCB.Checked;
                GlobalDependencies[index] = SelectedGlobalDependency;
            }
            else if (DatabaseEditorMode == EditorMode.Dependency)
            {
                int index = Dependencies.IndexOf(SelectedDependency);
                SelectedDependency.PackageName = ObjectPackageNameTB.Text;
                SelectedDependency.StartAddress = ObjectStartAddressTB.Text;
                SelectedDependency.EndAddress = ObjectEndAddressTB.Text;
                SelectedDependency.DevURL = ObjectDevURLTB.Text;
                if (!SelectedDependency.ZipFile.Equals(ObjectZipFileTB.Text))
                {
                    SelectedDependency.CRC = "f";
                    SelectedDependency.ZipFile = ObjectZipFileTB.Text;
                    SelectedDependency.Timestamp = Utils.GetCurrentUniversalFiletimeTimestamp();
                    ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(SelectedDependency.Timestamp));
                }
                SelectedDependency.Enabled = ObjectEnabledCheckBox.Checked;
                SelectedDependency.AppendExtraction = ObjectAppendExtractionCB.Checked;
                SelectedDependency.LogAtInstall = ObjectLogAtInstallCB.Checked;
                Dependencies[index] = SelectedDependency;
            }
            else if (DatabaseEditorMode == EditorMode.LogicalDependency)
            {
                int index = LogicalDependencies.IndexOf(SelectedLogicalDependency);
                SelectedLogicalDependency.PackageName = ObjectPackageNameTB.Text;
                SelectedLogicalDependency.StartAddress = ObjectStartAddressTB.Text;
                SelectedLogicalDependency.EndAddress = ObjectEndAddressTB.Text;
                SelectedLogicalDependency.DevURL = ObjectDevURLTB.Text;
                if (!SelectedLogicalDependency.ZipFile.Equals(ObjectZipFileTB.Text))
                {
                    SelectedLogicalDependency.CRC = "f";
                    SelectedLogicalDependency.ZipFile = ObjectZipFileTB.Text;
                    SelectedLogicalDependency.Timestamp = Utils.GetCurrentUniversalFiletimeTimestamp();
                    ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(SelectedLogicalDependency.Timestamp));
                }
                SelectedLogicalDependency.Enabled = ObjectEnabledCheckBox.Checked;
                SelectedLogicalDependency.LogAtInstall = ObjectLogAtInstallCB.Checked;
                SelectedLogicalDependency.AndOrLogic = LogicalDependency.GetAndOrID(AndOrLogicComboBox.Text);
                LogicalDependencies[index] = SelectedLogicalDependency;
            }
            else if (SelectedCategory != null)
            {
                MessageBox.Show("Editing Categories is not supported. Talk to Willster about why you fell this change is needed");
                return;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                if(!AcceptableType(SelectedDatabaseObject))
                {
                    MessageBox.Show("Invalid index of package type for level");
                    return;
                }
                //get the Packages list that contains this package
                ListThatContainsPackage = null;
                ListContainsPackage(SelectedDatabaseObject);
                //found the list, get the index of the currnet entry
                int index = ListThatContainsPackage.IndexOf(SelectedDatabaseObject);
                //make changes
                SelectedDatabaseObject.Name = ObjectNameTB.Text;
                SelectedDatabaseObject.PackageName = ObjectPackageNameTB.Text;
                SelectedDatabaseObject.StartAddress = ObjectStartAddressTB.Text;
                SelectedDatabaseObject.EndAddress = ObjectEndAddressTB.Text;
                if (!SelectedDatabaseObject.ZipFile.Equals(ObjectZipFileTB.Text))
                {
                    SelectedDatabaseObject.CRC = "f";
                    SelectedDatabaseObject.ZipFile = ObjectZipFileTB.Text;
                    SelectedDatabaseObject.Timestamp = Utils.GetCurrentUniversalFiletimeTimestamp();
                    ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(SelectedDatabaseObject.Timestamp));
                }
                SelectedDatabaseObject.DevURL = ObjectDevURLTB.Text;
                SelectedDatabaseObject.Version = ObjectVersionTB.Text;
                SelectedDatabaseObject.Type = (string)ObjectTypeComboBox.SelectedItem;
                SelectedDatabaseObject.Enabled = ObjectEnabledCheckBox.Checked;
                SelectedDatabaseObject.Visible = ObjectVisibleCheckBox.Checked;
                SelectedDatabaseObject.LogAtInstall = ObjectLogAtInstallCB.Checked;
                SelectedDatabaseObject.Description = ObjectDescTB.Text;
                SelectedDatabaseObject.UpdateComment = ObjectUpdateNotesTB.Text;
                ListThatContainsPackage[index] = SelectedDatabaseObject;
                
            }
            this.DisplayDatabase(false);
            CheckModInfoChanged();
        }
        private void ListContainsPackage(SelectablePackage sp)
        {
            ListThatContainsPackage = null;
            foreach (Category cat in ParsedCategoryList)
            {
                if (cat.Packages.Contains(sp))
                {
                    ListThatContainsPackage = cat.Packages;
                    return;
                }
                if(cat.Packages.Count > 0)
                    ListContainsPackageRecursive(cat.Packages, sp);
            }
        }
        private void ListContainsPackageRecursive(List<SelectablePackage> packageList, SelectablePackage sp)
        {
            foreach (SelectablePackage c in packageList)
            {
                if (c.Packages.Contains(sp) && ListThatContainsPackage == null)
                {
                    ListThatContainsPackage = c.Packages;
                    return;
                }
                if (c.Packages.Count > 0)
                {
                    ListContainsPackageRecursive(c.Packages, sp);
                }
            }
        }
        //mode set to globalDependency
        private void GlobalDependencyRB_CheckedChanged(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            RadioButton rb1 = (RadioButton)sender;
            if (rb1.Checked)
            {
                DatabaseEditorMode = EditorMode.GlobalDependnecy;
                ResetUI(false, true);
                DisplayDatabase();
            }
        }
        //mode set to dependency
        private void DependencyRB_CheckedChanged(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            RadioButton rb1 = (RadioButton)sender;
            if (rb1.Checked)
            {
                DatabaseEditorMode = EditorMode.Dependency;
                ResetUI(false, true);
                DisplayDatabase();
            }
        }
        //mode set to logicalDependency
        private void LogicalDependencyRB_CheckedChanged(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            RadioButton rb1 = (RadioButton)sender;
            if (rb1.Checked)
            {
                DatabaseEditorMode = EditorMode.LogicalDependency;
                ResetUI(false, true);
                DisplayDatabase();
            }
        }
        //mode set to DBO
        private void DBO_CheckedChanged(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            RadioButton rb1 = (RadioButton)sender;
            if (rb1.Checked)
            {
                DatabaseEditorMode = EditorMode.DBO;
                ResetUI(false, true);
                DisplayDatabase();
            }
        }

        private void DatabaseTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            try
            {
                DatabaseTreeView.SelectedNode = e.Node;
                DatabaseTreeView.Focus();
                e.Node.ForeColor = System.Drawing.Color.Blue;
                currentSelectedIndex = DatabaseTreeView.SelectedNode.Index;
                ResetUI(false, true);
                DatabaseTreeNode node = (DatabaseTreeNode)DatabaseTreeView.SelectedNode;
                if (node.GlobalDependency != null)
                {
                    SelectedGlobalDependency = node.GlobalDependency;
                    SelectedDependency = null;
                    SelectedLogicalDependency = null;
                    SelectedDatabaseObject = null;
                    SelectedCategory = null;

                    ObjectNameTB.Enabled = false;

                    ObjectPackageNameTB.Enabled = true;
                    ObjectPackageNameTB.Text = node.GlobalDependency.PackageName;

                    ObjectStartAddressTB.Enabled = true;
                    ObjectStartAddressTB.Text = node.GlobalDependency.StartAddress;

                    ObjectEndAddressTB.Enabled = true;
                    ObjectEndAddressTB.Text = node.GlobalDependency.EndAddress;

                    ObjectLogAtInstallCB.Enabled = true;
                    ObjectLogAtInstallCB.Checked = node.GlobalDependency.LogAtInstall;

                    ObjectZipFileTB.Enabled = true;
                    ObjectZipFileTB.Text = node.GlobalDependency.ZipFile;

                    ObjectLevelLabel.Text = "0";

                    ObjectVersionTB.Enabled = false;

                    if (node.GlobalDependency.Timestamp > 0)
                        ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(node.GlobalDependency.Timestamp));
                    else
                        ObjectLastUpdatedLabel.Text = "last updated: (none)";

                    ObjectDevURLTB.Enabled = true;
                    ObjectDevURLTB.Text = SelectedGlobalDependency.DevURL;

                    ObjectTypeComboBox.Enabled = false;
                    ObjectTypeComboBox.SelectedIndex = 0;

                    ObjectEnabledCheckBox.Enabled = true;
                    ObjectEnabledCheckBox.Checked = node.GlobalDependency.Enabled;

                    ObjectVisibleCheckBox.Enabled = false;

                    ObjectAppendExtractionCB.Enabled = true;
                    ObjectAppendExtractionCB.Checked = node.GlobalDependency.AppendExtraction;

                    DownloadZipfileButton.Enabled = true;

                    DescriptionTabPage.Enabled = false;

                    DependenciesTabPage.Enabled = false;
                    ObjectDependenciesLabel.Text = "dependencies (click to edit)";
                    AndOrLogicLabel.Visible = false;
                    AndOrLogicComboBox.Visible = false;
                    MediaTabPage.Enabled = false;
                    UserDatasTabPage.Enabled = false;
                }
                else if (node.Dependency != null)
                {
                    SelectedGlobalDependency = null;
                    SelectedDependency = node.Dependency;
                    SelectedLogicalDependency = null;
                    SelectedDatabaseObject = null;
                    SelectedCategory = null;

                    ObjectNameTB.Enabled = false;

                    ObjectPackageNameTB.Enabled = true;
                    ObjectPackageNameTB.Text = SelectedDependency.PackageName;

                    ObjectStartAddressTB.Enabled = true;
                    ObjectStartAddressTB.Text = SelectedDependency.StartAddress;

                    ObjectEndAddressTB.Enabled = true;
                    ObjectEndAddressTB.Text = SelectedDependency.EndAddress;

                    ObjectLogAtInstallCB.Enabled = true;
                    ObjectLogAtInstallCB.Checked = SelectedDependency.LogAtInstall;

                    ObjectZipFileTB.Enabled = true;
                    ObjectZipFileTB.Text = SelectedDependency.ZipFile;

                    ObjectVersionTB.Enabled = false;

                    if (SelectedDependency.Timestamp > 0)
                        ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(SelectedDependency.Timestamp));
                    else
                        ObjectLastUpdatedLabel.Text = "last updated: (none)";

                    ObjectDevURLTB.Enabled = true;
                    ObjectDevURLTB.Text = SelectedDependency.DevURL;

                    ObjectTypeComboBox.Enabled = false;
                    ObjectTypeComboBox.SelectedIndex = 0;

                    ObjectLevelLabel.Text = "0";

                    ObjectEnabledCheckBox.Enabled = true;
                    ObjectEnabledCheckBox.Checked = SelectedDependency.Enabled;

                    ObjectVisibleCheckBox.Enabled = false;

                    ObjectAppendExtractionCB.Enabled = true;
                    ObjectAppendExtractionCB.Checked = SelectedDependency.AppendExtraction;

                    DownloadZipfileButton.Enabled = true;

                    DescriptionTabPage.Enabled = false;

                    DependenciesTabPage.Enabled = true;
                    DependencyPanel.Enabled = true;
                    ObjectDependenciesLabel.Text = "Objects that use this dependency...";
                    ObjectDependenciesLabel.Enabled = true;
                    ObjectDependenciesList.Enabled = true;
                    ObjectDependenciesList.DataSource = BuildDatabaseLogic(SelectedDependency);

                    ObjectDependenciesList.Height = CurrentDependenciesCB.Height + CurrentDependenciesCB.Top - ObjectDependenciesList.Top;
                    DependencyPackageNameLabel.Visible = false;
                    CurrentDependenciesCB.Visible = false;
                    AndOrLogicLabel.Visible = false;
                    AndOrLogicComboBox.Visible = false;
                    AddDependencyButton.Visible = false;
                    RemoveDependencyButton.Visible = false;

                    LogicalDependencyPanel.Enabled = true;
                    ObjectLogicalDependenciesList.DataSource = null;
                    ObjectLogicalDependenciesList.Items.Clear();
                    ObjectLogicalDependenciesList.DataSource = SelectedDependency.LogicalDependencies;
                    CurrentLogicalDependenciesCB.DataSource = null;
                    CurrentLogicalDependenciesCB.Items.Clear();
                    CurrentLogicalDependenciesCB.DataSource = LogicalDependencies;
                    CurrentLogicalDependenciesCB.SelectedIndex = -1;
                    LogicalDependnecyNegateFlagCB.CheckedChanged -= LogicalDependnecyNegateFlagCB_CheckedChanged;
                    LogicalDependnecyNegateFlagCB.Checked = false;
                    LogicalDependnecyNegateFlagCB.CheckedChanged += LogicalDependnecyNegateFlagCB_CheckedChanged;

                    MediaTabPage.Enabled = false;
                    UserDatasTabPage.Enabled = false;
                }
                else if (node.LogicalDependency != null)
                {
                    SelectedGlobalDependency = null;
                    SelectedDependency = null;
                    SelectedLogicalDependency = node.LogicalDependency;
                    SelectedDatabaseObject = null;
                    SelectedCategory = null;

                    ObjectNameTB.Enabled = false;

                    ObjectPackageNameTB.Enabled = true;
                    ObjectPackageNameTB.Text = SelectedLogicalDependency.PackageName;

                    ObjectStartAddressTB.Enabled = true;
                    ObjectStartAddressTB.Text = SelectedLogicalDependency.StartAddress;

                    ObjectEndAddressTB.Enabled = true;
                    ObjectEndAddressTB.Text = SelectedLogicalDependency.EndAddress;

                    ObjectLogAtInstallCB.Enabled = true;
                    ObjectLogAtInstallCB.Checked = SelectedLogicalDependency.LogAtInstall;

                    ObjectZipFileTB.Enabled = true;
                    ObjectZipFileTB.Text = SelectedLogicalDependency.ZipFile;

                    ObjectVersionTB.Enabled = false;

                    if (SelectedLogicalDependency.Timestamp > 0)
                        ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(SelectedLogicalDependency.Timestamp));
                    else
                        ObjectLastUpdatedLabel.Text = "last updated: (none)";

                    ObjectDevURLTB.Enabled = true;
                    ObjectDevURLTB.Text = SelectedLogicalDependency.DevURL;

                    ObjectTypeComboBox.Enabled = false;
                    ObjectTypeComboBox.SelectedIndex = 0;

                    ObjectLevelLabel.Text = "0";

                    ObjectEnabledCheckBox.Enabled = true;
                    ObjectEnabledCheckBox.Checked = SelectedLogicalDependency.Enabled;

                    ObjectVisibleCheckBox.Enabled = false;

                    ObjectAppendExtractionCB.Enabled = false;

                    DownloadZipfileButton.Enabled = true;

                    DescriptionTabPage.Enabled = false;

                    DependenciesTabPage.Enabled = true;
                    DependencyPanel.Enabled = true;
                    LogicalDependencyPanel.Enabled = false;
                    MediaTabPage.Enabled = false;
                    UserDatasTabPage.Enabled = false;

                    ObjectDependenciesLabel.Text = "Objects that use this logical dependency...";
                    SelectedLogicalDependency.DatabasePackageLogic.Clear();
                    BuildDatabaseLogic(SelectedLogicalDependency);
                    ObjectDependenciesList.DataSource = null;
                    ObjectDependenciesList.Items.Clear();
                    ObjectDependenciesList.DataSource = SelectedLogicalDependency.DatabasePackageLogic;
                    ObjectDependenciesList.Height = CurrentDependenciesCB.Height + CurrentDependenciesCB.Top - ObjectDependenciesList.Top;
                    DependencyPackageNameLabel.Visible = false;
                    CurrentDependenciesCB.Visible = false;
                    AndOrLogicLabel.Visible = true;
                    AndOrLogicComboBox.Visible = true;
                    AndOrLogicComboBox.Text = LogicalDependency.GetAndOrString(SelectedLogicalDependency.AndOrLogic);
                    AddDependencyButton.Visible = false;
                    RemoveDependencyButton.Visible = false;
                }
                else if (node.DatabaseObject != null)
                {
                    SelectedGlobalDependency = null;
                    SelectedDependency = null;
                    SelectedLogicalDependency = null;
                    SelectedDatabaseObject = node.DatabaseObject;
                    SelectedCategory = null;

                    ObjectNameTB.Enabled = true;
                    ObjectNameTB.Text = SelectedDatabaseObject.Name;

                    ObjectPackageNameTB.Enabled = true;
                    ObjectPackageNameTB.Text = SelectedDatabaseObject.PackageName;

                    ObjectStartAddressTB.Enabled = true;
                    ObjectStartAddressTB.Text = SelectedDatabaseObject.StartAddress;

                    ObjectEndAddressTB.Enabled = true;
                    ObjectEndAddressTB.Text = SelectedDatabaseObject.EndAddress;

                    ObjectLogAtInstallCB.Enabled = true;
                    ObjectLogAtInstallCB.Checked = SelectedDatabaseObject.LogAtInstall;

                    ObjectZipFileTB.Enabled = true;
                    ObjectZipFileTB.Text = SelectedDatabaseObject.ZipFile;

                    ObjectVersionTB.Enabled = true;
                    ObjectVersionTB.Text = SelectedDatabaseObject.Version;

                    if (SelectedDatabaseObject.Timestamp > 0)
                        ObjectLastUpdatedLabel.Text = string.Format("last updated: {0}", Utils.ConvertFiletimeTimestampToDate(SelectedDatabaseObject.Timestamp));
                    else
                        ObjectLastUpdatedLabel.Text = "last updated: (none)";

                    ObjectDevURLTB.Enabled = true;
                    ObjectDevURLTB.Text = SelectedDatabaseObject.DevURL;

                    ObjectTypeComboBox.Enabled = true;
                    switch (SelectedDatabaseObject.Type)
                    {
                        case "single":
                            ObjectTypeComboBox.SelectedIndex = 1;
                            break;
                        case "single1":
                            ObjectTypeComboBox.SelectedIndex = 1;
                            break;
                        case "single_dropdown":
                            ObjectTypeComboBox.SelectedIndex = 2;
                            break;
                        case "single_dropdown1":
                            ObjectTypeComboBox.SelectedIndex = 2;
                            break;
                        case "single_dropdown2":
                            ObjectTypeComboBox.SelectedIndex = 3;
                            break;
                        case "multi":
                            ObjectTypeComboBox.SelectedIndex = 4;
                            break;
                    }
                    
                    ObjectLevelLabel.Text = "" + SelectedDatabaseObject.Level;

                    ObjectEnabledCheckBox.Enabled = true;
                    ObjectEnabledCheckBox.Checked = SelectedDatabaseObject.Enabled;

                    ObjectVisibleCheckBox.Enabled = true;
                    ObjectVisibleCheckBox.Checked = SelectedDatabaseObject.Visible;

                    ObjectAppendExtractionCB.Enabled = false;

                    DescriptionTabPage.Enabled = true;

                    ObjectDescTB.Enabled = true;
                    ObjectDescTB.Text = SelectedDatabaseObject.Description;

                    ObjectUpdateNotesTB.Enabled = true;
                    ObjectUpdateNotesTB.Text = SelectedDatabaseObject.UpdateComment;

                    DownloadZipfileButton.Enabled = true;

                    DependenciesTabPage.Enabled = true;
                    DependencyPanel.Enabled = true;
                    LogicalDependencyPanel.Enabled = true;
                    MediaTabPage.Enabled = true;
                    UserDatasTabPage.Enabled = true;

                    //logicalDependencies
                    ObjectLogicalDependenciesList.DataSource = SelectedDatabaseObject.LogicalDependencies;
                    CurrentLogicalDependenciesCB.DataSource = LogicalDependencies;
                    CurrentLogicalDependenciesCB.SelectedIndex = -1;

                    //dependencies
                    ObjectDependenciesLabel.Text = "dependencies (click to edit)";
                    ObjectDependenciesList.DataSource = SelectedDatabaseObject.Dependencies;
                    ObjectDependenciesList.Height = ObjectDependenciesListOriginalHeight;
                    DependencyPackageNameLabel.Visible = true;
                    CurrentDependenciesCB.Visible = true;
                    CurrentDependenciesCB.DataSource = Dependencies;
                    CurrentDependenciesCB.SelectedIndex = -1;
                    AndOrLogicLabel.Visible = false;
                    AndOrLogicComboBox.Visible = false;
                    AddDependencyButton.Visible = true;
                    RemoveDependencyButton.Visible = true;

                    //pictures
                    ObjectPicturesList.DataSource = SelectedDatabaseObject.PictureList;
                    if (SelectedDatabaseObject.PictureList.Count == 0)
                        PictureURLTB.Text = "";

                    //userdatas
                    ObjectUserdatasList.DataSource = SelectedDatabaseObject.UserFiles;
                }
                else if (node.Category != null)
                {
                    SelectedGlobalDependency = null;
                    SelectedDependency = null;
                    SelectedLogicalDependency = null;
                    SelectedDatabaseObject = null;
                    SelectedCategory = node.Category;

                    ObjectNameTB.Enabled = true;
                    ObjectNameTB.Text = SelectedCategory.Name;

                    ObjectPackageNameTB.Enabled = false;

                    ObjectStartAddressTB.Enabled = false;

                    ObjectEndAddressTB.Enabled = false;

                    ObjectLogAtInstallCB.Enabled = false;

                    ObjectZipFileTB.Enabled = false;

                    ObjectVersionTB.Enabled = false;

                    ObjectLastUpdatedLabel.Text = "";

                    ObjectLevelLabel.Text = "-1";

                    ObjectDevURLTB.Enabled = false;

                    ObjectTypeComboBox.Enabled = false;

                    ObjectEnabledCheckBox.Enabled = false;

                    ObjectVisibleCheckBox.Enabled = false;

                    ObjectAppendExtractionCB.Enabled = false;

                    DownloadZipfileButton.Enabled = false;

                    ObjectDescTB.Enabled = false;

                    ObjectUpdateNotesTB.Enabled = false;

                    DependenciesTabPage.Enabled = true;
                    DependencyPanel.Enabled = true;
                    LogicalDependencyPanel.Enabled = false;
                    MediaTabPage.Enabled = false;
                    UserDatasTabPage.Enabled = false;

                    ObjectDependenciesLabel.Text = "dependencies (click to edit)";
                    ObjectDependenciesList.DataSource = SelectedCategory.Dependencies;
                    ObjectDependenciesList.Height = ObjectDependenciesListOriginalHeight;
                    DependencyPackageNameLabel.Visible = true;
                    CurrentDependenciesCB.DataSource = Dependencies;
                    CurrentDependenciesCB.SelectedIndex = -1;
                    CurrentDependenciesCB.Visible = true;
                    AndOrLogicLabel.Visible = false;
                    AndOrLogicComboBox.Visible = false;
                    AddDependencyButton.Visible = true;
                    RemoveDependencyButton.Visible = true;
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("DatabaseTreeView_NodeMouseClick", ex);
                MessageBox.Show("Exception at DatabaseTreeView_NodeMouseClick()", "CRITICAL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
            }
        }

        private void MoveButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            if (SelectedCategory != null)
            {
                MessageBox.Show("Moving categories is not supported");
                return;
            }
            if (MessageBox.Show("Confirm you wish to move the object?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            using (DatabaseAdder dba = new DatabaseAdder(DatabaseEditorMode, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCategoryList, true))
            {
                dba.ShowDialog();
                if (dba.DialogResult != DialogResult.OK)
                    return;
                if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
                {
                    if (dba.SelectedGlobalDependency == null)
                        return;
                    GlobalDependencies.Remove(SelectedGlobalDependency);
                    int index = GlobalDependencies.IndexOf(dba.SelectedGlobalDependency);
                    GlobalDependencies.Insert(index, SelectedGlobalDependency);
                    DisplayDatabase();
                }
                else if (DatabaseEditorMode == EditorMode.Dependency)
                {
                    if (dba.SelectedDependency == null)
                        return;
                    Dependencies.Remove(SelectedDependency);
                    int index = Dependencies.IndexOf(dba.SelectedDependency);
                    Dependencies.Insert(index, SelectedDependency);
                    DisplayDatabase();
                }
                else if (DatabaseEditorMode == EditorMode.LogicalDependency)
                {
                    if (dba.SelectedLogicalDependency == null)
                        return;
                    LogicalDependencies.Remove(SelectedLogicalDependency);
                    int index = LogicalDependencies.IndexOf(dba.SelectedLogicalDependency);
                    LogicalDependencies.Insert(index, SelectedLogicalDependency);
                    DisplayDatabase();
                }
                else if (DatabaseEditorMode == EditorMode.DBO)
                {
                    if (!AcceptableType(dba.SelectedDatabaseObject))
                    {
                        MessageBox.Show("Invalid index of package type for level");
                        return;
                    }
                    ListContainsPackage(SelectedDatabaseObject);
                    ListThatContainsPackage.Remove(SelectedDatabaseObject);
                    ListContainsPackage(dba.SelectedDatabaseObject);
                    int index = ListThatContainsPackage.IndexOf(dba.SelectedDatabaseObject);
                    ListThatContainsPackage.Insert(index, SelectedDatabaseObject);
                }
                
            }
            this.DisplayDatabase(false);
            CheckModInfoChanged();
        }

        private void AddEntryButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            if (SelectedCategory != null)
            {
                MessageBox.Show("Adding categories is not supported");
                return;
            }
            using (DatabaseAdder dba = new DatabaseAdder(DatabaseEditorMode, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCategoryList, false))
            {
                dba.ShowDialog();
                if (dba.DialogResult == DialogResult.OK)
                {
                    if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
                    {
                        if (dba.SelectedGlobalDependency == null)
                            return;
                        Dependency newDep = new Dependency()
                        {
                            PackageName = this.GetNewPackageName(ObjectPackageNameTB.Text),
                            StartAddress = ObjectStartAddressTB.Text,
                            EndAddress = ObjectEndAddressTB.Text,
                            ZipFile = ObjectZipFileTB.Text,
                            Enabled = ObjectEnabledCheckBox.Checked,
                            AppendExtraction = ObjectAppendExtractionCB.Checked,
                            DevURL = ObjectDevURLTB.Text,
                            CRC = ""
                        };
                        //newDep.ExtractPath = "";
                        if (!ObjectZipFileTB.Text.Equals(""))
                            newDep.CRC = "f";
                        int index = GlobalDependencies.IndexOf(dba.SelectedGlobalDependency);
                        GlobalDependencies.Insert(index, newDep);
                        DisplayDatabase();
                    }
                    else if (DatabaseEditorMode == EditorMode.Dependency)
                    {
                        if (dba.SelectedDependency == null)
                            return;
                        Dependency newDep = new Dependency()
                        {
                            PackageName = this.GetNewPackageName(ObjectPackageNameTB.Text),
                            StartAddress = ObjectStartAddressTB.Text,
                            EndAddress = ObjectEndAddressTB.Text,
                            ZipFile = ObjectZipFileTB.Text,
                            Enabled = ObjectEnabledCheckBox.Checked,
                            AppendExtraction = ObjectAppendExtractionCB.Checked,
                            DevURL = ObjectDevURLTB.Text,
                            CRC = ""
                        };
                        //newDep.ExtractPath = "";
                        if (!ObjectZipFileTB.Text.Equals(""))
                            newDep.CRC = "f";
                        List<LogicalDependency> logicalDeps = (List<LogicalDependency>)ObjectLogicalDependenciesList.DataSource;
                        int index = Dependencies.IndexOf(dba.SelectedDependency);
                        Dependencies.Insert(index, newDep);
                        DisplayDatabase();
                    }
                    else if (DatabaseEditorMode == EditorMode.LogicalDependency)
                    {
                        if (dba.SelectedLogicalDependency == null)
                            return;
                        LogicalDependency newDep = new LogicalDependency()
                        {
                            PackageName = this.GetNewPackageName(ObjectPackageNameTB.Text),
                            StartAddress = ObjectStartAddressTB.Text,
                            EndAddress = ObjectEndAddressTB.Text,
                            ZipFile = ObjectZipFileTB.Text,
                            Enabled = ObjectEnabledCheckBox.Checked,
                            DevURL = ObjectDevURLTB.Text,
                            CRC = ""
                        };
                        if (!ObjectZipFileTB.Text.Equals(""))
                            newDep.CRC = "f";
                        int index = LogicalDependencies.IndexOf(dba.SelectedLogicalDependency);
                        LogicalDependencies.Insert(index, newDep);
                        DisplayDatabase();
                    }
                    else if (DatabaseEditorMode == EditorMode.DBO)
                    {
                        if (!AcceptableType(dba.SelectedDatabaseObject))
                        {
                            MessageBox.Show("Invalid index of package type for level");
                            return;
                        }
                        //make changes
                        SelectablePackage cfg = new SelectablePackage()
                        {
                            Name = ObjectNameTB.Text,
                            PackageName = GetNewPackageName(ObjectPackageNameTB.Text),
                            StartAddress = ObjectStartAddressTB.Text,
                            EndAddress = ObjectEndAddressTB.Text,
                            ZipFile = ObjectZipFileTB.Text,
                            DevURL = ObjectDevURLTB.Text,
                            Version = ObjectVersionTB.Text,
                            Enabled = ObjectEnabledCheckBox.Checked,
                            Visible = ObjectVisibleCheckBox.Checked,
                            Description = ObjectDescTB.Text,
                            UpdateComment = ObjectUpdateNotesTB.Text,
                            CRC = ObjectZipFileTB.Text.Equals("")? "" : "f",
                            Type = (string)ObjectTypeComboBox.SelectedItem
                        };
                        if (dba.sublist)
                        {
                            dba.SelectedDatabaseObject.Packages.Add(cfg);
                            cfg.Level = dba.SelectedDatabaseObject.Level + 1;
                        }
                        else
                        {
                            ListContainsPackage(dba.SelectedDatabaseObject);
                            if (ListThatContainsPackage == null)
                            {
                                MessageBox.Show("Error finding list for selected package");
                                return;
                            }
                            int index = ListThatContainsPackage.IndexOf(dba.SelectedDatabaseObject);
                            cfg.Level = dba.SelectedDatabaseObject.Level;
                            ListThatContainsPackage.Insert(index, cfg);
                        }
                    }
                }
            }
            this.DisplayDatabase(false);
            CheckModInfoChanged();
        }

        private void RemoveEntryButton_Click(object sender, EventArgs e)
        {
            if (ParsedCategoryList == null || GlobalDependencies == null || Dependencies == null || LogicalDependencies == null)
            {
                MessageBox.Show("Database Not Loaded");
                return;
            }
            if (SelectedCategory != null)
            {
                MessageBox.Show("Removing categories is not supported");
                return;
            }
            if (MessageBox.Show("Confirm you wish to remove the object?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            if (DatabaseEditorMode == EditorMode.GlobalDependnecy)
            {
                GlobalDependencies.Remove(SelectedGlobalDependency);
            }
            else if (DatabaseEditorMode == EditorMode.Dependency)
            {
                //check if the dependency is in use first
                if (DependencyInUse(SelectedDependency.PackageName, true))
                {
                    MessageBox.Show(string.Format("Cannot remove because it is in use:\n{0}",InUseSB.ToString()));
                }
                else
                {
                    Dependencies.Remove(SelectedDependency);
                }
            }
            else if (DatabaseEditorMode == EditorMode.LogicalDependency)
            {
                //check if the dependency is in use first
                if (DependencyInUse(SelectedLogicalDependency.PackageName, false))
                {
                    MessageBox.Show(string.Format("Cannot remove because it is in use:\n{0}", InUseSB.ToString()));
                }
                else
                {
                    LogicalDependencies.Remove(SelectedLogicalDependency);
                }
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                ListContainsPackage(SelectedDatabaseObject);
                ListThatContainsPackage.Remove(SelectedDatabaseObject);
            }
            DisplayDatabase(false);
            CheckModInfoChanged();
        }

        private bool DependencyInUse(string packageName, bool isDependency)
        {
            InUseSB = new StringBuilder();
            bool InUse = false;
            if (!isDependency)
            {
                foreach (Dependency d in Dependencies)
                {
                    if (d.PackageName.Equals(packageName))
                    {
                        InUse = true;
                        InUseSB.Append(string.Format("Dependency: {0}\n", d.PackageName));
                    }
                }
            }
            foreach (Category c in ParsedCategoryList)
            {
                foreach (Dependency d in c.Dependencies)
                {
                    if (d.PackageName.Equals(packageName))
                    {
                        InUse = true;
                        InUseSB.Append(string.Format("Category: {0}", c.Name));
                    }
                }
                foreach (SelectablePackage m in c.Packages)
                {
                    foreach (Dependency d in m.Dependencies)
                    {
                        if (d.PackageName.Equals(packageName))
                        {
                            InUse = true;
                            InUseSB.Append(string.Format("Mod: {0}\n", m.PackageName));
                        }
                    }
                    foreach (LogicalDependency d in m.LogicalDependencies)
                    {
                        if (d.PackageName.Equals(packageName))
                        {
                            InUse = true;
                            InUseSB.Append(string.Format("Mod: {0}\n", m.PackageName));
                        }
                    }
                    ProcessConfigsInUse(InUseSB, m.Packages, InUse, packageName);
                }
            }
            return InUse;
        }
        private void ProcessConfigsInUse(StringBuilder sb, List<SelectablePackage> configs, bool InUse, string packageName)
        {
            foreach (SelectablePackage c in configs)
            {
                foreach (Dependency d in c.Dependencies)
                {
                    if (d.PackageName.Equals(packageName))
                    {
                        InUse = true;
                        InUseSB.Append(string.Format("Config: {0}\n", c.PackageName));
                    }
                }
                foreach (LogicalDependency d in c.LogicalDependencies)
                {
                    if (d.PackageName.Equals(packageName))
                    {
                        InUse = true;
                        InUseSB.Append(string.Format("Config: {0}\n", c.PackageName));
                    }
                }
                ProcessConfigsInUse(sb, c.Packages, InUse, packageName);
            }
        }

        private void AddDependencyButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to add dependency", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            if (SelectedCategory != null)
            {
                Dependency ld = new Dependency();
                Dependency ld2 = (Dependency)CurrentDependenciesCB.SelectedItem;
                ld.PackageName = ld2.PackageName;
                SelectedCategory.Dependencies.Add(ld);
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedCategory.Dependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                Dependency ld = new Dependency();
                Dependency ld2 = (Dependency)CurrentDependenciesCB.SelectedItem;
                ld.PackageName = ld2.PackageName;
                SelectedDatabaseObject.Dependencies.Add(ld);
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedDatabaseObject.Dependencies;
            }
            CheckModInfoChanged();
        }

        private void RemoveDependencyButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to remove dependency", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            if (SelectedCategory != null)
            {
                SelectedCategory.Dependencies.Remove((Dependency)ObjectDependenciesList.SelectedItem);
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedCategory.Dependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                SelectedDatabaseObject.Dependencies.Remove((Dependency)ObjectDependenciesList.SelectedItem);
                ObjectDependenciesList.DataSource = null;
                ObjectDependenciesList.Items.Clear();
                ObjectDependenciesList.DataSource = SelectedDatabaseObject.Dependencies;
            }
            CheckModInfoChanged();
        }

        private void AddLogicalDependencyButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to add logical dependency", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            if (DatabaseEditorMode == EditorMode.Dependency)
            {
                LogicalDependency ld = new LogicalDependency();
                LogicalDependency ld2 = (LogicalDependency)CurrentLogicalDependenciesCB.SelectedItem;
                ld.PackageName = ld2.PackageName;
                ld.NegateFlag = LogicalDependnecyNegateFlagCB.Checked;
                SelectedDependency.LogicalDependencies.Add(ld);
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDependency.LogicalDependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                LogicalDependency ld = new LogicalDependency();
                LogicalDependency ld2 = (LogicalDependency)CurrentLogicalDependenciesCB.SelectedItem;
                ld.PackageName = ld2.PackageName;
                ld.NegateFlag = LogicalDependnecyNegateFlagCB.Checked;
                SelectedDatabaseObject.LogicalDependencies.Add(ld);
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDatabaseObject.LogicalDependencies;
            }
            CheckModInfoChanged();
        }

        private void RemoveLogicalDependencyButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to remove logical dependency", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            if (DatabaseEditorMode == EditorMode.Dependency)
            {
                SelectedDependency.LogicalDependencies.Remove((LogicalDependency)ObjectLogicalDependenciesList.SelectedItem);
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDependency.LogicalDependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                SelectedDatabaseObject.LogicalDependencies.Remove((LogicalDependency)ObjectLogicalDependenciesList.SelectedItem);
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDatabaseObject.LogicalDependencies;
            }
            CheckModInfoChanged();
        }

        private void MovePictureButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to move picture in list", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            int index = Utils.ParseInt(MovePictureTB.Text, -1);
            if (index == -1)
            {
                MessageBox.Show("Invalid index in move to position");
                return;
            }
            if (index > SelectedDatabaseObject.PictureList.Count)
            {
                MessageBox.Show("Invalid index in move to position");
                return;
            }
            Media media = (Media)ObjectPicturesList.SelectedItem;
            SelectedDatabaseObject.PictureList.Remove(media);
            //if the index is now out of range, just put it in the bottom
            if (index > SelectedDatabaseObject.PictureList.Count)
                index = SelectedDatabaseObject.PictureList.Count;
            SelectedDatabaseObject.PictureList.Insert(index, media);
            ObjectPicturesList.DataSource = null;
            ObjectPicturesList.Items.Clear();
            ObjectPicturesList.DataSource = SelectedDatabaseObject.PictureList;
            CheckModInfoChanged();
        }

        private void AddPictureButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to add picture", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            if (PicturesTypeCBox.SelectedIndex == -1 || PicturesTypeCBox.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid entry in Picture Type");
                return;
            }
            int index = Utils.ParseInt(AddPictureTB.Text, -1);
            if (index == -1)
            {
                MessageBox.Show("Invalid index in add at positon");
                return;
            }
            if (index > SelectedDatabaseObject.PictureList.Count)
            {
                MessageBox.Show("Invalid index in add at positon");
                return;
            }
            Media media = new Media()
            {
                URL = PictureURLTB.Text
            };
            switch(PicturesTypeCBox.SelectedIndex)
            {
                case 1:
                    media.MediaType = MediaType.Picture;
                    break;
                case 2:
                    media.MediaType = MediaType.Webpage;
                    break;
                case 3:
                    media.MediaType = MediaType.MediaFile;
                    break;
                case 4:
                    media.MediaType = MediaType.HTML;
                    break;
            }
            SelectedDatabaseObject.PictureList.Insert(index, media);
            ObjectPicturesList.DataSource = null;
            ObjectPicturesList.Items.Clear();
            ObjectPicturesList.DataSource = SelectedDatabaseObject.PictureList;
            CheckModInfoChanged();
        }

        private void RemovePictureButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to remove picture", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            Media media = (Media)ObjectPicturesList.SelectedItem;
            SelectedDatabaseObject.PictureList.Remove(media);
            ObjectPicturesList.DataSource = null;
            ObjectPicturesList.Items.Clear();
            ObjectPicturesList.DataSource = SelectedDatabaseObject.PictureList;
            CheckModInfoChanged();
        }

        private void ApplyPictureEditButton_Click(object sender, EventArgs e)
        {
            if (ObjectPicturesList.Items.Count == 0) return;
            Media media = (Media)ObjectPicturesList.SelectedItem;
            if (PicturesTypeCBox.SelectedIndex == -1 || PicturesTypeCBox.SelectedIndex == 0)
            {
                MessageBox.Show("Invalid entry in Picture Type", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show("Confirm you wish to edit picture entry", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            switch (PicturesTypeCBox.SelectedIndex)
            {
                case 1:
                    media.MediaType = MediaType.Picture;
                    break;
                case 2:
                    media.MediaType = MediaType.Webpage;
                    break;
                case 3:
                    media.MediaType = MediaType.MediaFile;
                    break;
                case 4:
                    media.MediaType = MediaType.HTML;
                    break;
            }
            media.URL = PictureURLTB.Text;
            ObjectPicturesList.DataSource = null;
            ObjectPicturesList.Items.Clear();
            ObjectPicturesList.DataSource = SelectedDatabaseObject.PictureList;
            CheckModInfoChanged();
        }

        private void ObjectLogicalDependenciesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.DataSource == null)
                return;
            LogicalDependency ld = (LogicalDependency)lb.SelectedItem;
            foreach (LogicalDependency d in LogicalDependencies)
            {
                if (d.PackageName.Equals(ld.PackageName))
                {
                    CurrentLogicalDependenciesCB.SelectedItem = d;
                    break;
                }
            }
            LogicalDependnecyNegateFlagCB.CheckedChanged -= LogicalDependnecyNegateFlagCB_CheckedChanged;
            LogicalDependnecyNegateFlagCB.Checked = ld.NegateFlag;
            LogicalDependnecyNegateFlagCB.CheckedChanged += LogicalDependnecyNegateFlagCB_CheckedChanged;
        }

        private void LogicalDependnecyNegateFlagCB_CheckedChanged(object sender, EventArgs e)
        {
            if (MessageBox.Show("confirm change logical dependency negate flag status (press 'no' if changing negate flag for adding a new logical dependency)", "confirm", MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            if (DatabaseEditorMode == EditorMode.Dependency)
            {
                LogicalDependency ld = (LogicalDependency)ObjectLogicalDependenciesList.SelectedItem;
                ld.NegateFlag = LogicalDependnecyNegateFlagCB.Checked;
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDependency.LogicalDependencies;
            }
            else if (DatabaseEditorMode == EditorMode.DBO)
            {
                LogicalDependency ld = (LogicalDependency)ObjectLogicalDependenciesList.SelectedItem;
                ld.NegateFlag = LogicalDependnecyNegateFlagCB.Checked;
                ObjectLogicalDependenciesList.DataSource = null;
                ObjectLogicalDependenciesList.Items.Clear();
                ObjectLogicalDependenciesList.DataSource = SelectedDatabaseObject.LogicalDependencies;
            }
            CheckModInfoChanged();
        }

        private void ObjectDependenciesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.DataSource == null)
                return;
            if (lb.SelectedItem is DatabaseLogic)
                return;
            if (lb.SelectedItem is string)
                return;
            if (lb.SelectedItem is Dependency ld)
            {
                foreach (Dependency d in Dependencies)
                {
                    if (d.PackageName.Equals(ld.PackageName))
                    {
                        CurrentDependenciesCB.SelectedItem = d;
                        break;
                    }
                }
            }
        }

        private void ObjectPicturesList_SelectedIndexChanged(object sender, EventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.DataSource == null)
                return;
            Media med = (Media)lb.SelectedItem;
            PicturesTypeCBox.SelectedIndex = (int)med.MediaType;
            PictureURLTB.Text = med.URL;
            MovePictureTB.Text = "" + SelectedDatabaseObject.PictureList.IndexOf(med);
            AddPictureTB.Text = "" + SelectedDatabaseObject.PictureList.IndexOf(med);
        }

        private void AddUserdatasButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to add userdata entry", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            SelectedDatabaseObject.UserFiles.Add(ObjectUserdatasTB.Text);
            ObjectUserdatasList.DataSource = null;
            ObjectUserdatasList.Items.Clear();
            ObjectUserdatasList.DataSource = SelectedDatabaseObject.UserFiles;
            CheckModInfoChanged();
        }

        private void RemoveUserdatasButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to remove userdata entry", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            int index = SelectedDatabaseObject.UserFiles.IndexOf((string)ObjectUserdatasList.SelectedItem);
            SelectedDatabaseObject.UserFiles.RemoveAt(index);
            ObjectUserdatasList.DataSource = null;
            ObjectUserdatasList.Items.Clear();
            ObjectUserdatasList.DataSource = SelectedDatabaseObject.UserFiles;
            CheckModInfoChanged();
        }

        private void ObjectUserdatasList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ObjectUserdatasList.DataSource == null)
                return;
            ObjectUserdatasTB.Text = ObjectUserdatasList.SelectedItem.ToString();
        }

        private void EditUserdatasButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to edit userdata entry", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            int index = SelectedDatabaseObject.UserFiles.IndexOf((string)ObjectUserdatasList.SelectedItem);
            SelectedDatabaseObject.UserFiles[index] = ObjectUserdatasTB.Text;
            ObjectUserdatasList.DataSource = null;
            ObjectUserdatasList.Items.Clear();
            ObjectUserdatasList.DataSource = SelectedDatabaseObject.UserFiles;
            CheckModInfoChanged();
        }

        private void ObjectDependenciesList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.DataSource == null)
                return;
            if (lb.SelectedItem is DatabaseLogic)
                return;
            if (lb.SelectedItem is string)
                return;
            if (MessageBox.Show("Confirm you wish to jump and loose any changes to this object you may have made", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            string savePackageName = "";
            Dependency d = (Dependency)ObjectDependenciesList.SelectedItem;
            savePackageName = d.PackageName;
            DependencyRB.Checked = true;
            foreach (DatabaseTreeNode dtn in DatabaseTreeView.Nodes)
            {
                if (dtn.Dependency.PackageName.Equals(savePackageName))
                {
                    dtn.EnsureVisible();
                    DatabaseTreeView.SelectedNode = dtn;
                    DatabaseTreeView_NodeMouseClick(null, new TreeNodeMouseClickEventArgs(dtn, MouseButtons.Left, 0, 0, 0));
                }
            }
        }

        private void ObjectLogicalDependenciesList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (MessageBox.Show("Confirm you wish to jump and loose any changes to this object you may have made", "confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                return;
            string savePackageName = "";
            LogicalDependency d = (LogicalDependency)ObjectLogicalDependenciesList.SelectedItem;
            savePackageName = d.PackageName;
            LogicalDependencyRB.Checked = true;
            foreach (DatabaseTreeNode dtn in DatabaseTreeView.Nodes)
            {
                if (dtn.LogicalDependency.PackageName.Equals(savePackageName))
                {
                    dtn.EnsureVisible();
                    DatabaseTreeView.SelectedNode = dtn;
                    DatabaseTreeView_NodeMouseClick(null, new TreeNodeMouseClickEventArgs(dtn, MouseButtons.Left, 0, 0, 0));
                }
            }
        }

        private void BuildDatabaseLogic(LogicalDependency d)
        {
            foreach (Dependency depD in Dependencies)
            {
                foreach (LogicalDependency ld in depD.LogicalDependencies)
                {
                    if (ld.PackageName.Equals(d.PackageName))
                    {
                        DatabaseLogic dbl = new DatabaseLogic()
                        {
                            PackageName = depD.PackageName,
                            Enabled = false,
                            Checked = false,
                            NotFlag = ld.NegateFlag
                        };
                        d.DatabasePackageLogic.Add(dbl);
                    }
                }
            }
            foreach (Category c in ParsedCategoryList)
            {
                //will itterate through every catagory once
                foreach (SelectablePackage m in c.Packages)
                {
                    foreach (LogicalDependency ld in m.LogicalDependencies)
                    {
                        if (ld.PackageName.Equals(d.PackageName))
                        {
                            DatabaseLogic dbl = new DatabaseLogic()
                            {
                                PackageName = m.PackageName,
                                Enabled = false,
                                Checked = false,
                                NotFlag = ld.NegateFlag
                            };
                            d.DatabasePackageLogic.Add(dbl);
                        }
                    }
                    if (m.Packages.Count > 0)
                        ProcessConfigsLogical(d, m.Packages);
                }
            }
        }

        private void ProcessConfigsLogical(LogicalDependency d, List<SelectablePackage> configList)
        {
            foreach (SelectablePackage config in configList)
            {
                foreach (LogicalDependency ld in config.LogicalDependencies)
                {
                    if (ld.PackageName.Equals(d.PackageName))
                    {
                        DatabaseLogic dl = new DatabaseLogic()
                        {
                            PackageName = config.PackageName,
                            Enabled = false,
                            Checked = false,
                            NotFlag = ld.NegateFlag
                        };
                        d.DatabasePackageLogic.Add(dl);
                    }
                }
                if (config.Packages.Count > 0)
                    ProcessConfigsLogical(d, config.Packages);
            }
        }

        private List<string> BuildDatabaseLogic(Dependency d)
        {
            List<string> objectsThatUseDependency = new List<string>();
            foreach (Category c in ParsedCategoryList)
            {
                foreach(Dependency catd in c.Dependencies)
                {
                    if(catd.PackageName.Equals(d.PackageName))
                    {
                        objectsThatUseDependency.Add(c.Name);
                    }
                }
                //will itterate through every catagory once
                foreach (SelectablePackage m in c.Packages)
                {
                    foreach (Dependency dep in m.Dependencies)
                    {
                        if (dep.PackageName.Equals(d.PackageName))
                        {
                            objectsThatUseDependency.Add(m.PackageName);
                        }
                    }
                    if (m.Packages.Count > 0)
                        ProcessConfigsLogical(d, m.Packages,objectsThatUseDependency);
                }
            }
            return objectsThatUseDependency;
        }
        
        private void ProcessConfigsLogical(Dependency d, List<SelectablePackage> configList, List<string> objectsThatUseDependency)
        {
            foreach (SelectablePackage config in configList)
            {
                foreach (Dependency dep in config.Dependencies)
                {
                    if (dep.PackageName.Equals(d.PackageName))
                    {
                        objectsThatUseDependency.Add(config.PackageName);
                    }
                }
                if (config.Packages.Count > 0)
                    ProcessConfigsLogical(d, config.Packages, objectsThatUseDependency);
            }
        }

        private void SearchBox_TextUpdate(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            string currentText = cb.Text;
            cb.Items.Clear();
            List<string> filteredItems = null;
            String[] filtered_parts = currentText.Split('*');
            filteredItems = new List<string>(allPackageNames);
            foreach (var f in filtered_parts)
            {
                filteredItems = filteredItems.FindAll(x => x.ToLower().Contains(f.ToLower()));
            }
            cb.Items.AddRange(filteredItems.ToArray());
            if (cb.Items.Count == 0)
            {
                cb.BackColor = System.Drawing.Color.Red;
                cb.Items.Add("none");
                cb.SelectedIndex = 0;
            }
            else
                cb.BackColor = System.Drawing.SystemColors.Window;
            cb.DroppedDown = true;
            Cursor.Current = Cursors.Default;
            cb.IntegralHeight = true;
            // set the position of the cursor
            cb.Text = currentText;
            cb.SelectionStart = currentText.Length;
            cb.SelectionLength = 0;
        }

        private void SearchBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ComboBox cb = (ComboBox)sender;
            if (cb.SelectedIndex == -1)
                return;
            string packageNameSearch = "";
            packageNameSearch = (string)cb.SelectedItem;
            foreach (DatabaseTreeNode node in DatabaseTreeView.Nodes)
            {
                if (node.GlobalDependency != null)
                {
                    if (node.GlobalDependency.PackageName.Equals(packageNameSearch))
                    {
                        DatabaseTreeView_NodeMouseClick(DatabaseTreeView, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 0, 0, 0));
                    }
                }
                else if (node.Dependency != null)
                {
                    if (node.Dependency.PackageName.Equals(packageNameSearch))
                    {
                        DatabaseTreeView_NodeMouseClick(DatabaseTreeView, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 0, 0, 0));
                    }
                }
                else if (node.LogicalDependency != null)
                {
                    if (node.LogicalDependency.PackageName.Equals(packageNameSearch))
                    {
                        DatabaseTreeView_NodeMouseClick(DatabaseTreeView, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 0, 0, 0));
                    }
                }
                else if (node.Category != null)
                {
                    if (node.Category.Name.Equals(packageNameSearch))
                    {
                        DatabaseTreeView_NodeMouseClick(DatabaseTreeView, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 0, 0, 0));
                    }
                }
                else if (node.DatabaseObject != null)
                {
                    if (node.DatabaseObject.PackageName.Equals(packageNameSearch))
                    {
                        DatabaseTreeView_NodeMouseClick(DatabaseTreeView, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 0, 0, 0));
                    }
                }
                if (node.Nodes.Count > 0)
                    SearchBoxConfig(node.Nodes, packageNameSearch);
            }
        }
        private void SearchBoxConfig(TreeNodeCollection nodes, string packageNameSearch)
        {
            foreach (DatabaseTreeNode node in nodes)
            {
                if (node.DatabaseObject != null)
                {
                    if (node.DatabaseObject.PackageName.Equals(packageNameSearch))
                    {
                        DatabaseTreeView_NodeMouseClick(DatabaseTreeView, new TreeNodeMouseClickEventArgs(node, MouseButtons.Left, 0, 0, 0));
                    }
                }
                if (node.Nodes.Count > 0)
                    SearchBoxConfig(node.Nodes, packageNameSearch);
            }
        }

        private void DownloadZipfileButton_Click(object sender, EventArgs e)
        {
            if (ObjectZipFileTB.Text.Equals(""))
            {
                MessageBox.Show("Nothing to download");
                return;
            }
            string DownloadURL = ObjectStartAddressTB.Text + ObjectZipFileTB.Text + ObjectEndAddressTB.Text;
            DatabaseDownloadEditor editor = new DatabaseDownloadEditor(DownloadURL.Replace(@"{onlineFolder}", Settings.TanksOnlineFolderVersion), Path.GetFileName(ObjectZipFileTB.Text));
            editor.Show();
        }

        private bool DuplicatePackageName(List<Dependency> GlobalDependencies, List<Dependency> Dependencies, List<LogicalDependency> LogicalDependencies, List<Category> ParsedCategoryList, string PackageName, int duplicate)
        {
            //int duplicate = 0;
            foreach (Dependency d in GlobalDependencies)
            {
                if (d.PackageName.Equals(PackageName))
                {
                    duplicate++;
                }
            }
            foreach (Dependency d in Dependencies)
            {
                if (d.PackageName.Equals(PackageName))
                {
                    duplicate++;
                }
            }
            foreach (LogicalDependency d in LogicalDependencies)
            {
                if (d.PackageName.Equals(PackageName))
                {
                    duplicate++;
                }
            }
            foreach (Category c in ParsedCategoryList)
            {
                foreach (SelectablePackage m in c.Packages)
                {
                    if (m.PackageName.Equals(PackageName))
                    {
                        duplicate++;
                    }
                    if (m.Packages.Count > 0)
                    {
                        DuplicatePackageNameConfig(m.Packages, PackageName, duplicate);
                    }
                }
            }
            if (duplicate > 1)
                return true;
            else 
                return false;
        }

        private void DuplicatePackageNameConfig(List<SelectablePackage> configList, string PackageName, int duplicate)
        {
            foreach(SelectablePackage c in configList)
            {
                if(c.PackageName.Equals(PackageName))
                {
                    duplicate++;
                }
            }
        }

        private string GetNewPackageName(string oldPackageName)
        {
            int i = 1;
            int temp = 0;
            string packageName = string.Format("{0}_NEW_{1}", oldPackageName, i);
            while (DuplicatePackageName(this.GlobalDependencies, this.Dependencies, this.LogicalDependencies, this.ParsedCategoryList, packageName, temp))
                packageName = string.Format("{0}_NEW_{1}", oldPackageName, i++);
            return packageName;
        }

        private void CallTextBoxURL_DoubleClick(object sender, EventArgs e)
        {
            if (sender is TextBox)
            {
                if ((TextBox)sender != null && !((TextBox)sender).Text.Equals(""))
                    System.Diagnostics.Process.Start(((TextBox)sender).Text);
            }
        }

        private void ObjectPicturesList_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.DataSource == null)
                return;
            Media med = (Media)lb.SelectedItem;
            string[] searchList = new string[] { "http://", "https://" };
            foreach (string r in searchList)
            {
                int pos = med.URL.IndexOf(r, StringComparison.OrdinalIgnoreCase);
                if (pos >= 0)
                {
                    System.Diagnostics.Process.Start(med.URL.Substring(pos));
                }
            }
        }

        private void DatabaseTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Action != TreeViewAction.Unknown)
            {
                DatabaseTreeView_NodeMouseClick(null, new TreeNodeMouseClickEventArgs(e.Node, MouseButtons.Left, 0, 0, 0));
            }
        }
        //checks to make sure that the selected database object (before applying an edit) is of acceptable type)
        //(level 0 can only be single1 and multi, other levels can take all)
        private bool AcceptableType(SelectablePackage SelectedDatabaseObjectt)
        {
            if (SelectedDatabaseObjectt.Level == 0)
            {
                //top level, can only be single1 and multi
                if (!(ObjectTypeComboBox.SelectedIndex == 1 || ObjectTypeComboBox.SelectedIndex == 4))
                {
                    return false;
                }
                return true;
            }
            else if (SelectedDatabaseObjectt.Level > 0)
            {
                if (ObjectTypeComboBox.SelectedIndex == -1 || ObjectTypeComboBox.SelectedIndex == 0)
                {
                    return false;
                }
                return true;
            }
            else
                return false;
        }
    }
}
