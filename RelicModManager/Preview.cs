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
    public partial class Preview : Form
    {
        public string modOrConfigName {get; set;}
        public string description {get; set;}
        public string updateComments {get; set;}
        List<String> pictures = new List<String>();
        private Image loadingImage;
        public string devURL {get; set;}
        private int currentlySelected = 0;
        private string picturesFolder = "https://dl.dropboxusercontent.com/u/44191620/RelicMod/pictures/";
        
        public Preview()
        {
            InitializeComponent();
        }
        
        public Preview(string title, List<String> pictureList, string desc, string update = "", string dev = "")
        {
            InitializeComponent();
            modOrConfigName = title;
            pictures = pictureList;
            updateComments = update;
            description = desc;
            devURL = dev;
            loadingImage = RelicModManager.Properties.Resources.loading;
        }
        
        public void displayPictures(string URL)
        {
            previewPicture.Image = null;
            previewPicture.Image = Settings.getLoadingImage();
            previewPicture.LoadAsync(URL);
            this.Text = modOrConfigName + " - " + currentlySelected;
        }
        
        private void makeLinkedLabel(int i)
        {
            LinkLabel label = new LinkLabel();
            label.Name = "" + i;
            label.Text = "" + i;
            label.LinkClicked += new LinkLabelLinkClickedEventHandler(label_LinkClicked);
            label.Size = new Size(10, 10);
            label.AutoSize = true;
            int xLocation = 14 * i;
            label.Location = new Point(xLocation,5);
            //fullSizeLabel.Size = new System.Drawing.Size(82, 13);
            label.TabStop = true;
            pictureCountPanel.Controls.Add(label);
        }

        //handler for when a link label is clicked in the panel picture selection panel
        void label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel lb = (LinkLabel)sender;
            int i = int.Parse(lb.Text);
            //i--;
            currentlySelected = i;
            this.displayPictures(pictures[i]);
        }

        private void previewPicture_LoadCompleted(object sender, AsyncCompletedEventArgs e)
        {

        }

        private void devLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!devURL.Equals(""))
                System.Diagnostics.Process.Start(devURL);
        }

        private void nextPicButton_Click(object sender, EventArgs e)
        {
            currentlySelected++;
            if (currentlySelected >= pictures.Count)
            {
                currentlySelected--;
                return;
            }
            this.displayPictures(pictures[currentlySelected]);
        }

        private void previousPicButton_Click(object sender, EventArgs e)
        {
            currentlySelected--;
            if (currentlySelected < 0)
            {
                currentlySelected++;
                return;
            }
            this.displayPictures(pictures[currentlySelected]);
        }

        private void Preview_SizeChanged(object sender, EventArgs e)
        {
            //previewPicture, diescriptionbox, nextpicturebutton and updatebox should all have the same size width.
            int width = this.Size.Width - 32;
            int applicationHeight = this.Size.Height;
            //do this from bottom to top
            updateBox.Size = new Size(width, updateBox.Size.Height);
            descriptionBox.Size = new Size(width, descriptionBox.Size.Height);
            previewPicture.Size = new Size(width, applicationHeight - 265);

            updateBox.Location = new Point(12, 12 + previewPicture.Size.Height + 6 + nextPicButton.Size.Height + 6 + descriptionBox.Size.Height + 6);
            descriptionBox.Location = new Point(12, 12 + previewPicture.Size.Height + 6 + nextPicButton.Size.Height + 6);
            nextPicButton.Location = new Point(this.Size.Width - 21 - nextPicButton.Size.Width, 12 + previewPicture.Size.Height + 6);
            previousPicButton.Location = new Point(12, 12 + previewPicture.Size.Height + 6);
            pictureCountPanel.Location = new Point(12 + previousPicButton.Size.Width + 12, 12 + previewPicture.Size.Height + 6);
            pictureCountPanel.Size = new Size(width - pictureCountPanel.Location.X - nextPicButton.Size.Width - 4, pictureCountPanel.Size.Height);
            devLinkLabel.Location = new Point(this.Size.Width - 12 - devLinkLabel.Size.Width, applicationHeight - 49);

            if (this.Size.Height < 700) this.Size = new Size(this.Size.Width, 700);
            if (this.Size.Width < 450) this.Size = new Size(450, this.Size.Height);
        }

        private void Preview_Load(object sender, EventArgs e)
        {
            this.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
            this.Text = modOrConfigName;
            for (int i = 0; i < pictures.Count; i++)
            {
                this.makeLinkedLabel(i);
            }
            previewPicture.WaitOnLoad = false;
            previewPicture.InitialImage = Settings.getLoadingImage();
            //previewPicture.Image = loadingImage;
            if (pictures != null)
            {
                currentlySelected = 0;
                this.displayPictures(pictures[0]);
            }
            descriptionBox.Lines = description.Split('@');
            updateBox.Lines = updateComments.Split('@');
            this.Preview_SizeChanged(null,null);
            this.Size = new Size(450, 700);
        }
    }
}
