using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Xml;


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
        public ModSelectionList(string version, string theTanksVersion)
        {
            InitializeComponent();
            tanksVersion = version;
            tanksLocation = theTanksVersion;
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
            pw = new PleaseWait();
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
                            pw.loadingDescBox.Text = Translations.getTranslatedString("loading") + " " + m.name;
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
            configPanel.Controls.Clear();
            //add configs to the panel
            //create the comboBox outside of the loop
            //later add it if the items count is above 0
            ComboBox configControlDD = new ComboBox();
            configControlDD.AutoSize = true;
            configControlDD.Location = new System.Drawing.Point(0, 0);
            configControlDD.Size = new System.Drawing.Size(200, 15);
            configControlDD.TabIndex = 1;
            configControlDD.TabStop = true;
            configControlDD.Enabled = false;
            configControlDD.SelectedIndexChanged += new EventHandler(configControlDD_SelectedIndexChanged);
            configControlDD.Name = t.Name + "_" + m.name + "_DropDown";
            configControlDD.DropDownStyle = ComboBoxStyle.DropDownList;
            configControlDD.Items.Clear();
            ComboBox configControlDD2 = new ComboBox();
            configControlDD2.AutoSize = true;
            configControlDD2.Location = new System.Drawing.Point(0, 0);
            configControlDD2.Size = new System.Drawing.Size(200, 15);
            configControlDD2.TabIndex = 1;
            configControlDD2.TabStop = true;
            configControlDD2.Enabled = false;
            configControlDD2.SelectedIndexChanged += new EventHandler(configControlDD_SelectedIndexChanged);
            configControlDD2.Name = t.Name + "_" + m.name + "_DropDown2";
            configControlDD2.DropDownStyle = ComboBoxStyle.DropDownList;
            configControlDD2.Items.Clear();
            //label for the dropdown selection. shows the mod size
            //and the eta download time (if applicable)
            Label dropDownSizeLabel = new Label();
            dropDownSizeLabel.Location = new Point(0, 0);
            //create the placeholders for up to 4 possible individual selections
            //panel 1
            Panel configSelection1 = new Panel();
            configSelection1.Location = new System.Drawing.Point(0, 0);
            Panel configSelection2 = new Panel();
            configSelection2.Location = new System.Drawing.Point(0, 0);
            Panel configSelection3 = new Panel();
            configSelection3.Location = new System.Drawing.Point(0, 0);
            Panel configSelection4 = new Panel();
            configSelection4.Location = new System.Drawing.Point(0, 0);
            int yPosition = 15;
            for (int i = 0; i < m.configs.Count; i++)
            {
                ComboBox configControlDDALL = null;
                Label configLabel = null;
                Panel configSelectionAll = null;
                if (m.configs[i].type.Equals("single") || m.configs[i].type.Equals("single1"))
                {
                    modHasRadioButtons = true;
                    //make default radioButton
                    //label
                    configLabel = new Label();
                    configLabel.AutoSize = true;
                    configLabel.Location = new System.Drawing.Point(5, yPosition);
                    configLabel.Size = new System.Drawing.Size(100, 15);
                    configLabel.TabIndex = 0;
                    configLabel.Text = m.configs[i].name;
                    configLabel.Name = t.Name + "_" + m.name + "_" + m.configs[i].name;
                    configLabel.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
                    if (m.enabled && m.modChecked)
                        configLabel.Enabled = true;
                    else
                        configLabel.Enabled = false;
                    configLabel.Location = new Point(configLabel.Location.X, getYLocation(configPanel.Controls));
                    configPanel.Controls.Add(configLabel);
                    //then radioButton
                    RadioButton configControlRB = new RadioButton();
                    configControlRB.AutoSize = true;
                    //configControlRB.Location = new System.Drawing.Point(100, yPosition - 10);
                    configControlRB.Location = new Point(configLabel.Location.X + configLabel.Size.Width + 6, getYLocation(configPanel.Controls));
                    configControlRB.Size = new System.Drawing.Size(150, 15);
                    configControlRB.TabIndex = 1;
                    configControlRB.TabStop = true;
                    configControlRB.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
                    if (m.enabled && m.modChecked && m.configs[i].enabled)
                        configControlRB.Enabled = true;
                    else
                        configControlRB.Enabled = false;
                    if (m.enabled && m.modChecked && m.configs[i].enabled && m.configs[i].configChecked)
                    {
                        configControlRB.Checked = true;
                        hasRadioButtonConfigSelected = true;
                    }
                    else
                        configControlRB.Checked = false;
                    configControlRB.CheckedChanged += new EventHandler(configControlRB_CheckedChanged);
                    configControlRB.Name = t.Name + "_" + m.name + "_" + m.configs[i].name;
                    string configDownloadPath2 = Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile;
                    if (File.Exists(configDownloadPath2))
                    {
                        string oldCRC = Settings.GetMd5Hash(configDownloadPath2);
                        if (!oldCRC.Equals(m.configs[i].crc))
                        {
                            configControlRB.Text = configControlRB.Text + "(Updated)";
                            if (m.configs[i].size > 0.0f)
                            {
                                int totalSec = (int)m.configs[i].size * 2;
                                int totalRealMin = totalSec / 60;
                                int totalRealSec = totalSec % 60;
                                configControlRB.Text = configControlRB.Text + " (" + m.configs[i].size + " MB)";
                            }
                        }
                    }
                    else if (!(File.Exists(configDownloadPath2)) && (m.configs[i].crc != null) && (!m.configs[i].crc.Equals("")))
                    {
                        configControlRB.Text = configControlRB.Text + "(Updated)";
                        if (m.configs[i].size > 0.0f)
                        {
                            int totalSec = (int)m.configs[i].size * 2;
                            int totalRealMin = totalSec / 60;
                            int totalRealSec = totalSec % 60;
                            configControlRB.Text = configControlRB.Text + " (" + m.configs[i].size + " MB)";
                        }
                    }
                    configPanel.Controls.Add(configControlRB);
                    continue;
                }
                else if (m.configs[i].type.Equals("single2") || m.configs[i].type.Equals("single3") || m.configs[i].type.Equals("single4"))
                {
                    //check to see if the configSelection1 panel needs to be
                    if (m.configs[i].type.Equals("single2"))
                    {
                        configSelectionAll = configSelection2;
                    }
                    else if (m.configs[i].type.Equals("single3"))
                    {
                        configSelectionAll = configSelection3;
                    }
                    else if (m.configs[i].type.Equals("single4"))
                    {
                        configSelectionAll = configSelection4;
                    }
                    if (configSelectionAll.Location.X == 0 && configSelectionAll.Location.Y == 0)
                    {
                        //initialize it
                        configSelectionAll.Enabled = true;
                        configSelectionAll.Name = m.configs[i].type;
                        configSelectionAll.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
                        configSelectionAll.Location = new System.Drawing.Point(3, getYLocation(configPanel.Controls));
                        configSelectionAll.TabIndex = 2;
                        configSelectionAll.AutoSize = true;
                        configSelectionAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowOnly;
                        configSelectionAll.Size = new System.Drawing.Size(t.Size.Width - 35, 30);
                        configSelectionAll.BackColor = Settings.getBackColor();
                        configSelectionAll.Controls.Clear();
                        yPosition += 5;
                        CheckBox EnableCB = new CheckBox();
                        EnableCB.AutoSize = true;
                        EnableCB.Location = new System.Drawing.Point(5, (15 * (configSelectionAll.Controls.Count / 2)) + 5);
                        EnableCB.Size = new System.Drawing.Size(100, 15);
                        EnableCB.TabIndex = 0;
                        EnableCB.Text = "";
                        EnableCB.Name = t.Name + "_" + m.name + "_" + m.configs[i].name + "-" + m.configs[i].type + "_sectionEnabler";
                        if (m.enabled && m.modChecked)
                            EnableCB.Enabled = true;
                        else
                            EnableCB.Enabled = false;
                        EnableCB.Checked = false;
                        EnableCB.CheckedChanged += new EventHandler(EnableCB_CheckedChanged);
                        configSelectionAll.Controls.Add(EnableCB);
                    }
                    //enable the checkbox for the radio button panel if a config is enabled and checked
                    if (m.enabled && m.modChecked && m.configs[i].enabled && m.configs[i].configChecked)
                    {
                        //clobber the controlls untill checkbox is found and enable it
                        foreach (Control findCB in configSelectionAll.Controls)
                        {
                            if (findCB is CheckBox)
                            {
                                CheckBox found = (CheckBox)findCB;
                                found.Checked = true;
                                configSelectionAll.BackColor = Color.BlanchedAlmond;
                                break;
                            }
                        }
                    }
                    configLabel = new Label();
                    configLabel.AutoSize = true;
                    configLabel.Location = new System.Drawing.Point(5, getYLocation(configSelectionAll.Controls));
                    configLabel.Size = new System.Drawing.Size(100, 15);
                    configLabel.TabIndex = 0;
                    configLabel.Text = m.configs[i].name;
                    configLabel.Name = t.Name + "_" + m.name + "_" + m.configs[i].name;
                    configLabel.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
                    if (m.enabled && m.modChecked)
                        configLabel.Enabled = true;
                    else
                        configLabel.Enabled = false;
                    configSelectionAll.Controls.Add(configLabel);
                    //make a radioButton
                    RadioButton configControlRB = new RadioButton();
                    configControlRB.AutoSize = true;
                    //configControlRB.Location = new System.Drawing.Point(100, yPosition - 10);
                    configControlRB.Location = new System.Drawing.Point(configLabel.Location.X + configLabel.Size.Width + 6, getYLocation(configSelectionAll.Controls));
                    configControlRB.Size = new System.Drawing.Size(150, 15);
                    configControlRB.TabIndex = 1;
                    configControlRB.TabStop = true;
                    configControlRB.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
                    if (m.enabled && m.modChecked && m.configs[i].enabled)
                        configControlRB.Enabled = true;
                    else
                        configControlRB.Enabled = false;
                    if (m.enabled && m.modChecked && m.configs[i].enabled && m.configs[i].configChecked)
                        configControlRB.Checked = true;
                    else
                        configControlRB.Checked = false;
                    configControlRB.CheckedChanged += new EventHandler(configControlRB_CheckedChanged);
                    configControlRB.Name = t.Name + "_" + m.name + "_" + m.configs[i].name;
                    //Check if a mod zip exists locally and if it's out of date
                    string configDownloadPath = Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile;
                    if (File.Exists(configDownloadPath))
                    {
                        string oldCRC = Settings.GetMd5Hash(configDownloadPath);
                        if (!oldCRC.Equals(m.configs[i].crc))
                        {
                            //the file exists, but it is out of date
                            configControlRB.Text = configControlRB.Text + "(Updated)";
                            if (m.configs[i].size > 0.0f)
                            {
                                int totalSec = (int)m.configs[i].size * 2;
                                int totalRealMin = totalSec / 60;
                                int totalRealSec = totalSec % 60;
                                configControlRB.Text = configControlRB.Text + " (" + m.configs[i].size + " MB)";
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
                            configControlRB.Text = configControlRB.Text + " (" + m.configs[i].size + " MB)";
                        }
                    }
                    //configPanel.Controls.Add(configControlRB);
                    configSelectionAll.Controls.Add(configControlRB);
                    //now the configSelectionAll loads the radiobuttons and later the configPanel loads the configSelection panels
                    configPanel.Controls.Add(configSelectionAll);
                    continue;
                }
                //may eventually have a second drop down menu option
                else if (m.configs[i].type.Equals("single_dropdown") || m.configs[i].type.Equals("single_dropdown1") || m.configs[i].type.Equals("single_dropdown2"))
                {

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
                        //add the label with text nothing selected
                        dropDownSizeLabel = new Label();
                        dropDownSizeLabel.AutoSize = true;
                        dropDownSizeLabel.Location = new Point(configControlDDALL.Location.X + configControlDDALL.Size.Width + 6, configControlDDALL.Location.Y + 3);
                        dropDownSizeLabel.TabIndex = 0;
                        dropDownSizeLabel.Text = "Nothing Selected";
                        dropDownSizeLabel.Name = t.Name + "_" + m.name + "_" + m.configs[i].type + "_size";
                        dropDownSizeLabel.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
                        configControlDDALL.Size = new Size(configControlDDALL.Size.Width, dropDownSizeLabel.Size.Height);
                        if (m.enabled && m.modChecked)
                        {
                            dropDownSizeLabel.Enabled = true;
                            configControlDDALL.Enabled = true;
                        }
                        else
                        {
                            dropDownSizeLabel.Enabled = false;
                            configControlDDALL.Enabled = false;
                        }
                        configPanel.Controls.Add(dropDownSizeLabel);
                    }
                    string configDownloadPath1 = Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile;
                    if (File.Exists(configDownloadPath1))
                    {
                        string oldCRC = Settings.GetMd5Hash(configDownloadPath1);
                        if (!oldCRC.Equals(m.configs[i].crc))
                        {
                            if (m.configs[i].enabled) configControlDDALL.Items.Add(m.configs[i].name + "_updated");
                            if (m.configs[i].configChecked) configControlDDALL.SelectedItem = m.configs[i].name + "_updated";
                            continue;
                        }
                    }
                    else if (!(File.Exists(configDownloadPath1)) && (m.configs[i].crc != null) && (!m.configs[i].crc.Equals("")))
                    {
                        //mod/config zip file does not exist locally, but a crc for it does, implying that the file needs to be downloaded
                        if (m.configs[i].enabled) configControlDDALL.Items.Add(m.configs[i].name + "_updated");
                        if (m.configs[i].configChecked) configControlDDALL.SelectedItem = m.configs[i].name + "_updated";
                        continue;
                    }
                    if (m.configs[i].enabled) configControlDDALL.Items.Add(m.configs[i].name);
                    //TODO: check if this works
                    if (m.configs[i].configChecked) configControlDDALL.SelectedItem = m.configs[i].name;
                    yPosition += 5;
                    continue;
                }
                else if (m.configs[i].type.Equals("multi"))
                {
                    //make configLabel if config type is not single_dropDown
                    configLabel = new Label();
                    configLabel.AutoSize = true;
                    configLabel.Location = new System.Drawing.Point(5, yPosition);
                    configLabel.Size = new System.Drawing.Size(100, 15);
                    configLabel.TabIndex = 0;
                    configLabel.Text = m.configs[i].name;
                    configLabel.Name = t.Name + "_" + m.name + "_" + m.configs[i].name;
                    configLabel.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
                    if (m.enabled && m.modChecked)
                        configLabel.Enabled = true;
                    else
                        configLabel.Enabled = false;
                    configLabel.Location = new Point(configLabel.Location.X, getYLocation(configPanel.Controls));
                    configPanel.Controls.Add(configLabel);
                    //make a checkBox
                    CheckBox configControlCB = new CheckBox();
                    configControlCB.AutoSize = true;
                    //configControlCB.Location = new System.Drawing.Point(100, yPosition - 10);
                    configControlCB.Location = new Point(configLabel.Location.X + configLabel.Size.Width + 6, getYLocation(configPanel.Controls));
                    configControlCB.Size = new System.Drawing.Size(150, 15);
                    configControlCB.TabIndex = 1;
                    configControlCB.TabStop = true;
                    configControlCB.Font = Settings.getFont(Settings.fontName, Settings.fontSize);
                    if (m.enabled && m.modChecked && m.configs[i].enabled)
                        configControlCB.Enabled = true;
                    else
                        configControlCB.Enabled = false;
                    if (m.enabled && m.modChecked && m.configs[i].enabled && m.configs[i].configChecked)
                        configControlCB.Checked = true;
                    else
                        configControlCB.Checked = false;
                    configControlCB.CheckedChanged += new EventHandler(configControlCB_CheckedChanged);
                    configControlCB.Name = t.Name + "_" + m.name + "_" + m.configs[i].name;
                    string configDownloadPath2 = Application.StartupPath + "\\RelHaxDownloads\\" + m.configs[i].zipConfigFile;
                    if (File.Exists(configDownloadPath2))
                    {
                        string oldCRC = Settings.GetMd5Hash(configDownloadPath2);
                        if (!oldCRC.Equals(m.configs[i].crc))
                        {
                            configControlCB.Text = configControlCB.Text + "(Updated)";
                            if (m.configs[i].size > 0.0f)
                            {
                                int totalSec = (int)m.configs[i].size * 2;
                                int totalRealMin = totalSec / 60;
                                int totalRealSec = totalSec % 60;
                                configControlCB.Text = configControlCB.Text + " (" + m.configs[i].size + " MB)";
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
                            configControlCB.Text = configControlCB.Text + " (" + m.configs[i].size + " MB)";
                        }
                    }
                    configPanel.Controls.Add(configControlCB);
                    continue;
                }
                else
                {
                    Settings.appendToLog("WANRING: Unknown config type for " + m.configs[i].name + ": " + m.configs[i].type);
                }

                yPosition += 15;
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
            string modDownloadPath = Application.StartupPath + "\\RelHaxDownloads\\" + m.modZipFile;
            if (File.Exists(modDownloadPath))
            {
                string oldCRC = Settings.GetMd5Hash(modDownloadPath);
                if (!oldCRC.Equals(m.crc))
                {
                    modCheckBox.Text = modCheckBox.Text + " (Updated)";
                    if (m.size > 0.0f)
                    {
                        int totalSec = (int)m.size * 2;
                        int totalRealMin = totalSec / 60;
                        int totalRealSec = totalSec % 60;
                        modCheckBox.Text = modCheckBox.Text + " (" + m.size + " MB)";
                    }
                }
            }
            else if (!(File.Exists(modDownloadPath)) && (m.crc != null) && (!m.crc.Equals("")))
            {
                //get the file size as well
                //float downloadSizeMB = this.netFileSize(downloadURL + m.modZipFile);
                //downloadSizeMB = (float)Math.Round(downloadSizeMB, 1);
                //modCheckBox.Text = modCheckBox.Text + " (" + downloadSizeMB + " MB, ~" + (downloadSizeMB * 2) + " sec)";
                modCheckBox.Text = modCheckBox.Text + " (Updated)";
                if (m.size > 0.0f)
                {
                    int totalSec = (int)m.size * 2;
                    int totalRealMin = totalSec / 60;
                    int totalRealSec = totalSec % 60;
                    modCheckBox.Text = modCheckBox.Text + " (" + m.size + " MB)";
                }
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

        void EnableCB_CheckedChanged(object sender, EventArgs e)
        {
            if (loadingConfig)
                return;
            CheckBox cb = (CheckBox)sender;
            string catagoryName = cb.Name.Split('_')[0];
            string modName = cb.Name.Split('_')[1];
            Panel selectionPanel = (Panel)cb.Parent;
            Mod m = this.linkMod(modName, catagoryName);
            if (cb.Enabled && cb.Checked)
            {
                foreach (Control radioControl in selectionPanel.Controls)
                {
                    if (radioControl is RadioButton)
                    {
                        RadioButton radioControlRB = (RadioButton)radioControl;
                        Config cgf = m.getConfig(radioControlRB.Name.Split('_')[2]);
                        radioControlRB.Enabled = cgf.enabled;
                        //radioControlRB.Checked = cgf.configChecked;
                        cgf.configChecked = radioControlRB.Checked;
                        configControlRB_CheckedChanged(radioControlRB, null);
                    }
                    if (radioControl is Label)
                    {
                        radioControl.Enabled = true;
                    }
                }
                selectionPanel.BackColor = Color.BlanchedAlmond;
            }
            else
            {
                foreach (Control radioControl in selectionPanel.Controls)
                {
                    if (radioControl is RadioButton)
                    {
                        RadioButton radioControlRB = (RadioButton)radioControl;
                        Config cgf = m.getConfig(radioControlRB.Name.Split('_')[2]);
                        radioControlRB.Enabled = false;
                        //radioControlRB.Checked = false;
                        configControlRB_CheckedChanged(radioControlRB, null);
                    }
                    if (radioControl is Label)
                    {
                        radioControl.Enabled = false;
                    }
                }
                selectionPanel.BackColor = Settings.getBackColor();
            }

            if (cb.Enabled && cb.Checked)
            {
                //chech to make sure at least one radiobutton is checked
                bool oneIsChecked = false;
                foreach (Control radioControl in selectionPanel.Controls)
                {
                    if (radioControl is RadioButton)
                    {
                        RadioButton rb = (RadioButton)radioControl;
                        if (rb.Checked)
                            oneIsChecked = true;
                    }
                }
                if (!oneIsChecked)
                {
                    //one needs to be checked
                    foreach (Control radioControl in selectionPanel.Controls)
                    {
                        if (radioControl is RadioButton)
                        {
                            RadioButton rb = (RadioButton)radioControl;
                            Config cgf = m.getConfig(rb.Name.Split('_')[2]);
                            if (cgf.enabled)
                            {
                                rb.Checked = true;
                                //means button is enabled
                                //set config true in database and trigger
                                cgf.configChecked = true;
                                configControlRB_CheckedChanged(rb, null);
                                break;
                            }

                        }
                    }
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
                if (c is CheckBox)
                {
                    y += c.Size.Height;
                    //spacing
                    y += 2;
                }
                if (c is ComboBox)
                {
                    y += c.Size.Height;
                    //spacing
                    y += 2;
                }
                if (c is Panel)
                {
                    y += c.Size.Height;
                    //spacing
                    y += 2;
                }
                if (c is RadioButton)
                {
                    y += c.Size.Height;
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
            string[] splitConfigName = configName.Split('_');
            configName = splitConfigName[0];
            string updateString = null;
            Config save = null;
            if (!cb.Enabled)
            {
                //disable the mod and return
                save = m.getConfig(configName);
                save.configChecked = false;
            }
            else
            {
                string dropDownConfigType = m.getConfig(configName).type;

                if (splitConfigName.Count() > 1)
                    updateString = splitConfigName[1];

                foreach (Config c in m.configs)
                {
                    //verify it is on correct type
                    if (c.type.Equals(dropDownConfigType))
                    {
                        //uncheck it
                        c.configChecked = false;
                        //unless it's the one the user selected
                        if (configName.Equals(c.name) && cb.Enabled)
                        {
                            c.configChecked = true;
                            save = c;
                        }
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
                    if (c.Name.Equals(catagory + "_" + mod + "_" + save.type + "_size"))
                    {
                        if (updateString != null)
                        {
                            int totalSec = (int)save.size * 2;
                            int totalRealMin = totalSec / 60;
                            int totalRealSec = totalSec % 60;
                            c.Text = save.size + " MB)";
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
            Config cfg = m.getConfig(configName);
            if (!cb.Enabled)
            {
                //uncheck config in database and leave
                cfg.configChecked = false;
                return;
            }
            else
            {
                //checkbox is enabled, toggle checked and checked
                cfg.configChecked = cb.Checked;
            }

        }
        //handler for when a config radioButton is pressed
        void configControlRB_CheckedChanged(object sender, EventArgs e)
        {
            //uncheck all other radioButton mods of the same type
            RadioButton rb = (RadioButton)sender;
            string modName = rb.Name.Split('_')[1];
            string catagoryName = rb.Name.Split('_')[0];
            string configName = rb.Name.Split('_')[2];
            Panel configSelection = (Panel)rb.Parent;
            Mod m = this.linkMod(modName, catagoryName);
            Config c = m.getConfig(configName);

            if (rb.Enabled && rb.Checked)
            {
                foreach (Control ctrl in configSelection.Controls)
                {
                    if (ctrl is RadioButton)
                    {
                        RadioButton rbz = (RadioButton)ctrl;
                        if (rbz.Enabled)
                        {
                            if (rbz.Name.Equals(rb.Name))
                                continue;
                            Config cfg = m.getConfig(rbz.Name.Split('_')[2]);
                            cfg.configChecked = false;
                            rbz.Checked = false;
                        }
                    }
                }
                rb.CheckedChanged -= configControlRB_CheckedChanged;
                rb.Checked = true;
                c.configChecked = true;
                rb.CheckedChanged += configControlRB_CheckedChanged;
            }
            else
            {
                //uncheck the config in databse and pull out
                c.configChecked = false;
                return;
            }

            foreach (Config cc in m.configs)
            {
                //verify only unchecking radiobutton ("single") configs
                //TODO: change this
                if (cc.type.Equals(configSelection.Name))
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
                        anyModsChecked = true;
                }
                if (anyModsChecked)
                {
                    if (!loadingConfig)
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
                        //remove the handler for a sec to prevent stack overflow
                        cb.CheckedChanged -= modCheckBox_CheckedChanged;
                        cb.Checked = true;
                        cb.CheckedChanged += modCheckBox_CheckedChanged;
                        //cb.Checked = false;
                    }

                }

            }

            //toggle the mod in memory, enabled or disabled
            m.modChecked = cb.Checked;
            //this deals with enabling the componets and triggering the handlers
            //checkbox and dropDownList, including if loading a config, if there are any
            if (modPanel.Controls.Count > 1)
            {
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
                        //toggle enabledness of checkbox based on cb checked
                        //and trigger the checkbox handler
                        if (cb.Checked && cfg.enabled)
                            cc.Enabled = true;
                        else
                            cc.Enabled = false;
                        configControlCB_CheckedChanged(cc, null);
                    }
                    else if (cc is Panel)
                    {
                        //toggle the checkbox and trigger the checked state
                        foreach (Control ccc in cc.Controls)
                        {
                            if (ccc is CheckBox)
                            {
                                ccc.Enabled = cb.Checked;
                                EnableCB_CheckedChanged(ccc, null);
                                break;
                            }
                        }
                    }
                    else if (cc is Label)
                    {
                        //toggle the label
                        cc.Enabled = cb.Checked;
                    }
                    else if (cc is RadioButton)
                    {
                        cfg = m.getConfig(cc.Name.Split('_')[2]);
                        RadioButton ccRB = (RadioButton)cc;
                        if (cb.Checked && cfg.enabled)
                        {
                            ccRB.Enabled = true;
                            ccRB.Checked = cfg.configChecked;
                            configControlRB_CheckedChanged(ccRB, null);
                        }
                        else
                        {
                            ccRB.Enabled = false;
                            configControlRB_CheckedChanged(ccRB, null);
                            continue;
                        }
                        //chech to make sure at least one radiobutton is checked
                        bool oneIsChecked = false;
                        foreach (Control radioControl in configPanel.Controls)
                        {
                            if (radioControl is RadioButton)
                            {
                                RadioButton rb = (RadioButton)radioControl;
                                if (rb.Checked)
                                    oneIsChecked = true;
                            }
                        }
                        if (!oneIsChecked)
                        {
                            //one needs to be checked
                            foreach (Control radioControl in configPanel.Controls)
                            {
                                if (radioControl is RadioButton)
                                {
                                    RadioButton rb = (RadioButton)radioControl;
                                    Config cgf = m.getConfig(rb.Name.Split('_')[2]);
                                    if (cgf.enabled)
                                    {
                                        rb.Checked = true;
                                        //means button is enabled
                                        //set config true in database and trigger
                                        cfg.configChecked = true;
                                        configControlRB_CheckedChanged(rb, null);
                                        break;
                                    }

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
                                        case "startAddress":
                                            m.startAddress = nn.InnerText;
                                            break;
                                        case "endAddress":
                                            m.endAddress = nn.InnerText;
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
                                        case "userDatas":
                                            foreach (XmlNode nnnnnnn in nn.ChildNodes)
                                            {

                                                switch (nnnnnnn.Name)
                                                {
                                                    case "userData":
                                                        string innerText = nnnnnnn.InnerText;
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
                                        case "dependencies":
                                            //parse all dependencies
                                            foreach (XmlNode nnnnnnn in nn.ChildNodes)
                                            {
                                                Dependency d = new Dependency();
                                                foreach (XmlNode nnnnnnnn in nnnnnnn.ChildNodes)
                                                {
                                                    switch (nnnnnnnn.Name)
                                                    {
                                                        case "dependencyZipFile":
                                                            d.dependencyZipFile = nnnnnnnn.InnerText;
                                                            break;
                                                        case "dependencyZipCRC":
                                                            d.dependencyZipCRC = nnnnnnnn.InnerText;
                                                            break;
                                                        case "startAddress":
                                                            d.startAddress = nnnnnnnn.InnerText;
                                                            break;
                                                        case "endAddress":
                                                            d.endAddress = nnnnnnnn.InnerText;
                                                            break;
                                                        case "dependencyenabled":
                                                            try
                                                            {
                                                                d.enabled = bool.Parse(nnnnnnnn.InnerText);
                                                            }
                                                            catch (FormatException)
                                                            {
                                                                d.enabled = false;
                                                            }
                                                            break;
                                                    }
                                                }
                                                m.modDependencies.Add(d);
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
                                                        case "startAddress":
                                                            c.startAddress = nnnn.InnerText;
                                                            break;
                                                        case "endAddress":
                                                            c.endAddress = nnnn.InnerText;
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
                                                        case "dependencies":
                                                            //parse all dependencies
                                                            foreach (XmlNode nnnnnnn in nnnn.ChildNodes)
                                                            {
                                                                Dependency d = new Dependency();
                                                                foreach (XmlNode nnnnnnnn in nnnnnnn.ChildNodes)
                                                                {
                                                                    switch (nnnnnnnn.Name)
                                                                    {
                                                                        case "dependencyZipFile":
                                                                            d.dependencyZipFile = nnnnnnnn.InnerText;
                                                                            break;
                                                                        case "dependencyZipCRC":
                                                                            d.dependencyZipCRC = nnnnnnnn.InnerText;
                                                                            break;
                                                                        case "startAddress":
                                                                            d.startAddress = nnnnnnnn.InnerText;
                                                                            break;
                                                                        case "endAddress":
                                                                            d.endAddress = nnnnnnnn.InnerText;
                                                                            break;
                                                                        case "dependencyenabled":
                                                                            try
                                                                            {
                                                                                d.enabled = bool.Parse(nnnnnnnn.InnerText);
                                                                            }
                                                                            catch (FormatException)
                                                                            {
                                                                                d.enabled = false;
                                                                            }
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
                                        case "startAddress":
                                            d.startAddress = nnnn.InnerText;
                                            break;
                                        case "endAddress":
                                            d.endAddress = nnnn.InnerText;
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
            continueButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - difference);
            cancelButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - difference);
            modTabGroups.Size = new Size(this.Size.Width - 20 - modTabGroups.Location.X, this.Size.Height - modTabGroups.Location.Y - 39 - continueButton.Size.Height - 6 - difference);
            label1.Text = "" + this.Size.Width + " x " + this.Size.Height;
            loadConfigButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width - 6 - loadConfigButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - difference);
            saveConfigButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - difference);
            clearSelectionsButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width - 6 - loadConfigButton.Size.Width - 6 - clearSelectionsButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - difference);
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
                                foreach (Control ccc in pp.Controls)
                                {
                                    if (ccc is Panel)
                                    {
                                        Panel ppp = (Panel)ccc;
                                        ppp.Size = new Size(t.Size.Width - 45, ppp.Size.Height);
                                    }
                                }
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
                this.makeTabs();
                Settings.setUIColor(this);
                this.addAllMods();
                this.addUserMods();
                Settings.setUIColor(this);
                this.UseWaitCursor = false;
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
            this.makeTabs();
            Settings.setUIColor(this);
            this.addAllMods();
            this.addUserMods();
            Settings.setUIColor(this);
            this.UseWaitCursor = false;
        }
    }
}
