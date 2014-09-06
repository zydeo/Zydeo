using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using DND.Common;
using DND.Gui.Zen;

namespace DND.Gui
{
    public class CharPicker : ZenControl
    {
        /// <summary>
        /// Represents one character's rectangle in the control.
        /// </summary>
        private class CharRect
        {
            /// <summary>
            /// Rectangle on my canvas.
            /// </summary>
            public RectangleF Rect;
            /// <summary>
            /// Animation state.
            /// 0: no animation
            /// 1: highlight building
            /// 2: highlight fading
            /// </summary>
            public int AnimState = 0;
            /// <summary>
            /// Highlight intensity at the moment.
            /// 0: no highlight
            /// 1: fully highlighted
            /// </summary>
            public float AnimVal = 0;
            /// <summary>
            /// Get BG color depending on current animation state.
            /// </summary>
            public Color BgColor
            {
                get
                {
                    float alfaMax = 64F;
                    byte alfa;
                    if (AnimState == 0) alfa = AnimVal == 0 ? (byte)0 : (byte)alfaMax;
                    else
                    {
                        // We don't want linear ease-in and ease-out
                        // Cubic's chique.
                        float ts = AnimVal * AnimVal;
                        float tc = ts * AnimVal;
                        float cubeVal = -2.0F * tc + 3.0F * ts;
                        alfa = (byte)(alfaMax * cubeVal);
                    }
                    return Color.FromArgb(alfa,
                        ZenParams.BtnGradDarkColorHover.R,
                        ZenParams.BtnGradDarkColorHover.G,
                        ZenParams.BtnGradDarkColorHover.B);
                }
            }
        }

        /// <summary>
        /// Delegate to handle "character picked" event.
        /// </summary>
        /// <param name="c"></param>
        public delegate void CharPickedDelegate(char c);
        /// <summary>
        /// Fired when user picks a character by clicking it.
        /// </summary>
        public event CharPickedDelegate CharPicked;

        /// <summary>
        /// The characters shown.
        /// </summary>
        private char[] items = new char[0];

        /// <summary>
        /// Font face to draw characters.
        /// </summary>
        private string fontFace = ZenParams.ZhoFontFamily;

        /// <summary>
        /// Lock object to access character rectangles.
        /// </summary>
        private readonly object animLO = new object();

        /// <summary>
        /// Visible character rectangles.
        /// </summary>
        private readonly List<CharRect> charRects;

        /// <summary>
        /// Character size - calculate by analyzing font and control's size.
        /// </summary>
        private SizeF charSize;

        /// <summary>
        /// X character position within each rectangle.
        /// </summary>
        float charOfsX;

        /// <summary>
        /// Y character position within each rectangle.
        /// </summary>
        float charOfsY;

        /// <summary>
        /// Actual font for drawing characters.
        /// </summary>
        private Font font;

        /// <summary>
        /// Ctor: take parent.
        /// </summary>
        public CharPicker(ZenControl owner)
            : base(owner)
        {
            charRects = new List<CharRect>();
            for (int i = 0; i != 10; ++i) charRects.Add(new CharRect());
        }

        /// <summary>
        /// Gets or sets the font face for drawing characters.
        /// </summary>
        public string FontFace
        {
            get { return fontFace; }
            set { fontFace = value; calibrateFont(); }
        }

