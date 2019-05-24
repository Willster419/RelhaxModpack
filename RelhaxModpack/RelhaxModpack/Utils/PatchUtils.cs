using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Globalization;

namespace RelhaxModpack
{
    #region JSON Patcher workaround
    //This class is for saving all the lines in an .xc xvm config file
    //the "best json api" can't handle "$" refrences, so they must be removed
    //prior to patching. This class stores all required information for that purpose.
    //TODO: remove this lol
    public struct StringSave
    {
        //the name of the property to put it back on later
        public string Name { get; set; }
        //the value of the property (the refrence)
        public string Value { get; set; }
    }
    #endregion

    public static class PatchUtils
    {
        #region Main Patch Method
        public static void RunPatch(Patch p)
        {
            //check if file exists
            if (!File.Exists(p.CompletePath))
            {
                Logging.Warning("File {0} not found", p.CompletePath);
                return;
            }

            //if from the editor, enable verbose logging (allows it to get debug log statements)
            bool tempVerboseLoggingSetting = ModpackSettings.VerboseLogging;
            if(p.FromEditor && !ModpackSettings.VerboseLogging)
            {
                Logging.Debug("p.FromEditor=true and ModpackSettings.VerboseLogging=false, setting to true for duration of patch method");
                ModpackSettings.VerboseLogging = true;
            }

            //macro parsing needs to go here
            Logging.Info(p.DumpPatchInfoForLog);

            //actually run the patches based on what type it is
            switch (p.Type.ToLower())
            {
                case "regex":
                case "regx":
                    if (p.Lines == null || p.Lines.Count() == 0)
                    {
                        Logging.Debug("Running regex patch as all lines, line by line");
                        RegxPatch(p, null);
                    }
                    else if (p.Lines.Count() == 1 && p.Lines[0].Trim().Equals("-1"))
                    {
                        Logging.Debug("Running regex patch as whole file");
                        RegxPatch(p, new int[] { -1 });
                    }
                    else
                    {
                        Logging.Debug("Running regex patch as specified lines, line by line");
                        int[] lines = new int[p.Lines.Count()];
                        for (int i = 0; i < p.Lines.Count(); i++)
                        {
                            lines[i] = int.Parse(p.Lines[i].Trim());
                        }
                        RegxPatch(p, lines);
                    }
                    break;
                case "xml":
                    XMLPatch(p);
                    break;
                case "json":
                    JSONPatchOld(p);
                    break;
                case "xvm":
                    throw new BadMemeException("xvm patches are not supported, please use the json patch method");
            }
            Logging.Debug("patch complete");
            //set the verbose setting back
            Logging.Debug("temp logging setting={0}, ModpackSettings.VerboseLogging={1}, setting logging back to temp");
            ModpackSettings.VerboseLogging = tempVerboseLoggingSetting;
        }
        #endregion

        #region XML
        private static void XMLPatch(Patch p)
        {
            //load the xml document
            XmlDocument doc = XMLUtils.LoadXmlDocument(p.CompletePath,XmlLoadType.FromFile);
            if (doc == null)
            {
                Logging.Error("xml document from xml path is null");
                return;
            }

            //check to see if it has the header info at the top to see if we need to remove it later
            bool hadHeader = false;
            string xmlDec = string.Empty;
            foreach (XmlNode node in doc)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    hadHeader = true;
                    xmlDec = node.OuterXml;
                    break;
                }
            }

