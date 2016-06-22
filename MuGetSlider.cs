using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    /*
     * Sample MuGet implementation.
     */
    class MuGetSlider : MuGet
    {
        //default colors and stuff
        //Pens are used for not filled objects.
        private static Pen linePen = System.Drawing.Pens.Black;
        //Brushes are used to fill objects.
        private static Brush sliderBrush = System.Drawing.Brushes.Red;
        private static Int32 sliderWidth = 10;
        private static Int32 sliderHeight = 20;
        private static Int32 sliderLineOffset = 10;

        private Rectangle slider;
        private bool dragMode = false;
        HandEventArgs.HandSide dragSide;

        //Constructor calls base class constructor of Control
        public MuGetSlider(String text, Int32 x, Int32 y, Int32 width, Int32 height)
            : base(text, x, y, width, height)
        {
            //creates the movable rectangle for the slider
            slider = new System.Drawing.Rectangle(10, (int)(this.Height/2-sliderHeight/2), sliderWidth, sliderHeight);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //draws the line of the slider and the slider
            Graphics g = e.Graphics;
            g.DrawLine(linePen, new Point(sliderLineOffset, (int)this.Height/2), new Point(this.Width-sliderLineOffset, this.Height/2));
            g.FillRectangle(sliderBrush, slider);

            //calls the OnPaint of the base class. Without this the border would disappear.
            base.OnPaint(e);
        }

        public override void HandStateChanged(HandEventArgs e)
        {
            //checks for the new hand state
            if (e.getHandState() == HandEventArgs.HandState.Closed)
            {
                //checks if the hand position is within the slider. The coordinates are relative to the widget coordinates (not application).
                if (slider.Contains(new Point(e.getHandX(), e.getHandY())))
                {
                    dragSide = e.agent;
                    dragMode = true;
                }
            }
            else
            {
                if (e.agent == dragSide)
                {
                    dragMode = false;
                }
            }
        }

        public override void HandMoved(HandEventArgs e)
        {
            //checks if in drag mode
            if(dragMode)
            {
                slider.X = e.getHandX() - sliderWidth/2;

                if (slider.X < 10) { slider.X = 10; }
                if (slider.X > this.Width - sliderLineOffset - sliderWidth) { slider.X = this.Width - sliderLineOffset - sliderWidth; }
            }
            //repaints the widget. Without this the changes would not appear instantly.
            this.Refresh();
        }
    }
}
