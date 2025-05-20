using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class SubMuscle
{
    public int SubMuscleId { get; set; }

    public string? SubMuscleName { get; set; }

    public int MuscleId { get; set; }

    public virtual Muscle Muscle { get; set; } = null!;

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}
