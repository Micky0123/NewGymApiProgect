using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Exercise
{
    public int ExerciseId { get; set; }

    public string ExerciseName { get; set; } = null!;

    public bool Active { get; set; }

    public int? MuscleId { get; set; }

    public int? MuscleTypeId { get; set; }

    public int? MuscleGroupId { get; set; }

    public int? Count { get; set; }

    public virtual ICollection<DeviceMuscleEdge> DeviceMuscleEdges { get; set; } = new List<DeviceMuscleEdge>();

    public virtual ICollection<ExercisePlan> ExercisePlans { get; set; } = new List<ExercisePlan>();

    public virtual ICollection<GraphEdge> GraphEdgeDevice1s { get; set; } = new List<GraphEdge>();

    public virtual ICollection<GraphEdge> GraphEdgeDevice2s { get; set; } = new List<GraphEdge>();

    public virtual Muscle? Muscle { get; set; }

    public virtual Size? MuscleGroup { get; set; }

    public virtual MuscleType? MuscleType { get; set; }

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual ICollection<Joint> Joints { get; set; } = new List<Joint>();

    public virtual ICollection<SubMuscle> SubMuscles { get; set; } = new List<SubMuscle>();
}
