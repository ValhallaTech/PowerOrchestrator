namespace PowerOrchestrator.Domain.ValueObjects;

/// <summary>
/// PowerShell version information
/// </summary>
public class PowerShellVersion
{
    /// <summary>
    /// Major version number
    /// </summary>
    public int Major { get; set; }

    /// <summary>
    /// Minor version number
    /// </summary>
    public int Minor { get; set; }

    /// <summary>
    /// Build number
    /// </summary>
    public int Build { get; set; }

    /// <summary>
    /// Revision number
    /// </summary>
    public int Revision { get; set; }

    /// <summary>
    /// Gets the version as a string
    /// </summary>
    public override string ToString()
    {
        return $"{Major}.{Minor}.{Build}.{Revision}";
    }
}