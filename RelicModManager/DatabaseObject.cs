namespace RelhaxModpack
{
    public interface DatabaseObject
    {
        string name { get; set; }
        //the developer's version of the mod
        string version { get; set; }
        string zipFile { get; set; }
        //the start address of the zip file location. enabled us to use sites that
        //generate random filenames for ly shared files.
        string startAddress { get; set; }
        //the end address of the zip file location. enables us to use dropbox (?dl=1)
        string endAddress { get; set; }
        string crc { get; set; }
        bool enabled { get; set; }
        //the index of where the mod is in the entire list ever
        int index { get; set; }
        //size of the mod zip file
        float size { get; set; }
        string updateComment { get; set; }
        string description { get; set; }
        string devURL { get; set; }
        bool Checked { get; set; }
        bool downloadFlag { get; set; }
    }
}
