using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RelhaxModpack.UIComponents
{
    public class RelhaxFormTreeNode : TreeNode
    {
        public IPackageUIComponent Component = null;
        public Category @Category = null;
    }
}
