using System;
using System.Collections.Generic;

namespace DBEntities.Models;

public partial class Equipment
{
    public int EquipmentId { get; set; }

    public string EquipmentName { get; set; } = null!;

    public virtual ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
}
