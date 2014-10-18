using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZD.Gui
{
    partial class OneResultControl
    {
        /// <summary>
        /// <para>Stores short strings in a single concatenated form to save memory.</para>
        /// <para>Avoids per-object overhead of each string, plus allows referring to strings with a ushort.</para>
        /// <para>Total length, and length of individual strings, must be below ushort.MaxValue.</para>
        /// </summary>
        private class TextPool
        {
            /// <summary>
            /// The pool. StringBuilder until <see cref="FinishBuilding"/> is called; string afterwards.
            /// </summary>
            private object pool = new StringBuilder();

            /// <summary>
            /// Ctor. Initializes object ready for pooling.
            /// </summary>
            public TextPool()
            {
                // Position zero is reserved for empty string. Just holding place here.
                (pool as StringBuilder).Append((char)65535);
            }

            /// <summary>
            /// Finishes pooling, compacts pool into a single string.
            /// </summary>
            public void FinishBuilding()
            {
                StringBuilder sb = pool as StringBuilder;
                if (sb == null) throw new InvalidOperationException("FinishBuilding has already been called.");
                pool = sb.ToString();
            }

            /// <summary>
            /// Pools a new string.
            /// </summary>
            /// <param name="str">The string to pool.</param>
            /// <returns>An ushort that identifies the string in the pool.</returns>
            public ushort PoolString(string str)
            {
                StringBuilder sb = pool as StringBuilder;
                if (sb == null) throw new InvalidOperationException("Cannot pool more strings after FinishBuilding has been called.");
                int pos = sb.Length;
                if (pos > ushort.MaxValue) throw new Exception("Maximum pool size exceeded: " + sb.Length.ToString());
                if (str == null) throw new ArgumentException("Null cannot be pooled.");
                if (str.Length > ushort.MaxValue) throw new Exception("String too long: " + str.Length.ToString());
                // Empty string is speciel - zero
                if (str.Length == 0) return 0;
                // First store length of string as a character.
                ushort ulen = (ushort)str.Length;
                char clen = (char)ulen;
                sb.Append(clen);
                // Then store string itself.
                sb.Append(str);
                return (ushort)pos;
            }

            /// <summary>
            /// Gets the string corresponding to the provided ID (cut out from pool through Substring).
            /// </summary>
            public string GetString(ushort pos)
            {
                // Zero is empty string
                if (pos == 0) return string.Empty;
                string str = pool as string;
                char clen;
                ushort ulen;
                if (str == null)
                {
                    StringBuilder sb = pool as StringBuilder;
                    clen = sb[pos];
                    ulen = (ushort)clen;
                    return sb.ToString(pos + 1, ulen);
                }
                clen = str[pos];
                ulen = (ushort)clen;
                return str.Substring(pos + 1, ulen);
            }
        }
    }
}
