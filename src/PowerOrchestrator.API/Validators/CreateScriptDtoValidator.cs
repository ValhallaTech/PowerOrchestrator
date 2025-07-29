using FluentValidation;
using PowerOrchestrator.API.DTOs;

namespace PowerOrchestrator.API.Validators;

/// <summary>
/// Validator for CreateScriptDto
/// </summary>
public class CreateScriptDtoValidator : AbstractValidator<CreateScriptDto>
{
    /// <summary>
    /// Initializes validation rules for CreateScriptDto
    /// </summary>
    public CreateScriptDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Script name is required")
            .MaximumLength(255).WithMessage("Script name cannot exceed 255 characters")
            .Matches(@"^[a-zA-Z0-9\s\-_\.]+$").WithMessage("Script name can only contain letters, numbers, spaces, hyphens, underscores, and periods");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Script content is required")
            .MinimumLength(10).WithMessage("Script content must be at least 10 characters");

        RuleFor(x => x.Version)
            .NotEmpty().WithMessage("Version is required")
            .MaximumLength(50).WithMessage("Version cannot exceed 50 characters")
            .Matches(@"^\d+\.\d+\.\d+$").WithMessage("Version must be in format X.Y.Z (e.g., 1.0.0)");

        RuleFor(x => x.Tags)
            .MaximumLength(500).WithMessage("Tags cannot exceed 500 characters");

        RuleFor(x => x.TimeoutSeconds)
            .GreaterThan(0).WithMessage("Timeout must be greater than 0")
            .LessThanOrEqualTo(3600).WithMessage("Timeout cannot exceed 1 hour (3600 seconds)");

        RuleFor(x => x.RequiredPowerShellVersion)
            .NotEmpty().WithMessage("PowerShell version is required")
            .MaximumLength(20).WithMessage("PowerShell version cannot exceed 20 characters");

        RuleFor(x => x.ParametersSchema)
            .Must(BeValidJsonOrNull).WithMessage("Parameters schema must be valid JSON")
            .When(x => !string.IsNullOrEmpty(x.ParametersSchema));
    }

    private static bool BeValidJsonOrNull(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return true;

        try
        {
            Newtonsoft.Json.JsonConvert.DeserializeObject(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
}