using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ExerciseEntry
    {
        //בעיקרון צריך להיות כאן 
        //public int TraineeId { get; set; }//מתאמן 
        public int ExerciseId { get; set; }//תרגיל 
        public int OrderInList { get; set; }//מיקום בתוכנית אימון
        // מצביע לסלוט שבו התרגיל נמצא
        public QueueSlot Slot { get; set; }
    }
}
