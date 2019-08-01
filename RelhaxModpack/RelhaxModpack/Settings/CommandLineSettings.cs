using System.Collections.Generic;
using System.IO;


namespace RelhaxModpack
{
    /// <summary>
    /// The primary functional modes the application can run in
    /// </summary>
    public enum ApplicationMode
    {
        /// <summary>
        /// The default mode of modpack installer. This is the primary focus of the application
        /// </summary>
        Default,

        /// <summary>
        /// The database editor mode
        /// </summary>
        Editor,

        /// <summary>
        /// The updater mode. Used for updating the database, application, and other various functions
        /// </summary>
        Updater,

        /// <summary>
        /// The patch designer mode. Allow the user to create and test patches
        /// </summary>
        PatchDesigner,

        /// <summary>
        /// The patch runner mode. Can be used in command line mode, used for patching files given patch file instructions
        /// </summary>
        Patcher
    }
    
    /// <summary>
    /// Handles all parsing and usage of command line arguments
    /// </summary>
    public static class CommandLineSettings
    {
        //application command line level settings
        //also serves as place to put default values
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
            Logging.Info("command line: " + string.Join(" ", args));
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
                        Logging.Info("test, loading in test mode");
                        TestMode = true;
                        break;
                    case "skip-update":
                        Logging.Info("skip-update, skipping updating");
                        SkipUpdate = true;
                        break;
                    case "silent-start":
                        Logging.Info("silent-start, loading in background");
                        SilentStart = true;
                        break;
                    case "auto-install":
                        AutoInstallFileName = args[++i];
                        Logging.Info("auto-install, attempting to launch installation using user configuration file: " + AutoInstallFileName);
                        break;
                    case "updateKeyFile":
                        //get key file
                        UpdateKeyFileName = args[++i];
                        Logging.Info("updateKeyFile, loading keyfile " + UpdateKeyFileName);
                        break;
                    case "editorAutoLoad":
                        EditorAutoLoadFileName = args[++i];
                        Logging.Info("editorAutoLoad, loading database from " + EditorAutoLoadFileName);
                        break;
                    //now check for different startup modes
                    case "patch-designer":
                        ApplicationMode = ApplicationMode.PatchDesigner;
                        Logging.Info("patch-designer, loading in patch design mode");
                        break;
                    case "database-updater":
                        ApplicationMode = ApplicationMode.Updater;
                        Logging.Info("database-updater, loading in database update mode");
                        break;
                    case "database-editor":
                        ApplicationMode = ApplicationMode.Editor;
                        Logging.Info("database-editor, loading in database edit mode");
                        break;
                    case "patcher":
                        ApplicationMode = ApplicationMode.Patcher;
                        Logging.Info("patcher, loading in patch mode");
                        PatchFilenames.Add(args[++i].Trim());
                        break;
                    case "macro":
                        string macroName = args[++i];
                        string macroValue = args[++i];
                        Logging.Info("parsing macro '{0}' with value '{1}'",macroName,macroValue);
                        /*
                        FilePathDict.Add(@"{versiondir}", Settings.WoTClientVersion);
                        FilePathDict.Add(@"{appdata}", Settings.AppDataFolder);
                        FilePathDict.Add(@"{app}", Settings.WoTDirectory);
                        */
                        if(!macroName[0].Equals('{'))
                        {
                            Logging.Info(@"macro not started with '{', adding");
                            macroName = string.Format("{{{0}}}", macroName);
                        }
                        Utils.FilePathDict.Add(macroName, macroValue);
                        break;
                    default:
                        if (ApplicationMode == ApplicationMode.Patcher)
                        {
                            Logging.Info("adding patch: {0}", commandArg.Trim());
                            PatchFilenames.Add(commandArg.Trim());
                        }
                        break;
                }
            }
        }
    }
}
