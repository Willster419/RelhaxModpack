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
using RelhaxModpack.Patching;
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
        /// Get a string value of the xml element or attribute inner text
        /// </summary>
        /// <param name="doc">The XDocument to get the value from</param>
        /// <param name="xpath">The xpath search term</param>
        /// <returns>The xpath return result, null if no value or failed expression</returns>
        public static string GetXmlStringFromXPath(XDocument doc, string xpath)
        {
            string result;
            try
            {
                XPathNavigator navigator = doc.CreateNavigator();
                result = navigator.SelectSingleNode(xpath).Value;
                return result;
            }
            catch
            {
                return null;
            }
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
        /// Get an Xml node value given an Xml path
        /// </summary>
        /// <param name="doc">The XmlDocument object to search</param>
        /// <param name="xpath">The xpath string</param>
        /// <returns>The XPathNavigator node of the search result, or null</returns>
        public static XPathNavigator GetXNodeFromXpath(XDocument doc, string xpath)
        {
            XPathNavigator node;
            try
            {
                XPathNavigator navigator = doc.CreateNavigator();
                node = navigator.SelectSingleNode(xpath);
                return node.NodeType == XPathNodeType.Attribute || node.NodeType == XPathNodeType.Element ? node : null;
            }
            catch
            {
                return null;
            }
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

        /// <summary>
        /// Get a List of XPathNavigators that match given an Xml path
        /// </summary>
        /// <param name="doc">The XmlDocument to search</param>
        /// <param name="xpath">The xml path string</param>
        /// <returns>The node list of matching results, or null</returns>
        public static List<XPathNavigator> GetXNodesFromXpath(XDocument doc, string xpath)
        {
            List<XPathNavigator> nodeList = new List<XPathNavigator>();
            try
            {
                XPathNavigator navigator = doc.CreateNavigator();
                XPathNodeIterator iterator = navigator.Select(xpath);
                foreach (XPathNavigator node in iterator)
                {
                    if (node.NodeType == XPathNodeType.Attribute || node.NodeType == XPathNodeType.Element)
                        nodeList.Add(node);
                }
                return nodeList;
            }
            catch
            {
                return null;
            }
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
        /// Convert an XmlElement to an XElement
        /// </summary>
        /// <param name="element">The element to convert to XElement</param>
        /// <returns>The converted XElement</returns>
        public static XElement ElementToXElement(XmlElement element)
        {
            if (element == null)
                throw new NullReferenceException();
            return XElement.Parse(element.OuterXml, LoadOptions.PreserveWhitespace);
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
    }
}
 