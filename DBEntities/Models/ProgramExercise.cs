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
    public int? ProgramRepetitionsMax { get; set; }

    public decimal? ProgramWeight { get; set; }

    public int? ExerciseOrder { get; set; }

    public virtual Exercise Exercise { get; set; } = null!;

    public virtual TrainingProgram Program { get; set; } = null!;
}
