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
using System.Reflection;
using RelhaxModpack.Utilities.Enums;
using RelhaxModpack.Settings;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for FirstLaunchSelectLanguage.xaml
    /// </summary>
    public partial class FirstLoadSelectLanguage : RelhaxWindow
    {
        /// <summary>
        /// Flag to determine if the user selected a language
        /// </summary>
        public bool Continue = false;

        /// <summary>
        /// Create and instance of the FirstLoadSelectLanguage Window
        /// </summary>
        public FirstLoadSelectLanguage(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //load all languages supported into the list
            //get all strings where name starts with language
            FieldInfo[] languageFields = typeof(Translations).GetFields();
            languageFields = languageFields.Where(item => item.FieldType.Equals(typeof(string)) && item.Name.Contains("Language")).ToArray();
            foreach(FieldInfo field in languageFields)
            {
                RadioButton button = new RadioButton()
                {
                    Tag = field.Name,
                    Content = field.GetValue(typeof(Translations)) as string
                };
                if (button.Tag.Equals(nameof(Translations.LanguageEnglish)))
                    button.IsChecked = true;
                else
                    button.IsChecked = false;
                button.Checked += Button_Checked;
                SelectLanguagesStackPanel.Children.Add(button);
            }
        }

        private void Button_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton button = sender as RadioButton;
            Languages language = Languages.English;
            string buttonLanguage = (button.Tag as string).Substring(8);
            if (Enum.TryParse(buttonLanguage, out Languages languagee))
            {
                language = languagee;
            }
            else
            {
                Logging.Error("failed to parse language form button name: {0}", buttonLanguage);
            }
            this.ModpackSettings.Language = language;
            Translations.SetLanguage(language);
            Translations.LocalizeWindow(this, false);
        }

        private void SelectLanguagesContinueButton_Click(object sender, RoutedEventArgs e)
        {
            Continue = true;
            Close();
        }
    }
}
