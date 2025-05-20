using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Muscle
{
    public int MuscleId { get; set; }

    public string MuscleName { get; set; } = null!;

    public virtual ICollection<DeviceMuscleEdge> DeviceMuscleEdges { get; set; } = new List<DeviceMuscleEdge>();

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();

    public virtual ICollection<MuscleEdge> MuscleEdgeMuscleId1Navigations { get; set; } = new List<MuscleEdge>();

    public virtual ICollection<MuscleEdge> MuscleEdgeMuscleId2Navigations { get; set; } = new List<MuscleEdge>();

    public virtual ICollection<SubMuscle> SubMuscles { get; set; } = new List<SubMuscle>();
}
