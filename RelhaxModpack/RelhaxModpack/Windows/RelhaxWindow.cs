using RelhaxModpack.Settings;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Defines a window with translation and custom UI color
    /// </summary>
    public class RelhaxWindow : Window
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
        /// Controls if the window should have color settings applied
        /// </summary>
        public bool ApplyColorSettings { get; set; } = false;

        /// <summary>
        /// Controls if the window should apply scaling values set from the main window
        /// </summary>
        public bool ApplyScaling { get; set; } = true;

        /// <summary>
        /// Controls if the window should apply custom font to this window.
        /// </summary>
        /// <remarks>This setting works in tandem with the ModpackSettings setting to use custom font.</remarks>
        public bool ApplyCustomFont { get; set; } = false;

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
        /// Creates an instance of the RelhaxWindow class
        /// </summary>
        public RelhaxWindow(ModpackSettings modpackSettings) : base()
        {
            if (this.ModpackSettings == null)
                this.ModpackSettings = modpackSettings;
            //subscribe to the loaded event to load custom code
            Loaded += OnWindowLoaded;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
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

            //apply UI color changes
            if(ApplyColorSettings)
            {
                UISettings.ApplyCustomStyles(this);
                UISettings.ApplyUIColorSettings(this);
            }

            //apply font changes
            if(ApplyCustomFont && ModpackSettings.EnableCustomFont)
            {
                UiUtils.ApplyFontToWindow(this, UiUtils.CustomFontFamily);
            }

            //deal with scaling
            if (ApplyScaling)
            {
                //get current scaling of window (like from display settings)
                double currentScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
                //if current scale is not target(modpackSetting), then update
                if (ModpackSettings.DisplayScale != currentScale)
                {
                    UiUtils.ApplyApplicationScale(this, ModpackSettings.DisplayScale);
                }
            }

            Loaded -= OnWindowLoaded;
        }
    }
}
