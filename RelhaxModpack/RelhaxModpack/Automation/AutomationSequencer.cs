using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using RelhaxModpack.Utilities;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Xml;
using System.IO;
using RelhaxModpack.Database;
using System.Xml.Linq;
using System.Reflection;
using RelhaxModpack.Common;
using RelhaxModpack.Windows;
using RelhaxModpack.Settings;
using System.Diagnostics;
using System.Threading;
using RelhaxModpack.UI;

namespace RelhaxModpack.Automation
{
    /// <summary>
    /// Handles the parsing, running and error management of all automation sequences loaded and set to run.
    /// </summary>
    public class AutomationSequencer : IDisposable, IComponentWithID
    {
        /// <summary>
        /// The API URL to return a json format document of the current branches in the automation repository.
        /// </summary>
        public const string BranchesURL = "https://api.github.com/repos/Relhax-Modpack-Team/DatabaseAutoUpdateScripts/branches";

        /// <summary>
        /// The url to the repository, macro replaced by branch, that can be used to direct download automation sequences.
        /// </summary>
        public const string AutomationXmlRepoFilebase = "https://raw.githubusercontent.com/Relhax-Modpack-Team/DatabaseAutoUpdateScripts/{branch}/";

        /// <summary>
        /// The name of the automation document that contains all sequences, templates, and macros.
        /// </summary>
        public const string AutomationXmlRootFilename = "root.xml";

        /// <summary>
        /// The default branch of the automation repository.
        /// </summary>
        public const string AutomationRepoDefaultBranch = "master";

        /// <summary>
        /// The list of application defined macros.
        /// </summary>
        public List<AutomationMacro> ApplicationMacros { get; } = new List<AutomationMacro>();

        /// <summary>
        /// The list of globally defined macros.
        /// </summary>
        /// <remarks>This list is defined in a global macros xml document in the automation repository.</remarks>
        public List<AutomationMacro> GlobalMacros { get; } = new List<AutomationMacro>();

        /// <summary>
        /// The list of user defined macros.
        /// </summary>
        /// <remarks>This list is defined in the automation runner's settings document.</remarks>
        private List<AutomationMacro> UserMacros { get; } = new List<AutomationMacro>();

        /// <summary>
        /// Get the list of automation sequences that have been parsed into the sequencer.
        /// </summary>
        public List<AutomationSequence> AutomationSequences { get; } = new List<AutomationSequence>();

