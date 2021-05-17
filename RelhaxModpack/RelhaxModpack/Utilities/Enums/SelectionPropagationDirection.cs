namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The UI checked propagation directions
    /// </summary>
    /// <remarks>When a UI package element is checked, it needs to propagate the checked behavior up or down to prevent an invalid selection</remarks>
    public enum SelectionPropagationDirection
    {
        /// <summary>
        /// Up the higher parent levels
        /// </summary>
        PropagateUp,

        /// <summary>
        /// Down to lower child levels
        /// </summary>
        PropagateDown
    }
}
