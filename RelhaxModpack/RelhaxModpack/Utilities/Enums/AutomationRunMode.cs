namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The run mode of the automation sequencer
    /// </summary>
    public enum AutomationRunMode
    {
        /// <summary>
        /// Debug run mode.Requires the user to manually step through each task.
        /// </summary>
        Debug,

        /// <summary>
        /// Gui run mode. Similar to batch, but will stop on a failed sequence.
        /// </summary>
        Sequence,

        /// <summary>
        /// Batch run mode. Requires no user interaction. The default run mode
        /// </summary>
        Batch
    }
}
