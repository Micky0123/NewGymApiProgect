using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Trainee
{
    public int TraineeId { get; set; }

    public string Idnumber { get; set; } = null!;

    public string TraineeName { get; set; } = null!;

    public int Age { get; set; }

    public decimal TraineeWeight { get; set; }

    public decimal TraineeHeight { get; set; }

    public string Gender { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public bool IsAdmin { get; set; }

    public string Password { get; set; } = null!;

    public DateTime LoginDateTime { get; set; }

    public virtual ICollection<TrainingPlan> TrainingPlans { get; set; } = new List<TrainingPlan>();
}
