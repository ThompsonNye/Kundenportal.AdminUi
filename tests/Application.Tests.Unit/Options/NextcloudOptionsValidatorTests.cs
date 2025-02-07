using FluentAssertions;
using FluentValidation.Results;
using Kundenportal.AdminUi.Application.Options;
using SharedUnitTestLogic;

namespace Application.Tests.Unit.Options;

public class NextcloudOptionsValidatorTests : FluentValidationInvariantCultureTestBase
{
	private readonly NextcloudOptionsValidator _sut = new();

	public readonly NextcloudOptions Options = new()
	{
		StructureBasePath = "/",
		Host = "http://localhost",
		Username = "user",
		Password = "password",
		RetryDelay = 1
	};

	[Fact]
	public void Validate_ShouldPass_WhenAllValuesAreValid()
	{
		// Arrange

		// Act
		ValidationResult? result = _sut.Validate(Options);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	#region Username

	[Fact]
	public void Validate_ShouldFail_WhenUsernameIsEmpty()
	{
		// Arrange
		Options.Username = "";

		// Act
		ValidationResult? result = _sut.Validate(Options);

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
		Options.Password = "";

		// Act
		ValidationResult? result = _sut.Validate(Options);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(x =>
			x.PropertyName == nameof(NextcloudOptions.Password) &&
			x.ErrorCode == "NotEmptyValidator" &&
			x.ErrorMessage == "'Password' must not be empty.");
	}

	#endregion

	#region RetryDelay

	[Theory]
	[InlineData(0)]
	[InlineData(0 - 1)]
	public void Validate_ShouldFail_WhenRetryDelayIsLessThanOrEqualTo0(double value)
	{
		// Arrange
		Options.RetryDelay = value;

		// Act
		ValidationResult? result = _sut.Validate(Options);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(x =>
			x.PropertyName == nameof(NextcloudOptions.RetryDelay) &&
			x.ErrorCode == "GreaterThanValidator" &&
			x.ErrorMessage == "'Retry Delay' must be greater than '0'.");
	}

	#endregion

	#region StructureBasePath

	[Theory]
	[InlineData("/")]
	[InlineData("/foo")]
	[InlineData("/foo/bar")]
	[InlineData("/foo/bar/test")]
	public void Validate_ShouldPass_WhenStructureBasePathStartsWithASlashAndDoesNotEndWithASlash(string value)
	{
		// Arrange
		Options.StructureBasePath = value;

		// Act
		ValidationResult? result = _sut.Validate(Options);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	[Fact]
	public void Validate_ShouldFail_WhenStructureBasePathIsEmpty()
	{
		// Arrange
		Options.StructureBasePath = "";

		// Act
		ValidationResult? result = _sut.Validate(Options);

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
		Options.StructureBasePath = value;

		// Act
		ValidationResult? result = _sut.Validate(Options);

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
		Options.StructureBasePath = value;

		// Act
		ValidationResult? result = _sut.Validate(Options);

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
		Options.Host = "";

		// Act
		ValidationResult? result = _sut.Validate(Options);

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
		Options.Host = "foobar";

		// Act
		ValidationResult? result = _sut.Validate(Options);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(x =>
			x.PropertyName == nameof(NextcloudOptions.Host) &&
			x.ErrorCode == NextcloudOptionsValidator.ErrorCodeNotAUri &&
			x.ErrorMessage == "'Host' is not a valid uri");
	}

	#endregion
}
