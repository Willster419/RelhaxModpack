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
using System.Windows.Media.Imaging;
using System.Reflection;
using System.ComponentModel;

namespace RelhaxModpack
{
    /// <summary>
    /// Structure to allow for custom color brush elements to be loaded and saved to/from disk
    /// </summary>
    public struct CustomBrushSetting
    {
        /// <summary>
        /// The brush for color application
        /// </summary>
        public Brush @Brush;

        /// <summary>
        /// The internal name of the setting
        /// </summary>
        public string SettingName;

        //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/using-structs
        /// <summary>
        /// Create an instance of the CustomBrushSetting structure
        /// </summary>
        /// <param name="settingName">The internal name of the setting</param>
        /// <param name="brush">The brush for color application</param>
        public CustomBrushSetting(string settingName, Brush brush)
        { Brush = brush; SettingName = settingName; }
    };

    public struct ReplacedBrushes
    {
        public Brush BackgroundBrush;

        public Brush TextBrush;

        public ReplacedBrushes(Brush background, Brush text)
        { BackgroundBrush = background; TextBrush = text; }
    }

    /// <summary>
    /// Handles all custom UI settings
    /// </summary>
    public static class UISettings
    {
        #region Statics and constants
        /// <summary>
        /// The name of the Xml element to hold all custom color settings
        /// </summary>
        /// <remarks>See CustomSettings array for list of custom colors</remarks>
        public const string CustomColorSettingsPathV1 = "CustomColorSettings";

        /// <summary>
        /// The parsed XML document containing the xml color settings
        /// </summary>
        public static XmlDocument UIDocument;

        /// <summary>
        /// The color to use for when a component is selected in the selection list
        /// </summary>
        public static CustomBrushSetting SelectedPanelColor = new CustomBrushSetting(nameof(SelectedPanelColor), new SolidColorBrush(Colors.BlanchedAlmond));

        /// <summary>
        /// The color to use for when a component is not selection in the selection list
        /// </summary>
        public static CustomBrushSetting NotSelectedPanelColor = new CustomBrushSetting(nameof(NotSelectedPanelColor), new SolidColorBrush(Colors.White));

        /// <summary>
        /// The color to use when a component is selected in the selection list
        /// </summary>
        public static CustomBrushSetting SelectedTextColor = new CustomBrushSetting(nameof(SelectedTextColor), SystemColors.ControlTextBrush);

        /// <summary>
        /// The color to use when a component is not selected in the selection list
        /// </summary>
        public static CustomBrushSetting NotSelectedTextColor = new CustomBrushSetting(nameof(NotSelectedTextColor), SystemColors.ControlTextBrush);

        /// <summary>
        /// A list of custom colors for controlling color behavior of components that 
        /// </summary>
        /// <remarks>Settings that exist in here don't directly map to 1 setting and control other color settings.
        /// For example, changing the color of a selected component in the mod selection list</remarks>
        private static readonly CustomBrushSetting[] CustomColorSettings = new CustomBrushSetting[]
        {
            SelectedPanelColor,
            NotSelectedPanelColor,
            SelectedTextColor,
            NotSelectedTextColor,
            ButtonHighlightBrush,
            TabItemHighlightBrush,
            TabItemSelectedBrush,
            CheckboxHighlightBrush,
            CheckboxCheckmarkBrush,
            RadioButtonHighlightBrush,
            RadioButtonCheckmarkBrush,
            ComboboxOutsideHighlightBrush,
            ComboboxInsideColorBrush,
            ComboboxOutsideColorBrush
        };

        //white
        private static SolidColorBrush DarkThemeTextColor = new SolidColorBrush(Colors.White);

        //very dark grey
        private static SolidColorBrush DarkThemeBackground = new SolidColorBrush(Color.FromArgb(255,26,26,26));

        //dark gray
        private static SolidColorBrush DarkThemeButton = new SolidColorBrush(Color.FromArgb(255,42,42,42));

        private static Dictionary<string, ReplacedBrushes> OriginalColors = new Dictionary<string, ReplacedBrushes>();

        private static Dictionary<string, Brush> BackedUpWindows = new Dictionary<string, Brush>();

        private static Dictionary<string, ReplacedBrushes> DarkThemeCustomBrushes = new Dictionary<string, ReplacedBrushes>()
        {
            //{"test", new ReplacedBrushes(null,null) }
            {"HomepageButtonImageBorder", new ReplacedBrushes(new SolidColorBrush(Color.FromArgb(255,175,175,175)),null) },
            {"FindBugAddModButtonImageBorder", new ReplacedBrushes(new SolidColorBrush(Color.FromArgb(255,175,175,175)),null) },
            {"SendEmailButtonImageBorder", new ReplacedBrushes(new SolidColorBrush(Color.FromArgb(255,175,175,175)),null) },
            {"DonateButtonImageBorder", new ReplacedBrushes(new SolidColorBrush(Color.FromArgb(255,175,175,175)),null) },
        };

