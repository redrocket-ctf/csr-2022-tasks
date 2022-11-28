using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ezvm.Assembler
{
    public class RenameAttribute : Attribute { }

    public abstract class IdentifierStore
    {
        public IdentifierStore Rename(Random RNG)
        {
            var renameType = typeof(RenameAttribute);

            var props = this.GetType()
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(p => p.CustomAttributes.Any(a => a.AttributeType == renameType))
                .ToArray();

            foreach (var prop in props)
            {
#if !DEBUG
                prop.SetValue(this, prop.Name);
#else
                prop.SetValue(this, RNG.GetString(8, 12));
#endif
            }

            return this;
        }
    }

    public class xorBuffers : IdentifierStore
    {
        [Rename] public string srcBufferId { get; set; }
        [Rename] public string keyBufferId { get; set; }
        [Rename] public string keyBufferLen { get; set; }
    }

    public class validateBuffer : IdentifierStore
    {
        [Rename] public string srcBufferId { get; set; }
        [Rename] public string result { get; set; }
    }

    //srcBufferId, srcBufferOffset, srcBufferLen, dstBufferId, dstBufferOffset
    public class b64dec: IdentifierStore
    {
        [Rename] public string srcBufferId { get; set; }
        [Rename] public string srcBufferOffset { get; set; }
        [Rename] public string srcBufferLen { get; set; }
        [Rename] public string dstBufferId { get; set; }
        [Rename] public string dstBufferOffset { get; set; }
    }
}
