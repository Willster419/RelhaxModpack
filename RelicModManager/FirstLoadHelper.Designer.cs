namespace RelicModManager
{
    partial class FirstLoadHelper
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FirstLoadHelper));
            this.helperText = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // helperText
            // 
            this.helperText.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.helperText.Location = new System.Drawing.Point(12, 12);
            this.helperText.Name = "helperText";
            this.helperText.ReadOnly = true;
            this.helperText.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedVertical;
            this.helperText.Size = new System.Drawing.Size(268, 119);
            this.helperText.TabIndex = 0;
            this.helperText.Text = resources.GetString("helperText.Text");
            // 
            // FirstLoadHelper
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 138);
            this.Controls.Add(this.helperText);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FirstLoadHelper";
            this.Text = "FirstLoadHelper";
            this.Load += new System.EventHandler(this.FirstLoadHelper_Load);
            this.ResumeLayout(false);

        }

        #endregion

        public System.Windows.Forms.RichTextBox helperText;

    }
}