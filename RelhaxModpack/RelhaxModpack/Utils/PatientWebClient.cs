using System;
using System.Net;

namespace RelhaxModpack
{
    public class PatientWebClient : WebClient
    {
        public int Timeout { get; set; } = 5 * Utils.TO_SECONDS * Utils.TO_MINUETS;
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest w = base.GetWebRequest(uri);
            w.Timeout = Timeout;
            return w;
        }
    }
}
