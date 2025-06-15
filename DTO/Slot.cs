using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class Slot
    {
        public DateTime StartTime { get; set; }   // זמן התחלה
        public DateTime EndTime { get; set; }     // זמן סיום
        public Dictionary<int, PathResult> ExercisesByTrainee { get; set; } = new();
        public bool available { get; set; } = true; //זמינות התרגיל
        public long MaxCapacity { get; set; }//הגדרת משך הזמן של כל סלוט

        public Slot( DateTime startTime, DateTime endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
            MaxCapacity= this.EndTime.Ticks - this.StartTime.Ticks;
        }
        public void AddTranee(TraineeDTO trainee, PathResult pathResult)
        {
            if (!ExercisesByTrainee.ContainsKey(trainee.TraineeId))
            {
                ExercisesByTrainee.Add(trainee.TraineeId, pathResult);
            }
        }

    }
}

//public bool TryAddExercise(ExerciseEntry entry)
//{
//    if (Exercises.Count < MaxCapacity)
//    {
//        Exercises.Add(entry);
//        return true;
//    }
//    return false; // מלא
//}
