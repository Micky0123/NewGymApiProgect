using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Exercise
{
    public int ExerciseId { get; set; }

    public string? ExerciseName { get; set; }

    public bool? Active { get; set; }

    public virtual ICollection<DeviceMuscleEdge> DeviceMuscleEdges { get; set; } = new List<DeviceMuscleEdge>();

    public virtual ICollection<ExerciseChange> ExerciseChangeNewExercises { get; set; } = new List<ExerciseChange>();

    public virtual ICollection<ExerciseChange> ExerciseChangeOriginalExercises { get; set; } = new List<ExerciseChange>();

    public virtual ICollection<GraphEdge> GraphEdgeDevice1s { get; set; } = new List<GraphEdge>();

    public virtual ICollection<GraphEdge> GraphEdgeDevice2s { get; set; } = new List<GraphEdge>();

    public virtual ICollection<ProgramExercise> ProgramExercises { get; set; } = new List<ProgramExercise>();

    public virtual ICollection<RealTimeTraining> RealTimeTrainings { get; set; } = new List<RealTimeTraining>();

    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();

    public virtual ICollection<Equipment> Equipment { get; set; } = new List<Equipment>();

    public virtual ICollection<Joint> Joints { get; set; } = new List<Joint>();

    public virtual ICollection<Size> MuscleGroups { get; set; } = new List<Size>();

    public virtual ICollection<MuscleType> MuscleTypes { get; set; } = new List<MuscleType>();

    public virtual ICollection<Muscle> Muscles { get; set; } = new List<Muscle>();

    public virtual ICollection<SubMuscle> SubMuscles { get; set; } = new List<SubMuscle>();
}
