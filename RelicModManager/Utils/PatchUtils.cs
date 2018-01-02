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
using System.Globalization;

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
        
        public static void XMLPatch(Patch p, bool testMods = false, string testXVMBootLoc = "")
        {
            //verify the file exists...
            if (!File.Exists(p.completePath))
            {
                Logging.Manager("WARNING: file not found");
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.Load(p.completePath);
            //check to see if it has the header info at the top to see if we need to remove it later
            bool hadHeader = false;
            XmlDocument doc3 = new XmlDocument();
            doc3.Load(p.completePath);
            foreach (XmlNode node in doc3)
            {
                if (node.NodeType == XmlNodeType.XmlDeclaration)
                {
                    hadHeader = true;
                }
            }
            //determines which version of pathing will be done
            switch (p.mode)
            {
                case "add":
                    //check to see if it's already there
                    //make the full node path
                    string[] tempp = p.replace.Split('/');
                    string fullNodePath = p.path;
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
                    XmlNode reff = doc.SelectSingleNode(p.path);
                    //create node(s) to add to the element
                    string[] temp = p.replace.Split('/');
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
                    if (File.Exists(p.completePath))
                        File.Delete(p.completePath);
                    doc.Save(p.completePath);
                    break;

                case "edit":
                    //check to see if it's already there
                    XmlNodeList currentSoundBanksEdit = doc.SelectNodes(p.path);
                    foreach (XmlElement e in currentSoundBanksEdit)
                    {
                        string innerText = e.InnerText;
                        innerText = innerText.Trim();
                        if (e.InnerText.Equals(p.replace))
                            return;
                    }
                    //find and replace
                    //XmlNodeList rel1Edit = doc.SelectNodes(xpath);
                    foreach (XmlElement eee in currentSoundBanksEdit)
                    {
                        if (Regex.IsMatch(eee.InnerText, p.search))
                        {
                            eee.InnerText = p.replace;
                        }
                    }
                    //save it
                    if (File.Exists(p.completePath)) File.Delete(p.completePath);
                    doc.Save(p.completePath);
                    break;

                case "remove":
                    //check to see if it's there
                    XmlNodeList currentSoundBanksRemove = doc.SelectNodes(p.path);
                    foreach (XmlElement e in currentSoundBanksRemove)
                    {
                        if (Regex.IsMatch(e.InnerText, p.search))
                        {
                            e.RemoveAll();
                        }
                    }
                    //save it
                    if (File.Exists(p.completePath)) File.Delete(p.completePath);
                    doc.Save(p.completePath);
                    //remove empty elements
                    XDocument doc2 = XDocument.Load(p.completePath);
                    doc2.Descendants().Where(e => string.IsNullOrEmpty(e.Value)).Remove();
                    if (File.Exists(p.completePath))
                        File.Delete(p.completePath);
                    doc2.Save(p.completePath);
                    break;
            }
            //check to see if we need to remove the header
            bool hasHeader = false;
            XmlDocument doc5 = new XmlDocument();
            doc5.Load(p.completePath);
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
                doc4.Load(p.completePath);
                foreach (XmlNode node in doc4)
                {
                    if (node.NodeType == XmlNodeType.XmlDeclaration)
                    {
                        doc4.RemoveChild(node);
                    }
                }
                doc4.Save(p.completePath);
            }
        }
        //method to patch a standard text or json file
        //fileLocation is relative to res_mods folder
        public static void RegxPatch(Patch p, int lineNumber = 0, bool testMods = false, string testXVMBootLoc = "")
        {
            //verify the file exists...
            if (!File.Exists(p.completePath))
            {
                Logging.Manager("WARNING: file not found");
                return;
            }

            //replace all "fake escape characters" with real escape characters
            p.search = p.search.Replace(@"\n", "newline");
            p.search = p.search.Replace(@"\r", "\r");
            p.search = p.search.Replace(@"\t", "\t");

            //load file from disk...
            string file = File.ReadAllText(p.completePath);
            //parse each line into an index array
            string[] fileParsed = file.Split('\n');
            StringBuilder sb = new StringBuilder();
            if (lineNumber == 0)
            //search entire file and replace each instance
            {
                bool everReplaced = false;
                for (int i = 0; i < fileParsed.Count(); i++)
                {
                    if (Regex.IsMatch(fileParsed[i], p.search))
                    {
                        fileParsed[i] = Regex.Replace(fileParsed[i], p.search, p.replace);
                        fileParsed[i] = Regex.Replace(fileParsed[i], "newline", "\n");
                        //fileParsed[i] = Regex.Replace(fileParsed[i], @"\n", "\n");
                        everReplaced = true;
                    }
                    sb.Append(fileParsed[i] + "\n");
                }
                if (!everReplaced)
                {
                    Logging.Manager("WARNING: Regex never matched");
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
                    if (Regex.IsMatch(file, p.search))
                    {
                        file = Regex.Replace(file, p.search, p.replace);
                        file = Regex.Replace(file, "newline", "\n");
                        //file = Regex.Replace(file, @"\n", "\n");
                        sb.Append(file);
                    }
                    else
                    {
                        Logging.Manager("WARNING: Regex match not found");
                        return;
                    }
                }
                catch (ArgumentException)
                {
                    if (testMods) MessageBox.Show("invalid regex command");
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
                        if (Regex.IsMatch(value, p.search))
                        {
                            fileParsed[i] = Regex.Replace(fileParsed[i], p.search, p.replace);
                            fileParsed[i] = Regex.Replace(fileParsed[i], "newline", "\n");
                            //fileParsed[i] = Regex.Replace(fileParsed[i], @"\n", "\n");
                        }
                    }
                    sb.Append(fileParsed[i] + "\n");
                }
                if (!everReplaced)
                {
                    Logging.Manager("WARNING: Regex never matched");
                    return;
                }
            }
            //save the file back into the string and then the file
            file = sb.ToString();
            File.WriteAllText(p.completePath, file);
        }
        //method to parse json files
        public static void JSONPatch(Patch p, bool testMods = false, string testXVMBootLoc = "")
        {
            //try to convert the new value to a bool or an int or double first
            bool newValueBool = false;
            int newValueInt = -69420;
            double newValueDouble = -69420.0d;
            bool useBool = false;
            bool useInt = false;
            bool useDouble = false;
            //legacy compatibility: many json patches don't have a valid regex systax for p.search, assume they mean a forced replace
            if (p.search.Equals(""))
                p.search = @".*";
            //legacy compatibility: treat p.mode being nothing or null default to edit
            if (p.mode == null || p.mode.Equals("") || p.mode.Equals("arrayEdit"))
                p.mode = "edit";
            //split the replace path here so both can use it later
            string[] addPathArray = null;
            string testValue = p.replace;
            if (p.mode.Equals("add") || p.mode.Equals("arrayAdd"))
            {
                //.Split(new string[] { @"[index=" }, StringSplitOptions.None)
                addPathArray = p.replace.Split('/');
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
            
            //verify the file exists...
            if (!File.Exists(p.completePath))
            {
                Logging.Manager("WARNING: file not found");
                return;
            }

            //load file from disk...
            string file = File.ReadAllText(p.completePath);
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
            JsonLoadSettings settings = new JsonLoadSettings()
            {
                CommentHandling = CommentHandling.Ignore
            };
            JObject root = null;
            //load json for editing
            try
            {
                root = JObject.Parse(file, settings);
            }
            catch (JsonReaderException j)
            {
                Logging.Manager(string.Format("ERROR: Failed to patch {0}", p.completePath));
                if (Program.testMode)
                {
                    //in test mode this is worthy of an EXCEPTION
                    throw new JsonReaderException(j.Message);
                }
            }
            //if it failed to parse show the message (above) and pull out
            if (root == null)
            {
                Logging.Manager(string.Format("ERROR: Failed to patch {0}", p.completePath));
                return;
            }
            if (p.mode.Equals("add"))
            {
                //get to object from p.path
                //if in p.replace regex matches [array], add a blank array if not already there and return
                //if in p.replace regex matches [object], add a blank object if not already there and return
                //make full json path of what user wants to add to check, if the value is already there, return if so
                //for each split element, make objects for path IF not already exist, then property

                JContainer result = null;
                //mode 1: adding blank array
                //add if the desired path to the array return null
                if (Regex.IsMatch(p.replace,@".*\[array\]$"))
                {
                    string propName = p.replace.Replace(@"[array]", "");
                    result = (JContainer)root.SelectToken(p.path + "." + propName);
                    if(result != null)
                    {
                        Logging.Manager("ERROR: cannot add blank array when object already exists");
                        return;
                    }
                    JContainer pathForArray = (JContainer)root.SelectToken(p.path);
                    JArray ary = new JArray();
                    JProperty prop = new JProperty(propName, ary);
                    pathForArray.Add(prop);
                }
                //mode 2: adding property with blank object
                //add if the desired path returns null
                else if (Regex.IsMatch(p.replace,@".*\[object\]$"))
                {
                    string propName = p.replace.Replace(@"[object]", "");
                    result = (JContainer)root.SelectToken(p.path + "." + propName);
                    if (result != null)
                    {
                        Logging.Manager("ERROR: cannot add blank array when object already exists");
                        return;
                    }
                    JContainer pathForArray = (JContainer)root.SelectToken(p.path);
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
                    string fullJSONPath = p.path;
                    for (int i = 0; i < addPathArray.Count() - 1; i++)
                    {
                        fullJSONPath = fullJSONPath + "." + addPathArray[i];
                    }
                    JToken val = root.SelectToken(fullJSONPath);
                    //null means the fullJSONPath was invalid, the item is not already there
                    if (val != null)
                    {
                        if (!(val is JValue))
                        {
                            Logging.Manager(string.Format("ERROR: JToken found at {0}, but is not JValue", fullJSONPath));
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
                        JObject newJobject = (JObject)root.SelectToken(p.path);
                        if (newJobject == null)//error
                        {
                            Logging.Manager("fullJSONPath does not exist in the file");
                            return;
                        }
                        JObject jobjectPlaceholder = newJobject;
                        string newJsonpath = p.path;
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
                                JToken np = root.SelectToken(newJsonpath);
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
            else if (p.mode.Equals("edit"))
            {
                //get list of values from syntax
                //foreach returned item, if it's not a JValue, abort patch
                //if value equals regex value, edit it
                //List<JToken> results = null;
                IEnumerable<JToken> results = root.SelectTokens(p.path);
                if(results == null || results.Count() == 0)
                {
                    Logging.Manager("WARNING: p.path not found");
                    return;
                }
                List<JValue> Jresults = new List<JValue>();
                foreach(JToken jt in results)
                {
                    if(!(jt is JValue))
                    {
                        Logging.Manager("ERROR: returned token for p.path is not a JValue, aborting patch");
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
                    if (jsonValue.Trim().Equals(p.replace.Replace(@"[sl]", @"/")))
                    {
                        //no need to update it, just return
                        continue;
                    }
                    if (!Regex.IsMatch(jsonValue, p.search))
                        continue;
                    if (useBool)
                        jv.Value = newValueBool;
                    else if (useInt)
                        jv.Value = newValueInt;
                    else if (useDouble)
                        jv.Value = newValueDouble;
                    else //string
                        jv.Value = p.replace.Replace(@"[sl]", @"/");
                }
            }
            else if (p.mode.Equals("remove"))
            {
                //get to array/object/property
                //if is array, remove it
                //else if is value, remove parent property
                //else if is object, remove it
                JToken cont = root.SelectToken(p.path);
                if (cont == null)
                {
                    Logging.Manager(string.Format("ERROR: path \"{0}\" returns null", p.path));
                    return;
                }
                if(cont is JValue)
                {
                    if(cont.Parent is JArray)
                    {
                        Logging.Manager("ERROR: Selected from p.path is JValue and parent is JArray. Use arrayRemove for this function");
                        return;
                    }
                }
                cont = cont.Parent;//to get the JProperty
                if(Regex.IsMatch(cont.ToString(),p.search))
                {
                    cont.Remove();
                }
            }
            else if (p.mode.Equals("arrayAdd"))
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
                    Logging.Manager("ERROR: invalid syntax of p.replace (more than 2 items detected)");
                    return;
                }
                JToken newObject = root.SelectToken(p.path);
                //pull out if it failed to get the selection
                if (newObject == null)
                {
                    Logging.Manager(string.Format("WARNING: path {0} not found for {1}", p.path, Path.GetFileName(p.completePath)));
                    return;
                }
                if (!(newObject is JArray))
                {
                    Logging.Manager(string.Format("ERROR: the path \"{0}\" does not lead to a Jarray", p.path));
                    return;
                }
                JArray newObjectArray = (JArray)newObject;
                //check for index value in p.replace (name/value[index=NUMBER])
                int index = Utils.ParseInt(p.replace.Split(new string[] { @"[index=" }, StringSplitOptions.None)[1].Replace(@"]", ""), -1);
                if(index >= newObjectArray.Count)
                {
                    //if the array is empty and the index is 0, trying to add to a blank array, don't log it
                    if(index != 0)
                        Logging.Manager("WARNING: index value is greator than array count, putting at end of the array");
                    index = -1;
                }
                if (newObjectArray.Count > 0)
                {
                    if ((newObjectArray[0] is JValue) && (addPathArray.Count() == 2))
                    {
                        Logging.Manager("ERROR: array is of JValues and 2 replace arguemnts given");
                        return;
                    }
                    else if (!(newObjectArray[0] is JValue) && (addPathArray.Count() == 1))
                    {
                        Logging.Manager("ERROR: array is not of JValues and only 1 replace arguemnt given");
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
            else if (p.mode.Equals("arrayRemove"))
            {
                //get to array from p.path
                //foreach item.ToString in the array
                //if regex match the first one, remove and break
                JToken newObject = root.SelectToken(p.path);
                //pull out if it failed to get the selection
                if (newObject == null)
                {
                    Logging.Manager(string.Format("WARNING: path {0} not found for {1}", p.path, Path.GetFileName(p.completePath)));
                    return;
                }
                if (!(newObject is JArray))
                {
                    Logging.Manager(string.Format("ERROR: the path \"{0}\" does not lead to a JSON array", p.path));
                    return;
                }
                JArray newObjectArray = (JArray)newObject;
                if(newObjectArray.Count == 0)
                {
                    //can't remove from an array if it's empty #rollSafe
                    Logging.Manager("WARNING: array is already empty");
                    return;
                }
                bool found = false;
                for(int i = 0; i < newObjectArray.Count; i++)
                {
                    if(Regex.IsMatch(newObjectArray[i].ToString(),p.search))
                    {
                        found = true;
                        newObjectArray[i].Remove();
                        break;
                    }
                }
                if (!found)
                {
                    Logging.Manager(string.Format("WARNING: no results found for search \"{0}\", with path \"{1}\"", p.search, p.path));
                    return;
                }
            }
            else if (p.mode.Equals("arrayClear"))
            {
                //get to array from p.path
                //foreach item.ToString in the array
                //if regex match from p.search, remove
                JToken newObject = root.SelectToken(p.path);
                //pull out if it failed to get the selection
                if (newObject == null)
                {
                    Logging.Manager(string.Format("WARNING: path {0} not found for {1}", p.path, Path.GetFileName(p.completePath)));
                    return;
                }
                if (!(newObject is JArray))
                {
                    Logging.Manager(string.Format("ERROR: the path \"{0}\" does not lead to a JSON array", p.path));
                    return;
                }
                JArray newObjectArray = (JArray)newObject;
                if (newObjectArray.Count == 0)
                {
                    //can't remove from an array if it's empty #rollSafe
                    Logging.Manager("WARNING: array is already empty");
                    return;
                }
                bool found = false;
                for (int i = 0; i < newObjectArray.Count; i++)
                {
                    if (Regex.IsMatch(newObjectArray[i].ToString(), p.search))
                    {
                        found = true;
                        newObjectArray[i].Remove();
                        i--;
                        //break;
                    }
                }
                if (!found)
                {
                    Logging.Manager(string.Format("WARNING: no results found for search \"{0}\", with path \"{1}\"", p.search, p.path));
                    return;
                }
            }
            else
            {
                Logging.Manager(string.Format("ERROR: Unknown json patch mode, {0}", p.mode));
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
            {
                Logging.Manager(string.Format("There was an error with patching the file {0}, with extra refrences. aborting patch", p.completePath));
                return;
            }
            File.WriteAllText(p.completePath, rebuilder.ToString());
        }

        public static void PMODPatch(Patch p, bool testMods = false, string testXVMBootLoc = "")
        {
            NumByteReads = 0;
            PatchDone = false;
            GenericTraverse = 0;
            //check that the file exists
            if (!File.Exists(p.completePath))
                return;

            //patch "newline" out of the replace text
            //p.replace = Regex.Replace(p.replace, "newline", "\n");
            p.replace = p.replace.Replace("newline", "\n");
            
            //break down the path into an array
            string[] pathArrayy = p.path.Split('.');
            List<string> pathArray = new List<string>();
            //convert it to a List cause it has more features
            foreach (string s in pathArrayy)
                pathArray.Add(s);
            //load the file from disk
            NumByteReads = 0;
            ReadInside(pathArray, p.completePath, p.replace, p.search, p.mode, p.path);
        }

        public static void XVMPatch(Patch p, bool testMods = false, string testXVMBootLoc = "")
        {
            NumByteReads = 0;
            PatchDone = false;
            GenericTraverse = 0;
            //check that the file exists
            if (!File.Exists(p.completePath))
                return;

            //patch "newline" out of the replace text
            //p.replace = Regex.Replace(p.replace, "newline", "\n");
            p.replace = p.replace.Replace("newline", "\n");

            //break down the path into an array
            string[] pathArrayy = p.path.Split('.');
            List<string> pathArray = new List<string>();
            //convert it to a List cause it has more features
            foreach (string s in pathArrayy)
                pathArray.Add(s);
            //create the stringBuilder to rewrite the file
            StringBuilder sb = new StringBuilder();
            //load the file from disk

            //read untill *start of string*${
            NumByteReads = 0;
            string fileContents = File.ReadAllText(p.completePath);
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
            ReadInside(pathArray, Path.Combine(Path.GetDirectoryName(p.completePath), filePath), p.replace, p.search, p.mode, p.path);
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
                    Logging.Manager(string.Format("ERROR: Path not found: {0}", origXvmPath));
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
                        Logging.Manager(string.Format("ERROR: Path not found: {0}", origXvmPath));
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
                            Logging.Manager("invalid index: " + pathArray[0]);
                            return;
                        }
                    }
                    else if (Regex.IsMatch(pathArray[0], @"\[-1\]+"))
                    {
                        //-1 keyword for the add array method
                        if (!mode.Equals("array_add"))
                        {
                            Logging.Manager("To use -1 keyword, must be in array_add mode!");
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
                            Logging.Manager(string.Format("invalid index: {0}", pathArray[0]));
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
                        Logging.Manager(string.Format("Invalid mode: {0} for xvm patch {1}", mode, origXvmPath));
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
                Logging.Manager(string.Format("ERROR: Path not found: {0}", origXvmPath));
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
                Logging.Manager(string.Format("ERROR: Path not found: {0}", origXvmPath));
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
            //string bootFile = Path.Combine(tanksLocation, XVMBootFileLoc1);
            if (customBootFileLoc != null)
                bootFile = customBootFileLoc;
            if (!File.Exists(bootFile))
            {
                if (writeToLog)
                    Logging.Manager(string.Format("ERROR: xvm config boot file does not exist at {0}, checking {1}", XVMBootFileLoc1, XVMBootFileLoc2));
                else
                    Logging.Manager(string.Format("NOTICE: default run, xvm config boot file does not exist at {0}, checking {1}", XVMBootFileLoc1, XVMBootFileLoc2));
                bootFile = tanksLocation + XVMBootFileLoc2;
                //bootFile = Path.Combine(tanksLocation, XVMBootFileLoc2);
                if (!File.Exists(bootFile))
                {
                    if (writeToLog)
                        Logging.Manager(string.Format("ERROR: xvm config boot file does not exist at {0}, aborting patch", XVMBootFileLoc2));
                    else
                        Logging.Manager(string.Format("NOTICE: default run, xvm config boot file does not exist at {0}, user did not install xvm", XVMBootFileLoc2));
                    return null;
                }
            }
            Logging.Manager("xvm boot file located to parse");
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
