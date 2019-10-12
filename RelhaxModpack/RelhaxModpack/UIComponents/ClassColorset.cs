using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RelhaxModpack.UIComponents
{
    public class ClassColorset
    {
        public Type ClassType { get; set; } = null;

        public CustomBrush BackgroundBrush { get; set; } = null;

        public CustomBrush ForegroundBrush { get; set; } = null;

        public CustomBrush HighlightBrush { get; set; } = null;

        public CustomBrush SelectedBrush { get; set; } = null;

        public List<CustomBrush> BoundedBrushes
        {
            get
            {
                List <CustomBrush> list = new List<CustomBrush>();
                if (BackgroundBrush != null && BackgroundBrush.IsValid && BackgroundBrush.IsBound)
                    list.Add(BackgroundBrush);
                if (ForegroundBrush != null && ForegroundBrush.IsValid && ForegroundBrush.IsBound)
                    list.Add(ForegroundBrush);
                if (HighlightBrush != null && HighlightBrush.IsValid && HighlightBrush.IsBound)
                    list.Add(HighlightBrush);
                if (SelectedBrush != null && SelectedBrush.IsValid && SelectedBrush.IsBound)
                    list.Add(SelectedBrush);
                return list;
            }
        }
    }
}
