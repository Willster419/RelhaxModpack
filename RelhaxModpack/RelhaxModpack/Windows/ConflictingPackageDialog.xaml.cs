using RelhaxModpack.Database;
using RelhaxModpack.Settings;
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
using System.Windows.Shapes;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for ConflictingPackageDialog.xaml
    /// </summary>
    public partial class ConflictingPackageDialog : RelhaxWindow
    {
        public bool OptionASelected { get; set; } = false;

        public SelectablePackage PackageToTryToSelect { get; set; }

        public ConflictingPackageDialog(ModpackSettings modpackSettings, SelectablePackage packageToTryToSelect) : base(modpackSettings)
        {
            InitializeComponent();
            this.PackageToTryToSelect = packageToTryToSelect;
        }

        private void SelectOptionAButton_Click(object sender, RoutedEventArgs e)
        {
            OptionASelected = true;
            this.Close();
        }

        private void SelectOptionBButton_Click(object sender, RoutedEventArgs e)
        {
            OptionASelected = false;
            this.Close();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine(string.Format(Translations.GetTranslatedString("conflictingPackageMessagePartA"), PackageToTryToSelect.NameFormatted));
            messageBuilder.AppendLine();

            foreach (SelectablePackage conflictingPackage in PackageToTryToSelect.GetConflictingPackages())
            {
                messageBuilder.AppendLine(string.Format(Translations.GetTranslatedString("conflictingPackagePackageOfCategory"), conflictingPackage.NameFormatted, conflictingPackage.ParentCategory.Name));
            }
            messageBuilder.AppendLine();

            messageBuilder.AppendLine(string.Format(Translations.GetTranslatedString("conflictingPackageMessagePartB"), PackageToTryToSelect.NameFormatted));
            messageBuilder.AppendLine(string.Format(Translations.GetTranslatedString("conflictingPackageMessagePartC"), PackageToTryToSelect.NameFormatted));
            messageBuilder.AppendLine();

            messageBuilder.AppendLine(Translations.GetTranslatedString("conflictingPackageMessagePartD"));
            messageBuilder.AppendLine(Translations.GetTranslatedString("conflictingPackageMessagePartE"));

            SelectOptionATextblock.Text = Translations.GetTranslatedString("conflictingPackageMessageOptionA");
            SelectOptionBTextblock.Text = Translations.GetTranslatedString("conflictingPackageMessageOptionB");
            ConflictingPackageTextbox.Text = messageBuilder.ToString();
        }
    }
}
