using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RelhaxModpack.Automation
{
    public class AutomationWebBrowser : WebBrowser, IAutomationBrowserSession, IDisposable
    {
        public event BrowserSessionManagerDelegate DownloadProgress;

        public void SetHeader(string name, string value)
        {
            throw new NotImplementedException();
        }

        public async Task DownloadFileAsync(string url, string filepath)
        {
            await DownloadFileAsync(url, filepath, default);
        }

        public async Task DownloadFileAsync(string url, string filepath, CancellationToken token)
        {
            throw new NotImplementedException("soon tm");
        }

        public async Task<string> GetRequestStringAsync(string url)
        {
            throw new NotImplementedException();
        }

        public async Task<string> PostRequestStringAsync(string url, string postData)
        {
            throw new NotImplementedException();
        }
    }
}
