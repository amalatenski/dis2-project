using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using CSCore.Streams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    class MuGetTempoWidget : MuGet
    {
        class Ellipse
        {
            public readonly long creationTime;
            public Ellipse(long creationTime) { this.creationTime = creationTime; }
            public int position = 0;
        }

        private Stopwatch ellipsesStopwatch = new Stopwatch();
        private Timer ellipsesTimer = new Timer();
        private WasapiOut soundOutPling;
        private WasapiOut soundOutKlack;
        private readonly int factor;

        public MuGetTempoWidget(String text, Int32 x, Int32 y, Int32 width)
            : base(text, x, y, width, width)
        {
            soundOutPling = new WasapiOut();
            soundOutKlack = new WasapiOut();
            factor = width / 10;
            ellipsesStopwatch.Start();
            ellipsesTimer.Interval = 17;
            ellipsesTimer.Start();
            ellipsesTimer.Tick += ellipsesTimer_Tick;
        }

        private List<Ellipse> ellipses = new List<Ellipse>();
        private int ellipsesMax = 20;
        private int highlightCounter = 0;
        private bool inputting = false;
        private bool error = false;
        private int errorCounter = 0;
        private long previous = -1;
        private float volume = 0.5f;
        private long downTime = 0;
        private bool mouseDown = false;
        private bool ignoreClick = false;

        private static Pen linePen = new Pen(backgroundObjectColor);
        private static Pen linePen2 = new Pen(backgroundObjectLightColor);
        private static Brush labelBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);

        protected override void OnPaint(PaintEventArgs e)
        {
            //draws the line of the slider and the slider
            Graphics g = e.Graphics;

            //calls the OnPaint of the base class. Without this the border would disappear.
            base.OnPaint(e);
            g.FillRectangle(backgroundObjectLightBrush, 0, 0, this.Width - borderPen.Width, this.Height - borderPen.Width);
            g.FillEllipse(Brushes.LightYellow, 0, 0, this.Width - 2* borderPen.Width, this.Height - 2* borderPen.Width);
            g.DrawEllipse(linePen, 0, 0, this.Width - borderPen.Width, this.Height - borderPen.Width);

            //g.DrawEllipse(Pens.DarkGray, new Rectangle(0, 0, Width, Width));
            Brush b =
                error ? Brushes.Red : (
                    highlightCounter < 4 ? stateProgressBrush : (
                        inputting ? stateFinishBrush :
                            stateDefaultBrush
                    )
                );

            g.FillEllipse(b, new Rectangle(MuGet.BeatLength / factor, MuGet.BeatLength / factor, Width - 2 * MuGet.BeatLength / factor, Width - 2 * MuGet.BeatLength / factor));

            for (int i = 0; i < ellipses.Count; i++)
            {
                if ((Width - ellipses[i].position) - ellipses[i].position < 30)
                {
                    ellipses.RemoveAt(i);
                }
                else
                {
                    g.DrawEllipse(ellipses[i].position > MuGet.BeatLength / factor ? linePen2 : linePen, new Rectangle(ellipses[i].position, ellipses[i].position, Width - 2 * ellipses[i].position, Width - 2 * ellipses[i].position));
                }
            }

            g.FillEllipse(backgroundObjectLightBrush, Width / 2 - 20, Width / 2 - 20, 40, 40);
            g.DrawString(((int)(MuGet.Bpm)).ToString(), new Font("Arial", 12), labelBrush, Width / 2 - 12, Width / 2 - 8);

            g.DrawRectangle(currentPen, 0, 0, this.Width - borderPen.Width, this.Height - borderPen.Width);

        }

        private void handleBeat()
        {
            highlightCounter = 0;

            if (TaktPosition == 0)
            {
                soundOutKlack.Stop();
                soundOutPling.Stop();
                soundOutPling.Initialize(new LoopStream(CodecFactory.Instance.GetCodec("metronom-pling.wav")) { EnableLoop = false });
                soundOutPling.Volume = (float)(inputting ? 1.0 : volume);
                soundOutPling.Play();
            }
            else
            {
                soundOutKlack.Stop();
                soundOutKlack.Initialize(new LoopStream(CodecFactory.Instance.GetCodec("metronom-klack.wav")) { EnableLoop = false });
                soundOutKlack.Volume = (float)(inputting ? 1.0 : volume);
                soundOutKlack.Play();
            }
            this.ellipses.Add(new Ellipse(ellipsesStopwatch.ElapsedMilliseconds));
            if (this.ellipses.Count > ellipsesMax) this.ellipses.RemoveAt(0);
        }

        protected override void OnJumpToNextBeat(JumpToNextBeatEventArgs e)
        {
            handleBeat();
            base.OnJumpToNextBeat(e);
        }

        protected override void OnBeat(BeatEventArgs e)
        {
            handleBeat();
            base.OnBeat(e);
        }

        protected void ellipsesTimer_Tick(object sender, EventArgs e)
        {
            highlightCounter++;
            if (error) { errorCounter++; if (errorCounter > 4) { error = false; errorCounter = 0; } }
            long now = ellipsesStopwatch.ElapsedMilliseconds;
            if (inputting && (now - previous) > (2 * BeatLength)) { previous = -1; inputting = false; jumpToNextBeat(); }
            for (int i = 0; i < ellipses.Count; i++) ellipses[i].position = (int)(now - ellipses[i].creationTime) / factor;

            if (mouseDown && (now - downTime) > 1300 && !ignoreClick)
            {
                MuGetSlider slider1 = new MuGetSlider("volumeslider", 10, 10, Width - 20, Width / 2 - 60, 0, 1);
                MuGetSlider slider2 = new MuGetSlider("temposlider", 10, Width / 2 + 50, Width - 20, Width / 2 - 60, 40, 140);
                ignoreClick = true;
                slider1.setPosition(volume);
                slider1.UpdateStatus += ((_sender, _e) => { volume = slider1.value; });
                slider1.ActionEnded += ((_sender, _e) => { ignoreClick = false; slider1.Dispose(); slider2.Dispose(); });
                slider2.setPosition((float)Bpm);
                slider2.UpdateStatus += ((_sender, _e) => { Bpm = slider2.value; });
                slider2.ActionEnded += ((_sender, _e) => { ignoreClick = false; slider1.Dispose(); slider2.Dispose(); });

                this.Controls.Add(slider1);
                this.Controls.Add(slider2);
                slider1.BringToFront();
                slider2.BringToFront();
            }

            this.Refresh();
        }

        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e) { mouseDown = true; downTime = ellipsesStopwatch.ElapsedMilliseconds; }
        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e) { mouseDown = false; }

        protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("MouseClick beginning " + BeatDelta);
            base.OnMouseClick(e);
            if (ignoreClick) return;
            if (BeatDelta < BeatLength / 5)
            {
                inputting = true;
                long next = ellipsesStopwatch.ElapsedMilliseconds;
                //this.ellipses.Add(new Ellipse(next));
                jumpToNextBeat();
                waitForNextBeat();
                if (previous != -1)
                {
                    MuGet.BeatLength = (int)(next - previous);
                }
                previous = next;
                System.Diagnostics.Debug.WriteLine("MouseClick ending");
            }
            else
            {
                error = true;
            }
        }
    }
}