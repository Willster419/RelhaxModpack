using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace RelhaxModpack.UIComponents
{

    public class Theme
    {

        public string ThemeName { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;


        public CustomBrush SelectionListSelectedPanelColor { get; set; } = null;

        public CustomBrush SelectionListNotSelectedPanelColor { get; set; } = null;

        public CustomBrush SelectionListSelectedTextColor { get; set; } = null;

        public CustomBrush SelectionListNotSelectedTextColor { get; set; } = null;

        public ClassColorset RadioButtonColorset { get; set; } = null;

        public ClassColorset CheckboxColorset { get; set; } = null;

        public ClassColorset ButtonColorset { get; set; } = null;

        public ClassColorset TabItemColorset { get; set; } = null;

        public ClassColorset ComboboxColorset { get; set; } = null;

        public ClassColorset PanelColorset { get; set; } = null;

        public ClassColorset TextblockColorset { get; set; } = null;

        public ClassColorset BorderColorset { get; set; } = null;

        public ClassColorset ControlColorset { get; set; } = null;

        public Dictionary<Type, WindowColorset> WindowColorsets { get; set; } = null;
        
        public List<CustomBrush> BoundedClassColorsetBrushes
        {
            get
            {
                List<CustomBrush> list = new List<CustomBrush>();
                list.AddRange(RadioButtonColorset.BoundedBrushes);
                list.AddRange(CheckboxColorset.BoundedBrushes);
                list.AddRange(ButtonColorset.BoundedBrushes);
                list.AddRange(TabItemColorset.BoundedBrushes);
                list.AddRange(ComboboxColorset.BoundedBrushes);
                list.AddRange(PanelColorset.BoundedBrushes);
                list.AddRange(TextblockColorset.BoundedBrushes);
                list.AddRange(BorderColorset.BoundedBrushes);
                list.AddRange(ControlColorset.BoundedBrushes);
                return list;
            }
        }
    }
}
