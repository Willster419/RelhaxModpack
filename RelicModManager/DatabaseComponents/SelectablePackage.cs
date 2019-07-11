using RelhaxModpack.DatabaseComponents;
using RelhaxModpack.UIComponents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RelhaxModpack
{
    /// <summary>
    /// A package that has UI elements, and is eithor a mod or config that can be selected in the UI
    /// </summary>
    public class SelectablePackage : DatabasePackage
    {
        /// <summary>
        /// Constructior override for DatabasePackage. save memory by only enabling the components we need
        /// </summary>
        public SelectablePackage()
        {
            switch (Settings.SView)
            {
                case SelectionView.Default:
                    RelhaxFormComboBoxList = new RelhaxFormComboBox[2];
                    break;
                case SelectionView.DefaultV2:
                    RelhaxWPFComboBoxList = new RelhaxWPFComboBox[2];
                    ContentControl = new System.Windows.Controls.ContentControl();
                    break;
                case SelectionView.Legacy:
                    TreeViewItem = new System.Windows.Controls.TreeViewItem();
                    RelhaxWPFComboBoxList = new RelhaxWPFComboBox[2];
                    break;
            }
        }
        /// <summary>
        /// the name of the package
        /// </summary>
        public string Name = "";
        /// <summary>
        /// the name of the package with the version macro replaced for use display
        /// </summary>
        public string NameFormatted
        {
            get {
                    return Name.Replace("{version}",Version);
                    //string replace takes much less time than replaceMacro
                    //also name property currently only ever has {version} macro
                    //Utils.ReplaceMacro(Name,"version",Version);
                }
        }
        /// <summary>
        /// the TabPage refrence
        /// </summary>
        public TabPage TabIndex = null;
        /// <summary>
        /// the Category refrence
        /// </summary>
        public Category ParentCategory = null;
        /// <summary>
        /// the type of selectable package logic to follow (options are single1, single_dropdown1, single_dropdown2, multi)
        /// </summary>
        public string Type = "";
        /// <summary>
        /// the refrence for the direct parent of this package
        /// </summary>
        public SelectablePackage Parent = null;
        /// <summary>
        /// the refrence for the absolute top of the package tree
        /// </summary>
        public SelectablePackage TopParent = null;
        /// <summary>
        /// the UI element refrence for this package
        /// </summary>
        public IPackageUIComponent UIComponent = null;
        /// <summary>
        /// the UI element refrence for the direct parent of this package
        /// </summary>
        public IPackageUIComponent ParentUIComponent = null;
        /// <summary>
        /// the UI element refrence for the absolute top of the package tree
        /// </summary>
        public IPackageUIComponent TopParentUIComponent = null;
        /// <summary>
        /// a flag to determine wether or not the mod should be shown in UI
        /// </summary>
        public bool Visible = true;
        /// <summary>
        /// size of the mod zip file
        /// TODO: convert this to Uint64
        /// </summary>
        public Int64 Size = 0;
        /// <summary>
        /// update comments of the package
        /// </summary>
        public string UpdateComment = "";
        /// <summary>
        /// description of the package
        /// </summary>
        public string Description = "";
        /// <summary>
        /// field for wether the package is selected to install
        /// </summary>
        protected internal bool _Checked = false;
        /// <summary>
        /// property for if the package is selected by the user to install. handles all color change and single_dropdown updating code
        /// </summary>
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
                int dropDownSelectionType = -1;
                if (Type.Equals("single_dropdown") || Type.Equals("single_dropdown1"))
                {
                    dropDownSelectionType = 0;
                }
                else if (Type.Equals("single_dropdown2"))
                {
                    dropDownSelectionType = 1;
                }
                if (Enabled && dropDownSelectionType > -1)
                {
                    switch (Settings.SView)
                    {
                        case SelectionView.Default:
                            Parent.RelhaxFormComboBoxList[dropDownSelectionType].OnDropDownSelectionChanged(this, value);
                            break;
                        case SelectionView.DefaultV2:
                            Parent.RelhaxWPFComboBoxList[dropDownSelectionType].OnDropDownSelectionChanged(this, value);
                            break;
                        case SelectionView.Legacy:
                            Parent.RelhaxWPFComboBoxList[dropDownSelectionType].OnDropDownSelectionChanged(this, value);
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
                                        break;
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
                                    //special user CB code
                                    else if (UIComponent is RelhaxUserCheckBox rucb)
                                    {
                                        rucb.BackColor = Settings.GetBackColorWinForms();
                                        if (Settings.DarkUI)
                                        {
                                            rucb.ForeColor = Settings.GetTextColorWinForms();
                                        }
                                        break;
                                    }
                                }
                                break;
                        }
                        break;
                    case SelectionView.DefaultV2:
                        switch (_Checked)
                        {
                            case true:
                                //handle color change code
                                if (Settings.EnableColorChangeDefaultV2View)
                                {
                                    if(ParentBorder != null && ParentBorder.Background != System.Windows.Media.Brushes.BlanchedAlmond)
                                    {
                                        ParentBorder.Background = System.Windows.Media.Brushes.BlanchedAlmond;
                                    }
                                    //special user CB code
                                    else if (UIComponent is RelhaxUserCheckBox rucb)
                                    {
                                        rucb.BackColor = Color.BlanchedAlmond;
                                        if (Settings.DarkUI)
                                            rucb.ForeColor = SystemColors.ControlText;
                                        break;
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
                                if (Settings.EnableColorChangeDefaultV2View)
                                {
                                    if (ParentBorder != null && !AnyPackagesChecked())
                                        ParentBorder.Background = Settings.GetBackColorWPF();
                                    //special user CB code
                                    else if (UIComponent is RelhaxUserCheckBox rucb)
                                    {
                                        rucb.BackColor = Settings.GetBackColorWinForms();
                                        if (Settings.DarkUI)
                                        {
                                            rucb.ForeColor = Settings.GetTextColorWinForms();
                                        }
                                        break;
                                    }
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
                                        else if (Level == -1)
                                        {
                                            RelhaxWPFCheckBox r = (RelhaxWPFCheckBox)UIComponent;
                                            r.Foreground = Settings.GetTextColorWPF();
                                        }
                                    }
                                }
                                break;
                        }
                        break;
                    case SelectionView.Legacy:
                        switch (_Checked)
                        {
                            case true:
                                //handle color change code
                                if (Settings.EnableColorChangeLegacyView)
                                {
                                    if (ParentBorder != null && ParentBorder.Background != System.Windows.Media.Brushes.BlanchedAlmond)
                                    {
                                        ParentBorder.Background = System.Windows.Media.Brushes.BlanchedAlmond;
                                    }
                                    //special user CB code
                                    else if (UIComponent is RelhaxUserCheckBox rucb)
                                    {
                                        rucb.BackColor = Color.BlanchedAlmond;
                                        if (Settings.DarkUI)
                                            rucb.ForeColor = SystemColors.ControlText;
                                        break;
                                    }
                                    else if (Level == -1 && TreeView.Background != System.Windows.Media.Brushes.BlanchedAlmond)
                                    {
                                        TreeView.Background = System.Windows.Media.Brushes.BlanchedAlmond;
                                    }
                                    if(Settings.DarkUI && (ParentStackPanel != null))
                                    {
                                        //need to go through every treviewitem and parse
                                        foreach(System.Windows.Controls.TreeViewItem tview in ParentStackPanel.Children)
                                        {
                                            if(tview.Header is RelhaxWPFCheckBox  || tview.Header is RelhaxWPFRadioButton )
                                            {
                                                System.Windows.Controls.Control c = (System.Windows.Controls.Control)tview.Header;
                                                c.Foreground = System.Windows.Media.Brushes.Black;
                                            }
                                        }
                                    }
                                    if (Settings.DarkUI && Level == -1)
                                    {
                                        RelhaxWPFCheckBox r = (RelhaxWPFCheckBox)UIComponent;
                                        r.Foreground = System.Windows.Media.Brushes.Black;
                                    }
                                }
                                break;
                            case false:
                                //handle color change code
                                if (Settings.EnableColorChangeLegacyView)
                                {
                                    if (ParentBorder != null && !AnyPackagesChecked())
                                    {
                                        ParentBorder.Background = Settings.GetBackColorWPF();
                                    }
                                    //special user CB code
                                    else if (UIComponent is RelhaxUserCheckBox rucb)
                                    {
                                        rucb.BackColor = Settings.GetBackColorWinForms();
                                        if (Settings.DarkUI)
                                        {
                                            rucb.ForeColor = Settings.GetTextColorWinForms();
                                        }
                                        break;
                                    }
                                    else if (Level == -1 && !AnyPackagesChecked())
                                    {
                                        TreeView.Background = Settings.GetBackColorWPF();
                                    }
                                    if(Settings.DarkUI)
                                    {
                                        if(!AnyPackagesChecked() && (ParentStackPanel != null))
                                        {
                                            foreach (System.Windows.Controls.TreeViewItem tview in ParentStackPanel.Children)
                                            {
                                                if (tview.Header is RelhaxWPFCheckBox || tview.Header is RelhaxWPFRadioButton)
                                                {
                                                    System.Windows.Controls.Control c = (System.Windows.Controls.Control)tview.Header;
                                                    c.Foreground = Settings.GetTextColorWPF();
                                                }
                                            }
                                        }
                                        else if (Level == -1)
                                        {
                                            RelhaxWPFCheckBox r = (RelhaxWPFCheckBox)UIComponent;
                                            r.Foreground = Settings.GetTextColorWPF();
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
        /// <summary>
        /// The level in the database tree where the package resides.
        /// Category header is -1, each child is +1 from there
        /// </summary>
        public int Level = -2;

        //Components for WINFORMS
        /// <summary>
        /// the list of all dropDown options for each package type. winforms component
        /// </summary>
        public RelhaxFormComboBox[] RelhaxFormComboBoxList;
        /// <summary>
        /// the panel that this package sits in. winforms component
        /// </summary>
        public Panel ParentPanel = null;
        /// <summary>
        /// the panel that contains all child packages. winforms component
        /// </summary>
        public Panel ChildPanel = null;

        //Components for WPF (BOTH)
        public RelhaxWPFComboBox[] RelhaxWPFComboBoxList;
        /// <summary>
        /// the border for the legacy view to allow for putting all subchilderen in the border. sits inside treeviewitem. wpf component
        /// </summary>
        public System.Windows.Controls.Border ChildBorder;
        /// <summary>
        /// the stackpanel to allow the child treeviewitems to stack upon each other. sits inside the border. wpf component
        /// </summary>
        public System.Windows.Controls.StackPanel ChildStackPanel;
        /// <summary>
        /// the border that this component is in. wpf componment
        /// </summary>
        public System.Windows.Controls.Border ParentBorder;
        /// <summary>
        /// the stackpanel that this item is inside. wpf component
        /// </summary>
        public System.Windows.Controls.StackPanel ParentStackPanel;

        //only for DEFAULT V2
        /// <summary>
        /// ContentControl item to allow for right-clicking of disabled components. defaultv2 wpf component
        /// </summary>
        public System.Windows.Controls.ContentControl @ContentControl;

        //only for LEGACY
        /// <summary>
        /// the treeviewitem that corresponds to this package. legacy wpf component
        /// </summary>
        public System.Windows.Controls.TreeViewItem @TreeViewItem;
        /// <summary>
        /// the treeview that this package is in. legacy wpf component
        /// </summary>
        public System.Windows.Controls.TreeView @TreeView;

        /// <summary>
        /// the list of cache files that should be backed up before wiping the directory
        /// </summary>
        public List<UserFiles> UserFiles = new List<UserFiles>();
        /// <summary>
        /// the list of child SelectablePackage entries in this instance of SelectablePackages
        /// </summary>
        public List<SelectablePackage> Packages = new List<SelectablePackage>();
        /// <summary>
        /// list of LogicalDependency package names
        /// </summary>
        public List<LogicalDependency> LogicalDependencies = new List<LogicalDependency>();
        /// <summary>
        /// list of Dependency package names
        /// </summary>
        public List<Dependency> Dependencies = new List<Dependency>();
        /// <summary>
        /// list of media preview items associated with this package, shown in the preview window on right click of component
        /// </summary>
        public List<Media> PictureList = new List<Media>();
        /// <summary>
        /// provides a nice complete path that is more human readable than a packagePath
        /// </summary>
        public override string CompletePath
        {
            get
            {
                if(ParentCategory == null)
                {
                    return base.CompletePath;
                }
                //level is taken care of in createModStructure, so use that
                List<string> parentPackages = new List<string>();
                SelectablePackage package = this;
                while(package != null && package.Level > -1)
                {
                    parentPackages.Add(package.NameFormatted);
                    package = package.Parent;
                }
                parentPackages.Add(ParentCategory.Name);
                parentPackages.Reverse();
                return string.Join("->",parentPackages);
            }
        }
        /// <summary>
        /// Allows for alphabetical sorting of mods
        /// </summary>
        /// <param name="x">a package</param>
        /// <param name="y">another package</param>
        /// <returns></returns>
        public static int CompareMods(SelectablePackage x, SelectablePackage y)
        {
            return x.Name.CompareTo(y.Name);
        }
        /// <summary>
        /// DEPRECATED: old (v1.0) method of getting package names
        /// </summary>
        /// <param name="packageNameOld">the name (not packageName) of the package</param>
        /// <returns></returns>
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
        /// <summary>
        /// Allows for display in a combobox and when debugging
        /// </summary>
        /// <returns>The nameFormatted property of the pacakge</returns>
        public override string ToString()
        {
            return NameFormatted;
        }

        /// <summary>
        /// check if the color change should be changed on or off, based on if any other packages at this level are enabled and checked
        /// </summary>
        /// <returns>True if another package at this level is checked and enabled, false otherwise</returns>
        public bool AnyPackagesChecked()
        {
            if (Parent == null)
                return false;
            if (Parent.Packages.Count == 0)
                return false;
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
