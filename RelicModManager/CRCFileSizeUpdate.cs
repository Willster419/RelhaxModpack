using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;
using System.Xml;

namespace RelhaxModpack
{
    public partial class CRCFileSizeUpdate : Form
    {
        private List<Dependency> globalDependencies;
        private List<Catagory> parsedCatagoryList;
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
            parsedCatagoryList = new List<Catagory>();
            this.createModStructure2(databaseLocationTextBox.Text);
            //check for duplicates
            if (this.duplicates())
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
                    d.dependencyZipCRC = Settings.GetMd5Hash(addZipsDialog.FileNames[index]);
                    globalDepsSB.Append(d.dependencyZipFile + "\n");
                }
            }
            foreach (Catagory c in parsedCatagoryList)
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
                        d.dependencyZipCRC = Settings.GetMd5Hash(addZipsDialog.FileNames[index]);
                        dependenciesSB.Append(d.dependencyZipFile + "\n");
                    }
                }
                foreach (Mod m in c.mods)
                {
                    int index = this.getZipIndex(m.modZipFile);
                    if (index != -1)
                    {
                        m.size = this.getFileSize(addZipsDialog.FileNames[index]);
                        if (m.crc == null || m.crc.Equals("") || m.crc.Equals("f"))
                        {
                            m.crc = Settings.GetMd5Hash(addZipsDialog.FileNames[index]);
                            
                            modsSB.Append(m.modZipFile + "\n");
                        }
                    }
                    foreach (Config cat in m.configs)
                    {
                        int cindex = this.getZipIndex(cat.zipConfigFile);
                        if (cindex != -1)
                        {
                            cat.size = this.getFileSize(addZipsDialog.FileNames[cindex]);
                            if (cat.crc == null || cat.crc.Equals("") || cat.crc.Equals("f"))
                            {
                                cat.crc = Settings.GetMd5Hash(addZipsDialog.FileNames[cindex]);
                                
                                configsSB.Append(cat.zipConfigFile + "\n");
                            }
                        }
                    }
                }
            }
            //update the crc value

            //update the file size

            //save config file
            string newModInfo = databaseLocationTextBox.Text.Substring(0, databaseLocationTextBox.Text.Length - 4);
            newModInfo = newModInfo + "Updated.xml";
            newModInfo = databaseLocationTextBox.Text;
            this.saveDatabase(newModInfo);
            MessageBox.Show(globalDepsSB.ToString() + dependenciesSB.ToString() + modsSB.ToString() + configsSB.ToString());
            updatingLabel.Text = "Idle";
        }
        private float getFileSize(string file)
        {
            FileInfo fi = new FileInfo(file);
            float fileSizeBytes = fi.Length;
            float fileSizeKBytes = fileSizeBytes / 1024;
            float fileSizeMBytes = fileSizeKBytes / 1024;
            fileSizeMBytes = (float) Math.Round(fileSizeMBytes,1);
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
        private void createModStructure2(string databaseFilePath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(databaseFilePath);
            }
            catch (XmlException)
            {
                Settings.appendToLog("CRITICAL: Failed to read database!");
                MessageBox.Show("CRITICAL: Failed to read database!");
                Application.Exit();
            }
            //add the global dependencies
            globalDependencies = new List<Dependency>();
            XmlNodeList globalDependenciesList = doc.SelectNodes("//modInfoAlpha.xml/globaldependencies/globaldependency");
            foreach (XmlNode dependencyNode in globalDependenciesList)
            {
                Dependency d = new Dependency();
                foreach (XmlNode globs in dependencyNode.ChildNodes)
                {
                    switch (globs.Name)
                    {
                        case "dependencyZipFile":
                            d.dependencyZipFile = globs.InnerText;
                            break;
                        case "dependencyZipCRC":
                            d.dependencyZipCRC = globs.InnerText;
                            break;
                        case "dependencyenabled":
                            try
                            {
                                d.enabled = bool.Parse(globs.InnerText);
                            }
                            catch (FormatException)
                            {
                                d.enabled = false;
                            }
                            break;
                    }
                }
                globalDependencies.Add(d);
            }

            XmlNodeList catagoryList = doc.SelectNodes("//modInfoAlpha.xml/catagories/catagory");
            parsedCatagoryList = new List<Catagory>();
            foreach (XmlNode nnnnn in catagoryList)
            {
                Catagory cat = new Catagory();
                foreach (XmlNode nnnnnn in nnnnn.ChildNodes)
                {
                    switch (nnnnnn.Name)
                    {
                        case "name":
                            cat.name = nnnnnn.InnerText;
                            break;
                        case "selectionType":
                            cat.selectionType = nnnnnn.InnerText;
                            break;
                        case "mods":
                            foreach (XmlNode n in nnnnnn.ChildNodes)
                            {
                                Mod m = new Mod();
                                foreach (XmlNode nn in n.ChildNodes)
                                {
                                    switch (nn.Name)
                                    {
                                        case "name":
                                            m.name = nn.InnerText;
                                            break;
                                        case "version":
                                            m.version = nn.InnerText;
                                            break;
                                        case "size":
                                            try
                                            {
                                                m.size = float.Parse(nn.InnerText);
                                            }
                                            catch (FormatException)
                                            {
                                                m.size = (float)0.0;
                                            }
                                            break;
                                        case "modzipfile":
                                            m.modZipFile = nn.InnerText;
                                            break;
                                        case "modzipcrc":
                                            m.crc = nn.InnerText;
                                            break;
                                        case "enabled":
                                            try
                                            {
                                                m.enabled = bool.Parse(nn.InnerText);
                                            }
                                            catch (FormatException)
                                            {
                                                m.enabled = false;
                                            }
                                            break;
                                        case "description":
                                            m.description = nn.InnerText;
                                            break;
                                        case "updateComment":
                                            m.updateComment = nn.InnerText;
                                            break;
                                        case "devURL":
                                            m.devURL = nn.InnerText;
                                            break;
                                        case "userDatas":
                                            foreach (XmlNode nnnnnnn in nn.ChildNodes)
                                            {

                                                switch (nnnnnnn.Name)
                                                {
                                                    case "userData":
                                                        string innerText = nnnnnnn.InnerText;
                                                        if (innerText == null)
                                                            continue;
                                                        if (innerText.Equals(""))
                                                            continue;
                                                        m.userFiles.Add(innerText);
                                                        break;
                                                }

                                            }
                                            break;
                                        case "pictures":
                                            //parse every picture
                                            foreach (XmlNode nnnnnnn in nn.ChildNodes)
                                            {
                                                foreach (XmlNode nnnnnnnn in nnnnnnn.ChildNodes)
                                                {
                                                    switch (nnnnnnnn.Name)
                                                    {
                                                        case "URL":
                                                            string innerText = nnnnnnnn.InnerText;
                                                            if (innerText == null)
                                                                continue;
                                                            if (innerText.Equals(""))
                                                                continue;
                                                            m.picList.Add(new Picture("Mod: " + m.name, nnnnnnnn.InnerText));
                                                            break;
                                                    }
                                                }
                                            }
                                            break;
                                        case "configs":
                                            //parse every config for that mod
                                            foreach (XmlNode nnn in nn.ChildNodes)
                                            {
                                                Config c = new Config();
                                                foreach (XmlNode nnnn in nnn.ChildNodes)
                                                {
                                                    switch (nnnn.Name)
                                                    {
                                                        case "name":
                                                            c.name = nnnn.InnerText;
                                                            break;
                                                        case "configzipfile":
                                                            c.zipConfigFile = nnnn.InnerText;
                                                            break;
                                                        case "configzipcrc":
                                                            c.crc = nnnn.InnerText;
                                                            break;
                                                        case "configenabled":
                                                            try
                                                            {
                                                                c.enabled = bool.Parse(nnnn.InnerText);
                                                            }
                                                            catch (FormatException)
                                                            {
                                                                c.enabled = false;
                                                            }
                                                            break;
                                                        case "size":
                                                            try
                                                            {
                                                                c.size = float.Parse(nnnn.InnerText);
                                                            }
                                                            catch (FormatException)
                                                            {
                                                                c.size = (float)0.0;
                                                            }
                                                            break;
                                                        case "configtype":
                                                            c.type = nnnn.InnerText;
                                                            break;
                                                        case "pictures":
                                                            //parse every picture
                                                            foreach (XmlNode nnnnnnnn in nnnn.ChildNodes)
                                                            {
                                                                foreach (XmlNode nnnnnnnnnn in nnnnnnnn.ChildNodes)
                                                                {
                                                                    switch (nnnnnnnnnn.Name)
                                                                    {
                                                                        case "URL":
                                                                            string innerText = nnnnnnnn.InnerText;
                                                                            if (innerText == null)
                                                                                continue;
                                                                            if (innerText.Equals(""))
                                                                                continue;
                                                                            c.pictureList.Add(nnnnnnnn.InnerText);
                                                                            //m.picList.Add(new Picture("Config: " + c.name, nnnnnnnn.InnerText));
                                                                            break;
                                                                    }
                                                                }
                                                            }
                                                            break;
                                                    }
                                                }
                                                m.configs.Add(c);
                                            }
                                            break;
                                    }
                                }
                                cat.mods.Add(m);
                            }
                            break;
                        case "dependencies":
                            //parse every config for that mod
                            foreach (XmlNode nnn in nnnnnn.ChildNodes)
                            {
                                Dependency d = new Dependency();
                                foreach (XmlNode nnnn in nnn.ChildNodes)
                                {
                                    switch (nnnn.Name)
                                    {
                                        case "dependencyZipFile":
                                            d.dependencyZipFile = nnnn.InnerText;
                                            break;
                                        case "dependencyZipCRC":
                                            d.dependencyZipCRC = nnnn.InnerText;
                                            break;
                                        case "dependencyenabled":
                                            try
                                            {
                                                d.enabled = bool.Parse(nnnn.InnerText);
                                            }
                                            catch (FormatException)
                                            {
                                                d.enabled = false;
                                            }
                                            break;
                                    }
                                }
                                cat.dependencies.Add(d);
                            }
                            break;
                    }
                }
                parsedCatagoryList.Add(cat);
            }

        }
        //returns the mod based and mod name
        private Mod linkMod(string modName)
        {
            foreach (Catagory c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.name.Equals(modName))
                    {
                        //found it
                        return m;
                    }
                }
            }
            return null;
        }
        private bool duplicates()
        {
            //add every mod name to a new list
            List<string> modNameList = new List<string>();
            foreach (Catagory c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    modNameList.Add(m.name);
                }
            }
            //itterate through every mod name again
            foreach (Catagory c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    //in theory, there should only be one mathcing mod name
                    //between the two lists. more indicates a duplicates
                    int i = 0;
                    foreach (string s in modNameList)
                    {
                        if (s.Equals(m.name))
                            i++;
                    }
                    if (i > 1)//if there are 2 or more matching mods
                        return true;//duplicate detected
                }
            }
            //making it here means there are no duplicates
            return false;
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
            foreach (Catagory c in parsedCatagoryList)
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
                    modZipFile.InnerText = m.modZipFile;
                    modRoot.AppendChild(modZipFile);
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
                    foreach (Picture p in m.picList)
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
                        configZipFile.InnerText = cc.zipConfigFile;
                        configRoot.AppendChild(configZipFile);
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
                        configsHolder.AppendChild(configRoot);
                    }
                    modRoot.AppendChild(configsHolder);
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
