using PowerOrchestrator.Identity.Services;
using Xunit;
using FluentAssertions;

namespace PowerOrchestrator.UnitTests.Identity;

/// <summary>
/// Unit tests for MFA service
/// </summary>
public class MfaServiceTests
{
    private readonly MfaService _mfaService;

    public MfaServiceTests()
    {
        var logger = new Serilog.LoggerConfiguration()
            .CreateLogger();

        _mfaService = new MfaService(logger);
    }

    [Fact]
    public void GenerateSecret_ShouldReturnValidBase32Secret()
    {
        // Act
        var secret = _mfaService.GenerateSecret();

        // Assert
        secret.Should().NotBeNullOrEmpty();
        secret.Should().HaveLength(32); // 160-bit secret encoded in Base32
        secret.Should().MatchRegex("^[A-Z2-7]+$"); // Base32 character set
    }

    [Fact]
    public void GenerateQrCodeUrl_ValidParameters_ShouldReturnValidUrl()
    {
        // Arrange
        var email = "test@example.com";
        var secret = _mfaService.GenerateSecret();
        var issuer = "TestApp";

        // Act
        var qrCodeUrl = _mfaService.GenerateQrCodeUrl(email, secret, issuer);

        // Assert
        qrCodeUrl.Should().NotBeNullOrEmpty();
        qrCodeUrl.Should().StartWith("otpauth://totp/");
        qrCodeUrl.Should().Contain(System.Web.HttpUtility.UrlEncode(email));
        qrCodeUrl.Should().Contain(secret);
        qrCodeUrl.Should().Contain(issuer);
    }

    [Fact]
    public void ValidateCode_ValidCode_ShouldReturnTrue()
    {
        // Arrange
        var secret = _mfaService.GenerateSecret();
        
        // Generate a code using the current time window
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;
        var secretBytes = Base32Decode(secret);
        var expectedCode = GenerateTestTotpCode(secretBytes, currentTime);

        // Act
        var result = _mfaService.ValidateCode(secret, expectedCode);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void ValidateCode_InvalidCode_ShouldReturnFalse()
    {
        // Arrange
        var secret = _mfaService.GenerateSecret();
        var invalidCode = "000000";

        // Act
        var result = _mfaService.ValidateCode(secret, invalidCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateCode_EmptySecret_ShouldReturnFalse()
    {
        // Arrange
        var code = "123456";

        // Act
        var result = _mfaService.ValidateCode("", code);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void ValidateCode_EmptyCode_ShouldReturnFalse()
    {
        // Arrange
        var secret = _mfaService.GenerateSecret();

        // Act
        var result = _mfaService.ValidateCode(secret, "");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void GenerateBackupCodes_DefaultCount_ShouldReturn10Codes()
    {
        // Act
        var backupCodes = _mfaService.GenerateBackupCodes();

        // Assert
        backupCodes.Should().HaveCount(10);
        backupCodes.Should().AllSatisfy(code => 
        {
            code.Should().NotBeNullOrEmpty();
            code.Should().HaveLength(8);
            code.Should().MatchRegex("^[0-9]+$");
        });
        backupCodes.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void GenerateBackupCodes_CustomCount_ShouldReturnSpecifiedCount()
    {
        // Arrange
        var count = 5;

        // Act
        var backupCodes = _mfaService.GenerateBackupCodes(count);

        // Assert
        backupCodes.Should().HaveCount(count);
    }

    [Fact]
    public void ValidateBackupCode_ValidCode_ShouldReturnTrueAndRemoveCode()
    {
        // Arrange
        var backupCodes = _mfaService.GenerateBackupCodes(3);
        var codeToValidate = backupCodes.First();
        var originalCount = backupCodes.Count;

        // Act
        var result = _mfaService.ValidateBackupCode(backupCodes, codeToValidate);

        // Assert
        result.Should().BeTrue();
        backupCodes.Should().HaveCount(originalCount - 1);
        backupCodes.Should().NotContain(codeToValidate);
    }

    [Fact]
    public void ValidateBackupCode_InvalidCode_ShouldReturnFalse()
    {
        // Arrange
        var backupCodes = _mfaService.GenerateBackupCodes(3);
        var invalidCode = "99999999";
        var originalCount = backupCodes.Count;

        // Act
        var result = _mfaService.ValidateBackupCode(backupCodes, invalidCode);

        // Assert
        result.Should().BeFalse();
        backupCodes.Should().HaveCount(originalCount); // Should not remove any codes
    }

    /// <summary>
    /// Helper method to generate TOTP code for testing
    /// </summary>
    private static string GenerateTestTotpCode(byte[] secret, long timeStep)
    {
        var timeStepBytes = BitConverter.GetBytes(timeStep);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(timeStepBytes);
        }

        using var hmac = new System.Security.Cryptography.HMACSHA1(secret);
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
    /// Decodes Base32 string to bytes for testing
    /// </summary>
    private static byte[] Base32Decode(string base32)
    {
        var base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567".ToCharArray();
        var trimmed = base32.TrimEnd('=').ToUpperInvariant();
        var result = new List<byte>();
        var buffer = 0;
        var bitsLeft = 0;

        foreach (var c in trimmed)
        {
            var value = Array.IndexOf(base32Chars, c);
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