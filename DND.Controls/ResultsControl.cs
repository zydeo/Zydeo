using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DND.Common;

namespace DND.Controls
{
    public partial class ResultsControl : Control
    {
        private ReadOnlyCollection<CedictResult> results;
        private int pageSize;

        public ResultsControl()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ...
            }
            base.Dispose(disposing);
        }

        public void SetResults(ReadOnlyCollection<CedictResult> results, int pageSize)
        {
            this.results = results;
            this.pageSize = pageSize;
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            // NOP
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;
            // Background
            using (Brush b = new SolidBrush(Color.White))
            {
                g.FillRectangle(b, ClientRectangle);
            }
            // Border
            using (Pen p = new Pen(SystemColors.ControlDarkDark))
            {
                g.DrawLine(p, 0, 0, Width, 0);
                g.DrawLine(p, Width - 1, 0, Width - 1, Height);
                g.DrawLine(p, Width - 1, Height - 1, 0, Height - 1);
                g.DrawLine(p, 0, Height - 1, 0, 0);
            }
        }
    }
}
