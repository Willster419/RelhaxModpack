namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The types of text macro replacements
    /// </summary>
    public enum ReplacementTypes
    {
        /// <summary>
        /// Replacing macros with file paths
        /// </summary>
        FilePath,

        /// <summary>
        /// Replacing patch arguments of the patch object
        /// </summary>
        PatchArguementsReplace,

        /// <summary>
        /// Replacing modpack created macros (like [quote]) with the corresponding characters
        /// </summary>
        PatchFiles,

        /// <summary>
        /// Replacing literal interpretations of special characters like newline and tab with escaped versions
        /// </summary>
        TextEscape,

        /// <summary>
        /// Replacing escaped versions of special characters like newline and tab with the literal interpretations
        /// </summary>
        TextUnescape,

        /// <summary>
        /// Replacing zip path macros with absolute extraction paths
        /// </summary>
        ZipFilePath
    }
}
