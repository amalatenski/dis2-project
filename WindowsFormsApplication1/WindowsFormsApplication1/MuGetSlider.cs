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
            g.DrawLine(linePen, new System.Drawing.Point(sliderLineOffset, (int)this.Height/2), new System.Drawing.Point(this.Width-sliderLineOffset, this.Height/2));
            g.FillRectangle(sliderBrush, slider);

            //calls the OnPaint of the base class. Without this the border would disappear.
            base.OnPaint(e);
        }

        
        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (slider.Contains(new System.Drawing.Point(e.X, e.Y)))
            {
                dragMode = true;
            }
        }

        protected override void OnMouseUp(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseUp(e);
            dragMode = false;
        }

        protected override void OnMouseMove(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if(dragMode)
            {
                slider.X = e.X - sliderWidth/2;

                if (slider.X < sliderLineOffset) { slider.X = sliderLineOffset; }
                if (slider.X > this.Width - sliderLineOffset - sliderWidth) { slider.X = this.Width - sliderLineOffset - sliderWidth; }

                //Raises an event that gives the slider position between 0 and 1 as a string.
                UpdateStatus(((double)(slider.X - sliderLineOffset) / (double)(this.Width - 2 * sliderLineOffset)).ToString());
            }
            //Repaints the widget. Othewise the new position would not be drawn.
            this.Refresh();
        }
    }
}
