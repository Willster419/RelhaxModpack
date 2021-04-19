using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Reflection;

namespace RelhaxSandbox
{
    public struct CustomBrushSetting
    {
        /// <summary>
        /// The brush for color application
        /// </summary>
        public readonly Brush @Brush;

        /// <summary>
        /// The internal name of the setting
        /// </summary>
        public readonly string SettingName;

        //https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/using-structs
        /// <summary>
        /// Create an instance of the CustomBrushSetting structure
        /// </summary>
        /// <param name="settingName">The internal name of the setting</param>
        /// <param name="brush">The brush for color application</param>
        public CustomBrushSetting(string settingName, Brush brush)
        { Brush = brush; SettingName = settingName; }
    };

    public static class UISettings
    {
        #region Highlighting properties
        //button (highlight)
        private static CustomBrushSetting buttonHighlightBrush = new CustomBrushSetting(nameof(ButtonHighlightBrush), DefaultButtonHighlightBrush);
        public static CustomBrushSetting ButtonHighlightBrush
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
        private static Brush tabItemHighlightBrush;
        public static Brush TabItemHighlightBrush
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

        private static Brush tabItemSelectedBrush;
        public static Brush TabItemSelectedBrush
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
        private static Brush checkboxHighlightBrush;
        public static Brush CheckboxHighlightBrush
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

        private static Brush checkboxCheckmarkBrush;
        public static Brush CheckboxCheckmarkBrush
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
        private static Brush radioButtonHighlightBrush;
        public static Brush RadioButtonHighlightBrush
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

        private static Brush radioButtonCheckmarkBrush;
        public static Brush RadioButtonCheckmarkBrush
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
        private static Brush comboboxOutsideHighlightBrush;
        public static Brush ComboboxOutsideHighlightBrush
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

        private static Brush comboboxInsideColorBrush;
        public static Brush ComboboxInsideColorBrush
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

        private static Brush comboboxOutsideColorBrush;
        public static Brush ComboboxOutsideColorBrush
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

