using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class ExerciseChange
{
    public int ChangeId { get; set; }

    public int RealTimeTrainingId { get; set; }

    public int OriginalExerciseId { get; set; }

    public int NewExerciseId { get; set; }

    public DateTime ChangeDateTime { get; set; }

    public virtual Exercise NewExercise { get; set; } = null!;

    public virtual Exercise OriginalExercise { get; set; } = null!;

    public virtual RealTimeTraining RealTimeTraining { get; set; } = null!;
}
