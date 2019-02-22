using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml;
using Microsoft.Win32;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for DatabaseEditor.xaml
    /// </summary>
    public partial class DatabaseEditor : RelhaxWindow
    {

        private EditorSettings EditorSettings;
        private XmlDocument XmlDatabase;
        private List<DatabasePackage> GlobalDependencies;
        private List<Dependency> Dependencies;
        private List<Category> ParsedCategoryList;
        private OpenFileDialog OpenDatabaseDialog;
        private SaveFileDialog SaveDatabaseDialog;
        private OpenFileDialog OpenZipFileDialog;
        private SaveFileDialog SaveZipFileDialog;
        private string[] UIHeaders = new string[]
        {
            "-----Global Dependencies-----",
            "-----Dependencies-----",
        };

        public DatabaseEditor()
        {
            InitializeComponent();
        }

        private void OnApplicationLoad(object sender, RoutedEventArgs e)
        {
            EditorSettings = new EditorSettings();
            Logging.Info("Loading editor settings");
            if(!Settings.LoadSettings(Settings.EditorSettingsFilename, typeof(EditorSettings), null,EditorSettings))
            {
                Logging.Info("Failed to load editor settings, using defaults");
            }
            else
            {
                Logging.Info("Editor settings loaded success");
            }
            //check if we are loading the document auto from the command line
            if(!string.IsNullOrWhiteSpace(CommandLineSettings.EditorAutoLoadFileName))
            {
                Logging.Info("Attempting to auto-load xml file from {0}", CommandLineSettings.EditorAutoLoadFileName);
                if(File.Exists(CommandLineSettings.EditorAutoLoadFileName))
                {
                    OnLoadDatabaseClick(null, null);
                }
                else
                {
                    Logging.Info("file does not exist");
                }
            }
        }

        private void OnApplicationClose(object sender, EventArgs e)
        {
            if (!Logging.IsLogDisposed(Logfiles.Application))
            {
                Logging.WriteToLog("Saving editor settings");
                if (Settings.SaveSettings(Settings.EditorSettingsFilename, typeof(EditorSettings), null, EditorSettings))
                    Logging.WriteToLog("Editor settings saved");
            }
        }

        private void OnLoadDatabaseClick(object sender, RoutedEventArgs e)
        {
            string fileToLoad = string.Empty;
            //check if it's from the auto load function or not
            if(sender != null)
            {
                //from gui button press
                if (OpenDatabaseDialog == null)
                    OpenDatabaseDialog = new OpenFileDialog()
                    {
                        AddExtension = true,
                        CheckFileExists = true,
                        CheckPathExists = true,
                        DefaultExt = "xml",
                        InitialDirectory = Settings.ApplicationStartupPath,
                        Multiselect = false,
                        Title = "Load Database"
                    };
                if ((bool)OpenDatabaseDialog.ShowDialog() && File.Exists(OpenDatabaseDialog.FileName))
                {
                    fileToLoad = OpenDatabaseDialog.FileName;
                }
                else
                    return;
            }
            else
            {
                //from auto load function
                fileToLoad = CommandLineSettings.EditorAutoLoadFileName;
            }
            //the file exists, load it
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(fileToLoad);
            }
            catch (XmlException ex)
            {
                Logging.Exception(ex.ToString());
                MessageBox.Show(ex.ToString());
                return;
            }
            if (!XMLUtils.ParseDatabase(doc, GlobalDependencies, Dependencies, ParsedCategoryList))
            {
                MessageBox.Show("Failed to load the database, check the logfile");
                return;
            }
            LoadUI(GlobalDependencies, Dependencies, ParsedCategoryList);
        }

        private void LoadUI(List<DatabasePackage> globalDependencies, List<Dependency> dependnecies, List<Category> parsedCategoryList)
        {
            //clear and reset
            DatabaseTreeView.Items.Clear();
            //RESET UI TODO? or don't do it?

        }
    }
}
