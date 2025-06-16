using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class NextExerciseResponse
    {
        public int TraineeId { get; set; } // המתאמן
        public ExerciseEntry NextExercise { get; set; }
        public bool IsWorkoutComplete { get; set; }
        public string Message { get; set; }
        public int RemainingExercisesCount { get; set; } // כמה תרגילים נותרו
    }
}