        /// <summary>
        /// Get the list of user defined macros parsed from the automation runner's settings document.
        /// </summary>
        /// <remarks>This is a middle-man for loading the user macros from settings document to the UserMacros automation macro list.</remarks>
        /// <seealso cref="UserMacros"/>
        public Dictionary<string, string> UserMacrosDictionary { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Get name used for xml serialization when loading sequences.
        /// </summary>
        public string ComponentInternalName { get; } = "AutomationSequencer";

        /// <summary>
        /// Gets or sets the database manager instance.
        /// </summary>
        public DatabaseManager DatabaseManager { get; set; }

        /// <summary>
        /// Gets or sets the automation runner settings class.
        /// </summary>
        public AutomationRunnerSettings AutomationRunnerSettings { get; set; }

        /// <summary>
        /// Get the number of errors that occurred during a sequence run.
        /// </summary>
        public int NumErrors { get; private set; }

        /// <summary>
        /// Get the database package being currently processed in the running sequence.
        /// </summary>
        public List<DatabasePackage> DatabasePackages { get; private set; }

        /// <summary>
        /// Get the list of source control branches that exist on the automation repository.
        /// </summary>
        public string[] AutomationBranches { get; private set; }

        /// <summary>
        /// Get or set the automation runner window instance.
        /// </summary>
        public DatabaseAutomationRunner DatabaseAutomationRunner { get; set; } = null;

        /// <summary>
        /// The token to allow user cancellation.
        /// </summary>
        public CancellationToken CancellationToken { get; set; }

        /// <summary>
        /// The parsed version of the Wot client.
        /// </summary>
        /// <remarks>This is loaded from the database when parsed.</remarks>
        public string WoTClientVersion { get; set; }

        /// <summary>
        /// The parsed version of the online FTP folder.
        /// </summary>
        /// <remarks>This is loaded form the database when parsed.</remarks>
        public string WoTModpackOnlineFolderVersion { get; set; }

        private XmlDocument RootDocument;

        private XmlDocument GlobalMacrosDocument;

        private WebClient WebClient;

        private string AutomationRepoPathEscaped {
            get
            {
                if (AutomationRunnerSettings == null)
                    return null;
                if (AutomationRunnerSettings.UseLocalRunnerDatabase && string.IsNullOrEmpty(AutomationRunnerSettings.LocalRunnerDatabaseRoot))
                    return null;
                return AutomationRunnerSettings.UseLocalRunnerDatabase ? Path.GetDirectoryName(AutomationRunnerSettings.LocalRunnerDatabaseRoot) : AutomationXmlRepoFilebase.Replace("{branch}", AutomationRunnerSettings.SelectedBranch);
            }
        }

        private string AutomationRepoRootXmlFilepathEscaped
        {
            get
            {
                if (AutomationRunnerSettings == null)
                    return null;
                if (AutomationRunnerSettings.UseLocalRunnerDatabase && string.IsNullOrEmpty(AutomationRunnerSettings.LocalRunnerDatabaseRoot))
                    return null;

                return AutomationRunnerSettings.UseLocalRunnerDatabase ? Path.Combine(AutomationRepoPathEscaped, AutomationXmlRootFilename) : AutomationRepoPathEscaped + AutomationXmlRootFilename;
            }
        }

        private AutomationSequence RunningSequence;

        /// <summary>
        /// Create an instance of the AutomationSequencer class.
        /// </summary>
        public AutomationSequencer()
        {
            WebClient = new WebClient();
        }

        /// <summary>
        /// Load the list of branches from the automation repository.
        /// </summary>
        /// <returns>True if the branches list is loaded, false otherwise.</returns>
        public async Task<bool> LoadBranchesListAsync()
        {
            if (AutomationRunnerSettings == null)
                throw new NullReferenceException();

            if (AutomationRunnerSettings.UseLocalRunnerDatabase)
            {
                Logging.Info("UseLocalRunnerDatabase is true, no need to load branches");
                return true;
            }

            List<string> branches = await CommonUtils.GetListOfGithubRepoBranchesAsync(BranchesURL);

            if (branches == null || branches.Count == 0)
                return false;

            AutomationBranches = branches.ToArray();
            return true;
        }

        /// <summary>
        /// Load the automation root xml document.
        /// </summary>
        /// <returns>True if the document was loaded, false otherwise.</returns>
        /// <remarks>This loads the document into the xml document instance, but has not been parsed.</remarks>
        /// <seealso cref="ParseRootDocument"/>
        public async Task<bool> LoadRootDocumentAsync()
        {
            if (AutomationRunnerSettings == null)
                throw new NullReferenceException();

            if (string.IsNullOrWhiteSpace(AutomationRepoRootXmlFilepathEscaped))
                throw new ArgumentException("AutomationXmlRootEscaped cannot be null or whitespace");

            if (AutomationRunnerSettings.UseLocalRunnerDatabase)
            {
                RootDocument = XmlUtils.LoadXmlDocument(AutomationRepoRootXmlFilepathEscaped, XmlLoadType.FromFile);
            }
            else
            {
                string xmlString = await WebClient.DownloadStringTaskAsync(AutomationRepoRootXmlFilepathEscaped);
                RootDocument = XmlUtils.LoadXmlDocument(xmlString, XmlLoadType.FromString);
            }

            return RootDocument != null;
        }

        /// <summary>
        /// Parse the root xml document.
        /// </summary>
        /// <returns>True if the document was successful parsed, false otherwise.</returns>
        /// <remarks>The document xml instance is parsed by loading its contents into respective fields in this and other object instances.</remarks>
        public bool ParseRootDocument()
        {
            if (RootDocument == null)
                throw new NullReferenceException();
            if (AutomationRunnerSettings == null)
                throw new NullReferenceException();

            Logging.AutomationRunner(LogOptions.MethodName, "Checking root document for to build automation sequences", LogLevel.Info);

            XmlNodeList sequencesXml = XmlUtils.GetXmlNodesFromXPath(RootDocument, "//root.xml/AutomationSequence");
            if (sequencesXml == null || sequencesXml.Count == 0)
                return false;
            AutomationSequences.Clear();

            foreach (XmlElement result in sequencesXml)
            {
                string sequencePackageName = result.Attributes["packageName"].Value;
                string sequenceUID = result.Attributes["UID"].Value;
                string sequenceUrlPath = result.Attributes["path"].Value;
                string sequenceLoadString;

                if (AutomationRunnerSettings.UseLocalRunnerDatabase)
                {
                    sequenceUrlPath = sequenceUrlPath.Replace('/', Path.DirectorySeparatorChar);
                    sequenceLoadString = Path.Combine(AutomationRepoPathEscaped, sequenceUrlPath);
                }
                else
                {
                    sequenceLoadString = AutomationRepoPathEscaped + sequenceUrlPath;
                }

                AutomationSequences.Add(new AutomationSequence(DatabasePackages, ApplicationMacros, GlobalMacros, UserMacros, AutomationRunnerSettings, DatabaseManager, CancellationToken)
                {
                    AutomationSequencer = this,
                    Package = null,
                    PackageName = sequencePackageName,
                    PackageUID = sequenceUID,
                    SequenceDownloadUrl = sequenceLoadString
                });
            }

            return true;
        }

        /// <summary>
        /// Executes the tasks for each sequence to run.
        /// </summary>
        /// <param name="sequencesToRun">The list of sequences to run.</param>
        /// <returns>The exit code of the sequence runs operation.</returns>
        public async Task<SequencerExitCode> RunSequencerAsync(List<AutomationSequence> sequencesToRun)
        {
            if (RootDocument == null)
                throw new NullReferenceException();
            if (AutomationRunnerSettings == null)
                throw new NullReferenceException();
            if (DatabaseManager == null)
                throw new NullReferenceException();
            if (sequencesToRun == null)
                throw new NullReferenceException();
            if (sequencesToRun.Count == 0)
                throw new BadMemeException("packagesToRun must have at least one package to run automation on");

            Logging.Info("Linking database packages for each sequence");
            if (!LinkPackagesToAutomationSequences(sequencesToRun))
                return SequencerExitCode.LinkPackagesToAutomationSequencesFail;

            Logging.Info("Downloading xml for global macros");
            if (!await LoadGlobalMacrosAsync())
                return SequencerExitCode.LoadGlobalMacrosFail;

            Logging.Info("Downloading xml for each sequence");
            if (!await LoadAutomationSequencesXmlToRunAsync(sequencesToRun))
                return SequencerExitCode.LoadAutomationSequencesXmlToRunAsyncFail;

            Logging.Info("Parsing xml for each sequence");
            if (!ParseAutomationSequences(sequencesToRun))
                return SequencerExitCode.ParseAutomationSequencesPreRunFail;

            Logging.Info("Running sequences");
            return await RunSequencesAsync(sequencesToRun);
        }

        private bool LinkPackagesToAutomationSequences(List<AutomationSequence> sequencesToRun)
        {
            UpdateDatabasePackageList();
            foreach (AutomationSequence automationSequence in sequencesToRun)
            {
                automationSequence.DatabasePackages = DatabasePackages;
                Logging.Debug("Linking sequence to package reference: {0}, {1}", automationSequence.PackageName, automationSequence.PackageUID);

                DatabasePackage result = DatabasePackages.Find(pack => pack.UID.Equals(automationSequence.PackageUID));
                if (result == null)
                {
                    Logging.Error("A package does not exist in the database matching UID {0}", automationSequence.PackageUID);
                    return false;
                }

                if (result.PackageName != automationSequence.PackageName)
                {
                    Logging.Warning(Logfiles.AutomationRunner, "The packageName property is out of date. From database: {0}. From Package: {1}", result.PackageName, automationSequence.PackageName);
                }

                automationSequence.Package = result;
            }

            return true;
        }

        private async Task<bool> LoadGlobalMacrosAsync()
        {
            if (RootDocument == null)
                throw new NullReferenceException();
            if (AutomationRunnerSettings == null)
                throw new NullReferenceException();

            //get the url to download from
            string globalMacrosUrlFile = XmlUtils.GetXmlStringFromXPath(RootDocument, "/root.xml/GlobalMacros/@path");
            if (AutomationRunnerSettings.UseLocalRunnerDatabase)
            {
                string globalMacrosUrl = Path.Combine(AutomationRepoPathEscaped, globalMacrosUrlFile);
                GlobalMacrosDocument = XmlUtils.LoadXmlDocument(globalMacrosUrl, XmlLoadType.FromFile);
            }
            else
            {
                string globalMacrosUrl = string.Format("{0}{1}", AutomationRepoPathEscaped, globalMacrosUrlFile);
                string globalMacrosXml = await WebClient.DownloadStringTaskAsync(globalMacrosUrl);
                GlobalMacrosDocument = XmlUtils.LoadXmlDocument(globalMacrosXml, XmlLoadType.FromString);
            }

            return GlobalMacrosDocument != null;
        }

        private async Task<bool> LoadAutomationSequencesXmlToRunAsync(List<AutomationSequence> sequencesToRun)
        {
            foreach (AutomationSequence automationSequence in sequencesToRun)
            {
                Logging.Debug("Parsing sequence: {0}, {1}", automationSequence.PackageName, automationSequence.PackageUID);

                if (!(await automationSequence.LoadAutomationXmlAsync()))
                {
                    Logging.Error("Failed to load xml for sequence {0}", automationSequence.ComponentInternalName);
                    return false;
                }
            }

            return true;
        }

        private bool ParseAutomationSequences(List<AutomationSequence> sequencesToRun)
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Parsing each automationSequence from xml to class objects");
            foreach (AutomationSequence automationSequence in sequencesToRun)
            {
                Logging.Info(Logfiles.AutomationRunner, "Load automation sequence data for package {0}", automationSequence.Package.PackageName);
                if (!automationSequence.ParseAutomationTasks())
                {
                    Logging.Error("Failed to parse sequence {0}, check the syntax and try again", automationSequence.ComponentInternalName);
                    return false;
                }
            }
            return true;
        }

