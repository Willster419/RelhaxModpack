using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Xml;
using System.IO;
using System.Windows.Controls;
using RelhaxModpack.Windows;

namespace RelhaxModpack
{
    /// <summary>
    /// Handles all custom UI color settings
    /// </summary>
    public static class UISettings
    {
        #region statics and constants
        public const string UIRoot = "UISettings.XML";
        public const string CustomColorSettingsPath = "CustomColorSettings";
        public static readonly string[] CustomSettings = new string[]
        {
            "ColorChangeColor"
        };
        public static readonly Type[] WindowsWithColor = new Type[]
        {
            typeof(AddPicturesZip)
        };
        public static XmlDocument UIDocument;
        #endregion
        /// <summary>
        /// Load the custom color definitions from XML
        /// </summary>
        public static void LoadSettings(bool init)
        {
            //check for single time making
            if(UIDocument != null && init)
            {
                Logging.WriteToLog("UIDocument is not null and init=true! (is this not the first time in LoadSettings()?)",Logfiles.Application, LogLevel.Error);
                return;
            }
            if(init)
                UIDocument = new XmlDocument();
            if(!File.Exists(Settings.UISettingsFileName))
            {
                Logging.WriteToLog("UIDocument file does not exist, aborting!)",Logfiles.Application, LogLevel.Error);
                return;
            }
            try
            {
                UIDocument.Load(Settings.UISettingsFileName);
            }
            catch (XmlException exml)
            {
                Logging.WriteToLog(string.Format("Failed to load UISettings.xml document\n{0}",exml.Message), Logfiles.Application, LogLevel.Error);
                return;
            }
            Logging.WriteToLog("UIDocument xml file loaded sucessfully", Logfiles.Application, LogLevel.Debug);
        }
        
        public static void ApplyUIColorSettings(Window w)
        {
            if(UIDocument == null)
            {
                Logging.WriteToLog("UIDocument is null! in ApplyColorSettings()",Logfiles.Application, LogLevel.Error);
                return;
            }
            //get the UI format version of the xml file
            string formatVersion = XMLUtils.GetXMLStringFromXPath(UIDocument, "//" + UIRoot + "/@version");
            if(string.IsNullOrWhiteSpace(formatVersion))
            {
                Logging.WriteToLog("formatVersion string is null! in ApplyColorSettings()",Logfiles.Application, LogLevel.Error);
                return;
            }
            switch(formatVersion)
            {
                case "1.0":
                    ApplyUIColorsettingsV1(w);
                    break;
                default:
                    //unknonw
                    Logging.WriteToLog("formatVersion string not match case! in ApplyColorSettings()",Logfiles.Application, LogLevel.Error);
                    break;
            }
        }
        
        public static void ApplyUIColorsettingsV1(Window w)
        {
            //build the xpath string
            string windowXPathRefrence = w.GetType().ToString();
            string XPathWindowColor = string.Format("//{0}/{1}",UIRoot,windowXPathRefrence);
            XmlNode WindowColorSetting = XMLUtils.GetXMLNodeFromXPath(UIDocument, XPathWindowColor);
            //apply window color settings if exist
            if(WindowColorSetting !=null )
            {
                ApplyBrushSettings(w,WindowColorSetting);
            }
            //build list of all internal framework components
            List<FrameworkElement> allWindowControls = Utils.GetAllWindowComponentsVisual(w, false);
            foreach (FrameworkElement element in allWindowControls)
            {
                if(!(element is Control ctrl))
                {
                    Logging.WriteToLog(string.Format("skipping UI color apply of component {0} in window {1} due ot not a Control class element"
                        ,element.Name, w.GetType().ToString()),Logfiles.Application, LogLevel.Debug);
                    continue;
                }
                //make sure we have an element that we want color changing
                if(ctrl.Tag is string ID)
                {
                    if(!string.IsNullOrEmpty(ID))
                    {
                        //https://msdn.microsoft.com/en-us/library/ms256086(v=vs.110).aspx
                        //get the xpath component
                        string XPathColorSetting = string.Format("//{0}/{1}/ColorSetting[@ID = \"{2}\"]",UIRoot,windowXPathRefrence,ID);
                        XmlNode brushSettings = XMLUtils.GetXMLNodeFromXPath(UIDocument, XPathColorSetting);
                        //make sure setting is there
                        if(brushSettings != null)
                            ApplyBrushSettings(ctrl, brushSettings);
                    }
                }
            }
        }
        
