using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace RelhaxModpack.UI
{
    /// <summary>
    /// Interaction logic for RelhaxWPFCheckBox.xaml
    /// </summary>
    public partial class RelhaxWPFCheckBox : CheckBox, IPackageUIComponent
    {
        protected bool iconsSet = false;

        /// <summary>
        /// Create an instance of the RelhaxWPFCheckBox class
        /// </summary>
        public RelhaxWPFCheckBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// The package associated with this UI component
        /// </summary>
        public SelectablePackage Package { get; set; }

        /// <summary>
        /// Change any UI parent class properties that depends on the enabled SelectablePackage
        /// </summary>
        /// <param name="Enabled">The value from the SelectablePackage</param>
        public virtual void OnEnabledChanged(bool Enabled)
        {
            IsEnabled = Enabled;
        }

        /// <summary>
        /// Change any UI parent class properties that depends on the checked SelectablePackage
        /// </summary>
        /// <param name="Checked">The value from the SelectablePackage</param>
        public virtual void OnCheckedChanged(bool Checked)
        {
            IsChecked = Checked;

            if (Package.ChangeColorOnValueChecked && Package.Visible && Package.IsStructureVisible)
            {
                if (Checked || Package.AnyPackagesChecked())
                {
                    Package.ParentBorder.IsChildPackageChecked = true;
                }
                else
                {
                    Package.ParentBorder.IsChildPackageChecked = false;
                }
            }
        }

        private void TemplateRoot_Loaded(object sender, RoutedEventArgs e)
        {
            if (iconsSet)
                return;
            Grid templateGrid = sender as Grid;
            if (Package.ObfuscatedMod)
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
                    Source = new BitmapImage(new Uri(@"/RelhaxModpack;component/Resources/Images/grey_area_mod.png",UriKind.Relative))
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
            if (Package.FromWGmods)
            {
                templateGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(16) });
                Image img = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Top,
                    Width = 16,
                    Height = 16,
                    Source = new BitmapImage(new Uri(@"/RelhaxModpack;component/Resources/Images/wgmods_package.png", UriKind.Relative))
                };
                templateGrid.Children.Add(img);
                Grid.SetColumn(img, templateGrid.ColumnDefinitions.Count - 1);
            }
            iconsSet = true;
        }
    }
}