        private async Task<SequencerExitCode> RunSequencesAsync(List<AutomationSequence> sequencesToRun)
        {
            RunningSequence = null;
            if (sequencesToRun.Count == 0)
            {
                Logging.Error(Logfiles.AutomationRunner, LogOptions.ClassName, "No sequences specified in AutomationSequences list");
                return SequencerExitCode.NotRun;
            }

            Logging.Info(Logfiles.AutomationRunner, LogOptions.ClassName, "Running automation sequencer");
            NumErrors = 0;

            //backup the original text color if not done already, else set it if it's not already set
            foreach (AutomationSequence sequenceToRun in sequencesToRun)
            {
                if (sequenceToRun.AutomationComboBoxItem == null)
                    break;
                if (sequenceToRun.AutomationComboBoxItem.DefaultForgroundBrush == null)
                    sequenceToRun.AutomationComboBoxItem.DefaultForgroundBrush = sequenceToRun.AutomationComboBoxItem.Foreground;
                else if (sequenceToRun.AutomationComboBoxItem.Foreground != sequenceToRun.AutomationComboBoxItem.DefaultForgroundBrush)
                    sequenceToRun.AutomationComboBoxItem.Foreground = sequenceToRun.AutomationComboBoxItem.DefaultForgroundBrush;
            }

            SequencerExitCode exitCode;
            foreach (AutomationSequence sequence in sequencesToRun)
            {
                RunningSequence = sequence;
                Logging.Info(Logfiles.AutomationRunner, LogOptions.ClassName, "Preparing macro lists for sequence run: {0}", sequence.ComponentInternalName);

                //handling macros (application, global, local/user)
                if (!ResetApplicationMacros(sequence))
                    return SequencerExitCode.ResetApplicationMacrosFail;
                if (!ParseGlobalMacros())
                    return SequencerExitCode.LoadGlobalMacrosFail;
                LoadUserMacros();
                if (!sequence.ParseSequenceMacros())
                    return SequencerExitCode.LoadLocalMacrosFail;

                //handling if dumping macros and environment vars to disk
                if (AutomationRunnerSettings.DumpParsedMacrosPerSequenceRun)
                {
                    DumpApplicationMacros();
                    DumpGlobalMacros();
                }
                if (AutomationRunnerSettings.DumpShellEnvironmentVarsPerSequenceRun)
                {
                    DumpShellEnvironmentVariables();
                }

                Logging.Info(Logfiles.AutomationRunner, LogOptions.ClassName, "Running sequence: {0}", sequence.ComponentInternalName);
                await sequence.RunTasksAsync();
                exitCode = sequence.ExitCode;
                Logging.Info("----------------------- SEQUENCE RESULTS -----------------------");
                switch (exitCode)
                {
                    case SequencerExitCode.Errors:
                    case SequencerExitCode.NotRun:
                        Logging.Error(Logfiles.AutomationRunner, LogOptions.ClassName, "Sequence {0} result {1}. Check the log above or enable verbose logging for details.", sequence.ComponentInternalName, exitCode.ToString());
                        NumErrors++;
                        break;

                    case SequencerExitCode.NoErrors:
                    case SequencerExitCode.Cancel:
                        Logging.Info(Logfiles.AutomationRunner, LogOptions.ClassName, "Sequence {0} result {1}.", sequence.ComponentInternalName, exitCode.ToString());
                        break;
                }
                Logging.Info("----------------------------------------------------------------");

                //set color code as well if the comboboxes aren't null
                if (sequence.AutomationComboBoxItem != null)
                {
                    if (sequence.ExitCode == SequencerExitCode.Errors)
                        sequence.AutomationComboBoxItem.Foreground = System.Windows.Media.Brushes.Red;
                    else if (sequence.ExitCode == SequencerExitCode.NoErrors)
                        sequence.AutomationComboBoxItem.Foreground = System.Windows.Media.Brushes.Green;
                }

                if (NumErrors == 0)
                {
                    Logging.Info("Saving database before next sequence");
                    DatabaseManager.SaveDatabase(AutomationRunnerSettings.DatabaseSavePath);
                }
            }

            Logging.Info(Logfiles.AutomationRunner, LogOptions.ClassName, "Sequence run finished with {0} errors of {1} total sequences.", NumErrors, sequencesToRun.Count);
            RunningSequence = null;

            if (CancellationToken != null && CancellationToken.IsCancellationRequested)
                return SequencerExitCode.Cancel;
            else if (NumErrors == 0)
                return SequencerExitCode.NoErrors;
            else
                return SequencerExitCode.Errors;
        }

