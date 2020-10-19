namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The different ways that the apply and save database buttons can interact
    /// </summary>
    public enum ApplyBehavior
    {
        /// <summary>
        /// Default behavior. The buttons do not interact.
        /// </summary>
        Default,

        /// <summary>
        /// When you click the apply button, it also saves the database after, to the default save location.
        /// </summary>
        ApplyTriggersSave,

        /// <summary>
        /// When you click the save button, it also clicks the apply button before saving.
        /// </summary>
        SaveTriggersApply
    }
}
