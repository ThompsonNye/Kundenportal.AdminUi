using AutoFixture;
using FluentAssertions;
using FluentValidation.Results;
using Kundenportal.AdminUi.Application.Options;
using SharedUnitTestLogic;

namespace Application.Tests.Unit.Options;

public class NextcloudOptionsValidatorTests : FluentValidationInvariantCultureTestBase
{
    private readonly NextcloudOptionsValidator _sut = new();

    private readonly NextcloudOptions _validOptions = new();
    
    private readonly Fixture _fixture = new();

    [Theory]
    [InlineData("/")]
    [InlineData("/foo")]
    [InlineData("/foo/bar")]
    [InlineData("/foo/bar/test")]
    public void Validate_ShouldPass_WhenStructureBasePathStartsWithASlashAndDoesNotEndWithASlash(string value)
    {
        // Arrange
        _validOptions.StructureBasePath = value;

        // Act
        ValidationResult result = _sut.Validate(_validOptions);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenStructureBasePathIsEmpty()
    {
        // Arrange
        _validOptions.StructureBasePath = "";

        // Act
        ValidationResult result = _sut.Validate(_validOptions);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(NextcloudOptions.StructureBasePath) &&
            x.ErrorCode == "NotEmptyValidator" &&
            x.ErrorMessage == "'Structure Base Path' must not be empty.");
    }
    
    [Theory]
    [InlineData("foo")]
    [InlineData("foo/bar")]
    [InlineData("foo/bar/test")]
    public void Validate_ShouldFail_WhenStructureBasePathDoesNotStartWithASlash(string value)
    {
        // Arrange
        _validOptions.StructureBasePath = value;

        // Act
        ValidationResult result = _sut.Validate(_validOptions);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(NextcloudOptions.StructureBasePath) &&
            x.ErrorCode == NextcloudOptionsValidator.ErrorCodeMissingLeadingSlash &&
            x.ErrorMessage == "'Structure Base Path' has to start with a slash");
    }
    
    [Theory]
    [InlineData("/foo/")]
    [InlineData("/foo/bar/")]
    [InlineData("/foo/bar/test/")]
    public void Validate_ShouldFail_WhenStructureBasePathEndsWithASlash(string value)
    {
        // Arrange
        _validOptions.StructureBasePath = value;

        // Act
        ValidationResult result = _sut.Validate(_validOptions);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(NextcloudOptions.StructureBasePath) &&
            x.ErrorCode == NextcloudOptionsValidator.ErrorCodeMissingTrailingSlash &&
            x.ErrorMessage == "'Structure Base Path' cannot end with a slash");
    }
}