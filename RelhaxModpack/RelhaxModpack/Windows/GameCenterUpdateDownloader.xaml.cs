using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Text;
using System.Timers;
using System;
using System.Diagnostics;


namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for GameCenterUpdateDownloader.xaml
    /// </summary>
    public partial class GameCenterUpdateDownloader : RelhaxWindow
    {

        public string SelectedClient { get; set; } = string.Empty;

        public const string GameInfoXml = "game_info.xml";

        public const string MetaDataXml = "metadata.xml";

        public const string GameMetadataFolder = "game_metadata";

        public const string WgcProcessName = "wgc";

        public struct GameCenterProperty
        {
            public string FileName;
            public string Xpath;
            public TextBlock TextBlock;
            public string Value;
            public bool GaveError;
            public bool IsRequired;
        }

        private GameCenterProperty ClientType, Language_, MetadataVersion, MetadataProtocalVersion, ChainID,
            ClientCurrentVersion, LocaleCurrentVersion, SdContentCurrentVersion, HdContentCurrentVersion, GameId, VersionName;

        private GameCenterProperty[] GameCenterProperties = null;

        public struct PatchFileProperty
        {
            public string Filename;
            public string CompleteFilePath;
            public long Size;
        }

        private bool init = true;

        private string gameInfoXmlPath = string.Empty;

        private string metaDataXmlPath = string.Empty;

        private Timer timer = null;

        public GameCenterUpdateDownloader()
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //init the game center properties (xml file, xpath, textblock)
            ClientType = new GameCenterProperty()
            {
                FileName = GameInfoXml,
                Xpath = @"//protocol/game/client_type",
                TextBlock = ClientTypeValue,
                GaveError = false,
                IsRequired = true
            };
            Language_ = new GameCenterProperty()
            {
                FileName = GameInfoXml,
                Xpath = @"//protocol/game/localization",
                TextBlock = LangValue,
                GaveError = false,
                IsRequired = true
            };
            MetadataVersion = new GameCenterProperty()
            {
                FileName = MetaDataXml,
                Xpath = @"//protocol/version",
                TextBlock = MetadataVersionValue,
                GaveError = false,
                IsRequired = true
            };
            MetadataProtocalVersion = new GameCenterProperty()
            {
                FileName = MetaDataXml,
                // //modInfoAlpha.xml/@onlineFolder
                Xpath = @"//protocol/@version",
                TextBlock = MetadataProtocolVersionValue,
                GaveError = false,
                IsRequired = true
            };
            ChainID = new GameCenterProperty()
            {
                FileName = MetaDataXml,
                Xpath = @"//protocol/predefined_section/chain_id",
                TextBlock = ChainIDValue,
                GaveError = false,
                IsRequired = true
            };
            ClientCurrentVersion = new GameCenterProperty()
            {
                FileName = GameInfoXml,
                Xpath = @"//protocol/game/part_versions/value[@name='client']",
                TextBlock = ClientCurrentVersionValue,
                GaveError = false,
                IsRequired = true
            };
            LocaleCurrentVersion = new GameCenterProperty()
            {
                FileName = GameInfoXml,
                Xpath = @"//protocol/game/part_versions/value[@name='locale']",
                TextBlock = LocaleCurrentVersionValue,
                GaveError = false,
                IsRequired = true
            };
            SdContentCurrentVersion = new GameCenterProperty()
            {
                FileName = GameInfoXml,
                Xpath = @"//protocol/game/part_versions/value[@name='sdcontent']",
                TextBlock = SdContentCurrentVersionValue,
                GaveError = false,
                IsRequired = true
            };
            HdContentCurrentVersion = new GameCenterProperty()
            {
                FileName = GameInfoXml,
                Xpath = @"//protocol/game/part_versions/value[@name='hdcontent']",
                TextBlock = HdContentCurrentVersionValue,
                GaveError = false,
                IsRequired = false
            };
            GameId = new GameCenterProperty()
            {
                FileName = GameInfoXml,
                Xpath = @"//protocol/game/id",
                TextBlock = GameIDValue,
                GaveError = false,
                IsRequired = true
            };
            VersionName = new GameCenterProperty()
            {
                FileName = GameInfoXml,
                Xpath = @"//protocol/game/version_name",
                TextBlock = null,
                GaveError = false,
                IsRequired = true
            };
            GameCenterProperties = new GameCenterProperty[]
            {
                ClientType,
                Language_,
                MetadataVersion,
                MetadataProtocalVersion,
                ChainID,
                ClientCurrentVersion,
                LocaleCurrentVersion,
                SdContentCurrentVersion,
                HdContentCurrentVersion,
                GameId
            };

            Logging.Debug("GameCenterDownloader: SelectedClient = {0}", SelectedClient);
            GcDownloadStep1Init();
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

        private void GcDownloadStep1SelectClientButton_Click(object sender, RoutedEventArgs e)
        {
            //if client selected, get params
            OpenFileDialog manualWoTFind = new OpenFileDialog()
            {
                AddExtension = true,
                CheckFileExists = true,
                CheckPathExists = true,
                //https://stackoverflow.com/a/2069090/3128017
                //Office Files|*.doc;*.xls;*.ppt
                Filter = "WG Client|WorldOfTanks.exe;WorldOfWarships.exe;WorldOfWarplanes.exe",
                Multiselect = false,
                ValidateNames = true,
                Title = Translations.GetTranslatedString("GcDownloadSelectWgClient")
            };
            if ((bool)manualWoTFind.ShowDialog() && File.Exists(manualWoTFind.FileName))
            {
                SelectedClient = Path.GetDirectoryName(manualWoTFind.FileName);
                //replace the 'win32' or 'win64' directory with nothing (so removing it)
                SelectedClient = SelectedClient.Replace(Settings.WoT32bitFolderWithSlash, string.Empty).Replace(Settings.WoT64bitFolderWithSlash, string.Empty);
                Logging.Info("GameCenterDownloader: Selected install -> {0}", SelectedClient);
            }

            GcDownloadStep1Init();
        }

        private void GcDownloadStep1Init()
        {
            //if client selected, get params
            //else, reset UI to none
            if (string.IsNullOrWhiteSpace(SelectedClient))
            {
                Logging.Info("SelectedClient is empty, resetting");
                GcDownloadStep1ResetParams(true, true);
            }
            else
            {
                Logging.Info("SelectedClient is not empty ({0}), attempting to parse get request values",SelectedClient);
                GcDownloadStep1GetParams();
            }
        }

        private void GcDownloadStep1ResetParams(bool resetCurrentSelected, bool resetGameCenterProperties)
        {
            //reset UI window
            if (resetCurrentSelected)
            {
                GcDownloadStep1CurrentlySelectedClient.Text = string.Format(Translations.GetTranslatedString("GcDownloadStep1CurrentlySelectedClient"),
                    Translations.GetTranslatedString("SelectedInstallationNone"));
                GcDownloadStep1CurrentlySelectedClient.Foreground = System.Windows.Media.Brushes.Red;
            }

            if (resetGameCenterProperties)
            {
                foreach (GameCenterProperty gameCenterProp in GameCenterProperties)
                {
                    if (gameCenterProp.TextBlock != null)
                    {
                        gameCenterProp.TextBlock.Text = Translations.GetTranslatedString("none");
                        gameCenterProp.TextBlock.Foreground = System.Windows.Media.Brushes.Red;
                    }
                }
            }

            //disable next button
            GcDownloadStep1Next.IsEnabled = false;
        }

        private void GcDownloadStep1GetParams()
        {
            GcDownloadStep1CurrentlySelectedClient.Text = string.Format(Translations.GetTranslatedString("GcDownloadStep1CurrentlySelectedClient"),
                SelectedClient);
            GcDownloadStep1CurrentlySelectedClient.Foreground = System.Windows.Media.Brushes.DarkGreen;

            //get file paths set and make sure they work
            Logging.Info("checking if path to {0} is valid", GameInfoXml);
            bool filePathsValid = true;
            gameInfoXmlPath = Path.Combine(SelectedClient, GameInfoXml);
            metaDataXmlPath = Path.Combine(SelectedClient, GameMetadataFolder, MetaDataXml);
            StringBuilder missingFilesBuilder = new StringBuilder();
            if (!File.Exists(gameInfoXmlPath))
            {
                Logging.Error("{0} does not exist!", gameInfoXmlPath);
                filePathsValid = false;
                missingFilesBuilder.AppendLine(gameInfoXmlPath);
            }
            if (!File.Exists(metaDataXmlPath))
            {
                Logging.Error("{0} does not exist!", metaDataXmlPath);
                filePathsValid = false;
                missingFilesBuilder.AppendLine(metaDataXmlPath);
            }
            if (!filePathsValid)
            {
                MessageBox.Show(string.Format("{0}:{1}{2}", Translations.GetTranslatedString("GcMissingFiles"), System.Environment.NewLine, missingFilesBuilder.ToString()));
                GcDownloadStep1ResetParams(false, false);
                return;
            }

            Logging.Info("All required files found, collecting data for GET request");
            bool gotAllValues = true;
            //loop to get all the params
            for (int i = 0; i < GameCenterProperties.Length; i++)
            {
                GameCenterProperty gameCenterProperty = GameCenterProperties[i];
                string completeLocationPath = gameCenterProperty.FileName.Equals(GameInfoXml) ? gameInfoXmlPath : metaDataXmlPath;
                Logging.Info("getting property '{0}' for file {1}", gameCenterProperty.Xpath, gameCenterProperty.FileName);
                gameCenterProperty.Value = XmlUtils.GetXmlStringFromXPath(completeLocationPath, gameCenterProperty.Xpath);
                if (string.IsNullOrWhiteSpace(gameCenterProperty.Value))
                {
                    if (gameCenterProperty.IsRequired)
                    {
                        Logging.Error("Failure getting property!");
                        gameCenterProperty.GaveError = true;
                        if(gameCenterProperty.TextBlock != null)
                        {
                            gameCenterProperty.TextBlock.Text = Translations.GetTranslatedString("error");
                            gameCenterProperty.TextBlock.Foreground = System.Windows.Media.Brushes.Red;
                        }
                        gotAllValues = false;
                    }
                    else
                    {
                        gameCenterProperty.GaveError = false;
                        Logging.Warning("Did not get property, but IsRequired=False");
                        if (gameCenterProperty.TextBlock != null)
                        {
                            gameCenterProperty.TextBlock.Text = Translations.GetTranslatedString("none");
                            gameCenterProperty.TextBlock.Foreground = System.Windows.Media.Brushes.Orange;
                        }
                    }
                }
                else
                {
                    Logging.Info("Success getting property!");
                    gameCenterProperty.GaveError = false;
                    if (gameCenterProperty.TextBlock != null)
                    {
                        gameCenterProperty.TextBlock.Text = gameCenterProperty.Value;
                        gameCenterProperty.TextBlock.Foreground = System.Windows.Media.Brushes.DarkGreen;
                    }
                }
            }


            if (gotAllValues)
            {
                Logging.Info("All GET parameters found!");
                GcDownloadStep1Next.IsEnabled = true;
            }
            else
            {
                Logging.Info("Not all GET parameters found!");
                GcDownloadStep1ResetParams(false, false);
                return;
            }
        }

        private void GcDownloadStep2Init()
        {
            //start timer
            if(timer == null)
            {
                timer = new Timer(1000);
                timer.Elapsed += Timer_Elapsed;
            }
            //timer.Start();
            Timer_Elapsed(null, null);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //get list of processes for WG game center
            Process[] processes = Process.GetProcesses().Where(process => process.ProcessName.Equals(WgcProcessName)).ToArray();
            if(processes.Count() == 0)
            {
                //not running
                GcDownloadStep2GcStatus.Foreground = System.Windows.Media.Brushes.DarkGreen;
                GcDownloadStep2GcStatus.Text = string.Format(Translations.GetTranslatedString("GcDownloadStep2GcStatus")
                    , Translations.GetTranslatedString("GcDownloadStep2GcStatusClosed"));
                GcDownloadStep2NextButton.IsEnabled = true;
            }
            else
            {
                //running
                GcDownloadStep2GcStatus.Foreground = System.Windows.Media.Brushes.Red;
                GcDownloadStep2GcStatus.Text = string.Format(Translations.GetTranslatedString("GcDownloadStep2GcStatus")
                    , Translations.GetTranslatedString("GcDownloadStep2GcStatusOpened"));
                GcDownloadStep2NextButton.IsEnabled = true;
            }
        }

        private void GcDownloadStep3Init()
        {
            //get patch list info
        }

        private void GcDownloadStep4Init()
        {
            //create folder if not exist

            //download patch files to correct location
        }

        private void GcDownloadStep5Init()
        {
            //stub
        }

        private void GcDownloadStep1Next_Click(object sender, RoutedEventArgs e)
        {
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep2;
            GcDownloadStep2Init();
        }

        private void GcDownloadStep2PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null)
                timer.Stop();
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep1;
        }

        private void GcDownloadStep2NextButton_Click(object sender, RoutedEventArgs e)
        {
            if (timer != null)
                timer.Stop();
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep3;
        }

        private void GcDownloadStep3PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep2;
            GcDownloadStep2Init();
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

        private void RelhaxWindow_Closed(object sender, System.EventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }
    }
}
