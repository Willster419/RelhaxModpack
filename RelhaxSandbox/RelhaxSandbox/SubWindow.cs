using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RelhaxSandbox
{
    public class SubWindow : Window
    {

        public SubWindow() : base()
        {
            Loaded += SubWindow_Loaded;
        }

        private void SubWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //do a thing
        }
    }
}
