using ezvm.Core.Programming.DTO.Operations;
using ezvm.Core.VM;

namespace ezvm.Core.Programming.Operations
{
    abstract class Arithmetic
    {
        

        protected abstract long Calculate(long a, long b);

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            var destVar = vm.Stack.Last[data.Arithmetic.Destination];
            var valA = vm.ResolveValue(data.Arithmetic.OperatorA);
            var valB = vm.ResolveValue(data.Arithmetic.OperatorB);
            var result = Calculate(valA, valB);
            destVar.Value = result;
        }
    }

    class Add : Arithmetic, IOperation
    {
        public OperationCode Id => OperationCode.Add;
        protected override long Calculate(long a, long b) => a + b;
    }
    class Sub : Arithmetic, IOperation
    {
        public OperationCode Id => OperationCode.Subtract;
        protected override long Calculate(long a, long b) => a - b;
    }
    class Mul : Arithmetic, IOperation
    {
        public OperationCode Id => OperationCode.Multiply;
        protected override long Calculate(long a, long b) => a * b;
    }
    class Div : Arithmetic, IOperation
    {
        public OperationCode Id => OperationCode.Divide;
        protected override long Calculate(long a, long b) => a / b;
    }
    class Mod : Arithmetic, IOperation
    {
        public OperationCode Id => OperationCode.Modulo;
        protected override long Calculate(long a, long b) => a % b;
    }
    class Xor : Arithmetic, IOperation
    {
        public OperationCode Id => OperationCode.Xor;
        protected override long Calculate(long a, long b) => a ^ b;
    }
    class And : Arithmetic, IOperation
    {
        public OperationCode Id => OperationCode.And;
        protected override long Calculate(long a, long b) => a & b;
    }
    class Or : Arithmetic, IOperation
    {
        public OperationCode Id => OperationCode.Or;
        protected override long Calculate(long a, long b) => a | b;
    }
    class ShiftLeft : Arithmetic, IOperation
    {
        public OperationCode Id => OperationCode.ShiftLeft;
        protected override long Calculate(long a, long b) => a << (int)b;
    }
    class ShiftRight: Arithmetic, IOperation
    {
        public OperationCode Id => OperationCode.ShiftRight;
        protected override long Calculate(long a, long b) => a >> (int)b;
    }
}
