namespace RelhaxModpack.Utilities.Enums
{
    /// <summary>
    /// Registry key values used for specifying the emulated version of Internet Explorer to use
    /// </summary>
    /// <remarks>See https://stackoverflow.com/a/25650219/3128017
    /// and https://docs.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/general-info/ee330730(v=vs.85)?redirectedfrom=MSDN#browser_emulation
    /// for more information</remarks>
    public enum IERegistryVersion
    {
        /// <summary>
        /// Internet Explorer 11. Webpages are displayed in IE11 edge mode, regardless of the !DOCTYPE directive.
        /// </summary>
        IE11Forced = 11001,

        /// <summary>
        /// Internet Explorer 11. Webpages containing standards-based !DOCTYPE directives are displayed in IE11 edge mode. Default value for IE11.
        /// </summary>
        IE11Default = 11000,

        /// <summary>
        /// Internet Explorer 10. Webpages are displayed in IE10 Standards mode, regardless of the !DOCTYPE directive.
        /// </summary>
        IE10Forced = 10001,

        /// <summary>
        /// Internet Explorer 10. Webpages containing standards-based !DOCTYPE directives are displayed in IE10 Standards mode. Default value for Internet Explorer 10.
        /// </summary>
        IE10Default = 10000,

        /// <summary>
        /// Internet Explorer 9. Webpages are displayed in IE9 Standards mode, regardless of the !DOCTYPE directive.
        /// </summary>
        IE9Forced = 9999,

        /// <summary>
        /// Internet Explorer 9. Webpages containing standards-based !DOCTYPE directives are displayed in IE9 mode. Default value for Internet Explorer 9.
        /// </summary>
        IE9Default = 9000,

        /// <summary>
        /// Internet Explorer 8. Webpages are displayed in IE8 Standards mode, regardless of the !DOCTYPE directive.
        /// </summary>
        IE8Forced = 8888,

        /// <summary>
        /// Internet Explorer 8. Webpages containing standards-based !DOCTYPE directives are displayed in IE8 mode. Default value for Internet Explorer 8.
        /// </summary>
        IE8Default = 8000,

        /// <summary>
        /// Internet Explorer 7. Webpages containing standards-based !DOCTYPE directives are displayed in IE7 Standards mode. Default value for applications hosting the WebBrowser Control.
        /// </summary>
        IE7Default = 7000
    }
}