        #region Default UI options
        //x:Key="Button.MouseOver.Background" Color="#FFBEE6FD"
        public static Brush DefaultButtonHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 190, 230, 253));

        /*
           <LinearGradientBrush x:Key="TabItem.MouseOver.Background" EndPoint="0,1" StartPoint="0,0">
            <GradientStop Color="#ECF4FC" Offset="0.0"/>
            <GradientStop Color="#DCECFC" Offset="1.0"/>
           </LinearGradientBrush>
        */
        public static Brush DefaultTabItemHighlightBrush = new LinearGradientBrush(new GradientStopCollection()
        {
            new GradientStop(Color.FromArgb(255,236,244,252),0),
            new GradientStop(Color.FromArgb(255,220,237,252),1)
        })
        {
            EndPoint = new Point(0,1),
            StartPoint = new Point(0,0)
        };

        //x:Key="TabItem.Selected.Background" Color="#FFFFFF"
        public static Brush DefaultTabItemSelectedBrush = new SolidColorBrush(Colors.White);

        //x:Key="OptionMark.MouseOver.Background" Color="#FFF3F9FF"
        public static Brush DefaultCheckboxHighlightBrush = new SolidColorBrush(Color.FromArgb(255,243,249,255));

        //x:Key="OptionMark.MouseOver.Glyph" Color="#FF212121"
        //x:Key="OptionMark.Pressed.Glyph" Color="#FF212121"
        //x:Key="OptionMark.Static.Glyph" Color="#FF212121"
        public static Brush DefaultCheckboxCheckmarkBrush = new SolidColorBrush(Color.FromArgb(255,33,33,33));

        //x:Key="RadioButton.MouseOver.Background" Color="#FFF3F9FF"
        public static Brush DefaultRadioButtonHighlightBrush = new SolidColorBrush(Color.FromArgb(255,243,249,255));

        //x:Key="RadioButton.MouseOver.Glyph" Color="#FF212121"
        //x:Key="RadioButton.Pressed.Glyph" Color="#FF212121"
        //x:Key="RadioButton.Static.Glyph" Color="#FF212121"
        public static Brush DefaultRadioButtonCheckmarkBrush = new SolidColorBrush(Color.FromArgb(255,33,33,33));

        /*
          <LinearGradientBrush x:Key="ComboBox.MouseOver.Background" EndPoint="0,1" StartPoint="0,0">
            <GradientStop Color="#FFECF4FC" Offset="0.0"/>
            <GradientStop Color="#FFDCECFC" Offset="1.0"/>
          </LinearGradientBrush>
        */
        public static Brush DefaultComboboxOutsideHighlightBrush = new LinearGradientBrush(new GradientStopCollection()
        {
            new GradientStop(Color.FromArgb(255,236,244,252),0),
            new GradientStop(Color.FromArgb(255,220,236,252),1)
        })
        {
            EndPoint = new Point(0, 1),
            StartPoint = new Point(0, 0)
        };

        /*
          <LinearGradientBrush x:Key="ComboBox.Static.Background" EndPoint="0,1" StartPoint="0,0">
            <GradientStop Color="#FFF0F0F0" Offset="0.0"/>
            <GradientStop Color="#FFE5E5E5" Offset="1.0"/>
          </LinearGradientBrush>
        */
        public static Brush DefaultComboboxOutsideColorBrush = new LinearGradientBrush(new GradientStopCollection()
        {
            new GradientStop(Color.FromArgb(255,240,240,240),0),
            new GradientStop(Color.FromArgb(255,229,229,229),1)
        })
        {
            EndPoint = new Point(0, 1),
            StartPoint = new Point(0, 0)
        };

        //<Border x:Name="dropDownBorder" BorderBrush="{DynamicResource {x:Static SystemColors.WindowFrameBrushKey}}" BorderThickness="1" Background="{DynamicResource {x:Static SystemColors.WindowBrushKey}}">
        public static Brush DefaultComboboxInsideColorBrush = SystemColors.WindowBrush;

        #endregion

        #region Dark UI options
        public static Brush DarkButtonHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134));

        public static Brush DarkTabItemHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134));
        public static Brush DarkTabItemSelectedBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));

        public static Brush DarkCheckboxHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134));
        public static Brush DarkCheckboxCheckmarkBrush = new SolidColorBrush(Colors.White);

        public static Brush DarkRadioButtonHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134));
        public static Brush DarkRadioButtonCheckmarkBrush = new SolidColorBrush(Colors.White);

        public static Brush DarkComboboxOutsideHighlightBrush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150));
        public static Brush DarkComboboxOutsideColorBrush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134));
        public static Brush DarkComboboxInsideColorBrush = new SolidColorBrush(Colors.Gray);
        #endregion

        #region Init and Property handling code
        public static bool ThemeDefault = true;
        public static void InitUIBrushes()
        {
            ButtonHighlightBrush = UpdateBrush(ButtonHighlightBrush, DefaultButtonHighlightBrush);
            PropertyInfo buttonHighlightBrushProperty = typeof(UISettings).GetProperties().Where(prop => prop.Name.Equals(nameof(ButtonHighlightBrush))).ToList()[0];
            buttonHighlightBrushProperty.SetValue(null, UpdateBrush(ButtonHighlightBrush, DefaultButtonHighlightBrush));

            TabItemHighlightBrush = DefaultTabItemHighlightBrush;
            TabItemSelectedBrush = DefaultTabItemSelectedBrush;

            CheckboxHighlightBrush = DefaultCheckboxHighlightBrush;
            CheckboxCheckmarkBrush = DefaultCheckboxCheckmarkBrush;

            RadioButtonHighlightBrush = DefaultRadioButtonHighlightBrush;
            RadioButtonCheckmarkBrush = DefaultRadioButtonCheckmarkBrush;

            ComboboxInsideColorBrush = DefaultComboboxInsideColorBrush;
            ComboboxOutsideColorBrush = DefaultComboboxOutsideColorBrush;
            ComboboxOutsideHighlightBrush = DefaultComboboxOutsideHighlightBrush;
        }

        public static void ToggleUIBrushes()
        {
            ButtonHighlightBrush = UpdateBrush(ButtonHighlightBrush,ThemeDefault ? DefaultButtonHighlightBrush : DarkButtonHighlightBrush);

            TabItemHighlightBrush = ThemeDefault ? DefaultTabItemHighlightBrush : DarkTabItemHighlightBrush;
            TabItemSelectedBrush = ThemeDefault ? DefaultTabItemSelectedBrush : DarkTabItemSelectedBrush;

            CheckboxHighlightBrush = ThemeDefault ? DefaultCheckboxHighlightBrush : DarkCheckboxHighlightBrush;
            CheckboxCheckmarkBrush = ThemeDefault ? DefaultCheckboxCheckmarkBrush : DarkCheckboxCheckmarkBrush;

            RadioButtonHighlightBrush = ThemeDefault ? DefaultRadioButtonHighlightBrush : DarkRadioButtonHighlightBrush;
            RadioButtonCheckmarkBrush = ThemeDefault ? DefaultRadioButtonCheckmarkBrush : DarkRadioButtonCheckmarkBrush;

            ComboboxInsideColorBrush = ThemeDefault ? DefaultComboboxInsideColorBrush : DarkComboboxInsideColorBrush;
            ComboboxOutsideColorBrush = ThemeDefault ? DefaultComboboxOutsideColorBrush : DarkComboboxOutsideColorBrush;
            ComboboxOutsideHighlightBrush = ThemeDefault ? DefaultComboboxOutsideHighlightBrush : DarkComboboxOutsideHighlightBrush;
        }

        public static CustomBrushSetting UpdateBrush(CustomBrushSetting brush, Brush newBrush)
        {
            return new CustomBrushSetting(brush.SettingName,newBrush);
        }

        //https://stackoverflow.com/questions/34762879/static-binding-doesnt-update-when-resource-changes
        public static event PropertyChangedEventHandler StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
