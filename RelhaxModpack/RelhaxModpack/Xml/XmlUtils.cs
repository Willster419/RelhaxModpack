using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Windows;
using System.Reflection;
using System.Text;
using Ionic.Zip;
using RelhaxModpack.Xml;
using RelhaxModpack.Database;
using System.Net;
using System.Threading.Tasks;
using RelhaxModpack.Utilities;
using RelhaxModpack.Atlases;
using RelhaxModpack.Patches;
using RelhaxModpack.Shortcuts;
using RelhaxModpack.Utilities.Enums;

namespace RelhaxModpack.Xml
{
    /// <summary>
    /// Utility class for dealing with Xml features and functions
    /// </summary>
    public static class XmlUtils
    {
        #region Xml Validating
        /// <summary>
        /// Check to make sure an Xml file is valid
        /// </summary>
        /// <param name="filePath">The path to the Xml file</param>
        /// <returns>True if valid Xml, false otherwise</returns>
        public static bool IsValidXml(string filePath)
        {
            return IsValidXml(File.ReadAllText(filePath), Path.GetFileName(filePath));
        }

        /// <summary>
        /// Check to make sure an Xml file is valid
        /// </summary>
        /// <param name="xmlString">The Xml text string</param>
        /// <param name="fileName">the name of the file, used for debugging purposes</param>
        /// <returns>True if valid Xml, false otherwise</returns>
        public static bool IsValidXml(string xmlString, string fileName)
        {
            XmlTextReader read = new XmlTextReader(xmlString);
            try
            {
                //continue to read the entire document
                while (read.Read()) ;
                read.Close();
                read.Dispose();
                return true;
            }
            catch (Exception e)
            {
                Logging.Error("Invalid Xml file: {0}\n{1}",fileName,e.Message);
                read.Close();
                read.Dispose();
                return false;
            }
        }
        #endregion

        #region Xpath stuff
        /// <summary>
        /// Get an Xml element attribute given an Xml path
        /// </summary>
        /// <param name="file">The path to the Xml file</param>
        /// <param name="xpath">The xpath search string</param>
        /// <returns>The value from the xpath search, otherwise null</returns>
        public static string GetXmlStringFromXPath(string file, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read Xml file {0}\n{1}", Path.GetFileName(file),e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXmlStringFromXPath(doc, xpath);
        }

