using System.IO;


namespace RelhaxModpack
{
    //TODO: documentation
    public static class CommandLineSettings
    {
        //application command line level settings
        //also serves as place to put default values
        public static bool TestMode = false;
        public static bool SkipUpdate = false;
        public static bool SilentStart = false;
        public static bool ForceVisible = false;
        public static bool ForceEnabled = false;
        public static bool PatchCheck = false;
        public static bool DatabaseUpdate = false;
        public static bool DatabaseEdit = false;
        //use the filename as check for auto install
        public static string AutoInstallFileName = string.Empty;
        //use key filename as check for update key mode
        public static string UpdateKeyFileName = string.Empty;
        public static string EditorAutoLoadFileName = string.Empty;

        public static void ParseCommandLineConflicts()
        {
            //check for conflicting command line arguements
        }
    }
}
