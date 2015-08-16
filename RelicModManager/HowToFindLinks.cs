using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RelicModManager
{
    public partial class HowToFindLinks : Form
    {

        private int stepNumber;
        private Image step1 = RelicModManager.Properties.Resources.step1_new;
        private Image step2 = RelicModManager.Properties.Resources.step2;
        private Image step3 = RelicModManager.Properties.Resources.step3;
        private Image step4 = RelicModManager.Properties.Resources.step4;
        private Image step5 = RelicModManager.Properties.Resources.step5_new;
        private Point backButtonPoint = new Point();
        private Point nextButtonPoint = new Point();
        private Point gotoButtonPoint = new Point();

        public HowToFindLinks()
        {
            InitializeComponent();
            howToFindTheLink.InitialImage = null;
            stepNumber = 1;
        }

        private void backButton_Click(object sender, EventArgs e)
        {
            stepNumber--;
            if (stepNumber == 0) stepNumber = 1;
            if (stepNumber == 1) this.showStep1();
            if (stepNumber == 2) this.showStep2();
            if (stepNumber == 3) this.showStep3();
            if (stepNumber == 4) this.showStep4();
            if (stepNumber == 5) this.showStep5();
            if (stepNumber == 6) stepNumber = 5;
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            stepNumber++;
            if (stepNumber == 0) stepNumber = 1;
            if (stepNumber == 1) this.showStep1();
            if (stepNumber == 2) this.showStep5();
            if (stepNumber == 3) this.Close();
            /*if (stepNumber == 2) this.showStep2();
            if (stepNumber == 3) this.showStep3();
            if (stepNumber == 4) this.showStep4();
            if (stepNumber == 5) this.showStep5();
            if (stepNumber == 6) this.Close();*/
        }

        private void showStep1()
        {
            this.gotoFormPost.Enabled = true;
            this.gotoFormPost.Show();
            this.instructions.Text = "Right click copy the link for the desired zip file";
            this.howToFindTheLink.Image = step1;
            this.howToFindTheLink.Height = step1.Height;
            this.Height = step1.Height + 100;
            this.howToFindTheLink.Width = step1.Width;
            this.Width = step1.Width + 30;
            this.backButtonPoint.X = 12;
            this.backButtonPoint.Y = step1.Height + 40;
            this.nextButtonPoint.X = step1.Width - this.nextButton.Width + 12;
            this.nextButtonPoint.Y = step1.Height + 40;
            this.backButton.Location = backButtonPoint;
            this.nextButton.Location = nextButtonPoint;
            this.gotoButtonPoint.X = instructions.Width + 15;
            this.gotoButtonPoint.Y = this.gotoFormPost.Location.Y;
            this.gotoFormPost.Location = gotoButtonPoint;
        }

        private void showStep2()
        {
            this.gotoFormPost.Enabled = false;
            this.gotoFormPost.Hide();
            this.instructions.Text = "Click the download button";
            this.howToFindTheLink.Image = step2;
            this.howToFindTheLink.Height = step2.Height;
            this.Height = step2.Height + 100;
            this.howToFindTheLink.Width = step2.Width;
            this.Width = step2.Width + 30;
            this.backButtonPoint.X = 12;
            this.backButtonPoint.Y = step2.Height + 40;
            this.nextButtonPoint.X = step2.Width - this.nextButton.Width + 12;
            this.nextButtonPoint.Y = step2.Height + 40;
            this.backButton.Location = backButtonPoint;
            this.nextButton.Location = nextButtonPoint;
        }

        private void showStep3()
        {
            this.gotoFormPost.Enabled = false;
            this.gotoFormPost.Hide();
            this.instructions.Text = "Click cancel when it asks you where to save the file. However, keep the tap open. You need that link.";
            this.howToFindTheLink.Image = step3;
            this.howToFindTheLink.Height = step3.Height;
            this.Height = step3.Height + 100;
            this.howToFindTheLink.Width = step3.Width;
            this.Width = step3.Width + 30;
            this.backButtonPoint.X = 12;
            this.backButtonPoint.Y = step3.Height + 40;
            this.nextButtonPoint.X = step3.Width - this.nextButton.Width + 12;
            this.nextButtonPoint.Y = step3.Height + 40;
            this.backButton.Location = backButtonPoint;
            this.nextButton.Location = nextButtonPoint;
        }

        private void showStep4()
        {
            this.gotoFormPost.Enabled = false;
            this.gotoFormPost.Hide();
            this.instructions.Text = "Heighlight all of the link in the address bar and copy (ctrl+c)";
            this.howToFindTheLink.Image = step4;
            this.howToFindTheLink.Height = step4.Height;
            this.Height = step4.Height + 100;
            this.howToFindTheLink.Width = step4.Width;
            this.Width = step4.Width + 30;
            this.backButtonPoint.X = 12;
            this.backButtonPoint.Y = step4.Height + 40;
            this.nextButtonPoint.X = step4.Width - this.nextButton.Width + 12;
            this.nextButtonPoint.Y = step4.Height + 40;
            this.backButton.Location = backButtonPoint;
            this.nextButton.Location = nextButtonPoint;
        }

        private void showStep5()
        {
            this.gotoFormPost.Enabled = false;
            this.gotoFormPost.Hide();
            this.instructions.Text = "Paste it into the zip download bar in the form (ctrl+v)";
            this.howToFindTheLink.Image = step5;
            this.howToFindTheLink.Height = step5.Height;
            this.Height = step5.Height + 100;
            this.howToFindTheLink.Width = step5.Width;
            this.Width = step5.Width + 30;
            this.backButtonPoint.X = 12;
            this.backButtonPoint.Y = step5.Height + 40;
            this.nextButtonPoint.X = step5.Width - this.nextButton.Width + 12;
            this.nextButtonPoint.Y = step5.Height + 40;
            this.backButton.Location = backButtonPoint;
            this.nextButton.Location = nextButtonPoint;
            this.nextButton.Text = "done";
        }

        private void gotoFormPost_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("http://relicgaming.com/index.php?topic=165");
        }

        private void HowToFindLinks_Shown(object sender, EventArgs e)
        {
            stepNumber = 1;
            this.nextButton.Text = "next";
            this.showStep1();
        }
    }
}
