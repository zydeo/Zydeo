using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using DND.HanziLookup;
using DND.Controls;

namespace Sandbox
{
    public partial class MainForm : Form
    {
        private double looseness = 0.25;    // the "looseness" of lookup, 0-1, higher == looser, looser more computationally intensive
        private int numResults = 15;		// maximum number of results to return with each lookup

        private readonly StrokesDataSource strokesDataSource;

        public MainForm(StrokesDataSource strokesDataSource)
        {
            InitializeComponent();
            lblChars.Text = "";
            writingPad.StrokesChanged += writingPad_StrokesChanged;

            this.strokesDataSource = strokesDataSource;

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        #region Fixed aspect ratio resize, so input field stays rectangular

        const int WM_SIZING = 0x214;
        const int WMSZ_LEFT = 1;
        const int WMSZ_RIGHT = 2;
        const int WMSZ_TOP = 3;
        const int WMSZ_BOTTOM = 6;

        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SIZING)
            {
                RECT rc = (RECT)Marshal.PtrToStructure(m.LParam, typeof(RECT));
                int res = m.WParam.ToInt32();
                if (res == WMSZ_LEFT || res == WMSZ_RIGHT || res == WMSZ_RIGHT + WMSZ_BOTTOM || res == WMSZ_LEFT + WMSZ_BOTTOM)
                {
                    //Left or right resize -> adjust height (bottom)
                    //Lower-right corner resize -> adjust height (could have been width)
                    int widthDiff = (rc.Right - rc.Left) - Width;
                    rc.Bottom = rc.Top + Height + widthDiff;
                }
                else if (res == WMSZ_TOP || res == WMSZ_BOTTOM || res == WMSZ_LEFT + WMSZ_TOP || res == WMSZ_RIGHT + WMSZ_TOP)
                {
                    //Up or down resize -> adjust width (right)
                    //Upper-left corner -> adjust width (could have been height)
                    int heightDiff = (rc.Bottom - rc.Top) - Height;
                    rc.Right = rc.Left + Width + heightDiff;
                }
                Marshal.StructureToPtr(rc, m.LParam, true);
            }

            base.WndProc(ref m);
        }

        #endregion

        private void llClear_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            writingPad.Clear();
            lblChars.Text = "";
        }

        private void handleResults(char[] chars)
        {
            string str = "";
            foreach (char c in chars) str += c;
            lblChars.Text = str;
        }

        private void writingPad_StrokesChanged(Control sender, IEnumerable<WritingPad.Stroke> strokes)
        {
            // Convert stroke data to HanziLookup's format
            WrittenCharacter wc = new WrittenCharacter();
            foreach (WritingPad.Stroke stroke in strokes)
            {
                WrittenStroke ws = new WrittenStroke();
                foreach (PointF p in stroke.Points)
                {
                    WrittenPoint wp = new WrittenPoint((int)(p.X), (int)(p.Y));
                    ws.AddPoint(wp, ref wc.LeftX, ref wc.RightX, ref wc.TopY, ref wc.BottomY);
                }
                wc.AddStroke(ws);
            }
            if (wc.StrokeList.Count == 0)
            {
                // Don't bother doing anything if nothing has been input yet (number of strokes == 0).
                handleResults(new char[0]);
                return;
            }

            CharacterDescriptor id = wc.BuildCharacterDescriptor();

            bool searchTraditional = true;
            bool searchSimplified = true;

            strokesDataSource.Reset();
            StrokesMatcher matcher = new StrokesMatcher(id,
                                                     searchTraditional,
                                                     searchSimplified,
                                                     this.looseness,
                                                     this.numResults,
                                                     this.strokesDataSource);
            char[] res = matcher.DoMatching();
            handleResults(res);
        }
    }
}
