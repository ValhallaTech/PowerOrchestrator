using FluentValidation;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace PowerOrchestrator.Application.Validators;

/// <summary>
/// Validation extensions for GitHub service parameters
/// </summary>
public static class GitHubValidationExtensions
{
    /// <summary>
    /// Validates a GitHub repository owner name
    /// </summary>
    /// <param name="owner">Owner name to validate</param>
    /// <exception cref="ArgumentException">Thrown when owner name is invalid</exception>
    public static void ValidateOwner(string owner)
    {
        if (string.IsNullOrWhiteSpace(owner))
            throw new ArgumentException("Repository owner cannot be null or empty", nameof(owner));

        if (owner.Length > 39)
            throw new ArgumentException("GitHub usernames cannot exceed 39 characters", nameof(owner));

        if (!Regex.IsMatch(owner, @"^[a-zA-Z0-9]([a-zA-Z0-9]|-(?!-))*[a-zA-Z0-9]$|^[a-zA-Z0-9]$"))
            throw new ArgumentException("Invalid GitHub username format", nameof(owner));
    }

    /// <summary>
    /// Validates a GitHub repository name
    /// </summary>
    /// <param name="name">Repository name to validate</param>
    /// <exception cref="ArgumentException">Thrown when repository name is invalid</exception>
    public static void ValidateRepositoryName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Repository name cannot be null or empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Repository name cannot exceed 100 characters", nameof(name));

        if (!Regex.IsMatch(name, @"^[a-zA-Z0-9._-]+$"))
            throw new ArgumentException("Repository name can only contain alphanumeric characters, dots, underscores, and hyphens", nameof(name));
    }

    /// <summary>
    /// Validates a GitHub repository full name (owner/name)
    /// </summary>
    /// <param name="fullName">Full repository name to validate</param>
    /// <exception cref="ArgumentException">Thrown when full name is invalid</exception>
    public static void ValidateRepositoryFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Repository full name cannot be null or empty", nameof(fullName));

        var parts = fullName.Split('/');
        if (parts.Length != 2)
            throw new ArgumentException("Repository full name must be in format 'owner/repository'", nameof(fullName));

        ValidateOwner(parts[0]);
        ValidateRepositoryName(parts[1]);
    }

    /// <summary>
    /// Validates a branch name
    /// </summary>
    /// <param name="branch">Branch name to validate</param>
    /// <exception cref="ArgumentException">Thrown when branch name is invalid</exception>
    public static void ValidateBranchName(string? branch)
    {
        if (string.IsNullOrWhiteSpace(branch))
            return; // Optional parameter

        if (branch.Length > 255)
            throw new ArgumentException("Branch name cannot exceed 255 characters", nameof(branch));

        if (!Regex.IsMatch(branch, @"^(?!.*\.\.)[a-zA-Z0-9._/-]+(?<!\.lock)(?<!\.)$"))
            throw new ArgumentException("Invalid branch name format", nameof(branch));
    }

    /// <summary>
    /// Validates a file path
    /// </summary>
    /// <param name="path">File path to validate</param>
    /// <exception cref="ArgumentException">Thrown when file path is invalid</exception>
    public static void ValidateFilePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("File path cannot be null or empty", nameof(path));

        if (path.Length > 4096)
            throw new ArgumentException("File path cannot exceed 4096 characters", nameof(path));

        // Check for invalid characters
        var invalidChars = new[] { '<', '>', ':', '"', '|', '?', '*' };
        if (path.IndexOfAny(invalidChars) >= 0)
            throw new ArgumentException("File path contains invalid characters", nameof(path));

        // Check for directory traversal
        if (path.Contains(".."))
            throw new ArgumentException("File path cannot contain directory traversal sequences", nameof(path));
    }

    /// <summary>
    /// Validates a GUID parameter
    /// </summary>
    /// <param name="id">GUID to validate</param>
    /// <param name="parameterName">Parameter name for error messages</param>
    /// <exception cref="ArgumentException">Thrown when GUID is invalid</exception>
    public static void ValidateGuid(Guid id, string parameterName = "id")
    {
        if (id == Guid.Empty)
            throw new ArgumentException("GUID cannot be empty", parameterName);
    }

    /// <summary>
    /// Validates sync history limit parameter
    /// </summary>
    /// <param name="limit">Limit value to validate</param>
    /// <exception cref="ArgumentException">Thrown when limit is invalid</exception>
    public static void ValidateSyncHistoryLimit(int limit)
    {
        if (limit <= 0)
            throw new ArgumentException("Limit must be greater than 0", nameof(limit));

        if (limit > 1000)
            throw new ArgumentException("Limit cannot exceed 1000 records", nameof(limit));
    }
}