using ezvm.Core.Programming.DTO;
using ezvm.Core.Programming.DTO.Operations;
using ezvm.Core.VM;
using System;
using System.Linq;

namespace ezvm.Core.Programming.Operations
{
    public enum OperationCode : byte
    {
        SetVariable = 0,
        MapValue,
        Push,
        Call,
        Jump,
        Return,
        Add,
        Subtract,
        Multiply,
        Divide,
        Modulo,
        And,
        Or,
        Xor,
        ShiftLeft,
        ShiftRight,
        Compare,
        If,
        Print,
        BufferFree,
        BufferAllocate,
        BufferRead,
        BufferWrite,
        BufferGetLength,
        DebugDump,
        DebugBreak
    }

    public interface IOperation
    {
        OperationCode Id { get; }
        void Process(VirtualMachine vm, BaseOperationDto data);
    }

    public class Operations
    {
        public static Operations Instance
        {
            get
            {
                if (instance == null)
                    instance = new Operations();
                return instance;
            }
        }
        private static Operations instance;

        private readonly IOperation[] ops;
        public IOperation this[OperationCode id] { get => ops.First(op=>op.Id == id); }

        private Operations()
        {
            var type = typeof(IOperation);
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => type.IsAssignableFrom(p))
                .Where(p=>!p.IsAbstract && !p.IsInterface);
            ops = types
                .Select(t => Activator.CreateInstance(t))
                .Cast<IOperation>()
                .ToArray();
        }
    }
}
