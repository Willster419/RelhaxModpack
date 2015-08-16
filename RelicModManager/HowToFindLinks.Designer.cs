namespace RelicModManager
{
    partial class HowToFindLinks
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.backButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.instructions = new System.Windows.Forms.Label();
            this.howToFindTheLink = new System.Windows.Forms.PictureBox();
            this.gotoFormPost = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.howToFindTheLink)).BeginInit();
            this.SuspendLayout();
            // 
            // backButton
            // 
            this.backButton.Location = new System.Drawing.Point(12, 264);
            this.backButton.Name = "backButton";
            this.backButton.Size = new System.Drawing.Size(75, 23);
            this.backButton.TabIndex = 1;
            this.backButton.Text = "back";
            this.backButton.UseVisualStyleBackColor = true;
            this.backButton.Click += new System.EventHandler(this.backButton_Click);
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(197, 264);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(75, 23);
            this.nextButton.TabIndex = 2;
            this.nextButton.Text = "next";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // instructions
            // 
            this.instructions.AutoSize = true;
            this.instructions.Location = new System.Drawing.Point(12, 23);
            this.instructions.Name = "instructions";
            this.instructions.Size = new System.Drawing.Size(35, 13);
            this.instructions.TabIndex = 3;
            this.instructions.Text = "label1";
            // 
            // howToFindTheLink
            // 
            this.howToFindTheLink.InitialImage = null;
            this.howToFindTheLink.Location = new System.Drawing.Point(12, 39);
            this.howToFindTheLink.Name = "howToFindTheLink";
            this.howToFindTheLink.Size = new System.Drawing.Size(260, 219);
            this.howToFindTheLink.TabIndex = 4;
            this.howToFindTheLink.TabStop = false;
            // 
            // gotoFormPost
            // 
            this.gotoFormPost.Enabled = false;
            this.gotoFormPost.Location = new System.Drawing.Point(172, 13);
            this.gotoFormPost.Name = "gotoFormPost";
            this.gotoFormPost.Size = new System.Drawing.Size(100, 23);
            this.gotoFormPost.TabIndex = 5;
            this.gotoFormPost.Text = "go to thread post";
            this.gotoFormPost.UseVisualStyleBackColor = true;
            this.gotoFormPost.Click += new System.EventHandler(this.gotoFormPost_Click);
            // 
            // HowToFindLinks
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 299);
            this.Controls.Add(this.gotoFormPost);
            this.Controls.Add(this.howToFindTheLink);
            this.Controls.Add(this.instructions);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.backButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "HowToFindLinks";
            this.Text = "HowToFindLinks";
            this.Shown += new System.EventHandler(this.HowToFindLinks_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.howToFindTheLink)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button backButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Label instructions;
        public System.Windows.Forms.PictureBox howToFindTheLink;
        private System.Windows.Forms.Button gotoFormPost;
    }
}