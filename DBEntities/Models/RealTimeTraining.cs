//using System;
//using System.Collections.Generic;

//namespace DBEntities.Models;

//public partial class RealTimeTraining
//{
//    public int RealTimeTrainingId { get; set; }

//    public int ProgramId { get; set; }

//    public int TraineeId { get; set; }

//    public int ExerciseId { get; set; }

//    public int SetsExercise { get; set; }

//    public int RepetitionsMin { get; set; }

//    public int RepetitionsMax { get; set; }

//    public double? WeightExercise { get; set; }

//    public int ExerciseOrder { get; set; }

//    public int CategoryId { get; set; }

//    public int TimesMin { get; set; }

//    public int TimesMax { get; set; }

//    public int MuscleId { get; set; }

//    public int? SubMuscleId { get; set; }

//    public DateTime TrainingDateTime { get; set; }

//    public virtual Category Category { get; set; } = null!;

//    public virtual Exercise Exercise { get; set; } = null!;

//    public virtual ICollection<ExerciseChange> ExerciseChanges { get; set; } = new List<ExerciseChange>();

//    public virtual Muscle Muscle { get; set; } = null!;

//    public virtual TrainingProgram Program { get; set; } = null!;

//    public virtual SubMuscle? SubMuscle { get; set; }

//    public virtual Trainee Trainee { get; set; } = null!;
//}
