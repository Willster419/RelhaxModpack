using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RelhaxModpack
{
    //the preview window to preview a mod. will show the mod name,
    //ro config name, pictures, description, and any update comments
    public partial class Preview : Form
    {
        private Size previewComponentSize = new Size(418, 309);
        private Point previewComponentLocation = new Point(12, 12);
        private Color previewComponentBackColor = SystemColors.ControlDarkDark;
        private PictureBoxSizeMode previewComponentSizeMode = PictureBoxSizeMode.Zoom;
        public string modOrConfigName { get; set; }
        public string description { get; set; }
        public string updateComments { get; set; }
        List<Media> medias = new List<Media>();
        private Image loadingImage;
        public string devURL { get; set; }
        private int currentlySelected = 0;
        private const int titleBar = 23;//set origionally for 23
        private int difference = 0;
        private WebBrowser youtubedisplay;
        //Preview constructor that sets all the required values
        public Preview(string title, List<Media> pictureList, string desc, string update = "", string dev = "")
        {
            InitializeComponent();
            modOrConfigName = title;
            medias = pictureList;
            updateComments = update;
            description = desc;
            devURL = dev;
            if (devURL == null || devURL.Equals(""))
            {
                devLinkLabel.Enabled = false;
                devLinkLabel.Visible = false;
            }
            loadingImage = RelhaxModpack.Properties.Resources.loading;
        }
        //sets the window title to reflect the new picture, and
        //begine the async process of loading the new picture
        public void displayMedia(Media m)
        {
            previewPicture.Image = null;
            if (medias.Count == 0)
                return;
            if (m.URL.Equals(""))
                return;
            if (this.Controls.Contains(previewPicture))
            {
                this.Controls.Remove(previewPicture);
                previewPicture.Dispose();
                previewPicture = null;
            }
            if (this.Contains(youtubedisplay))
            {
                this.Controls.Remove(youtubedisplay);
                youtubedisplay.Dispose();
                youtubedisplay = null;
            }
            if (m.mediaType == MediaType.picture)
            {
                previewPicture = new PictureBox()
                {
                    Size = previewComponentSize,
                    BackColor = previewComponentBackColor,
                    Location = previewComponentLocation,
                    SizeMode = previewComponentSizeMode
                };
                previewPicture.Click += previewPicture_Click;
                this.Controls.Add(previewPicture);
                previewPicture.Image = Settings.getLoadingImage();
                previewPicture.LoadAsync(m.URL);
                this.Text = m.name + " - " + currentlySelected;
                Utils.appendToLog("Preview: started loading of picture '" + m.name + "' at URL '" + m.URL + "'");
            }
            else if (m.mediaType == MediaType.youtube)
            {
                youtubedisplay = new WebBrowser()
                {
                    Size = previewComponentSize,
                    Location = previewComponentLocation,
                    ScriptErrorsSuppressed = true
                };
                this.Controls.Add(youtubedisplay);
                youtubedisplay.Navigate(m.URL);
                this.Text = m.name + " - " + currentlySelected;
                Utils.appendToLog("Preview: started loading of youtube video '" + m.name + "' at URL '" + m.URL + "'");
            }
            else
            {
                Utils.appendToLog("ERROR: Unknown media type: " + m.mediaType);
            }
        }
        //make the linked labels for each picture in the picturesList
        //so a user can navagate easily through the pictures
        private void makeLinkedLabel(int i)
        {
            LinkLabel label = new LinkLabel();
            label.Name = "" + i;
            label.Text = "" + i;
            label.LinkClicked += new LinkLabelLinkClickedEventHandler(label_LinkClicked);
            label.AutoSize = true;
            int xLocation = 0;
            foreach (Control c in pictureCountPanel.Controls)
                xLocation += c.Size.Width;
            label.Location = new Point(xLocation, 5);
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
            this.displayMedia(medias[i]);
            Preview_SizeChanged(null, null);
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
            if (currentlySelected >= medias.Count)
            {
                currentlySelected--;
                return;
            }
            this.displayMedia(medias[currentlySelected]);
            Preview_SizeChanged(null, null);
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
            this.displayMedia(medias[currentlySelected]);
            Preview_SizeChanged(null, null);
        }
        //handler for if the user changes the size of the window
        private void Preview_SizeChanged(object sender, EventArgs e)
        {
            //previewPicture, descriptionBox, nextPicButton and updateBox should all have the same size width.
            int width = this.Size.Width - 32;
            int applicationHeight = this.Size.Height;
            //do this from bottom to top
            updateBox.Size = new Size(width, updateBox.Size.Height);
            descriptionBox.Size = new Size(width, descriptionBox.Size.Height);
            int scale = 0;
            switch (Settings.fontSizeforum)
            {
                case Settings.FontSize.font125:
                    scale = 30;
                    break;
                case Settings.FontSize.font175:
                    scale = 75;
                    break;
                case Settings.FontSize.font225:
                    scale = 145;
                    break;
                case Settings.FontSize.font275:
                    scale = 200;
                    break;
                case Settings.FontSize.DPI125:
                    scale = 30;
                    break;
                case Settings.FontSize.DPI175:
                    scale = 75;
                    break;
                case Settings.FontSize.DPI225:
                    scale = 145;
                    break;
                case Settings.FontSize.DPI275:
                    scale = 200;
                    break;
                case Settings.FontSize.DPIAUTO:
                    int settingsScale = (int)Settings.scaleSize;
                    scale = settingsScale * 45;
                    scale = scale + 30;
                    break;
            }
            Size tempSize = new Size(0, 0);
            if (previewPicture != null)
            {
                previewPicture.Size = new Size(width, applicationHeight - 265 - difference - scale);
                tempSize = previewPicture.Size;
            }
            if (youtubedisplay != null)
            {
                youtubedisplay.Size = new Size(width, applicationHeight - 265 - difference - scale);
                tempSize = youtubedisplay.Size;
            }
            updateBox.Location = new Point(12, 12 + tempSize.Height + 6 + nextPicButton.Size.Height + 6 + descriptionBox.Size.Height + 6);
            descriptionBox.Location = new Point(12, 12 + tempSize.Height + 6 + nextPicButton.Size.Height + 6);
            nextPicButton.Location = new Point(this.Size.Width - 21 - nextPicButton.Size.Width, 12 + tempSize.Height + 6);
            previousPicButton.Location = new Point(12, 12 + tempSize.Height + 6);
            pictureCountPanel.Location = new Point(12 + previousPicButton.Size.Width + 12, 12 + tempSize.Height + 6);
            pictureCountPanel.Size = new Size(width - pictureCountPanel.Location.X - nextPicButton.Size.Width - 4, pictureCountPanel.Size.Height);
            devLinkLabel.Location = new Point(this.Size.Width - 12 - devLinkLabel.Size.Width - 4, applicationHeight - 49 - difference - 5);
        }
        //applies translations
        private void applyTranslations()
        {
            nextPicButton.Text = Translations.getTranslatedString(nextPicButton.Name);
            previousPicButton.Text = Translations.getTranslatedString(previousPicButton.Name);
            devLinkLabel.Text = Translations.getTranslatedString(devLinkLabel.Name);
        }
        //handler that triggeres right before the window is shown
        private void Preview_Load(object sender, EventArgs e)
        {
            //update for translations
            this.applyTranslations();
            //font scaling
            this.AutoScaleMode = Settings.appScalingMode;
            this.Font = Settings.appFont;
            if (Settings.appScalingMode == System.Windows.Forms.AutoScaleMode.Dpi)
            {
                this.Scale(new System.Drawing.SizeF(Settings.scaleSize, Settings.scaleSize));
            }
            this.Text = modOrConfigName;
            for (int i = 0; i < medias.Count; i++)
            {
                this.makeLinkedLabel(i);
            }
            previewPicture.WaitOnLoad = false;
            previewPicture.InitialImage = Settings.getLoadingImage();
            if (medias != null)
            {
                currentlySelected = 0;
                if (medias.Count > 0)
                    this.displayMedia(medias[currentlySelected]);
            }
            if (description == null)
                description = Translations.getTranslatedString("noDescription");
            else if (description.Equals(""))
                description = Translations.getTranslatedString("noDescription");
            if (updateComments == null)
                updateComments = Translations.getTranslatedString("noUpdateInfo");
            else if (updateComments.Equals(""))
                updateComments = Translations.getTranslatedString("noUpdateInfo");
            descriptionBox.Text = description;
            updateBox.Text = updateComments;
            //get the size of the title bar window
            Rectangle screenRektangle = RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRektangle.Top - this.Top;
            //largest possible is 46
            //mine (programmed for) is 23
            if (titleHeight > titleBar)
            {
                difference = titleHeight - titleBar;
            }
            //idk why it's dones twice
            this.Preview_SizeChanged(null, null);
            this.Size = new Size(450, 700);
            Settings.setUIColor(this);
            //specify the start location
            this.Location = new Point(Settings.previewX, Settings.previewY);
        }

        private void descriptionBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void updateBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void previewPicture_Click(object sender, EventArgs e)
        {
            if (this.WindowState != FormWindowState.Maximized)
                this.WindowState = FormWindowState.Maximized;
            else
                this.WindowState = FormWindowState.Normal;
        }

        private void Preview_FormClosing(object sender, FormClosingEventArgs e)
        {
            // if preview window is minimized and will be closed directly via the taskbar, windows send -32000 coordinate X and Y, so not storing it
            if (this.Location.X > 0 && this.Location.Y > 0)
            {
                Settings.previewX = this.Location.X;
                Settings.previewY = this.Location.Y;
            }
        }
    }
}