        #region Highlighting properties
        //button (highlight)
        private static CustomBrushSetting buttonHighlightBrush = new CustomBrushSetting(nameof(ButtonHighlightBrush),null);
        public static CustomBrushSetting ButtonHighlightBrush
        {
            get
            {
                return buttonHighlightBrush;
            }
            set
            {
                buttonHighlightBrush = value;
                OnStaticPropertyChanged(nameof(ButtonHighlightBrush));
            }
        }

        //tabControl (highlight and selected)
        private static CustomBrushSetting tabItemHighlightBrush = new CustomBrushSetting(nameof(TabItemHighlightBrush), null);
        public static CustomBrushSetting TabItemHighlightBrush
        {
            get
            {
                return tabItemHighlightBrush;
            }
            set
            {
                tabItemHighlightBrush = value;
                OnStaticPropertyChanged(nameof(TabItemHighlightBrush));
            }
        }

        private static CustomBrushSetting tabItemSelectedBrush = new CustomBrushSetting(nameof(TabItemSelectedBrush), null);
        public static CustomBrushSetting TabItemSelectedBrush
        {
            get
            {
                return tabItemSelectedBrush;
            }
            set
            {
                tabItemSelectedBrush = value;
                OnStaticPropertyChanged(nameof(TabItemSelectedBrush));
            }
        }

        //checkbox (highlight and mark)
        private static CustomBrushSetting checkboxHighlightBrush = new CustomBrushSetting(nameof(CheckboxHighlightBrush), null);
        public static CustomBrushSetting CheckboxHighlightBrush
        {
            get
            {
                return checkboxHighlightBrush;
            }
            set
            {
                checkboxHighlightBrush = value;
                OnStaticPropertyChanged(nameof(CheckboxHighlightBrush));
            }
        }

        private static CustomBrushSetting checkboxCheckmarkBrush = new CustomBrushSetting(nameof(CheckboxCheckmarkBrush), null);
        public static CustomBrushSetting CheckboxCheckmarkBrush
        {
            get
            {
                return checkboxCheckmarkBrush;
            }
            set
            {
                checkboxCheckmarkBrush = value;
                OnStaticPropertyChanged(nameof(CheckboxCheckmarkBrush));
            }
        }

        //radioButton (highlight and mark)
        private static CustomBrushSetting radioButtonHighlightBrush = new CustomBrushSetting(nameof(RadioButtonHighlightBrush), null);
        public static CustomBrushSetting RadioButtonHighlightBrush
        {
            get
            {
                return radioButtonHighlightBrush;
            }
            set
            {
                radioButtonHighlightBrush = value;
                OnStaticPropertyChanged(nameof(RadioButtonHighlightBrush));
            }
        }

        private static CustomBrushSetting radioButtonCheckmarkBrush = new CustomBrushSetting(nameof(RadioButtonCheckmarkBrush), null);
        public static CustomBrushSetting RadioButtonCheckmarkBrush
        {
            get
            {
                return radioButtonCheckmarkBrush;
            }
            set
            {
                radioButtonCheckmarkBrush = value;
                OnStaticPropertyChanged(nameof(RadioButtonCheckmarkBrush));
            }
        }

        //combobox (highlight, outside color, inside color)
        private static CustomBrushSetting comboboxOutsideHighlightBrush = new CustomBrushSetting(nameof(ComboboxOutsideHighlightBrush), null);
        public static CustomBrushSetting ComboboxOutsideHighlightBrush
        {
            get
            {
                return comboboxOutsideHighlightBrush;
            }
            set
            {
                comboboxOutsideHighlightBrush = value;
                OnStaticPropertyChanged(nameof(ComboboxOutsideHighlightBrush));
            }
        }

        private static CustomBrushSetting comboboxInsideColorBrush = new CustomBrushSetting(nameof(ComboboxInsideColorBrush), null);
        public static CustomBrushSetting ComboboxInsideColorBrush
        {
            get
            {
                return comboboxInsideColorBrush;
            }
            set
            {
                comboboxInsideColorBrush = value;
                OnStaticPropertyChanged(nameof(ComboboxInsideColorBrush));
            }
        }

        private static CustomBrushSetting comboboxOutsideColorBrush = new CustomBrushSetting(nameof(ComboboxOutsideColorBrush), null);
        public static CustomBrushSetting ComboboxOutsideColorBrush
        {
            get
            {
                return comboboxOutsideColorBrush;
            }
            set
            {
                comboboxOutsideColorBrush = value;
                OnStaticPropertyChanged(nameof(ComboboxOutsideColorBrush));
            }
        }

        #endregion

