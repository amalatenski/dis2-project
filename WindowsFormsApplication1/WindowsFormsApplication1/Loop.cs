using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSCore;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.Effects;
using CSCore.XAudio2;
using CSCore.DMO;
using CSCore.Codecs.WAV;
using CSCore.Codecs;

namespace Test
{
    class Loop
    {
        static int count = 0;
        public int id { get; private set; }
        private string filename;

        private WasapiCapture capture;
        private ISoundOut soundOut;
        private LoopStream loop;
        private WaveWriter writer;

        public Loop()
        {
            id = count;
            count++;
            filename = String.Format("{0}.wav", id);

            soundOut = new WasapiOut();

            init();
        }

        private void init()
        {

            capture = new WasapiCapture();
            capture.Initialize();
            SoundInSource captureSource = new SoundInSource(capture);

            System.IO.File.Delete(filename);
            writer = new WaveWriter(filename, captureSource.WaveFormat);
            captureSource.DataAvailable += (s, e) =>
            {
                writer.Write(e.Data, e.Offset, e.ByteCount);
            };
        }

        public bool isRecording()
        {
            if (capture.RecordingState == RecordingState.Recording) return true;
            return false;
        }

        public void record()
        {
            if(loop == null) capture.Start();
            else
            {
                if(soundOut.PlaybackState == PlaybackState.Playing)
                {
                    soundOut.Stop();
                    loop.Dispose();
                }
                init();
                capture.Start();
            }
        }

        public void setVolume(float volume)
        {
            if (volume > 1) volume = 1;
            if (volume < 0) volume = 0;
            soundOut.Volume = volume;
        }

        public void stopRecord()
        {
            capture.Stop();
            capture.Dispose();

            ((IDisposable)writer).Dispose();

            IWaveSource file = CodecFactory.Instance.GetCodec(filename);
            loop = new LoopStream(file);
            loop.EnableLoop = true;
            soundOut.Initialize(loop);
        }

        public void play()
        {
            soundOut.Play();
        }

        public void pause()
        {
            soundOut.Pause();
        }

        public void stop()
        {
            soundOut.Stop();
            if(loop != null) loop.Dispose();
            soundOut.Dispose();
        }
    }
}
