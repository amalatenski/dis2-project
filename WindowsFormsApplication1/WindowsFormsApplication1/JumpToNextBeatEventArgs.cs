using System;

namespace Test
{
    class JumpToNextBeatEventArgs : EventArgs
    {
        public int TaktPosition { get; private set; }
        public int PreviousBeatPosition { get; private set; }

        public JumpToNextBeatEventArgs(int taktPosition, int previousBeatPosition)
        {
            this.TaktPosition = taktPosition;
            this.PreviousBeatPosition = previousBeatPosition;
        }
    }
}
