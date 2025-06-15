using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    // מחלקה פנימית לתוצאה - מייצגת מסלול תרגילים עם זמן התחלה וסיום
    public class PathResult
    {
        public TraineeDTO Trainee { get; set; }
        public Dictionary< int,ExerciseEntry> ExerciseIdsInPath { get; set; } = new();
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int AlternativesUsed { get; set; } = 0; // מספר החילופים שנעשו
    }
}
