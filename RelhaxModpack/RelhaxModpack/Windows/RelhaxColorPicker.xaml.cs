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
using System.Globalization;

namespace RelhaxModpack.Windows
{
    /// <summary>
    /// Interaction logic for RelhaxColorPicker.xaml
    /// </summary>
    public partial class RelhaxColorPicker : RelhaxWindow
    {
        private string LastSavedColorTypeString = "SolidColorBrush";
        bool readyToApply = false;

        /// <summary>
        /// Create an instance of the RelhaxColorPicker window
        /// </summary>
        public RelhaxColorPicker()
        {
            InitializeComponent();
        }

        private void RelhaxWindow_Loaded(object sender, RoutedEventArgs e)
        {
            readyToApply = true;
            ApplyColorChange(LastSavedColorTypeString);
        }

        private void AnySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ApplyColorChange(LastSavedColorTypeString);
        }

        private void TextColor_Click(object sender, RoutedEventArgs e)
        {
            ApplyColorChange(LastSavedColorTypeString);
        }

        private void RadialGradientBrush_Checked(object sender, RoutedEventArgs e)
        {
            LastSavedColorTypeString = (sender as RadioButton).Content as string;
            ApplyColorChange(LastSavedColorTypeString);
        }

        private void LinearGradientBrush_Checked(object sender, RoutedEventArgs e)
        {
            LastSavedColorTypeString = (sender as RadioButton).Content as string;
            ApplyColorChange(LastSavedColorTypeString);
        }

        private void SolidColorBrush_Checked(object sender, RoutedEventArgs e)
        {
            LastSavedColorTypeString = (sender as RadioButton).Content as string;
            ApplyColorChange(LastSavedColorTypeString);
        }

