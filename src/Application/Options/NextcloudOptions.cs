using FluentValidation;

namespace Kundenportal.AdminUi.Application.Options;

public sealed class NextcloudOptions
{
    public const string SectionName = "Nextcloud";
    
    public string StructureBasePath { get; set; } = "/";

    public string Username { get; set; } = "";

    public string Password { get; set; } = "";

    public string Host { get; set; } = "";
    
    public double RetryDelay { get; set; }

    /// <summary>
    /// Combines the given <paramref name="structureGroupName"/> with the <see cref="StructureBasePath"/> with a slash in between.
    /// </summary>
    /// <param name="structureGroupName"></param>
    /// <returns></returns>
    public string CombineWithStructureBasePath(string structureGroupName)
    {
        return $"{StructureBasePath}/{structureGroupName}";
    }
}

public sealed class NextcloudOptionsValidator : AbstractValidator<NextcloudOptions>
{
    public const string ErrorCodeMissingLeadingSlash = "MissingLeadingSlash";
    
    public const string ErrorCodeMissingTrailingSlash = "MissingTrailingSlash";

    public const string ErrorCodeNotAUri = "NotAnUri";
    
    public NextcloudOptionsValidator()
    {
        RuleFor(x => x.StructureBasePath)
            .NotEmpty();
        
        RuleFor(x => x.StructureBasePath)
            .Must(StartWithASlash)
                .WithErrorCode(ErrorCodeMissingLeadingSlash)
                .WithMessage("'{PropertyName}' has to start with a slash");
        
        RuleFor(x => x.StructureBasePath)
            .Must(NotEndWithASlash)
                .When(x => x.StructureBasePath.Length > 1)
                .WithErrorCode(ErrorCodeMissingTrailingSlash)
                .WithMessage("'{PropertyName}' cannot end with a slash");

        RuleFor(x => x.Host)
            .NotEmpty()
            .Must(BeAUri)
                .WithErrorCode(ErrorCodeNotAUri)
                .WithMessage("'{PropertyName}' is not a valid uri");

        RuleFor(x => x.Username)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty();

        RuleFor(x => x.RetryDelay)
            .GreaterThan(0);
    }

    private static bool StartWithASlash(string value)
    {
        return value.StartsWith('/');
    }

    private static bool NotEndWithASlash(string value)
    {
        return !value.EndsWith('/');
    }

    private static bool BeAUri(string value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out _);
    }
}
