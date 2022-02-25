using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using RelhaxModpack.Xml;
using RelhaxModpack.Utilities.Enums;
using System.Xml;
using System.IO;
using System.Reflection;
using RelhaxModpack.Utilities;
using System.Xml.XPath;

namespace RelhaxModpack.Settings
{
    /// <summary>
    /// A helper class to parse a settings object to and from xml
    /// </summary>
    public class SettingsParser
    {
        /// <summary>
        /// Load the settings object with values from the given xml file path
        /// </summary>
        /// <param name="settingsFile">The instance of the settings class object to load settings into</param>
        /// <param name="customSettingsPath">The custom location of the settings xml file</param>
        public void LoadSettings(ISettingsFile settingsFile, string customSettingsPath)
        {
            if (!File.Exists(customSettingsPath))
            {
                Logging.Info(LogOptions.MethodName, "Settings document {0} does not exist, loading defaults", settingsFile.Filename);
                return;
            }
            Logging.Debug(LogOptions.MethodAndClassName, "Loading settings document {0} from location {1}", settingsFile.GetType().Name, customSettingsPath);
            LoadSettings(settingsFile, XmlUtils.LoadXDocument(customSettingsPath, XmlLoadType.FromFile));
            Logging.Debug(LogOptions.MethodAndClassName, "Loaded settings document {0} from location {1}", settingsFile.GetType().Name, customSettingsPath);
        }

        /// <summary>
        /// Load the settings object with values from the given xml file path
        /// </summary>
        /// <param name="settingsFile">The instance of the settings class object to load settings into</param>
        public void LoadSettings(ISettingsFile settingsFile)
        {
            if (!File.Exists(settingsFile.Filename))
            {
                Logging.Info(LogOptions.MethodName, "Settings document {0} does not exist, loading defaults", settingsFile.Filename);
                return;
            }
            Logging.Debug(LogOptions.MethodAndClassName, "Loading settings document {0} from location {1}", settingsFile.GetType().Name, settingsFile.Filename);
            LoadSettings(settingsFile, XmlUtils.LoadXDocument(settingsFile.Filename, XmlLoadType.FromFile));
            Logging.Debug(LogOptions.MethodAndClassName, "Loaded settings document {0} from location {1}", settingsFile.GetType().Name, settingsFile.Filename);
        }

        private void LoadSettings(ISettingsFile settingsFile, XDocument document)
        {
            if (settingsFile == null)
                throw new NullReferenceException();

            //add setting values from fields and properties
            Type settingsClass = settingsFile.GetType();
            List<MemberInfo> memberInfos = new List<MemberInfo>();
            memberInfos.AddRange(settingsClass.GetFields().ToList());
            Logging.Debug(LogOptions.MethodName, "Added {0} Fields to memberInfos", memberInfos.Count);
            int fieldsCount = memberInfos.Count;
            memberInfos.AddRange(settingsClass.GetProperties().ToList());
            Logging.Debug(LogOptions.MethodName, "Added {0} Properties to memberInfos", memberInfos.Count - fieldsCount);

            //filter out components from exclude list
            Logging.Debug("Components to save before exclude list: {0}", memberInfos.Count);
            memberInfos = memberInfos.FindAll(meme => !settingsFile.MembersToExclude.Contains(meme.Name));
            Logging.Debug("Components to save after exclude list: {0}", memberInfos.Count);

            //get the top level xml node of the document. it should be the name of the class, like "ModpackSettings"
            List<XElement> settingsXml = document.XPathSelectElements(string.Format("/{0}/*", settingsClass.Name)).ToList();

            //legacy compatibility
            if (settingsClass.Equals(typeof(ModpackSettings)))
            {
                ((ModpackSettings)settingsFile).ApplyLegacySettings(settingsXml);
            }

            foreach (XElement settingsElement in settingsXml)
            {
                if (settingsFile.MembersToExclude.Contains(settingsElement.Name.LocalName))
                {
                    Logging.Info("Skipping loading property/field {0}, is on exclude list", settingsElement.Name.LocalName);
                    continue;
                }

                FieldInfo fi = settingsClass.GetField(settingsElement.Name.LocalName);
                PropertyInfo pi = settingsClass.GetProperty(settingsElement.Name.LocalName);

                if (fi != null)
                {
                    if (!CommonUtils.SetObjectField(settingsFile, fi, settingsElement.Value))
                    {
                        Logging.Error("Failed to set value for field {0}, value {1}, line {2}", fi.Name, settingsElement.Value, ((IXmlLineInfo)settingsElement).LineNumber);
                        continue;
                    } 
                }
                else if (pi != null)
                {
                    if (!CommonUtils.SetObjectProperty(settingsFile, pi, settingsElement.Value))
                    {
                        Logging.Error("Failed to set value for property {0}, value {1}, line {2}", pi.Name, settingsElement.Value, ((IXmlLineInfo)settingsElement).LineNumber);
                        continue;
                    }
                }
                else
                {
                    Logging.Warning("Settings xml element '{0}' not found in class structure, skipping (is it in the exclude list?)");
                }
            }
        }