            //determines which version of patching will be done
            switch (p.Mode.ToLower())
            {
                case "add":
                    //check to see if it's already there
                    //make the full node path
                    Logging.Debug("checking if xml element to add already exists, creating full xml path");

                    //the add syntax works by using "/" as the node creation path. the last one is the value to put in
                    //there should therefore be at least 2 split element array
                    string[] replacePathSplit = p.Replace.Split('/');

                    //join the base path with the "would-be" path of the new element to add
                    string fullNodePath = p.Path;
                    for (int i = 0; i < replacePathSplit.Count() - 1; i++)
                    {
                        fullNodePath = fullNodePath + "/" + replacePathSplit[i];
                    }

                    //in each node check if the element exist with the replace innerText
                    Logging.Debug("full path to check if exists created as '{0}'", fullNodePath);
                    XmlNodeList fullPathNodeList = doc.SelectNodes(fullNodePath);
                    if (fullPathNodeList.Count > 0)
                    {
                        foreach (XmlElement fullPathMatch in fullPathNodeList)
                        {
                            //get the last element in the replace syntax as value to compare against
                            string innerTextToMatch = replacePathSplit[replacePathSplit.Count() - 1];

                            //remove any tabs and white-spaces first before testing
                            innerTextToMatch = innerTextToMatch.Trim();

                            if (fullPathMatch.InnerText.Trim().Equals(innerTextToMatch))
                            {
                                Logging.Debug("full path found entry with matching text, aborting (no need to patch)");
                                return;
                            }
                            else
                                Logging.Debug("full path found entry, but text does not match. proceeding with add");
                        }
                    }
                    else
                        Logging.Debug("full path entry not found, proceeding with add");

                    //get to the node where to add the element
                    XmlNode xmlPath = doc.SelectSingleNode(p.Path);
                    if(xmlPath == null)
                    {
                        Logging.Error("patch xmlPath returns null!");
                        return;
                    }

                    //create node(s) to add to the element
                    Logging.Debug("Total inner xml elements to make: {0}", replacePathSplit.Count()-1);
                    List<XmlElement> nodesListToAdd = new List<XmlElement>();
                    for (int i = 0; i < replacePathSplit.Count() - 1; i++)
                    {
                        //make the next element using the array replace name
                        XmlElement addElementToMake = doc.CreateElement(replacePathSplit[i]);
                        //the last one is the text to add
                        if (i == replacePathSplit.Count() - 2)
                        {
                            string textToAddIntoNode = replacePathSplit[replacePathSplit.Count() - 1];
                            textToAddIntoNode = Utils.MacroReplace(textToAddIntoNode, ReplacementTypes.PatchArguements);
                            Logging.Debug("adding text: {0}", textToAddIntoNode);
                            addElementToMake.InnerText = textToAddIntoNode;
                        }
                        //add it to the list
                        nodesListToAdd.Add(addElementToMake);
                    }

                    //add nodes to the element in reverse for hierarchy order
                    for (int i = nodesListToAdd.Count - 1; i > -1; i--)
                    {
                        if (i == 0)
                        {
                            //getting here means this is the highest node
                            //that needs to be modified
                            xmlPath.InsertAfter(nodesListToAdd[i], xmlPath.FirstChild);
                            break;
                        }
                        XmlElement parrent = nodesListToAdd[i - 1];
                        XmlElement child = nodesListToAdd[i];
                        parrent.InsertAfter(child, parrent.FirstChild);
                    }
                    Logging.Debug("xml add complete");
                    break;

                case "edit":
                    //check to see if it's already there
                    Logging.Debug("checking if element exists in all results");

                    XmlNodeList xpathResults = doc.SelectNodes(p.Path);
                    if(xpathResults.Count == 0)
                    {
                        Logging.Error("xpath not found");
                        return;
                    }

                    //keep track if all xpath results equal this result
                    int matches = 0;
                    foreach (XmlElement match in xpathResults)
                    {
                        //matched, but trim and check if it matches the replace value
                        if (match.InnerText.Trim().Equals(p.Replace))
                        {
                            Logging.Debug("found replace match for path search, incrementing match counter");
                            matches++;
                        }
                    }
                    if (matches == xpathResults.Count)
                    {
                        Logging.Info("all {0} path results have values equal to replace, so can skip", matches);
                        return;
                    }
                    else
                        Logging.Info("{0} of {1} path results match, running patch");

                    //find and replace
                    foreach (XmlElement replaceMatch in xpathResults)
                    {
                        if (Regex.IsMatch(replaceMatch.InnerText, p.Search))
                        {
                            Logging.Debug("found match, oldValue={0}, new value={1}", replaceMatch.InnerText, p.Replace);
                            replaceMatch.InnerText = p.Replace;
                        }
                        else
                        {
                            Logging.Warning("Regex never matched for this xpath result: {0}",p.Path);
                        }
                    }
                    Logging.Debug("xml edit complete");
                    break;

                case "remove":
                    //check to see if it's there
                    XmlNodeList xpathMatchesToRemove = doc.SelectNodes(p.Path);
                    foreach (XmlElement match in xpathMatchesToRemove)
                    {
                        if (Regex.IsMatch(match.InnerText.Trim(), p.Search))
                        {
                            match.RemoveAll();
                        }
                        else
                        {
                            Logging.Warning("xpath match found, but regex search not matched");
                        }
                    }

                    //remove empty elements
                    Logging.Debug("Removing any empty xml elements");
                    XDocument doc2 = XMLUtils.DocumentToXDocument(doc);
                    //note that XDocuemnt toString drops declaration
                    //https://stackoverflow.com/questions/1228976/xdocument-tostring-drops-xml-encoding-tag
                    doc2.Descendants().Where(e => string.IsNullOrEmpty(e.Value)).Remove();

                    //update doc with doc2
                    doc = XMLUtils.LoadXmlDocument(doc2.ToString(), XmlLoadType.FromString);
                    Logging.Debug("xml remove complete");
                    break;
            }

