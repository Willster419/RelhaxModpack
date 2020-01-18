using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RelhaxModpack.UIComponents
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

        private void TemplateRootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (IconsApplied)
                return;
            Grid templateGrid = sender as Grid;
            if (true)
            {
                templateGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(16) });
                Image img = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 16,
                    Height = 16,
                    Source = new BitmapImage(new Uri(@"/RelhaxModpack;component/Resources/Images/obfuscated_package_icon.png", UriKind.Relative))
                }; templateGrid.Children.Add(img);
                Grid.SetColumn(img, templateGrid.ColumnDefinitions.Count - 1);
            }
            if (Package.GreyAreaMod)
            {
                templateGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(16) });
                Image img = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 16,
                    Height = 16,
                    Source = new BitmapImage(new Uri(@"/RelhaxModpack;component/Resources/Images/grey_area_mod.png", UriKind.Relative))
                }; templateGrid.Children.Add(img);
                Grid.SetColumn(img, templateGrid.ColumnDefinitions.Count - 1);
            }
            if (Package.PopularMod)
            {
                templateGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(16) });
                Image img = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 16,
                    Height = 16,
                    Source = new BitmapImage(new Uri(@"/RelhaxModpack;component/Resources/Images/popular_icon.png", UriKind.Relative))
                };
                templateGrid.Children.Add(img);
                Grid.SetColumn(img, templateGrid.ColumnDefinitions.Count - 1);
            }
            IconsApplied = true;
        }
    }
}
