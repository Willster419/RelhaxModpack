namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// The enumeration representations of the level of where the macro is defined. Implies protection level of the macro
    /// </summary>
    public enum MacroType
    {
        /// <summary>
        /// Macro is hard-code defined inside the application. It cannot be overridden
        /// </summary>
        ApplicationDefined,

        /// <summary>
        /// Macro is defined in the global macros definition file. Can be overridden with a local value.
        /// </summary>
        Global,

        /// <summary>
        /// Macro is defined in the local automation sequence.
        /// </summary>
        Local
    }
}
