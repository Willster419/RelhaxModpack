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
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.MediaPlayerLayout = new System.Windows.Forms.TableLayoutPanel();
            this.Stop = new System.Windows.Forms.Button();
            this.Seekbar = new System.Windows.Forms.TrackBar();
            this.PlayPause = new System.Windows.Forms.Button();
            this.Volume = new System.Windows.Forms.TrackBar();
            this.FileName = new System.Windows.Forms.Label();
            this.MediaPlayerLayout.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Seekbar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Volume)).BeginInit();
            this.SuspendLayout();
            // 
            // MediaPlayerLayout
            // 
            this.MediaPlayerLayout.ColumnCount = 3;
            this.MediaPlayerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.MediaPlayerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.MediaPlayerLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 606F));
            this.MediaPlayerLayout.Controls.Add(this.Stop, 0, 2);
            this.MediaPlayerLayout.Controls.Add(this.Seekbar, 0, 1);
            this.MediaPlayerLayout.Controls.Add(this.PlayPause, 1, 2);
            this.MediaPlayerLayout.Controls.Add(this.Volume, 2, 2);
            this.MediaPlayerLayout.Controls.Add(this.FileName, 0, 0);
            this.MediaPlayerLayout.Location = new System.Drawing.Point(3, 3);
            this.MediaPlayerLayout.Name = "MediaPlayerLayout";
            this.MediaPlayerLayout.RowCount = 3;
            this.MediaPlayerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.MediaPlayerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MediaPlayerLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.MediaPlayerLayout.Size = new System.Drawing.Size(371, 80);
            this.MediaPlayerLayout.TabIndex = 3;
            // 
            // Stop
            // 
            this.Stop.Location = new System.Drawing.Point(3, 53);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(94, 23);
            this.Stop.TabIndex = 0;
            this.Stop.Text = "stop";
            this.Stop.UseVisualStyleBackColor = true;
            this.Stop.Click += new System.EventHandler(this.Stop_Click);
            // 
            // Seekbar
            // 
            this.MediaPlayerLayout.SetColumnSpan(this.Seekbar, 3);
            this.Seekbar.Dock = System.Windows.Forms.DockStyle.Left;
            this.Seekbar.Location = new System.Drawing.Point(3, 23);
            this.Seekbar.Name = "Seekbar";
            this.Seekbar.Size = new System.Drawing.Size(368, 24);
            this.Seekbar.TabIndex = 1;
            this.Seekbar.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Seekbar_MouseDown);
            this.Seekbar.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Seekbar_MouseMove);
            // 
            // PlayPause
            // 
            this.PlayPause.Location = new System.Drawing.Point(103, 53);
            this.PlayPause.Name = "PlayPause";
            this.PlayPause.Size = new System.Drawing.Size(94, 23);
            this.PlayPause.TabIndex = 3;
            this.PlayPause.Text = "play/pause";
            this.PlayPause.UseVisualStyleBackColor = true;
            this.PlayPause.Click += new System.EventHandler(this.PlayPause_Click);
            // 
            // Volume
            // 
            this.Volume.Location = new System.Drawing.Point(203, 53);
            this.Volume.Name = "Volume";
            this.Volume.Size = new System.Drawing.Size(128, 24);
            this.Volume.TabIndex = 4;
            this.Volume.Scroll += new System.EventHandler(this.Volume_Scroll);
            // 
            // FileName
            // 
            this.FileName.AutoSize = true;
            this.MediaPlayerLayout.SetColumnSpan(this.FileName, 3);
            this.FileName.Location = new System.Drawing.Point(3, 0);
            this.FileName.Name = "FileName";
            this.FileName.Size = new System.Drawing.Size(35, 13);
            this.FileName.TabIndex = 5;
            this.FileName.Text = "label1";
            // 
            // RelhaxMediaPlayer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MediaPlayerLayout);
            this.Name = "RelhaxMediaPlayer";
            this.Size = new System.Drawing.Size(378, 88);
            this.SizeChanged += new System.EventHandler(this.RelhaxMediaPlayer_SizeChanged);
            this.MediaPlayerLayout.ResumeLayout(false);
            this.MediaPlayerLayout.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.Seekbar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Volume)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel MediaPlayerLayout;
        private System.Windows.Forms.TrackBar Seekbar;
        private System.Windows.Forms.TrackBar Volume;
        private System.Windows.Forms.Label FileName;
        private System.Windows.Forms.Button Stop;
        private System.Windows.Forms.Button PlayPause;
    }
}
