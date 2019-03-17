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

            //check if devURL element should be enabled or not
            if(string.IsNullOrWhiteSpace(Package.DevURL))
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
                //make the custom class element to host it
                RelhaxPreviewIndex previewIndex = new RelhaxPreviewIndex()
                {
                    Media = Package.Medias[i],
                    Text = i.ToString()
                };
                previewIndex.OnPreviewLinkClick += PreviewIndex_OnPreviewLinkClick;
                MediaIndexer.Children.Add(previewIndex);
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
            else if (Utils.PointWithinScreen(ModpackSettings.PreviewX, ModpackSettings.PreviewY))
            {
                WindowStartupLocation = WindowStartupLocation.Manual;
                //https://stackoverflow.com/questions/2734810/how-to-set-the-location-of-a-wpf-window
                Left = ModpackSettings.PreviewX;
                Top = ModpackSettings.PreviewY;
                //set width and height
                Width = ModpackSettings.PreviewWidth;
                Height = ModpackSettings.PreviewHeight;
            }
            else
            {
                Logging.Info("[{0}]: Position {1}x{2} is outside screen dimensions, use center of window owner",
                    Logfiles.Application, nameof(Preview),ModpackSettings.PreviewX,ModpackSettings.PreviewY);
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }

            //finally if there is at least one media element, display it
            if (Package.Medias.Count > 0)
                DisplayMedia(Package.Medias[0]);
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

        private void PreviewIndex_OnPreviewLinkClick(object sender, RelhaxPreviewIndexEventArgs e)
        {
            if (e.Media == null)
                throw new BadMemeException("MEDIA IS NULL HOW IS THAT EVEN POSSIBLE REEEEE");
            DisplayMedia(e.Media);
        }

        private async void DisplayMedia(Media media)
        {
            //null the child element and make it again
            MainPreviewBorder.Child = null;
            Logging.Debug("loading preview of MediaType {0}, URL={1}", nameof(Preview), media.MediaType.ToString(), media.URL);
            switch(media.MediaType)
            {
                case MediaType.Unknown:
                default:
                    Logging.Error("Invalid MediaType: {0}", nameof(Preview), media.MediaType.ToString());
                    return;
                case MediaType.HTML:
                    WebBrowser browser = new WebBrowser();
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
                    //MainPreviewBorder.Child = pictureViewer;
                    MainPreviewBorder.Child = new ProgressBar()
                    {
                        Minimum = 0,
                        Maximum = 1,
                        Value = 0,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Top,
                        Height = 20
                    };
                    //quick set to internal loading thing
                    pictureViewer.Source = UISettings.GetLoadingImageBitmap();
                    //https://stackoverflow.com/questions/9173904/byte-array-to-image-conversion
                    //https://stackoverflow.com/questions/18134234/how-to-convert-system-io-stream-into-an-image
                    using (WebClient client = new WebClient() { })
                    {
                        client.DownloadProgressChanged += Client_DownloadProgressChanged;
                        byte[] image = await client.DownloadDataTaskAsync(media.URL);
                        ImageStream = new MemoryStream(image);
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.StreamSource = ImageStream;
                        bitmapImage.EndInit();
                        pictureViewer.Source = bitmapImage;
                        MainPreviewBorder.Child = pictureViewer;
                    }
                    break;
                case MediaType.Webpage:
                    WebBrowser browserr = new WebBrowser();
                    //https://stackoverflow.com/questions/2585782/displaying-html-from-string-in-wpf-webbrowser-control
                    browserr.NavigateToString(media.URL);
                    MainPreviewBorder.Child = browserr;
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
            throw new BadMemeException("Finish your code jackass");
        }

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            Logging.Debug("Disposing image memory stream");
            if(ImageStream != null)
            {
                ImageStream.Dispose();
                ImageStream = null;
            }
        }
    }
}
