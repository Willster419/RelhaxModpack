namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The types of UI selections for building the selection tree
    /// </summary>
    public enum SelectionTypes
    {
        /// <summary>
        /// Used as catch-all for any mis-assigned selection types
        /// </summary>
        none,

        /// <summary>
        /// A radio button selection (only one of many), can have children
        /// </summary>
        single1,

        /// <summary>
        /// A combobox selection (only one of many), cannot have children
        /// </summary>
        single_dropdown1,

        /// <summary>
        /// Another combobox selection (only one of many), cannot have children
        /// </summary>
        single_dropdown2,

        /// <summary>
        /// A checkbox selection (many of many), can have children
        /// </summary>
        multi
    }
}
