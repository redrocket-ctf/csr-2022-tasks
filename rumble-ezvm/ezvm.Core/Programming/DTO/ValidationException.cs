using ezvm.Core.Programming.DTO.Operations;
using System;

namespace ezvm.Core.Programming.DTO
{
    public class ValidationException : Exception
    {
        public ProgramDto Program { get; set; }
        public MethodDto Method { get; set; }
        public BaseOperationDto Operation { get; set; }

        public ValidationException(string message, ProgramDto program, MethodDto method = null, BaseOperationDto operation = null) : base(message)
        {
            Program = program;
            Method = method;
            Operation = operation;
        }
    }
}