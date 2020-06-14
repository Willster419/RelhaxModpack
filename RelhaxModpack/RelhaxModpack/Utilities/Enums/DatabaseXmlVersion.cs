namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The enumeration representations of the Xml database saving format
    /// </summary>
    public enum DatabaseXmlVersion
    {
        /// <summary>
        /// The Legacy format. All in one document
        /// </summary>
        Legacy,

        /// <summary>
        /// The 1.1 format. A root file, a file for the global and standard dependencies, and a file for each categories
        /// </summary>
        OnePointOne
    }
}
