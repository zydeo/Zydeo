using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using ZD.Common;
using ZD.Gui.Zen;

namespace ZD.Gui
{
    /// <summary>
    /// Context menu UI for "copy" commands over a single result control.
    /// </summary>
    public partial class ResultsCtxtControl : UserControl, ICtxtMenuControl
    {
        /// <summary>
        /// Delegate to handle my notification when a command is chosen.
        /// </summary>
        /// <param name="sender">The context menu where the command originates from.</param>
        public delegate void CommandTriggeredDelegate(ResultsCtxtControl sender);

        /// <summary>
        /// The delegate I'm calling when user has issued a command.
        /// </summary>
        private readonly CommandTriggeredDelegate cmdTriggered;

        /// <summary>
        /// The dictionary entry for which I'm shown.
        /// </summary>
        private readonly CedictEntry entry;

        /// <summary>
        /// The search script, to know meaning of <see cref="lblHanzi1"/> when only one Hanzi command is shown.
        /// </summary>
        private readonly SearchScript script;

        /// <summary>
        /// Labels that are part of the hover game.
        /// </summary>
        private readonly Label[] lblColl;

        /// <summary>
        /// Index of label in <see cref="lblColl"/> being hovered over, or -1.
        /// </summary>
        private int hoverIx = -1;

        /// <summary>
        /// Ctor: init.
        /// </summary>
        /// <param name="cmdTriggered">Delegate that will be called when a command is issued.</param>
        /// <param name="entry">Cedict entry to fetch clipboard data from.</param>
        /// <param name="senseIX">Index of sense over which user right-clicked, or -1.</param>
        /// <param name="script">Search script (so two Hanzi items are shown if needed).</param>
        public ResultsCtxtControl(CommandTriggeredDelegate cmdTriggered, ITextProvider tprov,
            CedictEntry entry,
            int senseIX,
            SearchScript script)
        {
            senseIX = 0;

            this.cmdTriggered = cmdTriggered;
            this.entry = entry;
            this.script = script;
            InitializeComponent();
            BackColor = ZenParams.BorderColor;
            pnlTop.BackColor = ZenParams.WindowColor;
            tblFull.BackColor = ZenParams.WindowColor;
            tblZho.BackColor = ZenParams.WindowColor;
            tblSense.BackColor = ZenParams.WindowColor;

            // Display strings
            string title = tprov.GetString("CtxtCopyTitle");
            string fullFormatted, fullCedict, hanzi1, hanzi2, pinyin, sense;
            getDisplayStrings(tprov, senseIX, out fullFormatted, out fullCedict,
                out hanzi1, out hanzi2, out pinyin, out sense);
            lblFullFormatted.Text = fullFormatted;
            lblFullCedict.Text = fullCedict;
            lblHanzi1.Text = hanzi1;
            lblHanzi2.Text = hanzi2;
            lblPinyin.Text = pinyin;
            lblSense.Text = sense;

            // Margin/border tweaks: 1px also at higher DPIs
            tblLayout.Location = new Point(1, 1);
            pnlTop.Margin = new Padding(0, 0, 0, 1);
            tblLayout.RowStyles[1].Height = pnlTop.Height + 1;
            tblFull.Margin = new Padding(0, 0, 0, 1);
            tblLayout.RowStyles[1].Height = tblFull.Height + 1;
            tblZho.Margin = new Padding(0, 0, 0, 1);
            tblLayout.RowStyles[2].Height = tblZho.Height + 1;
            tblSense.Margin = new Padding(0, 0, 0, 0);
            tblLayout.RowStyles[3].Height = tblSense.Height;
            tblLayout.Height = tblSense.Bottom;

            // Hide rows we don't need: second hanzi
            if (hanzi2 == null)
            {
                int hHanzi2 = lblHanzi2.Height;
                tblZho.Controls.Remove(lblHanzi2);
                lblPinyin.Top = lblHanzi2.Top;
                lblHanzi2.Dispose();
                lblHanzi2 = null;
                tblZho.Controls.Remove(lblPinyin);
                tblZho.Controls.Add(lblPinyin, 0, 2);
                tblZho.RowCount -= 1;
                tblZho.RowStyles.RemoveAt(2);
                tblZho.Height -= hHanzi2;
                tblLayout.RowStyles[2].Height -= hHanzi2;
                tblLayout.Height -= hHanzi2;
            }
            // Sense
            if (sense == null)
            {
                int hSense = tblSense.Height;
                tblLayout.Controls.Remove(tblSense);
                tblSense.Dispose();
                tblSense = null;
                tblLayout.RowStyles.RemoveAt(tblLayout.RowStyles.Count - 1);
                tblLayout.RowCount -= 1;
                tblLayout.Height -= hSense + 1;
            }

            // Label collection for hover
            int lblCount = 6;
            if (lblHanzi2 == null) --lblCount;
            if (lblSense == null) --lblCount;
            lblColl = new Label[lblCount];
            lblColl[0] = lblFullFormatted;
            lblColl[1] = lblFullCedict;
            lblColl[2] = lblHanzi1;
            int ix = 3;
            if (lblHanzi2 != null) { lblColl[ix] = lblHanzi2; ++ix; }
            lblColl[ix] = lblPinyin; ++ix;
            if (lblSense != null) { lblColl[ix] = lblSense; ++ix; }

            // Event handling for hover
            tblFull.CellPaint += onTblLayoutCellPaint;
            tblZho.CellPaint += onTblLayoutCellPaint;
            if (tblSense != null) tblSense.CellPaint += onTblLayoutCellPaint;
        }

