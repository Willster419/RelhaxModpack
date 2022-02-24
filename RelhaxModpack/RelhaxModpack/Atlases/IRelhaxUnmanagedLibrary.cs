using TeximpNet.Unmanaged;

namespace RelhaxModpack.Atlases
{
    /// <summary>
    /// Represents an interface that all unmanaged libraries in the Modpack use. Allows for one method of testing.
    /// </summary>
    public interface IRelhaxUnmanagedLibrary
    {
        /// <summary>
        /// Gets the name of the embedded zip file containing the dll, 32 or 64 bit version.
        /// </summary>
        string EmbeddedFilename { get; }

        /// <summary>
        /// Gets the name of the dll file inside the embedded zip file, 32 or 64 bit version.
        /// </summary>
        string ExtractedFilename { get; }

        /// <summary>
        /// Gets the absolute path to the dll file.
        /// </summary>
        string Filepath { get; }

        /// <summary>
        /// Determines if the file is extracted to the Filepath property location.
        /// </summary>
        bool IsExtracted { get; }

        /// <summary>
        /// Determines if the library is loaded into memory.
        /// </summary>
        bool IsLoaded { get; }

        /// <summary>
        /// Attempts to load the library using the Filepath property.
        /// </summary>
        /// <returns>True if the library load was successful.</returns>
        bool Load();

        /// <summary>
        /// Attempts to unload the library.
        /// </summary>
        /// <returns>True if the library was unloaded, false otherwise.</returns>
        bool Unload();

        /// <summary>
        /// Extracts the embedded compressed library to the location in the Filepath property.
        /// </summary>
        void Extract();
    }
}
