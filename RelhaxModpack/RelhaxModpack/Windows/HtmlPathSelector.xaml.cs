using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.XPath;
using HtmlAgilityPack;
using RelhaxModpack.Common;
using RelhaxModpack.Settings;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for HtmlPathSelector.xaml
    /// </summary>
    public partial class HtmlPathSelector : RelhaxWindow
    {

        public const string HtmlDocumentTextFilename = "HtmlOutput.html";

        private bool browserNavigated, browserDocumentCompleted = false;

        private int browserFinishedLoadingScriptsCounter = 0;

        private int waitCounts = 3;

        public HtmlPathSelector(ModpackSettings modpackSettings) : base (modpackSettings)
        {
            InitializeComponent();

            WindowsInterop.SecurityAlertDialogWillBeShown += new GenericDelegate<Boolean, Boolean>(this.WindowsInterop_SecurityAlertDialogWillBeShown);

            WindowsInterop.Hook();
        }

        private async void UrlGoButton_Click(object sender, RoutedEventArgs e)
        {
            HtmlPathResultsTextBox.Text = "Loading...";
            
            if (Browser.Url != null && Browser.Url.Equals(UrlTextBox.Text))
            {
                Logging.Info("The URL did not change, we can skip the browser run");
            }
            else
            {
                Logging.Info("The URL changed or is null, load the browser page");
                await RunBrowserAsync();
            }

            RunHtmlPathSearch();
        }

        private async Task RunBrowserAsync()
        {
            //reset browser counts
            browserNavigated = false;
            browserDocumentCompleted = false;
            browserFinishedLoadingScriptsCounter = 0;
            waitCounts = 3;

            //run browser enough to get scripts parsed to get download link
            try
            {
                Browser.Navigate(UrlTextBox.Text);
            }
            catch (Exception ex)
            {
                Logging.Exception(ex.ToString());
            }

            //wait for browser events to finish
            while (!(browserDocumentCompleted && browserNavigated))
            {
                await Task.Delay((int)SetDelaySlider.Value);
                Logging.Debug(Logfiles.AutomationRunner, "The browser task events completed, wait additional {0} counts", waitCounts);
            }

            //this wait allows the browser to finish loading external scripts
            while (browserFinishedLoadingScriptsCounter <= waitCounts)
            {
                await Task.Delay((int)SetDelaySlider.Value);
                Logging.Debug(Logfiles.AutomationRunner, "Waiting {0} of {1} counts", ++browserFinishedLoadingScriptsCounter, waitCounts);
            }

            //save to string
            Logging.Info("Writing HTML to {0}", HtmlDocumentTextFilename);
            File.WriteAllText(HtmlDocumentTextFilename, Browser.Document.Body.OuterHtml);
            Logging.Info(Logfiles.AutomationRunner, "The browser reports all loading done, save html to string");
        }

        private void RunHtmlPathSearch()
        {
            HtmlDocument document = new HtmlDocument();
            string htmlText = Browser.Document.Body.OuterHtml;
            document.LoadHtml(htmlText);
            HtmlNodeNavigator navigator = (HtmlNodeNavigator)document.CreateNavigator();
            HtmlNodeNavigator result;
            //https://stackoverflow.com/questions/1390568/how-can-i-match-on-an-attribute-that-contains-a-certain-string
            //sample htmlPath to get download link: @"//a[contains(@class, 'ModDetails_hidden')]//@href"
            //HtmlNodeCollection clientVersionNode = node.SelectNodes(@"//div[contains(@class, 'ModDetails_label')]");
            Logging.Debug("Searching using html path: {0}", HtmlPathTextBox.Text.Trim());
            try
            {
                result = navigator.SelectSingleNode(HtmlPathTextBox.Text.Trim()) as HtmlNodeNavigator;
            }
            catch (XPathException ex)
            {
                HtmlPathResultsTextBox.Text = "Invalid HtmlPath expression: " + ex.Message;
                Logging.Exception(ex.ToString());
                return;
            }

            if (result == null)
            {
                Logging.Info("Result was not found");
                HtmlPathResultsTextBox.Text = "Result was not found";
            }
            else
            {
                Logging.Debug(Logfiles.AutomationRunner, "HtmlPath results in node value '{0}' of type '{1}'", result.InnerXml, result.NodeType.ToString());
                HtmlPathResultsTextBox.Text = string.Format("Result value as text: {0}\nResult node type: {1}\nResult inner html: {2}\nResult outer html: {3}",
                    result.Value, result.NodeType.ToString(), result.InnerXml, result.OuterXml);
            }
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Logging.Info("Checking for registry settings");
            RegistryUtils.SetRegisterKeyForIEVersion(IERegistryVersion.IE11Default);
            SetDelaySlider_ValueChanged(null, null);
        }

        private void Browser_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            Logging.Debug(Logfiles.AutomationRunner, "The browser reports document completed");
            browserDocumentCompleted = true;
        }

        private void Browser_Navigated(object sender, WebBrowserNavigatedEventArgs e)
        {
            Logging.Debug(Logfiles.AutomationRunner, "The browser reports navigation completed");
            browserNavigated = true;
        }

        private void SetDelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SetDelayValueTextBlock != null)
                SetDelayValueTextBlock.Text = string.Format("{0} ms", (int)SetDelaySlider.Value);
        }

        private Boolean WindowsInterop_SecurityAlertDialogWillBeShown(Boolean blnIsSSLDialog)
        {
            // Return true to ignore and not show the 
            // "Security Alert" dialog to the user
            return true;
        }

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            WindowsInterop.Unhook();
        }
    }
}
