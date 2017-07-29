using System.Collections.Generic;

namespace RelhaxModpack
{
    public class LogicalDependnecy
    {
        public string dependencyZipFile { get; set; }
        public string dependencyZipCRC { get; set; }
        public bool enabled { get; set; }
        //the start address of the zip file location. enabled us to use sites that
        //generate random filenames for publicly shared files.
        public string startAddress { get; set; }
        //the end address of the zip file location. enables us to use dropbox (?dl=1)
        public string endAddress { get; set; }
        //later a unique name of the config entry
        public string packageName { get; set; }
        //needed to excatly identify double packageNames and its position
        public int CheckDatabaseListIndex { get; set; }
        public List<DatabaseLogic> DatabasePackageLogic = new List<DatabaseLogic>();
        public LogicalDependnecy() { }
    }
}
