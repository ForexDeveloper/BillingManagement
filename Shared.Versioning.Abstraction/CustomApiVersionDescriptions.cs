namespace Shared.Versioning.Abstraction
{
    public record CustomApiVersionDescriptions()
    {
        public string GroupName { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public bool Deprecated { get; set; }
    }
}
