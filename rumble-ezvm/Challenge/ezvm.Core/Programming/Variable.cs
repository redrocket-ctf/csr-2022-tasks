using ezvm.Core.VM;
using System;
using System.Linq;
using System.Text;

namespace ezvm.Core.Programming
{
    public class Variable
    {
        public string Name { get; private set; }
        public long Value
        {
            get => Ref?.Value ?? value;
            set
            {
                if (Ref != null) Ref.Value = value;
                else this.value = value;
            }
        }
        public Variable Ref { get; private set; }

        private long value;

        public Variable(string name, Variable referenceVar)
        {
            Name = name;
            Ref = referenceVar;
        }
        public Variable(string name, long value = 0)
        {
            Name = name;
            this.value = value;
        }

        public override string ToString()
        {
            var chr = (char)Value;
            var str = new string(Encoding.ASCII.GetString(VM.Buffer.EnsureLittleEndian(BitConverter.GetBytes(Value)))
                .Select(c => Char.IsControl(c) ? '.' : c)
                .ToArray());

            var displays = new string[]
            {
                Value.ToString(),
                $"0x{Value:X}",
                char.IsControl(chr) ? null : $"'{chr}'",
                string.IsNullOrEmpty(str) ? null : $"\"{str}\""
            };
            var values = string.Join("/", displays
                .Where(d => d != null)
                .ToArray());

            return $"{Name}={values}";
        }
    }
}
