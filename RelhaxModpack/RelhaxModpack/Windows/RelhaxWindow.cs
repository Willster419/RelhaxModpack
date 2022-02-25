using RelhaxModpack.Common;
using RelhaxModpack.Settings;
using RelhaxModpack.UI;
using RelhaxModpack.UI.Extensions;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Defines a window with translation and custom UI color
    /// </summary>
    public class RelhaxWindow : Window, INotifyPropertyChanged
    {
        /// <summary>
        /// Controls if the window should have translation applied
        /// </summary>
        public bool LocalizeWindow { get; set; } = false;

        /// <summary>
        /// Controls if the window should have tool tips applied
        /// </summary>
        public bool ApplyToolTips { get; set; } = false;

        /// <summary>
        /// Controls if pressing the escape key while the window is open will close it.
        /// </summary>
        public bool EscapeKeyClosesWindow { get; set; } = false;

        /// <summary>
        /// The original Width and Height of the window before scaling
        /// </summary>
        public double OriginalWidth { get; set; }

        /// <summary>
        /// The original Width and Height of the window before scaling
        /// </summary>
        public double OriginalHeight { get; set; }

        /// <summary>
        /// A reference to the modpack settings window configuration class
        /// </summary>
        public ModpackSettings ModpackSettings { get; set; }

        /// <summary>
        /// A reference to the command line settings configuration class
        /// </summary>
        public CommandLineSettings CommandLineSettings { get; set; }

        /// <summary>
        /// Get or set the default font to use in this window.
        /// </summary>
        protected FontFamily DefaultFontFamily
        {
            get { return (Application.Current as App).DefaultFontFamily; }
            set { (Application.Current as App).DefaultFontFamily = value; }
        }

        /// <summary>
        /// Get or set the selected font to use in this window.
        /// </summary>
        protected FontFamily SelectedFontFamily { get; set; }

        /// <summary>
        /// Get a list of fonts installed on the system for this window to use.
        /// </summary>
        protected List<FontFamily> FontList
        { 
            get { return (Application.Current as App).Fonts; }
        }

        /// <summary>
        /// Creates an instance of the RelhaxWindow class
        /// </summary>
        public RelhaxWindow(ModpackSettings modpackSettings) : base()
        {
            if (this.ModpackSettings == null)
                this.ModpackSettings = modpackSettings;
            //subscribe to the loaded event to load custom code
            Loaded += OnWindowLoaded;
        }

        /// <summary>
        /// Method that occurs when they key up event is fired.
        /// </summary>
        /// <param name="sender">The object that sent the request.</param>
        /// <param name="e">The key event args to go with the event.</param>
        protected virtual void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Logging.Debug(LogOptions.ClassName, "Escape Key pressed, closing window");
                this.Close();
            }
        }

        /// <summary>
        /// Performs custom window loading functions that should be done to all windows of this class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>This function saves the original size that the window was designed for (scaling), applies localizations, applies color settings,
        /// applies custom font, and applies scaling. Each application action is controlled by a boolean.</remarks>
        protected virtual void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //get the original width and height
            OriginalHeight = Height;
            OriginalWidth = Width;

            //subscribe to the event of the escape key pressed closing the window
            if (EscapeKeyClosesWindow)
                KeyUp += OnKeyUp;

            //deal with the translations
            if (LocalizeWindow)
            {
                Translations.LocalizeWindow(this, ApplyToolTips);
            }

            //apply font changes
            if(ModpackSettings.EnableCustomFont)
            {
                ApplyFontToWindow();
            }

            //get current scaling of window (like from display settings)
            double currentScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
            //if current scale is not target(modpackSetting), then update
            if (ModpackSettings.DisplayScale != currentScale)
            {
                ApplyApplicationScale(ModpackSettings.DisplayScale);
            }

            Loaded -= OnWindowLoaded;
        }

        /// <summary>
        /// Applies vector based application scaling to the specified window
        /// </summary>
        /// <param name="scaleValue">The amount of scaling, in a multiplication factor, to apply to the window from</param>
        protected virtual void ApplyApplicationScale(double scaleValue)
        {
            //input filtering
            if (scaleValue < ApplicationConstants.MinimumDisplayScale)
            {
                Logging.Warning("Scale size of {0} is to small, setting to 1", scaleValue.ToString("N"));
                scaleValue = ApplicationConstants.MinimumDisplayScale;
            }
            if (scaleValue > ApplicationConstants.MaximumDisplayScale)
            {
                Logging.Warning("Scale size of {0} is to large, setting to 3", scaleValue.ToString("N"));
                scaleValue = ApplicationConstants.MaximumDisplayScale;
            }

            //scale internals
            (this.Content as FrameworkElement).LayoutTransform = new ScaleTransform(scaleValue, scaleValue, 0, 0);

            //scale window size itself
            this.Width = this.OriginalWidth * scaleValue;
            this.Height = this.OriginalHeight * scaleValue;
        }


        /// <summary>
        /// Applies the given FontFamily font type to the window
        /// </summary>
        protected virtual void ApplyFontToWindow()
        {
            FontFamily result = FontList.Find(font => font.FontName().Equals(ModpackSettings.CustomFontName));

            if (result == null)
            {
                Logging.Warning(LogOptions.MethodName, "Unable to find font {0} in system font list. Using default font {1}", ModpackSettings.CustomFontName, DefaultFontFamily.FontName());
                SelectedFontFamily = DefaultFontFamily;
            }

            Logging.Debug(LogOptions.MethodName, "Applying font '{0}' to window", result.FontName());

            SelectedFontFamily = result;
            this.FontFamily = SelectedFontFamily;
            foreach (FrameworkElement element in UiUtils.GetAllWindowComponentsLogical(this, true))
            {
                if (element is Control control)
                    control.FontFamily = this.SelectedFontFamily;
            }
        }

        #region Dark theme done in a way that's actually good
        /// <summary>
        /// Toggle if the application should use dark theme styles.
        /// </summary>
        public bool DarkTheme
        {
            get { return this.ModpackSettings.ApplicationTheme == UIThemes.Dark; }
            set
            {
                this.ModpackSettings.ApplicationTheme = value ? UIThemes.Dark : UIThemes.Default;
                OnPropertyChanged(nameof(DarkTheme));
            }
        }

        /// <summary>
        /// Occurs after a property that uses OnPropertyChanged has been set.
        /// </summary>
        /// <remarks>At the time of this writing, the only property in RelhaxWindow to use this is the DarkTheme property.</remarks>
        /// <seealso cref="OnPropertyChanged(string)"/>
        /// <seealso cref="INotifyPropertyChanged"/>
        /// <seealso cref="DarkTheme"/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Called from a property in this class that wants to tell PropertyChanged listeners that it has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        /// <remarks>At the time of this writing, the only property in RelhaxWindow to use this is the DarkTheme property.</remarks>
        /// <seealso cref="PropertyChanged"/>
        /// <seealso cref="INotifyPropertyChanged"/>
        /// <seealso cref="DarkTheme"/>
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion
    }
}
