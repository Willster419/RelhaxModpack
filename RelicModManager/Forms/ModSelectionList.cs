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
using System.Xml;
using RelhaxModpack.UIComponents;

namespace RelhaxModpack
{
    //the mod selectin window. allows users to select which mods they wish to install
    public partial class ModSelectionList : RelhaxForum
    {
        public List<Category> ParsedCatagoryList;//can be grabbed by MainWindow
        public List<SelectablePackage> UserMods;//can be grabbed by MainWindow
        private Preview p;
        private PleaseWait pw;
        public List<Dependency> GlobalDependencies;
        public List<Dependency> Dependencies;
        public List<LogicalDependency> LogicalDependencies;
        public List<SelectablePackage> CompleteModSearchList;
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
        private double DPISCALE = 0;
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
            //get the DPI scale value
            Graphics graphics = this.CreateGraphics();
            DPISCALE = graphics.DpiX;
            DPISCALE = DPISCALE / 96F;
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
            CompleteModSearchList = new List<SelectablePackage>();
            if (modTabGroups.TabPages.Count > 0)
                modTabGroups.TabPages.Clear();
            modTabGroups.Font = Settings.AppFont;
            AddAllMods();
            AddUserMods(false);
            //check the default checked mods. afterwards, any load will have the clear selection in it so it shouldn't be an issue
            if(!Program.testMode)
                CheckDefaultMods();
            //finish loading
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
            TabIndicatesTB.Text = Translations.getTranslatedString(TabIndicatesTB.Name);
            clearSelectionsButton.Text = Translations.getTranslatedString(clearSelectionsButton.Name);
            colapseAllButton.Text = Translations.getTranslatedString(colapseAllButton.Name);
            expandAllButton.Text = Translations.getTranslatedString(expandAllButton.Name);
            searchTB.Text = Translations.getTranslatedString(searchTB.Name);
            SearchToolTip.SetToolTip(searchCB, Translations.getTranslatedString("searchToolTip"));
            SearchToolTip.SetToolTip(searchTB, Translations.getTranslatedString("searchToolTip"));
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
                Settings.TanksOnlineFolderVersion = XMLUtils.GetXMLElementAttributeFromFile(databaseURL, "//modInfoAlpha.xml/@onlineFolder");
            }
            else if (Program.betaDatabase)
            {
                Logging.Manager("downloading modInfo.dat (betaDatabase url)");
                string xmlString = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "manager_version.xml");
                XDocument doc = XDocument.Parse(xmlString);
                //parse the database version
                databaseURL = doc.XPathSelectElement("//version/database_beta_url").Value;
                Settings.TanksOnlineFolderVersion = XMLUtils.GetXMLElementAttributeFromFile(databaseURL, "//modInfoAlpha.xml/@onlineFolder");
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
            UserMods = new List<SelectablePackage>();
            foreach (string s in userModFiles)
            {
                if (Path.GetExtension(s).Equals(".zip"))
                {
                    UserMods.Add(new SelectablePackage()
                    {
                        ZipFile = s,
                        Name = Path.GetFileNameWithoutExtension(s),
                        Enabled = true
                    });
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
            //start build category selections
            foreach (Category c in ParsedCatagoryList)
            {
                TabPage t = new TabPage(c.Name)
                {
                    AutoScroll = true,
                    //link the names of catagory and tab so eithor can be searched for
                    Name = c.Name,
                };
                c.TabPage = t;
                //matched the catagory to tab
                //add to the ui every mod of that catagory
                Utils.SortModsList(c.Packages);
                
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
                        Child = lsl,
                        Dock = DockStyle.Fill
                    };
                    //apparently there is an item in there. clear it.
                    lsl.legacyTreeView.Items.Clear();
                    lsl.MouseDown += Lsl_MouseDown;
                    t.Controls.Add(host);
                }
                modTabGroups.TabPages.Add(t);
            }
            //end build category selections
            //start package zip file crc checking
            if (FirstLoad)
            {
                Prog = 0;
                foreach (Category c in ParsedCatagoryList)
                {
                    foreach (SelectablePackage m in c.Packages)
                    {
                        if (pw != null)
                        {
                            pw.loadingDescBox.Text = string.Format("{0} {1}", Translations.getTranslatedString("checkingCache"), m.Name);
                            pw.SetProgress(Prog++);
                            Application.DoEvents();
                        }
                        CheckCRC(m);
                    }
                }
            }
            //end package zip file crc checking
            //start ui building
            Prog = 0;
            pw.SetProgress(0);
            Application.DoEvents();
            foreach (Category c in ParsedCatagoryList)
            {
                foreach (SelectablePackage m in c.Packages)
                {
                    if (pw != null)
                    {
                        pw.loadingDescBox.Text = string.Format("{0} {1}", Translations.getTranslatedString("loading"), m.Name);
                        pw.SetProgress(Prog++);
                        Application.DoEvents();
                    }
                    switch (Settings.SView)
                    {
                        case Settings.SelectionView.Default:
                            AddPackage(m, c, null, null);
                            break;
                        case Settings.SelectionView.Legacy:
                            ElementHost h = (ElementHost)c.TabPage.Controls[0];
                            AddPackage(m, c, null, (LegacySelectionList)h.Child);
                            break;
                    }
                }
            }
            //end ui building
        }

        //adds all usermods to thier own userMods tab
        private void AddUserMods(bool forceUnchecked)
        {
            //make the new tab
            TabPage tb = new TabPage("User Mods")
            {
                AutoScroll = true
            };
            //add all mods to the tab page
            for (int i = 0; i < UserMods.Count; i++)
            {
                //make modCheckBox
                RelhaxUserCheckBox UserPackage = new RelhaxUserCheckBox();
                UserPackage.Package = UserMods[i];
                int yLocation = 3 + (UserPackage.Size.Height * (i));
                UserPackage.Location = new System.Drawing.Point(3, yLocation);
                UserPackage.Text = UserMods[i].Name;
                if (forceUnchecked)
                {
                    UserPackage.Checked = false;
                }
                else
                {
                    UserPackage.Checked = UserMods[i].Checked;
                }
                UserMods[i].Enabled = true;
                UserPackage.Enabled = true;
                UserPackage.CheckedChanged += UserPackage_CheckedChanged;
                tb.Controls.Add(UserPackage);
            }
            modTabGroups.TabPages.Add(tb);
        }

        private void UserPackage_CheckedChanged(object sender, EventArgs e)
        {
            RelhaxUserCheckBox cb = (RelhaxUserCheckBox)sender;
            cb.Package.Checked = cb.Checked;
        }

        private void FinishLoad()
        {
            LoadingConfig = false;
            FirstLoad = false;
            //set the size to the last closed size
            //Size = new Size(Settings.ModSelectionWidth, Settings.ModSelectionHeight);
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
                this.ParseLoadConfig();
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

        public override void OnPostLoad()
        {
            //set the size to be the orig saved size
            Size = new Size(Settings.ModSelectionWidth, Settings.ModSelectionHeight);
            //then set it to fullscreen if it was fullscreen before
            if (Settings.ModSelectionFullscreen)
            {
                WindowState = FormWindowState.Maximized;
            }
        }
        #endregion
        //checks the crc of the zip file for each md and config
        private void CheckCRC(SelectablePackage m)
        {
            //get the local md5 hash. a -1 indicates the file is not on the disk
            string oldCRC2 = GetMD5Hash(Path.Combine(Application.StartupPath, "RelHaxDownloads", m.ZipFile));
            if ((!m.ZipFile.Equals("")) && (!m.CRC.Equals(oldCRC2)))
            {
                m.DownloadFlag = true;
            }
            if(m.Packages.Count > 0)
            {
                foreach (SelectablePackage c in m.Packages)
                    CheckCRC(c);
            }
        }
        //new method for adding mods with the same code
        private void AddPackage(SelectablePackage sp, Category c, SelectablePackage parent, LegacySelectionList lsl)
        {
            //if set to show all mods at comamnd line, show them
            if (!Program.forceVisible && !sp.Visible)
                return;
            //setup the refrences
            if(sp.Level == 0)
            {
                sp.TopParent = sp;
                sp.Parent = sp;
            }
            else
            {
                sp.Parent = parent;
                sp.TopParent = parent.TopParent;
            }
            string packageDisplayName = sp.NameFormatted;
            //write if the package is forced to be visible and/or enabled
            if(Program.forceVisible)
            {
                if(!sp.Visible || !sp.Parent.Visible || !sp.TopParent.Visible)
                {
                    packageDisplayName = packageDisplayName + " [invisible]";
                    sp.Visible = true;
                }
            }
            if(Program.forceEnabled)
            {
                if(!sp.Enabled || !sp.Parent.Enabled || !sp.TopParent.Enabled)
                {
                    packageDisplayName = packageDisplayName + " [disabled]";
                    sp.Enabled = true;
                }
            }
            sp.ParentCategory = c;
            sp.TabIndex = c.TabPage;
            if(sp.Level == 0 || sp.Level == 1)
                CompleteModSearchList.Add(sp);
            //write if the package needs to be downloaded
            //if the CRC's don't match and the package actually has a zip file
            if (sp.DownloadFlag)
            {
                packageDisplayName = string.Format("{0} ({1})", packageDisplayName, Translations.getTranslatedString("updated"));
                if ((sp.Size > 0))
                    packageDisplayName = string.Format("{0} ({1})", packageDisplayName, Utils.SizeSuffix(sp.Size, 1, true));
            }
            string dateFormat = sp.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(sp.Timestamp);
            string tooltipString = sp.Description.Equals("") ? NoDescriptionAvailable : sp.Description + (sp.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
            //determine if the mod can be enabled or not, reguardless if the package is enabled or not
            bool canBeEnabled = true;
            SelectablePackage temp = sp;
            while(temp.Level > 0)
            {
                if (!temp.Enabled)
                    canBeEnabled = false;
                temp = temp.Parent;
            }
            if (!temp.Enabled)
                canBeEnabled = false;
            //special code for each type of view
            switch (Settings.SView)
            {
                case Settings.SelectionView.Default:
                    //int newPanelCount = c.TabPage.Controls.Count + 1;
                    //start code for dealing with panels
                    if (sp.Level == 0)
                    {
                        if (sp.ParentPanel == null)
                        {
                            sp.ParentPanel = new Panel()
                            {
                                BorderStyle = Settings.DisableBorders ? BorderStyle.None : BorderStyle.FixedSingle,
                                //autosize is true by default...?
                                Size = new Size(c.TabPage.Size.Width - 25, 20),
                                Location = new Point(5, GetYLocation(c.TabPage.Controls)),
                                AutoSize = true,
                                AutoSizeMode = AutoSizeMode.GrowOnly
                            };
                            sp.ParentPanel.MouseDown += DisabledComponent_MouseDown;
                            sp.ParentCategory.TabPage.Controls.Add(sp.ParentPanel);
                        }
                    }
                    else if (sp.Level > 0)
                    {
                        sp.ParentPanel = sp.Parent.ChildPanel;
                        if (sp.ParentPanel == null)
                        {
                            sp.ParentPanel = new Panel()
                            {
                                BorderStyle = Settings.DisableBorders ? BorderStyle.None : BorderStyle.FixedSingle,
                                Size = new Size(c.TabPage.Size.Width - 35, 30),
                                Location = new Point(13, GetYLocation(sp.ParentPanel.Controls)),
                                AutoSize = true,
                                AutoSizeMode = AutoSizeMode.GrowOnly
                            };
                            sp.ParentPanel.MouseDown += DisabledComponent_MouseDown;
                        }
                    }
                    //end code for dealing with panels
                    switch (sp.Type)
                    {
                        case "single":
                        case "single1":
                            sp.UIComponent = new RelhaxFormRadioButton()
                            {
                                //autosize is true by default...
                                Location = new Point(3, GetYLocation(sp.ParentPanel.Controls)),
                                //Size = new Size(150, 15),
                                Text = packageDisplayName,
                                Package = sp,
                                Enabled = canBeEnabled,
                                Checked = (canBeEnabled && sp.Checked) ? true : false,
                                AutoCheck = false,
                                AutoSize = true
                            };
                            break;
                        case "single_dropdown":
                        case "single_dropdown1":
                            if (sp.Parent.RelhaxFormComboBoxList[0] == null)
                            {
                                sp.Parent.RelhaxFormComboBoxList[0] = new RelhaxFormComboBox()
                                {
                                    //autosize should be true...
                                    //location used to determind if addedyet
                                    Location = new Point(6, GetYLocation(sp.ParentPanel.Controls)),
                                    Size = new Size((int)(225 * DPISCALE), 15),
                                    Enabled = false,
                                    Name = "notAddedYet",
                                    DropDownStyle = ComboBoxStyle.DropDownList
                                };
                                //https://stackoverflow.com/questions/1882993/c-sharp-how-do-i-prevent-mousewheel-scrolling-in-my-combobox
                                sp.Parent.RelhaxFormComboBoxList[0].MouseWheel += (o, e) => ((HandledMouseEventArgs)e).Handled = true;
                            }
                            if(sp.Enabled)
                            {
                                ComboBoxItem cbi = new ComboBoxItem(sp, packageDisplayName);
                                sp.Parent.RelhaxFormComboBoxList[0].Items.Add(cbi);
                                if(sp.Checked)
                                {
                                    //sp.Parent.RelhaxFormComboBoxList[0].Enabled = true;
                                    sp.Parent.RelhaxFormComboBoxList[0].SelectedItem = cbi;
                                    DescriptionToolTip.SetToolTip(sp.Parent.RelhaxFormComboBoxList[0], tooltipString);
                                }
                            }
                            if(sp.Parent.RelhaxFormComboBoxList[0].Name.Equals("notAddedYet"))
                            {
                                if (sp.Parent.RelhaxFormComboBoxList[0].Items.Count > 0)
                                {
                                    sp.Parent.RelhaxFormComboBoxList[0].Enabled = true;
                                    if (sp.Parent.RelhaxFormComboBoxList[0].SelectedIndex == -1)
                                        sp.Parent.RelhaxFormComboBoxList[0].SelectedIndex = 0;
                                }
                                sp.Parent.RelhaxFormComboBoxList[0].Name = "added";
                                sp.ParentPanel.Controls.Add(sp.Parent.RelhaxFormComboBoxList[0]);
                            }
                            break;
                        case "single_dropdown2":
                            if (sp.Parent.RelhaxFormComboBoxList[1] == null)
                            {
                                sp.Parent.RelhaxFormComboBoxList[1] = new RelhaxFormComboBox()
                                {
                                    //autosize should be true...
                                    //location used to determind if addedyet
                                    Location = new Point(6, GetYLocation(sp.ParentPanel.Controls)),
                                    Size = new Size((int)(225 * DPISCALE), 15),
                                    Enabled = false,
                                    Name = "notAddedYet",
                                    DropDownStyle = ComboBoxStyle.DropDownList
                                };
                                //https://stackoverflow.com/questions/1882993/c-sharp-how-do-i-prevent-mousewheel-scrolling-in-my-combobox
                                sp.Parent.RelhaxFormComboBoxList[1].MouseWheel += (o, e) => ((HandledMouseEventArgs)e).Handled = true;
                            }
                            if (sp.Enabled)
                            {
                                ComboBoxItem cbi = new ComboBoxItem(sp, packageDisplayName);
                                sp.Parent.RelhaxFormComboBoxList[1].Items.Add(cbi);
                                if (sp.Checked)
                                {
                                    //sp.Parent.RelhaxFormComboBoxList[1].Enabled = true;
                                    sp.Parent.RelhaxFormComboBoxList[1].SelectedItem = cbi;
                                    DescriptionToolTip.SetToolTip(sp.Parent.RelhaxFormComboBoxList[1], tooltipString);
                                }
                            }
                            if (sp.Parent.RelhaxFormComboBoxList[1].Name.Equals("notAddedYet"))
                            {
                                if(sp.Parent.RelhaxFormComboBoxList[1].Items.Count > 0)
                                {
                                    sp.Parent.RelhaxFormComboBoxList[1].Enabled = true;
                                    if (sp.Parent.RelhaxFormComboBoxList[1].SelectedIndex == -1)
                                        sp.Parent.RelhaxFormComboBoxList[1].SelectedIndex = 0;
                                }
                                sp.Parent.RelhaxFormComboBoxList[1].Name = "added";
                                sp.ParentPanel.Controls.Add(sp.Parent.RelhaxFormComboBoxList[1]);
                            }
                            break;
                        case "multi":
                            sp.UIComponent = new RelhaxFormCheckBox()
                            {
                                Location = new Point(3, GetYLocation(sp.ParentPanel.Controls)),
                                //Size = new Size(150, 15),
                                Text = packageDisplayName,
                                Package = sp,
                                Enabled = canBeEnabled,
                                Checked = (canBeEnabled && sp.Checked) ? true : false,
                                AutoSize = true,
                                AutoCheck = false
                            };
                            break;
                    }
                    //color change code
                    if (canBeEnabled && sp.Enabled && sp.Checked && !Settings.DisableColorChange)
                        sp.ParentPanel.BackColor = Color.BlanchedAlmond;
                    else
                        sp.ParentPanel.BackColor = Settings.getBackColor();
                    //color change code
                    //start code for handlers tooltips and attaching
                    if ((sp.UIComponent != null) && (sp.UIComponent is Control cont))
                    {
                        //take care of spacing and handlers
                        cont.MouseDown += Generic_MouseDown;
                        //ADD HANDLERS HERE
                        if(cont is RelhaxFormCheckBox FormCheckBox)
                        {
                            FormCheckBox.Click += OnMultiPackageClick;
                        }
                        else if(cont is RelhaxFormRadioButton FormRadioButton)
                        {
                            FormRadioButton.Click += OnSinglePackageClick;
                        }
                        //add tooltip and attach to display
                        DescriptionToolTip.SetToolTip(cont, tooltipString);
                        sp.ParentPanel.Controls.Add(cont);
                    }
                    //end code for handlers tooltips and attaching
                    break;
                case Settings.SelectionView.Legacy:
                    if (Settings.DarkUI)
                        lsl.legacyTreeView.Background = System.Windows.Media.Brushes.Gray;
                    //in WPF underscores are only displayed when there's two of them
                    packageDisplayName = packageDisplayName.Replace(@"_", @"__");
                    switch(sp.Type)
                    {
                        case "single":
                        case "single1":
                            sp.UIComponent = new RelhaxWPFRadioButton()
                            {
                                ToolTip = tooltipString,
                                Package = sp,
                                FontFamily = new System.Windows.Media.FontFamily(Settings.FontName),
                                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                                VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                                FontWeight = Settings.DarkUI ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal,
                                Content = packageDisplayName,
                                IsEnabled = canBeEnabled,
                                IsChecked = (canBeEnabled && sp.Checked) ? true : false
                            };
                            if (sp.Level > 0)
                            {
                                sp.TreeViewItem.Header = sp.UIComponent;
                                sp.TreeViewItem.IsExpanded = Settings.ExpandAllLegacy ? true : false;
                                sp.Parent.TreeViewItem.Items.Add(sp.TreeViewItem);
                            }
                            break;
                        case "single_dropdown":
                        case "single_dropdown1":
                            if(sp.Parent.RelhaxWPFComboBoxList[0] == null)
                                sp.Parent.RelhaxWPFComboBoxList[0] = new RelhaxWPFComboBox()
                                {
                                    IsEditable = false,
                                    Name = "notAddedYet",
                                    IsEnabled = false,
                                    FontFamily = new System.Windows.Media.FontFamily(Settings.FontName),
                                    FontWeight = Settings.DarkUI ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal,
                                    MinWidth = 100
                                };
                            //here means the entry index is not null
                            if(sp.Enabled)
                            {
                                ComboBoxItem cbi = new ComboBoxItem(sp, sp.NameFormatted);
                                sp.Parent.RelhaxWPFComboBoxList[0].Items.Add(cbi);
                                if (sp.Checked)
                                {
                                    sp.Parent.RelhaxWPFComboBoxList[0].SelectedItem = cbi;
                                    sp.Parent.RelhaxWPFComboBoxList[0].ToolTip = tooltipString;
                                }
                            }
                            if(sp.Parent.RelhaxWPFComboBoxList[0].Name.Equals("notAddedYet"))
                            {
                                sp.Parent.RelhaxWPFComboBoxList[0].Name = "added";
                                sp.Parent.RelhaxWPFComboBoxList[0].PreviewMouseRightButtonDown += Generic_MouseDown;
                                //ADD HANDLER HERE
                                if (sp.Parent.RelhaxWPFComboBoxList[0].Items.Count > 0)
                                {
                                    sp.Parent.RelhaxWPFComboBoxList[0].IsEnabled = true;
                                    if (sp.Parent.RelhaxWPFComboBoxList[0].SelectedIndex == -1)
                                        sp.Parent.RelhaxWPFComboBoxList[0].SelectedIndex = 0;
                                }
                                sp.TreeViewItem.Header = sp.Parent.RelhaxWPFComboBoxList[0];
                                sp.Parent.TreeViewItem.Items.Add(sp.TreeViewItem);
                            }
                            break;
                        case "single_dropdown2":
                            if (sp.Parent.RelhaxWPFComboBoxList[1] == null)
                                sp.Parent.RelhaxWPFComboBoxList[1] = new RelhaxWPFComboBox()
                                {
                                    IsEditable = false,
                                    Name = "notAddedYet",
                                    IsEnabled = false,
                                    FontFamily = new System.Windows.Media.FontFamily(Settings.FontName),
                                    FontWeight = Settings.DarkUI ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal,
                                    MinWidth = 100
                                };
                            //here means the entry index is not null
                            if (sp.Enabled)
                            {
                                ComboBoxItem cbi = new ComboBoxItem(sp, sp.NameFormatted);
                                sp.Parent.RelhaxWPFComboBoxList[1].Items.Add(cbi);
                                if (sp.Checked)
                                {
                                    sp.Parent.RelhaxWPFComboBoxList[1].SelectedItem = cbi;
                                    sp.Parent.RelhaxWPFComboBoxList[1].ToolTip = tooltipString;
                                }
                            }
                            if (sp.Parent.RelhaxWPFComboBoxList[1].Name.Equals("notAddedYet"))
                            {
                                sp.Parent.RelhaxWPFComboBoxList[1].Name = "added";
                                sp.Parent.RelhaxWPFComboBoxList[1].PreviewMouseRightButtonDown += Generic_MouseDown;
                                //ADD HANDLER HERE
                                if (sp.Parent.RelhaxWPFComboBoxList[1].Items.Count > 0)
                                {
                                    sp.Parent.RelhaxWPFComboBoxList[1].IsEnabled = true;
                                    if (sp.Parent.RelhaxWPFComboBoxList[1].SelectedIndex == -1)
                                        sp.Parent.RelhaxWPFComboBoxList[1].SelectedIndex = 0;
                                }
                                sp.TreeViewItem.Header = sp.Parent.RelhaxWPFComboBoxList[1];
                                sp.Parent.TreeViewItem.Items.Add(sp.TreeViewItem);
                            }
                            break;
                        case "multi":
                            sp.UIComponent = new RelhaxWPFCheckBox()
                            {
                                ToolTip = tooltipString,
                                Package = sp,
                                FontFamily = new System.Windows.Media.FontFamily(Settings.FontName),
                                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                                VerticalContentAlignment = System.Windows.VerticalAlignment.Center,
                                FontWeight = Settings.DarkUI ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal,
                                Content = packageDisplayName,
                                IsEnabled = canBeEnabled,
                                IsChecked = (canBeEnabled && sp.Checked) ? true : false,
                            };
                            if(sp.Level > 0)
                            {
                                sp.TreeViewItem.Header = sp.UIComponent;
                                sp.TreeViewItem.IsExpanded = Settings.ExpandAllLegacy ? true : false;
                                sp.Parent.TreeViewItem.Items.Add(sp.TreeViewItem);
                            }
                            break;
                    }
                    //set the font size
                    if((sp.UIComponent != null) && (sp.UIComponent is System.Windows.Controls.Control cont2))
                    {
                        switch (Settings.FontSizeforum)
                        {
                            case Settings.FontSize.Font125:
                                cont2.FontSize = cont2.FontSize + 4;
                                break;
                            case Settings.FontSize.Font175:
                                cont2.FontSize = cont2.FontSize + 8;
                                break;
                            case Settings.FontSize.Font225:
                                cont2.FontSize = cont2.FontSize + 12;
                                break;
                            case Settings.FontSize.Font275:
                                cont2.FontSize = cont2.FontSize + 16;
                                break;
                        }
                        cont2.MouseDown += Generic_MouseDown;
                        if(sp.UIComponent is System.Windows.Controls.RadioButton rb)
                        {
                            //ADD CHECKED HEADERS HERE
                            rb.Click += OnWPFComponentCheck;
                        }
                        else if(sp.UIComponent is System.Windows.Controls.CheckBox cb)
                        {
                            //AND HERE
                            cb.Click += OnWPFComponentCheck;
                        }
                    }
                    //make the root tree view item for the package and set it with the UI component
                    if(sp.Level == 0)
                    {
                        sp.TreeViewItem.Header = sp.UIComponent;
                        sp.TreeViewItem.IsExpanded = Settings.ExpandAllLegacy ? true : false;
                        lsl.legacyTreeView.Items.Add(sp.TreeViewItem);
                    }
                    break;
            }
            if(sp.Packages.Count > 0)
            {
                if(Settings.SView == Settings.SelectionView.Default && sp.ChildPanel == null)
                {
                    sp.ChildPanel = new Panel()
                    {
                        BorderStyle = Settings.DisableBorders ? BorderStyle.None : BorderStyle.FixedSingle,
                        Size = new Size(c.TabPage.Size.Width - 35, 30),
                        Location = new Point(13, GetYLocation(sp.ParentPanel.Controls)),
                        AutoSize = true,
                        AutoSizeMode = AutoSizeMode.GrowOnly
                    };
                    sp.ChildPanel.MouseDown += DisabledComponent_MouseDown;
                    sp.ParentPanel.Controls.Add(sp.ChildPanel);
                }
                foreach(SelectablePackage sp2 in sp.Packages)
                {
                    AddPackage(sp2, c, sp, lsl);
                }
            }
        }

        //generic handler to disable the auto check like in forms, but for WPF
        void OnWPFComponentCheck(object sender, System.Windows.RoutedEventArgs e)
        {
            if(sender is RelhaxWPFCheckBox cb)
            {
                if ((bool)cb.IsChecked)
                    cb.IsChecked = false;
                else if (!(bool)cb.IsChecked)
                    cb.IsChecked = true;
                OnMultiPackageClick(sender, e);
            }
            else if (sender is RelhaxWPFRadioButton rb)
            {
                if ((bool)rb.IsChecked)
                    rb.IsChecked = false;
                else if (!(bool)rb.IsChecked)
                    rb.IsChecked = true;
                OnSinglePackageClick(sender, e);
            }
        }

        //when a single/single1 mod is selected
        void OnSinglePackageClick(object sender, EventArgs e)
        {
            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = ipc.Package;
            if (!spc.Enabled || !spc.Parent.Enabled || !spc.TopParent.Enabled)
                return;
            //uncheck all packages at this level that are single
            if(spc.Level == 0)
            {
                foreach (SelectablePackage childPackage in spc.ParentCategory.Packages)
                {
                    if (childPackage.Equals(spc))
                        continue;
                    if ((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
                    {
                        childPackage.Checked = false;
                        PropagateDownNotChecked(childPackage);
                    }
                }
            }
            else
            {
                foreach (SelectablePackage childPackage in spc.Parent.Packages)
                {
                    if ((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
                    {
                        if (childPackage.Equals(spc))
                            continue;
                        childPackage.Checked = false;
                        PropagateDownNotChecked(childPackage);
                    }
                }
            }
            if(spc.Level == 0 && spc.Checked)
            {
                //allow single selection of level 0 to be turned off
                spc.Checked = false;
                PropagateUpNotChecked(spc);
                PropagateDownNotChecked(spc);
            }
            else
            {
                //check the acutal mod
                spc.Checked = true;
                //propagate up (down will be taken care of)
                //up may go down, down won't go back up
                PropagateUpChecked(spc);
                PropagateDownChecked(spc);
            }
            
        }

        //when a single_dropdown mod is selected
        void OnSingleDDPackageClick(object sender, EventArgs e)
        {
            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = ipc.Package;
            if (!spc.Enabled || !spc.Parent.Enabled || !spc.TopParent.Enabled)
                return;

        }

        //when a multi mod is selected
        void OnMultiPackageClick(object sender, EventArgs e)
        {
            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = ipc.Package;
            if (!spc.Enabled || !spc.Parent.Enabled || !spc.TopParent.Enabled)
                return;
            //can be enabled
            if(!spc.Checked)
            {
                //check it and propagate change
                spc.Checked = true;
                //up the ndown
                PropagateUpChecked(spc);
                PropagateDownChecked(spc);
            }
            else if(spc.Checked)
            {
                //uncheck it and propagate change
                spc.Checked = false;
                //up then down
                PropagateUpNotChecked(spc);
                PropagateDownNotChecked(spc);
            }
        }

        //propagates the change back up the selection tree
        //can be sent from any component
        void PropagateUpChecked(SelectablePackage spc)
        {
            spc.Parent.Checked = true;
            if (spc.Level == 0 || spc.Level == 1)
            {
                if (spc.Parent.Type.Equals("single") || spc.Parent.Type.Equals("single1"))
                {
                    foreach (SelectablePackage childPackage in spc.Parent.ParentCategory.Packages)
                    {
                        if (childPackage.Equals(spc.Parent))
                            continue;
                        if ((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
                        {
                            childPackage.Checked = false;
                            PropagateDownNotChecked(childPackage);
                        }
                    }
                }
            }
            else
            {
                if (spc.Parent.Type.Equals("single") || spc.Parent.Type.Equals("single1"))
                {
                    foreach (SelectablePackage childPackage in spc.Parent.Parent.Packages)
                    {
                        if (childPackage.Equals(spc.Parent))
                            continue;
                        if ((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
                        {
                            childPackage.Checked = false;
                            PropagateDownNotChecked(childPackage);
                        }
                    }
                }
            }
            if (spc.Parent.Level > 0)
                PropagateUpChecked(spc.Parent);
        }

        //propagates the change back up the selection tree
        //NOTE: the only component that can propagate up for a not checked is a multi
        void PropagateUpNotChecked(SelectablePackage spc)
        {
            if (spc.Level == 0)
                return;
            //if nothing cheched at this level, uncheck the parent and propagate up not checked agailn
            bool anythingChecked = false;
            foreach(SelectablePackage childPackage in spc.Parent.Packages)
            {
                if (childPackage.Enabled && childPackage.Checked)
                    anythingChecked = true;
            }
            if(!anythingChecked)
            {
                spc.Parent.Checked = false;
                PropagateUpNotChecked(spc.Parent);
            }
        }

        //propagaetes the change down the selection tree
        void PropagateDownChecked(SelectablePackage spc)
        {
            bool singleSelected = false;
            foreach(SelectablePackage childPackage in spc.Packages)
            {
                if((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
                {
                    if(!singleSelected)
                    {
                        childPackage.Checked = true;
                        singleSelected = true;
                        PropagateDownChecked(childPackage);
                    }
                    else
                    {
                        childPackage.Checked = false;
                    }
                }
                //TODO: dropdown options
            }
        }

        //propagaetes the change down the selection tree
        void PropagateDownNotChecked(SelectablePackage spc)
        {
            foreach(SelectablePackage childPackage in spc.Packages)
            {
                if (!childPackage.Enabled)
                    continue;
                childPackage.Checked = false;
                //TODO: dropdown options
                PropagateDownNotChecked(childPackage);
            }
        }

        /*
        //when a legacy mod checkbox is clicked
        void modCheckBoxL_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LoadingConfig)
                return;
            ModWPFCheckBox cb = (ModWPFCheckBox)sender;
            SelectablePackage m = cb.mod;
            Category cat = m.ParentCategory;
            System.Windows.Controls.TreeViewItem TVI = (System.Windows.Controls.TreeViewItem)cb.Parent;
            System.Windows.Controls.TreeView TV = (System.Windows.Controls.TreeView)TVI.Parent;
            //check to see if this is a single selection categtory
            //if it is, then uncheck the other mod, then check this one
            if ((bool)cb.IsChecked && cat.SelectionType.Equals("single"))
            {
                //check if any other mods in this catagory are already checked
                bool anyModsChecked = false;
                foreach (SelectablePackage mm in cat.Packages)
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
            if (m.Packages.Count == 0)
                return;
            if (m.Checked)
            {
                //mod checked, check at least one single1, one single_dropdown1, one single_dropdown2
                //checking for single/single1 configs
                bool configSelected = false;
                foreach (SelectablePackage con in m.Packages)
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
                    foreach (SelectablePackage con in m.Packages)
                    {
                        if ((!con.Visible) || (!con.Enabled))
                            continue;
                        if ((con.Type.Equals("single")) || (con.Type.Equals("single1")))
                        {
                            con.Checked = true;
                            ConfigWPFRadioButton cwpfrb = (ConfigWPFRadioButton)con.UIComponent;
                            cwpfrb.IsChecked = true;
                            break;
                        }
                    }
                }
                //checking for single_dropdown/single_dropdown1 configs
                configSelected = false;
                foreach (SelectablePackage con in m.Packages)
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
                    foreach (SelectablePackage con in m.Packages)
                    {
                        if ((!con.Visible) || (!con.Enabled))
                            continue;
                        if ((con.Type.Equals("single_dropdown")) || (con.Type.Equals("single_dropdown1")))
                        {
                            con.Checked = true;
                            ConfigWPFComboBox cwpfcb = (ConfigWPFComboBox)con.UIComponent;
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
                foreach (SelectablePackage con in m.Packages)
                {
                    if ((!con.Visible) || (!con.Enabled) || (!con.Type.Equals("single_dropdown2")))
                        continue;
                    if (con.Checked)
                        configSelected = true;
                }
                if (!configSelected)
                {
                    foreach (SelectablePackage con in m.Packages)
                    {
                        if ((!con.Visible) || (!con.Enabled) || (!con.Type.Equals("single_dropdown2")))
                            continue;
                        con.Checked = true;
                        ConfigWPFComboBox cwpfcb = (ConfigWPFComboBox)con.UIComponent;
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
                foreach (SelectablePackage cfg in m.Packages)
                {
                    if (cfg.Enabled)
                    {
                        cfg.Checked = false;
                        // if (cfg.configUIComponent is ConfigFormCheckBox)
                        if (cfg.UIComponent is ConfigWPFCheckBox)
                        {
                            // ConfigFormCheckBox cfcb = (ConfigFormCheckBox)cfg.configUIComponent;
                            ConfigWPFCheckBox cfcb = (ConfigWPFCheckBox)cfg.UIComponent;
                            // cfcb.Checked = false;
                            cfcb.IsChecked = false;
                        }
                        // else if (cfg.configUIComponent is ConfigFormRadioButton)
                        else if (cfg.UIComponent is ConfigWPFRadioButton) 
                        {
                            // ConfigFormRadioButton cfrb = (ConfigFormRadioButton)cfg.configUIComponent;
                            ConfigWPFRadioButton cfrb = (ConfigWPFRadioButton)cfg.UIComponent;
                            // cfrb.Checked = false;
                            cfrb.IsChecked = false;
                        }
                        // else if (cfg.configUIComponent is ConfigFormComboBox)
                        else if (cfg.UIComponent is ConfigWPFComboBox) 
                        {
                            // ConfigFormComboBox cfcb = (ConfigFormComboBox)cfg.configUIComponent;
                            ConfigWPFComboBox cfcb = (ConfigWPFComboBox)cfg.UIComponent;
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
            SelectablePackage m = cb.mod;
            SelectablePackage cfg = cb.config;
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
            if (cfg.Packages.Count > 0 && cfg.Checked)
            {
                //determine if at least one radioButton is checked
                foreach (System.Windows.Controls.TreeViewItem subTVI in tvi.Items)
                {
                    if (subTVI.Header is ConfigWPFRadioButton)
                    {
                        radioButtonCount++;
                        ConfigWPFRadioButton subRB = (ConfigWPFRadioButton)subTVI.Header;
                        SelectablePackage subc = subRB.config;
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
                            SelectablePackage subc = subRB.config;
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
            else if (cfg.Packages.Count > 0 && !cfg.Checked)
            {
                foreach (SelectablePackage c in cfg.Packages)
                {
                    if (c.Type.Equals("single") || c.Type.Equals("single1") || c.Type.Equals("multi"))
                    {
                        c.Checked = false;
                        if (c.UIComponent is ConfigWPFCheckBox)
                        {
                            ConfigWPFCheckBox tempCB = (ConfigWPFCheckBox)c.UIComponent;
                            tempCB.IsChecked = false;
                        }
                        else if (c.UIComponent is ConfigWPFRadioButton)
                        {
                            ConfigWPFRadioButton tempCB = (ConfigWPFRadioButton)c.UIComponent;
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
            SelectablePackage m = cb.mod;
            SelectablePackage cfg = cb.config;
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
            if (cfg.Packages.Count > 0 && cfg.Checked)
            {
                //determine if at least one radioButton is checked
                foreach (System.Windows.Controls.TreeViewItem subTVI in tvi.Items)
                {
                    if (subTVI.Header is ConfigWPFRadioButton)
                    {
                        radioButtonCount++;
                        ConfigWPFRadioButton subRB = (ConfigWPFRadioButton)subTVI.Header;
                        SelectablePackage subc = subRB.config;
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
                            SelectablePackage subc = subRB.config;
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
            else if (cfg.Packages.Count > 0 && !cfg.Checked)
            {
                foreach (SelectablePackage c in cfg.Packages)
                {
                    if (c.Type.Equals("single") || c.Type.Equals("single1") || c.Type.Equals("multi"))
                    {
                        c.Checked = false;
                        if (c.UIComponent is ConfigWPFCheckBox)
                        {
                            ConfigWPFCheckBox tempCB = (ConfigWPFCheckBox)c.UIComponent;
                            tempCB.IsChecked = false;
                        }
                        else if (c.UIComponent is ConfigWPFRadioButton)
                        {
                            ConfigWPFRadioButton tempCB = (ConfigWPFRadioButton)c.UIComponent;
                            tempCB.IsChecked = false;
                        }
                    }
                }
            }
        }
        */
        //method for finding the location of which to put a control
        private int GetYLocation(System.Windows.Forms.Control.ControlCollection ctrl)
        {
            int y = 0;
            //the first 5 pixels to give it room
            y += 5;
            //only look for the dropDown menu options or checkboxes
            foreach (Control c in ctrl)
            {
                if ((c is RelhaxFormCheckBox) || (c is RelhaxFormComboBox) || (c is Panel) || (c is RelhaxFormRadioButton))
                {
                    y += c.Size.Height;
                    //spacing
                    y += 2;
                }
                if (c is RelhaxFormCheckBox)
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
        /*
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
            SelectablePackage m = cb.mod;
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
                foreach (SelectablePackage mm in cat.Packages)
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
            if (m.Packages.Count == 0)
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
                foreach (SelectablePackage con in m.Packages)
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
                    foreach (SelectablePackage con in m.Packages)
                    {
                        if ((!con.Visible) || (!con.Enabled))
                            continue;
                        if ((con.Type.Equals("single")) || (con.Type.Equals("single1")))
                        {
                            con.Checked = true;
                            ConfigFormRadioButton cfrb = (ConfigFormRadioButton)con.UIComponent;
                            cfrb.Checked = true;
                            break;
                        }
                    }
                }
                //checking for single_dropdown/single_dropdown1 configs
                configSelected = false;
                foreach (SelectablePackage con in m.Packages)
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
                    foreach (SelectablePackage con in m.Packages)
                    {
                        if ((!con.Visible) || (!con.Enabled))
                            continue;
                        if ((con.Type.Equals("single_dropdown")) || (con.Type.Equals("single_dropdown1")))
                        {
                            con.Checked = true;
                            ConfigFormComboBox cfcb = (ConfigFormComboBox)con.UIComponent;
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
                foreach (SelectablePackage con in m.Packages)
                {
                    if ((!con.Visible) || (!con.Enabled) || (!con.Type.Equals("single_dropdown2")))
                        continue;
                    if (con.Checked)
                        configSelected = true;
                }
                if (!configSelected)
                {
                    foreach (SelectablePackage con in m.Packages)
                    {
                        if ((!con.Visible) || (!con.Enabled) || (!con.Type.Equals("single_dropdown2")))
                            continue;
                        con.Checked = true;
                        ConfigFormComboBox cfcb = (ConfigFormComboBox)con.UIComponent;
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
                foreach (SelectablePackage cfg in m.Packages)
                {
                    if (cfg.Enabled)
                    {
                        cfg.Checked = false;
                        if (cfg.UIComponent is ConfigFormCheckBox)
                        {
                            ConfigFormCheckBox cfcb = (ConfigFormCheckBox)cfg.UIComponent;
                            cfcb.Checked = false;
                        }
                        else if (cfg.UIComponent is ConfigFormRadioButton)
                        {
                            ConfigFormRadioButton cfrb = (ConfigFormRadioButton)cfg.UIComponent;
                            cfrb.Checked = false;
                        }
                        else if (cfg.UIComponent is ConfigFormComboBox)
                        {
                            ConfigFormComboBox cfcb = (ConfigFormComboBox)cfg.UIComponent;
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
            SelectablePackage m = cb.mod;
            SelectablePackage cfg = cb.config;
            Category cat = cb.catagory;
            Panel configPanel = (Panel)cb.Parent;
            //checkbox is Enabled, toggle checked and checked
            cfg.Checked = cb.Checked;
            //propagate the check back up if required
#error will not work
            if (cfg.Checked)
            {
                SelectablePackage obj = cfg.Parent;
                if (obj is SelectablePackage)
                {
                    SelectablePackage parentM = (SelectablePackage)obj;
                    if (parentM.TopParentUIComponent is ModFormCheckBox)
                    {
                        ModFormCheckBox tempCB = (ModFormCheckBox)parentM.TopParentUIComponent;
                        if (!tempCB.Checked)
                            tempCB.Checked = true;
                    }
                }
                else if (obj is SelectablePackage)
                {
                    SelectablePackage parentC = (SelectablePackage)obj;
                    parentC.Checked = true;
                    if (parentC.UIComponent is ConfigFormCheckBox)
                    {
                        ConfigFormCheckBox parentCB = (ConfigFormCheckBox)parentC.UIComponent;
                        if (!parentCB.Checked)
                            parentCB.Checked = true;
                    }
                    else if (parentC.UIComponent is ConfigFormRadioButton)
                    {
                        ConfigFormRadioButton parentRB = (ConfigFormRadioButton)parentC.UIComponent;
                        if (!parentRB.Checked)
                            parentRB.Checked = true;
                    }
                }
            }
            //process any subconfigs
            bool configSelected = false;
            int radioButtonCount = 0;
            if (cfg.Packages.Count > 0 && cb.Checked)
            {
                foreach (SelectablePackage c in cfg.Packages)
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
                    foreach (SelectablePackage c in cfg.Packages)
                    {
                        if ((c.Type.Equals("single") || c.Type.Equals("single1")) && c.Enabled)
                        {
                            c.Checked = true;
                            ConfigFormRadioButton subRB = (ConfigFormRadioButton)c.UIComponent;
                            subRB.Checked = true;
                            break;
                        }
                    }
                }
            }
            else if (cfg.Packages.Count > 0 && !cb.Checked)
            {
                foreach (SelectablePackage c in cfg.Packages)
                {
                    if (c.Type.Equals("single") || c.Type.Equals("single1") || c.Type.Equals("multi"))
                    {
                        c.Checked = false;
                        if (c.UIComponent is ConfigFormCheckBox)
                        {
                            ConfigFormCheckBox tempCB = (ConfigFormCheckBox)c.UIComponent;
                            tempCB.Checked = false;
                        }
                        else if (c.UIComponent is ConfigFormRadioButton)
                        {
                            ConfigFormRadioButton tempCB = (ConfigFormRadioButton)c.UIComponent;
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
                SelectablePackage obj = cbi22.config.Parent;
#error will not work
                if (obj is SelectablePackage)
                {
                    SelectablePackage parentM = (SelectablePackage)obj;
                    if (parentM.TopParentUIComponent is ModFormCheckBox)
                    {
                        ModFormCheckBox tempCB = (ModFormCheckBox)parentM.TopParentUIComponent;
                        //adding the if statement prevents it from running again when it's not needed to
                        if (!tempCB.Checked)
                            tempCB.Checked = true;
                    }
                }
                else if (obj is SelectablePackage)
                {
                    SelectablePackage parentC = (SelectablePackage)obj;
                    parentC.Checked = true;
                    if (parentC.UIComponent is ConfigFormCheckBox)
                    {
                        ConfigFormCheckBox parentCB = (ConfigFormCheckBox)parentC.UIComponent;
                        if (!parentCB.Checked)
                            parentCB.Checked = true;
                    }
                    else if (parentC.UIComponent is ConfigFormRadioButton)
                    {
                        ConfigFormRadioButton parentRB = (ConfigFormRadioButton)parentC.UIComponent;
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
            SelectablePackage m = rb.mod;
            SelectablePackage cfg = rb.config;
            Category cat = rb.catagory;
            cfg.Checked = rb.Checked;
            //propagate the check back up if required
#error will not work
            if (cfg.Checked)
            {
                SelectablePackage obj = cfg.Parent;
                if (obj is SelectablePackage)
                {
                    SelectablePackage parentM = (SelectablePackage)obj;
                    if (parentM.TopParentUIComponent is ModFormCheckBox)
                    {
                        ModFormCheckBox tempCB = (ModFormCheckBox)parentM.TopParentUIComponent;
                        if (!tempCB.Checked)
                            tempCB.Checked = true;
                    }
                }
                else if (obj is SelectablePackage)
                {
                    SelectablePackage parentC = (SelectablePackage)obj;
                    parentC.Checked = true;
                    if (parentC.UIComponent is ConfigFormCheckBox)
                    {
                        ConfigFormCheckBox parentCB = (ConfigFormCheckBox)parentC.UIComponent;
                        if (!parentCB.Checked)
                            parentCB.Checked = true;
                    }
                    else if (parentC.UIComponent is ConfigFormRadioButton)
                    {
                        ConfigFormRadioButton parentRB = (ConfigFormRadioButton)parentC.UIComponent;
                        if (!parentRB.Checked)
                            parentRB.Checked = true;
                    }
                }
            }
            //propagate the change back down if required
            bool configSelected = false;
            int radioButtonCount = 0;
            if (cfg.Packages.Count > 0 && rb.Checked)
            {
                //configs present and the radio button is checked
                //singles - at lease one must be selected
                foreach (SelectablePackage c in cfg.Packages)
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
                    foreach (SelectablePackage c in cfg.Packages)
                    {
                        if ((c.Type.Equals("single") || c.Type.Equals("single1")) && c.Enabled)
                        {
                            c.Checked = true;
                            ConfigFormRadioButton subRB = (ConfigFormRadioButton)c.UIComponent;
                            subRB.Checked = true;
                            break;
                        }
                    }
                }
            }
            else if (cfg.Packages.Count > 0 && !rb.Checked)
            {
                //configs present and the radio button is not checked
                //singles - uncheck all of them
                foreach (SelectablePackage c in cfg.Packages)
                {
                    if (c.Type.Equals("single") || c.Type.Equals("single1") || c.Type.Equals("multi"))
                    {
                        c.Checked = false;
                        if (c.UIComponent is ConfigFormCheckBox)
                        {
                            ConfigFormCheckBox tempCB = (ConfigFormCheckBox)c.UIComponent;
                            tempCB.Checked = false;
                        }
                        else if (c.UIComponent is ConfigFormRadioButton)
                        {
                            ConfigFormRadioButton tempCB = (ConfigFormRadioButton)c.UIComponent;
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
        */

        #region Preview Code
        //generic hander for when any mouse button is clicked for MouseDown Events
        void Generic_MouseDown(object sender, EventArgs e)
        {
            if (e is MouseEventArgs m)
                if (m.Button != MouseButtons.Right)
                    return;
            if(sender is IPackageUIComponent ipc)
            {
                SelectablePackage spc = ipc.Package;
                if (spc.DevURL == null)
                    spc.DevURL = "";
                if (p != null)
                {
                    p.Close();
                    p.Dispose();
                    p = null;
                    GC.Collect();
                }
                p = new Preview()
                {
                    LastUpdated = LastUpdated,
                    DBO = spc,
                    Medias = spc.PictureList
                };
                p.Show();
            }
                /*
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
                */
        }
        
        private void DisabledComponent_MouseDown(object sender, MouseEventArgs e)
        {
            if (!(e.Button == MouseButtons.Right))
                return;
            Panel configPanel = (Panel)sender;
            Control c = configPanel.GetChildAtPoint(e.Location);
            if(c is IPackageUIComponent ui)
            {
                Generic_MouseDown(ui, null);
            }
        }
        
        //Handler for allowing right click of disabled mods
        private void Lsl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.RightButton != System.Windows.Input.MouseButtonState.Pressed)
            {
                return;
            }
            LegacySelectionList lsl = (LegacySelectionList)sender;
            if (e.OriginalSource is System.Windows.Controls.ContentPresenter cp)
            {
                if(cp.Content is IPackageUIComponent ipc)
                {
                    if(!(ipc.Package == null) && !ipc.Package.Enabled)
                    {
                        //disabled component, display via generic handler
                        Generic_MouseDown(ipc, null);
                    }
                }
            }
        }
        #endregion

        #region Config and selection code
        private void ClearSelectionsButton_Click(object sender, EventArgs e)
        {
            //not actually *loading* a config, but want to disable the handlers anyways
            LoadingConfig = true;
            Logging.Manager("clearSelectionsButton pressed, clearing selections");
            Utils.ClearSelectionMemory(ParsedCatagoryList, UserMods);
            //dispose of not needed stuff and reload the UI

            LoadingConfig = false;
            MessageBox.Show(Translations.getTranslatedString("selectionsCleared"));
            //ModSelectionList_SizeChanged(null, null);
        }

        private void ParseLoadConfig()
        {
            //disable any possible UI interaction that would not be desired
            LoadingConfig = true;
            //string filePath = "";
            string[] filePathArray = new string[2];
            //get the filePath of the selection file based on the mode of loading it
            switch (LoadMode)
            {
                case LoadConfigMode.FromAutoInstall:
                    filePathArray[0] = Path.Combine(Application.StartupPath, "RelHaxUserConfigs", Program.configName);
                    if (!File.Exists(filePathArray[0]))
                    {
                        Logging.Manager(string.Format("ERROR: {0} not found, not loading configs", filePathArray[0]));
                        MessageBox.Show(Translations.getTranslatedString("configLoadFailed"), Translations.getTranslatedString("critical"), MessageBoxButtons.OK, MessageBoxIcon.Stop);
                        LoadingConfig = false;
                        return;
                    }
                    break;
                case LoadConfigMode.FromSaveLastConfig:
                    filePathArray[0] = Path.Combine(Application.StartupPath, "RelHaxUserConfigs", "lastInstalledConfig.xml");
                    if (!File.Exists(filePathArray[0]))
                    {
                        Logging.Manager(string.Format("ERROR: {0} not found, not loading configs", filePathArray[0]));
                        LoadingConfig = false;
                        return;
                    }
                    break;
                default:
                    //use a selection viewer for selecting a dev config or a user config (local file)
                    using (SelectionViewer sv = new SelectionViewer(this.Location.X + 100, this.Location.Y + 100, Settings.ModInfoDatFile))
                    {
                        if (!(sv.ShowDialog() == DialogResult.OK))
                        {
                            LoadingConfig = false;
                            return;
                        }
                        //from preset:
                        //"F:\\Tanks Stuff\\RelicModManager\\RelicModManager\\bin\\Debug\\RelHaxTemp\\modInfo.dat,dirty20067.xml"
                        filePathArray[0] = sv.SelectedXML.Split(',')[0];//the path to modInfoDat
                        if (!filePathArray[0].Equals("localFile"))
                            filePathArray[1] = sv.SelectedXML.Split(',')[1];//the actual fileName
                        else
                            filePathArray[1] = "";
                        if (filePathArray[0].Equals("localFile"))
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
                                    LoadingConfig = false;
                                    return;
                                }
                                filePathArray[0] = loadLocation.FileName;
                            }
                        }
                    }
                    break;
            }
            //actually load the config
            XMLUtils.LoadConfig(LoadMode == LoadConfigMode.FromButton, filePathArray, ParsedCatagoryList, UserMods);
            //if it was from a button, tell the user it loaded the config sucessfully
            if (LoadMode == LoadConfigMode.FromButton)
            {
                if (LoadMode == LoadConfigMode.FromButton) MessageBox.Show(Translations.getTranslatedString("prefrencesSet"), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                ModSelectionList_SizeChanged(null, null);
            }
            //set this back to false so the user can interact
            LoadingConfig = false;
        }
        //handles checking of the "default checked" mods
        private void CheckDefaultMods()
        {
            Logging.Manager("Checking default mods");
            //create XmlDocument
            string xmlstring = Utils.GetStringFromZip(Settings.ManagerInfoDatFile, "default_checked.xml");
            XmlDocument doc = new XmlDocument();
            if (string.IsNullOrWhiteSpace(xmlstring))
            {
                Logging.Manager("ERROR: default_checked.xml is missing or invalid!");
                return;
            }
            doc.LoadXml(xmlstring);
            //call XMlUtils.LoadConfigV2
            XMLUtils.LoadConfigV2(doc, ParsedCatagoryList, UserMods, true);
            Logging.Manager("Finished checking default mods");
        }
        #endregion

        #region UI event handlers (resize, button press, expand toggling)
        //handler to set the cancel bool to false
        private void ContinueButton_Click(object sender, EventArgs e)
        {
            //save the last config if told to do so
            if (Settings.SaveLastConfig)
            {
                XMLUtils.SaveConfig(false, null, ParsedCatagoryList, UserMods);
            }
            DialogResult = DialogResult.OK;
        }
        //handler for when the cancal button is clicked
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        //handler for when the "load config" button is pressed
        private void LoadConfigButton_Click(object sender, EventArgs e)
        {
            LoadMode = LoadConfigMode.FromButton;
            this.ParseLoadConfig();
        }
        //handler for when the "save config" button is pressed
        private void SaveConfigButton_Click(object sender, EventArgs e)
        {
            XMLUtils.SaveConfig(true, null, ParsedCatagoryList, UserMods);
        }
        //handler for when the close button is pressed
        private void ModSelectionList_FormClosing(object sender, FormClosingEventArgs e)
        {
            //put the taskbar back if we need to
            if (TaskBarHidden)
                Settings.SetTaskbarState(Settings.AppBarStates.AutoHide);
            //save wether the window was in fullscreen mode before closing
            //also only save the size if the window is normal
            switch (WindowState)
            {
                case FormWindowState.Maximized:
                    Settings.ModSelectionFullscreen = true;
                    break;
                case FormWindowState.Minimized:
                    Settings.ModSelectionFullscreen = false;
                    break;
                case FormWindowState.Normal:
                    Settings.ModSelectionHeight = Size.Height;
                    Settings.ModSelectionWidth = Size.Width;
                    Settings.ModSelectionFullscreen = false;
                    break;
            }
            //close the preview window if it is open
            if (p != null)
            {
                p.Close();
                p.Dispose();
                p = null;
            }
        }
        //resizing handler for the window
        private void ModSelectionList_SizeChanged(object sender, EventArgs e)
        {
            if (!this.Visible)
                return;
            if (this.WindowState == FormWindowState.Minimized)
                return;
            WindowSizeTB.Text = "" + this.Size.Width + " x " + this.Size.Height;
            foreach (TabPage t in modTabGroups.TabPages)
            {
                foreach (Control c in t.Controls)
                {
                    //mod panel
                    if (c is Panel p)
                    {
                        //resizePanel(p, t, 0);//PAD
                        resizePanel(p, t, 25);//SIZE
                    }
                    else if (c is ElementHost eh)
                    {
                        eh.Dock = DockStyle.Fill;
                    }
                }
            }
        }
        //recursive resize of the control panals
        private void resizePanel(Panel current, TabPage tp, int shrinkFactor)
        {
            current.Size = new Size(tp.Size.Width - shrinkFactor, current.Size.Height);//SIZE
            //current.Padding = new Padding(shrinkFactor, current.Padding.Top, shrinkFactor, current.Padding.Bottom);//PAD
            foreach (Control controfds in current.Controls)
            {
                if (controfds is Panel subpp)
                {
                    //resizePanel(subpp, tp, 5);//PAD
                    resizePanel(subpp, tp, shrinkFactor + 25);//SIZE
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
            List<SelectablePackage> filteredItems = null;
            if (!String.IsNullOrWhiteSpace(filter_param))
            {
                String[] filtered_parts = filter_param.Split('*');
                //force filteredItems to be mod or first level config
                filteredItems = new List<SelectablePackage>(CompleteModSearchList);
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
            /*
            ComboBox sendah = (ComboBox)sender;
            if (sendah.SelectedIndex == -1 || IgnoreSelections)
            {
                return;
            }
            if (Settings.SView == Settings.SelectionView.Default)
            {
#error will not work anymore
                if (sendah.SelectedItem is SelectablePackage)
                {
                    SelectablePackage m = (SelectablePackage)sendah.SelectedItem;
                    if (modTabGroups.TabPages.Contains(m.TabIndex))
                    {
                        modTabGroups.SelectedTab = m.TabIndex;
                    }
                    ModFormCheckBox c = (ModFormCheckBox)m.TopParentUIComponent;
                    c.Focus();

                }
                else if (sendah.SelectedItem is SelectablePackage)
                {
                    SelectablePackage c = (SelectablePackage)sendah.SelectedItem;
                    if (modTabGroups.TabPages.Contains(c.TopParent.TabIndex))
                    {
                        modTabGroups.SelectedTab = c.TopParent.TabIndex;
                    }
                    if (c.UIComponent is ConfigFormCheckBox)
                    {
                        ConfigFormCheckBox cb = (ConfigFormCheckBox)c.UIComponent;
                        cb.Focus();
                    }
                    else if (c.UIComponent is ConfigFormComboBox)
                    {
                        ConfigFormComboBox cb = (ConfigFormComboBox)c.UIComponent;
                        cb.Focus();
                    }
                    else if (c.UIComponent is ConfigFormRadioButton)
                    {
                        //this one is the problem
                        ConfigFormRadioButton cb = (ConfigFormRadioButton)c.UIComponent;
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
#error will not work
                if (sendah.SelectedItem is SelectablePackage)
                {
                    SelectablePackage m = (SelectablePackage)sendah.SelectedItem;
                    if (modTabGroups.TabPages.Contains(m.TabIndex))
                    {
                        modTabGroups.SelectedTab = m.TabIndex;
                    }
                    ModWPFCheckBox c = (ModWPFCheckBox)m.TopParentUIComponent;
                    c.Focus();
                    this.ModSelectionList_SizeChanged(null, null);
                }
                else if (sendah.SelectedItem is SelectablePackage)
                {
                    SelectablePackage c = (SelectablePackage)sendah.SelectedItem;
                    if (modTabGroups.TabPages.Contains(c.TopParent.TabIndex))
                    {
                        modTabGroups.SelectedTab = c.TopParent.TabIndex;
                    }
                    System.Windows.Controls.Control con = (System.Windows.Controls.Control)c.UIComponent;
                    con.Focus();
                    this.ModSelectionList_SizeChanged(null, null);
                }
            }
            */
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
