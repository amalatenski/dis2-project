using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows;




namespace Test
{
    class NoteScroller : MuGet
    {

        // running checks if the notescroller is currently playing

        private bool running = false;
        
        // finished checks if the slider reached the end
        
        private bool finished = false;
        private bool dragMode = false;
        
        // height of slider window relative to note scroller window
        
        private static double nsToSliderRatio = 0.9;

        // pixelInterval is the number of pixels the picture is moved every timer tick

        private int pixelInterval = 10;
        
        // time between timer ticks
        
        private int speedInMS = 60;

        private int sliderWidth = 20; 


        private Pen borderPen = Pens.Black;
        private Brush sliderBrush = Brushes.Red;

        private Rectangle nsWindow;
        private Rectangle sliderWindow;
        private Rectangle slider;

        // rectangle to crop relevant rectangle from image
        private Rectangle notesTargetRect;

        private int widgetWidth;
        private int widgetHeight;

                    
        private int nsWindowHeight;
        
        private Image notesImage;
        private int imageWidth;

        private System.Windows.Forms.Timer timer;

        private float sliderOffsetHelp;


        public NoteScroller(String text, Int32 x, Int32 y, Int32 width, Int32 height)
            : base(text, x, y, width, height)
        {
            widgetWidth = width;
            widgetHeight = height;

            nsWindowHeight = (int) (nsToSliderRatio * widgetHeight);

            // notescroller window
            nsWindow = new Rectangle(0, 0, width, nsWindowHeight);
            
            sliderWindow = new Rectangle(0, nsWindowHeight, width, height - nsWindowHeight);
            slider = new Rectangle(0, nsWindowHeight, sliderWidth, height - nsWindowHeight);
            

            // load image. make sure to load in image properties
            try{
                notesImage = Image.FromFile("test.bmp");
                imageWidth = notesImage.Width;
                notesTargetRect = new Rectangle(0, 0, width, notesImage.Height);

            } catch (Exception e){
                Console.WriteLine("Error, make sure to load the picture. " + e);
            }

            timer = new System.Windows.Forms.Timer();
;
            
            // register EventHandler for timer
            timer.Tick += new EventHandler(timer_tick);
            timer.Interval = speedInMS;
            
            // double buffering against flickering
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {

            
            Graphics g = e.Graphics;
            g.DrawRectangle(borderPen, nsWindow);
            g.DrawRectangle(borderPen, sliderWindow);
            g.DrawRectangle(borderPen, slider);

            g.FillRectangle(sliderBrush, slider);
                      
            // draw notesTargetRect into nsWindow
            g.DrawImage(notesImage, nsWindow, notesTargetRect, GraphicsUnit.Pixel);                 
           
            base.OnPaint(e);
        }

       
        // with every timer tick, move notesTargetRect to the side
        private void timer_tick(object source, EventArgs e)
        {

            if (notesTargetRect.X + notesTargetRect.Width + pixelInterval < imageWidth)
            {
                notesTargetRect.X += pixelInterval;
                slider.X += sliderOffset();
                this.Refresh();

            }
            else
            {
                timer.Stop();
                finished = true;
                running = false;
            }
        }

       
        
        // calculate how many pixels notesTargetRect has to be moved with every timer tick
        private int sliderOffset()
        {
            float ratio = (float) (imageWidth - widgetWidth) / (float) (widgetWidth -  sliderWidth);
            
            float os = (float) pixelInterval / (float) ratio;
            sliderOffsetHelp += os % 1;
        
            if (sliderOffsetHelp >= 1)
            {
                os++;
                sliderOffsetHelp--;
            }
                

            return (int) os;
 
        }

        // moves notesTargetRect while slider is dragged
        private void moveSlider()
        {
            int start = sliderWindow.X;
            float diff = slider.X - start;
            float ratio = (float)(imageWidth - widgetWidth) / (float)(widgetWidth - sliderWidth);
            notesTargetRect.X = (int) (diff * ratio);

        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (nsWindow.Contains(new Point(e.X, e.Y)))                           
            {
             
                if (running && !finished)
                {
                    running = false;
                    timer.Stop();
                }
                else if(!running && !finished)
                {
                    running = true;
                    timer.Start();
                }
                //if(running)
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (slider.Contains(new Point(e.X, e.Y)))
            {
                dragMode = true;
                
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            
            dragMode = false;
           
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragMode)
            {
                
                slider.X = e.X - sliderWidth / 2;

                if (slider.X < sliderWindow.X) 
                {
                    slider.X = sliderWindow.X;
                    finished = false;
                }
                if (slider.X >= sliderWindow.X + sliderWindow.Width - sliderWidth)
                {
                    slider.X = sliderWindow.X + sliderWindow.Width - sliderWidth;
                    finished = true;
                }
                else
                {
                    finished = false;
                }
                
            }

            moveSlider();

            this.Refresh();
        }



    }
}
