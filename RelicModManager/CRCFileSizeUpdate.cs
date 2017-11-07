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
    public partial class CRCFileSizeUpdate : Form
    {
        private WebClient downloader;
        private List<Dependency> globalDependencies;
        private List<Dependency> dependencies;
        private List<LogicalDependency> logicalDependencies;
        private List<Category> parsedCatagoryList;
        StringBuilder globalDepsSB = new StringBuilder();
        StringBuilder dependenciesSB = new StringBuilder();
        StringBuilder logicalDependenciesSB = new StringBuilder();
        StringBuilder modsSB = new StringBuilder();
        StringBuilder configsSB = new StringBuilder();
        StringBuilder filesNotFoundSB = new StringBuilder();
        string serverInfo = "creating the manageInfo.dat file, containing the files: " +
            "\nmanager_version.xml\n" +
            "manager version.txt (will be deprecated)\n" +
            "supported_clients.xml\n" +
            "supported_clients.txt (will be deprecated)\n" +
            "databaseUpdate.txt\n" +
            "releaseNotes.txt\n";
        string database = "creating the database.xml file at every online version folder of WoT, containing the filename, size and MD5Hash of " +
            "the current folder, the script \"CreateMD5List.php\" is a needed subscript of CreateDatabase.php, \"relhax_db.sqlite\" is the needed sqlite database to " +
            "be fast on parsing all files and only working on new or changed files";
        string modInfo = "creating the modInfo.dat file at every online version folder  of WoT, added the onlineFolder name to the root element, " +
            "added the \"selections\" (developerSelections) names, creation date and filenames to the modInfo.xml, adding all parsed develeoperSelection-Config " +
            "files to the modInfo.dat archive";

        public CRCFileSizeUpdate()
        {
            InitializeComponent();
        }

        private void loadDatabaseButton_Click(object sender, EventArgs e)
        {
            if (loadDatabaseDialog.ShowDialog() == DialogResult.Cancel)
                return;
            databaseLocationTextBox.Text = loadDatabaseDialog.FileName;
        }

        private void updateDatabaseOnline_Click(object sender, EventArgs e)
        {
            // check for database
            if (databaseLocationTextBox.Text.Equals("-none-"))
                return;
            // read onlineFolder of the selected local modInfo.xml to get the right online database.xml
            Settings.TanksOnlineFolderVersion = XMLUtils.ReadOnlineFolderFromModInfo(databaseLocationTextBox.Text);
            // read gameVersion of the selected local modInfo.xml
            Settings.TanksVersion = XMLUtils.ReadVersionFromModInfo(databaseLocationTextBox.Text);
            Utils.AppendToLog(String.Format("working with game version: {0}, located at online Folder: {1}", Settings.TanksVersion, Settings.TanksOnlineFolderVersion));
            // download online database.xml
            try
            {
                using (downloader = new WebClient())
                {
                    string address = string.Format("http://wotmods.relhaxmodpack.com/WoT/{0}/database.xml", Settings.TanksOnlineFolderVersion);
                    string fileName = Path.Combine(Application.StartupPath, "RelHaxTemp", MainWindow.onlineDatabaseXmlFile);
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
            modsSB.Clear();
            configsSB.Clear();
            //load database
            globalDependencies = new List<Dependency>();
            parsedCatagoryList = new List<Category>();
            dependencies = new List<Dependency>();
            logicalDependencies = new List<LogicalDependency>();
            XMLUtils.CreateModStructure(databaseLocationTextBox.Text, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            //check for duplicates
            int duplicatesCounter = 0;
            if (Utils.Duplicates(parsedCatagoryList) && Utils.DuplicatesPackageName(parsedCatagoryList, ref duplicatesCounter ))
            {
                MessageBox.Show(string.Format("{0} duplicates found !!!",duplicatesCounter));
                Program.databaseUpdateOnline = false;
                return;
            }
            OnlineScriptOutput.Text = "Updating database...";
            Application.DoEvents();
            filesNotFoundSB.Append("FILES NOT FOUND:\n");
            globalDepsSB.Append("\nGlobal Dependencies updated:\n");
            dependenciesSB.Append("\nDependencies updated:\n");
            logicalDependenciesSB.Append("\nLogical Dependencies updated:\n");
            modsSB.Append("\nMods updated:\n");
            configsSB.Append("\nConfigs updated:\n");
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
                foreach (Mod m in c.mods)
                {
                    if (m.ZipFile.Trim().Equals(""))
                    {
                        m.CRC = "";
                    }
                    else
                    {
                        m.size = this.getFileSize(m.ZipFile);
                        hash = XMLUtils.GetMd5Hash(m.ZipFile);
                        if (!m.CRC.Equals(hash))
                        {
                            m.CRC = hash;

                            if (!hash.Equals("f"))
                            {
                                modsSB.Append(m.ZipFile + "\n");
                            }
                        }
                        if (hash.Equals("f"))
                        {
                            filesNotFoundSB.Append(m.ZipFile + "\n");
                        }
                    }
                    if (m.configs.Count > 0)
                    {
                        this.processConfigsCRCUpdate(m.configs);
                    }
                }
            }
            //update the CRC value
            //update the file size
            //save config file
            XMLUtils.SaveDatabase(databaseLocationTextBox.Text, Settings.TanksVersion, Settings.TanksOnlineFolderVersion, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            MessageBox.Show(filesNotFoundSB.ToString() + globalDepsSB.ToString() + dependenciesSB.ToString() + logicalDependenciesSB.ToString() + modsSB.ToString() + configsSB.ToString());
            Program.databaseUpdateOnline = false;
        }

        private void processConfigsCRCUpdate(List<Config> cfgList)
        {
            string hash;
            foreach (Config cat in cfgList)
            {
                if (cat.ZipFile.Trim().Equals(""))
                {
                    cat.CRC = "";
                }
                else
                {
                    hash = XMLUtils.GetMd5Hash(cat.ZipFile);
                    cat.size = this.getFileSize(cat.ZipFile);
                    if (cat.size != 0)
                    {
                        if (!cat.CRC.Equals(hash))
                        {
                            cat.CRC = hash;
                            if (!hash.Equals("f"))
                            {
                                configsSB.Append(cat.ZipFile + "\n");
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
                if (cat.configs.Count > 0)
                {
                    this.processConfigsCRCUpdate(cat.configs);
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
                    XDocument doc = XDocument.Load(MainWindow.onlineDatabaseXmlFile);
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
            //font scaling
            this.AutoScaleMode = Settings.AppScalingMode;
            this.Font = Settings.AppFont;
            if (Settings.AppScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new System.Drawing.SizeF(Settings.ScaleSize, Settings.ScaleSize));
            }
            loadDatabaseDialog.InitialDirectory = Application.StartupPath;
        }

        private void CRCFileSizeUpdate_FormClosing(object sender, FormClosingEventArgs e)
        {
            Utils.AppendToLog("|------------------------------------------------------------------------------------------------|");
        }

        private void updateDatabaseOffline_Click(object sender, EventArgs e)
        {
            //check for database
            if (databaseLocationTextBox.Text.Equals("-none-"))
                return;
            //show file dialog
            if (addZipsDialog.ShowDialog() == DialogResult.Cancel)
                return;
            globalDepsSB.Clear();
            dependenciesSB.Clear();
            modsSB.Clear();
            configsSB.Clear();
            //load database
            globalDependencies = new List<Dependency>();
            parsedCatagoryList = new List<Category>();
            dependencies = new List<Dependency>();
            logicalDependencies = new List<LogicalDependency>();
            Settings.TanksVersion = XMLUtils.ReadVersionFromModInfo(databaseLocationTextBox.Text);
            Settings.TanksOnlineFolderVersion = XMLUtils.ReadOnlineFolderFromModInfo(databaseLocationTextBox.Text);
            XMLUtils.CreateModStructure(databaseLocationTextBox.Text, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            int duplicatesCounter = 0;
            //check for duplicates
            if (Utils.Duplicates(parsedCatagoryList) && Utils.DuplicatesPackageName(parsedCatagoryList, ref duplicatesCounter))
            {
                MessageBox.Show(string.Format("{0} duplicates found !!!", duplicatesCounter));
                return;
            }
            OnlineScriptOutput.Text = "Updating database...";
            Application.DoEvents();
            globalDepsSB.Append("Global Dependencies updated:\n");
            dependenciesSB.Append("Dependencies updated:\n");
            modsSB.Append("Mods updated:\n");
            configsSB.Append("Configs updated:\n");
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
                foreach (Mod m in c.mods)
                {
                    int index = this.getZipIndex(m.ZipFile);
                    if (index != -1)
                    {
                        m.size = this.getFileSize(addZipsDialog.FileNames[index]);
                        if (m.CRC == null || m.CRC.Equals("") || m.CRC.Equals("f"))
                        {
                            m.CRC = Utils.CreateMd5Hash(addZipsDialog.FileNames[index]);

                            modsSB.Append(m.ZipFile + "\n");
                        }
                    }
                    if (m.configs.Count > 0)
                    {
                        this.processConfigsCRCUpdate_old(m.configs);
                    }
                }
            }
            //update the CRC value
            //update the file size
            //save config file
            XMLUtils.SaveDatabase(databaseLocationTextBox.Text, Settings.TanksVersion, Settings.TanksOnlineFolderVersion, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            MessageBox.Show(globalDepsSB.ToString() + dependenciesSB.ToString() + modsSB.ToString() + configsSB.ToString());
        }

        private void processConfigsCRCUpdate_old(List<Config> cfgList)
        {
            foreach (Config cat in cfgList)
            {
                int cindex = this.getZipIndex(cat.ZipFile);
                if (cindex != -1)
                {
                    cat.size = this.getFileSize(addZipsDialog.FileNames[cindex]);
                    if (cat.CRC == null || cat.CRC.Equals("") || cat.CRC.Equals("f"))
                    {
                        cat.CRC = Utils.CreateMd5Hash(addZipsDialog.FileNames[cindex]);

                        configsSB.Append(cat.ZipFile + "\n");
                    }
                }
                if (cat.configs.Count > 0)
                {
                    this.processConfigsCRCUpdate_old(cat.configs);
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
            OnlineScriptOutput.Text = "Running script CreateDatabase.php...";
            Application.DoEvents();
            using (WebClient client = new WebClient())
            {
                OnlineScriptOutput.Text = client.DownloadString("http://wotmods.relhaxmodpack.com/scripts/CreateDatabase.php").Replace("<br />", "\n");
            }
            Application.DoEvents();
        }

        private void RunCreateModInfoPHP_Click(object sender, EventArgs e)
        {
            OnlineScriptOutput.Text = "Running script CreateModInfo.php...";
            Application.DoEvents();
            using (WebClient client = new WebClient())
            {
                OnlineScriptOutput.Text = client.DownloadString("http://wotmods.relhaxmodpack.com/scripts/CreateModInfo.php").Replace("<br />", "\n");
            }
            Application.DoEvents();
        }

        private void RunCreateServerInfoPHP_Click(object sender, EventArgs e)
        {
            OnlineScriptOutput.Text = "Running script CreateServerInfo.php...";
            Application.DoEvents();
            using (WebClient client = new WebClient())
            {
                OnlineScriptOutput.Text = client.DownloadString("http://wotmods.relhaxmodpack.com/scripts/CreateServerInfo.php").Replace("<br />", "\n");
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
    }
}
