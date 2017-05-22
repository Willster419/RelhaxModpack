using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;
using System.Windows.Forms.Integration;


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
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("shell32.dll")]
        public static extern UInt32 SHAppBarMessage(UInt32 dwMessage, ref APPBARDATA pData);

        public enum AppBarMessages
        {
            New = 0x00,
            Remove = 0x01,
            QueryPos = 0x02,
            SetPos = 0x03,
            GetState = 0x04,
            GetTaskBarPos = 0x05,
            Activate = 0x06,
            GetAutoHideBar = 0x07,
            SetAutoHideBar = 0x08,
            WindowPosChanged = 0x09,
            SetState = 0x0a
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA
        {
            public UInt32 cbSize;
            public IntPtr hWnd;
            public UInt32 uCallbackMessage;
            public UInt32 uEdge;
            public Rectangle rc;
            public Int32 lParam;
        }

        public enum AppBarStates
        {
            AutoHide = 0x01,
            AlwaysOnTop = 0x02
        }

        /// <summary>
        /// Set the Taskbar State option
        /// </summary>
        /// <param name="option">AppBarState to activate</param>
        public void SetTaskbarState(AppBarStates option)
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
            msgData.hWnd = FindWindow("System_TrayWnd", null);
            msgData.lParam = (Int32)(option);
            SHAppBarMessage((UInt32)AppBarMessages.SetState, ref msgData);
        }

        /// <summary>
        /// Gets the current Taskbar state
        /// </summary>
        /// <returns>current Taskbar state</returns>
        public AppBarStates GetTaskbarState()
        {
            APPBARDATA msgData = new APPBARDATA();
            msgData.cbSize = (UInt32)Marshal.SizeOf(msgData);
            msgData.hWnd = FindWindow("System_TrayWnd", null);
            return (AppBarStates)SHAppBarMessage((UInt32)AppBarMessages.GetState, ref msgData);
        }
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
            this.createModStructure2(databaseURL);
            bool duplicates = this.duplicates();
            if (duplicates)
            {
                Settings.appendToLog("CRITICAL: Duplicate mod name detected!!");
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
            AppBarStates currentState = GetTaskbarState();
            if (currentState == AppBarStates.AutoHide)
            {
                taskBarHidden = true;
                SetTaskbarState(AppBarStates.AlwaysOnTop);
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
            Settings.appendToLog("Loading ModSelectionList with view " + Settings.sView);
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
                                this.addMod(m, t, i++);
                            }
                            else if (Settings.sView == Settings.SelectionView.legacy)
                            {
                                //use legacy OMC UI
                                this.addModTreeview(m, t, i++, lsl, c);
                            }
                            else
                            {
                                //default case, use default
                                this.addMod(m, t, i++);
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
            //create base mod checkbox
            //RelhaxCheckbox modCheckBox = new RelhaxCheckbox();
            RelhaxCheckbox modCheckBox = new RelhaxCheckbox();
            //apparnetly spaces arn't cool, so let's ger rid of them
            modCheckBox.realName = t.Name + "_" + m.name;
            if (Settings.largeFont)
                modCheckBox.FontSize = modCheckBox.FontSize + 4;
            modCheckBox.FontFamily = new System.Windows.Media.FontFamily(Settings.fontName);
            if (Settings.darkUI)
                modCheckBox.FontWeight = System.Windows.FontWeights.Bold;
            modCheckBox.Content = m.name;
            //get the local md5 hash. a -1 indicates the file is not on the disk
            string oldCRC2 = Settings.GetMd5Hash(modDownloadFilePath);
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
            RelhaxComboBox configControlDD = new RelhaxComboBox();
            configControlDD.Items.Clear();
            configControlDD.IsEditable = false;
            //configControlDD.FontSize = Settings.fontSize;
            configControlDD.Name = "notAddedYet";
            configControlDD.IsEnabled = false;
            RelhaxComboBox configControlDD2 = new RelhaxComboBox();
            configControlDD2.Items.Clear();
            configControlDD2.IsEditable = false;
            //configControlDD2.FontSize = Settings.fontSize;
            configControlDD2.Name = "notAddedYet";
            configControlDD2.IsEnabled = false;
            //process the configs
            for (int i = 0; i < m.configs.Count; i++)
            {
                //create the init stuff for each config
                RelhaxComboBox configControlDDALL = null;
                if (m.configs[i].type.Equals("single") || m.configs[i].type.Equals("single1"))
                {
                    modHasRadioButtons = true;
                    //make the radio button
                    RelhaxRadioButton configControlRB = new RelhaxRadioButton();
                    if (Settings.largeFont)
                        configControlRB.FontSize = modCheckBox.FontSize + 4;
                    configControlRB.FontFamily = new System.Windows.Media.FontFamily(Settings.fontName);
                    if (Settings.darkUI)
                        configControlRB.FontWeight = System.Windows.FontWeights.Bold;
                    configControlRB.realName = t.Name + "_" + m.name + "_" + m.configs[i].name;
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
                    string oldCRC = Settings.GetMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile);
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
                        //create the radioButton
                        RelhaxRadioButton subRB = new RelhaxRadioButton();
                        if (Settings.largeFont)
                            subRB.FontSize = modCheckBox.FontSize + 4;
                        subRB.FontFamily = new System.Windows.Media.FontFamily(Settings.fontName);
                        if (Settings.darkUI)
                            subRB.FontWeight = System.Windows.FontWeights.Bold;
                        subRB.realName = t.Name + "_" + m.name + "_" + m.configs[i].name + "_" + sc.name;
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
                        string oldSubCRC = Settings.GetMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + sc.zipFile);
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
                    string oldCRC = Settings.GetMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile);
                    if (!oldCRC.Equals(m.configs[i].crc) && (!m.configs[i].crc.Equals("")))
                    {
                        string toAdd = m.configs[i].name + "_Updated";
                        if (m.configs[i].size > 0.0f)
                            toAdd = toAdd + " (" + m.configs[i].size + " MB)";
                        //add it with _updated
                        if (m.configs[i].enabled) configControlDDALL.Items.Add(toAdd);
                        if (m.configs[i].configChecked) configControlDDALL.SelectedItem = toAdd;
                    }
                    else
                    {
                        //add it
                        if (m.configs[i].enabled) configControlDDALL.Items.Add(m.configs[i].name);
                        if (m.configs[i].configChecked) configControlDDALL.SelectedItem = m.configs[i].name;
                    }
                    //add the dropdown to the thing. it will only run this once
                    if(configControlDDALL.Name.Equals("notAddedYet"))
                    {
                        configControlDDALL.Name = "added";
                        configControlDDALL.realName = t.Name + "_" + m.name;
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
                    RelhaxCheckbox configControlCB = new RelhaxCheckbox();
                    if (Settings.largeFont)
                        configControlCB.FontSize = modCheckBox.FontSize + 4;
                    configControlCB.FontFamily = new System.Windows.Media.FontFamily(Settings.fontName);
                    if (Settings.darkUI)
                        configControlCB.FontWeight = System.Windows.FontWeights.Bold;
                    //configControlCB.FontSize = Settings.fontSize;
                    configControlCB.realName = t.Name + "_" + m.name + "_" + m.configs[i].name;
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
                    string oldCRC = Settings.GetMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile);
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
                        RelhaxRadioButton subRB = new RelhaxRadioButton();
                        if (Settings.largeFont)
                            subRB.FontSize = modCheckBox.FontSize + 4;
                        subRB.FontFamily = new System.Windows.Media.FontFamily(Settings.fontName);
                        if (Settings.darkUI)
                            subRB.FontWeight = System.Windows.FontWeights.Bold;
                        subRB.realName = t.Name + "_" + m.name + "_" + m.configs[i].name + "_" + sc.name;
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
                        string oldSubCRC = Settings.GetMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + sc.zipFile);
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
                    Settings.appendToLog("WARNING: Unknown config type for " + m.configs[i].name + ": " + m.configs[i].type);
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
            RelhaxRadioButton rb = (RelhaxRadioButton)sender;
            string modName = rb.realName.Split('_')[1];
            string catagoryName = rb.realName.Split('_')[0];
            string configName = rb.realName.Split('_')[2];
            string subConfigName = rb.realName.Split('_')[3];
            Mod m = this.linkMod(modName, catagoryName);
            Config cfg = m.getConfig(configName);
            SubConfig subc = cfg.getSubConfig(subConfigName);
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
                    if (item.Header is RelhaxRadioButton)
                    {
                        RelhaxRadioButton rbb = (RelhaxRadioButton)item.Header;
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
            RelhaxCheckbox cb = (RelhaxCheckbox)sender;
            string modName = cb.realName.Split('_')[1];
            string catagoryName = cb.realName.Split('_')[0];
            string configName = cb.realName.Split('_')[2];
            Mod m = this.linkMod(modName, catagoryName);
            Config cfg = m.getConfig(configName);
            cfg.configChecked = (bool)cb.IsChecked;
            //process the subconfigs
            bool configSelected = false;
            if (cfg.subConfigs.Count > 0)
            {
                System.Windows.Controls.TreeViewItem tvi = (System.Windows.Controls.TreeViewItem)cb.Parent;
                foreach (System.Windows.Controls.TreeViewItem subTVI in tvi.Items)
                {
                    RelhaxRadioButton subRB = (RelhaxRadioButton)subTVI.Header;
                    SubConfig subc = cfg.getSubConfig(subRB.realName.Split('_')[3]);
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
                        RelhaxRadioButton subRB = (RelhaxRadioButton)subTVI.Header;
                        SubConfig subc = cfg.getSubConfig(subRB.realName.Split('_')[3]);
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
            RelhaxComboBox cb = (RelhaxComboBox)sender;
            //first check if this is init, meaning first time enabled
            if (cb.SelectedIndex == -1)
            {
                //it will run recurse with this method again with a selected index of 0
                cb.SelectedIndex = 0;
                return;
            }
            //get all cool info
            string catagory = cb.realName.Split('_')[0];
            string mod = cb.realName.Split('_')[1];
            Mod m = this.getCatagory(catagory).getMod(mod);
            //getting here means that an item is confirmed to be selected
            string configName = (string)cb.SelectedItem;
            //in case "_updated" was appended, split the string
            configName = configName.Split('_')[0];
            if ((bool)!cb.IsEnabled)
            {
                //disable the mod and return
                Config save = m.getConfig(configName);
                save.configChecked = false;
                return;
            }
            //itterate through the items, get each config, disable it
            //unless it's the same name as the selectedItem
            foreach (string s in cb.Items)
            {
                Config cfg = m.getConfig(s.Split('_')[0]);
                cfg.configChecked = false;
                if (s.Split('_')[0].Equals(configName))
                {
                    cfg.configChecked = true;
                }
            }
        }
        //when a radiobutton of the legacy view mode is clicked
        void configControlRB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (loadingConfig)
                return;
            //get all required cool stuff
            RelhaxRadioButton cb = (RelhaxRadioButton)sender;
            string modName = cb.realName.Split('_')[1];
            string catagoryName = cb.realName.Split('_')[0];
            string configName = cb.realName.Split('_')[2];
            Mod m = this.linkMod(modName, catagoryName);
            Config cfg = m.getConfig(configName);
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
                    if (item.Header is RelhaxRadioButton)
                    {
                        RelhaxRadioButton rb = (RelhaxRadioButton)item.Header;
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
                    RelhaxRadioButton subRB = (RelhaxRadioButton)subTVI.Header;
                    SubConfig subc = cfg.getSubConfig(subRB.realName.Split('_')[3]);
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
                        RelhaxRadioButton subRB = (RelhaxRadioButton)subTVI.Header;
                        SubConfig subc = cfg.getSubConfig(subRB.realName.Split('_')[3]);
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
            if (sender is RelhaxCheckbox)
            {
                RelhaxCheckbox cb = (RelhaxCheckbox)sender;
                Mod m = this.linkMod(cb.realName.Split('_')[1]);
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
        //when a legacy mod checkbox is clicked
        void modCheckBoxL_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (loadingConfig)
                return;
            RelhaxCheckbox cb = (RelhaxCheckbox)sender;
            string modName = cb.realName.Split('_')[1];
            string catagoryName = cb.realName.Split('_')[0];
            Mod m = this.linkMod(modName, catagoryName);
            Catagory cat = this.getCatagory(catagoryName);
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
                        RelhaxCheckbox modCB = (RelhaxCheckbox)tvi.Header;
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
                if (c is RelhaxComboBox)
                {
                    RelhaxComboBox cbox = (RelhaxComboBox)c;
                    //if the mod is checked and it has more than 0 item enable, else disable, then trigger
                    if (m.modChecked && cbox.Items.Count > 0)
                        cbox.IsEnabled = true;
                    else
                        cbox.IsEnabled = false;
                    configControlDDALL_SelectionChanged(cbox, null);
                }
                else if (c is RelhaxCheckbox)
                {
                    RelhaxCheckbox cbox = (RelhaxCheckbox)c;
                    //multi CB code
                    //CB is enabled if the mod checked and the config is enabled
                    cfg = m.getConfig(cbox.realName.Split('_')[2]);
                    if (m.modChecked && cfg.enabled)
                        cbox.IsEnabled = true;
                    else
                        cbox.IsEnabled = false;
                    configControlCB_Click(cbox,null);
                }
                else if (c is RelhaxRadioButton)
                {
                    RelhaxRadioButton cbox = (RelhaxRadioButton)c;
                    cfg = m.getConfig(cbox.realName.Split('_')[2]);
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
                            if (c2 is RelhaxRadioButton)
                            {
                                RelhaxRadioButton c2r = (RelhaxRadioButton)c2;
                                cfg = m.getConfig(c2r.realName.Split('_')[2]);
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
        private void addMod(Mod m, TabPage t, int panelCount)
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
            ComboBox configControlDD = new ComboBox();
            configControlDD.AutoSize = true;
            configControlDD.Location = new System.Drawing.Point(0, 0);
            configControlDD.Size = new System.Drawing.Size(225, 15);
            configControlDD.TabIndex = 1;
            configControlDD.TabStop = true;
            configControlDD.Enabled = false;
            configControlDD.SelectedIndexChanged += new EventHandler(configControlDD_SelectedIndexChanged);
            configControlDD.Name = t.Name + "_" + m.name + "_DropDown";
            configControlDD.DropDownStyle = ComboBoxStyle.DropDownList;
            ComboBox configControlDD2 = new ComboBox();
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
                ComboBox configControlDDALL = null;
                if (m.configs[i].type.Equals("single") || m.configs[i].type.Equals("single1"))
                {
                    modHasRadioButtons = true;
                    //make default radioButton
                    RadioButton configControlRB = new RadioButton();
                    configControlRB.AutoSize = true;
                    configControlRB.Location = new Point(6, getYLocation(configPanel.Controls));
                    configControlRB.Size = new System.Drawing.Size(150, 15);
                    configControlRB.TabIndex = 1;
                    configControlRB.TabStop = true;
                    configControlRB.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
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
                    string oldCRC = Settings.GetMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile);
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
                        RadioButton subRB = new RadioButton();
                        subRB.AutoSize = true;
                        subRB.Location = new Point(6, (15 * j) + 3);
                        subRB.Size = new Size(150, 15);
                        subRB.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
                        subRB.Name = t.Name + "_" + m.name + "_" + m.configs[i].name + "_" + sc.name;
                        //logic for the radioButtons
                        subRB.Enabled = false;
                        subRB.Checked = false;
                        if (m.enabled && m.modChecked && m.configs[i].enabled && m.configs[i].configChecked && sc.enabled)
                        {
                            subRB.Enabled = true;
                            if (sc.Checked)
                                subRB.Checked = true;
                        }
                        //add handlers
                        subRB.CheckedChanged += new EventHandler(subRB_CheckedChanged);
                        //run checksum logic
                        subRB.Text = sc.name;
                        string oldSubCRC = Settings.GetMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + sc.zipFile);
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
                    string oldCRC = Settings.GetMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile);
                    if (!oldCRC.Equals(m.configs[i].crc) && (!m.configs[i].crc.Equals("")))
                    {
                        string toAdd = m.configs[i].name + "_Updated";
                        if (m.configs[i].size > 0.0f)
                            toAdd = toAdd + " (" + m.configs[i].size + " MB)";
                        //add it with _updated
                        if (m.configs[i].enabled) configControlDDALL.Items.Add(toAdd);
                        if (m.configs[i].configChecked)
                        {
                            configControlDDALL.SelectedItem = toAdd;
                            configControlDDALL.Enabled = true;
                        }
                    }
                    else
                    {
                        //add it
                        if (m.configs[i].enabled) configControlDDALL.Items.Add(m.configs[i].name);
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
                    CheckBox configControlCB = new CheckBox();
                    configControlCB.AutoSize = true;
                    configControlCB.Location = new Point(6, getYLocation(configPanel.Controls));
                    configControlCB.Size = new System.Drawing.Size(150, 15);
                    configControlCB.TabIndex = 1;
                    configControlCB.TabStop = true;
                    configControlCB.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
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
                    string oldCRC = Settings.GetMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile);
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
                        RadioButton subRB = new RadioButton();
                        subRB.AutoSize = true;
                        subRB.Location = new Point(6, (15 * j) + 3);
                        subRB.Size = new Size(150, 15);
                        subRB.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
                        subRB.Name = t.Name + "_" + m.name + "_" + m.configs[i].name + "_" + sc.name;
                        //logic for the radioButtons
                        subRB.Enabled = false;
                        subRB.Checked = false;
                        if (m.enabled && m.modChecked && m.configs[i].enabled && m.configs[i].configChecked && sc.enabled)
                        {
                            subRB.Enabled = true;
                            if (sc.Checked)
                                subRB.Checked = true;
                        }
                        //add handlers
                        subRB.CheckedChanged += new EventHandler(subRB_CheckedChanged);
                        //run checksum logic
                        subRB.Text = sc.name;
                        string oldSubCRC = Settings.GetMd5Hash(Application.StartupPath + "\\RelHaxDownloads\\" + sc.zipFile);
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
                    Settings.appendToLog("WARNING: Unknown config type for " + m.configs[i].name + ": " + m.configs[i].type);
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
            modCheckBox.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
            //the mod checksum logic
            string modDownloadPath = Application.StartupPath + "\\RelHaxDownloads\\" + m.modZipFile;
            string oldCRC2 = Settings.GetMd5Hash(modDownloadPath);
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
                if ((c is CheckBox) || (c is ComboBox) || (c is Panel) || (c is RadioButton))
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
            //if no index selected, select one
            if (cb.SelectedIndex == -1)
            {
                cb.SelectedIndex = 0;
                return;
            }
            //getting here means that an item is confirmed to be selected
            string configName = (string)cb.SelectedItem;
            //in case "_updated" was appended, split the string
            configName = configName.Split('_')[0];
            if (!cb.Enabled)
            {
                //disable the mod and return
                Config save = m.getConfig(configName);
                save.configChecked = false;
                return;
            }
            //itterate through the items, get each config, disable it
            //unless it's the same name as the selectedItem
            foreach (string s in cb.Items)
            {
                Config cfg = m.getConfig(s.Split('_')[0]);
                cfg.configChecked = false;
                if (s.Split('_')[0].Equals(configName))
                {
                    cfg.configChecked = true;
                }
            }
        }
        //handler for when the config checkbox is checked or unchecked
        void configControlCB_CheckedChanged(object sender, EventArgs e)
        {
            if (loadingConfig)
                return;
            //checkboxes don't need to be unselected
            CheckBox cb = (CheckBox)sender;
            string modName = cb.Name.Split('_')[1];
            string catagoryName = cb.Name.Split('_')[0];
            string configName = cb.Name.Split('_')[2];
            Mod m = this.linkMod(modName, catagoryName);
            Config cfg = m.getConfig(configName);
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
                        if ((p is Panel) && (p.Name.Equals(catagoryName + "_" + modName + "_" + configName + "_subConfigPanel")))
                        {
                            Panel pan = (Panel)p;
                            pan.BackColor = Settings.getBackColor();
                            foreach (Control cont in pan.Controls)
                            {
                                RadioButton r = (RadioButton)cont;
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
                        if ((p is Panel) && (p.Name.Equals(catagoryName + "_" + modName + "_" + configName + "_subConfigPanel")))
                        {
                            Panel pan = (Panel)p;
                            pan.BackColor = Color.BlanchedAlmond;
                            foreach (Control cont in pan.Controls)
                            {
                                RadioButton r = (RadioButton)cont;
                                SubConfig sc2 = cfg.getSubConfig(r.Name.Split('_')[3]);
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
                            if ((p is Panel) && (p.Name.Equals(catagoryName + "_" + modName + "_" + configName + "_subConfigPanel")))
                            {
                                Panel pan = (Panel)p;
                                foreach (Control cont in pan.Controls)
                                {
                                    RadioButton r = (RadioButton)cont;
                                    SubConfig sc2 = cfg.getSubConfig(r.Name.Split('_')[3]);
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
            RadioButton rb = (RadioButton)sender;
            string modName = rb.Name.Split('_')[1];
            string catagoryName = rb.Name.Split('_')[0];
            string configName = rb.Name.Split('_')[2];
            Panel configPanel = (Panel)rb.Parent;
            Mod m = this.linkMod(modName, catagoryName);
            Config c = m.getConfig(configName);
            //verify mod is enabled
            if (!rb.Enabled || !rb.Checked)
            {
                c.configChecked = false;
                if (c.subConfigs.Count > 0)
                {
                    //modify the panel and trigger the subConfigs
                    foreach (Control p in configPanel.Controls)
                    {
                        if ((p is Panel) && (p.Name.Equals(catagoryName + "_" + modName + "_" + configName + "_subConfigPanel")))
                        {
                            Panel pan = (Panel)p;
                            pan.BackColor = Settings.getBackColor();
                            foreach (Control cont in pan.Controls)
                            {
                                RadioButton r = (RadioButton)cont;
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
                    if (ctrl is RadioButton)
                    {
                        RadioButton rbz = (RadioButton)ctrl;
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
                    if ((p is Panel) && (p.Name.Equals(catagoryName + "_" + modName + "_" + configName + "_subConfigPanel")))
                    {
                        Panel pan = (Panel)p;
                        pan.BackColor = Color.BlanchedAlmond;
                        foreach (Control cont in pan.Controls)
                        {
                            RadioButton r = (RadioButton)cont;
                            SubConfig sc2 = c.getSubConfig(r.Name.Split('_')[3]);
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
                        if ((p is Panel) && (p.Name.Equals(catagoryName + "_" + modName + "_" + configName + "_subConfigPanel")))
                        {
                            Panel pan = (Panel)p;
                            foreach (Control cont in pan.Controls)
                            {
                                RadioButton r = (RadioButton)cont;
                                SubConfig sc2 = c.getSubConfig(r.Name.Split('_')[3]);
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
            RadioButton rb = (RadioButton)sender;
            string modName = rb.Name.Split('_')[1];
            string catagoryName = rb.Name.Split('_')[0];
            string configName = rb.Name.Split('_')[2];
            string subConfig = rb.Name.Split('_')[3];
            Panel configSelection = (Panel)rb.Parent;
            Mod m = this.linkMod(modName, catagoryName);
            Config c = m.getConfig(configName);
            Panel configPanel = (Panel)rb.Parent;
            SubConfig sfg = c.getSubConfig(subConfig);
            //SubConfig sc = c.getSubConfig(subConfig);
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
                    if (ctrl is RadioButton)
                    {
                        RadioButton rbz = (RadioButton)ctrl;
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
            //user mod code
            CheckBox cbUser = (CheckBox)sender;
            if (cbUser.Parent is TabPage)
            {
                TabPage t = (TabPage)cbUser.Parent;
                if (t.Text.Equals("User Mods"))
                {
                    //this is a check from the user checkboxes
                    Mod m2 = this.getUserMod(cbUser.Text);
                    if (m2 != null)
                        m2.modChecked = cbUser.Checked;
                    return;
                }
            }
            //the mod info handler should only be concerned with checking for enabling componets

            //get all required info for this checkbox change
            CheckBox cb = (CheckBox)sender;
            //this parent could nwo be the radioButton config selection panel or the config panel
            Panel modPanel = (Panel)cb.Parent;
            TabPage modTab = (TabPage)modPanel.Parent;
            string modName = cb.Name.Split('_')[1];
            string catagoryName = cb.Name.Split('_')[0];
            Mod m = this.linkMod(modName, catagoryName);
            Catagory cat = this.getCatagory(catagoryName);

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
                                if (ccc is CheckBox)
                                {
                                    CheckBox cbb = (CheckBox)ccc;
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
                if (cc is ComboBox)
                {
                    //comboBox could be recieved as enabled(configs of this type exist), but with 0 enabled configs which means count would be 0
                    //toggle the comboBox and trigger selected index
                    ComboBox ccB = (ComboBox)cc;
                    if (cb.Checked && (ccB.Items.Count > 0))
                        ccB.Enabled = true;
                    else
                        ccB.Enabled = false;
                    configControlDD_SelectedIndexChanged(ccB, null);
                }
                else if (cc is CheckBox)
                {
                    cfg = m.getConfig(cc.Name.Split('_')[2]);
                    //checkbox is enabled if mod is checked AND config is enabled
                    //toggle enabledness of checkbox based on modCheckBox checked
                    //and trigger the checkbox handler
                    if (cb.Checked && cfg.enabled)
                        cc.Enabled = true;
                    else
                        cc.Enabled = false;
                    configControlCB_CheckedChanged(cc, null);
                }
                else if (cc is RadioButton)
                {
                    cfg = m.getConfig(cc.Name.Split('_')[2]);
                    RadioButton ccRB = (RadioButton)cc;
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
                            if (cccc is RadioButton)
                            {
                                RadioButton ccccc = (RadioButton)cccc;
                                cfg = m.getConfig(ccccc.Name.Split('_')[2]);
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
        //parses the xml mod info into the memory database
        private void createModStructure2(string databaseURL)
        {
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.Load(databaseURL);
            }
            catch (XmlException)
            {
                Settings.appendToLog("CRITICAL: Failed to read database!");
                MessageBox.Show(Translations.getTranslatedString("databaseReadFailed"));
                Application.Exit();
            }
            catch (System.Net.WebException e)
            {
                Settings.appendToLog("EXCEPTION: WebException (call stack traceback)");
                Settings.appendToLog(e.StackTrace);
                Settings.appendToLog("inner message: " + e.Message);
                Settings.appendToLog("source: " + e.Source);
                Settings.appendToLog("target: " + e.TargetSite);
                Settings.appendToLog("Additional Info: Tried to access " + databaseURL);
                MessageBox.Show(Translations.getTranslatedString("databaseNotFound"));
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
                        case "startAddress":
                            d.startAddress = globs.InnerText;
                            break;
                        case "endAddress":
                            d.endAddress = globs.InnerText;
                            break;
                        case "dependencyenabled":
                            d.enabled = Settings.parseBool(globs.InnerText, false);
                            break;
                    }
                }
                globalDependencies.Add(d);
            }
            XmlNodeList catagoryList = doc.SelectNodes("//modInfoAlpha.xml/catagories/catagory");
            parsedCatagoryList = new List<Catagory>();
            foreach (XmlNode catagoryHolder in catagoryList)
            {
                Catagory cat = new Catagory();
                foreach (XmlNode catagoryNode in catagoryHolder.ChildNodes)
                {
                    switch (catagoryNode.Name)
                    {
                        case "name":
                            cat.name = catagoryNode.InnerText;
                            break;
                        case "selectionType":
                            cat.selectionType = catagoryNode.InnerText;
                            break;
                        case "mods":
                            foreach (XmlNode modHolder in catagoryNode.ChildNodes)
                            {
                                Mod m = new Mod();
                                foreach (XmlNode modNode in modHolder.ChildNodes)
                                {
                                    switch (modNode.Name)
                                    {
                                        case "name":
                                            m.name = modNode.InnerText;
                                            break;
                                        case "version":
                                            m.version = modNode.InnerText;
                                            break;
                                        case "size":
                                            m.size = Settings.parseFloat(modNode.InnerText, 0.0f);
                                            break;
                                        case "modzipfile":
                                            m.modZipFile = modNode.InnerText;
                                            break;
                                        case "startAddress":
                                            m.startAddress = modNode.InnerText;
                                            break;
                                        case "endAddress":
                                            m.endAddress = modNode.InnerText;
                                            break;
                                        case "modzipcrc":
                                            m.crc = modNode.InnerText;
                                            break;
                                        case "enabled":
                                            m.enabled = Settings.parseBool(modNode.InnerText, false);
                                            break;
                                        case "description":
                                            m.description = modNode.InnerText;
                                            break;
                                        case "updateComment":
                                            m.updateComment = modNode.InnerText;
                                            break;
                                        case "devURL":
                                            m.devURL = modNode.InnerText;
                                            break;
                                        case "userDatas":
                                            foreach (XmlNode userDataNode in modNode.ChildNodes)
                                            {

                                                switch (userDataNode.Name)
                                                {
                                                    case "userData":
                                                        string innerText = userDataNode.InnerText;
                                                        if (innerText == null)
                                                            continue;
                                                        if (innerText.Equals(""))
                                                            continue;
                                                        m.userFiles.Add(innerText);
                                                        break;
                                                }

                                            }
                                            break;
                                        case "pictures":
                                            //parse every picture
                                            foreach (XmlNode pictureHolder in modNode.ChildNodes)
                                            {
                                                foreach (XmlNode pictureNode in pictureHolder.ChildNodes)
                                                {
                                                    switch (pictureNode.Name)
                                                    {
                                                        case "URL":
                                                            string innerText = pictureNode.InnerText;
                                                            if (innerText == null)
                                                                continue;
                                                            if (innerText.Equals(""))
                                                                continue;
                                                            m.picList.Add(new Picture("Mod: " + m.name, pictureNode.InnerText));
                                                            break;
                                                    }
                                                }
                                            }
                                            break;
                                        case "dependencies":
                                            //parse all dependencies
                                            foreach (XmlNode dependencyHolder in modNode.ChildNodes)
                                            {
                                                Dependency d = new Dependency();
                                                foreach (XmlNode dependencyNode in dependencyHolder.ChildNodes)
                                                {
                                                    switch (dependencyNode.Name)
                                                    {
                                                        case "dependencyZipFile":
                                                            d.dependencyZipFile = dependencyNode.InnerText;
                                                            break;
                                                        case "dependencyZipCRC":
                                                            d.dependencyZipCRC = dependencyNode.InnerText;
                                                            break;
                                                        case "startAddress":
                                                            d.startAddress = dependencyNode.InnerText;
                                                            break;
                                                        case "endAddress":
                                                            d.endAddress = dependencyNode.InnerText;
                                                            break;
                                                        case "dependencyenabled":
                                                            d.enabled = Settings.parseBool(dependencyNode.InnerText, false);
                                                            break;
                                                    }
                                                }
                                                m.modDependencies.Add(d);
                                            }
                                            break;
                                        case "configs":
                                            //parse every config for that mod
                                            foreach (XmlNode configHolder in modNode.ChildNodes)
                                            {
                                                Config c = new Config();
                                                foreach (XmlNode configNode in configHolder.ChildNodes)
                                                {
                                                    switch (configNode.Name)
                                                    {
                                                        case "name":
                                                            c.name = configNode.InnerText;
                                                            break;
                                                        case "configzipfile":
                                                            c.zipConfigFile = configNode.InnerText;
                                                            break;
                                                        case "startAddress":
                                                            c.startAddress = configNode.InnerText;
                                                            break;
                                                        case "endAddress":
                                                            c.endAddress = configNode.InnerText;
                                                            break;
                                                        case "configzipcrc":
                                                            c.crc = configNode.InnerText;
                                                            break;
                                                        case "configenabled":
                                                            c.enabled = Settings.parseBool(configNode.InnerText, false);
                                                            break;
                                                        case "size":
                                                            c.size = Settings.parseFloat(configNode.InnerText, 0.0f);
                                                            break;
                                                        case "configtype":
                                                            c.type = configNode.InnerText;
                                                            break;
                                                        case "pictures":
                                                            //parse every picture
                                                            foreach (XmlNode pictureHolder in configNode.ChildNodes)
                                                            {
                                                                foreach (XmlNode pictureNode in pictureHolder.ChildNodes)
                                                                {
                                                                    switch (pictureNode.Name)
                                                                    {
                                                                        case "URL":
                                                                            string innerText = pictureNode.InnerText;
                                                                            if (innerText == null)
                                                                                continue;
                                                                            if (innerText.Equals(""))
                                                                                continue;
                                                                            m.picList.Add(new Picture("Config: " + c.name, pictureNode.InnerText));
                                                                            break;
                                                                    }
                                                                }
                                                            }
                                                            break;
                                                        case "subConfigs":
                                                            //parse every subConfig
                                                            foreach (XmlNode subConfigHolder in configNode.ChildNodes)
                                                            {
                                                                SubConfig subC = new SubConfig();
                                                                foreach (XmlNode subConfigNode in subConfigHolder.ChildNodes)
                                                                {
                                                                    switch (subConfigNode.Name)
                                                                    {
                                                                        case "name":
                                                                            subC.name = subConfigNode.InnerText;
                                                                            break;
                                                                        case "zipFile":
                                                                            subC.zipFile = subConfigNode.InnerText;
                                                                            break;
                                                                        case "crc":
                                                                            subC.crc = subConfigNode.InnerText;
                                                                            break;
                                                                        case "enabled":
                                                                            subC.enabled = Settings.parseBool(subConfigNode.InnerText, false);
                                                                            break;
                                                                        case "checked":
                                                                            subC.Checked = Settings.parseBool(subConfigNode.InnerText, false);
                                                                            break;
                                                                        case "type":
                                                                            subC.type = subConfigNode.InnerText;
                                                                            break;
                                                                        case "pictures":
                                                                            //parse every picture
                                                                            foreach (XmlNode pictureHolder in subConfigNode.ChildNodes)
                                                                            {
                                                                                foreach (XmlNode pictureNode in pictureHolder.ChildNodes)
                                                                                {
                                                                                    switch (pictureNode.Name)
                                                                                    {
                                                                                        case "URL":
                                                                                            string innerText = pictureNode.InnerText;
                                                                                            if (innerText == null)
                                                                                                continue;
                                                                                            if (innerText.Equals(""))
                                                                                                continue;
                                                                                            m.picList.Add(new Picture("Config: " + c.name, pictureNode.InnerText));
                                                                                            break;
                                                                                    }
                                                                                }
                                                                            }
                                                                            break;
                                                                        case "dependencies":
                                                                            //parse every dependency
                                                                            foreach (XmlNode dependencyHolder in subConfigNode.ChildNodes)
                                                                            {
                                                                                Dependency d = new Dependency();
                                                                                foreach (XmlNode dependencyNode in dependencyHolder.ChildNodes)
                                                                                {
                                                                                    switch (dependencyNode.Name)
                                                                                    {
                                                                                        case "dependencyZipFile":
                                                                                            d.dependencyZipFile = dependencyNode.InnerText;
                                                                                            break;
                                                                                        case "dependencyZipCRC":
                                                                                            d.dependencyZipCRC = dependencyNode.InnerText;
                                                                                            break;
                                                                                        case "startAddress":
                                                                                            d.startAddress = dependencyNode.InnerText;
                                                                                            break;
                                                                                        case "endAddress":
                                                                                            d.endAddress = dependencyNode.InnerText;
                                                                                            break;
                                                                                        case "dependencyEnabled":
                                                                                            d.enabled = Settings.parseBool(dependencyNode.InnerText, false);
                                                                                            break;
                                                                                    }
                                                                                }
                                                                                subC.dependencies.Add(d);
                                                                            }
                                                                            break;
                                                                        case "size":
                                                                            subC.size = Settings.parseFloat(subConfigNode.InnerText, 0.0f);
                                                                            break;
                                                                        case "startAddress":
                                                                            subC.startAddress = subConfigNode.InnerText;
                                                                            break;
                                                                        case "endAddress":
                                                                            subC.endAddress = subConfigNode.InnerText;
                                                                            break;
                                                                    }
                                                                }
                                                                c.subConfigs.Add(subC);
                                                            }
                                                            break;
                                                        case "dependencies":
                                                            //parse all dependencies
                                                            foreach (XmlNode dependencyHolder in configNode.ChildNodes)
                                                            {
                                                                Dependency d = new Dependency();
                                                                foreach (XmlNode dependencyNode in dependencyHolder.ChildNodes)
                                                                {
                                                                    switch (dependencyNode.Name)
                                                                    {
                                                                        case "dependencyZipFile":
                                                                            d.dependencyZipFile = dependencyNode.InnerText;
                                                                            break;
                                                                        case "dependencyZipCRC":
                                                                            d.dependencyZipCRC = dependencyNode.InnerText;
                                                                            break;
                                                                        case "startAddress":
                                                                            d.startAddress = dependencyNode.InnerText;
                                                                            break;
                                                                        case "endAddress":
                                                                            d.endAddress = dependencyNode.InnerText;
                                                                            break;
                                                                        case "dependencyenabled":
                                                                            d.enabled = Settings.parseBool(dependencyNode.InnerText, false);
                                                                            break;
                                                                    }
                                                                }
                                                                m.modDependencies.Add(d);
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
                            foreach (XmlNode dependencyHolder in catagoryNode.ChildNodes)
                            {
                                Dependency d = new Dependency();
                                foreach (XmlNode dependencyNode in dependencyHolder.ChildNodes)
                                {
                                    switch (dependencyNode.Name)
                                    {
                                        case "dependencyZipFile":
                                            d.dependencyZipFile = dependencyNode.InnerText;
                                            break;
                                        case "dependencyZipCRC":
                                            d.dependencyZipCRC = dependencyNode.InnerText;
                                            break;
                                        case "startAddress":
                                            d.startAddress = dependencyNode.InnerText;
                                            break;
                                        case "endAddress":
                                            d.endAddress = dependencyNode.InnerText;
                                            break;
                                        case "dependencyenabled":
                                            d.enabled = Settings.parseBool(dependencyNode.InnerText, false);
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
                    Settings.appendToLog("ERROR: " + filePath + " not found, not loading configs");
                    MessageBox.Show(Translations.getTranslatedString("configLoadFailed"));
                    return;
                }
            }
            else if (loadMode == loadConfigMode.fromSaveLastConfig)
            {
                filePath = Application.StartupPath + "\\RelHaxUserConfigs\\lastInstalledConfig.xml";
                if (!File.Exists(filePath))
                {
                    Settings.appendToLog("ERROR: " + filePath + " not found, not loading configs");
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
            Settings.appendToLog("Loading mod selections from " + filePath);
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
                            if (m == null)
                            {
                                Settings.appendToLog("WARNING: mod \"" + nn.InnerText + "\" not found");
                                MessageBox.Show(Translations.getTranslatedString("modNotFound_1") + nn.InnerText + Translations.getTranslatedString("modNotFound_2"));
                                continue;
                            }
                            if (m.enabled)
                            {
                                Settings.appendToLog("Checking mod " + m.name);
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
                                                Settings.appendToLog("WARNING: config \"" + nnnn.InnerText + "\" not found for mod \"" + nn.InnerText + "\"");
                                                MessageBox.Show(Translations.getTranslatedString("configNotFound_1") + nnnn.InnerText + Translations.getTranslatedString("configNotFound_2") + nn.InnerText + Translations.getTranslatedString("configNotFound_3"));
                                                continue;
                                            }
                                            if (c.enabled)
                                            {
                                                Settings.appendToLog("Checking config " + c.name);
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
                                    Settings.appendToLog("checking user mod " + m.modZipFile);
                                }
                            }
                            break;
                    }
                }
            }
            Settings.appendToLog("Finished loading mod selections");
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
                SetTaskbarState(AppBarStates.AutoHide);
            //save wether the window was in fullscreen mods before closing
            if (this.WindowState == FormWindowState.Maximized)
                Settings.ModSelectionFullscreen = true;
            else
                Settings.ModSelectionFullscreen = false;
            //close the preview window if it is open
            if (p != null)
                p.Close();
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
            Settings.appendToLog("Unchecking all mods");
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
