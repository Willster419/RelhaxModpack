using RelhaxModpack.Utilities;
using System;
using System.Net;

namespace RelhaxModpack
{
    /// <summary>
    /// A WebClient that allows the use to set a custom timeout value
    /// </summary>
    public class PatientWebClient : WebClient
    {
        /// <summary>
        /// Get or set the length of time, in milliseconds, until the operation will timeout
        /// </summary>
        public int Timeout { get; set; } = 5 * CommonUtils.TO_SECONDS * CommonUtils.TO_MINUTES;

        /// <summary>
        /// Set the URL to get the request data from
        /// </summary>
        /// <param name="uri">The website URL</param>
        /// <returns>The WebRequest object</returns>
        /// <remarks>Overrides the GetWebRequest() method to expose the WebRequest object. In doing so, you can set a custom timeout.</remarks>
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = Timeout;
            return w;
        }
    }
}
