using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kundenportal.AdminUi.Application.Models;

public sealed class StructureGroup
{
    public int? Id { get; set; }

    public string Name { get; set; } = "";

    public string Path { get; set; } = "";
}

