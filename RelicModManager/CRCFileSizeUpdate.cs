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
            //load database
            globalDependencies = new List<Dependency>();
            parsedCatagoryList = new List<Category>();
            Utils.createModStructure2(databaseLocationTextBox.Text, true, globalDependencies, parsedCatagoryList);
            //check for duplicates
            if (Utils.duplicates(parsedCatagoryList))
            {
                MessageBox.Show("Duplicates!!!");
                return;
            }
            updatingLabel.Text = "Updating database...";
            Application.DoEvents();
            StringBuilder globalDepsSB = new StringBuilder();
            globalDepsSB.Append("Global Dependencies updated:\n");
            StringBuilder dependenciesSB = new StringBuilder();
            dependenciesSB.Append("Dependencies updated:\n");
            StringBuilder modsSB = new StringBuilder();
            modsSB.Append("Mods updated:\n");
            StringBuilder configsSB = new StringBuilder();
            configsSB.Append("Configs updated:\n");
            StringBuilder subConfigsSB = new StringBuilder();
            subConfigsSB.Append("SubConfigs updated:\n");
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
                    foreach (Config cat in m.configs)
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
                        foreach (Config sc in cat.configs)
                        {
                            int scindex = this.getZipIndex(sc.zipFile);
                            if (scindex != -1)
                            {
                                sc.size = this.getFileSize(addZipsDialog.FileNames[scindex]);
                                if (sc.crc == null || sc.crc.Equals("") || sc.crc.Equals("f"))
                                {
                                    sc.crc = Utils.getMd5Hash(addZipsDialog.FileNames[scindex]);
                                    subConfigsSB.Append(sc.zipFile + "\n");
                                }
                            }
                            foreach (Dependency d in sc.dependencies)
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
                        }
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
            MessageBox.Show(globalDepsSB.ToString() + dependenciesSB.ToString() + modsSB.ToString() + configsSB.ToString() + subConfigsSB.ToString());
            updatingLabel.Text = "Idle";
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
            addZipsDialog.InitialDirectory = Application.StartupPath;
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
                    XmlElement modZipFile = doc.CreateElement("modzipfile");
                    modZipFile.InnerText = m.zipFile;
                    modRoot.AppendChild(modZipFile);
                    XmlElement modStartAddress = doc.CreateElement("startAddress");
                    modStartAddress.InnerText = m.startAddress;
                    modRoot.AppendChild(modStartAddress);
                    XmlElement modEndAddress = doc.CreateElement("endAddress");
                    modEndAddress.InnerText = m.endAddress;
                    modRoot.AppendChild(modEndAddress);
                    XmlElement modZipCRC = doc.CreateElement("modzipcrc");
                    modZipCRC.InnerText = m.crc;
                    modRoot.AppendChild(modZipCRC);
                    XmlElement modZipSize = doc.CreateElement("size");
                    modZipSize.InnerText = "" + m.size;
                    modRoot.AppendChild(modZipSize);
                    XmlElement modEnabled = doc.CreateElement("enabled");
                    modEnabled.InnerText = "" + m.enabled;
                    modRoot.AppendChild(modEnabled);
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
                    foreach (Config cc in m.configs)
                    {
                        //add the config to the list
                        XmlElement configRoot = doc.CreateElement("config");
                        configsHolder.AppendChild(configRoot);
                        XmlElement configName = doc.CreateElement("name");
                        configName.InnerText = cc.name;
                        configRoot.AppendChild(configName);
                        XmlElement configZipFile = doc.CreateElement("configzipfile");
                        configZipFile.InnerText = cc.zipFile;
                        configRoot.AppendChild(configZipFile);
                        XmlElement configStartAddress = doc.CreateElement("startAddress");
                        configStartAddress.InnerText = cc.startAddress;
                        configRoot.AppendChild(configStartAddress);
                        XmlElement configEndAddress = doc.CreateElement("endAddress");
                        configEndAddress.InnerText = cc.endAddress;
                        configRoot.AppendChild(configEndAddress);
                        XmlElement configZipCRC = doc.CreateElement("configzipcrc");
                        configZipCRC.InnerText = cc.crc;
                        configRoot.AppendChild(configZipCRC);
                        XmlElement configZipSize = doc.CreateElement("size");
                        configZipSize.InnerText = "" + cc.size;
                        configRoot.AppendChild(configZipSize);
                        XmlElement configEnabled = doc.CreateElement("configenabled");
                        configEnabled.InnerText = "" + cc.enabled;
                        configRoot.AppendChild(configEnabled);
                        XmlElement configType = doc.CreateElement("configtype");
                        configType.InnerText = cc.type;
                        configRoot.AppendChild(configType);
                        //pictures for the configs
                        XmlElement configPictures = doc.CreateElement("pictures");
                        foreach (string p in cc.pictureList)
                        {
                            XmlElement configpictureRoot = doc.CreateElement("picture");
                            XmlElement configpictureURL = doc.CreateElement("URL");
                            configpictureURL.InnerText = p;
                            configpictureRoot.AppendChild(configpictureURL);
                            configPictures.AppendChild(configpictureRoot);
                        }
                        configRoot.AppendChild(configPictures);
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
                            //attach dependency root
                            catDependencies.AppendChild(DependencyRoot);
                        }
                        configRoot.AppendChild(catDependencies);
                        //subconfigs for the configs
                        XmlElement subconfigHolder = doc.CreateElement("configs");
                        foreach (Config s in cc.configs)
                        {
                            //decalre subConfig root
                            XmlElement subConfigRoot = doc.CreateElement("subConfig");
                            //make subConfig
                            XmlElement subConfigName = doc.CreateElement("name");
                            subConfigName.InnerText = s.name;
                            subConfigRoot.AppendChild(subConfigName);
                            XmlElement subConfigZipFile = doc.CreateElement("zipFile");
                            subConfigZipFile.InnerText = s.zipFile;
                            subConfigRoot.AppendChild(subConfigZipFile);
                            XmlElement subConfigCRC = doc.CreateElement("crc");
                            subConfigCRC.InnerText = s.crc;
                            subConfigRoot.AppendChild(subConfigCRC);
                            XmlElement subConfigEnabled = doc.CreateElement("enabled");
                            subConfigEnabled.InnerText = "" + s.enabled;
                            subConfigRoot.AppendChild(subConfigEnabled);
                            XmlElement subConfigType = doc.CreateElement("type");
                            subConfigType.InnerText = s.type;
                            subConfigRoot.AppendChild(subConfigType);
                            XmlElement subConfigSize = doc.CreateElement("size");
                            subConfigSize.InnerText = "" + s.size;
                            subConfigRoot.AppendChild(subConfigSize);
                            XmlElement subConfigStartAddress = doc.CreateElement("startAddress");
                            subConfigStartAddress.InnerText = s.startAddress;
                            subConfigRoot.AppendChild(subConfigStartAddress);
                            XmlElement subConfigEndAddress = doc.CreateElement("endAddress");
                            subConfigEndAddress.InnerText = s.endAddress;
                            subConfigRoot.AppendChild(subConfigEndAddress);
                            //dependencies for the subconfigs
                            XmlElement catSubDependencies = doc.CreateElement("dependencies");
                            foreach (Dependency d in s.dependencies)
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
                                //attach dependency root
                                catDependencies.AppendChild(DependencyRoot);
                            }
                            subConfigRoot.AppendChild(catSubDependencies);
                            //dependencies for the pictures
                            XmlElement subConfigPictures = doc.CreateElement("pictures");
                            foreach (string p in s.pictureList)
                            {
                                XmlElement configpictureRoot = doc.CreateElement("picture");
                                XmlElement configpictureURL = doc.CreateElement("URL");
                                configpictureURL.InnerText = p;
                                configpictureRoot.AppendChild(configpictureURL);
                                configPictures.AppendChild(configpictureRoot);
                            }
                            subConfigRoot.AppendChild(subConfigPictures);
                            //attach subconfig root
                            subconfigHolder.AppendChild(subConfigRoot);
                        }
                        configRoot.AppendChild(subconfigHolder);
                        configsHolder.AppendChild(configRoot);
                    }
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
                        //attach dependency root
                        modDependencies.AppendChild(DependencyRoot);
                    }
                    modRoot.AppendChild(modDependencies);
                    XmlElement modDescription = doc.CreateElement("description");
                    modDescription.InnerText = m.description;
                    modRoot.AppendChild(modDescription);
                    XmlElement modUpdateComment = doc.CreateElement("updateComment");
                    modUpdateComment.InnerText = m.updateComment;
                    modRoot.AppendChild(modUpdateComment);
                    XmlElement modDevURL = doc.CreateElement("devURL");
                    modDevURL.InnerText = m.devURL;
                    modRoot.AppendChild(modDevURL);
                    modsHolder.AppendChild(modRoot);
                }
                catagoryRoot.AppendChild(modsHolder);
                //append catagory
                catagoriesHolder.AppendChild(catagoryRoot);
            }
            root.AppendChild(catagoriesHolder);
            doc.Save(saveLocation);
        }
    }
}
