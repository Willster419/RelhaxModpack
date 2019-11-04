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
        public TextBlock ValueBlock = null;
        public TextBlock KeyBlock = null;
        public string Value = string.Empty;
        public string GetRequestParamater = string.Empty;
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

        private List<GameCenterProperty> GameCenterProperties = null;
        private List<PatchFileProperty> PatchFileProperties = null;
        private bool init = true;
        private string gameInfoXmlPath = string.Empty;
        private string metaDataXmlPath = string.Empty;
        private Timer timer = null;
        private WebClient client = null;
        private bool step4DownloadCanceled = false;

        public GameCenterUpdateDownloader()
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {

            foreach (TabItem item in GcDownloadMainTabControl.Items)
            {
                item.Background = UISettings.CurrentTheme.TabItemColorset.BackgroundBrush.Brush;
            }

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
                item.Background = UISettings.CurrentTheme.TabItemColorset.BackgroundBrush.Brush;
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
                Filter = "WG Client|WorldOfTanks.*;WorldOfWarships.*;WorldOfWarplanes.*",
                Multiselect = false,
                ValidateNames = true,
                Title = Translations.GetTranslatedString("GcDownloadSelectWgClient")
            };
            if ((bool)manualWoTFind.ShowDialog())
            {
                GcDownloadStep1ResetParams(true, true);
                SelectedClient = Path.GetDirectoryName(manualWoTFind.FileName);
                //replace the 'win32' or 'win64' directory with nothing (so removing it)
                SelectedClient = SelectedClient.Replace(Settings.WoT32bitFolderWithSlash, string.Empty).Replace(Settings.WoT64bitFolderWithSlash, string.Empty);
                Logging.Info("GameCenterDownloader: Selected install -> {0}", SelectedClient);
                GcDownloadStep1SetupArray();
                GcDownloadStep1GetParams();
            }
        }

        private void GcDownloadStep1Init()
        {
            if (GameCenterProperties == null)
                GameCenterProperties = new List<GameCenterProperty>();
            GameCenterProperties.Clear();
            GcDownloadStep1KeyValueGrid.Children.Clear();

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
                GcDownloadStep1SetupArray();
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
                if (GameCenterProperties == null)
                    GameCenterProperties = new List<GameCenterProperty>();
                GameCenterProperties.Clear();
                GcDownloadStep1KeyValueGrid.Children.Clear();
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
            GcDownloadStep1KeyValueGrid.Children.Clear();

            //loop to get all the params
            for (int i = 0; i < GameCenterProperties.Count; i++)
            {
                GameCenterProperty gameCenterProperty = GameCenterProperties[i];

                gameCenterProperty.KeyBlock = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Text = gameCenterProperty.GetRequestParamater,
                    Foreground = UISettings.CurrentTheme.TextblockColorset.ForegroundBrush.Brush
                };
                //https://stackoverflow.com/questions/18659435/programmatically-add-label-to-grid
                Grid.SetColumn(gameCenterProperty.KeyBlock, 0);
                Grid.SetRow(gameCenterProperty.KeyBlock, i);
                GcDownloadStep1KeyValueGrid.Children.Add(gameCenterProperty.KeyBlock);

                gameCenterProperty.ValueBlock = new TextBlock()
                {
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch
                };
                Grid.SetColumn(gameCenterProperty.ValueBlock, 1);
                Grid.SetRow(gameCenterProperty.ValueBlock, i);
                GcDownloadStep1KeyValueGrid.Children.Add(gameCenterProperty.ValueBlock);

                string completeLocationPath = gameCenterProperty.FileName.Equals(GameInfoXml) ? gameInfoXmlPath : metaDataXmlPath;
                Logging.Info("getting property '{0}' for file {1}", gameCenterProperty.Xpath, gameCenterProperty.FileName);
                gameCenterProperty.Value = XmlUtils.GetXmlStringFromXPath(completeLocationPath, gameCenterProperty.Xpath);
                if (string.IsNullOrWhiteSpace(gameCenterProperty.Value))
                {
                    if (gameCenterProperty.IsRequired)
                    {
                        Logging.Error("Failure getting property!");
                        gameCenterProperty.GaveError = true;
                        if(gameCenterProperty.ValueBlock != null)
                        {
                            gameCenterProperty.ValueBlock.Text = Translations.GetTranslatedString("error");
                            gameCenterProperty.ValueBlock.Foreground = System.Windows.Media.Brushes.Red;
                        }
                        gotAllValues = false;
                    }
                    else
                    {
                        gameCenterProperty.GaveError = false;
                        Logging.Warning("Did not get property '{0}, but IsRequired=False", gameCenterProperty.Xpath);
                        if (gameCenterProperty.ValueBlock != null)
                        {
                            gameCenterProperty.ValueBlock.Text = Translations.GetTranslatedString("none");
                            gameCenterProperty.ValueBlock.Foreground = System.Windows.Media.Brushes.Orange;
                        }
                    }
                }
                else
                {
                    Logging.Info("Success getting property!");
                    gameCenterProperty.GaveError = false;
                    if (gameCenterProperty.ValueBlock != null)
                    {
                        gameCenterProperty.ValueBlock.Text = gameCenterProperty.Value;
                        gameCenterProperty.ValueBlock.Foreground = System.Windows.Media.Brushes.DarkGreen;
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

        private void GcDownloadStep1SetupArray()
        {
            string compareID = string.Empty;
            //common
            GameCenterProperties.Add(new GameCenterProperty()
            {
                FileName = MetaDataXml,
                Xpath = @"//protocol/@version",
                GetRequestParamater = "metadata_protocol_version",
                IsRequired = true
            });
            GameCenterProperties.Add(new GameCenterProperty()
            {
                FileName = GameInfoXml,
                Xpath = @"//protocol/game/part_versions/value[@name='client']",
                GetRequestParamater = "client_current_version",
                IsRequired = true
            });
            GameCenterProperties.Add(new GameCenterProperty()
            {
                FileName = GameInfoXml,
                Xpath = @"//protocol/game/id",
                GetRequestParamater = "game_id",
                IsRequired = true
            });
            GameCenterProperty idProp = GameCenterProperties[GameCenterProperties.Count - 1];
            string pathToGameId = Path.Combine(SelectedClient, GameInfoXml);
            //set wot as default
            idProp.Value = "wot";
            if(!File.Exists(pathToGameId))
            {
                Logging.Error("xml file {0} does not exist!");
            }
            else
            {
                idProp.Value = XmlUtils.GetXmlStringFromXPath(pathToGameId, idProp.Xpath);
                Logging.Info("id processed as {0}", idProp.Value);
            }
            GameCenterProperties.Add(new GameCenterProperty()
            {
                FileName = GameInfoXml,
                Xpath = @"//protocol/game/client_type",
                GetRequestParamater = "client_type",
                IsRequired = true
            });
            GameCenterProperties.Add(new GameCenterProperty()
            {
                FileName = GameInfoXml,
                Xpath = @"//protocol/game/localization",
                GetRequestParamater = "lang",
                IsRequired = true
            });
            GameCenterProperties.Add(new GameCenterProperty()
            {
                FileName = GameInfoXml,
                Xpath = @"//protocol/game/update_urls/value",
                GetRequestParamater = "BASE_URL",
                IsRequired = true
            });

            //different
            if (idProp.Value.ToLower().Contains("wot"))
            {
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = MetaDataXml,
                    Xpath = @"//protocol/predefined_section/chain_id",
                    GetRequestParamater = "chain_id",
                    IsRequired = true
                });
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = MetaDataXml,
                    Xpath = @"//protocol/version",
                    GetRequestParamater = "metadata_version",
                    IsRequired = true
                });
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = GameInfoXml,
                    Xpath = @"//protocol/game/part_versions/value[@name='locale']",
                    GetRequestParamater = "locale_current_version",
                    IsRequired = true
                });
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = GameInfoXml,
                    Xpath = @"//protocol/game/part_versions/value[@name='sdcontent']",
                    GetRequestParamater = "sdcontent_current_version",
                    IsRequired = true
                });
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = GameInfoXml,
                    Xpath = @"//protocol/game/part_versions/value[@name='hdcontent']",
                    GetRequestParamater = "hdcontent_current_version",
                    IsRequired = false
                });
            }
            else if (idProp.Value.ToLower().Contains("wows"))
            {
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = MetaDataXml,
                    Xpath = @"//protocol/predefined_section/chain_id",
                    GetRequestParamater = "chain_id",
                    IsRequired = true
                });
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = MetaDataXml,
                    Xpath = @"//protocol/version",
                    GetRequestParamater = "metadata_version",
                    IsRequired = true
                });
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = GameInfoXml,
                    Xpath = @"//protocol/game/part_versions/value[@name='locale']",
                    GetRequestParamater = "locale_current_version",
                    IsRequired = true
                });
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = GameInfoXml,
                    Xpath = @"//protocol/game/part_versions/value[@name='sdcontent']",
                    GetRequestParamater = "sdcontent_current_version",
                    IsRequired = true
                });
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = GameInfoXml,
                    Xpath = @"//protocol/game/part_versions/value[@name='udsound']",
                    GetRequestParamater = "udsound_current_version",
                    IsRequired = false
                });
            }
            else if (idProp.Value.ToLower().Contains("wowp"))
            {
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = MetaDataXml,
                    Xpath = @"//protocol/chain_id",
                    GetRequestParamater = "chain_id",
                    IsRequired = true
                });
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = MetaDataXml,
                    Xpath = @"//protocol/metadata_version",
                    GetRequestParamater = "metadata_version",
                    IsRequired = true
                });
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = GameInfoXml,
                    Xpath = @"//protocol/game/part_versions/value[@name='wwlocale']",
                    GetRequestParamater = "wwlocale_current_version",
                    IsRequired = true
                });
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = GameInfoXml,
                    Xpath = @"//protocol/game/part_versions/value[@name='content']",
                    GetRequestParamater = "content_current_version",
                    IsRequired = true
                });
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = GameInfoXml,
                    Xpath = @"//protocol/game/part_versions/value[@name='hdcontent']",
                    GetRequestParamater = "hdcontent_current_version",
                    IsRequired = false
                });
                GameCenterProperties.Add(new GameCenterProperty()
                {
                    FileName = GameInfoXml,
                    Xpath = @"//protocol/game/part_versions/value[@name='tm']",
                    GetRequestParamater = "tm_current_version",
                    IsRequired = true
                });
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
            List<GameCenterProperty> baseUrlPropertyList = GameCenterProperties.Where(gc => gc.GetRequestParamater.Equals("BASE_URL")).ToList();
            if(baseUrlPropertyList == null || baseUrlPropertyList.Count == 0)
            {
                Logging.Error("Failed to get WG patch instructions (getting BASE_URL)");
                GcDownloadStep3StackPanel.Children.Clear();
                GcDownloadStep3StackPanel.Children.Add(new TextBlock()
                {
                    Text = Translations.GetTranslatedString("error"),
                    Foreground = UISettings.CurrentTheme.TextblockColorset.ForegroundBrush.Brush
                });
                GcDownloadStep3NextButton.IsEnabled = false;
                return;
            }
            GameCenterProperty urlProperty = baseUrlPropertyList[0];
            requestBuilder.AppendFormat("{0}api/v1/patches_chain/?protocol_version=1.8", urlProperty.Value);
            foreach(GameCenterProperty gameCenterProperty in GameCenterProperties)
            {
                if (string.IsNullOrWhiteSpace(gameCenterProperty.Value) && !gameCenterProperty.IsRequired)
                    continue;
                else if (gameCenterProperty.GetRequestParamater.Equals("BASE_URL"))
                    continue;
                requestBuilder.AppendFormat("&{0}={1}", gameCenterProperty.GetRequestParamater, gameCenterProperty.Value);
            }
            requestBuilder.AppendFormat("&{0}={1}", "installation_id", "relhax_update_request");
            Logging.Info("Built GET request: {0}", requestBuilder.ToString());

            XmlDocument xmlDocument = null;
            //download based on request
            using (client = new WebClient())
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
                    GcDownloadStep3StackPanel.Children.Add(new TextBlock()
                    {
                        Text = Translations.GetTranslatedString("error"),
                        Foreground = UISettings.CurrentTheme.TextblockColorset.ForegroundBrush.Brush
                    });
                    GcDownloadStep3NextButton.IsEnabled = false;
                    return;
                }
                if(string.IsNullOrWhiteSpace(xmlString))
                {
                    Logging.Error("Failed to get WG patch instructions (empty string)");
                    GcDownloadStep3StackPanel.Children.Clear();
                    GcDownloadStep3StackPanel.Children.Add(new TextBlock()
                    {
                        Text = Translations.GetTranslatedString("error"),
                        Foreground = UISettings.CurrentTheme.TextblockColorset.ForegroundBrush.Brush
                    });
                    GcDownloadStep3NextButton.IsEnabled = false;
                    return;
                }
                xmlDocument = XmlUtils.LoadXmlDocument(xmlString, XmlLoadType.FromString);
                if(xmlDocument == null)
                {
                    Logging.Error("Failed to get WG patch instructions (XML parse error)");
                    GcDownloadStep3StackPanel.Children.Clear();
                    GcDownloadStep3StackPanel.Children.Add(new TextBlock()
                    {
                        Text = Translations.GetTranslatedString("error"),
                        Foreground = UISettings.CurrentTheme.TextblockColorset.ForegroundBrush.Brush
                    });
                    GcDownloadStep3NextButton.IsEnabled = false;
                    return;
                }
            }

            //get all file notes
            XmlNodeList fileList = XmlUtils.GetXmlNodesFromXPath(xmlDocument, @"//file");
            if (fileList == null || fileList.Count == 0)
            {
                Logging.Info("0 WG patches (empty node list with xpath '//file')");
                GcDownloadStep3StackPanel.Children.Clear();
                GcDownloadStep3StackPanel.Children.Add(new TextBlock()
                {
                    Text = Translations.GetTranslatedString("GcDownloadStep3NoFilesUpToDate"),
                    Foreground = UISettings.CurrentTheme.TextblockColorset.ForegroundBrush.Brush
                });
                GcDownloadStep3NextButton.IsEnabled = false;
                return;
            }

            //build list of files to download
            string baseURL = XmlUtils.GetXmlStringFromXPath(xmlDocument, @"//protocol/patches_chain/web_seeds/url");
            if(string.IsNullOrEmpty(baseURL))
            {
                Logging.Error("Failed to get WG patch instructions (XML parse error of web_seeds)");
                GcDownloadStep3StackPanel.Children.Clear();
                GcDownloadStep3StackPanel.Children.Add(new TextBlock()
                {
                    Text = Translations.GetTranslatedString("error"),
                    Foreground = UISettings.CurrentTheme.TextblockColorset.ForegroundBrush.Brush
                });
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
                    GcDownloadStep3StackPanel.Children.Add(new TextBlock()
                    {
                        Text = Translations.GetTranslatedString("error"),
                        Foreground = UISettings.CurrentTheme.TextblockColorset.ForegroundBrush.Brush
                    });
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
                    GcDownloadStep3StackPanel.Children.Add(new TextBlock()
                    {
                        Text = Translations.GetTranslatedString("error"),
                        Foreground = UISettings.CurrentTheme.TextblockColorset.ForegroundBrush.Brush
                    });
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
                    Text = string.Format("File={0}, Size={1}", pfp.Filename, Utils.SizeSuffix(pfp.Size, 1, true, false)),
                    Foreground = UISettings.CurrentTheme.TextblockColorset.ForegroundBrush.Brush
                });
            }
            GcDownloadStep3NextButton.IsEnabled = true;
        }

        private async void GcDownloadStep4Init()
        {
            step4DownloadCanceled = false;
            GcDownloadStep4PreviousButton.IsEnabled = true;
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
            using (client = new WebClient())
            {
                client.DownloadProgressChanged += Client_DownloadProgressChanged;
                foreach (PatchFileProperty patchFile in PatchFileProperties)
                {
                    GcDownloadStep4DownloadingText.Text = string.Format(Translations.GetTranslatedString("GcDownloadStep4DownloadingText"),
                        GcDownloadStep4TotalFileProgress.Value + 1, PatchFileProperties.Count, patchFile.Filename);
                    Logging.Info("Downloading patch file {0}", patchFile.Filename);
                    string completeURL = string.Format("{0}{1}/{2}", patchFile.BaseURL, patchFile.FolderName, patchFile.Filename);
                    string completePath = Path.Combine(updatesRoot, patchFile.FolderName, patchFile.Filename);
                    Logging.Info("Download URL is {0}", completeURL);
                    try
                    {
                        if (File.Exists(completePath))
                            File.Delete(completePath);
                        await client.DownloadFileTaskAsync(completeURL, completePath);
                    }
                    catch (Exception ex)
                    {
                        if(step4DownloadCanceled)
                        {
                            Logging.Info("Successfully processed cancel async");
                            GcDownloadStep4DownloadingText.Text = Translations.GetTranslatedString("canceled");
                            GcDownloadStep4DownloadingSizes.Text = string.Empty;
                            step4DownloadCanceled = false;
                            GcDownloadStep4PreviousButton.IsEnabled = true;
                            GcDownloadStep4NextButton.IsEnabled = false;
                            return;
                        }
                        Logging.Exception(ex.ToString());
                        string temp = ex.GetType().ToString();
                        GcDownloadStep4DownloadingText.Text = Translations.GetTranslatedString("error");
                        GcDownloadStep4DownloadingText.Foreground = System.Windows.Media.Brushes.Red;
                        Logging.Error("Failed to download file {0} using URL {1}", patchFile.Filename, completeURL);
                        return;
                    }
                    GcDownloadStep4TotalFileProgress.Value++;
                }
            }
            GcDownloadStep4DownloadingText.Text = Translations.GetTranslatedString("GcDownloadStep4DownloadComplete");
            GcDownloadStep4DownloadingText.Foreground = System.Windows.Media.Brushes.DarkGreen;
            GcDownloadStep4NextButton.IsEnabled = true;
            GcDownloadStep4PreviousButton.IsEnabled = false;
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            GcDownloadStep4DownloadingSizes.Text = string.Format("{0} {1} {2}", Utils.SizeSuffix((ulong)e.BytesReceived,1,true,false),
                Translations.GetTranslatedString("of"), Utils.SizeSuffix((ulong)e.TotalBytesToReceive, 1, true, false));
            GcDownloadStep4SingleFileProgress.Maximum = e.TotalBytesToReceive;
            GcDownloadStep4SingleFileProgress.Value = e.BytesReceived;
        }

        private void GcDownloadStep4DownloadingCancelButton_Click(object sender, RoutedEventArgs e)
        {
            Logging.Info("Processing cancel");
            step4DownloadCanceled = true;
            if (client != null)
                client.CancelAsync();
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
            GcDownloadStep1Init();
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

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }
    }
}
