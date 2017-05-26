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
        public List<Catagory> parsedCatagoryList;//can be grabbed by MainWindow
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
            parsedCatagoryList = new List<Catagory>();
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
                this.loadConfig(loadMode);
            }
            else if (loadMode == loadConfigMode.fromAutoInstall)
            {
                this.loadConfig(loadMode);
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
            Utils.appendToLog("Loading ModSelectionList with view " + Settings.sView);
            foreach (TabPage t in this.modTabGroups.TabPages)
            {
                foreach (Catagory c in parsedCatagoryList)
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
        //adds a mod m to a tabpage t, OMC treeview style
        private void addModTreeview(Mod m, TabPage t, int panelCount, LegacySelectionList lsl, Catagory c)
        {
            if (Settings.darkUI)
                lsl.legacyTreeView.Background = System.Windows.Media.Brushes.Gray;
            //helpfull stuff
            string modDownloadFilePath = Application.StartupPath + "\\RelHaxDownloads\\" + m.modZipFile;
            bool hasRadioButtonConfigSelected = false;
            bool modHasRadioButtons = false;
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
            modCheckBox.Content = m.name;
            //get the local md5 hash. a -1 indicates the file is not on the disk
            string oldCRC2 = Utils.getMd5Hash(modDownloadFilePath);
            //if the CRC's don't match and the mod actually has a zip file
            if (!(m.crc.Equals(oldCRC2)) && (!m.modZipFile.Equals("")))
            {
                modCheckBox.Content = modCheckBox.Content + " (Updated)";
                if ((m.size > 0.0f) )
                    modCheckBox.Content = modCheckBox.Content + " (" + m.size + " MB)";
            }
            //set mod's enabled status
            modCheckBox.IsEnabled = m.enabled;
            //set the mods checked status
            if (m.enabled && m.modChecked)
                modCheckBox.IsChecked = true;
            //make the tree view item for the modCheckBox
            System.Windows.Controls.TreeViewItem tvi = new System.Windows.Controls.TreeViewItem();
            tvi.Header = modCheckBox;
            //add it's handlers, right click and when checked
            modCheckBox.MouseDown += new System.Windows.Input.MouseButtonEventHandler(modCheckBoxL_MouseDown);
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
            for (int i = 0; i < m.configs.Count; i++)
            {
                //link the config and mod in memory
                m.configs[i].parent = m;
                //create the init stuff for each config
                ConfigWPFComboBox configControlDDALL = null;
                if (m.configs[i].type.Equals("single") || m.configs[i].type.Equals("single1"))
                {
                    modHasRadioButtons = true;
                    //make the radio button
                    ConfigWPFRadioButton configControlRB = new ConfigWPFRadioButton();
                    if (Settings.largeFont)
                        configControlRB.FontSize = modCheckBox.FontSize + 4;
                    configControlRB.FontFamily = new System.Windows.Media.FontFamily(Settings.fontName);
                    if (Settings.darkUI)
                        configControlRB.FontWeight = System.Windows.FontWeights.Bold;
                    configControlRB.catagory = c;
                    configControlRB.mod = m;
                    configControlRB.config = m.configs[i];
                    //the logic for enabling it
                    //set them to false first
                    configControlRB.IsEnabled = false;
                    configControlRB.IsChecked = false;
                    if (m.enabled && m.modChecked && m.configs[i].enabled)
                    {
                        configControlRB.IsEnabled = true;
                        //the logic for checking it
                        if (m.configs[i].configChecked)
                        {
                            configControlRB.IsChecked = true;
                            hasRadioButtonConfigSelected = true;
                        }
                    }
                    //run the checksum logix
                    configControlRB.Content = m.configs[i].name;
                    string oldCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile);
                    if (!oldCRC.Equals(m.configs[i].crc) && (!m.configs[i].crc.Equals("")))
                    {
                        configControlRB.Content = configControlRB.Content + " (Updated)";
                        if (m.configs[i].size > 0.0f)
                            configControlRB.Content = configControlRB.Content + " (" + m.configs[i].size + " MB)";
                    }
                    //add the handlers at the end
                    configControlRB.Click += new System.Windows.RoutedEventHandler(configControlRB_Click);
                    //add it to the mod config list
                    System.Windows.Controls.TreeViewItem configControlTVI = new System.Windows.Controls.TreeViewItem();
                    configControlTVI.Header = configControlRB;
                    tvi.Items.Add(configControlTVI);
                    //process the subconfigs
                    //code to loop through the subconfigs
                    foreach (SubConfig sc in m.configs[i].subConfigs)
                    {
                        //link the subconfig in memory
                        sc.parent = m.configs[i];
                        //create the radioButton
                        ConfigWPFRadioButton subRB = new ConfigWPFRadioButton();
                        if (Settings.largeFont)
                            subRB.FontSize = modCheckBox.FontSize + 4;
                        subRB.FontFamily = new System.Windows.Media.FontFamily(Settings.fontName);
                        if (Settings.darkUI)
                            subRB.FontWeight = System.Windows.FontWeights.Bold;
                        subRB.catagory = c;
                        subRB.mod = m;
                        subRB.config = m.configs[i];
                        subRB.subconfig = sc;
                        //logic for the radioButton
                        subRB.IsEnabled = false;
                        subRB.IsChecked = false;
                        if (m.enabled && m.modChecked && m.configs[i].enabled && m.configs[i].configChecked && sc.enabled)
                        {
                            subRB.IsEnabled = true;
                            if (sc.Checked)
                                subRB.IsChecked = true;
                        }
                        //add handlers
                        subRB.Click += new System.Windows.RoutedEventHandler(subRB_Click);
                        //run checksum logic
                        subRB.Content = sc.name;
                        string oldSubCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + sc.zipFile);
                        if (!oldSubCRC.Equals(sc.crc) && (!sc.zipFile.Equals("")))
                        {
                            subRB.Content = subRB.Content + " (Updated)";
                            if (sc.size > 0.0f)
                                subRB.Content = subRB.Content + " (" + sc.size + " MB)";
                        }
                        //add component
                        System.Windows.Controls.TreeViewItem subRBTVI = new System.Windows.Controls.TreeViewItem();
                        subRBTVI.Header = subRB;
                        configControlTVI.Items.Add(subRBTVI);
                    }
                }
                else if (m.configs[i].type.Equals("single_dropdown") || m.configs[i].type.Equals("single_dropdown1") || m.configs[i].type.Equals("single_dropdown2"))
                {
                    //set the all to whichever one it actually is
                    if (m.configs[i].type.Equals("single_dropdown") || m.configs[i].type.Equals("single_dropdown1"))
                    {
                        configControlDDALL = configControlDD;
                    }
                    else if (m.configs[i].type.Equals("single_dropdown2"))
                    {
                        configControlDDALL = configControlDD2;
                    }
                    //make the dropdown selection list
                    configControlDDALL.MinWidth = 100;
                    //run the crc logics
                    string oldCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile);
                    if (!oldCRC.Equals(m.configs[i].crc) && (!m.configs[i].crc.Equals("")))
                    {
                        string toAdd = m.configs[i].name + "_Updated";
                        if (m.configs[i].size > 0.0f)
                            toAdd = toAdd + " (" + m.configs[i].size + " MB)";
                        //add it with _updated
                        if (m.configs[i].enabled) configControlDDALL.Items.Add(new ComboBoxItem(m.configs[i],null,toAdd));
                        if (m.configs[i].configChecked) configControlDDALL.SelectedItem = toAdd;
                    }
                    else
                    {
                        //add it
                        if (m.configs[i].enabled) configControlDDALL.Items.Add(new ComboBoxItem(m.configs[i], null, m.configs[i].name));
                        if (m.configs[i].configChecked) configControlDDALL.SelectedItem = m.configs[i].name;
                    }
                    //add the dropdown to the thing. it will only run this once
                    if(configControlDDALL.Name.Equals("notAddedYet"))
                    {
                        configControlDDALL.Name = "added";
                        configControlDDALL.catagory = c;
                        configControlDDALL.mod = m;
                        //configControlDDALL.config = m.configs[i];
                        if (m.enabled && m.modChecked)
                            configControlDDALL.IsEnabled = true;
                        configControlDDALL.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(configControlDDALL_SelectionChanged);
                        System.Windows.Controls.TreeViewItem configControlTVI = new System.Windows.Controls.TreeViewItem();
                        configControlTVI.Header = configControlDDALL;
                        tvi.Items.Add(configControlTVI);
                    }
                }
                else if (m.configs[i].type.Equals("multi"))
                {
                    //make the checkbox
                    ConfigWPFCheckBox configControlCB = new ConfigWPFCheckBox();
                    if (Settings.largeFont)
                        configControlCB.FontSize = modCheckBox.FontSize + 4;
                    configControlCB.FontFamily = new System.Windows.Media.FontFamily(Settings.fontName);
                    if (Settings.darkUI)
                        configControlCB.FontWeight = System.Windows.FontWeights.Bold;
                    configControlCB.catagory = c;
                    configControlCB.mod = m;
                    configControlCB.config = m.configs[i];
                    //the logic for enabling it
                    //set them to false first
                    configControlCB.IsEnabled = false;
                    configControlCB.IsChecked = false;
                    if (m.enabled && m.configs[i].enabled && m.modChecked)
                    {
                        configControlCB.IsEnabled = true;
                        //the logic for checking it
                        if (m.modChecked && m.configs[i].configChecked)
                            configControlCB.IsChecked = true;
                    }
                    //run the checksum logix
                    configControlCB.Content = m.configs[i].name;
                    string oldCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile);
                    if (!oldCRC.Equals(m.configs[i].crc) && (!m.configs[i].crc.Equals("")))
                    {
                        configControlCB.Content = configControlCB.Content + " (Updated)";
                        if (m.configs[i].size > 0.0f)
                            configControlCB.Content = configControlCB.Content + " (" + m.configs[i].size + " MB)";
                    }
                    //add the handlers at the end
                    configControlCB.Click += new System.Windows.RoutedEventHandler(configControlCB_Click);
                    //add it to the mod config list
                    System.Windows.Controls.TreeViewItem configControlTVI = new System.Windows.Controls.TreeViewItem();
                    configControlTVI.Header = configControlCB;
                    tvi.Items.Add(configControlTVI);
                    //process the subconfigs
                    //code to loop through the subconfigs
                    foreach (SubConfig sc in m.configs[i].subConfigs)
                    {
                        //create the radioButton
                        ConfigWPFRadioButton subRB = new ConfigWPFRadioButton();
                        if (Settings.largeFont)
                            subRB.FontSize = modCheckBox.FontSize + 4;
                        subRB.FontFamily = new System.Windows.Media.FontFamily(Settings.fontName);
                        if (Settings.darkUI)
                            subRB.FontWeight = System.Windows.FontWeights.Bold;
                        subRB.catagory = c;
                        subRB.mod = m;
                        subRB.config = m.configs[i];
                        subRB.subconfig = sc;
                        //logic for the radioButton
                        subRB.IsEnabled = false;
                        subRB.IsChecked = false;
                        if (m.enabled && m.modChecked && m.configs[i].enabled && m.configs[i].configChecked && sc.enabled)
                        {
                            subRB.IsEnabled = true;
                            if (sc.Checked)
                                subRB.IsChecked = true;
                        }
                        //add handlers
                        subRB.Click += new System.Windows.RoutedEventHandler(subRB_Click);
                        //run checksum logic
                        subRB.Content = sc.name;
                        string oldSubCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + sc.zipFile);
                        if (!oldSubCRC.Equals(sc.crc) && (!sc.zipFile.Equals("")))
                        {
                            subRB.Content = subRB.Content + " (Updated)";
                            if (sc.size > 0.0f)
                                subRB.Content = subRB.Content + " (" + sc.size + " MB)";
                        }
                        //add component
                        System.Windows.Controls.TreeViewItem subRBTVI = new System.Windows.Controls.TreeViewItem();
                        subRBTVI.Header = subRB;
                        configControlTVI.Items.Add(subRBTVI);
                    }
                }
                else
                {
                    Utils.appendToLog("WARNING: Unknown config type for " + m.configs[i].name + ": " + m.configs[i].type);
                }
            }
            //add the mod check box to the legacy tree view
            lsl.legacyTreeView.Items.Add(tvi);
            if ((bool)modCheckBox.IsChecked && modHasRadioButtons && !hasRadioButtonConfigSelected)
            {
                //getting here means that the user has saved the prefrence for a selected mandatory radiobutton config that has been disabled, so his selection of that mod needs to be disabled
                m.modChecked = false;
                modCheckBox.IsChecked = false;
            }
            modCheckBox.Click += new System.Windows.RoutedEventHandler(modCheckBoxL_Click);
        }

        void subRB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            ConfigWPFRadioButton rb = (ConfigWPFRadioButton)sender;
            Mod m = rb.mod;
            Config cfg = rb.config;
            SubConfig subc = rb.subconfig;
            //the subconfig treeviewitem
            System.Windows.Controls.TreeViewItem subCFGTVI = (System.Windows.Controls.TreeViewItem)rb.Parent;
            //the config treeviewitem
            System.Windows.Controls.TreeViewItem cfgTVI = (System.Windows.Controls.TreeViewItem)subCFGTVI.Parent;
            if (!(bool)rb.IsEnabled)
            {
                subc.Checked = false;
                return;
            }
            if ((bool)rb.IsEnabled && (bool)rb.IsChecked)
            {
                //uncheck all subconfigs in memory
                foreach (SubConfig sc in cfg.subConfigs)
                {
                    sc.Checked = false;
                }
                //uncheck all subconfigs in UI
                foreach (System.Windows.Controls.TreeViewItem item in cfgTVI.Items)
                {
                    if (item.Header is ConfigWPFRadioButton)
                    {
                        ConfigWPFRadioButton rbb = (ConfigWPFRadioButton)item.Header;
                        if (!rbb.Equals(rb))
                        {
                            rbb.IsChecked = false;
                        }
                    }
                }
            }
            subc.Checked = (bool)rb.IsChecked;
        }

        //when a legacy checkbox of OMC view is clicked
        void configControlCB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            //checkboxes still don't need to be be unselected
            ConfigWPFCheckBox cb = (ConfigWPFCheckBox)sender;
            Mod m = cb.mod;
            Config cfg = cb.config;
            cfg.configChecked = (bool)cb.IsChecked;
            //process the subconfigs
            bool configSelected = false;
            if (cfg.subConfigs.Count > 0)
            {
                System.Windows.Controls.TreeViewItem tvi = (System.Windows.Controls.TreeViewItem)cb.Parent;
                foreach (System.Windows.Controls.TreeViewItem subTVI in tvi.Items)
                {
                    ConfigWPFRadioButton subRB = (ConfigWPFRadioButton)subTVI.Header;
                    SubConfig subc = subRB.subconfig;
                    if (!(bool)cb.IsEnabled || !(bool)cb.IsChecked)
                    {
                        subRB.IsEnabled = false;
                        
                    }
                    else if ((bool)cb.IsEnabled && (bool)cb.IsChecked)
                    {
                        //getting here means cb is enabled
                        subRB.IsEnabled = true;
                        subRB_Click(subRB, null);
                        if (subc.Checked)
                            configSelected = true;
                    }
                }
                if (!configSelected && (bool)cb.IsChecked && (bool)cb.IsEnabled)
                {
                    //select the first possible one
                    foreach (System.Windows.Controls.TreeViewItem subTVI in tvi.Items)
                    {
                        ConfigWPFRadioButton subRB = (ConfigWPFRadioButton)subTVI.Header;
                        SubConfig subc = subRB.subconfig;
                        if (subc.enabled)
                        {
                            subc.Checked = true;
                            subRB.Click -= subRB_Click;
                            subRB.IsChecked = true;
                            subRB.Click += subRB_Click;
                            break;
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
            if (cb.SelectedIndex == -1)
            {
                //it will run recurse with this method again with a selected index of 0
                cb.SelectedIndex = 0;
                return;
            }
            //getting here means that an item is confirmed to be selected
            //itterate through the items, get each config, disable it
            //unless it's the same name as the selectedItem
            foreach (ComboBoxItem cbi in cb.Items)
            {
                cbi.config.configChecked = false;
            }
            ComboBoxItem cbi2 = (ComboBoxItem)cb.SelectedItem;
            cbi2.config.configChecked = true;
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
            System.Windows.Controls.TreeViewItem item0 = (System.Windows.Controls.TreeViewItem)cb.Parent;
            //the mod treeview
            System.Windows.Controls.TreeViewItem item1 = (System.Windows.Controls.TreeViewItem)item0.Parent;
            if ((bool)cb.IsEnabled && (bool)cb.IsChecked)
            {
                //uncheck all single and single1 mods in memory
                foreach (Config configs in m.configs)
                {
                    if (configs.type.Equals("single") || configs.type.Equals("single1"))
                    {
                        configs.configChecked = false;
                    }
                }
                //uincheck all single and single1 mods in UI
                foreach (System.Windows.Controls.TreeViewItem item in item1.Items)
                {
                    if (item.Header is ConfigWPFRadioButton)
                    {
                        ConfigWPFRadioButton rb = (ConfigWPFRadioButton)item.Header;
                        if ((bool)rb.IsChecked && (bool)!rb.Equals(cb))
                        {
                            //this was the previous radiobutton checked
                            rb.IsChecked = false;
                            configControlRB_Click(rb, null);
                        }
                    }
                }
            }
            cfg.configChecked = (bool)cb.IsChecked;
            //process the subconfigs
            bool configSelected = false;
            if (cfg.subConfigs.Count > 0)
            {
                System.Windows.Controls.TreeViewItem tvi = (System.Windows.Controls.TreeViewItem)cb.Parent;
                foreach (System.Windows.Controls.TreeViewItem subTVI in tvi.Items)
                {
                    ConfigWPFRadioButton subRB = (ConfigWPFRadioButton)subTVI.Header;
                    SubConfig subc = subRB.subconfig;
                    if (!(bool)cb.IsEnabled || !(bool)cb.IsChecked)
                    {
                        subRB.IsEnabled = false;
                        
                    }
                    else if ((bool)cb.IsEnabled && (bool)cb.IsChecked)
                    {
                        //getting here means cb is enabled
                        subRB.IsEnabled = true;
                        subRB_Click(subRB, null);
                        if (subc.Checked)
                            configSelected = true;
                    }
                }
                if (!configSelected && (bool)cb.IsEnabled && (bool)cb.IsChecked)
                {
                    //select the first possible one
                    foreach (System.Windows.Controls.TreeViewItem subTVI in tvi.Items)
                    {
                        ConfigWPFRadioButton subRB = (ConfigWPFRadioButton)subTVI.Header;
                        SubConfig subc = subRB.subconfig;
                        if (subc.enabled)
                        {
                            subc.Checked = true;
                            subRB.Click -= subRB_Click;
                            subRB.IsChecked = true;
                            subRB.Click += subRB_Click;
                            break;
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
                p = new Preview(m.name, m.picList, m.description, m.updateComment, m.devURL);
                p.Show();
            }
        }
        //when a legacy mod checkbox is clicked
        void modCheckBoxL_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (loadingConfig)
                return;
            //RelhaxCheckbox cb = (RelhaxCheckbox)sender;
            ModWPFCheckBox cb = (ModWPFCheckBox)sender;
            //string modName = cb.realName.Split('_')[1];
            //string catagoryName = cb.realName.Split('_')[0];
            //Mod m = Utils.linkMod(modName, catagoryName, parsedCatagoryList);
            //Catagory cat = Utils.getCatagory(catagoryName,parsedCatagoryList);
            Mod m = cb.mod;
            Catagory cat = m.parent;
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
                    if (mm.modChecked)
                    {
                        anyModsChecked = true;
                        mm.modChecked = false;
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
                        if((bool)modCB.IsChecked)
                        modCB.IsChecked = false;
                        modCheckBoxL_Click(modCB, null);
                        //modCB.Click += modCheckBoxL_Click;
                    }
                }
                //now it is safe to check the mod we want
                cb.Click -= modCheckBoxL_Click;
                cb.IsChecked = true;
                cb.Click += modCheckBoxL_Click;
            }
            m.modChecked = (bool)cb.IsChecked;

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
                if (c is ConfigWPFComboBox)
                {
                    ConfigWPFComboBox cbox = (ConfigWPFComboBox)c;
                    //if the mod is checked and it has more than 0 item enable, else disable, then trigger
                    if (m.modChecked && cbox.Items.Count > 0)
                        cbox.IsEnabled = true;
                    else
                        cbox.IsEnabled = false;
                    configControlDDALL_SelectionChanged(cbox, null);
                }
                else if (c is ConfigWPFCheckBox)
                {
                    ConfigWPFCheckBox cbox = (ConfigWPFCheckBox)c;
                    //multi CB code
                    //CB is enabled if the mod checked and the config is enabled
                    cfg = cbox.config;
                    if (m.modChecked && cfg.enabled)
                        cbox.IsEnabled = true;
                    else
                        cbox.IsEnabled = false;
                    configControlCB_Click(cbox,null);
                }
                else if (c is ConfigWPFRadioButton)
                {
                    ConfigWPFRadioButton cbox = (ConfigWPFRadioButton)c;
                    cfg = cbox.config;
                    if (m.modChecked && cfg.enabled)
                        cbox.IsEnabled = true;
                    else
                        cbox.IsEnabled = false;
                    configControlRB_Click(cbox, null);
                    if (cfg.configChecked)
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
                                if ((bool)c2r.IsEnabled)
                                {
                                    c2r.Click -= configControlRB_Click;
                                    c2r.IsChecked = true;
                                    configControlRB_Click(c2r, null);
                                    c2r.Click += configControlRB_Click;
                                    cfg.configChecked = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }

        }
        //adds a mod m to a tabpage t
        private void addMod(Mod m, TabPage t, int panelCount, Catagory catagory)
        {
            //bool for keeping track if a radioButton config has been selected
            bool hasRadioButtonConfigSelected = false;
            bool modHasRadioButtons = false;
            //make config panel
            Panel configPanel = new Panel();
            configPanel.Enabled = true;
            configPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            configPanel.Location = new System.Drawing.Point(3, 10);
            configPanel.TabIndex = 2;
            configPanel.AutoSize = true;
            configPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
            configPanel.Size = new System.Drawing.Size(t.Size.Width - 35, 30);
            if (m.enabled && m.modChecked)
                configPanel.BackColor = Color.BlanchedAlmond;
            else
                configPanel.BackColor = Settings.getBackColor();
            //add configs to the panel
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
            configControlDD2.Name = t.Name + "_" + m.name + "_DropDown2";
            configControlDD2.DropDownStyle = ComboBoxStyle.DropDownList;
            for (int i = 0; i < m.configs.Count; i++)
            {
                ConfigFormComboBox configControlDDALL = null;
                if (m.configs[i].type.Equals("single") || m.configs[i].type.Equals("single1"))
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
                    configControlRB.config = m.configs[i];
                    //logic for radiobutton
                    configControlRB.Enabled = false;
                    configControlRB.Checked = false;
                    if (m.enabled && m.modChecked && m.configs[i].enabled)
                    {
                        configControlRB.Enabled = true;
                        if (m.configs[i].configChecked)
                        {
                            configControlRB.Checked = true;
                            hasRadioButtonConfigSelected = true;
                        }
                    }
                    //add handlers
                    configControlRB.CheckedChanged += new EventHandler(configControlRB_CheckedChanged);
                    configControlRB.Name = t.Name + "_" + m.name + "_" + m.configs[i].name;
                    //run checksum logic
                    configControlRB.Text = m.configs[i].name;
                    string oldCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile);
                    if (!oldCRC.Equals(m.configs[i].crc) && (!m.configs[i].crc.Equals("")))
                    {
                        configControlRB.Text = configControlRB.Text + " (Updated)";
                        if (m.configs[i].size > 0.0f)
                            configControlRB.Text = configControlRB.Text + " (" + m.configs[i].size + " MB)";
                    }
                    //add the config to the form
                    configPanel.Controls.Add(configControlRB);
                    //process the subconfigs
                    //code to declare refrences
                    Panel subConfigPanel = null;

                    //code to run once to init setup the panels and stuff
                    if (m.configs[i].subConfigs.Count > 0)
                    {
                        subConfigPanel = new Panel();
                        subConfigPanel.Enabled = true;
                        subConfigPanel.BorderStyle = BorderStyle.FixedSingle;
                        subConfigPanel.Location = new Point(3, getYLocation(configPanel.Controls));
                        subConfigPanel.Size = new Size(t.Size.Width - 45, 30);
                        subConfigPanel.AutoSize = true;
                        subConfigPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                        subConfigPanel.BackColor = Settings.getBackColor();
                        subConfigPanel.Name = t.Name + "_" + m.name + "_" + m.configs[i].name + "_subConfigPanel";
                    }
                    //code th loop through the subconfigs
                    for(int j = 0; j < m.configs[i].subConfigs.Count; j++)
                    {
                        SubConfig sc = m.configs[i].subConfigs[j];
                        ConfigFormRadioButton subRB = new ConfigFormRadioButton();
                        subRB.AutoSize = true;
                        subRB.Location = new Point(6, (15 * j) + 3);
                        subRB.Size = new Size(150, 15);
                        subRB.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
                        subRB.Name = t.Name + "_" + m.name + "_" + m.configs[i].name + "_" + sc.name;
                        subRB.catagory = catagory;
                        subRB.mod = m;
                        subRB.config = m.configs[i];
                        subRB.subconfig = sc;
                        //logic for the radioButtons
                        subRB.Enabled = false;
                        subRB.Checked = false;
                        if (m.enabled && m.modChecked && m.configs[i].enabled && m.configs[i].configChecked && sc.enabled)
                        {
                            subRB.Enabled = true;
                            if (sc.Checked)
                            {
                                subRB.Checked = true;
                                //also set the panel to blanched almond
                                subConfigPanel.BackColor = Color.BlanchedAlmond;
                            }
                        }
                        //add handlers
                        subRB.CheckedChanged += new EventHandler(subRB_CheckedChanged);
                        //run checksum logic
                        subRB.Text = sc.name;
                        string oldSubCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + sc.zipFile);
                        if (!oldSubCRC.Equals(sc.crc) && (!sc.zipFile.Equals("")))
                        {
                            subRB.Text = subRB.Text + " (Updated)";
                            if (sc.size > 0.0f)
                                subRB.Text = subRB.Text + " (" + sc.size + " MB)";
                        }
                        //add component
                        subConfigPanel.Controls.Add(subRB);
                    }
                    //add subconfig to the form
                    if (subConfigPanel != null) configPanel.Controls.Add(subConfigPanel);
                    continue;
                }
                else if (m.configs[i].type.Equals("single_dropdown") || m.configs[i].type.Equals("single_dropdown1") || m.configs[i].type.Equals("single_dropdown2"))
                {
                    //set the all one to the version is actually is
                    if (m.configs[i].type.Equals("single_dropdown") || m.configs[i].type.Equals("single_dropdown1"))
                    {
                        configControlDDALL = configControlDD;
                    }
                    else if (m.configs[i].type.Equals("single_dropdown2"))
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
                    string oldCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile);
                    if (!oldCRC.Equals(m.configs[i].crc) && (!m.configs[i].crc.Equals("")))
                    {
                        string toAdd = m.configs[i].name + "_Updated";
                        if (m.configs[i].size > 0.0f)
                            toAdd = toAdd + " (" + m.configs[i].size + " MB)";
                        //add it with _updated
                        if (m.configs[i].enabled) configControlDDALL.Items.Add(new ComboBoxItem(m.configs[i],null,toAdd));
                        if (m.configs[i].configChecked)
                        {
                            configControlDDALL.SelectedItem = toAdd;
                            configControlDDALL.Enabled = true;
                        }
                    }
                    else
                    {
                        //add it
                        if (m.configs[i].enabled) configControlDDALL.Items.Add(new ComboBoxItem(m.configs[i],null,m.configs[i].name));
                        if (m.configs[i].configChecked)
                        {
                            configControlDDALL.SelectedItem = m.configs[i].name;
                            configControlDDALL.Enabled = true;
                        }
                    }
                    continue;
                }
                else if (m.configs[i].type.Equals("multi"))
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
                    configControlCB.config = m.configs[i];
                    //the logic for enabling it
                    configControlCB.Enabled = false;
                    configControlCB.Checked = false;
                    if (m.enabled && m.modChecked && m.configs[i].enabled)
                    {
                        configControlCB.Enabled = true;
                        //the logic for checking it
                        if (m.configs[i].configChecked)
                            configControlCB.Checked = true;
                    }
                    //add handlers
                    configControlCB.CheckedChanged += new EventHandler(configControlCB_CheckedChanged);
                    configControlCB.Name = t.Name + "_" + m.name + "_" + m.configs[i].name;
                    //checksum logic
                    configControlCB.Text = m.configs[i].name;
                    string oldCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile);
                    if (!oldCRC.Equals(m.configs[i].crc) && (!m.configs[i].crc.Equals("")))
                    {
                        configControlCB.Text = configControlCB.Text + " (Updated)";
                        if (m.configs[i].size > 0.0f)
                            configControlCB.Text = configControlCB.Text + " (" + m.configs[i].size + " MB)";
                    }
                    //add config to the form
                    configPanel.Controls.Add(configControlCB);
                    //process the subconfigs
                    //code to declare refrences
                    Panel subConfigPanel = null;

                    //code to run once to init setup the panels and stuff
                    if (m.configs[i].subConfigs.Count > 0)
                    {
                        subConfigPanel = new Panel();
                        subConfigPanel.Enabled = true;
                        subConfigPanel.BorderStyle = BorderStyle.FixedSingle;
                        subConfigPanel.Location = new Point(15, getYLocation(configPanel.Controls));
                        subConfigPanel.Size = new Size(t.Size.Width - 45, 30);
                        subConfigPanel.AutoSize = true;
                        subConfigPanel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                        subConfigPanel.BackColor = Settings.getBackColor();
                        subConfigPanel.Name = t.Name + "_" + m.name + "_" + m.configs[i].name + "_subConfigPanel";
                    }
                    //code th loop through the subconfigs
                    for (int j = 0; j < m.configs[i].subConfigs.Count; j++)
                    {
                        SubConfig sc = m.configs[i].subConfigs[j];
                        ConfigFormRadioButton subRB = new ConfigFormRadioButton();
                        subRB.AutoSize = true;
                        subRB.Location = new Point(6, (13 * j) + 3);
                        subRB.Size = new Size(150, 15);
                        subRB.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
                        subRB.Name = t.Name + "_" + m.name + "_" + m.configs[i].name + "_" + sc.name;
                        subRB.catagory = catagory;
                        subRB.mod = m;
                        subRB.config= m.configs[i];
                        subRB.subconfig = sc;
                        //logic for the radioButtons
                        subRB.Enabled = false;
                        subRB.Checked = false;
                        if (m.enabled && m.modChecked && m.configs[i].enabled && m.configs[i].configChecked && sc.enabled)
                        {
                            subRB.Enabled = true;
                            if (sc.Checked)
                            {
                                subRB.Checked = true;
                                //also set the panel to blanched almond
                                subConfigPanel.BackColor = Color.BlanchedAlmond;
                            }
                        }
                        //add handlers
                        subRB.CheckedChanged += new EventHandler(subRB_CheckedChanged);
                        //run checksum logic
                        subRB.Text = sc.name;
                        string oldSubCRC = Utils.getMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + sc.zipFile);
                        if (!oldSubCRC.Equals(sc.crc) && (!sc.zipFile.Equals("")))
                        {
                            subRB.Text = subRB.Text + " (Updated)";
                            if (sc.size > 0.0f)
                                subRB.Text = subRB.Text + " (" + sc.size + " MB)";
                        }
                        //add component
                        subConfigPanel.Controls.Add(subRB);
                    }
                    //add subconfig to the form
                    if (subConfigPanel != null) configPanel.Controls.Add(subConfigPanel);
                    continue;
                }
                else
                {
                    Utils.appendToLog("WARNING: Unknown config type for " + m.configs[i].name + ": " + m.configs[i].type);
                }
            }
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
            //the mod checksum logic
            string modDownloadPath = Application.StartupPath + "\\RelHaxDownloads\\" + m.modZipFile;
            string oldCRC2 = Utils.getMd5Hash(modDownloadPath);
            //if the CRC's don't match and the mod actually has a zip file
            if (!(m.crc.Equals(oldCRC2)) && (!m.modZipFile.Equals("")))
            {
                modCheckBox.Text = modCheckBox.Text + " (Updated)";
                if ((m.size > 0.0f))
                    modCheckBox.Text = modCheckBox.Text + " (" + m.size + " MB)";
            }
            modCheckBox.UseVisualStyleBackColor = true;
            modCheckBox.Enabled = m.enabled;
            modCheckBox.MouseDown += new MouseEventHandler(modCheckBox_MouseDown);
            //in theory it should trigger the handler for checked
            //when initially made it should be false, if enabled from
            //from user configs
            int spacer = modCheckBox.Location.Y + modCheckBox.Size.Height + 5;
            if (Settings.largeFont) spacer += 3;
            configPanel.Location = new Point(configPanel.Location.X, spacer);
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
            modCheckBox.Checked = m.modChecked;
            if (modHasRadioButtons && modCheckBox.Checked && !hasRadioButtonConfigSelected)
            {
                //getting here means that the user has saved the prefrence for a selected mandatory radiobutton config that has been disabled, so his selection of that mod needs to be disabled
                modCheckBox.Checked = false;
                m.modChecked = false;
            }
            modCheckBox.CheckedChanged += new EventHandler(modCheckBox_CheckedChanged);
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
                p = new Preview(m.name, m.picList, m.description, m.updateComment, m.devURL);
                p.Show();
            }
        }
        //handler for when a config selection is made from the drop down list
        void configControlDD_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (loadingConfig)
                return;
            //uncheck all other dorp down configs
            ConfigFormComboBox cb = (ConfigFormComboBox)sender;
            Mod m = cb.mod;
            //if no index selected, select one
            if (cb.SelectedIndex == -1)
            {
                cb.SelectedIndex = 0;
                return;
            }
            //getting here means that an item is confirmed to be selected
            ComboBoxItem cbi = (ComboBoxItem)cb.SelectedItem;
            if (!cb.Enabled)
            {
                //disable the mod and return
                cbi.config.configChecked = false;
                return;
            }
            //itterate through the items, get each config, disable it
            //unless it's the same name as the selectedItem
            foreach (ComboBoxItem s in cb.Items)
            {
                s.config.configChecked = false;
            }
            cbi.config.configChecked = true;
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
            Catagory cat = cb.catagory;
            Panel configPanel = (Panel)cb.Parent;
            if (!cb.Enabled || !cb.Checked)
            {
                //uncheck config in database
                cfg.configChecked = false;
                if (cfg.subConfigs.Count > 0)
                {
                    //modify the panel and trigger the subConfigs
                    foreach (Control p in configPanel.Controls)
                    {
                        if ((p is Panel) && (p.Name.Equals(cat.name + "_" + m.name + "_" + cfg.name + "_subConfigPanel")))
                        {
                            Panel pan = (Panel)p;
                            pan.BackColor = Settings.getBackColor();
                            foreach (Control cont in pan.Controls)
                            {
                                ConfigFormRadioButton r = (ConfigFormRadioButton)cont;
                                r.Enabled = false;
                                subRB_CheckedChanged(r, null);
                            }
                            break;
                        }
                    }
                }
                return;
            }
            
                //checkbox is enabled, toggle checked and checked
                cfg.configChecked = cb.Checked;
                //at this point it is enabled
                if (cfg.subConfigs.Count > 0)
                {
                    bool configSelected = false;
                    string lastConfig = "null";
                    foreach (SubConfig sc in cfg.subConfigs)
                    {
                        lastConfig = sc.name;
                    }
                    //modify the panel and trigger the subConfigs
                    foreach (Control p in configPanel.Controls)
                    {
                        if ((p is Panel) && (p.Name.Equals(cat.name + "_" + m.name + "_" + cfg.name + "_subConfigPanel")))
                        {
                            Panel pan = (Panel)p;
                            pan.BackColor = Color.BlanchedAlmond;
                            foreach (Control cont in pan.Controls)
                            {
                                ConfigFormRadioButton r = (ConfigFormRadioButton)cont;
                                SubConfig sc2 = r.subconfig;
                                r.Enabled = true;
                                subRB_CheckedChanged(r, null);
                                if (sc2.Checked)
                                    configSelected = true;
                            }
                            break;
                        }
                    }
                    if (!configSelected)
                    {
                        foreach (Control p in configPanel.Controls)
                        {
                            if ((p is Panel) && (p.Name.Equals(cat.name + "_" + m.name + "_" + cfg.name + "_subConfigPanel")))
                            {
                                Panel pan = (Panel)p;
                                foreach (Control cont in pan.Controls)
                                {
                                    ConfigFormRadioButton r = (ConfigFormRadioButton)cont;
                                    SubConfig sc2 = r.subconfig;
                                    if (r.Enabled)
                                    {
                                        r.CheckedChanged -= subRB_CheckedChanged;
                                        r.Checked = true;
                                        r.CheckedChanged += subRB_CheckedChanged;
                                        sc2.Checked = true;
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            

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
            Config c = rb.config;
            Catagory cat = rb.catagory;
            //verify mod is enabled
            if (!rb.Enabled || !rb.Checked)
            {
                c.configChecked = false;
                if (c.subConfigs.Count > 0)
                {
                    //modify the panel and trigger the subConfigs
                    foreach (Control p in configPanel.Controls)
                    {
                        if ((p is Panel) && (p.Name.Equals(cat.name + "_" + m.name + "_" + c.name + "_subConfigPanel")))
                        {
                            Panel pan = (Panel)p;
                            pan.BackColor = Settings.getBackColor();
                            foreach (Control cont in pan.Controls)
                            {
                                ConfigFormRadioButton r = (ConfigFormRadioButton)cont;
                                r.Enabled = false;
                                subRB_CheckedChanged(r, null);
                            }
                            break;
                        }
                    }
                }
                return;
            }
            //uncheck all single and single1 mods in memory
            foreach (Config configs in m.configs)
            {
                if (configs.type.Equals("single") || configs.type.Equals("single1"))
                {
                    configs.configChecked = false;
                }
            }
            //uncheck all single and single1 configs in UI
            if (rb.Enabled && rb.Checked)
            {
                foreach (Control ctrl in configPanel.Controls)
                {
                    if (ctrl is ConfigFormRadioButton)
                    {
                        ConfigFormRadioButton rbz = (ConfigFormRadioButton)ctrl;
                        if (!rbz.Equals(rb))
                            rbz.Checked = false;
                    }
                }
                
            }
            c.configChecked = rb.Checked;
            //at this point it is enabled
            if (c.subConfigs.Count > 0)
            {
                bool configSelected = false;
                string lastConfig = "null";
                foreach (SubConfig sc in c.subConfigs)
                {
                    lastConfig = sc.name;
                }
                //modify the panel and trigger the subConfigs
                foreach (Control p in configPanel.Controls)
                {
                    if ((p is Panel) && (p.Name.Equals(cat.name + "_" + m.name + "_" + c.name + "_subConfigPanel")))
                    {
                        Panel pan = (Panel)p;
                        pan.BackColor = Color.BlanchedAlmond;
                        foreach (Control cont in pan.Controls)
                        {
                            ConfigFormRadioButton r = (ConfigFormRadioButton)cont;
                            SubConfig sc2 = r.subconfig;
                            r.Enabled = true;
                            subRB_CheckedChanged(r, null);
                            if (sc2.Checked)
                                configSelected = true;
                        }
                        break;
                    }
                }
                if (!configSelected)
                {
                    foreach (Control p in configPanel.Controls)
                    {
                        if ((p is Panel) && (p.Name.Equals(cat.name + "_" + m.name + "_" + c.name + "_subConfigPanel")))
                        {
                            Panel pan = (Panel)p;
                            foreach (Control cont in pan.Controls)
                            {
                                ConfigFormRadioButton r = (ConfigFormRadioButton)cont;
                                SubConfig sc2 = r.subconfig;
                                if (r.Enabled)
                                {
                                    r.CheckedChanged -= subRB_CheckedChanged;
                                    r.Checked = true;
                                    r.CheckedChanged += subRB_CheckedChanged;
                                    sc2.Checked = true;
                                    break;
                                }
                            }
                            break;
                        }
                    }
                }
            }
        }
        //handler for when a subconfig radiobutton is changed
        void subRB_CheckedChanged(object sender, EventArgs e)
        {
            if (loadingConfig)
                return;
            //get all required cool stuff
            ConfigFormRadioButton rb = (ConfigFormRadioButton)sender;
            Panel configSelection = (Panel)rb.Parent;
            Mod m = rb.mod;
            Config c = rb.config;
            Panel configPanel = (Panel)rb.Parent;
            SubConfig sfg = rb.subconfig;
            //verify mod is enabled
            if (!rb.Enabled)
            {
                c.configChecked = false;
                return;
            }
            //uncheck all mods in memory
            foreach (SubConfig sc in c.subConfigs)
            {
                sc.Checked = false;
            }
            //uncheck all mods in UI
            if (rb.Enabled && rb.Checked)
            {
                foreach (Control ctrl in configSelection.Controls)
                {
                    if (ctrl is ConfigFormRadioButton)
                    {
                        ConfigFormRadioButton rbz = (ConfigFormRadioButton)ctrl;
                        if (!rbz.Equals(rb))
                            rbz.Checked = false;
                    }
                }

            }
            sfg.Checked = rb.Checked;
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
                            m2.modChecked = cbUser.Checked;
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
            Catagory cat = cb.catagory;

            //check to see if the mod is part of a single selection only catagory
            //if it is uncheck the other mods first, then deal with mod loop selection
            if (cat.selectionType.Equals("single") && cb.Checked)
            {
                //check if any other mods in this catagory are already checked
                bool anyModsChecked = false;
                foreach (Mod mm in cat.mods)
                {
                    if (mm.modChecked)
                    {
                        anyModsChecked = true;
                        mm.modChecked = false;
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
                                    modCheckBox_CheckedChanged(cbb, null);
                                    //cbb.CheckedChanged += modCheckBox_CheckedChanged;
                                }
                            }
                        }
                    }
                    //now it's safe to check the mods
                    //remove the handler for a sec to prevent stack overflow
                    cb.CheckedChanged -= modCheckBox_CheckedChanged;
                    cb.Checked = true;
                    cb.CheckedChanged += modCheckBox_CheckedChanged;
                    //modCheckBox.Checked = false;
                }

            }

            //toggle the mod in memory, enabled or disabled
            m.modChecked = cb.Checked;
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
                if (cc is ConfigFormComboBox)
                {
                    //comboBox could be recieved as enabled(configs of this type exist), but with 0 enabled configs which means count would be 0
                    //toggle the comboBox and trigger selected index
                    ConfigFormComboBox ccB = (ConfigFormComboBox)cc;
                    if (cb.Checked && (ccB.Items.Count > 0))
                        ccB.Enabled = true;
                    else
                        ccB.Enabled = false;
                    configControlDD_SelectedIndexChanged(ccB, null);
                }
                else if (cc is ConfigFormCheckBox)
                {
                    ConfigFormCheckBox configCTRL = (ConfigFormCheckBox)cc;
                    cfg = configCTRL.config;
                    //checkbox is enabled if mod is checked AND config is enabled
                    //toggle enabledness of checkbox based on modCheckBox checked
                    //and trigger the checkbox handler
                    if (cb.Checked && cfg.enabled)
                        cc.Enabled = true;
                    else
                        cc.Enabled = false;
                    configControlCB_CheckedChanged(cc, null);
                }
                else if (cc is ConfigFormRadioButton)
                {
                    ConfigFormRadioButton ccRB = (ConfigFormRadioButton)cc;
                    cfg = ccRB.config;
                    if (m.modChecked && cfg.enabled)
                        ccRB.Enabled = true;
                    else
                        ccRB.Enabled = false;
                    configControlRB_CheckedChanged(ccRB,null);
                    if (cfg.configChecked)
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
                                if (ccccc.Enabled)
                                {
                                    ccccc.CheckedChanged -= configControlRB_CheckedChanged;
                                    ccccc.Checked = true;
                                    configControlRB_CheckedChanged(ccccc, null);
                                    ccccc.CheckedChanged += configControlRB_CheckedChanged;
                                    cfg.configChecked = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            
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
                        Panel p = (Panel)c;
                        p.Size = new Size(t.Size.Width - 25, p.Size.Height);
                        foreach (Control cc in p.Controls)
                        {
                            if (cc is Panel)
                            {
                                //config panel
                                Panel pp = (Panel)cc;
                                pp.Size = new Size(t.Size.Width - 35, pp.Size.Height);
                                foreach (Control ccc in pp.Controls)
                                {
                                    if (ccc is Panel)
                                    {
                                        //subconfig panel
                                        Panel ppp = (Panel)ccc;
                                        ppp.Size = new Size(t.Size.Width - 58, ppp.Size.Height);
                                    }
                                }
                            }
                        }
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
                Utils.appendToLog("Save last config checked, saving to " + savePath);
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
                                XmlElement config = null;
                                if (cc.configChecked)
                                {
                                    //add the config to the list
                                    config = doc.CreateElement("config");
                                    configsHolder.AppendChild(config);
                                    XmlElement configName = doc.CreateElement("name");
                                    configName.InnerText = cc.name;
                                    config.AppendChild(configName);

                                    if (cc.subConfigs.Count > 0)
                                    {
                                        XmlElement subConfigsHolder = doc.CreateElement("subConfigs");
                                        config.AppendChild(subConfigsHolder);
                                        foreach (SubConfig sc in cc.subConfigs)
                                        {
                                            if (sc.Checked)
                                            {
                                                XmlElement subConfig = doc.CreateElement("subConfig");
                                                subConfigsHolder.AppendChild(subConfig);
                                                XmlElement subConfigName = doc.CreateElement("name");
                                                subConfigName.InnerText = sc.name;
                                                subConfig.AppendChild(subConfigName);
                                            }
                                        }
                                    }
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
                MessageBox.Show(Translations.getTranslatedString("configSaveSucess"));
            }
        }
        //loads a saved config from xml and parses it into the memory database
        private void loadConfig(loadConfigMode loadMode)
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
            this.clearSelectionMemory();
            Utils.appendToLog("Loading mod selections from " + filePath);
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
                            m = Utils.linkMod(nn.InnerText, parsedCatagoryList);
                            if (m == null)
                            {
                                Utils.appendToLog("WARNING: mod \"" + nn.InnerText + "\" not found");
                                MessageBox.Show(Translations.getTranslatedString("modNotFound_1") + nn.InnerText + Translations.getTranslatedString("modNotFound_2"));
                                continue;
                            }
                            if (m.enabled)
                            {
                                Utils.appendToLog("Checking mod " + m.name);
                                m.modChecked = true;
                            }
                            break;
                        case "configs":
                            foreach (XmlNode nnn in nn.ChildNodes)
                            {
                                if (m == null)
                                {
                                    continue;
                                }
                                Config c = new Config();
                                foreach (XmlNode nnnn in nnn.ChildNodes)
                                {
                                    switch (nnnn.Name)
                                    {
                                        case "name":
                                            c = m.getConfig(nnnn.InnerText);
                                            if (c == null)
                                            {
                                                Utils.appendToLog("WARNING: config \"" + nnnn.InnerText + "\" not found for mod \"" + nn.InnerText + "\"");
                                                MessageBox.Show(Translations.getTranslatedString("configNotFound_1") + nnnn.InnerText + Translations.getTranslatedString("configNotFound_2") + nn.InnerText + Translations.getTranslatedString("configNotFound_3"));
                                                continue;
                                            }
                                            if (c.enabled)
                                            {
                                                Utils.appendToLog("Checking config " + c.name);
                                                c.configChecked = true;
                                            }
                                            break;
                                        case "subConfigs":
                                            foreach (XmlNode subConfigHolder in nnnn.ChildNodes)
                                            {
                                                if (c == null)
                                                    continue;
                                                SubConfig sc = new SubConfig();
                                                foreach (XmlNode subConfigNode in subConfigHolder.ChildNodes)
                                                {
                                                    switch (subConfigNode.Name)
                                                    {
                                                        case "name":
                                                            sc = c.getSubConfig(subConfigNode.InnerText);
                                                            if (sc == null)
                                                            {
                                                                Utils.appendToLog("WARNING: subConfig \"" + subConfigNode.InnerText + "\" not found for config \"" + nnnn.InnerText + "\"");
                                                                MessageBox.Show(Translations.getTranslatedString("configNotFound_1") + subConfigNode.InnerText + Translations.getTranslatedString("configNotFound_2") + nnnn.InnerText + Translations.getTranslatedString("configNotFound_3"));
                                                                continue;
                                                            }
                                                            if (sc.enabled)
                                                            {
                                                                Utils.appendToLog("Checking subConfig " + sc.name);
                                                                sc.Checked = true;
                                                            }
                                                            break;
                                                    }
                                                }
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
                            m = Utils.getUserMod(nn.InnerText,userMods);
                            if (m != null)
                            {
                                string filename = m.name + ".zip";
                                if (File.Exists(Application.StartupPath + "\\RelHaxUserMods\\" + filename))
                                {
                                    m.modChecked = true;
                                    Utils.appendToLog("checking user mod " + m.modZipFile);
                                }
                            }
                            break;
                    }
                }
            }
            Utils.appendToLog("Finished loading mod selections");
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
        //handler for when the "load config" button is pressed
        private void loadConfigButton_Click(object sender, EventArgs e)
        {
            loadMode = loadConfigMode.fromButton;
            this.loadConfig(loadMode);
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
        //unchecks all mods from memory
        private void clearSelectionMemory()
        {
            Utils.appendToLog("Unchecking all mods");
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
                                foreach (SubConfig subc in cc.subConfigs)
                                {
                                    if (subc.enabled)
                                        subc.Checked = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void clearSelectionsButton_Click(object sender, EventArgs e)
        {
            this.clearSelectionMemory();
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
    }
}
