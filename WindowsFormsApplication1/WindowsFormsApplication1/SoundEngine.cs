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
        private const float defaultEchoLeftDelay = 500;
        private const float defaultEchoRightDelay = 500;
        private const bool defaultEchoPanDelay = false;
        private const float defaultEchoWetDryMix = 50;
        private const float defaultEchoFeedback = 50;


        private WasapiCapture capture;
        private SoundInSource captureSource;
        private ISoundOut soundOut;
        private List<DmoAggregator> effects;
        private List<Loop> loops;
        DmoEchoEffect echoEffect;
        DmoAggregator effect;
        DmoDistortionEffect distortionEffect;
        
        public SoundEngine()
        {
            loops = new List<Loop>();
            effects = new List<DmoAggregator>();

            capture = new WasapiCapture();
            capture.Initialize();
            captureSource = new SoundInSource(capture) { FillWithZeros = true };
            Console.WriteLine(effects.FindIndex(x => x.GetType() == typeof(DmoEchoEffect)));

            //IWaveSource waveSource = GetSoundSource(@"E:\Musik\Music\Unknown Artist\Unknown Album\Here's to the people.wav");

            echoEffect = new DmoEchoEffect(captureSource);
            effects.Add(echoEffect);
            echoEffect.LeftDelay = 500; //500 ms
            echoEffect.RightDelay = 250; //250 ms
            //echoEffect.IsEnabled = true;
            distortionEffect = new DmoDistortionEffect(captureSource);
            effects.Add(distortionEffect);
            
            effect = new DmoChorusEffect(captureSource);
            //effects.Add(effect);
            soundOut = GetSoundOut();
            //loop = new LoopStream(buffer);
            //loop.EnableLoop = true;
            soundOut.Initialize(compileEffectChain());
            startPlayback();
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
            List<DmoAggregator> newEffects = new List<DmoAggregator>();


            foreach(DmoAggregator effect in effects)
            {
                if(effect.GetType() == typeof(DmoEchoEffect))
                {
                    DmoEchoEffect echo = effect as DmoEchoEffect;
                    DmoEchoEffect newEcho = new DmoEchoEffect(currentInput);
                    newEcho.LeftDelay = echo.LeftDelay;
                    newEcho.RightDelay = echo.RightDelay;
                    newEcho.WetDryMix = echo.WetDryMix;
                    newEcho.PanDelay = echo.PanDelay;
                    newEcho.Feedback = echo.Feedback;
                    newEffects.Add(newEcho);
                    currentInput = newEcho;
                } else if(effect.GetType() == typeof(DmoDistortionEffect))
                {
                    DmoDistortionEffect distortion = effect as DmoDistortionEffect;
                    DmoDistortionEffect newDistortion = new DmoDistortionEffect(currentInput);
                    newDistortion.Edge = distortion.Edge;
                    newDistortion.Gain = distortion.Gain;
                    newEffects.Add(newDistortion);
                    currentInput = newDistortion;
                } else if(effect.GetType() == typeof(DmoChorusEffect))
                {
                    DmoChorusEffect chorus = effect as DmoChorusEffect;
                    DmoChorusEffect newChorus = new DmoChorusEffect(currentInput);
                    newChorus.Delay = chorus.Delay;
                    newChorus.Depth = chorus.Depth;
                    newChorus.Feedback = chorus.Feedback;
                    newChorus.Frequency = chorus.Frequency;
                    newEffects.Add(newChorus);
                    currentInput = newChorus;
                }
            }

            effects = newEffects;
            return currentInput;
        }

        public void newEchoEffect(float feedback = defaultEchoFeedback, float leftDelay = defaultEchoLeftDelay, float rightDelay = defaultEchoRightDelay, float wetDryMix = defaultEchoWetDryMix, bool panDelay = defaultEchoPanDelay)
        {
            DmoEchoEffect echo = new DmoEchoEffect(captureSource);
            echo.Feedback = feedback;
            echo.LeftDelay = leftDelay;
            echo.RightDelay = rightDelay;
            echo.WetDryMix = wetDryMix;
            echo.PanDelay = panDelay;

            int index = effects.FindIndex(x => x.GetType() == typeof(DmoEchoEffect));
            if (index != -1)
            {
                effects[index] = new DmoEchoEffect(captureSource);
            }
            else
            {
                effects.Add(echo);
            }
            newSoundOut(compileEffectChain());
        }

        public void tmp(string value)
        {
            DmoEchoEffect echo = effects.Find(x => x.GetType() == typeof(DmoEchoEffect)) as DmoEchoEffect;
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


        //old but gold
        public void PlayASound()
        {
            //Contains the sound to play
            using (IWaveSource soundSource = GetSoundSource("test.wav"))
            {
                //SoundOut implementation which plays the sound
                using (ISoundOut soundOut = GetSoundOut())
                {
                    //Tell the SoundOut which sound it has to play
                    soundOut.Initialize(soundSource);

                    //Play the sound
                    soundOut.Play();

                    Thread.Sleep(2000);

                    //Stop the playback
                    soundOut.Stop();
                }
            }
        }
    }
}
