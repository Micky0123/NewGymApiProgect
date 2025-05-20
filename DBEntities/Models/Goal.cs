using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Goal
{
    public int GoalId { get; set; }

    public string GoalName { get; set; } = null!;

    public bool Active { get; set; }

    public virtual ICollection<TrainingPlan> TrainingPlans { get; set; } = new List<TrainingPlan>();
}
