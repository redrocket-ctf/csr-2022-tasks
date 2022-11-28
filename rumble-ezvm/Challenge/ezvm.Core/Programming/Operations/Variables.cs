using ezvm.Core.Programming.DTO.Operations;
using ezvm.Core.VM;
using System;

namespace ezvm.Core.Programming.Operations
{
    class SetVar : IOperation
    {
        public OperationCode Id => OperationCode.SetVariable;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            var destVar = vm.Stack.Last[data.SetVar.Destination];
            destVar.Value = data.SetVar.Source.Source switch
            {
                VirtualMachine.ValueSource.Constant => data.SetVar.Source.Constant.Value,
                VirtualMachine.ValueSource.Variable => vm.Stack.Last[data.SetVar.Source.Variable].Value,
                _ => throw new Exception("Invalid SetVarDto"),
            };
        }
    }

    class MapValue : IOperation
    {
        public OperationCode Id => OperationCode.MapValue;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            var srcVal = vm.ResolveValue(data.MapValue.Source);
            var dstVar = vm.Stack.Last[data.MapValue.Destination];

            if (data.MapValue.Values.ContainsKey(srcVal))
                dstVar.Value = vm.ResolveValue(data.MapValue.Values[srcVal]);
            else
                dstVar.Value = vm.ResolveValue(data.MapValue.Default);
        }
    }
}
