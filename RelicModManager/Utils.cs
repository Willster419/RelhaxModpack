using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using System.Security.Principal;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml.Linq;
using System;
using System.Linq;

namespace RelhaxModpack
{
    //a static utility class with usefull methods that all other forms can use if they need it
    public static class Utils
    {
        private static int numByteReads = 0;
        private static bool patchDone = false;
        private static int genericTraverse = 0;
        private static List<string> parsedZips;
        private static string xvmBootFileLoc1 = "\\res_mods\\configs\\xvm\\xvm.xc";
        private static string xvmBootFileLoc2 = "\\mods\\configs\\xvm\\xvm.xc";
        public static int totalModConfigComponents = 0;

        //logs string info to the log output
        public static void appendToLog(string info)
        {
            //the method should automaticly make the file if it's not there
            File.AppendAllText(Application.StartupPath + "\\RelHaxLog.txt", info + "\n");
        }

        //returns the md5 hash of the file based on the input file string location. It is searching in the database first. If not found in database or the filetime is not the same, it will create a new Hash and update the database
        public static string getMd5Hash(string inputFile)
        {
            // check if databse exists and if not, create it
            Utils.createMd5HashDatabase();
            // get filetime from file, convert it to string with base 10
            string tempFiletime = Convert.ToString(File.GetLastWriteTime(inputFile).ToFileTime(), 10);
            // extract filename with path
            string tempFilename = Path.GetFileName(inputFile);
            // check database for filename with filetime
            string tempHash = Utils.getMd5HashDatabase(tempFilename, tempFiletime);
            if (tempHash == "-1")   // file not found in database
            {
                // create Md5Hash from file
                tempHash = Utils.createMd5Hash(inputFile);

                if (tempHash == "-1")
                {
                    // no file found, then delete from database
                    Utils.deleteMd5HashDatabase(tempFilename);
                }
                else
                {
                    // file found. update the database with new values
                    Utils.updateMd5HashDatabase(tempFilename, tempHash, tempFiletime);
                }
                // report back the created Hash
                return tempHash;
            }
            else                    // Hash found in database
            {
                // report back the stored Hash
                return tempHash;
            }
        }

        //returns the md5 hash of the file based on the input file string location
        public static string createMd5Hash(string inputFile)
        {
            //first, return if the file does not exist
            if (!File.Exists(inputFile))
                return "-1";
            MD5 md5Hash = MD5.Create();
            // Convert the input string to a byte array and compute the hash.
            var stream = File.OpenRead(inputFile);
            byte[] data = md5Hash.ComputeHash(stream);
            stream.Close();
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            return sBuilder.ToString();
        }

        public static void createMd5HashDatabase()
        {
            if (!File.Exists(MainWindow.md5HashDatabaseXmlFile))
            {
                XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("database")
                );
                doc.Save(MainWindow.md5HashDatabaseXmlFile);
            }
        }

        // need filename and filetime to check the database
        public static string getMd5HashDatabase(string inputFile, string inputFiletime)
        {
            XDocument doc = XDocument.Load(MainWindow.md5HashDatabaseXmlFile);
            try
            {
                XElement element = doc.Descendants("file")
                   .Where(arg => arg.Attribute("filename").Value == inputFile && arg.Attribute("filetime").Value == inputFiletime)
                   .Single();
                return element.Attribute("md5").Value;
            }
            // catch the Exception if no entry is found
            catch (InvalidOperationException)
            {
                return "-1";
            }
        }

        public static void updateMd5HashDatabase(string inputFile, string inputMd5Hash, string inputFiletime)
        {
            XDocument doc = XDocument.Load(MainWindow.md5HashDatabaseXmlFile);
            try
            {
                XElement element = doc.Descendants("file")
                   .Where(arg => arg.Attribute("filename").Value == inputFile)
                   .Single();
                element.Attribute("filetime").Value = inputFiletime;
                element.Attribute("md5").Value = inputMd5Hash;
            }
            catch (InvalidOperationException)
            {
                doc.Element("database").Add(new XElement("file",
                    new XAttribute("filename", inputFile),
                    new XAttribute("filetime", inputFiletime),
                    new XAttribute("md5", inputMd5Hash)));
            }
            doc.Save(MainWindow.md5HashDatabaseXmlFile);
        }