        private bool ResetApplicationMacros(AutomationSequence sequence)
        {
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.ClassName, "Resetting application macros");

            //check that required automation settings are set for being able to reset macaros
            if (string.IsNullOrEmpty(AutomationRunnerSettings.WoTClientInstallLocation))
            {
                Logging.Error("WoTClientInstallLocation is not set in settings tab!");
                return false;
            }
            if (!File.Exists(AutomationRunnerSettings.WoTClientInstallLocation))
            {
                Logging.Error("WoTClientInstallLocation file does not exist!");
                return false;
            }

            DatabasePackage package = sequence.Package;
            SelectablePackage selectablePackage = package as SelectablePackage;
            ApplicationMacros.Clear();
            ApplicationMacros.Add(new AutomationMacro() { Name = "date", Value = DateTime.UtcNow.ToString("yyyy-MM-dd"), MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "category.name", Value = (selectablePackage != null) ? selectablePackage.ParentCategory.Name : "null", MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "name", Value = (selectablePackage != null) ? selectablePackage.NameFormatted : "null", MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "packageName", Value = package.PackageName, MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "packageUID", Value = package.UID, MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "zipfile", Value = package.ZipFile, MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "level", Value = (selectablePackage != null) ? selectablePackage.Level.ToString() : "null", MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "patchGroup", Value = package.PatchGroup.ToString(), MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "installGroup", Value = package.InstallGroupWithOffset.ToString(), MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "clientVersion", Value = WoTClientVersion, MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "onlineFolderVersion", Value = WoTModpackOnlineFolderVersion, MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "author", Value = package.Author, MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "version", Value = package.Version, MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "applicationPath", Value = ApplicationConstants.ApplicationStartupPath, MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "relhaxTemp", Value = ApplicationConstants.RelhaxTempFolderPath, MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "workDirectory", Value = string.Format("{0}\\{1}", ApplicationConstants.RelhaxTempFolderPath, package.PackageName), MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "automationRepoRoot", Value = AutomationRepoPathEscaped, MacroType = MacroType.ApplicationDefined });
            ApplicationMacros.Add(new AutomationMacro() { Name = "clientPath", Value = Path.GetDirectoryName(AutomationRunnerSettings.WoTClientInstallLocation) });
            return true;
        }

