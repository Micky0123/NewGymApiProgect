﻿using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class FitnessLevel
{
    public int FitnessLevelId { get; set; }

    public string FitnessLevelName { get; set; } = null!;

    public string? Description { get; set; }

    public virtual ICollection<TrainingPlan> TrainingPlans { get; set; } = new List<TrainingPlan>();
}