        public static void deleteMd5HashDatabase(string inputFile)
        {
            // only for caution
            Utils.createMd5HashDatabase();

            // extract filename from path (if call with full path)
            string tempFilename = Path.GetFileName(inputFile);

            XDocument doc = XDocument.Load(MainWindow.md5HashDatabaseXmlFile);
            try
            {
                doc.Descendants("file")
                   .Where(arg => arg.Attribute("filename").Value == tempFilename)
                    .Remove();
                doc.Save(MainWindow.md5HashDatabaseXmlFile);
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }

        public static bool parseBool(string input, bool defaultValue)
        {
            bool returnVal;
            try
            {
                returnVal = bool.Parse(input);
            }
            catch (System.FormatException)
            {
                returnVal = defaultValue;
            }
            return returnVal;
        }
        public static int parseInt(string input, int defaultValue)
        {
            int returnVal;
            try
            {
                returnVal = int.Parse(input);
            }
            catch (System.FormatException)
            {
                returnVal = defaultValue;
            }
            return returnVal;
        }
        public static float parseFloat(string input, float defaultValue)
        {
            float returnVal;
            try
            {
                returnVal = float.Parse(input);
            }
            catch (System.FormatException)
            {
                returnVal = defaultValue;
            }
            return returnVal;
        }
        //parses the xml mod info into the memory database
        public static void createModStructure2(string databaseURL, bool backendFlag, List<Dependency> globalDependencies, List<Category> parsedCatagoryList)
        {
            totalModConfigComponents = 0;
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(databaseURL);
            }
            catch (XmlException)
            {
                Utils.appendToLog("CRITICAL: Failed to read database!");
                MessageBox.Show(Translations.getTranslatedString("databaseReadFailed"));
                Application.Exit();
            }
            catch (System.Net.WebException e)
            {
                Utils.appendToLog("EXCEPTION: WebException (call stack traceback)");
                Utils.appendToLog(e.StackTrace);
                Utils.appendToLog("inner message: " + e.Message);
                Utils.appendToLog("source: " + e.Source);
                Utils.appendToLog("target: " + e.TargetSite);
                Utils.appendToLog("Additional Info: Tried to access " + databaseURL);
                MessageBox.Show(Translations.getTranslatedString("databaseNotFound"));
                Application.Exit();
            }
            //add the global dependencies
            //globalDependencies = new List<Dependency>();
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
                        case "startAddress":
                            d.startAddress = globs.InnerText;
                            break;
                        case "endAddress":
                            d.endAddress = globs.InnerText;
                            break;
                        case "dependencyenabled":
                            d.enabled = Utils.parseBool(globs.InnerText, false);
                            break;
                    }
                }
                globalDependencies.Add(d);
            }
            XmlNodeList catagoryList = doc.SelectNodes("//modInfoAlpha.xml/catagories/catagory");
            //parsedCatagoryList = new List<Category>();
            foreach (XmlNode catagoryHolder in catagoryList)
            {
                Category cat = new Category();
                foreach (XmlNode catagoryNode in catagoryHolder.ChildNodes)
                {
                    switch (catagoryNode.Name)
                    {
                        case "name":
                            cat.name = catagoryNode.InnerText;
                            break;
                        case "selectionType":
                            cat.selectionType = catagoryNode.InnerText;
                            break;
                        case "mods":
                            foreach (XmlNode modHolder in catagoryNode.ChildNodes)
                            {
                                Mod m = new Mod();
                                foreach (XmlNode modNode in modHolder.ChildNodes)
                                {
                                    switch (modNode.Name)
                                    {
                                        case "name":
                                            m.name = modNode.InnerText;
                                            totalModConfigComponents++;
                                            break;
                                        case "version":
                                            m.version = modNode.InnerText;
                                            break;
                                        case "size":
                                            m.size = Utils.parseFloat(modNode.InnerText, 0.0f);
                                            break;
                                        case "zipFile":
                                            m.zipFile = modNode.InnerText;
                                            break;
                                        case "startAddress":
                                            m.startAddress = modNode.InnerText;
                                            break;
                                        case "endAddress":
                                            m.endAddress = modNode.InnerText;
                                            break;
                                        case "crc":
                                            m.crc = modNode.InnerText;
                                            break;
                                        case "enabled":
                                            m.enabled = Utils.parseBool(modNode.InnerText, false);
                                            break;
                                        case "description":
                                            m.description = modNode.InnerText;
                                            break;
                                        case "updateComment":
                                            m.updateComment = modNode.InnerText;
                                            break;
                                        case "devURL":
                                            m.devURL = modNode.InnerText;
                                            break;
                                        case "userDatas":
                                            foreach (XmlNode userDataNode in modNode.ChildNodes)
                                            {

                                                switch (userDataNode.Name)
                                                {
                                                    case "userData":
                                                        string innerText = userDataNode.InnerText;
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
                                            foreach (XmlNode pictureHolder in modNode.ChildNodes)
                                            {
                                                foreach (XmlNode pictureNode in pictureHolder.ChildNodes)
                                                {
                                                    switch (pictureNode.Name)
                                                    {
                                                        case "URL":
                                                            string innerText = pictureNode.InnerText;
                                                            if (innerText == null)
                                                                continue;
                                                            if (innerText.Equals(""))
                                                                continue;
                                                            m.pictureList.Add(new Picture(m.name, pictureNode.InnerText));
                                                            break;
                                                    }
                                                }
                                            }
                                            break;
                                        case "dependencies":
                                            //parse all dependencies
                                            foreach (XmlNode dependencyHolder in modNode.ChildNodes)
                                            {
                                                Dependency d = new Dependency();
                                                foreach (XmlNode dependencyNode in dependencyHolder.ChildNodes)
                                                {
                                                    switch (dependencyNode.Name)
                                                    {
                                                        case "dependencyZipFile":
                                                            d.dependencyZipFile = dependencyNode.InnerText;
                                                            break;
                                                        case "dependencyZipCRC":
                                                            d.dependencyZipCRC = dependencyNode.InnerText;
                                                            break;
                                                        case "startAddress":
                                                            d.startAddress = dependencyNode.InnerText;
                                                            break;
                                                        case "endAddress":
                                                            d.endAddress = dependencyNode.InnerText;
                                                            break;
                                                        case "dependencyenabled":
                                                            d.enabled = Utils.parseBool(dependencyNode.InnerText, false);
                                                            break;
                                                    }
                                                }
                                                m.dependencies.Add(d);
                                            }
                                            break;
                                        case "configs":
                                            //run the process configs method
                                            Utils.processConfigs(modNode, backendFlag, m, true);
                                            break;
                                    }
                                }
                                cat.mods.Add(m);
                            }
                            break;
                        case "dependencies":
                            //parse every config for that mod
                            foreach (XmlNode dependencyHolder in catagoryNode.ChildNodes)
                            {
                                Dependency d = new Dependency();
                                foreach (XmlNode dependencyNode in dependencyHolder.ChildNodes)
                                {
                                    switch (dependencyNode.Name)
                                    {
                                        case "dependencyZipFile":
                                            d.dependencyZipFile = dependencyNode.InnerText;
                                            break;
                                        case "dependencyZipCRC":
                                            d.dependencyZipCRC = dependencyNode.InnerText;
                                            break;
                                        case "startAddress":
                                            d.startAddress = dependencyNode.InnerText;
                                            break;
                                        case "endAddress":
                                            d.endAddress = dependencyNode.InnerText;
                                            break;
                                        case "dependencyenabled":
                                            d.enabled = Utils.parseBool(dependencyNode.InnerText, false);
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
        //recursivly processes the configs
        public static void processConfigs(XmlNode holder, bool backendFlag, Mod m, bool parentIsMod, Config con = null)
        {
            //parse every config for that mod
            foreach (XmlNode configHolder in holder.ChildNodes)
            {
                Config c = new Config();
                foreach (XmlNode configNode in configHolder.ChildNodes)
                {
                    switch (configNode.Name)
                    {
                        case "name":
                            c.name = configNode.InnerText;
                            //totalModConfigComponents++;
                            break;
                        case "version":
                            c.version = configNode.InnerText;
                            break;
                        case "zipFile":
                            c.zipFile = configNode.InnerText;
                            break;
                        case "startAddress":
                            c.startAddress = configNode.InnerText;
                            break;
                        case "endAddress":
                            c.endAddress = configNode.InnerText;
                            break;
                        case "crc":
                            c.crc = configNode.InnerText;
                            break;
                        case "enabled":
                            c.enabled = Utils.parseBool(configNode.InnerText, false);
                            break;
                        case "index":
                            c.index = Utils.parseInt(configNode.InnerText, 0);
                            break;
                        case "size":
                            c.size = Utils.parseFloat(configNode.InnerText, 0.0f);
                            break;
                        case "updateComment":
                            c.updateComment = configNode.InnerText;
                            break;
                        case "description":
                            c.description = configNode.InnerText;
                            break;
                        case "devURL":
                            c.devURL = configNode.InnerText;
                            break;
                        case "type":
                            c.type = configNode.InnerText;
                            break;
                        case "configs":
                            Utils.processConfigs(configNode, backendFlag, m, false, c);
                            break;
                        case "userDatas":
                            foreach (XmlNode userDataNode in configNode.ChildNodes)
                            {

                                switch (userDataNode.Name)
                                {
                                    case "userData":
                                        string innerText = userDataNode.InnerText;
                                        if (innerText == null)
                                            continue;
                                        if (innerText.Equals(""))
                                            continue;
                                        c.userFiles.Add(innerText);
                                        break;
                                }

                            }
                            break;
                        case "pictures":
                            //parse every picture
                            foreach (XmlNode pictureHolder in configNode.ChildNodes)
                            {
                                foreach (XmlNode pictureNode in pictureHolder.ChildNodes)
                                {
                                    switch (pictureNode.Name)
                                    {
                                        case "URL":
                                            string innerText = pictureNode.InnerText;
                                            if (innerText == null)
                                                continue;
                                            if (innerText.Equals(""))
                                                continue;
                                            c.pictureList.Add(new Picture(c.name, pictureNode.InnerText));
                                            break;
                                    }
                                }
                            }
                            break;
                        case "dependencies":
                            //parse all dependencies
                            foreach (XmlNode dependencyHolder in configNode.ChildNodes)
                            {
                                Dependency d = new Dependency();
                                foreach (XmlNode dependencyNode in dependencyHolder.ChildNodes)
                                {
                                    switch (dependencyNode.Name)
                                    {
                                        case "dependencyZipFile":
                                            d.dependencyZipFile = dependencyNode.InnerText;
                                            break;
                                        case "dependencyZipCRC":
                                            d.dependencyZipCRC = dependencyNode.InnerText;
                                            break;
                                        case "startAddress":
                                            d.startAddress = dependencyNode.InnerText;
                                            break;
                                        case "endAddress":
                                            d.endAddress = dependencyNode.InnerText;
                                            break;
                                        case "dependencyenabled":
                                            d.enabled = Utils.parseBool(dependencyNode.InnerText, false);
                                            break;
                                    }
                                }
                                m.dependencies.Add(d);
                            }
                            break;
                    }
                }
                //attach it to eithor the config of correct level or the mod
                if (parentIsMod)
                    m.configs.Add(c);
                else
                    con.configs.Add(c);
            }
        }
        //checks for duplicates
        public static bool duplicates(List<Category> parsedCatagoryList)
        {
            //add every mod name to a new list
            List<string> modNameList = new List<string>();
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    modNameList.Add(m.name);
                }
            }
            //itterate through every mod name again
            foreach (Category c in parsedCatagoryList)
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
        //returns the mod based on catagory and mod name
        public static Mod linkMod(string modName, string catagoryName, List<Category> parsedCatagoryList)
        {
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (c.name.Equals(catagoryName) && m.name.Equals(modName))
                    {
                        //found it
                        return m;
                    }
                }
            }
            return null;
        }
        //returns the mod based and mod name
        public static Mod linkMod(string modName, List<Category> parsedCatagoryList)
        {
            foreach (Category c in parsedCatagoryList)
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
        //returns the catagory based on the catagory name
        public static Category getCatagory(string catName, List<Category> parsedCatagoryList)
        {
            foreach (Category c in parsedCatagoryList)
            {
                if (c.name.Equals(catName)) return c;
            }
            return null;
        }
        //gets the user mod based on it's name
        public static Mod getUserMod(string modName, List<Mod> userMods)
        {
            foreach (Mod m in userMods)
            {
                if (m.name.Equals(modName))
                {
                    return m;
                }
            }
            return null;
        }
        //sorts a list of mods alphabetaicaly
        public static void sortModsList(List<Mod> modList)
        {
            //sortModsList
            modList.Sort(Mod.CompareMods);
        }
        //sorte a list of catagoris alphabetaicaly
        public static void sortCatagoryList(List<Category> catagoryList)
        {
            catagoryList.Sort(Category.CompareCatagories);
        }
        //sorts a list of pictures by mod or config, then name
        public static List<Picture> sortPictureList(List<Picture> pictureList)
        {
            //don't actually sort them anymore
            //they will not apprea in the order of which they were loaded from the xml file
            return pictureList;
        }
        //method to patch a part of an xml file
        //fileLocation is relative to res_mods folder
        public static void xmlPatch(string filePath, string xpath, string mode, string search, string replace, string tanksLocation, string tanksVersion, bool testMods = false, string testXVMBootLoc = "")
        {
            if (Regex.IsMatch(filePath, "^\\\\\\\\res_mods"))
            {
                //new style patch, res_mods folder
                filePath = tanksLocation + filePath;
            }
            else if (Regex.IsMatch(filePath, "^\\\\\\\\mods"))
            {
                //new style patch, mods folder
                filePath = tanksLocation + filePath;
            }
            else if (testMods)
            {

            }
            else
            {
                //old style patch
                filePath = tanksLocation + "\\res_mods" + filePath;
            }

            if (!testMods && Regex.IsMatch(filePath, "versiondir"))
            {
                //patch versiondir out of filePath
                filePath = Regex.Replace(filePath, "versiondir", tanksVersion);
            }
            //patch xvmConfigFolderName out of fileLocation
            if (!testMods && Regex.IsMatch(filePath, "xvmConfigFolderName"))
            {
                string s = getXVMBootLoc(tanksLocation);
                if (s != null)
                    filePath = Regex.Replace(filePath, "xvmConfigFolderName", s);
            }
            else
            {
                //patch check mode, try to get boot xvm file from the xvm boot textbox
                if (testXVMBootLoc.Equals("") && Regex.IsMatch(filePath, "xvmConfigFolderName"))
                {
                    MessageBox.Show("Attempted to use variable \"xvmConfigFolderName\", but nothing in the xvm boot file location text box");
                    return;
                }
            }

            //verify the file exists...
            if (!File.Exists(filePath))
                return;
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            //check to see if it has the header info at the top to see if we need to remove it later
            bool hadHeader = false;
            XmlDocument doc3 = new XmlDocument();
            doc3.Load(filePath);
            foreach (XmlNode node in doc3)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    hadHeader = true;
                }
            }
            //determines which version of pathing will be done
            switch (mode)
            {
                case "add":
                    //check to see if it's already there
                    string[] tempp = replace.Split('/');
                    string tempPath = xpath;
                    //make the full node path
                    for (int i = 0; i < tempp.Count() - 1; i++)
                    {
                        tempPath = tempPath + "/" + tempp[i];
                    }
                    XmlNodeList currentSoundBanksAdd = doc.SelectNodes(tempPath);
                    //in each node check if the element exist with the replace innerText
                    foreach (XmlElement e in currentSoundBanksAdd)
                    {
                        string innerText = tempp[tempp.Count() - 1];
                        //remove any tabs and whitespaces first
                        innerText = Regex.Replace(innerText, @"\t", "");
                        innerText = innerText.Trim();
                        if (e.InnerText.Equals(innerText))
                            return;
                    }
                    //get to the node where to add the element
                    XmlNode reff = doc.SelectSingleNode(xpath);
                    //create node(s) to add to the element
                    string[] temp = replace.Split('/');
                    List<XmlElement> nodes = new List<XmlElement>();
                    for (int i = 0; i < temp.Count() - 1; i++)
                    {
                        XmlElement ele = doc.CreateElement(temp[i]);
                        if (i == temp.Count() - 2)
                        {
                            //last node with actual data to add
                            ele.InnerText = temp[temp.Count() - 1];
                        }
                        nodes.Add(ele);
                    }
                    //add nodes to the element in reverse for hierarchy order
                    for (int i = nodes.Count - 1; i > -1; i--)
                    {
                        if (i == 0)
                        {
                            //getting here means this is the highmost node
                            //that needto be modified
                            reff.InsertAfter(nodes[i], reff.FirstChild);
                            break;
                        }
                        XmlElement parrent = nodes[i - 1];
                        XmlElement child = nodes[i];
                        parrent.InsertAfter(child, parrent.FirstChild);
                    }
                    //save it
                    if (File.Exists(filePath)) File.Delete(filePath);
                    doc.Save(filePath);
                    break;

                case "edit":
                    //check to see if it's already there
                    XmlNodeList currentSoundBanksEdit = doc.SelectNodes(xpath);
                    foreach (XmlElement e in currentSoundBanksEdit)
                    {
                        string innerText = e.InnerText;
                        innerText = Regex.Replace(innerText, "\t", "");
                        innerText = innerText.Trim();
                        if (e.InnerText.Equals(replace))
                            return;
                    }
                    //find and replace
                    XmlNodeList rel1Edit = doc.SelectNodes(xpath);
                    foreach (XmlElement eee in rel1Edit)
                    {
                        if (Regex.IsMatch(eee.InnerText, search))
                        {
                            eee.InnerText = replace;
                        }
                    }
                    //save it
                    if (File.Exists(filePath)) File.Delete(filePath);
                    doc.Save(filePath);
                    break;

                case "remove":
                    //check to see if it's there
                    XmlNodeList currentSoundBanksRemove = doc.SelectNodes(xpath);
                    foreach (XmlElement e in currentSoundBanksRemove)
                    {
                        if (Regex.IsMatch(e.InnerText, search))
                        {
                            e.RemoveAll();
                        }
                    }
                    //save it
                    if (File.Exists(filePath)) File.Delete(filePath);
                    doc.Save(filePath);
                    //remove empty elements
                    XDocument doc2 = XDocument.Load(filePath);
                    doc2.Descendants().Where(e => string.IsNullOrEmpty(e.Value)).Remove();
                    if (File.Exists(filePath)) File.Delete(filePath);
                    doc2.Save(filePath);
                    break;
            }
            //check to see if we need to remove the header
            bool hasHeader = false;
            XmlDocument doc5 = new XmlDocument();
            doc5.Load(filePath);
            foreach (XmlNode node in doc5)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    hasHeader = true;
                }
            }
            //if not had header and has header, remove header
            //if had header and has header, no change
            //if not had header and not has header, no change
            //if had header and not has header, no change
            if (!hadHeader && hasHeader)
            {
                XmlDocument doc4 = new XmlDocument();
                doc4.Load(filePath);
                foreach (XmlNode node in doc4)
                {
                    if (node.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        doc4.RemoveChild(node);
                    }
                }
                doc4.Save(filePath);
            }
        }
        //method to patch a standard text or json file
        //fileLocation is relative to res_mods folder
        public static void RegxPatch(string fileLocation, string search, string replace, string tanksLocation, string tanksVersion, int lineNumber = 0, bool testMods = false, string testXVMBootLoc = "")
        {
            if (Regex.IsMatch(fileLocation, "^\\\\\\\\res_mods"))
            {
                //new style patch, res_mods folder
                fileLocation = tanksLocation + fileLocation;
            }
            else if (Regex.IsMatch(fileLocation, "^\\\\\\\\mods"))
            {
                //new style patch, mods folder
                fileLocation = tanksLocation + fileLocation;
            }
            else if (testMods)
            {

            }
            else
            {
                //old style patch
                fileLocation = tanksLocation + "\\res_mods" + fileLocation;
            }

            if (!testMods && Regex.IsMatch(fileLocation, "versiondir"))
            {
                //patch versiondir out of fileLocation
                fileLocation = Regex.Replace(fileLocation, "versiondir", tanksVersion);
            }
            //patch xvmConfigFolderName out of fileLocation
            if (!testMods && Regex.IsMatch(fileLocation, "xvmConfigFolderName"))
            {
                string s = getXVMBootLoc(tanksLocation);
                if (s != null)
                    fileLocation = Regex.Replace(fileLocation, "xvmConfigFolderName", s);
            }
            else
            {
                //patch check mode, try to get boot xvm file from the xvm boot textbox
                if (testXVMBootLoc.Equals("") && Regex.IsMatch(fileLocation, "xvmConfigFolderName"))
                {
                    MessageBox.Show("Attempted to use variable \"xvmConfigFolderName\", but nothing in the xvm boot file location text box");
                    return;
                }
            }

            //check that the file exists
            if (!File.Exists(fileLocation))
                return;

            //load file from disk...
            string file = File.ReadAllText(fileLocation);
            //parse each line into an index array
            string[] fileParsed = file.Split('\n');
            StringBuilder sb = new StringBuilder();
            if (lineNumber == 0)
            //search entire file and replace each instance
            {
                for (int i = 0; i < fileParsed.Count(); i++)
                {
                    if (Regex.IsMatch(fileParsed[i], search))
                    {
                        fileParsed[i] = Regex.Replace(fileParsed[i], search, replace);
                        fileParsed[i] = Regex.Replace(fileParsed[i], "newline", "\n");
                    }
                    sb.Append(fileParsed[i] + "\n");
                }
            }
            else if (lineNumber == -1)
            //search entire file and string and make one giant regex replacement
            {
                //but remove newlines first
                file = Regex.Replace(file, "\n", "newline");
                try
                {
                    if (Regex.IsMatch(file, search))
                    {
                        file = Regex.Replace(file, search, replace);
                    }
                    file = Regex.Replace(file, "newline", "\n");
                    sb.Append(file);
                }
                catch (ArgumentException)
                {
                    if (testMods) MessageBox.Show("invalid regex command");
                }
            }
            else
            {
                for (int i = 0; i < fileParsed.Count(); i++)
                {
                    if (i == lineNumber - 1)
                    {
                        string value = fileParsed[i];
                        if (Regex.IsMatch(value, search))
                        {
                            fileParsed[i] = Regex.Replace(fileParsed[i], search, replace);
                        }
                    }
                    sb.Append(fileParsed[i] + "\n");
                }
            }
            //save the file back into the string and then the file
            file = sb.ToString();
            File.WriteAllText(fileLocation, file);
        }
        //method to parse json files
        public static void jsonPatch(string jsonFile, string jsonPath, string newValue, string mode, string tanksLocation, string tanksVersion, bool testMods = false, string testXVMBootLoc = "")
        {
            //try to convert the new value to a bool or an int or double first
            bool newValueBool = false;
            int newValueInt = -69420;
            double newValueDouble = -69420.0d;
            bool useBool = false;
            bool useInt = false;
            bool useDouble = false;
            //try a bool first, only works with "true" and "false"
            try
            {
                newValueBool = bool.Parse(newValue);
                useBool = true;
                useInt = false;
                useDouble = false;
            }
            catch (FormatException)
            { }
            //try a double nixt. it will parse a double and int. at this point it could be eithor
            try
            {
                newValueDouble = double.Parse(newValue);
                useDouble = true;
            }
            catch (FormatException)
            { }
            //try an int next. if it works than turn double to false and int to true
            try
            {
                newValueInt = int.Parse(newValue);
                useInt = true;
                useDouble = false;
            }
            catch (FormatException)
            { }
            //check if it's the new structure
            if (Regex.IsMatch(jsonFile, "^\\\\\\\\res_mods"))
            {
                //new style patch, res_mods folder
                jsonFile = tanksLocation + jsonFile;
            }
            else if (Regex.IsMatch(jsonFile, "^\\\\\\\\mods"))
            {
                //new style patch, mods folder
                jsonFile = tanksLocation + jsonFile;
            }
            else if (testMods)
            {

            }
            else
            {
                //old style patch
                jsonFile = tanksLocation + "\\res_mods" + jsonFile;
            }

            //patch versiondir out of fileLocation
            if (!testMods && Regex.IsMatch(jsonFile, "versiondir"))
            {
                jsonFile = Regex.Replace(jsonFile, "versiondir", tanksVersion);
            }
            //patch xvmConfigFolderName out of fileLocation
            if (!testMods && Regex.IsMatch(jsonFile, "xvmConfigFolderName"))
            {
                string s = getXVMBootLoc(tanksLocation);
                if (s != null)
                    jsonFile = Regex.Replace(jsonFile, "xvmConfigFolderName", s);
            }
            else
            {
                //patch check mode, try to get boot xvm file from the xvm boot textbox
                if (testXVMBootLoc.Equals("") && Regex.IsMatch(jsonFile, "xvmConfigFolderName"))
                {
                    MessageBox.Show("Attempted to use variable \"xvmConfigFolderName\", but nothing in the xvm boot file location text box");
                    return;
                }
            }

            //check that the file exists
            if (!File.Exists(jsonFile))
                return;

            //load file from disk...
            string file = File.ReadAllText(jsonFile);
            //save the "$" lines
            List<StringSave> ssList = new List<StringSave>();
            StringBuilder backTogether = new StringBuilder();
            string[] removeComments = file.Split('\n');
            for (int i = 0; i < removeComments.Count(); i++)
            {
                string temp = removeComments[i];
                //determine if it has (had) a comma at the end of the string
                bool hadComma = false;
                bool modified = false;
                if (Regex.IsMatch(temp, @",[ \t\r]*$"))
                    hadComma = true;

                //determine if it is a illegal refrence in jarray or jobject
                StringSave ss = new StringSave();
                if (Regex.IsMatch(temp, @"^[ \t]*\"".*\"" *: *\$\{ *\"".*\""\ *}"))
                {
                    modified = true;
                    //jobject
                    ss.name = temp.Split('"')[1];
                    ss.value = temp.Split('$')[1];
                    ssList.Add(ss);
                    temp = "\"" + ss.name + "\"" + ": -69420";
                }
                else if (Regex.IsMatch(temp, @"^[ \t]*\$ *\{ *\"".*\"" *\}"))
                {
                    modified = true;
                    //jarray
                    string comment = "//\"comment_Willster419\"";
                    temp = comment + temp;
                    ss.name = temp.Split('"')[1];
                    ss.value = temp.Split('$')[1];
                    ssList.Add(ss);
                    temp = "-42069";
                }
                else if (Regex.IsMatch(temp, @"^[ \t]*""\$ref"" *: *{.*}"))
                {
                    modified = true;
                    //jobject
                    ss.name = temp.Split('"')[1];
                    ss.value = temp.Split('{')[1];
                    ssList.Add(ss);
                    temp = "\"" + "willster419_refReplace" + "\"" + ": -6969";
                }
                if (hadComma && modified)
                    temp = temp + ",";
                backTogether.Append(temp + "\n");
            }
            file = backTogether.ToString();
            JsonLoadSettings settings = new JsonLoadSettings();
            settings.CommentHandling = CommentHandling.Ignore;
            JObject root = null;
            //load json for editing
            try
            {
                root = JObject.Parse(file, settings);
            }
            catch (JsonReaderException j)
            {
                Utils.appendToLog("ERROR: Failed to patch " + jsonFile);
                if (Program.testMode)
                {
                    //in test mode this is worthy of an EXCEPTION
                    throw new JsonReaderException(j.Message);
                }
            }
            //if it failed to parse show the message (above) and pull out
            if (root == null)
                return;
            if (mode == null || mode.Equals("") || mode.Equals("edit") || mode.Equals("arrayEdit"))
            {
                //the actual patch method
                JValue newObject = (JValue)root.SelectToken(jsonPath);
                //JValue newObject = null;
                //pull out if it failed to get the selection
                if (newObject == null)
                {
                    Utils.appendToLog("ERROR: path " + jsonPath + " not found for " + Path.GetFileName(jsonFile));
                }
                else if (useBool)
                {
                    newObject.Value = newValueBool;
                }
                else if (useInt)
                {
                    newObject.Value = newValueInt;
                }
                else if (useDouble)
                {
                    newObject.Value = newValueDouble;
                }
                else //string
                {
                    newObject.Value = newValue;
                }
            }
            else if (mode.Equals("remove"))
            {
                //TODO
            }
            else if (mode.Equals("arrayRemove"))
            {
                //TODO
            }
            else if (mode.Equals("add"))
            {
                //TODO
            }
            else if (mode.Equals("arrayAdd"))
            {
                //TODO
            }
            else
            {
                Utils.appendToLog("ERROR: Unknown json patch mode, " + mode);
            }
            StringBuilder rebuilder = new StringBuilder();
            string[] putBackDollas = root.ToString().Split('\n');
            for (int i = 0; i < putBackDollas.Count(); i++)
            {
                string temp = putBackDollas[i];
                if (Regex.IsMatch(temp, "-69420"))//look for the temp value
                {
                    //array of string save and text file are in sync, so when one is found,
                    //take it from index 0 and remove from the list at index 0
                    temp = "\"" + ssList[0].name + "\"" + ": $" + ssList[0].value;
                    putBackDollas[i] = temp;//update the text file file
                    ssList.RemoveAt(0);//remove the entry from the list of entries to fix/replace
                }
                else if (Regex.IsMatch(temp, "-42069"))
                {
                    temp = "$" + ssList[0].value;
                    putBackDollas[i] = temp;
                    ssList.RemoveAt(0);
                }
                else if (Regex.IsMatch(temp, "willster419_refReplace"))
                {
                    temp = "\"" + ssList[0].name + "\"" + ": {" + ssList[0].value;
                    putBackDollas[i] = temp;
                    ssList.RemoveAt(0);
                }
                rebuilder.Append(putBackDollas[i] + "\n");
            }
            if (ssList.Count != 0)
                Utils.appendToLog("There was an error with patching the file " + jsonFile + ", with extra refrences");
            File.WriteAllText(jsonFile, rebuilder.ToString());
        }
        public static void pmodPatch(string bootFile, string xvmPath, string search, string newValue, string mode, string tanksLocation, string tanksVersion, bool testMods = false, string testXVMBootLoc = "")
        {
            numByteReads = 0;
            patchDone = false;
            genericTraverse = 0;
            //check if it's the new structure
            if (Regex.IsMatch(bootFile, "^\\\\\\\\res_mods"))
            {
                //new style patch, res_mods folder
                bootFile = tanksLocation + bootFile;
            }
            else if (Regex.IsMatch(bootFile, "^\\\\\\\\mods"))
            {
                //new style patch, mods folder
                bootFile = tanksLocation + bootFile;
            }
            else if (testMods)
            {

            }
            else
            {
                //old style patch
                bootFile = tanksLocation + "\\res_mods" + bootFile;
            }

            //patch versiondir out of fileLocation
            if (!testMods && Regex.IsMatch(bootFile, "versiondir"))
            {
                bootFile = Regex.Replace(bootFile, "versiondir", tanksVersion);
            }

            //check that the file exists
            if (!File.Exists(bootFile))
                return;
            //break down the path into an array
            string[] pathArrayy = xvmPath.Split('.');
            List<string> pathArray = new List<string>();
            //convert it to a List cause it has more features
            foreach (string s in pathArrayy)
                pathArray.Add(s);
            //load the file from disk
            numByteReads = 0;
            readInside(pathArray, bootFile, newValue, search, mode, xvmPath);
        }

        public static void xvmPatch(string bootFile, string xvmPath, string search, string newValue, string mode, string tanksLocation, string tanksVersion, bool testMods = false, string testXVMBootLoc = "")
        {
            numByteReads = 0;
            patchDone = false;
            genericTraverse = 0;
            //check if it's the new structure
            if (Regex.IsMatch(bootFile, "^\\\\\\\\res_mods"))
            {
                //new style patch, res_mods folder
                bootFile = tanksLocation + bootFile;
            }
            else if (Regex.IsMatch(bootFile, "^\\\\\\\\mods"))
            {
                //new style patch, mods folder
                bootFile = tanksLocation + bootFile;
            }
            else if (testMods)
            {

            }
            else
            {
                //old style patch
                bootFile = tanksLocation + "\\res_mods" + bootFile;
            }

            //patch versiondir out of fileLocation
            if (!testMods && Regex.IsMatch(bootFile, "versiondir"))
            {
                bootFile = Regex.Replace(bootFile, "versiondir", tanksVersion);
            }
            //patch xvmConfigFolderName out of fileLocation
            if (!testMods && Regex.IsMatch(bootFile, "xvmConfigFolderName"))
            {
                string s = getXVMBootLoc(tanksLocation);
                if (s != null)
                    bootFile = Regex.Replace(bootFile, "xvmConfigFolderName", s);
            }
            else
            {
                //patch check mode, try to get boot xvm file from the xvm boot textbox
                if (testXVMBootLoc.Equals("") && Regex.IsMatch(bootFile, "xvmConfigFolderName"))
                {
                    MessageBox.Show("Attempted to use variable \"xvmConfigFolderName\", but nothing in the xvm boot file location text box");
                    return;
                }
            }

            //check that the file exists
            if (!File.Exists(bootFile))
                return;
            //break down the path into an array
            string[] pathArrayy = xvmPath.Split('.');
            List<string> pathArray = new List<string>();
            //convert it to a List cause it has more features
            foreach (string s in pathArrayy)
                pathArray.Add(s);
            //create the stringBuilder to rewrite the file
            StringBuilder sb = new StringBuilder();
            //load the file from disk

            //read untill *start of string*${
            numByteReads = 0;
            string fileContents = File.ReadAllText(bootFile);
            fileContents = Regex.Replace(fileContents, @"\/\*.*\*\/", "", RegexOptions.Singleline);
            string[] removeComments = fileContents.Split('\n');
            StringBuilder bootBuilder = new StringBuilder();
            foreach (string s in removeComments)
            {
                if (Regex.IsMatch(s, @"\/\/.*$"))
                    continue;
                bootBuilder.Append(s + "\n");
            }
            fileContents = bootBuilder.ToString();
            readUntill(fileContents, sb, @"^[ \t]*\$[ \t]*{[ \t]*""");
            //now read untill the next quote for the temp path
            string filePath = readUntill(fileContents, sb, "\"");
            //flip the folder path things
            filePath = Regex.Replace(filePath, "/", "\\");
            //remove the last one
            filePath = filePath.Substring(0, filePath.Length - 1);
            filePath = filePath.Trim();
            readInside(pathArray, Path.GetDirectoryName(bootFile) + "\\" + filePath, newValue, search, mode, xvmPath);
        }

        //getting into this means that we've started reading a new config file, maybe patch this one?
        private static void readInside(List<string> pathArray, string newFilePath, string replaceValue, string search, string mode, string origXvmPath)
        {
            numByteReads = 0;
            //create the (new) stringBuilder to rewrite the file
            StringBuilder sb = new StringBuilder();
            bool isToEnd = false;
            //load the file from disk
            string fileContents = File.ReadAllText(newFilePath);
            while (pathArray.Count != 0)
            {
                string regex = "";
                bool isArrayIndex = false;
                //check if the patharray has array index in it
                if (Regex.IsMatch(pathArray[0], @"\["))
                {
                    regex = @"[ \t]*""" + pathArray[0].Split('[')[0] + "\"[ \t]*:";
                    isArrayIndex = true;
                }
                else
                {
                    regex = @"[ \t]*""" + pathArray[0] + "\"[ \t]*:";
                }
                //read untill the value we want
                if (readUntill(fileContents, sb, regex) == "null")
                {
                    Utils.appendToLog("ERROR: Path not found: " + origXvmPath);
                    return;
                }
                //determine if the this value is actually a file refrence
                string refrenceTest = peekUntill(fileContents, @"[\[,}\]]$");
                if (Regex.IsMatch(refrenceTest, @"\${[ \t]*""") && !isArrayIndex)
                {
                    parseRefrence1(pathArray, newFilePath, replaceValue, search, mode, origXvmPath, fileContents, sb);
                }
                //determine if it is the other type of refrence
                string refrenceTest2 = peekUntill(fileContents, @"[\[,}\]]$");
                if (Regex.IsMatch(refrenceTest2, @"""\$ref"":") && !isArrayIndex)
                {
                    parseRefrence2(pathArray, newFilePath, replaceValue, search, mode, origXvmPath, fileContents, sb);
                }
                //determine if it is an array
                isToEnd = false;
                if (isArrayIndex)
                {
                    int indexToReadTo = 0;
                    //boolean flag for it you want to get to the end of the jarray
                    isToEnd = false;
                    //split the array into an array lol
                    if (readUntill(fileContents, sb, @"\[") == "null")
                    {
                        Utils.appendToLog("ERROR: Path not found: " + origXvmPath);
                        return;
                    }
                    string arrayContents = peekUntill(fileContents, @"\]");
                    //split the array based on "},"
                    List<string> carray = split(arrayContents, @"[,}]$");
                    //if it is an index, just use it
                    if (Regex.IsMatch(pathArray[0], @"\[\d\]+"))
                    {
                        string splitter = pathArray[0].Split('[')[1];
                        splitter = splitter.Split(']')[0];
                        //also repair the path
                        pathArray[0] = Regex.Replace(pathArray[0], "endIndex", "");
                        indexToReadTo = int.Parse(splitter);
                        if (indexToReadTo < 0 || indexToReadTo >= carray.Count())
                        {
                            //error and abort
                            Utils.appendToLog("invalid index: " + pathArray[0]);
                            return;
                        }
                    }
                    else if (Regex.IsMatch(pathArray[0], @"\[-1\]+"))
                    {
                        //-1 keyword for the add array method
                        if(!mode.Equals("array_add"))
                        {
                            Utils.appendToLog("To use -1 keyword, must be in array_add mode!");
                            return;
                        }
                        //set the flag and reset the values
                        indexToReadTo = carray.Count - 1;
                        isToEnd = true;
                    }
                    //if it is a search, 
                    else
                    {
                        //grab the regex search out of it an repair the array
                        //figure out how broken (how many "." are part of regex
                        string borken1 = "";
                        foreach (string s in pathArray)
                            borken1 = borken1 + s + ".";
                        genericTraverse = 0;
                        readUntillGeneric(borken1, @"\[");
                        genericTraverse = 0;
                        string anotherString2 = readUntillGeneric(borken1, @"\]endIndex");
                        genericTraverse = 0;
                        pathArray[0] = anotherString2;
                        //figure out how many indexes got rekt
                        string[] anotherTemp = pathArray[0].Split('.');
                        int numIndexesLost = anotherTemp.Count() - 1;
                        for (int i = 1; i != numIndexesLost + 1; i++)
                        {
                            pathArray.RemoveAt(1);
                        }
                        string[] vals = pathArray[0].Split('[');
                        int origLength = vals[0].Length;
                        origLength++;
                        string splitter = pathArray[0];
                        splitter = splitter.Substring(origLength);
                        splitter = splitter.Substring(0, splitter.Length - 9);
                        //search each index for the regex match
                        //for loop it and check at the end if it went all the way through
                        for (indexToReadTo = 0; indexToReadTo < carray.Count(); indexToReadTo++)
                        {
                            string carrayVal = carray[indexToReadTo];
                            if (Regex.IsMatch(carrayVal, splitter))
                            {
                                break;
                            }
                        }
                        if (indexToReadTo < 0 || indexToReadTo >= carray.Count())
                        {
                            //error and abort
                            Utils.appendToLog("invalid index: " + pathArray[0]);
                            return;
                        }
                    }
                    //ok now we have a valid index to actually make the change in the requested index
                    string advanceTo = "";
                    for (int i = 0; i < indexToReadTo; i++)
                    {
                        advanceTo = advanceTo + carray[i];
                    }
                    if (isToEnd)
                    {
                        advanceTo = advanceTo + carray[carray.Count - 1];
                    }
                    //get it to right before the desired index starts
                    readUntill(fileContents, sb, advanceTo.Count());
                    //determine if the this value is actually a file refrence
                    string refrenceTest5 = peekUntill(fileContents, @"[,}\]]$");
                    if (Regex.IsMatch(refrenceTest5, @"\${[ \t]*"""))
                    {
                        if (mode.Equals("add") || mode.Equals("edit"))
                            parseRefrence1(pathArray, newFilePath, replaceValue, search, mode, origXvmPath, fileContents, sb);
                    }
                }
                //we found it so remove it from the path
                if (pathArray.Count != 0)
                    pathArray.RemoveAt(0);
            }
            //split off into the cases for different xvm modification types
            if (!patchDone)
            {
                patchDone = true;
                switch (mode)
                {
                    default:
                        //do nothing
                        Utils.appendToLog("Invalid mode: " + mode + " for xvm patch " + origXvmPath);
                        break;
                    case "edit":
                        xvmEdit(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                    case "add":
                        xvmAdd(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                    case "remove":
                        //TODO
                        break;
                    case "array_clear":
                        xvmArrayClear(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                    case "array_add":
                        xvmArrayAdd(fileContents, sb, newFilePath, replaceValue, search, isToEnd);
                        break;
                    case "array_edit":
                        xvmArrayEdit(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                    case "array_remove":
                        xvmArrayRemove(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                }
            }
        }
        //parse the xvm style refrence
        private static void parseRefrence1(List<string> pathArray, string newFilePath, string replaceValue, string search, string mode, string origXvmPath, string fileContents, StringBuilder sb)
        {
            //it's a refrence, move it to the next file and readInsideEdit (yes recursion)
            if (readUntill(fileContents, sb, @"\${[ \t]*""") == "null")
            {
                Utils.appendToLog("ERROR: Path not found: " + origXvmPath);
                return;
            }
            //now read untill the next quote for the temp path
            string filePath = readUntill(fileContents, sb, "\"");
            //check to see if it is only a refrence withen the same file
            string test = peekUntill(fileContents, "}");
            if (Regex.IsMatch(test, ":"))
            {
                //new file refrence
                //read untill the next quote
                readUntill(fileContents, sb, "\"");
                string tempPath = readUntill(fileContents, sb, "\"");
                tempPath = tempPath.Substring(0, tempPath.Length - 1);
                string[] newPathSplit = tempPath.Split('.');
                pathArray[0] = newPathSplit[0];
                for (int i = 1; i < newPathSplit.Count(); i++)
                    pathArray.Insert(1, newPathSplit[i]);

                //flip the folder path things
                filePath = Regex.Replace(filePath, "/", "\\");
                //remove the last one
                filePath = filePath.Substring(0, filePath.Length - 1);
                filePath = filePath.Trim();
                readInside(pathArray, Path.GetDirectoryName(newFilePath) + "\\" + filePath, replaceValue, search, mode, origXvmPath);
            }
            else if (Regex.IsMatch(filePath, @"\.(json|xc)"))
            {
                //new file
                string tempPath = filePath.Substring(0, filePath.Length - 1);
                pathArray.RemoveAt(0);
                readInside(pathArray, Path.GetDirectoryName(newFilePath) + "\\" + tempPath, replaceValue, search, mode, origXvmPath);
            }
            else
            {
                //same file refrence
                //EXCEPT it could eithor be a new file refrence
                //3 types of refrences
                //"file.json"
                //"path.within.file
                //"file.json":"path.to.new.ting"
                string[] newPathSplit = filePath.Split('.');
                pathArray[0] = newPathSplit[0];
                for (int i = 1; i < newPathSplit.Count(); i++)
                    pathArray.Insert(1, newPathSplit[i]);
                readInside(pathArray, newFilePath, replaceValue, search, mode, origXvmPath);
            }

        }
        //parse the ref style refrence
        private static void parseRefrence2(List<string> pathArray, string newFilePath, string replaceValue, string search, string mode, string origXvmPath, string fileContents, StringBuilder sb)
        {
            //ref style refrence
            if (readUntill(fileContents, sb, @"""\$ref"":") == "null")
            {
                Utils.appendToLog("ERROR: Path not found: " + origXvmPath);
                return;
            }
            readUntill(fileContents, sb, ":");
            readUntill(fileContents, sb, "\"");
            string filePath = readUntill(fileContents, sb, "\"");
            filePath = Regex.Replace(filePath, "/", "\\");
            filePath = filePath.Substring(0, filePath.Length - 1);
            filePath = filePath.Trim();
            readInside(pathArray, Path.GetDirectoryName(newFilePath) + "\\" + filePath, replaceValue, search, mode, origXvmPath);
            //readUntill(fileContents, sb, ":");
        }
        private static void xvmEdit(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            bool modified = false;
            //this is the actual value we want to change
            //get past all the boring stuff
            string toReplace = readUntill(fileContents, sb, @"[,}\]]", false);
            //actually replace the value
            //check if it's a comma type (not last) or curley bracket (last)
            string replaced = "";
            if (Regex.IsMatch(toReplace, @",$"))
            {
                if (Regex.IsMatch(toReplace, search + ",$"))
                {
                    replaced = " " + replaceValue + ",";
                    modified = true;
                    patchDone = true;
                }
            }
            else
            {
                if (Regex.IsMatch(toReplace, search))
                {
                    replaced = replaceValue + "\n";
                    modified = true;
                    patchDone = true;
                }
            }
            sb.Append(replaced);
            readUntillEnd(fileContents, sb);

            if (modified)
            {
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
            }

        }
        //adding a new entry. adds to the top of the parent entry
        private static void xvmAdd(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            bool modified = false;
            string replaced = "";
            //read to the end of that line
            //read for (white space) (anything but white space)
            string temp = readUntill(fileContents, sb, @"\s*\S+\s");
            //back up one
            numByteReads--;
            sb.Remove(sb.Length - 1, 1);
            //check if the last one was a comma
            //if not then add it
            //determine if it stopped in groups of eithor ([ , {) or
            char c = fileContents[numByteReads - 1];
            if (c.Equals(','))
            {
                replaced = "\n" + replaceValue + ",";
            }
            else if (c.Equals('{'))
            {
                replaced = "\n" + replaceValue + ",";
            }
            else
            {
                replaced = ",\n" + replaceValue;
            }
            //append it in the sb
            sb.Append(replaced);
            readUntillEnd(fileContents, sb);
            modified = true;
            patchDone = true;
            if (modified)
            {
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //clearing out an array
        private static void xvmArrayClear(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            //advance reader to start of the array
            readUntill(fileContents, sb, @"\[");
            //advance reader to end of array, not saving sb
            readUntill(fileContents, sb, @"\]", false);
            sb.Append("]");
            bool modified = false;
            readUntillEnd(fileContents, sb);
            modified = true;
            patchDone = true;
            if (modified)
            {
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //adding a new entry
        private static void xvmArrayAdd(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search, bool isToEnd = false)
        {
            bool modified = false;
            string replaced = "";
            //all it needs to do is add it
            //unless it's to end
            if (!isToEnd)
            {
                replaced = replaced + replaceValue + ",";
            }
            else
            {
                //need to not add the comma
                replaced = replaced + replaceValue;
                //need to also go back to add the comma to the stringbuilder
                string temp = peekBehindUntill(fileContents, @"[},]");
                //temp = temp.Trim();
                sb.Remove(sb.ToString().Count() - (temp.Count()-1), temp.Count()-1);
                string toApeend = temp.Substring(0, 1);
                sb.Append(toApeend);
                //temp.Remove(0, 1);
                sb.Append(",");
                sb.Append(temp.Substring(1,temp.Length-2));
            }
            //append it in the sb
            sb.Append(replaced);
            if (isToEnd)
                sb.Append("\n");
            readUntillEnd(fileContents, sb);
            modified = true;
            patchDone = true;
            if (modified)
            {
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //editing an existing entry
        private static void xvmArrayEdit(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            bool modified = false;
            bool hadComma = false;
            //move past it and save int input
            string editCheck = readUntill(fileContents, sb, @"[,\]]", false);
            if (Regex.IsMatch(editCheck, search))
            {
                if (Regex.IsMatch(editCheck, @",$"))
                    hadComma = true;
                editCheck = " " + replaceValue;
                if (hadComma)
                    editCheck = editCheck + ",";
                //append it in the sb
                sb.Append(editCheck);
                readUntillEnd(fileContents, sb);
                modified = true;
                patchDone = true;
            }
            if (modified)
            {
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //removing an existing entry
        private static void xvmArrayRemove(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            bool modified = false;
            bool lastItem = false;
            //move past it and save int input
            string editCheck = readUntill(fileContents, sb, @"[,\]]", false);
            if (Regex.IsMatch(editCheck, @"\]$"))
            {
                lastItem = true;
                //remove last comma from sb index
                sb.Remove(sb.Length - 1, 1);
            }
            if (Regex.IsMatch(editCheck, search))
            {
                editCheck = Regex.Replace(editCheck, search, "");
                if (lastItem)
                {
                    editCheck = editCheck + "]";
                }
                //append it in the sb
                sb.Append(editCheck);
                readUntillEnd(fileContents, sb);
                modified = true;
                patchDone = true;
            }
            if (modified)
            {
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //advances the reader untill a certain set of characters are found
        private static string readUntill(string fileContents, StringBuilder sb, string stopAt, bool save = true)
        {
            string readValues = "";
            string temp = readValues;
            while (!Regex.IsMatch(temp, stopAt, RegexOptions.Multiline))
            {
                int numChars = fileContents.Count();
                if (numByteReads >= numChars)
                {
                    return "null";
                }
                char c = fileContents[numByteReads];
                numByteReads++;
                readValues = readValues + c;
                if (save)
                    sb.Append(c);
                if (readValues.Length > stopAt.Length)
                {
                    temp = readValues.Substring(readValues.Length - stopAt.Length, stopAt.Length);
                }
                else
                {
                    temp = readValues;
                }
            }
            return readValues;
        }
        //generic readAhead method of above
        private static string readUntillGeneric(string s, string stopAt)
        {
            //genericTraverse = 0;
            string readValues = "";
            while (!Regex.IsMatch(readValues, stopAt, RegexOptions.Multiline))
            {
                if (genericTraverse >= s.Count())
                    return readValues;
                char c = s[genericTraverse];
                genericTraverse++;
                readValues = readValues + c;
            }
            return readValues;
        }
        //advances the reader x ammout of characters
        private static string readUntill(string fileContents, StringBuilder sb, int stopAt, bool save = true)
        {
            string readValues = "";
            int session = 0;
            while (session != stopAt)
            {
                char c = fileContents[numByteReads];
                numByteReads++;
                session++;
                readValues = readValues + c;
                if (save)
                    sb.Append(c);
            }
            return readValues;
        }
        //advances the reader untill the end
        private static string readUntillEnd(string fileContents, StringBuilder sb)
        {
            string readValues = "";
            while (numByteReads < fileContents.Length)
            {
                char c = fileContents[numByteReads];
                numByteReads++;
                readValues = readValues + c;
                sb.Append(c);
            }
            return readValues;
        }
        //looks ahead untill a certain set of characters are found
        private static string peekUntill(string fileContents, string stopAt)
        {
            string readValues = "";
            int numPeeks = 0;
            while (!Regex.IsMatch(readValues, stopAt, RegexOptions.Multiline))
            {
                if (numByteReads > fileContents.Count())
                {
                    numByteReads = numByteReads + numPeeks;
                    return readValues;
                }
                char c = fileContents[numByteReads];
                numByteReads++;
                readValues = readValues + c;
                numPeeks++;
            }
            numByteReads = numByteReads - numPeeks;
            return readValues;
        }
        //looks behind untill a certain set of characters are found
        private static string peekBehindUntill(string fileContents, string stopAt)
        {
            string readValues = "";
            int numPeeks = 0;
            while (!Regex.IsMatch(readValues, stopAt, RegexOptions.Multiline))
            {
                if (numByteReads == 0)
                {
                    numByteReads = numByteReads + numPeeks;
                    return readValues;
                }
                char c = fileContents[numByteReads];
                numByteReads--;
                readValues = c + readValues;
                numPeeks++;
            }
            numByteReads = numByteReads + numPeeks;
            return readValues;
        }
        //looks ahead untill a certain amount of characters are found
        private static string peekUntill(string fileContents, int stopAt)
        {
            string readValues = "";
            int numPeeks = 0;
            while (numPeeks != stopAt)
            {
                if (numByteReads > fileContents.Count())
                {
                    numByteReads = numByteReads - numPeeks;
                    return readValues;
                }
                char c = fileContents[numByteReads];
                numByteReads++;
                readValues = readValues + c;
                numPeeks++;
            }
            numByteReads = numByteReads - numPeeks;
            return readValues;
        }
        //splits text into an array based on a regex match
        private static List<string> split(string stringToSplit, string regexSplitCommand)
        {
            List<string> temp = new List<string>();
            int saveNumReadBytes = 0;
            string readValues = "";
            while (saveNumReadBytes <= stringToSplit.Count())
            {
                int startBracketCount = 0;
                while (true)
                {
                    if (saveNumReadBytes >= stringToSplit.Count())
                    {
                        readValues = readValues.Substring(0, readValues.Length - 1);
                        temp.Add(readValues);
                        return temp;
                    }
                    char c = stringToSplit[saveNumReadBytes];
                    if (c.Equals('{'))
                        startBracketCount++;
                    else if (c.Equals('}'))
                        startBracketCount--;
                    else if (c.Equals(','))
                    {
                        if (startBracketCount == 0)
                        {
                            saveNumReadBytes++;
                            readValues = readValues + c;
                            break;
                        }
                    }
                    saveNumReadBytes++;
                    readValues = readValues + c;
                }
                temp.Add(readValues);
                readValues = "";
            }
            return temp;
        }
        //returns the folder(s) to get to the xvm config folder directory
        public static string getXVMBootLoc(string tanksLocation, string customBootFileLoc = null)
        {
            string bootFile = tanksLocation + xvmBootFileLoc1;
            if (customBootFileLoc != null)
                bootFile = customBootFileLoc;
            if (!File.Exists(bootFile))
            {
                appendToLog("ERROR: xvm config boot file does not exist at " + xvmBootFileLoc1 + ", checking " + xvmBootFileLoc2);
                bootFile = xvmBootFileLoc2;
                if (!File.Exists(bootFile))
                {
                    appendToLog("ERROR: xvm config boot file does not exist at " + xvmBootFileLoc2 + ", aborting patch");
                    return null;
                }
            }
            appendToLog("xvm boot file located to parse");
            string fileContents = File.ReadAllText(bootFile);
            //patch block comments out
            fileContents = Regex.Replace(fileContents, @"\/\*.*\*\/", "", RegexOptions.Singleline);
            //patch single line comments out
            string[] removeComments = fileContents.Split('\n');
            StringBuilder bootBuilder = new StringBuilder();
            foreach (string s in removeComments)
            {
                if (Regex.IsMatch(s, @"\/\/.*$"))
                    continue;
                bootBuilder.Append(s + "\n");
            }
            fileContents = bootBuilder.ToString();
            //read to the actual file path
            genericTraverse = 0;
            readUntillGeneric(fileContents, @"^[ \t]*\$[ \t]*{[ \t]*""");
            //now read untill the next quote for the temp path
            string filePath = readUntillGeneric(fileContents, "\"");
            //flip the folder path things
            filePath = Regex.Replace(filePath, "/", "\\");
            //remove the last one
            filePath = filePath.Substring(0, filePath.Length - 1);
            filePath = filePath.Trim();
            genericTraverse = 0;
            string theNewPath = Path.GetDirectoryName(filePath);
            return theNewPath;
        }
        //unchecks all mods from memory
        public static void clearSelectionMemory(List<Category> parsedCatagoryList)
        {
            Utils.appendToLog("Unchecking all mods");
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    m.Checked = false;
                    Utils.uncheckProcessConfigs(m.configs);
                }
            }
        }
        private static void uncheckProcessConfigs(List<Config> configList)
        {
            foreach (Config cc in configList)
            {
                cc.Checked = false;
                Utils.uncheckProcessConfigs(cc.configs);
            }
        }
        //saves the currently checked configs and mods
        public static void saveConfig(bool fromButton, List<Category> parsedCatagoryList, List<Mod> userMods)
        {
            //dialog box to ask where to save the config to
            SaveFileDialog saveLocation = new SaveFileDialog();
            saveLocation.AddExtension = true;
            saveLocation.DefaultExt = ".xml";
            saveLocation.Filter = "*.xml|*.xml";
            saveLocation.InitialDirectory = Application.StartupPath + "\\RelHaxUserConfigs";
            saveLocation.Title = "Select where to save user prefs";
            if (fromButton)
            {
                if (saveLocation.ShowDialog().Equals(DialogResult.Cancel))
                {
                    //cancel
                    return;
                }
            }
            string savePath = saveLocation.FileName;
            if (Settings.saveLastConfig && !fromButton)
            {
                savePath = Application.StartupPath + "\\RelHaxUserConfigs\\lastInstalledConfig.xml";
                Utils.appendToLog("Save last config checked, saving to " + savePath);
            }
            //XmlDocument save time!
            XmlDocument doc = new XmlDocument();
            //mods root
            XmlElement modsHolderBase = doc.CreateElement("mods");
            doc.AppendChild(modsHolderBase);
            //relhax mods root
            XmlElement modsHolder = doc.CreateElement("relhaxMods");
            modsHolderBase.AppendChild(modsHolder);
            //user mods root
            XmlElement userModsHolder = doc.CreateElement("userMods");
            modsHolderBase.AppendChild(userModsHolder);
            //check every mod
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.Checked)
                    {
                        //add it to the list
                        XmlElement mod = doc.CreateElement("mod");
                        modsHolder.AppendChild(mod);
                        XmlElement modName = doc.CreateElement("name");
                        modName.InnerText = m.name;
                        mod.AppendChild(modName);
                        if (m.configs.Count > 0)
                        {
                            XmlElement configsHolder = doc.CreateElement("configs");
                            Utils.saveProcessConfigs(doc, m.configs, configsHolder);
                            mod.AppendChild(configsHolder);
                        }
                    }
                }
            }
            //check user mods
            foreach (Mod m in userMods)
            {
                if (m.Checked)
                {
                    //add it to the list
                    XmlElement mod = doc.CreateElement("mod");
                    modsHolder.AppendChild(mod);
                    XmlElement modName = doc.CreateElement("name");
                    modName.InnerText = m.name;
                    mod.AppendChild(modName);
                    userModsHolder.AppendChild(mod);
                }
            }
            doc.Save(savePath);
            if (fromButton)
            {
                MessageBox.Show(Translations.getTranslatedString("configSaveSucess"));
            }
        }
        private static void saveProcessConfigs(XmlDocument doc, List<Config> configList, XmlElement configsHolder)
        {
            foreach (Config cc in configList)
            {
                XmlElement config = null;
                if (cc.Checked)
                {
                    //add the config to the list
                    config = doc.CreateElement("config");
                    configsHolder.AppendChild(config);
                    XmlElement configName = doc.CreateElement("name");
                    configName.InnerText = cc.name;
                    config.AppendChild(configName);

                    if (cc.configs.Count > 0)
                    {
                        XmlElement configsHolderSub = doc.CreateElement("configs");
                        Utils.saveProcessConfigs(doc, cc.configs, configsHolderSub);
                        config.AppendChild(configsHolderSub);
                    }
                }
            }
        }
        //loads a saved config from xml and parses it into the memory database
        public static void loadConfig(string filePath, List<Category> parsedCatagoryList, List<Mod> userMods)
        {
            Utils.clearSelectionMemory(parsedCatagoryList);
            Utils.appendToLog("Loading mod selections from " + filePath);
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            //get a list of mods
            XmlNodeList xmlModList = doc.SelectNodes("//mods/relhaxMods/mod");
            foreach (XmlNode n in xmlModList)
            {
                //gets the inside of each mod
                //also store each config that needsto be enabled
                Mod m = new Mod();
                foreach (XmlNode nn in n.ChildNodes)
                {
                    switch (nn.Name)
                    {
                        case "name":
                            m = Utils.linkMod(nn.InnerText, parsedCatagoryList);
                            if (m == null)
                            {
                                Utils.appendToLog("WARNING: mod \"" + nn.InnerText + "\" not found");
                                MessageBox.Show(Translations.getTranslatedString("modNotFound_1") + nn.InnerText + Translations.getTranslatedString("modNotFound_2"));
                                continue;
                            }
                            if (m.enabled)
                            {
                                Utils.appendToLog("Checking mod " + m.name);
                                m.Checked = true;
                            }
                            break;
                        case "configs":
                            Utils.loadProcessConfigs(nn, m, true);
                            break;
                        case "subConfigs":
                            Utils.loadProcessConfigs(nn, m, true);
                            break;
                    }
                }
            }
            //user mods
            XmlNodeList xmlUserModList = doc.SelectNodes("//mods/userMods/mod");
            foreach (XmlNode n in xmlUserModList)
            {
                //gets the inside of each user mod
                Mod m = new Mod();
                foreach (XmlNode nn in n.ChildNodes)
                {
                    switch (nn.Name)
                    {
                        case "name":
                            m = Utils.getUserMod(nn.InnerText, userMods);
                            if (m != null)
                            {
                                string filename = m.name + ".zip";
                                if (File.Exists(Application.StartupPath + "\\RelHaxUserMods\\" + filename))
                                {
                                    m.Checked = true;
                                    Utils.appendToLog("checking user mod " + m.zipFile);
                                }
                            }
                            break;
                    }
                }
            }
            Utils.appendToLog("Finished loading mod selections");
        }
        private static void loadProcessConfigs(XmlNode holder, Mod m, bool parentIsMod, Config con = null)
        {
            foreach (XmlNode nnn in holder.ChildNodes)
            {
                if (parentIsMod)
                {
                    if (m == null)
                    {
                        continue;
                    }
                }
                else
                {
                    if (con == null)
                    {
                        continue;
                    }
                }
                Config c = new Config();
                foreach (XmlNode nnnn in nnn.ChildNodes)
                {
                    switch (nnnn.Name)
                    {
                        case "name":
                            if (parentIsMod)
                            {
                                c = m.getConfig(nnnn.InnerText);
                            }
                            else
                            {
                                c = con.getSubConfig(nnnn.InnerText);
                            }
                            if (c == null)
                            {
                                Utils.appendToLog("WARNING: config \"" + nnnn.InnerText + "\" not found for mod/config \"" + holder.InnerText + "\"");
                                MessageBox.Show(Translations.getTranslatedString("configNotFound_1") + nnnn.InnerText + Translations.getTranslatedString("configNotFound_2") + holder.InnerText + Translations.getTranslatedString("configNotFound_3"));
                                continue;
                            }
                            if (c.enabled)
                            {
                                Utils.appendToLog("Checking config " + c.name);
                                c.Checked = true;
                            }
                            break;
                        case "configs":
                            Utils.loadProcessConfigs(nnnn, m, false, c);
                            break;
                        case "subConfigs":
                            Utils.loadProcessConfigs(nnnn, m, false, c);
                            break;
                    }
                }
            }
        }
        public static List<string> createDownloadedOldZipsList(List<string> currentZipFiles, List<Category> parsedCatagoryList, List<Dependency> globalDependencies)
        {
            parsedZips = new List<string>();
            foreach (Dependency d in globalDependencies)
            {
                if (!d.dependencyZipFile.Equals(""))
                {
                    parsedZips.Add(d.dependencyZipFile);
                }
            }
            foreach (Category cat in parsedCatagoryList)
            {
                foreach (Dependency d in cat.dependencies)
                {
                    if (!d.dependencyZipFile.Equals(""))
                    {
                        parsedZips.Add(d.dependencyZipFile);
                    }
                }
                foreach (Mod m in cat.mods)
                {
                    foreach (Dependency d in m.dependencies)
                    {
                        if (!d.dependencyZipFile.Equals(""))
                        {
                            parsedZips.Add(d.dependencyZipFile);
                        }
                    }
                    if (!m.zipFile.Equals(""))
                    {
                        parsedZips.Add(m.zipFile);
                    }
                    if (m.configs.Count > 0)
                        parseZipFileConfigs(m.configs);
                }
            }
            //now parsedZips has every single possible zipFile in the database
            //for each zipfile in it, remove it in currentZipFiles if it exists
            foreach (string s in parsedZips)
            {
                currentZipFiles.Remove(s);
            }
            return currentZipFiles;
        }
        public static void parseZipFileConfigs(List<Config> configList)
        {
            foreach (Config c in configList)
            {
                foreach (Dependency d in c.dependencies)
                {
                    if (!d.dependencyZipFile.Equals(""))
                    {
                        parsedZips.Add(d.dependencyZipFile);
                    }
                }
                if (!c.zipFile.Equals(""))
                {
                    parsedZips.Add(c.zipFile);
                }
                if (c.configs.Count > 0)
                    parseZipFileConfigs(c.configs);
            }
        }
        //deletes all empty directories from a given start location
        public static void processDirectory(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                processDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Utils.appendToLog("Deleting empty directory " + directory);
                    Directory.Delete(directory, false);
                }
            }
        }
        //returns true if the CRC's of each file match, false otherwise
        public static bool CRCsMatch(string localFile, string remoteCRC)
        {
            if (!File.Exists(localFile))
                return false;
            string crc = Utils.getMd5Hash(localFile);
            if (crc.Equals(remoteCRC))
                return true;
            return false;
        }
    }
}
