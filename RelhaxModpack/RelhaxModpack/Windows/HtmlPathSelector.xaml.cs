using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Xml.XPath;
using HtmlAgilityPack;
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

        public const string HtmlDocumentTextFilename = "HtmlOutput.html";

        private int waitCounts = 3;

        HtmlWebscrapeParser htmlXpathParser;

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
            ToggleBrowserView(BrowserView.HTML);
            htmlXpathParser = new HtmlWebscrapeParser(HtmlPathTextBox.Text, UrlTextBox.Text, true, HtmlDocumentTextFilename);
            HandleResult(await htmlXpathParser.RunParserAsync());
        }

        private async Task RunIEEngineAsync()
        {
            Logging.AutomationRunner("Running IE Engine");
            ToggleBrowserView(BrowserView.IE);
            htmlXpathParser = new HtmlBrowserParser(HtmlPathTextBox.Text, UrlTextBox.Text, (int)SetDelaySlider.Value, waitCounts, true, HtmlDocumentTextFilename, IEBrowser);
            HandleResult(await htmlXpathParser.RunParserAsync());
        }

        private async Task RunEdgeEngineAsync()
        {
            Logging.AutomationRunner("Running Edge Engine");
            ToggleBrowserView(BrowserView.EDGE);
            htmlXpathParser = new HtmlBrowserParser(HtmlPathTextBox.Text, UrlTextBox.Text, (int)SetDelaySlider.Value, waitCounts, true, HtmlDocumentTextFilename, EdgeBrowser);
            HandleResult(await htmlXpathParser.RunParserAsync());
        }

        private void ToggleBrowserView(BrowserView browserView)
        {
            switch (browserView)
            {
                case BrowserView.HTML:
                    WinFormsIEHost.IsEnabled = false;
                    WinFormsIEHost.Visibility = Visibility.Hidden;
                    WinFormsEdgeHost.IsEnabled = false;
                    WinFormsEdgeHost.Visibility = Visibility.Hidden;
                    break;
                case BrowserView.IE:
                    WinFormsIEHost.IsEnabled = true;
                    WinFormsIEHost.Visibility = Visibility.Visible;
                    WinFormsEdgeHost.IsEnabled = false;
                    WinFormsEdgeHost.Visibility = Visibility.Hidden;
                    break;
                case BrowserView.EDGE:
                    WinFormsIEHost.IsEnabled = false;
                    WinFormsIEHost.Visibility = Visibility.Hidden;
                    WinFormsEdgeHost.IsEnabled = true;
                    WinFormsEdgeHost.Visibility = Visibility.Visible;
                    break;
            }
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
    }
}
