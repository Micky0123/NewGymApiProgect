using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class TrainingProgram
{
    public int ProgramId { get; set; }

    public int TraineeId { get; set; }

    public string ProgramName { get; set; } = null!;

    public DateTime CreationDate { get; set; }

    public DateTime TrainingDateTime { get; set; }

    public virtual ICollection<ProgramExercise> ProgramExercises { get; set; } = new List<ProgramExercise>();

    public virtual Trainee Trainee { get; set; } = null!;
}
