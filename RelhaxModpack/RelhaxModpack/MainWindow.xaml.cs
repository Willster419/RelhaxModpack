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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using RelhaxModpack.Windows;
using RelhaxModpack.Utils;

namespace RelhaxModpack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Creates the instance of the MainWindow class
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        private void TheMainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //don't actually do this yet
            //Translations.ApplyTranslationsOnWindowLoad(this);

            //create the notify icon
            /*NotifyIcon relhaxIcon = new NotifyIcon()
            {
                Visible = true,
                Icon = Properties.Resources.modpack_icon,
                Text = Title
            };*/
            //DatabaseEditor edit = new DatabaseEditor();
            //edit.Show();
            //PatchTester pt = new PatchTester();
            //pt.Show();
            //ModSelectionList mls = new ModSelectionList();
            //mls.Show();
            //Preview p = new Preview();
            //p.Show();
            //DatabaseUpdater dba = new DatabaseUpdater();
            //dba.Show();
        }
    }
}
