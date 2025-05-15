using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class DeviceMuscleEdge
{
    public int EdgeId { get; set; }

    public int DeviceId { get; set; }

    public int MuscleId { get; set; }

    public virtual Exercise Device { get; set; } = null!;

    public virtual Muscle Muscle { get; set; } = null!;
}
