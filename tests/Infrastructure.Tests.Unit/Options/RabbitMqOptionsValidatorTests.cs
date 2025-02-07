using FluentAssertions;
using FluentValidation.Results;
using Kundenportal.AdminUi.Infrastructure.Options;
using SharedUnitTestLogic;

namespace Infrastructure.Tests.Unit.Options;

public class RabbitMqOptionsValidatorTests
	: FluentValidationInvariantCultureTestBase
{
	private readonly RabbitMqOptions _rabbitMqOptions = new();
	private readonly RabbitMqOptionsValidator _sut = new();

	[Fact]
	public void Validate_ShouldPass_WhenUsingDefaultValues()
	{
		// Arrange

		// Act
		ValidationResult? result = _sut.Validate(_rabbitMqOptions);

		// Assert
		result.IsValid.Should().BeTrue();
	}

	#region Host

	[Fact]
	public void Validate_ShouldFail_WhenHostIsEmpty()
	{
		// Arrange
		_rabbitMqOptions.Host = "";

		// Act
		ValidationResult? result = _sut.Validate(_rabbitMqOptions);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(x =>
			x.PropertyName == nameof(RabbitMqOptions.Host) &&
			x.ErrorCode == "NotEmptyValidator" &&
			x.ErrorMessage == "'Host' must not be empty.");
	}

	#endregion

	#region Username

	[Fact]
	public void Validate_ShouldFail_WhenUsernameIsEmpty()
	{
		// Arrange
		_rabbitMqOptions.Username = "";

		// Act
		ValidationResult? result = _sut.Validate(_rabbitMqOptions);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(x =>
			x.PropertyName == nameof(RabbitMqOptions.Username) &&
			x.ErrorCode == "NotEmptyValidator" &&
			x.ErrorMessage == "'Username' must not be empty.");
	}

	#endregion

	#region Password

	[Fact]
	public void Validate_ShouldFail_WhenPasswordIsEmpty()
	{
		// Arrange
		_rabbitMqOptions.Password = "";

		// Act
		ValidationResult? result = _sut.Validate(_rabbitMqOptions);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(x =>
			x.PropertyName == nameof(RabbitMqOptions.Password) &&
			x.ErrorCode == "NotEmptyValidator" &&
			x.ErrorMessage == "'Password' must not be empty.");
	}

	#endregion

	#region GetUri

	[Fact]
	public void Validate_ShouldFail_WhenUriIsInvalid()
	{
		// Arrange
		_rabbitMqOptions.Host = "";

		// Act
		ValidationResult? result = _sut.Validate(_rabbitMqOptions);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(x =>
			x.ErrorCode == RabbitMqOptionsValidator.ErrorCodeInvalidUri &&
			x.ErrorMessage == "The configured options have to result in a valid uri");
	}

	#endregion

	#region Port

	[Theory]
	[InlineData(0)]
	[InlineData(-1)]
	public void Validate_ShouldFail_WhenPortIsTooLow(int port)
	{
		// Arrange
		_rabbitMqOptions.Port = port;

		// Act
		ValidationResult? result = _sut.Validate(_rabbitMqOptions);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(x =>
			x.PropertyName == nameof(RabbitMqOptions.Port) &&
			x.ErrorCode == "GreaterThanValidator" &&
			x.ErrorMessage == "'Port' must be greater than '0'.");
	}

	[Fact]
	public void Validate_ShouldFail_WhenPortIsTooHigh()
	{
		// Arrange
		_rabbitMqOptions.Port = 65536;

		// Act
		ValidationResult? result = _sut.Validate(_rabbitMqOptions);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(x =>
			x.PropertyName == nameof(RabbitMqOptions.Port) &&
			x.ErrorCode == "LessThanValidator" &&
			x.ErrorMessage == "'Port' must be less than '65536'.");
	}

	#endregion

	#region VirtualHost

	[Fact]
	public void Validate_ShouldFail_WhenVirtualHostIsEmpty()
	{
		// Arrange
		_rabbitMqOptions.VirtualHost = "";

		// Act
		ValidationResult? result = _sut.Validate(_rabbitMqOptions);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(x =>
			x.PropertyName == nameof(RabbitMqOptions.VirtualHost) &&
			x.ErrorCode == "NotEmptyValidator" &&
			x.ErrorMessage == "'Virtual Host' must not be empty.");
	}

	[Fact]
	public void Validate_ShouldFail_WhenDoesNotStartWithASlash()
	{
		// Arrange
		_rabbitMqOptions.VirtualHost = "foobar";

		// Act
		ValidationResult? result = _sut.Validate(_rabbitMqOptions);

		// Assert
		result.IsValid.Should().BeFalse();
		result.Errors.Should().Contain(x =>
			x.PropertyName == nameof(RabbitMqOptions.VirtualHost) &&
			x.ErrorCode == RabbitMqOptionsValidator.ErrorCodeMissingLeadingSlash &&
			x.ErrorMessage == "'Virtual Host' has to start with a slash");
	}

	#endregion
}
