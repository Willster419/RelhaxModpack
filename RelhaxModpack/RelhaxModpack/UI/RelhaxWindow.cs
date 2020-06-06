using RelhaxModpack.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

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
        public bool ApplyScaling { get; set; } = false;

        /// <summary>
        /// The original Width and Height of the window before scaling
        /// </summary>
        public double OriginalWidth { get; set; }

        /// <summary>
        /// The original Width and Height of the window before scaling
        /// </summary>
        public double OriginalHeight { get; set; }

        /// <summary>
        /// Creates an instance of the RelhaxWindow class
        /// </summary>
        public RelhaxWindow() : base()
        {
            //subscribe to the loaded event to load custom code
            Loaded += OnWindowLoaded;
        }

        //hook into the window loaded event to apply things that should be done to all child windows of the mainWindow
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //get the original width and height
            OriginalHeight = Height;
            OriginalWidth = Width;

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
            if(ModpackSettings.EnableCustomFont)
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
        }
    }
}
