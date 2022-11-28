using System.Collections.Generic;
using System.Linq;

namespace ezvm.Core.VM
{
    public class BufferAllocator
    {
        private readonly List<Buffer> buffers;
        private long id;

        public Buffer this[long id] => buffers.First(mem => mem.Id == id);

        public BufferAllocator()
        {
            buffers = new List<Buffer>();
        }

        public Buffer Allocate(long length)
        {
            var buff = new Buffer(id++, length);
            buffers.Add(buff);
            return buff;
        }

        public Buffer Allocate(byte[] data)
        {
            var buff = new Buffer(id++, data);
            buffers.Add(buff);
            return buff;
        }

        public void Free(Buffer buff)
        {
            buffers.Remove(buff);
        }
    }
}
