using Serilog;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace PowerOrchestrator.Identity.Services;

/// <summary>
/// Multi-Factor Authentication service implementation using TOTP
/// </summary>
public class MfaService : IMfaService
{
    private readonly ILogger _logger;
    private static readonly char[] Base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();

    /// <summary>
    /// Initializes a new instance of the MfaService class
    /// </summary>
    /// <param name="logger">Logger</param>
    public MfaService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public string GenerateSecret()
    {
        var bytes = new byte[20]; // 160-bit secret
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        
        var secret = Base32Encode(bytes);
        _logger.Information("Generated new MFA secret");
        
        return secret;
    }

    /// <inheritdoc />
    public string GenerateQrCodeUrl(string userEmail, string secret, string issuer = "PowerOrchestrator")
    {
        var label = HttpUtility.UrlEncode($"{issuer}:{userEmail}");
        var qrCodeUrl = $"otpauth://totp/{label}?secret={secret}&issuer={HttpUtility.UrlEncode(issuer)}";
        
        _logger.Information("Generated QR code URL for user {Email}", userEmail);
        
        return qrCodeUrl;
    }

    /// <inheritdoc />
    public bool ValidateCode(string secret, string code, int timeWindow = 1)
    {
        if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        try
        {
            var secretBytes = Base32Decode(secret);
            var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;

            // Check the current time window and adjacent windows
            for (var i = -timeWindow; i <= timeWindow; i++)
            {
                var timeStep = currentTime + i;
                var expectedCode = GenerateTotpCode(secretBytes, timeStep);
                
                if (string.Equals(code, expectedCode, StringComparison.Ordinal))
                {
                    _logger.Information("MFA code validated successfully");
                    return true;
                }
            }

            _logger.Warning("MFA code validation failed");
            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error validating MFA code");
            return false;
        }
    }

    /// <inheritdoc />
    public List<string> GenerateBackupCodes(int count = 10)
    {
        var codes = new List<string>();
        
        for (var i = 0; i < count; i++)
        {
            var bytes = new byte[4];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            
            var code = (BitConverter.ToUInt32(bytes, 0) % 100000000).ToString("D8");
            codes.Add(code);
        }

        _logger.Information("Generated {Count} backup codes", count);
        
        return codes;
    }

    /// <inheritdoc />
    public bool ValidateBackupCode(List<string> backupCodes, string code)
    {
        if (backupCodes.Contains(code))
        {
            backupCodes.Remove(code);
            _logger.Information("Backup code used and removed");
            return true;
        }

        _logger.Warning("Invalid backup code attempted");
        return false;
    }

    /// <summary>
    /// Generates a TOTP code for the given secret and time step
    /// </summary>
    /// <param name="secret">The secret key bytes</param>
    /// <param name="timeStep">The time step</param>
    /// <returns>The 6-digit TOTP code</returns>
    private static string GenerateTotpCode(byte[] secret, long timeStep)
    {
        var timeStepBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timeStepBytes);
        }

        using var hmac = new HMACSHA1(secret);
        var hash = hmac.ComputeHash(timeStepBytes);

        var offset = hash[hash.Length - 1] & 0x0F;
        var binaryCode = (hash[offset] & 0x7F) << 24
                        | (hash[offset + 1] & 0xFF) << 16
                        | (hash[offset + 2] & 0xFF) << 8
                        | (hash[offset + 3] & 0xFF);

        var code = binaryCode % 1000000;
        return code.ToString("D6");
    }

    /// <summary>
    /// Encodes bytes to Base32 string
    /// </summary>
    /// <param name="bytes">The bytes to encode</param>
    /// <returns>Base32 encoded string</returns>
    private static string Base32Encode(byte[] bytes)
    {
        var result = new StringBuilder();
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var b in bytes)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;

            while (bitsLeft >= 5)
            {
                result.Append(Base32Chars[(buffer >> (bitsLeft - 5)) & 0x1F]);
                bitsLeft -= 5;
            }
        }

        if (bitsLeft > 0)
        {
            result.Append(Base32Chars[(buffer << (5 - bitsLeft)) & 0x1F]);
        }

        return result.ToString();
    }

    /// <summary>
    /// Decodes Base32 string to bytes
    /// </summary>
    /// <param name="base32">The Base32 string</param>
    /// <returns>Decoded bytes</returns>
    private static byte[] Base32Decode(string base32)
    {
        var trimmed = base32.TrimEnd('=').ToUpperInvariant();
        var result = new List<byte>();
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var c in trimmed)
        {
            var value = Array.IndexOf(Base32Chars, c);
            if (value < 0)
            {
                throw new ArgumentException($"Invalid Base32 character: {c}");
            }

            buffer = (buffer << 5) | value;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                result.Add((byte)(buffer >> (bitsLeft - 8)));
                bitsLeft -= 8;
            }
        }

        return result.ToArray();
    }
}