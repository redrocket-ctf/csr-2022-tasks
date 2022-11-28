using Newtonsoft.Json;

namespace ezvm.Core.Programming.DTO
{
    public class ProgramDto
    {
        [JsonProperty(PropertyName = "name", Required = Required.Always)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "methods", Required = Required.Always)]
        public MethodDto[] Methods { get; set; } = System.Array.Empty<MethodDto>();

        public void Validate()
        {
            if (string.IsNullOrEmpty(Name))
                throw new ValidationException("Program name unspecified", this);

            foreach (var m in Methods)
                m.Validate(this);
        }
    }
}
