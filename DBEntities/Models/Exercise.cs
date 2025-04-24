using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Exercise
{
    public int ExerciseId { get; set; }

    public string? ExerciseName { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<ProgramExercise> ProgramExercises { get; set; } = new List<ProgramExercise>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual ICollection<Joint> Joints { get; set; } = new List<Joint>();

    public virtual ICollection<Size> MuscleGroups { get; set; } = new List<Size>();

    public virtual ICollection<Muscle> Muscles { get; set; } = new List<Muscle>();

    public virtual ICollection<SubMuscle> SubMuscles { get; set; } = new List<SubMuscle>();
}
