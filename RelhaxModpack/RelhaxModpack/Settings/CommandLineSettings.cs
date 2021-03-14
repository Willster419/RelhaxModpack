using RelhaxModpack.Windows;
using RelhaxModpack.Utilities;
using System.Collections.Generic;
using System.IO;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Linq;

namespace RelhaxModpack.Settings
{
    /// <summary>
    /// Handles all parsing and usage of command line arguments.
    /// </summary>
    public class CommandLineSettings
    {
        /// <summary>
        /// A list of command line arguments that, when specified, would create a custom window to
        /// run the application in a specific mode.
        /// </summary>
        /// <remarks>For example, if "database-editor" is detected, then the application is going to open the database editor</remarks>
        public static readonly string[] CommandLineArgsToSpawnCustomWindow =
        {
            PatchDesigner.CommandLineArg,
            "database-updater", //old version of modpack-toolbox
            ModpackToolbox.CommandLineArg,
            DatabaseEditor.CommandLineArg,
            "patcher",
            DatabaseAutomationRunner.CommandLineArg
        };

        /// <summary>
        /// Using the application is database test mode. Allows you to test a local database
        /// </summary>
        /// <remarks>Activated with "/test"</remarks>
        public bool TestMode = false;

        /// <summary>
        /// Skip the application update check
        /// </summary>
        /// <remarks>Activated with "/skip-update"</remarks>
        public bool SkipUpdate = false;

        /// <summary>
        /// Allows the application (in installer mode) to be launched in a minimized state
        /// </summary>
        /// <remarks>Activated with "/silent-start"</remarks>
        public bool SilentStart = false;

        /// <summary>
        /// The name of the auto install selection file
        /// </summary>
        /// <remarks>The application uses the filename as check for if in auto install mode i.e. if the string is not empty.
        /// The file must be in the "RelhaxUserSelections" folder</remarks>
        public string AutoInstallFileName = string.Empty;

        /// <summary>
        /// The name of the file that contains the key for unlocking the updater
        /// </summary>
        /// <remarks>use key filename as check for update key mode</remarks>
        public string UpdateKeyFileName = string.Empty;

        /// <summary>
        /// The path to load the database from when the application starts
        /// </summary>
        public string EditorAutoLoadFileName = string.Empty;

        /// <summary>
        /// The parsed list of patch instruction files for patch mode
        /// </summary>
        /// <remarks>The application will run the patches in order loaded from the command line i.e. left to right</remarks>
        public List<string> PatchFilenames = new List<string>();

        /// <summary>
        /// The mode that the application is currently running in
        /// </summary>
        public ApplicationMode ApplicationMode = ApplicationMode.Default;

        /// <summary>
        /// Creates an instance of the CommandLineSettings class
        /// </summary>
        /// <param name="args">The list of command line arguments provided from the Environment class</param>
        /// <remarks>The first arg to the exe is skipped</remarks>
        public CommandLineSettings(string[] args)
        {
            this.CommandLineArgs = args;
        }

