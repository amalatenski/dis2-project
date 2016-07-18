using System;

namespace Test
{
    class BeatEventArgs : EventArgs
    {
        public int TaktPosition { get; private set; }

        public BeatEventArgs(int taktPosition)
        {
            this.TaktPosition = taktPosition;
        }
    }
}
