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

namespace RelicModManager
{
    public partial class ModSelectionList : Form
    {
        public List<Catagory> parsedCatagoryList;//can be grabbed by MainWindow
        public List<Mod> userMods;//can be grabbed by MainWindow
        public bool cancel = true;//used to determine if the user canceled
        private Preview p = new Preview();

        public ModSelectionList()
        {
            InitializeComponent();
        }

        //called on application startup
        private void ModSelectionList_Load(object sender, EventArgs e)
        {
            this.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
            this.createModStructure2();
            bool duplicates = this.duplicates();
            if (duplicates)
            {
                this.appendToLog("CRITICAL: Duplicate mod name detected!!");
                MessageBox.Show("CRITICAL: Duplicate mod name detected!!");
                Application.Exit();
            }
            this.makeTabs();
            this.addAllMods();
            this.addUserMods();
            this.ModSelectionList_SizeChanged(null, null);
            this.Size = new Size(520, 250);
        }
        
        //adds all usermods to thier own userMods tab
        private void addUserMods()
        {
            //make the new tab
            TabPage tb = new TabPage("User Mods");
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
                  m.modChecked = false;
                  userMods.Add(m);
              }
            }
            //add all mods to the tab page
            for (int i = 0; i < userMods.Count; i++)
            {
                //make modCheckBox
                CheckBox modCheckBox = new CheckBox();
                modCheckBox.AutoSize = true;
                int yLocation = 3 + (17*(i));
                modCheckBox.Location = new System.Drawing.Point(3, yLocation);
                //modCheckBox.Name = "modCheckBox";
                modCheckBox.Size = new System.Drawing.Size(49, 17);
                modCheckBox.TabIndex = 1;
                modCheckBox.Text = userMods[i].name;
                modCheckBox.UseVisualStyleBackColor = true;
                modCheckBox.Enabled = true;
                modCheckBox.CheckedChanged += new EventHandler(modCheckBox_CheckedChanged);
                tb.Controls.Add(modCheckBox);
            }
            modTabGroups.TabPages.Add(tb);
        }
        
        private void addAllMods()
        {
            foreach (TabPage t in this.modTabGroups.TabPages)
            {
                foreach (Catagory c in parsedCatagoryList)
                {
                    if (c.name.Equals(t.Text))
                    {
                        //matched the catagory to tab
                        //add to the ui every mod of that catagory
                        this.sortModsList(c.mods);
                        int i = 1;
                        foreach (Mod m in c.mods)
                        {
                            this.addMod(m, t, i++);
                        }
                        break;
                    }
                }
            }
        }

        //adds a tab view for each mod catagory
        private void makeTabs()
        {
            modTabGroups.TabPages.Clear();
            this.sortCatagoryList(parsedCatagoryList);
            foreach (Catagory c in parsedCatagoryList)
            {
                TabPage t = new TabPage(c.name);
                t.AutoScroll = true;
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
            //configPanel.Name = "configPanel";
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
            configControlDD.Location = new System.Drawing.Point(0,0);
            configControlDD.Size = new System.Drawing.Size(150, 15);
            configControlDD.TabIndex = 1;
            configControlDD.TabStop = true;
            configControlDD.Enabled = false;
            configControlDD.SelectedIndexChanged += new EventHandler(configControlDD_SelectedIndexChanged);
            //configControlDD.MouseClick += new MouseEventHandler(modCheckBox_MouseClick);
            configControlDD.Name = t.Text + "_" + m.name + "_DropDown";
            configControlDD.DropDownStyle = ComboBoxStyle.DropDownList;
            configControlDD.Items.Clear();
            for (int i = 0; i < m.configs.Count; i++)
            {
                int yPosition = 15 * (i + 1);
                //make configLabel if config type is not single_dropDown
                if (!m.configs[i].type.Equals("single_dropdown"))
                {
                    Label configLabel = new Label();
                    configLabel.AutoSize = true;
                    configLabel.Location = new System.Drawing.Point(5, yPosition-10);
                    configLabel.Size = new System.Drawing.Size(100, 15);
                    configLabel.TabIndex = 0;
                    configLabel.Text = m.configs[i].name;
                    configLabel.Enabled = false;
                    //configLabel.MouseClick += new MouseEventHandler(modCheckBox_MouseClick);
                    configPanel.Controls.Add(configLabel);
                }
                switch (m.configs[i].type)
                {
                    case "single":
                        //make a radioButton
                        RadioButton configControlRB = new RadioButton();
                        configControlRB.AutoSize = true;
                        configControlRB.Location = new System.Drawing.Point(100, yPosition-10);
                        configControlRB.Size = new System.Drawing.Size(150, 15);
                        configControlRB.TabIndex = 1;
                        configControlRB.TabStop = true;
                        configControlRB.Enabled = false;
                        
                        //configControlRB.MouseClick += new MouseEventHandler(modCheckBox_MouseClick);
                        configControlRB.Checked = m.configs[i].configChecked;
                        configControlRB.CheckedChanged += new EventHandler(configControlRB_CheckedChanged);
                        configControlRB.Name = t.Text + "_" + m.name + "_" + m.configs[i].name;
                        configPanel.Controls.Add(configControlRB);
                        break;

                    case "single_dropdown":
                        //make a dropDown selection box
                        if (configControlDD.Location.X == 0 && configControlDD.Location.Y == 0)configControlDD.Location = new System.Drawing.Point(100, yPosition - 10);
                        if (m.configs[i].enabled) configControlDD.Items.Add(m.configs[i].name);
                        if(m.configs[i].configChecked) configControlDD.SelectedIndex = i;
                        break;

                    case "multi":
                        //make a checkBox
                        CheckBox configControlCB = new CheckBox();
                        configControlCB.AutoSize = true;
                        configControlCB.Location = new System.Drawing.Point(100, yPosition - 10);
                        configControlCB.Size = new System.Drawing.Size(150, 15);
                        configControlCB.TabIndex = 1;
                        configControlCB.TabStop = true;
                        //the handler for modCheckBox should take care of the enabled stuff
                        configControlCB.Enabled = false;
                        configControlCB.Checked = m.configs[i].configChecked;
                        configControlCB.CheckedChanged += new EventHandler(configControlCB_CheckedChanged);
                        //configControlCB.MouseClick += new MouseEventHandler(modCheckBox_MouseClick);
                        
                        configControlCB.Name = t.Text + "_" + m.name + "_" + m.configs[i].name;
                        configPanel.Controls.Add(configControlCB);
                        break;

                    case "value_enter":
                        //DEPRECATED: make a text box
                        TextBox configControlTB = new TextBox();
                        configControlTB.Text = m.configDefault;
                        configControlTB.AutoSize = true;
                        configControlTB.Location = new System.Drawing.Point(100, yPosition-10);
                        configControlTB.Size = new System.Drawing.Size(150, 15);
                        configControlTB.TabIndex = 1;
                        configControlTB.TabStop = true;
                        configControlTB.Enabled = false;
                        configControlTB.TextChanged += new EventHandler(configControlTB_TextChanged);
                        configControlTB.Name = t.Text + "_" + m.name + "_" + m.configs[i].name;
                        configControlTB.BackColor = Color.Green;
                        configPanel.Controls.Add(configControlTB);
                        break;
                }
            }
            if (configControlDD.Items.Count > 0)
                configPanel.Controls.Add(configControlDD);
            //make the mod check box
            CheckBox modCheckBox = new CheckBox();
            modCheckBox.AutoSize = true;
            modCheckBox.Location = new System.Drawing.Point(3, 3);
            modCheckBox.Size = new System.Drawing.Size(49, 15);
            modCheckBox.TabIndex = 1;
            modCheckBox.Text = m.name;
            modCheckBox.UseVisualStyleBackColor = true;
            modCheckBox.Enabled = m.enabled;
            
            //modCheckBox.Click += new EventHandler(modCheckBox_MouseClick);
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
            if (panelCount > 1)
            {
                panelCountYLocation = (panelCount - 1)*(t.Controls[0].Size.Height);
                panelCountYLocation = panelCountYLocation + 5;
            }
            mainPanel.Location = new System.Drawing.Point(5, panelCountYLocation+5);
            
            mainPanel.TabIndex = 0;
            mainPanel.AutoSize = true;
            mainPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
            mainPanel.Size = new System.Drawing.Size(t.Size.Width - 25, 20);
            //add to main panel
            mainPanel.Controls.Clear();
            mainPanel.Controls.Add(modCheckBox);
            mainPanel.Controls.Add(configPanel);

            //add to tab
            t.Controls.Add(mainPanel);
            modCheckBox.CheckedChanged += new EventHandler(modCheckBox_CheckedChanged);
            modCheckBox.Checked = m.modChecked;
        }

        void modCheckBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            if (sender is CheckBox)
            {
                CheckBox cb = (CheckBox)sender;
                Mod m = this.getMod(cb.Text);
                string name = m.name;
                //get the mod and/or config
                List<string> picturesList = m.picturesList;
                string desc = m.description;
                string updateNotes = m.updateComment;
                string devurl = m.devURL;
                if (devurl == null)
                    devurl = "";
                p.Close();
                p = new Preview(name, picturesList, desc, updateNotes,devurl);
                p.Show();
            }
            else if (sender is RadioButton)
            {
                RadioButton rb = (RadioButton)sender;
                Mod m = this.getMod(rb.Text);
                string name = m.name;
                //get the mod and/or config
                List<string> picturesList = m.picturesList;
                string desc = m.description;
                string updateNotes = m.updateComment;
                p.Close();
                p = new Preview(name, picturesList, desc, updateNotes);
                p.Show();
            }
            else if (sender is Label)
            {
                Label l = (Label)sender;
                //get the name based on config or mod
                Mod m = this.getMod(l.Text);
                string name = m.name;
                //get the mod and/or config
                List<string> picturesList = m.picturesList;
                string desc = m.description;
                string updateNotes = m.updateComment;
                p.Close();
                p = new Preview(name, picturesList, desc, updateNotes);
                p.Show();
            }
        }

        void configControlDD_SelectedIndexChanged(object sender, EventArgs e)
        {
            //uncheck all other configs
            ComboBox cb = (ComboBox) sender;
            //get the mod this config is associated with
            //this is safe because it will never be a user mod
            string catagory = cb.Name.Split('_')[0];
            string mod = cb.Name.Split('_')[1];
            Mod m = this.getCatagory(catagory).getMod(mod);
            string configName = (string)cb.SelectedItem;
            foreach (Config c in m.configs)
            {
                if (c.type.Equals("single_dropdown"))
                {
                    c.configChecked = false;
                    if (configName.Equals(c.name))
                    {
                        c.configChecked = true;
                    }
                }
            }
        }

        //DEPRECATED: handler for when the config of text box is changed
        void configControlTB_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            try
            {
                //check to see if the user has entered a valid value like an int
                int temp = int.Parse(tb.Text);
                tb.BackColor = Color.Green;
                string modName = tb.Parent.Parent.Controls[0].Text;
                string catagoryName = tb.Parent.Parent.Parent.Text;
                foreach (Catagory c in parsedCatagoryList)
                {
                    foreach (Mod m in c.mods)
                    {
                        if (c.name.Equals(catagoryName) && m.name.Equals(modName))
                        {
                            //get the mod in memory to update
                            foreach (Config cc in m.configs)
                            {
                                string configName = tb.Name.Split('_')[2];
                                if (configName.Equals(cc.name))
                                {
                                    //cc.setValue = tb.Text;
                                    cc.configChecked = true;
                                }
                            }
                        }
                    }
                }
            }
            catch (FormatException)
            {
                //the user has not entered a vaid value like decimals or letters
                tb.BackColor = Color.Red;
                string modName = tb.Parent.Parent.Controls[0].Text;//text name of mod
                string catagoryName = tb.Parent.Parent.Parent.Text;//text name of catagory
                foreach (Catagory c in parsedCatagoryList)
                {
                    foreach (Mod m in c.mods)
                    {
                        if (c.name.Equals(catagoryName) && m.name.Equals(modName))
                        {
                            //catagory and mod in memory linked to catagory and mod in ui
                            foreach (Config cc in m.configs)
                            {
                                //get the name of the config
                                string configName = tb.Name.Split('_')[2];
                                if (configName.Equals(cc.name))
                                {
                                    //config ui linked to config memory
                                    cc.configChecked = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        //handler for when the config checkbox is checked or unchecked
        void configControlCB_CheckedChanged(object sender, EventArgs e)
        {
            //it's a RelHax modpack config checkBox
            CheckBox cb = (CheckBox)sender;
            string modName = cb.Parent.Parent.Controls[0].Text;
            string catagoryName = cb.Parent.Parent.Parent.Text;
            Mod m = this.linkMod(modName,catagoryName);
            foreach (Config cc in m.configs)
            {
                string configName = cb.Name.Split('_')[2];
                if (configName.Equals(cc.name) && cc.type.Equals("multi"))
                {
                    cc.configChecked = cb.Checked;
                }
            }
        }

        //handler for when a config radioButton is pressed
        void configControlRB_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = (RadioButton)sender;
            string modName = rb.Parent.Parent.Controls[0].Text;
            string catagoryName = rb.Parent.Parent.Parent.Text;
            Mod m = this.linkMod(modName,catagoryName);
            foreach (Config cc in m.configs)
            {
                string configName = rb.Name.Split('_')[2];
                if (cc.type.Equals("single"))
                {
                    cc.configChecked = false;
                    if (configName.Equals(cc.name))
                    {
                        //enable that config for that mod in memory
                        cc.configChecked = rb.Checked;
                    }
                }
            }
        }

        //handler for when a mod checkbox is changed
        //understand this cancer and re-write it cause holy crap it's awefull
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
                    foreach (Mod m in userMods)
                    {
                        if (m.name.Equals(cbUser.Text))
                        {
                            //linked the cb to the user mod
                            m.modChecked = cbUser.Checked;
                            return;
                        }
                    }
                }
            }
            //update the ui with the change
            CheckBox cb = (CheckBox)sender;
            Panel p = (Panel)cb.Parent;
            Panel innerPanel = (Panel)p.Controls[1];
            if (cb.Checked) innerPanel.BackColor = Color.BlanchedAlmond;
            else innerPanel.BackColor = SystemColors.Control;
            
            foreach (Catagory c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.name.Equals(cb.Text))
                    {
                        //enable the mod in memory
                        m.modChecked = cb.Checked;
                        //update configs
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
                                        /*if (cc is RadioButton)
                                        {
                                            RadioButton b = (RadioButton)cc;
                                            b.Checked = false;
                                        }

                                        if (cc is CheckBox)
                                        {
                                            CheckBox b = (CheckBox)cc;
                                            b.Checked = false;
                                        }*/
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
            //if the mod checkbox was changed to checked state
            if (cb.Checked)
            {
                //check to make sure at least one config is selected
                bool oneSelected = false;
                foreach (Control c in innerPanel.Controls)
                {
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
                        //c.Enabled = true;
                        ComboBox cbox = (ComboBox)c;
                        if (cbox.SelectedIndex == -1) oneSelected = false;
                    }
                }
                if (!oneSelected)
                {
                    //select one randomly
                    foreach (Control c in innerPanel.Controls)
                    {
                        if (c is RadioButton)
                        {
                            RadioButton b = (RadioButton)c;
                            if (b.Enabled)
                            {
                                b.Checked = true;
                                break;
                            }
                        }

                        if (c is CheckBox)
                        {
                            CheckBox b = (CheckBox)c;
                            if (b.Enabled)
                            {
                                b.Checked = true;
                                break;
                            }
                        }

                        if (c is ComboBox)
                        {
                            ComboBox cbox = (ComboBox)c;
                            cbox.SelectedIndex = 0;
                        }
                    }
                }
            }
        }

        //parses the xml mod info into the memory database
        private void createModStructure2()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load("https://dl.dropboxusercontent.com/u/44191620/RelicMod/mods/modInfo.xml");
            //DEGUB
            //doc.Load("modInfo.xml");
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
                                            m.version = float.Parse(nn.InnerText);
                                            break;
                                        case "modzipfile":
                                            m.modZipFile = nn.InnerText;
                                            break;
                                        case "modzipcrc":
                                            m.crc = nn.InnerText;
                                            break;
                                        case "enabled":
                                            m.enabled = bool.Parse(nn.InnerText);
                                            break;
                                        case "configselectiontype":
                                            m.configType = nn.InnerText;
                                            break;
                                        case "configdefault":
                                            m.configDefault = nn.InnerText;
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
                                                            m.picturesList.Add(nnnnnnnn.InnerText);
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
                                                            c.enabled = bool.Parse(nnnn.InnerText);
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
                                                                            m.picturesList.Add(nnnnnnnnnn.InnerText);
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
                                            d.enabled = bool.Parse(nnnn.InnerText);
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
            //helpLabel.Location = new Point(9, modTabGroups.Location.Y + modTabGroups.Size.Height + 6);
            loadConfigButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width - 6 - loadConfigButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height);
            saveConfigButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height);
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
            this.Close();
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        //stub for possible implementation later
        private void ModSelectionList_FormClosing(object sender, FormClosingEventArgs e)
        {
            
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
                if (c.name.Equals(catName))return c;
            }
            return null;
        }
        
        //gets the name of the user mod based on it's name
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
        private void saveConfig()
        {
            //dialog box to ask where to save the config to
            SaveFileDialog saveLocation = new SaveFileDialog();
            saveLocation.AddExtension = true;
            saveLocation.DefaultExt = ".xml";
            saveLocation.Filter = "*.xml|*.xml";
            //TODO: CHECK THIS
            saveLocation.InitialDirectory = Application.StartupPath + "\\RelHaxUserConfigs";
            saveLocation.Title = "Select where to save user prefs";
            if (saveLocation.ShowDialog().Equals(DialogResult.Cancel))
            {
                //cancel
                return;
            }
            string savePath = saveLocation.FileName;
            //XmlDocument save time!
            XmlDocument doc = new XmlDocument();
            //XmlElement root = doc.DocumentElement;
            XmlElement modsHolder = doc.CreateElement("mods");
            doc.AppendChild(modsHolder);
            //doc.InsertAfter(modsHolder,root);
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
            doc.Save(savePath);
            MessageBox.Show("Config Saved Sucessfully");
        }
        
        //loads a saved config from xml and parses it into the memory database
        private void loadConfig()
        {
          OpenFileDialog loadLocation = new OpenFileDialog();
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
          string filePath = loadLocation.FileName;
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
          this.appendToLog("Loading mod selections from " + filePath);
          XmlDocument doc = new XmlDocument();
          doc.Load(filePath);
          //get a list of mods
          XmlNodeList xmlModList = doc.SelectNodes("//mods/mod");
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
            this.appendToLog("Finished loading mod selections");
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
                    if (i>1)//if there are 2 or more matching mods
                        return true;//duplicate detected
                }
            }
            //making it here means there are no duplicates
            return false;
        }

        //gets a mod based on any catagory
        public Mod getMod(string modName)
        {
            foreach (Catagory c in parsedCatagoryList)
            {
                foreach (Mod m in c.mods)
                {
                    if (m.name.Equals(modName))
                    {
                        return m;
                    }
                }
            }
            return null;
        }
        
        //to be over-ridden with the handler for mouse click
        private void rightclick(object sender, MouseEventArgs e)
        {
            
        }
        
        //sorts a list of mods
        private void sortModsList(List<Mod> modList)
        {
            //sortModsList
            modList.Sort(Mod.CompareMods);
        }

        private void sortCatagoryList(List<Catagory> catagoryList)
        {
            catagoryList.Sort(Catagory.CompareCatagories);
        }

        private void loadConfigButton_Click(object sender, EventArgs e)
        {
            this.loadConfig();
        }

        private void saveConfigButton_Click(object sender, EventArgs e)
        {
            this.saveConfig();
        }

        private void modTabGroups_TabIndexChanged(object sender, EventArgs e)
        {

        }

        private void modTabGroups_Selected(object sender, TabControlEventArgs e)
        {
            this.ModSelectionList_SizeChanged(null, null);
        }

        private void modTabGroups_Click(object sender, EventArgs e)
        {
            this.ModSelectionList_SizeChanged(null, null);
        }
    }
}
