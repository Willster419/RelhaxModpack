namespace RelicModManager
{
    partial class SelectFeatures
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
            this.relhaxBox = new System.Windows.Forms.CheckBox();
            this.relhaxCensoredBox = new System.Windows.Forms.CheckBox();
            this.guiBox = new System.Windows.Forms.CheckBox();
            this.sixthSenseBox = new System.Windows.Forms.CheckBox();
            this.selectYourFeatures = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.sampleIngameVoice = new System.Windows.Forms.Button();
            this.sampleCensoredIngameVoice = new System.Windows.Forms.Button();
            this.sampleGui = new System.Windows.Forms.Button();
            this.sampleSixthSense = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.continueButton = new System.Windows.Forms.Button();
            this.stopPlaying = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // relhaxBox
            // 
            this.relhaxBox.AutoSize = true;
            this.relhaxBox.Location = new System.Drawing.Point(15, 43);
            this.relhaxBox.Name = "relhaxBox";
            this.relhaxBox.Size = new System.Drawing.Size(151, 17);
            this.relhaxBox.TabIndex = 0;
            this.relhaxBox.Text = "Ingame voice during battle";
            this.relhaxBox.UseVisualStyleBackColor = true;
            this.relhaxBox.CheckStateChanged += new System.EventHandler(this.relhaxBox_CheckStateChanged);
            // 
            // relhaxCensoredBox
            // 
            this.relhaxCensoredBox.AutoSize = true;
            this.relhaxCensoredBox.Location = new System.Drawing.Point(15, 66);
            this.relhaxCensoredBox.Name = "relhaxCensoredBox";
            this.relhaxCensoredBox.Size = new System.Drawing.Size(153, 17);
            this.relhaxCensoredBox.TabIndex = 1;
            this.relhaxCensoredBox.Text = "Censored version of above";
            this.relhaxCensoredBox.UseVisualStyleBackColor = true;
            this.relhaxCensoredBox.CheckStateChanged += new System.EventHandler(this.relhaxCensoredBox_CheckStateChanged);
            // 
            // guiBox
            // 
            this.guiBox.AutoSize = true;
            this.guiBox.Location = new System.Drawing.Point(15, 89);
            this.guiBox.Name = "guiBox";
            this.guiBox.Size = new System.Drawing.Size(150, 17);
            this.guiBox.TabIndex = 2;
            this.guiBox.Text = "gui fx (spotted, battle start)";
            this.guiBox.UseVisualStyleBackColor = true;
            // 
            // sixthSenseBox
            // 
            this.sixthSenseBox.AutoSize = true;
            this.sixthSenseBox.Location = new System.Drawing.Point(15, 112);
            this.sixthSenseBox.Name = "sixthSenseBox";
            this.sixthSenseBox.Size = new System.Drawing.Size(113, 17);
            this.sixthSenseBox.TabIndex = 3;
            this.sixthSenseBox.Text = "6th Sense Sounds";
            this.sixthSenseBox.UseVisualStyleBackColor = true;
            // 
            // selectYourFeatures
            // 
            this.selectYourFeatures.AutoSize = true;
            this.selectYourFeatures.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.selectYourFeatures.Location = new System.Drawing.Point(12, 9);
            this.selectYourFeatures.Name = "selectYourFeatures";
            this.selectYourFeatures.Size = new System.Drawing.Size(191, 13);
            this.selectYourFeatures.TabIndex = 4;
            this.selectYourFeatures.Text = "Selext which parts of the Relhax";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(129, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "you wish to download";
            // 
            // sampleIngameVoice
            // 
            this.sampleIngameVoice.Location = new System.Drawing.Point(174, 39);
            this.sampleIngameVoice.Name = "sampleIngameVoice";
            this.sampleIngameVoice.Size = new System.Drawing.Size(75, 23);
            this.sampleIngameVoice.TabIndex = 6;
            this.sampleIngameVoice.Text = "Sample";
            this.sampleIngameVoice.UseVisualStyleBackColor = true;
            this.sampleIngameVoice.Click += new System.EventHandler(this.sampleIngameVoice_Click);
            // 
            // sampleCensoredIngameVoice
            // 
            this.sampleCensoredIngameVoice.Location = new System.Drawing.Point(174, 62);
            this.sampleCensoredIngameVoice.Name = "sampleCensoredIngameVoice";
            this.sampleCensoredIngameVoice.Size = new System.Drawing.Size(75, 23);
            this.sampleCensoredIngameVoice.TabIndex = 7;
            this.sampleCensoredIngameVoice.Text = "Sample";
            this.sampleCensoredIngameVoice.UseVisualStyleBackColor = true;
            this.sampleCensoredIngameVoice.Click += new System.EventHandler(this.sampleCensoredIngameVoice_Click);
            // 
            // sampleGui
            // 
            this.sampleGui.Location = new System.Drawing.Point(174, 85);
            this.sampleGui.Name = "sampleGui";
            this.sampleGui.Size = new System.Drawing.Size(75, 23);
            this.sampleGui.TabIndex = 8;
            this.sampleGui.Text = "Sample";
            this.sampleGui.UseVisualStyleBackColor = true;
            this.sampleGui.Click += new System.EventHandler(this.sampleGui_Click);
            // 
            // sampleSixthSense
            // 
            this.sampleSixthSense.Location = new System.Drawing.Point(174, 108);
            this.sampleSixthSense.Name = "sampleSixthSense";
            this.sampleSixthSense.Size = new System.Drawing.Size(75, 23);
            this.sampleSixthSense.TabIndex = 9;
            this.sampleSixthSense.Text = "Sample";
            this.sampleSixthSense.UseVisualStyleBackColor = true;
            this.sampleSixthSense.Click += new System.EventHandler(this.sampleSixthSense_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(15, 138);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 10;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // continueButton
            // 
            this.continueButton.Location = new System.Drawing.Point(174, 138);
            this.continueButton.Name = "continueButton";
            this.continueButton.Size = new System.Drawing.Size(75, 23);
            this.continueButton.TabIndex = 11;
            this.continueButton.Text = "Continue";
            this.continueButton.UseVisualStyleBackColor = true;
            this.continueButton.Click += new System.EventHandler(this.continueButton_Click);
            // 
            // stopPlaying
            // 
            this.stopPlaying.Location = new System.Drawing.Point(96, 138);
            this.stopPlaying.Name = "stopPlaying";
            this.stopPlaying.Size = new System.Drawing.Size(72, 23);
            this.stopPlaying.TabIndex = 12;
            this.stopPlaying.Text = "Stop";
            this.stopPlaying.UseVisualStyleBackColor = true;
            this.stopPlaying.Click += new System.EventHandler(this.stopPlaying_Click);
            // 
            // SelectFeatures
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(265, 172);
            this.Controls.Add(this.stopPlaying);
            this.Controls.Add(this.continueButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.sampleSixthSense);
            this.Controls.Add(this.sampleGui);
            this.Controls.Add(this.sampleCensoredIngameVoice);
            this.Controls.Add(this.sampleIngameVoice);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.selectYourFeatures);
            this.Controls.Add(this.sixthSenseBox);
            this.Controls.Add(this.guiBox);
            this.Controls.Add(this.relhaxCensoredBox);
            this.Controls.Add(this.relhaxBox);
            this.Name = "SelectFeatures";
            this.Text = "Select Features";
            this.Load += new System.EventHandler(this.SelectFeatures_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label selectYourFeatures;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button sampleIngameVoice;
        private System.Windows.Forms.Button sampleCensoredIngameVoice;
        private System.Windows.Forms.Button sampleGui;
        private System.Windows.Forms.Button sampleSixthSense;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button continueButton;
        private System.Windows.Forms.Button stopPlaying;
        public System.Windows.Forms.CheckBox relhaxBox;
        public System.Windows.Forms.CheckBox relhaxCensoredBox;
        public System.Windows.Forms.CheckBox guiBox;
        public System.Windows.Forms.CheckBox sixthSenseBox;
    }
}