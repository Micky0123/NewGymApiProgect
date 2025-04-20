using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class ExerciseEquipment
{
    public int ExerciseId { get; set; }

    public int EquipmentId { get; set; }

    public virtual Equipment Equipment { get; set; } = null!;

    public virtual Exercise Exercise { get; set; } = null!;

    public virtual ProgramExercise ExerciseNavigation { get; set; } = null!;
}
