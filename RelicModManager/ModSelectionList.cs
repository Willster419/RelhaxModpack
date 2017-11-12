using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Net;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Linq;

namespace RelhaxModpack
{
    //the mod selectin window. allows users to select which mods they wish to install
    public partial class ModSelectionList : RelhaxForum
    {
        public List<Category> ParsedCatagoryList;//can be grabbed by MainWindow
        public List<Mod> UserMods;//can be grabbed by MainWindow
        private Preview p;
        private PleaseWait pw;
        public List<Dependency> GlobalDependencies;
        public List<Dependency> Dependencies;
        public List<LogicalDependency> LogicalDependencies;
        public List<SelectableDatabasePackage> CompleteModSearchList;
        private bool LoadingConfig = false;
        private bool TaskBarHidden = false;
        public string TanksVersion { get; set; }
        public string TanksLocation { get; set; }
        public int MainWindowStartX { get; set; }
        public int MainWindowStartY { get; set; }
        bool FirstLoad = true;
        bool IgnoreSelections = true;
        private string NoDescriptionAvailable = Translations.getTranslatedString("noDescription");
        private string LastUpdated = Translations.getTranslatedString("lastUpdated");
        private string Md5DatabaseFile = "";
        private XDocument ModInfoDocument;
        int Prog = 0;
        private enum LoadConfigMode
        {
            Error = -1,
            FromButton = 0,//this is the default state
            FromSaveLastConfig = 1,//this is the state if the user selected the setting "save last install's selection"
            FromAutoInstall = 2,//this is for when the user started the application in auto install mode. this takes precedence over the above 2
            FromModInfoDat = 3//this is when the manager is reading the modInfo.xml from the downloaded moInfo.dat file
        };
        private LoadConfigMode LoadMode = LoadConfigMode.FromButton;

        public ModSelectionList()
        {
            InitializeComponent();
        }
        //called on application startup
        private void ModSelectionList_Load(object sender, EventArgs e)
        {
            Md5DatabaseFile = Path.Combine(Application.StartupPath, "RelHaxDownloads", "MD5HashDatabase.xml");
            //create the loading window
            pw = new PleaseWait(MainWindowStartX, MainWindowStartY);
            pw.Show();
            Prog = 0;
            //apply the translations
            ApplyTranslations();
            pw.loadingDescBox.Text = Translations.getTranslatedString("readingDatabase");
            Application.DoEvents();
            //init the database
            string databaseLocation = InitDatabase(Settings.ModInfoDatFile);
            //create new lists for memory database and serialize from xml->lists
            GlobalDependencies = new List<Dependency>();
            ParsedCatagoryList = new List<Category>();
            Dependencies = new List<Dependency>();
            LogicalDependencies = new List<LogicalDependency>();
            //actually parse the mod structure
            XMLUtils.CreateModStructure(databaseLocation, GlobalDependencies, Dependencies, LogicalDependencies, ParsedCatagoryList);
            if (Program.testMode)
            {
                if (Utils.Duplicates(ParsedCatagoryList))
                {
                    Logging.Manager("WARNING: Duplicate mod name detected!!");
                    MessageBox.Show(Translations.getTranslatedString("duplicateMods"), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Application.Exit();
                }
                int duplicatesCounter = 0;
                if (Utils.DuplicatesPackageName(ParsedCatagoryList, ref duplicatesCounter))
                {
                    Logging.Manager(string.Format("ERROR: {0} duplicate PackageName's detected", duplicatesCounter));
                    MessageBox.Show(string.Format("ERROR: {0} duplicate PackageName's detected!\n\nmore information, see Logfile ...", duplicatesCounter), "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    Application.Exit();
                }
            }
            pw.loadingDescBox.Text = Translations.getTranslatedString("buildingUI");
            Application.DoEvents();
            //check if databse exists and if not, create it
            if (!File.Exists(Md5DatabaseFile))
            {
                ModInfoDocument = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("database"));
            }
            else
            {
                ModInfoDocument = XDocument.Load(Md5DatabaseFile);
            }
            InitUserMods();
            InitDependencies();
            //build the UI display
            pw.progres_max = XMLUtils.TotalModConfigComponents;
            pw.SetProgress(0);
            //setting loadingConfig to true will disable all UI interaction
            LoadingConfig = true;
            Logging.Manager("Loading ModSelectionList with view " + Settings.SView);
            CompleteModSearchList = new List<SelectableDatabasePackage>();
            if (modTabGroups.TabPages.Count > 0)
                modTabGroups.TabPages.Clear();
            modTabGroups.Font = Settings.AppFont;
            AddAllMods();
            AddUserMods(false);
            FinishLoad();
        }
        #region Loading Methods

        private void ApplyTranslations()
        {
            continueButton.Text = Translations.getTranslatedString(continueButton.Name);
            cancelButton.Text = Translations.getTranslatedString(cancelButton.Name);
            helpLabel.Text = Translations.getTranslatedString(helpLabel.Name);
            loadConfigButton.Text = Translations.getTranslatedString(loadConfigButton.Name);
            saveConfigButton.Text = Translations.getTranslatedString(saveConfigButton.Name);
            label2.Text = Translations.getTranslatedString(label2.Name);
            clearSelectionsButton.Text = Translations.getTranslatedString(clearSelectionsButton.Name);
            colapseAllButton.Text = Translations.getTranslatedString(colapseAllButton.Name);
            expandAllButton.Text = Translations.getTranslatedString(expandAllButton.Name);
        }

        private string InitDatabase(string databaseURL)
        {
            if (Program.testMode)
            {
                // if customModInfoPath is empty, this creates a full valid path to the current manager location folder
                databaseURL = Path.Combine(string.IsNullOrEmpty(Settings.CustomModInfoPath) ? Application.StartupPath : Settings.CustomModInfoPath, "modInfo.xml");
                if (!File.Exists(databaseURL))
                {
                    Logging.Manager("Databasefile not found: " + databaseURL);
                    MessageBox.Show(string.Format(Translations.getTranslatedString("testModeDatabaseNotFound"), databaseURL));
                    Application.Exit();
                }
                Settings.TanksOnlineFolderVersion = XMLUtils.ReadOnlineFolderFromModInfo(databaseURL);
            }
            else if (Program.betaDatabase)
            {
                Logging.Manager("downloading modInfo.dat (betaDatabase url)");
                string xmlString = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "manager_version.xml");
                XDocument doc = XDocument.Parse(xmlString);
                //parse the database version
                databaseURL = doc.XPathSelectElement("//version/database_beta_url").Value;
                string localDest = Path.Combine(Application.StartupPath, "RelHaxTemp", "modInfo_beta.xml");
                //always delete the file before redownloading
                if (File.Exists(localDest))
                    File.Delete(localDest);
                using (WebClient downloader = new WebClient() { Proxy = null })
                {
                    try
                    {
                        downloader.DownloadFile(databaseURL, localDest);
                        databaseURL = localDest;
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog("ModSelectionList_Load", string.Format(@"Tried to access {0}", databaseURL), ex);
                        MessageBox.Show(string.Format("{0} modInfo.dat", Translations.getTranslatedString("failedToDownload_1")));
                        Application.Exit();
                    }
                }
            }
            else
            {
                //download the modInfo.dat
                Logging.Manager("downloading modInfo.dat");
                string dlURL = "";
                using (WebClient downloader = new WebClient() { Proxy = null })
                {
                    try
                    {
                        dlURL = string.Format("http://wotmods.relhaxmodpack.com/WoT/{0}/modInfo.dat", Settings.TanksOnlineFolderVersion);
                        downloader.DownloadFile(dlURL, Settings.ModInfoDatFile);
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog(string.Format("ModSelectionList_Load", @"Tried to access {0}", dlURL), ex);
                        MessageBox.Show(string.Format("{0} modInfo.dat", Translations.getTranslatedString("failedToDownload_1")));
                        Application.Exit();
                    }
                }
            }
            return databaseURL;
        }
        //initializes the userMods list. This should only be run once
        private void InitUserMods()
        {
            //create all the user mod objects
            string modsPath = Path.Combine(Application.StartupPath, "RelHaxUserMods");
            string[] userModFiles = Directory.GetFiles(modsPath);
            UserMods = new List<Mod>();
            foreach (string s in userModFiles)
            {
                if (Path.GetExtension(s).Equals(".zip"))
                {
                    Mod m = new Mod();
                    m.ZipFile = s;
                    m.Name = Path.GetFileNameWithoutExtension(s);
                    m.Enabled = true;
                    UserMods.Add(m);
                }
            }
        }
        //initialize the globalDependency, dependency, and logicalDependency list. should only be done once
        private void InitDependencies()
        {
            string modDownloadFilePath = "";
            //init the global dependencies
            foreach (Dependency d in GlobalDependencies)
            {
                //default to false, then check if it can be true
                d.DownloadFlag = false;
                modDownloadFilePath = Path.Combine(Application.StartupPath, "RelHaxDownloads", d.ZipFile);
                //get the local md5 hash. a -1 indicates the file is not on the disk
                string oldCRC2 = GetMD5Hash(modDownloadFilePath);
                if ((!d.ZipFile.Equals("")) && (!d.CRC.Equals(oldCRC2)))
                {
                    d.DownloadFlag = true;
                }
            }
            //init the dependencies
            foreach (Dependency d in Dependencies)
            {
                //default to false, then check if it can be true
                d.DownloadFlag = false;
                modDownloadFilePath = Path.Combine(Application.StartupPath, "RelHaxDownloads", d.ZipFile);
                //get the local md5 hash. a -1 indicates the file is not on the disk
                string oldCRC2 = GetMD5Hash(modDownloadFilePath);
                if ((!d.ZipFile.Equals("")) && (!d.CRC.Equals(oldCRC2)))
                {
                    d.DownloadFlag = true;
                }
            }
            //init the logicalDependencies
            foreach (LogicalDependency d in LogicalDependencies)
            {
                //default to false, then check if it can be true
                d.DownloadFlag = false;
                modDownloadFilePath = Path.Combine(Application.StartupPath, "RelHaxDownloads", d.ZipFile);
                //get the local md5 hash. a -1 indicates the file is not on the disk
                string oldCRC2 = GetMD5Hash(modDownloadFilePath);
                if ((!d.ZipFile.Equals("")) && (!d.CRC.Equals(oldCRC2)))
                {
                    d.DownloadFlag = true;
                }
            }
        }
        //adds all the tab pages for each catagory
        //must be only one catagory
        private void AddAllMods()
        {
            foreach (Category c in ParsedCatagoryList)
            {
                TabPage t = new TabPage(c.Name)
                {
                    AutoScroll = true,
                    //link the names of catagory and tab so eithor can be searched for
                    Name = c.Name,
                    Text = c.SelectionType.Equals("single") ? c.Name + "*" : c.Name
                };
                c.TabPage = t;
                //matched the catagory to tab
                //add to the ui every mod of that catagory
                Utils.SortModsList(c.Mods);

                LegacySelectionList lsl = null;
                if (Settings.SView == Settings.SelectionView.Legacy)
                {
                    //create the WPF host for this tabPage
                    lsl = new LegacySelectionList();
                    ElementHost host = new ElementHost()
                    {
                        Location = new Point(5, 5),
                        Size = new Size(t.Size.Width - 5 - 5, t.Size.Height - 5 - 5),
                        BackColorTransparent = false,
                        BackColor = Color.White,
                        Child = lsl
                    };
                    //apparently there is an item in there. clear it.
                    lsl.legacyTreeView.Items.Clear();
                    t.Controls.Add(host);
                }
                modTabGroups.TabPages.Add(t);
            }
            foreach (Category c in ParsedCatagoryList)
            {
                foreach (Mod m in c.Mods)
                {

                    if (pw != null)
                    {
                        pw.loadingDescBox.Text = string.Format("{0} {1}", Translations.getTranslatedString("loading"), m.Name);
                        Prog++;
                        pw.SetProgress(Prog);
                        Application.DoEvents();
                    }

                    if (Settings.SView == Settings.SelectionView.Default)
                    {
                        //use default UI
                        AddModDefaultView(m, c.TabPage, c);
                    }
                    else if (Settings.SView == Settings.SelectionView.Legacy)
                    {
                        //use legacy OMC UI
                        ElementHost h = (ElementHost)c.TabPage.Controls[0];
                        AddModOMCView(m, c.TabPage, (LegacySelectionList)h.Child, c);
                    }
                    else
                    {
                        //default case, use default view
                        AddModDefaultView(m, c.TabPage, c);
                    }
                }
            }
        }
        //adds all usermods to thier own userMods tab
        private void AddUserMods(bool forceUnchecked)
        {
            //make the new tab
            TabPage tb = new TabPage("User Mods");
            tb.AutoScroll = true;
            //add all mods to the tab page
            for (int i = 0; i < UserMods.Count; i++)
            {
                //make modCheckBox
                ModFormCheckBox modCheckBox = new ModFormCheckBox();
                UserMods[i].ModFormCheckBox = modCheckBox;
                modCheckBox.mod = UserMods[i];
                modCheckBox.Font = Settings.AppFont;
                modCheckBox.AutoSize = true;
                int yLocation = 3 + (modCheckBox.Size.Height * (i));
                modCheckBox.Location = new System.Drawing.Point(3, yLocation);
                modCheckBox.TabIndex = 1;
                modCheckBox.Text = UserMods[i].Name;
                if (forceUnchecked)
                {
                    modCheckBox.Checked = false;
                }
                else
                {
                    modCheckBox.Checked = UserMods[i].Checked;
                }
                modCheckBox.UseVisualStyleBackColor = true;
                modCheckBox.Enabled = true;
                modCheckBox.CheckedChanged += new EventHandler(modCheckBox_CheckedChanged);
                tb.Controls.Add(modCheckBox);
            }
            modTabGroups.TabPages.Add(tb);
        }

