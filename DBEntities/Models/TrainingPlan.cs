using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class TrainingPlan
{
    public int TrainingPlanId { get; set; }

    public int TraineeId { get; set; }

    public int GoalId { get; set; }

    public int TrainingDays { get; set; }

    public int TrainingDurationId { get; set; }

    public int FitnessLevelId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public virtual FitnessLevel FitnessLevel { get; set; } = null!;

    public virtual Goal Goal { get; set; } = null!;

    public virtual ICollection<PlanDay> PlanDays { get; set; } = new List<PlanDay>();

    public virtual Trainee Trainee { get; set; } = null!;

    public virtual TrainingDuration TrainingDuration { get; set; } = null!;
}
