using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RelhaxModpack.Automation
{
    public class AutomationWebBrowser : WebBrowser, IAutomationBrowserSession, IAutomationBrowser, IDisposable
    {
        public event BrowserSessionManagerDelegate DownloadProgress;

        public event BrowserManagerDelegate DocumentCompleted;

        public event BrowserManagerDelegate NavigationCompleted;

        public Control Browser { get { return this; } }

        public AutomationWebBrowser() : base()
        {
            this.ScriptErrorsSuppressed = true;
        }

        public string GetHtmlDocument()
        {
            return this.Document.Body.OuterHtml;
        }

        public void Cancel()
        {
            this.Stop();
        }

        public void SetHeader(string name, string value)
        {
            throw new NotImplementedException();
        }

        public void RemoveHeader(string name)
        {
            throw new NotImplementedException();
        }

        protected override void OnNavigated(WebBrowserNavigatedEventArgs e)
        {
            base.OnNavigated(e);
            NavigationCompleted?.Invoke(this, e);
        }

        protected override void OnDocumentCompleted(WebBrowserDocumentCompletedEventArgs e)
        {
            base.OnDocumentCompleted(e);
            DocumentCompleted?.Invoke(this, e);
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

        public async Task<string> PostRequestStringAsync(string url, string postData, string contentType)
        {
            throw new NotImplementedException();
        }
    }
}
