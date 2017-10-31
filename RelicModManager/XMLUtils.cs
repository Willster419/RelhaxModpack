using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace RelhaxModpack
{
    public static class XMLUtils
    {
        public static int TotalModConfigComponents = 0;

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

        public static string ReadVersionFromModInfo(string f)
        {
            XDocument doc = XDocument.Load(f);
            try
            {
                XElement element = doc.Descendants("modInfoAlpha.xml").Single();
                return element.Attribute("version").Value;
            }
            catch (InvalidOperationException)
            {
                return "error"; // catch the Exception if no entry is found
            }
        }

        public static string ReadOnlineFolderFromModInfo(string f)
        {
            XDocument doc = XDocument.Load(f);
            try
            {
                XElement element = doc.Descendants("modInfoAlpha.xml").Single();
                return element.Attribute("onlineFolder").Value;
            }
            catch (InvalidOperationException)
            {
                return "error"; // catch the Exception if no entry is found
            }
        }
        //parses the xml mod info into the memory database (change XML reader from XMLDocument to XDocument)
        // https://www.google.de/search?q=c%23+xdocument+get+line+number&oq=c%23+xdocument+get+line+number&aqs=chrome..69i57j69i58.11773j0j7&sourceid=chrome&ie=UTF-8
        // public static void createModStructure(string databaseURL, List<Dependency> globalDependencies, List<Dependency> dependencies, List<LogicalDependnecy> logicalDependencies, List<Category> parsedCatagoryList, List<DeveloperSelections> developerSelections = null)
        public static void CreateModStructure(string databaseURL, List<Dependency> globalDependencies, List<Dependency> dependencies, List<LogicalDependnecy> logicalDependencies, List<Category> parsedCatagoryList)
        {
            try
            {
                MainWindow.developerSelections.Clear();
                TotalModConfigComponents = 0;
                XDocument doc = null;
                try
                {
                    if (databaseURL.ToLower().Equals(Settings.modInfoDatFile.ToLower()))
                    {
                        Utils.AppendToLog("loading dat config file");
                        string xmlString = Utils.GetStringFromZip(Settings.modInfoDatFile, "modInfo.xml");
                        doc = XDocument.Parse(xmlString, LoadOptions.SetLineInfo);
                        // create new developerSelections NameList
                        ParseDeveloperSelections(doc);
                    }
                    else
                    {
                        Utils.AppendToLog("loading local config file");
                        doc = XDocument.Load(databaseURL, LoadOptions.SetLineInfo);
                    }
                }
                catch (XmlException ex)
                {
                    Utils.AppendToLog(string.Format("CRITICAL: Failed to read database: {0}\nMessage: {1}", databaseURL, ex.Message));
                    MessageBox.Show(Translations.getTranslatedString("databaseReadFailed"));
                    Application.Exit();
                    return;
                }
                catch (Exception ex)
                {
                    Utils.ExceptionLog("createModStructure", string.Format("tried to access {0}", databaseURL), ex);
                    MessageBox.Show(Translations.getTranslatedString("databaseNotFound"));
                    Application.Exit();
                    return;
                }
                //add the global dependencies
                foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/globaldependencies/globaldependency"))
                {
                    string[] depNodeList = new string[] { "dependencyZipFile", "dependencyZipCRC", "startAddress", "endAddress", "dependencyenabled", "appendExtraction", "packageName", "devURL", "timestamp" };
                    Dependency d = new Dependency();
                    d.packageName = "";
                    foreach (XElement globs in dependencyNode.Elements())
                    {
                        depNodeList = depNodeList.Except(new string[] { globs.Name.ToString() }).ToArray();
                        switch (globs.Name.ToString())
                        {
                            case "dependencyZipFile":
                                d.dependencyZipFile = globs.Value;
                                break;
                            case "timestamp":
                                d.timestamp = long.Parse("0" + globs.Value);
                                break;
                            case "dependencyZipCRC":
                                d.dependencyZipCRC = globs.Value;
                                break;
                            case "startAddress":
                                d.startAddress = globs.Value;
                                break;
                            case "endAddress":
                                d.endAddress = globs.Value;
                                break;
                            case "devURL":
                                d.devURL = globs.Value;
                                break;
                            case "dependencyenabled":
                                d.enabled = Utils.ParseBool(globs.Value, false);
                                break;
                            case "appendExtraction":
                                d.appendExtraction = Utils.ParseBool(globs.Value, false);
                                break;
                            case "packageName":
                                d.packageName = globs.Value.Trim();
                                if (d.packageName.Equals(""))
                                {
                                    Utils.AppendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => globsPend {1} (line {2})", globs.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)globs).LineNumber));
                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => globsPend {1}\n\nmore informations, see logfile", globs.Name.ToString(), d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                }
                                break;
                            default:
                                Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => globsPend {1} (line {2})", globs.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)globs).LineNumber));
                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: dependencyZipFile, dependencyZipCRC, startAddress, endAddress, dependencyenabled, packageName\n\nNode found: {0}\n\nmore informations, see logfile", globs.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                break;
                        }
                    }
                    if (d != null)
                    {
                        if (depNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => globsPend {1} (line {2})", string.Join(",", depNodeList), d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber)); };
                        if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.AppendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                        globalDependencies.Add(d);
                    };
                }
                //add the dependencies
                foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/dependencies/dependency"))
                {
                    string[] depNodeList = new string[] { "dependencyZipFile", "dependencyZipCRC", "startAddress", "endAddress", "dependencyenabled", "appendExtraction", "packageName", "logicalDependencies", "devURL", "timestamp", "shortCuts" };
                    Dependency d = new Dependency();
                    d.packageName = "";
                    foreach (XElement globs in dependencyNode.Elements())
                    {
                        depNodeList = depNodeList.Except(new string[] { globs.Name.ToString() }).ToArray();
                        switch (globs.Name.ToString())
                        {
                            case "dependencyZipFile":
                                d.dependencyZipFile = globs.Value;
                                break;
                            case "timestamp":
                                d.timestamp = long.Parse("0" + globs.Value);
                                break;
                            case "dependencyZipCRC":
                                d.dependencyZipCRC = globs.Value;
                                break;
                            case "startAddress":
                                d.startAddress = globs.Value;
                                break;
                            case "endAddress":
                                d.endAddress = globs.Value;
                                break;
                            case "devURL":
                                d.devURL = globs.Value;
                                break;
                            case "dependencyenabled":
                                d.enabled = Utils.ParseBool(globs.Value, false);
                                break;
                            case "appendExtraction":
                                d.appendExtraction = Utils.ParseBool(globs.Value, false);
                                break;
                            case "packageName":
                                d.packageName = globs.Value.Trim();
                                if (d.packageName.Equals(""))
                                {
                                    Utils.AppendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => globsPend {1} (line {2})", globs.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)globs).LineNumber));
                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => globsPend {1}\n\nmore informations, see logfile", globs.Name.ToString(), d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                }
                                break;
                            case "shortCuts":
                                //parse all shortCuts
                                foreach (XElement shortCutHolder in globs.Elements())
                                {
                                    ShortCut sc = new ShortCut();
                                    string[] depScNodeList = new string[] { "path", "name", "enabled" };
                                    foreach (XElement shortCutNode in shortCutHolder.Elements())
                                    {
                                        depScNodeList = depScNodeList.Except(new string[] { shortCutNode.Name.ToString() }).ToArray();
                                        switch (shortCutNode.Name.ToString())
                                        {
                                            case "path":
                                                sc.path = shortCutNode.Value;
                                                break;
                                            case "name":
                                                sc.name = shortCutNode.Value;
                                                break;
                                            case "enabled":
                                                sc.enabled = Utils.ParseBool(shortCutNode.Value, false);
                                                break;
                                        }
                                    }
                                    if (sc != null)
                                    {
                                        if (depScNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => Dep {1} (line {2})", string.Join(",", depScNodeList), d.dependencyZipFile, ((IXmlLineInfo)shortCutHolder).LineNumber)); };
                                        d.shortCuts.Add(sc);
                                    }
                                }
                                break;
                            case "logicalDependencies":
                                //parse all dependencies
                                foreach (XElement logDependencyHolder in globs.Elements())
                                {
                                    string[] logDepNodeList = new string[] { "packageName", "negateFlag" };
                                    LogicalDependnecy ld = new LogicalDependnecy();
                                    ld.packageName = "";
                                    foreach (XElement logDependencyNode in logDependencyHolder.Elements())
                                    {
                                        logDepNodeList = logDepNodeList.Except(new string[] { logDependencyNode.Name.ToString() }).ToArray();
                                        switch (logDependencyNode.Name.ToString())
                                        {
                                            case "packageName":
                                                ld.packageName = logDependencyNode.Value.Trim();
                                                if (ld.packageName.Equals(""))
                                                {
                                                    Utils.AppendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => logDep {1} (line {2})", logDependencyNode.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)logDependencyNode).LineNumber));
                                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\"  => dep {1}", logDependencyNode.Name.ToString(), d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                }
                                                break;
                                            case "negateFlag":
                                                ld.negateFlag = Utils.ParseBool(logDependencyNode.Value, true);
                                                break;
                                            default:
                                                Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => logDep {1} (line {2})", logDependencyNode.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)logDependencyNode).LineNumber));
                                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: packageName\n\nNode found: {0}\n\nmore informations, see logfile", logDependencyNode.Name.ToString())); };
                                                break;
                                        }
                                    }
                                    if (ld != null)
                                    {
                                        if (logDepNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => logDep {1} (line {2})", string.Join(",", logDepNodeList), ld.dependencyZipFile, ((IXmlLineInfo)logDependencyHolder).LineNumber)); };
                                        if (ld.packageName.Equals("")) { string rad = Utils.RandomString(30); ld.packageName = rad; Utils.AppendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                        d.logicalDependencies.Add(ld);
                                    };
                                }
                                break;
                            default:
                                Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => globsPend {1} (line {2})", globs.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)globs).LineNumber));
                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: dependencyZipFile, dependencyZipCRC, startAddress, endAddress, dependencyenabled, packageName\n\nNode found: {0}\n\nmore informations, see logfile", globs.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                break;
                        }
                    }
                    if (d != null)
                    {
                        if (depNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => globsPend {1} (line {2})", string.Join(",", depNodeList), d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber)); };
                        if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.AppendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                        dependencies.Add(d);
                    };
                }
                //add the logicalDependencies (TODO)
                foreach (XElement dependencyNode in doc.XPathSelectElements("/modInfoAlpha.xml/logicalDependencies/logicalDependency"))
                {
                    string[] depNodeList = new string[] { "dependencyZipFile", "dependencyZipCRC", "startAddress", "endAddress", "dependencyenabled", "packageName", "devURL", "timestamp" };
                    LogicalDependnecy d = new LogicalDependnecy();
                    d.packageName = "";
                    foreach (XElement globs in dependencyNode.Elements())
                    {
                        depNodeList = depNodeList.Except(new string[] { globs.Name.ToString() }).ToArray();
                        switch (globs.Name.ToString())
                        {
                            case "dependencyZipFile":
                                d.dependencyZipFile = globs.Value;
                                break;
                            case "timestamp":
                                d.timestamp = long.Parse("0" + globs.Value);
                                break;
                            case "dependencyZipCRC":
                                d.dependencyZipCRC = globs.Value;
                                break;
                            case "startAddress":
                                d.startAddress = globs.Value;
                                break;
                            case "endAddress":
                                d.endAddress = globs.Value;
                                break;
                            case "devURL":
                                d.devURL = globs.Value;
                                break;
                            case "dependencyenabled":
                                d.enabled = Utils.ParseBool(globs.Value, false);
                                break;
                            case "shortCuts":
                                //parse all shortCuts
                                foreach (XElement shortCutHolder in globs.Elements())
                                {
                                    ShortCut sc = new ShortCut();
                                    string[] logDepScNodeList = new string[] { "path", "name", "enabled" };
                                    foreach (XElement shortCutNode in shortCutHolder.Elements())
                                    {
                                        logDepScNodeList = logDepScNodeList.Except(new string[] { shortCutNode.Name.ToString() }).ToArray();
                                        switch (shortCutNode.Name.ToString())
                                        {
                                            case "path":
                                                sc.path = shortCutNode.Value;
                                                break;
                                            case "name":
                                                sc.name = shortCutNode.Value;
                                                break;
                                            case "enabled":
                                                sc.enabled = Utils.ParseBool(shortCutNode.Value, false);
                                                break;
                                        }
                                    }
                                    if (sc != null)
                                    {
                                        if (logDepScNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => logDep {1} (line {2})", string.Join(",", logDepScNodeList), d.dependencyZipFile, ((IXmlLineInfo)shortCutHolder).LineNumber)); };
                                        d.shortCuts.Add(sc);
                                    }
                                }
                                break;
                            case "packageName":
                                d.packageName = globs.Value.Trim();
                                if (d.packageName.Equals(""))
                                {
                                    Utils.AppendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => logDep {1} (line {2})", globs.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)globs).LineNumber));
                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => logDep {1}\n\nmore informations, see logfile", globs.Name.ToString(), d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                }
                                break;
                            default:
                                Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => logDep {1} (line {2})", globs.Name.ToString(), d.dependencyZipFile, ((IXmlLineInfo)globs).LineNumber));
                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: dependencyZipFile, dependencyZipCRC, startAddress, endAddress, dependencyenabled, packageName\n\nNode found: {0}\n\nmore informations, see logfile", globs.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                break;
                        }
                    }
                    if (d != null)
                    {
                        if (depNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => globsPend {1} (line {2})", string.Join(",", depNodeList), d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber)); };
                        if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.AppendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                        logicalDependencies.Add(d);
                    };
                }
                foreach (XElement catagoryHolder in doc.XPathSelectElements("/modInfoAlpha.xml/catagories/catagory"))
                {
                    Category cat = new Category();
                    string[] catNodeList = new string[] { "name", "selectionType", "mods", "dependencies" };
                    foreach (XElement catagoryNode in catagoryHolder.Elements())
                    {
                        catNodeList = catNodeList.Except(new string[] { catagoryNode.Name.ToString() }).ToArray();
                        switch (catagoryNode.Name.ToString())
                        {
                            case "name":
                                cat.name = catagoryNode.Value;
                                break;
                            case "selectionType":
                                cat.selectionType = catagoryNode.Value;
                                break;
                            case "mods":
                                foreach (XElement modHolder in catagoryNode.Elements())
                                {
                                    switch (modHolder.Name.ToString())
                                    {
                                        case "mod":
                                            string[] modNodeList = new string[] { "name", "version", "zipFile", "timestamp", "startAddress", "endAddress", "crc", "enabled", "visible", "packageName", "size", "description", "updateComment", "devURL", "userDatas", "pictures", "dependencies", "logicalDependencies", "configs" };
                                            Mod m = new Mod();
                                            m.packageName = "";
                                            foreach (XElement modNode in modHolder.Elements())
                                            {
                                                modNodeList = modNodeList.Except(new string[] { modNode.Name.ToString() }).ToArray();
                                                switch (modNode.Name.ToString())
                                                {
                                                    case "name":
                                                        m.name = modNode.Value;
                                                        TotalModConfigComponents++;
                                                        break;
                                                    case "version":
                                                        m.version = modNode.Value;
                                                        break;
                                                    case "zipFile":
                                                        m.zipFile = modNode.Value;
                                                        break;
                                                    case "timestamp":
                                                        m.timestamp = long.Parse("0" + modNode.Value);
                                                        break;
                                                    case "startAddress":
                                                        m.startAddress = modNode.Value;
                                                        break;
                                                    case "endAddress":
                                                        m.endAddress = modNode.Value;
                                                        break;
                                                    case "crc":
                                                        m.crc = modNode.Value;
                                                        break;
                                                    case "enabled":
                                                        m.enabled = Utils.ParseBool(modNode.Value, false);
                                                        break;
                                                    case "visible":
                                                        m.visible = Utils.ParseBool(modNode.Value, true);
                                                        break;
                                                    case "packageName":
                                                        m.packageName = modNode.Value.Trim();
                                                        if (m.packageName.Equals(""))
                                                        {
                                                            Utils.AppendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => mod {1} ({2}) (line {3})", modNode.Name.ToString(), m.name, m.zipFile, ((IXmlLineInfo)modNode).LineNumber));
                                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => mod {1} ({2})", modNode.Name.ToString(), m.name, m.zipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                        }
                                                        break;
                                                    case "size":
                                                        // m.size = Utils.parseFloat(modNode.Value, 0.0f);
                                                        m.size = Utils.ParseInt(modNode.Value, 0);
                                                        break;
                                                    case "description":
                                                        m.description = ConvertFromXmlSaveFormat(modNode.Value);
                                                        break;
                                                    case "updateComment":
                                                        m.updateComment = ConvertFromXmlSaveFormat(modNode.Value);
                                                        break;
                                                    case "devURL":
                                                        m.devURL = modNode.Value;
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
                                                                    m.userFiles.Add(innerText);
                                                                    break;
                                                                default:
                                                                    Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => userDatas => expected node: userData (line {3})", userDataNode.Name.ToString(), m.name, m.zipFile, ((IXmlLineInfo)userDataNode).LineNumber));
                                                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected node: userData\n\nNode found: {0}\n\nmore informations, see logfile", userDataNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                    break;
                                                            }
                                                        }
                                                        break;
                                                    case "pictures":
                                                        //parse every picture
                                                        foreach (XElement pictureHolder in modNode.Elements())
                                                        {
                                                            Media med = new Media(null, null);
                                                            switch (pictureHolder.Name.ToString())
                                                            {
                                                                case "picture":
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
                                                                                med.name = m.name;
                                                                                med.URL = innerText;
                                                                                break;
                                                                            case "type":
                                                                                int innerValue = Utils.ParseInt(pictureNode.Value, 1);
                                                                                switch (innerValue)
                                                                                {
                                                                                    case 1:
                                                                                        med.mediaType = MediaType.picture;
                                                                                        break;
                                                                                    case 2:
                                                                                        med.mediaType = MediaType.youtube;
                                                                                        break;
                                                                                }
                                                                                break;
                                                                            default:
                                                                                Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => picture =>expected nodes: URL (line {3})", pictureNode.Name.ToString(), m.name, m.zipFile, ((IXmlLineInfo)pictureNode).LineNumber));
                                                                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: URL\n\nNode found: {0}\n\nmore informations, see logfile", pictureNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                                break;
                                                                        }
                                                                    }
                                                                    break;
                                                                default:
                                                                    Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => expected node: picture (line {3})", pictureHolder.Name, m.name, m.zipFile, ((IXmlLineInfo)pictureHolder).LineNumber));
                                                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected node: picture\n\nNode found: {0}\n\nmore informations, see logfile", pictureHolder.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                    break;
                                                            }
                                                            m.pictureList.Add(med);
                                                        }
                                                        break;
                                                    case "shortCuts":
                                                        //parse all shortCuts
                                                        foreach (XElement shortCutHolder in modNode.Elements())
                                                        {
                                                            ShortCut sc = new ShortCut();
                                                            string[] depScNodeList = new string[] { "path", "name", "enabled" };
                                                            foreach (XElement shortCutNode in shortCutHolder.Elements())
                                                            {
                                                                depScNodeList = depScNodeList.Except(new string[] { shortCutNode.Name.ToString() }).ToArray();
                                                                switch (shortCutNode.Name.ToString())
                                                                {
                                                                    case "path":
                                                                        sc.path = shortCutNode.Value;
                                                                        break;
                                                                    case "name":
                                                                        sc.name = shortCutNode.Value;
                                                                        break;
                                                                    case "enabled":
                                                                        sc.enabled = Utils.ParseBool(shortCutNode.Value, false);
                                                                        break;
                                                                }
                                                            }
                                                            if (sc != null)
                                                            {
                                                                if (depScNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => mod {1} (line {2})", string.Join(",", depScNodeList), m.zipFile, ((IXmlLineInfo)shortCutHolder).LineNumber)); };
                                                                m.shortCuts.Add(sc);
                                                            }
                                                        }
                                                        break;
                                                    case "dependencies":
                                                        //parse all dependencies
                                                        foreach (XElement dependencyHolder in modNode.Elements())
                                                        {
                                                            string[] depNodeList = new string[] { "packageName" };
                                                            Dependency d = new Dependency();
                                                            d.packageName = "";
                                                            foreach (XElement dependencyNode in dependencyHolder.Elements())
                                                            {
                                                                depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                                                switch (dependencyNode.Name.ToString())
                                                                {
                                                                    case "packageName":
                                                                        d.packageName = dependencyNode.Value.Trim();
                                                                        if (d.packageName.Equals(""))
                                                                        {
                                                                            Utils.AppendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => mod {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), m.name, m.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => mod {1} ({2}) => dep {3}", dependencyNode.Name.ToString(), m.name, m.zipFile, d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                        }
                                                                        break;
                                                                    default:
                                                                        Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), m.name, m.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                        if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: packageName\n\nNode found: {0}\n\nmore informations, see logfile", dependencyNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                        break;
                                                                }
                                                            }
                                                            if (d != null)
                                                            {
                                                                if (depNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => mod {1} ({2}) => dep {3} (line {4})", string.Join(",", depNodeList), m.name, m.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber)); };
                                                                if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.AppendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                                                m.dependencies.Add(d);
                                                            };
                                                        }
                                                        break;
                                                    case "logicalDependencies":
                                                        //parse all dependencies
                                                        foreach (XElement dependencyHolder in modNode.Elements())
                                                        {
                                                            string[] depNodeList = new string[] { "packageName", "negateFlag" };
                                                            LogicalDependnecy d = new LogicalDependnecy();
                                                            d.packageName = "";
                                                            foreach (XElement dependencyNode in dependencyHolder.Elements())
                                                            {
                                                                depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                                                switch (dependencyNode.Name.ToString())
                                                                {
                                                                    case "packageName":
                                                                        d.packageName = dependencyNode.Value.Trim();
                                                                        if (d.packageName.Equals(""))
                                                                        {
                                                                            Utils.AppendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => config {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), m.name, m.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => config {1} ({2}) => dep {3}", dependencyNode.Name.ToString(), m.name, m.zipFile, d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                        }
                                                                        break;
                                                                    case "negateFlag":
                                                                        d.negateFlag = Utils.ParseBool(dependencyNode.Value, true);
                                                                        break;
                                                                    default:
                                                                        Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), m.name, m.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                                        if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: packageName\n\nNode found: {0}\n\nmore informations, see logfile", dependencyNode.Name.ToString())); };
                                                                        break;
                                                                }
                                                            }
                                                            if (d != null)
                                                            {
                                                                if (depNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) => dep {3} (line {4})", string.Join(",", depNodeList), m.name, m.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber)); };
                                                                if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.AppendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                                                m.logicalDependencies.Add(d);
                                                            };
                                                        }
                                                        break;
                                                    case "configs":
                                                        //run the process configs method
                                                        XMLUtils.ProcessConfigs(modNode, m, true);
                                                        break;
                                                    default:
                                                        Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) (line {3})", modNode.Name.ToString(), m.name, m.zipFile, ((IXmlLineInfo)modNode).LineNumber));
                                                        if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: name, version, zipFile, startAddress, endAddress, crc, enabled, packageName, size, description, updateComment, devURL, userDatas, pictures, dependencies, configs\n\nNode found: {0}\n\nmore informations, see logfile", modNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                        break;
                                                }
                                            }
                                            if (m != null)
                                            {
                                                if (modNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => mod {1} ({2}) (line {3})", string.Join(",", modNodeList), m.name, m.zipFile, ((IXmlLineInfo)modHolder).LineNumber)); };
                                                if (m.packageName.Equals("")) { string rad = Utils.RandomString(30); m.packageName = rad; Utils.AppendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                                cat.mods.Add(m);
                                            };
                                            break;
                                        default:
                                            Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => cat {1} (line {2})", modHolder.Name.ToString(), cat.name, ((IXmlLineInfo)modHolder).LineNumber));
                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: mod\n\nNode found: {0}\n\nmore informations, see logfile", modHolder.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
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
                                    d.packageName = "";
                                    foreach (XElement dependencyNode in dependencyHolder.Elements())
                                    {
                                        depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                        switch (dependencyNode.Name.ToString())
                                        {
                                            case "packageName":
                                                d.packageName = dependencyNode.Value.Trim();
                                                if (d.packageName.Equals(""))
                                                {
                                                    Utils.AppendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => cat {1} => dep {2} (line {3})", dependencyNode.Name.ToString(), cat.name, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => cat {1} => dep {2}", dependencyNode.Name.ToString(), cat.name, d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                }
                                                break;
                                            default:
                                                Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => cat {1} => dep {2} (line {3})", dependencyNode.Name, cat.name, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: packageName\n\nNode found: {0}\n\nmore informations, see logfile", dependencyNode.Name), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                break;
                                        }
                                    }
                                    if (d != null)
                                    {
                                        if (depNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => cat {1} => dep {2} (line {3})", string.Join(",", depNodeList), cat.name, d.dependencyZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber)); };
                                        if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.AppendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                        cat.dependencies.Add(d);
                                    };
                                }
                                break;
                            default:
                                Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => cat {1} (line {2})", catagoryNode.Name.ToString(), cat.name, ((IXmlLineInfo)catagoryNode).LineNumber));
                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: name, selectionType, mods, dependencies\n\nNode found: {0}\n\nmore informations, see logfile", catagoryNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                break;
                        }
                    }
                    if (cat != null)
                    {
                        if (catNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => cat {1} (line {2})", string.Join(",", catNodeList), cat.name, ((IXmlLineInfo)catagoryHolder).LineNumber)); };
                        parsedCatagoryList.Add(cat);
                    };
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("createModStructure", ex);
            }
        }
        //recursivly processes the configs
        public static void ProcessConfigs(XElement holder, Mod m, bool parentIsMod, Config con = null)
        {
            try
            {
                //parse every config for that mod
                foreach (XElement configHolder in holder.Elements())
                {
                    switch (configHolder.Name.ToString())
                    {
                        case "config":
                            string[] confNodeList = new string[] { "name", "version", "zipFile", "timestamp", "startAddress", "endAddress", "crc", "enabled", "visible", "packageName", "size", "updateComment", "description", "devURL", "type", "configs", "userDatas", "pictures", "dependencies", "logicalDependencies" };
                            Config c = new Config();
                            c.packageName = "";
                            foreach (XElement configNode in configHolder.Elements())
                            {
                                confNodeList = confNodeList.Except(new string[] { configNode.Name.ToString() }).ToArray();
                                switch (configNode.Name.ToString())
                                {
                                    case "name":
                                        c.name = configNode.Value;
                                        break;
                                    case "version":
                                        c.version = configNode.Value;
                                        break;
                                    case "zipFile":
                                        c.zipFile = configNode.Value;
                                        break;
                                    case "timestamp":
                                        c.timestamp = long.Parse("0" + configNode.Value);
                                        break;
                                    case "startAddress":
                                        c.startAddress = configNode.Value;
                                        break;
                                    case "endAddress":
                                        c.endAddress = configNode.Value;
                                        break;
                                    case "crc":
                                        c.crc = configNode.Value;
                                        break;
                                    case "enabled":
                                        c.enabled = Utils.ParseBool(configNode.Value, false);
                                        break;
                                    case "visible":
                                        c.visible = Utils.ParseBool(configNode.Value, true);
                                        break;
                                    case "packageName":
                                        c.packageName = configNode.Value.Trim();
                                        if (c.packageName.Equals(""))
                                        {
                                            Utils.AppendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => config {1} ({2}) (line {3})", configNode.Name.ToString(), c.name, c.zipFile, ((IXmlLineInfo)configNode).LineNumber));
                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => config {1} ({2})", configNode.Name.ToString(), c.name, c.zipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                        }
                                        break;
                                    case "size":
                                        c.size = Utils.ParseInt(configNode.Value, 0);
                                        break;
                                    case "updateComment":
                                        c.updateComment = ConvertFromXmlSaveFormat(configNode.Value);
                                        break;
                                    case "description":
                                        c.description = ConvertFromXmlSaveFormat(configNode.Value);
                                        break;
                                    case "devURL":
                                        c.devURL = configNode.Value;
                                        break;
                                    case "type":
                                        c.type = configNode.Value;
                                        break;
                                    case "configs":
                                        XMLUtils.ProcessConfigs(configNode, m, false, c);
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
                                                    c.userFiles.Add(innerText);
                                                    break;
                                                default:
                                                    Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => userDatas => expected nodes: userData (line {3})", userDataNode.Name.ToString(), c.name, c.zipFile, ((IXmlLineInfo)configNode).LineNumber));
                                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: userData\n\nNode found: {0}\n\nmore informations, see logfile", userDataNode.Name.ToString())); };
                                                    break;
                                            }
                                        }
                                        break;
                                    case "pictures":
                                        //parse every picture
                                        foreach (XElement pictureHolder in configNode.Elements())
                                        {
                                            Media med = new Media(null, null);
                                            switch (pictureHolder.Name.ToString())
                                            {
                                                case "picture":
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
                                                                med.name = c.name;
                                                                med.URL = innerText;
                                                                break;
                                                            case "type":
                                                                int innerValue = Utils.ParseInt(pictureNode.Value, 1);
                                                                switch (innerValue)
                                                                {
                                                                    case 1:
                                                                        med.mediaType = MediaType.picture;
                                                                        break;
                                                                    case 2:
                                                                        med.mediaType = MediaType.youtube;
                                                                        break;
                                                                }
                                                                break;
                                                            default:
                                                                Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => picture =>expected nodes: URL (line {3})", pictureNode.Name.ToString(), m.name, m.zipFile, ((IXmlLineInfo)pictureNode).LineNumber));
                                                                if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: URL\n\nNode found: {0}\n\nmore informations, see logfile", pictureNode.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                                break;
                                                        }
                                                    }
                                                    break;
                                                default:
                                                    Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => pictures => expected node: picture (line {3})", pictureHolder.Name, m.name, m.zipFile, ((IXmlLineInfo)pictureHolder).LineNumber));
                                                    if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected node: picture\n\nNode found: {0}\n\nmore informations, see logfile", pictureHolder.Name.ToString()), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                    break;
                                            }
                                            c.pictureList.Add(med);
                                        }
                                        break;
                                    case "shortCuts":
                                        //parse all shortCuts
                                        foreach (XElement shortCutHolder in configNode.Elements())
                                        {
                                            ShortCut sc = new ShortCut();
                                            string[] cScNodeList = new string[] { "path", "name", "enabled" };
                                            foreach (XElement shortCutNode in shortCutHolder.Elements())
                                            {
                                                cScNodeList = cScNodeList.Except(new string[] { shortCutNode.Name.ToString() }).ToArray();
                                                switch (shortCutNode.Name.ToString())
                                                {
                                                    case "path":
                                                        sc.path = shortCutNode.Value;
                                                        break;
                                                    case "name":
                                                        sc.name = shortCutNode.Value;
                                                        break;
                                                    case "enabled":
                                                        sc.enabled = Utils.ParseBool(shortCutNode.Value, false);
                                                        break;
                                                }
                                            }
                                            if (sc != null)
                                            {
                                                if (cScNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => mod {1} (line {2})", string.Join(",", cScNodeList), c.zipFile, ((IXmlLineInfo)shortCutHolder).LineNumber)); };
                                                c.shortCuts.Add(sc);
                                            }
                                        }
                                        break;
                                    case "dependencies":
                                        //parse all dependencies
                                        foreach (XElement dependencyHolder in configNode.Elements())
                                        {
                                            string[] depNodeList = new string[] { "packageName" };
                                            Dependency d = new Dependency();
                                            d.packageName = "";
                                            foreach (XElement dependencyNode in dependencyHolder.Elements())
                                            {
                                                depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                                switch (dependencyNode.Name.ToString())
                                                {
                                                    case "packageName":
                                                        d.packageName = dependencyNode.Value.Trim();
                                                        if (d.packageName.Equals(""))
                                                        {
                                                            Utils.AppendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => config {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), c.name, c.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => config {1} ({2}) => dep {3}", dependencyNode.Name.ToString(), c.name, c.zipFile, d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                        }
                                                        break;
                                                    default:
                                                        Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), c.name, c.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                        if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: packageName\n\nNode found: {0}\n\nmore informations, see logfile", dependencyNode.Name.ToString())); };
                                                        break;
                                                }
                                            }
                                            if (d != null)
                                            {
                                                if (depNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) => dep {3} (line {4})", string.Join(",", depNodeList), c.name, c.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber)); };
                                                if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.AppendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                                c.dependencies.Add(d);
                                            };
                                        }
                                        break;
                                    case "logicalDependencies":
                                        //parse all dependencies
                                        foreach (XElement dependencyHolder in configNode.Elements())
                                        {
                                            string[] depNodeList = new string[] { "packageName", "negateFlag" };
                                            LogicalDependnecy d = new LogicalDependnecy();
                                            d.packageName = "";
                                            foreach (XElement dependencyNode in dependencyHolder.Elements())
                                            {
                                                depNodeList = depNodeList.Except(new string[] { dependencyNode.Name.ToString() }).ToArray();
                                                switch (dependencyNode.Name.ToString())
                                                {
                                                    case "packageName":
                                                        d.packageName = dependencyNode.Value.Trim();
                                                        if (d.packageName.Equals(""))
                                                        {
                                                            Utils.AppendToLog(string.Format("Error modInfo.xml: packageName not defined. node \"{0}\" => config {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), c.name, c.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml: packageName not defined.\nnode \"{0}\" => config {1} ({2}) => dep {3}", dependencyNode.Name.ToString(), c.name, c.zipFile, d.dependencyZipFile), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning); };
                                                        }
                                                        break;
                                                    case "negateFlag":
                                                        d.negateFlag = Utils.ParseBool(dependencyNode.Value, true);
                                                        break;
                                                    default:
                                                        Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) => dep {3} (line {4})", dependencyNode.Name.ToString(), c.name, c.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyNode).LineNumber));
                                                        if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: packageName\n\nNode found: {0}\n\nmore informations, see logfile", dependencyNode.Name.ToString())); };
                                                        break;
                                                }
                                            }
                                            if (d != null)
                                            {
                                                if (depNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) => dep {3} (line {4})", string.Join(",", depNodeList), c.name, c.zipFile, d.dependencyZipFile, ((IXmlLineInfo)dependencyHolder).LineNumber)); };
                                                if (d.packageName.Equals("")) { string rad = Utils.RandomString(30); d.packageName = rad; Utils.AppendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                                                c.logicalDependencies.Add(d);
                                            };
                                        }
                                        break;
                                    default:
                                        Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => config {1} ({2}) (line {3})", configNode.Name.ToString(), c.name, c.zipFile, ((IXmlLineInfo)configNode).LineNumber));
                                        if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: name, version, zipFile, startAddress, endAddress, crc, enabled, packageName, size, updateComment, description, devURL, type, configs, userDatas, pictures, dependencies\n\nNode found: {0}\n\nmore informations, see logfile", configNode.Name.ToString())); };
                                        break;
                                }
                            }
                            if (c != null && confNodeList.Length > 0) { Utils.AppendToLog(string.Format("Error: modInfo.xml nodes not used: {0} => config {1} ({2}) (line {3})", string.Join(",", confNodeList), c.name, c.zipFile, ((IXmlLineInfo)configHolder).LineNumber)); };
                            if (c.packageName.Equals("")) { string rad = Utils.RandomString(30); c.packageName = rad; Utils.AppendToLog("packageName is random generated: " + rad); };              // to avoid exceptions
                            //attach it to eithor the config of correct level or the mod
                            if (parentIsMod)
                                m.configs.Add(c);
                            else
                                con.configs.Add(c);
                            break;
                        default:
                            Utils.AppendToLog(string.Format("Error: modInfo.xml incomprehensible node \"{0}\" => mod {1} ({2}) => expected nodes: config (line {3})", configHolder.Name.ToString(), m.name, m.zipFile, ((IXmlLineInfo)configHolder).LineNumber));
                            if (Program.testMode) { MessageBox.Show(string.Format("modInfo.xml file is incomprehensible.\nexpected nodes: config\n\nNode found: {0}\n\nmore informations, see logfile", configHolder.Name)); };
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("processConfigs", ex);
            }
        }
        //saves the currently checked configs and mods
        public static void SaveConfig(bool fromButton, string fileToConvert, List<Category> parsedCatagoryList, List<Mod> userMods)
        {
            //dialog box to ask where to save the config to
            System.Windows.Forms.SaveFileDialog saveLocation = new System.Windows.Forms.SaveFileDialog();
            saveLocation.AddExtension = true;
            saveLocation.DefaultExt = ".xml";
            saveLocation.Filter = "*.xml|*.xml";
            saveLocation.InitialDirectory = Path.Combine(Application.StartupPath, "RelHaxUserConfigs");
            saveLocation.Title = Translations.getTranslatedString("selectWhereToSave");
            if (fromButton)
            {
                if (saveLocation.ShowDialog().Equals(DialogResult.Cancel))
                {
                    //cancel
                    return;
                }
            }
            string savePath = saveLocation.FileName;
            if (Settings.saveLastConfig && !fromButton && fileToConvert == null)
            {
                savePath = Path.Combine(Application.StartupPath, "RelHaxUserConfigs", "lastInstalledConfig.xml");
                Utils.AppendToLog(string.Format("Save last config checked, saving to {0}", savePath));
            }
            else if (!fromButton && !(fileToConvert == null))
            {
                savePath = fileToConvert;
                Utils.AppendToLog(string.Format("convert saved config file \"{0}\" to format {1}", savePath, Settings.configFileVersion));
            }

            //create saved config xml layout
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("mods", new XAttribute("ver", Settings.configFileVersion), new XAttribute("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))));

            //relhax mods root
            doc.Element("mods").Add(new XElement("relhaxMods"));
            //user mods root
            doc.Element("mods").Add(new XElement("userMods"));

            var nodeRelhax = doc.Descendants("relhaxMods").FirstOrDefault();
            //check every mod
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.Checked)
                    {
                        //add it to the list
                        nodeRelhax.Add(new XElement("mod", m.packageName));
                        if (m.configs.Count > 0)
                        {
                            XMLUtils.SaveProcessConfigs(ref doc, m.configs);
                        }
                    }
                }
            }

            var nodeUserMods = doc.Descendants("userMods").FirstOrDefault();
            //check user mods
            foreach (Mod m in userMods)
            {
                if (m.Checked)
                {
                    //add it to the list
                    nodeUserMods.Add(new XElement("mod", m.name));
                }
            }
            doc.Save(savePath);
            if (fromButton)
            {
                MessageBox.Show(Translations.getTranslatedString("configSaveSuccess"));
            }
        }

        private static void SaveProcessConfigs(ref XDocument doc, List<Config> configList)
        {
            var node = doc.Descendants("relhaxMods").FirstOrDefault();
            foreach (Config cc in configList)
            {
                if (cc.Checked)
                {
                    //add the config to the list
                    node.Add(new XElement("mod", cc.packageName));
                    if (cc.configs.Count > 0)
                    {
                        XMLUtils.SaveProcessConfigs(ref doc, cc.configs);
                    }
                }
            }
        }

        public static void LoadConfig(bool fromButton, string filePath, List<Category> parsedCatagoryList, List<Mod> userMods)
        {
            //uncheck everythihng in memory first
            Utils.ClearSelectionMemory(parsedCatagoryList, userMods);
            XmlDocument doc = new XmlDocument();
            string[] filePathSplit = filePath.Split(',');
            if (filePathSplit.Count() > 1)
            {
                string xmlString = Utils.GetStringFromZip(filePathSplit[0], filePathSplit[1]);
                doc.LoadXml(xmlString);
            }
            else
            {
                doc.Load(filePath);
            }
            //check config file version
            XmlNode xmlNode = doc.SelectSingleNode("//mods");
            string ver = "";
            // check if attribut exists and if TRUE, get the value
            if (xmlNode.Attributes != null && xmlNode.Attributes["ver"] != null)
            {
                ver = xmlNode.Attributes["ver"].Value;
            }
            if (ver.Equals("2.0"))      //the file is version v2.0, so go "loadConfigV2" (packageName depended)
            {
                LoadConfigV2(filePath, parsedCatagoryList, userMods);
            }
            else // file is still version v1.0 (name dependend)
            {
                LoadConfigV1(fromButton, filePath, parsedCatagoryList, userMods);
            }
        }
        //loads a saved config from xml and parses it into the memory database
        public static void LoadConfigV1(bool fromButton, string filePath, List<Category> parsedCatagoryList, List<Mod> userMods)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            Utils.AppendToLog("Loading mod selections v1.0 from " + filePath);
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
                            m = Utils.LinkMod(nn.InnerText, parsedCatagoryList);
                            if ((m != null) && (!m.visible))
                                return;
                            if (m == null)
                            {
                                Utils.AppendToLog(string.Format("WARNING: mod \"{0}\" not found", nn.InnerText));
                                MessageBox.Show(string.Format(Translations.getTranslatedString("modNotFound"), nn.InnerText));
                                continue;
                            }
                            if (m.enabled)
                            {
                                m.Checked = true;
                                if (m.modFormCheckBox != null)
                                {
                                    if (m.modFormCheckBox is ModFormCheckBox)
                                    {
                                        ModFormCheckBox mfcb = (ModFormCheckBox)m.modFormCheckBox;
                                        mfcb.Checked = true;
                                        if (!Settings.disableColorChange)
                                            mfcb.Parent.BackColor = System.Drawing.Color.BlanchedAlmond;
                                    }
                                    else if (m.modFormCheckBox is ModWPFCheckBox)
                                    {
                                        ModWPFCheckBox mfCB2 = (ModWPFCheckBox)m.modFormCheckBox;
                                        mfCB2.IsChecked = true;
                                    }
                                }
                                Utils.AppendToLog(string.Format("Checking mod {0}", m.name));
                            }
                            else
                            {
                                //uncheck
                                if (m.modFormCheckBox != null)
                                {
                                    if (m.modFormCheckBox is ModFormCheckBox)
                                    {
                                        ModFormCheckBox mfcb = (ModFormCheckBox)m.modFormCheckBox;
                                        mfcb.Checked = false;
                                        mfcb.Parent.BackColor = Settings.getBackColor();
                                    }
                                    else if (m.modFormCheckBox is ModWPFCheckBox)
                                    {
                                        ModWPFCheckBox mfCB2 = (ModWPFCheckBox)m.modFormCheckBox;
                                        mfCB2.IsChecked = false;
                                    }
                                }
                            }
                            break;
                        case "configs":
                            XMLUtils.LoadProcessConfigsV1(nn, m, true);
                            break;
                        //compatibility in case it's a super legacy with subConfigs
                        case "subConfigs":
                            XMLUtils.LoadProcessConfigsV1(nn, m, true);
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
                            m = Utils.GetUserMod(nn.InnerText, userMods);
                            if (m != null)
                            {
                                string filename = m.name + ".zip";
                                if (File.Exists(Path.Combine(Application.StartupPath, "RelHaxUserMods", filename)))
                                {
                                    m.Checked = true;
                                    if (m.modFormCheckBox != null)
                                    {
                                        ModFormCheckBox mfcb = (ModFormCheckBox)m.modFormCheckBox;
                                        mfcb.Checked = true;
                                    }
                                    Utils.AppendToLog(string.Format("checking user mod {0}", m.zipFile));
                                }
                            }
                            break;
                    }
                }
            }
            Utils.AppendToLog("Finished loading mod selections v1.0");
            if (fromButton)
            {
                DialogResult result = MessageBox.Show(Translations.getTranslatedString("oldSavedConfigFile"), Translations.getTranslatedString("information"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // create Path to UserConfigs Backup
                    string backupFolder = Path.Combine(Application.StartupPath, "RelHaxUserConfigs", "Backup");
                    // create Backup folder at UserConfigs
                    Directory.CreateDirectory(backupFolder);
                    // exctrat filename to create a new filename with backup date and time
                    string filename = Path.GetFileNameWithoutExtension(filePath);
                    string fileextention = Path.GetExtension(filePath);
                    // create target path
                    string targetFilePath = Path.Combine(backupFolder, string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss}{2}", filename, DateTime.Now, fileextention));
                    // move file to new location now
                    try
                    {
                        File.Move(filePath, targetFilePath);
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog("loadConfigV1", string.Format("sourceFile: {0}\ntargetFile: {1}", filePath, targetFilePath), ex);
                    }
                    // create saved config file with new format
                    SaveConfig(false, filePath, parsedCatagoryList, userMods);
                }
            }
        }
        //loads a saved config from xml and parses it into the memory database
        public static void LoadConfigV2(string filePath, List<Category> parsedCatagoryList, List<Mod> userMods)
        {
            Utils.AppendToLog(string.Format("Loading mod selections v2.0 from {0}", filePath));
            List<string> savedConfigList = new List<string>();
            XPathDocument doc;
            string[] filePathSplit = filePath.Split(',');
            if (filePathSplit.Count() > 1)
            {
                // go here, if the config file selected is a developerSelection config and stored at the modInfo.dat file
                Utils.AppendToLog("parsing developerSelection file: " + filePath);
                string xmlString = Utils.GetStringFromZip(filePathSplit[0], filePathSplit[1]);
                StringReader rdr = new StringReader(xmlString);
                doc = new XPathDocument(rdr);
            }
            else
            {
                Utils.AppendToLog("parsing config file: " + filePath);
                doc = new XPathDocument(filePath);
            }

            foreach (var mod in doc.CreateNavigator().Select("//relhaxMods/mod"))
            {
                savedConfigList.Add(mod.ToString());
            }
            foreach (Category c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.visible)
                    {
                        if (savedConfigList.Contains(m.packageName))
                        {
                            savedConfigList.Remove(m.packageName);
                            if (!m.enabled)
                            {
                                MessageBox.Show(string.Format(Translations.getTranslatedString("modDeactivated"), Utils.ReplaceMacro(m.name, "version", m.version)), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                m.Checked = true;
                                if (m.modFormCheckBox != null)
                                {
                                    if (m.modFormCheckBox is ModFormCheckBox)
                                    {
                                        ModFormCheckBox mfcb = (ModFormCheckBox)m.modFormCheckBox;
                                        mfcb.Checked = true;
                                        if (!Settings.disableColorChange)
                                            mfcb.Parent.BackColor = System.Drawing.Color.BlanchedAlmond;
                                    }
                                    else if (m.modFormCheckBox is ModWPFCheckBox)
                                    {
                                        ModWPFCheckBox mfCB2 = (ModWPFCheckBox)m.modFormCheckBox;
                                        mfCB2.IsChecked = true;
                                    }
                                }
                                Utils.AppendToLog(string.Format("Checking mod {0}", Utils.ReplaceMacro(m.name, "version", m.version)));
                            }
                        }
                        else
                        {
                            //uncheck
                            if (m.modFormCheckBox != null)
                            {
                                if (m.modFormCheckBox is ModFormCheckBox)
                                {
                                    ModFormCheckBox mfcb = (ModFormCheckBox)m.modFormCheckBox;
                                    mfcb.Checked = false;
                                    mfcb.Parent.BackColor = Settings.getBackColor();
                                }
                                else if (m.modFormCheckBox is ModWPFCheckBox)
                                {
                                    ModWPFCheckBox mfCB2 = (ModWPFCheckBox)m.modFormCheckBox;
                                    mfCB2.IsChecked = false;
                                }
                            }
                        }
                        if (m.configs.Count > 0)
                        {
                            LoadProcessConfigsV2(m.name, m.configs, ref savedConfigList);
                        }
                    }
                }
            }
            List<string> savedUserConfigList = new List<string>();
            foreach (var userMod in doc.CreateNavigator().Select("//userMods/mod"))
            {
                savedUserConfigList.Add(userMod.ToString());
            }
            foreach (Mod um in userMods)
            {
                if (savedUserConfigList.Contains(um.name))
                {
                    string filename = um.name + ".zip";
                    if (File.Exists(Path.Combine(Application.StartupPath, "RelHaxUserMods", filename)))
                    {
                        //it will be done in the UI code
                        um.Checked = true;
                        if (um.modFormCheckBox != null)
                        {
                            ModFormCheckBox mfcb = (ModFormCheckBox)um.modFormCheckBox;
                            mfcb.Checked = true;
                        }
                        Utils.AppendToLog(string.Format("Checking user mod {0}", um.zipFile));
                    }
                }
            }
            if (savedConfigList.Count > 0)
            {
                string modsNotFoundList = "";
                foreach (var s in savedConfigList)
                {
                    modsNotFoundList += "\n" + s;
                }
                MessageBox.Show(string.Format(Translations.getTranslatedString("modsNotFoundTechnical"), modsNotFoundList), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            Utils.AppendToLog("Finished loading mod selections v2.0");
        }

        private static void LoadProcessConfigsV1(XmlNode holder, Mod m, bool parentIsMod, Config con = null)
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
                                if ((c != null) && (!c.visible))
                                    return;
                            }
                            else
                            {
                                c = con.getSubConfig(nnnn.InnerText);
                                if ((c != null) && (!c.visible))
                                    return;
                            }
                            if (c == null)
                            {
                                Utils.AppendToLog(string.Format("WARNING: config \"{0}\" not found for mod/config \"{1}\"", nnnn.InnerText, holder.InnerText));
                                MessageBox.Show(string.Format(Translations.getTranslatedString("configNotFound"), nnnn.InnerText, holder.InnerText));
                                continue;
                            }
                            if (c.enabled)
                            {
                                c.Checked = true;
                                if (c.configUIComponent != null)
                                {
                                    if (c.configUIComponent is ConfigFormCheckBox)
                                    {
                                        ConfigFormCheckBox CBTemp = (ConfigFormCheckBox)c.configUIComponent;
                                        CBTemp.Checked = true;
                                        if (!Settings.disableColorChange)
                                            CBTemp.Parent.BackColor = System.Drawing.Color.BlanchedAlmond;
                                    }
                                    else if (c.configUIComponent is ConfigFormComboBox)
                                    {
                                        ConfigFormComboBox CBTemp = (ConfigFormComboBox)c.configUIComponent;
                                        foreach (Object o in CBTemp.Items)
                                        {
                                            if (o is ComboBoxItem)
                                            {
                                                ComboBoxItem tempCBI = (ComboBoxItem)o;
                                                if (tempCBI.config.packageName.Equals(c.packageName))
                                                {
                                                    CBTemp.SelectedItem = o;
                                                    break;
                                                }
                                            }
                                        }
                                        if (!Settings.disableColorChange)
                                            CBTemp.Parent.BackColor = System.Drawing.Color.BlanchedAlmond;
                                    }
                                    else if (c.configUIComponent is ConfigFormRadioButton)
                                    {
                                        ConfigFormRadioButton CBTemp = (ConfigFormRadioButton)c.configUIComponent;
                                        CBTemp.Checked = true;
                                        if (!Settings.disableColorChange)
                                            CBTemp.Parent.BackColor = System.Drawing.Color.BlanchedAlmond;
                                    }
                                    else if (c.configUIComponent is ConfigWPFCheckBox)
                                    {
                                        ConfigWPFCheckBox CBTemp = (ConfigWPFCheckBox)c.configUIComponent;
                                        CBTemp.IsChecked = true;
                                    }
                                    else if (c.configUIComponent is ConfigWPFComboBox)
                                    {
                                        ConfigWPFComboBox CBTemp = (ConfigWPFComboBox)c.configUIComponent;
                                        foreach (Object o in CBTemp.Items)
                                        {
                                            if (o is ComboBoxItem)
                                            {
                                                ComboBoxItem tempCBI = (ComboBoxItem)o;
                                                if (tempCBI.config.packageName.Equals(c.packageName))
                                                {
                                                    CBTemp.SelectedItem = o;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    else if (c.configUIComponent is ConfigWPFRadioButton)
                                    {
                                        ConfigWPFRadioButton CBTemp = (ConfigWPFRadioButton)c.configUIComponent;
                                        CBTemp.IsChecked = true;
                                    }
                                }
                                Utils.AppendToLog(string.Format("Checking mod {0}", Utils.ReplaceMacro(c.name, "version", c.version)));
                            }
                            else
                            {
                                if (c.configUIComponent != null)
                                {
                                    if (c.configUIComponent is ConfigFormCheckBox)
                                    {
                                        ConfigFormCheckBox CBTemp = (ConfigFormCheckBox)c.configUIComponent;
                                        CBTemp.Checked = false;
                                        CBTemp.Parent.BackColor = Settings.getBackColor();
                                    }
                                    else if (c.configUIComponent is ConfigFormComboBox)
                                    {
                                        ConfigFormComboBox CBTemp = (ConfigFormComboBox)c.configUIComponent;
                                        CBTemp.Parent.BackColor = Settings.getBackColor();
                                    }
                                    else if (c.configUIComponent is ConfigFormRadioButton)
                                    {
                                        ConfigFormRadioButton CBTemp = (ConfigFormRadioButton)c.configUIComponent;
                                        CBTemp.Checked = false;
                                        CBTemp.Parent.BackColor = Settings.getBackColor();
                                    }
                                    else if (c.configUIComponent is ConfigWPFCheckBox)
                                    {
                                        ConfigWPFCheckBox CBTemp = (ConfigWPFCheckBox)c.configUIComponent;
                                        CBTemp.IsChecked = false;
                                    }
                                    else if (c.configUIComponent is ConfigWPFComboBox)
                                    {
                                        //do nothing...
                                    }
                                    else if (c.configUIComponent is ConfigWPFRadioButton)
                                    {
                                        ConfigWPFRadioButton CBTemp = (ConfigWPFRadioButton)c.configUIComponent;
                                        CBTemp.IsChecked = false;
                                    }
                                }
                            }
                            break;
                        case "configs":
                            XMLUtils.LoadProcessConfigsV1(nnnn, m, false, c);
                            break;
                        case "subConfigs":
                            XMLUtils.LoadProcessConfigsV1(nnnn, m, false, c);
                            break;
                    }
                }
            }
        }

        private static void LoadProcessConfigsV2(string parentName, List<Config> configList, ref List<string> savedConfigList)
        {
            bool shouldBeBA = false;
            Panel panelRef = null;
            foreach (Config c in configList)
            {
                if (c.visible)
                {
                    if (savedConfigList.Contains(c.packageName))
                    {
                        savedConfigList.Remove(c.packageName);
                        if (!c.enabled)
                        {
                            MessageBox.Show(string.Format(Translations.getTranslatedString("configDeactivated"), Utils.ReplaceMacro(c.name, "version", c.version), parentName), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            c.Checked = true;
                            if (c.configUIComponent != null)
                            {
                                if (c.configUIComponent is ConfigFormCheckBox)
                                {
                                    ConfigFormCheckBox CBTemp = (ConfigFormCheckBox)c.configUIComponent;
                                    CBTemp.Checked = true;
                                    shouldBeBA = true;
                                    if (CBTemp.Parent is Panel)
                                        panelRef = (Panel)CBTemp.Parent;
                                }
                                else if (c.configUIComponent is ConfigFormComboBox)
                                {
                                    ConfigFormComboBox CBTemp = (ConfigFormComboBox)c.configUIComponent;
                                    foreach (Object o in CBTemp.Items)
                                    {
                                        if (o is ComboBoxItem)
                                        {
                                            ComboBoxItem tempCBI = (ComboBoxItem)o;
                                            if (tempCBI.config.packageName.Equals(c.packageName))
                                            {
                                                CBTemp.SelectedItem = o;
                                                break;
                                            }
                                        }
                                    }
                                    shouldBeBA = true;
                                    if (CBTemp.Parent is Panel)
                                        panelRef = (Panel)CBTemp.Parent;
                                }
                                else if (c.configUIComponent is ConfigFormRadioButton)
                                {
                                    ConfigFormRadioButton CBTemp = (ConfigFormRadioButton)c.configUIComponent;
                                    CBTemp.Checked = true;
                                    shouldBeBA = true;
                                    if (CBTemp.Parent is Panel)
                                        panelRef = (Panel)CBTemp.Parent;
                                }
                                else if (c.configUIComponent is ConfigWPFCheckBox)
                                {
                                    ConfigWPFCheckBox CBTemp = (ConfigWPFCheckBox)c.configUIComponent;
                                    CBTemp.IsChecked = true;
                                }
                                else if (c.configUIComponent is ConfigWPFComboBox)
                                {
                                    ConfigWPFComboBox CBTemp = (ConfigWPFComboBox)c.configUIComponent;
                                    foreach (Object o in CBTemp.Items)
                                    {
                                        if (o is ComboBoxItem)
                                        {
                                            ComboBoxItem tempCBI = (ComboBoxItem)o;
                                            if (tempCBI.config.packageName.Equals(c.packageName))
                                            {
                                                CBTemp.SelectedItem = o;
                                                break;
                                            }
                                        }
                                    }
                                }
                                else if (c.configUIComponent is ConfigWPFRadioButton)
                                {
                                    ConfigWPFRadioButton CBTemp = (ConfigWPFRadioButton)c.configUIComponent;
                                    CBTemp.IsChecked = true;
                                }
                            }
                            Utils.AppendToLog(string.Format("Checking mod {0}", Utils.ReplaceMacro(c.name, "version", c.version)));
                        }
                    }
                    else
                    {
                        if (c.configUIComponent != null)
                        {
                            if (c.configUIComponent is ConfigFormCheckBox)
                            {
                                ConfigFormCheckBox CBTemp = (ConfigFormCheckBox)c.configUIComponent;
                                CBTemp.Checked = false;
                                CBTemp.Parent.BackColor = Settings.getBackColor();
                            }
                            else if (c.configUIComponent is ConfigFormComboBox)
                            {
                                ConfigFormComboBox CBTemp = (ConfigFormComboBox)c.configUIComponent;
                                CBTemp.Parent.BackColor = Settings.getBackColor();
                            }
                            else if (c.configUIComponent is ConfigFormRadioButton)
                            {
                                ConfigFormRadioButton CBTemp = (ConfigFormRadioButton)c.configUIComponent;
                                CBTemp.Checked = false;
                                CBTemp.Parent.BackColor = Settings.getBackColor();
                            }
                            else if (c.configUIComponent is ConfigWPFCheckBox)
                            {
                                ConfigWPFCheckBox CBTemp = (ConfigWPFCheckBox)c.configUIComponent;
                                CBTemp.IsChecked = false;
                            }
                            else if (c.configUIComponent is ConfigWPFComboBox)
                            {
                                //do nothing...
                            }
                            else if (c.configUIComponent is ConfigWPFRadioButton)
                            {
                                ConfigWPFRadioButton CBTemp = (ConfigWPFRadioButton)c.configUIComponent;
                                CBTemp.IsChecked = false;
                            }
                        }
                    }
                    if (c.configs.Count > 0)
                    {
                        LoadProcessConfigsV2(c.name, c.configs, ref savedConfigList);
                    }
                }
            }
            if (shouldBeBA && panelRef != null)
            {
                if (!Settings.disableColorChange)
                    panelRef.BackColor = System.Drawing.Color.BlanchedAlmond;
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
        public static void SaveDatabase(string saveLocation, string gameVersion, string onlineFolderVersion, List<Dependency> globalDependencies, List<Dependency> dependencies, List<LogicalDependnecy> logicalDependencies, List<Category> parsedCatagoryList)
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
                //make dependency
                XmlElement globalDepZipFile = doc.CreateElement("dependencyZipFile");
                if (!d.dependencyZipFile.Trim().Equals(""))
                    globalDepZipFile.InnerText = d.dependencyZipFile.Trim();
                globalDependencyRoot.AppendChild(globalDepZipFile);
                XmlElement globalDepTimestamp = doc.CreateElement("timestamp");
                if (d.timestamp != 0)
                    globalDepTimestamp.InnerText = d.timestamp.ToString();
                globalDependencyRoot.AppendChild(globalDepTimestamp);
                XmlElement globalDepStartAddress = doc.CreateElement("startAddress");
                if (!d.startAddress.Trim().Equals(""))
                    globalDepStartAddress.InnerText = d.startAddress.Trim();
                globalDependencyRoot.AppendChild(globalDepStartAddress);
                XmlElement globalDepEndAddress = doc.CreateElement("endAddress");
                if (!d.endAddress.Trim().Equals(""))
                    globalDepEndAddress.InnerText = d.endAddress.Trim();
                globalDependencyRoot.AppendChild(globalDepEndAddress);
                XmlElement globalDepURL = doc.CreateElement("devURL");
                if (!d.devURL.Trim().Equals(""))
                    globalDepURL.InnerText = d.devURL.Trim();
                globalDependencyRoot.AppendChild(globalDepURL);
                XmlElement globalDepCRC = doc.CreateElement("dependencyZipCRC");
                if (!d.dependencyZipCRC.Trim().Equals(""))
                    globalDepCRC.InnerText = d.dependencyZipCRC.Trim();
                globalDependencyRoot.AppendChild(globalDepCRC);
                XmlElement globalDepEnabled = doc.CreateElement("dependencyenabled");
                if (!d.enabled.ToString().Trim().Equals(""))
                    globalDepEnabled.InnerText = "" + d.enabled;
                globalDependencyRoot.AppendChild(globalDepEnabled);
                XmlElement globalDepAppendExtraction = doc.CreateElement("appendExtraction");
                if (!d.appendExtraction.ToString().Trim().Equals(""))
                    globalDepAppendExtraction.InnerText = "" + d.appendExtraction;
                globalDependencyRoot.AppendChild(globalDepAppendExtraction);
                XmlElement globalDepPackageName = doc.CreateElement("packageName");
                if (!d.packageName.Trim().Equals(""))
                    globalDepPackageName.InnerText = d.packageName.Trim();
                globalDependencyRoot.AppendChild(globalDepPackageName);
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
                XmlElement depZipFile = doc.CreateElement("dependencyZipFile");
                if (!d.dependencyZipFile.Trim().Equals(""))
                    depZipFile.InnerText = d.dependencyZipFile.Trim();
                dependencyRoot.AppendChild(depZipFile);
                XmlElement depTimestamp = doc.CreateElement("timestamp");
                if (d.timestamp != 0)
                    depTimestamp.InnerText = d.timestamp.ToString();
                dependencyRoot.AppendChild(depTimestamp);
                XmlElement depStartAddress = doc.CreateElement("startAddress");
                if (!d.startAddress.Trim().Equals(""))
                    depStartAddress.InnerText = d.startAddress.Trim();
                dependencyRoot.AppendChild(depStartAddress);
                XmlElement depEndAddress = doc.CreateElement("endAddress");
                if (!d.endAddress.Trim().Equals(""))
                    depEndAddress.InnerText = d.endAddress.Trim();
                dependencyRoot.AppendChild(depEndAddress);
                XmlElement depdevURL = doc.CreateElement("devURL");
                if (!d.devURL.Trim().Equals(""))
                    depdevURL.InnerText = d.devURL.Trim();
                dependencyRoot.AppendChild(depdevURL);
                XmlElement depCRC = doc.CreateElement("dependencyZipCRC");
                if (!d.dependencyZipCRC.Trim().Equals(""))
                    depCRC.InnerText = d.dependencyZipCRC.Trim();
                dependencyRoot.AppendChild(depCRC);
                XmlElement depEnabled = doc.CreateElement("dependencyenabled");
                if (!d.enabled.ToString().Trim().Equals(""))
                    depEnabled.InnerText = "" + d.enabled;
                dependencyRoot.AppendChild(depEnabled);
                XmlElement depAppendExtraction = doc.CreateElement("appendExtraction");
                if (!d.appendExtraction.ToString().Trim().Equals(""))
                    depAppendExtraction.InnerText = "" + d.appendExtraction;
                dependencyRoot.AppendChild(depAppendExtraction);
                XmlElement depPackageName = doc.CreateElement("packageName");
                if (!d.packageName.Trim().Equals(""))
                    depPackageName.InnerText = d.packageName.Trim();
                dependencyRoot.AppendChild(depPackageName);
                //logicalDependencies for the configs
                XmlElement depLogicalDependencies = doc.CreateElement("logicalDependencies");
                foreach (LogicalDependnecy ld in d.logicalDependencies)
                {
                    //declare logicalDependency root
                    XmlElement LogicalDependencyRoot = doc.CreateElement("logicalDependency");
                    //make logicalDependency
                    XmlElement LogicalDependencyPackageName = doc.CreateElement("packageName");
                    if (!ld.packageName.Trim().Equals(""))
                        LogicalDependencyPackageName.InnerText = ld.packageName.Trim();
                    LogicalDependencyRoot.AppendChild(LogicalDependencyPackageName);
                    XmlElement LogicalDependencyNegateFlag = doc.CreateElement("negateFlag");
                    if (!ld.negateFlag.ToString().Trim().Equals(""))
                        LogicalDependencyNegateFlag.InnerText = "" + ld.negateFlag;
                    LogicalDependencyRoot.AppendChild(LogicalDependencyNegateFlag);
                    //attach logicalDependency root
                    depLogicalDependencies.AppendChild(LogicalDependencyRoot);
                }
                dependencyRoot.AppendChild(depLogicalDependencies);
                XmlElement shortCuts = doc.CreateElement("shortCuts");
                foreach (ShortCut sc in d.shortCuts)
                {
                    //declare ShortCut root
                    XmlElement ShortCutRoot = doc.CreateElement("shortCut");
                    //make ShortCut
                    XmlElement ShortCutPath = doc.CreateElement("path");
                    if (!sc.path.Trim().Equals(""))
                        ShortCutPath.InnerText = sc.path.Trim();
                    ShortCutRoot.AppendChild(ShortCutPath);
                    XmlElement ShortCutName = doc.CreateElement("name");
                    if (!sc.name.Trim().Equals(""))
                        ShortCutName.InnerText = sc.name.Trim();
                    ShortCutRoot.AppendChild(ShortCutName);
                    XmlElement ShortCutEnabled = doc.CreateElement("enabled");
                    ShortCutEnabled.InnerText = sc.enabled.ToString().Trim();
                    ShortCutRoot.AppendChild(ShortCutEnabled);
                    shortCuts.AppendChild(ShortCutRoot);
                }
                //attach ShortCuts to root
                dependencyRoot.AppendChild(shortCuts);
                DependenciesXml.AppendChild(dependencyRoot);
            }
            root.AppendChild(DependenciesXml);
            //dependencies
            XmlElement logicalDependenciesXml = doc.CreateElement("logicalDependencies");
            foreach (LogicalDependnecy d in logicalDependencies)
            {
                //declare dependency root
                XmlElement logicalDependencyRoot = doc.CreateElement("logicalDependency");
                //make dependency
                XmlElement logicalDepZipFile = doc.CreateElement("dependencyZipFile");
                if (!d.dependencyZipFile.Trim().Equals(""))
                    logicalDepZipFile.InnerText = d.dependencyZipFile.Trim();
                logicalDependencyRoot.AppendChild(logicalDepZipFile);
                XmlElement logicalDepTimestamp = doc.CreateElement("timestamp");
                if (d.timestamp != 0)
                    logicalDepTimestamp.InnerText = d.timestamp.ToString();
                logicalDependencyRoot.AppendChild(logicalDepTimestamp);
                XmlElement logicalDepStartAddress = doc.CreateElement("startAddress");
                if (!d.startAddress.Trim().Equals(""))
                    logicalDepStartAddress.InnerText = d.startAddress.Trim();
                logicalDependencyRoot.AppendChild(logicalDepStartAddress);
                XmlElement logicalDepEndAddress = doc.CreateElement("endAddress");
                if (!d.endAddress.Trim().Equals(""))
                    logicalDepEndAddress.InnerText = d.endAddress.Trim();
                logicalDependencyRoot.AppendChild(logicalDepEndAddress);
                XmlElement logicalDepdevURL = doc.CreateElement("devURL");
                if (!d.devURL.Trim().Equals(""))
                    logicalDepdevURL.InnerText = d.devURL.Trim();
                logicalDependencyRoot.AppendChild(logicalDepdevURL);
                XmlElement logicalDepCRC = doc.CreateElement("dependencyZipCRC");
                if (!d.dependencyZipCRC.Trim().Equals(""))
                    logicalDepCRC.InnerText = d.dependencyZipCRC.Trim();
                logicalDependencyRoot.AppendChild(logicalDepCRC);
                XmlElement logicalDepEnabled = doc.CreateElement("dependencyenabled");
                if (!d.enabled.ToString().Trim().Equals(""))
                    logicalDepEnabled.InnerText = "" + d.enabled;
                logicalDependencyRoot.AppendChild(logicalDepEnabled);
                XmlElement logicalDepPackageName = doc.CreateElement("packageName");
                if (!d.packageName.Trim().Equals(""))
                    logicalDepPackageName.InnerText = d.packageName.Trim();
                logicalDependencyRoot.AppendChild(logicalDepPackageName);
                XmlElement shortCuts = doc.CreateElement("shortCuts");
                foreach (ShortCut sc in d.shortCuts)
                {
                    //declare ShortCut root
                    XmlElement ShortCutRoot = doc.CreateElement("shortCut");
                    //make ShortCut
                    XmlElement ShortCutPath = doc.CreateElement("path");
                    if (!sc.path.Trim().Equals(""))
                        ShortCutPath.InnerText = sc.path.Trim();
                    ShortCutRoot.AppendChild(ShortCutPath);
                    XmlElement ShortCutName = doc.CreateElement("name");
                    if (!sc.name.Trim().Equals(""))
                        ShortCutName.InnerText = sc.name.Trim();
                    ShortCutRoot.AppendChild(ShortCutName);
                    XmlElement ShortCutEnabled = doc.CreateElement("enabled");
                    ShortCutEnabled.InnerText = sc.enabled.ToString().Trim();
                    ShortCutRoot.AppendChild(ShortCutEnabled);
                    shortCuts.AppendChild(ShortCutRoot);
                }
                //attach ShortCuts to root
                logicalDependencyRoot.AppendChild(shortCuts);
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
                if (!c.name.Trim().Equals(""))
                    catagoryName.InnerText = c.name.Trim();
                catagoryRoot.AppendChild(catagoryName);
                XmlElement catagorySelectionType = doc.CreateElement("selectionType");
                if (!c.selectionType.Trim().Equals(""))
                    catagorySelectionType.InnerText = c.selectionType;
                catagoryRoot.AppendChild(catagorySelectionType);
                //dependencies for catagory
                XmlElement catagoryDependencies = doc.CreateElement("dependencies");
                foreach (Dependency d in c.dependencies)
                {
                    //declare dependency root
                    XmlElement DependencyRoot = doc.CreateElement("dependency");
                    XmlElement DepPackageName = doc.CreateElement("packageName");
                    if (!d.packageName.Trim().Equals(""))
                        DepPackageName.InnerText = d.packageName.Trim();
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
                    if (!m.name.Trim().Equals(""))
                        modName.InnerText = m.name.Trim();
                    modRoot.AppendChild(modName);
                    XmlElement modVersion = doc.CreateElement("version");
                    if (!m.version.Trim().Equals(""))
                        modVersion.InnerText = m.version.Trim();
                    modRoot.AppendChild(modVersion);
                    XmlElement modZipFile = doc.CreateElement("zipFile");
                    if (!m.zipFile.Trim().Equals(""))
                        modZipFile.InnerText = m.zipFile.Trim();
                    modRoot.AppendChild(modZipFile);
                    XmlElement modTimestamp = doc.CreateElement("timestamp");
                    if (m.timestamp != 0)
                        modTimestamp.InnerText = m.timestamp.ToString();
                    modRoot.AppendChild(modTimestamp);
                    XmlElement modStartAddress = doc.CreateElement("startAddress");
                    if (!m.startAddress.Trim().Equals(""))
                        modStartAddress.InnerText = m.startAddress.Trim();
                    modRoot.AppendChild(modStartAddress);
                    XmlElement modEndAddress = doc.CreateElement("endAddress");
                    if (!m.endAddress.Trim().Equals(""))
                        modEndAddress.InnerText = m.endAddress.Trim();
                    modRoot.AppendChild(modEndAddress);
                    XmlElement modZipCRC = doc.CreateElement("crc");
                    if (!m.crc.Trim().Equals(""))
                        modZipCRC.InnerText = m.crc.Trim();
                    modRoot.AppendChild(modZipCRC);
                    XmlElement modEnabled = doc.CreateElement("enabled");
                    if (!m.enabled.ToString().Trim().Equals(""))
                        modEnabled.InnerText = "" + m.enabled;
                    modRoot.AppendChild(modEnabled);
                    XmlElement modVisible = doc.CreateElement("visible");
                    if (!m.visible.ToString().Trim().Equals(""))
                        modVisible.InnerText = "" + m.visible;
                    modRoot.AppendChild(modVisible);
                    XmlElement modPackageName = doc.CreateElement("packageName");
                    if (!m.packageName.Trim().Equals(""))
                        modPackageName.InnerText = m.packageName.Trim();
                    modRoot.AppendChild(modPackageName);
                    XmlElement modZipSize = doc.CreateElement("size");
                    if (!m.size.ToString().Trim().Equals(""))
                        modZipSize.InnerText = "" + m.size;
                    modRoot.AppendChild(modZipSize);
                    XmlElement modUpdateComment = doc.CreateElement("updateComment");
                    if (!m.updateComment.Trim().Equals(""))
                        modUpdateComment.InnerText = ConvertToXmlSaveFormat(m.updateComment);
                    modRoot.AppendChild(modUpdateComment);
                    XmlElement modDescription = doc.CreateElement("description");
                    if (!m.description.Trim().Equals(""))
                        modDescription.InnerText = ConvertToXmlSaveFormat(m.description);
                    modRoot.AppendChild(modDescription);
                    XmlElement modDevURL = doc.CreateElement("devURL");
                    if (!m.devURL.Trim().Equals(""))
                        modDevURL.InnerText = m.devURL.Trim();
                    modRoot.AppendChild(modDevURL);
                    //datas for the mods
                    XmlElement modDatas = doc.CreateElement("userDatas");
                    foreach (string s in m.userFiles)
                    {
                        XmlElement userData = doc.CreateElement("userData");
                        if (!s.Trim().Equals(""))
                            userData.InnerText = s.Trim();
                        modDatas.AppendChild(userData);
                    }
                    modRoot.AppendChild(modDatas);
                    //pictures for the mods
                    XmlElement modPictures = doc.CreateElement("pictures");
                    foreach (Media p in m.pictureList)
                    {
                        XmlElement pictureRoot = doc.CreateElement("picture");
                        XmlElement pictureType = doc.CreateElement("type");
                        XmlElement pictureURL = doc.CreateElement("URL");
                        if (!p.URL.Trim().Equals(""))
                            pictureURL.InnerText = p.URL.Trim();
                        if (!p.mediaType.ToString().Trim().Equals(""))
                            pictureType.InnerText = "" + (int)p.mediaType;
                        pictureRoot.AppendChild(pictureType);
                        pictureRoot.AppendChild(pictureURL);
                        modPictures.AppendChild(pictureRoot);
                    }
                    modRoot.AppendChild(modPictures);
                    //configs for the mods
                    XmlElement configsHolder = doc.CreateElement("configs");
                    //if statement here
                    if (m.configs.Count > 0)
                        SaveDatabaseConfigLevel(doc, configsHolder, m.configs);
                    modRoot.AppendChild(configsHolder);
                    XmlElement modDependencies = doc.CreateElement("dependencies");
                    foreach (Dependency d in m.dependencies)
                    {
                        //declare dependency root
                        XmlElement DependencyRoot = doc.CreateElement("dependency");
                        //make dependency
                        XmlElement DepPackageName = doc.CreateElement("packageName");
                        if (!d.packageName.Trim().Equals(""))
                            DepPackageName.InnerText = d.packageName.Trim();
                        DependencyRoot.AppendChild(DepPackageName);
                        //attach dependency root
                        modDependencies.AppendChild(DependencyRoot);
                    }
                    modRoot.AppendChild(modDependencies);
                    //logicalDependencies for the configs
                    XmlElement modLogicalDependencies = doc.CreateElement("logicalDependencies");
                    foreach (LogicalDependnecy ld in m.logicalDependencies)
                    {
                        //declare logicalDependency root
                        XmlElement LogicalDependencyRoot = doc.CreateElement("logicalDependency");
                        //make logicalDependency
                        XmlElement LogicalDependencyPackageName = doc.CreateElement("packageName");
                        if (!ld.packageName.Trim().Equals(""))
                            LogicalDependencyPackageName.InnerText = ld.packageName.Trim();
                        LogicalDependencyRoot.AppendChild(LogicalDependencyPackageName);
                        XmlElement LogicalDependencyNegateFlag = doc.CreateElement("negateFlag");
                        if (!ld.negateFlag.ToString().Trim().Equals(""))
                            LogicalDependencyNegateFlag.InnerText = "" + ld.negateFlag;
                        LogicalDependencyRoot.AppendChild(LogicalDependencyNegateFlag);
                        //attach logicalDependency root
                        modLogicalDependencies.AppendChild(LogicalDependencyRoot);
                    }
                    modRoot.AppendChild(modLogicalDependencies);
                    XmlElement shortCuts = doc.CreateElement("shortCuts");
                    foreach (ShortCut sc in m.shortCuts)
                    {
                        //declare ShortCut root
                        XmlElement ShortCutRoot = doc.CreateElement("shortCut");
                        //make ShortCut
                        XmlElement ShortCutPath = doc.CreateElement("path");
                        if (!sc.path.Trim().Equals(""))
                            ShortCutPath.InnerText = sc.path.Trim();
                        ShortCutRoot.AppendChild(ShortCutPath);
                        XmlElement ShortCutName = doc.CreateElement("name");
                        if (!sc.name.Trim().Equals(""))
                            ShortCutName.InnerText = sc.name.Trim();
                        ShortCutRoot.AppendChild(ShortCutName);
                        XmlElement ShortCutEnabled = doc.CreateElement("enabled");
                        ShortCutEnabled.InnerText = sc.enabled.ToString().Trim();
                        ShortCutRoot.AppendChild(ShortCutEnabled);
                        shortCuts.AppendChild(ShortCutRoot);
                    }
                    //attach ShortCuts to root
                    modRoot.AppendChild(shortCuts);
                    modsHolder.AppendChild(modRoot);
                }
                catagoryRoot.AppendChild(modsHolder);
                //append catagory
                catagoriesHolder.AppendChild(catagoryRoot);
            }
            root.AppendChild(catagoriesHolder);

            // Create an XML declaration.
            XmlDeclaration xmldecl;
            xmldecl = doc.CreateXmlDeclaration("1.0", null, null);
            xmldecl.Encoding = "UTF-8";
            xmldecl.Standalone = "yes";

            // Add the new node to the document.
            XmlElement cDoc = doc.DocumentElement;
            doc.InsertBefore(xmldecl, cDoc);

            // save database file
            doc.Save(saveLocation);
        }

        private static void SaveDatabaseConfigLevel(XmlDocument doc, XmlElement configsHolder, List<Config> configsList)
        {
            foreach (Config cc in configsList)
            {
                //add the config to the list
                XmlElement configRoot = doc.CreateElement("config");
                configsHolder.AppendChild(configRoot);
                XmlElement configName = doc.CreateElement("name");
                if (!cc.name.Trim().Equals(""))
                    configName.InnerText = cc.name.Trim();
                configRoot.AppendChild(configName);
                XmlElement configVersion = doc.CreateElement("version");
                if (!cc.version.Trim().Equals(""))
                    configVersion.InnerText = cc.version.Trim();
                configRoot.AppendChild(configVersion);
                XmlElement configZipFile = doc.CreateElement("zipFile");
                if (!cc.zipFile.Trim().Equals(""))
                    configZipFile.InnerText = cc.zipFile.Trim();
                configRoot.AppendChild(configZipFile);
                XmlElement configTimestamp = doc.CreateElement("timestamp");
                if (cc.timestamp != 0)
                    configTimestamp.InnerText = cc.timestamp.ToString();
                configRoot.AppendChild(configTimestamp);
                XmlElement configStartAddress = doc.CreateElement("startAddress");
                if (!cc.startAddress.Trim().Equals(""))
                    configStartAddress.InnerText = cc.startAddress.Trim();
                configRoot.AppendChild(configStartAddress);
                XmlElement configEndAddress = doc.CreateElement("endAddress");
                if (!cc.endAddress.Trim().Equals(""))
                    configEndAddress.InnerText = cc.endAddress.Trim();
                configRoot.AppendChild(configEndAddress);
                XmlElement configZipCRC = doc.CreateElement("crc");
                if (!cc.crc.Trim().Equals(""))
                    configZipCRC.InnerText = cc.crc.Trim();
                configRoot.AppendChild(configZipCRC);
                XmlElement configEnabled = doc.CreateElement("enabled");
                if (!cc.enabled.ToString().Trim().Equals(""))
                    configEnabled.InnerText = "" + cc.enabled;
                configRoot.AppendChild(configEnabled);
                XmlElement configVisible = doc.CreateElement("visible");
                if (!cc.visible.ToString().Trim().Equals(""))
                    configVisible.InnerText = "" + cc.visible;
                configRoot.AppendChild(configVisible);
                XmlElement configPackageName = doc.CreateElement("packageName");
                if (!cc.packageName.Trim().Equals(""))
                    configPackageName.InnerText = cc.packageName.Trim();
                configRoot.AppendChild(configPackageName);
                XmlElement configSize = doc.CreateElement("size");
                if (!cc.size.ToString().Trim().Equals(""))
                    configSize.InnerText = "" + cc.size;
                configRoot.AppendChild(configSize);
                XmlElement configComment = doc.CreateElement("updateComment");
                if (!cc.updateComment.Trim().Equals(""))
                    configComment.InnerText = ConvertToXmlSaveFormat(cc.updateComment);
                configRoot.AppendChild(configComment);
                XmlElement configDescription = doc.CreateElement("description");
                if (!cc.description.Trim().Equals(""))
                    configDescription.InnerText = ConvertToXmlSaveFormat(cc.description);
                configRoot.AppendChild(configDescription);
                XmlElement configDevURL = doc.CreateElement("devURL");
                if (!cc.devURL.Trim().Equals(""))
                    configDevURL.InnerText = cc.devURL.Trim();
                configRoot.AppendChild(configDevURL);
                XmlElement configType = doc.CreateElement("type");
                if (!cc.type.ToString().Trim().Equals(""))
                    configType.InnerText = cc.type;
                configRoot.AppendChild(configType);
                //datas for the mods
                XmlElement configDatas = doc.CreateElement("userDatas");
                foreach (string s in cc.userFiles)
                {
                    XmlElement userData = doc.CreateElement("userData");
                    if (!s.Trim().Equals(""))
                        userData.InnerText = s.Trim();
                    configDatas.AppendChild(userData);
                }
                configRoot.AppendChild(configDatas);
                //pictures for the configs
                XmlElement configPictures = doc.CreateElement("pictures");
                foreach (Media p in cc.pictureList)
                {
                    XmlElement pictureRoot = doc.CreateElement("picture");
                    XmlElement pictureType = doc.CreateElement("type");
                    XmlElement pictureURL = doc.CreateElement("URL");
                    if (!p.URL.Trim().Equals(""))
                        pictureURL.InnerText = p.URL.Trim();
                    if (!p.mediaType.ToString().Trim().Equals(""))
                        pictureType.InnerText = "" + (int)p.mediaType;
                    pictureRoot.AppendChild(pictureType);
                    pictureRoot.AppendChild(pictureURL);
                    configPictures.AppendChild(pictureRoot);
                }
                configRoot.AppendChild(configPictures);
                //configs for the configs (meta)
                XmlElement configsHolderSub = doc.CreateElement("configs");
                //if statement here
                if (cc.configs.Count > 0)
                    SaveDatabaseConfigLevel(doc, configsHolderSub, cc.configs);
                configRoot.AppendChild(configsHolderSub);
                //dependencies for the configs
                XmlElement catDependencies = doc.CreateElement("dependencies");
                foreach (Dependency d in cc.dependencies)
                {
                    //declare dependency root
                    XmlElement DependencyRoot = doc.CreateElement("dependency");
                    //make dependency
                    XmlElement DepPackageName = doc.CreateElement("packageName");
                    if (!d.packageName.Trim().Equals(""))
                        DepPackageName.InnerText = d.packageName.Trim();
                    DependencyRoot.AppendChild(DepPackageName);
                    //attach dependency root
                    catDependencies.AppendChild(DependencyRoot);
                }
                configRoot.AppendChild(catDependencies);
                //logicalDependencies for the configs
                XmlElement conLogicalDependencies = doc.CreateElement("logicalDependencies");
                foreach (LogicalDependnecy ld in cc.logicalDependencies)
                {
                    //declare logicalDependency root
                    XmlElement LogicalDependencyRoot = doc.CreateElement("logicalDependency");
                    //make logicalDependency
                    XmlElement LogicalDependencyPackageName = doc.CreateElement("packageName");
                    if (!ld.packageName.Trim().Equals(""))
                        LogicalDependencyPackageName.InnerText = ld.packageName.Trim();
                    LogicalDependencyRoot.AppendChild(LogicalDependencyPackageName);
                    XmlElement LogicalDependencyNegateFlag = doc.CreateElement("negateFlag");
                    if (!ld.negateFlag.ToString().Trim().Equals(""))
                        LogicalDependencyNegateFlag.InnerText = "" + ld.negateFlag;
                    LogicalDependencyRoot.AppendChild(LogicalDependencyNegateFlag);
                    //attach logicalDependency root
                    conLogicalDependencies.AppendChild(LogicalDependencyRoot);
                }
                configRoot.AppendChild(conLogicalDependencies);
                XmlElement shortCuts = doc.CreateElement("shortCuts");
                foreach (ShortCut sc in cc.shortCuts)
                {
                    //declare ShortCut root
                    XmlElement ShortCutRoot = doc.CreateElement("shortCut");
                    //make ShortCut
                    XmlElement ShortCutPath = doc.CreateElement("path");
                    if (!sc.path.Trim().Equals(""))
                        ShortCutPath.InnerText = sc.path.Trim();
                    ShortCutRoot.AppendChild(ShortCutPath);
                    XmlElement ShortCutName = doc.CreateElement("name");
                    if (!sc.name.Trim().Equals(""))
                        ShortCutName.InnerText = sc.name.Trim();
                    ShortCutRoot.AppendChild(ShortCutName);
                    XmlElement ShortCutEnabled = doc.CreateElement("enabled");
                    ShortCutEnabled.InnerText = sc.enabled.ToString().Trim();
                    ShortCutRoot.AppendChild(ShortCutEnabled);
                    shortCuts.AppendChild(ShortCutRoot);
                }
                //attach ShortCuts to root
                configRoot.AppendChild(shortCuts);
                configsHolder.AppendChild(configRoot);
            }
        }

        private static void ParseDeveloperSelections(XDocument doc)
        {
            DeveloperSelections d;
            var xMembers = from members in doc.Descendants("selections").Elements() select members;
            foreach (XElement x in xMembers)
            {
                d = new DeveloperSelections();
                d.internalName = x.Value;
                d.displayName = x.Attribute("displayName").Value;
                d.date = x.Attribute("date").Value;
                MainWindow.developerSelections.Add(d);
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
                    XDocument doc = XDocument.Load(MainWindow.onlineDatabaseXmlFile);
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
            if (!File.Exists(MainWindow.md5HashDatabaseXmlFile))
            {
                XDocument doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("database"));
                doc.Save(MainWindow.md5HashDatabaseXmlFile);
            }
        }
        // need filename and filetime to check the database
        public static string GetMd5HashDatabase(string inputFile, string inputFiletime)
        {
            try
            {
                XDocument doc = XDocument.Load(MainWindow.md5HashDatabaseXmlFile);
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
                File.Delete(MainWindow.md5HashDatabaseXmlFile);     // delete damaged XML database
                CreateMd5HashDatabase();                            // create new XML database
            }
            return "-1";
        }

        public static void UpdateMd5HashDatabase(string inputFile, string inputMd5Hash, string inputFiletime)
        {
            try
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
                    doc.Element("database").Add(new XElement("file", new XAttribute("filename", inputFile), new XAttribute("filetime", inputFiletime), new XAttribute("md5", inputMd5Hash)));
                }
                doc.Save(MainWindow.md5HashDatabaseXmlFile);
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

            XDocument doc = XDocument.Load(MainWindow.md5HashDatabaseXmlFile);
            try
            {
                doc.Descendants("file").Where(arg => arg.Attribute("filename").Value == tempFilename).Remove();
                doc.Save(MainWindow.md5HashDatabaseXmlFile);
            }
            catch (InvalidOperationException)
            {
                return;
            }
        }
    }
}
