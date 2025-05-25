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
        public Dictionary<TraineeDTO, PathResult> ExercisesByTrainee { get; set; } = new();
       // public List<ExerciseEntry> Exercises { get; set; } = new List<ExerciseEntry>(); //רשימה של תרגילים כלומר מצביעים לתרגילים בתוכנית אימון 
        public bool available { get; set; } = true; //זמינות התרגיל
        //public int MaxCapacity { get; set; }//הגדרת משך הזמן של כל סלוט

        public Slot( DateTime startTime, DateTime endTime)
        {
            //MaxCapacity = maxCapacity;
            StartTime = startTime;
            EndTime = endTime;
        }
        public void AddTranee(TraineeDTO trainee)
        {
            ExercisesByTrainee.Add(trainee, null);
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
