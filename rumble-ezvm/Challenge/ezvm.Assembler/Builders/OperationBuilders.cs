using ezvm.Core.Programming.DTO.Operations;
using ezvm.Core.Programming.Operations;
using ezvm.Core.VM;
using System;
using System.Linq;

namespace ezvm.Assembler.Builders
{
    public class OperatorBuilder<R> : NestedBuilder<OperatorDto, R>
    {
        public OperatorBuilder(R parentBuilder, Action<OperatorDto> callback) : base(parentBuilder, callback) { }

        public OperatorBuilder<R> SetConstant(long constant)
        {
            Item.Source = Core.VM.VirtualMachine.ValueSource.Constant;
            Item.Constant = constant;
            return this;
        }

        public OperatorBuilder<R> SetVariable(string variable)
        {
            Item.Source = Core.VM.VirtualMachine.ValueSource.Variable;
            Item.Variable = variable;
            return this;
        }
    }

    public class OpSetVariableBuilder : NestedBuilder<SetVarDto, MethodBuilder>
    {
        public OpSetVariableBuilder(MethodBuilder parentBuilder, Action<SetVarDto> callback, string dstName) : base(parentBuilder, callback)
        {
            Item.Destination = dstName;
        }

        public OperatorBuilder<OpSetVariableBuilder> SetValue()
        {
            return new OperatorBuilder<OpSetVariableBuilder>(this, (op) => Item.Source = op);
        }
    }

    public class KeyValue
    {
        public long Key { get; set; }
        public OperatorDto Value { get; set; }
    }

    public class KeyValueBuilder : NestedBuilder<KeyValue, OpMapValueBuilder>
    {
        public KeyValueBuilder(OpMapValueBuilder parentBuilder, Action<KeyValue> callback, long key) : base(parentBuilder, callback)
        {
            Item.Key = key;
        }

        public OperatorBuilder<KeyValueBuilder> SetValue()
        {
            return new OperatorBuilder<KeyValueBuilder>(this, (op) => Item.Value = op);
        }
    }

    public class OpMapValueBuilder : NestedBuilder<MapValueDto, MethodBuilder>
    {
        public OpMapValueBuilder(MethodBuilder parentBuilder, Action<MapValueDto> callback, string dstName) : base(parentBuilder, callback)
        {
            Item.Destination = dstName;
        }

        public OperatorBuilder<OpMapValueBuilder> SetSource()
        {
            return new OperatorBuilder<OpMapValueBuilder>(this, (op) => Item.Source = op);
        }
        public OperatorBuilder<OpMapValueBuilder> SetDefault()
        {
            return new OperatorBuilder<OpMapValueBuilder>(this, (op) => Item.Default = op);
        }
        public KeyValueBuilder AddKeyValue(long key)
        {
            return new KeyValueBuilder(this, (kv) => Item.Values[kv.Key] = kv.Value, key);
        }
    }

    public class OpPrintBuilder : NestedBuilder<PrintDto, MethodBuilder>
    {
        public OpPrintBuilder(MethodBuilder parentBuilder, Action<PrintDto> callback, PrintType type) : base(parentBuilder, callback) 
        {
            Item.Type = type;
        }

