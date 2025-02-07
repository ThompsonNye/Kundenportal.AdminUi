using Kundenportal.AdminUi.WebApp.Components.Pages.Structures;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApp.Tests.bUnit;

public class StructureGroupsTests : TestContext
{
    private readonly ILogger<StructureGroups> _logger = Substitute.For<ILogger<StructureGroups>>();

    [Fact]
    public void Option1LogsOptions()
    {
        Services.AddSingleton(_logger);

        var cut = RenderComponent<StructureGroups>();

        //cut.Find("#optionen1").Click();

        cut.Find("#option-1").Click();

        _logger.Received().LogInformation("Option 1 ausgewählt");
    }
}
