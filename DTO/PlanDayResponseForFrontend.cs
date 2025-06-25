using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    // זהו ה-DTO  שנשלח ל-Frontend עבור כל יום אימון.
    // הוא מכיל רק את השדות שה-Frontend צריך לצורך תצוגה והלוגיקה.
    public class PlanDayResponseForFrontend
    {
        public int PlanDayId { get; set; }
        public string ProgramName { get; set; } = null!;
        public int DayOrder { get; set; }
        public bool IsDefaultProgram { get; set; } // עדיין חשוב ל-Frontend לדעת אם זו תוכנית דיפולטיבית
        public bool IsCompletedThisWeek { get; set; }// יציין האם תוכנית זו (הדיפולטיבית) הושלמה כבר בשבוע הנוכחי
        public int TrainingPlanId { get; set; }
        public DateTime CreationDate { get; set; }
        public int? ParentProgramId { get; set; }
        public bool IsHistoricalProgram { get; set; }
    }
}
