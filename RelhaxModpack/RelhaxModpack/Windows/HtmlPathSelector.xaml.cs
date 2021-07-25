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

        public const string HtmlDocumentTextFilename = "HtmlOutput.html";

        private int waitCounts = 3;

        HtmlWebscrapeParser htmlXpathParser;

        public HtmlPathSelector(ModpackSettings modpackSettings) : base (modpackSettings)
        {
            InitializeComponent();
        }

        private async void UrlGoButton_Click(object sender, RoutedEventArgs e)
        {
            Logging.AutomationRunner("Running browser execution code");
            htmlXpathParser = new HtmlBrowserParser(HtmlPathTextBox.Text, UrlTextBox.Text, (int)SetDelaySlider.Value, waitCounts, true, HtmlDocumentTextFilename, Browser);
            HandleResult(await htmlXpathParser.RunParserAsync());
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

        private async void HtmlScrapeGoButton_Click(object sender, RoutedEventArgs e)
        {
            Logging.AutomationRunner("Running web scrape code");
            htmlXpathParser = new HtmlWebscrapeParser(HtmlPathTextBox.Text, UrlTextBox.Text, true, HtmlDocumentTextFilename);
            HandleResult(await htmlXpathParser.RunParserAsync());
        }

        private void HandleResult(HtmlXpathParserExitCode exitCode)
        {
            if (exitCode != HtmlXpathParserExitCode.None)
            {
                string errorMessage = string.Format("The html browser parser exited with code {0} ({1}).", (int)exitCode, exitCode.ToString());
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
