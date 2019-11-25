using Microsoft.Win32;
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
using System.IO;
using System.Net;
using Ionic.Zip;
using System.Xml;
using RelhaxModpack.DatabaseComponents;
using System.Reflection;
using HtmlAgilityPack;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for AutoUpdatePackageWindow.xaml
    /// </summary>
    public partial class AutoUpdatePackageWindow : RelhaxWindow
    {

        /// <summary>
        /// Get or set the list of packages to display in the package update window
        /// </summary>
        public List<DatabasePackage> Packages
        {
            get
            {
                return packages;
            }
            set
            {
                packages.Clear();
                packages.AddRange(value);
            }
        }

        /// <summary>
        /// Gets or sets the current directory where the window will download and upload files to/from
        /// </summary>
        public string WorkingDirectory { get; set; }

        /// <summary>
        /// The absolute path inside the zip file to the download instructions xml
        /// </summary>
        public const string AutoUpdateDownloadInstructionsXml = "_autoUpdate/download.xml";

        /// <summary>
        /// The absolute path inside the zip file to the file list instructions xml
        /// </summary>
        public const string AutoUpdateFileInstructionsXml = "_autoUpdate/files.xml";

        private List<DatabasePackage> packages = new List<DatabasePackage>();
        private int CurrentUpdateStep = 1;
        private WebClient client = new WebClient();

        /// <summary>
        /// Create an instance of the AutoUpdatePackageWindow window
        /// </summary>
        public AutoUpdatePackageWindow()
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            Logging.Editor("Checking if registry key is set for IE11 for this application");
            using (System.Windows.Forms.WebBrowser bro = new System.Windows.Forms.WebBrowser())
                SetRegistryKey(System.Diagnostics.Process.GetCurrentProcess().ProcessName, bro.Version.Major);

            PackageNamesListbox.ItemsSource = Packages;
        }

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            client.Dispose();
            client = null;
        }

        private void StartContinueUpdateProcessButton_Click(object sender, RoutedEventArgs e)
        {
            if(PackageNamesListbox.SelectedItems.Count == 0)
            {
                MessageBox.Show("No items selected");
                return;
            }
            ResetUpdateProcessButton.IsEnabled = true;
            switch(CurrentUpdateStep)
            {
                case 1:
                    UpdateProcessStep1();
                    break;
                case 2:
                    UpdateProcessStep2();
                    break;
            }
            CurrentUpdateStep++;
            StartContinueUpdateProcessButton.Content = string.Format("Continue from step {0}", CurrentUpdateStep);
        }

        private async void UpdateProcessStep1()
        {
            //check if update directory exists
            Logging.Editor("Checking if {0} exists", LogLevel.Info, WorkingDirectory);
            if (!Directory.Exists(WorkingDirectory))
            {
                Logging.Editor("Does not exist, creating");
                Directory.CreateDirectory(WorkingDirectory);
            }
            else
                Logging.Editor("Exists");

            DatabasePackage package = PackageNamesListbox.SelectedItems[0] as DatabasePackage;

            string downloadDir = Path.Combine(WorkingDirectory, package.PackageName);
            if (!Directory.Exists(downloadDir))
                Directory.CreateDirectory(downloadDir);

            string downloadPathCurrent = Path.Combine(WorkingDirectory, package.PackageName, package.ZipFile);
            bool downloadNeeded = false;
            if (File.Exists(downloadPathCurrent))
            {
                Logging.Editor("Current filename already exists, hashing for version");
                string hash = await Utils.CreateMD5HashAsync(downloadPathCurrent);
                if (hash.Equals(package.CRC))
                {
                    Logging.Editor("Hash matches, no need to download");
                }
                else
                {
                    Logging.Editor("Hash not match, setting for download");
                    downloadNeeded = true;
                }
            }
            else
                downloadNeeded = true;

            if(downloadNeeded)
            {
                Logging.Editor("Download needed, starting");
                await client.DownloadFileTaskAsync(package.StartAddress.Replace("{onlineFolder}", Settings.WoTModpackOnlineFolderVersion) + package.ZipFile + package.EndAddress, downloadPathCurrent);
                Logging.Editor("Download completed");
                AutoUpdateProgressBar.Value = AutoUpdateProgressBar.Minimum;
            }

            //check inside zip file for download instructions xml file
            using (ZipFile currentZip = new ZipFile(downloadPathCurrent))
            {
                ZipEntry downloadxml = currentZip[AutoUpdateDownloadInstructionsXml];
                ZipEntry filesxml = currentZip[AutoUpdateFileInstructionsXml];
                if (downloadxml == null)
                {
                    Logging.Editor("This zip file does not support auto update, needs xml instructions");
                    return;
                }
                if (filesxml == null)
                {
                    Logging.Editor("This zip file does not support auto update, needs xml instructions");
                    return;
                }
                downloadxml.Extract(Path.Combine(WorkingDirectory, package.PackageName),ExtractExistingFileAction.OverwriteSilently);
                filesxml.Extract(Path.Combine(WorkingDirectory, package.PackageName), ExtractExistingFileAction.OverwriteSilently);
            }
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            AutoUpdateProgressBar.Maximum = e.TotalBytesToReceive;
            AutoUpdateProgressBar.Minimum = 0;
            AutoUpdateProgressBar.Value = e.BytesReceived;
        }

        private async void UpdateProcessStep2()
        {
            DatabasePackage package = PackageNamesListbox.SelectedItems[0] as DatabasePackage;
            //parse download instructions xml files
            XmlDocument downloadDocument = XmlUtils.LoadXmlDocument(Path.Combine(WorkingDirectory, package.PackageName, "_autoUpdate", "download.xml"), XmlLoadType.FromFile);

            if (downloadDocument == null)
            {
                Logging.Editor("Failed to parse download xml document");
                return;
            }

            //parse to class objects
            DownloadInstructions downloadInstructions = ParseDownloadInstructions(downloadDocument);

            string directDownloadURL = string.Empty;
            switch(downloadInstructions.DownloadType)
            {
                case DownloadTypes.StaticLink:
                    directDownloadURL = downloadInstructions.UpdateURL;
                    break;
                case DownloadTypes.WgMods:
                    directDownloadURL = await GetWGmodsDownloadLink(downloadInstructions.UpdateURL);
                    break;
            }

            if (string.IsNullOrWhiteSpace(directDownloadURL))
                return;

            string downloadLocation = Path.Combine(WorkingDirectory, package.PackageName, downloadInstructions.DownloadFilename);

            await client.DownloadFileTaskAsync(directDownloadURL, downloadLocation);
            AutoUpdateProgressBar.Value = AutoUpdateProgressBar.Minimum;
        }

        private void UpdateProcessStep3()
        {
            DatabasePackage package = PackageNamesListbox.SelectedItems[0] as DatabasePackage;
            //parse download instructions xml files
            XmlDocument filesDocument = XmlUtils.LoadXmlDocument(Path.Combine(WorkingDirectory, package.PackageName, "_autoUpdate", "download.xml"), XmlLoadType.FromFile);

            if (filesDocument == null)
            {
                Logging.Editor("Failed to parse download xml document");
                return;
            }

            //parse to class objects
            UpdateInstructions updateInstructions = ParseUpdateInstructions(filesDocument);

            switch(updateInstructions.UpdateType)
            {
                case UpdateTypes.wotmod:

                    break;
            }
        }

        private DownloadInstructions ParseDownloadInstructions(XmlDocument doc)
        {
            DownloadInstructions instructions = new DownloadInstructions();
            string formatVersion = doc.DocumentElement.Attributes["formatVersion"].Value;
            instructions.InstructionsVersion = formatVersion;
            switch(formatVersion)
            {
                case "1.0":
                    ParseDownloadInstructionsV1(instructions, doc);
                    break;
            }

            return instructions;
        }

        private DownloadInstructions ParseDownloadInstructionsV1(DownloadInstructions instructions, XmlDocument doc)
        {
            //public string InstructionsVersion { get; set; } (already got)
            //public string ModVersion { get; set; }
            //public string ClientVersion { get; set; }
            //public DownloadTypes DownloadType { get; set; }
            //public string UpdateURL { get; set; }
            foreach(XmlNode node in doc.ChildNodes[1].ChildNodes)
            {
                switch(node.Name)
                {
                    case "ModVersion":
                        instructions.ModVersion = node.InnerText;
                        break;
                    case "ClientVersion":
                        instructions.ClientVersion = node.InnerText;
                        break;
                    case "DownloadType":
                        instructions.DownloadType = (DownloadTypes)Enum.Parse(instructions.DownloadType.GetType(), node.InnerText);
                        break;
                    case "UpdateURL":
                        instructions.UpdateURL = node.InnerText;
                        break;
                    case "DownloadFilename":
                        instructions.DownloadFilename = node.InnerText;
                        break;
                }
            }
            return instructions;
        }

        private UpdateInstructions ParseUpdateInstructions(XmlDocument doc)
        {
            UpdateInstructions instructions = new UpdateInstructions();
            string formatVersion = doc.DocumentElement.Attributes["formatVersion"].Value;
            instructions.InstructionsVersion = formatVersion;
            switch (formatVersion)
            {
                case "1.0":
                    ParseUpdateInstructions(instructions, doc);
                    break;
            }

            return instructions;
        }

        private UpdateInstructions ParseUpdateInstructions(UpdateInstructions instructions, XmlDocument doc)
        {
            //public string InstructionsVersion { get; set; } (already got)
            //public UpdateTypes UpdateType { get; set; }
            //public string WotmodFilenameInZip { get; set; }
            //public string WotmodMD5 { get; set; }
            foreach (XmlNode node in doc.ChildNodes[1].ChildNodes)
            {
                switch (node.Name)
                {
                    case "WotmodFilenameInZip":
                        instructions.WotmodFilenameInZip = node.InnerText;
                        break;
                    case "UpdateType":
                        instructions.UpdateType = (UpdateTypes)Enum.Parse(instructions.UpdateType.GetType(), node.InnerText);
                        break;
                    case "WotmodMD5":
                        instructions.WotmodMD5 = node.InnerText;
                        break;
                }
            }
            return instructions;
        }

        private async Task<string> GetWGmodsDownloadLink(string wgmodsBaseUrl)
        {
            bool browserLoaded = false;
            browser.LoadCompleted += (sendahh, endArgs) =>
            {
                browserLoaded = true;
            };
            
            //https://stackoverflow.com/questions/1298255/how-do-i-suppress-script-errors-when-using-the-wpf-webbrowser-control
            dynamic activeX = this.browser.GetType().InvokeMember("ActiveXInstance",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, this.browser, new object[] { });
            activeX.Silent = true;
            
            browser.Navigate(wgmodsBaseUrl);

            while (!browserLoaded)
                await Task.Delay(500);

            var doc = browser.Document as mshtml.HTMLDocument;
            string s = doc.body.outerHTML;

            //http://blog.olussier.net/2010/03/30/easily-parse-html-documents-in-csharp/
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(s);
            HtmlNode node = document.DocumentNode;
            //https://stackoverflow.com/questions/1390568/how-can-i-match-on-an-attribute-that-contains-a-certain-string
            HtmlNodeCollection clientVersionNode = node.SelectNodes(@"//div[contains(@class, 'ModDetails_label')]");
            string version = string.Empty;
            if (clientVersionNode != null)
            {
                HtmlNode nodeTest = clientVersionNode[3];
                HtmlNode versionNode = nodeTest.ChildNodes[0].ChildNodes[1];
                version = versionNode.InnerText;
            }

            HtmlNode downloadUrlNode = node.SelectSingleNode(@"//a[contains(@class, 'ModDetails_hidden')]");
            string downloadURL = downloadUrlNode.Attributes["href"].Value;

            Logging.Editor(string.Format("For client: {0}, download link: {1}", version, downloadURL));

            return downloadURL;
        }

        private void ResetUpdateProcessButton_Click(object sender, RoutedEventArgs e)
        {
            ResetUpdateProcessButton.IsEnabled = false;
            CurrentUpdateStep = 1;
        }

        private void DetailedChangesWindow_Click(object sender, RoutedEventArgs e)
        {

        }

        private void PackageNamesListbox_Selected(object sender, RoutedEventArgs e)
        {
            SelectedPackagesStackPanel.Children.Clear();
            foreach(DatabasePackage package in PackageNamesListbox.SelectedItems)
            {
                TextBlock block = new TextBlock()
                {
                    Text = string.Format("Zipfile: {0}, CRC: {1}, Last Updated: {2}", package.ZipFile, package.CRC, package.Timestamp)
                };
                SelectedPackagesStackPanel.Children.Add(block);
            }
        }

        private void SetRegistryKey(string exeName, int IEVersion)
        {
            //https://weblog.west-wind.com/posts/2011/May/21/Web-Browser-Control-Specifying-the-IE-Version#Using-the-X--UA--Compatible-HTML-Meta-Tag
            //https://stackoverflow.com/questions/17922308/use-latest-version-of-internet-explorer-in-the-webbrowser-control

            int registryToSet = 0;
            int currentRegistryValue = 0;

            if (IEVersion >= 11)
                registryToSet = 11001;
            else if (IEVersion == 10)
                registryToSet = 10001;
            else if (IEVersion == 9)
                registryToSet = 9999;
            else if (IEVersion == 8)
                registryToSet = 8888;
            else
                registryToSet = 7000;

            using (RegistryKey Key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION",
                RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (Key.GetValue(exeName + ".exe") != null)
                {
                    currentRegistryValue = (int)Key.GetValue(exeName + ".exe");
                }
                else
                {
                    currentRegistryValue = -1;
                }

                Logging.Editor("IEVersion: {0} -> RegistryCurrent:{1}, RegistryToSet: {2}", LogLevel.Info, IEVersion, currentRegistryValue, registryToSet);
                if(currentRegistryValue != registryToSet)
                {
                    Logging.Editor("Values are not same, update registry");
                    Key.SetValue(exeName + ".exe", registryToSet, RegistryValueKind.DWord);
                    Logging.Editor("Registry updated");
                }
                else
                {
                    Logging.Editor("Values are same, continue");
                }
            }
        }
    }
}
