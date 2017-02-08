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
        
        public Preview()
        {
            InitializeComponent();
        }
        
        public Preview(string title, List<String> pictureList, string desc, string update)
        {
            InitializeComponent();
            modOrConfigName = title;
            pictures = pictureList;
            updateComments = update;
            description = desc;
            loadingImage = RelicModManager.Properties.Resources.loading;
        }
        //to be over-ridden with the form load
        public void load()
        {
            this.Text = modOrConfigName;
            for (int i = 0; i < pictures.Count; i++)
            {
                this.makeLinkedLabel(i);
            }
            previewPicture.WaitOnLoad = false;
            previewPicture.InitialImage = loadingImage;
            if(pictures != null)
            {
                this.displayPictures(pictures[0]);
            }
            //set the updatecomment and description text boxes
            //set dev URL
        }
        
        public void displayPictures(string URL)
        {
            previewPicture.LoadAsync(URL); 
        }
        
        private void makeLinkedLabel(int i)
        {
            LinkLabel label = new LinkLabel();
            label.Name = "" + i;
            label.Text = "" + i;
            label.LinkClicked += new LinkLabelLinkClickedEventHandler(label_LinkClicked);
        }

        void label_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            LinkLabel lb = (LinkLabel)sender;
            int i = int.Parse(lb.Text);
            i--;
            this.displayPictures(pictures[i]);
        }
        
        //to be over-ridden with previewPicture load completed
        private void previewPictureLoadDone()
        {
            //do i need this???
        }
    }
}
