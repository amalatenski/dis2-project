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
        private static int loopId = 0;

        //default colors and stuff
        //Pens are used for not filled objects.
        private static Pen linePen = System.Drawing.Pens.Black;
        private static Pen thickPen = new Pen(Color.FromArgb(255, 0, 0, 0), 3);
        //Brushes are used to fill objects.
        private static Brush buttonBrush = activeBrush;
        private static Brush timerBrush = stateDefaultBrush;
        private static Brush timerRecordBrush = stateProgressBrush;
        private static Brush timerPlayBrush = stateFinishBrush;

        private System.Windows.Forms.Timer time;
        private int taktInMS = 3000;

        private System.Windows.Forms.Timer playStopTime;
        private int loopInMS;
        private int nrOfTakts;

        private System.Windows.Forms.Timer refresh;
        private int refreshRate = 15;

        private string labelTakts;
        System.Drawing.Font labelFont;
        System.Drawing.SolidBrush labelBrush;
        System.Drawing.StringFormat labelFormat;
        int labelX;
        int labelY;

        //replaces the global takt stopwatch
        System.Diagnostics.Stopwatch stopwatch;

        //local takt stopwatch
        System.Diagnostics.Stopwatch playStopWatch;

        System.Diagnostics.Stopwatch buttonHighlight;
        private int highlightTime = 150;

        enum ButtonStates { RecordEmpty, WaitingToRecord, Recording, WaitingToEndRecord, StoppedRecording, Sound, Mute, Playing, Stopping };
        ButtonStates Mic;
        ButtonStates Volume;
        ButtonStates Play;

        private bool waitingToRecord = false;
        private bool waitingToEndRecord = false;
        private bool recordInside = false;

        private bool buttonRecordPushed = false;
        private bool buttonMutePushed = false;
        private bool buttonPlayPushed = false;

        private static int timerInnerCircleSize = 25;

        Bitmap recordEmptyIcon = Properties.Resources.recordEmpty;
        Bitmap recordingIcon = Properties.Resources.recording;
        Bitmap stopRecordingIcon = Properties.Resources.recordStop;

        Bitmap soundIcon = Properties.Resources.sound;
        Bitmap muteIcon = Properties.Resources.mute;

        Bitmap pauseIcon = Properties.Resources.pause;
        Bitmap playIcon = Properties.Resources.play;

        Bitmap waitingPie;

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

            nrOfTakts = 0;

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

            //counter label
            labelTakts = nrOfTakts.ToString();
            labelFont = new System.Drawing.Font("Arial", 12);
            labelBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Black);
            labelFormat = new System.Drawing.StringFormat();
            labelX = (int)(0.13 * height);
            labelY = (int)(0.07 * height);

            //time properties
            time = new System.Windows.Forms.Timer();
            refresh = new System.Windows.Forms.Timer();
            stopwatch = new System.Diagnostics.Stopwatch();
            buttonHighlight = new System.Diagnostics.Stopwatch();
            playStopTime = new System.Windows.Forms.Timer();

            //register EventHandler for timer
            time.Tick += new EventHandler(time_tick);
            time.Interval = taktInMS;

            refresh.Tick += new EventHandler(refresh_tick);
            refresh.Interval = refreshRate;

            playStopTime.Tick += new EventHandler(playStopTime_tick);

            //has to be moved
            time.Start();
            refresh.Start();
            stopwatch.Start();
            
            // double buffering against flickering
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            //button color
            g.FillRectangle(buttonBrush, buttonRecord);

            if (Mic == ButtonStates.StoppedRecording)
            {
                g.FillRectangle(buttonBrush, buttonMute);
                g.FillRectangle(buttonBrush, buttonPlay);
            }
            else
            {
                g.FillRectangle(inactiveBrush, buttonMute);
                g.FillRectangle(inactiveBrush, buttonPlay);
            }

            //button outline
            if (Mic == ButtonStates.WaitingToRecord ||
                Mic == ButtonStates.WaitingToEndRecord ||
                (buttonRecordPushed && buttonHighlight.ElapsedMilliseconds < highlightTime))
            {
                g.DrawRectangle(thickPen, buttonRecord);
            }
            else
            {
                g.DrawRectangle(linePen, buttonRecord);
            }

            if (buttonMutePushed && buttonHighlight.ElapsedMilliseconds < highlightTime)
            {
                g.DrawRectangle(thickPen, buttonMute);
            }
            else
            {
                g.DrawRectangle(linePen, buttonMute);
            }

            if (buttonPlayPushed && buttonHighlight.ElapsedMilliseconds < highlightTime)
            {
                g.DrawRectangle(thickPen, buttonPlay);
            }
            else
            {
                g.DrawRectangle(linePen, buttonPlay);
            }

            //counter
            g.FillRectangle(stateDefaultBrush, counter);
            g.FillRectangle(backgroundObjectLightBrush, counter);
            g.FillEllipse(backgroundBrush, counterCurve);

            labelTakts = nrOfTakts.ToString();
            g.DrawString(labelTakts, labelFont, labelBrush, labelX, labelY, labelFormat);

            //timer
            if ((Mic == ButtonStates.RecordEmpty ||
                Mic == ButtonStates.WaitingToRecord) &&
                !recordInside)
            {
                g.FillEllipse(timerBrush, timer);
            }
            else if (Mic == ButtonStates.Recording ||
                Mic == ButtonStates.WaitingToEndRecord)
            {
                g.FillEllipse(timerRecordBrush, timer);
            }
            else
            {
                g.FillEllipse(timerPlayBrush, timer);
            }

            g.DrawEllipse(linePen, timer);

            //starting marker
            g.DrawLine(linePen, timerMid, getPointOnCircle(0.0));

            //moving marker
            if (Mic == ButtonStates.RecordEmpty)
            {
                g.DrawLine(thickPen, timerMid, getPointOnCircle(0.0));
            }
            else if (Mic == ButtonStates.WaitingToRecord)
            {
                waitingPie = getBmpPie(timerRecordBrush);

                g.DrawImage(waitingPie, timer.X, timer.Y);

                g.DrawEllipse(linePen, timer);
            }
            else if (Mic == ButtonStates.WaitingToEndRecord)
            {
                waitingPie = getBmpPie(timerPlayBrush);

                g.DrawImage(waitingPie, timer.X, timer.Y);

                g.DrawEllipse(linePen, timer);
            }
            else if (Mic == ButtonStates.StoppedRecording &&
                     (Play == ButtonStates.Playing ||
                      Play == ButtonStates.Stopping))
            {
                for (double i = 1; i < nrOfTakts; i++)
                {
                    double ratio = i / nrOfTakts;
                    g.DrawLine(linePen, timerMid, getPointOnCircle(ratio));
                }

                g.DrawLine(thickPen, timerMid, getPointOnCircle(getCustomTaktRatio()));
            }
            if (Mic == ButtonStates.Recording ||
                Mic == ButtonStates.WaitingToRecord ||
                Mic == ButtonStates.WaitingToEndRecord)
            {
                g.DrawLine(thickPen, timerMid, getPointOnCircle(getTaktRatio()));
            }

            //inner circle
            g.FillEllipse(stateDefaultBrush, timerInnerCircle);
            g.FillEllipse(backgroundObjectLightBrush, timerInnerCircle);

            //places icons
            if (Mic == ButtonStates.RecordEmpty)
            {
                g.DrawImage(recordEmptyIcon, buttonRecordIcon);
            }
            else if (Mic == ButtonStates.WaitingToRecord)
            {
                if (recordInside)
                {
                    g.DrawImage(stopRecordingIcon, buttonRecordIcon);
                }
                else
                {
                    g.DrawImage(recordEmptyIcon, buttonRecordIcon);
                }
            }
            else if (Mic == ButtonStates.Recording)
            {
                g.DrawImage(recordingIcon, buttonRecordIcon);
            }
            else if (Mic == ButtonStates.WaitingToEndRecord)
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

            if (buttonRecord.Contains(new System.Drawing.Point(e.X, e.Y)))
            {
                buttonRecordPushed = true;
                buttonHighlight.Restart();
            }

            if (buttonMute.Contains(new System.Drawing.Point(e.X, e.Y)))
            {
                buttonMutePushed = true;
                buttonHighlight.Restart();
            }

            if (buttonPlay.Contains(new System.Drawing.Point(e.X, e.Y)))
            {
                buttonPlayPushed = true;
                buttonHighlight.Restart();
            }
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);

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

        //returns the circular angle (between 0 and 360) of a ratio (between 0 and 1)
        private float getAngle(double ratio)
        {
            float var = (float)(ratio * 360);
            if (var < 90)
            {
                //var += 270;
            }
            return var;
        }

        private Bitmap getBmpPie(Brush pieBrush)
        {
            Bitmap bmp = new Bitmap(2 * timerRadius, 2 * timerRadius);
            Rectangle bmpRect = new Rectangle(0, 0, 2 * timerRadius, 2 * timerRadius);

            using (Graphics gfx = Graphics.FromImage(bmp))
            {
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                gfx.FillPie(pieBrush, bmpRect, getAngle(0.0), getAngle(getTaktRatio()));
            }

            bmp.RotateFlip(RotateFlipType.Rotate270FlipNone);

            return bmp;
        }

        private void time_tick(object source, EventArgs e)
        {
            onBar();
            stopwatch.Restart();
        }

        private void playStopTime_tick(object source, EventArgs e)
        {
            onCustomBar();
            playStopWatch.Restart();
        }

        private void refresh_tick(object source, EventArgs e)
        {
            if (buttonHighlight.ElapsedMilliseconds > highlightTime)
            {
                buttonRecordPushed = false;
                buttonMutePushed = false;
                buttonPlayPushed = false;
            }

            this.Refresh();
        }

        private void onBar()
        {
            if (Mic == ButtonStates.WaitingToRecord &&
                waitingToRecord)
            {
                Mic = ButtonStates.Recording;
                waitingToRecord = false;
                nrOfTakts = 0;

                if (recordInside)
                {
                    //TODO: remove audio
                }

                //TODO: start recording
                SoundEngine.recordLoop(loopId);

                Volume = ButtonStates.Sound;
                Play = ButtonStates.Playing;
            }
            else if (Mic == ButtonStates.Recording)
            {
                nrOfTakts += 1;
            }
            else if (Mic == ButtonStates.WaitingToEndRecord &&
                waitingToEndRecord)
            {
                Mic = ButtonStates.StoppedRecording;
                recordInside = true;
                nrOfTakts += 1;

                //TODO: stop recording
                SoundEngine.stopRecordLoop(loopId);

                loopInMS = nrOfTakts * taktInMS;

                playStopTime = new System.Windows.Forms.Timer();
                playStopWatch = new System.Diagnostics.Stopwatch();

                playStopTime.Interval = loopInMS;

                //TODO: start playing audio
                SoundEngine.playLoop(loopId);

                playStopTime.Start();
                playStopWatch.Start();
            }

            if (nrOfTakts >= 10)
            {
                labelX = (int)(0.1 * widgetHeight);
            }
            else
            {
                labelX = (int)(0.13 * widgetHeight);
            }
        }

        private void onCustomBar()
        {
            //TODO: play the audio again
            SoundEngine.playLoop(loopId);
        }

        //returns the elapsed time value since the last takt (between 0 and 1)
        private double getTaktRatio()
        {
            double elapsed = stopwatch.ElapsedMilliseconds;
            double taktRatio = elapsed / taktInMS;
            return taktRatio;
        }

        private double getCustomTaktRatio()
        {
            double elapsed = playStopWatch.ElapsedMilliseconds;
            double taktRatio = elapsed / loopInMS;
            return taktRatio;
        }

        private void changeButtonState(Rectangle rect)
        {
            if (buttonRecord == rect)
            {
                if (Mic == ButtonStates.RecordEmpty)
                {
                    Mic = ButtonStates.WaitingToRecord;

                    waitingToRecord = true;
                }
                else if (Mic == ButtonStates.WaitingToRecord)
                {
                    if (recordInside)
                    {
                        Mic = ButtonStates.StoppedRecording;
                    }
                    else
                    {
                        Mic = ButtonStates.RecordEmpty;
                    }
                    
                    waitingToRecord = false;
                }
                else if (Mic == ButtonStates.Recording)
                {
                    Mic = ButtonStates.WaitingToEndRecord;

                    waitingToEndRecord = true;
                }
                else if (Mic == ButtonStates.WaitingToEndRecord)
                {
                    Mic = ButtonStates.Recording;

                    waitingToEndRecord = false;
                }
                else if (Mic == ButtonStates.StoppedRecording)
                {
                    Mic = ButtonStates.WaitingToRecord;

                    waitingToRecord = true;
                }
            }
            else if (buttonMute == rect)
            {
                if (Mic == ButtonStates.StoppedRecording)
                {
                    if (Volume == ButtonStates.Sound)
                    {
                        Volume = ButtonStates.Mute;
                        //TODO: mute sound
                    }
                    else if (Volume == ButtonStates.Mute)
                    {
                        Volume = ButtonStates.Sound;
                        //TODO: unmute sound
                    }
                }
            }
            else if (buttonPlay == rect)
            {
                if (Mic == ButtonStates.StoppedRecording)
                {
                    if (Play == ButtonStates.Playing)
                    {
                        Play = ButtonStates.Stopping;

                        playStopTime.Stop();
                        playStopWatch.Stop();

                        //TODO: stop audio
                        //Edit form Niklas: when you stop the loop it is disposed and cant be started again. I changed it to pause.
                        SoundEngine.pauseLoop(loopId);
                    }
                    else if (Play == ButtonStates.Stopping)
                    {
                        Play = ButtonStates.Playing;

                        playStopTime.Start();
                        playStopWatch.Start();

                        //TODO: resume audio
                        SoundEngine.playLoop(loopId);
                    }
                }
            }
        }

    }
}
