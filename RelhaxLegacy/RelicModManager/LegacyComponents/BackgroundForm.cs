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
            MenuItemAppClose.Text = Translations.GetTranslatedString(MenuItemAppClose.Name);
            MenuItemRestore.Text = Translations.GetTranslatedString(MenuItemRestore.Name);
            MenuItemCheckUpdates.Text = Translations.GetTranslatedString(MenuItemCheckUpdates.Name);
        }

        private void MenuItemRestore_Click(object sender, EventArgs e)
        {
            if (HostWindow != null && HostWindow.WindowState != FormWindowState.Normal)
                HostWindow.WindowState = FormWindowState.Normal;
        }

        private void MenuItemCheckUpdates_Click(object sender, EventArgs e)
        {
            if (HostWindow != null && HostWindow.ins == null)
            {
                using (PleaseWait wait = new PleaseWait())
                {
                    //save the last old database version
                    string oldDatabaseVersion = Settings.DatabaseVersion;
                    HostWindow.Hide();
                    wait.Show();
                    wait.loadingDescBox.Text = Translations.GetTranslatedString("checkForUpdates");
                    Application.DoEvents();
                    HostWindow.CheckmanagerUpdates();
                    wait.Close();
                    HostWindow.Show();
                    //get the new database version and compare. if new, inform the user
                    if(!Settings.DatabaseVersion.Equals(oldDatabaseVersion))
                    {
                        //TODO: translate
                        MessageBox.Show(Translations.GetTranslatedString("newDBApplied"));
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
            MenuItemAppClose.Text = Translations.GetTranslatedString(MenuItemAppClose.Name);
            MenuItemRestore.Text = Translations.GetTranslatedString(MenuItemRestore.Name);
            MenuItemCheckUpdates.Text = Translations.GetTranslatedString(MenuItemCheckUpdates.Name);
        }
    }
}
