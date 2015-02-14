using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// A minimal, borderless, double-buffered window that displays a zen form's canvas instead of painting.
    /// </summary>
    internal class ZenWinForm : Form
    {
        /// <summary>
        /// Encapsulates mutex-protected access to form's canvas.
        /// </summary>
        public class CanvasToShow : IDisposable
        {
            /// <summary>
            /// The mutex protecting the bitmap.
            /// </summary>
            private readonly Mutex canvasMutex;
            /// <summary>
            /// The bitmap to draw on screen.
            /// </summary>
            public readonly Bitmap Canvas;
            /// <summary>
            /// Ctor: initialize. Acquires the mutex.
            /// </summary>
            /// <param name="canvasMutex">The mutex protecting the bitmap.</param>
            /// <param name="canvas">The bitmap to draw on screen.</param>
            public CanvasToShow(Mutex canvasMutex, Bitmap canvas)
            {
                this.canvasMutex = canvasMutex;
                Canvas = canvas;
            }
            /// <summary>
            /// Releases the mutex protecting the bitmap.
            /// </summary>
            public void Dispose()
            {
                if (Canvas != null) canvasMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Delegate for acquring access to the canvas from the Windows paint event handler.
        /// </summary>
        /// <returns></returns>
        public delegate CanvasToShow GetCanvasDelegate();
        /// <summary>
        /// Gives access to the canvas; called from the Windows paint event handler.
        /// </summary>
        private readonly GetCanvasDelegate getCanvas;
        /// <summary>
        /// The current scale (96DPI times this).
        /// </summary>
        private readonly float scale;

        public ZenWinForm(GetCanvasDelegate getCanvas)
        {
            this.getCanvas = getCanvas;

            SuspendLayout();
            KeyPreview = true;
            DoubleBuffered = false;
            FormBorderStyle = FormBorderStyle.None;
            Size = new Size(800, 300);
            AutoScaleDimensions = new SizeF(6.0F, 13.0F);
            AutoScaleMode = AutoScaleMode.Font;
            scale = CurrentAutoScaleDimensions.Height / 13.0F;
            ResumeLayout();
        }

        internal new float Scale
        {
            get { return scale; }
        }

        internal new Point MousePosition
        {
            get { return Form.MousePosition; }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // NOP!
        }

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth,
           int nHeight, IntPtr hObjSource, int nXSrc, int nYSrc, TernaryRasterOperations dwRop);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr CreateCompatibleDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteDC(IntPtr hdc);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

        [DllImport("gdi32.dll", ExactSpelling = true, SetLastError = true)]
        static extern bool DeleteObject(IntPtr hObject);

        public enum TernaryRasterOperations : uint
        {
            SRCCOPY = 0x00CC0020,
        }

        HiResTimer hrt = new HiResTimer();
        Int64 timeSpent = 0;
        Int64 eventCount = 0;

        public double AvgTime
        {
            get
            {
                return ((double)timeSpent) / ((double)eventCount);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            CanvasToShow cts = getCanvas();
            if (cts == null) return;
            try
            {
                // Blit canvas to screen

                //Int64 before = hrt.Value;
                //e.Graphics.DrawImageUnscaled(cts.Canvas, 0, 0);
                IntPtr pTarget = e.Graphics.GetHdc();
                IntPtr pSource = CreateCompatibleDC(pTarget);
                IntPtr pBitmap = cts.Canvas.GetHbitmap();
                IntPtr pOrig = SelectObject(pSource, pBitmap);
                BitBlt(pTarget, 0, 0, cts.Canvas.Width, cts.Canvas.Height, pSource, 0, 0, TernaryRasterOperations.SRCCOPY);
                DeleteObject(pOrig);
                DeleteObject(pBitmap);
                DeleteDC(pSource);
                e.Graphics.ReleaseHdc(pTarget);
                //Int64 after = hrt.Value;
                //timeSpent += (after - before);
                //++eventCount;
            }
            finally { cts.Dispose(); }
        }
    }
}
