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
            InitializeComponent();
            BackColor = ZenParams.BorderColor;
            pnlTop.BackColor = ZenParams.WindowColor;
            tblFull.BackColor = ZenParams.WindowColor;
            tblZho.BackColor = ZenParams.WindowColor;
            tblSense.BackColor = ZenParams.WindowColor;

            // Display strings
            string title = tprov.GetString("CtxtCopyTitle");
            string fullFormatted, fullCedict, hanzi1, hanzi2, pinyin, sense;
            getDisplayStrings(entry, script, tprov, senseIX, out fullFormatted, out fullCedict,
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
            if (sender == tblZho && hoverTable == tblZho)
            {
                int i = 0;
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
        /// Gets display strings by combining entry, sense index, and localized strings.
        /// </summary>
        private void getDisplayStrings(CedictEntry entry, SearchScript script, ITextProvider tprov, int senseIx,
            out string fullFormatted, out string fullCedict,
            out string hanzi1, out string hanzi2, out string pinyin, out string sense)
        {
            fullFormatted = tprov.GetString("CtxtCopyEntryFormatted");
            fullCedict = tprov.GetString("CtxtCopyEntryCedict");
            pinyin = tprov.GetString("CtxtCopyPinyin");
            int ia, ib;
            var pinyinArr = entry.GetPinyinForDisplay(true, -1, 0, out ia, out ib);
            string pinyinVal = "";
            foreach (var x in pinyinArr)
            {
                if (pinyinVal.Length > 0) pinyinVal += " ";
                pinyinVal += x.GetDisplayString(true);
            }
            pinyin = string.Format(pinyin, pinyinVal);
            sense = null;
            hanzi1 = null;
            hanzi2 = null;
            if (script == SearchScript.Simplified || script == SearchScript.Traditional || entry.ChSimpl == entry.ChTrad)
            {
                hanzi1 = tprov.GetString("CtxtCopyHanzi");
                if (script == SearchScript.Traditional)
                    hanzi1 = string.Format(hanzi1, entry.ChTrad);
                else
                    hanzi1 = string.Format(hanzi1, entry.ChSimpl);
            }
            else
            {
                hanzi1 = tprov.GetString("CtxtCopySimplified");
                hanzi1 = string.Format(hanzi1, entry.ChSimpl);
                hanzi2 = tprov.GetString("CtxtCopyTraditional");
                hanzi2 = string.Format(hanzi2, entry.ChTrad);
            }
            if (senseIx != -1)
            {
                var cs = entry.GetSenseAt(senseIx);
                string senseVal = cs.Domain.GetPlainText();
                if (cs.Equiv != HybridText.Empty)
                {
                    if (senseVal != string.Empty) senseVal += " ";
                    senseVal += cs.Equiv.GetPlainText();
                }
                if (cs.Note != HybridText.Empty)
                {
                    if (senseVal != string.Empty) senseVal += " ";
                    senseVal += cs.Note.GetPlainText();
                }
                sense = tprov.GetString("CtxtCopySense");
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
            // Let parent control know that command has been triggered: time to close context menu.
            cmdTriggered(this);
        }
    }
}