            //check to see if we need to remove the header
            bool hasHeader = false;
            foreach (XmlNode node in doc)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    hasHeader = true;
                    break;
                }
            }
            //if not had header and has header, remove header
            //if had header and has header, no change
            //if not had header and not has header, no change
            //if had header and not has header, add header
            Logging.Debug("hadHeader={0}, hasHeader={1}", hadHeader, hasHeader);
            if (!hadHeader && hasHeader)
            {
                Logging.Debug("removing header");
                foreach (XmlNode node in doc)
                {
                    if (node.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        doc.RemoveChild(node);
                        break;
                    }
                }
            }
            else if (hadHeader && !hasHeader)
            {
                Logging.Debug("adding header");
                if(string.IsNullOrEmpty(xmlDec))
                {
                    throw new BadMemeException("nnnice.");
                }
                string[] splitDec = xmlDec.Split('=');
                string xmlVer = splitDec[1].Substring(1).Split('"')[0];
                string xmlenc = splitDec[2].Substring(1).Split('"')[0];
                string xmlStandAlone = splitDec[3].Substring(1).Split('"')[0];
                XmlDeclaration dec = doc.CreateXmlDeclaration(xmlVer, xmlenc, xmlStandAlone);
                doc.InsertBefore(dec, doc.DocumentElement);
            }

            //save to disk
            Logging.Debug("saving to disk");
            doc.Save(p.CompletePath);
            Logging.Debug("xml patch completed successfully");
        }
        #endregion

        #region REGEX
        //method to patch a standard text or json file
        //fileLocation is relative to res_mods folder
        private static void RegxPatch(Patch p, int[] lines)
        {
            //replace all "fake escape characters" with real escape characters
            p.Search = Utils.MacroReplace(p.Search, ReplacementTypes.TextUnescape);

            //legacy compatibility: if the replace text has "newline", then replace it with "\n" and log the warning
            if(p.Replace.Contains("newline"))
            {
                Logging.Warning("This patch has the \"newline\" replace syntax and should be updated");
                p.Replace = p.Replace.Replace("newline", "\n");
            }

            //load file from disk
            string file = File.ReadAllText(p.CompletePath);

            //parse each line into an index array
            string[] fileParsed = file.Split('\n');
            StringBuilder sb = new StringBuilder();
            try
            {
                if (lines == null || lines.Count() == 0)
                {
                    //search entire file and replace each instance
                    bool everReplaced = false;
                    for (int i = 0; i < fileParsed.Count(); i++)
                    {
                        if (Regex.IsMatch(fileParsed[i], p.Search))
                        {
                            Logging.Debug("line {0} matched ({1})", i + 1, fileParsed[i]);
                            fileParsed[i] = Regex.Replace(fileParsed[i], p.Search, p.Replace);
                            everReplaced = true;
                        }
                        //we split by \n so put it back in by \n
                        sb.Append(fileParsed[i] + "\n");
                    }
                    if (!everReplaced)
                    {
                        Logging.Warning("Regex never matched");
                        return;
                    }
                }
                else if (lines.Count() == 1 && lines[0] == -1)
                {
                    //search entire file and string and make one giant regex replacement
                    //but remove newlines first
                    file = Regex.Replace(file, "\n", "newline");
                    if (Regex.IsMatch(file, p.Search))
                    {
                        file = Regex.Replace(file, p.Search, p.Replace);
                        file = Regex.Replace(file, "newline", "\n");
                        sb.Append(file);
                    }
                    else
                    {
                        Logging.Warning("Regex never matched");
                        return;
                    }
                }
                else
                {
                    bool everReplaced = false;
                    for (int i = 0; i < fileParsed.Count(); i++)
                    {
                        //factor for "off by one" (no line number 0 in the text file)
                        if (lines.Contains(i+1))
                        {
                            if (Regex.IsMatch(fileParsed[i], p.Search))
                            {
                                Logging.Debug("line {0} matched ({1})", i + 1, fileParsed[i]);
                                fileParsed[i] = Regex.Replace(fileParsed[i], p.Search, p.Replace);
                                fileParsed[i] = Regex.Replace(fileParsed[i], "newline", "\n");
                                everReplaced = true;
                            }
                        }
                        sb.Append(fileParsed[i] + "\n");
                    }
                    if (!everReplaced)
                    {
                        Logging.Warning("Regex never matched");
                        return;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Logging.Error("Invalid regex command");
                Logging.Debug(ex.ToString());
            }

            //save the file back into the string and then the file
            file = sb.ToString().Trim();
            File.WriteAllText(p.CompletePath, file);
            Logging.Debug("regex patch completed successfully");
        }
        #endregion

        #region JSON_NEW
        private static void JsonPatch(Patch p)
        {
            //apply and log legacy compatibilities

        }
        #endregion

        #region JSON_OLD
        //method to parse json files
        private static void JSONPatchOld(Patch p)
        {
            //try to convert the new value to a bool or an int or double first
            bool newValueBool = false;
            int newValueInt = -69420;
            double newValueDouble = -69420.0d;
            bool useBool = false;
            bool useInt = false;
            bool useDouble = false;
            Logging.Debug("TODO: update json patch replace syntax");
            Logging.Debug("TODO: re-write value type replace system");
            Logging.Debug("TODO: fix handling of \"ref\" json keywords");
            //legacy compatibility: many json patches don't have a valid regex systax for p.search, assume they mean a forced replace
            if (p.Search.Equals(""))
                p.Search = @".*";
            //legacy compatibility: treat p.mode being nothing or null default to edit
            if (p.Mode == null || p.Mode.Equals("") || p.Mode.Equals("arrayEdit"))
                p.Mode = "edit";
            //check if the replace value is an xvm path and manually put in the macro equivilants
            //testValue = testValue.Replace(@"[dollar]", @"$").Replace(@"[lbracket]", @"{").Replace(@"[quote]", @"""").Replace(@"[rbracket]""", @"}");
            //match ${"
            if (Regex.IsMatch(p.Replace, @"\$[ \t]*\{[ \t]*"""))
                p.Replace = p.Replace.Replace(@"$", @"[dollar]").Replace(@"{", @"[lbracket]").Replace(@"""", @"[quote]").Replace(@":",@"[colon]").Replace(@"}", @"[rbracket]");
            //split the replace path here so both can use it later
            string[] addPathArray = null;
            string testValue = p.Replace;
            if (p.Mode.Equals("add") || p.Mode.Equals("arrayAdd"))
            {
                //.Split(new string[] { @"[index=" }, StringSplitOptions.None)
                addPathArray = p.Replace.Split('/');
                testValue = addPathArray[addPathArray.Count() - 1];
                testValue = testValue.Split(new string[] { @"[index=" }, StringSplitOptions.None)[0];
            }
            //try a bool first, only works with "true" and "false"
            try
            {
                newValueBool = bool.Parse(testValue);
                useBool = true;
                useInt = false;
                useDouble = false;
            }
            catch (FormatException)
            { }
            //try a double nixt. it will parse a double and int. at this point it could be eithor
            try
            {
                newValueDouble = double.Parse(testValue, CultureInfo.InvariantCulture);
                useDouble = true;
            }
            catch (FormatException)
            { }
            //try an int next. if it works than turn double to false and int to true
            try
            {
                newValueInt = int.Parse(testValue);
                useInt = true;
                useDouble = false;
            }
            catch (FormatException)
            { }

            //load file from disk...
            string file = File.ReadAllText(p.CompletePath);

            //replace all the invalid"${" refrences with macros
            string[] fileSplit = Regex.Split(file, @"\$[ \t]*\{[ \t]*""");
            for(int i = 1; i < fileSplit.Length; i++)
            {
                fileSplit[i] = @"""[dollar][lbracket][quote]" + fileSplit[i];
                //split it again so we don't replace more than we need to
                //we only want to replace the first entry that the original split fond
                string[] splitAgain = fileSplit[i].Split('}');
                if(Regex.IsMatch(splitAgain[0], @"""[\t ]*\:[\t ]*"""))
                    splitAgain[0] = Regex.Replace(splitAgain[0], @"""[\t ]*\:[\t ]*""", @"[quote][colon][quote]");
                //splitAgain[0] = splitAgain[0] + "}";
                fileSplit[i] = string.Join("}", splitAgain);
                //only match the first one...
                Match m = Regex.Match(fileSplit[i], @"""[\t ]*\}");
                if(m.Success)
                {
                    //create two strings, one for before, one for after
                    string before = fileSplit[i].Substring(0, m.Index);
                    string after = fileSplit[i].Substring(m.Index + m.Length);
                    fileSplit[i] = before + @"[quote][rbracket]""" + after;
                }
                //fileSplit[i] = Regex.Replace(fileSplit[i], @"""[\t ]*\}", @"[quote][rbracket]""");
            }
            file = string.Join("", fileSplit);

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
                if (Regex.IsMatch(temp, @"^[ \t]*""\$ref"" *: *{.*}"))
                {
                    modified = true;
                    //jobject
                    ss.Name = temp.Split('"')[1];
                    ss.Value = temp.Split('{')[1];
                    ssList.Add(ss);
                    temp = "\"" + "willster419_refReplace" + "\"" + ": -6969";
                }
                if (hadComma && modified)
                    temp = temp + ",";
                backTogether.Append(temp + "\n");
            }
            file = backTogether.ToString();

            JsonLoadSettings settings = new JsonLoadSettings()
            {
                CommentHandling = CommentHandling.Ignore
            };
            JObject root = null;
            //load json for editing
            if (ModpackSettings.ApplicationDistroVersion == ApplicationVersions.Alpha)
            {
                Logging.WriteToLog("Dumping escaped file for debug: " + Path.Combine(Settings.ApplicationStartupPath, "escaped.xc"));
                File.WriteAllText(Path.Combine(Settings.ApplicationStartupPath, "escaped.xc"), file);
            }
            try
            {
                root = JObject.Parse(file, settings);
            }
            catch (JsonReaderException j)
            {
                Logging.WriteToLog(string.Format("JsonReaderException: Failed to patch {0}, the file is not valid json", p.CompletePath));
                Logging.WriteToLog("(run test db or beta application to get more details)");
                Logging.WriteToLog(j.ToString(),Logfiles.Application,LogLevel.Debug);
            }
            //if it failed to parse show the message (above) and pull out
            if (root == null)
            {
                return;
            }
            if (p.Mode.Equals("add"))
            {
                try
                {
                    //get to object from p.path
                    //if in p.replace regex matches [array], add a blank array if not already there and return
                    //if in p.replace regex matches [object], add a blank object if not already there and return
                    //make full json path of what user wants to add to check, if the value is already there, return if so
                    //for each split element, make objects for path IF not already exist, then property

                    JContainer result = null;
                    //mode 1: adding blank array
                    //add if the desired path to the array return null
                    if (Regex.IsMatch(p.Replace,@".*\[array\]$"))
                    {
                        string propName = p.Replace.Replace(@"[array]", "");
                        try
                        {
                            result = (JContainer)root.SelectToken(p.Path + "." + propName);
                        }
                        catch (Exception array)
                        {
                            Logging.WriteToLog(string.Format("error in replace syntax: {0}\n{1}", propName, array.ToString()),
                                Logfiles.Application, LogLevel.Exception);
                        }
                        if (result != null)
                        {
                            Logging.WriteToLog("cannot add blank array when object already exists",Logfiles.Application,LogLevel.Error);
                            return;
                        }
                        JContainer pathForArray = null;
                        try
                        {
                            pathForArray = (JContainer)root.SelectToken(p.Path);
                        }
                        catch (Exception array)
                        {
                            Logging.WriteToLog(string.Format("error in replace syntax: {0}\n{1}", p.Path, array.ToString()),
                                 Logfiles.Application, LogLevel.Exception);
                        }
                        JArray ary = new JArray();
                        JProperty prop = new JProperty(propName, ary);
                        pathForArray.Add(prop);
                    }
                    //mode 2: adding property with blank object
                    //add if the desired path returns null
                    else if (Regex.IsMatch(p.Replace,@".*\[object\]$"))
                    {
                        string propName = p.Replace.Replace(@"[object]", "");
                        try
                        {
                            result = (JContainer)root.SelectToken(p.Path + "." + propName);
                        }
                        catch (Exception exObject)
                        {
                            Logging.WriteToLog(string.Format("error in replace syntax: {0}\n{1}", propName, exObject.ToString()),
                                Logfiles.Application, LogLevel.Exception);
                        }
                        if (result != null)
                        {
                            Logging.WriteToLog("cannot add blank array when object already exists",Logfiles.Application,LogLevel.Error);
                            return;
                        }
                        JContainer pathForArray = null;
                        try
                        {
                            pathForArray = (JContainer)root.SelectToken(p.Path);
                        }
                        catch (Exception exObject)
                        {
                            Logging.WriteToLog(string.Format("error in replace syntax: {0}\n{1}", p.Path, exObject.ToString()),
                                Logfiles.Application, LogLevel.Exception);
                        }
                        JObject obj = new JObject();
                        JProperty prop = new JProperty(propName, obj);
                        pathForArray.Add(prop);
                    }
                    //mode 3: adding property with value
                    //add if the path returns null if path returns value that direct matches replace
                    else
                    {
                        //check to see if it is there first
                        //add all elements of the new path (all the / in the replace field) up to the last one
                        string fullJSONPath = p.Path;
                        for (int i = 0; i < addPathArray.Count() - 1; i++)
                        {
                            fullJSONPath = fullJSONPath + "." + addPathArray[i];
                        }
                        JToken val = null;
                        try
                        {
                            val = root.SelectToken(fullJSONPath);
                        }
                        catch (Exception exVal)
                        {
                            Logging.WriteToLog(string.Format("error in path syntax: {0}\n{1}", fullJSONPath, exVal.ToString()),
                                Logfiles.Application, LogLevel.Exception);
                        }
                        //null means the fullJSONPath was invalid, the item is not already there
                        if (val != null)
                        {
                            if (!(val is JValue))
                            {
                                Logging.WriteToLog(string.Format("JToken found at {0}, but is not JValue", fullJSONPath),Logfiles.Application,LogLevel.Error);
                                return;
                            }
                            //path is valid, but is the value what we want?
                            JValue temp = (JValue)val;
                            string value = "" + temp.ToString();
                            value = value.Trim();
                            if (useBool)
                                value = value.ToLower();
                            string replaceValue = addPathArray[addPathArray.Count() - 1];
                            if (value.Equals(replaceValue))
                            {
                                //value is already what we want, no need to be here
                                return;
                            }
                            else
                            {
                                //update the value to what we want
                                if (useBool)
                                    temp.Value = newValueBool;
                                else if (useInt)
                                    temp.Value = newValueInt;
                                else if (useDouble)
                                    temp.Value = newValueDouble;
                                else //string
                                    temp.Value = replaceValue.Replace(@"[sl]", @"/");
                            }
                        }
                        else
                        {
                            //at this point the json is parsed, the property is not already there
                            JObject newJobject = null;
                            try
                            {
                                newJobject = (JObject)root.SelectToken(p.Path);
                            }
                            catch (Exception exNewObject)
                            {
                                Logging.WriteToLog(string.Format("error in adding json object: {0}\n{1}", p.Path, exNewObject.ToString()),
                                Logfiles.Application, LogLevel.Exception);
                            }
                            if (newJobject == null)//error
                            {
                                Logging.WriteToLog("fullJSONPath does not exist in the file: " + p.Path,Logfiles.Application,LogLevel.Error);
                                return;
                            }
                            JObject jobjectPlaceholder = newJobject;
                            string newJsonpath = p.Path;
                            for (int i = 0; i < addPathArray.Count(); i++)
                            {
                                if (i == addPathArray.Count() - 2)
                                {
                                    //need to create a property
                                    string newPropValue = addPathArray[i + 1];
                                    newPropValue = newPropValue.Replace(@"[sl]", @"/");
                                    JProperty propToAdd = null;
                                    if (useBool)
                                        propToAdd = new JProperty(addPathArray[i], newValueBool);
                                    else if (useInt)
                                        propToAdd = new JProperty(addPathArray[i], newValueInt);
                                    else if (useDouble)
                                        propToAdd = new JProperty(addPathArray[i], newValueDouble);
                                    else //string
                                        propToAdd = new JProperty(addPathArray[i], newPropValue);
                                    jobjectPlaceholder.Add(propToAdd);
                                    break;
                                }
                                else
                                {
                                    //need to create a property with objects
                                    //check if object of this name already exists
                                    newJsonpath = newJsonpath + "." + addPathArray[i];
                                    JToken np = null;
                                    try
                                    {
                                        np = root.SelectToken(newJsonpath);
                                    }
                                    catch (Exception exNewJsonpath)
                                    {
                                        Logging.WriteToLog(string.Format("error in adding json object: {0}\n{1}", newJsonpath, exNewJsonpath.ToString()),
                                            Logfiles.Application, LogLevel.Exception);
                                    }
                                    if ((np != null) && (np is JObject))
                                    {
                                        //part of the path already exists, use that instead
                                        jobjectPlaceholder = (JObject)np;
                                        continue;
                                    }
                                    JObject nextObject = new JObject();
                                    JProperty nextProperty = new JProperty(addPathArray[i], nextObject);
                                    jobjectPlaceholder.Add(nextProperty);
                                    jobjectPlaceholder = nextObject;
                                }
                            }
                        }
                    }
                }
                catch (Exception add)
                {
                    Logging.WriteToLog(string.Format("Error adding json object\n{0}", add.ToString()), Logfiles.Application, LogLevel.Exception);
                }
            }
            else if (p.Mode.Equals("edit"))
            {
                try
                {
                    //get list of values from syntax
                    //foreach returned item, if it's not a JValue, abort patch
                    //if value equals regex value, edit it
                    //List<JToken> results = null;
                    IEnumerable<JToken> results = null;
                    try
                    {
                        results = root.SelectTokens(p.Path);
                    }
                    catch (Exception exResults)
                    {
                        Logging.WriteToLog(string.Format("error with selecting token: {0}, mode={1}\n{2}", p.Path, p.Mode, exResults.ToString()),
                            Logfiles.Application, LogLevel.Exception);
                    }
                    if (results == null || results.Count() == 0)
                    {
                        Logging.WriteToLog(string.Format("Path not found: {0}",p.Path),Logfiles.Application,LogLevel.Error);
                        return;
                    }
                    List<JValue> Jresults = new List<JValue>();
                    foreach(JToken jt in results)
                    {
                        if(jt is JValue Jvalue)
                        {
                            Jresults.Add(Jvalue);
                        }
                        else
                        {
                            Logging.WriteToLog(string.Format("Expected results of type JValue, returned {0}", jt.Type.ToString()),
                                Logfiles.Application, LogLevel.Error);
                            return;
                        }
                    }
                    Logging.Debug("number of Jvalues: {0}", Jresults.Count);
                    if (Jresults.Count == 0)
                    {
                        Logging.Debug("Jresults count is 0 (is this the intent?)");
                        return;
                    }
                    foreach(JValue jv in Jresults)
                    {
                        string jsonValue = "" + jv.Value;
                        if (useBool)
                            jsonValue = jsonValue.ToLower();
                        if (jsonValue.Trim().Equals(p.Replace.Replace(@"[sl]", @"/")))
                        {
                            //no need to update it, just return
                            continue;
                        }
                        if (!Regex.IsMatch(jsonValue, p.Search))
                            continue;
                        if (useBool)
                            jv.Value = newValueBool;
                        else if (useInt)
                            jv.Value = newValueInt;
                        else if (useDouble)
                            jv.Value = newValueDouble;
                        else //string
                            jv.Value = p.Replace.Replace(@"[sl]", @"/");
                    }
                }
                catch (Exception edit)
                {
                    Logging.WriteToLog(string.Format("error parsing patch {0} \n{1}", p.ActualPatchName, edit.ToString()),
                        Logfiles.Application, LogLevel.Exception);
                }
            }
            else if (p.Mode.Equals("remove"))
            {
                //get to array/object/property
                //if is array, remove it
                //else if is value, remove parent property
                //else if is object, remove it
                JToken cont = root.SelectToken(p.Path);
                
                if (cont == null)
                {
                    Logging.WriteToLog(string.Format("path to element not found: {0}", p.Path), Logfiles.Application, LogLevel.Error);
                    return;
                }
                if(cont is JValue)
                {
                    if(cont.Parent is JArray)
                    {
                        Logging.WriteToLog("Selected from p.path is JValue and parent is JArray. Use arrayRemove for this function",
                            Logfiles.Application,LogLevel.Error);
                        return;
                    }
                }
                cont = cont.Parent;//to get the JProperty
                if(Regex.IsMatch(cont.ToString(),p.Search))
                {
                    cont.Remove();
                }
            }
            else if (p.Mode.Equals("arrayAdd"))
            {
                //get to the array
                //if split string count is > 2, abort (invalid)
                //if split string count is 2, property and value
                //if split string count is 1, value
                //check thay array is of similar contents (if it has items)
                //unless p.search is "null", check each item.TString via regex if it already exists, if so abort
                //insert into array from index

                if(addPathArray.Count() > 2)
                {
                    Logging.WriteToLog(string.Format("invalid syntax of p.replace, more than 2 items detected: {0}", p.Replace),
                        Logfiles.Application, LogLevel.Error);
                    return;
                }
                JToken newObject = root.SelectToken(p.Path);
                
                //pull out if it failed to get the selection
                if (newObject == null)
                {
                    Logging.WriteToLog(string.Format("path {0} not found for {1}", p.Path, Path.GetFileName(p.CompletePath)),
                        Logfiles.Application,LogLevel.Error);
                    return;
                }
                if (!(newObject is JArray))
                {
                    Logging.WriteToLog(string.Format("ERROR: the path \"{0}\" does not lead to a Jarray", p.Path),Logfiles.Application,LogLevel.Error);
                    return;
                }
                JArray newObjectArray = (JArray)newObject;
                //check for index value in p.replace (name/value[index=NUMBER])
                string[] splitForAdd = p.Replace.Split(new string[] { @"[index=" }, StringSplitOptions.None);
                if(splitForAdd.Length < 2)
                {
                    Logging.WriteToLog(string.Format("ERROR: arrayAdd selected but replace value does not have [index=] tag"),
                        Logfiles.Application,LogLevel.Error);
                    return;
                }
                int index = Utils.ParseInt(splitForAdd[1].Replace(@"]", ""), -1);
                if(index >= newObjectArray.Count)
                {
                    //if the array is empty and the index is 0, trying to add to a blank array, don't log it
                    if(index != 0)
                        Logging.WriteToLog(string.Format("index value ({0})>= array count ({1}), putting at end of the array (is this the intent?)",
                            index,newObjectArray.Count), Logfiles.Application,LogLevel.Warning);
                    index = -1;
                }
                if (newObjectArray.Count > 0)
                {
                    if ((newObjectArray[0] is JValue) && (addPathArray.Count() == 2))
                    {
                        Logging.WriteToLog("array is of JValues and 2 replace arguemnts given",Logfiles.Application,LogLevel.Error);
                        return;
                    }
                    else if (!(newObjectArray[0] is JValue) && (addPathArray.Count() == 1))
                    {
                        Logging.WriteToLog("array is not of JValues and only 1 replace arguemnt given",Logfiles.Application,LogLevel.Error);
                        return;
                    }
                }
                JValue val = null;
                string value = addPathArray[0];
                if (addPathArray.Count() == 2)
                    value = addPathArray[1];
                value = value.Split(new string[] { @"[index=" }, StringSplitOptions.None)[0];
                value = value.Replace(@"[sl]", @"/");
                if (useBool)
                    val = new JValue(newValueBool);
                else if (useInt)
                    val = new JValue(newValueInt);
                else if (useDouble)
                    val = new JValue(newValueDouble);
                else //string
                    val = new JValue(value);
                if (addPathArray.Count() == 2)
                {
                    //add object with property
                    if(index == -1)
                    {
                        newObjectArray.Add(new JObject(new JProperty(addPathArray[0], val)));
                    }
                    else
                    {
                        newObjectArray.Insert(index, (new JObject(new JProperty(addPathArray[0], val))));
                    }
                }
                else
                {
                    //add value
                    if (index == -1)
                    {
                        newObjectArray.Add(val);
                    }
                    else
                    {
                        newObjectArray.Insert(index, val);
                    }
                }
            }
            else if (p.Mode.Equals("arrayRemove"))
            {
                //get to array from p.path
                //foreach item.ToString in the array
                //if regex match the first one, remove and break
                JToken newObject = root.SelectToken(p.Path);
                
                //pull out if it failed to get the selection
                if (newObject == null)
                {
                    Logging.WriteToLog(string.Format("path {0} not found for {1}", p.Path, Path.GetFileName(p.CompletePath)),
                        Logfiles.Application,LogLevel.Error);
                    return;
                }
                if (!(newObject is JArray))
                {
                    Logging.WriteToLog(string.Format("the path \"{0}\" does not lead to a JSON array", p.Path),
                        Logfiles.Application,LogLevel.Error);
                    return;
                }
                JArray newObjectArray = (JArray)newObject;
                if(newObjectArray.Count == 0)
                {
                    //can't remove from an array if it's empty #rollSafe
                    Logging.WriteToLog("WARNING: array is already empty, is this the intent? (i bet it's not)",Logfiles.Application,LogLevel.Warning);
                    return;
                }
                bool found = false;
                for(int i = 0; i < newObjectArray.Count; i++)
                {
                    if(Regex.IsMatch(newObjectArray[i].ToString(),p.Search))
                    {
                        found = true;
                        newObjectArray[i].Remove();
                        break;
                    }
                }
                if (!found)
                {
                    Logging.WriteToLog(string.Format("no results found for search \"{0}\", with path \"{1}\"", p.Search, p.Path),
                        Logfiles.Application,LogLevel.Error);
                    return;
                }
            }
            else if (p.Mode.Equals("arrayClear"))
            {
                //get to array from p.path
                //foreach item.ToString in the array
                //if regex match from p.search, remove
                JToken newObject = root.SelectToken(p.Path);
                
                //pull out if it failed to get the selection
                if (newObject == null)
                {
                    Logging.WriteToLog(string.Format("path {0} not found for {1}", p.Path, Path.GetFileName(p.CompletePath)),
                        Logfiles.Application,LogLevel.Warning);
                    return;
                }
                if (!(newObject is JArray))
                {
                    Logging.WriteToLog(string.Format("the path \"{0}\" does not lead to a JSON array", p.Path),Logfiles.Application,LogLevel.Error);
                    return;
                }
                JArray newObjectArray = (JArray)newObject;
                if (newObjectArray.Count == 0)
                {
                    //can't remove from an array if it's empty #rollSafe
                    Logging.WriteToLog("array is already empty",Logfiles.Application,LogLevel.Warning);
                    return;
                }
                bool found = false;
                for (int i = 0; i < newObjectArray.Count; i++)
                {
                    if (Regex.IsMatch(newObjectArray[i].ToString(), p.Search))
                    {
                        found = true;
                        newObjectArray[i].Remove();
                        i--;
                        //break;
                    }
                }
                if (!found)
                {
                    Logging.WriteToLog(string.Format("no results found for search \"{0}\", with path \"{1}\"", p.Search, p.Path),
                        Logfiles.Application,LogLevel.Warning);
                    return;
                }
            }
            else
            {
                Logging.WriteToLog(string.Format("ERROR: Unknown json patch mode, {0}", p.Mode),Logfiles.Application,LogLevel.Error);
                return;
            }

            StringBuilder rebuilder = new StringBuilder();
            string[] putBackDollas = root.ToString().Split('\n');
            for (int i = 0; i < putBackDollas.Count(); i++)
            {
                string temp = putBackDollas[i];
                if (Regex.IsMatch(temp, "willster419_refReplace"))
                {
                    temp = "\"" + ssList[0].Name + "\"" + ": {" + ssList[0].Value;
                    putBackDollas[i] = temp;
                    ssList.RemoveAt(0);
                }
                rebuilder.Append(putBackDollas[i] + "\n");
            }
            if (ssList.Count != 0)
            {
                Logging.WriteToLog(string.Format("There was an error with patching the file {0}, with extra refrences. aborting patch", p.CompletePath),
                    Logfiles.Application,LogLevel.Error);
                return;
            }
            string toWrite = rebuilder.ToString();
            try
            {
                JObject @object = JObject.Parse(toWrite);
            }
            catch (JsonReaderException e)
            {
                Logging.WriteToLog(string.Format("Failed to write file {0}: there was an error with it's json syntax (most likly an error in" +
                    " the replace property or it could indicate an error with the patching system) \n{1}", p.CompletePath,e.ToString()),
                    Logfiles.Application,LogLevel.Exception);
                return;
            }
            //unescape the macros
            //unescaped string looks like this (sample)
            //"[dollar][lbracket][quote]battle.xc[quote][colon][quote]expertPanel[quote][rbracket]"
            //^quotes included
            toWrite = toWrite
                //first replace the refrences (the ones with the quotes to remove the quotes)
                .Replace(@"""[dollar]", @"$")//start of refrence with dollar
                .Replace(@"[rbracket]""", @"}")//end of refrence with colon
                .Replace(@"[lbracket]", @"{")
                .Replace(@"[quote]", @"""")
                .Replace(@"[colon]",@":")
                .Replace(@"[dollar]", @"$")
                .Replace(@"[rbracket]", @"}");
            File.WriteAllText(p.CompletePath, toWrite);
            Logging.Debug("json patch completed successfully");
        }
        #endregion

        #region Helpers
        //returns the folder(s) to get to the xvm config folder directory
        public static string GetXvmFolderName()
        {
            //form where it should be
            string xvmBootFile = Path.Combine(Settings.WoTDirectory, "res_mods\\configs\\xvm\\xvm.xc");

            //check if it exists there
            if (!File.Exists(xvmBootFile))
            {
                Logging.Error("extractor asked to get location of xvm folder name, but boot file does not exist! returning \"default\"");
                return "default";
            }

            string fileContents = File.ReadAllText(xvmBootFile);

            //patch block comments out
            fileContents = Regex.Replace(fileContents, @"\/\*.*\*\/", string.Empty, RegexOptions.Singleline);

            //remove return character
            fileContents = fileContents.Replace("\r",string.Empty);

            //patch single line comments out
            string[] removeComments = fileContents.Split('\n');
            StringBuilder bootBuilder = new StringBuilder();
            foreach (string s in removeComments)
            {
                if (Regex.IsMatch(s, @"\/\/.*$"))
                    continue;
                bootBuilder.Append(s + "\n");
            }
            fileContents = bootBuilder.ToString().Trim();

            //get the path from the json style
            Match match = Regex.Match(fileContents, @"\${.*:.*}");
            string innerJsonValue = match.Value;

            //the second quote set is what we're interested in, the file path
            string[] splitIt = innerJsonValue.Split('"');
            string filePath = splitIt[1];

            //split again to get just the folder name
            string folderName = filePath.Split('/')[0];

            return folderName;
        }
        #endregion
    }
}
