using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace ZD.Common
{
    public class BinWriter : IDisposable
    {
        private Stream stream;
        private BinaryWriter writer;

        public BinWriter(string fileName)
        {
            stream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite);
            writer = new BinaryWriter(stream);
        }

        public BinWriter(Stream stream)
        {
            this.stream = stream;
            writer = new BinaryWriter(stream);
        }

        public int Position
        {
            get { return (int)stream.Position; }
            set { stream.Position = value; }
        }

        public void Dispose()
        {
            if (writer != null) writer.Dispose();
            if (stream != null) stream.Dispose();
        }

        public void MoveToEnd()
        {
            stream.Seek(0, SeekOrigin.End);
        }

        public void WriteChar(char c)
        {
            writer.Write(c);
        }

        public void WriteLong(long l)
        {
            writer.Write(l);
        }

        public void WriteInt(int i)
        {
            writer.Write(i);
        }

        public void WriteShort(short s)
        {
            writer.Write(s);
        }

        public void WriteByte(byte b)
        {
            writer.Write(b);
        }

        public void WriteBytes(byte[] buf)
        {
            writer.Write(buf);
        }

        public void WriteString(string str)
        {
            if (str.Length > ushort.MaxValue) throw new InvalidOperationException("String exceeds supported length.");
            ushort length = (ushort)str.Length;
            writer.Write(length);
            foreach (char c in str)
            {
                ushort cu = (ushort)c;
                cu = (ushort)(cu ^ 6939);
                writer.Write(cu);
            }
        }

        public void WriteArray<ItemType>(IList<ItemType> items, Action<ItemType, BinWriter> writeAction)
        {
            WriteInt(items.Count);
            foreach (ItemType item in items)
                writeAction(item, this);

        }

        public void WriteArray<ItemType>(IList<ItemType> items)
            where ItemType : IBinSerializable
        {
            WriteArray(items, (item, bwr) => item.Serialize(bwr));
        }
    }
}
