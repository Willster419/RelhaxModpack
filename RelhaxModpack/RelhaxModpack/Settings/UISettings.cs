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
using System.Windows.Data;
using RelhaxModpack.UIComponents;

namespace RelhaxModpack
{
    /// <summary>
    /// Handles all custom UI settings
    /// </summary>
    public static class UISettings
    {
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
        /// The color to use in the selection list for a tab which is not selected
        /// </summary>
        /// <remarks>It starts as null because the color is unknown (and can be different types based on the user's theme).
        /// It is set on user selection on a component in the selection list.</remarks>
        public static Brush NotSelectedTabColor = null;

        private static Theme currentTheme = Themes.Default;
        public static Theme CurrentTheme
        {
            get { return currentTheme; }
            set
            {
                currentTheme = value;
                foreach(CustomBrush customBrush in currentTheme.BoundedClassColorsetBrushes)
                {
                    BoundUISettings.SetBrushProperty(customBrush);
                }
            }
        }

        private static string parsedFormatVersion = string.Empty;

        private static bool isDefaultThemeBackedUp = false;

        #region Apply theme to window
        public static void ApplyCustomStyles(Window window)
        {
            //get the list
            List<FrameworkElement> UIComponents = Utils.GetAllWindowComponentsVisual(window, false);
            UIComponents = UIComponents.Where(component => component.Tag is string ID && !string.IsNullOrEmpty(ID)).ToList();
            foreach (FrameworkElement element in UIComponents)
            {
                if (element is Button button)
                {
                    //https://stackoverflow.com/questions/1754615/how-to-assign-a-dynamic-resource-style-in-code
                    button.Style = (Style)Application.Current.Resources["RelhaxButtonStyle"];
                    //button.SetResourceReference(Button.StyleProperty, "RelhaxButtonStyle");
                }
                else if (element is CheckBox checkbox)
                {
                    checkbox.Style = (Style)Application.Current.Resources["RelhaxCheckboxStyle"];
                }
                else if (element is RadioButton radioButton)
                {
                    radioButton.Style = (Style)Application.Current.Resources["RelhaxRadioButtonStyle"];
                }
                else if (element is ComboBox combobox)
                {
                    combobox.Style = (Style)Application.Current.Resources["RelhaxComboboxStyle"];
                }
                else if (element is TabItem tabItem)
                {
                    tabItem.Style = (Style)Application.Current.Resources["RelhaxTabItemStyle"];
                }
            }
        }

