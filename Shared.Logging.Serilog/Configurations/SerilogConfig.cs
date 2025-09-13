using System.Collections.ObjectModel;

namespace Shared.Logging.Serilog.Configurations;

public enum ApplicationType
{
    Api,
    Web,
    Worker
}

public class SerilogConfiguration
{
    public string ApplicationName { get; set; }
    public ApplicationType ApplicationType { get; set; }
    public string BaseFilePath { get; set; }
    public Collection<string> ExcludedLogPaths { get; set; } = new();
    public SerilogMaskConfiguration MaskConfig { get; set; } = new();
}

public class SerilogMaskConfiguration
{
    public bool Enabled { get; set; } = true;
    public bool MaskPan { get; set; } = true;
    public bool MaskIban { get; set; }

    public string CustomRegex { get; set; } = string.Empty;
}