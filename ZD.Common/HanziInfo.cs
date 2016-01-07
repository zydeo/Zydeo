using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// One (graphical) stroke in a Hanzi, along with its median.
    /// </summary>
    public class OneStroke : IBinSerializable
    {
        /// <summary>
        /// The stroke, as SVG. Canvas is 1024x1024.
        /// </summary>
        public readonly string SVG;

        /// <summary>
        /// The stroke's median points, in the correct order (for stroke direction).
        /// </summary>
        public readonly ReadOnlyCollection<Tuple<short, short>> Median;

        /// <summary>
        /// Ctor: init immutable instance.
        /// </summary>
        public OneStroke(string svg, IList<Tuple<short, short>> median)
        {
            if (string.IsNullOrEmpty(svg)) throw new ArgumentException("svg");
            if (median == null || median.Count == 0) throw new ArgumentException("median");
            SVG = svg;
            Median = new ReadOnlyCollection<Tuple<short, short>>(median);
        }

        /// <summary>
        /// Ctor: deserialize from binary stream.
        /// </summary>
        public OneStroke(BinReader br)
        {
            short svgLen = br.ReadShort();
            StringBuilder sb = new StringBuilder(svgLen);
            for (int i = 0; i != svgLen; ++i)
            {
                byte b = br.ReadByte();
                sb.Append((char)b);
            }
            SVG = sb.ToString();
            short mLen = br.ReadShort();
            List<Tuple<short, short>> median = new List<Tuple<short,short>>(mLen);
            for (int i = 0; i != mLen; ++i)
            {
                short x = br.ReadShort();
                short y = br.ReadShort();
                median.Add(new Tuple<short, short>(x, y));
            }
            Median = new ReadOnlyCollection<Tuple<short, short>>(median);
        }

        /// <summary>
        /// Serialize into binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            if (SVG.Length > short.MaxValue) throw new Exception("SVG too long for serialization.");
            bw.WriteShort((short)SVG.Length);
            string[] svgParts = SVG.Split(' ');
            foreach (string sp in svgParts)
            {
                float val;
                if (float.TryParse(sp, out val)) bw.WriteShort((short)val);
                else
                {
                    if (sp.Length != 1) throw new Exception("Invalid SVG.");
                    int ival = (int)sp[0];
                    if (ival > byte.MaxValue) throw new Exception("Invalid character in SVG.");
                    bw.WriteShort((short)(ival + 4096));
                }
            }
            //foreach (char c in SVG)
            //{
            //    if (((int)c) > (int)byte.MaxValue) throw new Exception("Non-ASCII character in SVG.");
            //    bw.WriteByte((byte)(int)c);
            //}
            if (Median.Count > short.MaxValue) throw new Exception("Median too long for serialization.");
            bw.WriteShort((short)Median.Count);
            foreach (var x in Median)
            {
                bw.WriteShort(x.Item1);
                bw.WriteShort(x.Item2);
            }
        }
    }

    /// <summary>
    /// Information about a single Hanzi (strokes and their medians; decomposition; radical)
    /// </summary>
    public class HanziInfo : IBinSerializable
    {
        /// <summary>
        /// The strokes that make up this Hanzi.
        /// </summary>
        public readonly ReadOnlyCollection<OneStroke> Strokes;

        /// <summary>
        /// Decomposition, as a Unicode string.
        /// </summary>
        public readonly string Decomp;

        /// <summary>
        /// The radical, as a Unicode character.
        /// </summary>
        public readonly char Radical;

        /// <summary>
        /// The phonetic component, if the character's etymology is pictophonetic.
        /// </summary>
        public readonly char Phon;

        /// <summary>
        /// The semantic component, if the character's etymology is pictophonetic.
        /// </summary>
        public readonly char Seman;

        /// <summary>
        /// Ctor: init immutable instance.
        /// </summary>
        public HanziInfo(IList<OneStroke> strokes, string decomp,
            char radical, char phon, char seman)
        {
            if (strokes == null || strokes.Count == 0) throw new ArgumentException("strokes");
            if (decomp == null) throw new ArgumentException("decomp");
            Strokes = new ReadOnlyCollection<OneStroke>(strokes);
            Decomp = decomp;
            Radical = radical;
            Phon = phon;
            Seman = seman;
        }

        /// <summary>
        /// Ctor: deserialize from binary stream.
        /// </summary>
        public HanziInfo(BinReader br)
        {
            Radical = br.ReadChar();
            Phon = br.ReadChar();
            Seman = br.ReadChar();
            Decomp = br.ReadString();
            short sLen = br.ReadShort();
            List<OneStroke> strokes = new List<OneStroke>(sLen);
            for (int i = 0; i != sLen; ++i)
            {
                strokes.Add(new OneStroke(br));
            }
            Strokes = new ReadOnlyCollection<OneStroke>(strokes);
        }

        /// <summary>
        /// Serialize into binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            bw.WriteChar(Radical);
            bw.WriteChar(Phon);
            bw.WriteChar(Seman);
            bw.WriteString(Decomp);
            if (Strokes.Count > short.MaxValue) throw new Exception("Strokes too long for serialization.");
            bw.WriteShort((short)Strokes.Count);
            foreach (var os in Strokes) os.Serialize(bw);
        }
    }
}
