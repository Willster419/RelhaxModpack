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
    /// Interaction logic for Preview.xaml
    /// </summary>
    public partial class Preview : RelhaxWindow
    {

        public SelectablePackage Package { get; set; } = null;

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
                DevUrlBlock.IsEnabled = false;
                DevUrlBlock.Visibility = Visibility.Hidden;
            }

            //set the loading properties
            //TODO: this lol

            //set the name of the window to be the package name
            Title = Package.NameFormatted;

            //make the linked labels in the link box
            for(int i =0; i < Package.Medias.Count; i++)
            {
                //make the custom class element to host it
                UIComponents.RelhaxPreviewIndex previewIndex = new UIComponents.RelhaxPreviewIndex()
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

            //if the saved preview window point is within the screen, then load it to there
            if (Utils.PointWithinScreen(ModpackSettings.PreviewX, ModpackSettings.PreviewY))
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

        private void PreviewIndex_OnPreviewLinkClick(object sender, UIComponents.RelhaxPreviewIndexEventArgs e)
        {
            if (e.Media == null)
                throw new BadMemeException("MEDIA IS NULL HOW IS THAT EVEN POSSIBLE REEEEE");
            DisplayMedia(e.Media);
        }

        private void DisplayMedia(Media media)
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

                    break;
                case MediaType.Picture:
                    //https://docs.microsoft.com/en-us/dotnet/api/system.windows.controls.image?view=netframework-4.7.2
                    Image pictureViewer = new Image();
                    pictureViewer.MouseLeftButtonDown += PictureViewer_MouseLeftButtonDown;
                    //http://dasselsoftwaredevelopment.com/code-samples/changing-the-source-of-an-image-in-c/
                    pictureViewer.Source = new BitmapImage(new Uri(media.URL));
                    MainPreviewBorder.Child = pictureViewer;
                    break;
                case MediaType.Webpage:
                    WebBrowser browserr = new WebBrowser();
                    //https://stackoverflow.com/questions/2585782/displaying-html-from-string-in-wpf-webbrowser-control
                    browserr.NavigateToString(media.URL);
                    MainPreviewBorder.Child = browserr;
                    break;
            }
        }

        private void PictureViewer_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
