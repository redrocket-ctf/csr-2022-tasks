using ezvm.Core.Programming.DTO.Operations;
using ezvm.Core.VM;

namespace ezvm.Core.Programming.Operations
{
    class BufferAllocate : IOperation
    {
        public OperationCode Id => OperationCode.BufferAllocate;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            var length = vm.ResolveValue(data.BufferAllocate.Length);
            var buff = vm.Buffers.Allocate(length);
            vm.Stack.Last[data.BufferAllocate.Destination].Value = buff.Id;
        }
    }

    class BufferFree : IOperation
    {
        public OperationCode Id => OperationCode.BufferFree;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            var id = vm.ResolveValue(data.BufferInfo.Id);
            vm.Buffers.Free(vm.Buffers[id]);
        }
    }

    class BufferGetLength : IOperation
    {
        public OperationCode Id => OperationCode.BufferGetLength;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            var id = vm.ResolveValue(data.BufferGetLength.Id);
            var buff = vm.Buffers[id];
            var dstVar = vm.Stack.Last[data.BufferGetLength.Destination];
            dstVar.Value = buff.Length;
        }
    }

    class BufferRead : IOperation
    {
        public OperationCode Id => OperationCode.BufferRead;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            var id = vm.ResolveValue(data.BufferRead.Id);
            var buff = vm.Buffers[id];
            var offset = vm.ResolveValue(data.BufferRead.Offset);
            var dstVar = vm.Stack.Last[data.BufferRead.Destination];
            dstVar.Value = buff.Read(offset, data.BufferRead.Size);
        }
    }

    class BufferWrite : IOperation
    {
        public OperationCode Id => OperationCode.BufferWrite;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            var id = vm.ResolveValue(data.BufferWrite.Id);
            var buff = vm.Buffers[id];
            var offset = vm.ResolveValue(data.BufferWrite.Offset);
            var value = vm.ResolveValue(data.BufferWrite.Value);
            buff.Write(offset, value, data.BufferWrite.Size);
        }
    }
}
