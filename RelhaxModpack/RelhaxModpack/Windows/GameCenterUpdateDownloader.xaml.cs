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

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for GameCenterUpdateDownloader.xaml
    /// </summary>
    public partial class GameCenterUpdateDownloader : RelhaxWindow
    {

        private bool init = true;

        public GameCenterUpdateDownloader()
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {

            init = false;
        }

        private void GcDownloadMainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (init)
                return;

            //disable all tabItems but but selected
            foreach(TabItem item in GcDownloadMainTabControl.Items)
            {
                if(!item.Equals(GcDownloadMainTabControl.SelectedItem as TabItem))
                {
                    item.IsEnabled = false;
                }
                else
                {
                    item.IsEnabled = true;
                }
            }
        }

        private void GcDownloadStep1Next_Click(object sender, RoutedEventArgs e)
        {
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep2;
        }

        private void GcDownloadStep2PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep1;
        }

        private void GcDownloadStep2NextButton_Click(object sender, RoutedEventArgs e)
        {
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep3;
        }

        private void GcDownloadStep3PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep2;
        }

        private void GcDownloadStep3NextButton_Click(object sender, RoutedEventArgs e)
        {
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep4;
        }

        private void GcDownloadStep4PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep3;
        }

        private void GcDownloadStep4NextButton_Click(object sender, RoutedEventArgs e)
        {
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep5;
        }

        private void GcDownloadStep5CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
