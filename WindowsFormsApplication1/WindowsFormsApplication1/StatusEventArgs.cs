using System;

namespace Test
{
    class StatusEventArgs : EventArgs
    {
        public string Status { get; private set; }

        public StatusEventArgs(String status)
        {
            this.Status = status;
        }
    }
}
