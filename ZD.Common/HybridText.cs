using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace ZD.Common
{
    /// <summary>
    /// Represents text that contains a mixture of Latin characters and embedded, structured Chinese.
    /// </summary>
    [System.Diagnostics.DebuggerDisplay("{GetPlainText()}")]
    public class HybridText : IBinSerializable
    {
        /// <summary>
        /// Singleton instance of the "empty" hybrid text.
        /// </summary>
        private static HybridText empty;

        /// <summary>
        /// Returns the "empty" hybrid text object.
        /// </summary>
        public static HybridText Empty
        {
            get { return empty; }
        }

        /// <summary>
        /// The hybrid text's runs.
        /// </summary>
        private readonly List<TextRun> runs;

        /// <summary>
        /// Gets an enumerator to the hybrid text's runs.
        /// </summary>
        public IEnumerable<TextRun> Runs
        {
            get { return runs; }
        }

        /// <summary>
        /// Gets number of runs in hybrid text.
        /// </summary>
        public int RunCount
        {
            get { return runs.Count; }
        }

        /// <summary>
        /// Returns text runs at the provided index in array of runs.
        /// </summary>
        public TextRun GetRunAt(int ix)
        {
            return runs[ix];
        }

        /// <summary>
        /// Returns true if this is an empty text.
        /// </summary>
        public bool IsEmpty
        {
            get { return runs.Count == 0; }
        }

        /// <summary>
        /// Returns true if object is a single plain text run that equals provided string.
        /// </summary>
        public bool EqualsPlainText(string what)
        {
            if (runs.Count != 1) return false;
            TextRunLatin trl = runs[0] as TextRunLatin;
            if (trl == null) return false;
            return trl.GetPlainText() == what;
        }

        /// <summary>
        /// Static ctor: initializes empty hybrid text object.
        /// </summary>
        static HybridText()
        {
            List<TextRun> emptyRuns = new List<TextRun>();
            empty = new HybridText(new ReadOnlyCollection<TextRun>(emptyRuns));
        }

        /// <summary>
        /// Gets "plain", unstructured display text for hybrid text.
        /// </summary>
        public string GetPlainText()
        {
            if (runs.Count == 0) return string.Empty;
            if (runs.Count == 1) return runs[0].GetPlainText();
            StringBuilder sb = new StringBuilder(runs[0].GetPlainText());
            for (int i = 1; i != runs.Count; ++i)
            {
                sb.Append(' ');
                sb.Append(runs[i].GetPlainText());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns as plain text in the CEDICT format.
        /// </summary>
        /// <returns></returns>
        public string GetCedict()
        {
            if (runs.Count == 0) return string.Empty;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i != runs.Count; ++i)
            {
                if (i != 0) sb.Append(' ');
                TextRun run = runs[i];
                if (run is TextRunZho) sb.Append((run as TextRunZho).GetCedict());
                else sb.Append(run.GetPlainText());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Initializes a pure plain-text instance.
        /// </summary>
        public HybridText(string plain)
        {
            runs = new List<TextRun>(1);
            TextRunLatin run = new TextRunLatin(plain);
            runs.Add(run);
        }

        /// <summary>
        /// Ctor: initialize instance.
        /// </summary>
        public HybridText(ReadOnlyCollection<TextRun> runs)
        {
            if (runs == null) throw new ArgumentNullException("runs");
            this.runs = new List<TextRun>(runs);
        }

        /// <summary>
        /// Static creator: deserialize instance from binary stream.
        /// </summary>
        public static HybridText Deserialize(BinReader br)
        {
            // Read number of runs
            short length = br.ReadShort();
            // Zero runs: return one and only "empty" object
            if (length == 0) return Empty;

            // OK, deserialize runs.
            List<TextRun> runs = new List<TextRun>(length);
            // Deserialize each run after looking at flags for polymorphism
            for (short i = 0; i != length; ++i)
            {
                byte flags = br.ReadByte();
                bool isZho = ((flags & 1) == 1);
                TextRun tr;
                if (isZho) tr = new TextRunZho(br);
                else tr = new TextRunLatin(br);
                runs.Add(tr);
            }
            // Done
            return new HybridText(new ReadOnlyCollection<TextRun>(runs));
        }

        /// <summary>
        /// Serialize into binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            // Write number of runs
            short length = (short)runs.Count;
            bw.WriteShort(length);
            // Write each run; polymorphism here through byte flags
            foreach (TextRun tr in runs)
            {
                bool isZho = (tr is TextRunZho);
                // "1" in flags indicates Chinese run
                byte flags = 0;
                if (isZho) flags |= 1;
                bw.WriteByte(flags);
                // Write run itself
                if (isZho) (tr as TextRunZho).Serialize(bw);
                else (tr as TextRunLatin).Serialize(bw);
            }
        }

    }

    /// <summary>
    /// <para>Represents one text run in mixed text (senses of Cedict entries).</para>
    /// <para>Can be Latin text or Chinese.</para>
    /// </summary>
    public abstract class TextRun
    {
        /// <summary>
        /// This is a virtual base class, not to be constructed directly.
        /// </summary>
        protected TextRun()
        { }

        /// <summary>
        /// Gets "plain", unstructured display text for run.
        /// </summary>
        public abstract string GetPlainText();
    }

    /// <summary>
    /// A Chinese text run.
    /// </summary>
    public class TextRunZho : TextRun, IBinSerializable
    {
        /// <summary>
        /// Text in simplified characters.
        /// </summary>
        public readonly string Simp;

        /// <summary>
        /// Text in traditional characters; may be identical to <see cref="Simp"/>.
        /// </summary>
        public readonly string Trad;

        /// <summary>
        /// Pinyin transcription (may be null).
        /// </summary>
        public readonly PinyinSyllable[] Pinyin;

        /// <summary>
        /// See <see cref="TextRun.GetPlainText"/>.
        /// </summary>
        public override string GetPlainText()
        {
            if (Simp == null) return GetPinyinInOne(true);

            string py = GetPinyinInOne(true);
            if (py == null) py = "";
            else py = " [" + py + "]";
            
            if (Simp == Trad)
                return Simp + py;
            else return Simp + " • " + Trad + py;
        }

        /// <summary>
        /// Gets text run's text in the CEDICT text format.
        /// </summary>
        /// <returns></returns>
        public string GetCedict()
        {
            if (Simp == null) return GetPinyinInOne(false);

            string py = GetPinyinInOne(false);
            if (py == null) py = "";
            else py = "[" + py + "]";

            if (Simp == Trad)
                return Simp + py;
            else return Trad + "|" + Simp + py;
        }

        /// <summary>
        /// Gets raw pinyin string: all syllables concatenated, with numbers for tone marks.
        /// </summary>
        public string GetPinyinRaw()
        {
            StringBuilder sb = new StringBuilder();
            foreach (PinyinSyllable syll in Pinyin)
            {
                sb.Append(syll.Text);
                int tone = syll.Tone;
                if (tone == 0) tone = 5;
                if (tone != -1) sb.Append(tone.ToString());
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns concatenated pinyin syllables.
        /// </summary>
        /// <param name="diacritics">If yes, adds diacritics for tone marks; otherwise, appends number.</param>
        public string GetPinyinInOne(bool diacritics)
        {
            if (Pinyin == null || Pinyin.Length == 0) return string.Empty;
            if (Pinyin.Length == 1) return Pinyin[0].GetDisplayString(diacritics);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i != Pinyin.Length; ++i)
            {
                if (i != 0) sb.Append(' ');
                sb.Append(Pinyin[i].GetDisplayString(diacritics));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Ctor: Hanzi, plus optional pinyin.
        /// </summary>
        /// <param name="simp">Simplified Hanzi. Must not be null.</param>
        /// <param name="trad">Traditional Hanzi. Null means simplified is the same as traditional.</param>
        /// <param name="pinyin">Pinyin. Can be null, but not empty array if present.</param>
        public TextRunZho(string simp, string trad, PinyinSyllable[] pinyin)
        {
            if (simp == null) throw new ArgumentNullException("simp");
            if (pinyin != null && pinyin.Length == 0) throw new ArgumentException("Pinyin syllables must not be an empty array.");
            Simp = simp;
            if (trad == null || trad == simp) Trad = simp;
            else Trad = trad;
            Pinyin = pinyin;
        }

        /// <summary>
        /// Ctor: only pinyin.
        /// </summary>
        public TextRunZho(PinyinSyllable[] pinyin)
        {
            if (pinyin == null) throw new ArgumentNullException("pinyin");
            if (pinyin.Length == 0) throw new ArgumentException("Pinyin syllables must not be an empty array.");
            Simp = Trad = null;
            Pinyin = pinyin;
        }

        /// <summary>
        /// Ctor: read from binary stream.
        /// </summary>
        public TextRunZho(BinReader br)
        {
            // Read flags
            // 1: Traditional and simplified are different
            // 2: Pinyin present
            byte flags = br.ReadByte();
            // Read simplified
            if ((flags & 1) == 1) Simp = br.ReadString();
            // Is traditional different?
            if ((flags & 2) == 2) Trad = br.ReadString();
            else Trad = Simp;
            // Is pinyin present?
            if ((flags & 4) == 4)
            {
                int pinyinCount = (int)br.ReadByte();
                Pinyin = new PinyinSyllable[pinyinCount];
                for (int i = 0; i != pinyinCount; ++i) Pinyin[i] = new PinyinSyllable(br);
            }
            else Pinyin = null;
        }

        /// <summary>
        /// Serialize into binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            // Write flags
            byte flags = 0;
            if (Simp != null) flags |= 1;
            if (Trad != Simp) flags |= 2;
            if (Pinyin != null) flags |= 4;
            bw.WriteByte(flags);
            // Write simplified
            if (Simp != null) bw.WriteString(Simp);
            // Write traditional, if different
            if (Trad != Simp) bw.WriteString(Trad);
            // Write pinyin, if present
            if (Pinyin != null)
            {
                if (Pinyin.Length > byte.MaxValue) throw new Exception("Pinyin syllable count exceeds maximum byte value: " + Pinyin.Length.ToString());
                byte pinyinCount = (byte)Pinyin.Length;
                bw.WriteByte(pinyinCount);
                foreach (PinyinSyllable py in Pinyin) py.Serialize(bw);
            }
        }
    }

    /// <summary>
    /// A Latin text run.
    /// </summary>
    public class TextRunLatin : TextRun, IBinSerializable
    {
        /// <summary>
        /// The run's text.
        /// </summary>
        private readonly string Text;
        
        /// <summary>
        /// See <see cref="TextRun.GetPlainText"/>.
        /// </summary>
        public override string GetPlainText()
        {
            return Text;
        }

        /// <summary>
        /// Ctor: init from value.
        /// </summary>
        /// <param name="text"></param>
        public TextRunLatin(string text)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentException("Must not be null or empty.", "text");
            Text = text;
        }

        /// <summary>
        /// Ctor: read from binary stream.
        /// </summary>
        public TextRunLatin(BinReader br)
        {
            Text = br.ReadString();
        }

        /// <summary>
        /// Serialize into binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            bw.WriteString(Text);
        }
    }
}
