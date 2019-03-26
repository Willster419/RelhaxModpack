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
        #region Statics
        //note that the "\\" is for escaping. it means that \\ is acually "\"
        private static string XVMBootFileLoc1 = "\\res_mods\\configs\\xvm\\xvm.xc";
        private static string XVMBootFileLoc2 = "\\mods\\configs\\xvm\\xvm.xc";
        #endregion

        #region Main Patch Method
        public static void RunPatch(Patch p)
        {
            //check if file exists
            if (!File.Exists(p.CompletePath))
            {
                Logging.Warning("File {0} not found", p.CompletePath);
                return;
            }
            //macro parsing needs to go here
            Logging.Info(p.DumpPatchInfoForLog,Logfiles.Application);

            //actually run the patches based on what type it is
            switch (p.Type.ToLower())
            {
                case "regex":
                case "regx":
                    RegxPatch(p);
                    break;
                case "xml":
                    XMLPatch(p);
                    break;
                case "json":
                    JSONPatch(p);
                    break;
                case "xvm":
                    throw new BadMemeException("xvm patches are not supported, please use the json patch method");
            }
        }
        #endregion

        #region XML
        public static void XMLPatch(Patch p)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(p.CompletePath);
            //check to see if it has the header info at the top to see if we need to remove it later
            bool hadHeader = false;
            XmlDocument doc3 = new XmlDocument();
            doc3.Load(p.CompletePath);
            foreach (XmlNode node in doc3)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    hadHeader = true;
                }
            }
            //determines which version of pathing will be done
            switch (p.Mode)
            {
                case "add":
                    //check to see if it's already there
                    //make the full node path
                    string[] tempp = p.Replace.Split('/');
                    string fullNodePath = p.Path;
                    for (int i = 0; i < tempp.Count() - 1; i++)
                    {
                        fullNodePath = fullNodePath + "/" + tempp[i];
                    }
                    //in each node check if the element exist with the replace innerText
                    XmlNodeList currentSoundBanksAdd = doc.SelectNodes(fullNodePath);
                    foreach (XmlElement e in currentSoundBanksAdd)
                    {
                        string innerText = tempp[tempp.Count() - 1];
                        //remove any tabs and whitespaces first before testing
                        innerText = innerText.Trim();
                        if (e.InnerText.Equals(innerText))
                            return;
                    }
                    //get to the node where to add the element
                    XmlNode reff = doc.SelectSingleNode(p.Path);
                    //create node(s) to add to the element
                    string[] temp = p.Replace.Split('/');
                    List<XmlElement> nodes = new List<XmlElement>();
                    for (int i = 0; i < temp.Count() - 1; i++)
                    {
                        XmlElement ele = doc.CreateElement(temp[i]);
                        if (i == temp.Count() - 2)
                        {
                            //last node with actual data to add
                            string data = temp[temp.Count() - 1];
                            data = data.Replace(@"[sl]", @"/");
                            ele.InnerText = data;
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
                    if (File.Exists(p.CompletePath))
                        File.Delete(p.CompletePath);
                    doc.Save(p.CompletePath);
                    break;

                case "edit":
                    //check to see if it's already there
                    XmlNodeList currentSoundBanksEdit = doc.SelectNodes(p.Path);
                    foreach (XmlElement e in currentSoundBanksEdit)
                    {
                        string innerText = e.InnerText;
                        innerText = innerText.Trim();
                        if (e.InnerText.Equals(p.Replace))
                            return;
                    }
                    //find and replace
                    //XmlNodeList rel1Edit = doc.SelectNodes(xpath);
                    foreach (XmlElement eee in currentSoundBanksEdit)
                    {
                        if (Regex.IsMatch(eee.InnerText, p.Search))
                        {
                            eee.InnerText = p.Replace;
                        }
                    }
                    //save it
                    if (File.Exists(p.CompletePath)) File.Delete(p.CompletePath);
                    doc.Save(p.CompletePath);
                    break;

                case "remove":
                    //check to see if it's there
                    XmlNodeList currentSoundBanksRemove = doc.SelectNodes(p.Path);
                    foreach (XmlElement e in currentSoundBanksRemove)
                    {
                        if (Regex.IsMatch(e.InnerText, p.Search))
                        {
                            e.RemoveAll();
                        }
                    }
                    //save it
                    if (File.Exists(p.CompletePath)) File.Delete(p.CompletePath);
                    doc.Save(p.CompletePath);
                    //remove empty elements
                    XDocument doc2 = XDocument.Load(p.CompletePath);
                    doc2.Descendants().Where(e => string.IsNullOrEmpty(e.Value)).Remove();
                    if (File.Exists(p.CompletePath))
                        File.Delete(p.CompletePath);
                    doc2.Save(p.CompletePath);
                    break;
            }
            //check to see if we need to remove the header
            bool hasHeader = false;
            XmlDocument doc5 = new XmlDocument();
            doc5.Load(p.CompletePath);
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
                doc4.Load(p.CompletePath);
                foreach (XmlNode node in doc4)
                {
                    if (node.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        doc4.RemoveChild(node);
                    }
                }
                doc4.Save(p.CompletePath);
            }
        }
        #endregion

        #region REGEX
        //method to patch a standard text or json file
        //fileLocation is relative to res_mods folder
        public static void RegxPatch(Patch p, int lineNumber = 0)
        {
            //replace all "fake escape characters" with real escape characters
            //TODO: fix newlines and add warning for search and replace
            p.Search = p.Search.Replace(@"\n", "newline");
            p.Search = p.Search.Replace(@"\r", "\r");
            p.Search = p.Search.Replace(@"\t", "\t");

            //load file from disk...
            string file = File.ReadAllText(p.CompletePath);
            //parse each line into an index array
            string[] fileParsed = file.Split('\n');
            StringBuilder sb = new StringBuilder();
            if (lineNumber == 0)
            //search entire file and replace each instance
            {
                bool everReplaced = false;
                for (int i = 0; i < fileParsed.Count(); i++)
                {
                    if (Regex.IsMatch(fileParsed[i], p.Search))
                    {
                        fileParsed[i] = Regex.Replace(fileParsed[i], p.Search, p.Replace);
                        fileParsed[i] = Regex.Replace(fileParsed[i], "newline", "\n");
                        //fileParsed[i] = Regex.Replace(fileParsed[i], @"\n", "\n");
                        everReplaced = true;
                    }
                    sb.Append(fileParsed[i] + "\n");
                }
                if (!everReplaced)
                {
                    Logging.WriteToLog("Regex never matched, is this the intent?", Logfiles.Application, LogLevel.Warning);
                    return;
                }
            }
            else if (lineNumber == -1)
            //search entire file and string and make one giant regex replacement
            {
                //but remove newlines first
                file = Regex.Replace(file, "\n", "newline");
                try
                {
                    if (Regex.IsMatch(file, p.Search))
                    {
                        file = Regex.Replace(file, p.Search, p.Replace);
                        file = Regex.Replace(file, "newline", "\n");
                        //file = Regex.Replace(file, @"\n", "\n");
                        sb.Append(file);
                    }
                    else
                    {
                        Logging.WriteToLog("Regex match not found", Logfiles.Application, LogLevel.Warning);
                        return;
                    }
                }
                catch (ArgumentException)
                {
                    Logging.WriteToLog("Invalid regex command", Logfiles.Application, LogLevel.Error);
                }
            }
            else
            {
                bool everReplaced = false;
                for (int i = 0; i < fileParsed.Count(); i++)
                {
                    if (i == lineNumber - 1)
                    {
                        string value = fileParsed[i];
                        if (Regex.IsMatch(value, p.Search))
                        {
                            fileParsed[i] = Regex.Replace(fileParsed[i], p.Search, p.Replace);
                            fileParsed[i] = Regex.Replace(fileParsed[i], "newline", "\n");
                            //fileParsed[i] = Regex.Replace(fileParsed[i], @"\n", "\n");
                        }
                    }
                    sb.Append(fileParsed[i] + "\n");
                }
                if (!everReplaced)
                {
                    Logging.WriteToLog("Regex never matched, is this the intent?", Logfiles.Application, LogLevel.Warning);
                    return;
                }
            }
            //save the file back into the string and then the file
            file = sb.ToString();
            File.WriteAllText(p.CompletePath, file);
        }
        #endregion

        #region JSON
        //method to parse json files
        public static void JSONPatch(Patch p)
        {
            //try to convert the new value to a bool or an int or double first
            bool newValueBool = false;
            int newValueInt = -69420;
            double newValueDouble = -69420.0d;
            bool useBool = false;
            bool useInt = false;
            bool useDouble = false;
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
                        if(!(jt is JValue))
                        {
                            //Logging.WriteToLog("ERROR: returned token for p.path is not a JValue, aborting patch");
                            Logging.WriteToLog(string.Format("Expected results of type JValue, returned {0}", jt.Type.ToString()),
                                Logfiles.Application, LogLevel.Error);
                            return;
                        }
                        Jresults.Add((JValue)jt);
                    }
                    //Logging.Manager("DEBUG: number of Jvalues: " + Jresults.Count);
                    if (Jresults.Count == 0)
                        return;
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
        }
        #endregion
    }
}
