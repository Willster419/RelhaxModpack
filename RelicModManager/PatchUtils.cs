using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;

namespace RelhaxModpack
{
    public static class PatchUtils
    {
        private static int NumByteReads = 0;
        private static bool PatchDone = false;
        private static int GenericTraverse = 0;
        private static string XVMBootFileLoc1 = "\\res_mods\\configs\\xvm\\xvm.xc";
        private static string XVMBootFileLoc2 = "\\mods\\configs\\xvm\\xvm.xc";
        //method to patch a part of an xml file
        //fileLocation is relative to res_mods folder
        public static void XMLPatch(string filePath, string xpath, string mode, string search, string replace, string tanksLocation, string tanksVersion, bool testMods = false, string testXVMBootLoc = "")
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
                string s = GetXVMBootLoc(tanksLocation);
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
                string s = GetXVMBootLoc(tanksLocation);
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

            //replace all "fake escape characters" with real escape characters
            search = search.Replace(@"\n", "newline");
            search = search.Replace(@"\r", "\r");
            search = search.Replace(@"\t", "\t");

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
        public static void JSONPatch(string jsonFile, string jsonPath, string newValue, string mode, string tanksLocation, string tanksVersion, bool testMods = false, string testXVMBootLoc = "")
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
                string s = GetXVMBootLoc(tanksLocation);
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
                if (Regex.IsMatch(temp, @",[ \/\w\t\r\n()]*$"))
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
                Utils.appendToLog(string.Format("ERROR: Failed to patch {0}", jsonFile));
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
                    Utils.appendToLog(string.Format("WARNING: path {0} not found for {1}", jsonPath, Path.GetFileName(jsonFile)));
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
                Utils.appendToLog(string.Format("ERROR: Unknown json patch mode, {0}", mode));
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
                Utils.appendToLog(string.Format("There was an error with patching the file {0}, with extra refrences", jsonFile));
            File.WriteAllText(jsonFile, rebuilder.ToString());
        }

        public static void PMODPatch(string bootFile, string xvmPath, string search, string newValue, string mode, string tanksLocation, string tanksVersion, bool testMods = false, string testXVMBootLoc = "")
        {
            NumByteReads = 0;
            PatchDone = false;
            GenericTraverse = 0;
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

            //patch "newline" out of the replace text
            newValue = Regex.Replace(newValue, "newline", "\n");

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
            NumByteReads = 0;
            ReadInside(pathArray, bootFile, newValue, search, mode, xvmPath);
        }