        /// <summary>
        /// Handles various table layout panels' cell paint events for hover background.
        /// </summary>
        private void onTblLayoutCellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            // No hover: all backgrounds default.
            if (hoverIx == -1) return;
            Label lblHover = lblColl[hoverIx];
            object hoverTable;
            int rowIx = -1;
            if (lblHover == lblFullFormatted || lblHover == lblFullCedict)
            {
                hoverTable = tblFull;
                rowIx = lblHover == lblFullFormatted ? 1 : 2;
            }
            else if (lblHover == lblHanzi1 || lblHover == lblHanzi2 || lblHover == lblPinyin)
            {
                hoverTable = tblZho;
                if (lblHover == lblHanzi1) rowIx = 1;
                else if (lblHover == lblHanzi2) rowIx = 2;
                else rowIx = lblHanzi2 == null ? 2 : 3;
            }
            else
            {
                hoverTable = tblSense;
                rowIx = 1;
            }
            if (sender == hoverTable && e.Row == rowIx)
            {
                using (Brush b = new SolidBrush(ZenParams.CtxtMenuHoverColor))
                {
                    Rectangle rect = e.CellBounds;
                    rect.Width = e.ClipRectangle.Width;
                    e.Graphics.FillRectangle(b, rect);
                    rect.X = 0;
                    e.Graphics.FillRectangle(b, rect);
                }
            }
        }

        /// <summary>
        /// Gets pinyin display string from entry.
        /// </summary>
        private string getPinyin(int syllLimit = -1)
        {
            int ia, ib;
            var pinyinFull = entry.GetPinyinForDisplay(true, -1, 0, out ia, out ib);
            List<PinyinSyllable> pinyinList = new List<PinyinSyllable>();
            bool ellipsed = false;
            if (syllLimit == -1) pinyinList.AddRange(pinyinFull);
            else
            {
                int i;
                for (i = 0; i < pinyinFull.Count && i < syllLimit; ++i)
                    pinyinList.Add(pinyinFull[i]);
                if (i != pinyinFull.Count) ellipsed = true;
            }
            string res = "";
            foreach (var x in pinyinList)
            {
                if (res.Length > 0) res += " ";
                res += x.GetDisplayString(true);
            }
            if (ellipsed) res += " …";
            return res;
        }

        /// <summary>
        /// Gets sense display string from entry, leaving traditional/simplified away for monolingual searches.
        /// </summary>
        private string getSense(int senseIx)
        {
            var cs = entry.GetSenseAt(senseIx);
            string res = cs.Domain.GetPlainText();
            if (cs.Equiv != HybridText.Empty)
            {
                if (res != string.Empty) res += " ";
                res += cs.Equiv.GetPlainText();
            }
            if (cs.Note != HybridText.Empty)
            {
                if (res != string.Empty) res += " ";
                res += cs.Note.GetPlainText();
            }
            return res;
        }

        /// <summary>
        /// Ellipses string if it's longer than provided length limit.
        /// </summary>
        private static string ellipse(string what, int limit)
        {
            if (what.Length > limit) what = what.Substring(0, limit).Trim() + "…";
            return what;
        }

        /// <summary>
        /// Gets display strings by combining entry, sense index, and localized strings.
        /// </summary>
        private void getDisplayStrings(ITextProvider tprov, int senseIx,
            out string fullFormatted, out string fullCedict,
            out string hanzi1, out string hanzi2, out string pinyin, out string sense)
        {
            fullFormatted = tprov.GetString("CtxtCopyEntryFormatted");
            fullCedict = tprov.GetString("CtxtCopyEntryCedict");
            pinyin = tprov.GetString("CtxtCopyPinyin");
            string pinyinVal = getPinyin(Magic.CtxtMenuMaxSyllableLength);
            pinyin = string.Format(pinyin, pinyinVal);
            sense = null;
            hanzi1 = null;
            hanzi2 = null;
            if (script == SearchScript.Simplified || script == SearchScript.Traditional || entry.ChSimpl == entry.ChTrad)
            {
                hanzi1 = tprov.GetString("CtxtCopyHanzi");
                string hanzi1Val = script == SearchScript.Traditional ? entry.ChTrad : entry.ChSimpl;
                hanzi1Val = ellipse(hanzi1Val, Magic.CtxtMenuMaxSyllableLength);
                hanzi1 = string.Format(hanzi1, hanzi1Val);
            }
            else
            {
                hanzi1 = tprov.GetString("CtxtCopySimplified");
                string hanzi1Val = ellipse(entry.ChSimpl, Magic.CtxtMenuMaxSyllableLength);
                hanzi1 = string.Format(hanzi1, hanzi1Val);
                hanzi2 = tprov.GetString("CtxtCopyTraditional");
                string hanzi2Val = ellipse(entry.ChTrad, Magic.CtxtMenuMaxSyllableLength);
                hanzi2 = string.Format(hanzi2, hanzi2Val);
            }
            if (senseIx != -1)
            {
                sense = tprov.GetString("CtxtCopySense");
                string senseVal = getSense(senseIx);
                senseVal = ellipse(senseVal, Magic.CtxtMenuMaxSenseLength);
                sense = string.Format(sense, senseVal);
            }
        }

