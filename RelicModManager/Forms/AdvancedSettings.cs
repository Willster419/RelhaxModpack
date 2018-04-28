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
    public partial class AdvancedSettings : RelhaxForum
    {
        private LoadingGifPreview gp;

        public AdvancedSettings()
        {
            InitializeComponent();
        }

        private void AdvancedSettings_Load(object sender, EventArgs e)
        {
            ApplySavedSettings();
        }

        //apply saved settings
        private void ApplySavedSettings()
        {
            cleanInstallCB.Checked = Settings.CleanInstallation;
            ShowInstallCompleteWindowCB.Checked = Settings.ShowInstallCompleteWindow;
            forceManuel.Checked = Settings.ForceManuel;
            clearCacheCB.Checked = Settings.ClearCache;
            createShortcutsCB.Checked = Settings.CreateShortcuts;
            InstantExtractionCB.Checked = Settings.InstantExtraction;
            ExportModeCB.Checked = Settings.ExportMode;
            switch (Settings.UninstallMode)
            {
                case (Settings.UninstallModes.Smart):
                    SmartUninstallModeRB.Checked = true;
                    break;
                case (Settings.UninstallModes.Quick):
                    CleanUninstallModeRB.Checked = true;
                    break;
            }
            switch (Settings.GIF)
            {
                case (Settings.LoadingGifs.Standard):
                    {
                        standardImageRB.Checked = true;
                        break;
                    }
                case (Settings.LoadingGifs.ThirdGuards):
                    {
                        thirdGuardsLoadingImageRB.Checked = true;
                        break;
                    }
            }
            
            if (Settings.ExportMode)
                forceManuel.Enabled = false;
        }
        //toggle UI buttons to be Enabled or disabled
        public void ToggleUIButtons(bool enableToggle)
        {
            if (ExportModeCB.Checked)
                forceManuel.Enabled = false;
            else
                forceManuel.Enabled = enableToggle;
            clearCacheCB.Enabled = enableToggle;
            createShortcutsCB.Enabled = enableToggle;
            InstantExtractionCB.Enabled = enableToggle;
            SmartUninstallModeRB.Enabled = enableToggle;
            CleanUninstallModeRB.Enabled = enableToggle;
            cleanInstallCB.Enabled = enableToggle;
            ShowInstallCompleteWindowCB.Enabled = enableToggle;
            ExportModeCB.Enabled = enableToggle;
        }

        #region Loading animations handlers
        //handler for when the "standard" loading animation is clicked
        private void standardImageRB_CheckedChanged(object sender, EventArgs e)
        {
            if (standardImageRB.Checked)
            {
                Settings.GIF = Settings.LoadingGifs.Standard;
            }
            else if (thirdGuardsLoadingImageRB.Checked)
            {
                Settings.GIF = Settings.LoadingGifs.ThirdGuards;
            }
        }

        private void standardImageRB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != System.Windows.Forms.MouseButtons.Right)
                return;
            RadioButton rb = (RadioButton)sender;
            Settings.LoadingGifs backup = Settings.GIF;
            if (rb.Name.Equals("standardImageRB"))
            {
                Settings.GIF = Settings.LoadingGifs.Standard;
            }
            else if (rb.Name.Equals("thirdGuardsLoadingImageRB"))
            {
                Settings.GIF = Settings.LoadingGifs.ThirdGuards;
            }
            else
                return;
            //create the preview
            using (gp = new LoadingGifPreview(Location.X + Size.Width + 5, Location.Y))
            {
                gp.Hide();
                gp.SetLoadingImage();
                gp.ShowDialog();
            }
        }
        #endregion

        #region CheckedChanged Handlers
        //handler for when the "force manuel" checkbox is checked
        private void forceManuel_CheckedChanged(object sender, EventArgs e)
        {
            Settings.ForceManuel = forceManuel.Checked;
        }

        private void clearCacheCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.ClearCache = clearCacheCB.Checked;
        }
        private void ShowInstallCompleteWindow_CheckedChanged(object sender, EventArgs e)
        {
            Settings.ShowInstallCompleteWindow = ShowInstallCompleteWindowCB.Checked;
        }

        private void CreateShortcutsCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.CreateShortcuts = createShortcutsCB.Checked;
        }

        private void InstantExtractionCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.InstantExtraction = InstantExtractionCB.Checked;
        }

        private void SmartUninstallModeRB_CheckedChanged(object sender, EventArgs e)
        {
            if (SmartUninstallModeRB.Checked)
                Settings.UninstallMode = Settings.UninstallModes.Smart;
        }
        //handler for what happends when the check box "clean install" is checked or not
        private void cleanInstallCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.CleanInstallation = cleanInstallCB.Checked;
        }
        private void CleanUninstallModeRB_CheckedChanged(object sender, EventArgs e)
        {
            if (CleanUninstallModeRB.Checked)
                Settings.UninstallMode = Settings.UninstallModes.Quick;
        }

        private void ExportModeCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.ExportMode = ExportModeCB.Checked;
            forceManuel.Enabled = !ExportModeCB.Checked;
        }
        #endregion

        private void Generic_MouseEnter(object sender, EventArgs e)
        {
            Control c = (Control)sender;
            AdvancedSettingsDescription.Text = Translations.getTranslatedString(c.Name + "Description");
        }

        private void Generic_MouseLeave(object sender, EventArgs e)
        {
            AdvancedSettingsDescription.Text = "";
        }
    }
}
