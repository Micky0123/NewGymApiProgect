using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Trainee
{
    public int TraineeId { get; set; }

    public string? Idnumber { get; set; }

    public string? TraineeName { get; set; }

    public int? Age { get; set; }

    public decimal? TraineeWeight { get; set; }

    public decimal? TraineeHeight { get; set; }

    public string? Gender { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public bool IsAdmin { get; set; }

    public string Password { get; set; } = null!;

    public DateTime? LoginDateTime { get; set; }

    public virtual ICollection<MonthlyProgram> MonthlyPrograms { get; set; } = new List<MonthlyProgram>();

    public virtual ICollection<RealTimeTraining> RealTimeTrainings { get; set; } = new List<RealTimeTraining>();

    public virtual ICollection<TrainingProgram> TrainingPrograms { get; set; } = new List<TrainingProgram>();

    public virtual ICollection<Goal> Goals { get; set; } = new List<Goal>();
}
