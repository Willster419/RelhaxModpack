using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.XPath;
using HtmlAgilityPack;
using Microsoft.Toolkit.Forms.UI.Controls;
using RelhaxModpack.Automation;
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
        private enum BrowserView { HTML, IE, EDGE }

        /// <summary>
        /// The filename to write the downloaded html document to, to use for additional debugging by the user.
        /// </summary>
        public const string HtmlDocumentTextFilename = "HtmlOutput.html";

        private int waitCounts = 3;

        HtmlWebscrapeParser htmlXpathParser;

        /// <summary>
        /// Create an instance of the HtmlpathSelector class window.
        /// </summary>
        /// <param name="modpackSettings">The modpack settings object</param>
        public HtmlPathSelector(ModpackSettings modpackSettings) : base (modpackSettings)
        {
            InitializeComponent();
        }

        private async void GoButton_Click(object sender, RoutedEventArgs e)
        {
            if ((bool)HtmlEngineRadioButton.IsChecked)
                await RunHtmlEngineAsync();
            else if ((bool)IEEngineRadioButton.IsChecked)
                await RunIEEngineAsync();
            else if ((bool)EdgeEngineRadioButton.IsChecked)
                await RunEdgeEngineAsync();
            else
                throw new BadMemeException("w h a t");
        }

        private async Task RunHtmlEngineAsync()
        {
            Logging.AutomationRunner("Running HTML Engine");
            CancelAndDisposeParserIfBrowser();
            htmlXpathParser = new HtmlWebscrapeParser(HtmlPathTextBox.Text, UrlTextBox.Text, true, HtmlDocumentTextFilename);
            ToggleBrowserView(BrowserView.HTML);
            HandleResult(await htmlXpathParser.RunParserAsync());
        }

        private async Task RunIEEngineAsync()
        {
            Logging.AutomationRunner("Running IE Engine");
            CancelAndDisposeParserIfBrowser();
            htmlXpathParser = new HtmlBrowserParser(HtmlPathTextBox.Text, UrlTextBox.Text, (int)SetDelaySlider.Value, waitCounts, true, HtmlDocumentTextFilename, BrowserType.WebBrowser, this.Dispatcher);
            ToggleBrowserView(BrowserView.IE);
            (htmlXpathParser as HtmlBrowserParser).BrowserCreated += HtmlPathSelector_BrowserCreated;
            HandleResult(await htmlXpathParser.RunParserAsync());
        }

        private async Task RunEdgeEngineAsync()
        {
            Logging.AutomationRunner("Running Edge Engine");
            CancelAndDisposeParserIfBrowser();
            htmlXpathParser = new HtmlBrowserParser(HtmlPathTextBox.Text, UrlTextBox.Text, (int)SetDelaySlider.Value, waitCounts, true, HtmlDocumentTextFilename, BrowserType.WebView, this.Dispatcher);
            ToggleBrowserView(BrowserView.EDGE);
            (htmlXpathParser as HtmlBrowserParser).BrowserCreated += HtmlPathSelector_BrowserCreated;
            HandleResult(await htmlXpathParser.RunParserAsync());
        }

        private void ToggleBrowserView(BrowserView browserView)
        {
            switch (browserView)
            {
                case BrowserView.HTML:
                    WinFormsHost.IsEnabled = false;
                    WinFormsHost.Visibility = Visibility.Hidden;
                    WinFormsHost.Child = null;
                    break;
                case BrowserView.IE:
                case BrowserView.EDGE:
                    WinFormsHost.IsEnabled = true;
                    WinFormsHost.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void HtmlPathSelector_BrowserCreated(object sender, EventArgs e)
        {
            HtmlBrowserParser parser = sender as HtmlBrowserParser;
            Control browser = parser.BrowserManager.Browser;
            WinFormsHost.Child = browser;
            browser.Dock = DockStyle.Fill;
            browser.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            Logging.Info("Checking for registry settings");
            RegistryUtils.SetRegisterKeyForIEVersion(IERegistryVersion.IE11Default);
            SetDelaySlider_ValueChanged(null, null);
        }

        private void SetDelaySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (SetDelayValueTextBlock != null)
                SetDelayValueTextBlock.Text = string.Format("{0} ms", (int)SetDelaySlider.Value);
        }

        private void HandleResult(HtmlXpathParserExitCode exitCode)
        {
            if (exitCode != HtmlXpathParserExitCode.None)
            {
                string errorMessage = string.Format("The parser exited with code {0} ({1}).", (int)exitCode, exitCode.ToString());
                HtmlPathResultsTextBox.Text = errorMessage;
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, errorMessage);
                return;
            }

            Logging.Debug(Logfiles.AutomationRunner, "HtmlPath results in node value '{0}' of type '{1}'", htmlXpathParser.ResultNode.InnerXml, htmlXpathParser.ResultNode.NodeType.ToString());
            HtmlPathResultsTextBox.Text = string.Format("Result value as text: {0}\nResult node type: {1}\nResult inner html: {2}\nResult outer html: {3}",
                htmlXpathParser.ResultNode.Value, htmlXpathParser.ResultNode.NodeType.ToString(), htmlXpathParser.ResultNode.InnerXml, htmlXpathParser.ResultNode.OuterXml);
        }

        private void RelhaxWindow_Closing(object sender, CancelEventArgs e)
        {
            CancelAndDisposeParserIfBrowser();
        }

        private void CancelAndDisposeParserIfBrowser()
        {
            if (this.htmlXpathParser != null)
            {
                this.htmlXpathParser.Cancel();
                if (htmlXpathParser is HtmlBrowserParser browserParser)
                {
                    browserParser.BrowserCreated -= HtmlPathSelector_BrowserCreated;
                    browserParser.Dispose();
                }
                htmlXpathParser = null;
            }
        }
    }
}
