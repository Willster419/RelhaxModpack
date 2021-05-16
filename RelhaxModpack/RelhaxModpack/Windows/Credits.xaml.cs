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
    /// Interaction logic for Credits.xaml
    /// </summary>
    public partial class Credits : RelhaxWindow
    {
        /// <summary>
        /// Create an instance of the Credits window
        /// </summary>
        public Credits(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //translate title
            Title = Translations.GetTranslatedString(nameof(Credits));

            StringBuilder creditsBuilder = new StringBuilder();
            //project lead and database managers
            creditsBuilder.AppendFormat("{0}: Willster419{1}{1}", Translations.GetTranslatedString("creditsProjectLeader"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}:{1}", Translations.GetTranslatedString("creditsDatabaseManagers"), Environment.NewLine);
            creditsBuilder.AppendLine(string.Join(", ",new string[] { "elektrosmoker", "Dirty20067", "123GAUSS", "TheIllusion"}));
            creditsBuilder.AppendLine();

            //Translators
            creditsBuilder.AppendFormat("{0}:{1}", Translations.GetTranslatedString("creditsTranslators"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}{2}", Translations.LanguageEnglish, string.Join(", ",new string[] { "Rkk1945", "Willster419"}), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}{2}", Translations.LanguageGerman, string.Join(", ", new string[] {"Grumpelumpf", "Dirty20067", "123GAUSS", "elektrosmoker" }), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}{2}", Translations.LanguageFrench, string.Join(", ", new string[] {"Merkk", "Toshiro" }), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}{2}", Translations.LanguageSpanish, "LordFelix", Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}{2}", Translations.LanguagePolish, string.Join(", ", new string[] {"Neoros","Nullmaruzero" }), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}{2}", Translations.LanguageRussian, "DrWeb7_1", Environment.NewLine);
            creditsBuilder.AppendLine();

            //Open source
            creditsBuilder.AppendFormat("{0}:{1}", Translations.GetTranslatedString("creditsusingOpenSourceProjs"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "Json.NET", "https://www.newtonsoft.com/json", Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "DotNetZip", "https://github.com/haf/DotNetZip.Semverd", Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "TeximpNet", "https://bitbucket.org/Starnick/teximpnet", Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "NAudio", "https://github.com/naudio/NAudio", Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "WindowsApiCodePack", "https://github.com/contre/Windows-API-Code-Pack-1.1", Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "SpriteSheetPacker", "https://github.com/amakaseev/sprite-sheet-packer", Environment.NewLine);
            creditsBuilder.AppendLine();

            //Special thanks
            creditsBuilder.AppendFormat("{0}:{1}", Translations.GetTranslatedString("creditsSpecialThanks"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Grumpelumpf", Translations.GetTranslatedString("creditsGrumpelumpf"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Rkk1945", Translations.GetTranslatedString("creditsRkk1945"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Relic Gaming Community", Translations.GetTranslatedString("creditsRgc"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", Translations.GetTranslatedString("creditsBetaTestersName"), Translations.GetTranslatedString("creditsBetaTesters"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Silvers", Translations.GetTranslatedString("creditsSilvers"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Xantier", Translations.GetTranslatedString("creditsXantier"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Javier Arevalo / Markus Ewald", Translations.GetTranslatedString("creditsSpritePacker"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Wargaming", Translations.GetTranslatedString("creditsWargaming"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}!{1}", Translations.GetTranslatedString("creditsUsersLikeU"), Environment.NewLine);

            CreditsTextBox.Text = creditsBuilder.ToString();
        }
    }
}
