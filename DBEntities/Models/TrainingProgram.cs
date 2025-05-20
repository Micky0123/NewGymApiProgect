//using System;
//using System.Collections.Generic;

//namespace DBEntities.Models;

//public partial class TrainingProgram
//{
//    public int ProgramId { get; set; }

//    public int TraineeId { get; set; }

//    public string ProgramName { get; set; } = null!;

//    public DateTime CreationDate { get; set; }

//    public DateTime TrainingDateTime { get; set; }

//    public DateTime? LastUpdateDate { get; set; }

//    public bool? IsDefaultProgram { get; set; }

//    public int? ParentProgramId { get; set; }

//    public bool? IsHistoricalProgram { get; set; }

//    public virtual ICollection<DefaultProgram> DefaultPrograms { get; set; } = new List<DefaultProgram>();

//    public virtual ICollection<TrainingProgram> InverseParentProgram { get; set; } = new List<TrainingProgram>();

//    public virtual ICollection<MonthlyProgram> MonthlyPrograms { get; set; } = new List<MonthlyProgram>();

//    public virtual TrainingProgram? ParentProgram { get; set; }

//    public virtual ICollection<ProgramChange> ProgramChanges { get; set; } = new List<ProgramChange>();

//    public virtual ICollection<ProgramExercise> ProgramExercises { get; set; } = new List<ProgramExercise>();

//    public virtual ICollection<RealTimeTraining> RealTimeTrainings { get; set; } = new List<RealTimeTraining>();

//    public virtual Trainee Trainee { get; set; } = null!;
//}