        public static void XVMPatch(string bootFile, string xvmPath, string search, string newValue, string mode, string tanksLocation, string tanksVersion, bool testMods = false, string testXVMBootLoc = "")
        {
            NumByteReads = 0;
            PatchDone = false;
            GenericTraverse = 0;
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
                string s = GetXVMBootLoc(tanksLocation);
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

            //patch "newline" out of the replace text
            newValue = Regex.Replace(newValue, "newline", "\n");

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
            NumByteReads = 0;
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
            ReadUntil(fileContents, sb, @"^[ \t]*\$[ \t]*{[ \t]*""");
            //now read untill the next quote for the temp path
            string filePath = ReadUntil(fileContents, sb, "\"");
            //flip the folder path things
            filePath = Regex.Replace(filePath, "/", "\\");
            //remove the last one
            filePath = filePath.Substring(0, filePath.Length - 1);
            filePath = filePath.Trim();
            ReadInside(pathArray, Path.GetDirectoryName(bootFile) + "\\" + filePath, newValue, search, mode, xvmPath);
        }
        //getting into this means that we've started reading a new config file, maybe patch this one?
        private static void ReadInside(List<string> pathArray, string newFilePath, string replaceValue, string search, string mode, string origXvmPath)
        {
            NumByteReads = 0;
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
                if (ReadUntil(fileContents, sb, regex) == "null")
                {
                    Utils.appendToLog(string.Format("ERROR: Path not found: {0}", origXvmPath));
                    return;
                }
                //determine if the this value is actually a file refrence
                string refrenceTest = PeekUntil(fileContents, @"[\[,}\]]$");
                if (Regex.IsMatch(refrenceTest, @"\${[ \t]*""") && !isArrayIndex)
                {
                    ParseRefrence1(pathArray, newFilePath, replaceValue, search, mode, origXvmPath, fileContents, sb);
                }
                //determine if it is the other type of refrence
                string refrenceTest2 = PeekUntil(fileContents, @"[\[,}\]]$");
                if (Regex.IsMatch(refrenceTest2, @"""\$ref"":") && !isArrayIndex)
                {
                    ParseRefrence2(pathArray, newFilePath, replaceValue, search, mode, origXvmPath, fileContents, sb);
                }
                //determine if it is an array
                isToEnd = false;
                if (isArrayIndex)
                {
                    int indexToReadTo = 0;
                    //boolean flag for it you want to get to the end of the jarray
                    isToEnd = false;
                    //split the array into an array lol
                    if (ReadUntil(fileContents, sb, @"\[") == "null")
                    {
                        Utils.appendToLog(string.Format("ERROR: Path not found: {0}", origXvmPath));
                        return;
                    }
                    string arrayContents = PeekUntil(fileContents, @"\]");
                    //split the array based on "},"
                    List<string> carray = Split(arrayContents, @"[,}]$");
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
                        if (!mode.Equals("array_add"))
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
                        GenericTraverse = 0;
                        ReadUntilGeneric(borken1, @"\[");
                        GenericTraverse = 0;
                        string anotherString2 = ReadUntilGeneric(borken1, @"\]endIndex");
                        GenericTraverse = 0;
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
                            Utils.appendToLog(string.Format("invalid index: {0}", pathArray[0]));
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
                    ReadUntil(fileContents, sb, advanceTo.Count());
                    //determine if the this value is actually a file refrence
                    string refrenceTest5 = PeekUntil(fileContents, @"[,}\]]$");
                    if (Regex.IsMatch(refrenceTest5, @"\${[ \t]*"""))
                    {
                        if (mode.Equals("add") || mode.Equals("edit"))
                            ParseRefrence1(pathArray, newFilePath, replaceValue, search, mode, origXvmPath, fileContents, sb);
                    }
                }
                //we found it so remove it from the path
                if (pathArray.Count != 0)
                    pathArray.RemoveAt(0);
            }
            //split off into the cases for different xvm modification types
            if (!PatchDone)
            {
                PatchDone = true;
                switch (mode)
                {
                    default:
                        //do nothing
                        Utils.appendToLog(string.Format("Invalid mode: {0} for xvm patch {1}", mode, origXvmPath));
                        break;
                    case "edit":
                        XVMEdit(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                    case "add":
                        XVMAdd(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                    case "remove":
                        //TODO
                        break;
                    case "array_clear":
                        XVMArrayClear(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                    case "array_add":
                        XVMArrayAdd(fileContents, sb, newFilePath, replaceValue, search, isToEnd);
                        break;
                    case "array_edit":
                        XVMArrayEdit(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                    case "array_remove":
                        XVMArrayRemove(fileContents, sb, newFilePath, replaceValue, search);
                        break;
                }
            }
        }
        //parse the xvm style refrence
        private static void ParseRefrence1(List<string> pathArray, string newFilePath, string replaceValue, string search, string mode, string origXvmPath, string fileContents, StringBuilder sb)
        {
            //it's a refrence, move it to the next file and readInsideEdit (yes recursion)
            if (ReadUntil(fileContents, sb, @"\${[ \t]*""") == "null")
            {
                Utils.appendToLog(string.Format("ERROR: Path not found: {0}", origXvmPath));
                return;
            }
            //now read untill the next quote for the temp path
            string filePath = ReadUntil(fileContents, sb, "\"");
            //check to see if it is only a refrence withen the same file
            string test = PeekUntil(fileContents, "}");
            if (Regex.IsMatch(test, ":"))
            {
                //new file refrence
                //read untill the next quote
                ReadUntil(fileContents, sb, "\"");
                string tempPath = ReadUntil(fileContents, sb, "\"");
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
                ReadInside(pathArray, Path.GetDirectoryName(newFilePath) + "\\" + filePath, replaceValue, search, mode, origXvmPath);
            }
            else if (Regex.IsMatch(filePath, @"\.(json|xc)"))
            {
                //new file
                string tempPath = filePath.Substring(0, filePath.Length - 1);
                pathArray.RemoveAt(0);
                ReadInside(pathArray, Path.GetDirectoryName(newFilePath) + "\\" + tempPath, replaceValue, search, mode, origXvmPath);
            }
            else
            {
                //same file refrence
                //EXCEPT it could eithor be a new file refrence
                //3 types of refrences
                //"file.json"
                //"path.withinFile
                //"file.json":"path.to.new.ting"
                string[] newPathSplit = filePath.Split('.');
                pathArray[0] = newPathSplit[0];
                for (int i = 1; i < newPathSplit.Count(); i++)
                    pathArray.Insert(1, newPathSplit[i]);
                ReadInside(pathArray, newFilePath, replaceValue, search, mode, origXvmPath);
            }

        }
        //parse the ref style refrence
        private static void ParseRefrence2(List<string> pathArray, string newFilePath, string replaceValue, string search, string mode, string origXvmPath, string fileContents, StringBuilder sb)
        {
            //ref style refrence
            if (ReadUntil(fileContents, sb, @"""\$ref"":") == "null")
            {
                Utils.appendToLog(string.Format("ERROR: Path not found: {0}", origXvmPath));
                return;
            }
            ReadUntil(fileContents, sb, ":");
            ReadUntil(fileContents, sb, "\"");
            string filePath = ReadUntil(fileContents, sb, "\"");
            filePath = Regex.Replace(filePath, "/", "\\");
            filePath = filePath.Substring(0, filePath.Length - 1);
            filePath = filePath.Trim();
            ReadInside(pathArray, Path.GetDirectoryName(newFilePath) + "\\" + filePath, replaceValue, search, mode, origXvmPath);
            //readUntill(fileContents, sb, ":");
        }

        private static void XVMEdit(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            bool modified = false;
            //this is the actual value we want to change
            //get past all the boring stuff
            string toReplace = ReadUntil(fileContents, sb, @"[,}\]]", false);
            //actually replace the value
            //check if it's a comma type (not last) or curley bracket (last)
            string replaced = "";
            if (Regex.IsMatch(toReplace, @",$"))
            {
                if (Regex.IsMatch(toReplace, search + ",$"))
                {
                    replaced = " " + replaceValue + ",";
                    modified = true;
                    PatchDone = true;
                }
            }
            else
            {
                if (Regex.IsMatch(toReplace, search))
                {
                    replaced = replaceValue + "\n";
                    modified = true;
                    PatchDone = true;
                }
            }
            sb.Append(replaced);
            ReadUntilEnd(fileContents, sb);

            if (modified)
            {
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
            }

        }
        //adding a new entry. adds to the top of the parent entry
        private static void XVMAdd(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            bool modified = false;
            string replaced = "";
            //read to the end of that line
            //read for (white space) (anything but white space)
            string temp = ReadUntil(fileContents, sb, @"\s*\S+\s");
            //back up one
            NumByteReads--;
            sb.Remove(sb.Length - 1, 1);
            //check if the last one was a comma
            //if not then add it
            //determine if it stopped in groups of eithor ([ , {) or
            char c = fileContents[NumByteReads - 1];
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
            ReadUntilEnd(fileContents, sb);
            modified = true;
            PatchDone = true;
            if (modified)
            {
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //clearing out an array
        private static void XVMArrayClear(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            //advance reader to start of the array
            ReadUntil(fileContents, sb, @"\[");
            //advance reader to end of array, not saving sb
            ReadUntil(fileContents, sb, @"\]", false);
            sb.Append("]");
            bool modified = false;
            ReadUntilEnd(fileContents, sb);
            modified = true;
            PatchDone = true;
            if (modified)
            {
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //adding a new entry
        private static void XVMArrayAdd(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search, bool isToEnd = false)
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
                string temp = PeekBehindUntil(fileContents, @"[},]");
                //temp = temp.Trim();
                sb.Remove(sb.ToString().Count() - (temp.Count() - 1), temp.Count() - 1);
                string toApeend = temp.Substring(0, 1);
                sb.Append(toApeend);
                //temp.Remove(0, 1);
                sb.Append(",");
                sb.Append(temp.Substring(1, temp.Length - 2));
            }
            //append it in the sb
            sb.Append(replaced);
            if (isToEnd)
                sb.Append("\n");
            ReadUntilEnd(fileContents, sb);
            modified = true;
            PatchDone = true;
            if (modified)
            {
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //editing an existing entry
        private static void XVMArrayEdit(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            bool modified = false;
            bool hadComma = false;
            //move past it and save int input
            string editCheck = ReadUntil(fileContents, sb, @"[,\]]", false);
            if (Regex.IsMatch(editCheck, search))
            {
                if (Regex.IsMatch(editCheck, @",$"))
                    hadComma = true;
                editCheck = " " + replaceValue;
                if (hadComma)
                    editCheck = editCheck + ",";
                //append it in the sb
                sb.Append(editCheck);
                ReadUntilEnd(fileContents, sb);
                modified = true;
                PatchDone = true;
            }
            if (modified)
            {
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //removing an existing entry
        private static void XVMArrayRemove(string fileContents, StringBuilder sb, string newFilePath, string replaceValue, string search)
        {
            bool modified = false;
            bool lastItem = false;
            //move past it and save int input
            string editCheck = ReadUntil(fileContents, sb, @"[,\]]", false);
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
                ReadUntilEnd(fileContents, sb);
                modified = true;
                PatchDone = true;
            }
            if (modified)
            {
                File.Delete(newFilePath);
                File.WriteAllText(newFilePath, sb.ToString());
            }
        }
        //advances the reader until a certain set of characters are found
        private static string ReadUntil(string fileContents, StringBuilder sb, string stopAt, bool save = true)
        {
            string readValues = "";
            string temp = readValues;
            while (!Regex.IsMatch(temp, stopAt, RegexOptions.Multiline))
            {
                int numChars = fileContents.Count();
                if (NumByteReads >= numChars)
                {
                    return "null";
                }
                char c = fileContents[NumByteReads];
                NumByteReads++;
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
        private static string ReadUntilGeneric(string s, string stopAt)
        {
            //genericTraverse = 0;
            string readValues = "";
            while (!Regex.IsMatch(readValues, stopAt, RegexOptions.Multiline))
            {
                if (GenericTraverse >= s.Count())
                    return readValues;
                char c = s[GenericTraverse];
                GenericTraverse++;
                readValues = readValues + c;
            }
            return readValues;
        }
        //advances the reader x ammout of characters
        private static string ReadUntil(string fileContents, StringBuilder sb, int stopAt, bool save = true)
        {
            string readValues = "";
            int session = 0;
            while (session != stopAt)
            {
                char c = fileContents[NumByteReads];
                NumByteReads++;
                session++;
                readValues = readValues + c;
                if (save)
                    sb.Append(c);
            }
            return readValues;
        }
        //advances the reader until the end
        private static string ReadUntilEnd(string fileContents, StringBuilder sb)
        {
            string readValues = "";
            while (NumByteReads < fileContents.Length)
            {
                char c = fileContents[NumByteReads];
                NumByteReads++;
                readValues = readValues + c;
                sb.Append(c);
            }
            return readValues;
        }
        //looks ahead until a certain set of characters are found
        private static string PeekUntil(string fileContents, string stopAt)
        {
            string readValues = "";
            int numPeeks = 0;
            while (!Regex.IsMatch(readValues, stopAt, RegexOptions.Multiline))
            {
                if (NumByteReads > fileContents.Count())
                {
                    NumByteReads = NumByteReads + numPeeks;
                    return readValues;
                }
                char c = fileContents[NumByteReads];
                NumByteReads++;
                readValues = readValues + c;
                numPeeks++;
            }
            NumByteReads = NumByteReads - numPeeks;
            return readValues;
        }
        //looks behind until a certain set of characters are found
        private static string PeekBehindUntil(string fileContents, string stopAt)
        {
            string readValues = "";
            int numPeeks = 0;
            while (!Regex.IsMatch(readValues, stopAt, RegexOptions.Multiline))
            {
                if (NumByteReads == 0)
                {
                    NumByteReads = NumByteReads + numPeeks;
                    return readValues;
                }
                char c = fileContents[NumByteReads];
                NumByteReads--;
                readValues = c + readValues;
                numPeeks++;
            }
            NumByteReads = NumByteReads + numPeeks;
            return readValues;
        }
        //looks ahead untill a certain amount of characters are found
        private static string PeekUntil(string fileContents, int stopAt)
        {
            string readValues = "";
            int numPeeks = 0;
            while (numPeeks != stopAt)
            {
                if (NumByteReads > fileContents.Count())
                {
                    NumByteReads = NumByteReads - numPeeks;
                    return readValues;
                }
                char c = fileContents[NumByteReads];
                NumByteReads++;
                readValues = readValues + c;
                numPeeks++;
            }
            NumByteReads = NumByteReads - numPeeks;
            return readValues;
        }
        //splits text into an array based on a regex match
        private static List<string> Split(string stringToSplit, string regexSplitCommand)
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
        public static string GetXVMBootLoc(string tanksLocation, string customBootFileLoc = null, bool writeToLog = true)
        {
            string bootFile = tanksLocation + XVMBootFileLoc1;
            if (customBootFileLoc != null)
                bootFile = customBootFileLoc;
            if (!File.Exists(bootFile))
            {
                if (writeToLog)
                    Utils.appendToLog(string.Format("ERROR: xvm config boot file does not exist at {0}, checking {1}", XVMBootFileLoc1, XVMBootFileLoc2));
                else
                    Utils.appendToLog(string.Format("NOTICE: default run, xvm config boot file does not exist at {0}, checking {1}", XVMBootFileLoc1, XVMBootFileLoc2));
                bootFile = XVMBootFileLoc2;
                if (!File.Exists(bootFile))
                {
                    if (writeToLog)
                        Utils.appendToLog(string.Format("ERROR: xvm config boot file does not exist at {0}, aborting patch", XVMBootFileLoc2));
                    else
                        Utils.appendToLog(string.Format("NOTICE: default run, xvm config boot file does not exist at {0}, user did not install xvm", XVMBootFileLoc2));
                    return null;
                }
            }
            Utils.appendToLog("xvm boot file located to parse");
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
            GenericTraverse = 0;
            ReadUntilGeneric(fileContents, @"^[ \t]*\$[ \t]*{[ \t]*""");
            //now read untill the next quote for the temp path
            string filePath = ReadUntilGeneric(fileContents, "\"");
            //flip the folder path things
            filePath = Regex.Replace(filePath, "/", "\\");
            //remove the last one
            filePath = filePath.Substring(0, filePath.Length - 1);
            filePath = filePath.Trim();
            GenericTraverse = 0;
            string theNewPath = Path.GetDirectoryName(filePath);
            return theNewPath;
        }
    }
}
