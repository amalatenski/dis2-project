using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows;

namespace Test
{
    /*
     * Sample MuGet implementation.
     */
    class MuGetLoop : MuGet
    {
        //default colors and stuff
        //Pens are used for not filled objects.
        private static Pen linePen = System.Drawing.Pens.Black;
        private static Pen thickPen = new Pen(Color.FromArgb(255, 0, 0, 0), 3);
        //Brushes are used to fill objects.
        private static Brush backgroundBrush = System.Drawing.Brushes.LightGray;
        private static Brush buttonBrush = System.Drawing.Brushes.Red;
        private static Brush timerBrush = System.Drawing.Brushes.Lime;
        private static Brush timerRecordBrush = System.Drawing.Brushes.Blue;
        private static Brush timerPlayBrush = System.Drawing.Brushes.Green;

        private System.Windows.Forms.Timer time;
        private int taktInMS = 3600;

        private System.Windows.Forms.Timer refresh;
        private int refreshRate = 15;

        System.Diagnostics.Stopwatch stopwatch;

        enum ButtonStates {RecordEmpty, Recording, StoppedRecording, Sound, Mute, Playing, Stopping};
        ButtonStates Mic;
        ButtonStates Volume;
        ButtonStates Play;

        private static int timerInnerCircleSize = 25;
        private static int timerLineThickness = 2;

        Bitmap recordEmptyIcon = new Bitmap("C:/Users/amala_000/Documents/icons/recordEmpty.png");
        Bitmap recordingIcon = new Bitmap("C:/Users/amala_000/Documents/icons/recording.png");
        Bitmap stopRecordingIcon = new Bitmap("C:/Users/amala_000/Documents/icons/recordStop.png");

        Bitmap soundIcon = new Bitmap("C:/Users/amala_000/Documents/icons/sound.png");
        Bitmap muteIcon = new Bitmap("C:/Users/amala_000/Documents/icons/mute.png");

        Bitmap pauseIcon = new Bitmap("C:/Users/amala_000/Documents/icons/pause.png");
        Bitmap playIcon = new Bitmap("C:/Users/amala_000/Documents/icons/play.png");
        
        
        private Rectangle buttonRecord;
        private Rectangle buttonMute;
        private Rectangle buttonPlay;

        private Rectangle buttonRecordIcon;
        private Rectangle buttonMuteIcon;
        private Rectangle buttonPlayIcon;

        private Rectangle timer;
        private Rectangle timerInnerCircle;
        private Rectangle counter;
        private Rectangle counterCurve;

        private bool dragMode = false;

        private int widgetWidth;
        private int widgetHeight;
        private int gap;
        private int buttonX;

        private System.Drawing.Point timerMid = new System.Drawing.Point();
        private int timerRadius;

