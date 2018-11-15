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
    public class DevleoperSelectionsClosedEWventArgs : EventArgs
    {
        public bool LoadSelection = false;
        public string FileToLoad = "";
    }
    public delegate void DeveloperSelectionsClosedDelagate(object sender, DevleoperSelectionsClosedEWventArgs e);
    /// <summary>
    /// Interaction logic for DeveloperSelectionsViewer.xaml
    /// </summary>
    public partial class DeveloperSelectionsViewer : RelhaxWindow
    {
        private bool loadSelection = false;
        private string fileToLoad = "";
        public event DeveloperSelectionsClosedDelagate OnDeveloperSelectionsClosed;

        public DeveloperSelectionsViewer()
        {
            InitializeComponent();
        }

        private void OnApplicationLoading(object sender, RoutedEventArgs e)
        {
            //add loading message to stack panel
            //event based wait (for easy canceling?
        }

        private void OnApplicationClosed(object sender, EventArgs e)
        {
            if (OnDeveloperSelectionsClosed != null)
            {
                OnDeveloperSelectionsClosed(this, new DevleoperSelectionsClosedEWventArgs()
                {
                    LoadSelection = loadSelection,
                    FileToLoad = fileToLoad
                });
            }
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            this.loadSelection = true;
            //get filename of zip xml file to load
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.loadSelection = false;
            this.fileToLoad = string.Empty;
            this.Close();
        }

        private void LocalFile_Click(object sender, RoutedEventArgs e)
        {
            this.loadSelection = true;
            this.fileToLoad = "LOCAL";
            this.Close();
        }
    }
}
