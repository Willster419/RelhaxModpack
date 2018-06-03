using System;
using System.Drawing;
using System.Windows.Forms;

namespace RelhaxModpack
{
    public class RelhaxForum : Form
    {
        //option to apply form translations by itteration over controls
        public bool ApplyControlTranslationsOnLoad = false;

        protected override void OnLoad(EventArgs e)
        {
            SuspendLayout();
            base.OnLoad(e);
            if (Settings.AppScalingMode == AutoScaleMode.Dpi)
            {
                AutoScaleDimensions = new SizeF(96F, 96F);//for design in 96 DPI
                AutoScaleMode = Settings.AppScalingMode;
                Scale(new SizeF(Settings.ScaleSize, Settings.ScaleSize));
            }
            else
            {
                AutoScaleMode = Settings.AppScalingMode;
            }
            Font = Settings.AppFont;
            //set the UI colors
            Settings.setUIColor(this);
            ResumeLayout(false);
            if (ApplyControlTranslationsOnLoad)
                ApplyControlTranslations();
            OnPostLoad();
        }

        public virtual void OnPostLoad()
        {
            //stub, to be overridden
            //so that any code that should run after UI scaling can be done
        }

        private void ApplyControlTranslations()
        {
            foreach (Control c in Controls)
            {
                //only apply for common controls
                if (c is RadioButton || c is CheckBox || c is GroupBox || c is Label || c is LinkLabel || c is Button)
                    c.Text = Translations.GetTranslatedString(c.Name);
                if (c is Panel || c is GroupBox || c is TableLayoutPanel)
                    ApplyControlTranslations(c.Controls);
            }
            Text = Translations.GetTranslatedString(Name);
        }

        private void ApplyControlTranslations(Control.ControlCollection conts)
        {
            foreach (Control c in conts)
            {
                //only apply for common controls
                if (c is RadioButton || c is CheckBox || c is GroupBox || c is Label || c is LinkLabel || c is Button)
                    c.Text = Translations.GetTranslatedString(c.Name);

                if (c is Panel || c is GroupBox || c is TableLayoutPanel)
                    ApplyControlTranslations(c.Controls);
            }
        }
    }
}
