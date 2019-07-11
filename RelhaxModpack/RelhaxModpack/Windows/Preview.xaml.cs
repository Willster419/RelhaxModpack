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
using RelhaxModpack.UIComponents;
using System.Threading;
using System.IO;
using System.Net;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for Preview.xaml
    /// </summary>
    public partial class Preview : RelhaxWindow
    {

        public SelectablePackage Package = null;
        public bool EditorMode = false;
        private MemoryStream ImageStream = null;
        private Media CurrentDispalyMedia = null;
        private WebBrowser browser = null;

        public Preview()
        {
            InitializeComponent();
        }

        private void OnPreviewWindowLoad(object sender, RoutedEventArgs e)
        {
            //check to make sure the Package element exists
            if(Package == null)
            {
                Logging.Error("Package is null, it should never be null!", nameof(Preview));
                MessageBox.Show(Translations.GetTranslatedString("previewEncounteredError"));
                Close();
            }

            //and for the medias element
            if(Package.Medias == null)
            {
                Logging.Error("Package.Medias is null, it should never be null!", nameof(Preview));
                MessageBox.Show(Translations.GetTranslatedString("previewEncounteredError"));
                Close();
            }

            //translate 3 components
            DevUrlHeader.Text = Translations.GetTranslatedString(DevUrlHeader.Name);
            PreviewNextPicButton.Content = Translations.GetTranslatedString(PreviewNextPicButton.Name);
            PreviewPreviousPicButton.Content = Translations.GetTranslatedString(PreviewPreviousPicButton.Name);

            //check if devURL element should be enabled or not
            if (string.IsNullOrWhiteSpace(Package.DevURL))
            {
                DevUrlHeader.IsEnabled = false;
                DevUrlHeader.Visibility = Visibility.Hidden;
                DevUrlHolder.IsEnabled = false;
                DevUrlHolder.Visibility = Visibility.Hidden;
            }
            else
            {
                //devURL is now array of elements separated by newline
                //load the stack with textblocks with tooltips for the URLs
                string[] devURLS = Package.DevURL.Split('\n');
                for(int i = 0; i < devURLS.Count(); i++)
                {
                    //make a textbox
                    TextBlock block = new TextBlock()
                    {
                        ToolTip = devURLS[i].Trim()
                    };
                    //https://stackoverflow.com/questions/21214450/how-to-add-a-hyperlink-in-a-textblock-in-code?noredirect=1&lq=1
                    block.Inlines.Clear();
                    Hyperlink h = new Hyperlink(new Run(i.ToString()))
                    {
                        NavigateUri = new Uri(devURLS[i].Trim())
                    };
                    h.RequestNavigate += OnHyperLinkClick;
                    block.Inlines.Add(h);
                    DevUrlHolder.Children.Add(block);
                }
            }

            //set the name of the window to be the package name
            Title = Package.NameFormatted;
            if (Package.PopularMod)
                Title = string.Format("{0} ({1})", Title, Translations.GetTranslatedString("popular"));

            //make the linked labels in the link box
            for(int i =0; i < Package.Medias.Count; i++)
            {
                TextBlock block = new TextBlock()
                {

                };
                block.Inlines.Clear();
                Hyperlink h = new Hyperlink(new Run(i.ToString()))
                {
                    Tag = Package.Medias[i],
                    //dummy URI just to make the request navigate work
                    NavigateUri = new Uri("http://google.com")
                };
                h.RequestNavigate += OnMediaHyperlinkClick;
                block.Inlines.Add(h);
                MediaIndexer.Children.Add(block);
            }

            //format the descriptions and update info text strings
            PreviewDescriptionBox.Text = string.IsNullOrWhiteSpace(Package.Description) ?
                Translations.GetTranslatedString("noDescription") : Package.Description;

            //0 is update notes, 1 is update time (last updated)
            PreviewUpdatesBox.Text = string.Format("{0}\n{1}", string.IsNullOrWhiteSpace(Package.UpdateComment) ?
                Translations.GetTranslatedString("noUpdateInfo") : Package.UpdateComment,
                Package.Timestamp == 0 ? Translations.GetTranslatedString("noTimestamp") : Utils.ConvertFiletimeTimestampToDate(Package.Timestamp));

            if(EditorMode)
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
            }

            //if the saved preview window point is within the screen, then load it to there
            else
            {
                if (Utils.PointWithinScreen(ModpackSettings.PreviewX, ModpackSettings.PreviewY))
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

            //finally if there is at least one media element, display it
            if (Package.Medias.Count > 0)
                DisplayMedia(Package.Medias[0]);
        }

        private void OnMediaHyperlinkClick(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if(sender is Hyperlink link)
            {
                if(link.Tag is Media media)
                {
                    if(!media.Equals(CurrentDispalyMedia))
                        DisplayMedia(media);
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

        private async void DisplayMedia(Media media)
        {
            CurrentDispalyMedia = media;
            //null the child element and make it again
            MainPreviewBorder.Child = null;
            Logging.Debug("loading preview of MediaType {0}, URL={1}", media.MediaType.ToString(), media.URL);
            switch(media.MediaType)
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
                    MainPreviewBorder.Child = new RelhaxMediaPlayer(media.URL);
                    break;
                case MediaType.Picture:
                    //https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.image?view=netframework-4.7.2
                    Image pictureViewer = new Image();
                    pictureViewer.MouseLeftButtonDown += PictureViewer_MouseLeftButtonDown;
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
                        catch(Exception ex)
                        {
                            Logging.Error("failed to load picture");
                            Logging.Error(ex.ToString());
                            pictureViewer.Source = Utils.BitmapToImageSource(Properties.Resources.error_loading_picture);
                        }
                        MainPreviewBorder.Child = pictureViewer;
                    }
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

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if(MainPreviewBorder.Child is ProgressBar bar)
            {
                if (bar.Maximum != e.TotalBytesToReceive)
                    bar.Maximum = e.TotalBytesToReceive;
                bar.Value = e.BytesReceived;
            }
        }

        private void PictureViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (WindowState != WindowState.Maximized)
                WindowState = WindowState.Maximized;
            else
                WindowState = WindowState.Normal;
        }

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            //save window location, size and fullscreen property (if not in editor mode)
            if(!EditorMode)
            {
                ModpackSettings.PreviewFullscreen = WindowState == WindowState.Maximized ? true : false;
                ModpackSettings.PreviewHeight = (int)Height;
                ModpackSettings.PreviewWidth = (int)Width;
                if(Utils.PointWithinScreen((int)Left, (int)Top))
                {
                    ModpackSettings.PreviewX = (int)Left;
                    ModpackSettings.PreviewY = (int)Top;
                }
            }

            Logging.Debug(" Preview:  Disposing image memory stream");
            if(ImageStream != null)
            {
                ImageStream.Dispose();
                ImageStream = null;
            }

            Logging.Debug("Preview:  Disposing browser");
            if(browser != null)
            {
                browser.Dispose();
                browser = null;
            }
        }

        private void PreviewNextPicButton_Click(object sender, RoutedEventArgs e)
        {
            int index = Package.Medias.IndexOf(CurrentDispalyMedia);
            if(index < Package.Medias.Count-1)
            {
                DisplayMedia(Package.Medias[index + 1]);
            }
        }

        private void PreviewPreviousPicButton_Click(object sender, RoutedEventArgs e)
        {
            int index = Package.Medias.IndexOf(CurrentDispalyMedia);
            if(index > 0)
            {
                DisplayMedia(Package.Medias[index - 1]);
            }
        }
    }
}