        /// <summary>
        /// Save the settings object to a custom location.
        /// </summary>
        /// <param name="settingsFile">The settings object to save to xml</param>
        /// <param name="customSettingsPath">The custom path to save the settings xml document to</param>
        public void SaveSettings(ISettingsFile settingsFile, string customSettingsPath)
        {
            SaveSettings(settingsFile, new XmlDocument(), customSettingsPath);
        }

        /// <summary>
        /// Save the settings to the default location stored in the file.
        /// </summary>
        /// <param name="settingsFile">The settings object to save to xml</param>
        public void SaveSettings(ISettingsFile settingsFile)
        {
            SaveSettings(settingsFile, new XmlDocument(), settingsFile.Filename);
        }

        private void SaveSettings(ISettingsFile settingsFile, XmlDocument document, string settingsPath)
        {
            Type settingsClass = settingsFile.GetType();
            Logging.Debug(LogOptions.MethodAndClassName, "Saving settings document {0} to location {1}", settingsClass.Name, settingsPath);

            //add declaration
            document.AppendChild(document.CreateXmlDeclaration("1.0", "UTF-8", "yes"));

            //create root settings holder
            XmlElement settingsHolder = document.CreateElement(settingsClass.Name);
            settingsHolder.SetAttribute("documentVersion", "1.1");
            document.AppendChild(settingsHolder);

            //add setting values from fields and properties
            List<MemberInfo> memberInfos = new List<MemberInfo>();
            memberInfos.AddRange(settingsClass.GetFields().ToList());
            memberInfos.AddRange(settingsClass.GetProperties().ToList());

            //filter out components from exclude list
            Logging.Debug("Components to save before exclude list: {0}", memberInfos.Count);
            memberInfos = memberInfos.FindAll(meme => !settingsFile.MembersToExclude.Contains(meme.Name));
            Logging.Debug("Components to save after exclude list: {0}", memberInfos.Count);

            //loop through each member
            foreach (MemberInfo member in memberInfos)
            {
                XmlElement element = document.CreateElement(member.Name);

                if (member is FieldInfo fieldInfo)
                {
                    element.InnerText = fieldInfo.GetValue(settingsFile).ToString();
                }
                else if (member is PropertyInfo propertyInfo)
                {
                    element.InnerText = propertyInfo.GetValue(settingsFile).ToString();
                }
                else
                {
                    Logging.Error("Failed to save property {0} from settings type {1}", member.Name, settingsClass.Name);
                    continue;
                }

                settingsHolder.AppendChild(element);
            }

            //save to disk
            if (File.Exists(settingsPath))
                File.Delete(settingsPath);
            document.Save(settingsPath);

            Logging.Debug(LogOptions.MethodAndClassName, "Saved settings document {0} to location {1}", settingsClass.Name, settingsPath);
        }
    }
}
