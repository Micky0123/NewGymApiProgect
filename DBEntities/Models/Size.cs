using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Size
{
    public int MuscleGroupId { get; set; }

    public string MuscleGroupName { get; set; } = null!;

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}