        /// <summary>
        /// Get an Xml element attribute given an Xml path
        /// </summary>
        /// <param name="xmlString">The Xml text string</param>
        /// <param name="xpath">The xpath search string</param>
        /// <param name="filename"></param>
        /// <returns>The value from the xpath search, otherwise null</returns>
        public static string GetXmlStringFromXPath(string xmlString, string xpath, string filename)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlString);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read Xml file {0}\n{1}", Path.GetFileName(filename), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXmlStringFromXPath(doc, xpath);
        }
        
        /// <summary>
        /// Get an Xml element attribute given an Xml path
        /// </summary>
        /// <param name="doc">The Xml document object to check</param>
        /// <param name="xpath">The xpath search string</param>
        /// <returns>The value from the xpath search, otherwise null</returns>
        /// <remarks>
        /// The following are Xml attribute examples
        /// element example: "//root/element"
        /// attribute example: "//root/element/@attribute"
        /// for the onlineFolder version: //modInfoAlpha.xml/@onlineFolder
        /// for the folder version: //modInfoAlpha.xml/@version
        /// </remarks>
        public static string GetXmlStringFromXPath(XmlDocument doc, string xpath)
        {
            //set to something dumb for temporary purposes
            XmlNode result;
            try
            {
                result = doc.SelectSingleNode(xpath);
            }
            catch
            {
                return null;
            }
            if (result == null)
                return null;
            return result.InnerText;
        }

        /// <summary>
        /// Get an Xml node value given an Xml path
        /// </summary>
        /// <param name="file">The path to the Xml file</param>
        /// <param name="xpath">The xpath search string</param>
        /// <returns>The Xml node object of the search result, or null</returns>
        public static XmlNode GetXmlNodeFromXPath(string file, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read Xml file {0}\n{1}", Path.GetFileName(file), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXmlNodeFromXPath(doc, xpath);
        }

        /// <summary>
        /// Get an Xml node value given an Xml path
        /// </summary>
        /// <param name="xmlString">The Xml string to parse</param>
        /// <param name="xpath">The xpath search string</param>
        /// <param name="filename">The name of the file, used for logging purposes</param>
        /// <returns>The Xml node object of the search result, or null</returns>
        public static XmlNode GetXmlNodeFromXPath(string xmlString, string xpath, string filename)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlString);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read Xml file {0}\n{1}", Path.GetFileName(filename), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXmlNodeFromXPath(doc, xpath);
        }

        /// <summary>
        /// Get an Xml node value given an Xml path
        /// </summary>
        /// <param name="doc">The XmlDocument object to search</param>
        /// <param name="xpath">The xpath string</param>
        /// <returns>The Xml node object of the search result, or null</returns>
        public static XmlNode GetXmlNodeFromXPath(XmlDocument doc, string xpath)
        {
          XmlNode result = doc.SelectSingleNode(xpath);
          if (result == null)
              return null;
          return result;
        }

        /// <summary>
        /// Get a List of Xml nodes that match given an Xml path
        /// </summary>
        /// <param name="file">The path to the Xml file</param>
        /// <param name="xpath">The xpath string</param>
        /// <returns>The node list of matching results, or null</returns>
        public static XmlNodeList GetXmlNodesFromXPath(string file, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read Xml file {0}\n{1}", Path.GetFileName(file), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXmlNodesFromXPath(doc, xpath);
        }

        /// <summary>
        /// Get a List of Xml nodes that match given an Xml path
        /// </summary>
        /// <param name="xmlString">The xml document in a string</param>
        /// <param name="filename">The name of the document for logging purposes</param>
        /// <param name="xpath">The xpath string</param>
        /// <returns>The node list of matching results, or null</returns>
        public static XmlNodeList GetXmlNodesFromXPath(string xmlString, string xpath, string filename)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlString);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read Xml file {0}\n{1}", Path.GetFileName(filename), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXmlNodesFromXPath(doc, xpath);
        }


        /// <summary>
        /// Get a List of Xml nodes that match given an Xml path
        /// </summary>
        /// <param name="doc">The XmlDocument to search</param>
        /// <param name="xpath">The xml path string</param>
        /// <returns>The node list of matching results, or null</returns>
        /// <remarks>XmlElement and XmlAttribute inherit from XmlNode</remarks> 
        public static XmlNodeList GetXmlNodesFromXPath(XmlDocument doc, string xpath)
        {
            XmlNodeList results = doc.SelectNodes(xpath);
          if (results == null || results.Count == 0)
          {
            return null;
          }
          return results;
        }
        #endregion

        #region Other Xml stuffs
        /// <summary>
        /// Convert an XmlDocument to an XDocument
        /// </summary>
        /// <param name="doc">The XmlDocument to convert</param>
        /// <returns>The converted XDocument</returns>
        /// <remarks>See https://blogs.msdn.microsoft.com/xmlteam/2009/03/31/converting-from-xmldocument-to-xdocument/ </remarks>
        public static XDocument DocumentToXDocument(XmlDocument doc)
        {
            if (doc == null)
                return null;
            return XDocument.Parse(doc.OuterXml,LoadOptions.SetLineInfo);
        }

        /// <summary>
        /// Load an Xml document to an XDocument object
        /// </summary>
        /// <param name="fileOrXml">The filepath or string representation of the Xml document</param>
        /// <param name="type">The type to define if fileOrXml is a file path or the Xml string</param>
        /// <returns>A parsed XDocument of the Xml document</returns>
        public static XDocument LoadXDocument(string fileOrXml, XmlLoadType type)
        {
            if(type == XmlLoadType.FromFile && !File.Exists(fileOrXml))
            {
                Logging.Error("XmlLoadType set to file and file does not exist at {0}", fileOrXml);
                return null;
            }
            try
            {
                switch(type)
                {
                    case XmlLoadType.FromFile:
                        return XDocument.Load(fileOrXml, LoadOptions.SetLineInfo);
                    case XmlLoadType.FromString:
                        return XDocument.Parse(fileOrXml, LoadOptions.SetLineInfo);
                }
            }
            catch(XmlException ex)
            {
                Logging.Exception(ex.ToString());
                return null;
            }
            return null;
        }

        /// <summary>
        /// Load an Xml document to an XmlDocument object
        /// </summary>
        /// <param name="fileOrXml">The filepath or string representation of the Xml document</param>
        /// <param name="type">The type to define if fileOrXml is a file path or the Xml string</param>
        /// <returns>A parsed XmlDocument of the Xml document</returns>
        public static XmlDocument LoadXmlDocument(string fileOrXml, XmlLoadType type)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                switch(type)
                {
                    case XmlLoadType.FromFile:
                        doc.Load(fileOrXml);
                        break;
                    case XmlLoadType.FromString:
                        doc.LoadXml(fileOrXml);
                        break;
                }
            }
            catch (XmlException xmlEx)
            {
                if (File.Exists(fileOrXml))
                    Logging.Exception("Failed to load Xml file: {0}\n{1}", Path.GetFileName(fileOrXml), xmlEx.ToString());
                else
                    Logging.Exception("failed to load Xml string:\n" + xmlEx.ToString());
                return null;
            }
            return doc;
        }

        /// <summary>
        /// Copies an xml file from an archive or directory path and unpacks it from binary Xml to human-readable Xml
        /// </summary>
        /// <param name="xmlUnpack">The Xml unpack instructions object</param>
        /// <param name="unpackBuilder">The stringBuilder to log the generated files location for the install log</param>
        public static bool UnpackXmlFile(XmlUnpack xmlUnpack, StringBuilder unpackBuilder)
        {
            //log info for debugging if need be
            Logging.Info(xmlUnpack.DumpInfoToLog);

            //check if new destination name for replacing
            string destinationFilename = string.IsNullOrWhiteSpace(xmlUnpack.NewFileName) ? xmlUnpack.FileName : xmlUnpack.NewFileName;
            string destinationCompletePath = Path.Combine(xmlUnpack.ExtractDirectory, destinationFilename);
            string sourceCompletePath = Path.Combine(xmlUnpack.DirectoryInArchive, xmlUnpack.FileName);

            //if the destination file already exists, then don't copy it over
            if(File.Exists(destinationCompletePath))
            {
                Logging.Info("Replacement file already exists, skipping");
                return true;
            }

            FileUtils.Unpack(xmlUnpack.Pkg, sourceCompletePath, destinationCompletePath);
            unpackBuilder.AppendLine(destinationCompletePath);

            Logging.Info("unpacking Xml binary file (if binary)");
            try
            {
                XmlBinaryHandler binaryHandler = new XmlBinaryHandler();
                binaryHandler.UnpackXmlFile(destinationCompletePath);
                return true;
            }
            catch (Exception xmlUnpackExceptino)
            {
                Logging.Exception(xmlUnpackExceptino.ToString());
                return false;
            }
        }
        #endregion

        #region Component Parsing methods
        /// <summary>
        /// Parse a list of patch instructions from an Xml file into patch objects
        /// </summary>
        /// <param name="patches">The list of patches to parse into</param>
        /// <param name="filename">The name of the file to parse from</param>
        /// <param name="originalNameFromZip">The original name when extracted from the zip file during install time</param>
        public static void AddPatchesFromFile(List<Patch> patches, string filename, string originalNameFromZip = null)
        {
            //make an Xml document to get all patches
            XmlDocument doc = LoadXmlDocument(filename, XmlLoadType.FromFile);
            if (doc == null)
                return;
            //make new patch object for each entry
            //remember to add lots of logging
            XmlNodeList XMLpatches = GetXmlNodesFromXPath(doc, "//patchs/patch");
            if (XMLpatches == null || XMLpatches.Count == 0)
            {
                Logging.Error("File {0} contains no patch entries", filename);
                return;
            }
            Logging.Info("Adding {0} patches from patchFile: {1}", XMLpatches.Count, filename);
            foreach (XmlNode patchNode in XMLpatches)
            {
                Patch p = new Patch
                {
                    NativeProcessingFile = Path.GetFileName(filename),
                    ActualPatchName = originalNameFromZip
                };
                Logging.Debug("Adding patch from file: {0} -> original name: {1}", Path.GetFileName(filename), originalNameFromZip);
                
                //we have the patchNode "patch" object, now we need to get it's children to actually get the properties of said patch
                foreach (XmlNode property in patchNode.ChildNodes)
                {
                    //each element in the Xml gets put into the
                    //the corresponding attribute for the Patch instance
                    switch (property.Name)
                    {
                        case "type":
                            p.Type = property.InnerText.Trim();
                            break;
                        case "mode":
                            p.Mode = property.InnerText.Trim();
                            break;
                        case "patchPath":
                            p.PatchPath = property.InnerText.Trim();
                            break;
                        case "followPath":
                            p.FollowPath = CommonUtils.ParseBool(property.InnerText.Trim(), false);
                            break;
                        case "file":
                            p.File = property.InnerText.Trim();
                            break;
                        case "path":
                            p.Path = property.InnerText.Trim();
                            break;
                        case "version":
                            p.Version = CommonUtils.ParseInt(property.InnerText.Trim(), 1);
                            break;
                        case "line":
                            if (!string.IsNullOrWhiteSpace(property.InnerText.Trim()))
                                p.Lines = property.InnerText.Trim().Split(',');
                            break;
                        case "search":
                            p.Search = property.InnerText.Trim();
                            break;
                        case "replace":
                            p.Replace = property.InnerText.Trim();
                            break;
                    }
                }
                patches.Add(p);
            }
        }

        /// <summary>
        /// Parse a list of shortcut instructions from an Xml file into shortcut objects
        /// </summary>
        /// <param name="shortcuts">The list of shortcuts to parse into</param>
        /// <param name="filename">The name of the file to parse from</param>
        public static void AddShortcutsFromFile(List<Shortcut> shortcuts, string filename)
        {
            //make an Xml document to get all shortcuts
            XmlDocument doc = LoadXmlDocument(filename, XmlLoadType.FromFile);
            if (doc == null)
            {
                Logging.Error("Failed to parse Xml shortcut file, skipping");
                return;
            }
            //make new patch object for each entry
            //remember to add lots of logging
            XmlNodeList XMLshortcuts = GetXmlNodesFromXPath(doc, "//shortcuts/shortcut");
            if (XMLshortcuts == null || XMLshortcuts.Count == 0)
            {
                Logging.Warning("File {0} contains no shortcut entries", filename);
                return;
            }
            Logging.Info("Adding {0} shortcuts from shortcutFile: {1}", XMLshortcuts.Count, filename);
            foreach (XmlNode patchNode in XMLshortcuts)
            {
                Shortcut sc = new Shortcut();
                //we have the patchNode "patch" object, now we need to get it's children to actually get the properties of said patch
                foreach (XmlNode property in patchNode.ChildNodes)
                {
                    //each element in the Xml gets put into the
                    //the corresponding attribute for the Patch instance
                    switch (property.Name)
                    {
                        case "path":
                            sc.Path = property.InnerText.Trim();
                            break;
                        case "name":
                            sc.Name = property.InnerText.Trim();
                            break;
                        case "enabled":
                            sc.Enabled = CommonUtils.ParseBool(property.InnerText.Trim(), false);
                            break;
                    }
                }
                shortcuts.Add(sc);
            }
        }

        /// <summary>
        /// Parse a list of Xml unpack instructions from an Xml file into XmlUnpack objects
        /// </summary>
        /// <param name="xmlUnpacks">The list of XmlUnpacks to parse into</param>
        /// <param name="filename">The name of the file to parse from</param>
        public static void AddXmlUnpackFromFile(List<XmlUnpack> xmlUnpacks, string filename)
        {
            //make an Xml document to get all Xml Unpacks
            XmlDocument doc = LoadXmlDocument(filename, XmlLoadType.FromFile);
            if (doc == null)
            {
                Logging.Error("failed to parse Xml file");
                return;
            }
            //make new patch object for each entry
            //remember to add lots of logging
            XmlNodeList XMLUnpacks = GetXmlNodesFromXPath(doc, "//files/file");
            if (XMLUnpacks == null || XMLUnpacks.Count == 0)
            {
                Logging.Error("File {0} contains no XmlUnapck entries", filename);
                return;
            }
            Logging.Info("Adding {0} Xml unpack entries from file: {1}", XMLUnpacks.Count, filename);
            foreach (XmlNode patchNode in XMLUnpacks)
            {
                XmlUnpack xmlup = new XmlUnpack();
                //we have the patchNode "patch" object, now we need to get it's children to actually get the properties of said patch
                foreach (XmlNode property in patchNode.ChildNodes)
                {
                    //each element in the Xml gets put into the
                    //the corresponding attribute for the Patch instance
                    switch (property.Name)
                    {
                        case "pkg":
                            xmlup.Pkg = property.InnerText.Trim();
                            break;
                        case "directoryInArchive":
                            xmlup.DirectoryInArchive = property.InnerText.Trim();
                            break;
                        case "fileName":
                            xmlup.FileName = property.InnerText.Trim();
                            break;
                        case "extractDirectory":
                            xmlup.ExtractDirectory = property.InnerText.Trim();
                            break;
                        case "newFileName":
                            xmlup.NewFileName = property.InnerText.Trim();
                            break;
                    }
                }
                xmlUnpacks.Add(xmlup);
            }
        }

        /// <summary>
        /// Parse a list of Xml atlas creation instructions from an Xml file into Atlas objects
        /// </summary>
        /// <param name="atlases">The list of Atlases to parse into</param>
        /// <param name="filename">The name of the file to parse from</param>
        public static void AddAtlasFromFile(List<Atlas> atlases, string filename)
        {
            //make an Xml document to get all Xml Unpacks
            XmlDocument doc = LoadXmlDocument(filename, XmlLoadType.FromFile);
            if (doc == null)
                return;
            //make new patch object for each entry
            //remember to add lots of logging
            XmlNodeList XMLAtlases = GetXmlNodesFromXPath(doc, "//atlases/atlas");
            if (XMLAtlases == null || XMLAtlases.Count == 0)
            {
                Logging.Error("File {0} contains no atlas entries", filename);
                return;
            }
            Logging.Info("Adding {0} atlas entries from file: {1}", XMLAtlases.Count, filename);
            foreach (XmlNode atlasNode in XMLAtlases)
            {
                Atlas sc = new Atlas();
                //we have the patchNode "patch" object, now we need to get it's children to actually get the properties of said patch
                foreach (XmlNode property in atlasNode.ChildNodes)
                {
                    //each element in the Xml gets put into the
                    //the corresponding attribute for the Patch instance
                    switch (property.Name)
                    {
                        case "pkg":
                            sc.Pkg = property.InnerText.Trim();
                            break;
                        case "directoryInArchive":
                            sc.DirectoryInArchive = property.InnerText.Trim();
                            break;
                        case "atlasFile":
                            sc.AtlasFile = property.InnerText.Trim();
                            break;
                        case "mapFile":
                            sc.MapFile = property.InnerText.Trim();
                            break;
                        case "powOf2":
                            sc.PowOf2 = CommonUtils.ParseBool(property.InnerText.Trim(), false);
                            break;
                        case "square":
                            sc.Square = CommonUtils.ParseBool(property.InnerText.Trim(), false);
                            break;
                        case "fastImagePacker":
                            sc.FastImagePacker = CommonUtils.ParseBool(property.InnerText.Trim(), false);
                            break;
                        case "padding":
                            sc.Padding = CommonUtils.ParseInt(property.InnerText.Trim(), 1);
                            break;
                        case "atlasWidth":
                            sc.AtlasWidth = CommonUtils.ParseInt(property.InnerText.Trim(), 2400);
                            break;
                        case "atlasHeight":
                            sc.AtlasHeight = CommonUtils.ParseInt(property.InnerText.Trim(), 8192);
                            break;
                        case "atlasSaveDirectory":
                            sc.AtlasSaveDirectory = property.InnerText.Trim();
                            break;
                        case "imageFolders":
                            foreach (XmlNode imageFolder in property.ChildNodes)
                            {
                                sc.ImageFolderList.Add(imageFolder.InnerText.Trim());
                            }
                            break;
                    }
                }
                atlases.Add(sc);
            }
        }

        public static XmlDocument SavePatchToXmlDocument(List<Patch> PatchesList)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement patchHolder = doc.CreateElement("patchs");
            doc.AppendChild(patchHolder);
            int counter = 0;
            foreach (Patch patch in PatchesList)
            {
                Logging.Patcher("[SavePatchToXmlDocument]: Saving patch {0} of {1}: {2}", LogLevel.Info, ++counter, PatchesList.Count, patch.ToString());
                Logging.Patcher("{0}", LogLevel.Info, patch.DumpPatchInfoForLog);
                XmlElement xmlPatch = doc.CreateElement("patch");
                patchHolder.AppendChild(xmlPatch);

                XmlElement version = doc.CreateElement("version");
                version.InnerText = patch.Version.ToString();
                xmlPatch.AppendChild(version);

                XmlElement type = doc.CreateElement("type");
                type.InnerText = patch.Type;
                xmlPatch.AppendChild(type);

                XmlElement mode = doc.CreateElement("mode");
                mode.InnerText = patch.Mode;
                xmlPatch.AppendChild(mode);

                XmlElement patchPath = doc.CreateElement("patchPath");
                patchPath.InnerText = patch.PatchPath;
                xmlPatch.AppendChild(patchPath);

                XmlElement followPath = doc.CreateElement("followPath");
                followPath.InnerText = patch.Version == 1 ? false.ToString() : patch.FollowPath.ToString();
                xmlPatch.AppendChild(followPath);

                XmlElement file = doc.CreateElement("file");
                file.InnerText = patch.File;
                xmlPatch.AppendChild(file);

                if (patch.Type.Equals("regex"))
                {
                    XmlElement line = doc.CreateElement("line");
                    line.InnerText = string.Join(",", patch.Lines);
                    xmlPatch.AppendChild(line);
                }
                else
                {
                    XmlElement line = doc.CreateElement("path");
                    line.InnerText = patch.Path;
                    xmlPatch.AppendChild(line);
                }

                XmlElement search = doc.CreateElement("search");
                search.InnerText = patch.Search;
                xmlPatch.AppendChild(search);

                XmlElement replace = doc.CreateElement("replace");
                replace.InnerText = MacroUtils.MacroReplace(patch.Replace, ReplacementTypes.TextEscape);
                xmlPatch.AppendChild(replace);
            }
            return doc;
        }
        #endregion
    }
}
 