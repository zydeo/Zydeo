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
                if (sb.Length != 0) sb.Append(' ');
                short val = br.ReadShort();
                // Character
                if (val > 16384) sb.Append((char)(val - 16384));
                // Value
                else
                {
                    string strVal = val.ToString();
                    if (strVal.Length == 1) sb.Append(strVal);
                    else
                    {
                        // Up to decimal point
                        sb.Append(strVal.Substring(0, strVal.Length - 1));
                        // Has fraction
                        if (strVal.Length > 1 && strVal[strVal.Length - 1] != '0')
                        {
                            sb.Append('.');
                            sb.Append(strVal[strVal.Length - 1]);
                        }
                    }
                }
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
            string[] svgParts = SVG.Split(' ');
            if (svgParts.Length > short.MaxValue) throw new Exception("SVG too long for serialization.");
            bw.WriteShort((short)svgParts.Length);
            foreach (string sp in svgParts)
            {
                if (string.IsNullOrEmpty(sp)) throw new Exception("Empty string inside split SVG.");
                float val;
                if (float.TryParse(sp, out val))
                {
                    short sVal = (short)(val * 10);
                    if (sVal > 16384) throw new Exception("Value in SVG path too large for serialization.");
                    bw.WriteShort(sVal);
                }
                else
                {
                    if (sp.Length != 1) throw new Exception("Invalid SVG.");
                    int ival = (int)sp[0];
                    if (ival > byte.MaxValue) throw new Exception("Invalid character in SVG.");
                    ival += 16384;
                    bw.WriteShort((short)ival);
                }
            }
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
