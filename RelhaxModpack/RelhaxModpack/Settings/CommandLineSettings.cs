using RelhaxModpack.Windows;
using RelhaxModpack.Utilities;
using System.Collections.Generic;
using System.IO;
using RelhaxModpack.Utilities.Enums;
using System;
using System.Linq;

namespace RelhaxModpack
{
    /// <summary>
    /// Handles all parsing and usage of command line arguments
    /// </summary>
    public static class CommandLineSettings
    {
        /// <summary>
        /// A list of command line arguments that, when specified, would create a custom window to
        /// run the application in a specific mode.
        /// </summary>
        /// <remarks>For example, if "database-editor" is detected, then the application is going to open the database editor</remarks>
        public static readonly string[] CommandLineArgsToSpawnCustomWindow =
        {
            "patch-designer",
            "database-updater",
            "modpack-toolbox",
            "database-editor",
            "patcher",
            DatabaseAutomationRunner.DatabaseAutomationRunnerCommandLineArg
        };

        /// <summary>
        /// Using the application is database test mode. Allows you to test a local database
        /// </summary>
        /// <remarks>Activated with "/test"</remarks>
        public static bool TestMode = false;

        /// <summary>
        /// Skip the application update check
        /// </summary>
        /// <remarks>Activated with "/skip-update"</remarks>
        public static bool SkipUpdate = false;

        /// <summary>
        /// Allows the application (in installer mode) to be launched in a minimized state
        /// </summary>
        /// <remarks>Activated with "/silent-start"</remarks>
        public static bool SilentStart = false;

        /// <summary>
        /// The name of the auto install selection file
        /// </summary>
        /// <remarks>The application uses the filename as check for if in auto install mode i.e. if the string is not empty.
        /// The file must be in the "RelhaxUserSelections" folder</remarks>
        public static string AutoInstallFileName = string.Empty;

        /// <summary>
        /// The name of the file that contains the key for unlocking the updater
        /// </summary>
        /// <remarks>use key filename as check for update key mode</remarks>
        public static string UpdateKeyFileName = string.Empty;

        /// <summary>
        /// The path to load the database from when the application starts
        /// </summary>
        public static string EditorAutoLoadFileName = string.Empty;

        /// <summary>
        /// The parsed list of patch instruction files for patch mode
        /// </summary>
        /// <remarks>The application will run the patches in order loaded from the command line i.e. left to right</remarks>
        public static List<string> PatchFilenames = new List<string>();

        /// <summary>
        /// The mode that the application is currently running in
        /// </summary>
        public static ApplicationMode ApplicationMode = ApplicationMode.Default;

        /// <summary>
        /// Parse any conflicting command line arguments
        /// </summary>
        public static void ParseCommandLineConflicts()
        {
            //check for conflicting command line arguments
        }

        /// <summary>
        /// Parse the command line arguments
        /// </summary>
        /// <param name="args">A string array of command line arguments</param>
        public static void ParseCommandLine(string[] args)
        {
            Logging.Info(LogOptions.ClassName, "Command line: " + string.Join(" ", args));
            for (int i = 0; i < args.Length; i++)
            {
                string commandArg = args[i];
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
                        AutoInstallFileName = args[++i];
                        Logging.Info(LogOptions.ClassName, "{0}, attempting to launch installation using user configuration file {1}", commandArg, AutoInstallFileName);
                        break;
                    case "updateKeyFile":
                        //get key file
                        UpdateKeyFileName = args[++i];
                        Logging.Info(LogOptions.ClassName, "{0}, loading keyfile {1}", commandArg, UpdateKeyFileName);
                        break;
                    case "editorAutoLoad":
                        EditorAutoLoadFileName = args[++i];
                        Logging.Info(LogOptions.ClassName, "{0}, loading database from {1}", commandArg, EditorAutoLoadFileName);
                        break;
                    case "updater-hardcode-path":
                        Logging.Info(LogOptions.ClassName, "{0}, forcing folder path as {1}", commandArg, ModpackToolbox.HardCodeRepoPath);
                        ModpackToolbox.UseHardCodePath = true;
                        break;
                    case "toolbox-hardcode-path":
                        Logging.Info(LogOptions.ClassName, "{0}, forcing folder path as {1}", commandArg, ModpackToolbox.HardCodeRepoPath);
                        ModpackToolbox.UseHardCodePath = true;
                        break;
                    //now check for different startup modes
                    case "patch-designer":
                        ApplicationMode = ApplicationMode.PatchDesigner;
                        Logging.Info(LogOptions.ClassName, "{0}, loading in patch design mode", commandArg);
                        break;
                    case "database-updater":
                        ApplicationMode = ApplicationMode.Updater;
                        Logging.Info(LogOptions.ClassName, "{0}, loading in database update mode", commandArg);
                        break;
                    case "modpack-toolbox":
                        ApplicationMode = ApplicationMode.Updater;
                        Logging.Info(LogOptions.ClassName, "{0}, loading in database update mode", commandArg);
                        break;
                    case "database-editor":
                        ApplicationMode = ApplicationMode.Editor;
                        Logging.Info(LogOptions.ClassName, "{0}, loading in database edit mode", commandArg);
                        break;
                    case DatabaseAutomationRunner.DatabaseAutomationRunnerCommandLineArg:
                        ApplicationMode = ApplicationMode.AutomationRunner;
                        Logging.Info(LogOptions.ClassName, "{0}, loading in database automation runner mode", commandArg);
                        break;
                    case "patcher":
                        ApplicationMode = ApplicationMode.Patcher;
                        Logging.Info(LogOptions.ClassName, "{0}, loading in patch mode", commandArg);
                        PatchFilenames.Add(args[++i].Trim());
                        break;
                    //and also check for adding macros
                    case "macro":
                        string macroName = args[++i];
                        string macroValue = args[++i];
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

        private static bool ArgsLaunchCustomWindowChecked = false;

        private static bool ArgsOpenCustomWindow_ = false;

        /// <summary>
        /// Determines if any specified command line args will cause the application to open a custom (non MainWindow) window
        /// </summary>
        public static bool ArgsOpenCustomWindow
        { 
            get
            {
                if (ArgsLaunchCustomWindowChecked)
                    return ArgsOpenCustomWindow_;

                foreach(string commandLineArg in Environment.GetCommandLineArgs().ToList().Skip(1))
                {
                    foreach(string argsToLaunchCustomWindow in CommandLineArgsToSpawnCustomWindow)
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
}