        /// <summary>
        /// Dispose: free owned resources.
        /// </summary>
        public override void Dispose()
        {
            if (font != null) font.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Sets the characters to shown in this control.
        /// </summary>
        public void SetItems(char[] items)
        {
            if (items == null) items = new char[0];
            List<char> filteredItems = new List<char>();
            foreach (char c in items)
            {
                if (char.IsLetter(c)) filteredItems.Add(c);
                if (filteredItems.Count == 10) break;
            }
            this.items = filteredItems.ToArray();
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Recalibrates display when size changes.
        /// </summary>
        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            // Function below triggers repaint!
            calibrateFont();
        }

        /// <summary>
        /// <para>Finds the right font size to fit characters (2x5 with default size, but it varies).</para>
        /// <para>Finds right vertical area based on font's actual display properties.</para>
        /// </summary>
        private void calibrateFont()
        {
            float width = Width;
            float fontSize = 10.0F;

            // Measuring artefacts
            using (Bitmap bmp = new Bitmap(1, 1))
            using (Graphics g = Graphics.FromImage(bmp))
            {
                StringFormat sf = StringFormat.GenericTypographic;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                // Keep growing font until we reach a comfortable width
                while (true)
                {
                    using (Font fnt = new Font(fontFace, fontSize))
                    {
                        charSize = g.MeasureString("中", fnt, 65535, sf);
                    }
                    if (charSize.Width * 5.0F >= width * 0.8F) break;
                    fontSize += 0.5F;
                }
                if (font != null) font.Dispose(); font = null;
                font = new Font(fontFace, fontSize);
            }

            // Width of rectangle: using my space equally
            float rectWidth = (width - 2.0F) / 5.0F;
            // Height of rectange: depends on font's actual drawing behavior!
            var si = HanziMeasure.Instance.GetMeasures(fontFace, fontSize);
            float rectHeight = si.RealRect.Bottom + si.RealRect.Top;
            // Horizontal padding is rectangle width minus measured char width, over two
            float hPad = (rectWidth - si.AllegedSize.Width) / 2.0F;
            // Add twice horizontal padding to rectangle height; offset chars from top by padding
            rectHeight += 3.0F * hPad;

            lock (animLO)
            {
                for (int i = 0; i != 5; ++i)
                {
                    float x = ((float)i) * rectWidth + 1.0F;
                    RectangleF rtop = new RectangleF(x, 1.0F, rectWidth, rectHeight);
                    RectangleF rbot = new RectangleF(x, rectHeight + 1.0F, rectWidth, rectHeight);
                    charRects[i].Rect = rtop;
                    charRects[i + 5].Rect = rbot;
                }
            }
            charOfsX = (rectWidth - charSize.Width) / 2.0F;
            charOfsY = 1.5F * hPad;
            Height = (int)Math.Round((rectHeight) * 2.0F + 0.5F);
            MakeMePaint(true, RenderMode.Invalidate);
        }

        /// <summary>
        /// Gets the index of the rectangle under the provided point.
        /// </summary>
        private int getCharRectIx(Point p)
        {
            lock (animLO)
            {
                int ix = -1;
                for (int i = 0; i != charRects.Count; ++i)
                {
                    if (charRects[i].Rect.Contains(p)) { ix = i; break; }
                }
                return ix;
            }
        }

        /// <summary>
        /// Handles mouse move: highlight hovered-over rectangle.
        /// </summary>
        public override bool DoMouseMove(Point p, System.Windows.Forms.MouseButtons button)
        {
            int ix = getCharRectIx(p);
            doAnimate(ix);
            return true;
        }

        /// <summary>
        /// Handles mouse leave: turns off hover for any rectangle that may have it.
        /// </summary>
        public override void DoMouseLeave()
        {
            doAnimate(-1);
        }

        /// <summary>
        /// Moves on a single character rectangle's animation.
        /// </summary>
        private bool doAnimateRect(CharRect cr)
        {
            if (cr.AnimState == 0) return false;
            // Lighting up
            if (cr.AnimState == 1)
            {
                cr.AnimVal += 0.125F;
                if (cr.AnimVal > 1) { cr.AnimVal = 1; cr.AnimState = 0; }
            }
            // Cooling down
            else
            {
                cr.AnimVal -= 0.125F;
                if (cr.AnimVal < 0) { cr.AnimVal = 0; cr.AnimState = 0; }
            }
            return true;
        }

        /// <summary>
        /// Handles timer even for animations.
        /// </summary>
        public override void DoTimer()
        {
            lock (animLO)
            {
                bool timerNeeded = false;
                for (int i = 0; i != charRects.Count; ++i)
                {
                    timerNeeded |= doAnimateRect(charRects[i]);
                }
                if (!timerNeeded) UnsubscribeFromTimer();
            }
            MakeMePaint(false, RenderMode.Invalidate);
        }

        /// <summary>
        /// Launches/changes animation to show hover background.
        /// </summary>
        /// <param name="ix">Index of rectangle with hover, or -1.</param>
        private void doAnimate(int ix)
        {
            bool needTimer = false;
            if (ix >= items.Length) ix = -1;
            lock (animLO)
            {
                for (int i = 0; i != charRects.Count; ++i)
                {
                    CharRect cr = charRects[i];
                    // This is current hover
                    if (i == ix)
                    {
                        // Rectangle is not fully highlighted: start animation
                        if (cr.AnimVal != 1) cr.AnimState = 1;
                    }
                    // Rectangle is not hovered over. Need to turn off?
                    else
                    {
                        if (cr.AnimVal != 0) cr.AnimState = -1;
                    }
                    // Timer is needed if anything is animating.
                    needTimer |= cr.AnimState != 0;
                }
            }
            if (needTimer) SubscribeToTimer();
        }

        /// <summary>
        /// Copies character rectangles out of their container in a thread-safe way.
        /// </summary>
        /// <returns></returns>
        private CharRect[] getCharRects()
        {
            CharRect[] rects = new CharRect[charRects.Count];
            lock (animLO)
            {
                for (int i = 0; i != rects.Length; ++i)
                {
                    CharRect cr = charRects[i];
                    rects[i] = new CharRect
                    {
                        Rect = cr.Rect,
                        AnimState = cr.AnimState,
                        AnimVal = cr.AnimVal
                    };
                }
            }
            return rects;
        }

        /// <summary>
        /// Paints the control.
        /// </summary>
        public override void DoPaint(System.Drawing.Graphics g)
        {
            // Get character rectangles
            CharRect[] rects = getCharRects();

            // Background
            using (Brush b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }
            // Characters
            StringFormat sf = StringFormat.GenericTypographic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            using (Brush b = new SolidBrush(Color.Black))
            {
                for (int i = 0; i != rects.Length; ++i)
                {
                    RectangleF rect = rects[i].Rect;
                    // Background
                    using (Brush bgb = new SolidBrush(rects[i].BgColor))
                    {
                        g.FillRectangle(bgb, rect);
                    }
                    // Draw character, if any
                    if (i >= items.Length) continue;
                    string str = ""; str += items[i];
                    g.DrawString(str, font, b, rect.X + charOfsX, rect.Y + charOfsY, sf);
                }
            }
            // Border
            using (Pen p = new Pen(ZenParams.BorderColor))
            {
                g.DrawLine(p, 0, 0, Width, 0);
                g.DrawLine(p, Width - 1, 0, Width - 1, Height);
                g.DrawLine(p, Width - 1, Height - 1, 0, Height - 1);
                g.DrawLine(p, 0, Height - 1, 0, 0);
            }
        }

        /// <summary>
        /// Handles mouse click: fires the "character picked" event if applicable.
        /// </summary>
        public override bool DoMouseClick(Point p, System.Windows.Forms.MouseButtons button)
        {
            doAnimate(-1);

            int ix = getCharRectIx(p);
            if (ix == -1) return true;
            if (ix >= items.Length) return true;
            if (CharPicked != null) CharPicked(items[ix]);
            return true;
        }
    }
}
