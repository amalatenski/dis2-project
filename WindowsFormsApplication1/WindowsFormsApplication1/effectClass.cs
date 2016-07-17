using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class EffectClass
    {
    }

    class EchoEffect : EffectClass
    {
        private const float defaultEchoLeftDelay = 500;
        private const float defaultEchoRightDelay = 500;
        private const bool defaultEchoPanDelay = false;
        private const float defaultEchoWetDryMix = 50;
        private const float defaultEchoFeedback = 50;

        public float leftDelay { get; set; }
        public float rightDelay { get; set; }
        public bool panDelay { get; set; }
        public float wetDryMix { get; set; }
        public float feedback { get; set; }

        public EchoEffect (float feedback = defaultEchoFeedback, float leftDelay = defaultEchoLeftDelay, float rightDelay = defaultEchoRightDelay, float wetDryMix = defaultEchoWetDryMix, bool panDelay = defaultEchoPanDelay)
        {
            this.leftDelay = leftDelay;
            this.rightDelay = rightDelay;
            this.panDelay = panDelay;
            this.wetDryMix = wetDryMix;
            this.feedback = feedback;
        }
    }

    class DistortionEffect : EffectClass
    {
        private const float defaultDistortionEdge = 15;
        private const float defaultDistortionGain = -18;

        public float edge { get; set; }
        public float gain { get; set; }

        public DistortionEffect ( float edge = defaultDistortionEdge, float gain = defaultDistortionGain)
        {
            this.edge = edge;
            this.gain = gain;
        }
    }

    class ChorusEffect : EffectClass
    {
        private const float defaultChorusDelay = 16;
        private const float defaultChorusDepth = 10;
        private const float defaultChorusFeedback = 25;
        private const float defaultChorusFrequency = 1.1f;

        public float delay { get; set; }
        public float depth { get; set; }
        public float feedback { get; set; }
        public float frequency { get; set; }

        public ChorusEffect ( float delay = defaultChorusDelay, float depth = defaultChorusDepth, float feedback = defaultChorusFeedback, float frequency = defaultChorusFrequency)
        {
            this.delay = delay;
            this.depth = depth;
            this.feedback = feedback;
            this.frequency = frequency;
        }
    }
}
