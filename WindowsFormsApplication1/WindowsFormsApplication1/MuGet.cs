using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;

namespace Test
{
    //public delegate void HandEventHandler (HandEventArgs e);
    class MuGet : Control
    {
        public delegate void StatusUpdateHandler(object sender, StatusEventArgs e);
        public event StatusUpdateHandler OnUpdateStatus;
        //default colors and stuff
        private static Pen borderPen = System.Drawing.Pens.Black;
        private static Color backgroundColor = Color.LightGray;
        private static Color hoverColor = Color.DarkGray;
        private static Pen hoverPen = System.Drawing.Pens.White;

        private Pen currentPen;

        //# beats per minute.. standard 120
        public static double Bpm { get; set; }
        //# beats in a takt.. standard 4
        public static int TaktLength { get; set; }
        //# ms in a beat.. standard 500 (because 120 bpm is standard)
        public static int BeatLength {
          get {
            return (int)(1000 * (60 / Bpm));   // Unit check:  1000 = ms/s , Bpm = Beats/min , 60 = s/min   =>   (60 / Bpm) = s/Beat   =>   1000 * (Bpm / 60) = ms/Beat
          }
          set {
            Bpm = (1000.0 / value) * 60;   // Unit check:  1000.0 = ms/s , value = ms/Beat , 60 = s/min   =>   (1000.0 / value) = Beats/s   =>   (1000.0 / value) * 60 = Beats/min
          }
        }

        //Static constructor to set Bpm and TaktLength (and thus also BeatLength) defaults
        static MuGet() {
          Bpm = 120;
          TaktLength = 4;
        }

        //Constructor calls base class constructor of Control
        public MuGet(String text, Int32 x, Int32 y, Int32 width, Int32 height)
            : base(text, x, y, width, height)
        {
            currentPen = borderPen;
            this.BackColor = backgroundColor;
        }

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


        //Events
        //Gets raised by the application on each beat. Measure the time difference between occurance to get the current speed.
        public virtual void OnBeat()
        {

        }

        //Gets raised on the beginning on each bar/takt.
        public virtual void OnBar()
        {

        }

        //Override to handle event. They say mouse, but it should work with touch.

        //Dont know if this gets called...
        protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e)
        {

        }
        
        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
 	         
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {

        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {

        }

        //throws event
        protected void UpdateStatus(string status)
        {
            // check if subscribers exist
            if (OnUpdateStatus == null) return;

            //sends the content of the string to all subscribers. put all relevant information in there.
            StatusEventArgs args = new StatusEventArgs(status);
            OnUpdateStatus(this, args);
        }
    }
}