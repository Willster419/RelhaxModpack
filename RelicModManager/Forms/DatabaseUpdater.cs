using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace RelhaxModpack
{
    public partial class DatabaseUpdater : RelhaxForum
    {
        private enum AuthLevel
        {
            None=0,
            View=1,
            UpdateDatabase=2,
            Admin=3
        }
        private AuthLevel CurrentAuthLevel = AuthLevel.None;
        private WebClient downloader;
        private List<Dependency> globalDependencies;
        private List<Dependency> dependencies;
        private List<LogicalDependency> logicalDependencies;
        private List<Category> parsedCatagoryList;
        StringBuilder globalDepsSB = new StringBuilder();
        StringBuilder dependenciesSB = new StringBuilder();
        StringBuilder logicalDependenciesSB = new StringBuilder();
        StringBuilder packagesSB = new StringBuilder();
        StringBuilder filesNotFoundSB = new StringBuilder();
        private string level1Password = "RelhaxReadOnly2018";
        private const string L2keyFileName = "L2Key.txt";
        private const string L3keyFileName = "L3key.txt";
        private const string L2KeyAddress = "aHR0cDovL3dvdG1vZHMucmVsaGF4bW9kcGFjay5jb20vUmVsaGF4TW9kcGFjay9SZXNvdXJjZXMvZXh0ZXJuYWwvTDJLZXkudHh0";
        private const string L3KeyAddress = "aHR0cDovL3dvdG1vZHMucmVsaGF4bW9kcGFjay5jb20vUmVsaGF4TW9kcGFjay9SZXNvdXJjZXMvZXh0ZXJuYWwvTDNrZXkudHh0";
        
        string serverInfo = "creating the manageInfo.dat file, containing the files: " +
            "\nmanager_version.xml\n" +
            "supported_clients.xml\n" +
            "databaseUpdate.txt\n" +
            "releaseNotes.txt\n" +
            "releaseNotes_beta.txt\n" +
            "default_checked.xml";
        string database = "creating the database.xml file at every online version folder of WoT, containing the filename, size and MD5Hash of " +
            "the current folder, the script \"CreateMD5List.php\" is a needed subscript of CreateDatabase.php, \"relhax_db.sqlite\" is the needed sqlite database to " +
            "be fast on parsing all files and only working on new or changed files";
        string modInfo = "creating the modInfo.dat file at every online version folder  of WoT, added the onlineFolder name to the root element, " +
            "added the \"selections\" (developerSelections) names, creation date and filenames to the modInfo.xml, adding all parsed develeoperSelection-Config " +
            "files to the modInfo.dat archive";

        public DatabaseUpdater()
        {
            InitializeComponent();
        }

        private void OnAuthLevelChange(AuthLevel al)
        {
            CurrentAuthLevel = al;
            //disable everything first, then enable via falling case statements
            foreach (Control c in UpdateDatabaseTab.Controls)
                c.Enabled = false;
            foreach (Control c in UpdateApplicationTab.Controls)
                c.Enabled = false;
            foreach (Control c in CleanOnlineFolders.Controls)
                c.Enabled = false;
            if((int)CurrentAuthLevel > 1)
            {
                foreach (Control c in UpdateDatabaseTab.Controls)
                    c.Enabled = true;
                UpdateDatabaseStep3Advanced.Enabled = false;
            }
            if((int)CurrentAuthLevel > 2)
            {
                UpdateDatabaseStep3Advanced.Enabled = true;
                foreach (Control c in UpdateApplicationTab.Controls)
                    c.Enabled = true;
                foreach (Control c in CleanOnlineFolders.Controls)
                    c.Enabled = true;
            }
            AuthStatusLabel.Text = CurrentAuthLevel.ToString();
        }

        private void loadDatabaseButton_Click(object sender, EventArgs e)
        {
            if (loadDatabaseDialog.ShowDialog() == DialogResult.Cancel)
                return;
            DatabaseLocationTextBox.Text = loadDatabaseDialog.FileName;
        }

        private void updateDatabaseOnline_Click(object sender, EventArgs e)
        {
            // check for database
            if (DatabaseLocationTextBox.Text.Equals("-none-"))
                return;
            // read onlineFolder of the selected local modInfo.xml to get the right online database.xml
            Settings.TanksOnlineFolderVersion = XMLUtils.GetXMLElementAttributeFromFile(DatabaseLocationTextBox.Text, "//modInfoAlpha.xml/@onlineFolder");
            // read gameVersion of the selected local modInfo.xml
            Settings.TanksVersion = XMLUtils.GetXMLElementAttributeFromFile(DatabaseLocationTextBox.Text, "//modInfoAlpha.xml/@version");
            Logging.Manager(String.Format("working with game version: {0}, located at online Folder: {1}", Settings.TanksVersion, Settings.TanksOnlineFolderVersion));
            // download online database.xml
            try
            {
                using (downloader = new WebClient())
                {
                    string address = string.Format("http://wotmods.relhaxmodpack.com/WoT/{0}/database.xml", Settings.TanksOnlineFolderVersion);
                    string fileName = Path.Combine(Application.StartupPath, "RelHaxTemp", Settings.OnlineDatabaseXmlFile);
                    downloader.DownloadFile(address, fileName);
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("loadZipFilesButton_Click", string.Format("http://wotmods.relhaxmodpack.com/WoT/{0}/database.xml", Settings.TanksOnlineFolderVersion), ex);
                MessageBox.Show("FAILED to download online file database");
                Application.Exit();
            }
            // set this flag, so getMd5Hash and getFileSize should parse downloaded online database.xml
            Program.databaseUpdateOnline = true;
            filesNotFoundSB.Clear();
            globalDepsSB.Clear();
            dependenciesSB.Clear();
            logicalDependenciesSB.Clear();
            packagesSB.Clear();
            //load database
            globalDependencies = new List<Dependency>();
            parsedCatagoryList = new List<Category>();
            dependencies = new List<Dependency>();
            logicalDependencies = new List<LogicalDependency>();
            XMLUtils.CreateModStructure(DatabaseLocationTextBox.Text, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            //check for duplicates
            int duplicatesCounter = 0;
            if (Utils.Duplicates(parsedCatagoryList) && Utils.DuplicatesPackageName(parsedCatagoryList, ref duplicatesCounter ))
            {
                MessageBox.Show(string.Format("{0} duplicates found !!!",duplicatesCounter));
                Program.databaseUpdateOnline = false;
                return;
            }
            ScriptLogOutput.Text = "Updating database...";
            Application.DoEvents();
            filesNotFoundSB.Append("FILES NOT FOUND:\n");
            globalDepsSB.Append("\nGlobal Dependencies updated:\n");
            dependenciesSB.Append("\nDependencies updated:\n");
            logicalDependenciesSB.Append("\nLogical Dependencies updated:\n");
            packagesSB.Append("\nPackages updated:\n");
            string hash;
            //foreach zip file name
            foreach (Dependency d in globalDependencies)
            {
                if (d.ZipFile.Trim().Equals(""))
                {
                    d.CRC = "";
                }
                else
                {
                    hash = XMLUtils.GetMd5Hash(d.ZipFile);
                    if (!d.CRC.Equals(hash))
                    {
                        d.CRC = hash;
                        if (!hash.Equals("f"))
                        {
                            globalDepsSB.Append(d.ZipFile + "\n");
                        }
                    }
                    if (hash.Equals("f"))
                    {
                        filesNotFoundSB.Append(d.ZipFile + "\n");
                    }
                }
            }
            foreach (Dependency d in dependencies)
            {
                if (d.ZipFile.Trim().Equals(""))
                {
                    d.CRC = "";
                }
                else
                {
                    hash = XMLUtils.GetMd5Hash(d.ZipFile);
                    if (!d.CRC.Equals(hash))
                    {
                        d.CRC = hash;
                        if (!hash.Equals("f"))
                        {
                            dependenciesSB.Append(d.ZipFile + "\n");
                        }
                    }
                    if (hash.Equals("f"))
                    {
                        filesNotFoundSB.Append(d.ZipFile + "\n");
                    }
                }
            }
            foreach (LogicalDependency d in logicalDependencies)
            {
                if (d.ZipFile.Trim().Equals(""))
                {
                    d.CRC = "";
                }
                else
                {
                    hash = XMLUtils.GetMd5Hash(d.ZipFile);
                    if (!d.CRC.Equals(hash))
                    {
                        d.CRC = hash;
                        if (!hash.Equals("f"))
                        {
                            logicalDependenciesSB.Append(d.ZipFile + "\n");
                        }
                    }
                    if (hash.Equals("f"))
                    {
                        filesNotFoundSB.Append(d.ZipFile + "\n");
                    }
                }
            }
            foreach (Category c in parsedCatagoryList)
            {
                foreach (SelectablePackage m in c.Packages)
                {
                    if (m.ZipFile.Trim().Equals(""))
                    {
                        m.CRC = "";
                    }
                    else
                    {
                        m.Size = this.getFileSize(m.ZipFile);
                        hash = XMLUtils.GetMd5Hash(m.ZipFile);
                        if (!m.CRC.Equals(hash))
                        {
                            m.CRC = hash;

                            if (!hash.Equals("f"))
                            {
                                packagesSB.Append(m.ZipFile + "\n");
                            }
                        }
                        if (hash.Equals("f"))
                        {
                            filesNotFoundSB.Append(m.ZipFile + "\n");
                        }
                    }
                    if (m.Packages.Count > 0)
                    {
                        this.processConfigsCRCUpdate(m.Packages);
                    }
                }
            }
            //update the CRC value
            //update the file size
            //save config file
            XMLUtils.SaveDatabase(DatabaseLocationTextBox.Text, Settings.TanksVersion, Settings.TanksOnlineFolderVersion, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            //MessageBox.Show(filesNotFoundSB.ToString() + globalDepsSB.ToString() + dependenciesSB.ToString() + logicalDependenciesSB.ToString() + modsSB.ToString() + configsSB.ToString());
            ScriptLogOutput.Text = filesNotFoundSB.ToString() + globalDepsSB.ToString() + dependenciesSB.ToString() + logicalDependenciesSB.ToString() + packagesSB.ToString();
            Program.databaseUpdateOnline = false;
        }

        private void processConfigsCRCUpdate(List<SelectablePackage> cfgList)
        {
            string hash;
            foreach (SelectablePackage cat in cfgList)
            {
                if (cat.ZipFile.Trim().Equals(""))
                {
                    cat.CRC = "";
                }
                else
                {
                    hash = XMLUtils.GetMd5Hash(cat.ZipFile);
                    cat.Size = this.getFileSize(cat.ZipFile);
                    if (cat.Size != 0)
                    {
                        if (!cat.CRC.Equals(hash))
                        {
                            cat.CRC = hash;
                            if (!hash.Equals("f"))
                            {
                                packagesSB.Append(cat.ZipFile + "\n");
                            }
                        }
                    }
                    else
                    {
                        cat.CRC = "f";
                    }
                    if (hash.Equals("f") | cat.CRC.Equals("f"))
                    {
                        filesNotFoundSB.Append(cat.ZipFile + "\n");
                    }

                }
                if (cat.Packages.Count > 0)
                {
                    this.processConfigsCRCUpdate(cat.Packages);
                }
            }
        }
        
        private Int64 getFileSize(string file)
        {
            Int64 fileSizeBytes = 0;
            if (Program.databaseUpdateOnline)
            {
                try
                {
                    XDocument doc = XDocument.Load(Settings.OnlineDatabaseXmlFile);
                    try
                    {
                        XElement element = doc.Descendants("file")
                           .Where(arg => arg.Attribute("name").Value == file)
                           .Single();
                        Int64.TryParse(element.Attribute("size").Value, out fileSizeBytes);
                    }
                    catch (InvalidOperationException)
                    {
                        // catch the Exception if no entry is found
                    }
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("getFileSize", "read from onlineDatabaseXml: " + file, ex);
                }
            }
            else
            {
                try
                {
                    FileInfo fi = new FileInfo(file);
                    fileSizeBytes = fi.Length;
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("getFileSize", "FileInfo from local file: " + file, ex);
                }
            }
            try
            {
                return fileSizeBytes;
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("getFileSize", "building format", ex);
            }
            return 0;
        }

        private void CRCFileSizeUpdate_Load(object sender, EventArgs e)
        {
            loadDatabaseDialog.InitialDirectory = Application.StartupPath;
            //DEBUG ONLY
            OnAuthLevelChange(AuthLevel.Admin);
        }

        private void CRCFileSizeUpdate_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logging.Manager("|------------------------------------------------------------------------------------------------|");
        }

        private void updateDatabaseOffline_Click(object sender, EventArgs e)
        {
            //check for database
            if (DatabaseLocationTextBox.Text.Equals("-none-"))
                return;
            //show file dialog
            if (addZipsDialog.ShowDialog() == DialogResult.Cancel)
                return;
            globalDepsSB.Clear();
            dependenciesSB.Clear();
            packagesSB.Clear();
            //load database
            globalDependencies = new List<Dependency>();
            parsedCatagoryList = new List<Category>();
            dependencies = new List<Dependency>();
            logicalDependencies = new List<LogicalDependency>();
            Settings.TanksVersion = XMLUtils.GetXMLElementAttributeFromFile(DatabaseLocationTextBox.Text, "//modInfoAlpha.xml/@version");
            Settings.TanksOnlineFolderVersion = XMLUtils.GetXMLElementAttributeFromFile(DatabaseLocationTextBox.Text, "//modInfoAlpha.xml/@onlineFolder");
            XMLUtils.CreateModStructure(DatabaseLocationTextBox.Text, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            int duplicatesCounter = 0;
            //check for duplicates
            if (Utils.Duplicates(parsedCatagoryList) && Utils.DuplicatesPackageName(parsedCatagoryList, ref duplicatesCounter))
            {
                MessageBox.Show(string.Format("{0} duplicates found !!!", duplicatesCounter));
                return;
            }
            ScriptLogOutput.Text = "Updating database...";
            Application.DoEvents();
            globalDepsSB.Append("Global Dependencies updated:\n");
            dependenciesSB.Append("Dependencies updated:\n");
            packagesSB.Append("Packages updated:\n");
            //foreach zip file name
            foreach (Dependency d in globalDependencies)
            {
                int index = this.getZipIndex(d.ZipFile);
                if (index == -1)
                {
                    continue;
                }
                if (d.CRC == null || d.CRC.Equals("") || d.CRC.Equals("f"))
                {
                    d.CRC = Utils.CreateMd5Hash(addZipsDialog.FileNames[index]);
                    globalDepsSB.Append(d.ZipFile + "\n");
                }
            }
            foreach (Dependency d in dependencies)
            {
                int index = this.getZipIndex(d.ZipFile);
                if (index == -1)
                {
                    continue;
                }
                if (d.CRC == null || d.CRC.Equals("") || d.CRC.Equals("f"))
                {
                    d.CRC = Utils.CreateMd5Hash(addZipsDialog.FileNames[index]);
                    dependenciesSB.Append(d.ZipFile + "\n");
                }
            }
            foreach (Category c in parsedCatagoryList)
            {
                foreach (SelectablePackage m in c.Packages)
                {
                    int index = this.getZipIndex(m.ZipFile);
                    if (index != -1)
                    {
                        m.Size = this.getFileSize(addZipsDialog.FileNames[index]);
                        if (m.CRC == null || m.CRC.Equals("") || m.CRC.Equals("f"))
                        {
                            m.CRC = Utils.CreateMd5Hash(addZipsDialog.FileNames[index]);

                            packagesSB.Append(m.ZipFile + "\n");
                        }
                    }
                    if (m.Packages.Count > 0)
                    {
                        this.processConfigsCRCUpdate_old(m.Packages);
                    }
                }
            }
            //update the CRC value
            //update the file size
            //save config file
            XMLUtils.SaveDatabase(DatabaseLocationTextBox.Text, Settings.TanksVersion, Settings.TanksOnlineFolderVersion, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            //MessageBox.Show(globalDepsSB.ToString() + dependenciesSB.ToString() + modsSB.ToString() + configsSB.ToString());
            ScriptLogOutput.Text = globalDepsSB.ToString() + dependenciesSB.ToString() + packagesSB.ToString();
        }

        private void processConfigsCRCUpdate_old(List<SelectablePackage> cfgList)
        {
            foreach (SelectablePackage cat in cfgList)
            {
                int cindex = this.getZipIndex(cat.ZipFile);
                if (cindex != -1)
                {
                    cat.Size = this.getFileSize(addZipsDialog.FileNames[cindex]);
                    if (cat.CRC == null || cat.CRC.Equals("") || cat.CRC.Equals("f"))
                    {
                        cat.CRC = Utils.CreateMd5Hash(addZipsDialog.FileNames[cindex]);

                        packagesSB.Append(cat.ZipFile + "\n");
                    }
                }
                if (cat.Packages.Count > 0)
                {
                    this.processConfigsCRCUpdate_old(cat.Packages);
                }
            }
        }

        private float getFileSize_old(string file)
        {
            FileInfo fi = new FileInfo(file);
            float fileSizeBytes = fi.Length;
            float fileSizeKBytes = fileSizeBytes / 1024;
            float fileSizeMBytes = fileSizeKBytes / 1024;
            fileSizeMBytes = (float)Math.Round(fileSizeMBytes, 1);
            if (fileSizeMBytes == 0.0)
                fileSizeMBytes = 0.1f;
            return fileSizeMBytes;
        }

        private int getZipIndex(string zipFile)
        {
            for (int i = 0; i < addZipsDialog.FileNames.Count(); i++)
            {
                string fileName = Path.GetFileName(addZipsDialog.FileNames[i]);
                if (fileName.Equals(zipFile))
                    return i;
            }
            return -1;
        }

        private void RunOnlineScriptButton_Click(object sender, EventArgs e)
        {
            ScriptLogOutput.Text = "Running script CreateDatabase.php...";
            Application.DoEvents();
            using (WebClient client = new WebClient())
            {
                ScriptLogOutput.Text = client.DownloadString("http://wotmods.relhaxmodpack.com/scripts/CreateDatabase.php").Replace("<br />", "\n");
            }
            Application.DoEvents();
        }

        private void RunCreateModInfoPHP_Click(object sender, EventArgs e)
        {
            ScriptLogOutput.Text = "Running script CreateModInfo.php...";
            Application.DoEvents();
            using (WebClient client = new WebClient())
            {
                ScriptLogOutput.Text = client.DownloadString("http://wotmods.relhaxmodpack.com/scripts/CreateModInfo.php").Replace("<br />", "\n");
            }
            Application.DoEvents();
        }

        private void RunCreateServerInfoPHP_Click(object sender, EventArgs e)
        {
            ScriptLogOutput.Text = "Running script CreateServerInfo.php...";
            Application.DoEvents();
            using (WebClient client = new WebClient())
            {
                ScriptLogOutput.Text = client.DownloadString("http://wotmods.relhaxmodpack.com/scripts/CreateManagerInfo.php").Replace("<br />", "\n");
            }
            Application.DoEvents();
        }

        private void RunCreateDatabasePHP_MouseEnter(object sender, EventArgs e)
        {
            InfoTB.Text = database;
        }

        private void RunCreateModInfoPHP_MouseEnter(object sender, EventArgs e)
        {
            InfoTB.Text = modInfo;
        }

        private void RunCreateServerInfoPHP_MouseEnter(object sender, EventArgs e)
        {
            InfoTB.Text = serverInfo;
        }

        private void Generic_MouseLeave(object sender, EventArgs e)
        {
            InfoTB.Text = "";
        }

        private void RunCreateOutdatedFilesList_Click(object sender, EventArgs e)
        {
            ScriptLogOutput.Text = "Running script CreateOutDatesFilesList.php...";
            Application.DoEvents();
            using (WebClient client = new WebClient())
            {
                ScriptLogOutput.Text = client.DownloadString("http://wotmods.relhaxmodpack.com/scripts/CreateOutdatedFileList.php").Replace("<br />", "\n");
            }
            Application.DoEvents();
        }

        private void RunCreateOutdatedFileList_MouseEnter(object sender, EventArgs e)
        {
            //todo
            InfoTB.Text = "Creates an xml list of files not linked in modInfo.xml, but still in the folder.";
        }

        private void DatabaseUpdateTabControl_Selected(object sender, TabControlEventArgs e)
        {
            if (CurrentAuthLevel == AuthLevel.None)
                DatabaseUpdateTabControl.SelectedTab = AuthStatus;
        }

        private void RequestL1AuthButton_Click(object sender, EventArgs e)
        {
            ScriptLogOutput.Text = "Verifying...";
            if (L1AuthPasswordAttempt.Text.Equals(level1Password))
            {
                MessageBox.Show("Success!");
                OnAuthLevelChange(AuthLevel.View);
            }
            else
            {
                MessageBox.Show("Denied!");
            }
            ScriptLogOutput.Text = "";
        }

        private void RequestL2AuthButton_Click(object sender, EventArgs e)
        {
            ScriptLogOutput.Text = "Verifying...";
            string downloadedKey = "";
            using (downloader = new WebClient())
            {
                downloadedKey = Utils.Base64Decode(downloader.DownloadString(Utils.Base64Decode(L2KeyAddress)));
            }
            if(downloadedKey.Equals(L2PasswordAttempt.Text))
            {
                MessageBox.Show("Success!");
                OnAuthLevelChange(AuthLevel.UpdateDatabase);
            }
            else
            {
                MessageBox.Show("Denied!");
            }
            ScriptLogOutput.Text = "";
        }

        private void RequestL3AuthButton_Click(object sender, EventArgs e)
        {
            ScriptLogOutput.Text = "Verifying...";
            string downloadedKey = "";
            using (downloader = new WebClient())
            {
                downloadedKey = Utils.Base64Decode(downloader.DownloadString(Utils.Base64Decode(L3KeyAddress)));
            }
            if (downloadedKey.Equals(L3PasswordAttempt.Text))
            {
                MessageBox.Show("Success!");
                OnAuthLevelChange(AuthLevel.Admin);
            }
            else
            {
                MessageBox.Show("Denied!");
            }
            ScriptLogOutput.Text = "";
        }

        private void GenerateL2Password_Click(object sender, EventArgs e)
        {
            if (File.Exists(L2keyFileName))
                File.Delete(L2keyFileName);
            File.WriteAllText(L2keyFileName, Utils.Base64Encode(L2TextPassword.Text));
            L2GenerateOutput.Text = "Password key file saved to" + L2keyFileName;
        }

        private void GenerateL3Password_Click(object sender, EventArgs e)
        {
            if (File.Exists(L3keyFileName))
                File.Delete(L3keyFileName);
            File.WriteAllText(L3keyFileName, Utils.Base64Encode(L3TextPassword.Text));
            L3GenerateOutput.Text = "Password key file saved to " + L3keyFileName;
        }

        private void ReadbackL2Password_Click(object sender, EventArgs e)
        {
            if(!File.Exists(L2keyFileName))
            {
                L2GenerateOutput.Text = L2keyFileName + " does not exist!";
            }
            else
            {
                L2GenerateOutput.Text = "L2 Key read as " + Utils.Base64Decode(File.ReadAllText(L2keyFileName));
            }
        }

        private void ReadbackL3Password_Click(object sender, EventArgs e)
        {
            if (!File.Exists(L3keyFileName))
            {
                L3GenerateOutput.Text = L3keyFileName + " does not exist!";
            }
            else
            {
                L3GenerateOutput.Text = "L3 Key read as " + Utils.Base64Decode(File.ReadAllText(L3keyFileName));
            }
        }
    }
}
