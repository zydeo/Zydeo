using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ZD.Common;

namespace ZD.CedictEngine
{
    /// <summary>
    /// Index for hanzi, pinyin and target-language content.
    /// </summary>
    internal class Index : IBinSerializable
    {
        /// <summary>
        /// Maps hanzi characters to the list of entries where they occur.
        /// </summary>
        public readonly Dictionary<char, IdeoIndexItem> IdeoIndex;

        /// <summary>
        /// Maps basic pinyin syllables (w/o tone) to entries where they occur with different tones.
        /// </summary>
        public readonly Dictionary<string, PinyinIndexItem> PinyinIndex;

        /// <summary>
        /// Token-to-index map.
        /// </summary>
        public readonly WordHolder WordHolder;

        /// <summary>
        /// Maps word IDs to tokenized senses where they occur.
        /// </summary>
        public readonly Dictionary<int, SenseIndexItem> SenseIndex;

        /// <summary>
        /// Ctor: creates an empty instance (used while compiling index).
        /// </summary>
        public Index()
        {
            WordHolder = new WordHolder();
            SenseIndex = new Dictionary<int, SenseIndexItem>();
            IdeoIndex = new Dictionary<char, IdeoIndexItem>();
            PinyinIndex = new Dictionary<string, PinyinIndexItem>();
        }

        /// <summary>
        /// Ctor: deserializes binary data.
        /// </summary>
        public Index(BinReader br)
        {
            WordHolder = new CedictEngine.WordHolder(br);
            SenseIndex = new Dictionary<int, SenseIndexItem>();
            int senseIndexKeyCount = br.ReadInt();
            for (int i = 0; i != senseIndexKeyCount; ++i)
            {
                int tokenId = br.ReadInt();
                SenseIndexItem sii = new SenseIndexItem(br);
                SenseIndex[tokenId] = sii;
            }

            IdeoIndex = new Dictionary<char, IdeoIndexItem>();
            PinyinIndex = new Dictionary<string, PinyinIndexItem>();

            int ideoIndexKeyCount = br.ReadInt();
            for (int i = 0; i != ideoIndexKeyCount; ++i)
            {
                char c = br.ReadChar();
                IdeoIndexItem iii = new IdeoIndexItem(br);
                IdeoIndex[c] = iii;
            }

            int pinyinIndexKeyCount = br.ReadInt();
            for (int i = 0; i != pinyinIndexKeyCount; ++i)
            {
                string str = br.ReadString();
                PinyinIndexItem pyi = new PinyinIndexItem(br);
                PinyinIndex[str] = pyi;
            }
        }

        /// <summary>
        /// Serializes index into binary stream.
        /// </summary>
        public void Serialize(BinWriter bw)
        {
            WordHolder.Serialize(bw);

            int senseIndexKeyCount = SenseIndex.Count;
            bw.WriteInt(senseIndexKeyCount);
            foreach (var x in SenseIndex)
            {
                bw.WriteInt(x.Key);
                x.Value.Serialize(bw);
            }

            int ideoIndexKeyCount = IdeoIndex.Count;
            bw.WriteInt(ideoIndexKeyCount);
            foreach (var x in IdeoIndex)
            {
                bw.WriteChar(x.Key);
                x.Value.Serialize(bw);
            }

            int pinyinIndexKeyCount = PinyinIndex.Keys.Count;
            bw.WriteInt(pinyinIndexKeyCount);
            foreach (var x in PinyinIndex)
            {
                bw.WriteString(x.Key);
                x.Value.Serialize(bw);
            }
        }
    }
}
