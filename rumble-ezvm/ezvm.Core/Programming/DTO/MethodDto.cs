using ezvm.Core.Programming.DTO.Operations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ezvm.Core.Programming.DTO
{
    public class MethodDto
    {
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "params")]
        public string[] Parameters { get; set; } = Array.Empty<string>();

        [JsonProperty(PropertyName = "locals")]
        public string[] Locals { get; set; } = Array.Empty<string>();

        [JsonProperty(PropertyName = "markers")]
        public Dictionary<string, int> Markers { get; set; } = new Dictionary<string, int>();

        [JsonProperty(PropertyName = "ops", Required = Required.Always)]
        public BaseOperationDto[] Operations { get; set; } = Array.Empty<BaseOperationDto>();

        public override string ToString()
        {
            return $"[{Name} Params({string.Join(", ", Parameters)}), Locals({string.Join(", ", Locals)})]";
        }

        public bool HasLocalOrParameter(string name) {
            return !string.IsNullOrEmpty(name) && Locals.Concat(Parameters).Contains(name);
        }

        public void Validate(ProgramDto program)
        {
            if (string.IsNullOrEmpty(Name))
                throw new ValidationException("Method name unspecified", program, this);

            foreach (var op in Operations)
                op.Validate(program, this);
        }
    }
}
