using ezvm.Core.Programming.DTO.Operations;
using ezvm.Core.VM;
using System;
using System.Linq;

namespace ezvm.Core.Programming.Operations
{
    class Push : IOperation
    {
        public OperationCode Id => OperationCode.Push;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            Variable var;
            if (data.Push.Type == PushType.ByRef)
            {
                var refVar = vm.Stack.Last[data.Push.Value.Variable];
                var = new Variable(data.Push.Parameter, refVar);
                //var = vm.Stack.Last[data.Push.Value.Variable];
            }
            else
            {
                var value = vm.ResolveValue(data.Push.Value);
                var = new Variable(data.Push.Parameter, value);
            }
            vm.Stack.Last.Push(var);
        }
    }

    class Call : IOperation
    {
        public OperationCode Id => OperationCode.Call;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            var destMethod = vm.Program.Methods.First(m => m.Name == data.Call);
            vm.Stack.Last.CallMethod(destMethod);
        }
    }

    class Jump : IOperation
    {
        public OperationCode Id => OperationCode.Jump;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            vm.Stack.Last.OperationIdx = vm.Stack.Last.Method.Markers[data.Jump];
        }
    }
    class Return : IOperation
    {
        public OperationCode Id => OperationCode.Return;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            vm.Stack.Last.ExitMethod();
        }
    }
    class Compare : IOperation
    {
        public OperationCode Id => OperationCode.Compare;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            var valA = vm.ResolveValue(data.Compare.OperatorA);
            var valB = vm.ResolveValue(data.Compare.OperatorB);
            var destVar = vm.Stack.Last[data.Compare.Destination];
            var result = false;

            switch (data.Compare.Comparator)
            {
                case Comparator.Equal:
                    result = valA == valB;
                    break;
                case Comparator.NotEqual:
                    result = valA != valB;
                    break;
                case Comparator.Greater:
                    result = valA > valB;
                    break;
                case Comparator.GreaterEqual:
                    result = valA >= valB;
                    break;
                case Comparator.Lesser:
                    result = valA < valB;
                    break;
                case Comparator.LesserEqual:
                    result = valA <= valB;
                    break;
            }

            destVar.Value = result ? 1 : 0;
        }
    }
    class If : IOperation
    {
        public OperationCode Id => OperationCode.If;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            var srcVariable = vm.Stack.Last[data.If.Variable];
            var markerName = srcVariable.Value > 0 ? data.If.MarkerTrue : data.If.MarkerFalse;
            vm.Stack.Last.OperationIdx = vm.Stack.Last.Method.Markers[markerName];
        }
    }
}
