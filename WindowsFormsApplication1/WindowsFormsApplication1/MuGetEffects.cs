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
        private int labelWidth = 55;
        private int inBarOffset = 7;
        private List<Tuple<Label, EffectClass>> effectRefereence;
        private FlowLayoutPanel mainSoundBar;
        private FlowLayoutPanel effectBar;
        private MuGetSlider slider;
        private MuGet2DSlider slider2D;
        private EffectClass currentEffect;

        private Label mainSoundText;
        private Label echoEffect;
        private Label distortionEffect;
        private Label chorusEffect;
        private Label effectDrag;
        private bool effectDragging;


        public MuGetEffects(string text, int x, int y, int width, int height) : base(text, x, y, width, height)
        {
            effectRefereence = new List<Tuple<Label, EffectClass>>();
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
            mainSoundText = new Label {
                //Location = new Point(sideOffset, sideOffset),
                Height = sideOffset * 4,
                Width = (2*labelWidth) / 3,
                Text = "Input",
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = backgroundObjectColor
            };
            mainSoundText.MouseClick += MainSoundText_MouseClick;
            //this.Controls.Add(mainSoundText);
            mainSoundBar.Controls.Add(mainSoundText);


            effectBar = new FlowLayoutPanel
            {
                Location = new Point(sideOffset, sideOffset * 6),
                Width = width - 2 * sideOffset,
                Height = sideOffset * 4,
                Margin = new Padding(0, 0, inBarOffset, 0),
                Anchor = AnchorStyles.None
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
            echoEffect.MouseDown += EffectSelection_MouseDown;
            echoEffect.MouseUp += EffectSelection_MouseUp;
            echoEffect.MouseMove += EffectSelection_MouseMove;

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
            distortionEffect.MouseDown += EffectSelection_MouseDown;
            distortionEffect.MouseUp += EffectSelection_MouseUp;
            distortionEffect.MouseMove += EffectSelection_MouseMove;

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
            chorusEffect.MouseDown += EffectSelection_MouseDown;
            chorusEffect.MouseUp += EffectSelection_MouseUp;
            chorusEffect.MouseMove += EffectSelection_MouseMove;
        }

        private void MainSoundText_MouseClick(object sender, MouseEventArgs e)
        {
            slider = new MuGetSlider("volumeslider", Width / 2 - (8 * sideOffset), Height / 2 - (2 * sideOffset), 16 * sideOffset, 4 * sideOffset, 0, 1);
            slider.setPosition(SoundEngine.volume);
            slider.ActionEnded += Slider_ActionEnded;
            slider.UpdateStatus += Slider_UpdateStatus;

            this.Controls.Add(slider);
            slider.BringToFront();
        }

        private void Slider_UpdateStatus(object sender, StatusEventArgs e)
        {
            SoundEngine.volume = float.Parse(e.Status);
        }

        private void EffectSelection_MouseUp(object sender, MouseEventArgs e)
        {
            if (effectDragging) {
                Label senderLabel = sender as Label;
                Point location = this.PointToClient(senderLabel.PointToScreen(e.Location));
                if (mainSoundBar.Bounds.Contains(location))
                {

                    EffectClass effect;
                    switch (senderLabel.Text)
                    {
                        case "Echo":
                            effect = new EchoEffect();
                            break;
                        case "Distortion":
                            effect = new DistortionEffect();
                            break;
                        case "Chorus":
                            effect = new ChorusEffect();
                            break;
                        default:
                            effect = null;
                            break;
                    }

                    if (effect != null) {
                        SoundEngine.newEffect(effect);

                        Label effectLabel = new Label
                        {
                            //Location = new Point(mainSoundBar.Bounds.X + mainSoundText.Width + inBarOffset*(1+effectLabels.Count) + effectDrag.Width*effectLabels.Count, mainSoundBar.Y),
                            Width = effectDrag.Width,
                            Height = effectDrag.Height,
                            Text = effectDrag.Text,
                            TextAlign = effectDrag.TextAlign,
                            BackColor = effectDrag.BackColor,
                            BorderStyle = effectDrag.BorderStyle
                        };
                        effectLabel.MouseClick += Effect_MouseClick;

                        //this.Controls.Add(effect);
                        Label hitLabel = mainSoundBar.GetChildAtPoint(location) as Label;
                        mainSoundBar.Controls.Add(effectLabel);
                        effectRefereence.Add(new Tuple<Label, EffectClass>(effectLabel, effect));

                        if (hitLabel != null)
                        {
                            location = hitLabel.PointToClient(senderLabel.PointToScreen(e.Location));
                            int index = mainSoundBar.Controls.IndexOf(hitLabel);
                            if (location.X < hitLabel.Width / 2 && index != 0) mainSoundBar.Controls.SetChildIndex(effectLabel, index);
                            else mainSoundBar.Controls.SetChildIndex(effectLabel, index + 1);
                        }
                    }       
                }
                else
                {
                }
                effectDrag.Dispose();
            }
            effectDragging = false;
        }

        private void Effect_MouseClick(object sender, MouseEventArgs e)
        {
            Label senderLabel = sender as Label;
            EffectClass effect = effectRefereence.Find(x => x.Item1 == senderLabel).Item2;
            currentEffect = effect;
            if (effect.GetType() == typeof(EchoEffect))
            {
                slider2D = new MuGet2DSlider("echoSlider", Width / 2 - (Height - 2 * sideOffset) / 2, sideOffset, Height - 2 * sideOffset, Height - 2 * sideOffset, 0, 100, 1, 2000);
                slider2D.setPosition((effect as EchoEffect).feedback, (effect as EchoEffect).leftDelay);
            } else if(effect.GetType() == typeof(DistortionEffect))
            {
                slider2D = new MuGet2DSlider("distortionslider", Width / 2 - (Height - 2 * sideOffset) / 2, sideOffset, Height - 2 * sideOffset, Height - 2 * sideOffset, 0, 100, -60, 0);
                slider2D.setPosition((effect as DistortionEffect).edge, (effect as DistortionEffect).gain);
            } else if (effect.GetType() == typeof(ChorusEffect))
            {
                slider2D = new MuGet2DSlider("chorusslider", Width / 2 - (Height - 2 * sideOffset) / 2, sideOffset, Height - 2 * sideOffset, Height - 2 * sideOffset, 0, 20, 0, 100);
                slider2D.setPosition((effect as ChorusEffect).delay, (effect as ChorusEffect).depth);
            }

            this.Controls.Add(slider2D);
            slider2D.BringToFront();
            slider2D.UpdateStatus += Slider2D_UpdateStatus;
            slider2D.ActionEnded += Slider_ActionEnded;
        }

        private void Slider_ActionEnded(object sender, EventArgs e)
        {
            (sender as MuGet).Dispose();
            SoundEngine.refreshSound();
        }

        private void Slider2D_UpdateStatus(object sender, StatusEventArgs e)
        {
            float valueX = float.Parse(e.Status.Split(new char[] { ' ' })[0]);
            float valueY = float.Parse(e.Status.Split(new Char[] { ' ' })[1]);
            if(currentEffect.GetType() == typeof(EchoEffect))
            {
                EchoEffect echo = currentEffect as EchoEffect;
                echo.feedback = valueX;
                echo.leftDelay = valueY;
                echo.rightDelay = valueY;
            } else if(currentEffect.GetType() == typeof(DistortionEffect))
            {
                DistortionEffect distortion = currentEffect as DistortionEffect;
                distortion.edge = valueX;
                distortion.gain = valueY;
            } else if (currentEffect.GetType() == typeof(ChorusEffect))
            {
                ChorusEffect chorus = currentEffect as ChorusEffect;
                chorus.delay = valueX;
                chorus.depth = valueY;
            }
            //TODO: could be done with effectChain.
            //SoundEngine.refreshSound();
        }

        private void EffectSelection_MouseDown(object sender, MouseEventArgs e)
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

        private void EffectSelection_MouseMove(object sender, MouseEventArgs e)
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
