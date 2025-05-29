using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class TraineeExerciseStatus
    {
        public TraineeDTO Trainee { get; set; }
        public List<ExerciseStatusEntry> Exercises { get; set; } = new();
        public int planDayId { get; set; }


        // לשימוש ב-Undo:
        public Stack<List<ExerciseStatusEntry>> History { get; set; } = new();
    }
}