        /// <summary>
        /// Applies custom color settings to a window
        /// </summary>
        /// <param name="window">The Window object to apply color settings to</param>
        public static void ApplyUIColorSettings(Window window)
        {
            if (!isDefaultThemeBackedUp)
            {
                BackupDefaultThemeColorSettings();
                isDefaultThemeBackedUp = true;
            }

            //if the colorset object does not exist for this window in the theme, then make it
            WindowColorset windowColorset = null;
            if(Themes.Default.WindowColorsets.ContainsKey(window.GetType()))
            {
                windowColorset = Themes.Default.WindowColorsets[window.GetType()];
            }
            else
            {
                windowColorset = new WindowColorset()
                {
                    WindowType = window.GetType(),
                    BackgroundBrush = new CustomBrush()
                    {
                        IsBound = false,
                        IsValid = true,
                        Brush = window.Background
                    },
                    ColorsetBackedUp = false,
                    ComponentColorsets = new Dictionary<string, ComponentColorset>()
                };
                Themes.Default.WindowColorsets.Add(window.GetType(), windowColorset);
            }

            //if the window colorset objects are not backup for this window of this theme, then back them up
            if(!windowColorset.ColorsetBackedUp)
            {
                BackupDefaultComponentColorSettings(window);
                windowColorset.ColorsetBackedUp = true;
            }

            switch (ModpackSettings.ApplicationTheme)
            {
                case UIThemes.Default:
                    Logging.Debug("Applying default UI theme for window {0}", window.GetType().Name);
                    CurrentTheme = Themes.Default;
                    ApplyThemeToWindow(window);
                    return;
                case UIThemes.Dark:
                    Logging.Debug("Applying dark UI theme for window {0}", window.GetType().Name);
                    CurrentTheme = Themes.Dark;
                    ApplyThemeToWindow(window);
                    return;
            }
            if(UIDocument == null)
            {
                Logging.Info("UIDocument is null, no custom color settings to apply");
                return;
            }

            GetDocumentVersion();
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

        private static void ApplyThemeToWindow(Window window)
        {
            string windowType = window.GetType().Name;
            //using RelhaxWindow type allows us to directly control/check if the window should be color changed
            if (window is RelhaxWindow relhaxWindow && !relhaxWindow.ApplyColorSettings)
            {
                Logging.Warning("Window of type '{0}' is set to not have color setting applied, skipping", windowType);
                return;
            }

            //apply theme to window
            Logging.Info("Applying theme: {0}", CurrentTheme.ThemeName);
            bool customWindowDefinition = CurrentTheme.WindowColorsets.ContainsKey(window.GetType());
            if (customWindowDefinition)
            {
                if (CurrentTheme.WindowColorsets[window.GetType()].BackgroundBrush.IsValid)
                    window.Background = CurrentTheme.WindowColorsets[window.GetType()].BackgroundBrush.Brush;
            }

            //build list of all internal framework components
            List<FrameworkElement> allWindowControls = Utils.GetAllWindowComponentsVisual(window, false).Distinct().ToList();
            allWindowControls = allWindowControls.Where(element => element.Tag is string ID && !string.IsNullOrWhiteSpace(ID)).ToList();

            //apply all class level color sets
            foreach (FrameworkElement element in allWindowControls)
            {
                //don't apply the background or foreground if the component is disabled. so, if it is disabled, enable it, apply the theme, then disabled it again
                bool componentWasDisabled = !element.IsEnabled;
                if (componentWasDisabled)
                    element.IsEnabled = true;

                ClassColorset colorset = null;
                if (element is RadioButton radioButton)
                {
                    //button.SetCurrentValue(Button.BackgroundProperty, OriginalColors[element.Tag as string].BackgroundBrush);
                    colorset = CurrentTheme.CheckboxColorset;

                    if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                        radioButton.SetCurrentValue(RadioButton.BackgroundProperty, colorset.BackgroundBrush.Brush);

                    if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid && !colorset.ForegroundBrush.IsBound)
                        radioButton.SetCurrentValue(RadioButton.ForegroundProperty, colorset.ForegroundBrush.Brush);
                }
                else if (element is CheckBox checkbox)
                {
                    colorset = CurrentTheme.CheckboxColorset;

                    if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                        checkbox.SetCurrentValue(CheckBox.BackgroundProperty, colorset.BackgroundBrush.Brush);

                    if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid && !colorset.ForegroundBrush.IsBound)
                        checkbox.SetCurrentValue(CheckBox.ForegroundProperty, colorset.ForegroundBrush.Brush);
                }
                else if (element is Button button)
                {
                    colorset = CurrentTheme.ButtonColorset;

                    if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                        button.SetCurrentValue(Button.BackgroundProperty, colorset.BackgroundBrush.Brush);

                    if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid && !colorset.ForegroundBrush.IsBound)
                        button.SetCurrentValue(Button.ForegroundProperty, colorset.ForegroundBrush.Brush);
                }
                else if (element is TabItem tabItem)
                {
                    colorset = CurrentTheme.TabItemColorset;

                    if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                        tabItem.SetCurrentValue(TabItem.BackgroundProperty, colorset.BackgroundBrush.Brush);

                    if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid && !colorset.ForegroundBrush.IsBound)
                        tabItem.SetCurrentValue(TabItem.ForegroundProperty, colorset.ForegroundBrush.Brush);
                }
                else if (element is ComboBox combobox)
                {
                    colorset = CurrentTheme.ComboboxColorset;
                    //nothing to apply, it's all bounded
                }
                else if (element is Panel panel)
                {
                    colorset = CurrentTheme.PanelColorset;

                    if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                        panel.SetCurrentValue(Panel.BackgroundProperty, colorset.BackgroundBrush.Brush);
                }
                else if (element is TextBlock textblock)
                {
                    colorset = CurrentTheme.TextblockColorset;

                    if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                        textblock.SetCurrentValue(TextBlock.BackgroundProperty, colorset.BackgroundBrush.Brush);

                    if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid && !colorset.ForegroundBrush.IsBound)
                        textblock.SetCurrentValue(TextBlock.ForegroundProperty, colorset.ForegroundBrush.Brush);
                }
                else if (element is Border border)
                {
                    colorset = CurrentTheme.BorderColorset;

                    if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                        border.SetCurrentValue(Border.BackgroundProperty, colorset.BackgroundBrush.Brush);
                }
                else if (element is Control control)
                {
                    colorset = CurrentTheme.ControlColorset;

                    if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                        control.SetCurrentValue(Control.BackgroundProperty, colorset.BackgroundBrush.Brush);

                    if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid && !colorset.ForegroundBrush.IsBound)
                        control.SetCurrentValue(Control.ForegroundProperty, colorset.ForegroundBrush.Brush);
                }
                else
                    throw new BadMemeException("what the hell is this control");

                //re-disable if it was originally disabled
                if (componentWasDisabled)
                    element.IsEnabled = false;
            }

            //apply component level color sets
            if (customWindowDefinition)
            {
                WindowColorset windowColorset = CurrentTheme.WindowColorsets[window.GetType()];
                Dictionary<string, ComponentColorset> customDictionaries = windowColorset.ComponentColorsets;
                if (customDictionaries != null)
                {
                    foreach (FrameworkElement element in allWindowControls)
                    {
                        //check if it exists in the list
                        string ID = element.Tag as string;
                        if(customDictionaries.ContainsKey(ID))
                        {
                            //apply it based on class type
                            ComponentColorset colorset = customDictionaries[ID];
                            ApplyThemeToComponent(element, colorset);
                        }
                    }
                }
                else
                    Logging.Debug("customDictionaries for windowColorset of theme '{0}' is null, cannot apply custom component color themes", currentTheme.ThemeName);
            }
        }

        private static void ApplyThemeToComponent(FrameworkElement element, ComponentColorset colorset)
        {
            if (element is RadioButton radioButton)
            {
                if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                    radioButton.SetCurrentValue(RadioButton.BackgroundProperty, colorset.BackgroundBrush.Brush);

                if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid && !colorset.ForegroundBrush.IsBound)
                    radioButton.SetCurrentValue(RadioButton.ForegroundProperty, colorset.ForegroundBrush.Brush);
            }
            else if (element is CheckBox checkbox)
            {
                if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                    checkbox.SetCurrentValue(CheckBox.BackgroundProperty, colorset.BackgroundBrush.Brush);

                if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid && !colorset.ForegroundBrush.IsBound)
                    checkbox.SetCurrentValue(CheckBox.ForegroundProperty, colorset.ForegroundBrush.Brush);
            }
            else if (element is Button button)
            {
                if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                    button.SetCurrentValue(Button.BackgroundProperty, colorset.BackgroundBrush.Brush);

                if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid && !colorset.ForegroundBrush.IsBound)
                    button.SetCurrentValue(Button.ForegroundProperty, colorset.ForegroundBrush.Brush);
            }
            else if (element is TabItem tabItem)
            {
                if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                    tabItem.SetCurrentValue(TabItem.BackgroundProperty, colorset.BackgroundBrush.Brush);

                if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid && !colorset.ForegroundBrush.IsBound)
                    tabItem.SetCurrentValue(TabItem.ForegroundProperty, colorset.ForegroundBrush.Brush);
            }
            else if (element is ComboBox combobox)
            {
                //nothing to apply, it's all bounded
            }
            else if (element is Panel panel)
            {
                if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                    panel.SetCurrentValue(Panel.BackgroundProperty, colorset.BackgroundBrush.Brush);
            }
            else if (element is TextBlock textblock)
            {
                if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                    textblock.SetCurrentValue(TextBlock.BackgroundProperty, colorset.BackgroundBrush.Brush);

                if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid && !colorset.ForegroundBrush.IsBound)
                    textblock.SetCurrentValue(TextBlock.ForegroundProperty, colorset.ForegroundBrush.Brush);
            }
            else if (element is Border border)
            {
                if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                    border.SetCurrentValue(Border.BackgroundProperty, colorset.BackgroundBrush.Brush);
            }
            else if (element is Control control)
            {
                if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid && !colorset.BackgroundBrush.IsBound)
                    control.SetCurrentValue(Control.BackgroundProperty, colorset.BackgroundBrush.Brush);

                if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid && !colorset.ForegroundBrush.IsBound)
                    control.SetCurrentValue(Control.ForegroundProperty, colorset.ForegroundBrush.Brush);
            }
            else
                throw new BadMemeException("what the hell is this control");
        }
        #endregion

        #region Backup of default theme runtime
        private static void BackupDefaultThemeColorSettings()
        {
            //make an instance of mainWindow to get the default component colors
            //note control is not backed up, because it is so generic that it should not have a default
            //at the theme applying level, this would be from a tag (not class) level. but this can change between themes
            MainWindow win = new MainWindow();

            List<FrameworkElement> mainWindowComponents = Utils.GetAllWindowComponentsLogical(win, false).Distinct().ToList();

            Button b = mainWindowComponents.First( element => element is Button) as Button;
            Themes.Default.ButtonColorset.BackgroundBrush = new CustomBrush()
            {
                IsBound = false,
                IsValid = true,
                Brush = b.Background
            };
            Themes.Default.ButtonColorset.ForegroundBrush = new CustomBrush()
            {
                IsBound = false,
                IsValid = true,
                Brush = b.Foreground
            };

            TabItem ti = mainWindowComponents.First(element => element is TabItem) as TabItem;
            Themes.Default.TabItemColorset.BackgroundBrush = new CustomBrush()
            {
                IsBound = false,
                IsValid = true,
                Brush = ti.Background
            };
            Themes.Default.TabItemColorset.ForegroundBrush = new CustomBrush()
            {
                IsBound = false,
                IsValid = true,
                Brush = ti.Foreground
            };

            CheckBox cb = mainWindowComponents.First(element => element is CheckBox) as CheckBox;
            Themes.Default.CheckboxColorset.BackgroundBrush = new CustomBrush()
            {
                IsBound = false,
                IsValid = true,
                Brush = cb.Background
            };
            Themes.Default.CheckboxColorset.ForegroundBrush = new CustomBrush()
            {
                IsBound = false,
                IsValid = true,
                Brush = cb.Foreground
            };

            RadioButton rb = mainWindowComponents.First(element => element is RadioButton) as RadioButton;
            Themes.Default.RadioButtonColorset.BackgroundBrush = new CustomBrush()
            {
                IsBound = false,
                IsValid = true,
                Brush = rb.Background
            };
            Themes.Default.RadioButtonColorset.ForegroundBrush = new CustomBrush()
            {
                IsBound = false,
                IsValid = true,
                Brush = rb.Foreground
            };

            TextBlock textBlock = mainWindowComponents.First(element => element is TextBlock) as TextBlock;
            Themes.Default.TextblockColorset.BackgroundBrush = new CustomBrush()
            {
                IsBound = false,
                IsValid = true,
                Brush = textBlock.Background
            };
            Themes.Default.TextblockColorset.ForegroundBrush = new CustomBrush()
            {
                IsBound = false,
                IsValid = true,
                Brush = textBlock.Foreground
            };

            Border border = mainWindowComponents.First(element => element is Border) as Border;
            Themes.Default.BorderColorset.BackgroundBrush = new CustomBrush()
            {
                IsBound = false,
                IsValid = true,
                Brush = border.Background
            };

            Panel panel = mainWindowComponents.First(element => element is Panel) as Panel;
            Themes.Default.PanelColorset.BackgroundBrush = new CustomBrush()
            {
                IsBound = false,
                IsValid = true,
                Brush = panel.Background
            };
        }

        private static void BackupDefaultComponentColorSettings(Window window)
        {
            //backup the component colorsettings (per window)
            List<FrameworkElement> allWindowControls = Utils.GetAllWindowComponentsVisual(window, false).Distinct().ToList();
            allWindowControls = allWindowControls.Where(element => element.Tag is string ID && !string.IsNullOrWhiteSpace(ID)).ToList();

            WindowColorset windowColorset = Themes.Default.WindowColorsets[window.GetType()];
            Dictionary<string, ComponentColorset> windowDictionary = windowColorset.ComponentColorsets;
            foreach (FrameworkElement element in allWindowControls)
            {
                string ID = element.Tag as string;

                //check if the key already exists first
                if (windowDictionary.ContainsKey(ID))
                    throw new BadMemeException("how does the key already exist");

                //a element that is disabled will have a different current foreground and background applied to it
                //get around this by temporarily enabling the component, then disabling it
                bool triggered = false;
                if (!element.IsEnabled)
                {
                    triggered = true;
                    element.IsEnabled = true;
                }

                if (element is Control control)
                {
                    windowDictionary.Add(ID, new ComponentColorset()
                    {
                        ID = ID,
                        BackgroundBrush = new CustomBrush() { IsValid = true, Brush = control.Background },
                        ForegroundBrush = new CustomBrush() { IsValid = true, Brush = control.Foreground }
                    });
                }
                else if (element is Panel panel)
                {
                    windowDictionary.Add(ID, new ComponentColorset()
                    {
                        ID = ID,
                        BackgroundBrush = new CustomBrush() { IsValid = true, Brush = panel.Background },
                        ForegroundBrush = new CustomBrush()
                    });
                }
                else if (element is TextBlock block)
                {
                    windowDictionary.Add(ID, new ComponentColorset()
                    {
                        ID = ID,
                        BackgroundBrush = new CustomBrush() { IsValid = true, Brush = block.Background },
                        ForegroundBrush = new CustomBrush() { IsValid = true, Brush = block.Foreground }
                    });
                }
                else if (element is Border border)
                {
                    windowDictionary.Add(ID, new ComponentColorset()
                    {
                        ID = ID,
                        BackgroundBrush = new CustomBrush() { IsValid = true, Brush = border.Background },
                        ForegroundBrush = new CustomBrush()
                    });
                }
                else
                {
                    throw new BadMemeException("what is this");
                }

                //and set it back
                if (triggered)
                {
                    element.IsEnabled = false;
                }
            }
            windowColorset.ColorsetBackedUp = true;
        }
        #endregion

        #region Custom theme apply to window
        private static void ApplyCustomColorSettingsV1()
        {
            throw new NotImplementedException();
            /*
            for (int i = 0; i < CustomColorSettings.Count(); i++)
            {
                CustomBrushSetting customBrush = CustomColorSettings[i];
                string instanceName = customBrush.SettingName;
                string customColorSettingXpath = string.Format("//{0}/{1}", Settings.UISettingsColorFile, instanceName);
                Logging.Debug("using xpath {0} to set color of custom property {1}", customColorSettingXpath, instanceName);
                XmlNode customColorNode = XmlUtils.GetXmlNodeFromXPath(UIDocument, customColorSettingXpath);
                if (customColorNode == null)
                {
                    Logging.Info("custom color instance {0} not defined, skipping", instanceName);
                    continue;
                }
                if (ApplyCustomThemeBrushSettings(instanceName, instanceName, customColorNode, out Brush customBrushOut))
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
            */
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
                Logging.Warning("window of type '{0}' is set to not have color setting applied, skipping", windowType);
                return;
            }

            //build the xpath string
            //root of the file is the fileName, has an array of elements with each name is the type of the window
            string XpathWindow = string.Format("//{0}/{1}", Settings.UISettingsColorFile, windowType);
            Logging.Debug("using window type xpath string {0}", XpathWindow);
            XmlNode windowColorNode = XmlUtils.GetXmlNodeFromXPath(UIDocument, XpathWindow);

            if (windowColorNode == null)
            {
                Logging.Error("failed to get the window color node using xml search path {0}", XpathWindow);
                return;
            }
            //apply window color settings if exist
            ApplyCustomThemeBrushSettings(window, windowColorNode);

            //build list of all internal framework components
            List<FrameworkElement> allWindowControls = Utils.GetAllWindowComponentsVisual(window, false);
            foreach (FrameworkElement element in allWindowControls)
            {
                //make sure we have an element that we want color changing
                if (element.Tag is string ID && !string.IsNullOrWhiteSpace(ID))
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
                if (ApplyCustomThemeBrushSettings(control.Name, (string)control.Tag, brushSettings, out Brush backgroundColorToChange))
                {
                    if (backgroundColorToChange != null)
                        control.Background = backgroundColorToChange;
                    if (!(element is Window))
                    {
                        if (ApplyCustomThemeTextBrushSettings(control.Name, (string)control.Tag, brushSettings, out Brush textColorToChange))
                            if (textColorToChange != null)
                                control.Foreground = textColorToChange;
                    }
                }
            }
            else if (element is Panel panel)
            {
                if (ApplyCustomThemeBrushSettings(panel.Name, (string)panel.Tag, brushSettings, out Brush backgroundColorToChange))
                {
                    if (backgroundColorToChange != null)
                        panel.Background = backgroundColorToChange;
                }
            }
            else if (element is TextBlock block)
            {
                if (ApplyCustomThemeBrushSettings(block.Name, (string)block.Tag, brushSettings, out Brush backgroundColorToChange))
                {
                    if (backgroundColorToChange != null)
                        block.Background = backgroundColorToChange;
                }
            }
            else if (element is Border border)
            {
                if (ApplyCustomThemeBrushSettings(border.Name, (string)border.Tag, brushSettings, out Brush backgroundColorToChange))
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
                if (ParseColorFromString(textColor.InnerText, out Color color))
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
            if (brushType == null)
            {
                Logging.Warning("failed to apply brush setting: type attribute not exist!");
                return false;
            }
            //https://docs.microsoft.com/en-us/dotnet/api/system.windows.media.color.fromargb?view=netframework-4.7.2#System_Windows_Media_Color_FromArgb_System_Byte_System_Byte_System_Byte_System_Byte_
            XmlAttribute color1 = brushSettings.Attributes["color1"];
            XmlAttribute color2 = brushSettings.Attributes["color2"];
            XmlAttribute point1 = brushSettings.Attributes["point1"];
            XmlAttribute point2 = brushSettings.Attributes["point2"];
            if (color1 != null)
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

        #region Custom theme file load
        /// <summary>
        /// Load the custom color definitions from XML
        /// </summary>
        public static bool LoadSettings()
        {
            //first check if the file exists
            if (!File.Exists(Settings.UISettingsFileName))
            {
                Logging.Info("UIDocument file does not exist, using defaults");
                return false;
            }

            //try to create a new one first in a temp. If it fails then abort.
            XmlDocument loadedDoc = XmlUtils.LoadXmlDocument(Settings.UISettingsFileName, XmlLoadType.FromFile);
            if (loadedDoc == null)
            {
                Logging.Error("failed to parse UIDocument, check messages above for parsing errors");
                return false;
            }
            UIDocument = loadedDoc;
            Logging.Info("UIDocument xml file loaded successfully, loading custom color instances");

            GetDocumentVersion();
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

        private static void GetDocumentVersion()
        {
            //get the UI xml format version of the file
            string versionXpath = "//" + Settings.UISettingsColorFile + "/@version";
            parsedFormatVersion = XmlUtils.GetXmlStringFromXPath(UIDocument, versionXpath);
            Logging.Debug("using xpath search '{0}' found format version '{1}'", versionXpath, parsedFormatVersion.Trim());
            //trim it
            parsedFormatVersion = parsedFormatVersion.Trim();
        }
        #endregion

        #region Custom theme file save
        /// <summary>
        /// Saves all currently enabled color settings to an xml file
        /// </summary>
        /// <param name="savePath">The path to save the xml file to</param>
        /// <param name="mainWindow">If method is called from MainWindow, use itself for getting its color properties</param>
        public static void DumpAllWindowColorSettingsToFile(string savePath, MainWindow mainWindow = null)
        {
            throw new NotImplementedException();
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
            /*
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
            */

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