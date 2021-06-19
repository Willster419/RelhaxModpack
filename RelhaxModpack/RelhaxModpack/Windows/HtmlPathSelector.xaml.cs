using HtmlAgilityPack;
using RelhaxModpack.Settings;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
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
using System.Xml.XPath;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for HtmlPathSelector.xaml
    /// </summary>
    public partial class HtmlPathSelector : RelhaxWindow
    {

        private bool browserNavigated, browserDocumentCompleted = false;

        private int browserFinishedLoadingScriptsCounter = 0;

        private int waitCounts = 3;

        public HtmlPathSelector(ModpackSettings modpackSettings) : base (modpackSettings)
        {
            InitializeComponent();
        }

        private async void UrlGoButton_Click(object sender, RoutedEventArgs e)
        {
            HtmlPathResultsTextBox.Text = "Loading...";
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

            Logging.Info(Logfiles.AutomationRunner, "The browser reports all loading done, save html to string");

            await Task.Delay((int)SetDelaySlider.Value);

            HtmlDocument document = new HtmlDocument();
            string htmlText = Browser.Document.Body.OuterHtml;
            document.LoadHtml(htmlText);
            HtmlNodeNavigator navigator = (HtmlAgilityPack.HtmlNodeNavigator)document.CreateNavigator();
            HtmlNodeNavigator result = null;
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
            Logging.Debug(Logfiles.AutomationRunner, "HtmlPath results in node value '{0}' of type '{1}'", result.InnerXml, result.NodeType.ToString());
            HtmlPathResultsTextBox.Text = string.Format("Result value as text: {0}\nResult node type: {1}\nResult inner html: {2}\nResult outer html: {3}",
                result.Value, result.NodeType.ToString(), result.InnerXml, result.OuterXml);
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Logging.Info("Checking for registry settings");
            RegistryUtils.SetRegisterKeyForIEVersion(IERegistryVersion.IE11Default);
            SetDelaySlider_ValueChanged(null, null);
        }

        private async void Browser_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            Logging.Debug(Logfiles.AutomationRunner, "The browser reports document completed, wait for timeout");
            browserDocumentCompleted = true;
            
        }

        private void Browser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
            Logging.Debug(Logfiles.AutomationRunner, "The browser reports navigation completed, wait for document completed and timeout");
            browserNavigated = true;
        }

        private void SetDelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SetDelayValueTextBlock != null)
                SetDelayValueTextBlock.Text = string.Format("{0} ms", (int)SetDelaySlider.Value);
        }
    }
}
