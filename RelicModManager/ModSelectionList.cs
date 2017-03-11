using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Security.Cryptography;

namespace RelicModManager
{
    //the mod selectin window. allows users to select which mods they wish to install
    public partial class ModSelectionList : Form
    {
        public List<Catagory> parsedCatagoryList;//can be grabbed by MainWindow
        public List<Mod> userMods;//can be grabbed by MainWindow
        public bool cancel = true;//used to determine if the user canceled
        private Preview p = new Preview(null, null, null);
        private PleaseWait pw;
        public List<Dependency> globalDependencies;
        private bool loadingConfig = false;

        public ModSelectionList()
        {
            InitializeComponent();
        }
        //called on application startup
        private void ModSelectionList_Load(object sender, EventArgs e)
        {
            pw = new PleaseWait();
            pw.Show();
            this.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
            pw.loadingDescBox.Text = "Reading Database...";
            Application.DoEvents();
            this.createModStructure2();
            bool duplicates = this.duplicates();
            if (duplicates)
            {
                this.appendToLog("CRITICAL: Duplicate mod name detected!!");
                MessageBox.Show("CRITICAL: Duplicate mod name detected!!");
                Application.Exit();
            }
            pw.loadingDescBox.Text = "Building UI...";
            Application.DoEvents();
            this.initUserMods();
            //load the last selection if setting is set
            if (Settings.saveLastConfig && !Program.autoInstall)
            {
                this.loadConfig(false);
            }
            this.makeTabs();
            this.addAllMods();
            this.addUserMods();
            this.ModSelectionList_SizeChanged(null, null);
            this.Size = new Size(Settings.modSelectionWidth,Settings.modSelectionHeight);
            if (Program.autoInstall)
            {
                //automate the installation process
                //have it load the config
                //don't say prefs set
                //close
                this.loadConfig(false);
                this.cancel = false;
                pw.Close();
                this.Close();
            }
            pw.Close();
        }
        //initializes the userMods list. This should only be run once
        private void initUserMods()
        {
            //create all the user mod objects
            string modsPath = Application.StartupPath + "\\RelHaxUserMods";
            string[] userModFiles = Directory.GetFiles(modsPath);
            userMods = new List<Mod>();
            foreach (string s in userModFiles)
            {
                if (Path.GetExtension(s).Equals(".zip"))
                {
                    Mod m = new Mod();
                    m.modZipFile = s;
                    m.name = Path.GetFileNameWithoutExtension(s);
                    m.enabled = true;
                    userMods.Add(m);
                }
            }
        }
        //adds all usermods to thier own userMods tab
        private void addUserMods()
        {
            //make the new tab
            TabPage tb = new TabPage("User Mods");
            //add all mods to the tab page
            for (int i = 0; i < userMods.Count; i++)
            {
                //make modCheckBox
                CheckBox modCheckBox = new CheckBox();
                modCheckBox.AutoSize = true;
                int yLocation = 3 + (17 * (i));
                modCheckBox.Location = new System.Drawing.Point(3, yLocation);
                modCheckBox.Size = new System.Drawing.Size(49, 17);
                modCheckBox.TabIndex = 1;
                modCheckBox.Text = userMods[i].name;
                modCheckBox.Checked = userMods[i].modChecked;
                modCheckBox.UseVisualStyleBackColor = true;
                modCheckBox.Enabled = true;
                modCheckBox.CheckedChanged += new EventHandler(modCheckBox_CheckedChanged);
                tb.Controls.Add(modCheckBox);
            }
            modTabGroups.TabPages.Add(tb);
        }
        //adds all the tab pages for each catagory
        //must be only one catagory
        private void addAllMods()
        {
            foreach (TabPage t in this.modTabGroups.TabPages)
            {
                foreach (Catagory c in parsedCatagoryList)
                {
                    if (c.name.Equals(t.Name))
                    {
                        //matched the catagory to tab
                        //add to the ui every mod of that catagory
                        this.sortModsList(c.mods);
                        int i = 1;
                        foreach (Mod m in c.mods)
                        {
                            pw.loadingDescBox.Text = "Loading " + m.name;
                            Application.DoEvents();
                            this.addMod(m, t, i++);
                        }
                        break;
                    }
                }
            }
            loadingConfig = false;
        }
        //adds a tab view for each mod catagory
        private void makeTabs()
        {
            modTabGroups.TabPages.Clear();
            //this.sortCatagoryList(parsedCatagoryList);
            foreach (Catagory c in parsedCatagoryList)
            {
                TabPage t = new TabPage(c.name);
                t.AutoScroll = true;
                //link the names of catagory and tab so eithor can be searched for
                t.Name = c.name;
                //if the catagory selection type is only one mod allowed
                if (c.selectionType.Equals("single"))
                {
                    //append a star so the user knows
                    t.Text = t.Text + "*";
                }
                modTabGroups.TabPages.Add(t);
            }
        }
        //adds a mod m to a tabpage t
        private void addMod(Mod m, TabPage t, int panelCount)
        {
            //make config panel
            Panel configPanel = new Panel();
            configPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            configPanel.Location = new System.Drawing.Point(3, 26);
            configPanel.TabIndex = 2;
            configPanel.AutoSize = true;
            configPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            configPanel.Size = new System.Drawing.Size(t.Size.Width - 35, 30);
            configPanel.Controls.Clear();
            //add configs to the panel
            //create the comboBox outside of the loop
            //later add it if the items count is above 0
            ComboBox configControlDD = new ComboBox();
            configControlDD.AutoSize = true;
            configControlDD.Location = new System.Drawing.Point(0, 0);
            configControlDD.Size = new System.Drawing.Size(150, 15);
            configControlDD.TabIndex = 1;
            configControlDD.TabStop = true;
            configControlDD.Enabled = false;
            configControlDD.SelectedIndexChanged += new EventHandler(configControlDD_SelectedIndexChanged);
            configControlDD.Name = t.Name + "_" + m.name + "_DropDown";
            configControlDD.DropDownStyle = ComboBoxStyle.DropDownList;
            configControlDD.Items.Clear();
            //label for the dropdown selection. shows the mod size
            //and the eta download time (if applicable)
            Label dropDownSizeLabel = new Label();
            dropDownSizeLabel.Location = new Point(0, 0);
            for (int i = 0; i < m.configs.Count; i++)
            {
                int yPosition = 15 * (i + 1);
                //make configLabel if config type is not single_dropDown
                Label configLabel = new Label();
                if (!m.configs[i].type.Equals("single_dropdown"))
                {
                    configLabel.AutoSize = true;
                    configLabel.Location = new System.Drawing.Point(5, yPosition - 10);
                    configLabel.Size = new System.Drawing.Size(100, 15);
                    configLabel.TabIndex = 0;
                    configLabel.Text = m.configs[i].name;
                    configLabel.Name = t.Name + "_" + m.name + "_" + m.configs[i].name;
                    configLabel.Enabled = false;
                    configPanel.Controls.Add(configLabel);
                }
                switch (m.configs[i].type)
                {
                    case "single":
                        //make a radioButton
                        RadioButton configControlRB = new RadioButton();
                        configControlRB.AutoSize = true;
                        //configControlRB.Location = new System.Drawing.Point(100, yPosition - 10);
                        configControlRB.Location = new System.Drawing.Point(configLabel.Location.X + configLabel.Size.Width + 6, yPosition - 10);
                        configControlRB.Size = new System.Drawing.Size(150, 15);
                        configControlRB.TabIndex = 1;
                        configControlRB.TabStop = true;
                        configControlRB.Enabled = false;
                        configControlRB.Checked = m.configs[i].configChecked;
                        configControlRB.CheckedChanged += new EventHandler(configControlRB_CheckedChanged);
                        configControlRB.Name = t.Name + "_" + m.name + "_" + m.configs[i].name;
                        //Check if a mod zip exists locally and if it's out of date
                        string configDownloadPath = Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile;
                        if (File.Exists(configDownloadPath))
                        {
                            MD5 hash = MD5.Create();
                            string oldCRC = this.GetMd5Hash(hash, configDownloadPath);
                            if (!oldCRC.Equals(m.configs[i].crc))
                            {
                                //the file exists, but it is out of date
                                configControlRB.Text = configControlRB.Text + "(Updated)";
                                if (m.configs[i].size > 0.0f)
                                {
                                    int totalSec = (int)m.configs[i].size * 2;
                                    int totalRealMin = totalSec / 60;
                                    int totalRealSec = totalSec % 60;
                                    configControlRB.Text = configControlRB.Text + " (" + m.configs[i].size + " MB, ~" + totalRealMin + " min " + totalRealSec + " sec)";
                                }
                            }
                        }
                        else if (!(File.Exists(configDownloadPath)) && (m.configs[i].crc != null) && (!m.configs[i].crc.Equals("")))
                        {
                            //file does not exist, but a crc does. File is not download.
                            configControlRB.Text = configControlRB.Text + "(Updated)";
                            if (m.configs[i].size > 0.0f)
                            {
                                int totalSec = (int)m.configs[i].size * 2;
                                int totalRealMin = totalSec / 60;
                                int totalRealSec = totalSec % 60;
                                configControlRB.Text = configControlRB.Text + " (" + m.configs[i].size + " MB, ~" + totalRealMin + " min " + totalRealSec + " sec)";
                            }
                        }
                        configPanel.Controls.Add(configControlRB);
                        break;

                    case "single_dropdown":
                        //make a dropDown selection box
                        if (configControlDD.Location.X == 0 && configControlDD.Location.Y == 0)
                        {
                            //this has been activated
                            configControlDD.Location = new System.Drawing.Point(configLabel.Location.X + configLabel.Size.Width + 6, yPosition - 10);
                            configPanel.Controls.Add(configControlDD);
                        }
                        string configDownloadPath1 = Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile;
                        if (File.Exists(configDownloadPath1))
                        {
                            MD5 hash = MD5.Create();
                            string oldCRC = this.GetMd5Hash(hash, configDownloadPath1);
                            if (!oldCRC.Equals(m.configs[i].crc))
                            {
                                if (m.configs[i].enabled) configControlDD.Items.Add(m.configs[i].name + "_updated");
                                if (m.configs[i].configChecked) configControlDD.SelectedItem = m.configs[i].name + "_updated";
                                break;
                            }
                        }
                        else if (!(File.Exists(configDownloadPath1)) && (m.configs[i].crc != null) && (!m.configs[i].crc.Equals("")))
                        {
                            //mod/config zip file does not exist locally, but a crc for it does, implying that the file needs to be downloaded
                            if (m.configs[i].enabled) configControlDD.Items.Add(m.configs[i].name + "_updated");
                            if (m.configs[i].configChecked) configControlDD.SelectedItem = m.configs[i].name + "_updated";
                            break;
                        }
                        if (m.configs[i].enabled) configControlDD.Items.Add(m.configs[i].name);
                        if (m.configs[i].configChecked) configControlDD.SelectedItem = m.configs[i].name;
                        break;

                    case "multi":
                        //make a checkBox
                        CheckBox configControlCB = new CheckBox();
                        configControlCB.AutoSize = true;
                        //configControlCB.Location = new System.Drawing.Point(100, yPosition - 10);
                        configControlCB.Location = new Point(configLabel.Location.X + configLabel.Size.Width + 6, yPosition - 10);
                        configControlCB.Size = new System.Drawing.Size(150, 15);
                        configControlCB.TabIndex = 1;
                        configControlCB.TabStop = true;
                        //the handler for modCheckBox should take care of the enabled stuff
                        configControlCB.Enabled = false;
                        configControlCB.Checked = m.configs[i].configChecked;
                        configControlCB.CheckedChanged += new EventHandler(configControlCB_CheckedChanged);
                        configControlCB.Name = t.Name + "_" + m.name + "_" + m.configs[i].name;
                        string configDownloadPath2 = Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile;
                        if (File.Exists(configDownloadPath2))
                        {
                            MD5 hash = MD5.Create();
                            string oldCRC = this.GetMd5Hash(hash, configDownloadPath2);
                            if (!oldCRC.Equals(m.configs[i].crc))
                            {
                                configControlCB.Text = configControlCB.Text + "(Updated)";
                                if (m.configs[i].size > 0.0f)
                                {
                                    int totalSec = (int)m.configs[i].size * 2;
                                    int totalRealMin = totalSec / 60;
                                    int totalRealSec = totalSec % 60;
                                    configControlCB.Text = configControlCB.Text + " (" + m.configs[i].size + " MB, ~" + totalRealMin + " min " + totalRealSec + " sec)";
                                }
                            }
                        }
                        else if (!(File.Exists(configDownloadPath2)) && (m.configs[i].crc != null) && (!m.configs[i].crc.Equals("")))
                        {
                            configControlCB.Text = configControlCB.Text + "(Updated)";
                            if (m.configs[i].size > 0.0f)
                            {
                                int totalSec = (int)m.configs[i].size * 2;
                                int totalRealMin = totalSec / 60;
                                int totalRealSec = totalSec % 60;
                                configControlCB.Text = configControlCB.Text + " (" + m.configs[i].size + " MB, ~" + totalRealMin + " min " + totalRealSec + " sec)";
                            }
                        }
                        configPanel.Controls.Add(configControlCB);
                        break;
                }
                if (m.configs[i].type.Equals("single_dropdown") && dropDownSizeLabel.Location.X == 0 && dropDownSizeLabel.Location.Y == 0)
                {
                    //add the label with text nothing selected
                    dropDownSizeLabel.AutoSize = true;
                    dropDownSizeLabel.Location = new Point(configControlDD.Location.X + configControlDD.Size.Width + 6, configControlDD.Location.Y + 3);
                    dropDownSizeLabel.TabIndex = 0;
                    dropDownSizeLabel.Text = "Nothing Selected";
                    dropDownSizeLabel.Name = t.Name + "_" + m.name + "_size";
                    dropDownSizeLabel.Enabled = false;
                    configPanel.Controls.Add(dropDownSizeLabel);
                }
            }
            //make the mod check box
            CheckBox modCheckBox = new CheckBox();
            modCheckBox.AutoSize = true;
            modCheckBox.Location = new System.Drawing.Point(3, 3);
            modCheckBox.Size = new System.Drawing.Size(49, 15);
            modCheckBox.TabIndex = 1;
            modCheckBox.Text = m.name;
            modCheckBox.Name = t.Name + "_" + m.name;
            string modDownloadPath = Application.StartupPath + "\\RelHaxDownloads\\" + m.modZipFile;
            if (File.Exists(modDownloadPath))
            {
                MD5 hash = MD5.Create();
                string oldCRC = this.GetMd5Hash(hash, modDownloadPath);
                if (!oldCRC.Equals(m.crc))
                {
                    modCheckBox.Text = modCheckBox.Text + "(Updated)";
                    if (m.size > 0.0f)
                    {
                        int totalSec = (int)m.size * 2;
                        int totalRealMin = totalSec / 60;
                        int totalRealSec = totalSec % 60;
                        modCheckBox.Text = modCheckBox.Text + " (" + m.size + " MB, ~" + totalRealMin + " min " + totalRealSec + " sec)";
                    }
                }
            }
            else if (!(File.Exists(modDownloadPath)) && (m.crc != null) && (!m.crc.Equals("")))
            {
                //get the file size as well
                //float downloadSizeMB = this.netFileSize(downloadURL + m.modZipFile);
                //downloadSizeMB = (float)Math.Round(downloadSizeMB, 1);
                //modCheckBox.Text = modCheckBox.Text + " (" + downloadSizeMB + " MB, ~" + (downloadSizeMB * 2) + " sec)";
                modCheckBox.Text = modCheckBox.Text + "(Updated)";
                if (m.size > 0.0f)
                {
                    int totalSec = (int)m.size * 2;
                    int totalRealMin = totalSec / 60;
                    int totalRealSec = totalSec % 60;
                    modCheckBox.Text = modCheckBox.Text + " (" + m.size + " MB, ~" + totalRealMin + " min " + totalRealSec + " sec)";
                }
            }
            modCheckBox.UseVisualStyleBackColor = true;
            modCheckBox.Enabled = m.enabled;
            modCheckBox.MouseDown += new MouseEventHandler(modCheckBox_MouseDown);
            //in theory it should trigger the handler for checked
            //when initially made it should be false, if enabled from
            //from user configs
            configPanel.Location = new Point(configPanel.Location.X, modCheckBox.Size.Height + 15);
            //make the main panel
            Panel mainPanel = new Panel();
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            mainPanel.Controls.Add(configPanel);
            mainPanel.Controls.Add(modCheckBox);
            int panelCountYLocation = 70 * (panelCount - 1);
            //if this is not the first mod being added to the panel
            int panelYLocation = 6; //tab plus delimiter
            if (panelCount > 1)
            {
                //create a list of other controlls and put this one 6 pixels below the others
                foreach (Control c in t.Controls)
                {
                    panelYLocation += c.Size.Height;
                    panelYLocation += 6;
                }
                panelCountYLocation = (panelCount - 1) * (t.Controls[0].Size.Height);
                panelCountYLocation = panelCountYLocation + 5;
            }
            mainPanel.Location = new System.Drawing.Point(5, panelYLocation);
            mainPanel.TabIndex = 0;
            mainPanel.AutoSize = true;
            mainPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
            mainPanel.Size = new System.Drawing.Size(t.Size.Width - 25, 20);
            //add to main panel
            mainPanel.Controls.Clear();
            mainPanel.Controls.Add(modCheckBox);
            if (m.configs.Count > 0)
                mainPanel.Controls.Add(configPanel);
            //add to tab
            t.Controls.Add(mainPanel);
            //add the event handler before changing the checked state so the event
            //event handler is #triggered
            modCheckBox.CheckedChanged += new EventHandler(modCheckBox_CheckedChanged);
            modCheckBox.Checked = m.modChecked;
            if (loadingConfig)
            {
                //trigger the handler
                this.configControlDD_SelectedIndexChanged(configControlDD, null);
            }
        }
        //hander for when any mouse button is clicked on a specific control
        void modCheckBox_MouseDown(object sender, MouseEventArgs e)
        {
            //we only care about the right mouse click tho
            if (e.Button != MouseButtons.Right)
                return;
            if (sender is CheckBox)
            {
                CheckBox cb = (CheckBox)sender;
                Mod m = this.linkMod(cb.Name.Split('_')[1]);
                string name = m.name;
                //get the mod and/or config
                List<Picture> picturesList = this.sortPictureList(m.picList);
                string desc = m.description;
                string updateNotes = m.updateComment;
                string devurl = m.devURL;
                if (devurl == null)
                    devurl = "";
                p.Close();
                p = new Preview(name, picturesList, desc, updateNotes, devurl);
                p.Show();
            }
        }
        //handler for when a config selection is made from the drop down list
        void configControlDD_SelectedIndexChanged(object sender, EventArgs e)
        {
            //uncheck all other dorp down configs
            ComboBox cb = (ComboBox)sender;
            //get the mod this config is associated with
            //this is safe because it will never be a user mod
            string catagory = cb.Name.Split('_')[0];
            string mod = cb.Name.Split('_')[1];
            Mod m = this.getCatagory(catagory).getMod(mod);
            string configName = (string)cb.SelectedItem;
            if (configName == null)
                return;
            //in case "_updated" was appended, split the string
            string[] splitConfigName = configName.Split('_');
            configName = splitConfigName[0];
            string updateString = null;
            //TODO: find out what this is
            if (splitConfigName.Count() > 1)
                updateString = splitConfigName[1];
            Config save = null;
            foreach (Config c in m.configs)
            {
                //verify it is on drodown type
                if (c.type.Equals("single_dropdown"))
                {
                    //uncheck it
                    c.configChecked = false;
                    //unless it's the one the user selected
                    if (configName.Equals(c.name))
                    {
                        c.configChecked = true;
                        save = c;
                    }
                }
            }
            //set the text of the selected label
            Panel configPanel = (Panel)cb.Parent;
            if (configPanel == null)
                return;
            foreach (Control c in configPanel.Controls)
            {
                if (c is Label)
                {
                    if (c.Name.Equals(catagory + "_" + mod + "_size"))
                    {
                        if (updateString != null)
                        {
                            int totalSec = (int)save.size * 2;
                            int totalRealMin = totalSec / 60;
                            int totalRealSec = totalSec % 60;
                            c.Text = save.size + " MB, ~" + totalRealMin + " min " + totalRealSec + " sec)";
                        }
                        else
                            c.Text = "";
                    }
                }
            }
        }
        //handler for when the config checkbox is checked or unchecked
        void configControlCB_CheckedChanged(object sender, EventArgs e)
        {
            //checkboxes don't need to be unselected
            CheckBox cb = (CheckBox)sender;
            string modName = cb.Name.Split('_')[1];
            string catagoryName = cb.Name.Split('_')[0];
            string configName = cb.Name.Split('_')[2];
            Mod m = this.linkMod(modName, catagoryName);
            foreach (Config cc in m.configs)
            {
                //multi part is not required, but enforces config types for good pratice
                if (configName.Equals(cc.name) && cc.type.Equals("multi"))
                {
                    cc.configChecked = cb.Checked;
                }
            }
        }
        //handler for when a config radioButton is pressed
        void configControlRB_CheckedChanged(object sender, EventArgs e)
        {
            //uncheck all other radioButton mods
            RadioButton rb = (RadioButton)sender;
            string modName = rb.Name.Split('_')[1];
            string catagoryName = rb.Name.Split('_')[0];
            string configName = rb.Name.Split('_')[2];
            Mod m = this.linkMod(modName, catagoryName);
            foreach (Config cc in m.configs)
            {
                //verify only unchecking radiobutton ("single") configs
                if (cc.type.Equals("single"))
                {
                    cc.configChecked = false;
                    //unless it's the one the user just selected
                    if (configName.Equals(cc.name))
                    {
                        //enable that config for that mod in memory
                        cc.configChecked = rb.Checked;
                    }
                }
            }
        }
        //handler for when a mod checkbox is changed
        void modCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            //check to see if it's the User Mods page or not
            //i don't think this will ever be run TBH
            CheckBox cbUser = (CheckBox)sender;
            if (cbUser.Parent is TabPage)
            {
                TabPage t = (TabPage)cbUser.Parent;
                if (t.Text.Equals("User Mods"))
                {
                    //this is a check from the user checkboxes
                    Mod m = this.getUserMod(cbUser.Text);
                    if (m != null)
                        m.modChecked = cbUser.Checked;
                    return;
                }
            }
            //get all required info for this checkbox change
            CheckBox cb = (CheckBox)sender;
            Panel p = (Panel)cb.Parent;
            string modName = cb.Name.Split('_')[1];
            string catagoryName = cb.Name.Split('_')[0];

