using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.Windows.Forms.Integration;
using System.Text;
using System.Text.RegularExpressions;


namespace RelhaxModpack
{
    //the mod selectin window. allows users to select which mods they wish to install
    public partial class ModSelectionList : Form
    {
        public List<Category> parsedCatagoryList;//can be grabbed by MainWindow
        public List<Mod> userMods;//can be grabbed by MainWindow
        public bool cancel = true;//used to determine if the user canceled
        private Preview p = new Preview(null, null, null);
        private PleaseWait pw;
        public List<Dependency> globalDependencies;
        private bool loadingConfig = false;
        private bool taskBarHidden = false;
        private const int titleBar = 23;//set origionally for 23
        private int difference = 0;
        private string tanksVersion = "";
        private string tanksLocation = "";
        private int mainWindowStartX = 0;
        private int mainWindowStartY = 0;
        bool hasRadioButtonConfigSelected = false;
        bool modHasRadioButtons = false;
        private enum loadConfigMode
        {
            error = -1,
            fromButton = 0,//this is the default state
            fromSaveLastConfig = 1,//this is the state if the user selected the setting "save last install's selection"
            fromAutoInstall = 2//this is for when the user started the application in auto install mode. this takes precedence over the above 2
        };
        private loadConfigMode loadMode = loadConfigMode.fromButton;
        
        public ModSelectionList(string version, string theTanksVersion, int mainWindowX, int mainWindowY)
        {
            InitializeComponent();
            tanksVersion = version;
            tanksLocation = theTanksVersion;
            mainWindowStartX = mainWindowX;
            mainWindowStartY = mainWindowY;
        }

        private void applyTranslations()
        {
            continueButton.Text = Translations.getTranslatedString(continueButton.Name);
            cancelButton.Text = Translations.getTranslatedString(cancelButton.Name);
            helpLabel.Text = Translations.getTranslatedString(helpLabel.Name);
            loadConfigButton.Text = Translations.getTranslatedString(loadConfigButton.Name);
            saveConfigButton.Text = Translations.getTranslatedString(saveConfigButton.Name);
            label2.Text = Translations.getTranslatedString(label2.Name);
            clearSelectionsButton.Text = Translations.getTranslatedString(clearSelectionsButton.Name);
        }

