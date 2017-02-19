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
    //the preview window to preview a mod. will show the mod name,
    //ro config name, pictures, description, and any update comments
    public partial class Preview : Form
    {
        public string modOrConfigName { get; set; }
        public string description { get; set; }
        public string updateComments { get; set; }
        List<Picture> pictures = new List<Picture>();
        private Image loadingImage;
        public string devURL { get; set; }
        private int currentlySelected = 0;
        //Preview constructor that sets all the required values
        public Preview(string title, List<Picture> pictureList, string desc, string update = "", string dev = "")
        {
            InitializeComponent();
            modOrConfigName = title;
            pictures = pictureList;
            updateComments = update;
            description = desc;
            devURL = dev;
            loadingImage = RelicModManager.Properties.Resources.loading;
        }
        //sets the window title to reflect the new picture, and
        //begine the async process of loading the new picture
        public void displayPictures(string name, string URL)
        {
            previewPicture.Image = null;
            if (pictures.Count == 0)
                return;
            if (URL.Equals(""))
                return;
            previewPicture.Image = Settings.getLoadingImage();
            previewPicture.LoadAsync(URL);
            this.Text = name + " - " + currentlySelected;
            Settings.appendToLog("Preview: started loading of picture '" + name + "' at URL '" + URL + "'");
        }
        //make the linked labels for each picture in the picturesList
        //so a user can navagate easily through the pictures
        private void makeLinkedLabel(int i)
        {
            LinkLabel label = new LinkLabel();
            label.Name = "" + i;
            label.Text = "" + i;
            label.LinkClicked += new LinkLabelLinkClickedEventHandler(label_LinkClicked);
            label.Size = new Size(10, 10);
            label.AutoSize = true;
            int xLocation = 14 * i;
            label.Location = new Point(xLocation, 5);
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
            this.displayPictures(pictures[i].name, pictures[i].URL);
        }
        //show the suplied dev url thread
        private void devLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!devURL.Equals(""))
                System.Diagnostics.Process.Start(devURL);
        }
        //load the next picture in the list
        private void nextPicButton_Click(object sender, EventArgs e)
        {
            currentlySelected++;
            if (currentlySelected >= pictures.Count)
            {
                currentlySelected--;
                return;
            }
            this.displayPictures(pictures[currentlySelected].name, pictures[currentlySelected].URL);
        }
        //load the previous picture in the list
        private void previousPicButton_Click(object sender, EventArgs e)
        {
            currentlySelected--;
            if (currentlySelected < 0)
            {
                currentlySelected++;
                return;
            }
            this.displayPictures(pictures[currentlySelected].name, pictures[currentlySelected].URL);
        }
        //handler for if the user changes the size of the window
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
        //handler that triggeres right before the window is shown
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
            if (pictures != null)
            {
                currentlySelected = 0;
                if (pictures.Count > 0)
                    this.displayPictures(pictures[currentlySelected].name, pictures[currentlySelected].URL);
            }
            if (description == null)
                description = "No Description Provided";
            else if (description.Equals(""))
                description = "No Description Proveded";
            if (updateComments == null)
                updateComments = "No Update Info Proviced";
            else if (updateComments.Equals(""))
                updateComments = "No Update Info Proviced";
            descriptionBox.Lines = description.Split('@');
            updateBox.Lines = updateComments.Split('@');
            this.Preview_SizeChanged(null, null);
            this.Size = new Size(450, 700);
        }

        private void descriptionBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void updateBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }
    }
}
