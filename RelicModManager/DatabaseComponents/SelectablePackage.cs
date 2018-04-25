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
        //the name of the package for the user display
        public string Name = "";
        public string NameFormatted
        {
            get {
                    return Name.Replace("{version}",Version);
                    //string replace takes much less time than replaceMacro
                    //also nameFormatted currently only ever has {version} macro
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
                    if (Parent.RelhaxFormComboBoxList[0] != null)
                    {
                        Parent.RelhaxFormComboBoxList[0].OnDropDownSelectionChanged(this, value);
                    }
                    else
                    {
                        Parent.RelhaxWPFComboBoxList[0].OnDropDownSelectionChanged(this,value);
                    }
                }
                else if (Type.Equals("single_dropdown2") && Enabled)
                {
                    if (Parent.RelhaxFormComboBoxList[1] != null)
                    {
                        Parent.RelhaxFormComboBoxList[1].OnDropDownSelectionChanged(this,value);
                    }
                    else
                    {
                        Parent.RelhaxWPFComboBoxList[1].OnDropDownSelectionChanged(this,value);
                    }
                }
                switch (Settings.SView)
                {
                    //default view UI selection code
                    case Settings.SelectionView.Default:
                        switch (_Checked)
                        {
                            case true:
                                //handle color change code
                                if (!Settings.DisableColorChange)
                                {
                                    if (ParentPanel != null && ParentPanel.BackColor != Color.BlanchedAlmond)
                                    {
                                        ParentPanel.BackColor = Color.BlanchedAlmond;
                                    }
                                }
                                break;
                            case false:
                                //handle color change code
                                if (!Settings.DisableColorChange)
                                {
                                    if (ParentPanel != null && !AnyPackagesChecked())
                                        ParentPanel.BackColor = Settings.getBackColor();
                                }
                                break;
                        }
                        break;
                    //WPF treeview is done with treeviewItem
                    case Settings.SelectionView.Legacy:
                        switch (_Checked)
                        {
                            case true:
                                //handle color change code
                                if (!Settings.DisableColorChange)
                                {
                                    
                                }
                                break;
                            case false:
                                //handle color change code
                                if (!Settings.DisableColorChange)
                                {

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
        //the list of all dropDown options for each package type
        public RelhaxFormComboBox[] RelhaxFormComboBoxList = new RelhaxFormComboBox[2];
        public RelhaxWPFComboBox[] RelhaxWPFComboBoxList = new RelhaxWPFComboBox[2];
        //the TreeViewItem for WPF
        public System.Windows.Controls.TreeViewItem @TreeViewItem = new System.Windows.Controls.TreeViewItem();
        //the TreeViewItem for winForms
        public RelhaxFormTreeNode TreeNode = new RelhaxFormTreeNode();
        //the panel that this package sits in
        public Panel ParentPanel = null;
        public Panel ChildPanel = null;
        //the list of cache files that should be backed up before wiping the directory
        public List<string> UserFiles = new List<string>();
        //the list of SelectablePackage entries within this instance of SelectablePackages
        public List<SelectablePackage> Packages = new List<SelectablePackage>();
        //list of LogicalDependency and Dependency package names used for linking the dependencies and logicaldependencies
        public List<LogicalDependency> LogicalDependencies = new List<LogicalDependency>();
        public List<Dependency> Dependencies = new List<Dependency>();
        //list of media preview items associated with this package
        public List<Media> PictureList = new List<Media>();
        public SelectablePackage() { }
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
