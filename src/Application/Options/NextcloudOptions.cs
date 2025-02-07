using FluentValidation;

namespace Kundenportal.AdminUi.Application.Options;

public sealed class NextcloudOptions
{
    public const string SectionName = "Nextcloud";
    
    public string StructureBasePath { get; set; } = "/";
}

public sealed class NextcloudOptionsValidator : AbstractValidator<NextcloudOptions>
{
    public const string ErrorCodeMissingLeadingSlash = "MissingLeadingSlash";
    
    public const string ErrorCodeMissingTrailingSlash = "MissingTrailingSlash";
    
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
    }

    private static bool StartWithASlash(string value)
    {
        return value.StartsWith('/');
    }

    private static bool NotEndWithASlash(string value)
    {
        return !value.EndsWith('/');
    }
}
