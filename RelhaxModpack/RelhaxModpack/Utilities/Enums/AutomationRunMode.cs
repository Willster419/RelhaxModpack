namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The run mode of the automation sequencer
    /// </summary>
    public enum AutomationRunMode
    {
        /// <summary>
        /// Debug run mode. Requires user interaction for each task, of each sequence.
        /// </summary>
        Debug,

        /// <summary>
        /// Gui run mode. Requires user interaction for each sequence.
        /// </summary>
        Interactive,

        /// <summary>
        /// Batch run mode. Requires no user interaction.
        /// </summary>
        Batch
    }
}
