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

        HtmlBrowserParser htmlBrowserParser;

        public HtmlPathSelector(ModpackSettings modpackSettings) : base (modpackSettings)
        {
            InitializeComponent();
        }

        private async void UrlGoButton_Click(object sender, RoutedEventArgs e)
        {
            Logging.AutomationRunner("Running Browser execution code");
            htmlBrowserParser = new HtmlBrowserParser(HtmlPathTextBox.Text, UrlTextBox.Text, (int)SetDelaySlider.Value, waitCounts, true, HtmlDocumentTextFilename, Browser);
            HtmlBrowserParserExitCode exitCode = await htmlBrowserParser.RunParserAsync();

            if (exitCode != HtmlBrowserParserExitCode.None)
            {
                string errorMessage = string.Format("ExitCode {0}: The html browser parser exited with code {1}.", exitCode);
                HtmlPathResultsTextBox.Text = errorMessage;
                Logging.Error(Logfiles.AutomationRunner, LogOptions.MethodName, errorMessage);
                return;
            }

            Logging.Debug(Logfiles.AutomationRunner, "HtmlPath results in node value '{0}' of type '{1}'", htmlBrowserParser.ResultNode.InnerXml, htmlBrowserParser.ResultNode.NodeType.ToString());
            HtmlPathResultsTextBox.Text = string.Format("Result value as text: {0}\nResult node type: {1}\nResult inner html: {2}\nResult outer html: {3}",
                htmlBrowserParser.ResultNode.Value, htmlBrowserParser.ResultNode.NodeType.ToString(), htmlBrowserParser.ResultNode.InnerXml, htmlBrowserParser.ResultNode.OuterXml);
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
    }
}
