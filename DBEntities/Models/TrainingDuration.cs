using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class TrainingDuration
{
    public int TrainingDurationId { get; set; }

    public int TimeTrainingDuration { get; set; }

    public virtual ICollection<DefaultProgram> DefaultPrograms { get; set; } = new List<DefaultProgram>();
}
