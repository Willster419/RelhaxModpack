namespace RelhaxModpack
{
    partial class RelhaxMediaPlayer
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
            //dispose of audio stuff
            waveOutDevice.Dispose();
            audioFileReader.Dispose();
            client.Dispose();
            audioStream.Dispose();

            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.Volume = new System.Windows.Forms.TrackBar();
            this.FileName = new System.Windows.Forms.Label();
            this.Seekbar = new System.Windows.Forms.TrackBar();
            this.StopButton = new System.Windows.Forms.Button();
            this.PlayPause = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.Volume)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Seekbar)).BeginInit();
            this.SuspendLayout();
            // 
            // Volume
            // 
            this.Volume.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.Volume.Location = new System.Drawing.Point(203, 57);
            this.Volume.Name = "Volume";
            this.Volume.Size = new System.Drawing.Size(128, 45);
            this.Volume.TabIndex = 4;
            this.Volume.Scroll += new System.EventHandler(this.Volume_Scroll);
            // 
            // FileName
            // 
            this.FileName.AutoSize = true;
            this.FileName.Location = new System.Drawing.Point(3, 0);
            this.FileName.Name = "FileName";
            this.FileName.Size = new System.Drawing.Size(51, 13);
            this.FileName.TabIndex = 6;
            this.FileName.Text = "FileName";
            // 
            // Seekbar
            // 
            this.Seekbar.Location = new System.Drawing.Point(0, 15);
            this.Seekbar.Name = "Seekbar";
            this.Seekbar.Size = new System.Drawing.Size(400, 45);
            this.Seekbar.TabIndex = 7;
            this.Seekbar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Seekbar_MouseMove);
            this.Seekbar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Seekbar_MouseMove);
            // 
            // StopButton
            // 
            this.StopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.StopButton.Image = global::RelhaxModpack.Properties.Resources.stop2;
            this.StopButton.Location = new System.Drawing.Point(3, 50);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(40, 37);
            this.StopButton.TabIndex = 0;
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.Stop_Click);
            // 
            // PlayPause
            // 
            this.PlayPause.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.PlayPause.Image = global::RelhaxModpack.Properties.Resources.play_pause;
            this.PlayPause.Location = new System.Drawing.Point(49, 50);
            this.PlayPause.Name = "PlayPause";
            this.PlayPause.Size = new System.Drawing.Size(40, 37);
            this.PlayPause.TabIndex = 3;
            this.PlayPause.UseVisualStyleBackColor = true;
            this.PlayPause.Click += new System.EventHandler(this.PlayPause_Click);
            // 
            // RelhaxMediaPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.Controls.Add(this.Volume);
            this.Controls.Add(this.PlayPause);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.Seekbar);
            this.Controls.Add(this.FileName);
            this.MaximumSize = new System.Drawing.Size(400, 90);
            this.MinimumSize = new System.Drawing.Size(400, 90);
            this.Name = "RelhaxMediaPlayer";
            this.Size = new System.Drawing.Size(400, 90);
            this.Load += new System.EventHandler(this.RelhaxMediaPlayer_Load);
            ((System.ComponentModel.ISupportInitialize)(this.Volume)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Seekbar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TrackBar Volume;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.Button PlayPause;
        private System.Windows.Forms.Label FileName;
        private System.Windows.Forms.TrackBar Seekbar;
    }
}
