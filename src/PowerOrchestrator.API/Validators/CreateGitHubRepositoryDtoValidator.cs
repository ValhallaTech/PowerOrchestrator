using FluentValidation;
using PowerOrchestrator.API.DTOs;
using System.Text.RegularExpressions;

namespace PowerOrchestrator.API.Validators;

/// <summary>
/// Validator for CreateGitHubRepositoryDto
/// </summary>
public class CreateGitHubRepositoryDtoValidator : AbstractValidator<CreateGitHubRepositoryDto>
{
    /// <summary>
    /// Initializes validation rules for CreateGitHubRepositoryDto
    /// </summary>
    public CreateGitHubRepositoryDtoValidator()
    {
        RuleFor(x => x.Owner)
            .NotEmpty().WithMessage("Repository owner is required")
            .MaximumLength(39).WithMessage("GitHub usernames cannot exceed 39 characters")
            .Matches(@"^[a-zA-Z0-9]([a-zA-Z0-9]|-(?!-))*[a-zA-Z0-9]$|^[a-zA-Z0-9]$")
            .WithMessage("Invalid GitHub username format. Only alphanumeric characters and hyphens are allowed, cannot start or end with hyphen");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Repository name is required")
            .MaximumLength(100).WithMessage("Repository name cannot exceed 100 characters")
            .Matches(@"^[a-zA-Z0-9._-]+$")
            .WithMessage("Repository name can only contain alphanumeric characters, dots, underscores, and hyphens");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Repository description cannot exceed 1000 characters")
            .When(x => !string.IsNullOrEmpty(x.Description));

        RuleFor(x => x.DefaultBranch)
            .NotEmpty().WithMessage("Default branch is required")
            .MaximumLength(255).WithMessage("Branch name cannot exceed 255 characters")
            .Matches(@"^(?!.*\.\.)[a-zA-Z0-9._/-]+(?<!\.lock)(?<!\.)$")
            .WithMessage("Invalid branch name format");

        // Custom validation to ensure full name format is correct
        RuleFor(x => x)
            .Must(x => IsValidRepositoryFullName(x.Owner, x.Name))
            .WithMessage("Invalid repository format. Must be in format 'owner/repository'")
            .When(x => !string.IsNullOrEmpty(x.Owner) && !string.IsNullOrEmpty(x.Name));
    }

    private static bool IsValidRepositoryFullName(string owner, string name)
    {
        if (string.IsNullOrEmpty(owner) || string.IsNullOrEmpty(name))
            return false;

        var fullName = $"{owner}/{name}";
        
        // GitHub repository full names must not exceed 100 characters
        if (fullName.Length > 100)
            return false;

        // Must match GitHub's repository naming conventions
        return Regex.IsMatch(fullName, @"^[a-zA-Z0-9]([a-zA-Z0-9]|-(?!-))*[a-zA-Z0-9]/[a-zA-Z0-9._-]+$|^[a-zA-Z0-9]/[a-zA-Z0-9._-]+$");
    }
}