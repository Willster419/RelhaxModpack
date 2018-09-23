using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RelhaxModpack.Windows
{
    public class RelhaxWindow : Window
    {

        public bool LocalizeWindow { get; set; } = false;
        public bool ApplyToolTips { get; set; } = false;

        public RelhaxWindow() : base()
        {
            this.Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //deal with the translatios
            if(LocalizeWindow)
            {
                Translations.LocalizeWindow(this, ApplyToolTips);
            }
        }
    }
}
