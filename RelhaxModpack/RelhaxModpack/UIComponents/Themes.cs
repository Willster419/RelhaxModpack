using RelhaxModpack.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RelhaxModpack.UIComponents
{
    public static class Themes
    {
        public static Theme Default = new Theme()
        {
            ThemeName = "Default",
            FileName = string.Empty,
            SelectionListSelectedPanelColor = new CustomBrush()
            {
                IsValid = true,
                Brush = new SolidColorBrush(Colors.BlanchedAlmond)
            },
            SelectionListNotSelectedPanelColor = new CustomBrush()
            {
                IsValid = true,
                Brush = new SolidColorBrush(Colors.White)
            },
            SelectionListSelectedTextColor = new CustomBrush()
            {
                IsValid = true,
                Brush = SystemColors.ControlTextBrush
            },
            SelectionListNotSelectedTextColor = new CustomBrush()
            {
                IsValid = true,
                Brush = SystemColors.ControlTextBrush
            },
            ButtonColorset = new ClassColorset()
            {
                ClassType = typeof(Button),
                //empty definition(s) to be built at runtime
                BackgroundBrush = null,
                ForegroundBrush = null,
                SelectedBrush = null,
                HighlightBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.ButtonHighlightBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 190, 230, 253))
                }
            },
            TabItemColorset = new ClassColorset()
            {
                ClassType = typeof(TabItem),
                BackgroundBrush = null,
                ForegroundBrush = null,
                HighlightBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.TabItemHighlightBrush),
                    IsValid = true,
                    Brush = new LinearGradientBrush(new GradientStopCollection()
                    {
                        new GradientStop(Color.FromArgb(255, 236, 244, 252), 0),
                        new GradientStop(Color.FromArgb(255, 220, 237, 252), 1)
                    })
                    {
                        EndPoint = new Point(0, 1),
                        StartPoint = new Point(0, 0)
                    }
                },
                SelectedBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.TabItemSelectedBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Colors.White)
                }
            },
            CheckboxColorset = new ClassColorset()
            {
                ClassType = typeof(CheckBox),
                BackgroundBrush = null,
                ForegroundBrush = null,
                HighlightBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.CheckboxHighlightBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 243, 249, 255))
                },
                SelectedBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.CheckboxCheckmarkBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 33, 33, 33))
                }
            },
            RadioButtonColorset = new ClassColorset()
            {
                ClassType = typeof(RadioButton),
                BackgroundBrush = null,
                ForegroundBrush = null,
                HighlightBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.RadioButtonHighlightBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 243, 249, 255)),
                },
                SelectedBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.RadioButtonCheckmarkBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 33, 33, 33)),
                }
            },
            ComboboxColorset = new ClassColorset()
            {
                ClassType = typeof(ComboBox),
                BackgroundBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.ComboboxOutsideColorBrush),
                    IsValid = true,
                    Brush = new LinearGradientBrush(new GradientStopCollection()
                    {
                        new GradientStop(Color.FromArgb(255, 240, 240, 240), 0),
                        new GradientStop(Color.FromArgb(255, 229, 229, 229), 1)
                    })
                    {
                        EndPoint = new Point(0, 1),
                        StartPoint = new Point(0, 0)
                    }
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.ComboboxInsideColorBrush),
                    IsValid = true,
                    Brush = SystemColors.WindowBrush
                },
                HighlightBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.ComboboxOutsideHighlightBrush),
                    IsValid = true,
                    Brush = new LinearGradientBrush(new GradientStopCollection()
                    {
                        new GradientStop(Color.FromArgb(255, 236, 244, 252), 0),
                        new GradientStop(Color.FromArgb(255, 220, 236, 252), 1)
                    })
                    {
                        EndPoint = new Point(0, 1),
                        StartPoint = new Point(0, 0)
                    }
                },
                SelectedBrush = null
            },
            //empty definition to not apply any changes
            ControlColorset = new ClassColorset()
            {
                ClassType = typeof(Control)
            },
            BorderColorset = new ClassColorset()
            {
                ClassType = typeof(Border)
            },
            PanelColorset = new ClassColorset()
            {
                ClassType = typeof(Panel)
            },
            TextblockColorset = new ClassColorset()
            {
                ClassType = typeof(TextBlock)
            },
            WindowColorsets = new Dictionary<Type, WindowColorset>()
        };

        #region Dark UI reusable colors
        //white text
        private static SolidColorBrush DarkThemeTextColor = new SolidColorBrush(Colors.White);

        //very dark grey background
        private static SolidColorBrush DarkThemeBackground = new SolidColorBrush(Color.FromArgb(255, 26, 26, 26));

        //dark gray but lighter for button
        private static SolidColorBrush DarkThemeButton = new SolidColorBrush(Color.FromArgb(255, 42, 42, 42));

        private static SolidColorBrush DarkThemeImageButtonBackground = new SolidColorBrush(Color.FromArgb(255, 175, 175, 175));
        #endregion

        public static Theme Dark = new Theme()
        {
            ThemeName = "Dark",
            FileName = string.Empty,
            SelectionListSelectedPanelColor = new CustomBrush()
            {
                IsValid = true,
                Brush = new SolidColorBrush(Colors.BlanchedAlmond)
            },
            SelectionListNotSelectedPanelColor = new CustomBrush()
            {
                IsValid = true,
                Brush = new SolidColorBrush(Colors.White)
            },
            SelectionListSelectedTextColor = new CustomBrush()
            {
                IsValid = true,
                Brush = SystemColors.ControlTextBrush
            },
            SelectionListNotSelectedTextColor = new CustomBrush()
            {
                IsValid = true,
                Brush = SystemColors.ControlTextBrush
            },
            ButtonColorset = new ClassColorset()
            {
                ClassType = typeof(Button),
                //empty definition(s)
                BackgroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeButton
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeTextColor
                },
                SelectedBrush = null,
                HighlightBrush = new CustomBrush()
                {
                    IsBound = true,
                    //this string name will need to link with the property name
                    BoundPropertyName = nameof(BoundUISettings.ButtonHighlightBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134))
                }
            },
            TabItemColorset = new ClassColorset()
            {
                ClassType = typeof(TabItem),
                BackgroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeBackground
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeTextColor
                },
                HighlightBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.TabItemHighlightBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134))
                },
                SelectedBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.TabItemSelectedBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150))
                }
            },
            CheckboxColorset = new ClassColorset()
            {
                ClassType = typeof(CheckBox),
                BackgroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeBackground
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeTextColor
                },
                HighlightBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.CheckboxHighlightBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134))
                },
                SelectedBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.CheckboxCheckmarkBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Colors.White)
                }
            },
            RadioButtonColorset = new ClassColorset()
            {
                ClassType = typeof(RadioButton),
                BackgroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeBackground
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeTextColor
                },
                HighlightBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.RadioButtonHighlightBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134)),
                },
                SelectedBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.RadioButtonCheckmarkBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Colors.White),
                }
            },
            ComboboxColorset = new ClassColorset()
            {
                ClassType = typeof(ComboBox),
                BackgroundBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.ComboboxOutsideColorBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134))
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.ComboboxInsideColorBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Colors.Gray)
                },
                HighlightBrush = new CustomBrush()
                {
                    IsBound = true,
                    BoundPropertyName = nameof(BoundUISettings.ComboboxOutsideHighlightBrush),
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150))
                },
                SelectedBrush = null
            },
            PanelColorset = new ClassColorset()
            {
                ClassType = typeof(Panel),
                BackgroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeBackground
                }
            },
            BorderColorset = new ClassColorset()
            {
                ClassType = typeof(Border),
                BackgroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeBackground
                }
            },
            TextblockColorset = new ClassColorset()
            {
                ClassType = typeof(TextBlock),
                BackgroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeBackground
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeTextColor
                }
            },
            ControlColorset = new ClassColorset()
            {
                ClassType = typeof(Control),
                BackgroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeBackground
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsBound = false,
                    IsValid = true,
                    Brush = DarkThemeTextColor
                }
            },
            WindowColorsets = new Dictionary<Type, WindowColorset>()
            {
                {
                    typeof(MainWindow), new WindowColorset()
                    {
                        WindowType = typeof(MainWindow),
                        BackgroundBrush = new CustomBrush()
                        {
                            IsBound = false,
                            IsValid = true,
                            Brush = DarkThemeBackground
                        },
                        ComponentColorsets = new Dictionary<string, ComponentColorset>()
                        {
                            {
                                "HomepageButtonImageBorder", new ComponentColorset()
                                {
                                    ID = "HomepageButtonImageBorder",
                                    ForegroundBrush = null,
                                    BackgroundBrush = new CustomBrush()
                                    {
                                        IsBound = false,
                                        IsValid = true,
                                        Brush = DarkThemeImageButtonBackground
                                    }
                                }
                            },
                            {
                                "FindBugAddModButtonImageBorder", new ComponentColorset()
                                {
                                    ID = "FindBugAddModButtonImageBorder",
                                    ForegroundBrush = null,
                                    BackgroundBrush = new CustomBrush()
                                    {
                                        IsBound = false,
                                        IsValid = true,
                                        Brush = DarkThemeImageButtonBackground
                                    }
                                }
                            },
                            {
                                "SendEmailButtonImageBorder", new ComponentColorset()
                                {
                                    ID = "SendEmailButtonImageBorder",
                                    ForegroundBrush = null,
                                    BackgroundBrush = new CustomBrush()
                                    {
                                        IsBound = false,
                                        IsValid = true,
                                        Brush = DarkThemeImageButtonBackground
                                    }
                                }
                            },
                            {
                                "DonateButtonImageBorder", new ComponentColorset()
                                {
                                    ID = "DonateButtonImageBorder",
                                    ForegroundBrush = null,
                                    BackgroundBrush = new CustomBrush()
                                    {
                                        IsBound = false,
                                        IsValid = true,
                                        Brush = DarkThemeImageButtonBackground
                                    }
                                }
                            },
                        }
                    }
                },
                {
                    typeof(Diagnostics), new WindowColorset()
                    {
                        WindowType = typeof(Diagnostics),
                        BackgroundBrush = new CustomBrush()
                        {
                            IsBound = false,
                            IsValid = true,
                            Brush = DarkThemeBackground
                        }
                    }
                }
            }
        };
    }
}
