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
            Logging.WriteToLog("command line: " + string.Join(" ", args));
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
                        Logging.WriteToLog("test, loading in test mode");
                        TestMode = true;
                        break;
                    case "skip-update":
                        Logging.WriteToLog("skip-update, skipping updating");
                        SkipUpdate = true;
                        break;
                    case "silent-start":
                        Logging.WriteToLog("silent-start, loading in background");
                        SilentStart = true;
                        break;
                    case "auto-install":
                        AutoInstallFileName = args[++i];
                        Logging.WriteToLog("auto-install, attempting to parse user configuration file: " + AutoInstallFileName);
                        break;
                    case "updateKeyFile":
                        //get key file
                        UpdateKeyFileName = args[++i];
                        Logging.WriteToLog("updateKeyFile, loading keyfile " + UpdateKeyFileName);
                        break;
                    case "editorAutoLoad":
                        EditorAutoLoadFileName = args[++i];
                        Logging.WriteToLog("editorAutoLoad, loading database from " + EditorAutoLoadFileName);
                        break;
                    //now check for different startup modes
                    case "patch-designer":
                        ApplicationMode = ApplicationMode.PatchDesigner;
                        Logging.WriteToLog("patch-designer, loading in patch design mode");
                        break;
                    case "database-updater":
                        ApplicationMode = ApplicationMode.Updater;
                        Logging.WriteToLog("database-updater, loading in database update mode");
                        break;
                    case "database-editor":
                        ApplicationMode = ApplicationMode.Editor;
                        Logging.WriteToLog("database-editor, loading in database edit mode");
                        break;
                    case "patcher":
                        ApplicationMode = ApplicationMode.Patcher;
                        Logging.Info("patcher, loading in patch mode");
                        PatchFilenames.Add(args[++i].Trim());
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
