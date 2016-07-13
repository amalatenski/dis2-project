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
using Un4seen.Bass;
using Un4seen.Bass.AddOn.Fx;

namespace Test
{
    class SoundEngine
    {
        private WasapiCapture capture;
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
            SoundInSource captureSource = new SoundInSource(capture) { FillWithZeros = true };

            
            //IWaveSource waveSource = GetSoundSource(@"E:\Musik\Music\Unknown Artist\Unknown Album\Here's to the people.wav");
            
            echoEffect = new DmoEchoEffect(captureSource);
            
            echoEffect.LeftDelay = 500; //500 ms
            echoEffect.RightDelay = 250; //250 ms
            //echoEffect.IsEnabled = true;
            distortionEffect = new DmoDistortionEffect(echoEffect);
            effect = new DmoChorusEffect(distortionEffect);
            soundOut = GetSoundOut();
            //loop = new LoopStream(buffer);
            //loop.EnableLoop = true;
            soundOut.Initialize(echoEffect);
        }

        public void tmp(string value)
        {
            echoEffect.LeftDelay = float.Parse(value.Split(new char[] { ' ' })[0]) * 1000 + 1;
            echoEffect.RightDelay = float.Parse(value.Split(new char[] { ' ' })[1]) * 500 + 1;
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
