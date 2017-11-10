using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RelhaxModpack
{
    //the preview window to preview a mod. will show the mod name,
    //ro config name, pictures, description, and any update comments
    public partial class Preview : RelhaxForum
    {
        private Size PreviewComponentSize = new Size(418, 309);
        private Point PreviewComponentLocation = new Point(12, 12);
        private Color PreviewComponentBackColor = SystemColors.ControlDarkDark;
        private PictureBoxSizeMode PreviewComponentSizeMode = PictureBoxSizeMode.Zoom;
        public List<Media> Medias { get; set; }
        private Image LoadingImage;
        private int CurrentlySelected = 0;
        private WebBrowser Youtubedisplay;
        private string DateFormat;
        public SelectableDatabasePackage DBO { get; set; }
        public string LastUpdated { get; set; }
        
        public Preview()
        {
            InitializeComponent();
        }

        private void Preview_Load(object sender, EventArgs e)
        {
            if (Medias == null)
                Medias = new List<Media>();
            //update for translations
            NextPicButton.Text = Translations.getTranslatedString(NextPicButton.Name);
            PreviousPicButton.Text = Translations.getTranslatedString(PreviousPicButton.Name);
            DevLinkLabel.Text = Translations.getTranslatedString(DevLinkLabel.Name);
            //check if devURL should be visable or not
            if (DBO.DevURL == null || DBO.DevURL.Equals(""))
            {
                DevLinkLabel.Enabled = false;
                DevLinkLabel.Visible = false;
            }
            //set default loading images and image properties
            LoadingImage = RelhaxModpack.Properties.Resources.loading;
            PreviewPicture.WaitOnLoad = false;
            PreviewPicture.InitialImage = Settings.getLoadingImage();
            Text = DBO.NameFormatted;
            for (int i = 0; i < Medias.Count; i++)
            {
                MakeLinkedLabel(i);
            }
            if (Medias != null)
            {
                CurrentlySelected = 0;
                if (Medias.Count > 0)
                    DisplayMedia(Medias[CurrentlySelected]);
            }
            DescriptionBox.Text = (DBO.Description == null || DBO.Description.Equals(""))? Translations.getTranslatedString("noDescription"): DBO.Description;
            DateFormat = DBO.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(DBO.Timestamp);
            UpdateBox.Text = (DBO.UpdateComment == null || DBO.UpdateComment.Equals("")) ? Translations.getTranslatedString("noUpdateInfo") : DBO.UpdateComment;
            UpdateBox.Text = UpdateBox.Text + "\n" + LastUpdated + DateFormat;
            Size = new Size(450, 700);
            Preview_SizeChanged(null, null);
            //specify the start location
            Location = new Point(Settings.PreviewX, Settings.PreviewY);
        }
        //sets the window title to reflect the new picture, and
        //begine the async process of loading the new picture
        public void DisplayMedia(Media m)
        {
            PreviewPicture.Image = null;
            if (Medias.Count == 0)
                return;
            if (m.URL.Equals(""))
                return;
            if (Controls.Contains(PreviewPicture))
            {
                Controls.Remove(PreviewPicture);
                PreviewPicture.Dispose();
                PreviewPicture = null;
            }
            if (Contains(Youtubedisplay))
            {
                Controls.Remove(Youtubedisplay);
                Youtubedisplay.Dispose();
                Youtubedisplay = null;
            }
            if (m.MediaType == MediaType.Picture)
            {
                PreviewPicture = new PictureBox()
                {
                    Size = PreviewComponentSize,
                    BackColor = PreviewComponentBackColor,
                    Location = PreviewComponentLocation,
                    SizeMode = PreviewComponentSizeMode
                };
                PreviewPicture.Click += PreviewPicture_Click;
                Controls.Add(PreviewPicture);
                PreviewPicture.Image = Settings.getLoadingImage();
                PreviewPicture.LoadAsync(m.URL);
                Text = DBO.NameFormatted + " - " + CurrentlySelected;
                Utils.AppendToLog("Preview: started loading of picture '" + DBO.NameFormatted + "' at URL '" + m.URL + "'");
            }
            else if (m.MediaType == MediaType.Youtube)
            {
                Youtubedisplay = new WebBrowser()
                {
                    Size = PreviewComponentSize,
                    Location = PreviewComponentLocation,
                    ScriptErrorsSuppressed = true
                };
                Controls.Add(Youtubedisplay);
                Youtubedisplay.Navigate(m.URL);
                Text = DBO.NameFormatted + " - " + CurrentlySelected;
                Utils.AppendToLog("Preview: started loading of youtube video '" + DBO.NameFormatted + "' at URL '" + m.URL + "'");
            }
            else
            {
                Utils.AppendToLog("ERROR: Unknown media type: " + m.MediaType);
            }
        }
        //make the linked labels for each picture in the picturesList
        //so a user can navagate easily through the pictures
        private void MakeLinkedLabel(int i)
        {
            LinkLabel label = new LinkLabel();
            label.Name = "" + i;
            label.Text = "" + i;
            label.LinkClicked += new LinkLabelLinkClickedEventHandler(Label_LinkClicked);
            label.AutoSize = true;
            int xLocation = 0;
            foreach (Control c in PictureCountPanel.Controls)
                xLocation += c.Size.Width;
            label.Location = new Point(xLocation, 5);
            label.TabStop = true;
            PictureCountPanel.Controls.Add(label);
        }
        //handler for when a link label is clicked in the panel picture selection panel
        void Label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel lb = (LinkLabel)sender;
            int i = int.Parse(lb.Text);
            //i--;
            CurrentlySelected = i;
            DisplayMedia(Medias[i]);
            Preview_SizeChanged(null, null);
        }
        //show the suplied dev url thread
        private void DevLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!DBO.DevURL.Equals(""))
                System.Diagnostics.Process.Start(DBO.DevURL);
        }
        //load the next picture in the list
        private void NextPicButton_Click(object sender, EventArgs e)
        {
            CurrentlySelected++;
            if (CurrentlySelected >= Medias.Count)
            {
                CurrentlySelected--;
                return;
            }
            DisplayMedia(Medias[CurrentlySelected]);
            Preview_SizeChanged(null, null);
        }
        //load the previous picture in the list
        private void PreviousPicButton_Click(object sender, EventArgs e)
        {
            CurrentlySelected--;
            if (CurrentlySelected < 0)
            {
                CurrentlySelected++;
                return;
            }
            DisplayMedia(Medias[CurrentlySelected]);
            Preview_SizeChanged(null, null);
        }
        //handler for if the user changes the size of the window
        private void Preview_SizeChanged(object sender, EventArgs e)
        {
            //previewPicture, descriptionBox, nextPicButton and updateBox should all have the same size width.
            int width = Size.Width - 32;
            int applicationHeight = Size.Height;
            //do this from bottom to top
            UpdateBox.Size = new Size(width, UpdateBox.Size.Height);
            DescriptionBox.Size = new Size(width, DescriptionBox.Size.Height);
            int scale = 0;
            switch (Settings.FontSizeforum)
            {
                case Settings.FontSize.Font125:
                    scale = 30;
                    break;
                case Settings.FontSize.Font175:
                    scale = 75;
                    break;
                case Settings.FontSize.Font225:
                    scale = 145;
                    break;
                case Settings.FontSize.Font275:
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
                    int settingsScale = (int)Settings.ScaleSize;
                    scale = settingsScale * 45;
                    scale = scale + 30;
                    break;
            }
            Size tempSize = new Size(0, 0);
            if (PreviewPicture != null)
            {
                PreviewPicture.Size = new Size(width, applicationHeight - 265 - TitleBarDifference - scale);
                tempSize = PreviewPicture.Size;
            }
            if (Youtubedisplay != null)
            {
                Youtubedisplay.Size = new Size(width, applicationHeight - 265 - TitleBarDifference - scale);
                tempSize = Youtubedisplay.Size;
            }
            UpdateBox.Location = new Point(12, 12 + tempSize.Height + 6 + NextPicButton.Size.Height + 6 + DescriptionBox.Size.Height + 6);
            DescriptionBox.Location = new Point(12, 12 + tempSize.Height + 6 + NextPicButton.Size.Height + 6);
            NextPicButton.Location = new Point(Size.Width - 21 - NextPicButton.Size.Width, 12 + tempSize.Height + 6);
            PreviousPicButton.Location = new Point(12, 12 + tempSize.Height + 6);
            PictureCountPanel.Location = new Point(12 + PreviousPicButton.Size.Width + 12, 12 + tempSize.Height + 6);
            PictureCountPanel.Size = new Size(width - PictureCountPanel.Location.X - NextPicButton.Size.Width - 4, PictureCountPanel.Size.Height);
            DevLinkLabel.Location = new Point(Size.Width - 12 - DevLinkLabel.Size.Width - 4, applicationHeight - 49 - TitleBarDifference - 5);
        }

        private void DescriptionBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void UpdateBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start(e.LinkText);
        }

        private void PreviewPicture_Click(object sender, EventArgs e)
        {
            if (WindowState != FormWindowState.Maximized)
                WindowState = FormWindowState.Maximized;
            else
                WindowState = FormWindowState.Normal;
        }

        private void Preview_FormClosing(object sender, FormClosingEventArgs e)
        {
            // if preview window is minimized and will be closed directly via the taskbar, windows send -32000 coordinate X and Y, so not storing it
            if (Location.X > 0 && Location.Y > 0)
            {
                Settings.PreviewX = Location.X;
                Settings.PreviewY = Location.Y;
            }
        }
    }
}
