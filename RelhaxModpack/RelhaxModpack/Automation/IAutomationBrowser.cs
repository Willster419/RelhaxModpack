using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RelhaxModpack.Automation
{
    public interface IAutomationBrowser : IDisposable
    {
        event BrowserManagerDelegate DocumentCompleted;

        event BrowserManagerDelegate NavigationCompleted;

        Control Browser { get; }

        int Height { get; set; }

        int Width { get; set; }

        string GetHtmlDocument();

        void Navigate(string url);

        void Cancel();
    }
}
