using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RelhaxModpack.UI;
using System.IO;
using System.Net;
using System.Windows.Threading;
using RelhaxModpack.Database;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Settings;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for Preview.xaml
    /// </summary>
    public partial class Preview : RelhaxWindow
    {
        /// <summary>
        /// Get or set the list of media preview components
        /// </summary>
        public List<Media> Medias { get; set; } = null;

        /// <summary>
        /// Sets if the preview was launched from the editor or from the selection list
        /// </summary>
        public bool EditorMode { get; set; } = false;

        /// <summary>
        /// Get or set if the package received contains media from multiple packages from a combobox
        /// </summary>
        public bool ComboBoxItemsInsideMode { get; set; } = false;

        /// <summary>
        /// Get or set the package that invoked the preview window
        /// </summary>
        public SelectablePackage InvokedPackage = null;

        private Media CurrentDispalyMedia = null;
        private SelectablePackage CurrentDisplaySP = null;
        private MemoryStream ImageStream = null;
        private WebBrowser browser = null;
        private ZoomBorder zoomBorder = null;
        private DispatcherTimer OMCViewLegacyFocusTimer = null;

        /// <summary>
        /// Create an instance of the Preview window
        /// </summary>
        public Preview(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        private void OnPreviewWindowLoad(object sender, RoutedEventArgs e)
        {
            //make sure Medias is a valid entry
            if (Medias == null || InvokedPackage == null)
            {
                Logging.Error("Preview Medias list or InvokedPackage null: MediasNull?={0}, InvokedPacakgeNull?={1}", Medias==null, InvokedPackage==null);
                if(EditorMode)
                    MessageBox.Show(string.Format("Preview Medias list or InvokedPackage null: MediasNull?={0}, InvokedPacakgeNull?={1}", Medias == null, InvokedPackage == null));
                else
                    MessageBox.Show(Translations.GetTranslatedString("previewEncounteredError"));
                Close();
                return;
            }

            //translate 3 components
            DevUrlHeader.Text = Translations.GetTranslatedString(DevUrlHeader.Name);
            PreviewNextPicButton.Content = Translations.GetTranslatedString(PreviewNextPicButton.Name);
            PreviewPreviousPicButton.Content = Translations.GetTranslatedString(PreviewPreviousPicButton.Name);

            if (EditorMode)
            {
                Logging.Debug("EditorMode = true, ignoring setting for startup location");
                WindowStartupLocation = WindowStartupLocation.Manual;
            }
            else
            {
                //if the saved preview window point is within the screen, then load it to there
                if (UiUtils.PointWithinScreen(ModpackSettings.PreviewX, ModpackSettings.PreviewY))
                {
                    //set for manual window location setting
                    WindowStartupLocation = WindowStartupLocation.Manual;
                    //set starting location
                    //https://stackoverflow.com/questions/2734810/how-to-set-the-location-of-a-wpf-window
                    Left = ModpackSettings.PreviewX;
                    Top = ModpackSettings.PreviewY;
                }
                else
                {
                    Logging.Info("[{0}]: Position {1}x{2} is outside screen dimensions, use center of window owner",
                        Logfiles.Application, nameof(Preview), ModpackSettings.PreviewX, ModpackSettings.PreviewY);
                    WindowStartupLocation = WindowStartupLocation.CenterOwner;
                }

                //set width and height
                Width = ModpackSettings.PreviewWidth;
                Height = ModpackSettings.PreviewHeight;

                //set if full screen
                if (ModpackSettings.PreviewFullscreen)
                    WindowState = WindowState.Maximized;
            }

            //invoke displaying the first element
            Refresh(true);
        }

        /// <summary>
        /// Refresh the window to display new preview elements
        /// </summary>
        public void Refresh(bool init)
        {
            //check that all components have the package parent reference set
            foreach (Media media in Medias)
            {
                bool anyMediaErrors = false;
                if (media.SelectablePackageParent == null)
                {
                    Logging.Error("A media component does not have its SelectablePackageParent set");
                    Logging.Error(media.ToString());
                    anyMediaErrors = true;
                }
                if (anyMediaErrors)
                {
                    MessageBox.Show(Translations.GetTranslatedString("previewEncounteredError"));
                    Close();
                    return;
                }
            }
            CurrentDisplaySP = InvokedPackage;

            if (Medias.Count > 0)
            {
                DisplayMedia(Medias[0], true);
            }
            else
            {
                DisplayMedia(null, true);
            }

            //start the focus timer to bring focus to this window
            if (init && ModpackSettings.ModSelectionView == SelectionView.Legacy)
            {
                OMCViewLegacyFocusTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, Timer_Tick, this.Dispatcher) { IsEnabled = true };
            }
            else if (OMCViewLegacyFocusTimer == null)
            {
                OMCViewLegacyFocusTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(10), DispatcherPriority.Normal, Timer_Tick, this.Dispatcher) { IsEnabled = true };
            }
            else
            {
                OMCViewLegacyFocusTimer.Start();
            }
        }

        private async void DisplayMedia(Media media, bool fullRedraw)
        {
            //don't need to update if it's the same as what is currently displayed
            if (media != null && CurrentDispalyMedia != null && CurrentDispalyMedia.Equals(media))
            {
                Logging.Debug(LogOptions.ClassName, "No need to display media, is the same");
                return;
            }
            CurrentDispalyMedia = media;

            //devurl, description, update notes need to be changed if the parent package being displayed changed
            //this would happen only if we are in combobox mode (displaying medias of multiple packages)
            if (ComboBoxItemsInsideMode && media != null && (!CurrentDisplaySP.Equals(media.SelectablePackageParent)))
            {
                fullRedraw = true;
                CurrentDisplaySP = media.SelectablePackageParent;
            }

            if (fullRedraw)
            {
                ToggleDevUrlList(media, media == null? false : string.IsNullOrWhiteSpace(media.SelectablePackageParent.DevURL));

                //set the name of the window to be the package name
                if (EditorMode)
                {
                    Title = "EDITOR_TEST_MODE";
                }
                else if (ComboBoxItemsInsideMode)
                {
                    Title = string.Format("{0}: {1}", Translations.GetTranslatedString("dropDownItemsInside"), CurrentDisplaySP.NameFormatted);
                }
                else
                {
                    Title = CurrentDisplaySP.NameFormatted;
                }

                //set description
                PreviewDescriptionBox.Text = CurrentDisplaySP.DescriptionFormatted;

                //set update notes
                PreviewUpdatesBox.Text = CurrentDisplaySP.UpdateCommentFormatted;
            }

            //clear media url list and re-build it
            if (MediaIndexer.Children.Count > 0)
                MediaIndexer.Children.Clear();

            //make the linked labels in the link box
            for (int i = 0; i < Medias.Count; i++)
            {
                TextBlock block = new TextBlock();
                block.Inlines.Clear();
                Hyperlink h = new Hyperlink(new Run(i.ToString()))
                {
                    Tag = Medias[i],
                    //dummy URI just to make the request navigate work
                    NavigateUri = new Uri("https://google.com")
                };
                h.RequestNavigate += OnMediaHyperlinkClick;
                block.Inlines.Add(h);
                MediaIndexer.Children.Add(block);
            }

            HandlePlayerDisposal();

            //null the child element and make it again
            MainPreviewBorder.Child = null;
            if (media != null)
            {
                Logging.Debug(LogOptions.ClassName, "Loading preview of MediaType {0}, URL={1}", media.MediaType.ToString(), media.URL);
                Image pictureViewer;
                switch (media.MediaType)
                {
                    case MediaType.Unknown:
                    default:
                        Logging.Error("Invalid MediaType: {0}", media.MediaType.ToString());
                        return;
                    case MediaType.HTML:
                        if (browser != null)
                        {
                            browser.Dispose();
                            browser = null;
                        }
                        browser = new WebBrowser();
                        //https://stackoverflow.com/questions/2585782/displaying-html-from-string-in-wpf-webbrowser-control
                        browser.NavigateToString(media.URL);
                        MainPreviewBorder.Child = browser;
                        break;
                    case MediaType.MediaFile:
                        //show progress first
                        MainPreviewBorder.Child = new ProgressBar()
                        {
                            Minimum = 0,
                            Maximum = 1,
                            Value = 0,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(0, 20, 0, 0),
                            Height = 20
                        };
                        using (WebClient client = new WebClient() { })
                        {
                            client.DownloadProgressChanged += Client_DownloadProgressChanged;

                            //now load the media
                            try
                            {
                                byte[] data = await client.DownloadDataTaskAsync(media.URL);
                                MainPreviewBorder.Child = new RelhaxMediaPlayer(media.URL, data);
                            }
                            catch (Exception ex)
                            {
                                Logging.Exception("failed to load audio data");
                                Logging.Exception(ex.ToString());
                                pictureViewer = new Image
                                {
                                    ClipToBounds = true
                                };
                                pictureViewer.Source = CommonUtils.BitmapToImageSource(Properties.Resources.error_loading_picture);
                                MainPreviewBorder.Child = pictureViewer;
                            }
                        }
                        break;
                    case MediaType.Picture:
                        //https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.image?view=netframework-4.7.2
                        pictureViewer = new Image
                        {
                            ClipToBounds = true
                        };
                        MainContentControl.MouseRightButtonDown += MainContentControl_MouseRightButtonDown;
                        MainContentControl.PreviewMouseDoubleClick += MainContentControl_PreviewMouseDoubleClick;
                        MainPreviewBorder.Child = new ProgressBar()
                        {
                            Minimum = 0,
                            Maximum = 1,
                            Value = 0,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Top,
                            Margin = new Thickness(0, 20, 0, 0),
                            Height = 20
                        };
                        //https://stackoverflow.com/questions/9173904/byte-array-to-image-conversion
                        //https://stackoverflow.com/questions/18134234/how-to-convert-system-io-stream-into-an-image
                        using (WebClient client = new WebClient() { })
                        {
                            client.DownloadProgressChanged += Client_DownloadProgressChanged;
                            try
                            {
                                byte[] image = await client.DownloadDataTaskAsync(media.URL);
                                ImageStream = new MemoryStream(image);
                                BitmapImage bitmapImage = new BitmapImage();
                                bitmapImage.BeginInit();
                                bitmapImage.StreamSource = ImageStream;
                                bitmapImage.EndInit();
                                pictureViewer.Source = bitmapImage;
                            }
                            catch (Exception ex)
                            {
                                Logging.Exception("failed to load picture");
                                Logging.Exception(ex.ToString());
                                pictureViewer.Source = CommonUtils.BitmapToImageSource(Properties.Resources.error_loading_picture);
                            }
                        }
                        //put the zoom border inside the main preview one. already set, might as well use it
                        zoomBorder = new ZoomBorder()
                        {
                            Child = pictureViewer,
                            HorizontalAlignment = HorizontalAlignment.Stretch,
                            VerticalAlignment = VerticalAlignment.Stretch,
                            BorderThickness = new Thickness(1.0),
                            BorderBrush = Brushes.Black,
                            ClipToBounds = true
                        };
                        zoomBorder.SizeChanged += ZoomBorder_SizeChanged;
                        MainPreviewBorder.ClipToBounds = true;
                        MainPreviewBorder.BorderThickness = new Thickness(0.0);
                        MainPreviewBorder.Child = zoomBorder;
                        break;
                    case MediaType.Webpage:
                        if (browser != null)
                        {
                            browser.Dispose();
                            browser = null;
                        }
                        browser = new WebBrowser();
                        //https://stackoverflow.com/questions/2585782/displaying-html-from-string-in-wpf-webbrowser-control
                        browser.Navigate(media.URL);
                        MainPreviewBorder.Child = browser;
                        break;
                }
            }
        }

        private void ToggleDevUrlList(Media media, bool enable)
        {
            DevUrlHeader.IsEnabled = enable;
            DevUrlHeader.Visibility = enable ? Visibility.Visible : Visibility.Hidden;
            DevUrlHolder.IsEnabled = enable;
            DevUrlHolder.Visibility = enable ? Visibility.Visible : Visibility.Hidden;

            //clear last list of URL links
            if (DevUrlHolder.Children.Count > 0)
                DevUrlHolder.Children.Clear();

            if (enable)
            {
                //devURL is now array of elements separated by newline
                //load the stack with textblocks with tooltips for the URLs
                string[] devURLS = media.SelectablePackageParent.DevURLList;

                for (int i = 0; i < devURLS.Count(); i++)
                {
                    //make a URI to hold the goto devurl link
                    Uri goTo = null;
                    try
                    {
                        goTo = new Uri(devURLS[i].Trim());
                    }
                    catch (UriFormatException)
                    {
                        Logging.Error("Invalid URI string, skipping: {0}", devURLS[i].Trim());
                    }
                    if (goTo == null)
                        continue;

                    //make a textbox to hold the hyperlink object
                    TextBlock block = new TextBlock()
                    {
                        ToolTip = devURLS[i].Trim()
                    };
                    //https://stackoverflow.com/questions/21214450/how-to-add-a-hyperlink-in-a-textblock-in-code?noredirect=1&lq=1
                    block.Inlines.Clear();

                    //make a run to display the number of the link
                    Run inline = new Run(i.ToString());

                    //and the hyperlink will display the run
                    Hyperlink h = new Hyperlink(inline)
                    {
                        NavigateUri = goTo
                    };
                    h.RequestNavigate += OnHyperLinkClick;

                    //add hyperlink to textbox
                    block.Inlines.Add(h);

                    //add to developer url textbox
                    DevUrlHolder.Children.Add(block);
                }
            }
        }

        private void HandlePlayerDisposal()
        {
            //if the child is our media player, then stop and dispose
            if (MainPreviewBorder.Child != null && MainPreviewBorder.Child is RelhaxMediaPlayer player)
            {
                player.StopPlaybackIfPlaying();
                player.Dispose();
                player = null;
            }
        }

        #region Image mouse click and resizing
        private void MainContentControl_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //only work on left button
            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            //send cancel for the mouse click drag in the zoom border
            if (zoomBorder != null)
            {
                zoomBorder.CancelMouseDown = true;
            }

            if (WindowState != WindowState.Maximized)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void ZoomBorder_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            zoomBorder.Reset();
        }

        private void MainContentControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if the preview is still loading, then the preview is still loading, therefore zoomBorder is still null. Don't set it yet.
            zoomBorder?.Reset();
        }
        #endregion

        #region UI events
        private void Timer_Tick(object sender, EventArgs e)
        {
            OMCViewLegacyFocusTimer.Stop();
            this.Focus();
        }

        private void OnMediaHyperlinkClick(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (sender is Hyperlink link)
            {
                if (link.Tag is Media media)
                {
                    if (CurrentDispalyMedia == null || !media.Equals(CurrentDispalyMedia))
                        DisplayMedia(media, false);
                }
            }
        }

        private void OnHyperLinkClick(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(e.Uri.OriginalString);
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
            }
        }

        private void PreviewNextPicButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentDispalyMedia == null)
                return;
            int index = Medias.IndexOf(CurrentDispalyMedia);
            if (index < Medias.Count - 1)
            {
                DisplayMedia(Medias[index + 1], false);
            }
        }

        private void PreviewPreviousPicButton_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentDispalyMedia == null)
                return;
            int index = Medias.IndexOf(CurrentDispalyMedia);
            if (index > 0)
            {
                DisplayMedia(Medias[index - 1], false);
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (MainPreviewBorder.Child is ProgressBar bar)
            {
                if (bar.Maximum != e.TotalBytesToReceive)
                    bar.Maximum = e.TotalBytesToReceive;
                bar.Value = e.BytesReceived;
            }
        }
        #endregion

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            //save window location, size and fullscreen property (if not in editor mode)
            if (!EditorMode)
            {
                ModpackSettings.PreviewFullscreen = WindowState == WindowState.Maximized ? true : false;
                ModpackSettings.PreviewHeight = (int)Height;
                ModpackSettings.PreviewWidth = (int)Width;
                if (UiUtils.PointWithinScreen((int)Left, (int)Top))
                {
                    ModpackSettings.PreviewX = (int)Left;
                    ModpackSettings.PreviewY = (int)Top;
                }
            }

            Logging.Debug(LogOptions.ClassName, "Disposing media player");
            HandlePlayerDisposal();

            Logging.Debug(LogOptions.ClassName, "Disposing image memory stream");
            if (ImageStream != null)
            {
                ImageStream.Dispose();
                ImageStream = null;
            }

            Logging.Debug(LogOptions.ClassName, "Disposing browser");
            if (browser != null)
            {
                browser.Dispose();
                browser = null;
            }
        }
    }
}
