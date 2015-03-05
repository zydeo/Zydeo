using System;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Text;

namespace ZD.Gui.Zen
{
    public class AntiLabel : Label
    {
        protected override void OnPaint(PaintEventArgs pe)
        {
            pe.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            base.OnPaint(pe);
        }
    }
}
