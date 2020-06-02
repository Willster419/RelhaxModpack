using RelhaxModpack.UI.ClassThemeDefinitions;
using RelhaxModpack.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// A static list of themes used in the application
    /// </summary>
    public static class Themes
    {
        /// <summary>
        /// The default theme used in the application
        /// </summary>
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
            SelectionListBorderColor = new CustomBrush()
            {
                IsValid = true,
                Brush = Brushes.Black
            },
            SelectionListActiveTabHeaderBackgroundColor = new CustomBrush()
            {
                IsValid = true,
                Brush = Brushes.White
            },
            SelectionListActiveTabHeaderTextColor = new CustomBrush()
            {
                IsValid = true,
                Brush = SystemColors.ControlTextBrush
            },
            SelectionListNotActiveHasNoSelectionsBackgroundColor = new CustomBrush()
            {
                IsValid = false,
                Brush = null
            },
            SelectionListNotActiveHasNoSelectionsTextColor = new CustomBrush()
            {
                IsValid = true,
                Brush = SystemColors.ControlTextBrush
            },
            SelectionListNotActiveHasSelectionsBackgroundColor = new CustomBrush()
            {
                IsValid = true,
                Brush = new SolidColorBrush(Colors.BlanchedAlmond)
            },
            SelectionListNotActiveHasSelectionsTextColor = new CustomBrush()
            {
                IsValid = true,
                Brush = SystemColors.ControlTextBrush
            },

            ButtonColorset = new ClassColorset()
            {
                ClassThemeDefinition = new ButtonClassThemeDefinition(),
                //empty definition(s) to be built at runtime
                BackgroundBrush = null,
                ForegroundBrush = null,
                SelectedBrush = null,
                HighlightBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 190, 230, 253))
                }
            },
            TabItemColorset = new ClassColorset()
            {
                ClassThemeDefinition = new TabItemClassThemeDefinition(),
                BackgroundBrush = null,
                ForegroundBrush = null,
                HighlightBrush = new CustomBrush()
                {
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
                    IsValid = true,
                    Brush = new SolidColorBrush(Colors.White)
                }
            },
            CheckboxColorset = new ClassColorset()
            {
                ClassThemeDefinition = new CheckboxClassThemeDefinition(),
                BackgroundBrush = null,
                ForegroundBrush = null,
                HighlightBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 243, 249, 255))
                },
                SelectedBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 33, 33, 33))
                }
            },
            RadioButtonColorset = new ClassColorset()
            {
                ClassThemeDefinition = new RadioButtonClassThemeDefinition(),
                //gotten at run time
                BackgroundBrush = null,
                //gotten at run time
                ForegroundBrush = null,
                HighlightBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 243, 249, 255)),
                },
                SelectedBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 33, 33, 33)),
                }
            },
            ComboboxColorset = new ClassColorset()
            {
                ClassThemeDefinition = new ComboboxClassThemeDefinition(),
                BackgroundBrush = new CustomBrush()
                {
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
                    IsValid = true,
                    Brush = SystemColors.WindowBrush
                },
                HighlightBrush = new CustomBrush()
                {
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
            RelhaxHyperlinkColorset = new ClassColorset()
            {
                ClassThemeDefinition = new RelhaxHyperlinkClassThemeDefinition(),
                ForegroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = Brushes.Blue
                }
            },
            //empty definition to not apply any changes
            //colorSet object and xml object MUST exist for parsing to work!
            ControlColorset = new ClassColorset()
            {
                ClassThemeDefinition = new ControlClassThemeDefinition()
            },
            BorderColorset = new ClassColorset()
            {
                ClassThemeDefinition = new BorderClassThemeDefinition()
            },
            PanelColorset = new ClassColorset()
            {
                ClassThemeDefinition = new PanelClassThemeDefinition()
            },
            TextblockColorset = new ClassColorset()
            {
                ClassThemeDefinition = new TextBlockClassThemeDefinition()
            },
            ProgressBarColorset = new ClassColorset()
            {
                ClassThemeDefinition = new ProgressbarClassThemeDefinition()
            },
            WindowColorsets = new Dictionary<Type, WindowColorset>()
        };

        #region Dark UI reusable colors
        //white text
        private static SolidColorBrush DarkThemeTextColor = new SolidColorBrush(Colors.White);

        //very dark gray background
        private static SolidColorBrush DarkThemeBackground = new SolidColorBrush(Color.FromArgb(255, 26, 26, 26));

        //dark gray but lighter for button
        private static SolidColorBrush DarkThemeButton = new SolidColorBrush(Color.FromArgb(255, 42, 42, 42));

        private static SolidColorBrush DarkThemeActiveTab = new SolidColorBrush(Color.FromArgb(255, 100, 100, 100));

        private static SolidColorBrush DarkThemeImageButtonBackground = new SolidColorBrush(Color.FromArgb(255, 175, 175, 175));
        #endregion

        /// <summary>
        /// A pre-built dark theme
        /// </summary>
        public static Theme Dark = new Theme()
        {
            ThemeName = "Dark",
            FileName = string.Empty,

            SelectionListSelectedPanelColor = new CustomBrush()
            {
                IsValid = true,
                Brush = DarkThemeButton
            },
            SelectionListNotSelectedPanelColor = new CustomBrush()
            {
                IsValid = true,
                Brush = DarkThemeBackground
            },
            SelectionListSelectedTextColor = new CustomBrush()
            {
                IsValid = true,
                Brush = DarkThemeTextColor
            },
            SelectionListNotSelectedTextColor = new CustomBrush()
            {
                IsValid = true,
                Brush = DarkThemeTextColor
            },
            SelectionListBorderColor = new CustomBrush()
            {
                IsValid = true,
                Brush = Brushes.LightGray
            },
            SelectionListActiveTabHeaderBackgroundColor = new CustomBrush()
            {
                IsValid = true,
                Brush = DarkThemeActiveTab
            },
            SelectionListActiveTabHeaderTextColor = new CustomBrush()
            {
                IsValid = true,
                Brush = DarkThemeTextColor
            },
            SelectionListNotActiveHasNoSelectionsBackgroundColor = new CustomBrush()
            {
                IsValid = false,
                Brush = DarkThemeBackground
            },
            SelectionListNotActiveHasNoSelectionsTextColor = new CustomBrush()
            {
                IsValid = true,
                Brush = DarkThemeTextColor
            },
            SelectionListNotActiveHasSelectionsBackgroundColor = new CustomBrush()
            {
                IsValid = true,
                Brush = new SolidColorBrush(Colors.BlanchedAlmond)
            },
            SelectionListNotActiveHasSelectionsTextColor = new CustomBrush()
            {
                IsValid = true,
                Brush = SystemColors.ControlTextBrush
            },

            ButtonColorset = new ClassColorset()
            {
                ClassThemeDefinition = new ButtonClassThemeDefinition(),
                //empty definition(s)
                BackgroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeButton
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeTextColor
                },
                SelectedBrush = null,
                HighlightBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134))
                }
            },
            TabItemColorset = new ClassColorset()
            {
                ClassThemeDefinition = new TabItemClassThemeDefinition(),
                BackgroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeBackground
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeTextColor
                },
                HighlightBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134))
                },
                SelectedBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150))
                }
            },
            CheckboxColorset = new ClassColorset()
            {
                ClassThemeDefinition = new CheckboxClassThemeDefinition(),
                BackgroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeBackground
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeTextColor
                },
                HighlightBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134))
                },
                SelectedBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Colors.White)
                }
            },
            RadioButtonColorset = new ClassColorset()
            {
                ClassThemeDefinition = new RadioButtonClassThemeDefinition(),
                BackgroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeBackground
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeTextColor
                },
                HighlightBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134)),
                },
                SelectedBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Colors.White),
                }
            },
            ProgressBarColorset = new ClassColorset()
            {
                ClassThemeDefinition = new ProgressbarClassThemeDefinition(),
                BackgroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeButton
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new LinearGradientBrush(new GradientStopCollection()
                    {
                        new GradientStop(Color.FromArgb(95, 95, 95, 95), 0),
                        new GradientStop(Color.FromArgb(255, 0, 0, 255), 1)
                    })
                    {
                        EndPoint = new Point(0.75, 1),
                        StartPoint = new Point(0.25, 0)
                    }
                }
            },
            ComboboxColorset = new ClassColorset()
            {
                ClassThemeDefinition = new ComboboxClassThemeDefinition(),
                BackgroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 134, 134, 134))
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Colors.Gray)
                },
                HighlightBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = new SolidColorBrush(Color.FromArgb(255, 150, 150, 150))
                },
                SelectedBrush = null
            },
            PanelColorset = new ClassColorset()
            {
                ClassThemeDefinition = new PanelClassThemeDefinition(),
                BackgroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeBackground
                }
            },
            BorderColorset = new ClassColorset()
            {
                ClassThemeDefinition = new BorderClassThemeDefinition(),
                BackgroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeBackground
                }
            },
            TextblockColorset = new ClassColorset()
            {
                ClassThemeDefinition = new TextBlockClassThemeDefinition(),
                BackgroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeBackground
                },
                ForegroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeTextColor
                }
            },
            RelhaxHyperlinkColorset = new ClassColorset()
            {
                ClassThemeDefinition = new RelhaxHyperlinkClassThemeDefinition(),
                ForegroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = Brushes.LightBlue
                }
            },
            ControlColorset = new ClassColorset()
            {
                ClassThemeDefinition = new ControlClassThemeDefinition(),
                BackgroundBrush = new CustomBrush()
                {
                    IsValid = true,
                    Brush = DarkThemeBackground
                },
                ForegroundBrush = new CustomBrush()
                {
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
                            IsValid = true,
                            Brush = DarkThemeBackground
                        }
                    }
                },
                {
                    typeof(NewsViewer), new WindowColorset()
                    {
                        WindowType = typeof(NewsViewer),
                        BackgroundBrush = new CustomBrush()
                        {
                            IsValid = true,
                            Brush = DarkThemeBackground
                        }
                    }
                },
                {
                    typeof(AddPicturesZip), new WindowColorset()
                    {
                        WindowType = typeof(AddPicturesZip),
                        BackgroundBrush = new CustomBrush()
                        {
                            IsValid = true,
                            Brush = DarkThemeBackground
                        }
                    }
                },
                {
                    typeof(RelhaxColorPicker), new WindowColorset()
                    {
                        WindowType = typeof(RelhaxColorPicker),
                        BackgroundBrush = new CustomBrush()
                        {
                            IsValid = true,
                            Brush = DarkThemeBackground
                        }
                    }
                },
                {
                    typeof(DeveloperSelectionsViewer), new WindowColorset()
                    {
                        WindowType = typeof(DeveloperSelectionsViewer),
                        BackgroundBrush = new CustomBrush()
                        {
                            IsValid = true,
                            Brush = DarkThemeBackground
                        }
                    }
                },
                {
                    typeof(GameCenterUpdateDownloader), new WindowColorset()
                    {
                        WindowType = typeof(GameCenterUpdateDownloader),
                        BackgroundBrush = new CustomBrush()
                        {
                            IsValid = true,
                            Brush = DarkThemeBackground
                        }
                    }
                },
                {
                    typeof(Credits), new WindowColorset()
                    {
                        WindowType = typeof (Credits),
                        BackgroundBrush = new CustomBrush()
                        {
                            IsValid = true,
                            Brush = DarkThemeBackground
                        }
                    }
                },
                {
                    typeof(SelectionFileIssuesDisplay), new WindowColorset()
                    {
                        WindowType = typeof (SelectionFileIssuesDisplay),
                        BackgroundBrush = new CustomBrush()
                        {
                            IsValid = true,
                            Brush = DarkThemeBackground
                        }
                    }
                },
                {
                    typeof(VersionInfo), new WindowColorset()
                    {
                        WindowType = typeof(VersionInfo),
                        BackgroundBrush = new CustomBrush()
                        {
                            IsValid = true,
                            Brush = DarkThemeBackground
                        },
                        ComponentColorsets = new Dictionary<string, ComponentColorset>()
                        {
                            {
                                "VersionInfoNoButton", new ComponentColorset()
                                {
                                    ID = "VersionInfoNoButton",
                                    ForegroundBrush = null,
                                    BackgroundBrush = new CustomBrush()
                                    {
                                        IsValid = true,
                                        Brush = Brushes.Red
                                    }
                                }
                            },
                            {
                                "VersionInfoYesButton", new ComponentColorset()
                                {
                                    ID = "VersionInfoYesButton",
                                    ForegroundBrush = null,
                                    BackgroundBrush = new CustomBrush()
                                    {
                                        IsValid = true,
                                        Brush = Brushes.Green
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        /// <summary>
        /// The custom theme to be loaded from xml by the user
        /// </summary>
        public static Theme Custom = null;
    }
}
