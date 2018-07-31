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
        private Size PreviewComponentSize = new Size(418, 309);//hard-coded designer size
        private Point PreviewComponentLocation = new Point(12, 12);
        private Color PreviewComponentBackColor = SystemColors.ControlDarkDark;
        public List<Media> Medias { get; set; }
        private Image LoadingImage;
        private int CurrentlySelected = 0;
        private WebBrowser Browser;//to change to HTML5 chrome style browser
        private RelhaxMediaPlayer player;
        private string DateFormat;
        public SelectablePackage DBO { get; set; }
        public string LastUpdated { get; set; }
        Label ErrorLabel;
        
        public Preview()
        {
            InitializeComponent();
        }

        private void Preview_Load(object sender, EventArgs e)
        {
            if (Medias == null)
                Medias = new List<Media>();
            //update for translations
            NextPicButton.Text = Translations.GetTranslatedString(NextPicButton.Name);
            PreviousPicButton.Text = Translations.GetTranslatedString(PreviousPicButton.Name);
            DevLinkLabel.Text = Translations.GetTranslatedString(DevLinkLabel.Name);
            //check if devURL should be visable or not
            if (DBO.DevURL == null || DBO.DevURL.Equals(""))
            {
                DevLinkLabel.Enabled = false;
                DevLinkLabel.Visible = false;
            }
            //set default loading images and image properties
            LoadingImage = RelhaxModpack.Properties.Resources.loading;
            PreviewPicture.WaitOnLoad = false;
            PreviewPicture.InitialImage = Settings.GetLoadingImage();
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
            DescriptionBox.Text = (DBO.Description == null || DBO.Description.Equals(""))? Translations.GetTranslatedString("noDescription"): DBO.Description;
            DateFormat = DBO.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(DBO.Timestamp);
            UpdateBox.Text = (DBO.UpdateComment == null || DBO.UpdateComment.Equals("")) ? Translations.GetTranslatedString("noUpdateInfo") : DBO.UpdateComment;
            UpdateBox.Text = UpdateBox.Text + "\n" + LastUpdated + DateFormat;
            //specify the start location
            if (Utils.PointWithinScreen(Settings.PreviewX, Settings.PreviewY))
            {
                StartPosition = FormStartPosition.Manual;
                Location = new Point(Settings.PreviewX, Settings.PreviewY);
            }
            if (Settings.SView == SelectionView.Legacy)
            {
                if (Program.Version == Program.ProgramVersion.Alpha)
                    Logging.Manager(string.Format("DEBUG: from Preview load: Legacy view, p.ContainsFocus={0}", this.ContainsFocus));
                LegacyHotfixTimer.Start();
            }
        }
        public override void OnPostLoad()
        {
            //re-apply the backColor if it's the picture
            if ((PreviewPicture != null) && (Medias.Count > CurrentlySelected) && (Medias[CurrentlySelected].MediaType == MediaType.Picture))
                PreviewPicture.BackColor = PreviewComponentBackColor;
            //set the size to be the orig saved size
            Size = new Size(Settings.PreviewWidth, Settings.PreviewHeight);
            //then set it to fullscreen if it was fullscreen before
            if (Settings.PreviewFullscreen)
                WindowState = FormWindowState.Maximized;
        }
        //sets the window title to reflect the new picture, and
        //begine the async process of loading the new picture
        public void DisplayMedia(Media m)
        {
            if(PreviewPicture != null)
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
            if (Controls.Contains(Browser))
            {
                Controls.Remove(Browser);
                Browser.Dispose();
                Browser = null;
            }
            if(Controls.Contains(player))
            {
                Controls.Remove(player);
                player.Dispose();
                player = null;
            }
            if(Controls.Contains(ErrorLabel))
            {
                Controls.Remove(ErrorLabel);
                ErrorLabel.Dispose();
                ErrorLabel = null;
            }
            //calculate the new component size
            //width is from the top left of previous to the top right of next
            //height is PreviewComponentLocation (12) Y to location of top left of preview - 6
            int picturewidth = (NextPicButton.Location.X + NextPicButton.Size.Width)- PreviousPicButton.Location.X;
            int pictureHeight = PreviousPicButton.Location.Y - 6 - PreviewComponentLocation.Y;
            PreviewComponentSize = new Size(picturewidth, pictureHeight);
            switch(m.MediaType)
            {
                case MediaType.Picture:
                    PreviewPicture = new PictureBox()
                    {
                        Size = PreviewComponentSize,
                        BackColor = PreviewComponentBackColor,
                        Location = PreviewComponentLocation,
                        SizeMode = PictureBoxSizeMode.Zoom,
                        Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left)
                    };
                    PreviewPicture.Click += PreviewPicture_Click;
                    Controls.Add(PreviewPicture);
                    PreviewPicture.Image = Settings.GetLoadingImage();
                    PreviewPicture.LoadAsync(m.URL);
                    Text = DBO.NameFormatted + " - " + CurrentlySelected;
                    Logging.Manager("Preview: started loading of picture '" + DBO.NameFormatted + "' at URL '" + m.URL + "'");
                    break;
                case MediaType.Webpage:
                    //NOTE: needs to be fixed
                    Browser = new WebBrowser()
                    {
                        Size = PreviewComponentSize,
                        Location = PreviewComponentLocation,
                        ScriptErrorsSuppressed = true,
                        Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left)
                    };
                    Controls.Add(Browser);
                    Browser.Navigate(m.URL);
                    Text = DBO.NameFormatted + " - " + CurrentlySelected;
                    Logging.Manager("Preview: started loading of webpage '" + DBO.NameFormatted + "' at URL '" + m.URL + "'");
                    break;
                case MediaType.MediaFile:
                    try
                    {
                        player = new RelhaxMediaPlayer()
                        {
                            Size = PreviewComponentSize,
                            Location = PreviewComponentLocation,
                            //BackColor = PreviewComponentBackColor,
                            MediaURL = m.URL,
                            Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left)
                        };
                        Controls.Add(player);
                        Text = DBO.NameFormatted + " - " + CurrentlySelected;
                        Logging.Manager("Preview: started loading of direct media '" + DBO.NameFormatted + "' at URL '" + m.URL + "'");
                    }
                    catch (Exception e)
                    {
                        Utils.ExceptionLog(e);
                        ErrorLabel = new Label()
                        {
                            Text = "ERROR",
                            AutoSize = false,
                            Size = PreviewComponentSize
                        };
                        Controls.Add(ErrorLabel);
                        Text = DBO.NameFormatted + " - " + CurrentlySelected;
                    }
                    break;
                case MediaType.HTML:
                    Browser = new WebBrowser()
                    {
                        Size = PreviewComponentSize,
                        Location = PreviewComponentLocation,
                        ScriptErrorsSuppressed = true,
                        Anchor = (AnchorStyles.Bottom | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Left)
                    };
                    Controls.Add(Browser);
                    Browser.DocumentText = m.URL;
                    Text = DBO.NameFormatted + " - " + CurrentlySelected;
                    Logging.Manager("Preview: started loading of HTML '" + DBO.NameFormatted + "' at HTML '" + m.URL + "'");
                    break;
                default:
                    Logging.Manager("WARNING: Unknown mediaType: " + m.MediaType);
                    break;
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
        }
        //show the suplied dev url thread
        private void DevLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (!DBO.DevURL.Equals(""))
                // System.Diagnostics.Process.Start(DBO.DevURL);
                Utils.CallBrowser(DBO.DevURL);
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
        }

        private void DescriptionBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            // System.Diagnostics.Process.Start(e.LinkText);
            Utils.CallBrowser(e.LinkText);
        }

        private void UpdateBox_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            // System.Diagnostics.Process.Start(e.LinkText);
            Utils.CallBrowser(e.LinkText);
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
            //save wether the window was in fullscreen mode before closing
            //also only save the size if the window is normal
            switch (WindowState)
            {
                case FormWindowState.Maximized:
                    //save maximized property
                    Settings.PreviewFullscreen = true;
                    break;
                case FormWindowState.Minimized:
                    //save maximized property
                    Settings.PreviewFullscreen = false;
                    break;
                case FormWindowState.Normal:
                    //save maximzed property and window size
                    Settings.PreviewFullscreen = false;
                    Settings.PreviewHeight = Size.Height;
                    Settings.PreviewWidth = Size.Width;
                    break;
            }
            //save the location of the preview window if it's within the screen bounds
            if(Utils.PointWithinScreen(Location))
            {
                Settings.PreviewX = Location.X;
                Settings.PreviewY = Location.Y;
            }
        }

        private void LegacyHotfixTimer_Tick(object sender, EventArgs e)
        {
            if (Settings.SView == SelectionView.Legacy)
            {
                if (Program.Version == Program.ProgramVersion.Alpha)
                    Logging.Manager(string.Format("DEBUG: from Preview timer: Legacy view, p.ContainsFocus={0}", this.ContainsFocus));
                if(!this.ContainsFocus)
                {
                    if (Program.Version == Program.ProgramVersion.Alpha)
                        Logging.Manager(string.Format("DEBUG: from Preview timer: Legacy view, forcing focus conatin"));
                    this.Focus();
                }
            }
            LegacyHotfixTimer.Stop();
        }
    }
}
