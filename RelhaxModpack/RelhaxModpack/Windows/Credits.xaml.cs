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
    /// Interaction logic for Credits.xaml
    /// </summary>
    public partial class Credits : RelhaxWindow
    {

        private readonly string nl = Environment.NewLine;

        public Credits()
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //translate title
            Title = Translations.GetTranslatedString(nameof(Credits));

            StringBuilder creditsBuilder = new StringBuilder();
            //project lead and database managers
            creditsBuilder.AppendFormat("{0}: Willster419{1}{1}", Translations.GetTranslatedString("creditsProjectLeader"), nl);
            creditsBuilder.AppendFormat("{0}:{1}", "XVM Specialtist", string.Join(", ", new string[] { "elektrosmoker" }), nl);
            creditsBuilder.AppendFormat("{0}:{1}", "Consistancy Manager", string.Join(", ", new string[] { "elektrosmoker" }), nl);
            creditsBuilder.AppendFormat("{0}:{1}", "Configuration Builder", string.Join(", ", new string[] { "The Illusion" }), nl);
            creditsBuilder.AppendFormat("{0}:{1}", "Custom Tank Skins Specialtist", string.Join(", ", new string[] { "The Illusion" }), nl);
            creditsBuilder.AppendFormat("{0}:{1}", Translations.GetTranslatedString("creditsDatabaseManagers"), nl);
            creditsBuilder.AppendLine(string.Join(", ",new string[] { "elektrosmoker", "dirty20067", "123Gauss", "The Illusion"}));
            creditsBuilder.AppendLine();

            //Translators
            creditsBuilder.AppendFormat("{0}:{1}", Translations.GetTranslatedString("creditsTranslators"), nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", Translations.LanguageEnglish, string.Join(", ",new string[] { "Rkk1945", "Willster419"}), nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", Translations.LanguageGerman, string.Join(", ", new string[] {"Grumpelumpf", "dirty20067", "123Gauss", "elektrosmoker" }), nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", Translations.LanguageFrench, string.Join(", ", new string[] {"Merkk", "Toshiro" }), nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", Translations.LanguageSpanish, "LordFelix", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", Translations.LanguagePolish, string.Join(", ", new string[] {"Neoros","Nullmaruzero" }), nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", Translations.LanguageRussian, "DrWeb7_1", nl);
            creditsBuilder.AppendLine();

            //Open source
            creditsBuilder.AppendFormat("{0}:{1}", Translations.GetTranslatedString("creditsusingOpenSourceProjs"), nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "Json.NET", "https://www.newtonsoft.com/json", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "DotNetZip", "https://github.com/haf/DotNetZip.Semverd", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "TeximpNet", "https://bitbucket.org/Starnick/teximpnet", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "NAudio", "https://github.com/naudio/NAudio", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "WindowsApiCodePack", "https://github.com/contre/Windows-API-Code-Pack-1.1", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "SpriteSheetPacker", "https://github.com/amakaseev/sprite-sheet-packer", nl);
            creditsBuilder.AppendLine();

            //Special thanks
            creditsBuilder.AppendFormat("{0}:{1}", Translations.GetTranslatedString("creditsSpecialThanks"), nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Grumpelumpf", Translations.GetTranslatedString("creditsGrumpelumpf"), nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Rkk1945", Translations.GetTranslatedString("creditsRkk1945"), nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Relic Gaming Community", Translations.GetTranslatedString("creditsRgc"), nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", Translations.GetTranslatedString("creditsBetaTestersName"), Translations.GetTranslatedString("creditsBetaTesters"), nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Silvers", Translations.GetTranslatedString("creditsSilvers"), nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Xantier", Translations.GetTranslatedString("creditsXantier"), nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Javier Arevalo / Markus Ewald", Translations.GetTranslatedString("creditsSpritePacker"), nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Wargaming", Translations.GetTranslatedString("creditsWargaming"), nl);
            creditsBuilder.AppendFormat("{0}!{1}", Translations.GetTranslatedString("creditsUsersLikeU"), nl);

            CreditsTextBox.Text = creditsBuilder.ToString();
        }
    }
}
