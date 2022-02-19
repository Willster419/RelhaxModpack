using RelhaxModpack.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack.UI
{
    public interface IOnCheckedComponent
    {
        /// <summary>
        /// The package that the UI component belongs to
        /// </summary>
        SelectablePackage Package { get; set; }

        /// <summary>
        /// Method signature for when the checked property changes
        /// </summary>
        /// <param name="Checked">The value of the checked property</param>
        void OnCheckedChanged(bool Checked);
    }
}
