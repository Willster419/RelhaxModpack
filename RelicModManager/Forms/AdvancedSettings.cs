using System;
using System.Drawing;
using System.Windows.Forms;

namespace RelhaxModpack.Forms
{
    public partial class AdvancedSettings : RelhaxForum
    {
        private LoadingGifPreview gp;
        public int startX, startY;
        private bool WindowLoading = false;

        public AdvancedSettings()
        {
            InitializeComponent();
        }

        private void AdvancedSettings_Load(object sender, EventArgs e)
        {
            WindowLoading = true;
            Logging.Manager(string.Format("AdvancedSettings: Loading window at location x={0}, y={1}", startX, startY));
            ApplySavedSettings();
            Location = new Point(startX, startY);
            WindowLoading = false;
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
            UseAltUpdateMethodCB.Checked = Settings.UseAlternateUpdateMethod;
            UseBetaApplicationCB.Checked = Settings.BetaApplication;
            UseBetaDatabaseCB.Checked = Settings.BetaDatabase;
            switch (Settings.UninstallMode)
            {
                case (UninstallModes.Default):
                    DefaultUninstallModeRB.Checked = true;
                    break;
                case (UninstallModes.Quick):
                    CleanUninstallModeRB.Checked = true;
                    break;
            }
            switch (Settings.GIF)
            {
                case (LoadingGifs.Standard):
                    {
                        standardImageRB.Checked = true;
                        break;
                    }
                case (LoadingGifs.ThirdGuards):
                    {
                        thirdGuardsLoadingImageRB.Checked = true;
                        break;
                    }
            }
            if (Settings.ExportMode)
                forceManuel.Enabled = false;
        }

        #region Loading animations handlers
        private void ImageRB_CheckedChanged(object sender, EventArgs e)
        {
            if (standardImageRB.Checked)
            {
                Settings.GIF = LoadingGifs.Standard;
            }
            else if (thirdGuardsLoadingImageRB.Checked)
            {
                Settings.GIF = LoadingGifs.ThirdGuards;
            }
        }

        private void ImageRB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            RadioButton rb = (RadioButton)sender;
            LoadingGifs backup = Settings.GIF;
            if (rb.Name.Equals("standardImageRB"))
            {
                Settings.GIF = LoadingGifs.Standard;
            }
            else if (rb.Name.Equals("thirdGuardsLoadingImageRB"))
            {
                Settings.GIF = LoadingGifs.ThirdGuards;
            }
            else
                return;
            //create the preview
            using (gp = new LoadingGifPreview(Location.X, Location.Y + Size.Height + 5))
            {
                gp.SetLoadingImage();
                gp.ShowDialog();
            }
        }
        #endregion

        #region CheckedChanged Handlers
        //handler for when the "force manuel" checkbox is checked
        private void ForceManuel_CheckedChanged(object sender, EventArgs e)
        {
            Settings.ForceManuel = forceManuel.Checked;
        }

        private void ClearCacheCB_CheckedChanged(object sender, EventArgs e)
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
            if (DefaultUninstallModeRB.Checked)
                Settings.UninstallMode = UninstallModes.Default;
        }
        //handler for what happends when the check box "clean install" is checked or not
        private void CleanInstallCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.CleanInstallation = cleanInstallCB.Checked;
        }

        private void CleanUninstallModeRB_CheckedChanged(object sender, EventArgs e)
        {
            if (CleanUninstallModeRB.Checked)
                Settings.UninstallMode = UninstallModes.Quick;
        }

        private void ExportModeCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.ExportMode = ExportModeCB.Checked;
            forceManuel.Enabled = !ExportModeCB.Checked;
        }

        private void UseAltUpdateMethodCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.UseAlternateUpdateMethod = UseAltUpdateMethodCB.Checked;
        }

        private void UseBetaDatabaseCB_CheckedChanged(object sender, EventArgs e)
        {
            Settings.BetaDatabase = UseBetaDatabaseCB.Checked;
            if (Settings.BetaDatabase && !WindowLoading)
            {
                MessageBox.Show(Translations.GetTranslatedString("noChangeUntilRestart"));
            }
        }

        private void UseBetaApplication_CheckedChanged(object sender, EventArgs e)
        {
            Settings.BetaApplication = UseBetaApplicationCB.Checked;
            if (Settings.BetaApplication && !WindowLoading)
            {
                MessageBox.Show(Translations.GetTranslatedString("noChangeUntilRestart"));
            }
        }
        #endregion

        private void Generic_MouseEnter(object sender, EventArgs e)
        {
            Control c = (Control)sender;
            AdvancedSettingsDescription.Text = Translations.GetTranslatedString(c.Name + "Description");
        }

        private void AdvancedSettingsHeader_Paint(object sender, PaintEventArgs e)
        {
            Color backColor = SystemColors.ControlDark;       // default values for "normal" UI (not dark)
            Color textColor = Color.White;                          // default values for "normal" UI (not dark)
            RectangleF rec = e.ClipRectangle;
            rec.X = rec.X + ((Label)sender).Margin.Left + ((Label)sender).Padding.Left;
            rec.Y = rec.Y + ((Label)sender).Margin.Top + ((Label)sender).Padding.Top;
            rec.Width = rec.Width - ((Label)sender).Margin.Left - ((Label)sender).Margin.Right - (((Label)sender).Padding.Left - ((Label)sender).Padding.Right);
            rec.Height = rec.Height - ((Label)sender).Margin.Top - ((Label)sender).Margin.Bottom - (((Label)sender).Padding.Top - ((Label)sender).Padding.Bottom);
            if (Settings.DarkUI)
            {
                backColor = SystemColors.Control;
                textColor = SystemColors.ControlText;
            }
            using (SolidBrush brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, rec);
            }
            // https://www.neowin.net/forum/topic/1060552-c-tip-converting-contentalignment-to-stringalignment/
            StringFormat sf = StringFormat.GenericDefault;
            Int32 lNum = (Int32)Math.Log((Double)((Label)sender).TextAlign, 2);
            sf.LineAlignment = (StringAlignment)(lNum / 4);
            sf.Alignment = (StringAlignment)(lNum % 4);
            using (SolidBrush brush = new SolidBrush(textColor))
            {
                e.Graphics.DrawString(((Label)sender).Text, ((Label)sender).Font, brush, rec, sf);
            }
        }

        private void Generic_MouseLeave(object sender, EventArgs e)
        {
            AdvancedSettingsDescription.Text = "";
        }
    }
}
