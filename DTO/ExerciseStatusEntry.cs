using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ExerciseStatusEntry
    {
        public int OriginalExercise { get; set; }// מספר התרגיל ברשימה של המתאמן
        public int ExerciseId { get; set; }
        public int OrderInList { get; set; }// מיקום התרגיל ברשימה של המתאמן
        public bool IsDone { get; set; }//  האם התרגיל בוצע
        public DateTime? PerformedAt { get; set; } // 
        public DateTime? StartedAt { get; set; } // מתי התחיל התרגיל
    }
}
