using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Defines a window with translation and custom UI color
    /// </summary>
    public class RelhaxWindow : Window
    {
        /// <summary>
        /// Specifies if the window should have translatoin applied
        /// </summary>
        public bool LocalizeWindow { get; set; } = false;
        /// <summary>
        /// Specified if the window should have tooltips applied
        /// </summary>
        public bool ApplyToolTips { get; set; } = false;

        public bool ApplyColorSettings { get; set; } = false;

        public bool ApplyScaling { get; set; } = false;

        public double OriginalWidth { get; set; }

        public double OriginalHeight { get; set; }

        /// <summary>
        /// Creates an instance of the RelhaxWindow class
        /// </summary>
        public RelhaxWindow() : base()
        {
            //get the original width and height
            OriginalHeight = Height;
            OriginalWidth = Width;
            //subscribe to the loaded event to load custom code
            Loaded += OnWindowLoaded;
        }

        //hook int othe window loaded event to apply things that should be done to all child windows of the mainWindow
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //deal with the translatios
            if(LocalizeWindow)
            {
                Translations.LocalizeWindow(this, ApplyToolTips);
            }
            //apply UI color changes
            if(ApplyColorSettings)
            {
                UISettings.ApplyUIColorSettings(this);
            }
            //deal with scaling
            if(ApplyScaling)
            {
                //get current scaling of window (like from display settings)
                double currentScale = PresentationSource.FromVisual(this).CompositionTarget.TransformToDevice.M11;
                //if current scale is not target(modpackSetting), then update
                if (ModpackSettings.DisplayScale != currentScale)
                {
                    Utils.ApplyApplicationScale(this, ModpackSettings.DisplayScale);
                }
            }
        }
    }
}
