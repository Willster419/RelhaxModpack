﻿using System;
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
    /// <summary>
    /// Exit codes during a patch operation
    /// </summary>
    public enum PatchExitCode
    {
        /// <summary>
        /// The patch operation succeeded
        /// </summary>
        Success = 0,

        /// <summary>
        /// The patch operation succeeded, but with warnings
        /// </summary>
        Warning = -1,

        /// <summary>
        /// The patch operation did not succeeded
        /// </summary>
        Error = -2
    }

    /// <summary>
    /// A class for handling patch operations
    /// </summary>
    public static class PatchUtils
    {

        private static PatchExitCode PatchExitCodeForJson = PatchExitCode.Error;

        #region Main Patch Method
        /// <summary>
        /// Run a patch operation
        /// </summary>
        /// <param name="p">The patch instructions object</param>
        /// <returns>The operation exit code</returns>
        public static PatchExitCode RunPatch(Patch p)
        {
            //parse macros for when not from editor. because the editor will pre-parse macros before sending them
            if (!p.FromEditor)
            {
                //if we are in stand alone patch mode
                if(CommandLineSettings.ApplicationMode == ApplicationMode.Patcher)
                {
                    string patchPathStart = string.Empty;
                    if(Utils.FilePathDict.ContainsKey(@"{app}"))
                    {
                        Logging.Info("[PatchUtils]: {{app}} key found - using path replace macro ({0})", Settings.ApplicationStartupPath);
                        patchPathStart = Utils.MacroReplace(@"{app}", ReplacementTypes.FilePath);
                    }
                    else
                    {
                        Logging.Info("[PatchUtils]: no {{app}} key - using path relative to application ({0})", Settings.ApplicationStartupPath);
                        patchPathStart = Settings.ApplicationStartupPath;
                    }

                    if(p.File.Contains("versiondir"))
                    {
                        if (Utils.FilePathDict.ContainsKey(@"versiondir"))
                        {
                            Logging.Info("[PatchUtils]: 'versiondir' key found, replacing path with supplied tanks version");

                            p.File = Utils.MacroReplace(p.File, ReplacementTypes.FilePath);
                        }
                    }

                    if (p.File[0].Equals('\\'))
                    {
                        Logging.Debug("[PatchUtils]: p.file starts with '\\', removing for path combine");
                        p.File = p.File.Substring(1);
                    }

                    if (patchPathStart[patchPathStart.Length-1].Equals('\\'))
                    {
                        Logging.Debug("[PatchUtils]: patchPathStart end with '\\', removing for path combine");
                        patchPathStart = patchPathStart.Substring(0,patchPathStart.Length-1);
                    }
                    p.CompletePath = Path.Combine(patchPathStart, p.File);
                    Logging.Info("[PatchUtils]: complete path to patch parsed as '{0}'", p.CompletePath);
                }
                else//from application regular install
                {
                    Logging.Info("[PatchUtils]: Patch format version: {0}", p.Version);
                    //process the start of the path
                    if(string.IsNullOrWhiteSpace(p.PatchPath))
                    {
                        Logging.Warning("[PatchUtils]: PatchPath is empty, using '{{app}}'", p.PatchPath);
                        p.PatchPath = Settings.WoTDirectory;
                    }
                    else
                    {
                        if(!p.PatchPath[0].Equals('{'))
                        {
                            Logging.Warning("[PatchUtils]: application patchpath macro does not start with '{', needs to be updated");
                            //https://stackoverflow.com/questions/91362/how-to-escape-braces-curly-brackets-in-a-format-string-in-net
                            p.PatchPath = string.Format("{{{0}}}", p.PatchPath);
                        }
                        p.PatchPath = Utils.MacroReplace(p.PatchPath, ReplacementTypes.FilePath);
                    }

                    //check for that dumb filepath thing i did a while back
                    if(p.File.Contains(@"\\"))
                    {
                        Logging.Warning("[PatchUtils]: found legacy patch folder slashes, please update to remove extra slashes!");
                        p.File = p.File.Replace(@"\\", @"\");
                    }

                    if(p.File[0].Equals('\\'))
                    {
                        Logging.Info("[PatchUtils]: p.file starts with '\\', removing for path combine");
                        p.File = p.File.Substring(1);
                    }

                    //also check for if "xvmConfigFolderName" exists yet in the file path macro location
                    if(!Utils.FilePathDict.ContainsKey(@"xvmConfigFolderName"))
                        Utils.FilePathDict.Add(@"xvmConfigFolderName", GetXvmFolderName().Trim());

                    p.File = Utils.MacroReplace(p.File, ReplacementTypes.FilePath);
                    p.CompletePath = Path.Combine(p.PatchPath, p.File);
                }
            }

            //dump info for logging
            Logging.Info(p.DumpPatchInfoForLog);

            //check if file exists
            if (!File.Exists(p.CompletePath))
            {
                Logging.Warning("[PatchUtils]: File '{0}' not found", p.CompletePath);
                return PatchExitCode.Warning;
            }

            //if from the editor, enable verbose logging (allows it to get debug log statements)
            bool tempVerboseLoggingSetting = ModpackSettings.VerboseLogging;
            if(p.FromEditor && !ModpackSettings.VerboseLogging)
            {
                Logging.Debug("[PatchUtils]: p.FromEditor=true and ModpackSettings.VerboseLogging=false, setting to true for duration of patch method");
                ModpackSettings.VerboseLogging = true;
            }

            if(p.Version == 1)
            {
                Logging.Warning("[PatchUtils]: patch is V1 format, please update to V2!");
            }

            //actually run the patches based on what type it is
            PatchExitCode patchSuccess = PatchExitCode.Error;
            switch (p.Type.ToLower())
            {
                case "regex":
                case "regx":
                    if (p.Lines == null || p.Lines.Count() == 0)
                    {
                        Logging.Debug("[PatchUtils]: Running regex patch as all lines, line by line");
                        patchSuccess = RegxPatch(p, null);
                    }
                    else if (p.Lines.Count() == 1 && p.Lines[0].Trim().Equals("-1"))
                    {
                        Logging.Debug("[PatchUtils]: Running regex patch as whole file");
                        patchSuccess = RegxPatch(p, new int[] { -1 });
                    }
                    else
                    {
                        Logging.Debug("[PatchUtils]: Running regex patch as specified lines, line by line");
                        int[] lines = new int[p.Lines.Count()];
                        for (int i = 0; i < p.Lines.Count(); i++)
                        {
                            lines[i] = int.Parse(p.Lines[i].Trim());
                        }
                        patchSuccess = RegxPatch(p, lines);
                    }
                    break;
                case "xml":
                    patchSuccess = XMLPatch(p);
                    break;
                case "json":
                    patchSuccess = JsonPatch(p);
                    break;
                case "xvm":
                    Logging.Error("[PatchUtils]: xvm patches are not supported, please use the json patch method");
                    patchSuccess = PatchExitCode.Error;
                    break;
            }
            Logging.Debug("[PatchUtils]: patch complete");

            //set the verbose setting back
            Logging.Debug("[PatchUtils]: temp logging setting={0}, ModpackSettings.VerboseLogging={1}, setting logging back to temp",
                tempVerboseLoggingSetting, ModpackSettings.VerboseLogging);
            ModpackSettings.VerboseLogging = tempVerboseLoggingSetting;
            return patchSuccess;
        }
        #endregion

        #region XML
        /// <summary>
        /// Run an XML patch operation
        /// </summary>
        /// <param name="p">The patch instructions object</param>
        /// <returns>The operation exit code</returns>
        private static PatchExitCode XMLPatch(Patch p)
        {
            //load the xml document
            XmlDocument doc = XmlUtils.LoadXmlDocument(p.CompletePath,XmlLoadType.FromFile);
            if (doc == null)
            {
                Logging.Error("[PatchUtils]: xml document from xml path is null");
                return PatchExitCode.Error;
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
                    Logging.Debug("[PatchUtils]: checking if xml element to add already exists, creating full xml path");

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
                    Logging.Debug("[PatchUtils]: full path to check if exists created as '{0}'", fullNodePath);
                    XmlNodeList fullPathNodeList = null;
                    try
                    {
                        fullPathNodeList = doc.SelectNodes(fullNodePath);
                    }
                    catch (System.Xml.XPath.XPathException)
                    {
                        Logging.Error("[PatchUtils]: invalid xpath: {0}", fullNodePath);
                        return PatchExitCode.Error;
                    }
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
                                Logging.Debug("[PatchUtils]: full path found entry with matching text, aborting (no need to patch)");
                                return PatchExitCode.Success;
                            }
                            else
                                Logging.Debug("[PatchUtils]: full path found entry, but text does not match. proceeding with add");
                        }
                    }
                    else
                        Logging.Debug("[PatchUtils]: full path entry not found, proceeding with add");

                    //get to the node where to add the element
                    XmlNode xmlPath = doc.SelectSingleNode(p.Path);
                    if(xmlPath == null)
                    {
                        Logging.Error("[PatchUtils]: patch xmlPath returns null!");
                        return PatchExitCode.Error;
                    }

                    //create node(s) to add to the element
                    Logging.Debug("[PatchUtils]: Total inner xml elements to make: {0}", replacePathSplit.Count()-1);
                    List<XmlElement> nodesListToAdd = new List<XmlElement>();
                    for (int i = 0; i < replacePathSplit.Count() - 1; i++)
                    {
                        //make the next element using the array replace name
                        XmlElement addElementToMake = doc.CreateElement(replacePathSplit[i]);
                        //the last one is the text to add
                        if (i == replacePathSplit.Count() - 2)
                        {
                            string textToAddIntoNode = replacePathSplit[replacePathSplit.Count() - 1];
                            textToAddIntoNode = Utils.MacroReplace(textToAddIntoNode, ReplacementTypes.PatchArguementsReplace);
                            Logging.Debug("[PatchUtils]: adding text: {0}", textToAddIntoNode);
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
                    Logging.Debug("[PatchUtils]: xml add complete");
                    break;

                case "edit":
                    //check to see if it's already there
                    Logging.Debug("[PatchUtils]: checking if element exists in all results");

                    XmlNodeList xpathResults = null;
                    try
                    {
                        xpathResults = doc.SelectNodes(p.Path);
                    }
                    catch (System.Xml.XPath.XPathException)
                    {
                        Logging.Error("[PatchUtils]: invalid xpath: {0}", p.Path);
                        return PatchExitCode.Error;
                    }

                    if (xpathResults.Count == 0)
                    {
                        Logging.Error("[PatchUtils]: xpath not found");
                        return PatchExitCode.Error;
                    }

                    //keep track if all xpath results equal this result
                    int matches = 0;
                    foreach (XmlElement match in xpathResults)
                    {
                        //matched, but trim and check if it matches the replace value
                        if (match.InnerText.Trim().Equals(p.Replace))
                        {
                            Logging.Debug("[PatchUtils]: found replace match for path search, incrementing match counter");
                            matches++;
                        }
                    }
                    if (matches == xpathResults.Count)
                    {
                        Logging.Info("[PatchUtils]: all {0} path results have values equal to replace, so can skip", matches);
                        return PatchExitCode.Success;
                    }
                    else
                        Logging.Info("[PatchUtils]: {0} of {1} path results match, so run patch", matches, xpathResults.Count);

                    //find and replace
                    foreach (XmlElement replaceMatch in xpathResults)
                    {
                        if (Regex.IsMatch(replaceMatch.InnerText, p.Search))
                        {
                            Logging.Debug("[PatchUtils]: found match, oldValue={0}, new value={1}", replaceMatch.InnerText, p.Replace);
                            replaceMatch.InnerText = p.Replace;
                        }
                        else
                        {
                            Logging.Warning("[PatchUtils]: Regex never matched for this xpath result: {0}",p.Path);
                        }
                    }
                    Logging.Debug("[PatchUtils]: xml edit complete");
                    break;

                case "remove":
                    //check to see if it's there
                    XmlNodeList xpathMatchesToRemove = null;
                    try
                    {
                        xpathMatchesToRemove = doc.SelectNodes(p.Path);
                    }
                    catch (System.Xml.XPath.XPathException)
                    {
                        Logging.Error("[PatchUtils]: invalid xpath: {0}", p.Path);
                        return PatchExitCode.Error;
                    }

                    foreach (XmlElement match in xpathMatchesToRemove)
                    {
                        if (Regex.IsMatch(match.InnerText.Trim(), p.Search))
                        {
                            match.RemoveAll();
                        }
                        else
                        {
                            Logging.Warning("[PatchUtils]: xpath match found, but regex search not matched");
                        }
                    }

                    //remove empty elements
                    Logging.Debug("[PatchUtils]: Removing any empty xml elements");
                    XDocument doc2 = XmlUtils.DocumentToXDocument(doc);
                    //note that XDocuemnt toString drops declaration
                    //https://stackoverflow.com/questions/1228976/xdocument-tostring-drops-xml-encoding-tag
                    doc2.Descendants().Where(e => string.IsNullOrEmpty(e.Value)).Remove();

                    //update doc with doc2
                    doc = XmlUtils.LoadXmlDocument(doc2.ToString(), XmlLoadType.FromString);
                    Logging.Debug("[PatchUtils]: xml remove complete");
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
            Logging.Debug("[PatchUtils]: hadHeader={0}, hasHeader={1}", hadHeader, hasHeader);
            if (!hadHeader && hasHeader)
            {
                Logging.Debug("[PatchUtils]: removing header");
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
                Logging.Debug("[PatchUtils]: adding header");
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
            Logging.Debug("[PatchUtils]: saving to disk");
            doc.Save(p.CompletePath);
            Logging.Debug("[PatchUtils]: xml patch completed successfully");
            return PatchExitCode.Success;
        }
        #endregion

        #region REGEX
        /// <summary>
        /// Run a regex patch operation
        /// </summary>
        /// <param name="p">The patch instructions object</param>
        /// <param name="lines">The lines to patch on the file.</param>
        /// <returns>The operation exit code</returns>
        /// <remarks>Can be used to "batch patch" an xml or json file. See Database examples.
        /// Use -1 to indicate the whole file is being patched. Use 0 to check every line.</remarks>
        private static PatchExitCode RegxPatch(Patch p, int[] lines)
        {
            //replace all "fake escape characters" with real escape characters
            p.Search = Utils.MacroReplace(p.Search, ReplacementTypes.TextUnescape);

            //legacy compatibility: if the replace text has "newline", then replace it with "\n" and log the warning
            if(p.Replace.Contains("newline"))
            {
                Logging.Warning("[PatchUtils]: This patch has the \"newline\" replace syntax and should be updated");
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
                            Logging.Debug("[PatchUtils]: line {0} matched ({1})", i + 1, fileParsed[i]);
                            fileParsed[i] = Regex.Replace(fileParsed[i], p.Search, p.Replace);
                            everReplaced = true;
                        }
                        //we split by \n so put it back in by \n
                        sb.Append(fileParsed[i] + "\n");
                    }
                    if (!everReplaced)
                    {
                        Logging.Warning("[PatchUtils]: Regex never matched");
                        return PatchExitCode.Warning;
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
                        Logging.Warning("[PatchUtils]: Regex never matched");
                        return PatchExitCode.Warning;
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
                                Logging.Debug("[PatchUtils]: line {0} matched ({1})", i + 1, fileParsed[i]);
                                fileParsed[i] = Regex.Replace(fileParsed[i], p.Search, p.Replace);
                                fileParsed[i] = Regex.Replace(fileParsed[i], "newline", "\n");
                                everReplaced = true;
                            }
                        }
                        sb.Append(fileParsed[i] + "\n");
                    }
                    if (!everReplaced)
                    {
                        Logging.Warning("[PatchUtils]: Regex never matched");
                        return PatchExitCode.Warning;
                    }
                }
            }
            catch (ArgumentException ex)
            {
                Logging.Error("[PatchUtils]: Invalid regex command");
                Logging.Debug(ex.ToString());
                return PatchExitCode.Error;
            }

            //save the file back into the string and then the file
            file = sb.ToString().Trim();
            File.WriteAllText(p.CompletePath, file);
            Logging.Debug("[PatchUtils]: regex patch completed successfully");
            return PatchExitCode.Success;
        }
        #endregion

        #region JSON
        /// <summary>
        /// Run a Json patch operation
        /// </summary>
        /// <param name="p">The patch instructions object</param>
        /// <returns>The operation exit code</returns>
        private static PatchExitCode JsonPatch(Patch p)
        {
            //apply and log legacy compatibilities
            //if no search parameter, set it to the regex default "match all" search option
            if(string.IsNullOrEmpty(p.Search))
            {
                Logging.Warning("[PatchUtils]: Patch should have search value specified, please update it!");
                p.Search = @".*";
            }

            //if no mode, treat as edit
            if(string.IsNullOrWhiteSpace(p.Mode))
            {
                Logging.Warning("[PatchUtils]: Patch should have mode value specified, please update it!");
                p.Mode = "edit";
            }

            //arrayEdit is not a valid mode, but may be specified by mistake
            if(p.Mode.Equals("arrayEdit"))
            {
                Logging.Warning("[PatchUtils]: Patch mode \"arrayEdit\" is not a valid mode and will be treated as \"edit\", please update it!");
                p.Mode = "edit";
            }

            //if patch type is v1, run v1 patch changes
            if(p.Version == 1)
            {
                //V1 fix 1: if type if json, hard-code the replace to look for xvm reference, and replace it with the new macro used (start macro)
                if(p.Replace.Contains("[dollar][lbracket][quote]"))
                {
                    Logging.Info("[PatchUtils]: Applying v1 fix 1: if json type, update xvm reference macros (start macro)");
                    p.Replace = p.Replace.Replace("[dollar][lbracket][quote]", "[xvm_dollar][lbracket][quote]");
                }
                //V1 fix 2: if type if json, hard-code the replace to look for xvm reference, and replace it with the new macro used (end macro)
                if (p.Replace.Contains("[quote][rbracket]"))
                {
                    Logging.Warning("[PatchUtils]: Applying v1 fix 2: if json type, update xvm reference macros (end macro)");
                    p.Replace = p.Replace.Replace("[quote][rbracket]", "[quote][xvm_rbracket]");
                }

                if(p.FollowPath)
                {
                    Logging.Error("[PatchUtils]: Patch format 1 is not compatible with FollowPath!");
                }
            }

            //load the file into a string
            string file = File.ReadAllText(p.CompletePath);

            //save it for later if followpath and from editor
            if (p.FromEditor && p.FollowPath && string.IsNullOrEmpty(p.FollowPathEditorCompletePath))
                p.FollowPathEditorCompletePath = p.CompletePath;

            //if the file is xc then check it for xvm style references (clean it up for the json parser)
            if (Path.GetExtension(p.CompletePath).ToLower().Equals(".xc") || p.FromEditor)
            {
                //and also check if the replace value is an xvm direct reference, we don't allow those (needs to be escaped)
                if (Regex.IsMatch(p.Replace, @"\$[ \t]*\{[ \t]*"""))
                {
                    Logging.Error("[PatchUtils]: patch replace value detected as xvm reference, but is not in escaped form! must be escaped!");
                    return PatchExitCode.Error;
                }

                //replace all xvm references with escaped versions that can be parsed
                file = EscapeXvmRefrences(file);
            }

            //escape the "$ref" meta-data header 
            file = file.Replace("\"$ref\"", "\"[dollar]ref\"");

            //now that the string file is ready, parse it into json.net
            //load the settings
            JsonLoadSettings settings = new JsonLoadSettings()
            {
                //ignore comments and load line info
                //"jsOn DoeSnT sUpPorT coMmAs"
                CommentHandling = CommentHandling.Ignore,
                LineInfoHandling = LineInfoHandling.Load
            };
            JObject root = null;

            //if it's from the editor, then dump the file to disk to show an escaped version for debugging
            if (Settings.ApplicationVersion == ApplicationVersions.Alpha && p.FromEditor)
            {
                string filenameForDump = Path.GetFileNameWithoutExtension(p.CompletePath) + "_escaped" + Path.GetExtension(p.CompletePath);
                string filePathForDump = Path.Combine(Path.GetDirectoryName(p.CompletePath), filenameForDump);
                Logging.Debug("[PatchUtils]: Dumping escaped file for debug before json.net parse: " + filePathForDump);
                File.WriteAllText(filePathForDump, file);
            }
            try
            {
                root = JObject.Parse(file, settings);
            }
            catch (JsonReaderException j)
            {
                Logging.Error("[PatchUtils]: Failed to parse json file! {0}", Path.GetFileName(p.File));
                Logging.Debug(j.ToString());
                return PatchExitCode.Error;
            }

            //if it is an xvm configuration file, or from editor, and we are wanting to followPath, then the root object then becomes the last item in the path
            //this works based on splitting up the path itself (forces dot convention) and going into files based on the reference
            //note that it will modify the patch path variable
            JObject searchRoot = root;
            PatchExitCodeForJson = PatchExitCode.Error;
            if ((Path.GetExtension(p.CompletePath).ToLower().Equals(".xc") || p.FromEditor) && p.FollowPath)
            {
                Logging.Debug("[PatchUtils]: Followpath is true, and either editor or xc file, following path to get actual root json object");
                searchRoot = FollowXvmPath(p, root);
            }

            if(searchRoot == null && p.FollowPath)
            {
                Logging.Debug("[PatchUtils]: Root Jobject is null, meaning followPath previously completed, so stop here");
                return PatchExitCodeForJson;
            }

            //switch how it is handled based on the mode of the patch
            switch (p.Mode.ToLower())
            {
                case "add":
                    PatchExitCodeForJson = JsonAdd(p, searchRoot);
                    break;
                case "edit":
                    PatchExitCodeForJson = JsonEditRemove(p, searchRoot, true);
                    break;
                case "remove":
                    PatchExitCodeForJson = JsonEditRemove(p, searchRoot, false);
                    break;
                case "arrayadd":
                    PatchExitCodeForJson = JsonArrayAdd(p, searchRoot);
                    break;
                case "arrayremove":
                    PatchExitCodeForJson = JsonArrayRemoveClear(p, searchRoot, true);
                    break;
                case "arrayclear":
                    PatchExitCodeForJson = JsonArrayRemoveClear(p, searchRoot, false);
                    break;
                default:
                    Logging.Error("[PatchUtils]: Unknown json patch mode, {0}", p.Mode);
                    return PatchExitCodeForJson;
            }

            //un-escape the string with all ref metadata and xvm references
            file = Utils.MacroReplace(root.ToString(), ReplacementTypes.PatchFiles);

            //always have a newline at the end
            file = file.Trim() + Environment.NewLine;

            //write to disk and finish
            if (p.FollowPath && p.FromEditor)
            {
                //if followpath and from editor, actually save it to testfile
                File.WriteAllText(p.FollowPathEditorCompletePath, file);
            }
            else
            {
                File.WriteAllText(p.CompletePath, file);
            }

            Logging.Debug("[PatchUtils]: json patch completed successfully");
            return PatchExitCodeForJson;
        }
        #endregion

        #region Json modes
        private static PatchExitCode JsonAdd(Patch p, JObject root)
        {
            //3 modes for json adding: regular add, add blank array, add blank object

            //match replace with [array] or [object] at the end, special case
            if(Regex.IsMatch(p.Replace, @"\[array\]$"))
            {
                Logging.Debug("[PatchUtils]: adding blank array detected");
                p.Replace = Regex.Replace(p.Replace, @"\[array\]$", string.Empty);
                return JsonAddBlank(p, root, false);
            }
            else if (Regex.IsMatch(p.Replace, @"\[object\]$"))
            {
                Logging.Debug("[PatchUtils]: adding blank object detected");
                p.Replace = Regex.Replace(p.Replace, @"\[object\]$", string.Empty);
                return JsonAddBlank(p, root, true);
            }

            //here means it's a standard json add
            //split the replace into array to make path for new object
            Logging.Debug("[PatchUtils]: adding standard value");
            List<string> addPathArray = null;
            addPathArray = p.Replace.Split('/').ToList();

            //check it has at least 2 values
            if(addPathArray.Count < 2)
            {
                Logging.Error("[PatchUtils]: add syntax or replace value must have at least 2 values separated by \"/\" in its path");
                return PatchExitCode.Error;
            }

            //last item in array is item to add
            string valueToAdd = addPathArray[addPathArray.Count - 1];
            valueToAdd = Utils.MacroReplace(valueToAdd, ReplacementTypes.PatchArguementsReplace);

            //then remove it
            addPathArray.RemoveAt(addPathArray.Count - 1);

            //same idea for the property name
            string propertyName = addPathArray[addPathArray.Count - 1];
            addPathArray.RemoveAt(addPathArray.Count - 1);

            //now form the full path (including any extra object paths used in the replace syntax)
            string fullPath = p.Path;
            if(addPathArray.Count > 0)
            {
                foreach(string s in addPathArray)
                {
                    fullPath = fullPath + "." + s;
                }
            }

            //get the root object from the original jsonPath
            JContainer objectRoot = null;
            try
            {
                objectRoot = (JContainer)root.SelectToken(p.Path);
            }
            catch (Exception exVal)
            {
                Logging.Error("[PatchUtils]: error in jsonPath syntax: {0}", p.Path);
                Logging.Debug(exVal.ToString());
                return PatchExitCode.Error;
            }
            if(objectRoot == null)
            {
                Logging.Error("[PatchUtils]: jsonPath does not exist: {0}", p.Path);
                return PatchExitCode.Error;
            }
            if(!(objectRoot is JObject))
            {
                Logging.Error("[PatchUtils]: expected JObject, got {0}", objectRoot.Type.ToString());
                return PatchExitCode.Error;
            }

            //foreach string still in the addPath array, go into the inner object
            JObject previous = (JObject)objectRoot;
            foreach(string s in addPathArray)
            {
                Logging.Debug("[PatchUtils]: creating object for path: {0}", s);
                JContainer innerSearch = (JContainer)previous.SelectToken(s);

                //if it's null, then it does not exist, so make it
                if(innerSearch == null)
                {
                    //make a new property with key of name and value of object
                    JObject nextObject = new JObject();
                    JProperty nextProperty = new JProperty(s, nextObject);
                    previous.Add(nextProperty);
                    previous = nextObject;
                }
                else if (innerSearch is JObject innerObject)
                {
                    Logging.Debug("[PatchUtils]: following add path, found JObject exists from the add replace path: {0}",s);
                    previous = innerObject;
                }
                else
                {
                    Logging.Error("[PatchUtils]: following add path, expected JObject or null, got {0}", innerSearch.Type.ToString());
                    return PatchExitCode.Error;
                }
            }

            //search for if the key/value already exists
            Logging.Debug("[PatchUtils]: checking if value exists and needs to be replaced");
            objectRoot = previous;
            JToken resultValue = objectRoot.SelectToken(propertyName);
            //result is jvalue, parent is property
            if (resultValue is JValue jvalue)
            {
                Logging.Debug("[PatchUtils]: found result already exists, checking if value is direct equals");
                if (valueToAdd.Equals(JsonGetCompare(jvalue)))
                {
                    Logging.Debug("[PatchUtils]: value already matches, no need to replace");
                    return PatchExitCode.Success;
                }
                else
                {
                    Logging.Debug("[PatchUtils]: value does not match, replacing");
                    UpdateJsonValue(jvalue, valueToAdd);
                }
            }
            else
            {
                //add the property to the object
                Logging.Debug("[PatchUtils]: key-value pair does not exist, adding");
                JProperty prop = CreateJsonProperty(propertyName, valueToAdd);
                objectRoot.Add(prop);
            }
            return PatchExitCode.Success;
        }

        private static PatchExitCode JsonAddBlank(Patch p, JObject root, bool jObject)
        {
            //replace field has now already been parsed
            //see if the object already exists in the full form (so include replace being the new object/array name)
            JContainer result = null;
            try
            {
                result = (JContainer)root.SelectToken(p.Path + "." + p.Replace);
            }
            catch (Exception array)
            {
                Logging.Error("[PatchUtils]: error in replace syntax: {0}\n{1}", p.Replace, array.ToString());
                return PatchExitCode.Error;
            }
            if (result != null)
            {
                Logging.Error("[PatchUtils]: cannot add blank array or object when already exists");
                return PatchExitCode.Error;
            }

            //here means the object/array does not exist, and can be added
            //get the container for adding the new blank object/array to
            JContainer pathForArray = (JContainer)root.SelectToken(p.Path);

            //make object reference and make it array or object
            JContainer newObject = null;
            if (jObject)
                newObject = new JObject();
            else
                newObject = new JArray();

            //make the property to hold the new object/array, key-value style
            JProperty prop = new JProperty(p.Replace, newObject);

            //add it to the container
            pathForArray.Add(prop);
            return PatchExitCode.Success;
        }

        private static PatchExitCode JsonEditRemove(Patch p, JObject root, bool edit)
        {
            //get the list of all items that match the path
            IEnumerable<JToken> jsonPathresults = null;
            try
            {
                jsonPathresults = root.SelectTokens(p.Path);
            }
            catch (Exception exResults)
            {
                Logging.Error("[PatchUtils]: Error with jsonPath: {0}", p.Path);
                Logging.Error(exResults.ToString());
            }
            if (jsonPathresults == null || jsonPathresults.Count() == 0)
            {
                Logging.Warning("[PatchUtils]: no results from jsonPath search");
                return PatchExitCode.Warning;
            }

            //make sure results are all JValue
            List<JValue> Jresults = new List<JValue>();
            foreach (JToken jt in jsonPathresults)
            {
                if (jt is JValue Jvalue)
                {
                    Jresults.Add(Jvalue);
                }
                else
                {
                    Logging.Error("[PatchUtils]: Expected results of type JValue, returned {0}", jt.Type.ToString());
                    return PatchExitCode.Error;
                }
            }

            //check that we have results
            Logging.Debug("[PatchUtils]: number of Jvalues: {0}", Jresults.Count);
            if (Jresults.Count == 0)
            {
                Logging.Warning("[PatchUtils]: Jresults count is 0 (is this the intent?)");
                return PatchExitCode.Warning;
            }

            //foreach match from json search, match the result with search parameter
            foreach(JValue result in Jresults)
            {
                //parse the value to a string for comparison
                string jsonValue = JsonGetCompare(result);

                try
                {
                    //only update the value if the regex search matches
                    if (Regex.IsMatch(jsonValue, p.Search))
                    {
                        if (edit)
                        {
                            Logging.Debug("[PatchUtils]: regex match for result {0}, applying edit to {1}", jsonValue, p.Replace);
                            UpdateJsonValue(result, p.Replace);
                        }
                        else
                        {
                            Logging.Debug("[PatchUtils]: regex match for result {0}, removing", jsonValue);
                            //check if parent is array, we should not be removing from an array in this function
                            if (result.Parent is JArray)
                            {
                                Logging.Error("[PatchUtils]: Selected from p.path is JValue and parent is JArray. Use arrayRemove for this function");
                                return PatchExitCode.Error;
                            }
                            //get the jProperty above it and remove itself
                            else if (result.Parent is JProperty prop)
                            {
                                prop.Remove();
                            }
                            else
                            {
                                Logging.Error("[PatchUtils]: unknown parent type: {0}", result.Parent.GetType().ToString());
                                return PatchExitCode.Error;
                            }
                        }
                    }
                    else
                    {
                        Logging.Debug("[PatchUtils]: json value {0} matches jsonPath but does not match regex search {1}", jsonValue, p.Search);
                    }
                }
                catch (ArgumentException argEx)
                {
                    Logging.Error("[PatchUtils]: Invalid Regex search command");
                    Logging.Error(argEx.Message);
                    return PatchExitCode.Error;
                }
            }
            return PatchExitCode.Success;
        }

        private static PatchExitCode JsonArrayAdd(Patch p, JObject root)
        {
            //check syntax of what was added
            List<string> addPathArray = null;
            addPathArray = p.Replace.Split('/').ToList();

            //maximum number of args for replace is 2
            if(addPathArray.Count > 2)
            {
                Logging.Error("[PatchUtils]: invalid replace syntax: maximum arguments is 2. given: {0}", addPathArray.Count);
                return PatchExitCode.Error;
            }

            //get the property name (if exists, and value, and index to add to)
            string propertyName = addPathArray.Count == 2 ? addPathArray[0] : string.Empty;
            string valueToAdd = addPathArray.Count == 2? addPathArray[1] : addPathArray[0];

            //check for index value in p.replace (name/value[index=NUMBER])
            string indexString = valueToAdd.Split(new string[] { @"[index=" }, StringSplitOptions.None)[1];

            //split off the end brace, default is index 0 (add it to array at bottom if none provided)
            int index = Utils.ParseInt(indexString.Split(']')[0], -1);

            //and get it out of the valueToAdd
            valueToAdd = valueToAdd.Split(new string[] { @"[index=" }, StringSplitOptions.None)[0];

            //and run the result through the un-escape
            valueToAdd = Utils.MacroReplace(valueToAdd, ReplacementTypes.PatchArguementsReplace);

            JArray array = JsonArrayGet(p, root);
            if (array == null)
            {
                Logging.Error("[PatchUtils]: JArray is null");
                return PatchExitCode.Error;
            }

            //if index value is greater then count, then warning and set it to -1 (tells it to add to bottom of array)
            if(index >= array.Count)
            {
                if (index != 0)
                    Logging.Warning("[PatchUtils]: index value ({0})>= array count ({1}), putting at end of the array (is this the intent?)", index, array.Count);
                index = -1;
            }

            //check that the correct type of array was found for expected add (key-value to value array, vise versa)
            if (array.Count > 0)
            {
                if ((array[0] is JValue) && (addPathArray.Count() == 2))
                {
                    Logging.WriteToLog("array is of JValues and 2 replace arguments given", Logfiles.Application, LogLevel.Error);
                    return PatchExitCode.Error;
                }
                else if (!(array[0] is JValue) && (addPathArray.Count() == 1))
                {
                    Logging.WriteToLog("array is not of JValues and only 1 replace arguments given", Logfiles.Application, LogLevel.Error);
                    return PatchExitCode.Error;
                }
            }

            //add the value/key-value pair to the array at the specified index
            JValue val = CreateJsonValue(valueToAdd);
            if (addPathArray.Count() == 2)
            {
                //add object with property
                if (index == -1)
                {
                    array.Add(new JObject(new JProperty(propertyName, val)));
                }
                else
                {
                    array.Insert(index, (new JObject(new JProperty(propertyName, val))));
                }
            }
            else
            {
                //add value
                if (index == -1)
                {
                    array.Add(val);
                }
                else
                {
                    array.Insert(index, val);
                }
            }
            return PatchExitCode.Success;
        }

        private static PatchExitCode JsonArrayRemoveClear(Patch p, JObject root, bool remove)
        {
            JArray array = JsonArrayGet(p, root);
            if (array == null)
            {
                Logging.Error("[PatchUtils]: JArray is null");
                return PatchExitCode.Error;
            }

            //can't remove from an array if it's empty #rollSafe
            if (array.Count == 0)
            {
                Logging.Error("[PatchUtils]: array is already empty");
                return PatchExitCode.Warning;
            }

            //search and remove each item that matches. if it's remove mode, then stop at the first one
            bool found = false;
            for (int i = 0; i < array.Count; i++)
            {
                //can return jvalue or jobject
                string jsonResult = string.Empty;
                if (array[i] is JValue jvalue)
                    jsonResult = JsonGetCompare(array[i] as JValue);
                else //assuming jobject
                    jsonResult = array[i].ToString();

                try
                {
                    if (Regex.IsMatch(jsonResult, p.Search))
                    {
                        found = true;
                        array[i].Remove();
                        i--;
                        if (remove)
                            break;
                    }
                }
                catch (ArgumentException argEx)
                {
                    Logging.Error("[PatchUtils]: Invalid Regex search command");
                    Logging.Error(argEx.Message);
                    return PatchExitCode.Error;
                }
            }
            if (!found)
            {
                Logging.Warning("[PatchUtils]: no results found for search \"{0}\", with path \"{1}\"", p.Search, p.Path);
                return PatchExitCode.Warning;
            }
            return PatchExitCode.Success;
        }
        #endregion

        #region Helpers

        private static JObject FollowXvmPath(Patch p, JObject root)
        {
            //split the path into dots. each option therefore is a small path that it will be searching into
            Logging.Debug("[PatchUtils]: followPath start");
            List<string> pathArray = p.Path.Split('.').ToList();
            if(pathArray.Count == 1)
            {
                Logging.Info("[PatchUtils]: pathArray count is 1, no need to follow path");
                return root;
            }

            //go into each path from the array
            //also subtract from the pathArray as we go to put it back into the path
            JObject latest = root;
            while(pathArray.Count != 0)
            {
                string miniPath = pathArray[0];
                Logging.Debug("[PatchUtils]: searching from minipath: {0}", miniPath);
                JToken pathSearchResult = null;
                try
                {
                    pathSearchResult = latest.SelectToken(miniPath);
                }
                catch (JsonException tokenSearchException)
                {
                    Logging.Error("[PatchUtils]: error with minipath, stopping at previous");
                    Logging.Error(tokenSearchException.ToString());
                    break;
                }
                if(pathSearchResult == null)
                {
                    Logging.Error("[PatchUtils]: minipath search result is null, error with the given path?");
                    break;
                }
                if(pathSearchResult is JObject jobject)
                {
                    //if it's a jboject, then search inside with saving
                    Logging.Debug("[PatchUtils]: miniPath resulted in JObject, continue");
                    latest = jobject;
                }
                else if (pathSearchResult is JArray jarray)
                {
                    //if the result is an array then it's an arrayEdit. return the latest object and path. an array in an array has yet to be seen
                    Logging.Debug("[PatchUtils]: miniPath resulted in Jarray, stop and return latest object");
                    break;
                }
                else if (pathSearchResult is JValue jvalue)
                {
                    Logging.Debug("[PatchUtils]: miniPath resulted in jValue, checking if string for xvm reference");
                    if(jvalue.Value is string value)
                    {
                        Logging.Debug("[PatchUtils]: jValue is string, checking for xvm reference");
                        if(value.Contains(@"[xvm_dollar]"))
                        {
                            Logging.Debug("[PatchUtils]: xvm reference detected, checking if reference is target and not add");
                            if(pathArray.Count == 1 && !p.Mode.ToLower().Equals("add"))
                            {
                                Logging.Debug("[PatchUtils]: this reference is the target, so don't enter it. return latest object");
                                break;
                            }
                            Logging.Debug("[PatchUtils]: reference is not target");

                            //parse the first part of the reference, could be direct reference path inside file or filename
                            string fileOrReference = value.Split(new string[] { @"[xvm_dollar][lbracket][quote]"}, StringSplitOptions.RemoveEmptyEntries)[0].Split('[')[0];

                            //parse the second part of the reference, could be the new path for new file, or blank (if direct path inside current file)
                            string reference = value.Split(new string[] { @"[quote][colon][quote]" }, StringSplitOptions.RemoveEmptyEntries)[1].Split('[')[0];
                            if(string.IsNullOrEmpty(reference))
                            {
                                Logging.Debug("[PatchUtils]: reference is internal to file in new path");
                                latest = root;
                                pathArray[0] = fileOrReference;
                                continue;
                            }
                            else
                            {
                                Logging.Debug("[PatchUtils]: reference is external to new file");
                                //load and parse the new file
                                string folderPath = Path.GetDirectoryName(p.CompletePath);
                                string completePathNewFile = Path.Combine(folderPath, fileOrReference);

                                //check if file exists and load it
                                if(!File.Exists(completePathNewFile))
                                {
                                    Logging.Error("[PatchUtils]: following path resulted in \"{0}\", but does not exist!");
                                    return null;
                                }

                                //recursively enter the new patch file
                                p.CompletePath = completePathNewFile;
                                pathArray.RemoveAt(0);
                                p.Path = string.Format("$.{0}.{1}", reference ,string.Join(".", pathArray));
                                Logging.Debug("[PatchUtils]: starting new recursive patch run");
                                RunPatch(p);
                                return null;
                            }
                        }
                        else
                        {
                            Logging.Debug("[PatchUtils]: no reference detected");
                            break;
                        }
                    }
                    else
                    {
                        Logging.Debug("[PatchUtils]: jValue is not string, actual value, stop at previous");
                        break;
                    }
                }

                //remove it
                pathArray.RemoveAt(0);
            }

            //update the path for what was left
            p.Path = string.Join(".", pathArray);
            return latest;
        }

        private static string JsonGetCompare(JValue result)
        {
            //parse the value to a string for comparison
            string jsonValue = string.Empty;
            if (result.Value is string str)
                jsonValue = str;
            else if (result.Value is char c)
                jsonValue = c.ToString();
            else if (result.Value is bool b)
                jsonValue = b.ToString().ToLower();
            else if (result.Value == null)
                jsonValue = Utils.PatchJsonNullEscape;
            else
                jsonValue = result.Value.ToString();
            return jsonValue;
        }

        private static void UpdateJsonValue(JValue jvalue, string value)
        {
            //determine what type value should be used for the json item based on attempted parsing
            if (value.Equals(Utils.PatchJsonNullEscape))
                jvalue.Value = null;
            else if (Utils.ParseBool(value, out bool resultBool))
                jvalue.Value = resultBool;
            else if (Utils.ParseInt(value, out int resultInt))
                jvalue.Value = resultInt;
            else if (Utils.ParseFloat(value, out float resultFloat))
                jvalue.Value = resultFloat;
            else
                jvalue.Value = value;
            Logging.Debug("[PatchUtils]: Json value parsed as data type {0}", jvalue.Value == null? Utils.PatchJsonNullEscape : jvalue.Value.GetType().ToString());
        }

        private static JValue CreateJsonValue(string value)
        {
            //determine what type value should be used for the json item based on attempted parsing
            JValue jvalue = null;
            if (Utils.ParseBool(value, out bool resultBool))
                jvalue = new JValue(resultBool);
            else if (Utils.ParseInt(value, out int resultInt))
                jvalue = new JValue(resultInt);
            else if (Utils.ParseFloat(value, out float resultFloat))
                jvalue = new JValue(resultFloat);
            else
                jvalue = new JValue(value);
            Logging.Debug("[PatchUtils]: Json value parsed as {0}", jvalue.Value.GetType().ToString());
            return jvalue;
        }

        private static JProperty CreateJsonProperty(string propertyName, string value)
        {
            JValue jvalue = CreateJsonValue(value);
            return new JProperty(propertyName, jvalue);
        }

        private static JArray JsonArrayGet(Patch p, JObject root)
        {
            //get the root object from the original jsonPath
            JContainer objectRoot = null;
            try
            {
                objectRoot = (JContainer)root.SelectToken(p.Path);
            }
            catch (Exception exVal)
            {
                Logging.Error("[PatchUtils]: error in jsonPath syntax: {0}", p.Path);
                Logging.Debug(exVal.ToString());
            }
            if (objectRoot == null)
            {
                Logging.Error("[PatchUtils]: path does not exist: {0}", p.Path);
            }
            return objectRoot as JArray;
        }

        private static string EscapeXvmRefrences(string file)
        {
            //replace all xvm style references with escaped versions that won't cause invalid parsing
            //split regex based on the start of the xvm reference (the dollar, whitespace, left bracket, whitespace, quote)
            //note it will need to be put back in
            string[] fileSplit = Regex.Split(file, @"\$[ \t]*\{[ \t]*""");
            for (int i = 1; i < fileSplit.Length; i++)
            {
                fileSplit[i] = @"""[xvm_dollar][lbracket][quote]" + fileSplit[i];
                //looks like:      "[xvm_dollar][lbracket][quote]damageLog.log.x" }
                //"battleLoading": "[xvm_dollar][lbracket][quote]battleLoading.xc":"battleLoading"},

                //split it again so we don't replace more than we need to
                //stop at the next right bracket to indicate the end of the xvm reference
                //the escaped string can always be treated as a value in the key-value pair system
                string[] splitAgain = fileSplit[i].Split('}');

                //check for if it is a file:path reference and escape it
                if (Regex.IsMatch(splitAgain[0], @"""[\t ]*\:[\t ]*"""))
                    splitAgain[0] = Regex.Replace(splitAgain[0], @"""[\t ]*\:[\t ]*""", @"[quote][colon][quote]");
                //if that style, would look like "battleLoading": "[xvm_dollar][lbracket][quote]battleLoading.xc[quote][colon][quote]battleLoading"},

                //join it back to fileSplit
                fileSplit[i] = string.Join("}", splitAgain);

                //match the first occurrence only of the end of the reference ("})
                Match m = Regex.Match(fileSplit[i], @"""[\t ]*\}");
                if (m.Success)
                {
                    //create the string of everything before the match (up to and not include the quote)
                    string before = fileSplit[i].Substring(0, m.Index);

                    //create the string of everything after the match (after and not include the right bracket)
                    string after = fileSplit[i].Substring(m.Index + m.Length);

                    //make the string with the escape for the end of the xvm reference
                    fileSplit[i] = before + @"[quote][xvm_rbracket]""" + after;
                    //finished result: "[xvm_dollar][lbracket][quote]damageLog.log.x[quote][xvm_rbracket]"
                    //"battleLoading": "[xvm_dollar][lbracket][quote]battleLoading.xc[quote][colon][quote]battleLoading[quote][xvm_rbracket]",
                }
            }
            file = string.Join("", fileSplit);
            return file;
        }

        /// <summary>
        /// Gets the absolute path to the Xvm configuration folder
        /// </summary>
        /// <returns>The absolute path of the Xvm configuration folder if it exists, otherwise returns "default"</returns>
        public static string GetXvmFolderName()
        {
            //form where it should be
            string xvmBootFile = Path.Combine(Settings.WoTDirectory, "res_mods\\configs\\xvm\\xvm.xc");

            //check if it exists there
            if (!File.Exists(xvmBootFile))
            {
                Logging.Warning("[PatchUtils]: extractor asked to get location of xvm folder name, but boot file does not exist! returning \"default\"");
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