        /// <summary>
        /// Parse the command line arguments
        /// </summary>
        public void ParseCommandLineSwitches()
        {
            if (CommandLineArgs == null)
                throw new BadMemeException("CommandLineArgs is null");

            Logging.Info(LogOptions.ClassName, "Command line: " + string.Join(" ", CommandLineArgs));
            for (int i = 0; i < CommandLineArgs.Length; i++)
            {
                string commandArg = CommandLineArgs[i];
                char compare = commandArg[0];
                if (compare.Equals('/') || compare.Equals('-'))
                    commandArg = commandArg.Remove(0, 1);
                switch (commandArg)
                {
                    //start with settings for application configurations
                    case "test":
                        Logging.Info(LogOptions.ClassName, "{0}, loading in test mode", commandArg);
                        TestMode = true;
                        break;
                    case "skip-update":
                        Logging.Info(LogOptions.ClassName, "{0}, skipping updating", commandArg);
                        SkipUpdate = true;
                        break;
                    case "silent-start":
                        Logging.Info(LogOptions.ClassName, "{0}, loading in background", commandArg);
                        SilentStart = true;
                        break;
                    case "auto-install":
                        AutoInstallFileName = CommandLineArgs[++i];
                        Logging.Info(LogOptions.ClassName, "{0}, attempting to launch installation using user configuration file {1}", commandArg, AutoInstallFileName);
                        break;
                    case "updateKeyFile":
                        //get key file
                        UpdateKeyFileName = CommandLineArgs[++i];
                        Logging.Info(LogOptions.ClassName, "{0}, loading keyfile {1}", commandArg, UpdateKeyFileName);
                        break;
                    case "editorAutoLoad":
                        EditorAutoLoadFileName = CommandLineArgs[++i];
                        Logging.Info(LogOptions.ClassName, "{0}, loading database from {1}", commandArg, EditorAutoLoadFileName);
                        break;
                    case "updater-hardcode-path":
                        Logging.Warning(LogOptions.ClassName, "{0} is obsolete and will be skipped. See the {1} file for this setting.", commandArg, ModpackToolboxSettings.SettingsFilename);
                        break;
                    case "toolbox-hardcode-path":
                        Logging.Warning(LogOptions.ClassName, "{0} is obsolete and will be skipped. See the {1} file for this setting.", commandArg, ModpackToolboxSettings.SettingsFilename);
                        break;
                    //now check for different startup modes
                    case PatchDesigner.CommandLineArg:
                        ApplicationMode = ApplicationMode.PatchDesigner;
                        Logging.Info(LogOptions.ClassName, "{0}, loading in patch design mode", commandArg);
                        break;
                    case "database-updater":
                    case ModpackToolbox.CommandLineArg:
                        ApplicationMode = ApplicationMode.Updater;
                        Logging.Info(LogOptions.ClassName, "{0}, loading in database update mode", commandArg);
                        break;
                    case DatabaseEditor.CommandLineArg:
                        ApplicationMode = ApplicationMode.Editor;
                        Logging.Info(LogOptions.ClassName, "{0}, loading in database edit mode", commandArg);
                        break;
                    case DatabaseAutomationRunner.CommandLineArg:
                        ApplicationMode = ApplicationMode.AutomationRunner;
                        Logging.Info(LogOptions.ClassName, "{0}, loading in database automation runner mode", commandArg);
                        break;
                    case "patcher":
                        ApplicationMode = ApplicationMode.Patcher;
                        Logging.Info(LogOptions.ClassName, "{0}, loading in patch mode", commandArg);
                        PatchFilenames.Add(CommandLineArgs[++i].Trim());
                        break;
                    //and also check for adding macros
                    case "macro":
                        string macroName = CommandLineArgs[++i];
                        string macroValue = CommandLineArgs[++i];
                        Logging.Info(LogOptions.ClassName, "{0}, parsing macro '{1}' with value '{2}'", commandArg, macroName, macroValue);
                        /*
                        FilePathDict.Add(@"{versiondir}", Settings.WoTClientVersion);
                        FilePathDict.Add(@"{appdata}", Settings.AppDataFolder);
                        FilePathDict.Add(@"{app}", Settings.WoTDirectory);
                        */
                        if(!macroName[0].Equals('{'))
                        {
                            Logging.Info(LogOptions.ClassName, @"macro not started with '{', adding");
                            macroName = string.Format("{{{0}}}", macroName);
                        }
                        MacroUtils.FilePathDict.Add(macroName, macroValue);
                        break;
                    default:
                        if (ApplicationMode == ApplicationMode.Patcher)
                        {
                            Logging.Info(LogOptions.ClassName, "Adding patch: {0}", commandArg.Trim());
                            PatchFilenames.Add(commandArg.Trim());
                        }
                        break;
                }
            }
        }

        private string[] CommandLineArgs = null;

        private bool ArgsLaunchCustomWindowChecked = false;

        private bool ArgsOpenCustomWindow_ = false;

        /// <summary>
        /// Determines if any specified command line args will cause the application to open a custom (non MainWindow) window
        /// </summary>
        /// <returns>True if the provided arguments will return a custom array, false otherwise</returns>
        public bool ArgsOpenCustomWindow()
        {
            if (CommandLineArgs == null)
                throw new BadMemeException("CommandLineArgs is null");

            if (ArgsLaunchCustomWindowChecked)
                return ArgsOpenCustomWindow_;

            foreach (string commandLineArg in CommandLineArgs)
            {
                foreach (string argsToLaunchCustomWindow in CommandLineArgsToSpawnCustomWindow)
                {
                    if (commandLineArg.Contains(argsToLaunchCustomWindow))
                    {
                        ArgsOpenCustomWindow_ = true;
                        break;
                    }
                }

                if (ArgsOpenCustomWindow_)
                    break;
            }

            ArgsLaunchCustomWindowChecked = true;
            return ArgsOpenCustomWindow_;
        }
    }
}
