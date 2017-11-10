namespace RelhaxModpack.DatabaseComponents
{
    /*
     * A database component is an interface for all Components within the database. A DatabaseComponent MUST have the following properties:
     * PackageName (a unique identifier for each component in the database. No two components will have the same PackageName
     * Version (a method to keep track of the version of the package)
     * Timestamp (used to determine when the package entry was last modified)
     * ZipFile (the zip file to extract [can be "", but it MUST exist])
     * Enabled (a toggle to enable and disable the component)
     * CRC (the MD5Hash checksum of the zipfile)
     * StartAdress (the start address of the url)
     * EndAddress (the end address of the url)
     * Size (the size of the file)
     * DownloadFlag (an internal logic property used to determine at install time if the package needs to be downloaded)
     * ReadyForInstall (a bool to determine if the mod has been downloaded and is ready for installation)
     * ExtractPath (saves the extraction path for where the zip file should be extracted to)
     */
    public interface IDatabasePackage
    {
        string PackageName { get; set; }
        long Timestamp { get; set; }
        string ZipFile { get; set; }
        bool Enabled { get; set; }
        string CRC { get; set; }
        string StartAddress { get; set; }
        string EndAddress { get; set; }
        bool DownloadFlag { get; set; }
        bool ReadyForInstall { get; set; }
        string ExtractPath { get; set; }
    }
}
