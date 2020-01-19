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
            creditsBuilder.AppendFormat("{0}: Willster419{1}{1}", "Project Leader", nl);
            creditsBuilder.AppendFormat("{0}:{1}", "Database Managers", nl);
            creditsBuilder.AppendLine(string.Join(", ",new string[] { "Elektrosmoker", "Dirty20067", "123GAUSS", "TheIllusion"}));
            creditsBuilder.AppendLine();

            //Translators
            creditsBuilder.AppendFormat("{0}:{1}", "Translators", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "English", string.Join(", ",new string[] { "Rkk1945", "Willster419"}), nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "German", string.Join(", ", new string[] {"Grumpelumpf", "Dirty20067", "123GAUSS", "Elektrosmoker" }), nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "French", string.Join(", ", new string[] {"Merkk", "Toshiro" }), nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "Spanish", "LordFelix", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "Polish", string.Join(", ", new string[] {"Neoros","Nullmaruzero" }), nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "Russian", "DrWeb7_1", nl);
            creditsBuilder.AppendLine();

            //Open source
            creditsBuilder.AppendFormat("{0}:{1}", "Relhax Modpack uses the following Open Source projects", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "Json.NET", "https://www.newtonsoft.com/json", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "DotNetZip", "https://github.com/haf/DotNetZip.Semverd", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "TeximpNet", "https://bitbucket.org/Starnick/teximpnet", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "NAudio", "https://github.com/naudio/NAudio", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "WindowsApiCodePack", "https://github.com/contre/Windows-API-Code-Pack-1.1", nl);
            creditsBuilder.AppendFormat("{0}: {1}{2}", "SpriteSheetPacker", "https://github.com/amakaseev/sprite-sheet-packer", nl);
            creditsBuilder.AppendLine();

            //Special thanks
            creditsBuilder.AppendFormat("{0}:{1}", "Special thanks", nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Grumpelumpf", "Project leader of OMC, allowed us to pick up where he left off", nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Rkk1945", "The first beta tester who worked with me for months to get the project running", nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Relic Gaming Community", "Sponsoring the modpack and being my first beta tester group", nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Our Beta Testing team", "Continuing to test and report issues in the application before it goes live", nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Silvers", "Helping with the community outreach and social networking", nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Xantier", "Initial IT support and setting up our server", nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Javier Arevalo and Markus Ewald", "Developing the sprite sheet packer algorithm and porting to .NET", nl);
            creditsBuilder.AppendFormat("{0}: {1}.{2}", "Wargaming", "Making an easy to automate modding system", nl);
            creditsBuilder.Append("Users like you!");

            CreditsTextBox.Text = creditsBuilder.ToString();
        }
    }
}
