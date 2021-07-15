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

namespace RelhaxModpack.Automation
{
    public class AutomationSequencer : IDisposable, IComponentWithID
    {
        /// <summary>
        /// The API URL to return a json format document of the current branches in the automation repository
        /// </summary>
        public const string BranchesURL = "https://api.github.com/repos/Relhax-Modpack-Team/DatabaseAutoUpdateScripts/branches";

        public const string AutomationXmlRepoFilebase = "https://raw.githubusercontent.com/Relhax-Modpack-Team/DatabaseAutoUpdateScripts/{branch}/";

        public const string AutomationXmlRootFilename = "root.xml";

        public const string AutomationRepoDefaultBranch = "master";

        public List<AutomationMacro> ApplicationMacros { get; } = new List<AutomationMacro>();

        public List<AutomationMacro> GlobalMacros { get; } = new List<AutomationMacro>();

        public List<AutomationSequence> AutomationSequences { get; } = new List<AutomationSequence>();

        public List<DatabasePackage> DatabasePackages { get; private set; }

        public DatabaseManager DatabaseManager { get; set; }

        public AutomationRunnerSettings AutomationRunnerSettings { get; set; } = null;

        public int NumErrors { get; set; } = 0;

        public string[] AutomationBranches = null;

        public AutomationRunMode AutomationRunMode = AutomationRunMode.Interactive;

        public DatabaseAutomationRunner DatabaseAutomationRunner { get; set; } = null;

        private XmlDocument RootDocument = null;

        private XmlDocument GlobalMacrosDocument = null;

        private WebClient WebClient = null;

        public string WoTClientVersion { get; set; }

        public string WoTModpackOnlineFolderVersion { get; set; }

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

        public string ComponentInternalName { get; } = "AutomationSequencer";

        public CancellationToken CancellationToken { get; set; }

        protected AutomationSequence RunningSequence;

        public AutomationSequencer()
        {
            WebClient = new WebClient();
        }

        /// <summary>
        /// Load the list of branches from github
        /// </summary>
        /// <returns>A task of the asynchronous operation</returns>
        public async Task LoadBranchesListAsync()
        {
            if (AutomationRunnerSettings.UseLocalRunnerDatabase)
                return;

            List<string> branches = await CommonUtils.GetListOfGithubRepoBranchesAsync(BranchesURL);
            AutomationBranches = branches.ToArray();
        }

        public async Task LoadRootDocumentAsync()
        {
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
        }

        public async Task LoadGlobalMacrosAsync()
        {
            if (RootDocument == null)
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
        }

        public async Task<bool> ParseRootDocumentAsync()
        {
            if (RootDocument == null)
                throw new NullReferenceException();

            Logging.AutomationRunner(LogOptions.MethodName, "Checking root document for to build automation sequences", LogLevel.Info);

            XmlNodeList sequencesXml = XmlUtils.GetXmlNodesFromXPath(RootDocument, "//root.xml/AutomationSequence");
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
                Logging.AutomationRunner(LogOptions.MethodName, "Parsing sequence for package {0} (UID {1})", LogLevel.Info, sequencePackageName, sequenceUID);
                AutomationSequences.Add(new AutomationSequence(DatabasePackages, ApplicationMacros, GlobalMacros, AutomationRunnerSettings, DatabaseManager, CancellationToken)
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

        public async Task<SequencerExitCode> RunSequencerAsync(List<AutomationSequence> sequencesToRun)
        {
            if (RootDocument == null)
                throw new NullReferenceException();
            if (DatabaseManager == null)
                throw new NullReferenceException();
            if (sequencesToRun == null)
                throw new NullReferenceException();
            if (sequencesToRun.Count == 0)
                throw new BadMemeException("packagesToRun must have at least one package to run automation on");

            Logging.AutomationRunner("Linking database packages for each sequence");
            if (!LinkPackagesToAutomationSequences(sequencesToRun))
                return SequencerExitCode.NotRun;

            Logging.Info("Downloading xml for each sequence");
            if (!await LoadAutomationSequencesXmlToRunAsync(sequencesToRun))
                return SequencerExitCode.NotRun;

            Logging.Info("Parsing xml for each sequence");
            if (!ParseAutomationSequences(sequencesToRun))
                return SequencerExitCode.NotRun;

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

        public bool ParseAutomationSequences(List<AutomationSequence> sequencesToRun)
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

            foreach (AutomationSequence sequence in sequencesToRun)
            {
                RunningSequence = sequence;
                Logging.Info(Logfiles.AutomationRunner, LogOptions.ClassName, "Preparing macro lists for sequence run: {0}", sequence.ComponentInternalName);

                ResetApplicationMacros(sequence);
                ParseGlobalMacros();
                sequence.ParseSequenceMacros();

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
                SequencerExitCode exitCode = sequence.ExitCode;
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

        public void ResetApplicationMacros(AutomationSequence sequence)
        {
            Logging.Debug(Logfiles.AutomationRunner, LogOptions.ClassName, "Resetting application macros");
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
        }

        public bool ParseGlobalMacros()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.MethodName, "Parsing global macros from xml document to class objects");
            GlobalMacros.Clear();
            XmlElement result = XmlUtils.GetXmlNodeFromXPath(GlobalMacrosDocument, "/GlobalMacros") as XmlElement;
            XElement globalMacrosXmlHolder = XElement.Parse(result.OuterXml);
            PropertyInfo listPropertyInfo = this.GetType().GetProperty(nameof(GlobalMacros));
            try
            {
                CommonUtils.SetListEntries(this, listPropertyInfo, globalMacrosXmlHolder.Elements());
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

        public void DumpApplicationMacros()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.ClassName, "Dumping application parsed macros. Count = {0}", ApplicationMacros.Count);
            foreach (AutomationMacro macro in ApplicationMacros)
            {
                Logging.Info(Logfiles.AutomationRunner, LogOptions.None, "Macro: Name = {0}, Value = {1}", macro.Name, macro.Value);
            }
        }

        public void DumpGlobalMacros()
        {
            Logging.Info(Logfiles.AutomationRunner, LogOptions.ClassName, "Dumping global parsed macros. Count = {0}", GlobalMacros.Count);
            foreach (AutomationMacro macro in GlobalMacros)
            {
                Logging.Info(Logfiles.AutomationRunner, LogOptions.None, "Macro: Name = {0}, Value = {1}", macro.Name, macro.Value);
            }
        }

        public void DumpShellEnvironmentVariables()
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

        public void UpdateDatabasePackageList()
        {
            DatabasePackages = DatabaseUtils.GetFlatList(DatabaseManager.GlobalDependencies, DatabaseManager.Dependencies, DatabaseManager.ParsedCategoryList);
        }

        public void CancelSequence()
        {
            if (RunningSequence != null)
                RunningSequence.CancelTask();
        }

        public void Dispose()
        {
            ApplicationMacros.Clear();
            GlobalMacros.Clear();
            AutomationSequences.Clear();
            ((IDisposable)WebClient).Dispose();
        }
    }
}
