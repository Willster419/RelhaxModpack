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
using System.Globalization;

namespace RelhaxModpack
{
    /// <summary>
    /// Handles all custom UI color settings
    /// </summary>
    public static class UISettings
    {
        #region statics and constants
        public const string UIRoot = "UISettings.xml";
        public const string CustomColorSettingsPath = "CustomColorSettings";
        public static readonly string[] CustomSettings = new string[]
        {
            "SelectedColor",
            "NotSelectedColor",
            "SelectedTextColor",
            "NotSelectedTextColor"
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
        #region Apply to window
        public static void ApplyUIColorSettings(Window w)
        {
            if(UIDocument == null)
            {
                Logging.WriteToLog("in ApplyColorSettings(), UIDocument is null! ", Logfiles.Application, LogLevel.Error);
                return;
            }
            //get the UI format version of the xml file
            string versionXpath = "//" + UIRoot + "/@version";
            string formatVersion = XMLUtils.GetXMLStringFromXPath(UIDocument, versionXpath);
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
        
        private static void ApplyUIColorsettingsV1(Window w)
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
                //make sure we have an element that we want color changing
                if(element.Tag is string ID)
                {
                    if(!string.IsNullOrEmpty(ID))
                    {
                        //https://msdn.microsoft.com/en-us/library/ms256086(v=vs.110).aspx
                        //get the xpath component
                        string XPathColorSetting = string.Format("//{0}/{1}/ColorSetting[@ID = \"{2}\"]",UIRoot,windowXPathRefrence,ID);
                        XmlNode brushSettings = XMLUtils.GetXMLNodeFromXPath(UIDocument, XPathColorSetting);
                        //make sure setting is there
                        if(brushSettings != null)
                            ApplyBrushSettings(element, brushSettings);
                    }
                }
            }
            //custom code for ModSelectionList
            if(w is ModSelectionList modSelectionList)
            {
                //set enabled and disbled brushes properly
            }
        }
        
