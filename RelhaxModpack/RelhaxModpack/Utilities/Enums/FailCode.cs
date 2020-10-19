namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// List of possible areas in the Atlas creation process where it could fail
    /// </summary>
    public enum FailCode
    {
        /// <summary>
        /// No error occurred
        /// </summary>
        None = 0,

        /// <summary>
        /// Failed to import the DDS image file to a bitmap object
        /// </summary>
        ImageImporter,

        /// <summary>
        /// Failed to export the bitmap object to a DDS image file
        /// </summary>
        ImageExporter,

        /// <summary>
        /// Failed to load and parse the WG xml atlas map
        /// </summary>
        MapImporter,

        /// <summary>
        /// Failed to parse and save the WG xml atlas map
        /// </summary>
        MapExporter,

        /// <summary>
        /// No images to build for the atlas
        /// </summary>
        NoImages,

        /// <summary>
        /// Duplicate image names in list of images to pack
        /// </summary>
        ImageNameCollision,

        /// <summary>
        /// Failed to pack the images into one large image (most likely they don't fit into the provided dimensions)
        /// </summary>
        FailedToPackImage,

        /// <summary>
        /// Failed to compress an atlas that requires over the 2GB process limit on 32bit systems
        /// </summary>
        OutOfMemory32bit,

        /// <summary>
        /// Failed to create the atlas bitmap object
        /// </summary>
        FailedToCreateBitmapAtlas
    }
}
