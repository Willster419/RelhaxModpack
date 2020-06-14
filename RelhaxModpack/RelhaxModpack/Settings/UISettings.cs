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
using RelhaxModpack.UI;
using RelhaxModpack.UI.ClassThemeDefinitions;
using RelhaxModpack.Xml;
using RelhaxModpack.Utilities.Enums;

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
        /// The color to use in the selection list for a tab which is not selected
        /// </summary>
        /// <remarks>It starts as null because the color is unknown (and can be different types based on the user's theme).
        /// It is set on user selection on a component in the selection list.</remarks>
        public static Brush NotSelectedTabColor = null;

        /// <summary>
        /// A list of brush names that are allowed to have 'null' as the value for global brushes (global = at root of Theme object)
        /// </summary>
        private static readonly string[] NullAllowedGlobalBrushes = new string[]
        {
            "SelectionListNotActiveHasNoSelectionsBackgroundColor"
        };

        private static Theme currentTheme = Themes.Default;

        /// <summary>
        /// The currently applied theme in the UI engine
        /// </summary>
        public static Theme CurrentTheme
        {
            get { return currentTheme; }
            set
            {
                currentTheme = value;
                foreach(CustomPropertyBrush customBrush in currentTheme.BoundedClassColorsetBrushes)
                {
                    BoundUISettings.SetBrushProperty(customBrush);
                }
            }
        }

        private static string parsedFormatVersion = string.Empty;

        private static bool isDefaultThemeBackedUp = false;

        #region Apply theme to window
        /// <summary>
        /// Applies the custom style templates for a UI component class type when an ID is defined.
        /// This allows for custom color definitions to be loaded.
        /// </summary>
        /// <param name="window">The Window to apply the styles to</param>
        public static void ApplyCustomStyles(Window window)
        {
            //get the list
            List<FrameworkElement> UI = UiUtils.GetAllWindowComponentsLogical(window, false);
            UI = UI.Where(component => component.Tag is string ID && !string.IsNullOrEmpty(ID)).ToList();
            foreach (FrameworkElement element in UI)
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

            //if the colorset object does not exist for this window in the default theme, then make it
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
                    //only apply it if need be so that we're not constantly setting the bindings
                    if(!CurrentTheme.Equals(Themes.Default))
                        CurrentTheme = Themes.Default;
                    ApplyThemeToWindow(window);
                    return;
                case UIThemes.Dark:
                    Logging.Debug("Applying dark UI theme for window {0}", window.GetType().Name);
                    if (!CurrentTheme.Equals(Themes.Dark))
                        CurrentTheme = Themes.Dark;
                    ApplyThemeToWindow(window);
                    return;
                case UIThemes.Custom:
                    Logging.Debug("Applying custom UI theme for window {0}", window.GetType().Name);
                    bool themewasNullDisplayMessage = false;
                    if(Themes.Custom == null)
                    {
                        Logging.Info("Custom theme definition is null, load from xml file");
                        if (!LoadSettingsFile())
                        {
                            Logging.Info("Custom theme load failed. Enable verbose logging for more information");
                            MessageBox.Show(Translations.GetTranslatedString("failedToParseUISettingsFile"));
                            return;
                        }
                        else
                        {
                            themewasNullDisplayMessage = true;
                        }
                    }
                    if (!CurrentTheme.Equals(Themes.Custom))
                        CurrentTheme = Themes.Custom;
                    ApplyThemeToWindow(window);
                    if(themewasNullDisplayMessage)
                    {
                        Logging.Info("Custom theme loaded successfully");
                        MessageBox.Show(Translations.GetTranslatedString("UISettingsFileApplied"));
                    }
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
                {
                    //window.Background = CurrentTheme.WindowColorsets[window.GetType()].BackgroundBrush.Brush;
                    window.SetCurrentValue(Window.BackgroundProperty, CurrentTheme.WindowColorsets[window.GetType()].BackgroundBrush.Brush);
                }
            }
            ApplyThemeToRootComponent(window, customWindowDefinition);
        }

        /// <summary>
        /// Applies theme properties to UI components based on colorDefinition rule sets for that UI component class type
        /// </summary>
        /// <param name="rootElement">The element to start at for applying theme properties</param>
        /// <param name="customWindowDefinition">To determine if to apply custom theme properties based on a list of custom theme properties for that window</param>
        /// <param name="wcolorset">The property object of colorDefinition rules</param>
        /// <param name="includeSelf">Determine if applying the rootElement</param>
        public static void ApplyThemeToRootComponent(FrameworkElement rootElement, bool customWindowDefinition, WindowColorset wcolorset = null, bool includeSelf = false)
        {
            //build list of all internal framework components
            List<FrameworkElement> allWindowControls = UiUtils.GetAllWindowComponentsLogical(rootElement, includeSelf).Distinct().ToList();
            allWindowControls = allWindowControls.Where(element => element.Tag is string ID && !string.IsNullOrWhiteSpace(ID)).ToList();

            //apply all class level color sets
            foreach (FrameworkElement element in allWindowControls)
            {
                //don't apply the background or foreground if the component is disabled. so, if it is disabled, enable it, apply the theme, then disabled it again
                bool componentWasDisabled = !element.IsEnabled;
                if (componentWasDisabled)
                    element.IsEnabled = true;

                ClassColorset colorset = DetermineClassColorSet(element);

                if (colorset.ClassThemeDefinition.BackgroundAllowed && string.IsNullOrEmpty(colorset.ClassThemeDefinition.BackgroundBoundName))
                {
                    if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid)
                    {
                        //if highlight and selected aren't allowed, then set via local
                        if (colorset.ClassThemeDefinition.BackgroundAppliedLocal)
                            element.GetType().GetProperty("Background").SetValue(element, colorset.BackgroundBrush.Brush);
                        //else set via currentValue
                        else
                            element.SetCurrentValue(colorset.ClassThemeDefinition.BackgroundDependencyProperty, colorset.BackgroundBrush.Brush);
                    }
                }

                if (colorset.ClassThemeDefinition.ForegroundAllowed && string.IsNullOrEmpty(colorset.ClassThemeDefinition.ForegroundBoundName))
                {
                    if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid)
                    {
                        if (colorset.ClassThemeDefinition.ForegroundAppliedLocal)
                            element.GetType().GetProperty("Foreground").SetValue(element, colorset.ForegroundBrush.Brush);
                        else
                        {
                            element.SetCurrentValue(colorset.ClassThemeDefinition.ForegroundDependencyProperty, colorset.ForegroundBrush.Brush);
                            if (element is Button button)
                            {
                                button.Resources["testResource"] = colorset.ForegroundBrush.Brush;
                            }
                        }
                    }
                }

                //re-disable if it was originally disabled
                if (componentWasDisabled)
                    element.IsEnabled = false;
            }

            //apply component level color sets
            if (customWindowDefinition)
            {
                WindowColorset windowColorset = null;
                if (wcolorset != null)
                    windowColorset = wcolorset;
                else
                    windowColorset = CurrentTheme.WindowColorsets[rootElement.GetType()];
                Dictionary<string, ComponentColorset> customDictionaries = windowColorset.ComponentColorsets;
                if (customDictionaries != null)
                {
                    foreach (FrameworkElement element in allWindowControls)
                    {
                        //check if it exists in the list
                        string ID = element.Tag as string;
                        if (customDictionaries.ContainsKey(ID))
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

        private static void ApplyThemeToComponent(FrameworkElement element, ComponentColorset componentColorSet)
        {
            //don't apply the background or foreground if the component is disabled. so, if it is disabled, enable it, apply the theme, then disabled it again
            bool componentWasDisabled = !element.IsEnabled;
            if (componentWasDisabled)
                element.IsEnabled = true;

            ClassColorset colorset = DetermineClassColorSet(element);

            if (colorset.ClassThemeDefinition.BackgroundAllowed && string.IsNullOrEmpty(colorset.ClassThemeDefinition.BackgroundBoundName))
            {
                if (componentColorSet.BackgroundBrush != null && componentColorSet.BackgroundBrush.IsValid)
                {
                    //if highlight and selected aren't allowed, then set via local
                    if (colorset.ClassThemeDefinition.BackgroundAppliedLocal)
                        element.GetType().GetProperty("Background").SetValue(element, componentColorSet.BackgroundBrush.Brush);
                    //else set via currentValue
                    else
                        element.SetCurrentValue(colorset.ClassThemeDefinition.BackgroundDependencyProperty, componentColorSet.BackgroundBrush.Brush);
                }
            }

            if (colorset.ClassThemeDefinition.ForegroundAllowed && string.IsNullOrEmpty(colorset.ClassThemeDefinition.ForegroundBoundName))
            {
                if (componentColorSet.ForegroundBrush != null && componentColorSet.ForegroundBrush.IsValid)
                {
                    if (colorset.ClassThemeDefinition.ForegroundAppliedLocal)
                        element.GetType().GetProperty("Foreground").SetValue(element, componentColorSet.ForegroundBrush.Brush);
                    else
                    {
                        element.SetCurrentValue(colorset.ClassThemeDefinition.ForegroundDependencyProperty, componentColorSet.ForegroundBrush.Brush);
                        if (element is Button button)
                        {
                            button.Resources["testResource"] = componentColorSet.ForegroundBrush.Brush;
                        }
                    }
                }
            }

            //re-disable if it was originally disabled
            if (componentWasDisabled)
                element.IsEnabled = false;
        }

        private static ClassColorset DetermineClassColorSet(FrameworkElement element)
        {
            ClassColorset colorset = null;
            if (element is RadioButton)
            {
                colorset = CurrentTheme.CheckboxColorset;
            }
            else if (element is CheckBox)
            {
                colorset = CurrentTheme.CheckboxColorset;
            }
            else if (element is Button)
            {
                colorset = CurrentTheme.ButtonColorset;
            }
            else if (element is ProgressBar)
            {
                colorset = CurrentTheme.ProgressBarColorset;
            }
            else if (element is TabItem)
            {
                colorset = CurrentTheme.TabItemColorset;
            }
            else if (element is ComboBox)
            {
                colorset = CurrentTheme.ComboboxColorset;
                //nothing to apply, it's all bounded
            }
            else if (element is Panel)
            {
                colorset = CurrentTheme.PanelColorset;
            }
            else if (element is TextBlock)
            {
                colorset = CurrentTheme.TextblockColorset;
            }
            else if (element is Border)
            {
                colorset = CurrentTheme.BorderColorset;
            }
            else if (element is RelhaxHyperlink)
            {
                colorset = CurrentTheme.RelhaxHyperlinkColorset;
            }
            else if (element is Control)
            {
                colorset = CurrentTheme.ControlColorset;
            }
            else
                throw new BadMemeException("what the hell is this control");
            return colorset;
        }
        #endregion

        #region Backup of default theme runtime
        private static void BackupDefaultThemeColorSettings()
        {
            //make an instance of a template window for getting UI class component color default definitions
            //note control is not backed up, because it is so generic that it should not have a default
            //note Combobox is not backed up, because it is all done via WPF databinding
            //at the theme applying level, this would be from a tag (not class) level. but this can change between themes
            TemplateWindow templateWindow = new TemplateWindow();

            List<FrameworkElement> templateWindowComponents = UiUtils.GetAllWindowComponentsLogical(templateWindow, false).Distinct().ToList();

            Button b = templateWindowComponents.First( element => element is Button) as Button;
            Themes.Default.ButtonColorset.BackgroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = b.Background
            };
            Themes.Default.ButtonColorset.ForegroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = b.Foreground
            };

            TabItem ti = templateWindowComponents.First(element => element is TabItem) as TabItem;
            Themes.Default.TabItemColorset.BackgroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = ti.Background
            };
            Themes.Default.TabItemColorset.ForegroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = ti.Foreground
            };

            CheckBox cb = templateWindowComponents.First(element => element is CheckBox) as CheckBox;
            Themes.Default.CheckboxColorset.BackgroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = cb.Background
            };
            Themes.Default.CheckboxColorset.ForegroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = cb.Foreground
            };

            RadioButton rb = templateWindowComponents.First(element => element is RadioButton) as RadioButton;
            Themes.Default.RadioButtonColorset.BackgroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = rb.Background
            };
            Themes.Default.RadioButtonColorset.ForegroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = rb.Foreground
            };

            //combobox is all bound components, no need to get defaults

            TextBlock textBlock = templateWindowComponents.First(element => element is TextBlock) as TextBlock;
            Themes.Default.TextblockColorset.BackgroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = textBlock.Background
            };
            Themes.Default.TextblockColorset.ForegroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = textBlock.Foreground
            };

            Border border = templateWindowComponents.First(element => element is Border) as Border;
            Themes.Default.BorderColorset.BackgroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = border.Background
            };

            Panel panel = templateWindowComponents.First(element => element is Panel) as Panel;
            Themes.Default.PanelColorset.BackgroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = panel.Background
            };

            ProgressBar bar = templateWindowComponents.First(element => element is ProgressBar) as ProgressBar;
            Themes.Default.ProgressBarColorset.BackgroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = bar.Background
            };
            Themes.Default.ProgressBarColorset.ForegroundBrush = new CustomBrush()
            {
                IsValid = true,
                Brush = bar.Foreground
            };
        }

        private static void BackupDefaultComponentColorSettings(Window window)
        {
            //backup the component colorsettings (per window)
            List<FrameworkElement> allWindowControls = UiUtils.GetAllWindowComponentsLogical(window, false).Distinct().ToList();
            allWindowControls = allWindowControls.Where(element => element.Tag is string ID && !string.IsNullOrWhiteSpace(ID)).ToList();

            WindowColorset windowColorset = Themes.Default.WindowColorsets[window.GetType()];
            Dictionary<string, ComponentColorset> windowDictionary = windowColorset.ComponentColorsets;
            foreach (FrameworkElement element in allWindowControls)
            {
                string ID = element.Tag as string;

                //check if the key already exists first
                if (windowDictionary.ContainsKey(ID))
                    throw new BadMemeException("duplicate key");

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

        #region Custom theme file load/parse
        /// <summary>
        /// Load the custom color definitions from XML
        /// </summary>
        public static bool LoadSettingsFile()
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

            //get the UI xml format version of the file
            string versionXpath = "//" + Settings.UISettingsColorFile + "/@version";
            parsedFormatVersion = XmlUtils.GetXmlStringFromXPath(loadedDoc, versionXpath);
            Logging.Debug("using xpath search '{0}' found format version '{1}'", versionXpath, parsedFormatVersion.Trim());
            parsedFormatVersion = parsedFormatVersion.Trim();
            if (string.IsNullOrWhiteSpace(parsedFormatVersion))
            {
                Logging.Error("UIDocument formatVersion string is null, aborting parsing");
                return false;
            }

            Logging.Info("UIDocument is valid xml and format definition, loading custom color instances");
            switch (parsedFormatVersion)
            {
                case "1.0":
                    Logging.Info("Parsing custom color instances file using V1 parse method");
                    return LoadCustomThemeV1(loadedDoc);
                default:
                    //unknown
                    Logging.Error("Unknown format string or not supported: {0}", parsedFormatVersion);
                    return false;
            }
        }

        private static bool LoadCustomThemeV1(XmlDocument doc)
        {
            Theme customThemeToLoad = new Theme() { ThemeName = "Custom", FileName = Settings.UISettingsColorFile };

            //load global brushes
            List<PropertyInfo> customBrushes = customThemeToLoad.GetType().GetProperties().Where(prop => prop.PropertyType.Equals(typeof(CustomBrush))).ToList();
            foreach (PropertyInfo property in customBrushes)
            {
                string xPath = string.Format("//{0}/GlobalCustomBrushes/{1}", Settings.UISettingsColorFile, property.Name);
                Logging.Debug("Searching for global brush {0} using xpath {1}", property.Name, xPath);
                XmlNode globalBrush = XmlUtils.GetXmlNodeFromXPath(doc, xPath);

                if(NullAllowedGlobalBrushes.Contains(property.Name) && globalBrush == null)
                {
                    Logging.Debug("brush {0} is null, but on null allowed list. Set brush as invalid can continue", property.Name);
                    property.SetValue(customThemeToLoad, new CustomBrush() { IsValid = false, Brush = null });
                    continue;
                }

                if(!NullAllowedGlobalBrushes.Contains(property.Name) && globalBrush == null)
                {
                    Logging.Error("failed to get xml brush setting definition");
                    return false;
                }

                if(ApplyCustomThemeCustomBrushSettings(property.Name,globalBrush as XmlElement, out CustomBrush customBrush))
                {
                    property.SetValue(customThemeToLoad, customBrush);
                    Logging.Debug("global brush successfully applied");
                }
                else
                {
                    Logging.Error("failed to parse brush setting for global custom brush {0}", property.Name);
                    return false;
                }
            }

            //load classColorset brushes
            List<PropertyInfo> classColorsets = customThemeToLoad.GetType().GetProperties().Where(prop => prop.PropertyType.Equals(typeof(ClassColorset))).ToList();
            foreach (PropertyInfo property in classColorsets)
            {
                string xPath = string.Format("//{0}/ClassColorsets/{1}", Settings.UISettingsColorFile, property.Name);
                Logging.Debug("Searching for class definition {0} using xpath {1}", property.Name, xPath);
                XmlNode classColorsetXmlElement = XmlUtils.GetXmlNodeFromXPath(doc, xPath);
                if (classColorsetXmlElement == null)
                {
                    Logging.Error("failed to get brush setting for global custom brush {0}", property.Name);
                    return false;
                }
                if(ApplyCustomThemeClassColorsetBrushSettings(classColorsetXmlElement, out ClassColorset classColorset))
                {
                    property.SetValue(customThemeToLoad, classColorset);
                }
                else
                {
                    Logging.Error("failed to parse brush setting for class definition brush {0}", property.Name);
                    return false;
                }
            }

            //load windowColorset brushes
            if(customThemeToLoad.WindowColorsets == null)
            {
                customThemeToLoad.WindowColorsets = new Dictionary<Type, WindowColorset>();
            }
            List<Type> windows = Assembly.GetExecutingAssembly().GetTypes().ToList();
            //only get actual windows where attributes is public 
            windows = windows.Where(type =>
                type.IsClass &&
                !string.IsNullOrEmpty(type.Namespace) &&
                type.Namespace.Contains("RelhaxModpack.Windows") &&
                type.Attributes == (TypeAttributes.Public | TypeAttributes.BeforeFieldInit) &&
                type.BaseType.FullName.Contains("RelhaxModpack.Windows")
                ).ToList();
            //insert mainWindow
            windows.Insert(0, typeof(MainWindow));

            Window window = null;
            foreach (Type type in windows)
            {
                Logging.Debug("Creating UI component properties of windowType {0} for loading", type.Name);
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

                if (window is RelhaxWindow relhaxWindow)
                {
                    if (!relhaxWindow.ApplyColorSettings)
                    {
                        Logging.Info("window type {0} is set to not load custom types, skipping", type.Name);
                        window = null;
                        continue;
                    }
                }
                else if (!(window is MainWindow))
                    Logging.Warning("Window type {0} is not RelhaxWindow type!", type.Name);

                string xmlPath = string.Format("//{0}/{1}", Settings.UISettingsColorFile, type.Name);
                Logging.Debug("Using XmlPath {0} for finding window instance definitions", xmlPath);
                XmlNode classColorsetWindowXmlElement = XmlUtils.GetXmlNodeFromXPath(doc, xmlPath);
                if(classColorsetWindowXmlElement == null)
                {
                    Logging.Debug("no xml definition found for window {0} (used xmlPath {1})", type.Name, xmlPath);
                    continue;
                }

                WindowColorset windowColorset = new WindowColorset();
                if (ApplyCustomThemeCustomBrushSettings(type.Name,classColorsetWindowXmlElement, out CustomBrush windowCustomBrush))
                    windowColorset.BackgroundBrush = windowCustomBrush;
                else
                    Logging.Debug("failed to parse color definition for window {0}", type.Name);

                if (windowColorset.ComponentColorsets == null)
                    windowColorset.ComponentColorsets = new Dictionary<string, ComponentColorset>();

                //get all components of that windowType that have tags
                List<FrameworkElement> windowComponents = UiUtils.GetAllWindowComponentsLogical(window, false).Distinct().ToList();
                windowComponents = windowComponents.Where(ele => ele.Tag is string str && !string.IsNullOrWhiteSpace(str)).ToList();
                foreach(FrameworkElement element in windowComponents)
                {
                    string ID = element.Tag as string;
                    //select the CompoenentCOlorset where ID matches
                    string xPath = string.Format("//ComponentColorset[@ID='{0}']", ID);
                    Logging.Debug("Searching for ComponentColorset definition with xpath {0}", xPath);
                    XmlNode customBrushXml = classColorsetWindowXmlElement.SelectSingleNode(xPath);
                    if (customBrushXml != null)
                    {
                        Logging.Debug("Entry found, attempting to parse color settings");
                        ComponentColorset componentColorset = new ComponentColorset() { ID = ID };

                        XmlNode backgroundBrushXml = customBrushXml.SelectSingleNode("BackgroundBrush");
                        if (ApplyCustomThemeCustomBrushSettings(ID, backgroundBrushXml, out CustomBrush componentBackgroundBrush))
                            componentColorset.BackgroundBrush = componentBackgroundBrush;
                        else
                            componentColorset.BackgroundBrush = new CustomBrush();

                        XmlNode foregroundBrushXml = customBrushXml.SelectSingleNode("ForegroundBrush");
                        if (ApplyCustomThemeCustomBrushSettings(ID, foregroundBrushXml, out CustomBrush componentForegroundBrush))
                            componentColorset.ForegroundBrush = componentForegroundBrush;
                        else
                            componentColorset.ForegroundBrush = new CustomBrush();

                        windowColorset.ComponentColorsets.Add(ID, componentColorset);
                        Logging.Debug("Color settings parsed");
                    }
                    else
                        Logging.Debug("Entry not found");
                }

                //add to dict
                customThemeToLoad.WindowColorsets.Add(type, windowColorset);
            }

            Themes.Custom = customThemeToLoad;
            return true;
        }

        private static bool ApplyCustomThemeClassColorsetBrushSettings(XmlNode classColorsetXmlElement, out ClassColorset classColorset)
        {
            classColorset = new ClassColorset();

            XmlElement customBrushElement = null;
            string customBrushToApply = string.Empty;
            bool parsingSuccess = true;

            //get the class type
            string classType = classColorsetXmlElement.Attributes["ComponentType"].Value;
            switch (classType)
            {
                case "RadioButton":
                    classColorset.ClassThemeDefinition = new RadioButtonClassThemeDefinition();
                    break;
                case "CheckBox":
                    classColorset.ClassThemeDefinition = new CheckboxClassThemeDefinition();
                    break;
                case "Button":
                    classColorset.ClassThemeDefinition = new ButtonClassThemeDefinition();
                    break;
                case "TabItem":
                    classColorset.ClassThemeDefinition = new TabItemClassThemeDefinition();
                    break;
                case "ComboBox":
                    classColorset.ClassThemeDefinition = new ComboboxClassThemeDefinition();
                    break;
                case "Panel":
                    classColorset.ClassThemeDefinition = new PanelClassThemeDefinition();
                    break;
                case "TextBlock":
                    classColorset.ClassThemeDefinition = new TextBlockClassThemeDefinition();
                    break;
                case "Border":
                    classColorset.ClassThemeDefinition = new BorderClassThemeDefinition();
                    break;
                case "Control":
                    classColorset.ClassThemeDefinition = new ControlClassThemeDefinition();
                    break;
                case "ProgressBar":
                    classColorset.ClassThemeDefinition = new ProgressbarClassThemeDefinition();
                    break;
                default:
                    Logging.Error("Unknown theme class control type: {0}", classType);
                    return false;
            }

            Logging.Debug("Class definition {0} can load the following brush definitions", classType);
            Logging.Debug("Background={0}, Bound={1}", classColorset.ClassThemeDefinition.BackgroundAllowed, !string.IsNullOrEmpty(classColorset.ClassThemeDefinition.BackgroundBoundName));
            Logging.Debug("Foreground={0}, Bound={1}", classColorset.ClassThemeDefinition.ForegroundAllowed, !string.IsNullOrEmpty(classColorset.ClassThemeDefinition.ForegroundBoundName));
            Logging.Debug("Highlight={0}, Bound={1}", classColorset.ClassThemeDefinition.HighlightAllowed, !string.IsNullOrEmpty(classColorset.ClassThemeDefinition.HighlightBoundName));
            Logging.Debug("Select={0}, Bound={1}", classColorset.ClassThemeDefinition.SelectAllowed, !string.IsNullOrEmpty(classColorset.ClassThemeDefinition.SelectBoundName));

            //background
            if (classColorset.ClassThemeDefinition.BackgroundAllowed)
            {
                customBrushToApply = nameof(classColorset.BackgroundBrush);
                customBrushElement = classColorsetXmlElement.SelectSingleNode(customBrushToApply) as XmlElement;
                if (!ApplyCustomThemeCustomBrushSettings(customBrushToApply, customBrushElement, out CustomBrush rbcustomBrushBackground))
                    parsingSuccess = false;
                classColorset.BackgroundBrush = rbcustomBrushBackground;
            }

            //foreground
            if (classColorset.ClassThemeDefinition.ForegroundAllowed)
            {
                customBrushToApply = nameof(classColorset.ForegroundBrush);
                customBrushElement = classColorsetXmlElement.SelectSingleNode(customBrushToApply) as XmlElement;
                if (!ApplyCustomThemeCustomBrushSettings(customBrushToApply, customBrushElement, out CustomBrush rbcustomBrushForeground))
                    parsingSuccess = false;
                classColorset.ForegroundBrush = rbcustomBrushForeground;
            }

            //highlight
            if (classColorset.ClassThemeDefinition.HighlightAllowed)
            {
                customBrushToApply = nameof(classColorset.HighlightBrush);
                customBrushElement = classColorsetXmlElement.SelectSingleNode(customBrushToApply) as XmlElement;
                if (!ApplyCustomThemeCustomBrushSettings(customBrushToApply, customBrushElement, out CustomBrush rbcustomBrushHighlight))
                    parsingSuccess = false;
                classColorset.HighlightBrush = rbcustomBrushHighlight;
            }

            //selected
            if (classColorset.ClassThemeDefinition.SelectAllowed)
            { 
                customBrushToApply = nameof(classColorset.SelectedBrush);
                customBrushElement = classColorsetXmlElement.SelectSingleNode(customBrushToApply) as XmlElement;
                if (!ApplyCustomThemeCustomBrushSettings(customBrushToApply, customBrushElement, out CustomBrush rbcustomBrushSelect))
                    parsingSuccess = false;
                classColorset.SelectedBrush = rbcustomBrushSelect;
            }

            return parsingSuccess;
        }

        private static bool ApplyCustomThemeCustomBrushSettings(string customBrushName, XmlNode customBrushElement, out CustomBrush customBrush)
        {
            customBrush = null;
            //a null xml element means that the element should not be modified
            if (customBrushElement == null)
            {
                customBrush = new CustomBrush();
                return true;
            }

            if(ApplyCustomThemeBrushSettings(customBrushName,customBrushName,customBrushElement,out Brush brush))
            {
                customBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = brush
                };
            }
            else
            {
                Logging.Error("Failed to parse brush settings for CustomBrush {0} for Xml definition {1}", customBrushName, customBrushElement.Name);
                return false;
            }
            return true;
        }

        private static bool ApplyCustomThemeBrushSettings(string componentName, string componentTag, XmlNode brushSettings, out Brush colorToOutput)
        {
            if (string.IsNullOrEmpty(componentName))
                componentName = "null";

            bool someThingApplied = false;
            colorToOutput = new SolidColorBrush();
            //make sure type is set correctly
            XmlAttribute brushType = brushSettings.Attributes["type"];
            if (brushType == null)
            {
                Logging.Warning("failed to apply brush setting: type attribute not exist!");
                return false;
            }
            if(brushType.InnerText.ToLower().Equals("null"))
            {
                colorToOutput = null;
                someThingApplied = true;
                return someThingApplied;
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
                switch (brushType.InnerText)
                {
                    case "SolidColorBrush"://color=1
                        if (ParseColorFromString(color1.InnerText, out Color kolor1_solid))
                        {
                            colorToOutput = new SolidColorBrush(kolor1_solid);
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
                            colorToOutput = new LinearGradientBrush(kolor1_linear, kolor2_linear, point_1, point_2);
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
                            colorToOutput = new RadialGradientBrush(kolor1_radial, kolor2_radial);
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

        #region Custom theme file save
        /// <summary>
        /// Saves all currently enabled color settings to an xml file
        /// </summary>
        /// <param name="savePath">The path to save the xml file to</param>
        /// <param name="mainWindow">If method is called from MainWindow, use itself for getting its color properties</param>
        public static void DumpAllWindowColorSettingsToFile(string savePath, MainWindow mainWindow)
        {
            XmlDocument doc = new XmlDocument();

            //create and add declaration
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", "UTF-8", "yes");
            doc.AppendChild(dec);

            //make root element and version attribute
            XmlElement root = doc.CreateElement(Settings.UISettingsColorFile);
            //NOTE: version attribute should be incremented when large change in color loading structure is done
            //allows us to make whole new method to load UI settings
            XmlAttribute version = doc.CreateAttribute("version");
            version.Value = "1.0";

            //append version attribute to element, and element to main doc
            root.Attributes.Append(version);
            doc.AppendChild(root);

            //write all CustomBrush objects from the theme
            //create xml header element
            XmlElement customBrushRoot = doc.CreateElement("GlobalCustomBrushes");
            //get list of all custom brush elements in the application
            List<PropertyInfo> customBrushes = CurrentTheme.GetType().GetProperties().Where(prop => prop.PropertyType.Equals(typeof(CustomBrush))).ToList();
            foreach(PropertyInfo property in customBrushes)
            {
                XmlElement customBrushXml = doc.CreateElement(property.Name);
                CustomBrush customBrush = property.GetValue(CurrentTheme) as CustomBrush;
                if (customBrush.IsValid)
                {
                    WriteColorAttributesToXmlElement(customBrushXml, customBrush.Brush, doc);
                    customBrushRoot.AppendChild(customBrushXml);
                }
                else
                {
                    Logging.Info("GlobalBrush {0} is invalid, not saving. (note you can still make a custom definition for it if you wish)", property.Name);
                }
            }
            root.AppendChild(customBrushRoot);

            //write all ClassColorset objects from the theme
            //create xml header element
            XmlElement classColorsetxmlRoot = doc.CreateElement("ClassColorsets");
            List<PropertyInfo> classColorsets = CurrentTheme.GetType().GetProperties().Where(prop => prop.PropertyType.Equals(typeof(ClassColorset))).ToList();
            foreach(PropertyInfo property in classColorsets)
            {
                XmlElement classColorsetXml = doc.CreateElement(property.Name);
                ClassColorset classColorset = property.GetValue(CurrentTheme) as ClassColorset;

                //save type property
                XmlAttribute classColorsetType = doc.CreateAttribute("ComponentType");
                classColorsetType.Value = classColorset.ClassThemeDefinition.ClassType.Name;
                classColorsetXml.Attributes.Append(classColorsetType);

                //save customBrushes
                if(classColorset.ClassThemeDefinition.BackgroundAllowed && classColorset.BackgroundBrush != null)
                {
                    XmlElement brushName = doc.CreateElement(nameof(classColorset.BackgroundBrush));
                    if (classColorset.BackgroundBrush.IsValid)
                    {
                        WriteColorAttributesToXmlElement(brushName, classColorset.BackgroundBrush.Brush, doc);
                        classColorsetXml.AppendChild(brushName);
                    }
                }

                if (classColorset.ClassThemeDefinition.ForegroundAllowed && classColorset.ForegroundBrush != null)
                {
                    XmlElement brushName = doc.CreateElement(nameof(classColorset.ForegroundBrush));
                    if (classColorset.ForegroundBrush.IsValid)
                    {
                        WriteColorAttributesToXmlElement(brushName, classColorset.ForegroundBrush.Brush, doc);
                        classColorsetXml.AppendChild(brushName);
                    }
                }

                if (classColorset.ClassThemeDefinition.HighlightAllowed && classColorset.HighlightBrush != null)
                {
                    XmlElement brushName = doc.CreateElement(nameof(classColorset.HighlightBrush));
                    if (classColorset.HighlightBrush.IsValid)
                    {
                        WriteColorAttributesToXmlElement(brushName, classColorset.HighlightBrush.Brush, doc);
                        classColorsetXml.AppendChild(brushName);
                    }
                }

                if (classColorset.ClassThemeDefinition.SelectAllowed && classColorset.SelectedBrush != null)
                {
                    XmlElement brushName = doc.CreateElement(nameof(classColorset.SelectedBrush));
                    if (classColorset.SelectedBrush.IsValid)
                    {
                        WriteColorAttributesToXmlElement(brushName, classColorset.SelectedBrush.Brush, doc);
                        classColorsetXml.AppendChild(brushName);
                    }
                }

                //add to root holder
                classColorsetxmlRoot.AppendChild(classColorsetXml);
            }
            root.AppendChild(classColorsetxmlRoot);

            //add all window instances to document:
            //MainWindow first
            WriteWindowComponentSettingsToXml(root, doc, mainWindow);
            
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

                //check if we should include this into the definitions file
                if (window is RelhaxWindow relhaxWindow)
                {
                    if(relhaxWindow.ApplyColorSettings)
                    {
                        if (CurrentTheme.WindowColorsets.ContainsKey(relhaxWindow.GetType()))
                        {
                            Logging.Debug("RelhaxWindow instance {0} is allowed to be customized, include in theme saving", type.Name);
                        }
                        else
                        {
                            Logging.Debug("RelhaxWindow instance {0} is allowed to be customized, but has no custom tag components, don't include in theme saving", type.Name);
                            continue;
                        }
                    }
                    else
                    {
                        Logging.Debug("RelhaxWindow instance {0} is not allowed to be customized, don't include in theme saving", type.Name);
                        continue;
                    }
                }
                else
                    Logging.Warning("found window instance {0} that isn't of RelhaxWindow class. Is this the intent?", type.Name);

                WriteWindowComponentSettingsToXml(root, doc, window);
            }

            //save xml file
            doc.Save(savePath);
        }

        private static void WriteWindowComponentSettingsToXml(XmlElement documentRoot, XmlDocument doc, Window window)
        {
            //make window element
            string windowType = window.GetType().Name;
            XmlElement windowElement = doc.CreateElement(windowType);

            //save attributes to element
            WriteColorAttributesToXmlElement(windowElement, window.Background, doc);
            documentRoot.AppendChild(windowElement);

            WindowColorset windowColorset = CurrentTheme.WindowColorsets[window.GetType()];

            if (windowColorset.ComponentColorsets == null)
            {
                Logging.Debug("windowColorset for type {0}, is this expected?",windowColorset.WindowType.Name);
                return;
            }

            foreach(ComponentColorset colorset in windowColorset.ComponentColorsets.Values.ToList())
            {
                XmlElement component = doc.CreateElement(nameof(ComponentColorset));

                //ID
                XmlAttribute IDAttribute = doc.CreateAttribute("ID");
                IDAttribute.Value = colorset.ID;
                component.Attributes.Append(IDAttribute);

                //Background brush
                XmlElement backgroundBrushXml = doc.CreateElement(nameof(colorset.BackgroundBrush));
                if (colorset.BackgroundBrush != null && colorset.BackgroundBrush.IsValid)
                {
                    WriteColorAttributesToXmlElement(backgroundBrushXml, colorset.BackgroundBrush.Brush, doc);
                    component.AppendChild(backgroundBrushXml);
                }

                //Foreground brush
                XmlElement foregroundBrushXml = doc.CreateElement(nameof(colorset.ForegroundBrush));
                if (colorset.ForegroundBrush != null && colorset.ForegroundBrush.IsValid)
                {
                    WriteColorAttributesToXmlElement(foregroundBrushXml, colorset.ForegroundBrush.Brush, doc);
                    component.AppendChild(foregroundBrushXml);
                }

                windowElement.AppendChild(component);
            }
        }

        private static void WriteColorAttributesToXmlElement(XmlElement colorEntry, Brush brush, XmlDocument doc)
        {
            XmlAttribute colorType, color1;

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
        }
        #endregion
    }
}