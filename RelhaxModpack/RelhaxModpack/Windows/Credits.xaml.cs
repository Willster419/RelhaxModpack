using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for Credits.xaml
    /// </summary>
    public partial class Credits : RelhaxWindow
    {
        public Credits()
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //translate title
            Title = Translations.GetTranslatedString(nameof(Credits));

            StringBuilder creditsBuilder = new StringBuilder();


            CreditsTextBox.Text = creditsBuilder.ToString();
        }
    }
}
