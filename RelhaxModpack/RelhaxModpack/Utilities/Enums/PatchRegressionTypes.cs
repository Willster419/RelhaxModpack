namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The types of patch regression tests that can be performed
    /// </summary>
    public enum PatchRegressionTypes
    {
        /// <summary>
        /// Json regression (standard, non-XVM)
        /// </summary>
        json,

        /// <summary>
        /// Xml regression
        /// </summary>
        xml,

        /// <summary>
        /// Regex regression
        /// </summary>
        regex,

        /// <summary>
        /// Json regression (XVM style)
        /// </summary>
        followPath
    }
}