        /// <summary>
        /// See <see cref="ICtxtMenuControl.DoNavKey"/>.
        /// </summary>
        public void DoNavKey(CtxtMenuNavKey key)
        {
            if (key == CtxtMenuNavKey.Down || key == CtxtMenuNavKey.Up)
            {
                if (hoverIx == -1) hoverIx = key == CtxtMenuNavKey.Down ? 0 : lblColl.Length - 1;
                else
                {
                    hoverIx += key == CtxtMenuNavKey.Down ? 1 : -1;
                    if (hoverIx < 0) hoverIx = lblColl.Length - 1;
                    else if (hoverIx == lblColl.Length) hoverIx = 0;
                }
                tblLayout.Invalidate(true);
            }
            if (key == CtxtMenuNavKey.Enter || key == CtxtMenuNavKey.Space)
            {
                if (hoverIx == -1) return;
                fire(hoverIx);
            }
        }

        /// <summary>
        /// See <see cref="ICtxtMenuControl.DoMouseLeave"/>.
        /// </summary>
        public void DoMouseLeave()
        {
            if (hoverIx == -1) return;
            hoverIx = -1;
            tblLayout.Invalidate(true);
        }

        /// <summary>
        /// Gets index (in <see cref="lblColl"/>) that belongs to provided coordinates.
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        private int getIxForPoint(Point pt)
        {
            int ix = -1;
            for (int i = 0; i != lblColl.Length; ++i)
            {
                Label lbl = lblColl[i];
                if (pt.Y >= lbl.Top + lbl.Parent.Top && pt.Y <= lbl.Bottom + lbl.Parent.Top)
                {
                    ix = i;
                    break;
                }
            }
            return ix;
        }

        /// <summary>
        /// See <see cref="ICtxtMenuControl.DoMouseMove"/>.
        /// </summary>
        public void DoMouseMove(Point pt)
        {
            int newIx = getIxForPoint(pt);
            if (newIx != hoverIx)
            {
                hoverIx = newIx;
                tblLayout.Invalidate(true);
            }
        }

        /// <summary>
        /// See <see cref="ICtxtMenuControl.DoMouseClick"/>.
        /// </summary>
        public void DoMouseClick(Point pt)
        {
            int ix = getIxForPoint(pt);
            if (ix == -1) return;
            fire(ix);
        }

        /// <summary>
        /// See <see cref="ICtxtMenuControl.AsUserControl"/>.
        /// </summary>
        public UserControl AsUserControl
        {
            get { return this; }
        }

        /// <summary>
        /// See <see cref="ICtxtMenuControl.AssumeSize"/>.
        /// </summary>
        public void AssumeSize()
        {
            int maxLabelW = lblColl[0].PreferredWidth;
            for (int i = 1; i != lblColl.Length; ++i)
                if (lblColl[i].PreferredWidth > maxLabelW) maxLabelW = lblColl[i].PreferredWidth;
            int w = tblFull.Padding.Left + tblFull.Padding.Right + maxLabelW;
            tblLayout.Width = w;
            pnlTop.Width = w;
            tblFull.Width = w;
            tblZho.Width = w;
            tblSense.Width = w;
            foreach (Label lbl in lblColl)
                lbl.Width = maxLabelW;

            Size = new Size(w + 2, tblLayout.Height + 2);
        }

        /// <summary>
        /// Fire command for label index (in <see cref="lblColl"/>).
        /// </summary>
        private void fire(int ix)
        {
            // Prepare plain text for clipboard, and optionally (for formatted full entry), also html.
            string plainText = "text";
            string html = null;
            // Our plain text options
            Label lbl = lblColl[ix];
            if (lbl == lblHanzi1)
                plainText = script == SearchScript.Traditional ? entry.ChTrad : entry.ChSimpl;
            else if (lbl == lblHanzi2) plainText = entry.ChTrad;
            else if (lbl == lblPinyin) plainText = getPinyin();

            // Copy to clipboard: plain text
            if (html == null) Clipboard.SetText(plainText);

            // TO-DO:
            // - Cedict
            // - HTML formatted (along with plain text equivalent)
            // - Sense (after updating getSense to mind single search script)
            // !!!
            // - Sense ID hover behavior in OneResultControl, sense ID passed to context menu

            // Let parent control know that command has been triggered: time to close context menu.
            cmdTriggered(this);
        }
    }
}
