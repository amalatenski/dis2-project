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
        //Brushes are used to fill objects.
        private static Brush buttonBrush = System.Drawing.Brushes.Red;
        private static Brush timerBrush = System.Drawing.Brushes.Lime;
        private static Brush timerRecordBrush = System.Drawing.Brushes.Blue;
        private static Brush timerPlayBrush = System.Drawing.Brushes.Green;

        private static int timerInnerCircleSize = 25;
        private static int timerLineThickness = 2;

        /*
        Bitmap recordEmptyIcon = new Bitmap("C:/Users/amala_000/Documents/icons/recordEmpty.png");
        Bitmap recordingIcon = new Bitmap("C:/Users/amala_000/Documents/icons/record.png");
        Bitmap stopRecordingIcon = new Bitmap("C:/Users/amala_000/Documents/icons/record.png");

        Bitmap soundIcon = new Bitmap("C:/Users/amala_000/Documents/icons/sound.png");
        Bitmap muteIcon = new Bitmap("C:/Users/amala_000/Documents/icons/mute.png");

        Bitmap pauseIcon = new Bitmap("C:/Users/amala_000/Documents/icons/pause.png");
        Bitmap playIcon = new Bitmap("C:/Users/amala_000/Documents/icons/play.png");
        */
        
        private Rectangle buttonRecord;
        private Rectangle buttonMute;
        private Rectangle buttonPlay;

        private Rectangle buttonRecordIcon;
        private Rectangle buttonMuteIcon;
        private Rectangle buttonPlayIcon;

        private Rectangle timer;
        private Rectangle timerInnerCircle;

        private bool dragMode = false;

        private int widgetWidth;
        private int widgetHeight;
        private int gap;
        private int buttonX;

        private System.Drawing.Point timerMid = new System.Drawing.Point();
        private int timerRadius;

        private double taktPercent = 0;

        //Constructor calls base class constructor of Control
        public MuGetLoop(String text, Int32 x, Int32 y, Int32 width, Int32 height)
            : base(text, x, y, width, height)
        {
            widgetWidth = width;
            widgetHeight = height;

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

            //timer properties
            timer = new Rectangle(gap, gap, widgetHeight - 2 * gap, widgetHeight - 2 * gap);
            timerMid = getMidpoint(timer);
            timerRadius = timer.Height / 2;
            timerInnerCircle = getRectangleAroundPoint(timerMid.X, timerMid.Y, timerInnerCircleSize, timerInnerCircleSize);
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            //draws the timer and the buttons
            Graphics g = e.Graphics;
            
            g.FillRectangle(buttonBrush, buttonRecord);
            g.FillRectangle(buttonBrush, buttonMute);
            g.FillRectangle(buttonBrush, buttonPlay);

            g.FillEllipse(timerBrush, timer);
            g.FillEllipse(buttonBrush, timerInnerCircle);
            
            g.DrawImage(recordEmptyIcon, buttonRecordIcon);
            g.DrawImage(soundIcon, buttonMuteIcon);
            g.DrawImage(pauseIcon, buttonPlayIcon);
            
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

            //helping variable
            taktPercent += 0.5;
            if (taktPercent >= 100)
            {
                taktPercent -= 100;
            }
            Console.WriteLine(taktPercent);
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

        //old code that is maybe useful later on
        //public void HandStateChanged(TouchEventArgs e)
        //{
        //    System.Windows.Point tmp = e.TouchDevice.GetTouchPoint((IInputElement)this).Position;
        //    if (slider.Contains(new System.Drawing.Point((int)tmp.X, (int)tmp.Y)))
        //    {
        //        dragMode = true;
        //    }
        //    else
        //    {
        //        dragMode = false;

        //    }
        //}

        //public void HandMoved(TouchEventArgs e)
        //{
        //    //checks if in drag mode
        //    if (dragMode)
        //    {
        //        slider.X = e.getHandX() - sliderWidth / 2;

        //        if (slider.X < 10) { slider.X = 10; }
        //        if (slider.X > this.Width - sliderLineOffset - sliderWidth) { slider.X = this.Width - sliderLineOffset - sliderWidth; }
        //    }
        //    //repaints the widget. Without this the changes would not appear instantly.
        //    this.Refresh();
        //}
    }
}
