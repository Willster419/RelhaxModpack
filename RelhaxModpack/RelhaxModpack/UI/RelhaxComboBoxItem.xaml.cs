using RelhaxModpack.Database;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace RelhaxModpack.UI
{
    /// <summary>
    /// Interaction logic for RelhaxComboBoxItem.xaml
    /// </summary>
    public partial class RelhaxComboBoxItem : System.Windows.Controls.ComboBoxItem
    {
        /// <summary>
        /// The SelectablePackage object that is being wrapped around
        /// </summary>
        public SelectablePackage Package { get; set; }

        /// <summary>
        /// The text to display in the Combobox
        /// </summary>
        public string DisplayName { get; set; }


        private bool IconsApplied = false;

        /// <summary>
        /// Creates an instance of the RelhaxComboBoxItem class
        /// </summary>
        /// <param name="package">The package to wrap around</param>
        /// <param name="display">The text to display in the Combobox</param>
        public RelhaxComboBoxItem(SelectablePackage package, string display)
        {
            InitializeComponent();
            Package = package;
            DisplayName = display;
        }

        /// <summary>
        /// Allows for displaying of custom text in the Combobox
        /// </summary>
        /// <returns>The text to display in the Combobox (DisplayName property)</returns>
        public override string ToString()
        {
            return DisplayName;
        }

        private void TemplateRootPanel_Loaded(object sender, RoutedEventArgs e)
        {
            if (IconsApplied)
                return;
            StackPanel templatePanel = sender as StackPanel;
            if (Package.ObfuscatedMod)
            {
                Image img = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 16,
                    Height = 16,
                    Source = new BitmapImage(new Uri(@"/RelhaxModpack;component/Resources/Images/obfuscated_package_icon.png", UriKind.Relative))
                };
                templatePanel.Children.Add(img);
            }
            if (Package.GreyAreaMod)
            {
                Image img = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 16,
                    Height = 16,
                    Source = new BitmapImage(new Uri(@"/RelhaxModpack;component/Resources/Images/grey_area_mod.png", UriKind.Relative))
                };
                templatePanel.Children.Add(img);
            }
            if (Package.PopularMod)
            {
                Image img = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 16,
                    Height = 16,
                    Source = new BitmapImage(new Uri(@"/RelhaxModpack;component/Resources/Images/popular_icon.png", UriKind.Relative))
                };
                templatePanel.Children.Add(img);
            }
            if (Package.FromWGmods)
            {
                Image img = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 16,
                    Height = 16,
                    Source = new BitmapImage(new Uri(@"/RelhaxModpack;component/Resources/Images/wgmods_package.png", UriKind.Relative))
                };
            }
            IconsApplied = true;
        }
    }
}
