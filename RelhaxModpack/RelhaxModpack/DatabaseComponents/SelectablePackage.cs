using RelhaxModpack.UIComponents;
using RelhaxModpack.DatabaseComponents;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;

namespace RelhaxModpack
{
    /// <summary>
    /// The types of UI selections for building the selection tree
    /// </summary>
    public enum SelectionTypes
    {
        /// <summary>
        /// Used as catch-all for any mis-assigned selection types
        /// </summary>
        none,

        /// <summary>
        /// A radio button selection (only one of many), can have children
        /// </summary>
        single1,

        /// <summary>
        /// A combobox selection (only one of many), cannot have children
        /// </summary>
        single_dropdown1,

        /// <summary>
        /// Another combobox selection (only one of many), cannot have children
        /// </summary>
        single_dropdown2,

        /// <summary>
        /// A checkbox selection (many of many), can have children
        /// </summary>
        multi
    }
    /// <summary>
    /// A package that can be selected in the UI, most commonly a mod or a configuration parameter for a mod
    /// </summary>
    public class SelectablePackage : DatabasePackage, IComponentWithDependencies
    {
        #region XML Parsing

        private static readonly List<string> SelectableElementsToXmlParseAttributes = new List<string>()
        {
            nameof(Name),
            nameof(Type),
            nameof(Visible)
        };

        private static readonly List<string> SelectableElementsToXmlParseNodes = new List<string>()
        {
            nameof(Description),
            nameof(UpdateComment),
            nameof(PopularMod),
            nameof(GreyAreaMod),
            nameof(ShowInSearchList),
            nameof(Medias),
            nameof(UserFiles),
            nameof(ConflictingPackages),
            nameof(Dependencies),
            nameof(Packages)
        };

        /// <summary>
        /// Gets a list of fields (including from base classes) that can be parsed as xml attributes
        /// </summary>
        /// <returns>The string list</returns>
        new public static List<string> FieldsToXmlParseAttributes()
        {
            return DatabasePackage.FieldsToXmlParseAttributes().Concat(SelectableElementsToXmlParseAttributes).ToList();
        }

        /// <summary>
        /// Gets a list of fields (including from base classes) that can be parsed as xml elements
        /// </summary>
        /// <returns>The string list</returns>
        new public static List<string> FieldsToXmlParseNodes()
        {
            return DatabasePackage.FieldsToXmlParseNodes().Concat(SelectableElementsToXmlParseNodes).ToList();
        }
        #endregion

        #region Database Properties
        /// <summary>
        /// Create an instance of the SelectablePackage class and over-ride DatabasePackage default values
        /// </summary>
        public SelectablePackage()
        {
            InstallGroup = 4;
            PatchGroup = 4;
        }
        /// <summary>
        /// The display name of the package
        /// </summary>
        public string Name = string.Empty;

        /// <summary>
        /// The name of the package with the version macro replaced for use display
        /// </summary>
        public string NameFormatted
        {
            get
            {
                return Name
                    .Replace(@"{version}", Version)
                    .Replace(@"{author}", Author);
            }
        }

        /// <summary>
        /// The Category object reference
        /// </summary>
        public Category ParentCategory = null;

        /// <summary>
        /// The type of selectable package logic to follow (see SelectionTypes enumeration for options)
        /// </summary>
        public SelectionTypes Type = SelectionTypes.none;

        /// <summary>
        /// The reference for the direct parent of this package
        /// </summary>
        public SelectablePackage Parent = null;

        /// <summary>
        /// The reference for the absolute top of the package tree
        /// </summary>
        public SelectablePackage TopParent = null;

        /// <summary>
        /// A flag to determine whether or not the mod should be shown in UI
        /// </summary>
        public bool Visible = false;

        /// <summary>
        /// Update comments of the package
        /// </summary>
        public string UpdateComment = string.Empty;

        /// <summary>
        /// description of the package
        /// </summary>
        public string Description = string.Empty;

