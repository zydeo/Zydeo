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
        /// Scale - remember to avoid probs in time when we've alread been removed from parent.
        /// </summary>
        private readonly float scale;

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
        private ZenGradientButton btnUpdate;

        /// <summary>
        /// Width of update button - known from thestart, never changes.
        /// </summary>
        private readonly int btnUpdateWidth;

        /// <summary>
        /// Relative location of update button. Known even before button is ever created.
        /// </summary>
        private Point btnUpdateRelLoc;

        /// <summary>
        /// Display rectangle of release notes > hot area.
        /// </summary>
        private Rectangle rectNotesLink;

        // -- Disposables: fonts loaded only once
        private Font fntTitle;
        private Font fntNorm;
        private Font fntTblHead;
        private Font fntTblValues;
        // -----------------------------------------

        /// <summary>
        /// Ctor: take info about update.
        /// </summary>
        public WhiteUpdateControl(ZenControlBase owner, ITextProvider tprov,
            int vmaj, int vmin, DateTime rdate, string rnotes, UpdateNowDelegate updateNow)
            : base(owner)
        {
            this.tprov = tprov;
            scale = Scale;
            this.updateNow = updateNow;

            // Fonts. !! Dispose 'em.
            fntTitle = FontCollection.CreateFont(Magic.WhiteUpdFntTitle, Magic.WhiteUpFntTitleSz, FontStyle.Regular);
            fntNorm = FontCollection.CreateFont(Magic.WhiteUpdFntNorm, Magic.WhiteUpdFntNormSz, FontStyle.Regular);
            fntTblHead = FontCollection.CreateFont(Magic.WhiteUpdFntNorm, Magic.WhiteUpdFntNormSz, FontStyle.Bold);
            fntTblValues = FontCollection.CreateFont(Magic.WhiteUpdFntNorm, 10F, FontStyle.Italic);

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

            // Width update button is fixed
            btnUpdateWidth = (int)(scale * 200F);
        }

        /// <summary>
        /// Dispose our own cached resources (fonts).
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            fntTitle.Dispose();
            fntNorm.Dispose();
            fntTblHead.Dispose();
            fntTblValues.Dispose();
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
            if (rectNotesLink.Contains(p)) Cursor = CustomCursor.GetHand(scale);
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
        /// Lock object around <see cref="animVal"/>.
        /// </summary>
        private readonly object animLO = new object();

        /// <summary>
        /// <para>Current animation value.</para>
        /// <para>0: Nothing visible; no animation.</para>
        /// <para>0.0 - 1.0: Title fading in.</para>
        /// <para>0.5 - 1.5: Body fading in.</para>
        /// <para>1.0 - 2.0: Table fading in.</para>
        /// <para>>= 2.0: All done, button shown too.</para>
        /// </summary>
        private float animVal = 0;

        /// <summary>
        /// Kicks off animation if it never started yet.
        /// </summary>
        private void doKickoffAnimation()
        {
            lock (animLO)
            {
                if (animVal == 0)
                {
                    animVal = 0.001F;
                    SubscribeToTimer();
                }
            }
        }

        /// <summary>
        /// Creates "update now" button when it's time to show it in fade-in animation.
        /// </summary>
        private void doCreateBtnUpdate()
        {
            btnUpdate = new ZenGradientButton(this);
            btnUpdate.Height = (int)(scale * 24F);
            btnUpdate.Width = btnUpdateWidth;
            btnUpdate.RelLocation = btnUpdateRelLoc;
            btnUpdate.SetFont(ZenParams.GenericFontFamily, 10F);
            btnUpdate.Text = tprov.GetString("WhiteUpdateButton");
            btnUpdate.MouseClick += onBtnUpdateClick;
        }

        /// <summary>
        /// Handles timer event for fade-in animation.
        /// </summary>
        public override void DoTimer(out bool? needBackground, out RenderMode? renderMode)
        {
            lock (animLO)
            {
                // Past maximum? Make sure button is shown
                if (animVal > 1.5F)
                {
                    if (btnUpdate == null) doCreateBtnUpdate();
                }
                // When full done, unsubscribe from timer.
                if (animVal > 2F) UnsubscribeFromTimer();
                // Just nudge counter on.
                animVal += 0.03F;
                // No BG needed; lazy render.
                needBackground = false;
                renderMode = RenderMode.Invalidate;
            }
        }

        /// <summary>
        /// Gets current colors to use in painting (anim state dependent).
        /// </summary>
        private void getColors(out Color clrTitle, out Color clrBody, out Color clrDetailsLink,
            out Color clrDetailsText, out Color clrDetailsSep)
        {
            int alfaTitle;
            int alfaBody;
            int alfaTable;
            lock (animLO)
            {
                if (animVal < 1F) alfaTitle = (int)(255F * animVal * animVal);
                else alfaTitle = 255;
                if (animVal < 0.5F) alfaBody = 0;
                else if (animVal < 1.5F) alfaBody = (int)(255F * (animVal - 0.5F) * (animVal - 0.5F));
                else alfaBody = 255;
                if (animVal < 1F) alfaTable = 0;
                else if (animVal < 2F) alfaTable = (int)(255F * (animVal - 1F) * (animVal - 1F));
                else alfaTable = 255;
            }

            clrTitle = Color.FromArgb(alfaTitle, Magic.WhiteUpdClrTitle);
            clrBody = Color.FromArgb(alfaBody, Magic.WhiteUpdClrBody);
            clrDetailsLink = Color.FromArgb(alfaTable, Magic.WhiteUpdClrLink);
            clrDetailsText = Color.FromArgb(alfaTable, Magic.WhiteUpdClrBody);
            clrDetailsSep = Color.FromArgb(alfaTable, Magic.WhiteUpdClrDetailsSep);
        }

        /// <summary>
        /// Paints control.
        /// </summary>
        public override void DoPaint(Graphics g)
        {
            // Animation kicks in the first time control is painted.
            doKickoffAnimation();

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

            // Get colors as of now (animation)
            Color clrTitle;
            Color clrBody;
            Color clrDetailsText;
            Color clrDetailsSep;
            Color clrDetailsLink;
            getColors(out clrTitle, out clrBody, out clrDetailsLink, out clrDetailsText, out clrDetailsSep);

            // Draw stuff
            float y = 20F * scale;
            float padLR = 20F * scale;
            // Title
            using (Brush b = new SolidBrush(clrTitle))
            {
                // Neution is not good with "ClearTypeGridFit"
                g.TextRenderingHint = TextRenderingHint.AntiAlias;
                StringFormat sf = StringFormat.GenericDefault;
                sf.Alignment = StringAlignment.Center;
                float h = g.MeasureString(strTitle, fntTitle, 65535, sf).Height;
                RectangleF rect = new RectangleF(padLR, y, Width - padLR * 2, h);
                g.DrawString(strTitle, fntTitle, b, rect, sf);
                y += h + 10F * scale;
            }
            // Message body
            using (Brush b = new SolidBrush(clrBody))
            {
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                StringFormat sf = StringFormat.GenericDefault;
                sf.Alignment = StringAlignment.Center;
                float w = Width - padLR * 2;
                w /= scale;
                if (w > 500F) w = 500F;
                w *= scale;
                float h = g.MeasureString(strBody, fntNorm, (int)w, sf).Height;
                RectangleF rect = new RectangleF((Width - w) / 2F, y, w, h);
                g.DrawString(strBody, fntNorm, b, rect, sf);
                y += h + 10F * scale;
            }
            y += 10F;
            // Update details (the table)
            using (Brush bText = new SolidBrush(clrDetailsText))
            using (Brush bSep = new SolidBrush(clrDetailsSep))
            using (Brush bLink = new SolidBrush(clrDetailsLink))
            {
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                StringFormat sf = StringFormat.GenericDefault;
                // Measure all texts
                SizeF szTblHead = g.MeasureString(strTblHead, fntTblHead, 65535, sf);
                SizeF szTblVersion = g.MeasureString(strTblVersion, fntNorm, 65535, sf);
                SizeF szTblVersionVal = g.MeasureString(strVersionVal, fntTblValues, 65535, sf);
                SizeF szTblDate = g.MeasureString(strTblDate, fntNorm, 65535, sf);
                SizeF szTblNotes = g.MeasureString(strTblNotes, fntNorm, 65535, sf);
                SizeF szTblDateVal = g.MeasureString(strDateVal, fntTblValues, 65535, sf);
                SizeF szTblNotesVal = g.MeasureString(strTblNotesVal, fntTblValues, 65535, sf);
                // Vertical separator rectangle
                float halfSepW = (int)(2F * scale);
                float mid = (int)(Width / 2F);
                RectangleF rSep = new RectangleF(mid - halfSepW, y, 2 * halfSepW, 4 * szTblHead.Height);
                g.FillRectangle(bSep, rSep);
                // Draw texts: table "header"
                RectangleF rTblHead = new RectangleF(mid - 3 * halfSepW - szTblHead.Width, y, szTblHead.Width, szTblHead.Height);
                g.DrawString(strTblHead, fntTblHead, bText, rTblHead, sf);
                y += szTblHead.Height;
                // Draw texts: version
                RectangleF rTblVersion = new RectangleF(mid - 3 * halfSepW - szTblVersion.Width, y, szTblVersion.Width, szTblVersion.Height);
                g.DrawString(strTblVersion, fntNorm, bText, rTblVersion, sf);
                RectangleF rTblVersionVal = new RectangleF(mid + 2 * halfSepW, y, szTblVersionVal.Width, szTblVersionVal.Height);
                g.DrawString(strVersionVal, fntTblValues, bText, rTblVersionVal, sf);
                y += szTblVersion.Height;
                // Draw texts: Date
                RectangleF rTblDate = new RectangleF(mid - 3 * halfSepW - szTblDate.Width, y, szTblDate.Width, szTblDate.Height);
                g.DrawString(strTblDate, fntNorm, bText, rTblDate, sf);
                RectangleF rTblDateVal = new RectangleF(mid + 2 * halfSepW, y, szTblDateVal.Width, szTblDateVal.Height);
                g.DrawString(strDateVal, fntTblValues, bText, rTblDateVal, sf);
                y += szTblDate.Height;
                // Draw texts: Release notes
                RectangleF rTblNotes = new RectangleF(mid - 3 * halfSepW - szTblNotes.Width, y, szTblNotes.Width, szTblNotes.Height);
                g.DrawString(strTblNotes, fntNorm, bText, rTblNotes, sf);
                RectangleF rTblNotesVal = new RectangleF(mid + 2 * halfSepW, y, szTblNotesVal.Width, szTblNotesVal.Height);
                g.DrawString(strTblNotesVal, fntTblValues, bLink, rTblNotesVal, sf);
                rectNotesLink = Rectangle.Round(rTblNotesVal);
                y += szTblNotes.Height;
            }
            y += 30F * scale;

            // Position the button
            btnUpdateRelLoc = new Point((Width - btnUpdateWidth) / 2 , (int)y);
            if (btnUpdate != null) btnUpdate.RelLocation = btnUpdateRelLoc;
            // Well, the button, if anything.
            DoPaintChildren(g);
        }
    }
}