        public OpPrintBuilder SetConstant(string constant)
        {
            Item.Source = PrintSource.Constant;
            Item.Constant = constant;
            return this;
        }
        public OpPrintBuilder SetVariable(string srcVariable)
        {
            Item.Source = PrintSource.Variable;
            Item.Variable = srcVariable;
            return this;
        }
        public OpPrintBuilder SetAppendNewLine(bool append = true)
        {
            Item.AppendNewLine = append;
            return this;
        }
        public OpPrintBuilder SetBufferString(string buffVar)
        {
            Item.Source = PrintSource.BufferString;
            Item.BufferString = new OperatorDto()
            {
                Source = VirtualMachine.ValueSource.Variable,
                Variable = buffVar
            };
            return this;
        }
        public OpPrintBuilder SetBufferString(long buffConst)
        {
            Item.Source = PrintSource.BufferString;
            Item.BufferString = new OperatorDto()
            {
                Source = VirtualMachine.ValueSource.Constant,
                Constant = buffConst
            };
            return this;
        }
        public OpPrintBuilder SetBufferLength(long constA)
        {
            Item.BufferPrintStrategy = BufferStringLength.FixedDynamic;
            Item.BufferPrintLength = new OperatorDto()
            {
                Source = VirtualMachine.ValueSource.Constant,
                Constant = constA
            };
            return this;
        }
        public OpPrintBuilder SetBufferLength(string varA)
        {
            Item.BufferPrintStrategy = BufferStringLength.FixedDynamic;
            Item.BufferPrintLength = new OperatorDto()
            {
                Source = VirtualMachine.ValueSource.Variable,
                Variable = varA
            };
            return this;
        }
        public OpPrintBuilder SetBufferOffset(long constA)
        {
            Item.BufferPrintOffset = new OperatorDto()
            {
                Source = VirtualMachine.ValueSource.Constant,
                Constant = constA
            };
            return this;
        }
        public OpPrintBuilder SetBufferOffset(string varA)
        {
            Item.BufferPrintOffset = new OperatorDto()
            {
                Source = VirtualMachine.ValueSource.Variable,
                Variable = varA
            };
            return this;
        }

        public OpPrintBuilder SetBufferNullterminated()
        {
            Item.BufferPrintStrategy = BufferStringLength.NullTerminated;
            return this;
        }
    }

    public enum ArithmeticOperator
    {
        Add = OperationCode.Add,
        Subtract = OperationCode.Subtract,
        Multiply = OperationCode.Multiply,
        Divide = OperationCode.Divide,
        Modulo = OperationCode.Modulo,
        And = OperationCode.And,
        Or = OperationCode.Or,
        Xor = OperationCode.Xor,
        ShiftLeft = OperationCode.ShiftLeft,
        ShiftRight = OperationCode.ShiftRight,
    }
    public class OpArithmeticBuilder : NestedBuilder<ArithmeticDto, MethodBuilder>
    {
        public OpArithmeticBuilder(MethodBuilder parentBuilder, Action<ArithmeticDto> callback, string destVar) : base(parentBuilder, callback)
        {
            Item.Destination = destVar;
        }

        public OperatorBuilder<OpArithmeticBuilder> SetOperatorA()
        {
            return new OperatorBuilder<OpArithmeticBuilder>(this, (op) => Item.OperatorA = op);
        }
        public OperatorBuilder<OpArithmeticBuilder> SetOperatorB()
        {
            return new OperatorBuilder<OpArithmeticBuilder>(this, (op) => Item.OperatorB = op);
        }
    }
    public class OpPushBuilder : NestedBuilder<PushDto, MethodBuilder>
    {
        public OpPushBuilder(MethodBuilder parentBuilder, Action<PushDto> callback, PushType type, string parameter) : base(parentBuilder, callback)
        {
            Item.Parameter = parameter;
            Item.Type = type;
        }

        public OperatorBuilder<OpPushBuilder> SetValue()
        {
            return new OperatorBuilder<OpPushBuilder>(this, (op) => Item.Value = op);
        }
    }
    public class OpCompareBuilder : NestedBuilder<CompareDto, MethodBuilder>
    {
        public OpCompareBuilder(MethodBuilder parentBuilder, Action<CompareDto> callback, string destVar, Comparator cmp) : base(parentBuilder, callback)
        {
            Item.Destination = destVar;
            Item.Comparator = cmp;
        }

