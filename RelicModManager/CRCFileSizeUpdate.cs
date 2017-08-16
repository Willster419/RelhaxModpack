using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace RelhaxModpack
{
    public partial class CRCFileSizeUpdate : Form
    {
        private List<Dependency> globalDependencies;
        private List<Category> parsedCatagoryList;
        StringBuilder globalDepsSB = new StringBuilder();
        StringBuilder dependenciesSB = new StringBuilder();
        StringBuilder modsSB = new StringBuilder();
        StringBuilder configsSB = new StringBuilder();
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

        private void loadZipFilesButton_Click(object sender, EventArgs e)
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
            Utils.createModStructure(databaseLocationTextBox.Text, true, globalDependencies, parsedCatagoryList);
            int duplicatesCounter = 0;
            //check for duplicates
            if (Utils.duplicates(parsedCatagoryList) && Utils.duplicatesPackageName(parsedCatagoryList, ref duplicatesCounter ))
            {
                MessageBox.Show(string.Format("{0} duplicates found !!!",duplicatesCounter));
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
                    d.dependencyZipCRC = Utils.getMd5Hash(addZipsDialog.FileNames[index]);
                    globalDepsSB.Append(d.dependencyZipFile + "\n");
                }
            }
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Dependency d in c.dependencies)
                {
                    int index = this.getZipIndex(d.dependencyZipFile);
                    if (index == -1)
                    {
                        continue;
                    }
                    if (d.dependencyZipCRC == null || d.dependencyZipCRC.Equals("") || d.dependencyZipCRC.Equals("f"))
                    {
                        d.dependencyZipCRC = Utils.getMd5Hash(addZipsDialog.FileNames[index]);
                        dependenciesSB.Append(d.dependencyZipFile + "\n");
                    }
                }
                foreach (Mod m in c.mods)
                {
                    int index = this.getZipIndex(m.zipFile);
                    if (index != -1)
                    {
                        m.size = this.getFileSize(addZipsDialog.FileNames[index]);
                        if (m.crc == null || m.crc.Equals("") || m.crc.Equals("f"))
                        {
                            m.crc = Utils.getMd5Hash(addZipsDialog.FileNames[index]);

                            modsSB.Append(m.zipFile + "\n");
                        }
                    }
                    if (m.configs.Count > 0)
                    {
                        this.processConfigsCRCUpdate(m.configs);
                    }
                    foreach (Dependency d in m.dependencies)
                    {
                        int mindex = this.getZipIndex(d.dependencyZipFile);
                        if (mindex != -1)
                        {
                            if (d.dependencyZipCRC == null || d.dependencyZipCRC.Equals("") || d.dependencyZipCRC.Equals("f"))
                            {
                                d.dependencyZipCRC = Utils.getMd5Hash(addZipsDialog.FileNames[mindex]);
                                dependenciesSB.Append(d.dependencyZipFile + "\n");
                            }
                        }
                    }
                }
            }
            //update the crc value
            //update the file size
            //save config file
            string newModInfo = databaseLocationTextBox.Text;
            this.saveDatabase(newModInfo);
            MessageBox.Show(globalDepsSB.ToString() + dependenciesSB.ToString() + modsSB.ToString() + configsSB.ToString());
            updatingLabel.Text = "Idle";
        }
        private void processConfigsCRCUpdate(List<Config> cfgList)
        {
            foreach (Config cat in cfgList)
            {
                int cindex = this.getZipIndex(cat.zipFile);
                if (cindex != -1)
                {
                    cat.size = this.getFileSize(addZipsDialog.FileNames[cindex]);
                    if (cat.crc == null || cat.crc.Equals("") || cat.crc.Equals("f"))
                    {
                        cat.crc = Utils.getMd5Hash(addZipsDialog.FileNames[cindex]);

                        configsSB.Append(cat.zipFile + "\n");
                    }
                }
                foreach (Dependency d in cat.dependencies)
                {
                    int cindex2 = this.getZipIndex(d.dependencyZipFile);
                    if (cindex2 != -1)
                    {
                        if (d.dependencyZipCRC == null || d.dependencyZipCRC.Equals("") || d.dependencyZipCRC.Equals("f"))
                        {
                            d.dependencyZipCRC = Utils.getMd5Hash(addZipsDialog.FileNames[cindex2]);
                            dependenciesSB.Append(d.dependencyZipFile + "\n");
                        }
                    }
                }
                if (cat.configs.Count > 0)
                {
                    this.processConfigsCRCUpdate(cat.configs);
                }
            }
        }
        private float getFileSize(string file)
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
        private void CRCFileSizeUpdate_Load(object sender, EventArgs e)
        {
            //font scaling
            this.AutoScaleMode = Settings.appScalingMode;
            this.Font = Settings.appFont;
            if (Settings.appScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new System.Drawing.SizeF(Settings.scaleSize, Settings.scaleSize));
            }
            //addZipsDialog.InitialDirectory = Application.StartupPath;
            loadDatabaseDialog.InitialDirectory = Application.StartupPath;
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
        //saves the mod database
        private void saveDatabase(string saveLocation)
        {
            XmlDocument doc = new XmlDocument();
            //database root modInfo.xml
            XmlElement root = doc.CreateElement("modInfoAlpha.xml");
            doc.AppendChild(root);
            //global dependencies
            XmlElement globalDependenciesXml = doc.CreateElement("globaldependencies");
            foreach (Dependency d in globalDependencies)
            {
                //declare dependency root
                XmlElement globalDependencyRoot = doc.CreateElement("globaldependency");
                //make dependency
                XmlElement globalDepZipFile = doc.CreateElement("dependencyZipFile");
                globalDepZipFile.InnerText = d.dependencyZipFile;
                globalDependencyRoot.AppendChild(globalDepZipFile);
                XmlElement globalDepStartAddress = doc.CreateElement("startAddress");
                globalDepStartAddress.InnerText = d.startAddress;
                globalDependencyRoot.AppendChild(globalDepStartAddress);
                XmlElement globalDepEndAddress = doc.CreateElement("endAddress");
                globalDepEndAddress.InnerText = d.endAddress;
                globalDependencyRoot.AppendChild(globalDepEndAddress);
                XmlElement globalDepCRC = doc.CreateElement("dependencyZipCRC");
                globalDepCRC.InnerText = d.dependencyZipCRC;
                globalDependencyRoot.AppendChild(globalDepCRC);
                XmlElement globalDepEnabled = doc.CreateElement("dependencyenabled");
                globalDepEnabled.InnerText = "" + d.enabled;
                globalDependencyRoot.AppendChild(globalDepEnabled);
                XmlElement globalDepPackageName = doc.CreateElement("packageName");
                globalDepPackageName.InnerText = d.packageName;
                globalDependencyRoot.AppendChild(globalDepPackageName);
                //attach dependency root
                globalDependenciesXml.AppendChild(globalDependencyRoot);
            }
            root.AppendChild(globalDependenciesXml);
            //catagories
            XmlElement catagoriesHolder = doc.CreateElement("catagories");
            foreach (Category c in parsedCatagoryList)
            {
                //catagory root
                XmlElement catagoryRoot = doc.CreateElement("catagory");
                //make catagory
                XmlElement catagoryName = doc.CreateElement("name");
                catagoryName.InnerText = c.name;
                catagoryRoot.AppendChild(catagoryName);
                XmlElement catagorySelectionType = doc.CreateElement("selectionType");
                catagorySelectionType.InnerText = c.selectionType;
                catagoryRoot.AppendChild(catagorySelectionType);
                //dependencies for catagory
                XmlElement catagoryDependencies = doc.CreateElement("dependencies");
                foreach (Dependency d in c.dependencies)
                {
                    //declare dependency root
                    XmlElement DependencyRoot = doc.CreateElement("dependency");
                    //make dependency
                    XmlElement DepZipFile = doc.CreateElement("dependencyZipFile");
                    DepZipFile.InnerText = d.dependencyZipFile;
                    DependencyRoot.AppendChild(DepZipFile);
                    XmlElement DepStartAddress = doc.CreateElement("startAddress");
                    DepStartAddress.InnerText = d.startAddress;
                    DependencyRoot.AppendChild(DepStartAddress);
                    XmlElement DepEndAddress = doc.CreateElement("endAddress");
                    DepEndAddress.InnerText = d.endAddress;
                    DependencyRoot.AppendChild(DepEndAddress);
                    XmlElement DepCRC = doc.CreateElement("dependencyZipCRC");
                    DepCRC.InnerText = d.dependencyZipCRC;
                    DependencyRoot.AppendChild(DepCRC);
                    XmlElement DepEnabled = doc.CreateElement("dependencyenabled");
                    DepEnabled.InnerText = "" + d.enabled;
                    DependencyRoot.AppendChild(DepEnabled);
                    XmlElement DepPackageName = doc.CreateElement("packageName");
                    DepPackageName.InnerText = d.packageName;
                    DependencyRoot.AppendChild(DepPackageName);
                    //attach dependency root
                    catagoryDependencies.AppendChild(DependencyRoot);
                }
                catagoryRoot.AppendChild(catagoryDependencies);
                //mods for catagory
                XmlElement modsHolder = doc.CreateElement("mods");
                foreach (Mod m in c.mods)
                {
                    //add it to the list
                    XmlElement modRoot = doc.CreateElement("mod");
                    XmlElement modName = doc.CreateElement("name");
                    modName.InnerText = m.name;
                    modRoot.AppendChild(modName);
                    XmlElement modVersion = doc.CreateElement("version");
                    modVersion.InnerText = m.version;
                    modRoot.AppendChild(modVersion);
                    XmlElement modZipFile = doc.CreateElement("zipFile");
                    modZipFile.InnerText = m.zipFile;
                    modRoot.AppendChild(modZipFile);
                    XmlElement modStartAddress = doc.CreateElement("startAddress");
                    modStartAddress.InnerText = m.startAddress;
                    modRoot.AppendChild(modStartAddress);
                    XmlElement modEndAddress = doc.CreateElement("endAddress");
                    modEndAddress.InnerText = m.endAddress;
                    modRoot.AppendChild(modEndAddress);
                    XmlElement modZipCRC = doc.CreateElement("crc");
                    modZipCRC.InnerText = m.crc;
                    modRoot.AppendChild(modZipCRC);
                    XmlElement modEnabled = doc.CreateElement("enabled");
                    modEnabled.InnerText = "" + m.enabled;
                    modRoot.AppendChild(modEnabled);
                    XmlElement modPackageName = doc.CreateElement("packageName");
                    modPackageName.InnerText = m.packageName;
                    modRoot.AppendChild(modPackageName);
                    XmlElement modZipSize = doc.CreateElement("size");
                    modZipSize.InnerText = "" + m.size;
                    modRoot.AppendChild(modZipSize);
                    XmlElement modUpdateComment = doc.CreateElement("updateComment");
                    modUpdateComment.InnerText = m.updateComment;
                    modRoot.AppendChild(modUpdateComment);
                    XmlElement modDescription = doc.CreateElement("description");
                    modDescription.InnerText = m.description;
                    modRoot.AppendChild(modDescription);
                    XmlElement modDevURL = doc.CreateElement("devURL");
                    modDevURL.InnerText = m.devURL;
                    modRoot.AppendChild(modDevURL);
                    //datas for the mods
                    XmlElement modDatas = doc.CreateElement("userDatas");
                    foreach (string s in m.userFiles)
                    {
                        XmlElement userData = doc.CreateElement("userData");
                        userData.InnerText = s;
                        modDatas.AppendChild(userData);
                    }
                    modRoot.AppendChild(modDatas);
                    //pictures for the mods
                    XmlElement modPictures = doc.CreateElement("pictures");
                    foreach (Picture p in m.pictureList)
                    {
                        XmlElement pictureRoot = doc.CreateElement("picture");
                        XmlElement pictureURL = doc.CreateElement("URL");
                        pictureURL.InnerText = p.URL;
                        pictureRoot.AppendChild(pictureURL);
                        modPictures.AppendChild(pictureRoot);
                    }
                    modRoot.AppendChild(modPictures);
                    //configs for the mods
                    XmlElement configsHolder = doc.CreateElement("configs");
                    //if statement here
                    if (m.configs.Count > 0)
                        saveDatabaseConfigLevel(doc, configsHolder, m.configs);
                    modRoot.AppendChild(configsHolder);
                    XmlElement modDependencies = doc.CreateElement("dependencies");
                    foreach (Dependency d in m.dependencies)
                    {
                        //declare dependency root
                        XmlElement DependencyRoot = doc.CreateElement("dependency");
                        //make dependency
                        XmlElement DepZipFile = doc.CreateElement("dependencyZipFile");
                        DepZipFile.InnerText = d.dependencyZipFile;
                        DependencyRoot.AppendChild(DepZipFile);
                        XmlElement DepStartAddress = doc.CreateElement("startAddress");
                        DepStartAddress.InnerText = d.startAddress;
                        DependencyRoot.AppendChild(DepStartAddress);
                        XmlElement DepEndAddress = doc.CreateElement("endAddress");
                        DepEndAddress.InnerText = d.endAddress;
                        DependencyRoot.AppendChild(DepEndAddress);
                        XmlElement DepCRC = doc.CreateElement("dependencyZipCRC");
                        DepCRC.InnerText = d.dependencyZipCRC;
                        DependencyRoot.AppendChild(DepCRC);
                        XmlElement DepEnabled = doc.CreateElement("dependencyenabled");
                        DepEnabled.InnerText = "" + d.enabled;
                        DependencyRoot.AppendChild(DepEnabled);
                        XmlElement DepPackageName = doc.CreateElement("packageName");
                        DepPackageName.InnerText = d.packageName;
                        DependencyRoot.AppendChild(DepPackageName);
                        //attach dependency root
                        modDependencies.AppendChild(DependencyRoot);
                    }
                    modRoot.AppendChild(modDependencies);
                    modsHolder.AppendChild(modRoot);
                }
                catagoryRoot.AppendChild(modsHolder);
                //append catagory
                catagoriesHolder.AppendChild(catagoryRoot);
            }
            root.AppendChild(catagoriesHolder);
            doc.Save(saveLocation);
        }
        private void saveDatabaseConfigLevel(XmlDocument doc, XmlElement configsHolder, List<Config> configsList)
        {
            foreach (Config cc in configsList)
            {
                //add the config to the list
                XmlElement configRoot = doc.CreateElement("config");
                configsHolder.AppendChild(configRoot);
                XmlElement configName = doc.CreateElement("name");
                configName.InnerText = cc.name;
                configRoot.AppendChild(configName);
                XmlElement configVersion = doc.CreateElement("version");
                configVersion.InnerText = cc.version;
                configRoot.AppendChild(configVersion);
                XmlElement configZipFile = doc.CreateElement("zipFile");
                configZipFile.InnerText = cc.zipFile;
                configRoot.AppendChild(configZipFile);
                XmlElement configStartAddress = doc.CreateElement("startAddress");
                configStartAddress.InnerText = cc.startAddress;
                configRoot.AppendChild(configStartAddress);
                XmlElement configEndAddress = doc.CreateElement("endAddress");
                configEndAddress.InnerText = cc.endAddress;
                configRoot.AppendChild(configEndAddress);
                XmlElement configZipCRC = doc.CreateElement("crc");
                configZipCRC.InnerText = cc.crc;
                configRoot.AppendChild(configZipCRC);
                XmlElement configEnabled = doc.CreateElement("enabled");
                configEnabled.InnerText = "" + cc.enabled;
                configRoot.AppendChild(configEnabled);
                XmlElement configPackageName = doc.CreateElement("packageName");
                configPackageName.InnerText = cc.packageName;
                configRoot.AppendChild(configPackageName);
                XmlElement configSize = doc.CreateElement("size");
                configSize.InnerText = "" + cc.size;
                configRoot.AppendChild(configSize);
                XmlElement configComment = doc.CreateElement("updateComment");
                configComment.InnerText = cc.updateComment;
                configRoot.AppendChild(configComment);
                XmlElement configDescription = doc.CreateElement("description");
                configDescription.InnerText = cc.description;
                configRoot.AppendChild(configDescription);
                XmlElement configDevURL = doc.CreateElement("devURL");
                configDevURL.InnerText = cc.devURL;
                configRoot.AppendChild(configDevURL);
                XmlElement configType = doc.CreateElement("type");
                configType.InnerText = cc.type;
                configRoot.AppendChild(configType);
                //datas for the mods
                XmlElement configDatas = doc.CreateElement("userDatas");
                foreach (string s in cc.userFiles)
                {
                    XmlElement userData = doc.CreateElement("userData");
                    userData.InnerText = s;
                    configDatas.AppendChild(userData);
                }
                configRoot.AppendChild(configDatas);
                //pictures for the configs
                XmlElement configPictures = doc.CreateElement("pictures");
                foreach (Picture p in cc.pictureList)
                {
                    XmlElement configpictureRoot = doc.CreateElement("picture");
                    XmlElement configpictureURL = doc.CreateElement("URL");
                    configpictureURL.InnerText = p.URL;
                    configpictureRoot.AppendChild(configpictureURL);
                    configPictures.AppendChild(configpictureRoot);
                }
                configRoot.AppendChild(configPictures);
                //configs for the configs (meta)
                XmlElement configsHolderSub = doc.CreateElement("configs");
                //if statement here
                if (cc.configs.Count > 0)
                    saveDatabaseConfigLevel(doc, configsHolderSub, cc.configs);
                configRoot.AppendChild(configsHolderSub);
                //dependencies for the configs
                XmlElement catDependencies = doc.CreateElement("dependencies");
                foreach (Dependency d in cc.dependencies)
                {
                    //declare dependency root
                    XmlElement DependencyRoot = doc.CreateElement("dependency");
                    //make dependency
                    XmlElement DepZipFile = doc.CreateElement("dependencyZipFile");
                    DepZipFile.InnerText = d.dependencyZipFile;
                    DependencyRoot.AppendChild(DepZipFile);
                    XmlElement DepStartAddress = doc.CreateElement("startAddress");
                    DepStartAddress.InnerText = d.startAddress;
                    DependencyRoot.AppendChild(DepStartAddress);
                    XmlElement DepEndAddress = doc.CreateElement("endAddress");
                    DepEndAddress.InnerText = d.endAddress;
                    DependencyRoot.AppendChild(DepEndAddress);
                    XmlElement DepCRC = doc.CreateElement("dependencyZipCRC");
                    DepCRC.InnerText = d.dependencyZipCRC;
                    DependencyRoot.AppendChild(DepCRC);
                    XmlElement DepEnabled = doc.CreateElement("dependencyenabled");
                    DepEnabled.InnerText = "" + d.enabled;
                    DependencyRoot.AppendChild(DepEnabled);
                    XmlElement DepPackageName = doc.CreateElement("packageName");
                    DepPackageName.InnerText = d.packageName;
                    DependencyRoot.AppendChild(DepPackageName);
                    //attach dependency root
                    catDependencies.AppendChild(DependencyRoot);
                }
                configRoot.AppendChild(catDependencies);
                configsHolder.AppendChild(configRoot);
            }
        }
    }
}