        private void ApplyColorChange(string colorType)
        {
            if (!readyToApply)
                return;
            //setup variables
            Color color1 = new Color();
            Color color2 = new Color();
            Color textColor = new Color();
            Point point1 = new Point();
            Point point2 = new Point();
            
            //parse color codes to display
            switch(colorType)
            {
                case "SolidColorBrush":
                    color1 = Color.FromArgb(BitConverter.GetBytes((int)MainColorAlphaSlider.Value)[0], BitConverter.GetBytes((int)MainColorRedSlider.Value)[0],
                        BitConverter.GetBytes((int)MainColorGreenSlider.Value)[0], BitConverter.GetBytes((int)MainColorBlueSlider.Value)[0]);
                    SampleBackgroundColor.Background = new SolidColorBrush(color1);
                    PointsStackPanel.IsEnabled = false;
                    SecondColorStackPanel.IsEnabled = false;
                    break;
                case "LinearGradientBrush":
                    color1 = Color.FromArgb(BitConverter.GetBytes((int)MainColorAlphaSlider.Value)[0], BitConverter.GetBytes((int)MainColorRedSlider.Value)[0],
                        BitConverter.GetBytes((int)MainColorGreenSlider.Value)[0], BitConverter.GetBytes((int)MainColorBlueSlider.Value)[0]);
                    color2 = Color.FromArgb(BitConverter.GetBytes((int)SecondColorAlphaSlider.Value)[0], BitConverter.GetBytes((int)SecondColorRedSlider.Value)[0],
                        BitConverter.GetBytes((int)SecondColorGreenSlider.Value)[0], BitConverter.GetBytes((int)SecondColorBlueSlider.Value)[0]);
                    point1.X = Point1XSlider.Value;
                    point1.Y = Point1YSlider.Value;
                    point2.X = Point2XSlider.Value;
                    point2.Y = Point2YSlider.Value;
                    SampleBackgroundColor.Background = new LinearGradientBrush(color1, color2, point1, point2);
                    PointsStackPanel.IsEnabled = true;
                    SecondColorStackPanel.IsEnabled = true;
                    break;
                case "RadialGradientBrush":
                    color1 = Color.FromArgb(BitConverter.GetBytes((int)MainColorAlphaSlider.Value)[0], BitConverter.GetBytes((int)MainColorRedSlider.Value)[0],
                        BitConverter.GetBytes((int)MainColorGreenSlider.Value)[0], BitConverter.GetBytes((int)MainColorBlueSlider.Value)[0]);
                    color2 = Color.FromArgb(BitConverter.GetBytes((int)SecondColorAlphaSlider.Value)[0], BitConverter.GetBytes((int)SecondColorRedSlider.Value)[0],
                        BitConverter.GetBytes((int)SecondColorGreenSlider.Value)[0], BitConverter.GetBytes((int)SecondColorBlueSlider.Value)[0]);
                    SampleBackgroundColor.Background = new RadialGradientBrush(color1, color2);
                    PointsStackPanel.IsEnabled = false;
                    SecondColorStackPanel.IsEnabled = true;
                    break;
            }

            if((bool)TextColor.IsChecked)
            {
                textColor = Color.FromArgb(BitConverter.GetBytes((int)TextColorAlphaSlider.Value)[0], BitConverter.GetBytes((int)TextColorRedSlider.Value)[0],
                        BitConverter.GetBytes((int)TextColorGreenSlider.Value)[0], BitConverter.GetBytes((int)TextColorBlueSlider.Value)[0]);
                SampleTextColor.Foreground = new SolidColorBrush(textColor);
            }

            //display color text change
            //main color
            MainColorAlpha.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(MainColorAlpha)), ((int)MainColorAlphaSlider.Value).ToString());
            MainColorRed.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(MainColorRed)), ((int)MainColorRedSlider.Value).ToString());
            MainColorBlue.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(MainColorBlue)), ((int)MainColorBlueSlider.Value).ToString());
            MainColorGreen.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(MainColorGreen)), ((int)MainColorGreenSlider.Value).ToString());

            //text colors
            TextColorAlpha.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(TextColorAlpha)), ((int)TextColorAlphaSlider.Value).ToString());
            TextColorRed.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(TextColorRed)), ((int)TextColorRedSlider.Value).ToString());
            TextColorBlue.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(TextColorBlue)), ((int)TextColorBlueSlider.Value).ToString());
            TextColorGreen.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(TextColorGreen)), ((int)TextColorGreenSlider.Value).ToString());

            //second color
            SecondColorAlpha.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(SecondColorAlpha)), ((int)SecondColorAlphaSlider.Value).ToString());
            SecondColorRed.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(SecondColorRed)), ((int)SecondColorRedSlider.Value).ToString());
            SecondColorBlue.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(SecondColorBlue)), ((int)SecondColorBlueSlider.Value).ToString());
            SecondColorGreen.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(SecondColorGreen)), ((int)SecondColorGreenSlider.Value).ToString());

            //points
            Point1X.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(Point1X)), (Point1XSlider.Value).ToString("N2"));
            Point1Y.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(Point1Y)), (Point1YSlider.Value).ToString("N2"));
            Point2X.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(Point2X)), (Point2XSlider.Value).ToString("N2"));
            Point2Y.Text = string.Format("{0}: {1}", Translations.GetTranslatedString(nameof(Point2Y)), (Point2YSlider.Value).ToString("N2"));

            //build sample xml output
            //<ColorSetting ID="LanguagesSelector" type="LinearGradientBrush" color1="#FFF0F0F0" color2="#FFE5E5E5" point1="0,0" point2="0,1" textColor="#FF000000" />
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("<ColorSetting ID=\"Component_ID\" type=\"{0}\" color1=\"{1}\"",colorType,color1.ToString());
            if(colorType.Equals("LinearGradientBrush"))
            {
                builder.AppendFormat(" color2=\"{0}\" point1=\"{1}\" point2=\"{2}\"",color2.ToString(),
                    point1.ToString(CultureInfo.InvariantCulture),point2.ToString(CultureInfo.InvariantCulture));
            }
            else if (colorType.Equals("RadialGradientBrush"))
            {
                builder.AppendFormat(" color2=\"{0}\"", color2.ToString());
            }
            if((bool)TextColor.IsChecked)
            {
                builder.AppendFormat(" textColor=\"{0}\"", textColor.ToString());
            }
            builder.AppendFormat(" />");
            SampleXmlOutputTextbox.Text = builder.ToString();
        }
    }
}
