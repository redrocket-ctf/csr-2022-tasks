using System;

namespace ezvm.Core.VM
{
    public class Buffer
    {
        public enum Size
        {
            i8 = 1,
            i16 = 2,
            i32 = 4,
            i64 = 8
        }
        public long Id { get; private set; }
        public long Length => data.Length;
        private readonly byte[] data;

        public Buffer(long id, byte[] data)
        {
            Id = id;
            this.data = new byte[data.Length];
            data.CopyTo(this.data, 0);
        }
        public Buffer(long id, long length)
        {
            Id = id;
            data = new byte[length];
        }

        public static byte[] EnsureLittleEndian(byte[] data)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(data);
            
            return data;
        }

        public long Read(long offset, Size size)
        {
            byte[] odata = null;
            if (size > Size.i8)
            {
                odata = new byte[(int)size];
                Array.Copy(data, (int)offset, odata, 0, odata.Length);
                odata = EnsureLittleEndian(odata);
            }

            return size switch
            {
                Size.i8 => data[offset],
                Size.i16 => BitConverter.ToInt16(odata),
                Size.i32 => BitConverter.ToInt32(odata),
                Size.i64 => BitConverter.ToInt64(odata),
                _ => throw new Exception("Invalid size"),
            };
        }

        public int IndexOf(byte b, int def = -1)
        {
            for (int i = 0; i < Length; i++)
                if (data[i] == b)
                    return i;

            return def;
        }

        public ReadOnlySpan<byte> GetSlice(int offset, int length)
        {
            return new ReadOnlySpan<byte>(data, offset, length);
        }

        public void Write(long offset, long value, Size size)
        {
            switch (size)
            {
                case Size.i8:
                    data[offset] = (byte)value;
                    break;
                case Size.i16:
                    Array.Copy(EnsureLittleEndian(BitConverter.GetBytes((short)value)), 0, data, offset, (int)size);
                    break;
                case Size.i32:
                    Array.Copy(EnsureLittleEndian(BitConverter.GetBytes((int)value)), 0, data, offset, (int)size);
                    break;
                case Size.i64:
                    Array.Copy(EnsureLittleEndian(BitConverter.GetBytes(value)), 0, data, offset, (int)size);
                    break;
            }
        }
    }
}
