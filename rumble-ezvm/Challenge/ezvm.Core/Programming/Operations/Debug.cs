using ezvm.Core.Programming.DTO.Operations;
using ezvm.Core.VM;
using System;
using System.Diagnostics;

namespace ezvm.Core.Programming.Operations
{
    class DebugDump : IOperation
    {
        public OperationCode Id => OperationCode.DebugDump;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            var scope = data.DebugScope ?? DebugScope.CurrentMethod;
            switch (scope)
            {
                case DebugScope.Full:
                    Console.WriteLine(vm.Stack.Dump());
                    break;
                case DebugScope.CurrentMethod:
                    Console.WriteLine(vm.Stack.Last.Dump("", false));
                    break;
            }
        }
    }
    class DebugBreak : IOperation
    {
        public OperationCode Id => OperationCode.DebugBreak;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            var locals = vm?.Stack?.Last?.Locals;
            if (Debugger.IsAttached)
                Debugger.Break();
        }
    }
}
