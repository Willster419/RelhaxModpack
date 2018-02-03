namespace RelhaxModpack.Forms
{
    partial class BackgroundForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackgroundForm));
            this.RMIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.RelhaxMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItemRestore = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemCheckUpdates = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItemAppClose = new System.Windows.Forms.ToolStripMenuItem();
            this.RelhaxMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // RMIcon
            // 
            this.RMIcon.ContextMenuStrip = this.RelhaxMenuStrip;
            this.RMIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("RMIcon.Icon")));
            this.RMIcon.Text = "Relhax Modpack";
            this.RMIcon.Visible = true;
            this.RMIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.RMIcon_MouseDoubleClick);
            this.RMIcon.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RMIcon_MouseDown);
            // 
            // RelhaxMenuStrip
            // 
            this.RelhaxMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItemRestore,
            this.MenuItemCheckUpdates,
            this.MenuItemAppClose});
            this.RelhaxMenuStrip.Name = "RelhaxMenuStrip";
            this.RelhaxMenuStrip.Size = new System.Drawing.Size(172, 92);
            this.RelhaxMenuStrip.Text = "Test";
            // 
            // MenuItemRestore
            // 
            this.MenuItemRestore.Name = "MenuItemRestore";
            this.MenuItemRestore.Size = new System.Drawing.Size(171, 22);
            this.MenuItemRestore.Text = "Restore";
            this.MenuItemRestore.Click += new System.EventHandler(this.MenuItemRestore_Click);
            // 
            // MenuItemCheckUpdates
            // 
            this.MenuItemCheckUpdates.Name = "MenuItemCheckUpdates";
            this.MenuItemCheckUpdates.Size = new System.Drawing.Size(171, 22);
            this.MenuItemCheckUpdates.Text = "Check for Updates";
            this.MenuItemCheckUpdates.Click += new System.EventHandler(this.MenuItemCheckUpdates_Click);
            // 
            // MenuItemAppClose
            // 
            this.MenuItemAppClose.Name = "MenuItemAppClose";
            this.MenuItemAppClose.Size = new System.Drawing.Size(171, 22);
            this.MenuItemAppClose.Text = "Close";
            this.MenuItemAppClose.Click += new System.EventHandler(this.MenuItemAppClose_Click);
            // 
            // BackgroundForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(240, 42);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BackgroundForm";
            this.ShowInTaskbar = false;
            this.Text = "BackgroundForm";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.SizeChanged += new System.EventHandler(this.BackgroundForm_SizeChanged);
            this.RelhaxMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon RMIcon;
        private System.Windows.Forms.ContextMenuStrip RelhaxMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem MenuItemRestore;
        private System.Windows.Forms.ToolStripMenuItem MenuItemCheckUpdates;
        private System.Windows.Forms.ToolStripMenuItem MenuItemAppClose;
    }
}