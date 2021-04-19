namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The run mode of the automation sequencer
    /// </summary>
    public enum AutomationRunMode
    {
        /// <summary>
        /// Gui run mode. Will not stop on errors
        /// </summary>
        Interactive,

        /// <summary>
        /// Batch run mode. Will not stop on errors inside an automation script
        /// </summary>
        Batch
    }
}
