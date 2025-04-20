using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Joint
{
    public int JointId { get; set; }

    public string? JointName { get; set; }

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}
