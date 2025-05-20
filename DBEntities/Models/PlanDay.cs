using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class PlanDay
{
    public int PlanDayId { get; set; }

    public int TrainingPlanId { get; set; }

    public string ProgramName { get; set; } = null!;

    public int DayOrder { get; set; }

    public DateTime CreationDate { get; set; }

    public bool IsDefaultProgram { get; set; }

    public int? ParentProgramId { get; set; }

    public bool IsHistoricalProgram { get; set; }

    public virtual ICollection<ExercisePlan> ExercisePlans { get; set; } = new List<ExercisePlan>();

    public virtual TrainingPlan TrainingPlan { get; set; } = null!;
}