        public OperatorBuilder<OpCompareBuilder> SetOperatorA()
        {
            return new OperatorBuilder<OpCompareBuilder>(this, (op) => Item.OperatorA = op);
        }
        public OperatorBuilder<OpCompareBuilder> SetOperatorB()
        {
            return new OperatorBuilder<OpCompareBuilder>(this, (op) => Item.OperatorB = op);
        }
    }
    public class OpIfBuilder : NestedBuilder<IfDto, MethodBuilder>
    {
        public OpIfBuilder(MethodBuilder parentBuilder, Action<IfDto> callback, string var) : base(parentBuilder, callback)
        {
            Item.Variable = var;
        }

        public OpIfBuilder SetMarkerTrue(string label)
        {
            Item.MarkerTrue = label;
            return this;
        }
        public OpIfBuilder SetMarkerFalse(string label)
        {
            Item.MarkerFalse = label;
            return this;
        }
    }
    public class OpBufferGetLengthBuilder : NestedBuilder<BufferGetLengthDto, MethodBuilder>
    {
        public OpBufferGetLengthBuilder(MethodBuilder parentBuilder, Action<BufferGetLengthDto> callback, string destination) : base(parentBuilder, callback) 
        {
            Item.Destination = destination;
        }
        public OperatorBuilder<OpBufferGetLengthBuilder> SetId()
        {
            return new OperatorBuilder<OpBufferGetLengthBuilder>(this, (op) => Item.Id = op);
        }
    }
    public class OpBufferFreeBuilder : NestedBuilder<BufferInfoDto, MethodBuilder>
    {
        public OpBufferFreeBuilder(MethodBuilder parentBuilder, Action<BufferInfoDto> callback) : base(parentBuilder, callback) { }
        public OperatorBuilder<OpBufferFreeBuilder> SetId()
        {
            return new OperatorBuilder<OpBufferFreeBuilder>(this, (op) => Item.Id = op);
        }
    }
    public class OpBufferAllocateBuilder : NestedBuilder<BufferAllocateDto, MethodBuilder>
    {
        public OpBufferAllocateBuilder(MethodBuilder parentBuilder, Action<BufferAllocateDto> callback, string dstName) : base(parentBuilder, callback)
        {
            Item.Destination = dstName;
        }
        public OperatorBuilder<OpBufferAllocateBuilder> SetLength()
        {
            return new OperatorBuilder<OpBufferAllocateBuilder>(this, (op) => Item.Length = op);
        }
    }

    public class OpBufferAddressBuilder<T> : NestedBuilder<T, MethodBuilder> where T : BufferAddressOperationDto, new()
    {
        public OpBufferAddressBuilder(MethodBuilder parentBuilder, Action<T> callback) : base(parentBuilder, callback)
        {
            Item.Size = Core.VM.Buffer.Size.i8;
        }
        public OpBufferAddressBuilder<T> SetSize(Core.VM.Buffer.Size size)
        {
            Item.Size = size;
            return this;
        }
        public OperatorBuilder<OpBufferAddressBuilder<T>> SetOffset()
        {
            return new OperatorBuilder<OpBufferAddressBuilder<T>>(this, (op) => Item.Offset = op);
        }
        public OperatorBuilder<OpBufferAddressBuilder<T>> SetId()
        {
            return new OperatorBuilder<OpBufferAddressBuilder<T>>(this, (op) => Item.Id = op);
        }
    }

    public class OpBufferReadBuilder : OpBufferAddressBuilder<BufferReadDto>
    {
        public OpBufferReadBuilder(MethodBuilder parentBuilder, Action<BufferReadDto> callback, string dstVar) : base(parentBuilder, callback)
        {
            Item.Destination = dstVar;
        }
    }

    public class OpBufferWriteBuilder : OpBufferAddressBuilder<BufferWriteDto>
    {
        public OpBufferWriteBuilder(MethodBuilder parentBuilder, Action<BufferWriteDto> callback) : base(parentBuilder, callback) { }
        public OperatorBuilder<OpBufferWriteBuilder> SetValue()
        {
            return new OperatorBuilder<OpBufferWriteBuilder>(this, (op) => Item.Value = op);
        }
    }
}
