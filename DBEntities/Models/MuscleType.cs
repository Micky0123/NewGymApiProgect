using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class MuscleType
{
    public int MuscleTypeId { get; set; }

    public string MuscleTypeName { get; set; } = null!;

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}
