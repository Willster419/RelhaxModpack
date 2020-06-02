using RelhaxModpack.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// The staic location for all color definitions that need to be databound by WPF
    /// </summary>
    public static class BoundUISettings
    {
        #region Highlighting properties
        //button (highlight)
        private static CustomBrush buttonHighlightBrush = Themes.Default.ButtonColorset.HighlightBrush;

        /// <summary>
        /// Get or set the Button highlight color
        /// </summary>
        public static CustomBrush ButtonHighlightBrush
        {
            get
            {
                return buttonHighlightBrush;
            }
            set
            {
                buttonHighlightBrush = value;
                OnStaticPropertyChanged(nameof(ButtonHighlightBrush));
            }
        }

        //tabControl (highlight and selected)
        private static CustomBrush tabItemHighlightBrush = Themes.Default.TabItemColorset.HighlightBrush;

        /// <summary>
        /// Get or set the TabItem highlight color
        /// </summary>
        public static CustomBrush TabItemHighlightBrush
        {
            get
            {
                return tabItemHighlightBrush;
            }
            set
            {
                tabItemHighlightBrush = value;
                OnStaticPropertyChanged(nameof(TabItemHighlightBrush));
            }
        }

        private static CustomBrush tabItemSelectedBrush = Themes.Default.TabItemColorset.SelectedBrush;

        /// <summary>
        /// Get or set the TabItem selected (mouse down) color
        /// </summary>
        public static CustomBrush TabItemSelectedBrush
        {
            get
            {
                return tabItemSelectedBrush;
            }
            set
            {
                tabItemSelectedBrush = value;
                OnStaticPropertyChanged(nameof(TabItemSelectedBrush));
            }
        }

        //checkbox (highlight and mark)
        private static CustomBrush checkboxHighlightBrush = Themes.Default.CheckboxColorset.HighlightBrush;

        /// <summary>
        /// Get of set the Checkbox highlight color
        /// </summary>
        public static CustomBrush CheckboxHighlightBrush
        {
            get
            {
                return checkboxHighlightBrush;
            }
            set
            {
                checkboxHighlightBrush = value;
                OnStaticPropertyChanged(nameof(CheckboxHighlightBrush));
            }
        }

        private static CustomBrush checkboxCheckmarkBrush = Themes.Default.CheckboxColorset.SelectedBrush;

        /// <summary>
        /// Get or set the Checkbox checkmark color (selected)
        /// </summary>
        public static CustomBrush CheckboxCheckmarkBrush
        {
            get
            {
                return checkboxCheckmarkBrush;
            }
            set
            {
                checkboxCheckmarkBrush = value;
                OnStaticPropertyChanged(nameof(CheckboxCheckmarkBrush));
            }
        }

        //radioButton (highlight and mark)
        private static CustomBrush radioButtonHighlightBrush = Themes.Default.RadioButtonColorset.HighlightBrush;

        /// <summary>
        /// Get or set the RadioButton highlight color
        /// </summary>
        public static CustomBrush RadioButtonHighlightBrush
        {
            get
            {
                return radioButtonHighlightBrush;
            }
            set
            {
                radioButtonHighlightBrush = value;
                OnStaticPropertyChanged(nameof(RadioButtonHighlightBrush));
            }
        }

        private static CustomBrush radioButtonCheckmarkBrush = Themes.Default.RadioButtonColorset.SelectedBrush;

        /// <summary>
        /// Get or set the RadioButton checkmark color (selected)
        /// </summary>
        public static CustomBrush RadioButtonCheckmarkBrush
        {
            get
            {
                return radioButtonCheckmarkBrush;
            }
            set
            {
                radioButtonCheckmarkBrush = value;
                OnStaticPropertyChanged(nameof(RadioButtonCheckmarkBrush));
            }
        }

        //combobox (highlight, outside color, inside color)
        private static CustomBrush comboboxOutsideHighlightBrush = Themes.Default.ComboboxColorset.HighlightBrush;

        /// <summary>
        /// Get or set the combobox highlight background color
        /// </summary>
        public static CustomBrush ComboboxOutsideHighlightBrush
        {
            get
            {
                return comboboxOutsideHighlightBrush;
            }
            set
            {
                comboboxOutsideHighlightBrush = value;
                OnStaticPropertyChanged(nameof(ComboboxOutsideHighlightBrush));
            }
        }

        private static CustomBrush comboboxInsideColorBrush = Themes.Default.ComboboxColorset.ForegroundBrush;

        /// <summary>
        /// Get or set the combobox text color of the inside list items
        /// </summary>
        public static CustomBrush ComboboxInsideColorBrush
        {
            get
            {
                return comboboxInsideColorBrush;
            }
            set
            {
                comboboxInsideColorBrush = value;
                OnStaticPropertyChanged(nameof(ComboboxInsideColorBrush));
            }
        }

        private static CustomBrush comboboxOutsideColorBrush = Themes.Default.ComboboxColorset.BackgroundBrush;

        /// <summary>
        /// Get or set the combobox background color
        /// </summary>
        public static CustomBrush ComboboxOutsideColorBrush
        {
            get
            {
                return comboboxOutsideColorBrush;
            }
            set
            {
                comboboxOutsideColorBrush = value;
                OnStaticPropertyChanged(nameof(ComboboxOutsideColorBrush));
            }
        }

        #endregion

        #region Binding utilities
        /// <summary>
        /// Event handler for when a static property is changed in code
        /// </summary>
        /// <remarks>See https://stackoverflow.com/questions/34762879/static-binding-doesnt-update-when-resource-changes </remarks>
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        private static CustomBrush UpdateBrush(CustomBrush brushToApply)
        {
            return new CustomBrush
            {
                IsValid = brushToApply.IsValid,
                Brush = brushToApply.Brush
            };
        }

        /// <summary>
        /// Set the color of one of the properties in this class. The name of the property should be inside 
        /// </summary>
        /// <param name="customBrush">The color and property to update</param>
        public static void SetBrushProperty(CustomPropertyBrush customBrush)
        {
            if(string.IsNullOrWhiteSpace(customBrush.BrushPropertyName))
            {
                throw new ArgumentException("customBrush.BrushPropertyName is missing value");
            }
            typeof(BoundUISettings).GetProperty(customBrush.BrushPropertyName).SetValue(null, UpdateBrush(customBrush));
        }

        private static bool IsComponentBound(FrameworkElement element, DependencyProperty property)
        {
            BindingExpression expression = BindingOperations.GetBindingExpression(element, property);
            return expression != null;
        }
        #endregion
    }
}
