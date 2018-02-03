using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RelhaxModpack.Forms
{
    public partial class BackgroundForm : RelhaxForum
    {
        /*
         * https://www.developer.com/net/net/article.php/3336751/C-Tip-Placing-Your-C-Application-in-the-System-Tray.htm
         * This acts as a system tray icon holder, context menu, and 
         * Receiver for push notifications from the server about new application versions
         */
        //a refrence for the MainWindow
        public MainWindow HostWindow { get; set; }
        public BackgroundForm()
        {
            InitializeComponent();
        }

        private void BackgroundForm_SizeChanged(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private void RMIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (HostWindow != null && HostWindow.WindowState != FormWindowState.Normal)
                HostWindow.WindowState = FormWindowState.Normal;
        }

        private void RMIcon_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            //bring up the right click menu
        }

        private void MenuItemRestore_Click(object sender, EventArgs e)
        {
            if (HostWindow != null && HostWindow.WindowState != FormWindowState.Normal)
                HostWindow.WindowState = FormWindowState.Normal;
        }

        private void MenuItemCheckUpdates_Click(object sender, EventArgs e)
        {
            if (HostWindow != null)
            {
                using (PleaseWait wait = new PleaseWait())
                {
                    //save the last old database version
                    string oldDatabaseVersion = Settings.DatabaseVersion;
                    HostWindow.Hide();
                    wait.Show();
                    wait.loadingDescBox.Text = Translations.getTranslatedString("checkForUpdates");
                    Application.DoEvents();
                    HostWindow.CheckmanagerUpdates();
                    wait.Close();
                    HostWindow.Show();
                    //get the new database version and compare. if new, inform the user
                    if(!Settings.DatabaseVersion.Equals(oldDatabaseVersion))
                    {
                        //TODO: translate
                        MessageBox.Show(Translations.getTranslatedString("newDBApplied"));
                    }
                }
            }
        }

        private void MenuItemAppClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void BackgroundForm_Load(object sender, EventArgs e)
        {
            //apply translations for menu
            //TODO: get translations
            MenuItemAppClose.Text = Translations.getTranslatedString(MenuItemAppClose.Name);
            MenuItemRestore.Text = Translations.getTranslatedString(MenuItemRestore.Name);
            MenuItemCheckUpdates.Text = Translations.getTranslatedString(MenuItemCheckUpdates.Name);
        }
    }
}