        public static void ApplyBrushSettings(Control ctrl, XmlNode brushSettings)
        {
            //make sure type is set correctly
            XmlAttribute brushType = brushSettings.Attributes["type"];
            if(brushType == null)
            {
                Logging.WriteToLog("failed to apply brush setting: type attribute not exist!");
                return;
            }
            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.color.fromargb?view=netframework-4.7.2#System_Windows_Media_Color_FromArgb_System_Byte_System_Byte_System_Byte_System_Byte_
            XmlAttribute color1 = brushSettings.Attributes["color1"];
            XmlAttribute color2 = brushSettings.Attributes["color2"];
            XmlAttribute point1 = brushSettings.Attributes["point1"];
            XmlAttribute point2 = brushSettings.Attributes["point2"];
            if(color1 == null)
            {
                Logging.WriteToLog(string.Format("skipping coloring of control {0}: color1 is null, type={1}",ctrl.Name,brushType.InnerText),Logfiles.Application,LogLevel.Warning);
                return;
            }
            Point point_1;
            Point point_2;
            switch (brushType.InnerText)//TODO
            {
                case "SolidColorBrush"://color=1
                    if(ParseColorFromString(color1.InnerText, out Color kolor1_solid))
                    {
                        ctrl.Background = new SolidColorBrush(kolor1_solid);
                    }
                    else
                    {
                        Logging.WriteToLog(string.Format("failed to parse color to {0}, type={1}, color1={2}, ",
                            ctrl.Tag, brushType.InnerText, color1.InnerText), Logfiles.Application, LogLevel.Warning);
                    }
                    break;
                case "LinearGradientBrush"://color=2, point=2
                    if(color2 == null)
                    {
                        Logging.WriteToLog(string.Format("skipping coloring of control {0}: color2 is null, type={1}",
                            ctrl.Tag, brushType.InnerText),Logfiles.Application,LogLevel.Warning);
                        return;
                    }
                    if(point1 == null)
                    {
                        Logging.WriteToLog(string.Format("skipping coloring of control {0}: point1 is null, type={1}",
                            ctrl.Tag, brushType.InnerText),Logfiles.Application,LogLevel.Warning);
                        return;
                    }
                    if(point2 == null)
                    {
                        Logging.WriteToLog(string.Format("skipping coloring of control {0}: point2 is null, type={1}",
                            ctrl.Tag, brushType.InnerText),Logfiles.Application,LogLevel.Warning);
                        return;
                    }
                    if(ParseColorFromString(color1.InnerText, out Color kolor1_linear) &&
                        ParseColorFromString(color2.InnerText, out Color kolor2_linear))
                    {
                        try
                        {
                            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.point.parse?view=netframework-4.7.2
                            point_1 = Point.Parse(point1.InnerText);
                            point_2 = Point.Parse(point2.InnerText);
                        }
                        catch
                        {
                            Logging.WriteToLog(string.Format("failed to parse points, point1={0}, point2={1}",
                                point1.InnerText, point2.InnerText), Logfiles.Application, LogLevel.Warning);
                            return;
                        }
                        VerifyPoints(point_1);
                        VerifyPoints(point_2);
                        ctrl.Background = new LinearGradientBrush(kolor1_linear, kolor2_linear, point_1, point_2);
                    }
                    else
                    {
                        Logging.WriteToLog(string.Format("failed to parse color to {0}, type={1}, color1={2}, color2={3}",
                            ctrl.Tag, brushType.InnerText, color1.InnerText, color2.InnerText), Logfiles.Application, LogLevel.Warning);
                    }
                    break;
                case "RadialGradientBrush"://color=2
                    if(color2 == null)
                    {
                        Logging.WriteToLog(string.Format("skipping coloring of control {0}: color2 is null, type={1}",
                            ctrl.Tag, brushType.InnerText),Logfiles.Application,LogLevel.Warning);
                        return;
                    }
                    if(ParseColorFromString(color1.InnerText, out Color kolor1_radial)
                        && ParseColorFromString(color2.InnerText, out Color kolor2_radial))
                    {
                        ctrl.Background = new RadialGradientBrush(kolor1_radial, kolor2_radial);
                    }
                    else
                    {
                        Logging.WriteToLog(string.Format("failed to apply color to {0}, type={1}, color1={2}, color2={3}",
                            ctrl.Tag, brushType.InnerText, color1.InnerText, color2.InnerText), Logfiles.Application, LogLevel.Warning);
                    }
                    break;
                default:
                    Logging.WriteToLog(string.Format("unknown type parameter{0} in component {1} ",brushType.InnerText,ctrl.Tag));
                    return;
            }
        }
        
