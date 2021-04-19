namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The supported instruction types that can be loaded for execution
    /// </summary>
    public enum InstructionsType
    {
        /// <summary>
        /// The Atlas type instructions
        /// </summary>
        /// <seealso cref="Atlases.Atlas"/>
        Atlas,

        /// <summary>
        /// The Unpack and Copy type instructions, previously called XmlUnpack
        /// </summary>
        /// <seealso cref="Xml.XmlUnpack"/>
        UnpackCopy,

        /// <summary>
        /// The Patch type instructions
        /// </summary>
        /// <seealso cref="Patching.Patch"/>
        Patch,

        /// <summary>
        /// The shortcut type instructions
        /// </summary>
        /// <seealso cref="Shortcuts.Shortcut"/>
        Shortcut
    }
}
