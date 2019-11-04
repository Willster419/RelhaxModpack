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
using System.Web;
using System.Net;
using System.Xml;

namespace RelhaxModpack.Windows
{
    public class GameCenterProperty
    {
        public string FileName = string.Empty;
        public string Xpath = string.Empty;
        public TextBlock TextBlock = null;
        public string Value = string.Empty;
        public bool GaveError = false;
        public bool IsRequired = true;
    }
    public class PatchFileProperty
    {
        public string Filename = string.Empty;
        public string FolderName = string.Empty;
        public string BaseURL = string.Empty;
        public ulong Size = 0;
    }
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

        

        private GameCenterProperty ClientType, Language_, MetadataVersion, MetadataProtocalVersion, ChainID,
            ClientCurrentVersion, LocaleCurrentVersion, SdContentCurrentVersion, HdContentCurrentVersion, GameId;

        private GameCenterProperty[] GameCenterProperties = null;

        private List<PatchFileProperty> PatchFileProperties = null;

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
                        Logging.Warning("Did not get property '{0}, but IsRequired=False", gameCenterProperty.Xpath);
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
                GameCenterProperties[i] = gameCenterProperty;
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
            timer.Start();
            Timer_Elapsed(null, null);
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            //get list of processes for WG game center
            Process[] processes = Process.GetProcesses().Where(process => process.ProcessName.Equals(WgcProcessName)).ToArray();
            this.Dispatcher.Invoke(() =>
            {
                if (processes.Count() == 0)
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
                    GcDownloadStep2NextButton.IsEnabled = false;
                }
            });
        }

        private async void GcDownloadStep3Init()
        {
            GcDownloadStep3NextButton.IsEnabled = false;
            //get patch list info
            GcDownloadStep3StackPanel.Children.Clear();
            GcDownloadStep3StackPanel.Children.Add(new TextBlock() { Text = Translations.GetTranslatedString("loading") + "..." });

            //build the get request
            Logging.Info("Building GET request");
            StringBuilder requestBuilder = new StringBuilder();
            requestBuilder.Append("https://wgus-wotna.wargaming.net/api/v1/patches_chain/?protocol_version=1.8");
            requestBuilder.AppendFormat("&client_type={0}", ClientType.Value);
            requestBuilder.AppendFormat("&lang={0}", Language_.Value);
            requestBuilder.AppendFormat("&metadata_version={0}",MetadataVersion.Value);
            requestBuilder.AppendFormat("&metadata_protocol_version={0}",MetadataProtocalVersion.Value);
            requestBuilder.AppendFormat("&chain_id={0}",ChainID.Value);
            requestBuilder.AppendFormat("&installation_id=relhax_update_request");
            requestBuilder.AppendFormat("&client_current_version={0}",0);//ClientCurrentVersion.Value
            requestBuilder.AppendFormat("&locale_current_version={0}",0);//LocaleCurrentVersion.Value
            requestBuilder.AppendFormat("&sdcontent_current_version={0}",0);//SdContentCurrentVersion.Value
            if (!string.IsNullOrEmpty(HdContentCurrentVersion.Value))
            {
                requestBuilder.AppendFormat("&hdcontent_current_version={0}",HdContentCurrentVersion.Value);//HdContentCurrentVersion.Value
            }
            requestBuilder.AppendFormat("&game_id={0}",GameId.Value);
            Logging.Info("Built GET request: {0}", requestBuilder.ToString());

            XmlDocument xmlDocument = null;
            //download based on request
            using (WebClient client = new WebClient())
            {
                string xmlString = string.Empty;
                try
                {
                    xmlString = await client.DownloadStringTaskAsync(requestBuilder.ToString());
                }
                catch (Exception ex)
                {
                    Logging.Exception(ex.ToString());
                    Logging.Error("Failed to get WG patch instructions (webClient)");
                    GcDownloadStep3StackPanel.Children.Clear();
                    GcDownloadStep3StackPanel.Children.Add(new TextBlock() { Text = Translations.GetTranslatedString("error")});
                    GcDownloadStep3NextButton.IsEnabled = false;
                    return;
                }
                if(string.IsNullOrWhiteSpace(xmlString))
                {
                    Logging.Error("Failed to get WG patch instructions (empty string)");
                    GcDownloadStep3StackPanel.Children.Clear();
                    GcDownloadStep3StackPanel.Children.Add(new TextBlock() { Text = Translations.GetTranslatedString("error") });
                    GcDownloadStep3NextButton.IsEnabled = false;
                    return;
                }
                xmlDocument = XmlUtils.LoadXmlDocument(xmlString, XmlLoadType.FromString);
                if(xmlDocument == null)
                {
                    Logging.Error("Failed to get WG patch instructions (XML parse error)");
                    GcDownloadStep3StackPanel.Children.Clear();
                    GcDownloadStep3StackPanel.Children.Add(new TextBlock() { Text = Translations.GetTranslatedString("error") });
                    GcDownloadStep3NextButton.IsEnabled = false;
                    return;
                }
            }

            //get all file notes
            XmlNodeList fileList = XmlUtils.GetXmlNodesFromXPath(xmlDocument, @"//file");
            if (fileList.Count == 0)
            {
                Logging.Error("Failed to get WG patch instructions (empty node list with xpath '//file')");
                GcDownloadStep3StackPanel.Children.Clear();
                GcDownloadStep3StackPanel.Children.Add(new TextBlock() { Text = Translations.GetTranslatedString("noFilesUpToDate") });
                GcDownloadStep3NextButton.IsEnabled = false;
                return;
            }

            //build list of files to download
            string baseURL = XmlUtils.GetXmlStringFromXPath(xmlDocument, @"//protocol/patches_chain/web_seeds/url");
            if(string.IsNullOrEmpty(baseURL))
            {
                Logging.Error("Failed to get WG patch instructions (XML parse error of web_seeds)");
                GcDownloadStep3StackPanel.Children.Clear();
                GcDownloadStep3StackPanel.Children.Add(new TextBlock() { Text = Translations.GetTranslatedString("error") });
                GcDownloadStep3NextButton.IsEnabled = false;
                return;
            }
            Logging.Info("BaseURL parsed as {0}", baseURL);


            if (PatchFileProperties == null)
                PatchFileProperties = new List<PatchFileProperty>();
            PatchFileProperties.Clear();
            foreach (XmlNode element in fileList)
            {
                PatchFileProperty prop = new PatchFileProperty
                {
                    BaseURL = baseURL
                };

                //size
                XmlNode node = element.SelectSingleNode(@"size");
                if(node == null)
                {
                    Logging.Error("Failed to get WG patch instructions (XML parse error of nodes in fileList)");
                    GcDownloadStep3StackPanel.Children.Clear();
                    GcDownloadStep3StackPanel.Children.Add(new TextBlock() { Text = Translations.GetTranslatedString("error") });
                    GcDownloadStep3NextButton.IsEnabled = false;
                    return;
                }
                prop.Size = ulong.Parse(node.InnerText);

                //name
                node = element.SelectSingleNode(@"name");
                if (node == null)
                {
                    Logging.Error("Failed to get WG patch instructions (XML parse error of nodes in fileList)");
                    GcDownloadStep3StackPanel.Children.Clear();
                    GcDownloadStep3StackPanel.Children.Add(new TextBlock() { Text = Translations.GetTranslatedString("error") });
                    GcDownloadStep3NextButton.IsEnabled = false;
                    return;
                }
                prop.Filename = node.InnerText.Split('/')[1];
                prop.FolderName = node.InnerText.Split('/')[0];

                PatchFileProperties.Add(prop);
                Logging.Info("Parsed Patch File: Filename={0}, FolderName={1}, Size={2}", prop.Filename, prop.FolderName, prop.Size);
            }

            //display them to UI
            GcDownloadStep3StackPanel.Children.Clear();
            foreach (PatchFileProperty pfp in PatchFileProperties)
            {
                GcDownloadStep3StackPanel.Children.Add(new TextBlock()
                {
                    Text = string.Format("File={0}, Size={1}", pfp.Filename, Utils.SizeSuffix(pfp.Size, 1, true, false))
                });
            }
            GcDownloadStep3NextButton.IsEnabled = true;
        }

        private async void GcDownloadStep4Init()
        {
            GcDownloadStep4NextButton.IsEnabled = false;
            GcDownloadStep4SingleFileProgress.Minimum = 0;
            GcDownloadStep4SingleFileProgress.Maximum = 100;
            GcDownloadStep4SingleFileProgress.Value = 0;
            GcDownloadStep4TotalFileProgress.Minimum = 0;
            GcDownloadStep4TotalFileProgress.Maximum = PatchFileProperties.Count;
            GcDownloadStep4TotalFileProgress.Value = 0;

            //create folders if not exist
            string updatesRoot = Path.Combine(SelectedClient, "updates");
            List<string> folderNames = PatchFileProperties.Select(pfp => pfp.FolderName).ToList().Distinct().ToList();
            foreach(string folder in folderNames)
            {
                string completePath = Path.Combine(updatesRoot, folder);
                Logging.Info("Creating path {0} if does not already exists {0}", completePath);
                if (!Directory.Exists(completePath))
                    Directory.CreateDirectory(completePath);
            }

            //download patch files to correct location
            using (WebClient client = new WebClient())
            {
                client.DownloadProgressChanged += Client_DownloadProgressChanged;
                foreach (PatchFileProperty patchFile in PatchFileProperties)
                {
                    GcDownloadStep4DownloadingText.Text = string.Format(Translations.GetTranslatedString("GcDownloadStep4DownloadingText"),
                        GcDownloadStep4TotalFileProgress.Value + 1, PatchFileProperties.Count, patchFile.Filename);
                    Logging.Info("Downloading patch file {0}", patchFile.Filename);
                    string completeURL = string.Format("{0}{1}/{2}", patchFile.BaseURL, patchFile.FolderName, patchFile.Filename);
                    Logging.Info("Download URL is {0}", completeURL);
                    try
                    {
                        await client.DownloadFileTaskAsync(completeURL, Path.Combine(updatesRoot, patchFile.FolderName, patchFile.Filename));
                    }
                    catch (Exception ex)
                    {
                        Logging.Exception(ex.ToString());
                        GcDownloadStep4DownloadingText.Text = Translations.GetTranslatedString("error");
                        GcDownloadStep4DownloadingText.Foreground = System.Windows.Media.Brushes.Red;
                        Logging.Error("Failed to download file {0} using URL {1}", patchFile.Filename, completeURL);
                    }
                    GcDownloadStep4TotalFileProgress.Value++;
                }
            }
            GcDownloadStep4DownloadingText.Text = Translations.GetTranslatedString("GcDownloadStep4DownloadComplete");
            GcDownloadStep4DownloadingText.Foreground = System.Windows.Media.Brushes.DarkGreen;
            GcDownloadStep4NextButton.IsEnabled = true;
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            GcDownloadStep4SingleFileProgress.Maximum = e.TotalBytesToReceive;
            GcDownloadStep4SingleFileProgress.Value = e.BytesReceived;
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

            GcDownloadStep3Init();
        }

        private void GcDownloadStep3PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep2;
            GcDownloadStep2Init();
        }

        private void GcDownloadStep3NextButton_Click(object sender, RoutedEventArgs e)
        {
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep4;
            GcDownloadStep4Init();
        }

        private void GcDownloadStep4PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            GcDownloadMainTabControl.SelectedItem = GcDownloadStep3;
            GcDownloadStep3Init();
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
