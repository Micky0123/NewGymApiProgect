using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Goal
{
    public int GoalId { get; set; }

    public string? GoalName { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<Trainee> Trainees { get; set; } = new List<Trainee>();
}
