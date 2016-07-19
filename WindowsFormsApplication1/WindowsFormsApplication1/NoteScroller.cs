using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;




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

        private int pixelInterval;
        
        // time between timer ticks
        
        private int speedInMS = 50;

        private int sliderWidth = 20;

        private int ratioSliderImage;

        
        private static Color inactiveColor = Color.FromArgb(50, Color.Red);

        private static Color introColor = Color.FromArgb(127, 202, 219);
        private static Color verseColor = Color.FromArgb(20, 82, 96);
        private static Color chorusColor = Color.FromArgb(41, 115, 132);
        private static Color sliderColor = Color.FromArgb(254, 178, 119);




        private Pen borderPen = Pens.Black;
        private SolidBrush sliderBrush = new SolidBrush(sliderColor);
        private SolidBrush introBrush = new SolidBrush(introColor);
        private SolidBrush verseBrush = new SolidBrush(verseColor);
        private SolidBrush chorusBrush = new SolidBrush(chorusColor);
        private SolidBrush passedBrush = new SolidBrush(inactiveColor);



        private Rectangle nsWindow;
        private Rectangle sliderWindow;
        private Rectangle slider;
        private Rectangle inactiveRect;

        private Rectangle intro;
        private Rectangle chorus;
        private Rectangle chorus2;
        private Rectangle verse;
        private Rectangle verse2;
        

        // rectangle to crop relevant rectangle from image
        private Rectangle notesTargetRect;

        private int widgetWidth;
        private int widgetHeight;

                    
        private int nsWindowHeight;
        
        private Image notesImage;
        private int imageWidth;

        private System.Windows.Forms.Timer timer;

        private float sliderOffsetHelp;
        private int oldX;
        private double pixelOffset = 0;


        public NoteScroller(String text, Int32 x, Int32 y, Int32 width, Int32 height)
            : base(text, x, y, width, height)
        {
            widgetWidth = width;
            widgetHeight = height;

            pixelInterval = setPixelInterval();
            MuGet.TempoChanged += tempoChangedPixelInterval;
            nsWindowHeight = (int) (nsToSliderRatio * widgetHeight);

            // notescroller window
            nsWindow = new Rectangle(0, 0, width, nsWindowHeight);
            
            sliderWindow = new Rectangle(0, nsWindowHeight, width, height - nsWindowHeight);
            slider = new Rectangle(0, nsWindowHeight, sliderWidth, height - nsWindowHeight);

            intro = new Rectangle(0, nsWindowHeight, width / 5, height - nsWindowHeight);
            verse = new Rectangle(width / 5, nsWindowHeight, width / 5 - width / 20, height - nsWindowHeight);
            chorus = new Rectangle(2 *(width / 5)- width / 20, nsWindowHeight, width / 5 + width / 20, height - nsWindowHeight);
            verse2 = new Rectangle(3 * (width / 5), nsWindowHeight, width / 5 - width / 20, height - nsWindowHeight);
            chorus2 = new Rectangle(4 * (width / 5) - width / 20, nsWindowHeight, width / 5 + width / 20, height - nsWindowHeight);


            // load image. make sure to load in image properties
            try
            {
                //notesImage = Image.FromFile("test.bmp");
                notesImage = Properties.Resources.notes;
                imageWidth = notesImage.Width;
                notesTargetRect = new Rectangle(0, 0, width, notesImage.Height);
                ratioSliderImage = (imageWidth - nsWindow.Width) / (sliderWindow.Width - sliderWidth);

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

            g.FillRectangle(introBrush, intro);
            g.FillRectangle(verseBrush, verse);
            g.FillRectangle(verseBrush, verse2);
            g.FillRectangle(chorusBrush, chorus);
            g.FillRectangle(chorusBrush, chorus2);


            //g.DrawRectangle(borderPen, slider);
            g.FillRectangle(sliderBrush, slider);

            // draw notesTargetRect into nsWindow
            g.DrawImage(notesImage, nsWindow, notesTargetRect, GraphicsUnit.Pixel);      
                       
            inactiveRect = new Rectangle(0, nsWindowHeight, slider.X, sliderWindow.Height);
            g.FillRectangle(passedBrush, inactiveRect);


            base.OnPaint(e);
        }


        private void tempoChangedPixelInterval(object source, EventArgs e)
        {
            pixelInterval = setPixelInterval();
        }
       
        // with every timer tick, move notesTargetRect to the side
        private void timer_tick(object source, EventArgs e)
        {

            if (notesTargetRect.X + notesTargetRect.Width + pixelInterval < imageWidth)
            {
                notesTargetRect.X += pixelInterval;
                slider.X += sliderOffset(pixelInterval);
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
        private int sliderOffset(float interval)
        {
            float ratio = (float) (imageWidth - widgetWidth) / (float) (widgetWidth -  sliderWidth);
            
            float os = (float) interval / (float) ratio;
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

        private void moveNotes(MouseEventArgs e)
        {
            int start = nsWindow.X;
            float diff = e.X - start;
        }

        private int setPixelInterval()
        {
            double bpm = Bpm;
            double msPerBeat = BeatLength;
            double msPerBar = 4 * msPerBeat;



            double result = ( (speedInMS / msPerBar) * 448);
           
            pixelOffset += result % 1;
            if (pixelOffset >= 1)
            {
                result++;
                pixelOffset--;
            }

            return (int)result + 2;


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

            // if intro, verse or chorus is clicked, move slider and notesTargetRect to position (hard coded)

            } else if (intro.Contains(new Point(e.X, e.Y)))
            {
                notesTargetRect.X = 0;
                slider.X = 0;
                finished = false;
            }
            else if (verse.Contains(new Point(e.X, e.Y)))
            {
                notesTargetRect.X = (imageWidth - nsWindow.Width) / 5;
                slider.X = (sliderWindow.Width - sliderWidth) / 5;
                finished = false;
            }
            else if (chorus.Contains(new Point(e.X, e.Y)))
            {
                notesTargetRect.X = 2 * ((imageWidth - nsWindow.Width) / 5) - (imageWidth / 20);
                slider.X = 2 * ((sliderWindow.Width - sliderWidth) / 5) - sliderWindow.Width / 20;
                finished = false;
            }
            else if (verse2.Contains(new Point(e.X, e.Y)))
            {
                notesTargetRect.X = 3 * ((imageWidth - nsWindow.Width) / 5);
                slider.X = 3 * ((sliderWindow.Width - sliderWidth) / 5);
                finished = false;
            }
            else if (chorus2.Contains(new Point(e.X, e.Y)))
            {
                notesTargetRect.X = 4 * ((imageWidth - nsWindow.Width) / 5) - (imageWidth / 20);
                slider.X = 4 * ((sliderWindow.Width - sliderWidth) / 5) - sliderWindow.Width / 20;
                finished = false;
            }
            this.Refresh();
        }



        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (slider.Contains(new Point(e.X, e.Y)))
            {
            //    dragMode = true;
                
            }
            if (nsWindow.Contains(new Point(e.X, e.Y)))
            {
                oldX = e.X;
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

            //dragging: move notesTargetRect to the left or right. The slider has to be positioned too. 
            if (dragMode)
            {
                running = true;
                if (oldX <= e.X)
                {
                    if (slider.X < sliderWindow.X)
                    {
                        slider.X = sliderWindow.X;
                    } else {
                        slider.X -= (e.X - oldX) / ratioSliderImage;
                    }

                    notesTargetRect.X = notesTargetRect.X - (e.X - oldX);
                }
                else
                {
                    if (slider.X >= sliderWindow.X + sliderWindow.Width - sliderWidth)
                    {
                        slider.X = sliderWindow.X + sliderWindow.Width - sliderWidth;
                    }
                    else
                    {
                        slider.X += (oldX - e.X) / ratioSliderImage;
                    }                                            

                    notesTargetRect.X = notesTargetRect.X + (oldX - e.X);
                }

                if (notesTargetRect.X < 0)
                {
                    notesTargetRect.X = 0;
                    finished = false;
                }
                if (notesTargetRect.X > imageWidth - nsWindow.Width)
                {
                    notesTargetRect.X = imageWidth - nsWindow.Width;
                    finished = true;
                }
                else
                {
                    finished = false;


                }

              
                oldX = e.X;
                
                this.Refresh();
            }

        }

    }
}
