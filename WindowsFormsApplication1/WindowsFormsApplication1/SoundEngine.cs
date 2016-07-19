using CSCore;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.Effects;
using CSCore.XAudio2;
using CSCore.DMO;
using CSCore.Codecs.WAV;
using CSCore.Codecs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Test
{
    class SoundEngine
    { 
        private WasapiCapture capture;
        private SoundInSource captureSource;
        private ISoundOut soundOut;
        public List<EffectClass> effects { get; private set; }
        private List<DmoAggregator> effectChain;

        public float volume
        {
            get
            {
                return soundOut.Volume;
            }
            set
            {
                if (volume > 1) volume = 1;
                if (volume < 0) volume = 0;
                soundOut.Volume = value;
            }
        }
        
        private List<Loop> loops;
        
        public SoundEngine()
        {
            loops = new List<Loop>();
            effects = new List<EffectClass>();

            capture = new WasapiCapture();
            capture.Initialize();
            captureSource = new SoundInSource(capture) { FillWithZeros = true };
            soundOut = GetSoundOut();

            //IWaveSource waveSource = GetSoundSource(@"E:\Musik\Music\Unknown Artist\Unknown Album\Here's to the people.wav");

            //effects.Add(new DistortionEffect());
            //effects.Add(new ChorusEffect());
            //EchoEffect echo = new EchoEffect();
            //effects.Add(echo);
            //echo.leftDelay = 500;
            //echo.rightDelay = 250;

            //effects.Add(new DistortionEffect());
            //effects.Add(new ChorusEffect());

            soundOut.Initialize(compileEffectChain());
            startPlayback();
        }
        
        public void refreshSound()
        {
            newSoundOut(compileEffectChain());
        }

        private void newSoundOut(IWaveSource source)
        {
            if (soundOut.PlaybackState == PlaybackState.Playing)
            {
                soundOut.Stop();
                soundOut.Initialize(source);
                soundOut.Play();
            }
            else
            {
                soundOut.Stop();
                soundOut.Initialize(source);
            }
        }

        private IWaveSource compileEffectChain()
        {
            IWaveSource currentInput = captureSource;
            effectChain = new List<DmoAggregator>();

            foreach(EffectClass effect in effects)
            {
                if(effect.GetType() == typeof(EchoEffect))
                {
                    EchoEffect echo = effect as EchoEffect;
                    DmoEchoEffect newEcho = new DmoEchoEffect(currentInput);
                    newEcho.LeftDelay = echo.leftDelay;
                    newEcho.RightDelay = echo.rightDelay;
                    newEcho.WetDryMix = echo.wetDryMix;
                    newEcho.PanDelay = echo.panDelay;
                    newEcho.Feedback = echo.feedback;
                    effectChain.Add(newEcho);
                    currentInput = newEcho;
                } else if(effect.GetType() == typeof(DistortionEffect))
                {
                    DistortionEffect distortion = effect as DistortionEffect;
                    DmoDistortionEffect newDistortion = new DmoDistortionEffect(currentInput);
                    newDistortion.Edge = distortion.edge;
                    newDistortion.Gain = distortion.gain;
                    effectChain.Add(newDistortion);
                    currentInput = newDistortion;
                } else if(effect.GetType() == typeof(ChorusEffect))
                {
                    ChorusEffect chorus = effect as ChorusEffect;
                    DmoChorusEffect newChorus = new DmoChorusEffect(currentInput);
                    newChorus.Delay = chorus.delay;
                    newChorus.Depth = chorus.depth;
                    newChorus.Feedback = chorus.feedback;
                    newChorus.Frequency = chorus.frequency;
                    effectChain.Add(newChorus);
                    currentInput = newChorus;
                }
            }
            
            return currentInput;
        }

        public void newEffect (EffectClass effect)
        {
            effects.Add(effect);
            refreshSound();
        }

        public void removeEffect (EffectClass effect)
        {
            effects.Remove(effect);
            refreshSound();
        }

        public void moveEffectTo (EffectClass effect, int newIndex)
        {
            effects.Remove(effect);
            effects.Insert(newIndex, effect);
            refreshSound();
        }
        
        public void tmp(string value)
        {
            DmoEchoEffect echo = effectChain[effects.FindIndex(x => x.GetType() == typeof(EchoEffect))] as DmoEchoEffect;
            if (echo != null)
            {
                echo.LeftDelay = float.Parse(value.Split(new char[] { ' ' })[0]) * 1000 + 1;
                echo.RightDelay = float.Parse(value.Split(new char[] { ' ' })[1]) * 500 + 1;
            }
        }

        #region Loop stuff
        public int newLoop()
        {
            Loop loop = new Loop();
            loops.Add(loop);
            return loop.id;
        }

        public void setVolumeLoop(int id, float volume)
        {
            loops.Find(x => x.id == id).setVolume(volume);
        }

        public void recordLoop(int id)
        {
            loops.Find(x => x.id == id).record();
        }

        public void stopRecordLoop(int id)
        {
            loops.Find(x => x.id == id).stopRecord();
        }

        public void playLoop(int id)
        {
            loops.Find(x => x.id == id).play();
        }

        public void pauseLoop(int id)
        {
            loops.Find(x => x.id == id).pause();
        }

        public void stopLoop(int id)
        {
            loops.Find(x => x.id == id).stop();
        }

        public bool isLoopRecording(int id)
        {
            return loops.Find(x => x.id == id).isRecording();
        }

        public void stopAllLoops()
        {
            foreach (Loop i in loops)
            {
                if (i.isRecording()) i.stopRecord();
                else i.stop();
            }

        }
        #endregion

        public PlaybackState getPlaybackState()
        {
            return soundOut.PlaybackState;
        }

        public void stopPlayback()
        {
            capture.Stop();
            soundOut.Stop();
        }

        public void startPlayback()
        {
            capture.Start();
            soundOut.Play();  
        }

        private ISoundOut GetSoundOut()
        {
            if (WasapiOut.IsSupportedOnCurrentPlatform)
                return new WasapiOut();
            else
                return new DirectSoundOut();
        }

        private IWaveSource GetSoundSource(string filename)
        {
            //return any source ... in this example, we'll just play a mp3 file
            //return CodecFactory.Instance.GetCodec(@"E:\Musik\Music\Unknown Artist\Unknown Album\Here's to the people.wav");
            //return CodecFactory.Instance.GetCodec(@"C:\Users\Niklas\Dropbox\DIS2 projekt\WindowsFormsApplication1\WindowsFormsApplication1\bin\Debug\test.wav");
            return CodecFactory.Instance.GetCodec(filename);
        }
        
    }
}