        private static void ApplyBrushSettings(FrameworkElement element, XmlNode brushSettings)
        {
            if (element is Control control)
            {
                if(ApplyBrushSettings(control.Name, (string)control.Tag, brushSettings, out Brush backgroundColorToChange))
                {
                    control.Background = backgroundColorToChange;
                    if(!(element is Window) && ApplyTextBrushSettings(control.Name, (string)control.Tag, brushSettings, out Brush textColorToChange))
                        control.Foreground = textColorToChange;
                }
            }
            else if (element is Panel panel)
            {
                if(ApplyBrushSettings(panel.Name, (string)panel.Tag, brushSettings, out Brush backgroundColorToChange))
                {
                    panel.Background = backgroundColorToChange;
                }
            }  
        }
        private static bool ApplyTextBrushSettings(string componentName, string componentTag, XmlNode brushSettings, out Brush textColorToChange)
        {
            bool somethingApplied = false;
            textColorToChange = new SolidColorBrush();
            //make sure type is set correctly
            XmlAttribute brushType = brushSettings.Attributes["type"];
            if (brushType == null)
            {
                Logging.WriteToLog("failed to apply brush setting: type attribute not exist!");
                return false;
            }
            XmlAttribute textColor = brushSettings.Attributes["textColor"];
            //try to apply the text color
            if (textColor != null)
            {
                if(ParseColorFromString(textColor.InnerText, out Color color))
                {
                    textColorToChange = new SolidColorBrush(color);
                    somethingApplied = true;
                }
                else
                {
                    Logging.WriteToLog(string.Format("failed to parse color to {0}, type={1}, color1={2}, ",
                        componentTag, brushType.InnerText, textColor.InnerText), Logfiles.Application, LogLevel.Warning);
                }
            }
            else
            {
                Logging.WriteToLog(string.Format("skipping text coloring of control {0}: textColor is null", componentName),
                    Logfiles.Application, LogLevel.Warning);
            }
            return somethingApplied;
        }
        private static bool ApplyBrushSettings(string componentName, string componentTag, XmlNode brushSettings,
            out Brush backgroundColorToChange)
        {
            bool someThingApplied = false;
            backgroundColorToChange = new SolidColorBrush();
            //make sure type is set correctly
            XmlAttribute brushType = brushSettings.Attributes["type"];
            if(brushType == null)
            {
                Logging.WriteToLog("failed to apply brush setting: type attribute not exist!");
                return false;
            }
            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.color.fromargb?view=netframework-4.7.2#System_Windows_Media_Color_FromArgb_System_Byte_System_Byte_System_Byte_System_Byte_
            XmlAttribute color1 = brushSettings.Attributes["color1"];
            XmlAttribute color2 = brushSettings.Attributes["color2"];
            XmlAttribute point1 = brushSettings.Attributes["point1"];
            XmlAttribute point2 = brushSettings.Attributes["point2"];
            if(color1 != null)
            {
                Point point_1;
                Point point_2;
                switch (brushType.InnerText)//TODO
                {
                    case "SolidColorBrush"://color=1
                        if (ParseColorFromString(color1.InnerText, out Color kolor1_solid))
                        {
                            backgroundColorToChange = new SolidColorBrush(kolor1_solid);
                            someThingApplied = true;
                            break;
                        }
                        else
                        {
                            Logging.WriteToLog(string.Format("failed to parse color to {0}, type={1}, color1={2}, ",
                                componentTag, brushType.InnerText, color1.InnerText), Logfiles.Application, LogLevel.Warning);
                            break;
                        }
                    case "LinearGradientBrush"://color=2, point=2
                        if (color2 == null)
                        {
                            Logging.WriteToLog(string.Format("skipping coloring of control {0}: color2 is null, type={1}",
                                componentTag, brushType.InnerText), Logfiles.Application, LogLevel.Warning);
                            break;
                        }
                        if (point1 == null)
                        {
                            Logging.WriteToLog(string.Format("skipping coloring of control {0}: point1 is null, type={1}",
                                componentTag, brushType.InnerText), Logfiles.Application, LogLevel.Warning);
                            break;
                        }
                        if (point2 == null)
                        {
                            Logging.WriteToLog(string.Format("skipping coloring of control {0}: point2 is null, type={1}",
                                componentTag, brushType.InnerText), Logfiles.Application, LogLevel.Warning);
                            break;
                        }
                        if (ParseColorFromString(color1.InnerText, out Color kolor1_linear) &&
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
                                break;
                            }
                            VerifyPoints(point_1);
                            VerifyPoints(point_2);
                            backgroundColorToChange = new LinearGradientBrush(kolor1_linear, kolor2_linear, point_1, point_2);
                            someThingApplied = true;
                            break;
                        }
                        else
                        {
                            Logging.WriteToLog(string.Format("failed to parse color to {0}, type={1}, color1={2}, color2={3}",
                                componentTag, brushType.InnerText, color1.InnerText, color2.InnerText), Logfiles.Application, LogLevel.Warning);
                            break;
                        }
                    case "RadialGradientBrush"://color=2
                        if (color2 == null)
                        {
                            Logging.WriteToLog(string.Format("skipping coloring of control {0}: color2 is null, type={1}",
                                componentTag, brushType.InnerText), Logfiles.Application, LogLevel.Warning);
                            break;
                        }
                        if (ParseColorFromString(color1.InnerText, out Color kolor1_radial)
                            && ParseColorFromString(color2.InnerText, out Color kolor2_radial))
                        {
                            backgroundColorToChange = new RadialGradientBrush(kolor1_radial, kolor2_radial);
                            someThingApplied = true;
                            break;
                        }
                        else
                        {
                            Logging.WriteToLog(string.Format("failed to apply color to {0}, type={1}, color1={2}, color2={3}",
                                componentTag, brushType.InnerText, color1.InnerText, color2.InnerText), Logfiles.Application, LogLevel.Warning);
                            break;
                        }
                    default:
                        Logging.WriteToLog(string.Format("unknown type parameter{0} in component {1} ", brushType.InnerText, componentTag));
                        break;
                }
            }
            else
            {
                Logging.WriteToLog(string.Format("skipping coloring of control {0}: color1 is null, type={1}",
                    componentName, brushType.InnerText), Logfiles.Application, LogLevel.Warning);
            }
            return someThingApplied;
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
                aPart = color.Substring(1,2);
                rPart = color.Substring(3,2);
                gPart = color.Substring(5,2);
                bPart = color.Substring(7,2);
            }
            catch(ArgumentException)
            {
              Logging.WriteToLog(string.Format("failed to parse color, a={0}, r={1}, g={2}, b={3}",aPart, rPart, gPart, bPart)
                  ,Logfiles.Application,LogLevel.Warning);
              return false;
            }
            if((byte.TryParse(aPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte a)) &&
                (byte.TryParse(rPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte r))&&
                (byte.TryParse(gPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte g))&&
                (byte.TryParse(bPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b)))
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
        #endregion
        #region Dump to file
        public static void DumpAllWindowColorSettingsToFile(string savePath)
        {
            //make xml document and declaration
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            //append declaration to document
            doc.AppendChild(dec);
            //make root element and version attribute
            XmlElement root = doc.CreateElement(UIRoot);
            //NOTE: version attribute should be incrimented when large change in color loading structure is done
            //allows us to make whole new method to load UI settings
            XmlAttribute version = doc.CreateAttribute("version");
            //append to document
            version.Value = "1.0";
            root.Attributes.Append(version);
            doc.AppendChild(root);
            //add all window instances to document:
            //make windows for all appropriate windows
            //TODO: more windows
            DumpWindowColorSettingsToXml(root, doc, new MainWindow());
            //save custom color settings to document
            //for now, just use single solid color for these settings
            foreach(string s in CustomSettings)
            {
                XmlElement element = doc.CreateElement(s);
                XmlAttribute color = doc.CreateAttribute("color");
                color.Value = "TODO";
                element.Attributes.Append(color);
                root.AppendChild(element);
            }
            //save xml file
            doc.Save(savePath);
        }

