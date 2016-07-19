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
        private Brush barColor = Brushes.Orange;

        private int sideOffset;
        private int inBarOffset = 7;
        private SoundEngine soundEngine;
        private Rectangle mainSoundBar;
        private Rectangle effectBar;

        private Label mainSoundText;
        private Label echoEffect;
        private Label distortionEffect;
        private Label chorusEffect;
        private Label effectDrag;
        private bool effectDragging;
        private List<Label> effectLabels;


        public MuGetEffects(string text, int x, int y, int width, int height, SoundEngine soundEngine) : base(text, x, y, width, height)
        {
            effectLabels = new List<Label>();
            this.Controls.Add(effectDrag);
            this.soundEngine = soundEngine;
            sideOffset = height / 11;
            mainSoundBar = new Rectangle(sideOffset, sideOffset, width - 2 * sideOffset, sideOffset * 4);
            mainSoundText = new Label {
                Location = new Point(sideOffset, sideOffset),
                Height = sideOffset * 4,
                Width = sideOffset * 2,
                Text = "Input",
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.DarkOrange
            };
            this.Controls.Add(mainSoundText);

            effectBar = new Rectangle(sideOffset, sideOffset * 6, width - 2 * sideOffset, sideOffset * 4);
            echoEffect = new Label
            {
                Location = new Point(sideOffset, sideOffset * 6),
                Width = sideOffset * 4,
                Height = sideOffset * 4,
                Text = "Echo",
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.DarkOrange
            };
            this.Controls.Add(echoEffect);
            echoEffect.MouseDown += Effect_MouseDown;
            echoEffect.MouseUp += Effect_MouseUp;
            echoEffect.MouseMove += Effect_MouseMove;

            distortionEffect = new Label
            {
                Location = new Point(sideOffset * 5 + inBarOffset, sideOffset * 6),
                Width = sideOffset * 4,
                Height = sideOffset * 4,
                Text = "Distortion",
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.DarkOrange
            };
            this.Controls.Add(distortionEffect);
            distortionEffect.MouseDown += Effect_MouseDown;
            distortionEffect.MouseUp += Effect_MouseUp;
            distortionEffect.MouseMove += Effect_MouseMove;

            chorusEffect = new Label
            {
                Location = new Point(sideOffset * 9 + inBarOffset * 2, sideOffset * 6),
                Width = sideOffset * 4,
                Height = sideOffset * 4,
                Text = "Chorus",
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.DarkOrange,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(chorusEffect);
            chorusEffect.MouseDown += Effect_MouseDown;
        }

        private void Effect_MouseUp(object sender, MouseEventArgs e)
        {
            if (effectDragging) {
                Label tmp = sender as Label;
                Point location = this.PointToClient(tmp.PointToScreen(e.Location));
                if (mainSoundBar.Contains(location))
                {
                    Label effect = new Label
                    {
                        Location = new Point(mainSoundBar.X + mainSoundText.Width + inBarOffset*(1+effectLabels.Count) + effectDrag.Width*effectLabels.Count, mainSoundBar.Y),
                        Width = effectDrag.Width,
                        Height = effectDrag.Height,
                        Text = effectDrag.Text,
                        TextAlign = effectDrag.TextAlign,
                        BackColor = effectDrag.BackColor,
                        BorderStyle = effectDrag.BorderStyle
                    };
                    this.Controls.Add(effect);
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
            g.FillRectangle(barColor, mainSoundBar);
            g.FillRectangle(barColor, effectBar);
            base.OnPaint(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
        }
        
    }
}
