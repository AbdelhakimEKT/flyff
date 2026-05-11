using System;
using System.IO;
using System.Text;

namespace Hellion.Core.Network
{
    public class NetPacketBase : IDisposable
    {
        protected MemoryStream memoryStream;
        protected BinaryReader memoryReader = null!;
        protected BinaryWriter memoryWriter = null!;
        private bool disposed;

        public virtual byte[] Buffer => this.memoryStream.ToArray();

        public long Position
        {
            get => this.memoryStream.Position;
            set => this.memoryStream.Position = value;
        }

        public long Size => this.memoryStream.Length;

        public NetPacketBase()
        {
            this.memoryStream = new MemoryStream();
            this.memoryWriter = new BinaryWriter(this.memoryStream);
        }

        public NetPacketBase(byte[] buffer)
        {
            this.memoryStream = new MemoryStream(buffer);
            this.memoryReader = new BinaryReader(this.memoryStream);
        }

        public byte[] GetBuffer() => this.memoryStream.ToArray();

        public virtual void Write<T>(T value)
        {
            if (this.memoryWriter == null)
                throw new InvalidOperationException("Cannot write on a read-only packet.");

            switch (value)
            {
                case byte b: this.memoryWriter.Write(b); break;
                case sbyte sb: this.memoryWriter.Write(sb); break;
                case bool bo: this.memoryWriter.Write(bo); break;
                case char c: this.memoryWriter.Write(c); break;
                case short s: this.memoryWriter.Write(s); break;
                case ushort us: this.memoryWriter.Write(us); break;
                case int i: this.memoryWriter.Write(i); break;
                case uint ui: this.memoryWriter.Write(ui); break;
                case long l: this.memoryWriter.Write(l); break;
                case ulong ul: this.memoryWriter.Write(ul); break;
                case float f: this.memoryWriter.Write(f); break;
                case double d: this.memoryWriter.Write(d); break;
                case decimal de: this.memoryWriter.Write(de); break;
                case string str: this.memoryWriter.Write(str ?? string.Empty); break;
                case byte[] arr: this.memoryWriter.Write(arr); break;
                case null: throw new ArgumentNullException(nameof(value));
                default:
                    throw new NotSupportedException($"Unsupported type for NetPacketBase.Write: {typeof(T).FullName}");
            }
        }

        public virtual T Read<T>()
        {
            if (this.memoryReader == null)
                throw new InvalidOperationException("Cannot read on a write-only packet.");

            Type type = typeof(T);

            object value;
            if (type == typeof(byte)) value = this.memoryReader.ReadByte();
            else if (type == typeof(sbyte)) value = this.memoryReader.ReadSByte();
            else if (type == typeof(bool)) value = this.memoryReader.ReadBoolean();
            else if (type == typeof(char)) value = this.memoryReader.ReadChar();
            else if (type == typeof(short)) value = this.memoryReader.ReadInt16();
            else if (type == typeof(ushort)) value = this.memoryReader.ReadUInt16();
            else if (type == typeof(int)) value = this.memoryReader.ReadInt32();
            else if (type == typeof(uint)) value = this.memoryReader.ReadUInt32();
            else if (type == typeof(long)) value = this.memoryReader.ReadInt64();
            else if (type == typeof(ulong)) value = this.memoryReader.ReadUInt64();
            else if (type == typeof(float)) value = this.memoryReader.ReadSingle();
            else if (type == typeof(double)) value = this.memoryReader.ReadDouble();
            else if (type == typeof(decimal)) value = this.memoryReader.ReadDecimal();
            else if (type == typeof(string)) value = this.memoryReader.ReadString();
            else throw new NotSupportedException($"Unsupported type for NetPacketBase.Read: {type.FullName}");

            return (T)value;
        }

        public void Dispose() => this.Dispose(true);

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed) return;
            if (disposing)
            {
                this.memoryReader?.Dispose();
                this.memoryWriter?.Dispose();
                this.memoryStream?.Dispose();
            }
            this.disposed = true;
        }
    }
}