        //Constructor calls base class constructor of Control
        public MuGetLoop(String text, Int32 x, Int32 y, Int32 width, Int32 height)
            : base(text, x, y, width, height)
        {
            widgetWidth = width;
            widgetHeight = height;

            //thickPen.Alignment = PenAlignment.Center;

            /*
            Microphone mic = Microphone.Default;
            if (mic == null)
            {
                return false; // No microphone is attached to the device
            }
            */
            Mic = ButtonStates.RecordEmpty;
            Volume = ButtonStates.Sound;
            Play = ButtonStates.Playing;

            gap = (int)(0.07 * height);
            int buttonHeight = (int)((height - 4 * gap) / 3);
            int buttonWidth = buttonHeight;

            buttonX = (int)(width - 1.2 * gap - buttonWidth);
            int buttonRecordY = gap;
            int buttonMuteY = 2 * gap + buttonHeight;
            int buttonPlayY = 3 * gap + 2 * buttonHeight;

            int recordIconSizeX = (int)(0.5 * buttonWidth);
            int recordIconSizeY = (int)(0.8 * buttonWidth);
            int muteIconSize = (int)(0.8 * buttonWidth);
            int playIconSize = (int)(0.65 * buttonWidth);

            int buttonMidX = (int)(buttonX + buttonWidth / 2);
            int buttonRecordMidY = (int)(buttonRecordY + buttonHeight / 2);
            int buttonMuteMidY = (int)(buttonMuteY + buttonHeight / 2);
            int buttonPlayMidY = (int)(buttonPlayY + buttonHeight / 2);

            int buttonRecordIconPosX = (int)(buttonMidX - recordIconSizeX / 2);
            int buttonRecordIconPosY = (int)(buttonRecordMidY - recordIconSizeY / 2);

            int buttonMuteIconPosX = (int)(buttonMidX - muteIconSize / 2);
            int buttonMuteIconPosY = (int)(buttonMuteMidY - muteIconSize / 2);

            int buttonPlayIconPosX = (int)(buttonMidX - playIconSize / 2);
            int buttonPlayIconPosY = (int)(buttonPlayMidY - playIconSize / 2);

            //creates the buttons
            buttonRecord = new System.Drawing.Rectangle(buttonX, buttonRecordY, buttonWidth, buttonHeight);
            buttonMute = new System.Drawing.Rectangle(buttonX, buttonMuteY, buttonWidth, buttonHeight);
            buttonPlay = new System.Drawing.Rectangle(buttonX, buttonPlayY, buttonWidth, buttonHeight);

            buttonRecordIcon = new System.Drawing.Rectangle(buttonRecordIconPosX, buttonRecordIconPosY, recordIconSizeX, recordIconSizeY);
            buttonMuteIcon = new System.Drawing.Rectangle(buttonMuteIconPosX, buttonMuteIconPosY, muteIconSize, muteIconSize);
            buttonPlayIcon = new System.Drawing.Rectangle(buttonPlayIconPosX, buttonPlayIconPosY, playIconSize, playIconSize);

            //timer circle properties
            int timerGap = (int)(1.5 * gap);
            timer = new Rectangle((int)(0.1 * width), timerGap, widgetHeight - 2 * timerGap, widgetHeight - 2 * timerGap);
            timerMid = getMidpoint(timer);
            timerRadius = timer.Height / 2;
            timerInnerCircle = getRectangleAroundPoint(timerMid.X, timerMid.Y, timerInnerCircleSize, timerInnerCircleSize);

            //counter
            counter = new Rectangle((int)(0.1 * height), (int)(0.05 * height), (int)(0.3 * widgetWidth), (int)(0.3 * widgetHeight));
            counterCurve = new Rectangle(timer.X - 5, timer.Y - 5, timer.Width, timer.Height);

            //time properties
            time = new System.Windows.Forms.Timer();
            refresh = new System.Windows.Forms.Timer();
            stopwatch = new System.Diagnostics.Stopwatch();
            
            //register EventHandler for timer
            time.Tick += new EventHandler(time_tick);
            time.Interval = taktInMS;

            refresh.Tick += new EventHandler(refresh_tick);
            refresh.Interval = refreshRate;

            //has to be moved
            time.Start();
            refresh.Start();
            stopwatch.Start();
            
            // double buffering against flickering
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            //draws the timer and the buttons
            Graphics g = e.Graphics;
            
            g.FillRectangle(buttonBrush, buttonRecord);
            g.FillRectangle(buttonBrush, buttonMute);
            g.FillRectangle(buttonBrush, buttonPlay);

            g.DrawRectangle(linePen, buttonRecord);
            g.DrawRectangle(linePen, buttonMute);
            g.DrawRectangle(linePen, buttonPlay);

            g.FillRectangle(buttonBrush, counter);
            g.FillEllipse(backgroundBrush, counterCurve);

            g.FillEllipse(timerBrush, timer);
            g.DrawEllipse(linePen, timer);

            g.DrawLine(linePen, timerMid, getPointOnCircle(0.0));
            g.DrawLine(thickPen, timerMid, getPointOnCircle(getTaktRatio()));

            g.FillEllipse(buttonBrush, timerInnerCircle);

            if (Mic == ButtonStates.RecordEmpty)
            {
                g.DrawImage(recordEmptyIcon, buttonRecordIcon);
            }
            else if (Mic == ButtonStates.Recording)
            {
                g.DrawImage(recordingIcon, buttonRecordIcon);
            }
            else if (Mic == ButtonStates.StoppedRecording)
            {
                g.DrawImage(stopRecordingIcon, buttonRecordIcon);
            }

            if (Volume == ButtonStates.Sound)
            {
                g.DrawImage(soundIcon, buttonMuteIcon);
            }
            else if (Volume == ButtonStates.Mute)
            {
                g.DrawImage(muteIcon, buttonMuteIcon);
            }

            if (Play == ButtonStates.Playing)
            {
                g.DrawImage(pauseIcon, buttonPlayIcon);
            }
            else if (Play == ButtonStates.Stopping)
            {
                g.DrawImage(playIcon, buttonPlayIcon);
            }
            
            //calls the OnPaint of the base class. Without this the border would disappear.
            base.OnPaint(e);
        }


        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);
            /*if (slider.Contains(new System.Drawing.Point(e.X, e.Y)))
            {
                dragMode = true;
            }*/
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
            dragMode = false;