        private void FinishLoad()
        {
            LoadingConfig = false;
            FirstLoad = false;
            //set the size to the last closed size
            Size = new Size(Settings.ModSelectionWidth, Settings.ModSelectionHeight);
            Settings.setUIColor(this);
            pw.Close();
            pw.Dispose();
            //set label properties
            TanksVersionLabel.Text = TanksVersionLabel.Text + TanksVersion;
            TanksPath.Text = string.Format(Translations.getTranslatedString("InstallingTo"), TanksLocation);
            //if the task bar was set to auto hide, set it to always on top
            //it will be set back to auto hide when this window closes
            Settings.AppBarStates currentState = Settings.GetTaskbarState();
            if (currentState == Settings.AppBarStates.AutoHide)
            {
                TaskBarHidden = true;
                Settings.SetTaskbarState(Settings.AppBarStates.AlwaysOnTop);
            }
            //force a resize
            ModSelectionList_SizeChanged(null, null);
            if (Settings.ModSelectionFullscreen)
            {
                this.WindowState = FormWindowState.Maximized;
            }
            if (Settings.SView == Settings.SelectionView.Default)
            {
                colapseAllButton.Enabled = false;
                colapseAllButton.Visible = false;
                expandAllButton.Enabled = false;
                expandAllButton.Visible = false;
            }
            else
            {
                colapseAllButton.Enabled = true;
                colapseAllButton.Visible = true;
                expandAllButton.Enabled = true;
                expandAllButton.Visible = true;
            }
            MainWindow.usedFilesList = Utils.CreateUsedFilesList(ParsedCatagoryList, GlobalDependencies, Dependencies, LogicalDependencies);
            //save the database file
            ModInfoDocument.Save(Md5DatabaseFile);
            //parse the load information (set the loadMode)
            LoadMode = LoadConfigMode.FromButton;
            if (Settings.SaveLastConfig)
            {
                LoadMode = LoadConfigMode.FromSaveLastConfig;
            }
            //if this is in auto install mode, then it takes precedence
            if (Program.autoInstall)
            {
                LoadMode = LoadConfigMode.FromAutoInstall;
            }
            if (LoadMode == LoadConfigMode.FromAutoInstall || LoadMode == LoadConfigMode.FromSaveLastConfig)
            {
                this.parseLoadConfig();
                if (LoadMode == LoadConfigMode.FromAutoInstall)
                {
                    if(pw != null)
                    {
                        pw.Close();
                        pw.Dispose();
                    }
                    DialogResult = DialogResult.OK;
                    return;
                }
            }
        }
#endregion
        //adds a mod m to a tabpage t, OMC treeview style
        private void AddModOMCView(Mod m, TabPage t, LegacySelectionList lsl, Category c)
        {
            if (!m.Visible)
                return;
            if (Settings.DarkUI)
                lsl.legacyTreeView.Background = System.Windows.Media.Brushes.Gray;
            //helpfull stuff
            string modDownloadFilePath = Path.Combine(Application.StartupPath, "RelHaxDownloads", m.ZipFile);
            //link the catagory and mod in memory
            m.ParentCategory = c;
            //if there are underscores you need to actually display them
            string nameForModCB = Utils.ReplaceMacro(m);
            nameForModCB = nameForModCB.Replace(@"_", @"__");
            //create base mod checkbox
            //http://wpftutorial.net/ToolTip.html
            string dateFormat = m.Timestamp == 0 ? "": Utils.ConvertFiletimeTimestampToDate(m.Timestamp);
            string tooltipString = m.Description.Equals("") ? NoDescriptionAvailable : m.Description + (m.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
            ModWPFCheckBox modCheckBox = new ModWPFCheckBox()
            {
                ToolTip = tooltipString,
                mod = m,
                catagory = c,
                FontFamily = new System.Windows.Media.FontFamily(Settings.FontName),
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                FontWeight = Settings.DarkUI ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal,
                Content = nameForModCB,
                IsEnabled = m.Enabled,
                IsChecked = (m.Enabled && m.Checked) ? true : false
            };
            //add the root UI object to the memory database
            m.ModFormCheckBox = modCheckBox;
            m.TabIndex = t;
            CompleteModSearchList.Add(m);
            switch (Settings.FontSizeforum)
            {
                case Settings.FontSize.Font100:
                    break;
                case Settings.FontSize.Font125:
                    modCheckBox.FontSize = modCheckBox.FontSize + 4;
                    break;
                case Settings.FontSize.Font175:
                    modCheckBox.FontSize = modCheckBox.FontSize + 8;
                    break;
                case Settings.FontSize.Font225:
                    modCheckBox.FontSize = modCheckBox.FontSize + 12;
                    break;
                case Settings.FontSize.Font275:
                    modCheckBox.FontSize = modCheckBox.FontSize + 16;
                    break;
            }
            //make the tree view item for the modCheckBox
            System.Windows.Controls.TreeViewItem tvi = new System.Windows.Controls.TreeViewItem();
            if (Settings.ExpandAllLegacy)
                tvi.IsExpanded = true;
            //process configs
            if (m.configs.Count > 0)
                AddConfigsOMCView(c, m, m.configs, tvi, true);
            //if the CRC's don't match and the mod actually has a zip file
            if (FirstLoad)
            {
                //get the local md5 hash. a -1 indicates the file is not on the disk
                string oldCRC2 = GetMD5Hash(modDownloadFilePath);
                if ((!m.ZipFile.Equals("")) && (!m.CRC.Equals(oldCRC2)))
                {
                    modCheckBox.Content = string.Format("{0} ({1})", modCheckBox.Content, Translations.getTranslatedString("updated"));
                    m.DownloadFlag = true;
                    if ((m.Size > 0))
                        modCheckBox.Content = string.Format("{0} ({1})", modCheckBox.Content, Utils.SizeSuffix(m.Size, 1, true));
                }
            }
            else
            {
                if (m.DownloadFlag)
                {
                    modCheckBox.Content = string.Format("{0} ({1})", modCheckBox.Content, Translations.getTranslatedString("updated"));
                    if ((m.Size > 0))
                        modCheckBox.Content = string.Format("{0} ({1})", modCheckBox.Content, Utils.SizeSuffix(m.Size, 1, true));
                }
            }
            tvi.Header = modCheckBox;
            //add it's handlers, right click and when checked
            modCheckBox.MouseDown += Generic_MouseDown;
            //add the mod check box to the legacy tree view
            lsl.legacyTreeView.Items.Add(tvi);
            modCheckBox.Checked += modCheckBoxL_Click;
            modCheckBox.Unchecked += modCheckBoxL_Click;
        }

        void AddConfigsOMCView(Category c, Mod m, List<Config> configs, System.Windows.Controls.TreeViewItem tvi, bool parentIsMod = false, Config parentConfig = null)
        {
            //create the twp possible drop down options, and the mod optional config check box i guess
            ConfigWPFComboBox configControlDD = null;
            ConfigWPFComboBox configControlDD2 = null;
            //process the configs
            foreach (Config con in configs)
            {
                if (!con.Visible)
                    continue;
                //link stuff in memory
                con.ParentMod = m;
                if (parentIsMod)
                {
                    CompleteModSearchList.Add(con);
                    con.Parent = m;
                }
                else
                    con.Parent = parentConfig;
                //create the init stuff for each config
                ConfigWPFComboBox configControlDDALL = null;
                if (con.Type.Equals("single") || con.Type.Equals("single1"))
                {
                    string nameForModCB = Utils.ReplaceMacro(con);
                    //if there are underscores you need to actually display them
                    nameForModCB = nameForModCB.Replace(@"_", @"__");
                    //make the radio button
                    string dateFormat = con.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(con.Timestamp);
                    string tooltipString = con.Description.Equals("") ? NoDescriptionAvailable : con.Description + (con.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
                    ConfigWPFRadioButton configControlRB = new ConfigWPFRadioButton()
                    {
                        ToolTip = tooltipString,
                        FontFamily = new System.Windows.Media.FontFamily(Settings.FontName),
                        FontWeight = Settings.DarkUI ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal,
                        HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                        VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                        catagory = c,
                        mod = m,
                        config = con,
                        IsEnabled = false,
                        IsChecked = false,
                        Content = nameForModCB
                    };
                    switch (Settings.FontSizeforum)
                    {
                        case Settings.FontSize.Font100:
                            break;
                        case Settings.FontSize.Font125:
                            configControlRB.FontSize = configControlRB.FontSize + 4;
                            break;
                        case Settings.FontSize.Font175:
                            configControlRB.FontSize = configControlRB.FontSize + 8;
                            break;
                        case Settings.FontSize.Font225:
                            configControlRB.FontSize = configControlRB.FontSize + 12;
                            break;
                        case Settings.FontSize.Font275:
                            configControlRB.FontSize = configControlRB.FontSize + 16;
                            break;
                    }
                    //add the UI component to the config item in memory database
                    con.ConfigUIComponent = configControlRB;
                    //get all levels up to the mod, then deal with the mod
                    bool canBeEnabled = true;
                    //check all parent configs, if any
                    if (con.Parent is Config)
                    {
                        Config parentConfig2 = (Config)con.Parent;
                        while (parentConfig2 is Config)
                        {
                            if (!parentConfig2.Enabled)
                                canBeEnabled = false;
                            if (parentConfig2.Parent is Mod)
                                break;
                            parentConfig2 = (Config)parentConfig2.Parent;
                        }
                    }
                    //check the parent mod
                    if (!con.ParentMod.Enabled)
                        canBeEnabled = false;
                    //check itself (before it reks itself)
                    if (!con.Enabled)
                        canBeEnabled = false;
                    if (canBeEnabled)
                        configControlRB.IsEnabled = true;
                    if (configControlRB.IsEnabled)
                        if (con.Checked)
                            configControlRB.IsChecked = true;
                    //run the checksum logix
                    if (FirstLoad)
                    {
                        string oldCRC = GetMD5Hash(Path.Combine(Application.StartupPath, "RelHaxDownloads", con.ZipFile));
                        if ((!con.CRC.Equals("")) && (!oldCRC.Equals(con.CRC)))
                        {
                            configControlRB.Content = string.Format("{0} ({1})", configControlRB.Content, Translations.getTranslatedString("updated"));
                            con.DownloadFlag = true;
                            if (con.Size > 0)
                                configControlRB.Content = string.Format("{0} ({1})", configControlRB.Content, Utils.SizeSuffix(con.Size, 1, true));
                        }
                    }
                    else
                    {
                        if (con.DownloadFlag)
                        {
                            configControlRB.Content = string.Format("{0} ({1})", configControlRB.Content, Translations.getTranslatedString("updated"));
                            if (con.Size > 0)
                                configControlRB.Content = string.Format("{0} ({1})", configControlRB.Content, Utils.SizeSuffix(con.Size, 1, true));
                        }
                    }
                    //add the handlers at the end
                    configControlRB.Checked += configControlRB_Click;
                    configControlRB.Unchecked += configControlRB_Click;
                    configControlRB.MouseDown += Generic_MouseDown;
                    //add it to the mod config list
                    System.Windows.Controls.TreeViewItem configControlTVI = new System.Windows.Controls.TreeViewItem();
                    configControlTVI.IsExpanded = Settings.ExpandAllLegacy ? true : false;
                    configControlTVI.Header = configControlRB;
                    tvi.Items.Add(configControlTVI);
                    //process the subconfigs
                    if (con.configs.Count > 0)
                        AddConfigsOMCView(c, m, con.configs, configControlTVI, false, con);
                }
                else if (con.Type.Equals("single_dropdown") || con.Type.Equals("single_dropdown1") || con.Type.Equals("single_dropdown2"))
                {
                    //set the all to whichever one it actually is
                    if (con.Type.Equals("single_dropdown") || con.Type.Equals("single_dropdown1"))
                    {
                        if(configControlDD == null)
                        {
                            configControlDD = new ConfigWPFComboBox()
                            {
                                IsEditable = false,
                                Name = "notAddedYet",
                                IsEnabled = false,
                                FontFamily = new System.Windows.Media.FontFamily(Settings.FontName),
                                FontWeight = Settings.DarkUI ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal
                            };
                            switch (Settings.FontSizeforum)
                            {
                                case Settings.FontSize.Font100:
                                    break;
                                case Settings.FontSize.Font125:
                                    configControlDD.FontSize = configControlDD.FontSize + 4;
                                    break;
                                case Settings.FontSize.Font175:
                                    configControlDD.FontSize = configControlDD.FontSize + 8;
                                    break;
                                case Settings.FontSize.Font225:
                                    configControlDD.FontSize = configControlDD.FontSize + 12;
                                    break;
                                case Settings.FontSize.Font275:
                                    configControlDD.FontSize = configControlDD.FontSize + 16;
                                    break;
                            }
                        }
                        configControlDDALL = configControlDD;
                        //add the UI component to the config item in memory database
                        con.ConfigUIComponent = configControlDD;
                    }
                    else if (con.Type.Equals("single_dropdown2"))
                    {
                        if (configControlDD2 == null)
                        {
                            configControlDD2 = new ConfigWPFComboBox()
                            {
                                IsEditable = false,
                                Name = "notAddedYet",
                                IsEnabled = false,
                                FontFamily = new System.Windows.Media.FontFamily(Settings.FontName),
                                FontWeight = Settings.DarkUI ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal
                            };
                            switch (Settings.FontSizeforum)
                            {
                                case Settings.FontSize.Font100:
                                    break;
                                case Settings.FontSize.Font125:
                                    configControlDD2.FontSize = configControlDD2.FontSize + 4;
                                    break;
                                case Settings.FontSize.Font175:
                                    configControlDD2.FontSize = configControlDD2.FontSize + 8;
                                    break;
                                case Settings.FontSize.Font225:
                                    configControlDD2.FontSize = configControlDD2.FontSize + 12;
                                    break;
                                case Settings.FontSize.Font275:
                                    configControlDD2.FontSize = configControlDD2.FontSize + 16;
                                    break;
                            }
                        }
                        configControlDDALL = configControlDD2;
                        //add the UI component to the config item in memory database
                        con.ConfigUIComponent = configControlDD2;
                    }
                    //make the dropdown selection list
                    configControlDDALL.MinWidth = 100;
                    ComboBoxItem cbi = null;
                    string toAdd = Utils.ReplaceMacro(con);
                    //run the CRC logics
                    if (FirstLoad)
                    {
                        string oldCRC = GetMD5Hash(Path.Combine(Application.StartupPath, "RelHaxDownloads", con.ZipFile));
                        if ((!con.CRC.Equals("")) && (!oldCRC.Equals(con.CRC)))
                        {
                            toAdd = string.Format("{0} ({1})", toAdd, Translations.getTranslatedString("updated"));
                            con.DownloadFlag = true;
                            if (con.Size > 0)
                                toAdd = string.Format("{0} ({1})", toAdd, Utils.SizeSuffix(con.Size, 1, true));
                        }
                    }
                    else
                    {
                        if (con.DownloadFlag)
                        {
                            toAdd = string.Format("{0} ({1})", toAdd, Translations.getTranslatedString("updated"));
                            con.DownloadFlag = true;
                            if (con.Size > 0)
                                toAdd = string.Format("{0} ({1})", toAdd, Utils.SizeSuffix(con.Size, 1, true));
                        }
                    }
                    //add it
                    if (con.Enabled)
                    {
                        cbi = new ComboBoxItem(con, toAdd);
                        configControlDDALL.Items.Add(cbi);
                        if (con.Checked)
                        {
                            configControlDDALL.SelectedItem = cbi;
                            string dateFormat = con.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(con.Timestamp);
                            string tooltipString = con.Description.Equals("") ? NoDescriptionAvailable : con.Description + (con.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
                            configControlDDALL.ToolTip = tooltipString;
                        }
                    }
                    //add the dropdown to the thing. it will only run this once
                    if (configControlDDALL.Name.Equals("notAddedYet"))
                    {
                        configControlDDALL.Name = "added";
                        configControlDDALL.catagory = c;
                        configControlDDALL.mod = m;
                        configControlDDALL.SelectionChanged += configControlDDALL_SelectionChanged;
                        configControlDDALL.PreviewMouseRightButtonDown += Generic_MouseDown;
                        if (configControlDDALL.Items.Count > 0)
                            configControlDDALL.IsEnabled = true;
                        if (configControlDDALL.SelectedIndex == -1)
                            configControlDDALL.SelectedIndex = 0;
                        System.Windows.Controls.TreeViewItem configControlTVI = new System.Windows.Controls.TreeViewItem();
                        string dateFormat = con.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(con.Timestamp);
                        string tooltipString = con.Description.Equals("") ? NoDescriptionAvailable : con.Description + (con.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
                        configControlDDALL.ToolTip = tooltipString;
                        configControlTVI.Header = configControlDDALL;
                        tvi.Items.Add(configControlTVI);
                    }
                }
                else if (con.Type.Equals("multi"))
                {
                    string nameForModCB = Utils.ReplaceMacro(con);
                    //if there are underscores you need to actually display them
                    nameForModCB = nameForModCB.Replace(@"_", @"__");
                    //make the checkbox and add the tooltip
                    string dateFormat = con.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(con.Timestamp);
                    string tooltipString = con.Description.Equals("") ? NoDescriptionAvailable : con.Description + (con.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
                    ConfigWPFCheckBox configControlCB = new ConfigWPFCheckBox()
                    {
                        ToolTip = tooltipString,
                        FontFamily = new System.Windows.Media.FontFamily(Settings.FontName),
                        FontWeight = Settings.DarkUI ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal,
                        HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                        VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                        catagory = c,
                        mod = m,
                        config = con,
                        IsEnabled = false,
                        IsChecked = false,
                        Content = nameForModCB
                    };
                    switch (Settings.FontSizeforum)
                    {
                        case Settings.FontSize.Font100:
                            break;
                        case Settings.FontSize.Font125:
                            configControlCB.FontSize = configControlCB.FontSize + 4;
                            break;
                        case Settings.FontSize.Font175:
                            configControlCB.FontSize = configControlCB.FontSize + 8;
                            break;
                        case Settings.FontSize.Font225:
                            configControlCB.FontSize = configControlCB.FontSize + 12;
                            break;
                        case Settings.FontSize.Font275:
                            configControlCB.FontSize = configControlCB.FontSize + 16;
                            break;
                    }
                    //add the UI component to the config item in memory database
                    con.ConfigUIComponent = configControlCB;
                    //logic for determining if it can be Enabled
                    //get all levels up to the mod, then deal with the mod
                    bool canBeEnabled = true;
                    //check all parent configs, if any
                    if (con.Parent is Config parentConfig2)
                    {
                        while (parentConfig2 is Config)
                        {
                            if (!parentConfig2.Enabled)
                                canBeEnabled = false;
                            if (parentConfig2.Parent is Mod)
                                break;
                            parentConfig2 = (Config)parentConfig2.Parent;
                        }
                    }
                    //check the parent mod
                    if (!con.ParentMod.Enabled)
                        canBeEnabled = false;
                    //check itself (before it reks itself)
                    if (!con.Enabled)
                        canBeEnabled = false;
                    if (canBeEnabled)
                        configControlCB.IsEnabled = true;
                    if (configControlCB.IsEnabled)
                        if (con.Checked)
                            configControlCB.IsChecked = true;
                    //run the checksum logix
                    if (FirstLoad)
                    {
                        string oldCRC = XMLUtils.GetMd5Hash(Path.Combine(Application.StartupPath, "RelHaxDownloads", con.ZipFile));
                        if ((!con.CRC.Equals("")) && (!oldCRC.Equals(con.CRC)))
                        {
                            configControlCB.Content = string.Format("{0} ({1})", configControlCB.Content, Translations.getTranslatedString("updated"));
                            con.DownloadFlag = true;
                            if (con.Size > 0)
                                configControlCB.Content = string.Format("{0} ({1})", configControlCB.Content, Utils.SizeSuffix(con.Size, 1, true));
                        }
                    }
                    else
                    {
                        if (con.DownloadFlag)
                        {
                            configControlCB.Content = string.Format("{0} ({1})", configControlCB.Content, Translations.getTranslatedString("updated"));
                            con.DownloadFlag = true;
                            if (con.Size > 0)
                                configControlCB.Content = string.Format("{0} ({1})", configControlCB.Content, Utils.SizeSuffix(con.Size, 1, true));
                        }
                    }
                    //add the handlers at the end
                    configControlCB.Checked += configControlCB_Click;
                    configControlCB.Unchecked += configControlCB_Click;
                    configControlCB.MouseDown += Generic_MouseDown;
                    //add it to the mod config list
                    System.Windows.Controls.TreeViewItem configControlTVI = new System.Windows.Controls.TreeViewItem();
                    configControlTVI.IsExpanded = Settings.ExpandAllLegacy ? true : false;
                    configControlTVI.Header = configControlCB;
                    tvi.Items.Add(configControlTVI);
                    //process the subconfigs
                    if (con.configs.Count > 0)
                        AddConfigsOMCView(c, m, con.configs, configControlTVI, false, con);
                }
                else
                {
                    Logging.Manager(string.Format("WARNING: Unknown config type for {0}: {1}", con.Name, con.Type));
                }

            }
        }
        //when a legacy mod checkbox is clicked
        void modCheckBoxL_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LoadingConfig)
                return;
            ModWPFCheckBox cb = (ModWPFCheckBox)sender;
            Mod m = cb.mod;
            Category cat = m.ParentCategory;
            System.Windows.Controls.TreeViewItem TVI = (System.Windows.Controls.TreeViewItem)cb.Parent;
            System.Windows.Controls.TreeView TV = (System.Windows.Controls.TreeView)TVI.Parent;
            //check to see if this is a single selection categtory
            //if it is, then uncheck the other mod, then check this one
            if ((bool)cb.IsChecked && cat.SelectionType.Equals("single"))
            {
                //check if any other mods in this catagory are already checked
                bool anyModsChecked = false;
                foreach (Mod mm in cat.Mods)
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
                        if ((bool)modCB.IsChecked)
                        {
                            modCB.IsChecked = false;
                            modCB.mod.Checked = false;
                        }
                    }
                }
                //now it is safe to check the mod we want
                cb.Checked -= modCheckBoxL_Click;
                cb.Unchecked -= modCheckBoxL_Click;
                cb.IsChecked = true;
                cb.mod.Checked = true;
                cb.Checked += modCheckBoxL_Click;
                cb.Unchecked += modCheckBoxL_Click;
            }
            //check the mod in memory database
            m.Checked = (bool)cb.IsChecked;
            //this section deals with enabling the configs, if there are any
            if (m.configs.Count == 0)
                return;
            if (m.Checked)
            {
                //mod checked, check at least one single1, one single_dropdown1, one single_dropdown2
                //checking for single/single1 configs
                bool configSelected = false;
                foreach (Config con in m.configs)
                {
                    if ((!con.Visible) || (!con.Enabled))
                        continue;
                    if ((con.Type.Equals("single")) || (con.Type.Equals("single1")))
                    {
                        if (con.Checked)
                            configSelected = true;
                    }
                }
                if (!configSelected)
                {
                    foreach (Config con in m.configs)
                    {
                        if ((!con.Visible) || (!con.Enabled))
                            continue;
                        if ((con.Type.Equals("single")) || (con.Type.Equals("single1")))
                        {
                            con.Checked = true;
                            ConfigWPFRadioButton cwpfrb = (ConfigWPFRadioButton)con.ConfigUIComponent;
                            cwpfrb.IsChecked = true;
                            break;
                        }
                    }
                }
                //checking for single_dropdown/single_dropdown1 configs
                configSelected = false;
                foreach (Config con in m.configs)
                {
                    if ((!con.Visible) || (!con.Enabled))
                        continue;
                    if ((con.Type.Equals("single_dropdown")) || (con.Type.Equals("single_dropdown1")))
                    {
                        if (con.Checked)
                            configSelected = true;
                    }
                }
                if (!configSelected)
                {
                    foreach (Config con in m.configs)
                    {
                        if ((!con.Visible) || (!con.Enabled))
                            continue;
                        if ((con.Type.Equals("single_dropdown")) || (con.Type.Equals("single_dropdown1")))
                        {
                            con.Checked = true;
                            ConfigWPFComboBox cwpfcb = (ConfigWPFComboBox)con.ConfigUIComponent;
                            bool breakOut = false;
                            foreach (ComboBoxItem cbi in cwpfcb.Items)
                            {
                                if (cbi.config.Name.Equals(con.Name))
                                {
                                    cwpfcb.SelectionChanged -= configControlDDALL_SelectionChanged;
                                    cwpfcb.SelectedItem = cbi;
                                    cwpfcb.SelectionChanged += configControlDDALL_SelectionChanged;
                                    breakOut = true;
                                    break;
                                }
                            }
                            if (breakOut)
                                break;
                        }
                    }
                }
                //checking for single_dropdown2 configs
                configSelected = false;
                foreach (Config con in m.configs)
                {
                    if ((!con.Visible) || (!con.Enabled) || (!con.Type.Equals("single_dropdown2")))
                        continue;
                    if (con.Checked)
                        configSelected = true;
                }
                if (!configSelected)
                {
                    foreach (Config con in m.configs)
                    {
                        if ((!con.Visible) || (!con.Enabled) || (!con.Type.Equals("single_dropdown2")))
                            continue;
                        con.Checked = true;
                        ConfigWPFComboBox cwpfcb = (ConfigWPFComboBox)con.ConfigUIComponent;
                        bool breakOut = false;
                        foreach (ComboBoxItem cbi in cwpfcb.Items)
                        {
                            if (cbi.config.Name.Equals(con.Name))
                            {
                                cwpfcb.SelectionChanged -= configControlDDALL_SelectionChanged;
                                cwpfcb.SelectedItem = cbi;
                                cwpfcb.SelectionChanged += configControlDDALL_SelectionChanged;
                                breakOut = true;
                                break;
                            }
                        }
                        if (breakOut)
                            break;
                    }
                }
            }
            else
            {
                //mod not checked, uncheck all the configs
                foreach (Config cfg in m.configs)
                {
                    if (cfg.Enabled)
                    {
                        cfg.Checked = false;
                        // if (cfg.configUIComponent is ConfigFormCheckBox)
                        if (cfg.ConfigUIComponent is ConfigWPFCheckBox)
                        {
                            // ConfigFormCheckBox cfcb = (ConfigFormCheckBox)cfg.configUIComponent;
                            ConfigWPFCheckBox cfcb = (ConfigWPFCheckBox)cfg.ConfigUIComponent;
                            // cfcb.Checked = false;
                            cfcb.IsChecked = false;
                        }
                        // else if (cfg.configUIComponent is ConfigFormRadioButton)
                        else if (cfg.ConfigUIComponent is ConfigWPFRadioButton) 
                        {
                            // ConfigFormRadioButton cfrb = (ConfigFormRadioButton)cfg.configUIComponent;
                            ConfigWPFRadioButton cfrb = (ConfigWPFRadioButton)cfg.ConfigUIComponent;
                            // cfrb.Checked = false;
                            cfrb.IsChecked = false;
                        }
                        // else if (cfg.configUIComponent is ConfigFormComboBox)
                        else if (cfg.ConfigUIComponent is ConfigWPFComboBox) 
                        {
                            // ConfigFormComboBox cfcb = (ConfigFormComboBox)cfg.configUIComponent;
                            ConfigWPFComboBox cfcb = (ConfigWPFComboBox)cfg.ConfigUIComponent;
                            // cfcb.SelectedIndexChanged -= configControlDD_SelectedIndexChanged;
                            cfcb.SelectionChanged -= configControlDDALL_SelectionChanged;
                            cfcb.SelectedIndex = -1;
                            // cfcb.SelectedIndexChanged += configControlDD_SelectedIndexChanged;
                            cfcb.SelectionChanged += configControlDDALL_SelectionChanged;
                        }
                    }
                }
            }
        }
        //when a legacy checkbox of OMC view is clicked
        void configControlCB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LoadingConfig)
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
                }
                else if (parenttvi.Header is ConfigWPFCheckBox)
                {
                    ConfigWPFCheckBox c = (ConfigWPFCheckBox)parenttvi.Header;
                    c.IsChecked = true;
                }
                else if (parenttvi.Header is ConfigWPFRadioButton)
                {
                    ConfigWPFRadioButton c = (ConfigWPFRadioButton)parenttvi.Header;
                    c.IsChecked = true;
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
                            //getting here means cb is Enabled
                            subRB.IsEnabled = true;
                            //this needs to be changed
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
                            if ((bool)subRB.IsEnabled && subc.Enabled)
                            {
                                subc.Checked = true;
                                subRB.IsChecked = true;
                                break;
                            }
                        }
                    }
                }
            }
            else if (cfg.configs.Count > 0 && !cfg.Checked)
            {
                foreach (Config c in cfg.configs)
                {
                    if (c.Type.Equals("single") || c.Type.Equals("single1") || c.Type.Equals("multi"))
                    {
                        c.Checked = false;
                        if (c.ConfigUIComponent is ConfigWPFCheckBox)
                        {
                            ConfigWPFCheckBox tempCB = (ConfigWPFCheckBox)c.ConfigUIComponent;
                            tempCB.IsChecked = false;
                        }
                        else if (c.ConfigUIComponent is ConfigWPFRadioButton)
                        {
                            ConfigWPFRadioButton tempCB = (ConfigWPFRadioButton)c.ConfigUIComponent;
                            tempCB.IsChecked = false;
                        }
                    }
                }
            }
        }
        //when a dropdown legacy combobox is index changed
        void configControlDDALL_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ConfigWPFComboBox cb = (ConfigWPFComboBox)sender;
            if (!LoadingConfig)
            {
                //propagate the check if required
                System.Windows.Controls.TreeViewItem tvi = (System.Windows.Controls.TreeViewItem)cb.Parent;
                ComboBoxItem cbi22 = (ComboBoxItem)cb.SelectedItem;
                if (tvi.Parent is System.Windows.Controls.TreeViewItem && cb.SelectedIndex != -1)
                {
                    System.Windows.Controls.TreeViewItem parenttvi = (System.Windows.Controls.TreeViewItem)tvi.Parent;
                    if (parenttvi.Header is ModWPFCheckBox)
                    {
                        ModWPFCheckBox c = (ModWPFCheckBox)parenttvi.Header;
                        c.IsChecked = true;
                    }
                    else if (parenttvi.Header is ConfigWPFCheckBox)
                    {
                        ConfigWPFCheckBox c = (ConfigWPFCheckBox)parenttvi.Header;
                        c.IsChecked = true;
                    }
                    else if (parenttvi.Header is ConfigWPFRadioButton)
                    {
                        ConfigWPFRadioButton c = (ConfigWPFRadioButton)parenttvi.Header;
                        c.IsChecked = true;
                    }
                }
            }
            //first check if this is init, meaning first time Enabled
            //but now this should never have to run
            //getting here means that an item is confirmed to be selected
            //itterate through the items, get each config, disable it
            //enable the selected one at the end
            foreach (ComboBoxItem cbi in cb.Items)
            {
                cbi.config.Checked = false;
            }
            ComboBoxItem cbi2 = (ComboBoxItem)cb.SelectedItem;
            cbi2.config.Checked = true;
            //set the new tooltip
            string dateFormat = cbi2.config.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(cbi2.config.Timestamp);
            string tooltipString = cbi2.config.Description.Equals("") ? NoDescriptionAvailable : cbi2.config.Description + (cbi2.config.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
            cb.ToolTip = tooltipString;
        }
        //when a radiobutton of the legacy view mode is clicked
        void configControlRB_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LoadingConfig)
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
                        if ((bool)rb.IsEnabled && (bool)rb.IsChecked && (!rb.Equals(cb)))
                        {
                            rb.config.Checked = false;
                            rb.IsChecked = false;
                        }
                    }
                }
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
                }
                else if (parenttvi.Header is ConfigWPFCheckBox)
                {
                    ConfigWPFCheckBox c = (ConfigWPFCheckBox)parenttvi.Header;
                    c.IsChecked = true;
                }
                else if (parenttvi.Header is ConfigWPFRadioButton)
                {
                    ConfigWPFRadioButton c = (ConfigWPFRadioButton)parenttvi.Header;
                    c.IsChecked = true;
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
                            //getting here means cb is Enabled
                            subRB.IsEnabled = true;
                            //this needs to be changed
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
                            if ((bool)subRB.IsEnabled && subc.Enabled)
                            {
                                subc.Checked = true;
                                subRB.IsChecked = true;
                                break;
                            }
                        }
                    }
                }
            }
            else if (cfg.configs.Count > 0 && !cfg.Checked)
            {
                foreach (Config c in cfg.configs)
                {
                    if (c.Type.Equals("single") || c.Type.Equals("single1") || c.Type.Equals("multi"))
                    {
                        c.Checked = false;
                        if (c.ConfigUIComponent is ConfigWPFCheckBox)
                        {
                            ConfigWPFCheckBox tempCB = (ConfigWPFCheckBox)c.ConfigUIComponent;
                            tempCB.IsChecked = false;
                        }
                        else if (c.ConfigUIComponent is ConfigWPFRadioButton)
                        {
                            ConfigWPFRadioButton tempCB = (ConfigWPFRadioButton)c.ConfigUIComponent;
                            tempCB.IsChecked = false;
                        }
                    }
                }
            }
        }

        //adds a mod m to a tabpage t
        private void AddModDefaultView(Mod m, TabPage t, Category catagory)
        {
            if (!m.Visible)
                return;
            int newPanelCount = t.Controls.Count + 1;
            //make the mod check box
            ModFormCheckBox modCheckBox = new ModFormCheckBox() {
                AutoSize = true,
                Location = new Point(3, 3),
                Size = new Size(49, 15),
                TabIndex = 1,
                Text = Utils.ReplaceMacro(m),
                Font = Settings.AppFont,
                catagory = catagory,
                mod = m,
                UseVisualStyleBackColor = true,
                Enabled = m.Enabled,
                Checked = m.Checked
            };
            modCheckBox.MouseDown += Generic_MouseDown;
            //add the ToolTip description to the checkbox
            string dateFormat = m.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(m.Timestamp);
            string tooltipString = m.Description.Equals("") ? NoDescriptionAvailable : m.Description + (m.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
            DescriptionToolTip.SetToolTip(modCheckBox, tooltipString);
            //link the mod components to the UI
            m.TabIndex = t;
            m.ModFormCheckBox = modCheckBox;
            //add it to the search list
            CompleteModSearchList.Add(m);
            //the mod checksum logic
            if (!m.ZipFile.Equals(""))
            {
                string modDownloadPath = Path.Combine(Application.StartupPath, "RelHaxDownloads", m.ZipFile);
                //firstLoad was there first
                if (FirstLoad)
                {
                    string oldCRC2 = GetMD5Hash(modDownloadPath);
                    //if the CRC's don't match and the mod actually has a zip file
                    if ((!m.CRC.Equals(oldCRC2)))
                    {
                        modCheckBox.Text = string.Format("{0} ({1})", modCheckBox.Text, Translations.getTranslatedString("updated"));
                        m.DownloadFlag = true;
                        if ((m.Size > 0))
                            modCheckBox.Text = string.Format("{0} ({1})", modCheckBox.Text, Utils.SizeSuffix(m.Size, 1, true));
                    }
                }
                else
                {
                    if (m.DownloadFlag)
                    {
                        modCheckBox.Text = string.Format("{0} ({1})", modCheckBox.Text, Translations.getTranslatedString("updated"));
                        if ((m.Size > 0))
                            modCheckBox.Text = string.Format("{0} ({1})", modCheckBox.Text, Utils.SizeSuffix(m.Size, 1, true));
                    }
                }
            }
            //in theory it should trigger the handler for checked
            //when initially made it should be false, if Enabled from
            //from user configs
            //make the main panel
            Panel mainPanel = new Panel()
            {
                BorderStyle = Settings.DisableBorders ? BorderStyle.None : BorderStyle.FixedSingle,
                TabIndex = 0,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                Size = new Size(t.Size.Width - 25, 20)
            };
            if (m.Enabled && m.Checked && !Settings.DisableColorChange)
                mainPanel.BackColor = Color.BlanchedAlmond;
            else
                mainPanel.BackColor = Settings.getBackColor();
            int panelCountYLocation = 70 * (newPanelCount - 1);
            //if this is not the first mod being added to the panel
            int panelYLocation = 6; //tab plus delimiter
            if (newPanelCount > 1)
            {
                //create a list of other controlls and put this one 6 pixels below the others
                foreach (Control c in t.Controls)
                {
                    panelYLocation += c.Size.Height;
                    panelYLocation += 6;
                }
                panelCountYLocation = (newPanelCount - 1) * (t.Controls[0].Size.Height);
                panelCountYLocation = panelCountYLocation + 5;
            }
            mainPanel.Location = new Point(5, panelYLocation);
            //add to main panel
            mainPanel.Controls.Add(modCheckBox);
            //processes the subconfigs here
            if (m.configs.Count > 0)
                AddConfigsDefaultView(t, m, catagory, modCheckBox, mainPanel, true, m.configs, mainPanel);
            //add to tab
            t.Controls.Add(mainPanel);
            //add the event handler before changing the checked state so the event
            //event handler is #triggered
            modCheckBox.CheckedChanged += new EventHandler(modCheckBox_CheckedChanged);
        }

        private void AddConfigsDefaultView(TabPage t, Mod m, Category catagory, ModFormCheckBox modCheckBox, Panel mainPanel, bool parentIsMod, List<Config> configs, Panel topPanal, Config parentConfig = null)
        {
            //make config panel
            Panel configPanel = new Panel()
            {
                Enabled = true,
                BorderStyle = Settings.DisableBorders ? BorderStyle.None : BorderStyle.FixedSingle,
                Location = new System.Drawing.Point(3, 10),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowOnly,
                Size = new Size(t.Size.Width - 35, 30),
            };
            if (parentIsMod)
            {
                if (m.Enabled && m.Checked && !Settings.DisableColorChange)
                    configPanel.BackColor = Color.BlanchedAlmond;
                else
                    configPanel.BackColor = Settings.getBackColor();
            }
            else
            {
                if (parentConfig.Enabled && parentConfig.Checked && !Settings.DisableColorChange)
                    configPanel.BackColor = Color.BlanchedAlmond;
                else
                    configPanel.BackColor = Settings.getBackColor();
            }
            int spacer = modCheckBox.Location.Y + modCheckBox.Size.Height + 5;
            switch (Settings.FontSizeforum)
            {
                case Settings.FontSize.Font100:
                    break;
                case Settings.FontSize.Font125:
                    spacer += 3;
                    break;
                case Settings.FontSize.Font175:
                    spacer += 6;
                    break;
                case Settings.FontSize.Font225:
                    spacer += 9;
                    break;
                case Settings.FontSize.Font275:
                    spacer += 12;
                    break;
            }
            if (parentIsMod)
            {
                configPanel.Location = new Point(configPanel.Location.X + 10, spacer);
            }
            else
            {
                configPanel.Location = new Point(configPanel.Location.X + 10, getYLocation(mainPanel.Controls));
            }
            mainPanel.Controls.Add(configPanel);
            ConfigFormComboBox configControlDD = null;
            ConfigFormComboBox configControlDD2 = null;
            foreach (Config con in configs)
            {
                if (!con.Visible)
                    continue;
                ConfigFormComboBox configControlDDALL = null;
                con.ParentMod = m;
                if (parentIsMod)
                {
                    con.Parent = m;
                    CompleteModSearchList.Add(con);
                }
                else
                {
                    con.Parent = parentConfig;
                }
                if (con.Type.Equals("single") || con.Type.Equals("single1"))
                {
                    //make default radioButton
                    ConfigFormRadioButton configControlRB = new ConfigFormRadioButton()
                    {
                        AutoSize = true,
                        Location = new Point(6, getYLocation(configPanel.Controls)),
                        Size = new Size(150, 15),
                        Font = Settings.AppFont,
                        catagory = catagory,
                        mod = m,
                        config = con,
                        Enabled = false,
                        Checked = false
                    };
                    //add handlers
                    configControlRB.CheckedChanged += configControlRB_CheckedChanged;
                    configControlRB.MouseDown += Generic_MouseDown;
                    //add the ToolTip description to the checkbox
                    string dateFormat = con.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(con.Timestamp);
                    string tooltipString = con.Description.Equals("") ? NoDescriptionAvailable : con.Description + (con.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
                    DescriptionToolTip.SetToolTip(configControlRB, tooltipString);
                    //link the UI to the package
                    con.ConfigUIComponent = configControlRB;
                    //logic for determining if it can be Enabled
                    //get all levels up to the mod, then deal with the mod
                    bool canBeEnabled = true;
                    //check all parent configs, if any
                    if (con.Parent is Config parentConfig2)
                    {
                        while (parentConfig2 is Config)
                        {
                            if (!parentConfig2.Enabled)
                                canBeEnabled = false;
                            if (parentConfig2.Parent is Mod)
                                break;
                            parentConfig2 = (Config)parentConfig2.Parent;
                        }
                    }
                    //check the parent mod
                    if (!con.ParentMod.Enabled)
                        canBeEnabled = false;
                    //check itself (before it reks itself)
                    if (!con.Enabled)
                        canBeEnabled = false;
                    if (canBeEnabled)
                        configControlRB.Enabled = true;
                    if (configControlRB.Enabled)
                        if (con.Checked)
                            configControlRB.Checked = true;
                    configControlRB.Text = Utils.ReplaceMacro(con);
                    //run checksum logic
                    if (!con.ZipFile.Equals(""))
                    {
                        if (FirstLoad)
                        {
                            string oldCRC = GetMD5Hash(Path.Combine(Application.StartupPath, "RelHaxDownloads", con.ZipFile));
                            if ((!oldCRC.Equals(con.CRC)))
                            {
                                configControlRB.Text = string.Format("{0} ({1})", configControlRB.Text, Translations.getTranslatedString("updated"));
                                con.DownloadFlag = true;
                                if (con.Size > 0)
                                    configControlRB.Text = string.Format("{0} ({1})", configControlRB.Text, Utils.SizeSuffix(con.Size, 1, true));
                            }
                        }
                        else
                        {
                            if (con.DownloadFlag)
                            {
                                configControlRB.Text = string.Format("{0} ({1})", configControlRB.Text, Translations.getTranslatedString("updated"));
                                if (con.Size > 0)
                                    configControlRB.Text = string.Format("{0} ({1})", configControlRB.Text, Utils.SizeSuffix(con.Size, 1, true));
                            }
                        }
                    }
                    //add the config to the form
                    configPanel.Controls.Add(configControlRB);
                    //process the subconfigs
                    if (con.configs.Count > 0)
                        AddConfigsDefaultView(t, m, catagory, modCheckBox, configPanel, false, con.configs, topPanal, con);
                }
                else if (con.Type.Equals("single_dropdown") || con.Type.Equals("single_dropdown1") || con.Type.Equals("single_dropdown2"))
                {
                    //set the all one to the version is actually is
                    if (con.Type.Equals("single_dropdown") || con.Type.Equals("single_dropdown1"))
                    {
                        //create it if it's null
                        if (configControlDD == null)
                        {
                            configControlDD = new ConfigFormComboBox()
                            {
                                AutoSize = true,
                                Location = new Point(0, 0),
                                Size = new Size(225, 15),
                                Enabled = false,
                                DropDownStyle = ComboBoxStyle.DropDownList
                            };
                            configControlDD.SelectedIndexChanged += configControlDD_SelectedIndexChanged;
                            //https://stackoverflow.com/questions/1882993/c-sharp-how-do-i-prevent-mousewheel-scrolling-in-my-combobox
                            configControlDD.MouseWheel += (o, e) => ((HandledMouseEventArgs)e).Handled = true;
                            configControlDD.MouseDown += Generic_MouseDown;
                        }
                        configControlDDALL = configControlDD;
                        con.ConfigUIComponent = configControlDD;
                    }
                    else if (con.Type.Equals("single_dropdown2"))
                    {
                        //create it if it's null
                        if (configControlDD2 == null)
                        {
                            configControlDD2 = new ConfigFormComboBox()
                            {
                                AutoSize = true,
                                Location = new Point(0, 0),
                                Size = new Size(225, 15),
                                Enabled = false,
                                DropDownStyle = ComboBoxStyle.DropDownList
                            };
                            configControlDD2.SelectedIndexChanged += configControlDD_SelectedIndexChanged;
                            //https://stackoverflow.com/questions/1882993/c-sharp-how-do-i-prevent-mousewheel-scrolling-in-my-combobox
                            configControlDD2.MouseWheel += (o, e) => ((HandledMouseEventArgs)e).Handled = true;
                            configControlDD2.MouseDown += Generic_MouseDown;
                        }
                        configControlDDALL = configControlDD2;
                        con.ConfigUIComponent = configControlDD2;
                    }
                    
                    //make a dropDown selection box
                    if (configControlDDALL.Location.X == 0 && configControlDDALL.Location.Y == 0)
                    {
                        //init the box, including adding the label
                        configControlDDALL.Location = new Point(6, getYLocation(configPanel.Controls));
                        configPanel.Controls.Add(configControlDDALL);
                    }
                    ComboBoxItem cbi = null;
                    string toAdd = Utils.ReplaceMacro(con);
                    //run the checksum locics
                    if (FirstLoad)
                    {
                        string oldCRC = GetMD5Hash(Path.Combine(Application.StartupPath, "RelHaxDownloads", con.ZipFile));
                        if ((!con.CRC.Equals("")) && (!oldCRC.Equals(con.CRC)))
                        {
                            con.DownloadFlag = true;
                            toAdd = string.Format("{0} ({1})", toAdd, Translations.getTranslatedString("updated"));
                            if (con.Size > 0)
                                toAdd = string.Format("{0} ({1})", toAdd, Utils.SizeSuffix(con.Size, 1, true));
                        }
                    }
                    else
                    {
                        if (con.DownloadFlag)
                        {
                            toAdd = string.Format("{0} ({1})", toAdd, Translations.getTranslatedString("updated"));
                            if (con.Size > 0)
                                toAdd = string.Format("{0} ({1})", toAdd, Utils.SizeSuffix(con.Size, 1, true));
                        }
                    }
                    //add it
                    if (con.Enabled)
                    {
                        cbi = new ComboBoxItem(con, toAdd);
                        configControlDDALL.Items.Add(cbi);
                        if (con.Checked)
                        {
                            configControlDDALL.SelectedItem = cbi;
                            configControlDDALL.Enabled = true;
                            //set the tooltip to the checked option
                            string dateFormat = con.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(con.Timestamp);
                            string tooltipString = con.Description.Equals("") ? NoDescriptionAvailable : con.Description + (con.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
                            DescriptionToolTip.SetToolTip(configControlDDALL, tooltipString);
                        }
                    }
                    if (configControlDDALL.Items.Count > 0)
                    {
                        configControlDDALL.Enabled = true;
                        if (configControlDDALL.SelectedIndex == -1)
                        {
                            configControlDDALL.SelectedIndex = 0;
                            //set the tooltip since nothing has been selected
                            ComboBoxItem cbiTT = (ComboBoxItem)configControlDDALL.Items[0];
                            string dateFormat = con.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(con.Timestamp);
                            string tooltipString = con.Description.Equals("") ? NoDescriptionAvailable : con.Description + (con.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
                            DescriptionToolTip.SetToolTip(configControlDDALL, tooltipString);
                        }
                    }
                }
                else if (con.Type.Equals("multi"))
                {
                    //make a checkBox
                    ConfigFormCheckBox configControlCB = new ConfigFormCheckBox()
                    {
                        AutoSize = true,
                        Location = new Point(6, getYLocation(configPanel.Controls)),
                        Size = new Size(150, 15),
                        Font = Settings.AppFont,
                        catagory = catagory,
                        mod = m,
                        config = con,
                        Enabled = false,
                        Checked = false
                    };
                    //add the ToolTip description to the checkbox
                    string dateFormat = con.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(con.Timestamp);
                    string tooltipString = con.Description.Equals("") ? NoDescriptionAvailable : con.Description + (con.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
                    DescriptionToolTip.SetToolTip(configControlCB, tooltipString);
                    //link the Ui to the config
                    con.ConfigUIComponent = configControlCB;
                    //add handlers
                    configControlCB.CheckedChanged += configControlCB_CheckedChanged;
                    configControlCB.MouseDown += Generic_MouseDown;
                    //logic for determining if it can be Enabled
                    //get all levels up to the mod, then deal with the mod
                    bool canBeEnabled = true;
                    //check all parent configs, if any
                    if (con.Parent is Config parentConfig2)
                    {
                        while (parentConfig2 is Config)
                        {
                            if (!parentConfig2.Enabled)
                                canBeEnabled = false;
                            if (parentConfig2.Parent is Mod)
                                break;
                            parentConfig2 = (Config)parentConfig2.Parent;
                        }
                    }
                    //check the parent mod
                    if (!con.ParentMod.Enabled)
                        canBeEnabled = false;
                    //check itself (before it reks itself)
                    if (!con.Enabled)
                        canBeEnabled = false;
                    if (canBeEnabled)
                        configControlCB.Enabled = true;
                    if (configControlCB.Enabled)
                        if (con.Checked)
                            configControlCB.Checked = true;
                    
                    //checksum logic
                    configControlCB.Text = Utils.ReplaceMacro(con);
                    if (FirstLoad)
                    {
                        string oldCRC = GetMD5Hash(Path.Combine(Application.StartupPath, "RelHaxDownloads", con.ZipFile));
                        if ((!con.CRC.Equals("")) && (!oldCRC.Equals(con.CRC)))
                        {
                            con.DownloadFlag = true;
                            configControlCB.Text = string.Format("{0} ({1})", configControlCB.Text, Translations.getTranslatedString("updated"));
                            if (con.Size > 0)
                                configControlCB.Text = string.Format("{0} ({1})", configControlCB.Text, Utils.SizeSuffix(con.Size, 1, true));
                        }
                    }
                    else
                    {
                        if (con.DownloadFlag)
                        {
                            configControlCB.Text = string.Format("{0} ({1})", configControlCB.Text, Translations.getTranslatedString("updated"));
                            if (con.Size > 0)
                                configControlCB.Text = string.Format("{0} ({1})", configControlCB.Text, Utils.SizeSuffix(con.Size, 1, true));
                        }
                    }
                    //add config to the form
                    configPanel.Controls.Add(configControlCB);
                    //process subconfigs
                    if (con.configs.Count > 0)
                        AddConfigsDefaultView(t, m, catagory, modCheckBox, configPanel, false, con.configs, topPanal, con);
                }
                else
                {
                    Logging.Manager(string.Format("WARNING: Unknown config type for {0}: {1}", con.Name, con.Type));
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
                if (c is ConfigFormComboBox)
                {
                    switch (Settings.FontSizeforum)
                    {
                        case Settings.FontSize.Font100:
                            break;
                        case Settings.FontSize.Font125:
                            y += 3;
                            break;
                        case Settings.FontSize.Font175:
                            y += 6;
                            break;
                        case Settings.FontSize.Font225:
                            y += 9;
                            break;
                        case Settings.FontSize.Font275:
                            y += 12;
                            break;
                        case Settings.FontSize.DPI100:
                            break;
                        case Settings.FontSize.DPI125:
                            y += 3;
                            break;
                        case Settings.FontSize.DPI175:
                            y += 6;
                            break;
                        case Settings.FontSize.DPI225:
                            y += 9;
                            break;
                        case Settings.FontSize.DPI275:
                            y += 12;
                            break;
                    }
                }
            }
            return y;
        }
        
        //handler for when a mod checkbox is changed
        void modCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (LoadingConfig)
                return;
            ModFormCheckBox cbUser = (ModFormCheckBox)sender;
            if (cbUser.Parent is TabPage)
            {
                //user mod code
                TabPage t = (TabPage)cbUser.Parent;
                if (t.Text.Equals("User Mods"))
                {
                    //verified, this is a check from the user checkboxes
                    cbUser.mod.Checked = cbUser.Checked;
                    return;
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

            //just a check
            if (!m.Visible)
                return;

            //check to see if the mod is part of a single selection only catagory
            //if it is uncheck the other mods first, then deal with mod loop selection
            if (cat.SelectionType.Equals("single") && cb.Checked)
            {
                //check if any other mods in this catagory are already checked
                bool anyModsChecked = false;
                foreach (Mod mm in cat.Mods)
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
                                    cbb.Checked = false;
                                    cbb.mod.Checked = false;
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
                }

            }
            //toggle the mod in memory, Enabled or disabled
            m.Checked = cb.Checked;
            //toggle the mod panel color
            if (cb.Checked && !Settings.DisableColorChange)
            {
                modPanel.BackColor = Color.BlanchedAlmond;
            }
            else
            {
                modPanel.BackColor = Settings.getBackColor();
            }
            //this deals with enabling the componets and triggering the handlers
            if (m.configs.Count == 0)
                return;
            //the first one is always the mod checkbox
            //the second one is always the config panel
            Panel configPanel = (Panel)modPanel.Controls[1];
            if (cb.Checked && !Settings.DisableColorChange)
            {
                configPanel.BackColor = Color.BlanchedAlmond;
            }
            else
            {
                configPanel.BackColor = Settings.getBackColor();
            }
            if (m.Checked)
            {
                //mod checked, check at least one single1, one single_dropdown1, one single_dropdown2
                //checking for single/single1 configs
                bool configSelected = false;
                foreach (Config con in m.configs)
                {
                    if ((!con.Visible) || (!con.Enabled))
                        continue;
                    if ((con.Type.Equals("single")) || (con.Type.Equals("single1")))
                    {
                        if (con.Checked)
                            configSelected = true;
                    }
                }
                if (!configSelected)
                {
                    foreach (Config con in m.configs)
                    {
                        if ((!con.Visible) || (!con.Enabled))
                            continue;
                        if ((con.Type.Equals("single")) || (con.Type.Equals("single1")))
                        {
                            con.Checked = true;
                            ConfigFormRadioButton cfrb = (ConfigFormRadioButton)con.ConfigUIComponent;
                            cfrb.Checked = true;
                            break;
                        }
                    }
                }
                //checking for single_dropdown/single_dropdown1 configs
                configSelected = false;
                foreach (Config con in m.configs)
                {
                    if ((!con.Visible) || (!con.Enabled))
                        continue;
                    if ((con.Type.Equals("single_dropdown")) || (con.Type.Equals("single_dropdown1")))
                    {
                        if (con.Checked)
                            configSelected = true;
                    }
                }
                if (!configSelected)
                {
                    foreach (Config con in m.configs)
                    {
                        if ((!con.Visible) || (!con.Enabled))
                            continue;
                        if ((con.Type.Equals("single_dropdown")) || (con.Type.Equals("single_dropdown1")))
                        {
                            con.Checked = true;
                            ConfigFormComboBox cfcb = (ConfigFormComboBox)con.ConfigUIComponent;
                            bool breakOut = false;
                            foreach (ComboBoxItem cbi in cfcb.Items)
                            {
                                if (cbi.config.Name.Equals(con.Name))
                                {
                                    cfcb.SelectedIndexChanged -= configControlDD_SelectedIndexChanged;
                                    cfcb.SelectedItem = cbi;
                                    cfcb.SelectedIndexChanged += configControlDD_SelectedIndexChanged;
                                    breakOut = true;
                                    break;
                                }
                            }
                            if (breakOut)
                                break;
                        }
                    }
                }
                //checking for single_dropdown2 configs
                configSelected = false;
                foreach (Config con in m.configs)
                {
                    if ((!con.Visible) || (!con.Enabled) || (!con.Type.Equals("single_dropdown2")))
                        continue;
                    if (con.Checked)
                        configSelected = true;
                }
                if (!configSelected)
                {
                    foreach (Config con in m.configs)
                    {
                        if ((!con.Visible) || (!con.Enabled) || (!con.Type.Equals("single_dropdown2")))
                            continue;
                        con.Checked = true;
                        ConfigFormComboBox cfcb = (ConfigFormComboBox)con.ConfigUIComponent;
                        bool breakOut = false;
                        foreach (ComboBoxItem cbi in cfcb.Items)
                        {
                            if (cbi.config.Name.Equals(con.Name))
                            {
                                cfcb.SelectedIndexChanged -= configControlDD_SelectedIndexChanged;
                                cfcb.SelectedItem = cbi;
                                cfcb.SelectedIndexChanged += configControlDD_SelectedIndexChanged;
                                breakOut = true;
                                break;
                            }
                        }
                        if (breakOut)
                            break;
                    }
                }
            }
            else
            {
                //mod not checked, uncheck all the configs
                foreach (Config cfg in m.configs)
                {
                    if (cfg.Enabled)
                    {
                        cfg.Checked = false;
                        if (cfg.ConfigUIComponent is ConfigFormCheckBox)
                        {
                            ConfigFormCheckBox cfcb = (ConfigFormCheckBox)cfg.ConfigUIComponent;
                            cfcb.Checked = false;
                        }
                        else if (cfg.ConfigUIComponent is ConfigFormRadioButton)
                        {
                            ConfigFormRadioButton cfrb = (ConfigFormRadioButton)cfg.ConfigUIComponent;
                            cfrb.Checked = false;
                        }
                        else if (cfg.ConfigUIComponent is ConfigFormComboBox)
                        {
                            ConfigFormComboBox cfcb = (ConfigFormComboBox)cfg.ConfigUIComponent;
                            cfcb.SelectedIndexChanged -= configControlDD_SelectedIndexChanged;
                            cfcb.SelectedIndex = 0;
                            cfcb.SelectedIndexChanged += configControlDD_SelectedIndexChanged;
                        }
                    }
                }
            }
        }

        //handler for when the config checkbox is checked or unchecked
        void configControlCB_CheckedChanged(object sender, EventArgs e)
        {
            if (LoadingConfig)
                return;
            //checkboxes don't need to be unselected
            ConfigFormCheckBox cb = (ConfigFormCheckBox)sender;
            Mod m = cb.mod;
            Config cfg = cb.config;
            Category cat = cb.catagory;
            Panel configPanel = (Panel)cb.Parent;
            //checkbox is Enabled, toggle checked and checked
            cfg.Checked = cb.Checked;
            //propagate the check back up if required
            if (cfg.Checked)
            {
                SelectableDatabasePackage obj = cfg.Parent;
                if (obj is Mod)
                {
                    Mod parentM = (Mod)obj;
                    if (parentM.ModFormCheckBox is ModFormCheckBox)
                    {
                        ModFormCheckBox tempCB = (ModFormCheckBox)parentM.ModFormCheckBox;
                        if (!tempCB.Checked)
                            tempCB.Checked = true;
                    }
                }
                else if (obj is Config)
                {
                    Config parentC = (Config)obj;
                    parentC.Checked = true;
                    if (parentC.ConfigUIComponent is ConfigFormCheckBox)
                    {
                        ConfigFormCheckBox parentCB = (ConfigFormCheckBox)parentC.ConfigUIComponent;
                        if (!parentCB.Checked)
                            parentCB.Checked = true;
                    }
                    else if (parentC.ConfigUIComponent is ConfigFormRadioButton)
                    {
                        ConfigFormRadioButton parentRB = (ConfigFormRadioButton)parentC.ConfigUIComponent;
                        if (!parentRB.Checked)
                            parentRB.Checked = true;
                    }
                }
            }
            //process any subconfigs
            bool configSelected = false;
            int radioButtonCount = 0;
            if (cfg.configs.Count > 0 && cb.Checked)
            {
                foreach (Config c in cfg.configs)
                {
                    if (c.Type.Equals("single") || c.Type.Equals("single1"))
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
                        if ((c.Type.Equals("single") || c.Type.Equals("single1")) && c.Enabled)
                        {
                            c.Checked = true;
                            ConfigFormRadioButton subRB = (ConfigFormRadioButton)c.ConfigUIComponent;
                            subRB.Checked = true;
                            break;
                        }
                    }
                }
            }
            else if (cfg.configs.Count > 0 && !cb.Checked)
            {
                foreach (Config c in cfg.configs)
                {
                    if (c.Type.Equals("single") || c.Type.Equals("single1") || c.Type.Equals("multi"))
                    {
                        c.Checked = false;
                        if (c.ConfigUIComponent is ConfigFormCheckBox)
                        {
                            ConfigFormCheckBox tempCB = (ConfigFormCheckBox)c.ConfigUIComponent;
                            tempCB.Checked = false;
                        }
                        else if (c.ConfigUIComponent is ConfigFormRadioButton)
                        {
                            ConfigFormRadioButton tempCB = (ConfigFormRadioButton)c.ConfigUIComponent;
                            tempCB.Checked = false;
                        }
                    }
                }
            }
            //trigger the panel color change
            if (cb.Checked && !Settings.DisableColorChange)
                configPanel.BackColor = Color.BlanchedAlmond;
            else
                configPanel.BackColor = Settings.getBackColor();
        }
        //handler for when a config selection is made from the drop down list
        void configControlDD_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LoadingConfig)
                return;
            //uncheck all other dorp down configs
            ConfigFormComboBox cb = (ConfigFormComboBox)sender;
            //due to recursion, save the cbi up here
            ComboBoxItem cbi2 = (ComboBoxItem)cb.SelectedItem;
            //propagate the check back up if required
            if (cb.SelectedIndex != -1)
            {
                ComboBoxItem cbi22 = (ComboBoxItem)cb.SelectedItem;
                SelectableDatabasePackage obj = cbi22.config.Parent;
                if (obj is Mod)
                {
                    Mod parentM = (Mod)obj;
                    if (parentM.ModFormCheckBox is ModFormCheckBox)
                    {
                        ModFormCheckBox tempCB = (ModFormCheckBox)parentM.ModFormCheckBox;
                        //adding the if statement prevents it from running again when it's not needed to
                        if (!tempCB.Checked)
                            tempCB.Checked = true;
                    }
                }
                else if (obj is Config)
                {
                    Config parentC = (Config)obj;
                    parentC.Checked = true;
                    if (parentC.ConfigUIComponent is ConfigFormCheckBox)
                    {
                        ConfigFormCheckBox parentCB = (ConfigFormCheckBox)parentC.ConfigUIComponent;
                        if (!parentCB.Checked)
                            parentCB.Checked = true;
                    }
                    else if (parentC.ConfigUIComponent is ConfigFormRadioButton)
                    {
                        ConfigFormRadioButton parentRB = (ConfigFormRadioButton)parentC.ConfigUIComponent;
                        if (!parentRB.Checked)
                            parentRB.Checked = true;
                    }
                }
            }
            //itterate through the items, get each config, disable it
            //unless it's the same name as the selectedItem
            foreach (ComboBoxItem cbi in cb.Items)
            {
                cbi.config.Checked = false;
            }
            //ComboBoxItem cbi2 = (ComboBoxItem)cb.SelectedItem;
            cb.SelectedItem = cbi2;
            cbi2.config.Checked = true;
            string dateFormat = cbi2.config.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(cbi2.config.Timestamp);
            string tooltipString = cbi2.config.Description.Equals("") ? NoDescriptionAvailable : cbi2.config.Description + (cbi2.config.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
            DescriptionToolTip.SetToolTip(cb, tooltipString);
            Panel configPanel = (Panel)cb.Parent;
            if (!Settings.DisableColorChange)
                configPanel.BackColor = Color.BlanchedAlmond;
        }
        //handler for when a config radioButton is pressed
        void configControlRB_CheckedChanged(object sender, EventArgs e)
        {
            if (LoadingConfig)
                return;
            //get all required cool stuff
            ConfigFormRadioButton rb = (ConfigFormRadioButton)sender;
            Panel configPanel = (Panel)rb.Parent;
            Mod m = rb.mod;
            Config cfg = rb.config;
            Category cat = rb.catagory;
            cfg.Checked = rb.Checked;
            //propagate the check back up if required
            if (cfg.Checked)
            {
                SelectableDatabasePackage obj = cfg.Parent;
                if (obj is Mod)
                {
                    Mod parentM = (Mod)obj;
                    if (parentM.ModFormCheckBox is ModFormCheckBox)
                    {
                        ModFormCheckBox tempCB = (ModFormCheckBox)parentM.ModFormCheckBox;
                        if (!tempCB.Checked)
                            tempCB.Checked = true;
                    }
                }
                else if (obj is Config)
                {
                    Config parentC = (Config)obj;
                    parentC.Checked = true;
                    if (parentC.ConfigUIComponent is ConfigFormCheckBox)
                    {
                        ConfigFormCheckBox parentCB = (ConfigFormCheckBox)parentC.ConfigUIComponent;
                        if (!parentCB.Checked)
                            parentCB.Checked = true;
                    }
                    else if (parentC.ConfigUIComponent is ConfigFormRadioButton)
                    {
                        ConfigFormRadioButton parentRB = (ConfigFormRadioButton)parentC.ConfigUIComponent;
                        if (!parentRB.Checked)
                            parentRB.Checked = true;
                    }
                }
            }
            //propagate the change back down if required
            bool configSelected = false;
            int radioButtonCount = 0;
            if (cfg.configs.Count > 0 && rb.Checked)
            {
                //configs present and the radio button is checked
                //singles - at lease one must be selected
                foreach (Config c in cfg.configs)
                {
                    if (c.Type.Equals("single") || c.Type.Equals("single1"))
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
                        if ((c.Type.Equals("single") || c.Type.Equals("single1")) && c.Enabled)
                        {
                            c.Checked = true;
                            ConfigFormRadioButton subRB = (ConfigFormRadioButton)c.ConfigUIComponent;
                            subRB.Checked = true;
                            break;
                        }
                    }
                }
            }
            else if (cfg.configs.Count > 0 && !rb.Checked)
            {
                //configs present and the radio button is not checked
                //singles - uncheck all of them
                foreach (Config c in cfg.configs)
                {
                    if (c.Type.Equals("single") || c.Type.Equals("single1") || c.Type.Equals("multi"))
                    {
                        c.Checked = false;
                        if (c.ConfigUIComponent is ConfigFormCheckBox)
                        {
                            ConfigFormCheckBox tempCB = (ConfigFormCheckBox)c.ConfigUIComponent;
                            tempCB.Checked = false;
                        }
                        else if (c.ConfigUIComponent is ConfigFormRadioButton)
                        {
                            ConfigFormRadioButton tempCB = (ConfigFormRadioButton)c.ConfigUIComponent;
                            tempCB.Checked = false;
                        }
                    }
                }
            }
            //trigger the panel color change
            if (rb.Checked && !Settings.DisableColorChange)
                configPanel.BackColor = Color.BlanchedAlmond;
            else
                configPanel.BackColor = Settings.getBackColor();
        }
        //generic hander for when any mouse button is clicked for MouseDown Events
        void Generic_MouseDown(object sender, EventArgs e)
        {
            //we only care about the right mouse click tho
            if (e is MouseEventArgs)
            {
                MouseEventArgs ee = (MouseEventArgs)e;
                if (ee.Button != MouseButtons.Right)
                    return;
            }
            else if (e is System.Windows.Input.MouseButtonEventArgs)
            {
                System.Windows.Input.MouseButtonEventArgs ee = (System.Windows.Input.MouseButtonEventArgs)e;
                if (ee.RightButton != System.Windows.Input.MouseButtonState.Pressed)
                {
                    return;
                }
            }
            if (sender is UIComponent)
            {
                UIComponent UIC = (UIComponent)sender;
                SelectableDatabasePackage DBO = null;
                //check if comboBox before mod or config
                //check config before checking mod
                if (sender is ConfigFormComboBox)
                {
                    ConfigFormComboBox cfcb = (ConfigFormComboBox)sender;
                    ComboBoxItem cbi = (ComboBoxItem)cfcb.SelectedItem;
                    DBO = cbi.config;
                }
                else if (sender is ConfigWPFComboBox)
                {
                    ConfigWPFComboBox cwpfcb = (ConfigWPFComboBox)sender;
                    ComboBoxItem cbi = (ComboBoxItem)cwpfcb.SelectedItem;
                    DBO = cbi.config;
                }
                else if (UIC.config != null)
                {
                    DBO = UIC.config;
                }
                else if (UIC.mod != null)
                {
                    DBO = UIC.mod;
                }
                if (DBO != null)
                {
                    if (DBO.DevURL == null)
                        DBO.DevURL = "";
                    if (p != null)
                    {
                        p.Close();
                        p.Dispose();
                        p = null;
                        GC.Collect();
                    }
                    p = new Preview()
                    {
                        LastUpdated = this.LastUpdated,
                        DBO = DBO,
                        Medias = DBO.PictureList
                    };
                    p.Show();
                }
            }
        }

        //handler to set the cancel bool to false
        private void continueButton_Click(object sender, EventArgs e)
        {
            //save the last config if told to do so
            if (Settings.SaveLastConfig)
            {
                XMLUtils.SaveConfig(false, null, ParsedCatagoryList, UserMods);
            }
            DialogResult = DialogResult.OK;
        }
        //handler for when the cancal button is clicked
        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //handler for when the "load config" button is pressed
        private void loadConfigButton_Click(object sender, EventArgs e)
        {
            LoadMode = LoadConfigMode.FromButton;
            this.parseLoadConfig();
        }
        //handler for when the "save config" button is pressed
        private void saveConfigButton_Click(object sender, EventArgs e)
        {
            XMLUtils.SaveConfig(true, null, ParsedCatagoryList, UserMods);
        }
        //handler for when the close button is pressed
        private void ModSelectionList_FormClosing(object sender, FormClosingEventArgs e)
        {
            //save the size of this window for later.
            Settings.ModSelectionHeight = Size.Height;
            Settings.ModSelectionWidth = Size.Width;
            if (TaskBarHidden)
                Settings.SetTaskbarState(Settings.AppBarStates.AutoHide);
            //save wether the window was in fullscreen mods before closing
            if (WindowState == FormWindowState.Maximized)
                Settings.ModSelectionFullscreen = true;
            else
                Settings.ModSelectionFullscreen = false;
            //close the preview window if it is open
            if (p != null)
            {
                p.Close();
                p.Dispose();
                p = null;
            }
        }

        private void clearSelectionsButton_Click(object sender, EventArgs e)
        {
            //not actually *loading* a config, but want to disable the handlers anyways
            LoadingConfig = true;
            Logging.Manager("clearSelectionsButton pressed, clearing selections");
            Utils.ClearSelectionMemory(ParsedCatagoryList, UserMods);
            //dispose of not needed stuff and reload the UI

            LoadingConfig = false;
            MessageBox.Show(Translations.getTranslatedString("selectionsCleared"));
            ModSelectionList_SizeChanged(null, null);
        }

        private void parseLoadConfig()
        {
            //disable any possible UI interaction that would not be desired
            LoadingConfig = true;
            string filePath = "";
            //get the filePath of the selection file based on the mode of loading it
            switch(LoadMode)
            {
                case LoadConfigMode.FromAutoInstall:
                    filePath = Path.Combine(Application.StartupPath, "RelHaxUserConfigs", Program.configName);
                    if (!File.Exists(filePath))
                    {
                        Logging.Manager(string.Format("ERROR: {0} not found, not loading configs", filePath));
                        MessageBox.Show(Translations.getTranslatedString("configLoadFailed"), Translations.getTranslatedString("critical"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        return;
                    }
                    break;
                case LoadConfigMode.FromSaveLastConfig:
                    filePath = Path.Combine(Application.StartupPath, "RelHaxUserConfigs", "lastInstalledConfig.xml");
                    if (!File.Exists(filePath))
                    {
                        Logging.Manager(string.Format("ERROR: {0} not found, not loading configs", filePath));
                        return;
                    }
                    break;
                default:
                    //use a selection viewer for selecting a dev config or a user config (local file)
                    using (SelectionViewer sv = new SelectionViewer(this.Location.X + 100, this.Location.Y + 100, Settings.ModInfoDatFile))
                    {
                        if (!(sv.ShowDialog() == DialogResult.OK))
                        {
                            return;
                        }
                        filePath = sv.SelectedXML;
                        if (filePath.Equals("localFile"))
                        {
                            //user wants to load a personal custom config from file
                            using (OpenFileDialog loadLocation = new OpenFileDialog()
                            {
                                AddExtension = true,
                                DefaultExt = ".xml",
                                Filter = "*.xml|*.xml",
                                InitialDirectory = Path.Combine(Application.StartupPath, "RelHaxUserConfigs"),
                                Title = Translations.getTranslatedString("selectConfigFile")
                            })
                            {
                                if (!(loadLocation.ShowDialog().Equals(DialogResult.OK)))
                                {
                                    //quit
                                    return;
                                }
                                filePath = loadLocation.FileName;
                            }
                        }
                    }
                    break;
            }
            //actually load the config
            XMLUtils.LoadConfig(LoadMode == LoadConfigMode.FromButton, filePath, ParsedCatagoryList, UserMods);
            //if it was from a button, tell the user it loaded the config sucessfully
            if (LoadMode == LoadConfigMode.FromButton)
            {
                if (LoadMode == LoadConfigMode.FromButton) MessageBox.Show(Translations.getTranslatedString("prefrencesSet"), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadingConfig = false;
                ModSelectionList_SizeChanged(null, null);
            }
            //set this back to false so the user can interact
            LoadingConfig = false;
        }
        #region UI event handlers (resize, expand toggling)
        //resizing handler for the window
        private void ModSelectionList_SizeChanged(object sender, EventArgs e)
        {
            if (!this.Visible)
                return;
            if (this.WindowState == FormWindowState.Minimized)
                return;
            colapseAllButton.Location = new Point(this.Size.Width - 20 - colapseAllButton.Size.Width - 6 - expandAllButton.Size.Width, colapseAllButton.Location.Y);
            expandAllButton.Location = new Point(this.Size.Width - 20 - expandAllButton.Size.Width, expandAllButton.Location.Y);
            searchCB.Location = new Point(this.Size.Width - 20 - searchCB.Size.Width, searchCB.Location.Y);
            continueButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - TitleBarDifference);
            cancelButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - TitleBarDifference);
            modTabGroups.Size = new Size(this.Size.Width - 20 - modTabGroups.Location.X, this.Size.Height - modTabGroups.Location.Y - 39 - continueButton.Size.Height - 6 - TitleBarDifference);
            label1.Text = "" + this.Size.Width + " x " + this.Size.Height;
            loadConfigButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width - 6 - loadConfigButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - TitleBarDifference);
            saveConfigButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - TitleBarDifference);
            clearSelectionsButton.Location = new Point(this.Size.Width - 20 - continueButton.Size.Width - 6 - cancelButton.Size.Width - 6 - saveConfigButton.Size.Width - 6 - loadConfigButton.Size.Width - 6 - clearSelectionsButton.Size.Width, this.Size.Height - 39 - continueButton.Size.Height - TitleBarDifference);
            if (this.Size.Height < 250)
            {
                this.Size = new Size(this.Size.Width, 250);
            }
            if (this.Size.Width < 550)
            {
                this.Size = new Size(550, this.Size.Height);
            }
            foreach (TabPage t in modTabGroups.TabPages)
            {
                foreach (Control c in t.Controls)
                {
                    if (c is Panel)
                    {
                        //mod panel
                        Panel p = (Panel)c;
                        resizePanel(p, t, 25);
                    }
                    else if (c is ElementHost)
                    {
                        ElementHost eh = (ElementHost)c;
                        eh.Size = new Size(t.Size.Width - 12, t.Size.Height - 10);
                        LegacySelectionList lsl = (LegacySelectionList)eh.Child;
                        try
                        {
                            lsl.RenderSize = new System.Windows.Size(eh.Size.Width - 2, eh.Size.Height - 2);
                            lsl.legacyTreeView.Width = eh.Size.Width - 4;
                            lsl.legacyTreeView.Height = eh.Size.Height - 4;
                        }
                        catch
                        {
                            // values are negative !
                            // this is catching the exception if the ModSelectionWindow is going to minimize
                        }
                    }
                }
            }
        }
        //recursive resize of the control panals
        private void resizePanel(Panel current, TabPage tp, int shrinkFactor)
        {
            current.Size = new Size(tp.Size.Width - shrinkFactor, current.Size.Height);
            foreach (Control controfds in current.Controls)
            {
                if (controfds is Panel)
                {
                    Panel subpp = (Panel)controfds;
                    resizePanel(subpp, tp, shrinkFactor + 25);
                }
            }
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
        private void ColapseAllButton_Click(object sender, EventArgs e)
        {
            foreach (Control c in modTabGroups.SelectedTab.Controls)
            {
                if (c is ElementHost)
                {
                    ElementHost eh = (ElementHost)c;
                    LegacySelectionList lsl = (LegacySelectionList)eh.Child;
                    foreach (System.Windows.Controls.TreeViewItem tvi in lsl.legacyTreeView.Items)
                    {
                        tvi.IsExpanded = false;
                        if (tvi.Items.Count > 0)
                        {
                            processTreeViewItems(tvi.Items, false);
                        }
                    }
                }
            }
        }

        private void expandAllButton_Click(object sender, EventArgs e)
        {
            foreach (Control c in modTabGroups.SelectedTab.Controls)
            {
                if (c is ElementHost)
                {
                    ElementHost eh = (ElementHost)c;
                    LegacySelectionList lsl = (LegacySelectionList)eh.Child;
                    foreach (System.Windows.Controls.TreeViewItem tvi in lsl.legacyTreeView.Items)
                    {
                        tvi.IsExpanded = true;
                        if (tvi.Items.Count > 0)
                        {
                            processTreeViewItems(tvi.Items, true);
                        }
                    }
                }
            }
        }

        private void processTreeViewItems(System.Windows.Controls.ItemCollection ic, bool expand)
        {
            foreach (System.Windows.Controls.TreeViewItem tvi in ic)
            {
                if (expand)
                    tvi.IsExpanded = true;
                else
                    tvi.IsExpanded = false;
                if (tvi.Items.Count > 0)
                {
                    processTreeViewItems(tvi.Items, expand);
                }
            }
        }
        #endregion

        #region Search box code
        private void searchComboBox_TextUpdate(object sender, EventArgs e)
        {
            ComboBox searchComboBox = (ComboBox)sender;
            string filter_param = searchComboBox.Text;
            List<SelectableDatabasePackage> filteredItems = null;
            if (!String.IsNullOrWhiteSpace(filter_param))
            {
                String[] filtered_parts = filter_param.Split('*');
                //force filteredItems to be mod or first level config
                filteredItems = new List<SelectableDatabasePackage>(CompleteModSearchList);
                foreach (var f in filtered_parts)
                {
                    filteredItems = filteredItems.FindAll(x => x.NameFormatted.ToLower().Contains(f.ToLower()));
                }
            }
            if (filteredItems == null)
            {
                searchComboBox.DataSource = CompleteModSearchList;
                searchComboBox.SelectedIndex = -1;
            }
            else if (filteredItems.Count == 0)
            {
                searchComboBox.ForeColor = System.Drawing.Color.Red;
            }
            else
            {
                IgnoreSelections = false;
                searchComboBox.ForeColor = System.Drawing.Color.Black;
                searchComboBox.DataSource = filteredItems;
                searchComboBox.DroppedDown = true;
            }
            Cursor.Current = Cursors.Default;
            searchComboBox.IntegralHeight = true;
            searchComboBox.Text = filter_param;
            // set the position of the cursor
            searchComboBox.SelectionStart = filter_param.Length;
            searchComboBox.SelectionLength = 0;
        }

        private void searchCB_SelectionChangeCommitted(object sender, EventArgs e)
        {
            ComboBox sendah = (ComboBox)sender;
            if (sendah.SelectedIndex == -1 || IgnoreSelections)
            {
                return;
            }
            if (Settings.SView == Settings.SelectionView.Default)
            {
                if (sendah.SelectedItem is Mod)
                {
                    Mod m = (Mod)sendah.SelectedItem;
                    if (modTabGroups.TabPages.Contains(m.TabIndex))
                    {
                        modTabGroups.SelectedTab = m.TabIndex;
                    }
                    ModFormCheckBox c = (ModFormCheckBox)m.ModFormCheckBox;
                    c.Focus();

                }
                else if (sendah.SelectedItem is Config)
                {
                    Config c = (Config)sendah.SelectedItem;
                    if (modTabGroups.TabPages.Contains(c.ParentMod.TabIndex))
                    {
                        modTabGroups.SelectedTab = c.ParentMod.TabIndex;
                    }
                    if (c.ConfigUIComponent is ConfigFormCheckBox)
                    {
                        ConfigFormCheckBox cb = (ConfigFormCheckBox)c.ConfigUIComponent;
                        cb.Focus();
                    }
                    else if (c.ConfigUIComponent is ConfigFormComboBox)
                    {
                        ConfigFormComboBox cb = (ConfigFormComboBox)c.ConfigUIComponent;
                        cb.Focus();
                    }
                    else if (c.ConfigUIComponent is ConfigFormRadioButton)
                    {
                        //this one is the problem
                        ConfigFormRadioButton cb = (ConfigFormRadioButton)c.ConfigUIComponent;
                        cb.CheckedChanged -= configControlRB_CheckedChanged;
                        bool realChecked = cb.Checked;
                        cb.Focus();
                        cb.Checked = realChecked;
                        cb.CheckedChanged += configControlRB_CheckedChanged;
                    }
                }
            }
            else if (Settings.SView == Settings.SelectionView.Legacy)
            {
                if (sendah.SelectedItem is Mod)
                {
                    Mod m = (Mod)sendah.SelectedItem;
                    if (modTabGroups.TabPages.Contains(m.TabIndex))
                    {
                        modTabGroups.SelectedTab = m.TabIndex;
                    }
                    ModWPFCheckBox c = (ModWPFCheckBox)m.ModFormCheckBox;
                    c.Focus();
                    this.ModSelectionList_SizeChanged(null, null);
                }
                else if (sendah.SelectedItem is Config)
                {
                    Config c = (Config)sendah.SelectedItem;
                    if (modTabGroups.TabPages.Contains(c.ParentMod.TabIndex))
                    {
                        modTabGroups.SelectedTab = c.ParentMod.TabIndex;
                    }
                    System.Windows.Controls.Control con = (System.Windows.Controls.Control)c.ConfigUIComponent;
                    con.Focus();
                    this.ModSelectionList_SizeChanged(null, null);
                }
            }
        }

        private void searchCB_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                searchCB_SelectionChangeCommitted(sender, null);
            }
        }
        #endregion

        #region XML database code
        private string GetMD5Hash(string inputFile)
        {
            string hash = "";
            //get filetime from file, convert it to string with base 10
            string filetime = Convert.ToString(File.GetLastWriteTime(inputFile).ToFileTime(), 10);
            //extract filename with path
            string filename = Path.GetFileName(inputFile);
            //check database for filename with filetime
            hash = GetMd5HashDatabase(filename, filetime);
            if (hash == "-1")   //file not found in database
            {
                //create Md5Hash from file
                hash = Utils.CreateMd5Hash(inputFile);

                if (hash == "-1")
                {
                    //no file found, then delete from database
                    DeleteMd5HashDatabase(filename);
                }
                else
                {
                    //file found. update the database with new values
                    UpdateMd5HashDatabase(filename, hash, filetime);
                }
                //report back the created Hash
                return hash;
            }
            //Hash found in database
            else
            {
                //report back the stored Hash
                return hash;
            }
        }
        // need filename and filetime to check the database
        private string GetMd5HashDatabase(string inputFile, string inputFiletime)
        {
            try
            {
                bool exists = ModInfoDocument.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value.Equals(inputFile) && arg.Attribute("filetime").Value.Equals(inputFiletime))
                       .Any();
                if (exists)
                {
                    XElement element = ModInfoDocument.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value.Equals(inputFile) && arg.Attribute("filetime").Value.Equals(inputFiletime))
                       .Single();
                    return element.Attribute("md5").Value;
                }
            }
            catch (Exception e)
            {
                Utils.ExceptionLog("getMd5HashDatabase", e);
                File.Delete(Md5DatabaseFile);     // delete damaged XML database
            }
            return "-1";
        }

        private void UpdateMd5HashDatabase(string inputFile, string inputMd5Hash, string inputFiletime)
        {
            try
            {
                bool exists = ModInfoDocument.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value.Equals(inputFile))
                       .Any();
                if(exists)
                {
                    XElement element = ModInfoDocument.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value.Equals(inputFile))
                       .Single();
                    element.Attribute("filetime").Value = inputFiletime;
                    element.Attribute("md5").Value = inputMd5Hash;
                }
                else
                {
                    ModInfoDocument.Element("database").Add(new XElement("file", new XAttribute("filename", inputFile), new XAttribute("filetime", inputFiletime), new XAttribute("md5", inputMd5Hash)));
                }
            }
            catch (Exception ex)
            {
                Utils.ExceptionLog("updateMd5HashDatabase", ex);
            }
        }

        private void DeleteMd5HashDatabase(string inputFile)
        {
            // extract filename from path (if call with full path)
            string fileName = Path.GetFileName(inputFile);
            bool exists = ModInfoDocument.Descendants("file")
                       .Where(arg => arg.Attribute("filename").Value.Equals(inputFile))
                       .Any();
            if(exists)
                ModInfoDocument.Descendants("file").Where(arg => arg.Attribute("filename").Value.Equals(inputFile)).Remove();
            
        }
        #endregion
    }
}
