using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class GraphEdge
{
    public int Id { get; set; }

    public int Device1Id { get; set; }

    public int Device2Id { get; set; }

    public virtual Exercise Device1 { get; set; } = null!;

    public virtual Exercise Device2 { get; set; } = null!;
}
