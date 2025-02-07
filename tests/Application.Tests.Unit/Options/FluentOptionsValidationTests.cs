using FluentAssertions;
using FluentValidation;
using Kundenportal.AdminUi.Application.Options;
using Microsoft.Extensions.Options;
using NSubstitute;
using SharedUnitTestLogic;

namespace Application.Tests.Unit.Options;

public class FluentOptionsValidationTests : FluentValidationInvariantCultureTestBase
{
    private readonly FluentOptionsValidation<NextcloudOptions> _sut;

    private readonly IEnumerable<IValidator<NextcloudOptions>> _validators =
        Substitute.For<IEnumerable<IValidator<NextcloudOptions>>>();

    private const string Name = "OptionsName";

    private readonly NextcloudOptions _validOptions = new();

    public FluentOptionsValidationTests()
    {
        _sut = new FluentOptionsValidation<NextcloudOptions>(Name, _validators);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validate_ShouldSkip_WhenNameIsNotOptionsNameNull(string? name)
    {
        // Arrange

        // Act
        ValidateOptionsResult result = _sut.Validate(name, _validOptions);

        // Assert
        result.Skipped.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldThrowArgumentException_WhenOptionsAreNull()
    {
        // Arrange

        // Act
        Func<ValidateOptionsResult> action = () => _sut.Validate(Name, null!);

        // Assert
        action.Should().ThrowExactly<ArgumentNullException>()
            .WithMessage("Value cannot be null. (Parameter 'options')");
    }

    [Fact]
    public void Validate_ShouldSkip_WhenNoValidatorsExist()
    {
        // Arrange
        IEnumerable<IValidator<NextcloudOptions>> validators = Enumerable.Empty<IValidator<NextcloudOptions>>();
        _validators.GetEnumerator().Returns(validators.GetEnumerator());

        // Act
        ValidateOptionsResult result = _sut.Validate(Name, _validOptions);

        // Assert
        result.Skipped.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldSkip_WhenValidationPasses()
    {
        // Arrange
        IEnumerable<IValidator<NextcloudOptions>> validators = [new NextcloudOptionsValidator()];
        _validators.GetEnumerator().Returns(validators.GetEnumerator());

        // Act
        ValidateOptionsResult result = _sut.Validate(Name, _validOptions);

        // Assert
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void Validate_ShouldFail_WhenValidationFails()
    {
        // Arrange
        IEnumerable<IValidator<NextcloudOptions>> validators = [new NextcloudOptionsValidator()];
        _validators.GetEnumerator().Returns(validators.GetEnumerator());

        _validOptions.StructureBasePath = "foo";

        // Act
        ValidateOptionsResult result = _sut.Validate(Name, _validOptions);

        // Assert
        result.Failed.Should().BeTrue();
        result.FailureMessage.Should()
            .Be($"Fluent validation failed for options '{nameof(NextcloudOptions)}': 'Structure Base Path' has to start with a slash.");
    }
}