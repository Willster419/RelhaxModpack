using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RelhaxModpack.Automation
{
    public delegate void BrowserSessionManagerDelegate(object sender, DownloadProgressChangedEventArgs e);

    public interface IAutomationBrowserSession : IDisposable
    {
        event BrowserSessionManagerDelegate DownloadProgress;

        void SetHeader(string name, string value);

        Task DownloadFileAsync(string url, string filepath);

        Task DownloadFileAsync(string url, string filepath, CancellationToken token);

        Task<string> GetRequestStringAsync(string url);

        Task<string> PostRequestStringAsync(string url, string postData);
    }
}
