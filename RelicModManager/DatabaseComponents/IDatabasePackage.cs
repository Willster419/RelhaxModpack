using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
     */
    public interface IDatabasePackage
    {
        string packageName { get; set; }
        long timestamp { get; set; }
        string zipFile { get; set; }
        bool enabled { get; set; }
        string crc { get; set; }
        string startAddress { get; set; }
        string endAddress { get; set; }
        bool downloadFlag { get; set; }
    }
}
