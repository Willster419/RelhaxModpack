using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using RelhaxModpack;

namespace RelhaxModpack.UIComponents
{

    public delegate void RelhaxPreviewClickDelegate(object sender, RelhaxPreviewIndexEventArgs e);

    public class RelhaxPreviewIndexEventArgs : System.Windows.Navigation.RequestNavigateEventArgs
    {
        public Media Media { get; set; }
    }
    /// <summary>
    /// A RelhaxPreviewIndex item is a glorified textblock with a hyperlink with a refrence to a media element
    /// </summary>
    public class RelhaxPreviewIndex : TextBlock
    {

        public Media Media { get; set; }

        public event RelhaxPreviewClickDelegate OnPreviewLinkClick;

        public string Text
        {
            get
            {
                //https://stackoverflow.com/questions/19645110/how-to-get-hyperlink-text-from-c-sharp-in-wpf
                if (hyperlink == null)
                    return string.Empty;
                Run r = (Run)hyperlink.Inlines.FirstInline;
                return r.Text;
            }
            set
            {
                //if the hyperlink element is null, then make it
                if (hyperlink == null)
                {
                    hyperlink = new Hyperlink(new Run(value));
                    hyperlink.RequestNavigate += OnInnerHyperlinkClick;
                }
                //else then clear the inlines and add the new one
                else
                {
                    hyperlink.Inlines.Clear();
                    hyperlink.Inlines.Add(value);
                }
                Inlines.Clear();
                Inlines.Add(hyperlink);
            }
        }

        

        private void OnInnerHyperlinkClick(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (OnPreviewLinkClick == null)
                throw new BadMemeException("you forgot to subscribe to me dumbass");
            else
                OnPreviewLinkClick(this, new RelhaxPreviewIndexEventArgs() { Media = Media });
        }

        

        //the hyperlink object to handle the 
        private Hyperlink hyperlink;
    }
}
