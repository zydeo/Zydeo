using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// Types of system buttons implemented by <see cref="ZenSystemButton"/>.
    /// </summary>
    internal enum SystemButtonType
    {
        /// <summary>
        /// Close window
        /// </summary>
        Close,
        /// <summary>
        /// Minimize windows
        /// </summary>
        Minimize,
    }

    /// <summary>
    /// A system control: close button, minimize button, ...
    /// </summary>
    internal class ZenSystemButton : ZenControl
    {
        /// <summary>
        /// The button type implemented by this control.
        /// </summary>
        private readonly SystemButtonType btnType;

        /// <summary>
        /// Base line width for drawing shapes.
        /// </summary>
        private readonly int lineWidth;

        /// <summary>
        /// Ctor: take parent (form) and button type.
        /// </summary>
        /// <param name="owner"></param>
        public ZenSystemButton(ZenTabbedForm owner, SystemButtonType btnType)
            : base(owner)
        {
            this.btnType = btnType;
            if (btnType == SystemButtonType.Close)
                LogicalSize = ZenParams.CloseBtnLogicalSize;
            else
                LogicalSize = ZenParams.OtherSysBtnLogicalSize;
            lineWidth = (int)(2F * Scale);
        }

        /// <summary>
        /// Gets what type of button this istance is.
        /// </summary>
        public SystemButtonType BtnType
        {
            get { return btnType; }
        }

        /// <summary>
        /// Lock for anim state and value.
        /// </summary>
        private object animLO = new object();

        /// <summary>
        /// Animation value. 0: no hover. 1: fully lit under hover.
        /// </summary>
        private float animVal = 0;

        /// <summary>
        /// Animation state. 0: not animating. -1: fading out.
        /// </summary>
        private int animState = 0;

        /// <summary>
        /// Starts animation on mouse enter or leave;
        /// </summary>
        /// <param name="lightUp">True if mouse just entered (light up).</param>
        private void doAnimate(bool lightUp)
        {
            bool paintNow = false;
            lock (animLO)
            {
                // Light up is immediate
                if (lightUp)
                {
                    UnsubscribeFromTimer();
                    animVal = 1;
                    animState = 0;
                    paintNow = true;
                }
                // Fade out
                else
                {
                    animState = -1;
                    SubscribeToTimer();
                }
            }
            if (paintNow) MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Handle animation timer
        /// </summary>
        public override void DoTimer(out bool? needBackground, out RenderMode? renderMode)
        {
            needBackground = null;
            renderMode = null;
            bool needTimer = false;
            lock (animLO)
            {
                if (animVal > 1) { animVal = 1; animState = 0; }
                else if (animVal < 0) { animVal = 0; animState = 0; }
                else if (animState == -1)
                {
                    animVal -= 0.2F;
                    needTimer = true;
                    needBackground = false;
                    renderMode = RenderMode.Invalidate;
                }
            }
            if (!needTimer) UnsubscribeFromTimer();
        }

        /// <summary>
        /// Gets animation value for paint.
        /// </summary>
        private float getAnimVal()
        {
            lock (animLO)
            {
                if (animVal < 0) return 0;
                if (animVal > 1) return 1;
                return animVal;
            }
        }

        /// <summary>
        /// Mix from color A to B based on val (0-1).
        /// </summary>
        private static Color mix(Color ca, Color cb, float val)
        {
            float r = ((float)ca.R) + (((float)cb.R) - ((float)ca.R)) * val;
            float g = ((float)ca.G) + (((float)cb.G) - ((float)ca.G)) * val;
            float b = ((float)ca.B) + (((float)cb.B) - ((float)ca.B)) * val;
            float a = ((float)ca.A) + (((float)cb.A) - ((float)ca.A)) * val;
            return Color.FromArgb((int)a, (int)r, (int)g, (int)b);
        }

        private void doPaintClose(Graphics g, float val)
        {
            Color bgCol = mix(ZenParams.CloseBtnBgColorBase, ZenParams.CloseBtnBgColorHover, val);
            Color foreCol = ZenParams.CloseBtnForeColor;
            g.SmoothingMode = SmoothingMode.None;
            using (Brush b = new SolidBrush(bgCol))
            {
                g.FillRectangle(b, 0, 1, Width, Height - 1);
            }
            g.SmoothingMode = SmoothingMode.AntiAlias;
            using (Brush b = new SolidBrush(foreCol))
            using (Pen p = new Pen(b, (float)lineWidth))
            {
                p.StartCap = LineCap.Triangle;
                p.EndCap = LineCap.Triangle;
                int h = Height / 3;
                int w = Width / 4;
                int topX = (Width - w) / 2;
                int topY = (Height - h) / 2;
                g.DrawLine(p, topX, topY, topX + w, topY + h);
                g.DrawLine(p, topX, topY + h, topX + w, topY);
            }
        }

        private void doPaintMinimize(Graphics g, float val)
        {
            Color bgCol = mix(ZenParams.OtherSysBtnBgColorBase, ZenParams.OtherSysBtnBgColorHover, val);
            Color foreCol = mix(ZenParams.OtherSysBtnForeColorBase, ZenParams.OtherSysBtnForeColorHover, val);
            g.SmoothingMode = SmoothingMode.None;
            using (Brush b = new SolidBrush(bgCol))
            {
                g.FillRectangle(b, 0, 1, Width, Height - 1);
            }
            using (Brush b = new SolidBrush(foreCol))
            {
                g.FillRectangle(b,
                    Width / 4, 3 * Height / 4 - lineWidth,
                    Width / 2, lineWidth);
            }
        }

        public override void DoPaint(Graphics g)
        {
            float val = getAnimVal();
            if (btnType == SystemButtonType.Close) doPaintClose(g, val);
            else if (btnType == SystemButtonType.Minimize) doPaintMinimize(g, val);
            else throw new Exception("System button type not implemented: " + btnType.ToString());
        }

        public override void DoMouseEnter()
        {
            base.DoMouseEnter();
            doAnimate(true);
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public override void DoMouseLeave()
        {
            base.DoMouseLeave();
            doAnimate(false);
            MakeMePaint(false, RenderMode.Invalidate);
        }

        public override bool DoMouseDown(Point p, MouseButtons button)
        {
            return true;
        }

        public override bool DoMouseUp(Point p, MouseButtons button)
        {
            base.DoMouseClick(p, button);
            return true;
        }

        public override bool DoMouseClick(Point p, MouseButtons button)
        {
            return true;
        }
    }
}
