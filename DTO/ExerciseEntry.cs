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
        //בעיקרון צריך להיות כאן 
        //public int TraineeId { get; set; }//מתאמן 
        public int ExerciseId { get; set; }//תרגיל 
        public int OrderInList { get; set; }//מיקום בתוכנית אימון
        // מצביע לסלוט שבו התרגיל נמצא
        //public QueueSlot Slot { get; set; }
        //public Slot Slot { get; set; }

        // רשימת סלוטים (אם תרגיל יכול לתפוס יותר מסלוט אחד)
        public List<Slot> Slots { get; set; } = new List<Slot>();

        // אופציונלי: שמירת מידע נוסף
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; } // זמן סיום התרגיל
    }
}
