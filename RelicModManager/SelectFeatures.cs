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
    //DEPRECATED: the window for when the user clicks to install the Relhax Sound Mod
    public partial class SelectFeatures : Form
    {
        public bool canceling;
        public SoundPlayer thePlayah;
        private int ingameScroll;
        private int censoredScroll;
        private int sixthSenseScroll;
        private int guiScroll;
        private string soundURL = "http://willster419.atwebpages.com/Applications/RelHaxModPack/Resources/audio/";

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
            if (ingameScroll == 1) thePlayah.SoundLocation = soundURL + "ammoRack.wav";
            if (ingameScroll == 2) thePlayah.SoundLocation = soundURL + "battleStart.wav";
            if (ingameScroll == 3) thePlayah.SoundLocation = soundURL + "pen.wav";
            if (ingameScroll == 4)
            {
                ingameScroll = 0;
                thePlayah.SoundLocation = soundURL + "playerDied.wav";
            }
            thePlayah.Play();
        }

        private void sampleCensoredIngameVoice_Click(object sender, EventArgs e)
        {
            censoredScroll++;
            if (censoredScroll == 1)
            {
                thePlayah.SoundLocation = soundURL + "ammoRackCensored.wav";
                censoredScroll = 0;
            }
            thePlayah.Play();
        }

        private void sampleGui_Click(object sender, EventArgs e)
        {
            guiScroll++;
            if (guiScroll == 1) thePlayah.SoundLocation = soundURL + "battleCountdown1.wav";
            if (guiScroll == 2) thePlayah.SoundLocation = soundURL + "battleCountdown2.wav";
            if (guiScroll == 3)
            {
                thePlayah.SoundLocation = soundURL + "enemySighted1.wav";
                guiScroll = 0;
            }
            thePlayah.Play();
        }

        private void sampleSixthSense_Click(object sender, EventArgs e)
        {
            sixthSenseScroll++;
            if (sixthSenseScroll == 1) thePlayah.SoundLocation = soundURL + "spotted1.wav";
            if (sixthSenseScroll == 2) thePlayah.SoundLocation = soundURL + "spotted2.wav";
            if (sixthSenseScroll == 3)
            {
                thePlayah.SoundLocation = soundURL + "spotted3.wav";
                sixthSenseScroll = 0;
            }
            thePlayah.Play();
        }

        private void stopPlaying_Click(object sender, EventArgs e)
        {
            thePlayah.Stop();
        }

        private void relhaxBoxCen_CheckedChanged(object sender, EventArgs e)
        {
            if (relhaxBox.Checked) relhaxBoxCen.Checked = false;
        }

        private void relhaxBox_CheckedChanged(object sender, EventArgs e)
        {
            if (relhaxBoxCen.Checked) relhaxBox.Checked = false;
        }
    }
}
