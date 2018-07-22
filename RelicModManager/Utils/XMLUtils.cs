using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RelhaxModpack
{
    public static class XMLUtils
    {
        public static int TotalModConfigComponents = 0;
        //DeveloperSelections namelist
        public static List<DeveloperSelections> developerSelections = new List<DeveloperSelections>();
        
        //check to make sure an xml file is valid
        public static bool IsValidXml(string xmlString)
        {
            XmlTextReader read = new XmlTextReader(xmlString);
            try
            {
                while (read.Read()) ;
                read.Close();
                return true;
            }
            catch (Exception e)
            {
                Utils.ExceptionLog(e);
                read.Close();
                return false;
            }
        }
        //allows one to get an xml element ro attribute from an xml string
        public static string GetXMLElementAttributeFromFile(string file, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
            }
            catch
            {
                return null;
            }
            return GetXMLElementAttributeMain(doc, xpath);
        }
        //allows one to get an xml element or attribute from an xml file
        public static string GetXMLElementAttributeFromString(string xmlString, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlString);
            }
            catch
            {
                Logging.Manager("no valid XML string: " + xmlString);
                return null;
            }
            return GetXMLElementAttributeMain(doc, xpath);
        }
        //allows one to get an xml element or attribute value from the above methods
        //element example: "//root/element"
        //attribute example: "//root/element/@attribute"
        //for the onlineFolder version: //modInfoAlpha.xml/@onlineFolder
        //for the folder version: //modInfoAlpha.xml/@version
        public static string GetXMLElementAttributeMain(XmlDocument doc, string xpath)
        {
            XmlNode result = doc.SelectSingleNode(xpath);
            if (result == null)
                return null;
            return result.InnerText;
        }
        //parses the xml mod info into the memory database (change XML reader from XMLDocument to XDocument)
        // https://www.google.de/search?q=c%23+xdocument+get+line+number&oq=c%23+xdocument+get+line+number&aqs=chrome..69i57j69i58.11773j0j7&sourceid=chrome&ie=UTF-8
        public static void CreateModStructure(string databaseURL, List<Dependency> globalDependencies, List<Dependency> dependencies,
            List<LogicalDependency> logicalDependencies, List<Category> parsedCatagoryList, bool buildRefrences)
        {
            developerSelections.Clear();
            TotalModConfigComponents = 0;
            XDocument doc = null;
            try
            {
                if (databaseURL.ToLower().Equals(Settings.ModInfoDatFile.ToLower()))
                {
                    Logging.Manager("loading dat config file");
                    string xmlString = Utils.GetStringFromZip(Settings.ModInfoDatFile, "modInfo.xml");
                    if (!Settings.BetaDatabase && !Program.testMode)
                    {
                        Settings.TanksOnlineFolderVersion = GetXMLElementAttributeFromString(xmlString, "//modInfoAlpha.xml/@onlineFolder");
                    }
                    doc = XDocument.Parse(xmlString, LoadOptions.SetLineInfo);
                    // create new developerSelections NameList
                    ParseDeveloperSelections(doc);
                }
                else
                {
                    Logging.Manager("loading local config file");
                    doc = XDocument.Load(databaseURL, LoadOptions.SetLineInfo);
                }
            }
            catch (XmlException ex)
            {
                Logging.Manager(string.Format("CRITICAL: Failed to read database: {0}\nMessage: {1}", databaseURL, ex.Message));
                MessageBox.Show(Translations.GetTranslatedString("databaseReadFailed"));
                Application.Exit();
                return;
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("createModStructure", string.Format("tried to access {0}", databaseURL), ex);
                MessageBox.Show(Translations.GetTranslatedString("databaseNotFound"));
                Application.Exit();
                return;
            }
            //add the global dependencies
            foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/globaldependencies/globaldependency"))
            {
                List<string> depNodeList = new List<string>() { "zipFile", "crc", "enabled", "packageName", "appendExtraction" };
                List<string> optionalDepNodList = new List<string>() { "startAddress", "endAddress", "devURL", "timestamp", "logAtInstall" };
                List<string> unknownNodeList = new List<string>() { };
                Dependency d = new Dependency();
                foreach (XElement globs in dependencyNode.Elements())
                {
                    switch (globs.Name.ToString())
                    {
                        case "zipFile":
                            d.ZipFile = globs.Value;
                            break;
                        case "timestamp":
                            d.Timestamp = long.Parse("0" + globs.Value);
                            break;
                        case "crc":
                            d.CRC = globs.Value;
                            break;
                        case "startAddress":
                            d.StartAddress = globs.Value;
                            break;
                        case "endAddress":
                            d.EndAddress = globs.Value;
                            break;
                        case "logAtInstall":
                            d.LogAtInstall = Utils.ParseBool(globs.Value,true);
                            break;
                        case "devURL":
                            d.DevURL = globs.Value;
                            break;
                        case "enabled":
                            d.Enabled = Utils.ParseBool(globs.Value, false);
                            break;
                        case "appendExtraction":
                            d.AppendExtraction = Utils.ParseBool(globs.Value, false);
                            break;
                        case "packageName":
                            d.PackageName = globs.Value.Trim();
                            if (d.PackageName.Equals(""))
                            {
                                Logging.Manager(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => globsPend {1} (line {2})",
                                    globs.Name.ToString(), d.ZipFile, ((IXmlLineInfo)globs).LineNumber));
                                if (Program.testMode)
                                    MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => globsPend {1}\n\nmore informations, see logfile",
                                        globs.Name.ToString(), d.ZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            break;
                        default:
                            Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => globsPend {1} (line {2})",
                                globs.Name.ToString(), d.ZipFile, ((IXmlLineInfo)globs).LineNumber));
                            if (Program.testMode)
                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: ZipFile, CRC, StartAddress, EndAddress, enabled, PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                    globs.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                    }
                    //mandatory component
                    if (depNodeList.Contains(globs.Name.ToString()))
                        depNodeList.Remove(globs.Name.ToString());
                    //optional component
                    else if (optionalDepNodList.Contains(globs.Name.ToString()))
                        optionalDepNodList.Remove(globs.Name.ToString());
                    //unknown component
                    else
                        unknownNodeList.Add(globs.Name.ToString());
                }
                if (depNodeList.Count > 0)
                    Logging.Manager(string.Format("Error: modInfo.xml nodes not used: {0} => globsPend {1} (line {2})",
                        string.Join(",", depNodeList), d.PackageName, ((IXmlLineInfo)dependencyNode).LineNumber));
                if (unknownNodeList.Count > 0)
                    Logging.Manager(string.Format("Error: modInfo.xml unknown nodes: {0} => globsPend {1} (line {2})",
                        string.Join(",", unknownNodeList), d.PackageName, ((IXmlLineInfo)dependencyNode).LineNumber));
                if (d.PackageName.Equals(""))
                {
                    d.PackageName = Utils.RandomString(30);
                    Logging.Manager("PackageName is random generated: " + d.PackageName);   // to avoid exceptions
                }
                globalDependencies.Add(d);
            }
            //add the dependencies
            foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/dependencies/dependency"))
            {
                List<string> depNodeList = new List<string>() { "zipFile", "crc", "enabled", "packageName", "appendExtraction" };
                List<string> optionalDepNodList = new List<string>() { "startAddress", "endAddress", "devURL", "timestamp" , "logicalDependencies", "logAtInstall" };
                List<string> unknownNodeList = new List<string>() { };
                Dependency d = new Dependency();
                foreach (XElement globs in dependencyNode.Elements())
                {
                    switch (globs.Name.ToString())
                    {
                        case "zipFile":
                            d.ZipFile = globs.Value;
                            break;
                        case "timestamp":
                            d.Timestamp = long.Parse("0" + globs.Value);
                            break;
                        case "crc":
                            d.CRC = globs.Value;
                            break;
                        case "startAddress":
                            d.StartAddress = globs.Value;
                            break;
                        case "endAddress":
                            d.EndAddress = globs.Value;
                            break;
                        case "logAtInstall":
                            d.LogAtInstall = Utils.ParseBool(globs.Value, true);
                            break;
                        case "devURL":
                            d.DevURL = globs.Value;
                            break;
                        case "enabled":
                            d.Enabled = Utils.ParseBool(globs.Value, false);
                            break;
                        case "appendExtraction":
                            d.AppendExtraction = Utils.ParseBool(globs.Value, false);
                            break;
                        case "packageName":
                            d.PackageName = globs.Value.Trim();
                            if (d.PackageName.Equals(""))
                            {
                                Logging.Manager(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => globsPend {1} (line {2})",
                                    globs.Name.ToString(), d.ZipFile, ((IXmlLineInfo)globs).LineNumber));
                                if (Program.testMode)
                                    MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => globsPend {1}\n\nmore informations, see logfile",
                                    globs.Name.ToString(), d.ZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            break;
                        case "logicalDependencies":
                            //parse all dependencies
                            foreach (XElement logDependencyHolder in globs.Elements())
                            {
                                string[] logDepNodeList = new string[] { "packageName", "negateFlag" };
                                LogicalDependency ld = new LogicalDependency();
                                foreach (XElement logDependencyNode in logDependencyHolder.Elements())
                                {
                                    logDepNodeList = logDepNodeList.Except(new string[] { logDependencyNode.Name.ToString() }).ToArray();
                                    switch (logDependencyNode.Name.ToString())
                                    {
                                        case "packageName":
                                            ld.PackageName = logDependencyNode.Value.Trim();
                                            if (ld.PackageName.Equals(""))
                                            {
                                                Logging.Manager(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => logDep {1} (line {2})",
                                                    logDependencyNode.Name.ToString(), d.ZipFile, ((IXmlLineInfo)logDependencyNode).LineNumber));
                                                if (Program.testMode)
                                                    MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\"  => dep {1}",
                                                    logDependencyNode.Name.ToString(), d.ZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            }
                                            break;
                                        case "negateFlag":
                                            ld.NegateFlag = Utils.ParseBool(logDependencyNode.Value, true);
                                            break;
                                        default:
                                            Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => logDep {1} (line {2})",
                                                logDependencyNode.Name.ToString(), d.ZipFile, ((IXmlLineInfo)logDependencyNode).LineNumber));
                                            if (Program.testMode)
                                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                                    logDependencyNode.Name.ToString()));
                                            break;
                                    }
                                }
                                if (logDepNodeList.Length > 0)
                                    Logging.Manager(string.Format("Error: modInfo.xml nodes not used: {0} => logDep {1} (line {2})",
                                    string.Join(",", logDepNodeList), ld.ZipFile, ((IXmlLineInfo)logDependencyHolder).LineNumber));
                                if (ld.PackageName.Equals(""))
                                {
                                    ld.PackageName = Utils.RandomString(30);
                                    Logging.Manager("PackageName is random generated: " + ld.PackageName);              // to avoid exceptions
                                }
                                d.LogicalDependencies.Add(ld);
                            }
                            break;
                        default:
                            Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => globsPend {1} (line {2})",
                                globs.Name.ToString(), d.ZipFile, ((IXmlLineInfo)globs).LineNumber));
                            if (Program.testMode)
                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: ZipFile, CRC, StartAddress, EndAddress, enabled, PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                    globs.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                    }
                    //mandatory component
                    if (depNodeList.Contains(globs.Name.ToString()))
                        depNodeList.Remove(globs.Name.ToString());
                    //optional component
                    else if (optionalDepNodList.Contains(globs.Name.ToString()))
                        optionalDepNodList.Remove(globs.Name.ToString());
                    //unknown component
                    else
                        unknownNodeList.Add(globs.Name.ToString());
                }
                if (depNodeList.Count > 0)
                    Logging.Manager(string.Format("Error: modInfo.xml nodes not used: {0} => globsPend {1} (line {2})",
                        string.Join(",", depNodeList), d.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                if (unknownNodeList.Count > 0)
                    Logging.Manager(string.Format("Error: modInfo.xml unknown nodes: {0} => globsPend {1} (line {2})",
                        string.Join(",", unknownNodeList), d.PackageName, ((IXmlLineInfo)dependencyNode).LineNumber));
                if (d.PackageName.Equals(""))
                {
                    d.PackageName = Utils.RandomString(30);
                    Logging.Manager("PackageName is random generated: " + d.PackageName);    // to avoid exceptions
                }
                dependencies.Add(d);
            }
            //add the logicalDependencies
            foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/logicalDependencies/logicalDependency"))
            {
                List<string> depNodeList = new List<string>() { "zipFile", "crc", "enabled", "packageName", "logic" };
                List<string> optionalDepNodList = new List<string>() { "startAddress", "endAddress", "devURL", "timestamp", "logAtInstall" };
                List<string> unknownNodeList = new List<string>() { };
                LogicalDependency d = new LogicalDependency();
                foreach (XElement globs in dependencyNode.Elements())
                {
                    switch (globs.Name.ToString())
                    {
                        case "zipFile":
                            d.ZipFile = globs.Value;
                            break;
                        case "timestamp":
                            d.Timestamp = long.Parse("0" + globs.Value);
                            break;
                        case "crc":
                            d.CRC = globs.Value;
                            break;
                        case "startAddress":
                            d.StartAddress = globs.Value;
                            break;
                        case "endAddress":
                            d.EndAddress = globs.Value;
                            break;
                        case "logAtInstall":
                            d.LogAtInstall = Utils.ParseBool(globs.Value, true);
                            break;
                        case "devURL":
                            d.DevURL = globs.Value;
                            break;
                        case "enabled":
                            d.Enabled = Utils.ParseBool(globs.Value, false);
                            break;
                        case "logic":
                            d.AndOrLogic = LogicalDependency.GetAndOrID(globs.Value);
                            break;
                        case "packageName":
                            d.PackageName = globs.Value.Trim();
                            if (d.PackageName.Equals(""))
                            {
                                Logging.Manager(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => logDep {1} (line {2})",
                                    globs.Name.ToString(), d.ZipFile, ((IXmlLineInfo)globs).LineNumber));
                                if (Program.testMode)
                                    MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => logDep {1}\n\nmore informations, see logfile",
                                        globs.Name.ToString(), d.ZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            break;
                        default:
                            Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => logDep {1} (line {2})",
                                globs.Name.ToString(), d.ZipFile, ((IXmlLineInfo)globs).LineNumber));
                            if (Program.testMode)
                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: ZipFile, CRC, StartAddress, EndAddress, enabled, PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                    globs.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            break;
                    }
                    //mandatory component
                    if (depNodeList.Contains(globs.Name.ToString()))
                        depNodeList.Remove(globs.Name.ToString());
                    //optional component
                    else if (optionalDepNodList.Contains(globs.Name.ToString()))
                        optionalDepNodList.Remove(globs.Name.ToString());
                    //unknown component
                    else
                        unknownNodeList.Add(globs.Name.ToString());
                }
                if (depNodeList.Count > 0)
                    Logging.Manager(string.Format("Error: modInfo.xml nodes not used: {0} => globsPend {1} (line {2})",
                        string.Join(",", depNodeList), d.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                if (unknownNodeList.Count > 0)
                    Logging.Manager(string.Format("Error: modInfo.xml unknown nodes: {0} => globsPend {1} (line {2})",
                        string.Join(",", unknownNodeList), d.PackageName, ((IXmlLineInfo)dependencyNode).LineNumber));
                if (d.PackageName.Equals(""))
                {
                    d.PackageName = Utils.RandomString(30);
                    Logging.Manager("PackageName is random generated: " + d.PackageName);            // to avoid exceptions
                }
                logicalDependencies.Add(d);
            }
            foreach (XElement catagoryHolder in doc.XPathSelectElements("/modInfoAlpha.xml/catagories/catagory"))
            {
                Category cat = new Category();
                string[] catNodeList = new string[] { "name", "installGroup", "packages", "dependencies" };
                foreach (XElement catagoryNode in catagoryHolder.Elements())
                {
                    catNodeList = catNodeList.Except(new string[] { catagoryNode.Name.ToString() }).ToArray();
                    switch (catagoryNode.Name.ToString())
                    {
                        case "name":
                            cat.Name = catagoryNode.Value;
                            break;
                        case "installGroup":
                            cat.InstallGroup = Utils.ParseInt(catagoryNode.Value,0);
                            break;
                        case "packages":
                            foreach (XElement modHolder in catagoryNode.Elements())
                            {
                                switch (modHolder.Name.ToString())
                                {
                                    case "package":
                                        List<string> packageNodeList = new List<string>() { "name", "zipFile", "crc", "enabled", "visible", "packageName", "type" };
                                        List<string> optionalPackageNodeList = new List<string>() { "version", "timestamp", "startAddress", "endAddress", "size",
                                            "description", "updateComment", "devURL", "userDatas", "medias", "dependencies", "logicalDependencies", "packages", "logAtInstall" };
                                        List<string> unknownNodeList = new List<string>() { };
                                        SelectablePackage m = new SelectablePackage()
                                        {
                                            Level = 0
                                        };
                                        foreach (XElement modNode in modHolder.Elements())
                                        {
                                            switch (modNode.Name.ToString())
                                            {
                                                case "name":
                                                    m.Name = modNode.Value;
                                                    TotalModConfigComponents++;
                                                    break;
                                                case "version":
                                                    m.Version = modNode.Value;
                                                    break;
                                                case "zipFile":
                                                    m.ZipFile = modNode.Value;
                                                    break;
                                                case "timestamp":
                                                    m.Timestamp = long.Parse("0" + modNode.Value);
                                                    break;
                                                case "startAddress":
                                                    m.StartAddress = modNode.Value;
                                                    break;
                                                case "endAddress":
                                                    m.EndAddress = modNode.Value;
                                                    break;
                                                case "logAtInstall":
                                                    m.LogAtInstall = Utils.ParseBool(modNode.Value, true);
                                                    break;
                                                case "crc":
                                                    m.CRC = modNode.Value;
                                                    break;
                                                case "type":
                                                    m.Type = modNode.Value;
                                                    break;
                                                case "enabled":
                                                    m.Enabled = Utils.ParseBool(modNode.Value, false);
                                                    break;
                                                case "visible":
                                                    m.Visible = Utils.ParseBool(modNode.Value, true);
                                                    break;
                                                case "packageName":
                                                    m.PackageName = modNode.Value.Trim();
                                                    if (m.PackageName.Equals(""))
                                                    {
                                                        Logging.Manager(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => mod {1} ({2}) (line {3})",
                                                            modNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)modNode).LineNumber));
                                                        if (Program.testMode)
                                                            MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => mod {1} ({2})",
                                                                modNode.Name.ToString(), m.Name, m.ZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                    }
                                                    break;
                                                case "size":
                                                    m.Size = Utils.ParseInt(modNode.Value, 0);
                                                    break;
                                                case "description":
                                                    m.Description = ConvertFromXmlSaveFormat(modNode.Value);
                                                    break;
                                                case "updateComment":
                                                    m.UpdateComment = ConvertFromXmlSaveFormat(modNode.Value);
                                                    break;
                                                case "devURL":
                                                    m.DevURL = modNode.Value;
                                                    break;
                                                case "userDatas":
                                                    foreach (XElement userDataNode in modNode.Elements())
                                                    {
                                                        switch (userDataNode.Name.ToString())
                                                        {
                                                            case "userData":
                                                                string innerText = userDataNode.Value;
                                                                if (innerText == null)
                                                                    continue;
                                                                if (innerText.Equals(""))
                                                                    continue;
                                                                UserFiles uf = new UserFiles();
                                                                uf.Pattern = innerText;
                                                                if (userDataNode.Attribute("before") != null)
                                                                    uf.placeBeforeExtraction = Utils.ParseBool(userDataNode.Attribute("before").Value, false);
                                                                m.UserFiles.Add(uf);
                                                                break;
                                                            default:
                                                                Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => userDatas => expected node: userData (line {3})",
                                                                    userDataNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)userDataNode).LineNumber));
                                                                if (Program.testMode)
                                                                    MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected node: userData\n\nNode found: {0}\n\nmore informations, see logfile",
                                                                        userDataNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                                break;
                                                        }
                                                    }
                                                    break;
                                                case "medias":
                                                    //parse every picture
                                                    foreach (XElement pictureHolder in modNode.Elements())
                                                    {
                                                        Media med = new Media();
                                                        switch (pictureHolder.Name.ToString())
                                                        {
                                                            case "media":
                                                                foreach (XElement pictureNode in pictureHolder.Elements())
                                                                {
                                                                    switch (pictureNode.Name.ToString())
                                                                    {
                                                                        case "URL":
                                                                            string innerText = pictureNode.Value;
                                                                            if (innerText == null)
                                                                                continue;
                                                                            if (innerText.Equals(""))
                                                                                continue;
                                                                            med.URL = innerText;
                                                                            break;
                                                                        case "type":
                                                                            int innerValue = Utils.ParseInt(pictureNode.Value, 1);
                                                                            switch (innerValue)
                                                                            {
                                                                                case 1:
				                                                                    med.MediaType = MediaType.Picture;
				                                                                    break;
				                                                                case 2:
				                                                                    med.MediaType = MediaType.Webpage;
				                                                                    break;
				                                                                case 3:
				                                                                    med.MediaType = MediaType.MediaFile;
				                                                                    break;
				                                                                case 4:
				                                                                    med.MediaType = MediaType.HTML;
				                                                                    break;
                                                                            }
                                                                            break;
                                                                        default:
                                                                            Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => picture =>expected nodes: URL (line {3})",
                                                                                pictureNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)pictureNode).LineNumber));
                                                                            if (Program.testMode)
                                                                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: URL\n\nNode found: {0}\n\nmore informations, see logfile",
                                                                                    pictureNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                                            break;
                                                                    }
                                                                }
                                                                break;
                                                            default:
                                                                Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => expected node: picture (line {3})",
                                                                    pictureHolder.Name, m.Name, m.ZipFile, ((IXmlLineInfo)pictureHolder).LineNumber));
                                                                if (Program.testMode)
                                                                    MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected node: picture\n\nNode found: {0}\n\nmore informations, see logfile",
                                                                        pictureHolder.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                                break;
                                                        }
                                                        m.PictureList.Add(med);
                                                    }
                                                    break;
                                                case "dependencies":
                                                    //parse all dependencies
                                                    foreach (XElement dependencyHolder in modNode.Elements())
                                                    {
                                                        string[] depNodeList = new string[] { "packageName" };
                                                        Dependency d = new Dependency();
                                                        foreach (XElement dependencyNode in dependencyHolder.Elements())
                                                        {
                                                            depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                                            switch (dependencyNode.Name.ToString())
                                                            {
                                                                case "packageName":
                                                                    d.PackageName = dependencyNode.Value.Trim();
                                                                    if (d.PackageName.Equals(""))
                                                                    {
                                                                        Logging.Manager(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => mod {1} ({2}) => dep {3} (line {4})",
                                                                            dependencyNode.Name.ToString(), m.Name, m.ZipFile, d.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                        if (Program.testMode)
                                                                        {
                                                                            MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => mod {1} ({2}) => dep {3}",
                                                                                dependencyNode.Name.ToString(), m.Name, m.ZipFile, d.ZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                                        }
                                                                    }
                                                                    break;
                                                                default:
                                                                    Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => dep {3} (line {4})",
                                                                        dependencyNode.Name.ToString(), m.Name, m.ZipFile, d.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                    if (Program.testMode)
                                                                        MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                                                            dependencyNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                                    break;
                                                            }
                                                        }
                                                        if (depNodeList.Length > 0)
                                                        {
                                                            Logging.Manager(string.Format("Error: modInfo.xml nodes not used: {0} => mod {1} ({2}) => dep {3} (line {4})",
                                                                string.Join(",", depNodeList), m.Name, m.ZipFile, d.ZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber));
                                                        }
                                                        if (d.PackageName.Equals(""))
                                                        {
                                                            d.PackageName = Utils.RandomString(30);
                                                            Logging.Manager("PackageName is random generated: " + d.PackageName);
                                                        }              // to avoid exceptions
                                                        m.Dependencies.Add(d);
                                                    }
                                                    break;
                                                case "logicalDependencies":
                                                    //parse all dependencies
                                                    foreach (XElement dependencyHolder in modNode.Elements())
                                                    {
                                                        string[] depNodeList = new string[] { "packageName", "negateFlag" };
                                                        LogicalDependency d = new LogicalDependency();
                                                        foreach (XElement dependencyNode in dependencyHolder.Elements())
                                                        {
                                                            depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                                            switch (dependencyNode.Name.ToString())
                                                            {
                                                                case "packageName":
                                                                    d.PackageName = dependencyNode.Value.Trim();
                                                                    if (d.PackageName.Equals(""))
                                                                    {
                                                                        Logging.Manager(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => config {1} ({2}) => dep {3} (line {4})",
                                                                            dependencyNode.Name.ToString(), m.Name, m.ZipFile, d.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                        if (Program.testMode)
                                                                            MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => config {1} ({2}) => dep {3}",
                                                                                dependencyNode.Name.ToString(), m.Name, m.ZipFile, d.ZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                                    }
                                                                    break;
                                                                case "negateFlag":
                                                                    d.NegateFlag = Utils.ParseBool(dependencyNode.Value, true);
                                                                    break;
                                                                default:
                                                                    Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => dep {3} (line {4})",
                                                                        dependencyNode.Name.ToString(), m.Name, m.ZipFile, d.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                    if (Program.testMode)
                                                                    {
                                                                        MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                                                            dependencyNode.Name.ToString()));
                                                                    };
                                                                    break;
                                                            }
                                                        }
                                                        if (depNodeList.Length > 0)
                                                        {
                                                            Logging.Manager(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) => dep {3} (line {4})",
                                                                string.Join(",", depNodeList), m.Name, m.ZipFile, d.ZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber));
                                                        }
                                                        if (d.PackageName.Equals(""))
                                                        {
                                                            d.PackageName = Utils.RandomString(30);
                                                            Logging.Manager("PackageName is random generated: " + d.PackageName);
                                                        }              // to avoid exceptions
                                                        m.LogicalDependencies.Add(d);
                                                    }
                                                    break;
                                                case "packages":
                                                    //run the process configs method
                                                    ProcessConfigs(modNode, m, true,m.Level+1);
                                                    break;
                                                default:
                                                    Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) (line {3})",
                                                        modNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)modNode).LineNumber));
                                                    if (Program.testMode)
                                                        MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: name, version, ZipFile," +
                                                            "StartAddress, EndAddress, CRC, Enabled, PackageName, size, description, updateComment, devURL, userDatas," +
                                                            " pictures, dependencies, configs\n\nNode found: {0}\n\nmore informations, see logfile",
                                                            modNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                    break;
                                            }
                                            //mandatory component
                                            if (packageNodeList.Contains(modNode.Name.ToString()))
                                                packageNodeList.Remove(modNode.Name.ToString());
                                            //optional component
                                            else if (optionalPackageNodeList.Contains(modNode.Name.ToString()))
                                                optionalPackageNodeList.Remove(modNode.Name.ToString());
                                            //unknown component
                                            else
                                                unknownNodeList.Add(modNode.Name.ToString());
                                        }
                                        if (packageNodeList.Count > 0)
                                        Logging.Manager(string.Format("Error: modInfo.xml nodes not used: {0} => package {1} (line {2})",
                                            string.Join(",", packageNodeList), m.Name, ((IXmlLineInfo)modHolder).LineNumber));
                                        if (unknownNodeList.Count > 0)
                                            Logging.Manager(string.Format("Error: modInfo.xml unknown nodes: {0} => package {1} (line {2})",
                                                string.Join(",", unknownNodeList), m.PackageName, ((IXmlLineInfo)modHolder).LineNumber));
                                        if (m.PackageName.Equals(""))
                                        {
                                            m.PackageName = Utils.RandomString(30);
                                            Logging.Manager("PackageName is random generated: " + m.PackageName);
                                        }              // to avoid exceptions
                                        cat.Packages.Add(m);
                                        break;
                                    default:
                                        Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => cat {1} (line {2})",
                                            modHolder.Name.ToString(), cat.Name, ((IXmlLineInfo)modHolder).LineNumber));
                                        if (Program.testMode)
                                            MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: mod\n\nNode found: {0}\n\nmore informations, see logfile",
                                                modHolder.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                        break;
                                }
                            }
                            break;
                        case "dependencies":
                            //parse every dependency for that mod
                            foreach (XElement dependencyHolder in catagoryNode.Elements())
                            {
                                string[] depNodeList = new string[] { "packageName" };
                                Dependency d = new Dependency();
                                foreach (XElement dependencyNode in dependencyHolder.Elements())
                                {
                                    depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                    switch (dependencyNode.Name.ToString())
                                    {
                                        case "packageName":
                                            d.PackageName = dependencyNode.Value.Trim();
                                            if (d.PackageName.Equals(""))
                                            {
                                                Logging.Manager(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => cat {1} => dep {2} (line {3})",
                                                    dependencyNode.Name.ToString(), cat.Name, d.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                if (Program.testMode)
                                                {
                                                    MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => cat {1} => dep {2}",
                                                        dependencyNode.Name.ToString(), cat.Name, d.ZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                }
                                            }
                                            break;
                                        default:
                                            Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => cat {1} => dep {2} (line {3})",
                                                dependencyNode.Name, cat.Name, d.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                            if (Program.testMode)
                                            {
                                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                                    dependencyNode.Name), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            }
                                            break;
                                    }
                                }
                                if (depNodeList.Length > 0)
                                {
                                    Logging.Manager(string.Format("Error: modInfo.xml nodes not used: {0} => cat {1} => dep {2} (line {3})",
                                        string.Join(",", depNodeList), cat.Name, d.ZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber));
                                };
                                if (d.PackageName.Equals(""))
                                {
                                    d.PackageName = Utils.RandomString(30);
                                    Logging.Manager("PackageName is random generated: " + d.PackageName);
                                };              // to avoid exceptions
                                cat.Dependencies.Add(d);
                            }
                            break;
                        default:
                            Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => cat {1} (line {2})",
                                catagoryNode.Name.ToString(), cat.Name, ((IXmlLineInfo)catagoryNode).LineNumber));
                            if (Program.testMode)
                            {
                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: name, selectionType, mods, dependencies\n\nNode found: {0}\n\nmore informations, see logfile",
                                    catagoryNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                            break;
                    }
                }
                if (catNodeList.Length > 0)
                {
                    Logging.Manager(string.Format("Error: modInfo.xml nodes not used: {0} => cat {1} (line {2})",
                        string.Join(",", catNodeList), cat.Name, ((IXmlLineInfo)catagoryHolder).LineNumber));
                }
                parsedCatagoryList.Add(cat);
            }
            if (buildRefrences)
                Utils.BuildLinksRefrence(parsedCatagoryList);
        }
        //recursivly processes the configs
        public static void ProcessConfigs(XElement holder, SelectablePackage m, bool parentIsMod, int level, SelectablePackage con = null)
        {
            //parse every config for that mod
            foreach (XElement configHolder in holder.Elements())
            {
                switch (configHolder.Name.ToString())
                {
                    case "package":
                        List<string> packageNodeList = new List<string>() { "name", "zipFile", "crc", "enabled", "visible", "packageName", "type" };
                        List<string> optionalPackageNodeList = new List<string>() { "version", "timestamp", "startAddress", "endAddress", "size",
                            "description", "updateComment", "devURL", "userDatas", "medias", "dependencies", "logicalDependencies", "packages", "logAtInstall" };
                        List<string> unknownNodeList = new List<string>() { };
                        SelectablePackage c = new SelectablePackage()
                        {
                            Level = level
                        };
                        foreach (XElement configNode in configHolder.Elements())
                        {
                            switch (configNode.Name.ToString())
                            {
                                case "name":
                                    c.Name = configNode.Value;
                                    break;
                                case "version":
                                    c.Version = configNode.Value;
                                    break;
                                case "zipFile":
                                    c.ZipFile = configNode.Value;
                                    break;
                                case "timestamp":
                                    c.Timestamp = long.Parse("0" + configNode.Value);
                                    break;
                                case "startAddress":
                                    c.StartAddress = configNode.Value;
                                    break;
                                case "endAddress":
                                    c.EndAddress = configNode.Value;
                                    break;
                                case "logAtInstall":
                                    c.LogAtInstall = Utils.ParseBool(configNode.Value, true);
                                    break;
                                case "crc":
                                    c.CRC = configNode.Value;
                                    break;
                                case "enabled":
                                    c.Enabled = Utils.ParseBool(configNode.Value, false);
                                    break;
                                case "visible":
                                    c.Visible = Utils.ParseBool(configNode.Value, true);
                                    break;
                                case "packageName":
                                    c.PackageName = configNode.Value.Trim();
                                    if (c.PackageName.Equals(""))
                                    {
                                        Logging.Manager(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => config {1} ({2}) (line {3})",
                                            configNode.Name.ToString(), c.Name, c.ZipFile, ((IXmlLineInfo)configNode).LineNumber));
                                        if (Program.testMode)
                                            MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => config {1} ({2})",
                                                configNode.Name.ToString(), c.Name, c.ZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                    }
                                    break;
                                case "size":
                                    c.Size = Utils.ParseInt(configNode.Value, 0);
                                    break;
                                case "updateComment":
                                    c.UpdateComment = ConvertFromXmlSaveFormat(configNode.Value);
                                    break;
                                case "description":
                                    c.Description = ConvertFromXmlSaveFormat(configNode.Value);
                                    break;
                                case "devURL":
                                    c.DevURL = configNode.Value;
                                    break;
                                case "type":
                                    c.Type = configNode.Value;
                                    break;
                                case "packages":
                                    ProcessConfigs(configNode, m, false, c.Level+1 ,c);
                                    break;
                                case "userDatas":
                                    foreach (XElement userDataNode in configNode.Elements())
                                    {
                                        switch (userDataNode.Name.ToString())
                                        {
                                            case "userData":
                                                string innerText = userDataNode.Value;
                                                if (innerText == null)
                                                    continue;
                                                if (innerText.Equals(""))
                                                    continue;
                                                UserFiles uf = new UserFiles();
                                                uf.Pattern = innerText;
                                                if (userDataNode.Attribute("before") != null)
                                                    uf.placeBeforeExtraction = Utils.ParseBool(userDataNode.Attribute("before").Value, false);
                                                c.UserFiles.Add(uf);
                                                break;
                                            default:
                                                Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => userDatas => expected nodes: userData (line {3})",
                                                    userDataNode.Name.ToString(), c.Name, c.ZipFile, ((IXmlLineInfo)configNode).LineNumber));
                                                if (Program.testMode)
                                                    MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: userData\n\nNode found: {0}\n\nmore informations, see logfile",
                                                        userDataNode.Name.ToString()));
                                                break;
                                        }
                                    }
                                    break;
                                case "medias":
                                    //parse every picture
                                    foreach (XElement pictureHolder in configNode.Elements())
                                    {
                                        Media med = new Media();
                                        switch (pictureHolder.Name.ToString())
                                        {
                                            case "media":
                                                foreach (XElement pictureNode in pictureHolder.Elements())
                                                {
                                                    switch (pictureNode.Name.ToString())
                                                    {
                                                        case "URL":
                                                            string innerText = pictureNode.Value;
                                                            if (innerText == null)
                                                                continue;
                                                            if (innerText.Equals(""))
                                                                continue;
                                                            med.URL = innerText;
                                                            break;
                                                        case "type":
                                                            switch (Utils.ParseInt(pictureNode.Value, 1))
                                                            {
                                                                case 1:
                                                                    med.MediaType = MediaType.Picture;
                                                                    break;
                                                                case 2:
                                                                    med.MediaType = MediaType.Webpage;
                                                                    break;
                                                                case 3:
                                                                    med.MediaType = MediaType.MediaFile;
                                                                    break;
                                                                case 4:
                                                                    med.MediaType = MediaType.HTML;
                                                                    break;
                                                            }
                                                            break;
                                                        default:
                                                            Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => picture =>expected nodes: URL (line {3})",
                                                                pictureNode.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)pictureNode).LineNumber));
                                                            if (Program.testMode)
                                                                MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: URL\n\nNode found: {0}\n\nmore informations, see logfile",
                                                                    pictureNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                            break;
                                                    }
                                                }
                                                break;
                                            default:
                                                Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => expected node: picture (line {3})",
                                                    pictureHolder.Name, m.Name, m.ZipFile, ((IXmlLineInfo)pictureHolder).LineNumber));
                                                if (Program.testMode)
                                                    MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected node: picture\n\nNode found: {0}\n\nmore informations, see logfile",
                                                        pictureHolder.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                break;
                                        }
                                        c.PictureList.Add(med);
                                    }
                                    break;
                                case "dependencies":
                                    //parse all dependencies
                                    foreach (XElement dependencyHolder in configNode.Elements())
                                    {
                                        string[] depNodeList = new string[] { "packageName" };
                                        Dependency d = new Dependency();
                                        foreach (XElement dependencyNode in dependencyHolder.Elements())
                                        {
                                            depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                            switch (dependencyNode.Name.ToString())
                                            {
                                                case "packageName":
                                                    d.PackageName = dependencyNode.Value.Trim();
                                                    if (d.PackageName.Equals(""))
                                                    {
                                                        Logging.Manager(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => config {1} ({2}) => dep {3} (line {4})",
                                                            dependencyNode.Name.ToString(), c.Name, c.ZipFile, d.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                        if (Program.testMode)
                                                            MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => config {1} ({2}) => dep {3}",
                                                                dependencyNode.Name.ToString(), c.Name, c.ZipFile, d.ZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                    }
                                                    break;
                                                default:
                                                    Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => dep {3} (line {4})",
                                                        dependencyNode.Name.ToString(), c.Name, c.ZipFile, d.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                    if (Program.testMode)
                                                        MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                                            dependencyNode.Name.ToString()));
                                                    break;
                                            }
                                        }
                                        if (depNodeList.Length > 0)
                                            Logging.Manager(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) => dep {3} (line {4})",
                                                string.Join(",", depNodeList), c.Name, c.ZipFile, d.ZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber));
                                        if (d.PackageName.Equals(""))
                                        {
                                            d.PackageName = Utils.RandomString(30);
                                            Logging.Manager("PackageName is random generated: " + d.PackageName);
                                        }              // to avoid exceptions
                                        c.Dependencies.Add(d);
                                    }
                                    break;
                                case "logicalDependencies":
                                    //parse all dependencies
                                    foreach (XElement dependencyHolder in configNode.Elements())
                                    {
                                        string[] depNodeList = new string[] { "packageName", "negateFlag" };
                                        LogicalDependency d = new LogicalDependency();
                                        foreach (XElement dependencyNode in dependencyHolder.Elements())
                                        {
                                            depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                            switch (dependencyNode.Name.ToString())
                                            {
                                                case "packageName":
                                                    d.PackageName = dependencyNode.Value.Trim();
                                                    if (d.PackageName.Equals(""))
                                                    {
                                                        Logging.Manager(string.Format("Error modInfo.xml: PackageName not defined. node \"{0}\" => config {1} ({2}) => dep {3} (line {4})",
                                                            dependencyNode.Name.ToString(), c.Name, c.ZipFile, d.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                        if (Program.testMode)
                                                            MessageBox.Show(string.Format("modInfo.xml: PackageName not defined.\nnode \"{0}\" => config {1} ({2}) => dep {3}",
                                                                dependencyNode.Name.ToString(), c.Name, c.ZipFile, d.ZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                    }
                                                    break;
                                                case "negateFlag":
                                                    d.NegateFlag = Utils.ParseBool(dependencyNode.Value, true);
                                                    break;
                                                default:
                                                    Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => dep {3} (line {4})",
                                                        dependencyNode.Name.ToString(), c.Name, c.ZipFile, d.ZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                    if (Program.testMode)
                                                        MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: PackageName\n\nNode found: {0}\n\nmore informations, see logfile",
                                                            dependencyNode.Name.ToString()));
                                                    break;
                                            }
                                        }
                                        if (depNodeList.Length > 0)
                                            Logging.Manager(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) => dep {3} (line {4})",
                                                string.Join(",", depNodeList), c.Name, c.ZipFile, d.ZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber));
                                        if (d.PackageName.Equals(""))
                                        {
                                            d.PackageName = Utils.RandomString(30);
                                            Logging.Manager("PackageName is random generated: " + d.PackageName);
                                        }              // to avoid exceptions
                                        c.LogicalDependencies.Add(d);
                                    }
                                    break;
                                default:
                                    Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) (line {3})",
                                        configNode.Name.ToString(), c.Name, c.ZipFile, ((IXmlLineInfo)configNode).LineNumber));
                                    if (Program.testMode)
                                        MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: name, version, ZipFile, StartAddress, " +
                                            "EndAddress, CRC, Enabled, PackageName, size, updateComment, description, devURL, type, configs, userDatas, pictures, " +
                                            "dependencies\n\nNode found: {0}\n\nmore informations, see logfile",
                                            configNode.Name.ToString()));
                                    break;
                            }
                            //mandatory component
                            if (packageNodeList.Contains(configNode.Name.ToString()))
                                packageNodeList.Remove(configNode.Name.ToString());
                            //optional component
                            else if (optionalPackageNodeList.Contains(configNode.Name.ToString()))
                                optionalPackageNodeList.Remove(configNode.Name.ToString());
                            //unknown component
                            else
                                unknownNodeList.Add(configNode.Name.ToString());
                        }
                        if (packageNodeList.Count > 0)
                            Logging.Manager(string.Format("Error: modInfo.xml nodes not used: {0} => package {1} (line {2})",
                                string.Join(",", packageNodeList), m.Name, ((IXmlLineInfo)configHolder).LineNumber));
                        if (unknownNodeList.Count > 0)
                            Logging.Manager(string.Format("Error: modInfo.xml unknown nodes: {0} => package {1} (line {2})",
                                string.Join(",", unknownNodeList), m.PackageName, ((IXmlLineInfo)configHolder).LineNumber));
                        if (c.PackageName.Equals(""))
                        {
                            c.PackageName = Utils.RandomString(30);
                            Logging.Manager("PackageName is random generated: " + c.PackageName);
                        }              // to avoid exceptions
                        //attach it to eithor the config of correct level or the mod
                        if (parentIsMod)
                            m.Packages.Add(c);
                        else
                            con.Packages.Add(c);
                        break;
                    default:
                        Logging.Manager(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => expected nodes: config (line {3})",
                            configHolder.Name.ToString(), m.Name, m.ZipFile, ((IXmlLineInfo)configHolder).LineNumber));
                        if (Program.testMode)
                            MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: config\n\nNode found: {0}\n\nmore informations, see logfile",
                                configHolder.Name));
                        break;
                }
            }
        }
        
        //convert string with CR and/or LF from Xml save format
        private static string ConvertFromXmlSaveFormat(string s)
        {
            return s.TrimEnd().Replace("@", "\n").Replace(@"\r", "\r").Replace(@"\t", "\t").Replace(@"\n", "\n").Replace(@"&#92;", @"\");
        }
        
        //convert string with CR and/or LF to Xml save format
        private static string ConvertToXmlSaveFormat(string s)
        {
            return s.TrimEnd().Replace(@"\", @"&#92;").Replace("\r", @"\r").Replace("\t", @"\t").Replace("\n", @"\n");
        }
        
        //saves the mod database
        public static void SaveDatabase(string saveLocation, string gameVersion, string onlineFolderVersion, List<Dependency> globalDependencies, List<Dependency> dependencies, List<LogicalDependency> logicalDependencies, List<Category> parsedCatagoryList)
        {
            XmlDocument doc = new XmlDocument();
            //database root modInfo.xml
            XmlElement root = doc.CreateElement("modInfoAlpha.xml");
            root.SetAttribute("version", gameVersion);
            root.SetAttribute("onlineFolder", onlineFolderVersion);
            doc.AppendChild(root);
            //global dependencies
            XmlElement globalDependenciesXml = doc.CreateElement("globaldependencies");
            foreach (Dependency d in globalDependencies)
            {
                //declare dependency root
                XmlElement globalDependencyRoot = doc.CreateElement("globaldependency");
                //make dependency elements
                //mandatory
                XmlElement globalDepZipFile = doc.CreateElement("zipFile");
                if(!string.IsNullOrWhiteSpace(d.ZipFile))
                    globalDepZipFile.InnerText = d.ZipFile.Trim();
                globalDependencyRoot.AppendChild(globalDepZipFile);

                XmlElement globalDepCRC = doc.CreateElement("crc");
                if (!string.IsNullOrWhiteSpace(d.CRC))
                    globalDepCRC.InnerText = d.CRC.Trim();
                globalDependencyRoot.AppendChild(globalDepCRC);

                XmlElement globalDepEnabled = doc.CreateElement("enabled");
                globalDepEnabled.InnerText = "" + d.Enabled;
                globalDependencyRoot.AppendChild(globalDepEnabled);

                XmlElement globalDepAppendExtraction = doc.CreateElement("appendExtraction");
                globalDepAppendExtraction.InnerText = "" + d.AppendExtraction;
                globalDependencyRoot.AppendChild(globalDepAppendExtraction);

                XmlElement globalDepPackageName = doc.CreateElement("packageName");
                if (!string.IsNullOrWhiteSpace(d.PackageName))
                    globalDepPackageName.InnerText = d.PackageName.Trim();
                globalDependencyRoot.AppendChild(globalDepPackageName);

                //optional elements
                if (d.Timestamp > 0)
                {
                    XmlElement globalDepTimestamp = doc.CreateElement("timestamp");
                    globalDepTimestamp.InnerText = d.Timestamp.ToString();
                    globalDependencyRoot.AppendChild(globalDepTimestamp);
                }
                if (!d.StartAddress.Trim().Equals(Settings.DefaultStartAddress) && !string.IsNullOrWhiteSpace(d.StartAddress))
                {
                    XmlElement globalDepStartAddress = doc.CreateElement("startAddress");
                    globalDepStartAddress.InnerText = d.StartAddress.Trim();
                    globalDependencyRoot.AppendChild(globalDepStartAddress);
                }
                if (!d.EndAddress.Trim().Equals(Settings.DefaultEndAddress) && !string.IsNullOrWhiteSpace(d.EndAddress))
                {
                    XmlElement globalDepEndAddress = doc.CreateElement("endAddress");
                    globalDepEndAddress.InnerText = d.EndAddress.Trim();
                    globalDependencyRoot.AppendChild(globalDepEndAddress);
                }
                if (!string.IsNullOrWhiteSpace(d.DevURL))
                {
                    XmlElement globalDepURL = doc.CreateElement("devURL");
                    globalDepURL.InnerText = d.DevURL.Trim();
                    globalDependencyRoot.AppendChild(globalDepURL);
                }
                if (!d.LogAtInstall)
                {
                    XmlElement globalDepLogAtInstall = doc.CreateElement("logAtInstall");
                    globalDepLogAtInstall.InnerText = d.LogAtInstall.ToString();
                    globalDependencyRoot.AppendChild(globalDepLogAtInstall);
                }
                //attach dependency root
                globalDependenciesXml.AppendChild(globalDependencyRoot);
            }
            root.AppendChild(globalDependenciesXml);

            //dependencies
            XmlElement DependenciesXml = doc.CreateElement("dependencies");
            foreach (Dependency d in dependencies)
            {
                //declare dependency root
                XmlElement dependencyRoot = doc.CreateElement("dependency");
                //make dependency
                //mandatory
                XmlElement depZipFile = doc.CreateElement("zipFile");
                if (!string.IsNullOrWhiteSpace(d.ZipFile))
                    depZipFile.InnerText = d.ZipFile.Trim();
                dependencyRoot.AppendChild(depZipFile);

                XmlElement depCRC = doc.CreateElement("crc");
                if (!string.IsNullOrWhiteSpace(d.CRC))
                    depCRC.InnerText = d.CRC.Trim();
                dependencyRoot.AppendChild(depCRC);

                XmlElement depEnabled = doc.CreateElement("enabled");
                depEnabled.InnerText = "" + d.Enabled;
                dependencyRoot.AppendChild(depEnabled);

                XmlElement depAppendExtraction = doc.CreateElement("appendExtraction");
                depAppendExtraction.InnerText = "" + d.AppendExtraction;
                dependencyRoot.AppendChild(depAppendExtraction);

                XmlElement depPackageName = doc.CreateElement("packageName");
                if (!string.IsNullOrWhiteSpace(d.PackageName))
                    depPackageName.InnerText = d.PackageName.Trim();
                dependencyRoot.AppendChild(depPackageName);
                //optional
                if(d.Timestamp > 0)
                {
                    XmlElement depTimestamp = doc.CreateElement("timestamp");
                    depTimestamp.InnerText = d.Timestamp.ToString();
                    dependencyRoot.AppendChild(depTimestamp);
                }
                if(!d.StartAddress.Trim().Equals(Settings.DefaultStartAddress) && !string.IsNullOrWhiteSpace(d.StartAddress))
                {
                    XmlElement depStartAddress = doc.CreateElement("startAddress");
                    depStartAddress.InnerText = d.StartAddress.Trim();
                    dependencyRoot.AppendChild(depStartAddress);
                }
                if(!d.EndAddress.Trim().Equals(Settings.DefaultEndAddress) && !string.IsNullOrWhiteSpace(d.EndAddress))
                {
                    XmlElement depEndAddress = doc.CreateElement("endAddress");
                    depEndAddress.InnerText = d.EndAddress.Trim();
                    dependencyRoot.AppendChild(depEndAddress);
                }
                if(!string.IsNullOrWhiteSpace(d.DevURL.Trim()))
                {
                    XmlElement depdevURL = doc.CreateElement("devURL");
                    depdevURL.InnerText = d.DevURL.Trim();
                    dependencyRoot.AppendChild(depdevURL);
                }
                if (!d.LogAtInstall)
                {
                    XmlElement depLogAtInstall = doc.CreateElement("logAtInstall");
                    depLogAtInstall.InnerText = d.LogAtInstall.ToString();
                    dependencyRoot.AppendChild(depLogAtInstall);
                }

                //logicalDependencies for the configs
                if (d.LogicalDependencies.Count > 0)
                {
                    XmlElement depLogicalDependencies = doc.CreateElement("logicalDependencies");
                    foreach (LogicalDependency ld in d.LogicalDependencies)
                    {
                        //declare logicalDependency root
                        XmlElement LogicalDependencyRoot = doc.CreateElement("logicalDependency");
                        //make logicalDependency
                        XmlElement LogicalDependencyPackageName = doc.CreateElement("packageName");
                        LogicalDependencyPackageName.InnerText = ld.PackageName.Trim();
                        LogicalDependencyRoot.AppendChild(LogicalDependencyPackageName);
                        XmlElement LogicalDependencyNegateFlag = doc.CreateElement("negateFlag");
                        LogicalDependencyNegateFlag.InnerText = "" + ld.NegateFlag;
                        LogicalDependencyRoot.AppendChild(LogicalDependencyNegateFlag);
                        //attach logicalDependency root
                        depLogicalDependencies.AppendChild(LogicalDependencyRoot);
                    }
                    dependencyRoot.AppendChild(depLogicalDependencies);
                }
                DependenciesXml.AppendChild(dependencyRoot);
            }
            root.AppendChild(DependenciesXml);
            //dependencies
            XmlElement logicalDependenciesXml = doc.CreateElement("logicalDependencies");
            foreach (LogicalDependency d in logicalDependencies)
            {
                //declare dependency root
                XmlElement logicalDependencyRoot = doc.CreateElement("logicalDependency");
                //make dependency
                //mandatory
                XmlElement logicalDepZipFile = doc.CreateElement("zipFile");
                if (!d.ZipFile.Trim().Equals(""))
                    logicalDepZipFile.InnerText = d.ZipFile.Trim();
                logicalDependencyRoot.AppendChild(logicalDepZipFile);

                XmlElement logicalDepCRC = doc.CreateElement("crc");
                if (!d.ZipFile.Trim().Equals(""))
                    logicalDepCRC.InnerText = d.CRC.Trim();
                logicalDependencyRoot.AppendChild(logicalDepCRC);

                XmlElement logicalDepEnabled = doc.CreateElement("enabled");
                logicalDepEnabled.InnerText = "" + d.Enabled;
                logicalDependencyRoot.AppendChild(logicalDepEnabled);

                XmlElement logicalDepPackageName = doc.CreateElement("packageName");
                if (!d.ZipFile.Trim().Equals(""))
                    logicalDepPackageName.InnerText = d.PackageName.Trim();
                logicalDependencyRoot.AppendChild(logicalDepPackageName);

                XmlElement logicalDepLogic = doc.CreateElement("logic");
                logicalDepLogic.InnerText = LogicalDependency.GetAndOrString(d.AndOrLogic);
                logicalDependencyRoot.AppendChild(logicalDepLogic);
                //optional
                if (d.Timestamp > 0)
                {
                    XmlElement logicalDepTimestamp = doc.CreateElement("timestamp");
                    logicalDepTimestamp.InnerText = d.Timestamp.ToString();
                    logicalDependencyRoot.AppendChild(logicalDepTimestamp);
                }
                if (!d.StartAddress.Trim().Equals(Settings.DefaultStartAddress) && !string.IsNullOrWhiteSpace(d.StartAddress))
                {
                    XmlElement logicalDepStartAddress = doc.CreateElement("startAddress");
                    logicalDepStartAddress.InnerText = d.StartAddress.Trim();
                    logicalDependencyRoot.AppendChild(logicalDepStartAddress);
                }
                if (!d.StartAddress.Trim().Equals(Settings.DefaultEndAddress) && !string.IsNullOrWhiteSpace(d.EndAddress))
                {
                    XmlElement logicalDepEndAddress = doc.CreateElement("endAddress");
                    logicalDepEndAddress.InnerText = d.EndAddress.Trim();
                    logicalDependencyRoot.AppendChild(logicalDepEndAddress);
                }
                if(!string.IsNullOrWhiteSpace(d.DevURL))
                {
                    XmlElement logicalDepdevURL = doc.CreateElement("devURL");
                    logicalDepdevURL.InnerText = d.DevURL.Trim();
                    logicalDependencyRoot.AppendChild(logicalDepdevURL);
                }
                if (!d.LogAtInstall)
                {
                    XmlElement logicalDepLogAtInstall = doc.CreateElement("logAtInstall");
                    logicalDepLogAtInstall.InnerText = d.LogAtInstall.ToString();
                    logicalDependencyRoot.AppendChild(logicalDepLogAtInstall);
                }
                //attach dependency root
                logicalDependenciesXml.AppendChild(logicalDependencyRoot);
            }
            root.AppendChild(logicalDependenciesXml);
            //catagories
            XmlElement catagoriesHolder = doc.CreateElement("catagories");
            foreach (Category c in parsedCatagoryList)
            {
                //catagory root
                XmlElement catagoryRoot = doc.CreateElement("catagory");
                //make catagory
                XmlElement catagoryName = doc.CreateElement("name");
                catagoryName.InnerText = c.Name.Trim();
                catagoryRoot.AppendChild(catagoryName);
                XmlElement catagoryInstallGroup = doc.CreateElement("installGroup");
                catagoryInstallGroup.InnerText = "" + c.InstallGroup;
                catagoryRoot.AppendChild(catagoryInstallGroup);
                //dependencies for catagory
                XmlElement catagoryDependencies = doc.CreateElement("dependencies");
                foreach (Dependency d in c.Dependencies)
                {
                    //declare dependency root
                    XmlElement DependencyRoot = doc.CreateElement("dependency");
                    XmlElement DepPackageName = doc.CreateElement("packageName");
                    DepPackageName.InnerText = d.PackageName.Trim();
                    DependencyRoot.AppendChild(DepPackageName);
                    //attach dependency root
                    catagoryDependencies.AppendChild(DependencyRoot);
                }
                catagoryRoot.AppendChild(catagoryDependencies);
                //mods for catagory
                XmlElement modsHolder = doc.CreateElement("packages");
                foreach (SelectablePackage m in c.Packages)
                {
                    //add it to the list
                    XmlElement modRoot = doc.CreateElement("package");
                    //mandatory
                    XmlElement modName = doc.CreateElement("name");
                    if (!string.IsNullOrWhiteSpace(m.Name))
                        modName.InnerText = m.Name.Trim();
                    modRoot.AppendChild(modName);
                    XmlElement modZipFile = doc.CreateElement("zipFile");
                    if (!string.IsNullOrWhiteSpace(m.ZipFile))
                        modZipFile.InnerText = m.ZipFile.Trim();
                    modRoot.AppendChild(modZipFile);
                    XmlElement modZipCRC = doc.CreateElement("crc");
                    if (!string.IsNullOrWhiteSpace(m.CRC))
                        modZipCRC.InnerText = m.CRC.Trim();
                    modRoot.AppendChild(modZipCRC);
                    XmlElement modEnabled = doc.CreateElement("enabled");
                    modEnabled.InnerText = "" + m.Enabled;
                    modRoot.AppendChild(modEnabled);
                    XmlElement modVisible = doc.CreateElement("visible");
                    modVisible.InnerText = "" + m.Visible;
                    modRoot.AppendChild(modVisible);
                    XmlElement modPackageName = doc.CreateElement("packageName");
                    if (!string.IsNullOrWhiteSpace(m.PackageName))
                        modPackageName.InnerText = m.PackageName.Trim();
                    modRoot.AppendChild(modPackageName);
                    XmlElement modType = doc.CreateElement("type");
                    if (!string.IsNullOrWhiteSpace(m.Type))
                        modType.InnerText = m.Type.Trim();
                    modRoot.AppendChild(modType);
                    //optional
                    if (!string.IsNullOrWhiteSpace(m.Version))
                    {
                        XmlElement modVersion = doc.CreateElement("version");
                        modVersion.InnerText = m.Version.Trim();
                        modRoot.AppendChild(modVersion);
                    }
                    if(m.Timestamp > 0)
                    {
                        XmlElement modTimestamp = doc.CreateElement("timestamp");
                        modTimestamp.InnerText = m.Timestamp.ToString();
                        modRoot.AppendChild(modTimestamp);
                    }
                    if(!m.StartAddress.Trim().Equals(Settings.DefaultStartAddress) && !string.IsNullOrWhiteSpace(m.StartAddress))
                    {
                        XmlElement modStartAddress = doc.CreateElement("startAddress");
                        modStartAddress.InnerText = m.StartAddress.Trim();
                        modRoot.AppendChild(modStartAddress);
                    }
                    if(!m.EndAddress.Trim().Equals(Settings.DefaultEndAddress) && !string.IsNullOrWhiteSpace(m.StartAddress))
                    {
                        XmlElement modEndAddress = doc.CreateElement("endAddress");
                        modEndAddress.InnerText = m.EndAddress.Trim();
                        modRoot.AppendChild(modEndAddress);
                    }
                    if(m.Size > 0)
                    {
                        XmlElement modZipSize = doc.CreateElement("size");
                        modZipSize.InnerText = "" + m.Size;
                        modRoot.AppendChild(modZipSize);
                    }
                    if (!m.LogAtInstall)
                    {
                        XmlElement modLogAtInstall = doc.CreateElement("logAtInstall");
                        modLogAtInstall.InnerText = m.LogAtInstall.ToString();
                        modRoot.AppendChild(modLogAtInstall);
                    }
                    if (!string.IsNullOrWhiteSpace(m.UpdateComment))
                    {
                        XmlElement modUpdateComment = doc.CreateElement("updateComment");
                        modUpdateComment.InnerText = ConvertToXmlSaveFormat(m.UpdateComment);
                        modRoot.AppendChild(modUpdateComment);
                    }
                    if(!string.IsNullOrWhiteSpace(m.Description))
                    {
                        XmlElement modDescription = doc.CreateElement("description");
                        modDescription.InnerText = ConvertToXmlSaveFormat(m.Description);
                        modRoot.AppendChild(modDescription);
                    }
                    if(!string.IsNullOrWhiteSpace(m.DevURL))
                    {
                        XmlElement modDevURL = doc.CreateElement("devURL");
                        modDevURL.InnerText = m.DevURL.Trim();
                        modRoot.AppendChild(modDevURL);
                    }
                    //datas for the mods
                    if(m.UserFiles.Count > 0)
                    {
                        XmlElement modDatas = doc.CreateElement("userDatas");
                        foreach (UserFiles us in m.UserFiles)
                        {
                            XmlElement userData = doc.CreateElement("userData");
                            userData.InnerText = us.Pattern.Trim();
                            userData.SetAttribute("before", "" + us.placeBeforeExtraction);
                            modDatas.AppendChild(userData);
                        }
                        modRoot.AppendChild(modDatas);
                    }
                    //pictures for the mods
                    if(m.PictureList.Count > 0)
                    {
                        XmlElement modPictures = doc.CreateElement("medias");
                        foreach (Media p in m.PictureList)
                        {
                            XmlElement pictureRoot = doc.CreateElement("media");
                            XmlElement pictureType = doc.CreateElement("type");
                            XmlElement pictureURL = doc.CreateElement("URL");
                            pictureURL.InnerText = p.URL.Trim();
                            pictureType.InnerText = "" + (int)p.MediaType;
                            pictureRoot.AppendChild(pictureType);
                            pictureRoot.AppendChild(pictureURL);
                            modPictures.AppendChild(pictureRoot);
                        }
                        modRoot.AppendChild(modPictures);
                    }
                    if (m.Packages.Count > 0)
                    {
                        //configs for the mods
                        XmlElement configsHolder = doc.CreateElement("packages");
                        SaveDatabaseConfigLevel(doc, configsHolder, m.Packages);
                        modRoot.AppendChild(configsHolder);
                    }
                    if(m.Dependencies.Count > 0)
                    {
                        XmlElement modDependencies = doc.CreateElement("dependencies");
                        foreach (Dependency d in m.Dependencies)
                        {
                            //declare dependency root
                            XmlElement DependencyRoot = doc.CreateElement("dependency");
                            //make dependency
                            XmlElement DepPackageName = doc.CreateElement("packageName");
                            DepPackageName.InnerText = d.PackageName.Trim();
                            DependencyRoot.AppendChild(DepPackageName);
                            //attach dependency root
                            modDependencies.AppendChild(DependencyRoot);
                        }
                        modRoot.AppendChild(modDependencies);
                    }
                    //logicalDependencies for the configs
                    if(m.LogicalDependencies.Count > 0)
                    {
                        XmlElement modLogicalDependencies = doc.CreateElement("logicalDependencies");
                        foreach (LogicalDependency ld in m.LogicalDependencies)
                        {
                            //declare logicalDependency root
                            XmlElement LogicalDependencyRoot = doc.CreateElement("logicalDependency");
                            //make logicalDependency
                            XmlElement LogicalDependencyPackageName = doc.CreateElement("packageName");
                            LogicalDependencyPackageName.InnerText = ld.PackageName.Trim();
                            LogicalDependencyRoot.AppendChild(LogicalDependencyPackageName);
                            XmlElement LogicalDependencyNegateFlag = doc.CreateElement("negateFlag");
                            LogicalDependencyNegateFlag.InnerText = "" + ld.NegateFlag;
                            LogicalDependencyRoot.AppendChild(LogicalDependencyNegateFlag);
                            //attach logicalDependency root
                            modLogicalDependencies.AppendChild(LogicalDependencyRoot);
                        }
                        modRoot.AppendChild(modLogicalDependencies);
                    }
                    modsHolder.AppendChild(modRoot);
                }
                catagoryRoot.AppendChild(modsHolder);
                //append catagory
                catagoriesHolder.AppendChild(catagoryRoot);
            }
            root.AppendChild(catagoriesHolder);

            // Create an XML declaration.
            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");

            // Add the new node to the document.
            XmlElement cDoc = doc.DocumentElement;
            doc.InsertBefore(xmldecl, cDoc);

            // save database file
            doc.Save(saveLocation);
        }

        private static void SaveDatabaseConfigLevel(XmlDocument doc, XmlElement configsHolder, List<SelectablePackage> configsList)
        {
            foreach (SelectablePackage cc in configsList)
            {
                //add the config to the list
                XmlElement configRoot = doc.CreateElement("package");
                configsHolder.AppendChild(configRoot);
                //mandatory
                XmlElement configName = doc.CreateElement("name");
                if (!string.IsNullOrWhiteSpace(cc.Name))
                    configName.InnerText = cc.Name.Trim();
                configRoot.AppendChild(configName);
                XmlElement configZipFile = doc.CreateElement("zipFile");
                if (!string.IsNullOrWhiteSpace(cc.ZipFile))
                    configZipFile.InnerText = cc.ZipFile.Trim();
                configRoot.AppendChild(configZipFile);
                XmlElement configZipCRC = doc.CreateElement("crc");
                if (!string.IsNullOrWhiteSpace(cc.CRC))
                    configZipCRC.InnerText = cc.CRC.Trim();
                configRoot.AppendChild(configZipCRC);
                XmlElement configEnabled = doc.CreateElement("enabled");
                configEnabled.InnerText = "" + cc.Enabled;
                configRoot.AppendChild(configEnabled);
                XmlElement configVisible = doc.CreateElement("visible");
                configVisible.InnerText = "" + cc.Visible;
                configRoot.AppendChild(configVisible);
                XmlElement configPackageName = doc.CreateElement("packageName");
                if (!string.IsNullOrWhiteSpace(cc.PackageName))
                    configPackageName.InnerText = cc.PackageName.Trim();
                configRoot.AppendChild(configPackageName);
                XmlElement configType = doc.CreateElement("type");
                if (!string.IsNullOrWhiteSpace(cc.Type))
                    configType.InnerText = cc.Type;
                configRoot.AppendChild(configType);
                //optional
                if(!string.IsNullOrWhiteSpace(cc.Version))
                {
                    XmlElement configVersion = doc.CreateElement("version");
                    configVersion.InnerText = cc.Version.Trim();
                    configRoot.AppendChild(configVersion);
                }
                if(cc.Timestamp > 0)
                {
                    XmlElement configTimestamp = doc.CreateElement("timestamp");
                    configTimestamp.InnerText = cc.Timestamp.ToString();
                    configRoot.AppendChild(configTimestamp);
                }
                if(!cc.StartAddress.Trim().Equals(Settings.DefaultStartAddress) && !string.IsNullOrWhiteSpace(cc.StartAddress))
                {
                    XmlElement configStartAddress = doc.CreateElement("startAddress");
                    configStartAddress.InnerText = cc.StartAddress.Trim();
                    configRoot.AppendChild(configStartAddress);
                }
                if(!cc.EndAddress.Trim().Equals(Settings.DefaultEndAddress) && !string.IsNullOrWhiteSpace(cc.StartAddress))
                {
                    XmlElement configEndAddress = doc.CreateElement("endAddress");
                    configEndAddress.InnerText = cc.EndAddress.Trim();
                    configRoot.AppendChild(configEndAddress);
                }
                if(cc.Size > 0)
                {
                    XmlElement configSize = doc.CreateElement("size");
                    configSize.InnerText = "" + cc.Size;
                    configRoot.AppendChild(configSize);
                }
                if (!cc.LogAtInstall)
                {
                    XmlElement configLogAtInstall = doc.CreateElement("logAtInstall");
                    configLogAtInstall.InnerText = cc.LogAtInstall.ToString();
                    configRoot.AppendChild(configLogAtInstall);
                }
                if (!string.IsNullOrWhiteSpace(cc.UpdateComment))
                {
                    XmlElement configComment = doc.CreateElement("updateComment");
                    configComment.InnerText = ConvertToXmlSaveFormat(cc.UpdateComment);
                    configRoot.AppendChild(configComment);
                }
                if(!string.IsNullOrWhiteSpace(cc.Description))
                {
                    XmlElement configDescription = doc.CreateElement("description");
                    configDescription.InnerText = ConvertToXmlSaveFormat(cc.Description);
                    configRoot.AppendChild(configDescription);
                }
                if(!string.IsNullOrWhiteSpace(cc.DevURL))
                {
                    XmlElement configDevURL = doc.CreateElement("devURL");
                    configDevURL.InnerText = cc.DevURL.Trim();
                    configRoot.AppendChild(configDevURL);
                }
                //datas for the mods
                if(cc.UserFiles.Count > 0)
                {
                    XmlElement configDatas = doc.CreateElement("userDatas");
                    foreach (UserFiles us in cc.UserFiles)
                    {
                        XmlElement userData = doc.CreateElement("userData");
                        userData.InnerText = us.Pattern.Trim();
                        userData.SetAttribute("before", "" + us.placeBeforeExtraction);
                        configDatas.AppendChild(userData);
                    }
                    configRoot.AppendChild(configDatas);
                }
                //pictures for the configs
                if(cc.PictureList.Count > 0)
                {
                    XmlElement configPictures = doc.CreateElement("medias");
                    foreach (Media p in cc.PictureList)
                    {
                        XmlElement pictureRoot = doc.CreateElement("media");
                        XmlElement pictureType = doc.CreateElement("type");
                        XmlElement pictureURL = doc.CreateElement("URL");
                        pictureURL.InnerText = p.URL.Trim();
                        pictureType.InnerText = "" + (int)p.MediaType;
                        pictureRoot.AppendChild(pictureType);
                        pictureRoot.AppendChild(pictureURL);
                        configPictures.AppendChild(pictureRoot);
                    }
                    configRoot.AppendChild(configPictures);
                }
                //packages for the packages (meta)
                if(cc.Packages.Count > 0)
                {
                    XmlElement configsHolderSub = doc.CreateElement("packages");
                    //if statement here
                    SaveDatabaseConfigLevel(doc, configsHolderSub, cc.Packages);
                    configRoot.AppendChild(configsHolderSub);
                }
                //dependencies for the packages
                if(cc.Dependencies.Count > 0)
                {
                    XmlElement catDependencies = doc.CreateElement("dependencies");
                    foreach (Dependency d in cc.Dependencies)
                    {
                        //declare dependency root
                        XmlElement DependencyRoot = doc.CreateElement("dependency");
                        //make dependency
                        XmlElement DepPackageName = doc.CreateElement("packageName");
                        DepPackageName.InnerText = d.PackageName.Trim();
                        DependencyRoot.AppendChild(DepPackageName);
                        //attach dependency root
                        catDependencies.AppendChild(DependencyRoot);
                    }
                    configRoot.AppendChild(catDependencies);
                }
                //logicalDependencies for the packages
                if(cc.LogicalDependencies.Count > 0)
                {
                    XmlElement conLogicalDependencies = doc.CreateElement("logicalDependencies");
                    foreach (LogicalDependency ld in cc.LogicalDependencies)
                    {
                        //declare logicalDependency root
                        XmlElement LogicalDependencyRoot = doc.CreateElement("logicalDependency");
                        //make logicalDependency
                        XmlElement LogicalDependencyPackageName = doc.CreateElement("packageName");
                        LogicalDependencyPackageName.InnerText = ld.PackageName.Trim();
                        LogicalDependencyRoot.AppendChild(LogicalDependencyPackageName);
                        XmlElement LogicalDependencyNegateFlag = doc.CreateElement("negateFlag");
                        LogicalDependencyNegateFlag.InnerText = "" + ld.NegateFlag;
                        LogicalDependencyRoot.AppendChild(LogicalDependencyNegateFlag);
                        //attach logicalDependency root
                        conLogicalDependencies.AppendChild(LogicalDependencyRoot);
                    }
                    configRoot.AppendChild(conLogicalDependencies);
                }
                configsHolder.AppendChild(configRoot);
            }
        }

        private static void ParseDeveloperSelections(XDocument doc)
        {
            DeveloperSelections d;
            var xMembers = from members in doc.Descendants("selections").Elements() select members;
            foreach (XElement x in xMembers)
            {
                d = new DeveloperSelections()
                {
                    internalName = x.Value,
                    displayName = x.Attribute("displayName").Value,
                    date = x.Attribute("date").Value
                };
                developerSelections.Add(d);
            }
        }
        //returns the md5 hash of the file based on the input file string location. It is searching in the database first. If not found in database or the filetime is not the same, it will create a new Hash and update the database
        public static string GetMd5Hash(string inputFile)
        {
            if (Program.databaseUpdateOnline)
            {
                // if in databaseupdate mode, the online databse is downloader to get the informations of all accessable online files of this gameVersion
                try
                {
                    XDocument doc = XDocument.Load(Settings.OnlineDatabaseXmlFile);
                    try
                    {
                        XElement element = doc.Descendants("file")
                           .Where(arg => arg.Attribute("name").Value == inputFile)
                           .Single();
                        return element.Attribute("md5").Value;
                    }
                    catch (InvalidOperationException)
                    {
                        // catch the Exception if no entry is found
                    }
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("getMd5Hash", "read from databaseupdate", ex);
                }
                return "f";
            }

            // check if databse exists and if not, create it
            XMLUtils.CreateMd5HashDatabase();
            // get filetime from file, convert it to string with base 10
            string tempFiletime = Convert.ToString(File.GetLastWriteTime(inputFile).ToFileTime(), 10);
            // extract filename with path
            string tempFilename = Path.GetFileName(inputFile);
            // check database for filename with filetime
            string tempHash = XMLUtils.GetMd5HashDatabase(tempFilename, tempFiletime);
            if (tempHash == "-1")   // file not found in database
            {
                // create Md5Hash from file
                tempHash = Utils.CreateMd5Hash(inputFile);

                if (tempHash == "-1")
                {
                    // no file found, then delete from database
                    XMLUtils.DeleteMd5HashDatabase(tempFilename);
                }
                else
                {
                    // file found. update the database with new values
                    XMLUtils.UpdateMd5HashDatabase(tempFilename, tempHash, tempFiletime);
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

        public static void CreateMd5HashDatabase()
        {
            if (!File.Exists(Settings.MD5HashDatabaseXmlFile))
            {
                XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("database"));
                doc.Save(Settings.MD5HashDatabaseXmlFile);
            }
        }
        // need filename and filetime to check the database
        public static string GetMd5HashDatabase(string inputFile, string inputFiletime)
        {
            try
            {
                XDocument doc = XDocument.Load(Settings.MD5HashDatabaseXmlFile);
                bool exists = doc.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value == inputFile && arg.Attribute("filetime").Value == inputFiletime)
                       .Any();
                if (exists)
                {
                    XElement element = doc.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value == inputFile && arg.Attribute("filetime").Value == inputFiletime)
                       .Single();
                    return element.Attribute("md5").Value;
                }
            }
            catch (Exception e)
            {
                Utils.ExceptionLog("getMd5HashDatabase", e);
                File.Delete(Settings.MD5HashDatabaseXmlFile);     // delete damaged XML database
                CreateMd5HashDatabase();                            // create new XML database
            }
            return "-1";
        }

        public static void UpdateMd5HashDatabase(string inputFile, string inputMd5Hash, string inputFiletime)
        {
            try
            {
                XDocument doc = XDocument.Load(Settings.MD5HashDatabaseXmlFile);
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
                    doc.Element("database").Add(new XElement("file", new XAttribute("filename", inputFile), new XAttribute("filetime", inputFiletime), new XAttribute("md5", inputMd5Hash)));
                }
                doc.Save(Settings.MD5HashDatabaseXmlFile);
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("updateMd5HashDatabase", ex);
            }
        }

        public static void DeleteMd5HashDatabase(string inputFile)
        {
            // only for caution
            XMLUtils.CreateMd5HashDatabase();

            // extract filename from path (if call with full path)
            string tempFilename = Path.GetFileName(inputFile);

            XDocument doc = XDocument.Load(Settings.MD5HashDatabaseXmlFile);
            try
            {
                doc.Descendants("file").Where(arg => arg.Attribute("filename").Value == tempFilename).Remove();
                doc.Save(Settings.MD5HashDatabaseXmlFile);
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }
    }
}
