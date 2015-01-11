using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZD.DictEditor
{
    internal class HintingTextBox : RichTextBox
    {
        public enum SpecialKeys
        {
            None,
            Right,
            Left,
            Esc,
            Enter,
        }

        public delegate bool HandleSpecialKeyDelegate(SpecialKeys sk);
        public HandleSpecialKeyDelegate HandleSpecialKey;

        private const int KV_RIGHT = 39;
        private const int KV_LEFT = 37;
        private const int KV_ESC = 27;
        private const int KV_ENTER = 13;

        private static SpecialKeys getSpecialKey(KeyEventArgs e)
        {
            if (e.Modifiers != Keys.None) return SpecialKeys.None;
            if (e.KeyValue == KV_RIGHT) return SpecialKeys.Right;
            else if (e.KeyValue == KV_LEFT) return SpecialKeys.Left;
            else if (e.KeyValue == KV_ESC) return SpecialKeys.Esc;
            else if (e.KeyValue == KV_ENTER) return SpecialKeys.Enter;
            else return SpecialKeys.None;
        }

        bool swallowNextEnterPress = false;

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r' && swallowNextEnterPress)
            {
                e.Handled = true;
                swallowNextEnterPress = false;
            }
            else base.OnKeyPress(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            SpecialKeys sk = getSpecialKey(e);
            if (sk == SpecialKeys.None || HandleSpecialKey == null)
            {
                base.OnKeyDown(e);
                return;
            }
            else
            {
                e.Handled = HandleSpecialKey(sk);
                if (!e.Handled) base.OnKeyDown(e);
                else if (sk == SpecialKeys.Enter) swallowNextEnterPress = true;
            }
        }
    }
}
