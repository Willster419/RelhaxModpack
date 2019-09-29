using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RelhaxWPFConvert
{
    public static class UISettings
    {
        #region Highlighting properties
        //button (hilight)
        private static Brush buttonHighlightBrush;
        public static Brush ButtonHighlightBrush
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

        //tabControl (selected and highlight)
        #endregion

        #region Default UI options
        //x:Key="Button.MouseOver.Background" Color="#FFBEE6FD"
        public static Brush DefaultButtonHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 190, 230, 253));

        #endregion

        #region Dark UI options
        public static Brush DarkButtonHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134));

        #endregion


        #region Init and Property handling code
        public static void InitUIBrushes()
        {
            ButtonHighlightBrush = DefaultButtonHighlightBrush;
        }

        //https://stackoverflow.com/questions/34762879/static-binding-doesnt-update-when-resource-changes
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
