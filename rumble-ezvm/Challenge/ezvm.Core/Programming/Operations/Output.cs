using ezvm.Core.Programming.DTO.Operations;
using ezvm.Core.VM;
using System;
using System.Text;

namespace ezvm.Core.Programming.Operations
{
    class Output : IOperation
    {
        public OperationCode Id => OperationCode.Print;

        public void Process(VirtualMachine vm, BaseOperationDto data)
        {
            switch (data.Print.Source)
            {
                case PrintSource.Constant:
                    Console.Write(data.Print.Constant);
                    break;
                case PrintSource.Variable:
                    var srcVar = vm.Stack.Last[data.Print.Variable];
                    switch (data.Print.Type) {
                        case PrintType.Char:
                            Console.Write((char)srcVar.Value);
                            break;
                        case PrintType.IntDec:
                            Console.Write(srcVar.Value.ToString());
                            break;
                        case PrintType.IntHex2:
                            Console.Write(srcVar.Value.ToString("X2"));
                            break;
                        case PrintType.IntHex4:
                            Console.Write(srcVar.Value.ToString("X4"));
                            break;
                        case PrintType.IntHex8:
                            Console.Write(srcVar.Value.ToString("X8"));
                            break;
                        case PrintType.IntHex16:
                            Console.Write(srcVar.Value.ToString("X16"));
                            break;
                    }
                    break;
                case PrintSource.BufferString:
                    var buffId = vm.ResolveValue(data.Print.BufferString);
                    var buff = vm.Buffers[buffId];
                    var offset = vm.ResolveValue(data.Print.BufferPrintOffset);
                    var slice = buff.GetSlice((int)offset, (int)buff.Length - (int)offset);
                    var len = data.Print.BufferPrintStrategy switch {
                        BufferStringLength.NullTerminated => slice.IndexOf((byte)0),
                        BufferStringLength.FixedDynamic => vm.ResolveValue(data.Print.BufferPrintLength),
                        _ => throw new Exception()
                    };
                    if (len == -1) len = slice.Length;
                    Console.WriteLine(Encoding.ASCII.GetString(buff.GetSlice((int)offset, (int)len)));
                    break;
            }
            if (data.Print.AppendNewLine)
                Console.WriteLine();
        }
    }
}
