using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ExerciseDTO
    {
        public int ExerciseId { get; set; }

        public string? ExerciseName { get; set; }

        public bool? Active { get; set; }

        public int? MuscleId { get; set; }

        public int? MuscleTypeId { get; set; }

        public int? MuscleGroupId { get; set; }
        // אם דרוש, תוסיף:
        public TimeSpan? Duration { get; set; }
    }
}
