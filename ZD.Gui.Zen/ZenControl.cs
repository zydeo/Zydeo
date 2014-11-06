using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ZD.Gui.Zen
{
    /// <summary>
    /// Base class for user-defined controls to derive from.
    /// </summary>
    public class ZenControl : ZenControlBase, IDisposable
    {
        /// <summary>
        /// See <see cref="Tooltip"/>.
        /// </summary>
        private IZenTooltip tooltip = null;

        /// <summary>
        /// Ctor: takes parent.
        /// </summary>
        /// <param name="parent">The control's parent (owner).</param>
        public ZenControl(ZenControlBase parent)
            : base(parent)
        {
        }

        /// <summary>
        /// Releases resources owned by control. Derived control MUST call base.Dispose().
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
        }

        /// <summary>
        /// <para>Gets or sets the control's tooltip info provider.</para>
        /// <para>Set this property to make control show tooltips.</para>
        /// <para>Control must call base.DoMouseEnter and base.DoMouseLeave diligently for tooltips to work.</para>
        /// </summary>
        public IZenTooltip Tooltip
        {
            get { return tooltip; }
            set { tooltip = value; RegisterControlForTooltip(this, value); }
        }

        /// <summary>
        /// Called by derived control to register a WinForms control it contains/owns.
        /// </summary>
        protected override sealed void RegisterWinFormsControl(Control c)
        {
            base.RegisterWinFormsControl(c);
        }

        /// <summary>
        /// End of add/remove chain. Implemented by <see cref="ZenTabbedForm"/>.
        /// </summary>
        internal sealed override void AddWinFormsControl(Control c)
        {
            base.AddWinFormsControl(c);
        }

        /// <summary>
        /// End of add/remove chain. Implemented by <see cref="ZenTabbedForm"/>.
        /// </summary>
        internal sealed override void RemoveWinFormsControl(Control c)
        {
            base.RemoveWinFormsControl(c);
        }

        /// <summary>
        /// Gets or sets the control's size in actual, scaled pixels.
        /// </summary>
        public sealed override Size Size
        {
            get { return base.Size; }
            set { base.Size = value; }
        }

        /// <summary>
        /// Gets or sets the control's logical size (in unscaled pixels; real size is this times <see cref="Scale"/>.
        /// </summary>
        public sealed override Size LogicalSize
        {
            get { return base.LogicalSize; }
            set { base.LogicalSize = value; }
        }

        /// <summary>
        /// Gets the control's real absolute rectangle on the window's main canvas.
        /// </summary>
        public sealed override Rectangle AbsRect
        {
            get { return base.AbsRect; }
        }

        /// <summary>
        /// Paints a dummy bounded rectangle for control. Override in derived control; do not call base.
        /// </summary>
        public override void DoPaint(Graphics g)
        {
            using (Brush b = new SolidBrush(SystemColors.Control))
            {
                g.FillRectangle(b, 0, 0, Width, Height);
            }
            using (Pen p = new Pen(Color.DarkGray))
            {
                g.DrawRectangle(p, 0, 0, Width - 1, Height - 1);
            }
            // Paint children
            DoPaintChildren(g);
        }

        /// <summary>
        /// Gets the mouse position, expressed in the control's coordinates.
        /// </summary>
        protected Point MousePosition
        {
            get
            {
                Point pAbs = MousePositionAbs;
                Point pRel = new Point(pAbs.X - AbsLeft, pAbs.Y - AbsTop);
                return pRel;
            }
        }

        /// <summary>
        /// Gets or sets the form's cursor.
        /// </summary>
        public override sealed Cursor Cursor
        {
            get { return Parent.Cursor; }
            set { if (Parent != null) Parent.Cursor = value; }
        }
    }
}
