using AutoFixture;
using FluentAssertions;
using FluentValidation.Results;
using Kundenportal.AdminUi.Application.Options;
using SharedUnitTestLogic;

namespace Application.Tests.Unit.Options;

public class NextcloudOptionsValidatorTests : FluentValidationInvariantCultureTestBase
{
    private readonly NextcloudOptionsValidator _sut = new();

    private readonly NextcloudOptions _options = new()
    {
        StructureBasePath = "/",
        Host = "http://localhost",
        Username = "user",
        Password = "password"
    };
    
    private readonly Fixture _fixture = new();

    #region StructureBasePath

    [Theory]
    [InlineData("/")]
    [InlineData("/foo")]
    [InlineData("/foo/bar")]
    [InlineData("/foo/bar/test")]
    public void Validate_ShouldPass_WhenStructureBasePathStartsWithASlashAndDoesNotEndWithASlash(string value)
    {
        // Arrange
        _options.StructureBasePath = value;

        // Act
        ValidationResult result = _sut.Validate(_options);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenStructureBasePathIsEmpty()
    {
        // Arrange
        _options.StructureBasePath = "";

        // Act
        ValidationResult result = _sut.Validate(_options);

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
        _options.StructureBasePath = value;

        // Act
        ValidationResult result = _sut.Validate(_options);

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
        _options.StructureBasePath = value;

        // Act
        ValidationResult result = _sut.Validate(_options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(NextcloudOptions.StructureBasePath) &&
            x.ErrorCode == NextcloudOptionsValidator.ErrorCodeMissingTrailingSlash &&
            x.ErrorMessage == "'Structure Base Path' cannot end with a slash");
    }

    #endregion

    #region Host

    [Fact]
    public void Validate_ShouldFail_WhenHostIsEmpty()
    {
        // Arrange
        _options.Host = "";

        // Act
        var result = _sut.Validate(_options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(NextcloudOptions.Host) &&
            x.ErrorCode == "NotEmptyValidator" &&
            x.ErrorMessage == "'Host' must not be empty.");
    }

    [Fact]
    public void Validate_ShouldFail_WhenHostIsNotAUri()
    {
        // Arrange
        _options.Host = "foobar";

        // Act
        var result = _sut.Validate(_options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(NextcloudOptions.Host) &&
            x.ErrorCode == NextcloudOptionsValidator.ErrorCodeNotAUri &&
            x.ErrorMessage == "'Host' is not a valid uri");
    }

    #endregion

    #region Username

    [Fact]
    public void Validate_ShouldFail_WhenUsernameIsEmpty()
    {
        // Arrange
        _options.Username = "";

        // Act
        var result = _sut.Validate(_options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(NextcloudOptions.Username) &&
            x.ErrorCode == "NotEmptyValidator" &&
            x.ErrorMessage == "'Username' must not be empty.");
    }

    #endregion

    #region Password

    [Fact]
    public void Validate_ShouldFail_WhenPasswordIsEmpty()
    {
        // Arrange
        _options.Password = "";

        // Act
        var result = _sut.Validate(_options);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(x =>
            x.PropertyName == nameof(NextcloudOptions.Password) &&
            x.ErrorCode == "NotEmptyValidator" &&
            x.ErrorMessage == "'Password' must not be empty.");
    }

    #endregion
}