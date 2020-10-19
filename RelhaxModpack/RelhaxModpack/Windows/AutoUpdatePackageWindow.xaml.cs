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
using RelhaxModpack.Database;
using System.Reflection;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using RelhaxModpack.Utilities;
using RelhaxModpack.Xml;
using RelhaxModpack.UI;
using RelhaxModpack.Utilities.Enums;

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

        /// <summary>
        /// The FTP credentials
        /// </summary>
        public NetworkCredential Credential = null;

        private List<DatabasePackage> packages = new List<DatabasePackage>();
        private int CurrentUpdateStep = 1;
        private WebClient client = new WebClient();
        private WebClient databaseClient = null;
        private string UpdateOutputDirectory = string.Empty;
        private long databaseFtpDownloadsize = 0;

        /// <summary>
        /// Create an instance of the AutoUpdatePackageWindow window
        /// </summary>
        public AutoUpdatePackageWindow()
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (Credential == null)
                throw new NullReferenceException(nameof(Credential) +  " is null");

            databaseClient = new WebClient() { Credentials = Credential };
            databaseClient.DownloadProgressChanged += DatabaseClient_DownloadProgressChanged;
            client.DownloadProgressChanged += Client_DownloadProgressChanged;
            Logging.Editor("Checking if registry key is set for IE11 for this application");
            using (System.Windows.Forms.WebBrowser bro = new System.Windows.Forms.WebBrowser())
                SetRegistryKey(System.Diagnostics.Process.GetCurrentProcess().ProcessName, bro.Version.Major);

            Logging.Editor("Attaching datasources", LogLevel.Debug);
            PackageNamesListbox.ItemsSource = Packages;

            //set autoupdate output directory
            Logging.Editor("Setting update output directory", LogLevel.Debug);
            UpdateOutputDirectory = Path.Combine(WorkingDirectory, "Output");
            if (!Directory.Exists(UpdateOutputDirectory))
                Directory.CreateDirectory(UpdateOutputDirectory);

            //attach logfile reporting
            LogfileTextbox.Clear();
            Logging.GetLogfile(Logfiles.Editor).OnLogfileWrite += Logging_OnLoggingUIThreadReport;
        }

        private void Logging_OnLoggingUIThreadReport(string message)
        {
            LogfileTextbox.AppendText(message + Environment.NewLine);
        }

        private void RelhaxWindow_Closed(object sender, EventArgs e)
        {
            client.Dispose();
            client = null;
        }

        private async void StartContinueUpdateProcessButton_Click(object sender, RoutedEventArgs e)
        {
            if(PackageNamesListbox.SelectedItems.Count == 0)
            {
                MessageBox.Show("No items selected");
                return;
            }
            ResetUpdateProcessButton.IsEnabled = true;
            StartContinueUpdateProcessButton.IsEnabled = false;
            switch(CurrentUpdateStep)
            {
                case 1:
                    await UpdateProcessStep1();
                    break;
                case 2:
                    await UpdateProcessStep2();
                    break;
                case 3:
                    await UpdateProcessStep3();
                    break;
            }
            StartContinueUpdateProcessButton.IsEnabled = true;
            CurrentUpdateStep++;
            if (CurrentUpdateStep >= 4)
            {
                StartContinueUpdateProcessButton.Content = "Start";
                CurrentUpdateStep = 1;
                ResetUpdateProcessButton.IsEnabled = false;
            }
            else
            {
                StartContinueUpdateProcessButton.Content = string.Format("Continue from step {0}", CurrentUpdateStep);
            }
        }

        private async Task UpdateProcessStep1()
        {
            Logging.Editor("Starting update process step 1");

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
            package.DownloadInstructions = new DownloadInstructions() { DownloadedDatabaseZipFileLocation = downloadPathCurrent };
            bool downloadNeeded = false;
            if (File.Exists(downloadPathCurrent))
            {
                Logging.Editor("Current filename already exists, hashing for version");
                string hash = await FileUtils.CreateMD5HashAsync(downloadPathCurrent);
                Logging.Editor("Database MD5: {0}", LogLevel.Info, package.CRC);
                Logging.Editor("Download MD5: {0}", LogLevel.Info, hash);
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
                string completeDownloadURL = string.Format("{0}{1}/{2}", PrivateStuff.BigmodsFTPUsersRoot, Settings.WoTModpackOnlineFolderVersion, package.ZipFile);
                databaseFtpDownloadsize = await FtpUtils.FtpGetFilesizeAsync(completeDownloadURL, Credential);
                await databaseClient.DownloadFileTaskAsync(completeDownloadURL, downloadPathCurrent);
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
                    Logging.Editor("This zip file does not support auto update, needs xml instructions (download)");
                    return;
                }
                if (filesxml == null)
                {
                    Logging.Editor("This zip file does not support auto update, needs xml instructions (files)");
                    return;
                }
                //extraction in step 1 allows in verbose mode for modifications to be made to the files before step 2
                downloadxml.Extract(Path.Combine(WorkingDirectory, package.PackageName),ExtractExistingFileAction.OverwriteSilently);
                filesxml.Extract(Path.Combine(WorkingDirectory, package.PackageName), ExtractExistingFileAction.OverwriteSilently);
            }

            Logging.Editor("Finished update process step 1");
        }

        private void Client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if (AutoUpdateProgressBar.Maximum != e.TotalBytesToReceive)
                AutoUpdateProgressBar.Maximum = e.TotalBytesToReceive;

            if(AutoUpdateProgressBar.Minimum != 0)
                AutoUpdateProgressBar.Minimum = 0;

            AutoUpdateProgressBar.Value = e.BytesReceived;
        }

        private void DatabaseClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            if(AutoUpdateProgressBar.Maximum != databaseFtpDownloadsize)
                AutoUpdateProgressBar.Maximum = databaseFtpDownloadsize;

            if (AutoUpdateProgressBar.Minimum != 0)
                AutoUpdateProgressBar.Minimum = 0;

            AutoUpdateProgressBar.Value = e.BytesReceived;
        }

        private async Task UpdateProcessStep2()
        {
            Logging.Editor("Starting update process step 2");

            DatabasePackage package = PackageNamesListbox.SelectedItems[0] as DatabasePackage;
            //parse download instructions xml files
            XmlDocument downloadDocument = XmlUtils.LoadXmlDocument(Path.Combine(WorkingDirectory, package.PackageName, "_autoUpdate", "download.xml"), XmlLoadType.FromFile);

            if (downloadDocument == null)
            {
                Logging.Editor("Failed to parse download xml document");
                return;
            }

            //parse to class objects
            package.DownloadInstructions = ParseDownloadInstructions(downloadDocument, package.DownloadInstructions.DownloadedDatabaseZipFileLocation);

            //get download URL string based on download instructions type
            string directDownloadURL = string.Empty;
            Logging.Editor("Getting download URL");
            switch(package.DownloadInstructions.DownloadType)
            {
                case DownloadTypes.StaticLink:
                    directDownloadURL = package.DownloadInstructions.UpdateURL;
                    break;
                case DownloadTypes.WgMods:
                    directDownloadURL = await GetWGmodsDownloadLink(package.DownloadInstructions.UpdateURL);
                    break;
            }

            //check that download URL is valid
            if (string.IsNullOrWhiteSpace(directDownloadURL))
            {
                Logging.Editor("Download URL is blank", LogLevel.Error);
                return;
            }
            else
                Logging.Editor("Download URL is valid, attempting to download file");

            //create download string and download the file
            string downloadLocation = Path.Combine(WorkingDirectory, package.PackageName, package.DownloadInstructions.DownloadFilename);
            package.DownloadInstructions.DownloadedFileLocation = downloadLocation;

            if (File.Exists(downloadLocation))
                File.Delete(downloadLocation);

            await client.DownloadFileTaskAsync(directDownloadURL, downloadLocation);
            AutoUpdateProgressBar.Value = AutoUpdateProgressBar.Minimum;
            Logging.Editor("File downloaded, finished update process step 2");
        }

        private async Task UpdateProcessStep3()
        {
            Logging.Editor("Starting update process step 3: Loading files xml instructions");
            DatabasePackage package = PackageNamesListbox.SelectedItems[0] as DatabasePackage;
            //parse download instructions xml files
            XmlDocument filesDocument = XmlUtils.LoadXmlDocument(Path.Combine(WorkingDirectory, package.PackageName, "_autoUpdate", "files.xml"), XmlLoadType.FromFile);

            if (filesDocument == null)
            {
                Logging.Editor("Failed to parse files xml document");
                return;
            }

            //parse to class objects
            package.UpdateInstructions = ParseUpdateInstructions(filesDocument);

            Logging.Editor("Starting update zip file process");
            bool processUpdate = false;
            switch(package.UpdateInstructions.UpdateType)
            {
                case UpdateTypes.wotmod:
                    processUpdate = await ProcessWotmodUpdate(package);
                    break;
            }

            //output (move) updated zip file if successful creation
            if (processUpdate)
            {
                Logging.Editor("Update process complete, moving new zip to output directory");
                string locationToMoveTo = Path.Combine(UpdateOutputDirectory, Path.GetFileName(package.DownloadInstructions.DownloadedDatabaseZipFileLocation));
                if (File.Exists(locationToMoveTo))
                    File.Delete(locationToMoveTo);
                File.Move(package.DownloadInstructions.DownloadedDatabaseZipFileLocation, locationToMoveTo);

                //change the name if the end is in the pattern yyyy-mm-dd.zip
                Logging.Editor("Changing date of filename to today");
                string regexPattern = @"\d\d\d\d[-_]\d\d[-_]\d\d.zip$";
                string currentFileName = Path.GetFileName(locationToMoveTo);
                Logging.Editor("Current filename: {0}", LogLevel.Info, currentFileName);

                if (Regex.IsMatch(currentFileName,regexPattern))
                {
                    string newFileNameMatch = string.Format("{0}.zip", DateTime.Now.ToString("yyyy-MM-dd"));
                    string newFilename = Regex.Replace(currentFileName, regexPattern, newFileNameMatch);
                    string newFileLocation = Path.Combine(Path.GetDirectoryName(locationToMoveTo), newFilename);

                    //if it matches, then it's being updated no the same day. need to append "_x" to it
                    if (newFilename.Equals(currentFileName))
                    {
                        Logging.Editor("Current and new filenames match, probably editing twice in a day.",LogLevel.Warning);
                        Logging.Editor("Need to get offset at end of file and increment it", LogLevel.Warning);

                        //get the last two characters of the name (without extension)
                        string endName = Path.GetFileNameWithoutExtension(newFilename);
                        endName = endName.Substring(endName.Length - 2, 2);
                        Logging.Editor("Last 2 characters: '{0}'", LogLevel.Info, endName);
                        if(endName.Contains("_"))
                        {
                            Logging.Editor("Not first time editing in a day, get last int and increment", LogLevel.Warning);
                            int current = int.Parse(endName[1].ToString()) + 1;
                            newFilename = string.Format("{0}_{1}{2}", Path.GetFileNameWithoutExtension(newFilename), current.ToString(), Path.GetExtension(newFilename));
                        }
                        else
                        {
                            Logging.Editor("First multi edit in a day, add '_1'");
                            newFilename = string.Format("{0}_1{1}", Path.GetFileNameWithoutExtension(newFilename), Path.GetExtension(newFilename));
                        }

                        //update newFileLocation with newFilename
                        newFileLocation = Path.Combine(Path.GetDirectoryName(locationToMoveTo), newFilename);

                        /*
                        int offset = 1;
                        string oldNewFilename = newFilename;
                        while (File.Exists(newFileLocation))
                        {
                            newFilename = string.Format("{0}_{1}{2}", Path.GetFileNameWithoutExtension(oldNewFilename), offset++.ToString(), Path.GetExtension(oldNewFilename));
                            newFileLocation = Path.Combine(Path.GetDirectoryName(locationToMoveTo), newFilename);
                        }
                        */
                    }

                    Logging.Editor("New filename:     {0}", LogLevel.Info, newFilename);
                    if (File.Exists(newFileLocation))
                    {
                        Logging.Editor("File already exists, overwriting",LogLevel.Warning);
                        File.Delete(newFileLocation);
                    }
                    File.Move(locationToMoveTo, newFileLocation);
                    Logging.Editor("New output file exists at {0}", LogLevel.Info, newFileLocation);
                }
                else
                {
                    Logging.Editor("Failed to process new filename (is not correct format?)",LogLevel.Error);
                    return;
                }
            }

            Logging.Editor("Finished update process step 3");
        }

        private async Task<bool> ProcessWotmodUpdate(DatabasePackage package)
        {
            Logging.Editor("Processing wotmod update");

            DownloadInstructions downloadInstructions = package.DownloadInstructions;
            UpdateInstructions updateInstructions = package.UpdateInstructions;

            //verify that only one wotmod file exists in database file and get crc
            Logging.Editor("Checking for only 1 .wotmod file in the database zip file");
            using (ZipFile databaseZip = new ZipFile(downloadInstructions.DownloadedDatabaseZipFileLocation))
            {
                ZipEntry wotmodEntry = null;
                foreach (ZipEntry entry in databaseZip)
                {
                    if(entry.FileName.Contains(".wotmod"))
                    {
                        Logging.Editor("Found entry {0}", LogLevel.Info, entry.FileName);
                        if (wotmodEntry != null)
                        {
                            Logging.Editor("Entry for wotmod processing already exists and will be overriden!", LogLevel.Error);
                        }
                        wotmodEntry = entry;
                        updateInstructions.WotmodOldFilenameInZip = entry.FileName;
                    }
                }
                updateInstructions.WotmodDatabaseMD5 = await FileUtils.CreateMD5HashAsync(wotmodEntry.OpenReader());
            }

            //compare md5 of file in database zip to md5 of downloaded file
            updateInstructions.WotmodDownloadedMD5 = await FileUtils.CreateMD5HashAsync(downloadInstructions.DownloadedFileLocation);
            Logging.Editor("MD5 of download wotmod: {0}", LogLevel.Info, updateInstructions.WotmodDownloadedMD5);
            Logging.Editor("MD5 of database wotmod: {0}", LogLevel.Info, updateInstructions.WotmodDatabaseMD5);

            if(updateInstructions.WotmodDownloadedMD5.Equals(updateInstructions.WotmodDatabaseMD5))
            {
                Logging.Editor("MD5 files match, no need to update package");
                //DEBUG: comment this out to test method
                return false;
            }

            //update wotmod file in zip
            Logging.Editor("MD5s don't match, updating wotmod in database zip with downloaded one");
            
            using (ZipFile databaseZip = new ZipFile(downloadInstructions.DownloadedDatabaseZipFileLocation))
            {
                //remove current entry and add new entry
                await Task.Run(() =>
                {
                    databaseZip.RemoveEntry(databaseZip[updateInstructions.WotmodOldFilenameInZip]);
                    databaseZip.AddEntry(updateInstructions.WotmodFilenameInZip, File.ReadAllBytes(downloadInstructions.DownloadedFileLocation));
                });

                //process patch instructions
                Logging.Editor("Processing patches");
                int patchesCount = 0;
                foreach (PatchUpdate patchUpdate in updateInstructions.PatchUpdates)
                {
                    Logging.Editor("Processing patch {0} of {1}", LogLevel.Info, ++patchesCount, updateInstructions.PatchUpdates.Count);
                    Logging.Editor(patchUpdate.PatchUpdateInformation);
                    UiUtils.AllowUIToUpdate();
                    if(!ProcessUpdatePatch(patchUpdate, databaseZip, package.PackageName))
                    {
                        Logging.Editor("Failed to process update patch {0}", LogLevel.Error, patchesCount);
                        return false;
                    }
                }

                //save zip changes to disk
                Logging.Editor("Saving zip file changes to disk");
                databaseZip.SaveProgress += DatabaseZip_SaveProgress;
                await Task.Run(() =>
                {
                    databaseZip.Save();
                });
            }

            Logging.Editor("Save complete");
            AutoUpdateProgressBar.Value = AutoUpdateProgressBar.Minimum;
            return true;
        }

        private bool ProcessUpdatePatch(PatchUpdate patchUpdate, ZipFile zip, string packageName)
        {
            string patchProcessWD = Path.Combine(WorkingDirectory, packageName, "PatchProcessing");
            if (Directory.Exists(patchProcessWD))
                Directory.Delete(patchProcessWD, true);
            if (!Directory.Exists(patchProcessWD))
                Directory.CreateDirectory(patchProcessWD);

            //locate via zip files list regex search
            List<ZipEntry> matchingZipEntries = new List<ZipEntry>();
            Logging.Editor("Checking for entries that match the regex '{0}'", LogLevel.Debug, patchUpdate.PatchesToUpdate);
            foreach(ZipEntry zipEntry in zip)
            {
                if(Regex.IsMatch(zipEntry.FileName,patchUpdate.PatchesToUpdate))
                {
                    Logging.Editor("Match found: {0}", LogLevel.Debug, zipEntry.FileName);
                    matchingZipEntries.Add(zipEntry);
                }
            }

            Dictionary<string, string> patchNames = new Dictionary<string, string>();

            //for each found, extract, load, xpath, search, replace, update
            foreach(ZipEntry entryMatch in matchingZipEntries)
            {
                //extract text
                //https://stackoverflow.com/a/16187809/3128017
                string xmlText = string.Empty;
                using (MemoryStream stream = new MemoryStream())
                {
                    entryMatch.Extract(stream);
                    stream.Position = 0;
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        xmlText = reader.ReadToEnd();
                    }
                }

                //load to document
                XmlDocument document = XmlUtils.LoadXmlDocument(xmlText, XmlLoadType.FromString);
                if(document == null)
                {
                    Logging.Editor("Failed to load xml document from zip file", LogLevel.Error);
                    return false;
                }

                //do an xpath search
                XmlNodeList matchingNodes = XmlUtils.GetXmlNodesFromXPath(document, patchUpdate.XPath);
                int matches = 0;
                if (matchingNodes != null)
                    matches = matchingNodes.Count;
                Logging.Editor("Matching nodes: {0}", LogLevel.Debug, matches);
                if(matches == 0)
                {
                    Logging.Editor("0 matches, is this correct?", LogLevel.Warning);
                    continue;
                }

                //if regex search match, replace
                bool regexMatch = false;
                foreach(XmlNode matchNode in matchingNodes)
                {
                    string nodeText = matchNode.InnerText.Trim();
                    Logging.Editor("Checking for regex match: Text='{0}', Pattern='{1}'", LogLevel.Info, nodeText, patchUpdate.Search);
                    if (Regex.IsMatch(nodeText,patchUpdate.Search))
                    {
                        regexMatch = true;
                        Logging.Editor("Match found", LogLevel.Info);
                        nodeText = Regex.Replace(nodeText, patchUpdate.Search, patchUpdate.Replace);
                        Logging.Editor("Replaced to '{0}'", LogLevel.Info, nodeText);
                        matchNode.InnerText = nodeText;
                    }
                }

                if(!regexMatch)
                {
                    Logging.Editor("Regex was never matched, is this correct?", LogLevel.Warning);
                    continue;
                }

                //get just the name of the patch xml and save it to PatchProcessing dir
                string patchNameFromZip = Path.GetFileName(entryMatch.FileName);
                document.Save(Path.Combine(patchProcessWD, patchNameFromZip));
                patchNames.Add(entryMatch.FileName, patchNameFromZip);
            }

            //if any files exist in the directory, then they were modified and should be updated
            for(int i = 0; i < patchNames.Count; i++)
            {
                //https://stackoverflow.com/questions/40412340/c-sharp-dictionary-get-item-by-index
                string key = patchNames.ElementAt(i).Key;//zip entry string
                string value = patchNames.ElementAt(i).Value;//just filename in PatchProcessing folder
                Logging.Editor("Updating patch {0}", LogLevel.Info, value);
                zip.RemoveEntry(key);
                zip.AddFile(Path.Combine(patchProcessWD, value), "_patch");
            }

            return true;
        }

        private void DatabaseZip_SaveProgress(object sender, SaveProgressEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                AutoUpdateProgressBar.Maximum = e.TotalBytesToTransfer;
                AutoUpdateProgressBar.Minimum = 0;
                AutoUpdateProgressBar.Value = e.BytesTransferred;
            });
        }

        private DownloadInstructions ParseDownloadInstructions(XmlDocument doc, string databaseZipFileDownloadLocation)
        {
            DownloadInstructions instructions = new DownloadInstructions() { DownloadedDatabaseZipFileLocation = databaseZipFileDownloadLocation };
            string formatVersion = doc.DocumentElement.Attributes["formatVersion"].Value.Trim();
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
                        instructions.ModVersion = node.InnerText.Trim();
                        break;
                    case "ClientVersion":
                        instructions.ClientVersion = node.InnerText.Trim();
                        break;
                    case "DownloadType":
                        instructions.DownloadType = (DownloadTypes)Enum.Parse(instructions.DownloadType.GetType(), node.InnerText.Trim());
                        break;
                    case "UpdateURL":
                        instructions.UpdateURL = node.InnerText.Trim();
                        break;
                    case "DownloadFilename":
                        instructions.DownloadFilename = node.InnerText.Trim();
                        break;
                }
            }
            return instructions;
        }

        private UpdateInstructions ParseUpdateInstructions(XmlDocument doc)
        {
            UpdateInstructions instructions = new UpdateInstructions();
            string formatVersion = doc.DocumentElement.Attributes["formatVersion"].Value.Trim();
            instructions.InstructionsVersion = formatVersion;
            switch (formatVersion)
            {
                case "1.0":
                    ParseUpdateInstructionsV1(instructions, doc);
                    break;
            }

            return instructions;
        }

        private UpdateInstructions ParseUpdateInstructionsV1(UpdateInstructions instructions, XmlDocument doc)
        {
            //public string InstructionsVersion { get; set; } (already got)
            //public UpdateTypes UpdateType { get; set; }
            //public string WotmodFilenameInZip { get; set; }
            //public string WotmodMD5 { get; set; }
            foreach (XmlNode node in doc.ChildNodes[1].ChildNodes)
            {
                switch (node.Name)
                {
                    case nameof(instructions.WotmodFilenameInZip):
                        instructions.WotmodFilenameInZip = node.InnerText.Trim();
                        break;
                    case nameof(instructions.UpdateType):
                        instructions.UpdateType = (UpdateTypes)Enum.Parse(instructions.UpdateType.GetType(), node.InnerText.Trim());
                        break;
                    case nameof(instructions.PatchUpdates):
                        instructions.PatchUpdates = ParsePatchUpdates(node);
                        break;
                }
            }
            return instructions;
        }

        private List<PatchUpdate> ParsePatchUpdates(XmlNode patchNodee)
        {
            List<PatchUpdate> patchUpdates = new List<PatchUpdate>();
            foreach(XmlNode node in patchNodee.ChildNodes)
            {
                PatchUpdate patchUpdate = new PatchUpdate();
                foreach(XmlNode patchNode in node.ChildNodes)
                {
                    switch(patchNode.Name)
                    {
                        case nameof(patchUpdate.PatchesToUpdate):
                            patchUpdate.PatchesToUpdate = patchNode.InnerText.Trim();
                            break;
                        case nameof(patchUpdate.XPath):
                            patchUpdate.XPath = patchNode.InnerText.Trim();
                            break;
                        case nameof(patchUpdate.Search):
                            XmlAttribute singleReturnAttribute = patchNode.Attributes["single"];
                            if(singleReturnAttribute != null)
                            {
                                patchUpdate.SearchReturnFirst = bool.Parse(singleReturnAttribute.InnerText.Trim());
                                Logging.Editor("Search single attribute found and processed as {0}", LogLevel.Debug, patchUpdate.SearchReturnFirst);
                            }
                            patchUpdate.Search = patchNode.InnerText.Trim();
                            break;
                        case nameof(patchUpdate.Replace):
                            patchUpdate.Replace = patchNode.InnerText.Trim();
                            break;
                    }
                }
                patchUpdates.Add(patchUpdate);
            }
            return patchUpdates;
        }

        private async Task<string> GetWGmodsDownloadLink(string wgmodsBaseUrl)
        {
            bool browserLoaded = false;
            int browserLoadHits = 0;
            browser.LoadCompleted += (sendahh, endArgs) =>
            {
                if(++browserLoadHits >= 1)
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

            //get the entire loaded html document as a string
            var doc = browser.Document as mshtml.HTMLDocument;
            string s = doc.body.outerHTML;

            //load string into html document
            //http://blog.olussier.net/2010/03/30/easily-parse-html-documents-in-csharp/
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(s);
            HtmlNode node = document.DocumentNode;

            //attempt to get client version text and download link text
            //https://stackoverflow.com/questions/1390568/how-can-i-match-on-an-attribute-that-contains-a-certain-string
            HtmlNodeCollection clientVersionNode = node.SelectNodes(@"//div[contains(@class, 'ModDetails_label')]");
            string version = string.Empty;
            HtmlNode downloadUrlNode = node.SelectSingleNode(@"//a[contains(@class, 'ModDetails_hidden')]");
            string downloadURL = string.Empty;

            //parse html nodes into string values
            if (clientVersionNode != null && clientVersionNode.Count >= 4)
            {
                HtmlNode nodeTest = clientVersionNode[3];
                HtmlNode versionNode = nodeTest.ChildNodes[0].ChildNodes[1];
                version = versionNode.InnerText;
            }
            if(downloadUrlNode != null)
            {
                downloadURL = downloadUrlNode.Attributes["href"].Value;
            }

            //display to user
            Logging.Editor(string.Format("For client: {0}, download link: {1}",
                string.IsNullOrEmpty(version)? "(null)" : version,
                string.IsNullOrEmpty(downloadURL) ? "(null)" : downloadURL));

            //check for empty string parsed values
            if(string.IsNullOrEmpty(version))
            {
                Logging.Editor("clientVersionNode is incorrect format (count = {0}), possibly HTML did not completely load?", LogLevel.Warning,
                    clientVersionNode == null ? "null" : clientVersionNode.Count.ToString());
            }
            if(string.IsNullOrEmpty(downloadURL))
            {
                Logging.Editor("downloadUrlNode is null, possibly HTML did not completely load?", LogLevel.Error);
                return null;
            }

            return downloadURL;
        }

        private void ResetUpdateProcessButton_Click(object sender, RoutedEventArgs e)
        {
            ResetUpdateProcessButton.IsEnabled = false;
            CurrentUpdateStep = 1;
            StartContinueUpdateProcessButton.Content = "Start";
            if (!StartContinueUpdateProcessButton.IsEnabled)
                StartContinueUpdateProcessButton.IsEnabled = true;
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

        private void ClearLogButton_Click(object sender, RoutedEventArgs e)
        {
            LogfileTextbox.Clear();
        }

        private void LogfileTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            LogfileTextbox.ScrollToEnd();
        }
    }
}
