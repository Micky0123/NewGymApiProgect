using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    //תרגילים שבתוכנית אימון של מתאמן
    public class ExerciseEntry
    {
        public int ExerciseId { get; set; }//תרגיל 
        public int OrderInList { get; set; }//מיקום בתוכנית אימון
        public List<Slot> Slots { get; set; } = new List<Slot>();// רשימת סלוטים (אם תרגיל יכול לתפוס יותר מסלוט אחד)
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; } // זמן סיום התרגיל

        public int OriginalExercise { get; set; }// מספר התרגיל ברשימה של המתאמן
        public bool IsDone { get; set; }//  האם התרגיל בוצע

        public ExercisePlanDTO ExerciseDetails { get; set; } // פרטי תרגיל בסיסיים כמו שם התרגיל
    }
}

// מצביע לסלוט שבו התרגיל נמצא
//public QueueSlot Slot { get; set; }
//public Slot Slot { get; set; }
