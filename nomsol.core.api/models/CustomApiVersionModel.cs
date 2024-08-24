using Asp.Versioning;

namespace nomsol.core.api.models
{
    public class CustomApiVersionModel
    {
        public required ApiVersion Version { get; set; }
        public required string VersionPath { get; set; }
    }
}
