using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Text;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Reflection;
using System.Globalization;

using ZD.Gui.Zen;
using ZD.Common;

namespace ZD.Gui
{
    /// <summary>
    /// Type of callback when user chooses to close Zydeo and update.
    /// </summary>
    public delegate void UpdateNowDelegate();

    /// <summary>
    /// Control shown in white notification area at startup to notify of updates.
    /// </summary>
    internal class WhiteUpdateControl : ZenControl
    {
        /// <summary>
        /// Localized strings source.
        /// </summary>
        private readonly ITextProvider tprov;

        /// <summary>
        /// Delegate to call when user clicks "update now".
        /// </summary>
        private readonly UpdateNowDelegate updateNow;

        // -- Display strings, generated in ctor
        private readonly string strVersionVal;
        private readonly string strDateVal;
        private readonly string urlRelNotes;
        private readonly string strTitle;
        private readonly string strBody;
        private readonly string strTblHead;
        private readonly string strTblVersion;
        private readonly string strTblDate;
        private readonly string strTblNotes;
        private readonly string strTblNotesVal;
        // ----------------------------------------

        /// <summary>
        /// The button to trigger the update.
        /// </summary>
        private readonly ZenGradientButton btnUpdate;

        /// <summary>
        /// Display rectangle of release notes > hot area.
        /// </summary>
        private Rectangle rectNotesLink;

        /// <summary>
        /// Ctor: take info about update.
        /// </summary>
        public WhiteUpdateControl(ZenControlBase owner, ITextProvider tprov,
            int vmaj, int vmin, DateTime rdate, string rnotes, UpdateNowDelegate updateNow)
            : base(owner)
        {
            this.tprov = tprov;
            this.updateNow = updateNow;

            // Construct UI strings
            Version ver = Assembly.GetExecutingAssembly().GetName().Version;
            strVersionVal = tprov.GetString("WhiteUpdateTblVersionVal");
            strVersionVal = string.Format(strVersionVal, ver.Major + "." + ver.Minor, vmaj + "." + vmin);
            string longDateFormat = getLongDateFormat();
            strDateVal = rdate.ToString(longDateFormat);

            urlRelNotes = rnotes;
            strTitle = tprov.GetString("WhiteUpdateTitle");
            strBody = tprov.GetString("WhiteUpdateBody");
            strTblHead = tprov.GetString("WhiteUpdateTblHeader");
            strTblVersion = tprov.GetString("WhiteUpdateTblVersion");
            strTblDate = tprov.GetString("WhiteUpdateTblDate");
            strTblNotes = tprov.GetString("WhiteUpdateTblNotes");
            strTblNotesVal = tprov.GetString("WhiteUpdateTblNotesVal");

            // Button
            btnUpdate = new ZenGradientButton(this);
            btnUpdate.Height = (int)(Scale * 24F);
            btnUpdate.Width = (int)(Scale * 200F);
            btnUpdate.SetFont(ZenParams.GenericFontFamily, 10F);
            btnUpdate.Text = tprov.GetString("WhiteUpdateButton");
            btnUpdate.MouseClick += onBtnUpdateClick;
        }

        /// <summary>
        /// Finds long date format that does not include day of week.
        /// </summary>
        private static string getLongDateFormat()
        {
            var cultureInfo = CultureInfo.CurrentCulture;
            string res = null;
            foreach (var pattern in cultureInfo.DateTimeFormat.GetAllDateTimePatterns('D'))
            {
                if (!pattern.Contains("ddd"))
                {
                    res = pattern;
                    break;
                }
            }
            bool isFallbackRequired = string.IsNullOrEmpty(res);
            if (isFallbackRequired) res = cultureInfo.DateTimeFormat.ShortDatePattern;
            return res;
        }

        /// <summary>
        /// Handles click on "update now" button - triggers update.
        /// </summary>
        /// <param name="sender"></param>
        private void onBtnUpdateClick(ZenControlBase sender)
        {
            updateNow();
        }

        /// <summary>
        /// Handles mouse move: switch to hand cursor if hovering over link area.
        /// </summary>
        public override bool DoMouseMove(Point p, MouseButtons button)
        {
            if (rectNotesLink.Contains(p)) Cursor = CustomCursor.GetHand(Scale);
            else Cursor = Cursors.Arrow;
            return base.DoMouseMove(p, button);
        }

        /// <summary>
        /// Handles mouse click to show release notes.
        /// </summary>
        public override bool DoMouseClick(Point p, MouseButtons button)
        {
            if (rectNotesLink.Contains(p))
            {
                try { System.Diagnostics.Process.Start(urlRelNotes); }
                catch
                {
                    // Swallow it all. Worst case, we don't open link - so what.
                    // Not worth a crash or messages.
                }
            }
            return true;
        }

