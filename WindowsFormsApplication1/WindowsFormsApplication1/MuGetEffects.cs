using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace Test
{
    class MuGetEffects : MuGet
    {
        private Brush barColor = backgroundObjectLightBrush;

        private int sideOffset;
        private int labelWidth = 40;
        private int inBarOffset = 7;
        private FlowLayoutPanel mainSoundBar;
        private FlowLayoutPanel effectBar;

        private Label mainSoundText;
        private Label echoEffect;
        private Label distortionEffect;
        private Label chorusEffect;
        private Label effectDrag;
        private bool effectDragging;
        private List<Label> effectLabels;


        public MuGetEffects(string text, int x, int y, int width, int height) : base(text, x, y, width, height)
        {
            effectLabels = new List<Label>();
            this.Controls.Add(effectDrag);
            sideOffset = height / 11;
            //mainSoundBar = new Rectangle(sideOffset, sideOffset, width - 2 * sideOffset, sideOffset * 4);
            mainSoundBar = new FlowLayoutPanel
            {
                Location = new Point(sideOffset, sideOffset),
                Width = width - 2 * sideOffset,
                Height = sideOffset * 4,
                BackColor = backgroundObjectLightColor,
                Margin = new Padding(0, 0, inBarOffset, 0)
            };
            this.Controls.Add(mainSoundBar);

            mainSoundText = new Label
            {
                //Location = new Point(sideOffset, sideOffset),
                Height = sideOffset * 4,
                Width = (2 * labelWidth) / 3,
                Text = "Input",
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = backgroundObjectColor
            };
            //this.Controls.Add(mainSoundText);
            mainSoundBar.Controls.Add(mainSoundText);


            effectBar = new FlowLayoutPanel
            {
                Location = new Point(sideOffset, sideOffset * 6),
                Width = width - 2 * sideOffset,
                Height = sideOffset * 4,
                BackColor = backgroundObjectLightColor,
                Margin = new Padding(0, 0, inBarOffset, 0)
            };
            this.Controls.Add(effectBar);
            //effectBar = new Rectangle(sideOffset, sideOffset * 6, width - 2 * sideOffset, sideOffset * 4);
            echoEffect = new Label
            {
                //Location = new Point(sideOffset, sideOffset * 6),
                Width = labelWidth,
                Height = sideOffset * 4,
                Text = "Echo",
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = activeColor
            };
            //this.Controls.Add(echoEffect);
            effectBar.Controls.Add(echoEffect);
            echoEffect.MouseDown += Effect_MouseDown;
            echoEffect.MouseUp += Effect_MouseUp;
            echoEffect.MouseMove += Effect_MouseMove;

            distortionEffect = new Label
            {
                //Location = new Point(sideOffset * 5 + inBarOffset, sideOffset * 6),
                Width = labelWidth,
                Height = sideOffset * 4,
                Text = "Distortion",
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = activeColor
            };
            //this.Controls.Add(distortionEffect);
            effectBar.Controls.Add(distortionEffect);
            distortionEffect.MouseDown += Effect_MouseDown;
            distortionEffect.MouseUp += Effect_MouseUp;
            distortionEffect.MouseMove += Effect_MouseMove;

            chorusEffect = new Label
            {
                //Location = new Point(sideOffset * 9 + inBarOffset * 2, sideOffset * 6),
                Width = labelWidth,
                Height = sideOffset * 4,
                Text = "Chorus",
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = activeColor
            };
            //this.Controls.Add(chorusEffect);
            effectBar.Controls.Add(chorusEffect);
            chorusEffect.MouseDown += Effect_MouseDown;
            chorusEffect.MouseUp += Effect_MouseUp;
            chorusEffect.MouseMove += Effect_MouseMove;
        }
        
        private void Effect_MouseUp(object sender, MouseEventArgs e)
        {
            if (effectDragging)
            {
                Label tmp = sender as Label;
                Point location = this.PointToClient(tmp.PointToScreen(e.Location));
                if (mainSoundBar.Bounds.Contains(location))
                {
                    Console.WriteLine(mainSoundBar.GetChildAtPoint(location));
                    Label effect = new Label
                    {
                        //Location = new Point(mainSoundBar.Bounds.X + mainSoundText.Width + inBarOffset*(1+effectLabels.Count) + effectDrag.Width*effectLabels.Count, mainSoundBar.Y),
                        Width = effectDrag.Width,
                        Height = effectDrag.Height,
                        Text = effectDrag.Text,
                        TextAlign = effectDrag.TextAlign,
                        BackColor = effectDrag.BackColor,
                        BorderStyle = effectDrag.BorderStyle
                    };
                    //this.Controls.Add(effect);
                    mainSoundBar.Controls.Add(effect);
                    effectLabels.Add(effect);
                }
                else
                {
                }
                effectDrag.Dispose();
            }
            effectDragging = false;
        }
        
        private void Effect_MouseDown(object sender, MouseEventArgs e)
        {
            Label tmp = sender as Label;
            Point location = this.PointToClient(tmp.PointToScreen(e.Location));
            effectDrag = new Label
            {
                Location = new Point(location.X - tmp.Width / 2, location.Y - tmp.Height / 2),
                Width = tmp.Width,
                Height = tmp.Height,
                Text = tmp.Text,
                TextAlign = tmp.TextAlign,
                BackColor = tmp.BackColor,
                BorderStyle = tmp.BorderStyle
            };
            this.Controls.Add(effectDrag);
            effectDrag.BringToFront();
            effectDragging = true;
        }

        private void Effect_MouseMove(object sender, MouseEventArgs e)
        {
            if (effectDragging)
            {
                Label tmp = sender as Label;
                Point location = this.PointToClient(tmp.PointToScreen(e.Location));
                effectDrag.Location = new Point(location.X - tmp.Width / 2, location.Y - tmp.Height / 2);
                effectDrag.Refresh();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            //g.FillRectangle(barColor, mainSoundBar);
            //g.FillRectangle(barColor, effectBar);
            base.OnPaint(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }

    }
}
