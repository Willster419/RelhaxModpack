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
        private List<LogicalDependnecy> logicalDependencies;
        private List<Category> parsedCatagoryList;
        StringBuilder globalDepsSB = new StringBuilder();
        StringBuilder dependenciesSB = new StringBuilder();
        StringBuilder logicalDependenciesSB = new StringBuilder();
        StringBuilder modsSB = new StringBuilder();
        StringBuilder configsSB = new StringBuilder();
        StringBuilder filesNotFoundSB = new StringBuilder();

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
            string onlineFolderVersion = Utils.readOnlineFolderFromModInfo(databaseLocationTextBox.Text);
            // read gameVersion of the selected local modInfo.xml
            string gameVersion = Utils.readVersionFromModInfo(databaseLocationTextBox.Text);
            Utils.appendToLog("working with game version: " + onlineFolderVersion);
            // download online database.xml
            try
            {
                using (downloader = new WebClient())
                {
                    string address = "http://wotmods.relhaxmodpack.com/WoT/" + onlineFolderVersion + "/database.xml";
                    string fileName = Path.Combine(Application.StartupPath, "RelHaxTemp", MainWindow.onlineDatabaseXmlFile);
                    downloader.DownloadFile(address, fileName);
                }
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("loadZipFilesButton_Click", "http://wotmods.relhaxmodpack.com/WoT/" + onlineFolderVersion + "/database.xml", ex);
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
            logicalDependencies = new List<LogicalDependnecy>();
            Utils.createModStructure(databaseLocationTextBox.Text, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            //check for duplicates
            int duplicatesCounter = 0;
            if (Utils.duplicates(parsedCatagoryList) && Utils.duplicatesPackageName(parsedCatagoryList, ref duplicatesCounter ))
            {
                MessageBox.Show(string.Format("{0} duplicates found !!!",duplicatesCounter));
                Program.databaseUpdateOnline = false;
                return;
            }
            updatingLabel.Text = "Updating database...";
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
                hash = Utils.getMd5Hash(d.dependencyZipFile);
                if (!d.dependencyZipCRC.Equals(hash))
                {
                    d.dependencyZipCRC = hash;
                    if (!hash.Equals("f"))
                    {
                        globalDepsSB.Append(d.dependencyZipFile + "\n");
                    }
                }
                if (hash.Equals("f"))
                {
                    filesNotFoundSB.Append(d.dependencyZipFile + "\n");
                }
            }
            foreach (Dependency d in dependencies)
            {
                hash = Utils.getMd5Hash(d.dependencyZipFile);
                if (!d.dependencyZipCRC.Equals(hash))
                {
                    d.dependencyZipCRC = hash;
                    if (!hash.Equals("f"))
                    {
                        dependenciesSB.Append(d.dependencyZipFile + "\n");
                    }
                }
                if (hash.Equals("f"))
                {
                    filesNotFoundSB.Append(d.dependencyZipFile + "\n");
                }
            }
            foreach (LogicalDependnecy d in logicalDependencies)
            {
                hash = Utils.getMd5Hash(d.dependencyZipFile);
                if (!d.dependencyZipCRC.Equals(hash))
                {
                    d.dependencyZipCRC = hash;
                    if (!hash.Equals("f"))
                    {
                        logicalDependenciesSB.Append(d.dependencyZipFile + "\n");
                    }
                }
                if (hash.Equals("f"))
                {
                    filesNotFoundSB.Append(d.dependencyZipFile + "\n");
                }
            }
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (!m.zipFile.Equals(""))
                    {
                        m.size = this.getFileSize(m.zipFile);
                        hash = Utils.getMd5Hash(m.zipFile);
                        if (!m.crc.Equals(hash))
                        {
                            m.crc = hash;

                            if (!hash.Equals("f"))
                            {
                                modsSB.Append(m.zipFile + "\n");
                            }
                        }
                        if (hash.Equals("f"))
                        {
                            filesNotFoundSB.Append(m.zipFile + "\n");
                        }
                    }
                    if (m.configs.Count > 0)
                    {
                        this.processConfigsCRCUpdate(m.configs);
                    }
                }
            }
            //update the crc value
            //update the file size
            //save config file
            // string newModInfo = databaseLocationTextBox.Text;
            //this.saveDatabase(databaseLocationTextBox.Text, gameVersion);
            Utils.SaveDatabase(databaseLocationTextBox.Text, gameVersion, onlineFolderVersion, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            MessageBox.Show(filesNotFoundSB.ToString() + globalDepsSB.ToString() + dependenciesSB.ToString() + logicalDependenciesSB.ToString() + modsSB.ToString() + configsSB.ToString());
            updatingLabel.Text = "Idle";
            Program.databaseUpdateOnline = false;
        }

        private void processConfigsCRCUpdate(List<Config> cfgList)
        {
            string hash;
            foreach (Config cat in cfgList)
            {
                if (!cat.zipFile.Equals(""))
                {
                    hash = Utils.getMd5Hash(cat.zipFile);
                    cat.size = this.getFileSize(cat.zipFile);
                    if (cat.size != 0)
                    {
                        if (!cat.crc.Equals(hash))
                        {
                            cat.crc = hash;
                            if (!hash.Equals("f"))
                            {
                                configsSB.Append(cat.zipFile + "\n");
                            }
                        }
                    }
                    else
                    {
                        cat.crc = "f";
                    }
                    if (hash.Equals("f") | cat.crc.Equals("f"))
                    {
                        filesNotFoundSB.Append(cat.zipFile + "\n");
                    }

                }
                else
                {
                    cat.crc = "";
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
                    Utils.exceptionLog("getFileSize", "read from onlineDatabaseXml: " + file, ex);
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
                    Utils.exceptionLog("getFileSize", "FileInfo from local file: " + file, ex);
                }
            }
            try
            {
                return fileSizeBytes;
            }
            catch (Exception ex)
            {
                Utils.exceptionLog("getFileSize", "building format", ex);
            }
            return 0;
        }

        private void CRCFileSizeUpdate_Load(object sender, EventArgs e)
        {
            //font scaling
            this.AutoScaleMode = Settings.appScalingMode;
            this.Font = Settings.appFont;
            if (Settings.appScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new System.Drawing.SizeF(Settings.scaleSize, Settings.scaleSize));
            }
            loadDatabaseDialog.InitialDirectory = Application.StartupPath;
        }

        private void CRCFileSizeUpdate_FormClosing(object sender, FormClosingEventArgs e)
        {
            Utils.appendToLog("|------------------------------------------------------------------------------------------------|");
        }

        private void RunOnlineScriptButton_Click(object sender, EventArgs e)
        {
            OnlineScriptOutput.Text = "Running online script...";
            Application.DoEvents();
            using (WebClient client = new WebClient())
            {
                OnlineScriptOutput.Text = client.DownloadString("http://wotmods.relhaxmodpack.com/scripts/CreateDatabase.php").Replace("<br />", "\n");
            }
            Application.DoEvents();
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
            logicalDependencies = new List<LogicalDependnecy>();
            string gameVersion = Utils.readVersionFromModInfo(databaseLocationTextBox.Text);
            string onlineFolderVersion = Utils.readOnlineFolderFromModInfo(databaseLocationTextBox.Text);
            Utils.createModStructure(databaseLocationTextBox.Text, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            int duplicatesCounter = 0;
            //check for duplicates
            if (Utils.duplicates(parsedCatagoryList) && Utils.duplicatesPackageName(parsedCatagoryList, ref duplicatesCounter))
            {
                MessageBox.Show(string.Format("{0} duplicates found !!!", duplicatesCounter));
                return;
            }
            updatingLabel.Text = "Updating database...";
            Application.DoEvents();
            globalDepsSB.Append("Global Dependencies updated:\n");
            dependenciesSB.Append("Dependencies updated:\n");
            modsSB.Append("Mods updated:\n");
            configsSB.Append("Configs updated:\n");
            //foreach zip file name
            foreach (Dependency d in globalDependencies)
            {
                int index = this.getZipIndex(d.dependencyZipFile);
                if (index == -1)
                {
                    continue;
                }
                if (d.dependencyZipCRC == null || d.dependencyZipCRC.Equals("") || d.dependencyZipCRC.Equals("f"))
                {
                    d.dependencyZipCRC = Utils.createMd5Hash(addZipsDialog.FileNames[index]);
                    globalDepsSB.Append(d.dependencyZipFile + "\n");
                }
            }
            foreach (Dependency d in dependencies)
            {
                int index = this.getZipIndex(d.dependencyZipFile);
                if (index == -1)
                {
                    continue;
                }
                if (d.dependencyZipCRC == null || d.dependencyZipCRC.Equals("") || d.dependencyZipCRC.Equals("f"))
                {
                    d.dependencyZipCRC = Utils.createMd5Hash(addZipsDialog.FileNames[index]);
                    dependenciesSB.Append(d.dependencyZipFile + "\n");
                }
            }
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    int index = this.getZipIndex(m.zipFile);
                    if (index != -1)
                    {
                        m.size = this.getFileSize(addZipsDialog.FileNames[index]);
                        if (m.crc == null || m.crc.Equals("") || m.crc.Equals("f"))
                        {
                            m.crc = Utils.createMd5Hash(addZipsDialog.FileNames[index]);

                            modsSB.Append(m.zipFile + "\n");
                        }
                    }
                    if (m.configs.Count > 0)
                    {
                        this.processConfigsCRCUpdate_old(m.configs);
                    }
                }
            }
            //update the crc value
            //update the file size
            //save config file
            string newModInfo = databaseLocationTextBox.Text;
            //this.saveDatabase(databaseLocationTextBox.Text, gameVersion);
            Utils.SaveDatabase(databaseLocationTextBox.Text, gameVersion, onlineFolderVersion, globalDependencies, dependencies, logicalDependencies, parsedCatagoryList);
            MessageBox.Show(globalDepsSB.ToString() + dependenciesSB.ToString() + modsSB.ToString() + configsSB.ToString());
            updatingLabel.Text = "Idle";
        }

        private void processConfigsCRCUpdate_old(List<Config> cfgList)
        {
            foreach (Config cat in cfgList)
            {
                int cindex = this.getZipIndex(cat.zipFile);
                if (cindex != -1)
                {
                    cat.size = this.getFileSize(addZipsDialog.FileNames[cindex]);
                    if (cat.crc == null || cat.crc.Equals("") || cat.crc.Equals("f"))
                    {
                        cat.crc = Utils.createMd5Hash(addZipsDialog.FileNames[cindex]);

                        configsSB.Append(cat.zipFile + "\n");
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
    }
}