            //Console.WriteLine(getTaktRatio());
        }

        protected override void OnMouseClick(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (buttonRecord.Contains(new System.Drawing.Point(e.X, e.Y)))
            {
                changeButtonState(buttonRecord);
                this.Refresh();
            }
            else if (buttonMute.Contains(new System.Drawing.Point(e.X, e.Y)))
            {
                changeButtonState(buttonMute);
                this.Refresh();
            }
            else if (buttonPlay.Contains(new System.Drawing.Point(e.X, e.Y)))
            {
                changeButtonState(buttonPlay);
                this.Refresh();
            }
            //System.Console.Write();
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            /*if (dragMode)
            {
                slider.X = e.X - sliderWidth / 2;

                if (slider.X < 10) { slider.X = 10; }
                if (slider.X > this.Width - sliderLineOffset - sliderWidth) { slider.X = this.Width - sliderLineOffset - sliderWidth; }
            }
            this.Refresh();*/
        }

        private System.Drawing.Point getMidpoint(Rectangle rect)
        {
            System.Drawing.Point mid = new System.Drawing.Point();
            mid.X = (int)(rect.X + rect.Width / 2);
            mid.Y = (int)(rect.Y + rect.Height / 2);
            return mid;
            
        }

        private Rectangle getRectangleAroundPoint(int x, int y, int sizeX, int sizeY)
        {
            int rectX = (int)(x - sizeX / 2);
            int rectY = (int)(y - sizeY / 2);

            Rectangle rect = new Rectangle(rectX, rectY, sizeX, sizeY);
            return rect;
        }

        private System.Drawing.Point getPointOnCircle(double ratio)
        {
            double degrees = ratio * 360;
            double angle = Math.PI * degrees / 180.0;
            double sinAngle = Math.Sin(angle);
            double cosAngle = Math.Cos(angle);

            System.Drawing.Point ratioPoint = new System.Drawing.Point();

            ratioPoint.X = (int)(timerMid.X + timerRadius * sinAngle);
            ratioPoint.Y = (int)(timerMid.Y - timerRadius * cosAngle);
            
            return ratioPoint;

        }

        private void time_tick(object source, EventArgs e)
        {
            stopwatch.Restart();
            if (true)
            {
                //this.Refresh();

            }
            else
            {
                //time.Stop();
            }
        }

        private void refresh_tick(object source, EventArgs e)
        {
            this.Refresh();
        }

        //returns the elapsed time value since the last takt (between 0 and 1)
        private double getTaktRatio()
        {
            double elapsed = stopwatch.ElapsedMilliseconds;
            double taktRatio = elapsed / taktInMS;
            return taktRatio;
        }

        private void changeButtonState(Rectangle rect)
        {
            if (buttonRecord == rect)
            {
                if (Mic == ButtonStates.RecordEmpty)
                {
                    Mic = ButtonStates.Recording;
                }
                else if (Mic == ButtonStates.Recording)
                {
                    Mic = ButtonStates.StoppedRecording;
                }
                else if (Mic == ButtonStates.StoppedRecording)
                {
                    Mic = ButtonStates.Recording;
                }
            }
            else if (buttonMute == rect)
            {
                if (Volume == ButtonStates.Sound)
                {
                    Volume = ButtonStates.Mute;
                }
                else if (Volume == ButtonStates.Mute)
                {
                    Volume = ButtonStates.Sound;
                }
            }
            else if (buttonPlay == rect)
            {
                if (Play == ButtonStates.Playing)
                {
                    Play = ButtonStates.Stopping;
                }
                else if (Play == ButtonStates.Stopping)
                {
                    Play = ButtonStates.Playing;
                }
            }
        }

    }
}
