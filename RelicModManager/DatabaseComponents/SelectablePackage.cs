using RelhaxModpack.DatabaseComponents;
using RelhaxModpack.UIComponents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RelhaxModpack
{
    //a selectable package could be a mod or a config
    public class SelectablePackage : DatabasePackage
    {
        //constructor
        public SelectablePackage()
        {
            //save memory by only enabling the components we need
            switch (Settings.SView)
            {
                case SelectionView.Default:
                    RelhaxFormComboBoxList = new RelhaxFormComboBox[2];
                    break;
                case SelectionView.DefaultV2:
                    RelhaxWPFComboBoxList = new RelhaxWPFComboBox[2];
                    //TreeViewItem = new System.Windows.Controls.TreeViewItem();
                    //ChildBorder = new System.Windows.Controls.Border();
                    //ChildStackPanel = new System.Windows.Controls.StackPanel();
                    ContentControl = new System.Windows.Controls.ContentControl();
                    break;
                case SelectionView.Legacy:
                    TreeNode = new RelhaxFormTreeNode();
                    break;
            }

        }
        //the name of the package for the user display
        public string Name = "";
        public string NameFormatted
        {
            get {
                    return Name.Replace("{version}",Version);
                    //string replace takes much less time than replaceMacro
                    //also name property currently only ever has {version} macro
                    //Utils.ReplaceMacro(Name,"version",Version);
                }
        }
        //the TabPage refrence
        public TabPage TabIndex = null;
        //the Category refrence
        public Category ParentCategory = null;
        //can the user select multiple configs or one only?
        public string Type = "";
        //the refrence for the direct parent of this package
        public SelectablePackage Parent = null;
        //the refrence for the absolute top of the package tree
        public SelectablePackage TopParent = null;
        //the UI element refrence for this package
        public IPackageUIComponent UIComponent = null;
        //the UI element refrence for the direct parent of this package
        public IPackageUIComponent ParentUIComponent = null;
        //the UI element refrence for the absolute top of the package tree
        public IPackageUIComponent TopParentUIComponent = null;
        //a flag to determine wether or not the mod should be shown
        public bool Visible = true;
        //size of the mod zip file
        public Int64 Size = 0;
        //update comments of the package
        public string UpdateComment = "";
        //description of the package
        public string Description = "";
        //bool for wether the package is selected to install
        protected internal bool _Checked = false;
        public bool Checked
        {
            get
            {
                return _Checked;
            }
            set
            {
                _Checked = value;
                if(UIComponent != null)
                    UIComponent.OnCheckedChanged(value);
                if ((Type.Equals("single_dropdown") || Type.Equals("single_dropdown1")) && Enabled)
                {
                    switch(Settings.SView)
                    {
                        case SelectionView.Default:
                            Parent.RelhaxFormComboBoxList[0].OnDropDownSelectionChanged(this, value);
                            break;
                        case SelectionView.DefaultV2:
                            Parent.RelhaxWPFComboBoxList[0].OnDropDownSelectionChanged(this, value);
                            break;
                    }
                }
                else if (Type.Equals("single_dropdown2") && Enabled)
                {
                    switch (Settings.SView)
                    {
                        case SelectionView.Default:
                            Parent.RelhaxFormComboBoxList[1].OnDropDownSelectionChanged(this, value);
                            break;
                        case SelectionView.DefaultV2:
                            Parent.RelhaxWPFComboBoxList[1].OnDropDownSelectionChanged(this, value);
                            break;
                    }
                }
                switch (Settings.SView)
                {
                    //default view UI selection code
                    case SelectionView.Default:
                        switch (_Checked)
                        {
                            case true:
                                //handle color change code
                                if (Settings.EnableColorChangeDefaultView)
                                {
                                    if (ParentPanel != null && ParentPanel.BackColor != Color.BlanchedAlmond)
                                    {
                                        //set the panel color; the checkboxes and stuff are all transparent backgorund
                                        ParentPanel.BackColor = Color.BlanchedAlmond;
                                    }
                                    //special user CB code
                                    else if (UIComponent is RelhaxUserCheckBox rucb)
                                    {
                                        rucb.BackColor = Color.BlanchedAlmond;
                                        if (Settings.DarkUI)
                                            rucb.ForeColor = SystemColors.ControlText;
                                    }
                                }
                                break;
                            case false:
                                //handle color change code
                                if (Settings.EnableColorChangeDefaultView)
                                {
                                    if (ParentPanel != null && !AnyPackagesChecked())
                                    {
                                        ParentPanel.BackColor = Settings.GetBackColorWinForms();
                                    }
                                    else if (UIComponent is RelhaxUserCheckBox rucb)
                                    {
                                        rucb.BackColor = Settings.GetBackColorWinForms();
                                        if (Settings.DarkUI)
                                        {
                                            rucb.ForeColor = Settings.GetTextColorWinForms();
                                        }
                                    }

                                }
                                break;
                        }
                        break;
                    //WPF treeview is done with treeviewItem
                    case SelectionView.DefaultV2:
                        switch (_Checked)
                        {
                            case true:
                                //handle color change code
                                if (Settings.EnableColorChangeLegacyView)
                                {
                                    if(ParentBorder != null && ParentBorder.Background != System.Windows.Media.Brushes.BlanchedAlmond)
                                    {
                                        ParentBorder.Background = System.Windows.Media.Brushes.BlanchedAlmond;
                                    }
                                    if (Settings.DarkUI)
                                    {
                                        //need to go through every contentpresenter of a stackpanel and parse
                                        foreach (System.Windows.UIElement ele in ParentStackPanel.Children)
                                        {
                                            if (ele is System.Windows.Controls.ContentControl ctrl)
                                            {
                                                if(ctrl.Content is System.Windows.Controls.Control ctrl3 && !(ele is RelhaxWPFComboBox))
                                                {
                                                    ctrl3.Foreground = System.Windows.Media.Brushes.Black;
                                                }
                                                else if (ele is System.Windows.Controls.Control topHeader)
                                                {
                                                    topHeader.Foreground = System.Windows.Media.Brushes.Black;
                                                }
                                            }
                                            else if (ele is System.Windows.Controls.Control ctrl2 && !(ele is RelhaxWPFComboBox))
                                            {
                                                ctrl2.Foreground = System.Windows.Media.Brushes.Black;
                                            }
                                        }
                                    }
                                }
                                break;
                            case false:
                                //handle color change code
                                if (Settings.EnableColorChangeLegacyView)
                                {
                                    if (ParentBorder != null && !AnyPackagesChecked())
                                        ParentBorder.Background = Settings.GetBackColorWPF();
                                    if (Settings.DarkUI)
                                    {
                                        //need to go through every contentpresenter of a stackpanel and parse
                                        if (!AnyPackagesChecked())
                                        {
                                            foreach (System.Windows.UIElement ele in ParentStackPanel.Children)
                                            {
                                                if (ele is System.Windows.Controls.ContentControl ctrl)
                                                {
                                                    if (ctrl.Content is System.Windows.Controls.Control ctrl3 && !(ele is RelhaxWPFComboBox))
                                                    {
                                                        ctrl3.Foreground = Settings.GetTextColorWPF();
                                                    }
                                                    else if (ele is System.Windows.Controls.Control topHeader)
                                                    {
                                                        topHeader.Foreground = Settings.GetTextColorWPF();
                                                    }
                                                }
                                                else if (ele is System.Windows.Controls.Control ctrl2 && !(ele is RelhaxWPFComboBox))
                                                {
                                                    ctrl2.Foreground = Settings.GetTextColorWPF();
                                                }
                                            }
                                        }
                                    }
                                }
                                break;
                        }
                        break;
                }
            }
        }
        //overriding the enabled so we can trigger the UI components
        public override bool Enabled
        {
            get { return _Enabled; }
            set
            {
                _Enabled = value;
                if (UIComponent != null)
                    UIComponent.OnEnabledChanged(value);
            }
        }
        //keeping track of the level in the tree where it is. 0 is topmost level
        public int Level = -2;
        //Components for FORMS
        //the list of all dropDown options for each package type
        public RelhaxFormComboBox[] RelhaxFormComboBoxList;
        //the TreeViewItem for winForms
        public RelhaxFormTreeNode TreeNode;
        //the panel that this package sits in
        public Panel ParentPanel = null;
        public Panel ChildPanel = null;

        //Components for WPF
        public RelhaxWPFComboBox[] RelhaxWPFComboBoxList;
        //the border for the legacy view to allow for putting all subchilderen in the border. sits inside treeviewitem
        public System.Windows.Controls.Border ChildBorder;
        //the stackpanel to allow the child treeviewitems to stack upon each other. sits inside the border
        public System.Windows.Controls.StackPanel ChildStackPanel;
        public System.Windows.Controls.Border ParentBorder;
        public System.Windows.Controls.StackPanel ParentStackPanel;
        public System.Windows.Controls.ContentControl @ContentControl;
        //the list of cache files that should be backed up before wiping the directory
        public List<string> UserFiles = new List<string>();
        //the list of SelectablePackage entries within this instance of SelectablePackages
        public List<SelectablePackage> Packages = new List<SelectablePackage>();
        //list of LogicalDependency and Dependency package names used for linking the dependencies and logicaldependencies
        public List<LogicalDependency> LogicalDependencies = new List<LogicalDependency>();
        public List<Dependency> Dependencies = new List<Dependency>();
        //list of media preview items associated with this package
        public List<Media> PictureList = new List<Media>();
        //provides a nice complete path that is more human readable than a packagePath
        public override string CompletePath
        {
            get
            {
                if(ParentCategory == null)
                {
                    return base.CompletePath;
                }
                string _CompletePath = ParentCategory.Name + "->";
                List<string> parentPackages = new List<string>();
                SelectablePackage packageToIterate = Parent;
                while(!packageToIterate.Equals(TopParent))
                {
                    parentPackages.Add(packageToIterate.NameFormatted);
                }
                parentPackages.Add(TopParent.NameFormatted);
                parentPackages.Reverse();
                foreach (string s in parentPackages)
                {
                    _CompletePath = _CompletePath + s + "->";
                }
                _CompletePath = _CompletePath + NameFormatted;
                return _CompletePath;
            }
        }
        //sorts the mods
        public static int CompareMods(SelectablePackage x, SelectablePackage y)
        {
            return x.Name.CompareTo(y.Name);
        }
        //for old (v1.0) method of getting package names
        public SelectablePackage GetPackage(string packageNameOld)
        {
            if (Packages == null || Packages.Count == 0)
                return null;
            foreach (SelectablePackage sp in Packages)
            {
                if (sp.Name.Equals(packageNameOld))
                    return sp;
            }
            return null;
        }
        //for display in combobox
        public override string ToString()
        {
            return NameFormatted;
        }

        //check if the color change should be changed back on
        public bool AnyPackagesChecked()
        {
            foreach (SelectablePackage sp in Parent.Packages)
            {
                if (sp.Enabled && sp.Checked)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
