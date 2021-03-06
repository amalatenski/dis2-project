using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Test
{
    class MuGet : Control
    {
        /************** FIELDS ************/

        protected static Timer timer;
        protected static Stopwatch stopwatch;
        private static List<MuGet> instances;

        public SoundEngine SoundEngine { get; private set; }




        /************** EVENTS ***********/

        public delegate void StatusUpdateHandler(object sender, StatusEventArgs e);
        public event StatusUpdateHandler UpdateStatus;

        public delegate void BeatEventHandler(object sender, BeatEventArgs e);
        public event BeatEventHandler Beat;

        public delegate void TickEventHandler(object sender, EventArgs e);
        public event TickEventHandler Tick;

        public delegate void TempoChangedEventHandler(object sender, TempoChangedEventArgs e);
        public static event TempoChangedEventHandler TempoChanged;

        public delegate void WaitForNextBeatEventHandler(object sender, EventArgs e);
        public event WaitForNextBeatEventHandler WaitForNextBeat;

        public delegate void JumpToNextBeatEventHandler(object sender, JumpToNextBeatEventArgs e);
        public static event JumpToNextBeatEventHandler JumpToNextBeat;




        /************* COLORS, PENS, BRUSHES ***********/

        protected Pen currentPen;

        protected static Pen borderPen = System.Drawing.Pens.Black;
        protected static Pen hoverPen = System.Drawing.Pens.White;
        protected static Color hoverColor = Color.DarkGray;

        public static Color backgroundColor = Color.LightYellow;
        public static Color backgroundObjectColor = Color.SteelBlue;
        public static Color backgroundObjectLightColor = Color.FromArgb(150, backgroundObjectColor);

        public static Color inactiveColor = Color.DarkGray;
        public static Color activeColor = Color.BurlyWood;

        public static Color stateDefaultColor = Color.Khaki;
        public static Color stateProgressColor = Color.Yellow;
        public static Color stateFinishColor = Color.LightBlue;

        public static SolidBrush backgroundBrush = new SolidBrush(backgroundColor);
        public static SolidBrush backgroundObjectBrush = new SolidBrush(backgroundObjectColor);
        public static SolidBrush backgroundObjectLightBrush = new SolidBrush(backgroundObjectLightColor);

        public static SolidBrush inactiveBrush = new SolidBrush(inactiveColor);
        public static SolidBrush activeBrush = new SolidBrush(activeColor);

        public static SolidBrush stateDefaultBrush = new SolidBrush(stateDefaultColor);
        public static SolidBrush stateProgressBrush = new SolidBrush(stateProgressColor);
        public static SolidBrush stateFinishBrush = new SolidBrush(stateFinishColor);




        /*************** TEMPO *************/

        // # beats per minute
        private static double bpm;
        public static double Bpm
        {
            get
            {
                return bpm;
            }
            protected set
            {
                bpm = value;
                int newBeatLength = (int)(1000 * (60 / value));
                if (TempoChanged != null)
                {
                    TempoChanged(null, new TempoChangedEventArgs(newBeatLength));
                }
            }
        }

        // # beats in a takt
        public static int TaktLength { get; protected set; }

        // # ticks in a beat
        public static int BeatLength
        {
            get
            {
                return (int)(1000 * (60 / Bpm));   // Unit check:  1000 = ms/s , Bpm = Beats/min , 60 = s/min   =>   (60 / Bpm) = s/Beat   =>   1000 * (Bpm / 60) = ms/Beat
            }
            protected set
            {
                Bpm = (1000.0 / value) * 60;   // Unit check:  1000.0 = ms/s , value = ms/Beat , 60 = s/min   =>   (1000.0 / value) = Beats/s   =>   (1000.0 / value) * 60 = Beats/min
            }
        }

        // # of current beat in takt (starting from 0)
        public static int TaktPosition { get; private set; }

        // # of current tick in beat (starting from 0)
        public static int BeatPosition { get { return (int)(stopwatch.ElapsedMilliseconds - lastPosition); } }


        // amount of current Beat that has already passed (0.0 = nothing, 1.0 = full Takt passed)
        public static double BeatFractionPassed { get { return (BeatPosition / BeatLength); } }

        // amount of current Takt that has already passed (0.0 = nothing, 1.0 = full Takt passed)
        public static double TaktFractionPassed { get { return (TaktPosition + (BeatPosition / BeatLength)) / TaktLength; } }
        
        public static double BeatDelta { get { return Math.Min(BeatPosition, BeatLength - BeatPosition); } }

        // global takt stopwatch
        //public static Stopwatch stopwatchGlobal; // 2ND STOPWATCH


        /*************** ADDITIONS FOR TEMPO SETTING *************/

        public static bool Waiting { get; private set; }
        //public static bool TimerEnabled { get { return timer.Enabled; } }

        protected void jumpToNextBeat()
        {
            int oldBeatPosition = BeatPosition;
            //BeatPosition = 0;
            TaktPosition++;
            if (TaktPosition >= TaktLength)
            {
                TaktPosition = 0;
                //stopwatchGlobal.Restart(); // 2ND STOPWATCH
            }
            Waiting = false;         
            timer.Enabled = true;
            stopwatch.Start();
            foreach (MuGet instance in instances) instance.OnJumpToNextBeat(new JumpToNextBeatEventArgs(TaktPosition, oldBeatPosition));
         }

        protected void waitForNextBeat()
        {
            Waiting = true;
        }




        /************** CONSTRUCTORS ***********/

        static long initPosition;
        static long lastPosition;

        // Static constructor to setup Timer as well as TaktLength and Bpm/BeatLength defaults
        static MuGet() {
            TaktLength = 4;
            bpm = 72; // This sets BeatLength too.
            MuGet.instances = new List<MuGet>();

            //stopwatchGlobal = new System.Diagnostics.Stopwatch();// 2ND STOPWATCH
            //stopwatchGlobal.Start();// 2ND STOPWATCH
            TaktPosition = 0;
            timer = new Timer();
            timer.Interval = 17;
            stopwatch = new Stopwatch();
            timer.Start();
            initPosition = stopwatch.ElapsedMilliseconds;
            stopwatch.Start();
            timer.Tick += new EventHandler((sender, e) =>
            {
                foreach (MuGet instance in instances) instance.OnTick(new BeatEventArgs(TaktPosition));
                if (BeatPosition > BeatLength)
                {
                    if (Waiting)
                    {
                        stopwatch.Stop();
                        timer.Stop();
                        foreach (MuGet instance in instances) instance.OnWaitForNextBeat(new EventArgs());
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Beat " + TaktPosition);
                        lastPosition += BeatLength;
                        TaktPosition++;
                        if (TaktPosition >= TaktLength)
                        {
                            TaktPosition = 0;
                            //stopwatchGlobal.Restart();//2ND STOPWATCH
                        }
                        foreach (MuGet instance in instances) instance.OnBeat(new BeatEventArgs(TaktPosition));
                    }
                }
            });
        }

        // Constructor calls base class constructor of Control
        public MuGet(String text, Int32 x, Int32 y, Int32 width, Int32 height)
            : base(text, x, y, width, height)
        {
            this.DoubleBuffered = true;
            currentPen = borderPen;
            this.BackColor = backgroundColor;
            this.SoundEngine = null;
            MuGet.instances.Add(this);
        }

        // Sound Engine handling
        public void connectSoundEngine(SoundEngine soundEngine)
        {
            this.SoundEngine = soundEngine;
        }
        public void disconnectSoundEngine()
        {
            this.SoundEngine = null;
        }




        /************** DRAWING ***********/

        /*Override this to draw stuff.
         * Graphics g = e.CreateGraphics();
         * Then draw with g. E.g.
         *    System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(10, 10, 100, 100);
         *    graphics.DrawRectangle(System.Drawing.Pens.Red, rectangle);
        */
        protected override void OnPaint(PaintEventArgs e)
        {
            //this.BackColor is used to draw the background. Because of this it is not necessary to draw a rectangle for the background
            //draws a border
            Graphics g = e.Graphics;
            g.DrawRectangle(currentPen, 0, 0, this.Width - borderPen.Width, this.Height - borderPen.Width);
            
            base.OnPaint(e);
        }




        /************** HANDLING OWN EVENTS ***********/

        // For displaying status updates.
        protected virtual void OnUpdateStatus(StatusEventArgs e)
        {
            StatusUpdateHandler handler = UpdateStatus;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Gets raised by the application on each tick.
        protected virtual void OnTick(EventArgs e)
        {
            TickEventHandler handler = Tick;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Gets raised by the application on each beat.
        protected virtual void OnBeat(BeatEventArgs e)
        {
            BeatEventHandler handler = Beat;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Changes in Tempo Configuration
        protected virtual void OnTempoChanged(TempoChangedEventArgs e)
        {
            TempoChangedEventHandler handler = TempoChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Wait for next beat to be triggered by JumpToNextBeat
        protected virtual void OnWaitForNextBeat(EventArgs e)
        {
            WaitForNextBeatEventHandler handler = WaitForNextBeat;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // Trigger next beat
        protected virtual void OnJumpToNextBeat(JumpToNextBeatEventArgs e)
        {
            JumpToNextBeatEventHandler handler = JumpToNextBeat;
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