        public static bool ParseColorFromString(string color, out Color kolor)
        {
            kolor = new Color();
            string aPart = string.Empty;
            string rPart = string.Empty;
            string gPart = string.Empty;
            string bPart = string.Empty;
            try
            {
                aPart = color.Substring(0,2);
                rPart = color.Substring(2,2);
                gPart = color.Substring(4,2);
                bPart = color.Substring(6,2);
            }
            catch(ArgumentException)
            {
              Logging.WriteToLog(string.Format("failed to parse color, a={0}, r={1}, g={2}, b={3}",aPart, rPart, gPart, bPart)
                  ,Logfiles.Application,LogLevel.Warning);
              return false;
            }
            if((byte.TryParse(aPart, out byte a)) && (byte.TryParse(rPart, out byte r)) 
                && (byte.TryParse(gPart, out byte g)) && (byte.TryParse(bPart, out byte b)))
            {
                kolor = Color.FromArgb(a, r, g, b);
                return true;
            }
            else
                Logging.WriteToLog(string.Format("failed to parse color, a={0}, r={1}, g={2}, b={3}",aPart, rPart, gPart, bPart)
                    ,Logfiles.Application,LogLevel.Warning);
            return false;
        }
        
        public static void VerifyPoints(Point p)
        {
            if(p.X > 1)
            {
                Logging.WriteToLog(string.Format("point.X is out of bounds (must be between 0 and 1, current value={0}), setting to 1)")
                    ,Logfiles.Application, LogLevel.Warning);
                p.X = 1;
            }
            if(p.X < 0)
            {
                Logging.WriteToLog(string.Format("point.X is out of bounds (must be between 0 and 1, current value={0}), setting to 0)")
                    ,Logfiles.Application, LogLevel.Warning);
                p.X = 0;
            }
            if(p.Y > 1)
            {
                Logging.WriteToLog(string.Format("point.Y is out of bounds (must be between 0 and 1, current value={0}), setting to 1)")
                    ,Logfiles.Application, LogLevel.Warning);
                p.Y = 1;
            }
            if(p.Y < 0)
            {
                Logging.WriteToLog(string.Format("point.Y is out of bounds (must be between 0 and 1, current value={0}), setting to 0)")
                    ,Logfiles.Application, LogLevel.Warning);
                p.Y = 0;
            }
        }
        
        public static void DumpAllWindowColorSettingsToFile(string savePath)
        {
            //make xml document and declaration
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            doc.AppendChild(dec);
            //make root element and attribute
            XmlElement root = doc.CreateElement(UIRoot);
            XmlAttribute version = doc.CreateAttribute("version");
            version.Value = "1.0";
            root.Attributes.Append(version);
            doc.AppendChild(root);
            //add all window instances to document:
            //make windows for all appropriate windows
            DumpWindowColorSettingsToXml(root, doc, new AddPicturesZip());//sample TODO more
            //save xml file
            doc.Save(savePath);
        }

        public static void DumpWindowColorSettingsToXml(XmlElement root, XmlDocument doc, Window w)
        {
            //make window element
            XmlElement windowElement = doc.CreateElement(w.GetType().ToString());
            //save attributes to element
            //same to root
            root.AppendChild(windowElement);
            //get list of all frameowrk elements in the window
            //TODO: this may not work due to visual not being shown
            //TODO: need to disable translations to save CPU TIME
            List<FrameworkElement> AllUIElements = Utils.GetAllWindowComponentsVisual(w, false);
            for(int i = 0; i < AllUIElements.Count; )
            {
                if (AllUIElements[i].Tag == null)
                {
                    AllUIElements.RemoveAt(i);
                    continue;
                }
                if (!(AllUIElements[i].Tag is string s))
                {
                    AllUIElements.RemoveAt(i);
                    continue;
                }
                if (string.IsNullOrWhiteSpace(s))
                {
                    AllUIElements.RemoveAt(i);
                    continue;
                }
                i++;
            }
            //make xml entries for each UI element now
            foreach(FrameworkElement frameworkElement in AllUIElements)
            {
                XmlElement colorSetting = doc.CreateElement("ColorSetting");
                //save attributes to element

                windowElement.AppendChild(colorSetting);
            }
        }
    }
}