        //called on application startup
        private void ModSelectionList_Load(object sender, EventArgs e)
        {
            //create the loading window
            pw = new PleaseWait(mainWindowStartX,mainWindowStartY);
            pw.Show();
            //set the font from settings
            this.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
            //apply the translations
            this.applyTranslations();
            pw.loadingDescBox.Text = Translations.getTranslatedString("readingDatabase");
            Application.DoEvents();
            string databaseURL = "http://wotmods.relhaxmodpack.com/RelhaxModpack/modInfo_" + tanksVersion + ".xml";
            if (Program.testMode)
                databaseURL = "modInfo.xml";
            //create new lists for memory database and serialize from xml->lists
            globalDependencies = new List<Dependency>();
            parsedCatagoryList = new List<Category>();
            Utils.createModStructure2(databaseURL,false,globalDependencies,parsedCatagoryList);
            bool duplicates = Utils.duplicates(parsedCatagoryList);
            if (duplicates)
            {
                Utils.appendToLog("CRITICAL: Duplicate mod name detected!!");
                MessageBox.Show("CRITICAL: Duplicate mod name detected!!");
                Application.Exit();
            }
            this.initUserMods();
            pw.loadingDescBox.Text = Translations.getTranslatedString("buildingUI");
            Application.DoEvents();
            //the default loadConfig mode shold be from clicking the button
            loadMode = loadConfigMode.fromButton;
            //if the load config from last selection is checked, then set the mode to it
            if (Settings.saveLastConfig)
            {
                loadMode = loadConfigMode.fromSaveLastConfig;
            }
            //if this is in auto install mode, then it takes precedence
            if (Program.autoInstall)
            {
                loadMode = loadConfigMode.fromAutoInstall;
            }
            //check which mode the load config is in and act accordingly. it should only load the config in one of the following scenarios
            if (loadMode == loadConfigMode.fromSaveLastConfig)
            {
                this.parseLoadConfig();
            }
            else if (loadMode == loadConfigMode.fromAutoInstall)
            {
                this.parseLoadConfig();
                this.cancel = false;
                pw.Close();
                this.Close();
                return;
            }
            //actually build the UI display
            this.makeTabs();
            this.addAllMods();
            this.addUserMods();
            //set the size to the last closed size
            this.Size = new Size(Settings.modSelectionWidth, Settings.modSelectionHeight);
            //set the UI colors
            Settings.setUIColor(this);
            pw.Close();
            //set label properties
            TanksVersionLabel.Text = TanksVersionLabel.Text + tanksVersion;
            TanksPath.Text = Translations.getTranslatedString("InstallingTo") + " " + tanksLocation;
            //if the task bar was set to auto hide, set it to always on top
            //it will be set back to auto hide when this window closes
            Settings.AppBarStates currentState = Settings.GetTaskbarState();
            if (currentState == Settings.AppBarStates.AutoHide)
            {
                taskBarHidden = true;
                Settings.SetTaskbarState(Settings.AppBarStates.AlwaysOnTop);
            }
            //get the maximum height of the screen
            this.MaximumSize = Screen.FromControl(this).WorkingArea.Size;
            //get the size of the title bar window
            Rectangle screenRektangle = RectangleToScreen(this.ClientRectangle);
            int titleHeight = screenRektangle.Top - this.Top;
            //largest possible is 46
            //mine (programmed for) is 23
            if (titleHeight > titleBar)
            {
                difference = titleHeight - titleBar;
            }
            //force a resize
            this.ModSelectionList_SizeChanged(null, null);
            if (Settings.ModSelectionFullscreen)
            {
                this.WindowState = FormWindowState.Maximized;
            }
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
                    m.zipFile = s;
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
            tb.AutoScroll = true;
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
                modCheckBox.Checked = userMods[i].Checked;
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
            Utils.appendToLog("Loading ModSelectionList with view " + Settings.sView);
            foreach (TabPage t in this.modTabGroups.TabPages)
            {
                foreach (Category c in parsedCatagoryList)
                {
                    if (c.name.Equals(t.Name))
                    {
                        //matched the catagory to tab
                        //add to the ui every mod of that catagory
                        Utils.sortModsList(c.mods);
                        int i = 1;
                        LegacySelectionList lsl = null;
                        if (Settings.sView == Settings.SelectionView.legacy)
                        {
                            //create the WPF host for this tabPage
                            ElementHost host = new ElementHost();
                            host.Location = new Point(5, 5);
                            host.Size = new Size(t.Size.Width - 5 - 5, t.Size.Height - 5 - 5);
                            host.BackColorTransparent = false;
                            host.BackColor = Color.White;
                            lsl = new LegacySelectionList();
                            host.Child = lsl;
                            lsl.legacyTreeView.Items.Clear();
                            t.Controls.Add(host);
                        }
                        foreach (Mod m in c.mods)
                        {
                            pw.loadingDescBox.Text = Translations.getTranslatedString("loading") + " " + m.name;
                            Application.DoEvents();
                            if (Settings.sView == Settings.SelectionView.defaultt)
                            {
                                //use default UI
                                this.addMod(m, t, i++, c);
                            }
                            else if (Settings.sView == Settings.SelectionView.legacy)
                            {
                                //use legacy OMC UI
                                this.addModTreeview(m, t, i++, lsl, c);
                            }
                            else
                            {
                                //default case, use default
                                this.addMod(m, t, i++, c);
                            }
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
            modTabGroups.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
            //this.sortCatagoryList(parsedCatagoryList);
            foreach (Category c in parsedCatagoryList)
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
        //adds a mod m to a tabpage t, OMC treeview style
        private void addModTreeview(Mod m, TabPage t, int panelCount, LegacySelectionList lsl, Category c)
        {
            if (Settings.darkUI)
                lsl.legacyTreeView.Background = System.Windows.Media.Brushes.Gray;
            //helpfull stuff
            string modDownloadFilePath = Application.StartupPath + "\\RelHaxDownloads\\" + m.zipFile;
            hasRadioButtonConfigSelected = false;
            modHasRadioButtons = false;
            //link the catagory and mod in memory
            m.parent = c;
            //create base mod checkbox
            ModWPFCheckBox modCheckBox = new ModWPFCheckBox();
            //use a custom datatype for the name
            modCheckBox.mod = m;
            modCheckBox.catagory = c;
            if (Settings.largeFont)
                modCheckBox.FontSize = modCheckBox.FontSize + 4;
            modCheckBox.FontFamily = new System.Windows.Media.FontFamily(Settings.fontName);
            if (Settings.darkUI)
                modCheckBox.FontWeight = System.Windows.FontWeights.Bold;
            //make the tree view item for the modCheckBox
            System.Windows.Controls.TreeViewItem tvi = new System.Windows.Controls.TreeViewItem();
            //process configs
            if (m.configs.Count > 0)
                processConfigs(c, m, m.configs, tvi, true);
            modCheckBox.Content = m.name;
            //get the local md5 hash. a -1 indicates the file is not on the disk
            string oldCRC2 = Utils.getMd5Hash(modDownloadFilePath);
            //if the CRC's don't match and the mod actually has a zip file
            if (!(m.crc.Equals(oldCRC2)) && (!m.zipFile.Equals("")))
            {
                modCheckBox.Content = modCheckBox.Content + " (Updated)";
                m.downloadFlag = true;
                if ((m.size > 0.0f) )
                    modCheckBox.Content = modCheckBox.Content + " (" + m.size + " MB)";
            }
            //set mod's enabled status
            modCheckBox.IsEnabled = m.enabled;
            //set the mods checked status
            if (m.enabled && m.Checked)
                modCheckBox.IsChecked = true;
            
            tvi.Header = modCheckBox;
            //add it's handlers, right click and when checked
            modCheckBox.MouseDown += new System.Windows.Input.MouseButtonEventHandler(modCheckBoxL_MouseDown);
            
            //add the mod check box to the legacy tree view
            lsl.legacyTreeView.Items.Add(tvi);
            //disable the logic for now
            if (false && (bool)modCheckBox.IsChecked && modHasRadioButtons && !hasRadioButtonConfigSelected)
            {
                //getting here means that the user has saved the prefrence for a selected mandatory radiobutton config that has been disabled, so his selection of that mod needs to be disabled
                m.Checked = false;
                modCheckBox.IsChecked = false;
            }
            modCheckBox.Click += new System.Windows.RoutedEventHandler(modCheckBoxL_Click);
        }

        void processConfigs(Category c, Mod m, List<Config> configs, System.Windows.Controls.TreeViewItem tvi, bool parentIsMod = false, Config parentConfig = null)
        {
            //create the twp possible drop down options, and the mod optional config check box i guess
            ConfigWPFComboBox configControlDD = new ConfigWPFComboBox();
            configControlDD.Items.Clear();
            configControlDD.IsEditable = false;
            //configControlDD.FontSize = Settings.fontSize;
            configControlDD.Name = "notAddedYet";
            configControlDD.IsEnabled = false;
            ConfigWPFComboBox configControlDD2 = new ConfigWPFComboBox();
            configControlDD2.Items.Clear();
            configControlDD2.IsEditable = false;
            //configControlDD2.FontSize = Settings.fontSize;
            configControlDD2.Name = "notAddedYet";
            configControlDD2.IsEnabled = false;
            //process the configs
            foreach (Config con in configs)
            {
                //link stuff in memory
                con.parentMod = m;
                if (parentIsMod)
                    con.parent = m;
                else
                    con.parent = parentConfig;
                //create the init stuff for each config
                ConfigWPFComboBox configControlDDALL = null;
                if (con.type.Equals("single") || con.type.Equals("single1"))
                {
                    modHasRadioButtons = true;
                    //make the radio button
                    ConfigWPFRadioButton configControlRB = new ConfigWPFRadioButton();
                    if (Settings.largeFont)
                        configControlRB.FontSize = configControlRB.FontSize + 4;
                    configControlRB.FontFamily = new System.Windows.Media.FontFamily(Settings.fontName);
                    if (Settings.darkUI)
                        configControlRB.FontWeight = System.Windows.FontWeights.Bold;
                    configControlRB.catagory = c;
                    configControlRB.mod = m;
                    configControlRB.config = con;
                    //the logic for enabling it
                    //set them to false first
                    configControlRB.IsEnabled = false;
                    configControlRB.IsChecked = false;
                    if (parentIsMod)
                    {
                        if (m.enabled && con.enabled)
                        {
                            configControlRB.IsEnabled = true;
                            //the logic for checking it
                            if (m.Checked && con.Checked)
                            {
                                configControlRB.IsChecked = true;
                                hasRadioButtonConfigSelected = true;
                            }
                        }
                    }
                    else
                    {
                        if (parentConfig.enabled && con.enabled)
                        {
                            configControlRB.IsEnabled = true;
                            //the logic for checking it
                            if (parentConfig.Checked && con.Checked)
                            {
                                configControlRB.IsChecked = true;
                                hasRadioButtonConfigSelected = true;
                            }
                        }
                    }
                    //run the checksum logix
                    configControlRB.Content = con.name;
                    string oldCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + con.zipFile);
                    if (!oldCRC.Equals(con.crc) && (!con.crc.Equals("")))
                    {
                        configControlRB.Content = configControlRB.Content + " (Updated)";
                        con.downloadFlag = true;
                        if (con.size > 0.0f)
                            configControlRB.Content = configControlRB.Content + " (" + con.size + " MB)";
                    }
                    //add the handlers at the end
                    configControlRB.Click += new System.Windows.RoutedEventHandler(configControlRB_Click);
                    configControlRB.MouseDown += new System.Windows.Input.MouseButtonEventHandler(configControlRB_MouseDown);
                    //add it to the mod config list
                    System.Windows.Controls.TreeViewItem configControlTVI = new System.Windows.Controls.TreeViewItem();
                    configControlTVI.Header = configControlRB;
                    tvi.Items.Add(configControlTVI);
                    //process the subconfigs
                    if (con.configs.Count > 0)
                        processConfigs(c, m, con.configs, configControlTVI, false, con);
                }
                else if (con.type.Equals("single_dropdown") || con.type.Equals("single_dropdown1") || con.type.Equals("single_dropdown2"))
                {
                    //set the all to whichever one it actually is
                    if (con.type.Equals("single_dropdown") || con.type.Equals("single_dropdown1"))
                    {
                        configControlDDALL = configControlDD;
                    }
                    else if (con.type.Equals("single_dropdown2"))
                    {
                        configControlDDALL = configControlDD2;
                    }
                    //make the dropdown selection list
                    configControlDDALL.MinWidth = 100;
                    //run the crc logics
                    string oldCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + con.zipFile);
                    if (!oldCRC.Equals(con.crc) && (!con.crc.Equals("")))
                    {
                        string toAdd = con.name + "_Updated";
                        con.downloadFlag = true;
                        if (con.size > 0.0f)
                            toAdd = toAdd + " (" + con.size + " MB)";
                        //add it with _updated
                        if (con.enabled) configControlDDALL.Items.Add(new ComboBoxItem(con, toAdd));
                        if (con.Checked) configControlDDALL.SelectedItem = toAdd;
                    }
                    else
                    {
                        //add it
                        if (con.enabled) configControlDDALL.Items.Add(new ComboBoxItem(con, con.name));
                        if (con.Checked) configControlDDALL.SelectedItem = con.name;
                    }
                    //add the dropdown to the thing. it will only run this once
                    if (configControlDDALL.Name.Equals("notAddedYet"))
                    {
                        configControlDDALL.Name = "added";
                        configControlDDALL.catagory = c;
                        configControlDDALL.mod = m;
                        configControlDDALL.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(configControlDDALL_SelectionChanged);
                        configControlDDALL.PreviewMouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(configControlDDALL_MouseDown);
                        if (configControlDDALL.Items.Count > 0)
                            configControlDDALL.IsEnabled = true;
                        if (configControlDDALL.SelectedIndex == -1)
                            configControlDDALL.SelectedIndex = 0;
                        System.Windows.Controls.TreeViewItem configControlTVI = new System.Windows.Controls.TreeViewItem();
                        configControlTVI.Header = configControlDDALL;
                        tvi.Items.Add(configControlTVI);
                    }
                }
                else if (con.type.Equals("multi"))
                {
                    //make the checkbox
                    ConfigWPFCheckBox configControlCB = new ConfigWPFCheckBox();
                    if (Settings.largeFont)
                        configControlCB.FontSize = configControlCB.FontSize + 4;
                    configControlCB.FontFamily = new System.Windows.Media.FontFamily(Settings.fontName);
                    if (Settings.darkUI)
                        configControlCB.FontWeight = System.Windows.FontWeights.Bold;
                    configControlCB.catagory = c;
                    configControlCB.mod = m;
                    configControlCB.config = con;
                    //the logic for enabling it
                    //set them to false first
                    configControlCB.IsEnabled = false;
                    configControlCB.IsChecked = false;
                    if (parentIsMod)
                    {
                        if (m.enabled && con.enabled)
                        {
                            configControlCB.IsEnabled = true;
                            //the logic for checking it
                            if (m.Checked && con.Checked)
                            {
                                configControlCB.IsChecked = true;
                                hasRadioButtonConfigSelected = true;
                            }
                        }
                    }
                    else
                    {
                        if (parentConfig.enabled && con.enabled)
                        {
                            configControlCB.IsEnabled = true;
                            //the logic for checking it
                            if (parentConfig.Checked && con.Checked)
                            {
                                configControlCB.IsChecked = true;
                                hasRadioButtonConfigSelected = true;
                            }
                        }
                    }
                    //run the checksum logix
                    configControlCB.Content = con.name;
                    string oldCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + con.zipFile);
                    if (!oldCRC.Equals(con.crc) && (!con.crc.Equals("")))
                    {
                        configControlCB.Content = configControlCB.Content + " (Updated)";
                        con.downloadFlag = true;
                        if (con.size > 0.0f)
                            configControlCB.Content = configControlCB.Content + " (" + con.size + " MB)";
                    }
                    //add the handlers at the end
                    configControlCB.Click += new System.Windows.RoutedEventHandler(configControlCB_Click);
                    configControlCB.MouseDown += new System.Windows.Input.MouseButtonEventHandler(configControlCB_MouseDown);
                    //add it to the mod config list
                    System.Windows.Controls.TreeViewItem configControlTVI = new System.Windows.Controls.TreeViewItem();
                    configControlTVI.Header = configControlCB;
                    tvi.Items.Add(configControlTVI);
                    //process the subconfigs
                    if (con.configs.Count > 0)
                        processConfigs(c, m, con.configs, configControlTVI, false, con);
                }
                else
                {
                    Utils.appendToLog("WARNING: Unknown config type for " + con.name + ": " + con.type);
                }
            }
        }
        //when a legacy mod checkbox is clicked
        void modCheckBoxL_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (loadingConfig)
                return;
            ModWPFCheckBox cb = (ModWPFCheckBox)sender;
            Mod m = cb.mod;
            Category cat = m.parent;
            System.Windows.Controls.TreeViewItem TVI = (System.Windows.Controls.TreeViewItem)cb.Parent;
            System.Windows.Controls.TreeView TV = (System.Windows.Controls.TreeView)TVI.Parent;
            //check to see if this is a single selection categtory
            //if it is, then uncheck the other mod, then check this one
            if ((bool)cb.IsChecked && cat.selectionType.Equals("single"))
            {
                //check if any other mods in this catagory are already checked
                bool anyModsChecked = false;
                foreach (Mod mm in cat.mods)
                {
                    if (mm.Checked)
                    {
                        anyModsChecked = true;
                        mm.Checked = false;
                    }
                }
                if (anyModsChecked)
                {
                    cb.IsChecked = false;
                    //all other mods in this category need to be unchecked
                    foreach (System.Windows.Controls.TreeViewItem tvi in TV.Items)
                    {
                        ModWPFCheckBox modCB = (ModWPFCheckBox)tvi.Header;
                        //remove the handler before we make changes
                        //modCB.Click -= modCheckBoxL_Click;
                        if ((bool)modCB.IsChecked)
                        {
                            modCB.IsChecked = false;
                            modCB.mod.Checked = false;
                        }
                        //modCheckBoxL_Click(modCB, null);
                        //modCB.Click += modCheckBoxL_Click;
                    }
                }
                //now it is safe to check the mod we want
                cb.Click -= modCheckBoxL_Click;
                cb.IsChecked = true;
                cb.mod.Checked = true;
                cb.Click += modCheckBoxL_Click;
            }
            m.Checked = (bool)cb.IsChecked;

            //this section deals with enabling the configs, if there are any
            if (m.configs.Count == 0)
                return;
            //get the string name of the last radiobutton for refrence later
            string lastConfigName = "null";
            bool configSelected = false;
            foreach (Config configs in m.configs)
            {
                if (configs.type.Equals("single") || configs.type.Equals("single1"))
                {
                    lastConfigName = configs.name;
                }
            }
            //there is at least one config, so at least one UI element
            foreach (System.Windows.Controls.TreeViewItem item in TVI.Items)
            {
                System.Windows.Controls.Control c = (System.Windows.Controls.Control)item.Header;
                Config cfg = null;
                if (c is ConfigWPFRadioButton)
                {
                    ConfigWPFRadioButton cbox = (ConfigWPFRadioButton)c;
                    cfg = cbox.config;
                    if (cfg.Checked)
                        configSelected = true;
                    //create a section of code to run for only the last radioButton
                    if (cfg.name.Equals(lastConfigName) && !configSelected)
                    {
                        //last radioButton in the section, try to check at least one radioButton in the configs
                        foreach (System.Windows.Controls.TreeViewItem item2 in TVI.Items)
                        {
                            System.Windows.Controls.Control c2 = (System.Windows.Controls.Control)item2.Header;
                            if (c2 is ConfigWPFRadioButton)
                            {
                                ConfigWPFRadioButton c2r = (ConfigWPFRadioButton)c2;
                                cfg = c2r.config;
                                if (cfg.enabled)
                                {
                                    c2r.Click -= configControlRB_Click;
                                    c2r.IsChecked = true;
                                    cfg.Checked = true;
                                    configControlRB_Click(c2r, null);
                                    c2r.Click += configControlRB_Click;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        //when a legacy checkbox of OMC view is clicked
        void configControlCB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (loadingConfig)
                return;
            //checkboxes still don't need to be be unselected
            ConfigWPFCheckBox cb = (ConfigWPFCheckBox)sender;
            Mod m = cb.mod;
            Config cfg = cb.config;
            System.Windows.Controls.TreeViewItem tvi = (System.Windows.Controls.TreeViewItem)cb.Parent;
            cfg.Checked = (bool)cb.IsChecked;
            //propagate the check if required
            if (tvi.Parent is System.Windows.Controls.TreeViewItem && cfg.Checked)
            {
                System.Windows.Controls.TreeViewItem parenttvi = (System.Windows.Controls.TreeViewItem)tvi.Parent;
                if (parenttvi.Header is ModWPFCheckBox)
                {
                    ModWPFCheckBox c = (ModWPFCheckBox)parenttvi.Header;
                    c.IsChecked = true;
                    c.mod.Checked = true;
                    modCheckBoxL_Click(c, null);
                }
                else if (parenttvi.Header is ConfigWPFCheckBox)
                {
                    ConfigWPFCheckBox c = (ConfigWPFCheckBox)parenttvi.Header;
                    c.IsChecked = true;
                    c.mod.Checked = true;
                    configControlCB_Click(c, null);
                }
                else if (parenttvi.Header is ConfigWPFRadioButton)
                {
                    ConfigWPFRadioButton c = (ConfigWPFRadioButton)parenttvi.Header;
                    c.IsChecked = true;
                    c.mod.Checked = true;
                    configControlRB_Click(c, null);
                }
            }
            //process the subconfigs
            bool configSelected = false;
            int radioButtonCount = 0;
            if (cfg.configs.Count > 0 && cfg.Checked)
            {
                //determine if at least one radioButton is checked
                foreach (System.Windows.Controls.TreeViewItem subTVI in tvi.Items)
                {
                    if (subTVI.Header is ConfigWPFRadioButton)
                    {
                        radioButtonCount++;
                        ConfigWPFRadioButton subRB = (ConfigWPFRadioButton)subTVI.Header;
                        Config subc = subRB.config;
                        if ((bool)subRB.IsEnabled && (bool)subRB.IsChecked)
                        {
                            //getting here means cb is enabled
                            subRB.IsEnabled = true;
                            //this needs to be changed
                            //subRB_Click(subRB, null);
                            if (subc.Checked)
                                configSelected = true;
                        }
                    }
                }
                if (!configSelected && (bool)cb.IsChecked && (bool)cb.IsEnabled && radioButtonCount>0)
                {
                    foreach (System.Windows.Controls.TreeViewItem subTVI in tvi.Items)
                    {
                        if (subTVI.Header is ConfigWPFRadioButton)
                        {
                            ConfigWPFRadioButton subRB = (ConfigWPFRadioButton)subTVI.Header;
                            Config subc = subRB.config;
                            if ((bool)subRB.IsEnabled && subc.enabled)
                            {
                                subRB.IsChecked = true;
                                subc.Checked = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
        //when a dropdown legacy combobox is index changed
        void configControlDDALL_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            
            ConfigWPFComboBox cb = (ConfigWPFComboBox)sender;
            //first check if this is init, meaning first time enabled
            //but now this should never have to run
            /*if (cb.SelectedIndex == -1)
            {
                //it will run recurse with this method again with a selected index of 0
                cb.SelectedIndex = 0;
                return;
            }*/
            //getting here means that an item is confirmed to be selected
            //itterate through the items, get each config, disable it
            //enable the selected one at the end
            foreach (ComboBoxItem cbi in cb.Items)
            {
                cbi.config.Checked = false;
            }
            ComboBoxItem cbi2 = (ComboBoxItem)cb.SelectedItem;
            cbi2.config.Checked = true;
        }
        //when a radiobutton of the legacy view mode is clicked
        void configControlRB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (loadingConfig)
                return;
            //get all required cool stuff
            ConfigWPFRadioButton cb = (ConfigWPFRadioButton)sender;
            Mod m = cb.mod;
            Config cfg = cb.config;
            //the config treeview
            System.Windows.Controls.TreeViewItem tvi = (System.Windows.Controls.TreeViewItem)cb.Parent;
            //the mod treeview
            System.Windows.Controls.TreeViewItem item1 = (System.Windows.Controls.TreeViewItem)tvi.Parent;
            if ((bool)cb.IsEnabled && (bool)cb.IsChecked)
            {
                //uincheck all single and single1 mods
                foreach (System.Windows.Controls.TreeViewItem item in item1.Items)
                {
                    if (item.Header is ConfigWPFRadioButton)
                    {
                        ConfigWPFRadioButton rb = (ConfigWPFRadioButton)item.Header;
                        if ((bool)rb.IsEnabled && (bool)rb.IsChecked)
                        {
                            rb.Click -= configControlRB_Click;
                            rb.IsChecked = false;
                            rb.Click += configControlRB_Click;
                            rb.config.Checked = false;
                        }
                    }
                }
                cb.IsChecked = true;
                cb.config.Checked = true;
            }
            cfg.Checked = (bool)cb.IsChecked;
            //propagate the check if required
            if (tvi.Parent is System.Windows.Controls.TreeViewItem && cfg.Checked)
            {
                System.Windows.Controls.TreeViewItem parenttvi = (System.Windows.Controls.TreeViewItem)tvi.Parent;
                if (parenttvi.Header is ModWPFCheckBox)
                {
                    ModWPFCheckBox c = (ModWPFCheckBox)parenttvi.Header;
                    c.IsChecked = true;
                    c.mod.Checked = true;
                    modCheckBoxL_Click(c, null);
                }
                else if (parenttvi.Header is ConfigWPFCheckBox)
                {
                    ConfigWPFCheckBox c = (ConfigWPFCheckBox)parenttvi.Header;
                    c.IsChecked = true;
                    c.mod.Checked = true;
                    configControlCB_Click(c, null);
                }
                else if (parenttvi.Header is ConfigWPFRadioButton)
                {
                    ConfigWPFRadioButton c = (ConfigWPFRadioButton)parenttvi.Header;
                    c.IsChecked = true;
                    c.mod.Checked = true;
                    configControlRB_Click(c, null);
                }
            }
            //process the subconfigs
            bool configSelected = false;
            int radioButtonCount = 0;
            if (cfg.configs.Count > 0 && cfg.Checked)
            {
                //determine if at least one radioButton is checked
                foreach (System.Windows.Controls.TreeViewItem subTVI in tvi.Items)
                {
                    if (subTVI.Header is ConfigWPFRadioButton)
                    {
                        radioButtonCount++;
                        ConfigWPFRadioButton subRB = (ConfigWPFRadioButton)subTVI.Header;
                        Config subc = subRB.config;
                        if ((bool)subRB.IsEnabled && (bool)subRB.IsChecked)
                        {
                            //getting here means cb is enabled
                            subRB.IsEnabled = true;
                            //this needs to be changed
                            //subRB_Click(subRB, null);
                            if (subc.Checked)
                                configSelected = true;
                        }
                    }
                }
                if (!configSelected && (bool)cb.IsChecked && (bool)cb.IsEnabled && radioButtonCount > 0)
                {
                    foreach (System.Windows.Controls.TreeViewItem subTVI in tvi.Items)
                    {
                        if (subTVI.Header is ConfigWPFRadioButton)
                        {
                            ConfigWPFRadioButton subRB = (ConfigWPFRadioButton)subTVI.Header;
                            Config subc = subRB.config;
                            if ((bool)subRB.IsEnabled && subc.enabled)
                            {
                                subRB.IsChecked = true;
                                subc.Checked = true;
                                configControlRB_Click(subRB, null);
                                break;
                            }
                        }
                    }
                }
            }
        }

        void modCheckBoxL_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //we only care about the right mouse click tho
            if (e.RightButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                return;
            }
            if (sender is ModWPFCheckBox)
            {
                ModWPFCheckBox cb = (ModWPFCheckBox)sender;
                Mod m = cb.mod;
                if (m.devURL == null)
                    m.devURL = "";
                p.Close();
                p = new Preview(m.name, m.pictureList, m.description, m.updateComment, m.devURL);
                p.Show();
            }
        }

        void configControlDDALL_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                return;
            }
            ConfigWPFComboBox cb = (ConfigWPFComboBox)sender;
            ComboBoxItem cbi = (ComboBoxItem)cb.SelectedItem;
            Config m = cbi.config;
            if (m.devURL == null)
                m.devURL = "";
            p.Close();
            p = new Preview(m.name, m.pictureList, m.description, m.updateComment, m.devURL);
            p.Show();
        }

        void configControlCB_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                return;
            }
            ConfigWPFCheckBox cb = (ConfigWPFCheckBox)sender;
            Config m = cb.config;
            if (m.devURL == null)
                m.devURL = "";
            p.Close();
            p = new Preview(m.name, m.pictureList, m.description, m.updateComment, m.devURL);
            p.Show();
        }

        void configControlRB_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                return;
            }
            ConfigWPFRadioButton cb = (ConfigWPFRadioButton)sender;
            Config m = cb.config;
            if (m.devURL == null)
                m.devURL = "";
            p.Close();
            p = new Preview(m.name, m.pictureList, m.description, m.updateComment, m.devURL);
            p.Show();
        }
        
        //adds a mod m to a tabpage t
        private void addMod(Mod m, TabPage t, int panelCount, Category catagory)
        {
            //bool for keeping track if a radioButton config has been selected
            hasRadioButtonConfigSelected = false;
            modHasRadioButtons = false;
            //make the mod check box
            ModFormCheckBox modCheckBox = new ModFormCheckBox();
            modCheckBox.AutoSize = true;
            modCheckBox.Location = new System.Drawing.Point(3, 3);
            modCheckBox.Size = new System.Drawing.Size(49, 15);
            modCheckBox.TabIndex = 1;
            modCheckBox.Text = m.name;
            modCheckBox.Name = t.Name + "_" + m.name;
            modCheckBox.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
            modCheckBox.catagory = catagory;
            modCheckBox.mod = m;
            m.modFormCheckBox = modCheckBox;
            //the mod checksum logic
            string modDownloadPath = Application.StartupPath + "\\RelHaxDownloads\\" + m.zipFile;
            string oldCRC2 = Utils.getMd5Hash(modDownloadPath);
            //if the CRC's don't match and the mod actually has a zip file
            if (!(m.crc.Equals(oldCRC2)) && (!m.zipFile.Equals("")))
            {
                modCheckBox.Text = modCheckBox.Text + " (Updated)";
                m.downloadFlag = true;
                if ((m.size > 0.0f))
                    modCheckBox.Text = modCheckBox.Text + " (" + m.size + " MB)";
            }
            modCheckBox.UseVisualStyleBackColor = true;
            modCheckBox.Enabled = m.enabled;
            modCheckBox.MouseDown += new MouseEventHandler(modCheckBox_MouseDown);
            //in theory it should trigger the handler for checked
            //when initially made it should be false, if enabled from
            //from user configs
            //make the main panel
            Panel mainPanel = new Panel();
            mainPanel.BorderStyle = BorderStyle.FixedSingle;
            mainPanel.TabIndex = 0;
            mainPanel.AutoSize = true;
            mainPanel.AutoSizeMode = AutoSizeMode.GrowOnly;
            mainPanel.Size = new System.Drawing.Size(t.Size.Width - 25, 20);
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
            //processes the subconfigs here
            mainPanel.Controls.Clear();
            //add to main panel
            mainPanel.Controls.Add(modCheckBox);
            if (m.configs.Count > 0)
                processConfigsDefault(t, m, catagory, modCheckBox, mainPanel, true,m.configs, mainPanel);
            //add to tab
            t.Controls.Add(mainPanel);
            //add the event handler before changing the checked state so the event
            //event handler is #triggered
            modCheckBox.Checked = m.Checked;
            //disable the logic for now
            if (false && modHasRadioButtons && modCheckBox.Checked && !hasRadioButtonConfigSelected)
            {
                //getting here means that the user has saved the prefrence for a selected mandatory radiobutton config that has been disabled, so his selection of that mod needs to be disabled
                modCheckBox.Checked = false;
                m.Checked = false;
            }
            modCheckBox.CheckedChanged += new EventHandler(modCheckBox_CheckedChanged);
        }
        private void processConfigsDefault(TabPage t, Mod m, Category catagory, ModFormCheckBox modCheckBox, Panel mainPanel, bool parentIsMod, List<Config> configs, Panel topPanal, Config parentConfig = null)
        {
            //make config panel
            Panel configPanel = new Panel();
            configPanel.Enabled = true;
            configPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            configPanel.Location = new System.Drawing.Point(3, 10);
            configPanel.TabIndex = 2;
            configPanel.Size = new System.Drawing.Size(t.Size.Width - 35, 30);
            configPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            configPanel.AutoSize = true;
            if (parentIsMod)
            {
                if (m.enabled && m.Checked)
                    configPanel.BackColor = Color.BlanchedAlmond;
                else
                    configPanel.BackColor = Settings.getBackColor();
            }
            else
            {
                if (parentConfig.enabled && parentConfig.Checked)
                    configPanel.BackColor = Color.BlanchedAlmond;
                else
                    configPanel.BackColor = Settings.getBackColor();
            }
            int spacer = modCheckBox.Location.Y + modCheckBox.Size.Height + 5;
            if (Settings.largeFont) spacer += 3;
            if (parentIsMod)
            {
                configPanel.Location = new Point(configPanel.Location.X + 10, spacer);
            }
            else
            {
                configPanel.Location = new Point(configPanel.Location.X + 10, getYLocation(mainPanel.Controls));
            }
            mainPanel.Controls.Add(configPanel);
            //create the comboBox outside of the loop
            //later add it if the items count is above 0
            ConfigFormComboBox configControlDD = new ConfigFormComboBox();
            configControlDD.AutoSize = true;
            configControlDD.Location = new System.Drawing.Point(0, 0);
            configControlDD.Size = new System.Drawing.Size(225, 15);
            configControlDD.TabIndex = 1;
            configControlDD.TabStop = true;
            configControlDD.Enabled = false;
            configControlDD.SelectedIndexChanged += new EventHandler(configControlDD_SelectedIndexChanged);
            configControlDD.MouseDown += new MouseEventHandler(configControlDD_MouseDown);
            configControlDD.Name = t.Name + "_" + m.name + "_DropDown";
            configControlDD.DropDownStyle = ComboBoxStyle.DropDownList;
            ConfigFormComboBox configControlDD2 = new ConfigFormComboBox();
            configControlDD2.AutoSize = true;
            configControlDD2.Location = new System.Drawing.Point(0, 0);
            configControlDD2.Size = new System.Drawing.Size(225, 15);
            configControlDD2.TabIndex = 1;
            configControlDD2.TabStop = true;
            configControlDD2.Enabled = false;
            configControlDD2.SelectedIndexChanged += new EventHandler(configControlDD_SelectedIndexChanged);
            configControlDD2.MouseDown += new MouseEventHandler(configControlDD_MouseDown);
            configControlDD2.Name = t.Name + "_" + m.name + "_DropDown2";
            configControlDD2.DropDownStyle = ComboBoxStyle.DropDownList;
            foreach (Config con in configs)
            {
                ConfigFormComboBox configControlDDALL = null;
                con.parentMod = m;
                if (parentIsMod)
                {
                    con.parent = m;
                }
                else
                {
                    con.parent = parentConfig;
                }
                if (con.type.Equals("single") || con.type.Equals("single1"))
                {
                    modHasRadioButtons = true;
                    //make default radioButton
                    ConfigFormRadioButton configControlRB = new ConfigFormRadioButton();
                    configControlRB.AutoSize = true;
                    configControlRB.Location = new Point(6, getYLocation(configPanel.Controls));
                    configControlRB.Size = new System.Drawing.Size(150, 15);
                    configControlRB.TabIndex = 1;
                    configControlRB.TabStop = true;
                    configControlRB.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
                    configControlRB.catagory = catagory;
                    configControlRB.mod = m;
                    configControlRB.config = con;
                    con.configUIComponent = configControlRB;
                    //logic for radiobutton
                    configControlRB.Enabled = false;
                    configControlRB.Checked = false;
                    if (parentIsMod)
                    {
                        if (m.enabled && con.enabled)
                        {
                            configControlRB.Enabled = true;
                            if (m.Checked && con.Checked)
                            {
                                configControlRB.Checked = true;
                                hasRadioButtonConfigSelected = true;
                            }
                        }
                    }
                    else
                    {
                        if (parentConfig.enabled && con.enabled)
                        {
                            configControlRB.Enabled = true;
                            if (parentConfig.Checked && con.Checked)
                            {
                                configControlRB.Checked = true;
                                hasRadioButtonConfigSelected = true;
                            }
                        }
                    }
                    //add handlers
                    configControlRB.CheckedChanged += new EventHandler(configControlRB_CheckedChanged);
                    configControlRB.MouseDown += new MouseEventHandler(configControlRB_MouseDown);
                    configControlRB.Name = t.Name + "_" + m.name + "_" + con.name;
                    //run checksum logic
                    configControlRB.Text = con.name;
                    string oldCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + con.zipFile);
                    if (!oldCRC.Equals(con.crc) && (!con.crc.Equals("")))
                    {
                        configControlRB.Text = configControlRB.Text + " (Updated)";
                        con.downloadFlag = true;
                        if (con.size > 0.0f)
                            configControlRB.Text = configControlRB.Text + " (" + con.size + " MB)";
                    }
                    //add the config to the form
                    configPanel.Controls.Add(configControlRB);
                    //process the subconfigs
                    if(con.configs.Count > 0)
                    processConfigsDefault(t, m, catagory, modCheckBox, configPanel, false, con.configs, topPanal, con);
                }
                else if (con.type.Equals("single_dropdown") || con.type.Equals("single_dropdown1") || con.type.Equals("single_dropdown2"))
                {
                    //set the all one to the version is actually is
                    if (con.type.Equals("single_dropdown") || con.type.Equals("single_dropdown1"))
                    {
                        configControlDDALL = configControlDD;
                    }
                    else if (con.type.Equals("single_dropdown2"))
                    {
                        configControlDDALL = configControlDD2;
                    }
                    //make a dropDown selection box
                    if (configControlDDALL.Location.X == 0 && configControlDDALL.Location.Y == 0)
                    {
                        //init the box, including adding the label
                        configControlDDALL.Location = new System.Drawing.Point(6, getYLocation(configPanel.Controls));
                        configPanel.Controls.Add(configControlDDALL);
                    }
                    //run the checksum locics
                    string oldCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + con.zipFile);
                    if (!oldCRC.Equals(con.crc) && (!con.crc.Equals("")))
                    {
                        con.downloadFlag = true;
                        string toAdd = con.name + "_Updated";
                        if (con.size > 0.0f)
                            toAdd = toAdd + " (" + con.size + " MB)";
                        //add it with _updated
                        if (con.enabled) configControlDDALL.Items.Add(new ComboBoxItem(con, toAdd));
                        if (con.Checked)
                        {
                            configControlDDALL.SelectedItem = toAdd;
                            configControlDDALL.Enabled = true;
                        }
                    }
                    else
                    {
                        //add it
                        if (con.enabled) configControlDDALL.Items.Add(new ComboBoxItem(con, con.name));
                        if (con.Checked)
                        {
                            configControlDDALL.SelectedItem = con.name;
                            configControlDDALL.Enabled = true;
                        }
                    }
                    if(configControlDDALL.Items.Count > 0)
                        configControlDDALL.Enabled = true;
                    if(configControlDDALL.SelectedIndex == -1)
                        configControlDDALL.SelectedIndex = 0;
                }
                else if (con.type.Equals("multi"))
                {
                    //make a checkBox
                    ConfigFormCheckBox configControlCB = new ConfigFormCheckBox();
                    configControlCB.AutoSize = true;
                    configControlCB.Location = new Point(6, getYLocation(configPanel.Controls));
                    configControlCB.Size = new System.Drawing.Size(150, 15);
                    configControlCB.TabIndex = 1;
                    configControlCB.TabStop = true;
                    configControlCB.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
                    configControlCB.catagory = catagory;
                    configControlCB.mod = m;
                    configControlCB.config = con;
                    //the logic for enabling it
                    configControlCB.Enabled = false;
                    configControlCB.Checked = false;
                    if (parentIsMod)
                    {
                        if (m.enabled && con.enabled)
                        {
                            configControlCB.Enabled = true;
                            if (m.Checked && con.Checked)
                            {
                                configControlCB.Checked = true;
                                hasRadioButtonConfigSelected = true;
                            }
                        }
                    }
                    else
                    {
                        if (parentConfig.enabled && con.enabled)
                        {
                            configControlCB.Enabled = true;
                            if (parentConfig.Checked && con.Checked)
                            {
                                configControlCB.Checked = true;
                                hasRadioButtonConfigSelected = true;
                            }
                        }
                    }
                    //add handlers
                    configControlCB.CheckedChanged += new EventHandler(configControlCB_CheckedChanged);
                    configControlCB.MouseDown += new MouseEventHandler(configControlCB_MouseDown);
                    configControlCB.Name = t.Name + "_" + m.name + "_" + con.name;
                    //checksum logic
                    configControlCB.Text = con.name;
                    string oldCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + con.zipFile);
                    if (!oldCRC.Equals(con.crc) && (!con.crc.Equals("")))
                    {
                        con.downloadFlag = true;
                        configControlCB.Text = configControlCB.Text + " (Updated)";
                        if (con.size > 0.0f)
                            configControlCB.Text = configControlCB.Text + " (" + con.size + " MB)";
                    }
                    //add config to the form
                    configPanel.Controls.Add(configControlCB);
                    //process subconfigs
                    if (con.configs.Count > 0)
                        processConfigsDefault(t, m, catagory, modCheckBox, configPanel, false, con.configs, topPanal ,con);
                }
                else
                {
                    Utils.appendToLog("WARNING: Unknown config type for " + con.name + ": " + con.type);
                }
            }
        }
        //method for finding the location of which to put a control
        private int getYLocation(System.Windows.Forms.Control.ControlCollection ctrl)
        {
            int y = 0;
            //the first 5 pixels to give it room
            y += 5;
            //only look for the dropDown menu options or checkboxes
            foreach (Control c in ctrl)
            {
                if ((c is ConfigFormCheckBox) || (c is ConfigFormComboBox) || (c is Panel) || (c is ConfigFormRadioButton) || (c is ModFormCheckBox))
                {
                    y += c.Size.Height;
                    //spacing
                    y += 2;
                }
            }
            return y;
        }
        //handler for when a mod checkbox is changed
        void modCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (loadingConfig)
                return;
            if (sender is CheckBox)
            {
                //user mod code
                CheckBox cbUser = (CheckBox)sender;
                if (cbUser.Parent is TabPage)
                {
                    TabPage t = (TabPage)cbUser.Parent;
                    if (t.Text.Equals("User Mods"))
                    {
                        //this is a check from the user checkboxes
                        Mod m2 = Utils.getUserMod(cbUser.Text, userMods);
                        if (m2 != null)
                            m2.Checked = cbUser.Checked;
                        return;
                    }
                }
            }
            //the mod info handler should only be concerned with checking for enabling componets

            //get all required info for this checkbox change
            ModFormCheckBox cb = (ModFormCheckBox)sender;
            //this parent could nwo be the radioButton config selection panel or the config panel
            Panel modPanel = (Panel)cb.Parent;
            TabPage modTab = (TabPage)modPanel.Parent;
            Mod m = cb.mod;
            Category cat = cb.catagory;

            //check to see if the mod is part of a single selection only catagory
            //if it is uncheck the other mods first, then deal with mod loop selection
            if (cat.selectionType.Equals("single") && cb.Checked)
            {
                //check if any other mods in this catagory are already checked
                bool anyModsChecked = false;
                foreach (Mod mm in cat.mods)
                {
                    if (mm.Checked)
                    {
                        anyModsChecked = true;
                        mm.Checked = false;
                    }
                }
                if (anyModsChecked)
                {
                    //not safe to check the mod
                    //uncheck the other mod first
                    //each checkbox uncheck it
                    foreach (var cc in modTab.Controls)
                    {
                        if (cc is Panel)
                        {
                            //it's a mod panel
                            Panel pp = (Panel)cc;
                            foreach (var ccc in pp.Controls)
                            {
                                if (ccc is ModFormCheckBox)
                                {
                                    ModFormCheckBox cbb = (ModFormCheckBox)ccc;
                                    //disable the other mods
                                    //cbb.CheckedChanged -= modCheckBox_CheckedChanged;
                                    cbb.Checked = false;
                                    cbb.mod.Checked = false;
                                    //modCheckBox_CheckedChanged(cbb, null);
                                    //cbb.mod.Checked = false;
                                    //cbb.CheckedChanged += modCheckBox_CheckedChanged;
                                }
                            }
                        }
                    }
                    //now it's safe to check the mods
                    //remove the handler for a sec to prevent stack overflow
                    cb.CheckedChanged -= modCheckBox_CheckedChanged;
                    cb.Checked = true;
                    cb.mod.Checked = true;
                    cb.CheckedChanged += modCheckBox_CheckedChanged;
                    //modCheckBox.Checked = false;
                }

            }
            //toggle the mod in memory, enabled or disabled
            m.Checked = cb.Checked;
            //this deals with enabling the componets and triggering the handlers
            if (m.configs.Count == 0)
                return;
            string lastConfigName = "null";
            bool configSelected = false;
            foreach (Config configs in m.configs)
            {
                if (configs.type.Equals("single") || configs.type.Equals("single1"))
                {
                    lastConfigName = configs.name;
                }
            }
            //checkbox and dropDownList, including if loading a config, if there are any
            //the first one is always the mod checkbox
            //the second one is always the config panel
            Panel configPanel = (Panel)modPanel.Controls[1];
            if (cb.Checked) configPanel.BackColor = Color.BlanchedAlmond;
            else configPanel.BackColor = Settings.getBackColor();
            foreach (Control cc in configPanel.Controls)
            {
                Config cfg = null;
                if (cc is ConfigFormRadioButton)
                {
                    ConfigFormRadioButton ccRB = (ConfigFormRadioButton)cc;
                    cfg = ccRB.config;
                    if (cfg.Checked)
                        configSelected = true;
                    //create a section of code to run fo only the last radioButton
                    if (cfg.name.Equals(lastConfigName) && !configSelected)
                    {
                        //last radioButton in the section, try to check the first radioButton
                        foreach (Control cccc in configPanel.Controls)
                        {
                            if (cccc is ConfigFormRadioButton)
                            {
                                ConfigFormRadioButton ccccc = (ConfigFormRadioButton)cccc;
                                cfg = ccccc.config;
                                if (cfg.enabled)
                                {
                                    ccccc.CheckedChanged -= configControlRB_CheckedChanged;
                                    ccccc.Checked = true;
                                    cfg.Checked = true;
                                    configControlRB_CheckedChanged(ccccc, null);
                                    ccccc.CheckedChanged += configControlRB_CheckedChanged;
                                    break;
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
            if (loadingConfig)
                return;
            //checkboxes don't need to be unselected
            ConfigFormCheckBox cb = (ConfigFormCheckBox)sender;
            Mod m = cb.mod;
            Config cfg = cb.config;
            Category cat = cb.catagory;
            Panel configPanel = (Panel)cb.Parent;
            //checkbox is enabled, toggle checked and checked
            cfg.Checked = cb.Checked;
            //propagate the check back up if required
            DatabaseObject obj = cfg.parent;
            if (obj is Mod)
            {
                Mod parentM = (Mod)obj;
                //parentM.modFormCheckBox.CheckedChanged -= modCheckBox_CheckedChanged;
                parentM.modFormCheckBox.Checked = true;
                //parentM.modFormCheckBox.CheckedChanged += modCheckBox_CheckedChanged;
            }
            else if (obj is Config)
            {
                Config parentC = (Config)obj;
                parentC.Checked = true;
                if (parentC.configUIComponent is ConfigFormCheckBox)
                {
                    ConfigFormCheckBox parentCB = (ConfigFormCheckBox)parentC.configUIComponent;
                    parentCB.Checked = true;
                }
                else if (parentC.configUIComponent is ConfigFormRadioButton)
                {
                    ConfigFormRadioButton parentRB = (ConfigFormRadioButton)parentC.configUIComponent;
                    parentRB.Checked = true;
                }
            }
            //process any subconfigs
            bool configSelected = false;
            int radioButtonCount = 0;
            if (cfg.configs.Count > 0 && cb.Checked)
            {
                foreach (Config c in cfg.configs)
                {
                    if (c.type.Equals("single") || c.type.Equals("single1"))
                    {
                        radioButtonCount++;
                        if (c.Checked)
                            configSelected = true;
                    }
                }
                if (!configSelected && radioButtonCount > 0)
                {
                    //select the first one and leave
                    foreach (Config c in cfg.configs)
                    {
                        if ((c.type.Equals("single") || c.type.Equals("single1")) && c.enabled)
                        {
                            c.Checked = true;
                            ConfigFormRadioButton subRB = (ConfigFormRadioButton)c.configUIComponent;
                            subRB.Checked = true;
                            break;
                        }
                    }
                }
            }
            //trigger the panel color change
            if (cfg.configs.Count > 0)
            {
                if (cb.Checked)
                {
                    UIComponent comp = cfg.configs[0].configUIComponent;
                    if (comp is ConfigFormCheckBox)
                    {
                        ConfigFormCheckBox fcb = (ConfigFormCheckBox)comp;
                        Panel pan = (Panel)fcb.Parent;
                        pan.BackColor = Color.BlanchedAlmond;
                    }
                    else if (comp is ConfigFormRadioButton)
                    {
                        ConfigFormRadioButton fcb = (ConfigFormRadioButton)comp;
                        Panel pan = (Panel)fcb.Parent;
                        pan.BackColor = Color.BlanchedAlmond;
                    }
                }
                else
                {
                    UIComponent comp = cfg.configs[0].configUIComponent;
                    if (comp is ConfigFormCheckBox)
                    {
                        ConfigFormCheckBox fcb = (ConfigFormCheckBox)comp;
                        Panel pan = (Panel)fcb.Parent;
                        pan.BackColor = Settings.getBackColor();
                    }
                    else if (comp is ConfigFormRadioButton)
                    {
                        ConfigFormRadioButton fcb = (ConfigFormRadioButton)comp;
                        Panel pan = (Panel)fcb.Parent;
                        pan.BackColor = Settings.getBackColor();
                    }
                }
            }
        }
        //handler for when a config selection is made from the drop down list
        void configControlDD_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            //uncheck all other dorp down configs
            ConfigFormComboBox cb = (ConfigFormComboBox)sender;
            //if no index selected, select one
            //does not need to run this anymore tho
            /*if (cb.SelectedIndex == -1)
            {
                cb.SelectedIndex = 0;
                return;
            }*/
            //itterate through the items, get each config, disable it
            //unless it's the same name as the selectedItem
            foreach (ComboBoxItem cbi in cb.Items)
            {
                cbi.config.Checked = false;
            }
            ComboBoxItem cbi2 = (ComboBoxItem)cb.SelectedItem;
            cbi2.config.Checked = true;
        }
        //handler for when a config radioButton is pressed
        void configControlRB_CheckedChanged(object sender, EventArgs e)
        {
            if (loadingConfig)
                return;
            //get all required cool stuff
            ConfigFormRadioButton rb = (ConfigFormRadioButton)sender;
            Panel configPanel = (Panel)rb.Parent;
            Mod m = rb.mod;
            Config cfg = rb.config;
            Category cat = rb.catagory;
            cfg.Checked = rb.Checked;
            //propagate the check back up if required
            DatabaseObject obj = cfg.parent;
            if (obj is Mod)
            {
                Mod parentM = (Mod)obj;
                //parentM.modFormCheckBox.CheckedChanged -= modCheckBox_CheckedChanged;
                parentM.modFormCheckBox.Checked = true;
                //parentM.modFormCheckBox.CheckedChanged += modCheckBox_CheckedChanged;
            }
            else if (obj is Config)
            {
                Config parentC = (Config)obj;
                parentC.Checked = true;
                if (parentC.configUIComponent is ConfigFormCheckBox)
                {
                    ConfigFormCheckBox parentCB = (ConfigFormCheckBox)parentC.configUIComponent;
                    parentCB.Checked = true;
                }
                else if (parentC.configUIComponent is ConfigFormRadioButton)
                {
                    ConfigFormRadioButton parentRB = (ConfigFormRadioButton)parentC.configUIComponent;
                    parentRB.Checked = true;
                }
            }
            //process any subconfigs
            bool configSelected = false;
            int radioButtonCount = 0;
            if (cfg.configs.Count > 0 && rb.Checked)
            {
                foreach (Config c in cfg.configs)
                {
                    if (c.type.Equals("single") || c.type.Equals("single1"))
                    {
                        radioButtonCount++;
                        if (c.Checked)
                            configSelected = true;
                    }
                }
                if (!configSelected && radioButtonCount > 0)
                {
                    //select the first one and leave
                    foreach (Config c in cfg.configs)
                    {
                        if ((c.type.Equals("single") || c.type.Equals("single1")) && c.enabled)
                        {
                            c.Checked = true;
                            ConfigFormRadioButton subRB = (ConfigFormRadioButton)c.configUIComponent;
                            subRB.Checked = true;
                            break;
                        }
                    }
                }
            }
            //trigger the panel color change
            if (cfg.configs.Count > 0)
            {
                if (rb.Checked)
                {
                    UIComponent comp = cfg.configs[0].configUIComponent;
                    if (comp is ConfigFormCheckBox)
                    {
                        ConfigFormCheckBox fcb = (ConfigFormCheckBox)comp;
                        Panel pan = (Panel)fcb.Parent;
                        pan.BackColor = Color.BlanchedAlmond;
                    }
                    else if (comp is ConfigFormRadioButton)
                    {
                        ConfigFormRadioButton fcb = (ConfigFormRadioButton)comp;
                        Panel pan = (Panel)fcb.Parent;
                        pan.BackColor = Color.BlanchedAlmond;
                    }
                }
                else
                {
                    UIComponent comp = cfg.configs[0].configUIComponent;
                    if (comp is ConfigFormCheckBox)
                    {
                        ConfigFormCheckBox fcb = (ConfigFormCheckBox)comp;
                        Panel pan = (Panel)fcb.Parent;
                        pan.BackColor = Settings.getBackColor();
                    }
                    else if (comp is ConfigFormRadioButton)
                    {
                        ConfigFormRadioButton fcb = (ConfigFormRadioButton)comp;
                        Panel pan = (Panel)fcb.Parent;
                        pan.BackColor = Settings.getBackColor();
                    }
                }
            }
        }
        //hander for when any mouse button is clicked on a specific control
        void modCheckBox_MouseDown(object sender, MouseEventArgs e)
        {
            //we only care about the right mouse click tho
            if (e.Button != MouseButtons.Right)
                return;
            if (sender is ModFormCheckBox)
            {
                ModFormCheckBox cb = (ModFormCheckBox)sender;
                Mod m = cb.mod;
                if (m.devURL == null)
                    m.devURL = "";
                p.Close();
                p = new Preview(m.name, m.pictureList, m.description, m.updateComment, m.devURL);
                p.Show();
            }
        }

        void configControlCB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            ConfigFormCheckBox cb = (ConfigFormCheckBox)sender;
            Config m = cb.config;
            if (m.devURL == null)
                m.devURL = "";
            p.Close();
            p = new Preview(m.name, m.pictureList, m.description, m.updateComment, m.devURL);
            p.Show();
        }

        void configControlDD_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            ConfigFormComboBox cb = (ConfigFormComboBox)sender;
            ComboBoxItem cbi = (ComboBoxItem)cb.SelectedItem;
            Config m = cbi.config;
            if (m.devURL == null)
                m.devURL = "";
            p.Close();
            p = new Preview(m.name, m.pictureList, m.description, m.updateComment, m.devURL);
            p.Show();
        }

        void configControlRB_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;
            ConfigFormRadioButton cb = (ConfigFormRadioButton)sender;
            Config m = cb.config;
            if (m.devURL == null)
                m.devURL = "";
            p.Close();
            p = new Preview(m.name, m.pictureList, m.description, m.updateComment, m.devURL);
            p.Show();
        }
        //resizing handler for the window
        private void ModSelectionList_SizeChanged(object sender, EventArgs e)
        {
            continueButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - difference);
            cancelButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - difference);
            modTabGroups.Size = new Size(this.Size.Width - 20 - modTabGroups.Location.X, this.Size.Height - modTabGroups.Location.Y - 39 - continueButton.Size.Height - 6 - difference);
            label1.Text = "" + this.Size.Width + " x " + this.Size.Height;
            loadConfigButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width - 6 - loadConfigButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - difference);
            saveConfigButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - difference);
            clearSelectionsButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width - 6 - loadConfigButton.Size.Width - 6 - clearSelectionsButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - difference);
            if (this.Size.Height < 250) this.Size = new Size(this.Size.Width, 250);
            if (this.Size.Width < 550) this.Size = new Size(550, this.Size.Height);
            foreach (TabPage t in modTabGroups.TabPages)
            {
                foreach (Control c in t.Controls)
                {
                    if (c is Panel)
                    {
                        //mod panel
                        //TODO RECURSIVLY
                        Panel p = (Panel)c;
                        resizePanel(p, t, 25);
                    }
                    else if (c is ElementHost)
                    {
                        ElementHost eh = (ElementHost)c;
                        eh.Size = new Size(t.Size.Width - 12, t.Size.Height - 10);
                        LegacySelectionList lsl = (LegacySelectionList)eh.Child;
                        lsl.RenderSize = new System.Windows.Size(eh.Size.Width - 2, eh.Size.Height - 2);
                        lsl.legacyTreeView.Width = eh.Size.Width - 4;
                        lsl.legacyTreeView.Height = eh.Size.Height - 4;
                    }
                }
            }
        }
        //recursive resize of the control panals
        private void resizePanel(Panel current, TabPage tp ,int shrinkFactor)
        {
            current.Size = new Size(tp.Size.Width - shrinkFactor, current.Size.Height);
            foreach (Control controfds in current.Controls)
            {
                if (controfds is Panel)
                {
                    Panel subpp = (Panel)controfds;
                    resizePanel(subpp, tp, shrinkFactor+25);
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
                Utils.saveConfig(false,parsedCatagoryList,userMods);
            }
            this.Close();
        }
        //handler for when the cancal button is clicked
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //handler for when the "load config" button is pressed
        private void loadConfigButton_Click(object sender, EventArgs e)
        {
            loadMode = loadConfigMode.fromButton;
            this.parseLoadConfig();
        }
        //handler for when the "save config" button is pressed
        private void saveConfigButton_Click(object sender, EventArgs e)
        {
            Utils.saveConfig(true,parsedCatagoryList,userMods);
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
        //handler for when the close button is pressed
        private void ModSelectionList_FormClosing(object sender, FormClosingEventArgs e)
        {
            //save the size of this window for later.
            Settings.modSelectionHeight = this.Size.Height;
            Settings.modSelectionWidth = this.Size.Width;
            if (taskBarHidden)
                Settings.SetTaskbarState(Settings.AppBarStates.AutoHide);
            //save wether the window was in fullscreen mods before closing
            if (this.WindowState == FormWindowState.Maximized)
                Settings.ModSelectionFullscreen = true;
            else
                Settings.ModSelectionFullscreen = false;
            //close the preview window if it is open
            if (p != null)
                p.Close();
        }

        private void clearSelectionsButton_Click(object sender, EventArgs e)
        {
            Utils.clearSelectionMemory(parsedCatagoryList);
            Utils.appendToLog("clearSelectionsButton pressed, clearing selections");
            MessageBox.Show(Translations.getTranslatedString("selectionsCleared"));
            //reload the UI
            this.UseWaitCursor = true;
            modTabGroups.Enabled = false;
            this.makeTabs();
            Settings.setUIColor(this);
            this.addAllMods();
            this.addUserMods();
            Settings.setUIColor(this);
            this.UseWaitCursor = false;
            modTabGroups.Enabled = true;
            ModSelectionList_SizeChanged(null, null);
        }
        private void parseLoadConfig()
        {
            loadingConfig = true;
            OpenFileDialog loadLocation = new OpenFileDialog();
            string filePath = "";
            if (loadMode == loadConfigMode.fromAutoInstall)
            {
                filePath = Application.StartupPath + "\\RelHaxUserConfigs\\" + Program.configName;
                if (!File.Exists(filePath))
                {
                    Utils.appendToLog("ERROR: " + filePath + " not found, not loading configs");
                    MessageBox.Show(Translations.getTranslatedString("configLoadFailed"));
                    return;
                }
            }
            else if (loadMode == loadConfigMode.fromSaveLastConfig)
            {
                filePath = Application.StartupPath + "\\RelHaxUserConfigs\\lastInstalledConfig.xml";
                if (!File.Exists(filePath))
                {
                    Utils.appendToLog("ERROR: " + filePath + " not found, not loading configs");
                    //MessageBox.Show(Translations.getTranslatedString("configLoadFailed"));
                    return;
                }
            }
            else
            {
                loadLocation.AddExtension = true;
                loadLocation.DefaultExt = ".xml";
                loadLocation.Filter = "*.xml|*.xml";
                loadLocation.InitialDirectory = Application.StartupPath + "\\RelHaxUserConfigs";
                loadLocation.Title = Translations.getTranslatedString("selectConfigFile");
                if (loadLocation.ShowDialog().Equals(DialogResult.Cancel))
                {
                    //quit
                    return;
                }
                filePath = loadLocation.FileName;
            }
            Utils.loadConfig(filePath, parsedCatagoryList, userMods);
            if (loadMode == loadConfigMode.fromButton || loadMode == loadConfigMode.fromAutoInstall)
            {
                if (loadMode == loadConfigMode.fromButton) MessageBox.Show(Translations.getTranslatedString("prefrencesSet"));
                //reload the UI
                this.UseWaitCursor = true;
                modTabGroups.Enabled = false;
                this.makeTabs();
                Settings.setUIColor(this);
                this.addAllMods();
                this.addUserMods();
                Settings.setUIColor(this);
                this.UseWaitCursor = false;
                modTabGroups.Enabled = true;
                ModSelectionList_SizeChanged(null, null);
            }
        }
    }
}
