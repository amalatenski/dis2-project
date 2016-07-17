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
    class MuGet2DSlider : MuGet
    {
        //default colors and stuff
        //Pens are used for not filled objects.
        private static Pen linePen = System.Drawing.Pens.Black;
        //Brushes are used to fill objects.
        private static Brush sliderBrush = System.Drawing.Brushes.Red;
        private static Int32 sliderWidth = 20;
        private static Int32 sliderHeight = 20;

        private Rectangle slider;
        private bool dragMode = false;

        //Values for the two axis
        public float startX { get; set; }
        public float endX { get; set; }
        public float startY { get; set; }
        public float endY { get; set; }

        //Constructor calls base class constructor of Control
        public MuGet2DSlider(String text, Int32 x, Int32 y, Int32 width, Int32 height, float startX, float endX, float startY, float endY)
            : base(text, x, y, width, height)
        {
            this.startX = startX;
            this.startY = startY;
            this.endX = endX;
            this.endY = endY;
            //creates the movable rectangle for the slider
            slider = getRectangleAroundPoint(width/2, height/2, sliderWidth, sliderHeight);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //draws the line of the slider and the slider
            Graphics g = e.Graphics;
            g.DrawLine(linePen, new System.Drawing.Point(0, getMidpoint(slider).Y), new System.Drawing.Point(this.Width, getMidpoint(slider).Y));
            g.DrawLine(linePen, new System.Drawing.Point(getMidpoint(slider).X, 0), new System.Drawing.Point(getMidpoint(slider).X, this.Height));

            g.FillRectangle(sliderBrush, slider);

            //calls the OnPaint of the base class. Without this the border would disappear.
            base.OnPaint(e);
        }

        
        protected override void OnMouseDown(System.Windows.Forms.MouseEventArgs e)
        {
            base.OnMouseDown(e);

            slider.X = e.X - sliderWidth / 2;
            if (slider.X < 0) { slider.X = 0; }
            if (slider.X > this.Width - sliderWidth) { slider.X = this.Width - sliderWidth; }

            slider.Y = e.Y - sliderHeight / 2;
            if (slider.Y < 0) { slider.Y = 0; }
            if (slider.Y > this.Height - sliderHeight) { slider.Y = this.Height - sliderHeight; }

            updateStatus();
            //Repaints the widget. Othewise the new position would not be drawn.
            this.Refresh();
            dragMode = true;
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
                slider.X = e.X - sliderWidth / 2;
                if (slider.X < 0) { slider.X = 0; }
                if (slider.X > this.Width - sliderWidth) { slider.X = this.Width - sliderWidth; }

                slider.Y = e.Y - sliderHeight / 2;
                if (slider.Y < 0) { slider.Y = 0; }
                if (slider.Y > this.Height - sliderHeight) { slider.Y = this.Height - sliderHeight; }

                updateStatus();
                //Repaints the widget. Othewise the new position would not be drawn.
                this.Refresh();
            }
        }

        public void updateStatus()
        {
            float valueX = ((float)(getMidpoint(slider).X - sliderWidth / 2) / (float)(Width - sliderWidth)) * (endX - startX) + startX;
            float valueY = ((float)(getMidpoint(slider).Y - sliderHeight / 2) / (float)(Height - sliderHeight)) * (startY - endY) + endY;

            UpdateStatus(valueX + " " + valueY);
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
    }
}
