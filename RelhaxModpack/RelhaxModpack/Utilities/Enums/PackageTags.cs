namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// An enumeration to categorize the contents of a package, or its purpose
    /// </summary>
    public enum PackageTags
    {
        /// <summary>
        /// The package contains patching instructions
        /// </summary>
        Patch,

        /// <summary>
        /// The package contains Xml unpacking instructions
        /// </summary>
        XmlUnpack,

        /// <summary>
        /// The package contains python scripts
        /// </summary>
        Script,

        /// <summary>
        /// The package contains pre-built atlas files
        /// </summary>
        Atlas,

        /// <summary>
        /// The package contains atlas creation instructions
        /// </summary>
        AtlasBuilder,

        /// <summary>
        /// The package contains PNG image files
        /// </summary>
        ImagePNG,

        /// <summary>
        /// The package contains DDS image files
        /// </summary>
        ImageDDS,

        /// <summary>
        /// The package contains ActionScript swf files
        /// </summary>
        GuiFlash,

        /// <summary>
        /// The package contains GUI login video files
        /// </summary>
        GuiLoginVideo,

        /// <summary>
        /// The package contains xml configuration files
        /// </summary>
        ConfigurationFileXml,

        /// <summary>
        /// The package contains json configuration files
        /// </summary>
        ConfigurationFileJson,

        /// <summary>
        /// The package contains XVM (xc) configuration files
        /// </summary>
        ConfigurationFileXC,

        /// <summary>
        /// The package contains configuration files of an other format
        /// </summary>
        ConfigurationFileOther,

        /// <summary>
        /// The package contains sound bank (bnk/pck) files
        /// </summary>
        SoundBank,

        /// <summary>
        /// The package is used as a dependency link or a dependency with logical requirements
        /// </summary>
        DependencyLink,

        /// <summary>
        /// The package contains misc files
        /// </summary>
        OtherFiles
    }
}