        /// <summary>
        /// Flag to determine if the package is popular
        /// </summary>
        public bool PopularMod = false;

        /// <summary>
        /// Flag to determine if the package is of controversial nature, or if the developer is a controversial source
        /// </summary>
        public bool GreyAreaMod = false;

        /// <summary>
        /// Flag to determine any packages of this package should be sorted (by name)
        /// </summary>
        public bool SortChildPackages = false;

        /// <summary>
        /// Used as internal flag for if application settings is checked "SaveDisabledModsInSelection". Allows for disabled mods to be saved back to the user's selection
        /// </summary>
        public bool FlagForSelectionSave = false;

        /// <summary>
        /// Field for whether the package is selected to install
        /// </summary>
        protected internal bool _Checked = false;

        /// <summary>
        /// Property for if the package is selected by the user to install. handles all color change and single_dropdown updating code
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
                int dropDownSelectionType = -1;
                //inside here is for not comboboxes (checked)
                if (UIComponent != null)
                {
                    UIComponent.OnCheckedChanged(value);
                }
                if (Type == SelectionTypes.single_dropdown1)
                {
                    dropDownSelectionType = 0;
                }
                else if (Type == SelectionTypes.single_dropdown2)
                {
                    dropDownSelectionType = 1;
                }
                //inside here is for comboboxes (checked)
                if (Enabled && dropDownSelectionType > -1 && IsStructureEnabled)
                {
                    switch (ModpackSettings.ModSelectionView)
                    {
                        case SelectionView.DefaultV2:
                            Parent.RelhaxWPFComboBoxList[dropDownSelectionType].OnDropDownSelectionChanged(this, value);
                            break;
                        case SelectionView.Legacy:
                            Parent.RelhaxWPFComboBoxList[dropDownSelectionType].OnDropDownSelectionChanged(this, value);
                            break;
                    }
                }
                //inside here is for color change
                bool actuallyDoColorChange = false;
                if (ModpackSettings.ModSelectionView == SelectionView.DefaultV2 && ModpackSettings.EnableColorChangeDefaultV2View)
                    actuallyDoColorChange = true;
                else if (ModpackSettings.ModSelectionView == SelectionView.Legacy && ModpackSettings.EnableColorChangeLegacyView)
                    actuallyDoColorChange = true;
                if(UIComponent != null && actuallyDoColorChange)
                {
                    //set panel and text color based on true or false
                    switch(_Checked)
                    {
                        case true:
                            if (UIComponent.PanelColor != UISettings.SelectedPanelColor)
                                UIComponent.PanelColor = UISettings.SelectedPanelColor;
                                UIComponent.TextColor = UISettings.SelectedTextColor;
                            break;
                        case false:
                            if (!AnyPackagesChecked())
                            {
                                if (UIComponent.PanelColor != UISettings.NotSelectedPanelColor)
                                    UIComponent.PanelColor = UISettings.NotSelectedPanelColor;
                                UIComponent.TextColor = UISettings.NotSelectedTextColor;
                            }
                            break;
                    }
                }
                else if (Visible && dropDownSelectionType > -1 && actuallyDoColorChange && IsStructureVisible)
                {
                    //in here means it's a dropdown and doing color change
                    switch (_Checked)
                    {
                        case true:
                            if (ParentBorder.Background != UISettings.SelectedPanelColor)
                                ParentBorder.Background = UISettings.SelectedPanelColor;
                            break;
                        case false:
                            if (!AnyPackagesChecked())
                            {
                                if (ParentBorder.Background != UISettings.NotSelectedPanelColor)
                                    ParentBorder.Background = UISettings.NotSelectedPanelColor;
                            }
                            break;
                    }
                }
                //toggle the Tab Color based on if anything is selected, done for level -1 top item
                if(Level == -1)
                {
                    //if the color is not saved yet, then save what the default currently is
                    if (UISettings.NotSelectedTabColor == null)
                    {
                        //windows 10 uses a linear gradient brush (at least mine does)
                        //windows 7 in classic theme uses a solid color brush
                        UISettings.NotSelectedTabColor = TabIndex.Background;
                    }
                    if (_Checked)
                        TabIndex.Background = UISettings.SelectedPanelColor;
                    else
                        TabIndex.Background = UISettings.NotSelectedTabColor;
                    //workaround for legacy:
                    //top item is not going to correct color
                    if (ModpackSettings.ModSelectionView == SelectionView.Legacy)
                    {
                        if (_Checked)
                            TreeView.Background = UISettings.SelectedPanelColor;
                        else
                            TreeView.Background = UISettings.NotSelectedPanelColor;
                    }
                    else if (ModpackSettings.ModSelectionView == SelectionView.DefaultV2)
                        if (!_Checked)
                            ParentBorder.Background = UISettings.NotSelectedPanelColor;
                }
            }
        }

        /// <summary>
        /// Overrides DatabasePackage.Enabled property. Used to toggle if the mod should be selectable and installed in the selection list.
        /// The override also enables the triggering of the UI components to reflect the user's selection changes.
        /// </summary>
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

        /// <summary>
        /// The list of cache files that should be backed up before wiping the directory
        /// </summary>
        public List<UserFile> UserFiles = new List<UserFile>();

        /// <summary>
        /// The list of child SelectablePackage entries in this instance of SelectablePackages
        /// </summary>
        public List<SelectablePackage> Packages = new List<SelectablePackage>();

        /// <summary>
        /// List of media preview items associated with this package, shown in the preview window on right click of component
        /// </summary>
        public List<Media> Medias = new List<Media>();

        /// <summary>
        /// A list of packages (from dependencies list) that this package is dependent on in order to be installed
        /// </summary>
        public List<DatabaseLogic> Dependencies = new List<DatabaseLogic>();

        /// <summary>
        /// Property of Dependencies list to allow for interface implementation
        /// </summary>
        public List<DatabaseLogic> DependenciesProp { get { return Dependencies; } set { Dependencies = value; } }

        /// <summary>
        /// A list of any SelectablePackages that conflict with this mod. A conflict will result the package not being processed.
        /// Refer to examples for more information
        /// </summary>
        public List<string> ConflictingPackages = new List<string>();

        /// <summary>
        /// Toggle if the package should appear in the search list
        /// </summary>
        public bool ShowInSearchList = true;
        #endregion

        #region UI Properties Shared
        /// <summary>
        /// The UI element reference for this package
        /// </summary>
        public IPackageUIComponent UIComponent;

        /// <summary>
        /// The UI element reference for the direct parent of this package
        /// </summary>
        public IPackageUIComponent ParentUIComponent;

        /// <summary>
        /// The UI element reference for the absolute top of the package tree
        /// </summary>
        public IPackageUIComponent TopParentUIComponent;

        /// <summary>
        /// The list of WPF combo boxes for each combobox type
        /// </summary>
        public RelhaxWPFComboBox[] RelhaxWPFComboBoxList;

        /// <summary>
        /// The border for the legacy view to allow for putting all children in the border. sits inside TreeViewItem. WPF component
        /// </summary>
        public Border ChildBorder;

        /// <summary>
        /// The StackPanel to allow the child TreeViewItems to stack upon each other. sits inside the border. WPF component
        /// </summary>
        public StackPanel ChildStackPanel;

        /// <summary>
        /// The border that this component is in. WPF component
        /// </summary>
        public Border ParentBorder;

        /// <summary>
        /// The StackPanel that this item is inside. WPF component
        /// </summary>
        public StackPanel ParentStackPanel;
        #endregion

        #region UI Properties Default View
        /// <summary>
        /// ContentControl item to allow for right-clicking of disabled components. defaultv2 WPF component
        /// </summary>
        public ContentControl @ContentControl;

        /// <summary>
        /// Component used only in the top SelectablePackage to allow for scrolling of the package lists for each category
        /// </summary>
        public ScrollViewer @ScrollViewer;
        #endregion

        #region UI Properties OMC Legacy View
        /// <summary>
        /// The TreeViewItem that corresponds to this package. legacy WPF component
        /// </summary>
        public StretchingTreeViewItem @TreeViewItem;

        /// <summary>
        /// The TreeView that this package is in. legacy WPF component
        /// </summary>
        public StretchingTreeView @TreeView;

        /// <summary>
        /// The TabItem UI reference
        /// </summary>
        public TabItem TabIndex;
        #endregion

        #region Other Properties and Methods
        /// <summary>
        /// Provides a complete path of the name fields from the top package down to where this package is located in the tree
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
        /// Provides a complete path of the packageName fields from the top package down to where this package is located in the tree
        /// </summary>
        public override string CompletePackageNamePath
        {
            get
            {
                if (ParentCategory == null)
                {
                    return base.CompletePackageNamePath;
                }
                //level is taken care of in createModStructure, so use that
                List<string> parentPackages = new List<string>();
                SelectablePackage package = this;
                while (package != null && package.Level > -1)
                {
                    parentPackages.Add(package.PackageName);
                    package = package.Parent;
                }
                parentPackages.Reverse();
                return string.Join("->", parentPackages);
            }
        }

        /// <summary>
        /// Determines if the UI package structure to this package is of all visible components.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public bool IsStructureVisible
        {
            get
            {
                if (Parent == null || TopParent == null)
                    throw new BadMemeException("RUN THE LINKING CODE FIRST");
                if (!Visible) return false;
                bool structureVisible = true;
                SelectablePackage parentRef = Parent;
                //TopParent is the category header and thus is always visible
                while (parentRef != TopParent)
                {
                    if (!parentRef.Visible)
                    {
                        structureVisible = false;
                        //at this point it's false so we can exit early and save a few cycles
                        break;
                    }
                    parentRef = parentRef.Parent;
                }
                return structureVisible;
            }
        }

        /// <summary>
        /// Determines if the UI package structure to this package is of all enabled components.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public bool IsStructureEnabled
        {
            get
            {
                if (Parent == null || TopParent == null)
                    throw new BadMemeException("RUN THE LINKING CODE FIRST");
                if (!Enabled) return false;
                bool structureEnabled = true;
                SelectablePackage parentRef = Parent;
                while (parentRef != TopParent)
                {
                    if (!parentRef.Enabled)
                    {
                        structureEnabled = false;
                        //at this point it's false so we can exit early and save a few cycles
                        break;
                    }
                    parentRef = parentRef.Parent;
                }
                return structureEnabled;
            }
        }

        /// <summary>
        /// Returns the display name of the package for the UI, with version macros replaced and any other statuses appended
        /// </summary>
        public string NameDisplay
        {
            get
            {
                //get the original formatted name
                //get if the package is forced visible
                //get if the package is forced enabled
                //get if the package is need to be download "(updated)" text (and size)
                //(only happens if level > -1)
                string nameDisplay = NameFormatted;
                if (ModpackSettings.ForceVisible && !IsStructureVisible)
                    nameDisplay = string.Format("{0} [{1}]", nameDisplay, Translations.GetTranslatedString("invisible"));
                if (ModpackSettings.ForceEnabled && !IsStructureEnabled)
                    nameDisplay = string.Format("{0} [{1}]", nameDisplay, Translations.GetTranslatedString("disabled"));
                if(Level > -1 && DownloadFlag)
                {
                    nameDisplay = string.Format("{0} ({1})", nameDisplay, Translations.GetTranslatedString("updated"));
                    if (Size > 0)
                        nameDisplay = string.Format("{0} ({1})", nameDisplay, Utils.SizeSuffix(Size, 1, true));
                }
                //escape character fix
                return nameDisplay.Replace("_","__");
            }
        }

        /// <summary>
        /// Returns a string representation of the timestamp of when the zip file of this package was last modified
        /// </summary>
        public string TimeStampString
        {
            get
            {
                return Utils.ConvertFiletimeTimestampToDate(Timestamp);
            }
        }

        /// <summary>
        /// Returns the display tool tip string, or the translation string for "no description"
        /// </summary>
        public string ToolTipString
        {
            get
            {
                string toolTipResult = string.IsNullOrWhiteSpace(Description) ?
                    Translations.GetTranslatedString("noDescription") : Description;
                return string.Format("{0}\n\n{1}{2}",
                    toolTipResult, Translations.GetTranslatedString("lastUpdated"), TimeStampString).Replace("_","__");
            }
        }

        /// <summary>
        /// Alphabetical sorting of packages by PackageName property at this level (not recursive)
        /// </summary>
        /// <param name="x">First package to compare</param>
        /// <param name="y">Second package to compare</param>
        /// <returns></returns>
        public static int CompareModsPackageName(SelectablePackage x, SelectablePackage y)
        {
            return x.PackageName.CompareTo(y.PackageName);
        }

        /// <summary>
        /// Alphabetical sorting of packages by NameFormatted property at this level (not recursive)
        /// </summary>
        /// <param name="x">First package to compare</param>
        /// <param name="y">Second package to compare</param>
        /// <returns></returns>
        public static int CompareModsName(SelectablePackage x, SelectablePackage y)
        {
            return x.NameFormatted.CompareTo(y.NameFormatted);
        }

        /// <summary>
        /// Allows for display in a combobox and when debugging
        /// </summary>
        /// <returns>The nameFormatted property of the package</returns>
        public override string ToString()
        {
            return NameFormatted;
        }

        /// <summary>
        /// Check if the color change should be changed on or off, based on if any other packages at this level are enabled and checked
        /// </summary>
        /// <returns>True if another package at this level is checked and enabled, false otherwise</returns>
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

        /// <summary>
        /// Create an instance of the SelectablePackage class and over-ride DatabasePackage default values, while using values provided for copy objects
        /// </summary>
        /// <param name="packageToCopyFrom">The package to copy the information from</param>
        /// <param name="deep">Set to true to copy list objects, false to use new lists</param>
        public SelectablePackage(DatabasePackage packageToCopyFrom, bool deep) : base(packageToCopyFrom,deep)
        {
            InstallGroup = 4;
            PatchGroup = 4;

            if (packageToCopyFrom is Dependency dep)
            {
                if(deep)
                {
                    foreach (DatabaseLogic file in dep.Dependencies)
                        this.Dependencies.Add(DatabaseLogic.Copy(file));
                }
            }
            else if (packageToCopyFrom is SelectablePackage sp)
            {
                this.Type = sp.Type;
                this.Name = "WRITE_NEW_NAME";
                this.Visible = sp.Visible;
                this.Size = 0;

                this.UpdateComment = string.Empty;
                this.Description = string.Empty;
                this.PopularMod = false;
                this._Checked = false;

                this.Level = -2;
                this.UserFiles = new List<UserFile>();
                this.Packages = new List<SelectablePackage>();
                this.Medias = new List<Media>();
                this.Dependencies = new List<DatabaseLogic>();
                this.ConflictingPackages = new List<string>();
                this.ShowInSearchList = sp.ShowInSearchList;

                if (deep)
                {
                    this.UpdateComment = sp.UpdateComment;
                    this.Description = sp.Description;
                    this.PopularMod = sp.PopularMod;
                    this._Checked = sp._Checked;

                    foreach (UserFile file in this.UserFiles)
                        this.UserFiles.Add(UserFile.DeepCopy(file));

                    foreach (Media file in this.Medias)
                        this.Medias.Add(Media.Copy(file));

                    foreach (DatabaseLogic file in this.Dependencies)
                        this.Dependencies.Add(DatabaseLogic.Copy(file));
                }
            }
        }
        #endregion
    }
}
