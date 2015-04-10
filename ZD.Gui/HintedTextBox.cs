using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;

using ZD.Gui.Zen;

namespace ZD.Gui
{
    /// <summary>
    /// Represents a Windows text box control, with an opaque textual hint when it is empty.
    /// </summary>
    internal class HintedTextBox : TextBox
    {
        /// <summary>
        /// See <see cref="HintText"/>.
        /// </summary>
        private string hintText = string.Empty;

        /// <summary>
        /// The textual hint to display when the text box contains no actual text.
        /// </summary>
        public string HintText
        {
            get { return hintText; }
            set { if (value == null) hintText = string.Empty; else hintText = value; }
        }

        /// <summary>
        /// True if IME is composing (active).
        /// </summary>
        private bool imeActive = false;

        /// <summary>
        /// Ctor: inits colors to match Zen parameters.
        /// </summary>
        public HintedTextBox()
        {
            BackColor = ZenParams.WindowColor;
            ForeColor = ZenParams.StandardTextColor;
        }

        /// <summary>
        /// Windows paint event code.
        /// </summary>
        private const int WM_PAINT = 0x0f;

        /// <summary>
        /// IME composition started event.
        /// </summary>
        private const int WM_IME_STARTCOMPOSITION = 0x10d;

        /// <summary>
        /// IME composition ended event.
        /// </summary>
        private const int WM_IME_ENDCOMPOSITION = 0x10e;

        /// <summary>
        /// Intercepts (at least some) Paint events to display hint.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_IME_STARTCOMPOSITION || m.Msg == WM_IME_ENDCOMPOSITION)
            {
                imeActive = m.Msg == WM_IME_STARTCOMPOSITION;
                Invalidate();
            }
            base.WndProc(ref m);
            if (m.Msg == WM_PAINT) doPaintOver();
        }

        /// <summary>
        /// Shows or hides hint when user text changes.
        /// </summary>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            imeActive = false;
            doPaintOver();
        }

        /// <summary>
        /// Paints hint over control's area.s
        /// </summary>
        private void doPaintOver()
        {
            if (Text != string.Empty || hintText == string.Empty || imeActive) return;
            using (Graphics g = CreateGraphics())
            {
                using (Font f = new Font(this.Font, FontStyle.Italic))
                using (Brush b = new SolidBrush(Color.FromArgb(Magic.SearchInputHintOpacity, this.ForeColor)))
                {
                    // Vertical offset for Noto. Ugly but not my fault the whole thing. Stupid fonts.
                    float top = 0;
                    if (f.Name.StartsWith("Noto"))
                    {
                        float scale = this.FindForm().CurrentAutoScaleDimensions.Height / 13.0F;
                        top = scale * 4F;
                    }
                    g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                    g.DrawString(hintText, f, b, new PointF(0, top));
                }
            }
        }
    }
}
