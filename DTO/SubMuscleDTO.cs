using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class SubMuscleDTO
    {
        public int SubMuscleId { get; set; }

        public string SubMuscleName { get; set; }

        public int MuscleId { get; set; }

        public virtual Muscle Muscle { get; set; } = null!;
    }
}