            //check to see if the mod is part of a single selection only catagory
            //if it is uncheck the other mods first. this for loop only deals with mods
            foreach (Catagory c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.name.Equals(modName))
                    {
                        //mod linked
                        if (c.selectionType.Equals("single"))
                        {
                            if (!cb.Checked)
                            {
                                m.modChecked = false;
                            }
                            else
                            {
                                //check if any other mods in this catagory are already checked
                                bool anyModsChecked = false;
                                foreach (Mod mm in c.mods)
                                {
                                    if (mm.modChecked)
                                        anyModsChecked = true;
                                }
                                if (!anyModsChecked)
                                {
                                    //save to check the mod
                                    m.modChecked = cb.Checked;
                                }
                                else
                                {
                                    if (!loadingConfig)
                                    {
                                    //not safe to check the mod
                                    //uncheck the other mod first
                                    //each checkbox uncheck it
                                    Panel modPanel = (Panel)cb.Parent;
                                    TabPage modTab = (TabPage)modPanel.Parent;
                                    foreach (var cc in modTab.Controls)
                                    {
                                        if (cc is Panel)
                                        {
                                            Panel pp = (Panel)cc;
                                            foreach (var ccc in pp.Controls)
                                            {
                                                if (ccc is CheckBox)
                                                {
                                                    CheckBox cbb = (CheckBox)ccc;
                                                    //check that this makes the mod object false as well
                                                    //cbb.CheckedChanged -= modCheckBox_CheckedChanged;
                                                    cbb.Checked = false;
                                                    //cbb.CheckedChanged += modCheckBox_CheckedChanged;
                                                    string otherModName = cbb.Name.Split('_')[1];
                                                    string otherCatagoryName = cbb.Name.Split('_')[0];
                                                    Mod otherMod = this.linkMod(otherModName, otherCatagoryName);
                                                    otherMod.modChecked = false;
                                                }
                                            }
                                        }
                                    }
                                    //now it's safe to check the mods
                                    
                                        
                                        m.modChecked = true;
                                        cb.CheckedChanged -= modCheckBox_CheckedChanged;
                                        cb.Checked = true;
                                        cb.CheckedChanged += modCheckBox_CheckedChanged;
                                        //cb.Checked = false;
                                    }
                                    
                                }
                            }
                        }
                    }
                }
            }
            //this loop deals with enabling the config(s) with the mod, if there are any
            foreach (Catagory c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.name.Equals(modName))
                    {
                        //enable the mod in memory
                        m.modChecked = cb.Checked;
                        //update configs if they exist
                        if (p.Controls.Count > 1)
                        {
                            Panel innerPanel = (Panel)p.Controls[1];
                            if (cb.Checked) innerPanel.BackColor = Color.BlanchedAlmond;
                            else innerPanel.BackColor = SystemColors.Control;
                            foreach (Control cc in innerPanel.Controls)
                            {
                                foreach (Config ccc in m.configs)
                                {
                                    if (cc is ComboBox)
                                    {
                                        cc.Enabled = m.modChecked;
                                        break;
                                    }
                                    if (cc.Name.Equals(c.name + "_" + m.name + "_" + ccc.name))
                                    {
                                        //for the checkboxes
                                        if (ccc.enabled && m.enabled && cb.Checked)
                                        {
                                            //enable the control
                                            cc.Enabled = true;
                                        }
                                        else
                                        {
                                            cc.Enabled = false;
                                        }
                                    }
                                    //if contgrol text = config name
                                    if (cc.Text.Equals(ccc.name))
                                    {
                                        //for the lables
                                        cc.Enabled = cb.Checked;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            //dealing with config stuffs
            //if the mod checkbox was changed to checked state
            //and there are configs
            if (p.Controls.Count > 1)
            {
                //at lease one config exists
                Panel innerPanel = (Panel)p.Controls[1];
                if (cb.Checked) innerPanel.BackColor = Color.BlanchedAlmond;
                else innerPanel.BackColor = SystemColors.Control;
                if (cb.Checked)
                {
                    //check to make sure at least one config is selected
                    bool oneSelected = false;
                    foreach (Control c in innerPanel.Controls)
                    {
                        if (c is Label)
                        {
                            if (c.Name.Equals(catagoryName + "_" + modName + "_size"))
                            {
                                c.Enabled = true;
                            }
                        }
                        if (c is RadioButton)
                        {
                            RadioButton b = (RadioButton)c;
                            if (b.Checked)
                            {
                                oneSelected = true;
                            }
                        }

                        if (c is CheckBox)
                        {
                            CheckBox b = (CheckBox)c;
                            if (b.Checked)
                            {
                                oneSelected = true;
                            }
                        }

                        if (c is ComboBox)
                        {
                            ComboBox cbox = (ComboBox)c;
                            if (cbox.SelectedIndex == -1) oneSelected = false;
                        }
                    }
                    if (!oneSelected)
                    {
                        //enable at least one radioButton and one comboBox selection
                        bool radioSelected = false;
                        foreach (Control c in innerPanel.Controls)
                        {
                            if (c is RadioButton)
                            {
                                RadioButton b = (RadioButton)c;
                                if (b.Enabled)
                                {
                                    if (radioSelected) continue;
                                    b.Checked = true;
                                    radioSelected = true;
                                }
                            }
                            if (c is ComboBox)
                            {
                                ComboBox cbox = (ComboBox)c;
                                //only select if the index is on nothing
                                if (cbox.SelectedIndex == -1)
                                    cbox.SelectedIndex = 0;
                            }
                        }
                    }
                }
                else
                {
                    foreach (Control c in innerPanel.Controls)
                    {
                        if (c is Label)
                        {
                            if (c.Name.Equals(catagoryName + "_" + modName + "_size"))
                            {
                                c.Enabled = false;
                            }
                        }
                    }
                }
            }
        }
        //parses the xml mod info into the memory database
        private void createModStructure2()
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                if (Program.testMode)
                {
                    doc.Load("modInfo.xml");
                }
                else
                {
                    doc.Load("http://willster419.atwebpages.com/Applications/RelHaxModPack/modInfo.xml");
                }
            }
            catch (XmlException)
            {
                Settings.appendToLog("CRITICAL: Failed to read database!");
                MessageBox.Show("CRITICAL: Failed to read database!");
                Application.Exit();
            }
            //add the global dependencies
            globalDependencies = new List<Dependency>();
            XmlNodeList globalDependenciesList = doc.SelectNodes("//modInfoAlpha.xml/globaldependencies/globaldependency");
            foreach (XmlNode dependencyNode in globalDependenciesList)
            {
                Dependency d = new Dependency();
                foreach (XmlNode globs in dependencyNode.ChildNodes)
                {
                    switch (globs.Name)
                    {
                        case "dependencyZipFile":
                            d.dependencyZipFile = globs.InnerText;
                            break;
                        case "dependencyZipCRC":
                            d.dependencyZipCRC = globs.InnerText;
                            break;
                        case "dependencyenabled":
                            try
                            {
                                d.enabled = bool.Parse(globs.InnerText);
                            }
                            catch (FormatException)
                            {
                                d.enabled = false;
                            }
                            break;
                    }
                }
                globalDependencies.Add(d);
            }
            
            XmlNodeList catagoryList = doc.SelectNodes("//modInfoAlpha.xml/catagories/catagory");
            parsedCatagoryList = new List<Catagory>();
            foreach (XmlNode nnnnn in catagoryList)
            {
                Catagory cat = new Catagory();
                foreach (XmlNode nnnnnn in nnnnn.ChildNodes)
                {
                    switch (nnnnnn.Name)
                    {
                        case "name":
                            cat.name = nnnnnn.InnerText;
                            break;
                        case "selectionType":
                            cat.selectionType = nnnnnn.InnerText;
                            break;
                        case "mods":
                            foreach (XmlNode n in nnnnnn.ChildNodes)
                            {
                                Mod m = new Mod();
                                foreach (XmlNode nn in n.ChildNodes)
                                {
                                    switch (nn.Name)
                                    {
                                        case "name":
                                            m.name = nn.InnerText;
                                            break;
                                        case "version":
                                            m.version = nn.InnerText;
                                            break;
                                        case "size":
                                            try
                                            {
                                                m.size = float.Parse(nn.InnerText);
                                            }
                                            catch (FormatException)
                                            {
                                                m.size = (float)0.0;
                                            }
                                            break;
                                        case "modzipfile":
                                            m.modZipFile = nn.InnerText;
                                            break;
                                        case "modzipcrc":
                                            m.crc = nn.InnerText;
                                            break;
                                        case "enabled":
                                            try
                                            {
                                                m.enabled = bool.Parse(nn.InnerText);
                                            }
                                            catch (FormatException)
                                            {
                                                m.enabled = false;
                                            }
                                            break;
                                        case "description":
                                            m.description = nn.InnerText;
                                            break;
                                        case "updateComment":
                                            m.updateComment = nn.InnerText;
                                            break;
                                        case "devURL":
                                            m.devURL = nn.InnerText;
                                            break;
                                        case "pictures":
                                            //parse every picture
                                            foreach (XmlNode nnnnnnn in nn.ChildNodes)
                                            {
                                                foreach (XmlNode nnnnnnnn in nnnnnnn.ChildNodes)
                                                {
                                                    switch (nnnnnnnn.Name)
                                                    {
                                                        case "URL":
                                                            string innerText = nnnnnnnn.InnerText;
                                                            if (innerText == null)
                                                                continue;
                                                            if (innerText.Equals(""))
                                                                continue;
                                                            m.picList.Add(new Picture("Mod: " + m.name, nnnnnnnn.InnerText));
                                                            break;
                                                    }
                                                }
                                            }
                                            break;
                                        case "configs":
                                            //parse every config for that mod
                                            foreach (XmlNode nnn in nn.ChildNodes)
                                            {
                                                Config c = new Config();
                                                foreach (XmlNode nnnn in nnn.ChildNodes)
                                                {
                                                    switch (nnnn.Name)
                                                    {
                                                        case "name":
                                                            c.name = nnnn.InnerText;
                                                            break;
                                                        case "configzipfile":
                                                            c.zipConfigFile = nnnn.InnerText;
                                                            break;
                                                        case "configzipcrc":
                                                            c.crc = nnnn.InnerText;
                                                            break;
                                                        case "configenabled":
                                                            try
                                                            {
                                                                c.enabled = bool.Parse(nnnn.InnerText);
                                                            }
                                                            catch (FormatException)
                                                            {
                                                                c.enabled = false;
                                                            }
                                                            break;
                                                        case "size":
                                                            try
                                                            {
                                                                c.size = float.Parse(nnnn.InnerText);
                                                            }
                                                            catch (FormatException)
                                                            {
                                                                c.size = (float)0.0;
                                                            }
                                                            break;
                                                        case "configtype":
                                                            c.type = nnnn.InnerText;
                                                            break;
                                                        case "pictures":
                                                            //parse every picture
                                                            foreach (XmlNode nnnnnnnn in nnnn.ChildNodes)
                                                            {
                                                                foreach (XmlNode nnnnnnnnnn in nnnnnnnn.ChildNodes)
                                                                {
                                                                    switch (nnnnnnnnnn.Name)
                                                                    {
                                                                        case "URL":
                                                                            string innerText = nnnnnnnn.InnerText;
                                                                            if (innerText == null)
                                                                                continue;
                                                                            if (innerText.Equals(""))
                                                                                continue;
                                                                            m.picList.Add(new Picture("Config: " + c.name, nnnnnnnn.InnerText));
                                                                            break;
                                                                    }
                                                                }
                                                            }
                                                            break;
                                                    }
                                                }
                                                m.configs.Add(c);
                                            }
                                            break;
                                    }
                                }
                                cat.mods.Add(m);
                            }
                            break;
                        case "dependencies":
                            //parse every config for that mod
                            foreach (XmlNode nnn in nnnnnn.ChildNodes)
                            {
                                Dependency d = new Dependency();
                                foreach (XmlNode nnnn in nnn.ChildNodes)
                                {
                                    switch (nnnn.Name)
                                    {
                                        case "dependencyZipFile":
                                            d.dependencyZipFile = nnnn.InnerText;
                                            break;
                                        case "dependencyZipCRC":
                                            d.dependencyZipCRC = nnnn.InnerText;
                                            break;
                                        case "dependencyenabled":
                                            try
                                            {
                                                d.enabled = bool.Parse(nnnn.InnerText);
                                            }
                                            catch (FormatException)
                                            {
                                                d.enabled = false;
                                            }
                                            break;
                                    }
                                }
                                cat.dependencies.Add(d);
                            }
                            break;
                    }
                }
                parsedCatagoryList.Add(cat);
            }

        }
        //resizing handler for the window
        private void ModSelectionList_SizeChanged(object sender, EventArgs e)
        {
            continueButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height);
            cancelButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height);
            modTabGroups.Size = new Size(this.Size.Width - 20 - modTabGroups.Location.X, this.Size.Height - modTabGroups.Location.Y - 39 - continueButton.Size.Height - 6);
            label1.Text = "" + this.Size.Width + " x " + this.Size.Height;
            loadConfigButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width - 6 - loadConfigButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height);
            saveConfigButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height);
            clearSelectionsButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width - 6 - loadConfigButton.Size.Width - 6 - clearSelectionsButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height);
            if (this.Size.Height < 250) this.Size = new Size(this.Size.Width, 250);
            if (this.Size.Width < 520) this.Size = new Size(520, this.Size.Height);
            foreach (TabPage t in modTabGroups.TabPages)
            {
                foreach (Control c in t.Controls)
                {
                    if (c is Panel)
                    {
                        Panel p = (Panel)c;
                        p.Size = new Size(t.Size.Width - 25, p.Size.Height);
                        foreach (Control cc in p.Controls)
                        {
                            if (cc is Panel)
                            {
                                Panel pp = (Panel)cc;
                                pp.Size = new Size(t.Size.Width - 35, pp.Size.Height);
                            }
                        }
                    }
                }
            }
        }
        //handler to set the cancel bool to false
        private void continueButton_Click(object sender, EventArgs e)
        {
            cancel = false;
            //save the last config if told to do so
            if (Settings.saveLastConfig)
            {
                this.saveConfig(false);
            }
            this.Close();
        }
        //handler for when the cancal button is clicked
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //returns the mod based on catagory and mod name
        private Mod linkMod(string modName, string catagoryName)
        {
            foreach (Catagory c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (c.name.Equals(catagoryName) && m.name.Equals(modName))
                    {
                        //found it
                        return m;
                    }
                }
            }
            return null;
        }
        //returns the mod based and mod name
        private Mod linkMod(string modName)
        {
            foreach (Catagory c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.name.Equals(modName))
                    {
                        //found it
                        return m;
                    }
                }
            }
            return null;
        }
        //returns the catagory based on the catagory name
        private Catagory getCatagory(string catName)
        {
            foreach (Catagory c in parsedCatagoryList)
            {
                if (c.name.Equals(catName)) return c;
            }
            return null;
        }
        //gets the user mod based on it's name
        private Mod getUserMod(string modName)
        {
            foreach (Mod m in userMods)
            {
                if (m.name.Equals(modName))
                {
                    return m;
                }
            }
            return null;
        }
        //logs string info to the log output
        private void appendToLog(string info)
        {
            //the method should automaticly make the file if it's not there
            File.AppendAllText(Application.StartupPath + "\\RelHaxLog.txt", info + "\n");
        }
        //saves the currently checked configs and mods
        private void saveConfig(bool fromButton)
        {
            //dialog box to ask where to save the config to
            SaveFileDialog saveLocation = new SaveFileDialog();
            saveLocation.AddExtension = true;
            saveLocation.DefaultExt = ".xml";
            saveLocation.Filter = "*.xml|*.xml";
            saveLocation.InitialDirectory = Application.StartupPath + "\\RelHaxUserConfigs";
            saveLocation.Title = "Select where to save user prefs";
            if (fromButton)
            {
                if (saveLocation.ShowDialog().Equals(DialogResult.Cancel))
                {
                    //cancel
                    return;
                }
            }
            string savePath = saveLocation.FileName;
            if (Settings.saveLastConfig && !fromButton)
            {
                savePath = Application.StartupPath + "\\RelHaxUserConfigs\\lastInstalledConfig.xml";
                Settings.appendToLog("Save last config checked, saving to " + savePath);
            }
            //XmlDocument save time!
            XmlDocument doc = new XmlDocument();
            //mods root
            XmlElement modsHolderBase = doc.CreateElement("mods");
            doc.AppendChild(modsHolderBase);
            //relhax mods root
            XmlElement modsHolder = doc.CreateElement("relhaxMods");
            modsHolderBase.AppendChild(modsHolder);
            //user mods root
            XmlElement userModsHolder = doc.CreateElement("userMods");
            modsHolderBase.AppendChild(userModsHolder);
            //check every mod
            foreach (Catagory c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.modChecked)
                    {
                        //add it to the list
                        XmlElement mod = doc.CreateElement("mod");
                        modsHolder.AppendChild(mod);
                        XmlElement modName = doc.CreateElement("name");
                        modName.InnerText = m.name;
                        mod.AppendChild(modName);
                        if (m.configs.Count > 0)
                        {
                            XmlElement configsHolder = doc.CreateElement("configs");
                            mod.AppendChild(configsHolder);
                            foreach (Config cc in m.configs)
                            {
                                if (cc.configChecked)
                                {
                                    //add the config to the list
                                    XmlElement config = doc.CreateElement("config");
                                    configsHolder.AppendChild(config);
                                    XmlElement configName = doc.CreateElement("name");
                                    configName.InnerText = cc.name;
                                    config.AppendChild(configName);
                                }
                            }
                        }
                    }
                }
            }
            //check user mods
            foreach (Mod m in userMods)
            {
                if (m.modChecked)
                {
                    //add it to the list
                    XmlElement mod = doc.CreateElement("mod");
                    modsHolder.AppendChild(mod);
                    XmlElement modName = doc.CreateElement("name");
                    modName.InnerText = m.name;
                    mod.AppendChild(modName);
                    userModsHolder.AppendChild(mod);
                }
            }
            doc.Save(savePath);
            if (fromButton)
            {
                MessageBox.Show("Config Saved Sucessfully");
            }
        }
        //loads a saved config from xml and parses it into the memory database
        private void loadConfig(bool fromButton)
        {
            loadingConfig = true;
            OpenFileDialog loadLocation = new OpenFileDialog();
            string filePath = "";
            if (Program.autoInstall)
            {
                filePath = Application.StartupPath + "\\RelHaxUserConfigs\\" + Program.configName;
            }
            else if (Settings.saveLastConfig && !fromButton)
            {
                filePath = Application.StartupPath + "\\RelHaxUserConfigs\\lastInstalledConfig.xml";
                if (File.Exists(filePath))
                {
                    Settings.appendToLog("save last config checked, loading config from " + filePath);
                }
                else
                {
                    Settings.appendToLog("ERROR: lastInstalledConfig.xml not found, not loading configs");
                    return;
                }
            }
            else
            {
                loadLocation.AddExtension = true;
                loadLocation.DefaultExt = ".xml";
                loadLocation.Filter = "*.xml|*.xml";
                loadLocation.InitialDirectory = Application.StartupPath + "\\RelHaxUserConfigs";
                loadLocation.Title = "Select User pref to load";
                if (loadLocation.ShowDialog().Equals(DialogResult.Cancel))
                {
                    //quit
                    return;
                }
                filePath = loadLocation.FileName;
            }
            this.clearSelectionMemory();
            this.appendToLog("Loading mod selections from " + filePath);
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            //get a list of mods
            XmlNodeList xmlModList = doc.SelectNodes("//mods/relhaxMods/mod");
            foreach (XmlNode n in xmlModList)
            {
                //gets the inside of each mod
                //also store each config that needsto be enabled
                Mod m = new Mod();
                foreach (XmlNode nn in n.ChildNodes)
                {
                    switch (nn.Name)
                    {
                        case "name":
                            m = this.linkMod(nn.InnerText);
                            if (m == null) continue;
                            if (m.enabled)
                            {
                                this.appendToLog("Checking mod " + m.name);
                                m.modChecked = true;
                            }
                            break;
                        case "configs":
                            foreach (XmlNode nnn in nn.ChildNodes)
                            {
                                Config c = new Config();
                                foreach (XmlNode nnnn in nnn.ChildNodes)
                                {
                                    switch (nnnn.Name)
                                    {
                                        case "name":
                                            c = m.getConfig(nnnn.InnerText);
                                            if (c == null)
                                                continue;
                                            if (c.enabled)
                                            {
                                                this.appendToLog("Checking config " + c.name);
                                                c.configChecked = true;
                                            }
                                            break;
                                    }
                                }
                            }
                            break;
                    }
                }
            }
            //user mods
            XmlNodeList xmlUserModList = doc.SelectNodes("//mods/userMods/mod");
            foreach (XmlNode n in xmlUserModList)
            {
                //gets the inside of each user mod
                Mod m = new Mod();
                foreach (XmlNode nn in n.ChildNodes)
                {
                    switch (nn.Name)
                    {
                        case "name":
                            m = this.getUserMod(nn.InnerText);
                            if (m != null)
                            {
                                string filename = m.name + ".zip";
                                if (File.Exists(Application.StartupPath + "\\RelHaxUserMods\\" + filename))
                                {
                                    m.modChecked = true;
                                }
                            }
                            break;
                    }
                }
            }
            this.appendToLog("Finished loading mod selections");
            if (Settings.saveLastConfig && !fromButton)
                return;
            if (!Program.autoInstall || !Settings.saveLastConfig)
                MessageBox.Show("Prefrences Set");
            //reload the UI
            this.makeTabs();
            this.addAllMods();
            this.addUserMods();
        }
        //checks for duplicates
        private bool duplicates()
        {
            //add every mod name to a new list
            List<string> modNameList = new List<string>();
            foreach (Catagory c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    modNameList.Add(m.name);
                }
            }
            //itterate through every mod name again
            foreach (Catagory c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    //in theory, there should only be one mathcing mod name
                    //between the two lists. more indicates a duplicates
                    int i = 0;
                    foreach (string s in modNameList)
                    {
                        if (s.Equals(m.name))
                            i++;
                    }
                    if (i > 1)//if there are 2 or more matching mods
                        return true;//duplicate detected
                }
            }
            //making it here means there are no duplicates
            return false;
        }
        //sorts a list of mods alphabetaicaly
        private void sortModsList(List<Mod> modList)
        {
            //sortModsList
            modList.Sort(Mod.CompareMods);
        }
        //sorte a list of catagoris alphabetaicaly
        private void sortCatagoryList(List<Catagory> catagoryList)
        {
            catagoryList.Sort(Catagory.CompareCatagories);
        }
        //sorts a list of pictures by mod or config, then name
        private List<Picture> sortPictureList(List<Picture> pictureList)
        {
            pictureList.Sort(Picture.ComparePictures);
            return pictureList;
        }
        //handler for when the "load config" button is pressed
        private void loadConfigButton_Click(object sender, EventArgs e)
        {
            this.loadConfig(true);
        }
        //handler for when the "save config" button is pressed
        private void saveConfigButton_Click(object sender, EventArgs e)
        {
            this.saveConfig(true);
        }
        //handler for when a new tab page is selected
        private void modTabGroups_Selected(object sender, TabControlEventArgs e)
        {
            this.ModSelectionList_SizeChanged(null, null);
        }
        //handler for when a mod tab group is clicked on
        private void modTabGroups_Click(object sender, EventArgs e)
        {
            this.ModSelectionList_SizeChanged(null, null);
        }
        //returns a string of the MD5 hash of an object.
        //used to determine if a download is corrupted or not,
        //or if it needs to be updated
        private string GetMd5Hash(MD5 md5Hash, string inputFile)
        {
            // Convert the input string to a byte array and compute the hash.
            var stream = File.OpenRead(inputFile);
            byte[] data = md5Hash.ComputeHash(stream);
            stream.Close();
            // Create a new Stringbuilder to collect the bytes
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();
            // Loop through each byte of the hashed data 
            // and format each one as a hexadecimal string.
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            // Return the hexadecimal string.
            return sBuilder.ToString();
        }
        //handler for when the close button is pressed
        private void ModSelectionList_FormClosing(object sender, FormClosingEventArgs e)
        {
            //save the size of this window for later.
            Settings.modSelectionHeight = this.Size.Height;
            Settings.modSelectionWidth = this.Size.Width;
        }
        //gets the file size of a download
        private float netFileSize(string address)
        {
            System.Net.WebRequest req = System.Net.HttpWebRequest.Create(address);
            req.Method = "HEAD";
            using (System.Net.WebResponse resp = req.GetResponse())
            {
                int ContentLength;
                if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
                {
                    float result = (float)ContentLength;
                    result = result / (1024 * 1024);//mbytes
                    return result;
                }
            }
            return -1;
        }
        //unchecks all mods from memory
        private void clearSelectionMemory()
        {
            this.appendToLog("Unchecking all mods");
            foreach (Catagory c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.enabled)
                    {
                        m.modChecked = false;
                        foreach (Config cc in m.configs)
                        {
                            if (cc.enabled)
                            {
                                cc.configChecked = false;
                            }
                        }
                    }
                }
            }
        }

        private void clearSelectionsButton_Click(object sender, EventArgs e)
        {
            this.clearSelectionMemory();
            Settings.appendToLog("clearSelectionsButton pressed, clearing selections");
            MessageBox.Show("Selections cleared");
            //reload the UI
            this.makeTabs();
            this.addAllMods();
            this.addUserMods();
        }
    }
}
