using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Media;

namespace RelicModManager
{
    public partial class SelectFeatures : Form
    {
        public bool canceling;
        public SoundPlayer thePlayah;
        private int ingameScroll;
        private int censoredScroll;
        private int sixthSenseScroll;
        private int guiScroll;

        public SelectFeatures()
        {
            InitializeComponent();
        }

        private void SelectFeatures_Load(object sender, EventArgs e)
        {
            //Clear the previous enteries
            canceling = true;
            relhaxBox.Checked = false;
            guiBox.Checked = false;
            sixthSenseBox.Checked = false;
            thePlayah = new SoundPlayer();
            ingameScroll = 0;
            censoredScroll = 0;
            sixthSenseScroll = 0;
            guiScroll = 0;
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void continueButton_Click(object sender, EventArgs e)
        {
            canceling = false;
            this.Close();
        }

        private void sampleIngameVoice_Click(object sender, EventArgs e)
        {
            ingameScroll++;
            if (ingameScroll == 1) thePlayah.Stream = RelicModManager.Properties.Resources.ammoRack;
            if (ingameScroll == 2) thePlayah.Stream = RelicModManager.Properties.Resources.battleStart;
            if (ingameScroll == 3) thePlayah.Stream = RelicModManager.Properties.Resources.pen;
            if (ingameScroll == 4)
            {
                ingameScroll = 0;
                thePlayah.Stream = RelicModManager.Properties.Resources.playerDied;
            }
            thePlayah.Play();
        }

        private void sampleCensoredIngameVoice_Click(object sender, EventArgs e)
        {
            censoredScroll++;
            if (censoredScroll == 1)
            {
                thePlayah.Stream = RelicModManager.Properties.Resources.ammoRackCensored;
                censoredScroll = 0;
            }
            thePlayah.Play();
        }

        private void sampleGui_Click(object sender, EventArgs e)
        {
            guiScroll++;
            if (guiScroll == 1) thePlayah.Stream = RelicModManager.Properties.Resources.battleCountdown1;
            if (guiScroll == 2) thePlayah.Stream = RelicModManager.Properties.Resources.battleCountdown2;
            if (guiScroll == 3)
            {
                thePlayah.Stream = RelicModManager.Properties.Resources.enemySighted1;
                guiScroll = 0;
            }
            thePlayah.Play();
        }

        private void sampleSixthSense_Click(object sender, EventArgs e)
        {
            sixthSenseScroll++;
            if (sixthSenseScroll == 1) thePlayah.Stream = RelicModManager.Properties.Resources.spotted1;
            if (sixthSenseScroll == 2) thePlayah.Stream = RelicModManager.Properties.Resources.spotted2;
            if (sixthSenseScroll == 3)
            {
                thePlayah.Stream = RelicModManager.Properties.Resources.spotted3;
                sixthSenseScroll = 0;
            }
            thePlayah.Play();
        }

        private void stopPlaying_Click(object sender, EventArgs e)
        {
            thePlayah.Stop();
        }
    }
}
