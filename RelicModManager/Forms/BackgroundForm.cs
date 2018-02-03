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
        //a refrence for the mainWindow
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
                    HostWindow.Hide();
                    wait.Show();
                    wait.loadingDescBox.Text = Translations.getTranslatedString("checkForUpdates");
                    Application.DoEvents();
                    HostWindow.CheckmanagerUpdates();
                    wait.Close();
                    HostWindow.Show();
                }
            }
        }

        private void MenuItemAppClose_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
