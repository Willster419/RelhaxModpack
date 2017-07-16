namespace RelhaxModpack
{
    partial class DatabaseListGenerater
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
            this.LoadDatabaseButton = new System.Windows.Forms.Button();
            this.GenretateSpreadsheetButton = new System.Windows.Forms.Button();
            this.DatabaseLocation = new System.Windows.Forms.RichTextBox();
            this.SpreadsheetLocation = new System.Windows.Forms.RichTextBox();
            this.LoadDatabaseFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.SaveSpreadsheetFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.generateSpreadsheetUserButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // LoadDatabaseButton
            // 
            this.LoadDatabaseButton.Location = new System.Drawing.Point(100, 72);
            this.LoadDatabaseButton.Name = "LoadDatabaseButton";
            this.LoadDatabaseButton.Size = new System.Drawing.Size(87, 23);
            this.LoadDatabaseButton.TabIndex = 0;
            this.LoadDatabaseButton.Text = "load database";
            this.LoadDatabaseButton.UseVisualStyleBackColor = true;
            this.LoadDatabaseButton.Click += new System.EventHandler(this.LoadDatabaseButton_Click);
            // 
            // GenretateSpreadsheetButton
            // 
            this.GenretateSpreadsheetButton.Location = new System.Drawing.Point(69, 232);
            this.GenretateSpreadsheetButton.Name = "GenretateSpreadsheetButton";
            this.GenretateSpreadsheetButton.Size = new System.Drawing.Size(156, 23);
            this.GenretateSpreadsheetButton.TabIndex = 1;
            this.GenretateSpreadsheetButton.Text = "generate internal spreadsheet";
            this.GenretateSpreadsheetButton.UseVisualStyleBackColor = true;
            this.GenretateSpreadsheetButton.Click += new System.EventHandler(this.GenretateSpreadsheetButton_Click);
            // 
            // DatabaseLocation
            // 
            this.DatabaseLocation.Location = new System.Drawing.Point(12, 12);
            this.DatabaseLocation.Name = "DatabaseLocation";
            this.DatabaseLocation.ReadOnly = true;
            this.DatabaseLocation.Size = new System.Drawing.Size(266, 54);
            this.DatabaseLocation.TabIndex = 2;
            this.DatabaseLocation.Text = "";
            // 
            // SpreadsheetLocation
            // 
            this.SpreadsheetLocation.Location = new System.Drawing.Point(12, 101);
            this.SpreadsheetLocation.Name = "SpreadsheetLocation";
            this.SpreadsheetLocation.ReadOnly = true;
            this.SpreadsheetLocation.Size = new System.Drawing.Size(266, 125);
            this.SpreadsheetLocation.TabIndex = 3;
            this.SpreadsheetLocation.Text = "";
            // 
            // LoadDatabaseFileDialog
            // 
            this.LoadDatabaseFileDialog.FileName = "*.xml";
            this.LoadDatabaseFileDialog.Filter = "*.xml|*.xml";
            this.LoadDatabaseFileDialog.Title = "Load Database";
            // 
            // SaveSpreadsheetFileDialog
            // 
            this.SaveSpreadsheetFileDialog.FileName = "database.csv";
            this.SaveSpreadsheetFileDialog.Filter = "*.csv|*.csv";
            this.SaveSpreadsheetFileDialog.Title = "Save Database to CSV";
            // 
            // generateSpreadsheetUserButton
            // 
            this.generateSpreadsheetUserButton.Location = new System.Drawing.Point(75, 261);
            this.generateSpreadsheetUserButton.Name = "generateSpreadsheetUserButton";
            this.generateSpreadsheetUserButton.Size = new System.Drawing.Size(143, 23);
            this.generateSpreadsheetUserButton.TabIndex = 4;
            this.generateSpreadsheetUserButton.Text = "generate user spreadsheet";
            this.generateSpreadsheetUserButton.UseVisualStyleBackColor = true;
            this.generateSpreadsheetUserButton.Click += new System.EventHandler(this.generateSpreadsheetUserButton_Click);
            // 
            // DatabaseListGenerater
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(291, 293);
            this.Controls.Add(this.generateSpreadsheetUserButton);
            this.Controls.Add(this.SpreadsheetLocation);
            this.Controls.Add(this.DatabaseLocation);
            this.Controls.Add(this.GenretateSpreadsheetButton);
            this.Controls.Add(this.LoadDatabaseButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "DatabaseListGenerater";
            this.Text = "DatabaseListGenerater";
            this.Load += new System.EventHandler(this.DatabaseListGenerater_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button LoadDatabaseButton;
        private System.Windows.Forms.Button GenretateSpreadsheetButton;
        private System.Windows.Forms.RichTextBox DatabaseLocation;
        private System.Windows.Forms.RichTextBox SpreadsheetLocation;
        private System.Windows.Forms.OpenFileDialog LoadDatabaseFileDialog;
        private System.Windows.Forms.SaveFileDialog SaveSpreadsheetFileDialog;
        private System.Windows.Forms.Button generateSpreadsheetUserButton;
    }
}