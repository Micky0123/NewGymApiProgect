using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class TrainingDay
{
    public int TrainingDaysId { get; set; }

    public int MinNumberDays { get; set; }

    public int MaxNumberDays { get; set; }
}