        private bool ParseGlobalMacros()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Parsing global macros from xml document to class objects");
            GlobalMacros.Clear();
            XmlElement result = XmlUtils.GetXmlNodeFromXPath(GlobalMacrosDocument, "/GlobalMacros") as XmlElement;
            XElement globalMacrosXmlHolder = XElement.Parse(result.OuterXml);
            PropertyInfo listPropertyInfo = this.GetType().GetProperty(nameof(GlobalMacros));

            try
            {
                if (!CommonUtils.SetListEntries(this, listPropertyInfo, globalMacrosXmlHolder.Elements()))
                    return false;
            }
            catch (Exception ex)
            {
                Logging.AutomationRunner(ex.ToString(), LogLevel.Exception);
                return false;
            }

            foreach(AutomationMacro macro in GlobalMacros)
            {
                macro.MacroType = MacroType.Global;
            }
            return true;
        }

        private void LoadUserMacros()
        {
            UserMacros.Clear();
            foreach(KeyValuePair<string, string> userMacro in UserMacrosDictionary)
            {
                UserMacros.Add(new AutomationMacro() { Name = userMacro.Key, Value = userMacro.Value, MacroType = MacroType.Local });
            }
        }

        private void DumpApplicationMacros()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.ClassName, "Dumping application parsed macros. Count = {0}", ApplicationMacros.Count);
            foreach (AutomationMacro macro in ApplicationMacros)
            {
                Logging.Info(Logfiles.AutomationRunner, LogOptions.None, "Macro: Name = {0}, Value = {1}", macro.Name, macro.Value);
            }
        }

        private void DumpGlobalMacros()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.ClassName, "Dumping global parsed macros. Count = {0}", GlobalMacros.Count);
            foreach (AutomationMacro macro in GlobalMacros)
            {
                Logging.Info(Logfiles.AutomationRunner, LogOptions.None, "Macro: Name = {0}, Value = {1}", macro.Name, macro.Value);
            }
        }

        private void DumpShellEnvironmentVariables()
        {
            //dump vars before run
            Logging.AutomationRunner("Dumping current shell environment variables", LogLevel.Debug);
            using (Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    //Setting this property to false enables you to redirect input, output, and error streams
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            })
            {
                //https://stackoverflow.com/a/141098/3128017
                foreach (KeyValuePair<string, string> keyValuePair in process.StartInfo.Environment)
                {
                    Logging.AutomationRunner("Key = {0}, Value = {1}", LogLevel.Debug, keyValuePair.Key, keyValuePair.Value);
                }
            }
        }

        /// <summary>
        /// Updates the DatabasePackages property with the loaded packages from the database manager.
        /// </summary>
        public void UpdateDatabasePackageList()
        {
            DatabasePackages = DatabaseManager.GetFlatList();
        }

        /// <summary>
        /// Cleans the working directories for all sequences.
        /// </summary>
        /// <param name="reporter">The progress reporting provider.</param>
        /// <param name="progress">The object to report progress updates.</param>
        /// <param name="token">The token to allow cancellation of the operation.</param>
        public async Task CleanWorkingDirectoriesAsync(IProgress<RelhaxProgress> reporter, RelhaxProgress progress, CancellationToken token)
        {
            Logging.Info("Cleaning Working directories start");
            await Task.Run(() => CleanWorkingDirectories(reporter, token: token, progress: progress));
            Logging.Info("Cleaning Working directories finish");
        }

        private void CleanWorkingDirectories(IProgress<RelhaxProgress> reporter, RelhaxProgress progress, CancellationToken token)
        {
            if (AutomationSequences == null || AutomationSequences.Count == 0)
                return;

            //get the list of files
            List<string> FilesToDelete = new List<string>();
            List<string> FoldersToGetFilesFrom = AutomationSequences.Select(sequence => sequence.PackageName).ToList();

            foreach (string folder in FoldersToGetFilesFrom)
            {
                string folderPath = Path.Combine(ApplicationConstants.RelhaxTempFolderPath, folder);
                if (Directory.Exists(folderPath))
                    FilesToDelete.AddRange(FileUtils.FileSearch(folderPath, SearchOption.AllDirectories, false, true));

                if (token != null && token.IsCancellationRequested)
                    return;
            }

            if (progress != null)
            {
                progress.ChildTotal = FilesToDelete.Count;
                progress.ChildCurrent = progress.ParrentCurrent = 0;
                reporter?.Report(progress);
            }


            foreach (string file in FilesToDelete)
            {
                progress.ChildCurrent++;
                progress.ChildCurrentProgress = string.Format("Deleting file {0} of {1}", progress.ChildCurrent, progress.ChildTotal);
                reporter.Report(progress);

                FileUtils.FileDelete(file);
                if (token != null && token.IsCancellationRequested)
                    return;

                if (progress != null)
                {
                    progress.ChildCurrent = progress.ChildTotal;
                    progress.ChildCurrentProgress = string.Format("Finishing up");
                    reporter?.Report(progress);
                }
            }


            foreach (string folder in FoldersToGetFilesFrom)
            {
                string folderPath = Path.Combine(ApplicationConstants.RelhaxTempFolderPath, folder);
                if (Directory.Exists(folderPath))
                    FileUtils.DirectoryDelete(folderPath, true, true);

                if (token != null && token.IsCancellationRequested)
                    return;
            }
        }

        /// <summary>
        /// Cancel the current sequence operation.
        /// </summary>
        public void CancelSequence()
        {
            if (RunningSequence != null)
                RunningSequence.CancelTask();
        }

        /// <summary>
        /// Dispose of unmanaged resources (i.e. the web client)
        /// </summary>
        public void Dispose()
        {
            ApplicationMacros.Clear();
            GlobalMacros.Clear();
            AutomationSequences.Clear();
            ((IDisposable)WebClient).Dispose();
        }
    }
}
