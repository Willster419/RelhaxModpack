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
    /// Interaction logic for EndOfLife.xaml
    /// </summary>
    public partial class EndOfLife : RelhaxWindow
    {
        /// <summary>
        /// Create an instance of the EndOfLife window
        /// </summary>
        public EndOfLife(ModpackSettings modpackSettings) : base(modpackSettings)
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //translate title
            Title = Translations.GetTranslatedString(nameof(EndOfLife));

            //set translated message
            StringBuilder creditsBuilder = new StringBuilder();
            creditsBuilder.AppendFormat("{0}{1}{1}", Translations.GetTranslatedString("endOfLifeMessagePart1"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}{1}{2}{3}{4}{4}", Translations.GetTranslatedString("endOfLifeMessagePart2a"), Translations.GetTranslatedString("endOfLifeMessagePart2b"),
                Translations.GetTranslatedString("endOfLifeMessagePart2c"), Translations.GetTranslatedString("endOfLifeMessagePart2d"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}{1}{2}{3}{4}{4}", Translations.GetTranslatedString("endOfLifeMessagePart3a"), Translations.GetTranslatedString("endOfLifeMessagePart3b"),
                Translations.GetTranslatedString("endOfLifeMessagePart3c"), Translations.GetTranslatedString("endOfLifeMessagePart3d"), Environment.NewLine);
            creditsBuilder.AppendFormat("{0}", Translations.GetTranslatedString("endOfLifeMessagePart4"));
            creditsBuilder.AppendLine();
            endOfLifeTextbox.Text = creditsBuilder.ToString();

            //set other UI components
            WoTForumAnnouncementsTextBlock.Text = Translations.GetTranslatedString(nameof(WoTForumAnnouncementsTextBlock)) + "  ";
            CloseWindowButton.Content = Translations.GetTranslatedString(nameof(CloseWindowButton));
        }

        private void CloseWindowButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
