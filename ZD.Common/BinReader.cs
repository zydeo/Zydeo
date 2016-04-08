using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZD.Common
{
    public class BinReader : IDisposable
    {
        private Stream stream;
        private BinaryReader reader;

        public BinReader(string fileName)
        {
            stream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            reader = new BinaryReader(stream);
        }

        public BinReader(byte[] buf, int len)
        {
            stream = new MemoryStream(buf, 0, len);
            reader = new BinaryReader(stream);
        }

        public int Position
        {
            get { return (int)stream.Position; }
            set { stream.Position = value; }
        }

        public void Dispose()
        {
            if (reader != null) reader.Dispose();
            if (stream != null) stream.Dispose();
        }

        public char ReadChar()
        {
            return reader.ReadChar();
        }

        public long ReadLong()
        {
            return reader.ReadInt64();
        }

        public int ReadInt()
        {
            return reader.ReadInt32();
        }

        public short ReadShort()
        {
            return reader.ReadInt16();
        }

        public byte ReadByte()
        {
            return reader.ReadByte();
        }

        public string ReadString()
        {
            ushort length = reader.ReadUInt16();
            char[] res = new char[length];
            for (int i = 0; i != length; ++i)
            {
                ushort cu = reader.ReadUInt16();
                cu = (ushort)(cu ^ 6939);
                res[i] = (char)cu;
            }
            return new string(res);
        }

        public ItemType[] ReadArray<ItemType>(Func<BinReader, ItemType> readAction)
        {
            int length = reader.ReadInt32();
            ItemType[] items = new ItemType[length];
            for (int i = 0; i < items.Length; ++i)
                items[i] = readAction(this);
            return items;
        }
    }
}
