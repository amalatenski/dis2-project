using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class StatusEventArgs : EventArgs
    {
        public string status { get; private set; }

        public StatusEventArgs(String status)
        {
            this.status = status;
        }
    }
}
