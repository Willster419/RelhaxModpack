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
using System.Text;

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
        bool IgnoreSearchBoxFocus = false;
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
                using (WebClient downloader = new WebClient() {  })
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
                using (WebClient downloader = new WebClient() {  })
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
                    SelectablePackage sp = new SelectablePackage()
                    {
                        ZipFile = s,
                        Name = Path.GetFileNameWithoutExtension(s),
                        Enabled = true,
                        Level = 0
                    };
                    sp.Parent = sp;
                    sp.TopParent = sp;
                    UserMods.Add(sp);
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
            TreeView tv = null;
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
                //make the holder for the entire category list
                c.CategoryHeader = new SelectablePackage()
                {
                    Name = "---------[" + c.Name + "]---------",
                    TabIndex = c.TabPage,
                    ParentCategory = c,
                    Type = "multi",
                    Visible = true,
                    Enabled = true,
                    Level = -1,//because it's technically the category
                    PackageName = "Category_" + c.Name.Replace(' ', '_') + "_header"
                };
                c.CategoryHeader.Parent = c.CategoryHeader;
                c.CategoryHeader.TopParent = c.CategoryHeader;
                LegacySelectionList lsl = null;
                switch(Settings.SView)
                {
                    case Settings.SelectionView.Default:
                        RelhaxFormCheckBox cb = new RelhaxFormCheckBox()
                        {
                            Package = c.CategoryHeader,
                            Text = c.CategoryHeader.NameFormatted,
                            AutoSize = true,
                            AutoCheck = false
                        };
                        c.CategoryHeader.UIComponent = cb;
                        c.CategoryHeader.ParentUIComponent = cb;
                        c.CategoryHeader.TopParentUIComponent = cb;
                        c.CategoryHeader.Packages = c.Packages;
                        c.CategoryHeader.ParentPanel = new Panel()
                        {
                            BorderStyle = Settings.DisableBorders ? BorderStyle.None : BorderStyle.FixedSingle,
                            //autosize is true by default...?
                            Size = new Size(c.TabPage.Size.Width - 25, 60),
                            Location = new Point(5, GetYLocation(c.TabPage.Controls)),
                            AutoSize = true,
                            AutoSizeMode = AutoSizeMode.GrowOnly
                        };
                        c.CategoryHeader.ParentPanel.Controls.Add(cb);
                        c.TabPage.Controls.Add(c.CategoryHeader.ParentPanel);
                        cb.Click += OnMultiPackageClick;
                        break;
                    case Settings.SelectionView.Legacy:
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
                        RelhaxWPFCheckBox cb2 = new RelhaxWPFCheckBox()
                        {
                            Package = c.CategoryHeader,
                            Content = c.CategoryHeader.NameFormatted
                        };
                        c.CategoryHeader.UIComponent = cb2;
                        c.CategoryHeader.ParentUIComponent = cb2;
                        c.CategoryHeader.TopParentUIComponent = cb2;
                        c.CategoryHeader.Packages = c.Packages;
                        if (Settings.DarkUI)
                            lsl.legacyTreeView.Background = System.Windows.Media.Brushes.Gray;
                        c.CategoryHeader.TreeViewItem.Header = c.CategoryHeader.UIComponent;
                        c.CategoryHeader.TreeViewItem.IsExpanded = Settings.ExpandAllLegacy ? true : false;
                        lsl.legacyTreeView.Items.Add(c.CategoryHeader.TreeViewItem);
                        cb2.Click += OnWPFComponentCheck;
                        break;
                    case Settings.SelectionView.LegacyV2:
                        //make the treeview
                        tv = new TreeView()
                        {
                            Location = new Point(5, 5),
                            Size = new Size(t.Size.Width - 5 - 5, t.Size.Height - 5 - 5),
                            Dock = DockStyle.Fill,
                            DrawMode = TreeViewDrawMode.OwnerDrawText
                        };
                        tv.DrawNode += Tv_DrawNode;
                        tv.BeforeCollapse += Tv_BeforeCollapse;
                        c.TreeView = tv;
                        RelhaxFormCheckBox cbv2 = new RelhaxFormCheckBox()
                        {
                            Package = c.CategoryHeader,
                            Text = c.CategoryHeader.NameFormatted,
                            AutoSize = true,
                            AutoCheck = false
                        };
                        c.CategoryHeader.UIComponent = cbv2;
                        c.CategoryHeader.ParentUIComponent = cbv2;
                        c.CategoryHeader.TopParentUIComponent = cbv2;
                        c.CategoryHeader.Packages = c.Packages;
                        c.CategoryHeader.TreeNode.Category = c;
                        //add to the tree
                        c.CategoryHeader.TreeNode.Component = c.CategoryHeader.UIComponent;
                        cbv2.Click += OnMultiPackageClick;
                        tv.Nodes.Add(c.CategoryHeader.TreeNode);
                        c.TreeNodes.Add(c.CategoryHeader.TreeNode);
                        Control cont = (Control)c.CategoryHeader.UIComponent;
                        tv.Controls.Add(cont);
                        cont.Show();
                        t.Controls.Add(tv);
                        break;
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
                            pw.loadingDescBox.Text = string.Format("{0} {1}", Translations.getTranslatedString("checkingCache"), m.NameFormatted);
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
                        pw.loadingDescBox.Text = string.Format("{0} {1}", Translations.getTranslatedString("loading"), m.NameFormatted);
                        pw.SetProgress(Prog++);
                        Application.DoEvents();
                    }
                    AddPackage(m, c, c.CategoryHeader);
                    if (Settings.ExpandAllLegacy2)
                    {
                        m.TreeNode.ExpandAll();
                    }
                    else
                    {
                        m.TreeNode.Collapse();
                    }
                }
                if(Settings.ExpandAllLegacy2)
                {
                    c.CategoryHeader.TreeNode.Expand();
                }
                else
                {
                    c.CategoryHeader.TreeNode.Collapse();
                }
            }
            //end ui building
        }

        private void Tv_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if(e.Node is RelhaxFormTreeNode node)
            {
                foreach(RelhaxFormTreeNode subNode in node.Nodes)
                {
                    if(subNode.Component is Control c)
                    {
                        c.Hide();
                    }
                    if (subNode.Nodes.Count > 0)
                        BeforeCollapseSubNodes(subNode.Nodes);
                }
            }
        }

        private void BeforeCollapseSubNodes(TreeNodeCollection tnc)
        {
            foreach(RelhaxFormTreeNode subNode in tnc)
            {
                if(subNode.Component is Control c)
                {
                    c.Hide();
                }
                if (subNode.Nodes.Count > 0)
                    BeforeCollapseSubNodes(subNode.Nodes);
            }
        }

        //actually update UI bounds for legacy tree view
        //assumes the first one passed into the stack in the top view
        private void UpdateUIBounds(List<RelhaxFormTreeNode> nodes)
        {
            if (LoadingConfig)
                return;
            for (int i = 0; i < nodes.Count; i++)
            {
                if(nodes[i].Component != null)
                {
                    Control cont = (Control)nodes[i].Component;
                    RelhaxFormTreeNode node = nodes[i];
                    cont.SetBounds(node.Bounds.X, node.Bounds.Y, node.Bounds.Width, node.Bounds.Height);
                }
            }
        }

        private void Tv_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            if (LoadingConfig)
                return;
            if(e.Node is RelhaxFormTreeNode tn)
            {
                int index = tn.Category.TreeNodes.IndexOf(tn);
                List<RelhaxFormTreeNode> nodesList = tn.Category.TreeNodes;
                if ((index + 1 < nodesList.Count) && (!nodesList[index + 1].IsVisible))
                {
                    UpdateUIBounds(nodesList);
                }
                else if ((index - 1 > -1) && (!nodesList[index - 1].IsVisible))
                {
                    UpdateUIBounds(nodesList);
                }
                else if (index == 0)
                {
                    UpdateUIBounds(nodesList);
                }
                else if (index == nodesList.Count - 1)
                {
                    UpdateUIBounds(nodesList);
                }
            }
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
                RelhaxUserCheckBox UserPackage = new RelhaxUserCheckBox()
                {
                    Package = UserMods[i],
                    AutoCheck = false,
                    AutoSize = true
                };
                int yLocation = 3 + (UserPackage.Size.Height * (i));
                UserPackage.Location = new Point(3, yLocation);
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
                UserMods[i].UIComponent = UserPackage;
                UserMods[i].ParentUIComponent = UserPackage;
                UserMods[i].TopParentUIComponent = UserPackage;
                UserPackage.Click += OnMultiPackageClick;
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
        private void AddPackage(SelectablePackage sp, Category c, SelectablePackage parent)
        {
            //if set to show all mods at comamnd line, show them
            if (!Program.forceVisible && !sp.Visible)
                return;
            //setup the refrences
            sp.Parent = parent;
            sp.TopParent = c.CategoryHeader;
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
            string dateFormat = "";
            string tooltipString = "";
            if (sp.Level >=0)
            {
                if (sp.DownloadFlag)
                {
                    packageDisplayName = string.Format("{0} ({1})", packageDisplayName, Translations.getTranslatedString("updated"));
                    if ((sp.Size > 0))
                        packageDisplayName = string.Format("{0} ({1})", packageDisplayName, Utils.SizeSuffix(sp.Size, 1, true));
                }
                dateFormat = sp.Timestamp == 0 ? "" : Utils.ConvertFiletimeTimestampToDate(sp.Timestamp);
                tooltipString = sp.Description.Equals("") ? NoDescriptionAvailable : sp.Description + (sp.Timestamp == 0 ? "" : "\n\n" + LastUpdated + dateFormat);
            }
            //determine if the mod can be enabled or not, reguardless if the package is enabled or not
            //start at level -1 that can be enabled
            bool canBeEnabled = true;
            SelectablePackage temp = sp;
            while(temp.Level >= 0)
            {
                if (!temp.Enabled)
                    canBeEnabled = false;
                temp = temp.Parent;
            }
            //special code for each type of view
            switch (Settings.SView)
            {
                case Settings.SelectionView.Default:
                    //start code for dealing with panels
                    if (sp.Parent.ChildPanel == null)
                    {
                        sp.Parent.ChildPanel = new Panel()
                        {
                            BorderStyle = Settings.DisableBorders ? BorderStyle.None : BorderStyle.FixedSingle,
                            Size = new Size(c.TabPage.Size.Width - 35, 30),
                            AutoSize = true,
                            AutoSizeMode = AutoSizeMode.GrowOnly
                        };
                        sp.Parent.ChildPanel.Location = new Point(13, GetYLocation(sp.Parent.ParentPanel.Controls));
                        sp.Parent.ChildPanel.MouseDown += DisabledComponent_MouseDown;
                        sp.Parent.ParentPanel.Controls.Add(sp.Parent.ChildPanel);
                    }
                    sp.ParentPanel = sp.Parent.ChildPanel;
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
                                sp.Parent.RelhaxFormComboBoxList[0].MouseDown += Generic_MouseDown;
                                sp.Parent.RelhaxFormComboBoxList[0].SelectedIndexChanged += OnSingleDDPackageClick;
                                sp.Parent.RelhaxFormComboBoxList[0].handler = OnSingleDDPackageClick;
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
                                sp.Parent.RelhaxFormComboBoxList[1].MouseDown += Generic_MouseDown;
                                sp.Parent.RelhaxFormComboBoxList[1].SelectedIndexChanged += OnSingleDDPackageClick;
                                sp.Parent.RelhaxFormComboBoxList[1].handler = OnSingleDDPackageClick;
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
                                sp.Parent.RelhaxWPFComboBoxList[0].SelectionChanged += OnSingleDDPackageClick;
                                sp.Parent.RelhaxWPFComboBoxList[0].handler = OnSingleDDPackageClick;
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
                                sp.Parent.RelhaxWPFComboBoxList[1].SelectionChanged += OnSingleDDPackageClick;
                                sp.Parent.RelhaxWPFComboBoxList[1].handler = OnSingleDDPackageClick;
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
                            rb.Click += OnWPFComponentCheck;
                            sp.TreeViewItem.Header = sp.UIComponent;
                            sp.TreeViewItem.IsExpanded = Settings.ExpandAllLegacy ? true : false;
                            sp.Parent.TreeViewItem.Items.Add(sp.TreeViewItem);
                        }
                        else if(sp.UIComponent is System.Windows.Controls.CheckBox cb)
                        {
                            cb.Click += OnWPFComponentCheck;
                            sp.TreeViewItem.Header = sp.UIComponent;
                            sp.TreeViewItem.IsExpanded = Settings.ExpandAllLegacy ? true : false;
                            sp.Parent.TreeViewItem.Items.Add(sp.TreeViewItem);
                        }
                    }
                    break;
                case Settings.SelectionView.LegacyV2:
                    sp.TreeNode.Category = c;
                    switch (sp.Type)
                    {
                        case "single":
                        case "single1":
                            sp.UIComponent = new RelhaxFormRadioButton()
                            {
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
                            //sp.TreeNode.Bounds.Inflate(0, 25);
                            if (sp.Parent.RelhaxFormComboBoxList[0] == null)
                            {
                                sp.Parent.RelhaxFormComboBoxList[0] = new RelhaxFormComboBox()
                                {
                                    //autosize should be true...
                                    Size = new Size((int)(225 * DPISCALE), 15),
                                    Enabled = false,
                                    Name = "notAddedYet",
                                    DropDownStyle = ComboBoxStyle.DropDownList
                                };
                                //https://stackoverflow.com/questions/1882993/c-sharp-how-do-i-prevent-mousewheel-scrolling-in-my-combobox
                                sp.Parent.RelhaxFormComboBoxList[0].MouseWheel += (o, e) => ((HandledMouseEventArgs)e).Handled = true;
                                sp.Parent.RelhaxFormComboBoxList[0].MouseDown += Generic_MouseDown;
                                sp.Parent.RelhaxFormComboBoxList[0].SelectedIndexChanged += OnSingleDDPackageClick;
                                sp.Parent.RelhaxFormComboBoxList[0].handler = OnSingleDDPackageClick;
                            }
                            if (sp.Enabled)
                            {
                                ComboBoxItem cbi = new ComboBoxItem(sp, packageDisplayName);
                                sp.Parent.RelhaxFormComboBoxList[0].Items.Add(cbi);
                                if (sp.Checked)
                                {
                                    //sp.Parent.RelhaxFormComboBoxList[0].Enabled = true;
                                    sp.Parent.RelhaxFormComboBoxList[0].SelectedItem = cbi;
                                    DescriptionToolTip.SetToolTip(sp.Parent.RelhaxFormComboBoxList[0], tooltipString);
                                }
                            }
                            if (sp.Parent.RelhaxFormComboBoxList[0].Name.Equals("notAddedYet"))
                            {
                                if (sp.Parent.RelhaxFormComboBoxList[0].Items.Count > 0)
                                {
                                    sp.Parent.RelhaxFormComboBoxList[0].Enabled = true;
                                    if (sp.Parent.RelhaxFormComboBoxList[0].SelectedIndex == -1)
                                        sp.Parent.RelhaxFormComboBoxList[0].SelectedIndex = 0;
                                }
                                sp.Parent.RelhaxFormComboBoxList[0].Name = "added";
                                sp.TreeNode.Component = sp.Parent.RelhaxFormComboBoxList[0];
                                
                                sp.Parent.TreeNode.Nodes.Add(sp.TreeNode);
                                //also add the control
                                Control CBCONT1 = sp.Parent.RelhaxFormComboBoxList[0];
                                sp.ParentCategory.TreeView.Controls.Add(CBCONT1);
                                CBCONT1.Show();
                                sp.ParentCategory.TreeNodes.Add(sp.TreeNode);
                                //sp.ParentPanel.Controls.Add(sp.Parent.RelhaxFormComboBoxList[0]);
                            }
                            break;
                        case "single_dropdown2":
                            //sp.TreeNode.Bounds.Inflate(0, 5);
                            if (sp.Parent.RelhaxFormComboBoxList[1] == null)
                            {
                                sp.Parent.RelhaxFormComboBoxList[1] = new RelhaxFormComboBox()
                                {
                                    //autosize should be true...
                                    Size = new Size((int)(225 * DPISCALE), 15),
                                    Enabled = false,
                                    Name = "notAddedYet",
                                    DropDownStyle = ComboBoxStyle.DropDownList
                                };
                                //https://stackoverflow.com/questions/1882993/c-sharp-how-do-i-prevent-mousewheel-scrolling-in-my-combobox
                                sp.Parent.RelhaxFormComboBoxList[1].MouseWheel += (o, e) => ((HandledMouseEventArgs)e).Handled = true;
                                sp.Parent.RelhaxFormComboBoxList[1].MouseDown += Generic_MouseDown;
                                sp.Parent.RelhaxFormComboBoxList[1].SelectedIndexChanged += OnSingleDDPackageClick;
                                sp.Parent.RelhaxFormComboBoxList[1].handler = OnSingleDDPackageClick;
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
                                if (sp.Parent.RelhaxFormComboBoxList[1].Items.Count > 0)
                                {
                                    sp.Parent.RelhaxFormComboBoxList[1].Enabled = true;
                                    if (sp.Parent.RelhaxFormComboBoxList[1].SelectedIndex == -1)
                                        sp.Parent.RelhaxFormComboBoxList[1].SelectedIndex = 0;
                                }
                                sp.Parent.RelhaxFormComboBoxList[1].Name = "added";
                                sp.TreeNode.Component = sp.Parent.RelhaxFormComboBoxList[1];
                                sp.Parent.TreeNode.Nodes.Add(sp.TreeNode);
                                //also add the control
                                Control CBCONT = sp.Parent.RelhaxFormComboBoxList[1];
                                sp.ParentCategory.TreeView.Controls.Add(CBCONT);
                                CBCONT.Show();
                                sp.ParentCategory.TreeNodes.Add(sp.TreeNode);
                                //sp.ParentPanel.Controls.Add(sp.Parent.RelhaxFormComboBoxList[1]);
                            }
                            break;
                        case "multi":
                            sp.UIComponent = new RelhaxFormCheckBox()
                            {
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
                    
                    //color change code
                    //start code for handlers tooltips and attaching
                    if ((sp.UIComponent != null) && (sp.UIComponent is Control cont3))
                    {
                        //add tooltip
                        DescriptionToolTip.SetToolTip(cont3, tooltipString);
                        //take care of spacing and handlers
                        cont3.MouseDown += Generic_MouseDown;
                        //ADD HANDLERS HERE
                        if (cont3 is RelhaxFormCheckBox FormCheckBox)
                        {
                            FormCheckBox.Click += OnMultiPackageClick;
                        }
                        else if (cont3 is RelhaxFormRadioButton FormRadioButton)
                        {
                            FormRadioButton.Click += OnSinglePackageClick;
                        }
                        //attach to treeview
                        sp.TreeNode.Component = sp.UIComponent;
                        sp.Parent.TreeNode.Nodes.Add(sp.TreeNode);
                        //attach as control
                        Control MULSINCONT = (Control) sp.UIComponent;
                        sp.ParentCategory.TreeView.Controls.Add(MULSINCONT);
                        MULSINCONT.Show();
                        sp.ParentCategory.TreeNodes.Add(sp.TreeNode);
                    }
                    //end code for handlers tooltips and attaching
                    break;
            }
            if(sp.Packages.Count > 0)
            {
                foreach(SelectablePackage sp2 in sp.Packages)
                {
                    AddPackage(sp2, c, sp);
                }
            }
        }

        //generic handler to disable the auto check like in forms, but for WPF
        void OnWPFComponentCheck(object sender, System.Windows.RoutedEventArgs e)
        {
            if (LoadingConfig)
                return;
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
            if (LoadingConfig || IgnoreSearchBoxFocus)
                return;
            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = ipc.Package;
            if (!spc.Enabled || !spc.Parent.Enabled || !spc.TopParent.Enabled)
                return;
            //uncheck all packages at this level that are single
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
            //check the acutal package
            spc.Checked = true;
            //down
            PropagateChecked(spc, false);
            //up
            PropagateChecked(spc, true);
        }

        //when a single_dropdown mod is selected
        void OnSingleDDPackageClick(object sender, EventArgs e)
        {
            if (LoadingConfig || IgnoreSearchBoxFocus)
                return;
            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = null;
            if(ipc is RelhaxFormComboBox cb1)
            {
                ComboBoxItem cbi = (ComboBoxItem)cb1.SelectedItem;
                spc = cbi.Package;
            }
            else if (ipc is RelhaxWPFComboBox cb2)
            {
                ComboBoxItem cbi = (ComboBoxItem)cb2.SelectedItem;
                spc = cbi.Package;
            }
            if (!spc.Enabled || !spc.Parent.Enabled || !spc.TopParent.Enabled)
                return;
            foreach(SelectablePackage childPackage in spc.Parent.Packages)
            {
                if (childPackage.Equals(spc))
                    continue;
                //uncheck all packages of the same type
                if(childPackage.Type.Equals(spc.Type))
                {
                    childPackage.Checked = false;
                }
            }
            //verify selected is actually checked
            if (!spc.Checked)
                spc.Checked = true;
            //dropdown packages only need to propagate up when selected...
            PropagateChecked(spc, true);
        }

        //when a multi mod is selected
        void OnMultiPackageClick(object sender, EventArgs e)
        {
            if (LoadingConfig || IgnoreSearchBoxFocus)
                return;
            IPackageUIComponent ipc = (IPackageUIComponent)sender;
            SelectablePackage spc = ipc.Package;
            if (!spc.Enabled || !spc.Parent.Enabled || !spc.TopParent.Enabled)
                return;
            //can be enabled
            if(!spc.Checked)
            {
                //check it and propagate change
                spc.Checked = true;
                //if it's a user checkbox end here
                if (ipc is RelhaxUserCheckBox)
                    return;
                //down
                PropagateChecked(spc,false);
                //up
                PropagateChecked(spc,true);
            }
            else if(spc.Checked)
            {
                //uncheck it and propagate change
                spc.Checked = false;
                if (ipc is RelhaxUserCheckBox)
                    return;
                //up then down
                PropagateUpNotChecked(spc);
                PropagateDownNotChecked(spc);
            }
        }

        //propagates the change back up the selection tree
        //can be sent from any component
        //true = up, false = down
        void PropagateChecked(SelectablePackage spc, bool upDown)
        {
            //the parent of the package we just checked
            SelectablePackage parent = null;
            //if we're going up the tree, set the package to it's parent
            //else use itself
            if (upDown)
                parent = spc.Parent;
            else
                parent = spc;
            //first of all, check itself (if not checked already)
            parent.Checked = true;
            //for each type of requried single selection, check if the package has them, and if any are enabled
            bool hasSingles = false;
            bool singleSelected = false;
            bool hasDD1 = false;
            bool DD1Selected = false;
            bool hasDD2 = false;
            bool DD2Selected = false;
            foreach (SelectablePackage childPackage in parent.Packages)
            {
                //if the pacakge is enabled and it is of single type
                if ((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
                {
                    //then this package does have single type packages
                    hasSingles = true;
                    //if it's checked, set that bool as well
                    if (childPackage.Checked)
                        singleSelected = true;
                }
                //same idea as above
                else if ((childPackage.Type.Equals("single_dropdown") || childPackage.Type.Equals("single_dropdown1")) && childPackage.Enabled)
                {
                    hasDD1 = true;
                    if (childPackage.Checked)
                        DD1Selected = true;
                }
                else if (childPackage.Type.Equals("single_dropdown2") && childPackage.Enabled)
                {
                    hasDD2 = true;
                    if (childPackage.Checked)
                        DD2Selected = true;
                }
            }
            //if going up, will only ever see radiobuttons (not dropDown)
            //check if this package is of single type, if it is then we need to unselect all other packages of this level
            if (upDown && (parent.Type.Equals("single") || parent.Type.Equals("single1")))
            {
                foreach (SelectablePackage childPackage in parent.Parent.Packages)
                {
                    if ((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
                    {
                        if (!childPackage.Equals(parent))
                        {
                            childPackage.Checked = false;
                            PropagateDownNotChecked(childPackage);
                        }
                    }
                }
                //singleSelected = true;
            }
            if (hasSingles && !singleSelected)
            {
                //select one
                foreach(SelectablePackage childPackage in parent.Packages)
                {
                    if ((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
                    {
                        childPackage.Checked = true;
                        PropagateChecked(childPackage,false);
                        break;
                        //PropagateDownChecked(childPackage);
                    }
                }
            }
            if(hasDD1 && !DD1Selected)
            {
                //select one
                foreach (SelectablePackage childPackage in parent.Packages)
                {
                    if ((childPackage.Type.Equals("single_dropdown") || childPackage.Type.Equals("single_dropdown1")) && childPackage.Enabled)
                    {
                        childPackage.Checked = true;
                        break;
                        //no need to propagate, dropdown has no children
                    }
                }
            }
            if(hasDD2 && !DD2Selected)
            {
                //select one
                foreach (SelectablePackage childPackage in parent.Packages)
                {
                    if ((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
                    {
                        childPackage.Checked = true;
                        break;
                        //no need to propagate, dropdown has no children
                    }
                }
            }
            if(upDown)
                if (parent.Level >= 0)
                    //recursivly propagate the change back up the selection list
                    PropagateChecked(parent,true);
        }

        //propagates the change back up the selection tree
        //NOTE: the only component that can propagate up for a not checked is a multi
        void PropagateUpNotChecked(SelectablePackage spc)
        {
            if (spc.Level == -1)
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
        void PropagateDownNotChecked(SelectablePackage spc)
        {
            foreach(SelectablePackage childPackage in spc.Packages)
            {
                if (!childPackage.Enabled)
                    continue;
                childPackage.Checked = false;
                if(childPackage.Packages.Count > 0)
                    PropagateDownNotChecked(childPackage);
            }
        }

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

        #region Preview Code
        //generic hander for when any mouse button is clicked for MouseDown Events
        void Generic_MouseDown(object sender, EventArgs e)
        {
            if (LoadingConfig)
                return;
            if (e is MouseEventArgs m)
                if (m.Button != MouseButtons.Right)
                    return;
            if(sender is IPackageUIComponent ipc)
            {
                SelectablePackage spc = ipc.Package;
                if (ipc is RelhaxFormComboBox cb1)
                {
                    ComboBoxItem cbi = (ComboBoxItem)cb1.SelectedItem;
                    //fix for when dropdown option is on the screen and no packages are in it
                    if (cbi == null)
                        return;
                    spc = cbi.Package;
                }
                else if (ipc is RelhaxWPFComboBox cb2)
                {
                    ComboBoxItem cbi = (ComboBoxItem)cb2.SelectedItem;
                    spc = cbi.Package;
                }
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
        }
        
        private void DisabledComponent_MouseDown(object sender, MouseEventArgs e)
        {
            if (LoadingConfig)
                return;
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
            if (LoadingConfig)
                return;
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

        #region selection code

        //unchecks all mods from memory
        public static void ClearSelectionMemory(List<Category> parsedCatagoryList, List<SelectablePackage> UserMods)
        {
            Logging.Manager("Unchecking all mods");
            foreach (Category c in parsedCatagoryList)
            {
                if (c.CategoryHeader.Checked)
                    c.CategoryHeader.Checked = false;
                foreach (SelectablePackage m in c.Packages)
                {
                    if (m.Checked)
                        m.Checked = false;
                    //no need to clobber over UI controls, that is now done for us
                    UncheckProcessConfigs(m.Packages);
                }
            }
            if (UserMods != null)
            {
                foreach (SelectablePackage um in UserMods)
                {
                    if (um.Checked)
                        um.Checked = false;
                }
            }
        }

        private static void UncheckProcessConfigs(List<SelectablePackage> configList)
        {
            foreach (SelectablePackage c in configList)
            {
                if (c.Checked)
                    c.Checked = false;
                UncheckProcessConfigs(c.Packages);
            }
        }

        //saves the currently checked configs and mods
        public void SaveConfig(bool fromButton, string fileToConvert, List<Category> parsedCatagoryList, List<SelectablePackage> userMods)
        {
            //dialog box to ask where to save the config to
            SaveFileDialog saveLocation = new SaveFileDialog()
            {
                AddExtension = true,
                DefaultExt = ".xml",
                Filter = "*.xml|*.xml",
                InitialDirectory = Path.Combine(Application.StartupPath, "RelHaxUserConfigs"),
                Title = Translations.getTranslatedString("selectWhereToSave")
            };
            if (fromButton)
            {
                if (saveLocation.ShowDialog().Equals(DialogResult.Cancel))
                {
                    //cancel
                    return;
                }
            }
            string savePath = saveLocation.FileName;
            if (Settings.SaveLastConfig && !fromButton && fileToConvert == null)
            {
                savePath = Path.Combine(Application.StartupPath, "RelHaxUserConfigs", "lastInstalledConfig.xml");
                Logging.Manager(string.Format("Save last config checked, saving to {0}", savePath));
            }
            else if (!fromButton && !(fileToConvert == null))
            {
                savePath = fileToConvert;
                Logging.Manager(string.Format("convert saved config file \"{0}\" to format {1}", savePath, Settings.ConfigFileVersion));
            }

            //create saved config xml layout
            XDocument doc = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("mods", new XAttribute("ver", Settings.ConfigFileVersion), new XAttribute("date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")), new XAttribute("dbVersion", Program.betaDatabase ? "beta" : (String)Settings.DatabaseVersion)));

            //relhax mods root
            doc.Element("mods").Add(new XElement("relhaxMods"));
            //user mods root
            doc.Element("mods").Add(new XElement("userMods"));

            var nodeRelhax = doc.Descendants("relhaxMods").FirstOrDefault();
            //check every mod
            foreach (Category c in parsedCatagoryList)
            {
                foreach (SelectablePackage m in c.Packages)
                {
                    if (m.Checked)
                    {
                        //add it to the list
                        nodeRelhax.Add(new XElement("mod", m.PackageName));
                        if (m.Packages.Count > 0)
                        {
                            SaveProcessConfigs(ref doc, m.Packages);
                        }
                    }
                }
            }

            var nodeUserMods = doc.Descendants("userMods").FirstOrDefault();
            //check user mods
            foreach (SelectablePackage m in userMods)
            {
                if (m.Checked)
                {
                    //add it to the list
                    nodeUserMods.Add(new XElement("mod", m.Name));
                }
            }
            doc.Save(savePath);
            if (fromButton)
            {
                MessageBox.Show(Translations.getTranslatedString("configSaveSuccess"));
            }
        }

        private void SaveProcessConfigs(ref XDocument doc, List<SelectablePackage> configList)
        {
            var node = doc.Descendants("relhaxMods").FirstOrDefault();
            foreach (SelectablePackage cc in configList)
            {
                if (cc.Checked)
                {
                    //add the config to the list
                    node.Add(new XElement("mod", cc.PackageName));
                    if (cc.Packages.Count > 0)
                    {
                        SaveProcessConfigs(ref doc, cc.Packages);
                    }
                }
            }
        }

        public void LoadConfig(bool fromButton, string[] filePathArray, List<Category> parsedCatagoryList, List<SelectablePackage> userMods)
        {
            //uncheck everythihng in memory first
            ClearSelectionMemory(parsedCatagoryList, userMods);
            XmlDocument doc = new XmlDocument();
            //not being whitespace means there is an xml filename, means it is a developer selection
            if (!string.IsNullOrWhiteSpace(filePathArray[1]))
            {
                string xmlString = Utils.GetStringFromZip(filePathArray[0], filePathArray[1]);
                doc.LoadXml(xmlString);
            }
            else
            {
                doc.Load(filePathArray[0]);
            }
            //check config file version
            XmlNode xmlNode = doc.SelectSingleNode("//mods");
            string ver = "";
            // check if attribut exists and if TRUE, get the value
            if (xmlNode.Attributes != null && xmlNode.Attributes["ver"] != null)
            {
                ver = xmlNode.Attributes["ver"].Value;
            }
            if (ver.Equals("2.0"))      //the file is version v2.0, so go "loadConfigV2" (PackageName depended)
            {
                Logging.Manager(string.Format("Loading mod selections v2.0 from {0}", filePathArray[0]));
                LoadConfigV2(doc, parsedCatagoryList, userMods);
            }
            else // file is still version v1.0 (name dependend)
            {
                LoadConfigV1(fromButton, filePathArray[0], parsedCatagoryList, userMods);
            }
        }
        //loads a saved config from xml and parses it into the memory database
        public void LoadConfigV1(bool fromButton, string filePath, List<Category> parsedCatagoryList, List<SelectablePackage> userMods)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            Logging.Manager("Loading mod selections v1.0 from " + filePath);
            //get a list of mods
            XmlNodeList xmlModList = doc.SelectNodes("//mods/relhaxMods/mod");
            foreach (XmlNode n in xmlModList)
            {
                //gets the inside of each mod
                //also store each config that needsto be Enabled
                SelectablePackage m = new SelectablePackage();
                foreach (XmlNode nn in n.ChildNodes)
                {
                    switch (nn.Name)
                    {
                        case "name":
                            m = Utils.LinkMod(nn.InnerText, parsedCatagoryList);
                            if ((m != null) && (!m.Visible) && (!Program.forceVisible))
                                return;
                            if (m == null)
                            {
                                Logging.Manager(string.Format("WARNING: mod \"{0}\" not found", nn.InnerText));
                                MessageBox.Show(string.Format(Translations.getTranslatedString("modNotFound"), nn.InnerText));
                                continue;
                            }
                            if (m.Enabled)
                            {
                                //no need to clobber over UI code, now taken care of for us!
                                m.Checked = true;
                                Logging.Manager(string.Format("Checking mod {0}", m.Name));
                            }
                            else
                            {
                                //uncheck
                                if (m.Checked)
                                    m.Checked = false;
                            }
                            break;
                        case "configs":
                            LoadProcessConfigsV1(nn, m, true);
                            break;
                        //compatibility in case it's a super legacy with subConfigs
                        case "subConfigs":
                            LoadProcessConfigsV1(nn, m, true);
                            break;
                    }
                }
            }
            //user mods
            XmlNodeList xmlUserModList = doc.SelectNodes("//mods/userMods/mod");
            foreach (XmlNode n in xmlUserModList)
            {
                //gets the inside of each user mod
                SelectablePackage m = new SelectablePackage();
                foreach (XmlNode nn in n.ChildNodes)
                {
                    switch (nn.Name)
                    {
                        case "name":
                            m = Utils.GetUserMod(nn.InnerText, userMods);
                            if (m != null)
                            {
                                string filename = m.Name + ".zip";
                                if (File.Exists(Path.Combine(Application.StartupPath, "RelHaxUserMods", filename)))
                                {
                                    m.Checked = true;
                                    Logging.Manager(string.Format("checking user mod {0}", m.ZipFile));
                                }
                            }
                            break;
                    }
                }
            }
            Logging.Manager("Finished loading mod selections v1.0");
            if (fromButton)
            {
                DialogResult result = MessageBox.Show(Translations.getTranslatedString("oldSavedConfigFile"), Translations.getTranslatedString("information"), MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    // create Path to UserConfigs Backup
                    string backupFolder = Path.Combine(Application.StartupPath, "RelHaxUserConfigs", "Backup");
                    // create Backup folder at UserConfigs
                    Directory.CreateDirectory(backupFolder);
                    // exctrat filename to create a new filename with backup date and time
                    string filename = Path.GetFileNameWithoutExtension(filePath);
                    string fileextention = Path.GetExtension(filePath);
                    // create target path
                    string targetFilePath = Path.Combine(backupFolder, string.Format("{0}_{1:yyyy-MM-dd-HH-mm-ss}{2}", filename, DateTime.Now, fileextention));
                    // move file to new location now
                    try
                    {
                        File.Move(filePath, targetFilePath);
                    }
                    catch (Exception ex)
                    {
                        Utils.ExceptionLog("loadConfigV1", string.Format("sourceFile: {0}\ntargetFile: {1}", filePath, targetFilePath), ex);
                    }
                    // create saved config file with new format
                    SaveConfig(false, filePath, parsedCatagoryList, userMods);
                }
            }
        }
        //loads a saved config from xml and parses it into the memory database
        public void LoadConfigV2(XmlDocument doc, List<Category> parsedCatagoryList, List<SelectablePackage> userMods, bool defaultChecked = false)
        {
            List<string> savedConfigList = new List<string>();
            foreach (var mod in doc.CreateNavigator().Select("//relhaxMods/mod"))
            {
                savedConfigList.Add(mod.ToString());
            }
            foreach (Category c in parsedCatagoryList)
            {
                foreach (SelectablePackage m in c.Packages)
                {
                    if (m.Visible || Program.forceVisible)
                    {
                        if (savedConfigList.Contains(m.PackageName))
                        {
                            savedConfigList.Remove(m.PackageName);
                            if (!m.Enabled && !defaultChecked && !Program.forceEnabled)
                            {
                                MessageBox.Show(string.Format(Translations.getTranslatedString("modDeactivated"),
                                    m.NameFormatted), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            else
                            {
                                m.Checked = true;
                                //if it's the top level, chedk the category header
                                if (m.Level == 0)
                                {
                                    if (!m.ParentCategory.CategoryHeader.Checked)
                                        m.ParentCategory.CategoryHeader.Checked = true;
                                }
                                Logging.Manager(string.Format("Checking mod {0}", m.NameFormatted));
                            }
                        }
                        else
                        {
                            //uncheck
                            if (m.Checked)
                                m.Checked = false;
                        }
                        if (m.Packages.Count > 0)
                        {
                            LoadProcessConfigsV2(m.Name, m.Packages, ref savedConfigList, defaultChecked);
                        }
                    }
                }
            }
            List<string> savedUserConfigList = new List<string>();
            foreach (var userMod in doc.CreateNavigator().Select("//userMods/mod"))
            {
                savedUserConfigList.Add(userMod.ToString());
            }
            foreach (SelectablePackage um in userMods)
            {
                if (savedUserConfigList.Contains(um.Name))
                {
                    string filename = um.Name + ".zip";
                    if (File.Exists(Path.Combine(Application.StartupPath, "RelHaxUserMods", filename)))
                    {
                        //it will be done in the UI code
                        um.Checked = true;
                        Logging.Manager(string.Format("Checking user mod {0}", um.ZipFile));
                    }
                }
            }
            if (savedConfigList.Count > 0)
            {
                string modsNotFoundList = "";
                foreach (var s in savedConfigList)
                {
                    modsNotFoundList += "\n" + s;
                }
                if (!defaultChecked)
                    MessageBox.Show(string.Format(Translations.getTranslatedString("modsNotFoundTechnical"), modsNotFoundList), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            //check for structure issues
            if (!defaultChecked)
            {
                List<SelectablePackage> brokenPackages = IsValidStructure(parsedCatagoryList);
                Logging.Manager("Broken selection count: " + brokenPackages.Count);
                if (brokenPackages.Count > 0)
                {
                    //list the broken packages
                    StringBuilder sb = new StringBuilder();
                    foreach (SelectablePackage sp in brokenPackages)
                    {
                        sb.Append(string.Format("Name: {0}, Parent: {1}, Category: {2}\n", sp.NameFormatted, sp.Parent.NameFormatted, sp.ParentCategory.Name));
                    }
                    Logging.Manager("Broken selections: " + string.Join(", ", brokenPackages));
                    MessageBox.Show(Translations.getTranslatedString("modsBrokenStructure") + sb.ToString());
                }
            }
            Logging.Manager("Finished loading mod selections v2.0");
        }

        private void LoadProcessConfigsV1(XmlNode holder, SelectablePackage m, bool parentIsMod, SelectablePackage con = null)
        {
            foreach (XmlNode nnn in holder.ChildNodes)
            {
                if (parentIsMod)
                {
                    if (m == null)
                    {
                        continue;
                    }
                }
                else
                {
                    if (con == null)
                    {
                        continue;
                    }
                }
                SelectablePackage c = new SelectablePackage();
                foreach (XmlNode nnnn in nnn.ChildNodes)
                {
                    switch (nnnn.Name)
                    {
                        case "name":
                            if (parentIsMod)
                            {
                                c = m.GetPackage(nnnn.InnerText);
                                if ((c != null) && (!c.Visible))
                                    return;
                            }
                            else
                            {
                                c = con.GetPackage(nnnn.InnerText);
                                if ((c != null) && (!c.Visible))
                                    return;
                            }
                            if (c == null)
                            {
                                Logging.Manager(string.Format("WARNING: config \"{0}\" not found for mod/config \"{1}\"", nnnn.InnerText, holder.InnerText));
                                MessageBox.Show(string.Format(Translations.getTranslatedString("configNotFound"), nnnn.InnerText, holder.InnerText));
                                continue;
                            }
                            if (c.Enabled)
                            {
                                c.Checked = true;
                                Logging.Manager(string.Format("Checking mod {0}", c.NameFormatted));
                            }
                            else
                            {
                                if (c.Checked)
                                    c.Checked = false;
                            }
                            break;
                        case "configs":
                            LoadProcessConfigsV1(nnnn, m, false, c);
                            break;
                        case "subConfigs":
                            LoadProcessConfigsV1(nnnn, m, false, c);
                            break;
                    }
                }
            }
        }

        private void LoadProcessConfigsV2(string parentName, List<SelectablePackage> configList, ref List<string> savedConfigList, bool defaultChecked)
        {
            bool shouldBeBA = false;
            Panel panelRef = null;
            foreach (SelectablePackage c in configList)
            {
                if (c.Visible)
                {
                    if (savedConfigList.Contains(c.PackageName))
                    {
                        savedConfigList.Remove(c.PackageName);
                        if (!c.Enabled && !defaultChecked)
                        {
                            MessageBox.Show(string.Format(Translations.getTranslatedString("configDeactivated"), c.NameFormatted, parentName), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            c.Checked = true;
                            Logging.Manager(string.Format("Checking mod {0}", c.NameFormatted));
                        }
                    }
                    else
                    {
                        if (c.Checked)
                            c.Checked = false;
                    }
                    if (c.Packages.Count > 0)
                    {
                        LoadProcessConfigsV2(c.Name, c.Packages, ref savedConfigList, defaultChecked);
                    }
                }
            }
            if (shouldBeBA && panelRef != null)
            {
                if (!Settings.DisableColorChange)
                    panelRef.BackColor = System.Drawing.Color.BlanchedAlmond;
            }
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
            LoadConfig(LoadMode == LoadConfigMode.FromButton, filePathArray, ParsedCatagoryList, UserMods);
            //if it was from a button, tell the user it loaded the config successfully
            if (LoadMode == LoadConfigMode.FromButton)
            {
                if (LoadMode == LoadConfigMode.FromButton)
                    MessageBox.Show(Translations.getTranslatedString("prefrencesSet"), Translations.getTranslatedString("information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                ModSelectionList_SizeChanged(null, null);
            }
            //set this back to false so the user can interact
            LoadingConfig = false;
        }
        //checks for invalid structure in the selected packages
        //ex: a new mandatory option was added to a mod, but the user does not have it selected
        public List<SelectablePackage> IsValidStructure(List<Category> ParsedCategoryList)
        {
            List<SelectablePackage>  brokenPackages = new List<SelectablePackage>();
            foreach (Category cat in ParsedCategoryList)
            {
                if (cat.Packages.Count > 0)
                {
                    foreach (SelectablePackage sp in cat.Packages)
                        IsValidStructure(sp, ref brokenPackages);
                }
                //then check if the header should be checked
                //at this point it is assumed that the structure is valid, meanign that
                //if there is at least on package selected it should be propagated up to level 0
                //so ontly need to do this at level 0
                bool anyPackagesSelected = false;
                foreach(SelectablePackage sp in cat.Packages)
                {
                    if (sp.Enabled && sp.Checked)
                        anyPackagesSelected = true;
                }
                if (!anyPackagesSelected && cat.CategoryHeader.Checked)
                    cat.CategoryHeader.Checked = false;
            }
            return brokenPackages;
        }

        private void IsValidStructure(SelectablePackage Package, ref List<SelectablePackage> brokenPackages)
        {
            if (Package.Checked)
            {
                bool hasSingles = false;
                bool singleSelected = false;
                bool hasDD1 = false;
                bool DD1Selected = false;
                bool hasDD2 = false;
                bool DD2Selected = false;
                foreach (SelectablePackage childPackage in Package.Packages)
                {
                    if ((childPackage.Type.Equals("single") || childPackage.Type.Equals("single1")) && childPackage.Enabled)
                    {
                        hasSingles = true;
                        if (childPackage.Checked)
                            singleSelected = true;
                    }
                    else if ((childPackage.Type.Equals("single_dropdown") || childPackage.Type.Equals("single_dropdown1")) && childPackage.Enabled)
                    {
                        hasDD1 = true;
                        if (childPackage.Checked)
                            DD1Selected = true;
                    }
                    else if (childPackage.Type.Equals("single_dropdown2") && childPackage.Enabled)
                    {
                        hasDD2 = true;
                        if (childPackage.Checked)
                            DD2Selected = true;
                    }
                }
                if (hasSingles && !singleSelected)
                {
                    Package.Checked = false;
                    if (!brokenPackages.Contains(Package))
                        brokenPackages.Add(Package);
                }
                if (hasDD1 && !DD1Selected)
                {
                    Package.Checked = false;
                    if (!brokenPackages.Contains(Package))
                        brokenPackages.Add(Package);
                }
                if (hasDD2 && !DD2Selected)
                {
                    Package.Checked = false;
                    if (!brokenPackages.Contains(Package))
                        brokenPackages.Add(Package);
                }
                if (Package.Checked && !Package.Parent.Checked)
                {
                    Package.Checked = false;
                    if (!brokenPackages.Contains(Package))
                        brokenPackages.Add(Package);
                }
            }
            if (Package.Packages.Count > 0)
                foreach (SelectablePackage sep in Package.Packages)
                    IsValidStructure(sep, ref brokenPackages);
        }
        //handles checking of the "default checked" mods
        //runs this on all loadings of modSelectionList, but is overridden if the user wants to load his/her own config
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
            LoadConfigV2(doc, ParsedCatagoryList, UserMods, true);
            Logging.Manager("Finished checking default mods");
        }
        #endregion

        #region UI event handlers (resize, button press, expand toggling)
        //handler for when the clear selections button is pressed
        private void ClearSelectionsButton_Click(object sender, EventArgs e)
        {
            //not actually *loading* a config, but want to disable the handlers anyways
            LoadingConfig = true;
            Logging.Manager("clearSelectionsButton pressed, clearing selections");
            ClearSelectionMemory(ParsedCatagoryList, UserMods);
            //dispose of not needed stuff and reload the UI

            LoadingConfig = false;
            MessageBox.Show(Translations.getTranslatedString("selectionsCleared"));
            //ModSelectionList_SizeChanged(null, null);
        }
        //handler to set the cancel bool to false
        private void ContinueButton_Click(object sender, EventArgs e)
        {
            //save the last config if told to do so
            if (Settings.SaveLastConfig)
            {
                SaveConfig(false, null, ParsedCatagoryList, UserMods);
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
            SaveConfig(true, null, ParsedCatagoryList, UserMods);
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
            if (Settings.SView == Settings.SelectionView.Legacy)
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
            else if (Settings.SView == Settings.SelectionView.LegacyV2)
            {
                foreach (Control c in modTabGroups.SelectedTab.Controls)
                {

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
            ComboBox sendah = (ComboBox)sender;
            if (sendah.SelectedIndex == -1 || IgnoreSelections)
            {
                return;
            }
            if (Settings.SView == Settings.SelectionView.Default)
            {
                if (sendah.SelectedItem is SelectablePackage c)
                {
                    if (modTabGroups.TabPages.Contains(c.TopParent.TabIndex))
                    {
                        modTabGroups.SelectedTab = c.TopParent.TabIndex;
                    }
                    if (c.UIComponent is Control cont)
                    {
                        IgnoreSearchBoxFocus = true;
                        cont.Focus();
                        IgnoreSearchBoxFocus = false;
                    }
                }
            }
            else if (Settings.SView == Settings.SelectionView.Legacy)
            {
                if (sendah.SelectedItem is SelectablePackage m)
                {
                    if (modTabGroups.TabPages.Contains(m.TabIndex))
                    {
                        modTabGroups.SelectedTab = m.TabIndex;
                    }
                    if(m.UIComponent is System.Windows.Controls.Control c)
                    {
                        IgnoreSearchBoxFocus = true;
                        c.Focus();
                        IgnoreSearchBoxFocus = false;
                    }
                    
                    //this.ModSelectionList_SizeChanged(null, null);
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