        /// <summary>
        /// Paints control.
        /// </summary>
        public override void DoPaint(Graphics g)
        {
            // Background
            using (Brush b = new SolidBrush(ZenParams.WindowColor))
            {
                g.FillRectangle(b, new Rectangle(0, 0, Width, Height));
            }
            // Border
            using (Pen p = new Pen(ZenParams.BorderColor))
            {
                g.DrawLine(p, 0, 0, Width, 0);
                g.DrawLine(p, Width - 1, 0, Width - 1, Height);
                g.DrawLine(p, Width - 1, Height - 1, 0, Height - 1);
                g.DrawLine(p, 0, Height - 1, 0, 0);
            }
            float y = 20F * Scale;
            float padLR = 20F * Scale;
            // Title
            // TO-DO: font from private collection; magic numbers
            Color clrTitle = Color.FromArgb(0xa0, 0x74, 0x35, 0x00);
            using (Font fnt = new Font("Neuton", 24F))
            using (Brush b = new SolidBrush(clrTitle))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                StringFormat sf = StringFormat.GenericDefault;
                sf.Alignment = StringAlignment.Center;
                float h = g.MeasureString(strTitle, fnt, 65535, sf).Height;
                RectangleF rect = new RectangleF(padLR, y, Width - padLR * 2, h);
                g.DrawString(strTitle, fnt, b, rect, sf);
                y += h + 10F * Scale;
            }
            // Message body
            Color clrBody = Color.FromArgb(0xff, 0x26, 0x26, 0x26);
            using (Font fnt = new Font("Segoe UI", 10F))
            using (Brush b = new SolidBrush(clrBody))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                StringFormat sf = StringFormat.GenericDefault;
                sf.Alignment = StringAlignment.Center;
                float w = Width - padLR * 2;
                w /= Scale;
                if (w > 500F) w = 500F;
                w *= Scale;
                float h = g.MeasureString(strBody, fnt, (int)w, sf).Height;
                RectangleF rect = new RectangleF((Width - w) / 2F, y, w, h);
                g.DrawString(strBody, fnt, b, rect, sf);
                y += h + 10F * Scale;
            }
            y += 10F;
            // Update details (the table)
            Color clrDetailsText = Color.FromArgb(0xff, 0x26, 0x26, 0x26);
            Color clrDetailsSep = Color.FromArgb(0xff, 0xce, 0xce, 0xce);
            Color clrDetailsLink = Color.FromArgb(0xff, 0, 0, 192);
            using (Font fntHead = new Font("Segoe UI", 10F, FontStyle.Bold))
            using (Font fntLabels = new Font("Segoe UI", 10F))
            using (Font fntVals = new Font("Segoe UI", 10F, FontStyle.Italic))
            using (Brush bText = new SolidBrush(clrBody))
            using (Brush bSep = new SolidBrush(clrDetailsSep))
            using (Brush bLink = new SolidBrush(clrDetailsLink))
            {
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                StringFormat sf = StringFormat.GenericDefault;
                // Measure all texts
                SizeF szTblHead = g.MeasureString(strTblHead, fntHead, 65535, sf);
                SizeF szTblVersion = g.MeasureString(strTblVersion, fntLabels, 65535, sf);
                SizeF szTblVersionVal = g.MeasureString(strVersionVal, fntVals, 65535, sf);
                SizeF szTblDate = g.MeasureString(strTblDate, fntLabels, 65535, sf);
                SizeF szTblNotes = g.MeasureString(strTblNotes, fntLabels, 65535, sf);
                SizeF szTblDateVal = g.MeasureString(strDateVal, fntVals, 65535, sf);
                SizeF szTblNotesVal = g.MeasureString(strTblNotesVal, fntVals, 65535, sf);
                // Vertical separator rectangle
                float halfSepW = (int)(2F * Scale);
                float mid = (int)(Width / 2F);
                RectangleF rSep = new RectangleF(mid - halfSepW, y, 2 * halfSepW, 4 * szTblHead.Height);
                g.FillRectangle(bSep, rSep);
                // Draw texts: table "header"
                RectangleF rTblHead = new RectangleF(mid - 3 * halfSepW - szTblHead.Width, y, szTblHead.Width, szTblHead.Height);
                g.DrawString(strTblHead, fntHead, bText, rTblHead, sf);
                y += szTblHead.Height;
                // Draw texts: version
                RectangleF rTblVersion = new RectangleF(mid - 3 * halfSepW - szTblVersion.Width, y, szTblVersion.Width, szTblVersion.Height);
                g.DrawString(strTblVersion, fntLabels, bText, rTblVersion, sf);
                RectangleF rTblVersionVal = new RectangleF(mid + 2 * halfSepW, y, szTblVersionVal.Width, szTblVersionVal.Height);
                g.DrawString(strVersionVal, fntVals, bText, rTblVersionVal, sf);
                y += szTblVersion.Height;
                // Draw texts: Date
                RectangleF rTblDate = new RectangleF(mid - 3 * halfSepW - szTblDate.Width, y, szTblDate.Width, szTblDate.Height);
                g.DrawString(strTblDate, fntLabels, bText, rTblDate, sf);
                RectangleF rTblDateVal = new RectangleF(mid + 2 * halfSepW, y, szTblDateVal.Width, szTblDateVal.Height);
                g.DrawString(strDateVal, fntVals, bText, rTblDateVal, sf);
                y += szTblDate.Height;
                // Draw texts: Release notes
                RectangleF rTblNotes = new RectangleF(mid - 3 * halfSepW - szTblNotes.Width, y, szTblNotes.Width, szTblNotes.Height);
                g.DrawString(strTblNotes, fntLabels, bText, rTblNotes, sf);
                RectangleF rTblNotesVal = new RectangleF(mid + 2 * halfSepW, y, szTblNotesVal.Width, szTblNotesVal.Height);
                g.DrawString(strTblNotesVal, fntVals, bLink, rTblNotesVal, sf);
                rectNotesLink = Rectangle.Round(rTblNotesVal);
                y += szTblNotes.Height;
            }
            // Position the button
            y += 30F * Scale;
            btnUpdate.RelLocation = new Point((Width - btnUpdate.Width) / 2 , (int)y);
            // Well, the button.
            DoPaintChildren(g);
        }
    }
}
