using RelhaxModpack.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using RelhaxModpack.Settings;
using RelhaxModpack.Common;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// A return enumeration for the result of the AskCloseWoT window
    /// </summary>
    public enum AskCloseWoTResult
    {
        /// <summary>
        /// Try again to check if WoT is open
        /// </summary>
        Retry,

        /// <summary>
        /// Cancel the current installation
        /// </summary>
        CancelInstallation,

        /// <summary>
        /// Attempt to force close the WoT process
        /// </summary>
        ForceClosed
    }
    /// <summary>
    /// Interaction logic for AskCloseWoT.xaml
    /// </summary>
    public partial class AskCloseWoT : RelhaxWindow
    {
        /// <summary>
        /// The return structure for determining the result from closing this window
        /// </summary>
        public AskCloseWoTResult AskCloseWoTResult { get; set; } = AskCloseWoTResult.CancelInstallation;

        /// <summary>
        /// The location of the WoT installation directory parsed at installation time
        /// </summary>
        /// <remarks>The path is absolute, ending at "World_of_Tanks"</remarks>
        public string WoTDirectory { get; set; }

        /// <summary>
        /// Create an instance of the AskCloseWoT window
        /// </summary>
        public AskCloseWoT(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        private void WoTRunningCancelInstallButton_Click(object sender, RoutedEventArgs e)
        {
            AskCloseWoTResult = AskCloseWoTResult.CancelInstallation;
            DialogResult = false;
            Close();
        }

        private void WoTRunningForceCloseButton_Click(object sender, RoutedEventArgs e)
        {
            AskCloseWoTResult = AskCloseWoTResult.Retry;
            Logging.Info("Getting WoT process(es)");
            Process WoTProcess = CommonUtils.GetProcess(ApplicationConstants.WoTProcessName, WoTDirectory);
            if(WoTProcess == null)
            {
                Logging.Error("Failed to get process (null result)");
                MessageBox.Show(Translations.GetTranslatedString("failedCloseProcess"));
                return;
            }
            try
            {
                WoTProcess.Kill();
                System.Threading.Thread.Sleep(100);
                if (WoTProcess.HasExited)
                {
                    Logging.Info("success in ending process!");
                    AskCloseWoTResult = AskCloseWoTResult.ForceClosed;
                    DialogResult = true;
                    Close();
                }
                else
                {
                    Logging.Error("Failed to get process (timeout)");
                    MessageBox.Show(Translations.GetTranslatedString("failedCloseProcess"));
                    return;
                }
            }
            catch (Exception ex)
            {
                Logging.Error("Failed to kill process");
                Logging.Info(ex.ToString());
                MessageBox.Show(Translations.GetTranslatedString("failedCloseProcess"));
                return;
            }
        }

        private void WoTRunningRetryButton_Click(object sender, RoutedEventArgs e)
        {
            AskCloseWoTResult = AskCloseWoTResult.Retry;
            DialogResult = true;
            Close();
        }
    }
}
