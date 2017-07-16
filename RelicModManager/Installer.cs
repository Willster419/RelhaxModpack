using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelhaxModpack
{
    public class Installer
    {
        /*
         * This new installer class will handle all of the actual installation process in a seperate backgroundworker
         * so we can get out of using the MainWindow to install. It will handle all of the downloading, backing up, copying,
         * extracting and patching of the modpack. This way the code is easier to follow, and has one central place to take care of the entire install process.
         * This also enables us to use syncronous thinking when approaching the installation procedures of the modpack.
         * The main window will create an install instance which will take the following parameters:
         * 1. The path to World_of_Tanks
         * 2. The path to the application (Startup Path)
         * 3. The parsed list of Mods/Configs/Dependencies (actually DownloadItems) to download
         * 4. The parsed list of Dependencies to extract
         * 5. The parsed list of (other style) Dependnecies to extract
         * 6. The parsed list of Mods to extract
         * 7. The parsed list of Configs to extract
         * It will then do the above stuff in that order. Then parse the patches.
        */
    }
}
