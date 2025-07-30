namespace PowerOrchestrator.Identity.Services;

/// <summary>
/// Interface for Multi-Factor Authentication service
/// </summary>
public interface IMfaService
{
    /// <summary>
    /// Generates a new MFA secret for a user
    /// </summary>
    /// <returns>The base32-encoded secret</returns>
    string GenerateSecret();

    /// <summary>
    /// Generates a QR code URL for setting up MFA
    /// </summary>
    /// <param name="userEmail">The user's email</param>
    /// <param name="secret">The MFA secret</param>
    /// <param name="issuer">The issuer name (app name)</param>
    /// <returns>The QR code URL</returns>
    string GenerateQrCodeUrl(string userEmail, string secret, string issuer = "PowerOrchestrator");

    /// <summary>
    /// Validates a TOTP code against a secret
    /// </summary>
    /// <param name="secret">The MFA secret</param>
    /// <param name="code">The TOTP code to validate</param>
    /// <param name="timeWindow">The time window in minutes (default 1)</param>
    /// <returns>True if the code is valid</returns>
    bool ValidateCode(string secret, string code, int timeWindow = 1);

    /// <summary>
    /// Generates backup codes for a user
    /// </summary>
    /// <param name="count">Number of backup codes to generate (default 10)</param>
    /// <returns>List of backup codes</returns>
    List<string> GenerateBackupCodes(int count = 10);

    /// <summary>
    /// Validates a backup code
    /// </summary>
    /// <param name="backupCodes">The list of valid backup codes</param>
    /// <param name="code">The code to validate</param>
    /// <returns>True if the code is valid and not used</returns>
    bool ValidateBackupCode(List<string> backupCodes, string code);
}