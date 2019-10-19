using RelhaxModpack.UIComponents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace RelhaxModpack.UIComponents
{
    public static class BoundUISettings
    {
        #region Highlighting properties
        //button (highlight)
        private static CustomBrush buttonHighlightBrush = Themes.Default.ButtonColorset.HighlightBrush;
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

        #region binding utilities
        //https://stackoverflow.com/questions/34762879/static-binding-doesnt-update-when-resource-changes

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

        public static void SetBrushProperty(CustomPropertyBrush customBrush)
        {
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
