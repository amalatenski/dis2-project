using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Forms;
using Test;
using System.Windows;
//using Windows.Media.Audio;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using CSCore;
using CSCore.SoundOut;
using System.Threading;
using CSCore.Codecs;
using CSCore.Streams.Effects;
using CSCore.SoundIn;
using CSCore.Streams;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {

        SoundEngine soundEngine;
        int audioLoop;
        public Form1()
        {
            InitializeComponent();
            //InitAudioGraph();

            soundEngine = new SoundEngine();
            audioLoop = soundEngine.newLoop();

                MuGet loop = new MuGetLoop("loop", 25, 25, 250, 150);
            loop.connectSoundEngine(soundEngine);
            this.Controls.Add(loop);

            //this.Controls.Add(new NoteScroller("bla", 25, 200, 300, 100));
            //MuGet muGet = new MuGet2DSlider("bla", 25, 25, 150, 150);
            //this.Controls.Add(muGet);

            MuGet muGet2 = new MuGet2DSlider("pete", 300, 25, 150, 150, 0, 1, 0, 1);
            this.Controls.Add(muGet2);
            muGet2.UpdateStatus += muGet_UpdateStatus;


        }

        //private async Task InitAudioGraph()
        //{   

        //    AudioGraphSettings settings = new AudioGraphSettings(Windows.Media.Render.AudioRenderCategory.Media);

        //    CreateAudioGraphResult result = await AudioGraph.CreateAsync(settings);
        //    if (result.Status != AudioGraphCreationStatus.Success)
        //    {
        //        Console.WriteLine("AudioGraph creation error: " + result.Status.ToString());
        //    }

        //    audioGraph = result.Graph;

        //}

        private void muGet_UpdateStatus(object sender, StatusEventArgs e)
        {
            label1.Text = e.Status.Split(new char[] { ' ' })[1];
            soundEngine.tmp(e.Status);
        }

        private void Form1_Load(object sender, EventArgs e) { }
        

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            soundEngine.stopPlayback();
            soundEngine.stopAllLoops();
            base.OnFormClosed(e);
        }

        bool flag = false;
        private void button1_Click(object sender, EventArgs e)
        {
            if (!flag)
            {
                soundEngine.recordLoop(audioLoop);
                flag = true;
            }
            else
            {
                soundEngine.stopRecordLoop(audioLoop);
                soundEngine.playLoop(audioLoop);
                flag = false;
            }
        }

    }

    class DSPGain : ISampleSource
    {
        ISampleSource _source;
        public DSPGain(ISampleSource source)
        {
            if (source == null)
                throw new ArgumentNullException("source");
            _source = source;
        }
        public int Read(float[] buffer, int offset, int count)
        {
            float gainAmplification = (float)(Math.Pow(10.0, GainDB / 20.0));
            int samples = _source.Read(buffer, offset, count);
            for (int i = offset; i < offset + samples; i++)
            {
                buffer[i] = Math.Max(Math.Min(buffer[i] * gainAmplification, 1), -1);
            }
            return samples;
        }

        public float GainDB { get; set; }

        public bool CanSeek
        {
            //get { return _source.CanSeek; }
            get { return true; }
        }

        public WaveFormat WaveFormat
        {
            get { return _source.WaveFormat; }
        }

        public long Position
        {
            get
            {
                return _source.Position;
            }
            set
            {
                _source.Position = value;
            }
        }

        public long Length
        {
            get { return _source.Length; }
        }

        public void Dispose()
        {
        }
    }
}
