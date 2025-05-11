using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public virtual ICollection<ProgramExercise> ProgramExercises { get; set; } = new List<ProgramExercise>();

    public virtual ICollection<RealTimeTraining> RealTimeTrainings { get; set; } = new List<RealTimeTraining>();

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}
