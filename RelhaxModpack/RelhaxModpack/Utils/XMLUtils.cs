using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;

namespace RelhaxModpack
{
    /// <summary>
    /// Utility class for all XML static methods
    /// </summary>
    public static class XMLUtils
    {
        //static path for all developer selections
        public const string DeveloperSelectionsXPath = "TODO";
        //check to make sure an xml file is valid
        public static bool IsValidXml(string filePath)
        {
            return IsValidXml(File.ReadAllText(filePath), Path.GetFileName(filePath));
        }
        //check to make sure an xml file is valid
        public static bool IsValidXml(string xmlString, string fileName)
        {
            using (XmlTextReader read = new XmlTextReader(xmlString))
            {
                try
                {
                    //continue to read the entire document
                    while (read.Read()) ;
                    read.Close();
                    return true;
                }
                catch (Exception e)
                {
                    Logging.WriteToLog(string.Format("Invalid XML file: {0}\n{1}",fileName,e.Message),Logfiles.Application, LogLevel.Error);
                    read.Close();
                    return false;
                }
            }
        }
        //get an xml element attribute given an xml path
        public static string GetXMLStringFromXPath(string file, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read XML file {0}\n{1}", Path.GetFileName(file),e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXMLStringFromXPath(doc, xpath);
        }
        //get an xml element attribute given an xml path
        public static string GetXMLStringFromXPath(string xmlString, string xpath, string filename)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlString);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read XML file {0}\n{1}", Path.GetFileName(filename), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXMLStringFromXPath(doc, xpath);
        }
        //get an xml element attribute given an xml path
        //element example: "//root/element"
        //attribute example: "//root/element/@attribute"
        //for the onlineFolder version: //modInfoAlpha.xml/@onlineFolder
        //for the folder version: //modInfoAlpha.xml/@version
        public static string GetXMLStringFromXPath(XmlDocument doc, string xpath)
        {
            XmlNode result = doc.SelectSingleNode(xpath);
            if (result == null)
                return null;
            return result.InnerText;
        }
        //get an xml element attribute given an xml path
        public static XmlNode GetXMLNodeFromXPath(string file, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read XML file {0}\n{1}", Path.GetFileName(file), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXMLNodeFromXPath(doc, xpath);
        }
        //get an xml element attribute given an xml path
        public static XmlNode GetXMLNodeFromXPath(string xmlString, string xpath, string filename)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlString);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read XML file {0}\n{1}", Path.GetFileName(filename), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXMLNodeFromXPath(doc, xpath);
        }
        //same as above but in a node
        public static XmlNode GetXMLNodeFromXPath(XmlDocument doc, string xpath)
        {
          XmlNode result = doc.SelectSingleNode(xpath);
          if (result == null)
              return null;
          return result;
        }
        //get an xml element attribute given an xml path
        public static XmlNodeList GetXMLNodesFromXPath(string file, string xpath)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(file);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read XML file {0}\n{1}", Path.GetFileName(file), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXMLNodesFromXPath(doc, xpath);
        }
        //get an xml element attribute given an xml path
        public static XmlNodeList GetXMLNodesFromXPath(string xmlString, string xpath, string filename)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlString);
            }
            catch (Exception e)
            {
                Logging.WriteToLog(string.Format("Failed to read XML file {0}\n{1}", Path.GetFileName(filename), e.Message), Logfiles.Application, LogLevel.Error);
                return null;
            }
            return GetXMLNodesFromXPath(doc, xpath);
        }
        //same as above but in a node collection
        //NOTE: XmlElement and XmlAttribute inherit from XmlNode
        public static XmlNodeList GetXMLNodesFromXPath(XmlDocument doc, string xpath)
        {
            XmlNodeList results = doc.SelectNodes(xpath);
          if (results == null || results.Count == 0)
          {
            return null;
          }
          return results;
        }
        public static void ParseDatabase()
        {
            //stub
        }
        public static void CheckCRC()
        {
            //stub
        }
    }
}