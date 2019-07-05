using System.Collections.Generic;
using System.IO;


namespace RelhaxModpack
{
    public enum ApplicationMode
    {
        Default,
        Editor,
        Updater,
        PatchDesigner,
        Patcher
    }
    //TODO: documentation
    public static class CommandLineSettings
    {
        //application command line level settings
        //also serves as place to put default values
        public static bool TestMode = false;
        public static bool SkipUpdate = false;
        public static bool SilentStart = false;
        //use the filename as check for auto install
        public static string AutoInstallFileName = string.Empty;
        //use key filename as check for update key mode
        public static string UpdateKeyFileName = string.Empty;
        public static string EditorAutoLoadFileName = string.Empty;
        public static List<string> PatchFilenames = new List<string>();
        public static ApplicationMode ApplicationMode = ApplicationMode.Default;
        public static void ParseCommandLineConflicts()
        {
            //check for conflicting command line arguments
        }

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
