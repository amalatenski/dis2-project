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
            

            MuGet loop1 = new MuGetLoop("loop", 886, 25, 300, 180);
            this.Controls.Add(loop1);

            MuGet loop2 = new MuGetLoop("loop2", 886, 240, 300, 180);
            this.Controls.Add(loop2);

            MuGetEffects effects = new MuGetEffects("effects", 180, 25, 260, 180);
            this.Controls.Add(effects);
            
            this.Controls.Add(new NoteScroller("bla", 50, 500, 1016, 200));

            MuGet muGet3 = new MuGetTempoWidget("tempo", 1116, 500, 200);
            this.Controls.Add(muGet3);

                                  


            soundEngine = new SoundEngine();
            loop1.connectSoundEngine(soundEngine);
            loop2.connectSoundEngine(soundEngine);
            effects.connectSoundEngine(soundEngine);
            audioLoop = soundEngine.newLoop();
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

        private void Form1_Load(object sender, EventArgs e)
        {
        }

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
    
}