        private static void DumpWindowColorSettingsToXml(XmlElement root, XmlDocument doc, Window w)
        {
            //make window element
            XmlElement windowElement = doc.CreateElement(w.GetType().ToString());
            //save attributes to element
            ApplyColorattributesToElement(windowElement, w.Background, doc);
            //same to root
            root.AppendChild(windowElement);
            //get list of all frameowrk elements in the window
            //TODO: this may not work due to visual not being shown
            //TODO: need to disable translations to save CPU TIME
            List<FrameworkElement> AllUIElements = Utils.GetAllWindowComponentsLogical(w, false);
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
            foreach(FrameworkElement element in AllUIElements)
            {
                XmlElement colorSetting = doc.CreateElement("ColorSetting");
                string ID = (string)element.Tag;
                //save attributes to element
                XmlAttribute elementID = doc.CreateAttribute("ID");
                elementID.Value = ID;
                colorSetting.Attributes.Append(elementID);
                if (element is Panel panel)
                    ApplyColorattributesToElement(colorSetting, panel.Background, doc);
                else if (element is Control control)
                    ApplyColorattributesToElement(colorSetting, control.Background, doc, control.Foreground);
                else
                    continue;
                windowElement.AppendChild(colorSetting);
            }
            w.Close();
            w = null;
        }

        private static void ApplyColorattributesToElement(XmlElement colorEntry, Brush brush, XmlDocument doc, Brush textBrush = null)
        {
            XmlAttribute colorType, color1, textColor;
            if(brush is SolidColorBrush solidColorBrush)
            {
                //type
                colorType = doc.CreateAttribute("type");
                colorType.Value = nameof(SolidColorBrush);
                colorEntry.Attributes.Append(colorType);
                //color1
                color1 = doc.CreateAttribute("color1");
                color1.Value = solidColorBrush.Color.ToString(CultureInfo.InvariantCulture);
                colorEntry.Attributes.Append(color1);
            }
            else if (brush is LinearGradientBrush linearGradientBrush)
            {
                //type
                colorType = doc.CreateAttribute("type");
                colorType.Value = nameof(LinearGradientBrush);
                colorEntry.Attributes.Append(colorType);
                //color1
                color1 = doc.CreateAttribute("color1");
                color1.Value = linearGradientBrush.GradientStops[0].Color.ToString(CultureInfo.InvariantCulture);
                colorEntry.Attributes.Append(color1);
                //color2
                XmlAttribute color2 = doc.CreateAttribute("color2");
                color2.Value = linearGradientBrush.GradientStops[linearGradientBrush.GradientStops.Count-1].Color.ToString(CultureInfo.InvariantCulture);
                colorEntry.Attributes.Append(color2);
                //point1
                XmlAttribute point1 = doc.CreateAttribute("point1");
                point1.Value = linearGradientBrush.StartPoint.ToString(CultureInfo.InvariantCulture);
                colorEntry.Attributes.Append(point1);
                //point2
                XmlAttribute point2 = doc.CreateAttribute("point2");
                point2.Value = linearGradientBrush.EndPoint.ToString(CultureInfo.InvariantCulture);
                colorEntry.Attributes.Append(point2);
            }
            else if (brush is RadialGradientBrush radialGradientBrush)
            {
                //type
                colorType = doc.CreateAttribute("type");
                colorType.Value = nameof(RadialGradientBrush);
                colorEntry.Attributes.Append(colorType);
                //color1
                color1 = doc.CreateAttribute("color1");
                color1.Value = radialGradientBrush.GradientStops[0].Color.ToString(CultureInfo.InvariantCulture);
                colorEntry.Attributes.Append(color1);
                //color2
                XmlAttribute color2 = doc.CreateAttribute("color2");
                color2.Value = radialGradientBrush.GradientStops[radialGradientBrush.GradientStops.Count - 1].Color.ToString(CultureInfo.InvariantCulture);
                colorEntry.Attributes.Append(color2);
            }
            else
            {
                Logging.WriteToLog("Unknown background type: " + brush.GetType().ToString(), Logfiles.Application, LogLevel.Debug);
            }
            if(textBrush != null)
            {
                //text color (forground)
                textColor = doc.CreateAttribute("textColor");
                //should all be solid color brushes...
                SolidColorBrush solidColorTextBrush = (SolidColorBrush)textBrush;
                textColor.Value = solidColorTextBrush.Color.ToString(CultureInfo.InvariantCulture);
                colorEntry.Attributes.Append(textColor);
            }
        }
        #endregion
    }
}