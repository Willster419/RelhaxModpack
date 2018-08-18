using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace RelhaxModpack.Utils
{
    public class Translations
    {


        public void ApplyTranslationsOnWindowLoad(Window window)
        {
            Panel startingWindowPanel = (Panel)window.Content;
            ApplyPanelTranslations(startingWindowPanel);
        }

        private void ApplyPanelTranslations(Panel p)
        {
            foreach (FrameworkElement control in p.Children)
            {
                //check if it's a panel (we need to go deeper)
                if (control is Panel subP)
                    ApplyPanelTranslations(subP);
                //check if it's a tab control (itterate through the tabs)
                else if (control is TabControl tc)
                    ApplyTabControlTranslations(tc);
                //check if it's a GroupBox (check header and then content)
                else if (control is GroupBox gb)
                    ApplyGroupBoxTranslations(gb);
                //check if it's a ContentControl (dynamically apply based on sub class result)
                else if (control is ContentControl cc)
                    ApplyContentTranslations(cc);
                else if (control is Decorator dec)
                    ApplyDecoratorTranslations(dec);
            }
        }

        private void ApplyContentTranslations(ContentControl cc)
        {
            if (cc.Content is string)
            {
                cc.Content = "TODO";
            }
            else if (cc.Content is TextBox tb)
                tb.Text = "TODO";
            else if (cc.Content is TextBlock tb2)
                tb2.Text = "TODO";
            else if (cc.Content is Hyperlink hl)
            {

            }
            else if (cc.Content is Panel p)
                ApplyPanelTranslations(p);
            else if (cc.Content is TabControl tc)
                ApplyTabControlTranslations(tc);
            else if (cc.Content is ContentControl cc2)
                ApplyContentTranslations(cc2);
            else if (cc.Content is Decorator dec)
                ApplyDecoratorTranslations(dec);
        }

        private void ApplyTabControlTranslations(TabControl tc)
        {
            foreach (TabItem ti in tc.Items)
            {
                if (ti.Content is Panel p)
                    ApplyPanelTranslations(p);
                else if (ti.Content is ContentControl cc)
                    ApplyContentTranslations(cc);
                else if (ti.Content is Decorator dec)
                    ApplyDecoratorTranslations(dec);
            }
        }

        private void ApplyGroupBoxTranslations(GroupBox gb)
        {
            if (gb.Header is string)
                gb.Header = "TODO";
            else if (gb.Header is TextBlock htb)
                htb.Text = "TODO";
            else if (gb.Header is TextBox htb2)
                htb2.Text = "TODO";
            else if (gb.Header is ContentControl hcc)
                ApplyContentTranslations(hcc);
            if (gb.Content is TextBlock tb)
                tb.Text = "TODO";
            else if (gb.Content is TextBox tb2)
                tb2.Text = "TODO";
            else if (gb.Content is Decorator dec)
                ApplyDecoratorTranslations(dec);
            else if (gb.Content is ContentControl cc)
                ApplyContentTranslations(cc);
            else if (gb.Content is Panel p)
                ApplyPanelTranslations(p);
        }

        private void ApplyDecoratorTranslations(Decorator dec)
        {
            if (dec.Child is TextBlock tb)
                tb.Text = "TODO";
            else if (dec.Child is TextBox tb2)
                tb2.Text = "TODO";
            else if (dec.Child is Decorator dec2)
                ApplyDecoratorTranslations(dec2);
            else if (dec.Child is Panel p)
                ApplyPanelTranslations(p);
            else if (dec.Child is TabControl tc)
                ApplyTabControlTranslations(tc);
            else if (dec.Child is GroupBox gb)
                ApplyGroupBoxTranslations(gb);
            else if (dec.Child is ContentControl cc)
                ApplyContentTranslations(cc);
        }
    }
}
