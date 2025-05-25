using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ExerciseStatusEntry
    {
        public int ExerciseId { get; set; }
        public int OrderInList { get; set; }
        public bool IsDone { get; set; }
        public DateTime? PerformedAt { get; set; } // אופציונלי
    }
}
