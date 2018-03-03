using System.Collections.Generic;

namespace RelhaxModpack.DatabaseComponents
{
    /*
     * A database component is an abstract class for all Components within the database. A DatabaseComponent MUST have the following properties:
     * PackageName (a unique identifier for each component in the database. No two components will have the same PackageName)
     * Version (a method to keep track of the version of the package)
     * Timestamp (used to determine when the package entry was last modified)
     * ZipFile (the zip file to extract [can be "", but property MUST exist])
     * Enabled (a toggle to enable and disable the component)
     * CRC (the MD5Hash checksum of the zipfile)
     * StartAdress (the start address of the url to the zip)
     * EndAddress (the end address of the url to the zip file)
     * URL format: StartAddress + ZipFile + EndAddress
     * DownloadFlag (an internal logic property used to determine at install time if the package needs to be downloaded)
     * ReadyForInstall (a bool to determine if the mod has been downloaded and is ready for installation)
     * AppendExtraction (a bool to determine if the package needs to be installed at the end)
     * DevURL (string to hold the URL link of where you can view the webpage of the mod)
     * ShortCuts (a list containing all shortcuts that could be created based on package data)
     * CheckDatabaseListIndex (used to inentify [if a PackageName confilct occurs] where in the xml file the conflict is)
     */
    public abstract class DatabasePackage
    {
        //these should be fields, which also allows us to give them default values/first time instantiation
        public string PackageName = "";
        public string Version = "";
        public long Timestamp = 0;
        public string ZipFile = "";
        public bool Enabled = false;
        public string CRC = "";
        public string StartAddress = "";
        public string EndAddress = "";
        public bool DownloadFlag = false;
        public bool ReadyForInstall = false;
        public bool AppendExtraction = false;
        public string DevURL = "";
        public List<Shortcut> ShortCuts = new List<Shortcut>();
        public int CheckDatabaseListIndex = 0;
        public abstract override string ToString();
    }
}
