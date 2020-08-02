using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelhaxModpack
{   [Serializable]
    public class BadMemeException : Exception
    {
        public BadMemeException(string message) : base(message)
        {

        }
    }
}
