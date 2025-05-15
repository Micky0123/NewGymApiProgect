using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Muscle
{
    public int MuscleId { get; set; }

    public string? MuscleName { get; set; }

    public virtual ICollection<DeviceMuscleEdge> DeviceMuscleEdges { get; set; } = new List<DeviceMuscleEdge>();

    public virtual ICollection<MuscleEdge> MuscleEdgeMuscleId1Navigations { get; set; } = new List<MuscleEdge>();

    public virtual ICollection<MuscleEdge> MuscleEdgeMuscleId2Navigations { get; set; } = new List<MuscleEdge>();

    public virtual ICollection<ProgramExercise> ProgramExercises { get; set; } = new List<ProgramExercise>();

    public virtual ICollection<RealTimeTraining> RealTimeTrainings { get; set; } = new List<RealTimeTraining>();

    public virtual ICollection<SubMuscle> SubMuscles { get; set; } = new List<SubMuscle>();

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}
