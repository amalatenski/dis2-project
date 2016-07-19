using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Test
{
    class MuGetTempoWidget : MuGet
    {
        public MuGetTempoWidget(String text, Int32 x, Int32 y, Int32 width)
            : base(text, x, y, width, width)
        {
        }

        private List<int> ellipses = new List<int>();
        private int ellipsesMax = 20;

        protected override void OnPaint(PaintEventArgs e)
        {
            //draws the line of the slider and the slider
            Graphics g = e.Graphics;
            g.DrawEllipse(Pens.DarkGray, new Rectangle(0, 0, Width, Width));

            for (int i = 0; i < ellipses.Count; i++)
            {
                if (Width - ellipses[i] <= ellipses[i])
                {
                    ellipses.RemoveAt(i);
                }
                else
                {
                    g.DrawEllipse(Pens.Gray, new Rectangle(ellipses[i], ellipses[i], Width - 2 * ellipses[i], Width - 2 * ellipses[i]));
                }
            }

            //calls the OnPaint of the base class. Without this the border would disappear.
            base.OnPaint(e);
        }
        
        protected override void OnBeat(BeatEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnBeat");
            base.OnBeat(e);
            this.ellipses.Add(0);
            if (this.ellipses.Count > ellipsesMax) this.ellipses.RemoveAt(0);
        }

        protected override void OnTick(EventArgs e)
        {
            base.OnTick(e);
            for (int i = 0; i < ellipses.Count; ellipses[i++]++);
            this.Refresh();
        }
    }
}