using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// The root object for all definitions needed for the UI engine
    /// </summary>
    public class Theme
    {
        /// <summary>
        /// The name of the theme
        /// </summary>
        public string ThemeName { get; set; } = string.Empty;

        /// <summary>
        /// The xml theme filename
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// The color definition for panel components in the selection view when at least one package is checked
        /// </summary>
        public CustomBrush SelectionListSelectedPanelColor { get; set; } = null;

        /// <summary>
        /// The color definition for panel components in the selection view when no packages are checked
        /// </summary>
        public CustomBrush SelectionListNotSelectedPanelColor { get; set; } = null;

        /// <summary>
        /// The color definition for 'single' and 'multi' package types when it is checked
        /// </summary>
        public CustomBrush SelectionListSelectedTextColor { get; set; } = null;

        /// <summary>
        /// The color definition for 'single' and 'multi' package types when it is not checked
        /// </summary>
        public CustomBrush SelectionListNotSelectedTextColor { get; set; } = null;

        /// <summary>
        /// The color definition for border components in the selection view, for holding a sub level of packages
        /// </summary>
        public CustomBrush SelectionListBorderColor { get; set; } = null;

        /// <summary>
        /// The color definition for the background of the tab header
        /// </summary>
        public CustomBrush SelectionListActiveTabHeaderBackgroundColor { get; set; } = null;

        /// <summary>
        /// The color definition for the text color of the tab header
        /// </summary>
        public CustomBrush SelectionListActiveTabHeaderTextColor { get; set; } = null;

        /// <summary>
        /// The color definition for background of a not active tab header, when at least one component is selected
        /// </summary>
        public CustomBrush SelectionListNotActiveHasSelectionsBackgroundColor { get; set; } = null;

        /// <summary>
        /// The color definition for text of a not active tab header, when at least one component is selected
        /// </summary>
        public CustomBrush SelectionListNotActiveHasSelectionsTextColor { get; set; } = null;

        /// <summary>
        /// The color definition for background of a not active tab header, when no components are selected
        /// </summary>
        public CustomBrush SelectionListNotActiveHasNoSelectionsBackgroundColor { get; set; } = null;

        /// <summary>
        /// The color definition for text of a not active tab header, when no components are selected
        /// </summary>
        public CustomBrush SelectionListNotActiveHasNoSelectionsTextColor { get; set; } = null;

        /// <summary>
        /// The set of rules to use for what parts of a RadioButton UI object can be themed
        /// </summary>
        public ClassColorset RadioButtonColorset { get; set; } = null;

        /// <summary>
        /// The set of rules to use for what parts of a Checkbox UI object can be themed
        /// </summary>
        public ClassColorset CheckboxColorset { get; set; } = null;

        /// <summary>
        /// The set of rules to use for what parts of a Button UI object can be themed
        /// </summary>
        public ClassColorset ButtonColorset { get; set; } = null;

        /// <summary>
        /// The set of rules to use for what parts of a TabItem UI object can be themed
        /// </summary>
        public ClassColorset TabItemColorset { get; set; } = null;

        /// <summary>
        /// The set of rules to use for what parts of a Combobox UI object can be themed
        /// </summary>
        public ClassColorset ComboboxColorset { get; set; } = null;

        /// <summary>
        /// The set of rules to use for what parts of a Panel UI object can be themed
        /// </summary>
        public ClassColorset PanelColorset { get; set; } = null;

        /// <summary>
        /// The set of rules to use for what parts of a Textblock UI object can be themed
        /// </summary>
        public ClassColorset TextblockColorset { get; set; } = null;

        /// <summary>
        /// The set of rules to use for what parts of a Border UI object can be themed
        /// </summary>
        public ClassColorset BorderColorset { get; set; } = null;

        /// <summary>
        /// The set of rules to use for what parts of a Control UI object can be themed
        /// </summary>
        public ClassColorset ControlColorset { get; set; } = null;

        /// <summary>
        /// The set of rules to use for what parts of a ProgressBar UI object can be themed
        /// </summary>
        public ClassColorset ProgressBarColorset { get; set; } = null;

        /// <summary>
        /// The set of rules to use for what parts of a RelhaxHyperlink UI object can be themed
        /// </summary>
        public ClassColorset RelhaxHyperlinkColorset { get; set; } = null;

        /// <summary>
        /// A list of rules to use for each window definition type
        /// </summary>
        public Dictionary<Type, WindowColorset> WindowColorsets { get; set; } = null;
        
        /// <summary>
        /// Returns a complete list of all bounded brushes for each class component color definition
        /// </summary>
        public List<CustomPropertyBrush> BoundedClassColorsetBrushes
        {
            get
            {
                List<CustomPropertyBrush> list = new List<CustomPropertyBrush>();
                list.AddRange(RadioButtonColorset.BoundedBrushes);
                list.AddRange(CheckboxColorset.BoundedBrushes);
                list.AddRange(ButtonColorset.BoundedBrushes);
                list.AddRange(TabItemColorset.BoundedBrushes);
                list.AddRange(ComboboxColorset.BoundedBrushes);
                list.AddRange(PanelColorset.BoundedBrushes);
                list.AddRange(TextblockColorset.BoundedBrushes);
                list.AddRange(BorderColorset.BoundedBrushes);
                list.AddRange(ControlColorset.BoundedBrushes);
                list.AddRange(ProgressBarColorset.BoundedBrushes);
                return list;
            }
        }
    }
}
