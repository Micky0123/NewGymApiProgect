using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class MuscleEdge
{
    public int MuscleEdgeId { get; set; }

    public int MuscleId1 { get; set; }

    public int MuscleId2 { get; set; }

    public virtual Muscle MuscleId1Navigation { get; set; } = null!;

    public virtual Muscle MuscleId2Navigation { get; set; } = null!;
}
