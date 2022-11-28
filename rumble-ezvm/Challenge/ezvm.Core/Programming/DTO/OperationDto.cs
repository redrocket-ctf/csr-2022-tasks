using ezvm.Core.Programming.Operations;
using ezvm.Core.VM;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using System.Linq;
using static ezvm.Core.VM.VirtualMachine;
namespace ezvm.Core.Programming.DTO.Operations
{
    public class BaseOperationDto
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName="code", Required = Required.Always)]
        public OperationCode OpCode { get; set; }

        [JsonProperty(PropertyName = "setvar")]
        public SetVarDto SetVar { get; set; }

        [JsonProperty(PropertyName = "push")]
        public PushDto Push { get; set; }

        [JsonProperty(PropertyName = "call")]
        public string Call { get; set; }

        [JsonProperty(PropertyName = "jmp")]
        public string Jump { get; set; }

        [JsonProperty(PropertyName = "calc")]
        public ArithmeticDto Arithmetic { get; set; }

        [JsonProperty(PropertyName = "cmp")]
        public CompareDto Compare { get; set; }

        [JsonProperty(PropertyName = "if")]
        public IfDto If { get; set; }

        [JsonProperty(PropertyName = "map")]
        public MapValueDto MapValue { get; set; }

        [JsonProperty(PropertyName = "print")]
        public PrintDto Print { get; set; }

        [JsonProperty(PropertyName = "b_info")]
        public BufferInfoDto BufferInfo{ get; set; }

        [JsonProperty(PropertyName = "b_alloc")]
        public BufferAllocateDto BufferAllocate { get; set; }

        [JsonProperty(PropertyName = "b_getlen")]
        public BufferGetLengthDto BufferGetLength { get; set; }

        [JsonProperty(PropertyName = "b_read")]
        public BufferReadDto BufferRead { get; set; }

        [JsonProperty(PropertyName = "b_write")]
        public BufferWriteDto BufferWrite { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "dbg")]
        public DebugScope? DebugScope { get; set; }

        public override string ToString()
        {
            return OpCode.ToString();
        }

        public void Validate(ProgramDto program, MethodDto method)
        {
            switch (OpCode)
            {
                case OperationCode.Add:
                case OperationCode.And:
                case OperationCode.Divide:
                case OperationCode.Modulo:
                case OperationCode.Multiply:
                case OperationCode.Or:
                case OperationCode.Subtract:
                case OperationCode.Xor:
                    Arithmetic.Validate(program, method, this);
                    break;
                case OperationCode.BufferAllocate:
                    BufferAllocate.Validate(program, method, this);
                    break;
                case OperationCode.BufferFree:
                    BufferInfo.Validate(program, method, this);
                    break;
                case OperationCode.BufferGetLength:
                    BufferGetLength.Validate(program, method, this);
                    break;
                case OperationCode.BufferRead:
                    BufferRead.Validate(program, method, this);
                    break;
                case OperationCode.BufferWrite:
                    BufferWrite.Validate(program, method, this);
                    break;
                case OperationCode.Call:
                    if (string.IsNullOrEmpty(Call))
                        throw new ValidationException("Missing Call variable", program, method, this);
                    if (!program.Methods.Any(method=>method.Name == Call))
                        throw new ValidationException($"No method \"{Call}\" in program", program, method, this);
                    break;
                case OperationCode.Compare:
                    Compare.Validate(program, method, this);
                    break;
                case OperationCode.If:
                    If.Validate(program, method, this);
                    break;
                case OperationCode.Jump:
                    if (string.IsNullOrEmpty(Jump))
                        throw new ValidationException("Missing Jump variable", program, method, this);
                    if (!method.Markers.ContainsKey(Jump))
                        throw new ValidationException($"No marker \"{Jump}\" in method", program, method, this);
                    break;
                case OperationCode.MapValue:
                    MapValue.Validate(program, method, this);
                    break;
                case OperationCode.Print:
                    Print.Validate(program, method, this);
                    break;
                case OperationCode.Push:
                    Push.Validate(program, method, this);
                    break;
                case OperationCode.SetVariable:
                    SetVar.Validate(program, method, this);
                    break;
            }
        }
    }
    public class ValidatableDto
    {
        public virtual void Validate(ProgramDto program, MethodDto method, BaseOperationDto op) { }
    }
    public class SetVarDto : ValidatableDto
    {
        [JsonProperty(PropertyName = "src", Required = Required.Always)]
        public OperatorDto Source { get; set; }

        [JsonProperty(PropertyName = "dst", Required = Required.Always)]
        public string Destination { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            if (string.IsNullOrEmpty(Destination))
                throw new ValidationException("Missing Destination variable", program, method, op);

            if (!method.HasLocalOrParameter(Destination))
                throw new ValidationException($"Missing local or parameter \"{Destination}\" in method", program, method, op);

            Source.Validate(program, method, op);
        }
    }
    public enum PushType
    {
        ByRef,
        ByVal
    }
    public class PushDto : ValidatableDto
    {
        [JsonProperty(PropertyName = "val")]
        public OperatorDto Value { get; set; }

        [JsonProperty(PropertyName = "param", Required = Required.Always)]
        public string Parameter { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        public PushType Type { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            if (string.IsNullOrEmpty(Parameter))
                throw new ValidationException("Missing parameter variable", program, method, op);

            Value.Validate(program, method, op);
        }
    }
    public class OperatorDto : ValidatableDto
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "src", Required = Required.Always)]
        public ValueSource Source { get; set; }

        [JsonProperty(PropertyName = "const")]
        public long? Constant { get; set; }

        [JsonProperty(PropertyName = "var")]
        public string Variable { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            if (Source == ValueSource.Variable && string.IsNullOrEmpty(Variable))
                throw new ValidationException($"Missing Variable value", program, method, op);

            if (Source == ValueSource.Constant && Constant == null)
                throw new ValidationException($"Missing Constant value", program, method, op);

            if (Source == ValueSource.Variable && !method.HasLocalOrParameter(Variable))
                throw new ValidationException($"Missing local or parameter \"{Variable}\" in method", program, method, op);
        }
    }
    public class ArithmeticDto : ValidatableDto
    {
        [JsonProperty(PropertyName = "op_a", Required = Required.Always)]
        public OperatorDto OperatorA { get; set; }

        [JsonProperty(PropertyName = "op_b", Required = Required.Always)]
        public OperatorDto OperatorB { get; set; }

        [JsonProperty(PropertyName = "dst", Required = Required.Always)]
        public string Destination { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            OperatorA.Validate(program, method, op);
            OperatorB.Validate(program, method, op);


            if (string.IsNullOrEmpty(Destination))
                throw new ValidationException($"Missing Destination value", program, method, op);

            if (!method.HasLocalOrParameter(Destination))
                throw new ValidationException($"Missing local or parameter \"{Destination}\" in method", program, method, op);
        }
    }
    public enum Comparator
    {
        Equal,
        NotEqual,
        Greater,
        Lesser,
        GreaterEqual,
        LesserEqual
    }
    public class CompareDto : ValidatableDto
    {
        [JsonProperty(PropertyName = "op_a", Required = Required.Always)]
        public OperatorDto OperatorA { get; set; }

        [JsonProperty(PropertyName = "op_b", Required = Required.Always)]
        public OperatorDto OperatorB { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "cmp", Required = Required.Always)]
        public Comparator Comparator { get; set; }

        [JsonProperty(PropertyName = "dst", Required = Required.Always)]
        public string Destination { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            base.Validate(program, method, op);

            OperatorA.Validate(program, method, op);
            OperatorB.Validate(program, method, op);

            if (string.IsNullOrEmpty(Destination))
                throw new ValidationException("Missing Destination variable", program, method, op);

            if (!method.HasLocalOrParameter(Destination))
                throw new ValidationException($"Missing local or parameter \"{Destination}\" in method", program, method, op);
        }
    }
    public class IfDto : ValidatableDto
    {
        [JsonProperty(PropertyName = "var", Required = Required.Always)]
        public string Variable { get; set; }

        [JsonProperty(PropertyName = "mtrue", Required = Required.Always)]
        public string MarkerTrue { get; set; }

        [JsonProperty(PropertyName = "mfalse", Required = Required.Always)]
        public string MarkerFalse { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            base.Validate(program, method, op);

            if (string.IsNullOrEmpty(Variable))
                throw new ValidationException("Missing Variable variable", program, method, op);

            if (!method.HasLocalOrParameter(Variable))
                throw new ValidationException($"Missing local or parameter \"{Variable}\" in method", program, method, op);

            if (string.IsNullOrEmpty(MarkerTrue) || string.IsNullOrEmpty(MarkerFalse))
                throw new ValidationException("Missing MarkerTrue or MarkerFalse variable", program, method, op);

            if (!method.Markers.ContainsKey(MarkerTrue))
                throw new ValidationException($"Method is missing \"{MarkerTrue}\" marker", program, method, op);

            if (!method.Markers.ContainsKey(MarkerFalse))
                throw new ValidationException($"Method is missing \"{MarkerFalse}\" marker", program, method, op);
        }
    }
    public class MapValueDto : ValidatableDto
    {
        [JsonProperty(PropertyName = "src", Required = Required.Always)]
        public OperatorDto Source { get; set; }

        [JsonProperty(PropertyName = "dst", Required = Required.Always)]
        public string Destination { get; set; }

        [JsonProperty(PropertyName = "vals", Required = Required.Always)]
        public Dictionary<long, OperatorDto> Values { get; set; } = new Dictionary<long, OperatorDto>();
       
        [JsonProperty(PropertyName = "def", Required = Required.Always)]
        public OperatorDto Default { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            base.Validate(program, method, op);

            Source.Validate(program, method, op);
            Default.Validate(program, method, op);

            if (string.IsNullOrEmpty(Destination))
                throw new ValidationException("Missing Destination variable", program, method, op);

            if (!method.HasLocalOrParameter(Destination))
                throw new ValidationException($"Missing local or parameter \"{Destination}\" in method", program, method, op);
        }
    }
    public enum PrintSource
    {
        Variable,
        Constant,
        BufferString,
    }
    public enum PrintType
    {
        Char,
        IntDec,
        IntHex2,
        IntHex4,
        IntHex8,
        IntHex16
    }
    public enum BufferStringLength
    {
        NullTerminated,
        FixedDynamic
    }
    public class PrintDto : ValidatableDto
    {
        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "src", Required = Required.Always)]
        public PrintSource Source { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        public PrintType Type { get; set; }

        [JsonProperty(PropertyName = "const")]
        public string Constant { get; set; }

        [JsonProperty(PropertyName = "var")]
        public string Variable { get; set; }

        [JsonProperty(PropertyName = "b_src")]
        public OperatorDto BufferString { get; set; }

        [JsonProperty(PropertyName = "b_strat")]
        public BufferStringLength BufferPrintStrategy { get; set; }

        [JsonProperty(PropertyName = "b_len")]
        public OperatorDto BufferPrintLength { get; set; }

        [JsonProperty(PropertyName = "b_offset")]
        public OperatorDto BufferPrintOffset { get; set; }

        [JsonProperty(PropertyName = "nl")]
        public bool AppendNewLine { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            base.Validate(program, method, op);

            if (Source == PrintSource.Constant && string.IsNullOrEmpty(Constant))
                throw new ValidationException("Missing Constant variable", program, method, op);

            if (Source == PrintSource.Variable && string.IsNullOrEmpty(Variable))
                throw new ValidationException("Missing Variable variable", program, method, op);

            if (Source == PrintSource.BufferString)
            {
                if (op.Print.BufferString == null)
                    throw new ValidationException("Missing buffer string parameter", program, method, op);
                op.Print.BufferString.Validate(program, method, op);

                if (BufferPrintOffset == null)
                    throw new ValidationException("Missing BufferPrintOffset operator", program, method, op);
                BufferPrintOffset.Validate(program, method, op);

                switch (op.Print.BufferPrintStrategy)
                {
                    case BufferStringLength.FixedDynamic:
                        if (BufferPrintLength == null)
                            throw new ValidationException("Missing BufferPrintLengthFix operator", program, method, op);
                        BufferPrintLength.Validate(program, method, op);
                        break;
                }
            }
            if (Source == PrintSource.Variable && !method.HasLocalOrParameter(Variable))
                throw new ValidationException($"Missing local or parameter \"{Variable}\" in method", program, method, op);
        }
    }
    public class BufferAllocateDto : ValidatableDto
    {
        [JsonProperty(PropertyName = "len", Required = Required.Always)]
        public OperatorDto Length { get; set; }

        [JsonProperty(PropertyName = "dst", Required = Required.Always)]
        public string Destination { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            Length.Validate(program, method, op);
            if (string.IsNullOrEmpty(Destination))
                throw new ValidationException("Missing Destination variable", program, method, op);

            if (!method.HasLocalOrParameter(Destination))
                throw new ValidationException($"Missing local or parameter \"{Destination}\" in method", program, method, op);
        }
    }
    public class BufferInfoDto : ValidatableDto
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public OperatorDto Id { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            Id.Validate(program, method, op);
        }
    }
    public class BufferGetLengthDto : BufferInfoDto
    {
        [JsonProperty(PropertyName = "dst", Required = Required.Always)]
        public string Destination { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            base.Validate(program, method, op);
            if (string.IsNullOrEmpty(Destination))
                throw new ValidationException("Missing Destination variable", program, method, op);

            if (!method.HasLocalOrParameter(Destination))
                throw new ValidationException($"Missing local or parameter \"{Destination}\" in method", program, method, op);
        }

    }
    public class BufferAddressOperationDto : BufferInfoDto
    {
        [JsonProperty(PropertyName = "offs", Required = Required.Always)]
        public OperatorDto Offset { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [JsonProperty(PropertyName = "size", Required = Required.Always)]
        public Buffer.Size Size { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            base.Validate(program, method, op);
            Offset.Validate(program, method, op);
        }
    }
    public class BufferReadDto : BufferAddressOperationDto
    {
        [JsonProperty(PropertyName = "dst", Required = Required.Always)]
        public string Destination { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            base.Validate(program, method, op);
            if (string.IsNullOrEmpty(Destination))
                throw new ValidationException("Missing Destination variable", program, method, op);

            if (!method.HasLocalOrParameter(Destination))
                throw new ValidationException($"Missing local or parameter \"{Destination}\" in method", program, method, op);
        }
    }

    public class BufferWriteDto : BufferAddressOperationDto
    {
        [JsonProperty(PropertyName = "val", Required = Required.Always)]
        public OperatorDto Value { get; set; }

        public override void Validate(ProgramDto program, MethodDto method, BaseOperationDto op)
        {
            base.Validate(program, method, op);
            Value.Validate(program, method, op);
        }
    }

    public enum DebugScope
    {
        Full,
        CurrentMethod
    }
}
