using FluentAssertions;
using Kundenportal.AdminUi.Application.Options;

namespace Application.Tests.Unit.Options;

public class NextcloudOptionsTests
{
	private readonly NextcloudOptions _sut = new();

	[Theory]
	[InlineData("bar")]
	[InlineData("/bar")]
	[InlineData("/bar/")]
	[InlineData("bar/")]
	public void CombineWithStructureBasePath_ShouldCombineBasePathAndNameWithoutModifications_WhenCalled(string name)
	{
		// Arrange
		const string basePath = "/foo";
		_sut.StructureBasePath = basePath;

		// Act
		var result = _sut.CombineWithStructureBasePath(name);

		// Assert
		result.Should().Be($"{basePath}/{name}");
	}
}
