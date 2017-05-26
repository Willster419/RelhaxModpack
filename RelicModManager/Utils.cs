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
        //logs string info to the log output
        public static void appendToLog(string info)
        {
            //the method should automaticly make the file if it's not there
            File.AppendAllText(Application.StartupPath + "\\RelHaxLog.txt", info + "\n");
        }
        //returns the md5 hash of the file based on the input file string location
        public static string getMd5Hash(string inputFile)
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
        public static void createModStructure2(string databaseURL, bool backendFlag, List<Dependency> globalDependencies, List<Catagory> parsedCatagoryList)
        {
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
            //parsedCatagoryList = new List<Catagory>();
            foreach (XmlNode catagoryHolder in catagoryList)
            {
                Catagory cat = new Catagory();
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
                                            break;
                                        case "version":
                                            m.version = modNode.InnerText;
                                            break;
                                        case "size":
                                            m.size = Utils.parseFloat(modNode.InnerText, 0.0f);
                                            break;
                                        case "modzipfile":
                                            m.modZipFile = modNode.InnerText;
                                            break;
                                        case "startAddress":
                                            m.startAddress = modNode.InnerText;
                                            break;
                                        case "endAddress":
                                            m.endAddress = modNode.InnerText;
                                            break;
                                        case "modzipcrc":
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
                                                            m.picList.Add(new Picture(m.name, pictureNode.InnerText));
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
                                                m.modDependencies.Add(d);
                                            }
                                            break;
                                        case "configs":
                                            //parse every config for that mod
                                            foreach (XmlNode configHolder in modNode.ChildNodes)
                                            {
                                                Config c = new Config();
                                                foreach (XmlNode configNode in configHolder.ChildNodes)
                                                {
                                                    switch (configNode.Name)
                                                    {
                                                        case "name":
                                                            c.name = configNode.InnerText;
                                                            break;
                                                        case "configzipfile":
                                                            c.zipConfigFile = configNode.InnerText;
                                                            break;
                                                        case "startAddress":
                                                            c.startAddress = configNode.InnerText;
                                                            break;
                                                        case "endAddress":
                                                            c.endAddress = configNode.InnerText;
                                                            break;
                                                        case "configzipcrc":
                                                            c.crc = configNode.InnerText;
                                                            break;
                                                        case "configenabled":
                                                            c.enabled = Utils.parseBool(configNode.InnerText, false);
                                                            break;
                                                        case "size":
                                                            c.size = Utils.parseFloat(configNode.InnerText, 0.0f);
                                                            break;
                                                        case "configtype":
                                                            c.type = configNode.InnerText;
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
                                                                            if (backendFlag)
                                                                                c.pictureList.Add(innerText);
                                                                            else
                                                                                m.picList.Add(new Picture(c.name, pictureNode.InnerText));
                                                                            break;
                                                                    }
                                                                }
                                                            }
                                                            break;
                                                        case "subConfigs":
                                                            //parse every subConfig
                                                            foreach (XmlNode subConfigHolder in configNode.ChildNodes)
                                                            {
                                                                SubConfig subC = new SubConfig();
                                                                foreach (XmlNode subConfigNode in subConfigHolder.ChildNodes)
                                                                {
                                                                    switch (subConfigNode.Name)
                                                                    {
                                                                        case "name":
                                                                            subC.name = subConfigNode.InnerText;
                                                                            break;
                                                                        case "zipFile":
                                                                            subC.zipFile = subConfigNode.InnerText;
                                                                            break;
                                                                        case "crc":
                                                                            subC.crc = subConfigNode.InnerText;
                                                                            break;
                                                                        case "enabled":
                                                                            subC.enabled = Utils.parseBool(subConfigNode.InnerText, false);
                                                                            break;
                                                                        case "type":
                                                                            subC.type = subConfigNode.InnerText;
                                                                            break;
                                                                        case "pictures":
                                                                            //parse every picture
                                                                            foreach (XmlNode pictureHolder in subConfigNode.ChildNodes)
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
                                                                                            if (backendFlag)
                                                                                                subC.pictureList.Add(innerText);
                                                                                            else
                                                                                                m.picList.Add(new Picture(c.name, pictureNode.InnerText));
                                                                                            break;
                                                                                    }
                                                                                }
                                                                            }
                                                                            break;
                                                                        case "dependencies":
                                                                            //parse every dependency
                                                                            foreach (XmlNode dependencyHolder in subConfigNode.ChildNodes)
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
                                                                                        case "dependencyEnabled":
                                                                                            d.enabled = Utils.parseBool(dependencyNode.InnerText, false);
                                                                                            break;
                                                                                    }
                                                                                }
                                                                                subC.dependencies.Add(d);
                                                                            }
                                                                            break;
                                                                        case "size":
                                                                            subC.size = Utils.parseFloat(subConfigNode.InnerText, 0.0f);
                                                                            break;
                                                                        case "startAddress":
                                                                            subC.startAddress = subConfigNode.InnerText;
                                                                            break;
                                                                        case "endAddress":
                                                                            subC.endAddress = subConfigNode.InnerText;
                                                                            break;
                                                                    }
                                                                }
                                                                c.subConfigs.Add(subC);
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
                                                                m.modDependencies.Add(d);
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
        //checks for duplicates
        public static bool duplicates(List<Catagory> parsedCatagoryList)
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
        //returns the mod based on catagory and mod name
        public static Mod linkMod(string modName, string catagoryName, List<Catagory> parsedCatagoryList)
        {
            foreach (Catagory c in parsedCatagoryList)
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
        public static Mod linkMod(string modName, List<Catagory> parsedCatagoryList)
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
        //returns the catagory based on the catagory name
        public static Catagory getCatagory(string catName, List<Catagory> parsedCatagoryList)
        {
            foreach (Catagory c in parsedCatagoryList)
            {
                if (c.name.Equals(catName)) return c;
            }
            return null;
        }
        //gets the user mod based on it's name
        public static Mod getUserMod(string modName,List<Mod> userMods)
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
        public static void sortCatagoryList(List<Catagory> catagoryList)
        {
            catagoryList.Sort(Catagory.CompareCatagories);
        }
        //sorts a list of pictures by mod or config, then name
        public static List<Picture> sortPictureList(List<Picture> pictureList)
        {
            //don't actually sort them anymore
            //they will not apprea in the order of which they were loaded from the xml file
            //pictureList.Sort(Picture.ComparePictures);
            return pictureList;
        }
        //checks to see if the application is indeed in admin mode
        public static bool isAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            if (identity != null)
            {
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                bool isPowerUser = principal.IsInRole(WindowsBuiltInRole.PowerUser);
                bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                return (isPowerUser || isAdmin);
            }
            Utils.appendToLog("WARNING: user is not admin or power user");
            return false;
        }
        //method to patch a part of an xml file
        //fileLocation is relative to res_mods folder
        public static void xmlPatch(string filePath, string xpath, string mode, string search, string replace, string tanksLocation, string tanksVersion, bool testMods = false)
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

            if (testMods)
            {

            }
            else
            {
                //patch versiondir out of filePath
                filePath = Regex.Replace(filePath, "versiondir", tanksVersion);
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
        public static void RegxPatch(string fileLocation, string search, string replace, string tanksLocation, string tanksVersion, int lineNumber = 0, bool testMods = false)
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

            if (testMods)
            {

            }
            else
            {
                //patch versiondir out of fileLocation
                fileLocation = Regex.Replace(fileLocation, "versiondir", tanksVersion);
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
        public static void jsonPatch(string jsonFile, string jsonPath, string newValue, string mode, string tanksLocation, string tanksVersion, bool testMods = false)
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
            {

            }
            //try a double nixt. it will parse a double and int. at this point it could be eithor
            try
            {
                newValueDouble = double.Parse(newValue);
                useDouble = true;
            }
            catch (FormatException)
            {

            }
            //try an int next. if it works than turn double to false and int to true
            try
            {
                newValueInt = int.Parse(newValue);
                useInt = true;
                useDouble = false;
            }
            catch (FormatException)
            {

            }
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
            if (testMods)
            {

            }
            else
            {
                jsonFile = Regex.Replace(jsonFile, "versiondir", tanksVersion);
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
                //MessageBox.Show("ERROR: Failed to patch " + jsonFile);
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
                    putBackDollas[i] = temp;
                    ssList.RemoveAt(0);
                }
                else if (Regex.IsMatch(temp, "-42069"))
                {
                    temp = "$" + ssList[0].value;
                    putBackDollas[i] = temp;
                    ssList.RemoveAt(0);
                }
                rebuilder.Append(putBackDollas[i] + "\n");
            }
            if (ssList.Count != 0)
                Utils.appendToLog("There was an error with patching the file " + jsonFile + ", with extra refrences");
            File.WriteAllText(jsonFile, rebuilder.ToString());
        }
    }
}
