using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class ExercisePlan
{
    public int ExercisePlanId { get; set; }

    public int PlanDayId { get; set; }

    public int ExerciseId { get; set; }

    public int PlanSets { get; set; }

    public int PlanRepetitionsMin { get; set; }

    public decimal PlanWeight { get; set; }

    public int PlanRepetitionsMax { get; set; }

    public int CategoryId { get; set; }

    public int TimesMin { get; set; }

    public int TimesMax { get; set; }

    public int? SubMuscleId { get; set; }

    public int IndexOrder { get; set; }

    public DateTime TrainingDateTime { get; set; }

    public virtual Exercise Exercise { get; set; } = null!;

    public virtual PlanDay PlanDay { get; set; } = null!;
}