        #region Default UI options
        //x:Key="Button.MouseOver.Background" Color="#FFBEE6FD"
        public static Brush DefaultButtonHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 190, 230, 253));

        /*
           <LinearGradientBrush x:Key="TabItem.MouseOver.Background" EndPoint="0,1" StartPoint="0,0">
            <GradientStop Color="#ECF4FC" Offset="0.0"/>
            <GradientStop Color="#DCECFC" Offset="1.0"/>
           </LinearGradientBrush>
        */
        public static Brush DefaultTabItemHighlightBrush = new LinearGradientBrush(new GradientStopCollection()
        {
            new GradientStop(Color.FromArgb(255,236,244,252),0),
            new GradientStop(Color.FromArgb(255,220,237,252),1)
        })
        {
            EndPoint = new Point(0, 1),
            StartPoint = new Point(0, 0)
        };

        //x:Key="TabItem.Selected.Background" Color="#FFFFFF"
        public static Brush DefaultTabItemSelectedBrush = new SolidColorBrush(Colors.White);

        //x:Key="OptionMark.MouseOver.Background" Color="#FFF3F9FF"
        public static Brush DefaultCheckboxHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 243, 249, 255));

        //x:Key="OptionMark.MouseOver.Glyph" Color="#FF212121"
        //x:Key="OptionMark.Pressed.Glyph" Color="#FF212121"
        //x:Key="OptionMark.Static.Glyph" Color="#FF212121"
        public static Brush DefaultCheckboxCheckmarkBrush = new SolidColorBrush(Color.FromArgb(255, 33, 33, 33));

        //x:Key="RadioButton.MouseOver.Background" Color="#FFF3F9FF"
        public static Brush DefaultRadioButtonHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 243, 249, 255));

        //x:Key="RadioButton.MouseOver.Glyph" Color="#FF212121"
        //x:Key="RadioButton.Pressed.Glyph" Color="#FF212121"
        //x:Key="RadioButton.Static.Glyph" Color="#FF212121"
        public static Brush DefaultRadioButtonCheckmarkBrush = new SolidColorBrush(Color.FromArgb(255, 33, 33, 33));

        /*
          <LinearGradientBrush x:Key="ComboBox.MouseOver.Background" EndPoint="0,1" StartPoint="0,0">
            <GradientStop Color="#FFECF4FC" Offset="0.0"/>
            <GradientStop Color="#FFDCECFC" Offset="1.0"/>
          </LinearGradientBrush>
        */
        public static Brush DefaultComboboxOutsideHighlightBrush = new LinearGradientBrush(new GradientStopCollection()
        {
            new GradientStop(Color.FromArgb(255,236,244,252),0),
            new GradientStop(Color.FromArgb(255,220,236,252),1)
        })
        {
            EndPoint = new Point(0, 1),
            StartPoint = new Point(0, 0)
        };

        /*
          <LinearGradientBrush x:Key="ComboBox.Static.Background" EndPoint="0,1" StartPoint="0,0">
            <GradientStop Color="#FFF0F0F0" Offset="0.0"/>
            <GradientStop Color="#FFE5E5E5" Offset="1.0"/>
          </LinearGradientBrush>
        */
        public static Brush DefaultComboboxOutsideColorBrush = new LinearGradientBrush(new GradientStopCollection()
        {
            new GradientStop(Color.FromArgb(255,240,240,240),0),
            new GradientStop(Color.FromArgb(255,229,229,229),1)
        })
        {
            EndPoint = new Point(0, 1),
            StartPoint = new Point(0, 0)
        };

        //<Border x:Name="dropDownBorder" BorderBrush="{DynamicResource {x:Static SystemColors.WindowFrameBrushKey}}" BorderThickness="1" Background="{DynamicResource {x:Static SystemColors.WindowBrushKey}}">
        public static Brush DefaultComboboxInsideColorBrush = SystemColors.WindowBrush;

        #endregion

        #region Dark UI options

        public static Brush DarkButtonHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134));


        public static Brush DarkTabItemHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134));

        public static Brush DarkTabItemSelectedBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));


        public static Brush DarkCheckboxHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134));

        public static Brush DarkCheckboxCheckmarkBrush = new SolidColorBrush(Colors.White);


        public static Brush DarkRadioButtonHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134));

        public static Brush DarkRadioButtonCheckmarkBrush = new SolidColorBrush(Colors.White);


        public static Brush DarkComboboxOutsideHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));

        public static Brush DarkComboboxOutsideColorBrush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134));

        public static Brush DarkComboboxInsideColorBrush = new SolidColorBrush(Colors.Gray);
        #endregion

        /// <summary>
        /// The color to use in the selection list for a tab which is not selected
        /// </summary>
        /// <remarks>It starts as null because the color is unknown (and can be different types based on the user's theme).
        /// It is set on user selection on a component in the selection list.</remarks>
        public static Brush NotSelectedTabColor = null;

        private static string parsedFormatVersion = string.Empty;
        #endregion

        #region Custom theme file stuff
        /// <summary>
        /// Load the custom color definitions from XML
        /// </summary>
        public static bool LoadSettings()
        {
            //first check if the file exists
            if(!File.Exists(Settings.UISettingsFileName))
            {
                Logging.Info("UIDocument file does not exist, using defaults");
                return false;
            }

            //try to create a new one first in a temp. If it fails then abort.
            XmlDocument loadedDoc = XmlUtils.LoadXmlDocument(Settings.UISettingsFileName, XmlLoadType.FromFile);
            if(loadedDoc == null)
            {
                Logging.Error("failed to parse UIDocument, check messages above for parsing errors");
                return false;
            }
            UIDocument = loadedDoc;
            Logging.Info("UIDocument xml file loaded successfully, loading custom color instances");

            SetDocumentVersion();
            if (string.IsNullOrWhiteSpace(parsedFormatVersion))
            {
                Logging.Error("UIDocument formatVersion string is null, aborting parsing");
                return false;
            }
            switch (parsedFormatVersion)
            {
                case "1.0":
                    Logging.Info("parsing custom color instances file using V1 parse method");
                    ApplyCustomColorSettingsV1();
                    break;
                default:
                    //unknown
                    Logging.Error("Unknown format string or not supported: {0}", parsedFormatVersion);
                    return false;
            }
            Logging.Info("Custom color instances loaded");
            return true;
        }

        private static void SetDocumentVersion()
        {
            //get the UI xml format version of the file
            string versionXpath = "//" + Settings.UISettingsColorFile + "/@version";
            parsedFormatVersion = XmlUtils.GetXmlStringFromXPath(UIDocument, versionXpath);
            Logging.Debug("using xpath search '{0}' found format version '{1}'", versionXpath, parsedFormatVersion.Trim());
            //trim it
            parsedFormatVersion = parsedFormatVersion.Trim();
        }
        #endregion

        #region Custom color application
        private static void ApplyCustomColorSettingsV1()
        {
            for(int i = 0; i < CustomColorSettings.Count(); i++)
            {
                CustomBrushSetting customBrush = CustomColorSettings[i];
                string instanceName = customBrush.SettingName;
                string customColorSettingXpath = string.Format("//{0}/{1}", Settings.UISettingsColorFile, instanceName);
                Logging.Debug("using xpath {0} to set color of custom property {1}", customColorSettingXpath, instanceName);
                XmlNode customColorNode = XmlUtils.GetXmlNodeFromXPath(UIDocument, customColorSettingXpath);
                if(customColorNode == null)
                {
                    Logging.Info("custom color instance {0} not defined, skipping", instanceName);
                    continue;
                }
                if(ApplyCustomThemeBrushSettings(instanceName,instanceName,customColorNode, out Brush customBrushOut))
                {
                    //customBrush.Brush = customBrushOut;
                    SetBoundedBrushProperty(customBrush, customBrushOut);
                }
                else
                {
                    Logging.Warning("failed to apply color settings for custom instance {0}", instanceName);
                    continue;
                }
            }
        }
        
         /// <summary>
         /// Tries to parse a hex code color to a color object
         /// </summary>
         /// <param name="color">The string hex code for the color to use</param>
         /// <param name="outColor">The corresponding color object</param>
         /// <returns>True if color parsing was successful, a default color otherwise</returns>
         /// <remarks>Uses the 32bit color codes for generation (Alpha, Red, Green, Blue) Alpha is transparency</remarks>
        public static bool ParseColorFromString(string color, out Color outColor)
        {
            outColor = new Color();
            string aPart = string.Empty;
            string rPart = string.Empty;
            string gPart = string.Empty;
            string bPart = string.Empty;
            try
            {
                aPart = color.Substring(1, 2);
                rPart = color.Substring(3, 2);
                gPart = color.Substring(5, 2);
                bPart = color.Substring(7, 2);
            }
            catch (ArgumentException)
            {
                Logging.WriteToLog(string.Format("failed to parse color, a={0}, r={1}, g={2}, b={3}", aPart, rPart, gPart, bPart)
                    , Logfiles.Application, LogLevel.Warning);
                return false;
            }
            if ((byte.TryParse(aPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte a)) &&
                (byte.TryParse(rPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte r)) &&
                (byte.TryParse(gPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte g)) &&
                (byte.TryParse(bPart, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out byte b)))
            {
                outColor = Color.FromArgb(a, r, g, b);
                return true;
            }
            else
                Logging.WriteToLog(string.Format("failed to parse color, a={0}, r={1}, g={2}, b={3}", aPart, rPart, gPart, bPart)
                    , Logfiles.Application, LogLevel.Warning);
            return false;
        }

        /// <summary>
        /// Verifies that the points for applying color gradient directions are within 0-1
        /// </summary>
        /// <param name="p">The color gradient direction to verify</param>
        public static void VerifyPoints(Point p)
        {
            if (p.X > 1 || p.X < 0)
            {
                int settingToUse = p.X > 1 ? 1 : 0;
                Logging.Warning("point.X is out of bounds (must be between 0 and 1, current value={0}), setting to {1})", p.X, settingToUse);
                p.X = settingToUse;
            }
            if (p.Y > 1 || p.Y < 0)
            {
                int settingToUse = p.Y > 1 ? 1 : 0;
                Logging.Warning("point.Y is out of bounds (must be between 0 and 1, current value={0}), setting to {1})", p.Y, settingToUse);
                p.Y = settingToUse;
            }
        }
        #endregion

        #region Other theme stuff
        //https://stackoverflow.com/questions/34762879/static-binding-doesnt-update-when-resource-changes

        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        public static CustomBrushSetting UpdateBrush(CustomBrushSetting brush, Brush newBrush)
        {
            return new CustomBrushSetting(brush.SettingName, newBrush);
        }

        private static void SetBoundedBrushProperty(CustomBrushSetting customBrush, Brush brushToSet)
        {
            typeof(UISettings).GetProperties().Where(prop => prop.Name.Equals(customBrush.SettingName)).ToList()[0].SetValue(null, UpdateBrush(customBrush, brushToSet));
        }

        /// <summary>
        /// Applies custom color settings to a window
        /// </summary>
        /// <param name="window">The Window object to apply color settings to</param>
        public static void ApplyUIColorSettings(Window window)
        {
            if (!BackedUpWindows.ContainsKey(window.GetType().FullName))
            {
                BackupUIDefaultColorSettings(window);
                BackedUpWindows.Add(window.GetType().FullName, window.Background);
            }
            switch (ModpackSettings.ApplicationTheme)
            {
                case UIThemes.Default:
                    Logging.Debug("Applying default UI theme for window {0}", window.GetType().Name);
                    ApplyUIDefaultColorSettings(window);
                    return;
                case UIThemes.Dark:
                    Logging.Debug("Applying dark UI theme for window {0}", window.GetType().Name);
                    ApplyUIDarkColorSettings(window);
                    return;
            }
            if(UIDocument == null)
            {
                Logging.Info("UIDocument is null, no custom color settings to apply");
                return;
            }

            SetDocumentVersion();
            if (string.IsNullOrWhiteSpace(parsedFormatVersion))
            {
                Logging.Error("UIDocument formatVersion string is null, aborting parsing");
                return;
            }

            switch (parsedFormatVersion)
            {
                case "1.0":
                    Logging.Info("parsing color settings file using V1 parse method");
                    ApplyCustomThemeColorsettingsV1(window);
                    break;
                default:
                    //unknown
                    Logging.Error("Unknown format string or not supported: {0}", parsedFormatVersion);
                    return;
            }
        }

        private static void BackupUIDefaultColorSettings(Window window)
        {
            //build list of all internal framework components
            //original: 274
            //after distinct: 267
            List<FrameworkElement> allWindowControls = Utils.GetAllWindowComponentsVisual(window, false).Distinct().ToList();
            foreach (FrameworkElement element in allWindowControls)
            {
                //make sure we have an element that we want color changing
                if (element.Tag is string ID && !string.IsNullOrWhiteSpace(ID))
                {
                    if(element is Control control_)
                    {
                        if (!OriginalColors.ContainsKey(ID))
                        {
                            OriginalColors.Add(ID, new ReplacedBrushes(control_.Background, control_.Foreground));
                        }
                        else
                        {
                            throw new BadMemeException("how does the key already exist");
                        }
                    }
                    else if (element is Panel panel_)
                    {
                        if (!OriginalColors.ContainsKey(ID))
                        {
                            OriginalColors.Add(ID, new ReplacedBrushes(panel_.Background, null));
                        }
                        else
                        {
                            throw new BadMemeException("how does the key already exist");
                        }
                    }
                    else if (element is TextBlock block)
                    {
                        if (!OriginalColors.ContainsKey(ID))
                        {
                            OriginalColors.Add(ID, new ReplacedBrushes(block.Background,block.Foreground));
                        }
                        else
                        {
                            throw new BadMemeException("how does the key already exist");
                        }
                    }
                    else if (element is Border border_)
                    {
                        if (!OriginalColors.ContainsKey(ID))
                        {
                            OriginalColors.Add(ID, new ReplacedBrushes(border_.Background, null));
                        }
                        else
                        {
                            throw new BadMemeException("how does the key already exist");
                        }
                    }
                    else
                    {
                        throw new BadMemeException("what is this");
                    }
                }
            }
        }

        public static void ToggleUIBrushesDarkDefault(bool defaultTheme)
        {
            ButtonHighlightBrush = UpdateBrush(ButtonHighlightBrush, defaultTheme ? DefaultButtonHighlightBrush : DarkButtonHighlightBrush);

            TabItemHighlightBrush = UpdateBrush(TabItemHighlightBrush, defaultTheme ? DefaultTabItemHighlightBrush : DarkTabItemHighlightBrush);
            TabItemSelectedBrush = UpdateBrush(TabItemSelectedBrush, defaultTheme ? DefaultTabItemSelectedBrush : DarkTabItemSelectedBrush);

            CheckboxHighlightBrush = UpdateBrush(CheckboxHighlightBrush, defaultTheme ? DefaultCheckboxHighlightBrush : DarkCheckboxHighlightBrush);
            CheckboxCheckmarkBrush = UpdateBrush(CheckboxCheckmarkBrush, defaultTheme ? DefaultCheckboxCheckmarkBrush : DarkCheckboxCheckmarkBrush);

            RadioButtonHighlightBrush = UpdateBrush(RadioButtonHighlightBrush, defaultTheme ? DefaultRadioButtonHighlightBrush : DarkRadioButtonHighlightBrush);
            RadioButtonCheckmarkBrush = UpdateBrush(RadioButtonCheckmarkBrush, defaultTheme ? DefaultRadioButtonCheckmarkBrush : DarkRadioButtonCheckmarkBrush);

            ComboboxInsideColorBrush = UpdateBrush(ComboboxInsideColorBrush, defaultTheme ? DefaultComboboxInsideColorBrush : DarkComboboxInsideColorBrush);
            ComboboxOutsideColorBrush = UpdateBrush(ComboboxOutsideColorBrush, defaultTheme ? DefaultComboboxOutsideColorBrush : DarkComboboxOutsideColorBrush);
            ComboboxOutsideHighlightBrush = UpdateBrush(ComboboxOutsideHighlightBrush, defaultTheme ? DefaultComboboxOutsideHighlightBrush : DarkComboboxOutsideHighlightBrush);
        }
        #endregion

        #region Default theme color apply
        private static void ApplyUIDefaultColorSettings(Window window)
        {
            string windowType = window.GetType().Name;
            //using RelhaxWindow type allows us to directly control/check if the window should be color changed
            if (window is RelhaxWindow relhaxWindow && !relhaxWindow.ApplyColorSettings)
            {
                Logging.Warning("window of type '{0}' is set to not have color setting applied, skipping", windowType);
                return;
            }

            ApplyDefaultBrushSettings(window);

            //build list of all internal framework components
            List<FrameworkElement> allWindowControls = Utils.GetAllWindowComponentsVisual(window, false).Distinct().ToList();
            foreach (FrameworkElement element in allWindowControls)
            {
                //make sure we have an element that we want color changing
                if (element.Tag is string ID && !string.IsNullOrWhiteSpace(ID))
                {
                    ApplyDefaultBrushSettings(element);
                }
            }

            ToggleUIBrushesDarkDefault(true);
        }

        private static void ApplyDefaultBrushSettings(FrameworkElement element)
        {
            if (element is Window window)
            {
                window.Background = BackedUpWindows[window.GetType().FullName];
                return;
            }

            if(element.Tag is string ID)
            {
                if (!OriginalColors.ContainsKey(ID))
                    throw new BadMemeException("key not found");
            }

            if (element is Button button)
            {
                button.Background = OriginalColors[element.Tag as string].BackgroundBrush;
                button.Foreground = OriginalColors[element.Tag as string].TextBrush;
            }
            else if (element is Control control)
            {
                control.Background = OriginalColors[element.Tag as string].BackgroundBrush;
                control.Foreground = OriginalColors[element.Tag as string].TextBrush;
            }
            else if (element is Panel panel)
            {
                panel.Background = OriginalColors[element.Tag as string].BackgroundBrush;
            }
            else if (element is TextBlock block)
            {
                block.Background = OriginalColors[block.Tag as string].BackgroundBrush;
                block.Foreground = OriginalColors[block.Tag as string].TextBrush;
            }
            else if (element is Border border)
            {
                border.Background = OriginalColors[border.Tag as string].BackgroundBrush;
            }
        }
        #endregion

        #region Dark theme color apply
        private static void ApplyUIDarkColorSettings(Window window)
        {
            string windowType = window.GetType().Name;
            //using RelhaxWindow type allows us to directly control/check if the window should be color changed
            if (window is RelhaxWindow relhaxWindow && !relhaxWindow.ApplyColorSettings)
            {
                Logging.Warning("window of type '{0}' is set to not have color setting applied, skipping", windowType);
                return;
            }

            ApplyDarkBrushSettings(window);

            //build list of all internal framework components
            List<FrameworkElement> allWindowControls = Utils.GetAllWindowComponentsVisual(window, false);
            foreach (FrameworkElement element in allWindowControls)
            {
                //make sure we have an element that we want color changing
                if (element.Tag is string ID && !string.IsNullOrWhiteSpace(ID))
                {
                    ApplyDarkBrushSettings(element);
                }
            }

            ToggleUIBrushesDarkDefault(false);
        }

        private static void ApplyDarkBrushSettings(FrameworkElement element)
        {
            if (element is Window window)
            {
                window.Background = DarkThemeBackground;
            }
            else if (element is Button button)
            {
                if (DarkThemeCustomBrushes.ContainsKey(button.Tag as string))
                {
                    if (DarkThemeCustomBrushes[button.Tag as string].BackgroundBrush != null)
                        button.Background = DarkThemeCustomBrushes[button.Tag as string].BackgroundBrush;
                    if (DarkThemeCustomBrushes[button.Tag as string].TextBrush != null)
                        button.Foreground = DarkThemeCustomBrushes[button.Tag as string].TextBrush;
                }
                else
                {
                    button.Background = DarkThemeButton;
                    button.Foreground = DarkThemeTextColor;
                }
            }
            else if (element is Control control)
            {
                control.Background = DarkThemeBackground;
                control.Foreground = DarkThemeTextColor;
            }
            else if (element is Panel panel)
            {
                panel.Background = DarkThemeBackground;
            }
            else if (element is TextBlock block)
            {
                block.Background = DarkThemeBackground;
                block.Foreground = DarkThemeTextColor;
            }
            else if (element is Border border)
            {
                if (DarkThemeCustomBrushes.ContainsKey(border.Tag as string))
                {
                    border.Background = DarkThemeCustomBrushes[border.Tag as string].BackgroundBrush;
                }
                else
                { 
                    border.Background = DarkThemeBackground;
                }
            }
        }
        #endregion

        #region Custom theme color apply
        /// <summary>
        /// Applies color settings to a window of Xml document format 1.0
        /// </summary>
        /// <param name="window">The window to apply changes to</param>
        private static void ApplyCustomThemeColorsettingsV1(Window window)
        {
            string windowType = window.GetType().Name;
            //using RelhaxWindow type allows us to directly control/check if the window should be color changed
            if (window is RelhaxWindow relhaxWindow && !relhaxWindow.ApplyColorSettings)
            {
                Logging.Warning("window of type '{0}' is set to not have color setting applied, skipping",windowType);
                return;
            }

            //build the xpath string
            //root of the file is the fileName, has an array of elements with each name is the type of the window
            string XpathWindow = string.Format("//{0}/{1}", Settings.UISettingsColorFile,windowType);
            Logging.Debug("using window type xpath string {0}", XpathWindow);
            XmlNode windowColorNode = XmlUtils.GetXmlNodeFromXPath(UIDocument, XpathWindow);

            if(windowColorNode == null)
            {
                Logging.Error("failed to get the window color node using xml search path {0}",XpathWindow);
                return;
            }
            //apply window color settings if exist
            ApplyCustomThemeBrushSettings(window, windowColorNode);

            //build list of all internal framework components
            List<FrameworkElement> allWindowControls = Utils.GetAllWindowComponentsVisual(window, false);
            foreach (FrameworkElement element in allWindowControls)
            {
                //make sure we have an element that we want color changing
                if(element.Tag is string ID && !string.IsNullOrWhiteSpace(ID))
                {
                    //https://msdn.microsoft.com/en-us/library/ms256086(v=vs.110).aspx
                    //get the xpath component
                    string XPathColorSetting = string.Format("//{0}/{1}/ColorSetting[@ID = \"{2}\"]", Settings.UISettingsColorFile, windowType, ID);
                    XmlNode brushSettings = XmlUtils.GetXmlNodeFromXPath(UIDocument, XPathColorSetting);
                    //make sure setting is there
                    if (brushSettings != null)
                        ApplyCustomThemeBrushSettings(element, brushSettings);
                }
            }
        }
        
        private static void ApplyCustomThemeBrushSettings(FrameworkElement element, XmlNode brushSettings)
        {
            if (element is Control control)
            {
                if(ApplyCustomThemeBrushSettings(control.Name, (string)control.Tag, brushSettings, out Brush backgroundColorToChange))
                {
                    if (backgroundColorToChange != null)
                        control.Background = backgroundColorToChange;
                    if (!(element is Window))
                    {
                        if(ApplyCustomThemeTextBrushSettings(control.Name, (string)control.Tag, brushSettings, out Brush textColorToChange))
                            if (textColorToChange != null)
                                control.Foreground = textColorToChange;
                    }
                }
            }
            else if (element is Panel panel)
            {
                if(ApplyCustomThemeBrushSettings(panel.Name, (string)panel.Tag, brushSettings, out Brush backgroundColorToChange))
                {
                    if (backgroundColorToChange != null)
                        panel.Background = backgroundColorToChange;
                }
            }
            else if (element is TextBlock block)
            {
                if(ApplyCustomThemeBrushSettings(block.Name, (string)block.Tag, brushSettings, out Brush backgroundColorToChange))
                {
                    if (backgroundColorToChange != null)
                        block.Background = backgroundColorToChange;
                }
            }
            else if (element is Border border)
            {
                if(ApplyCustomThemeBrushSettings(border.Name, (string)border.Tag, brushSettings, out Brush backgroundColorToChange))
                {
                    if (backgroundColorToChange != null)
                        border.Background = backgroundColorToChange;
                }
            }
        }

        private static bool ApplyCustomThemeTextBrushSettings(string componentName, string componentTag, XmlNode brushSettings, out Brush textColorToChange)
        {
            bool somethingApplied = false;
            textColorToChange = new SolidColorBrush();
            //make sure type is set correctly
            XmlAttribute brushType = brushSettings.Attributes["type"];
            if (brushType == null)
            {
                Logging.Warning("failed to apply brush setting: type attribute not exist!");
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

        private static bool ApplyCustomThemeBrushSettings(string componentName, string componentTag, XmlNode brushSettings, out Brush backgroundColorToChange)
        {
            if (string.IsNullOrEmpty(componentName))
                componentName = "null";

            bool someThingApplied = false;
            backgroundColorToChange = new SolidColorBrush();
            //make sure type is set correctly
            XmlAttribute brushType = brushSettings.Attributes["type"];
            if(brushType == null)
            {
                Logging.Warning("failed to apply brush setting: type attribute not exist!");
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
                    case "null"://no color type
                        backgroundColorToChange = null;
                        someThingApplied = true;
                        break;
                    default:
                        Logging.Warning(string.Format("unknown type parameter{0} in component {1} ", brushType.InnerText, componentTag));
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
        #endregion

        #region Dump to file
        /// <summary>
        /// Saves all currently enabled color settings to an xml file
        /// </summary>
        /// <param name="savePath">The path to save the xml file to</param>
        /// <param name="mainWindow">If method is called from MainWindow, use itself for getting its color properties</param>
        public static void DumpAllWindowColorSettingsToFile(string savePath, MainWindow mainWindow = null)
        {
            //make xml document and declaration
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");

            //append declaration to document
            doc.AppendChild(dec);

            //make root element and version attribute
            XmlElement root = doc.CreateElement(Settings.UISettingsColorFile);

            //NOTE: version attribute should be incremented when large change in color loading structure is done
            //allows us to make whole new method to load UI settings
            XmlAttribute version = doc.CreateAttribute("version");

            //append to document
            version.Value = "1.0";
            root.Attributes.Append(version);
            doc.AppendChild(root);
            
            //add all window instances to document:
            //MainWindow first
            DumpWindowColorSettingsToXml(root, doc, mainWindow == null? new MainWindow() : mainWindow);
            
            //get list of all windows in "Windows" namespace
            //https://stackoverflow.com/questions/79693/getting-all-types-in-a-namespace-via-reflection
            List<Type> windows = Assembly.GetExecutingAssembly().GetTypes().ToList();
            //only get actual windows where attributes is public 
            windows = windows.Where(type => 
                type.IsClass &&
                !string.IsNullOrEmpty(type.Namespace) &&
                type.Namespace.Contains("RelhaxModpack.Windows") &&
                type.Attributes == (TypeAttributes.Public | TypeAttributes.BeforeFieldInit) &&
                type.BaseType.FullName.Contains("RelhaxModpack.Windows")
                ).ToList();

            //create instances and save them
            Window window;
            foreach (Type type in windows)
            {
                Logging.Debug("creating windows instance for UI parsing: {0}", type.Name);
                try
                {
                    window = (Window)Activator.CreateInstance(type);
                }
                catch (Exception ex)
                {
                    Logging.Exception("Failed to create window instance of type {0}", type.Name);
                    Logging.Exception(ex.ToString());
                    continue;
                }
                Logging.Debug("parsing configurable UI elements");
                DumpWindowColorSettingsToXml(root, doc, window);
                Logging.Debug("parsing complete, cleanup");
            }

            //save custom color settings to document
            //for now, just use single solid color for these settings
            foreach (CustomBrushSetting customBrush in CustomColorSettings)
            {
                string name = customBrush.SettingName;
                Logging.Debug("saving custom color SolidColorBrush element {0}", name);
                XmlElement element = doc.CreateElement(name);
                XmlAttribute color = doc.CreateAttribute("color1");
                //color1.Value = solidColorBrush.Color.ToString(CultureInfo.InvariantCulture);
                color.Value = (customBrush.Brush as SolidColorBrush).Color.ToString(CultureInfo.InvariantCulture);
                element.Attributes.Append(color);
                root.AppendChild(element);
            }

            //save xml file
            doc.Save(savePath);
        }

        private static void DumpWindowColorSettingsToXml(XmlElement root, XmlDocument doc, Window window)
        {
            string windowType = window.GetType().Name;
            //check to make sure we want to save it
            if (window is RelhaxWindow relhaxWindow && !relhaxWindow.ApplyColorSettings)
            {
                Logging.Info("skipping window {0}, not allowed to apply custom color settings",windowType);
                return;
            }
            //make window element
            XmlElement windowElement = doc.CreateElement(windowType);

            //save attributes to element
            ApplyColorattributesToElement(windowElement, window.Background, doc);

            //same to root
            root.AppendChild(windowElement);

            //get list of all framework elements in the window
            List<FrameworkElement> AllUIElements = Utils.GetAllWindowComponentsLogical(window, false);
            AllUIElements = AllUIElements.Where(element => element.Tag != null && element.Tag is string ID && !string.IsNullOrWhiteSpace(ID)).ToList();

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
            else if (brush == null)
            {
                colorType = doc.CreateAttribute("type");
                colorType.Value = "null";
                colorEntry.Attributes.Append(colorType);
            }
            else
            {
                Logging.WriteToLog("Unknown background type: " + brush.GetType().ToString(), Logfiles.Application, LogLevel.Debug);
            }
            if(textBrush != null)
            {
                //text color (foreground)
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