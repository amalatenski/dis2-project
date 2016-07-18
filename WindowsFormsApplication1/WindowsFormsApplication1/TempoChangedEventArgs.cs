using System;

namespace Test
{
    class TempoChangedEventArgs : EventArgs
    {
        public int BeatLength { get; private set; }

        public TempoChangedEventArgs(int beatLength)
        {
            this.BeatLength = beatLength;
        }
    }
}
