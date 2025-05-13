using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class ProgramExercise
{
    public int ProgramExerciseId { get; set; }

    public int ProgramId { get; set; }

    public int ExerciseId { get; set; }

    public int? ProgramSets { get; set; }

    public int? ProgramRepetitionsMin { get; set; }

    public decimal? ProgramWeight { get; set; }

    public int? ExerciseOrder { get; set; }

    public int? ProgramRepetitionsMax { get; set; }

    public int CategoryId { get; set; }

    public int TimesMin { get; set; }

    public int TimesMax { get; set; }

    public int MuscleId { get; set; }

    public int? SubMuscleId { get; set; }

    public int? DayOrder { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual Exercise Exercise { get; set; } = null!;

    public virtual Muscle Muscle { get; set; } = null!;

    public virtual TrainingProgram Program { get; set; } = null!;

    public virtual SubMuscle? SubMuscle { get; set; }
}
