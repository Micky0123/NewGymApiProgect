using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    // זהו ה-DTO שאנו נשלח ל-Frontend עבור כל יום אימון.
    // הוא מכיל רק את השדות שה-Frontend צריך לצורך תצוגה והלוגיקה שלנו.
    public class PlanDayResponseForFrontend
    {
        public int PlanDayId { get; set; }
        public string ProgramName { get; set; } = null!;
        public int DayOrder { get; set; }
        public bool IsDefaultProgram { get; set; } // עדיין חשוב ל-Frontend לדעת אם זו תוכנית דיפולטיבית

        // ***** זהו השדה המחושב החדש! *****
        // יציין האם תוכנית זו (הדיפולטיבית) הושלמה כבר בשבוע הנוכחי
        public bool IsCompletedThisWeek { get; set; }

        // הוסף את המאפיינים הבאים אם הם נדרשים ב-Frontend לצורך לוגיקה כלשהי
        // או אם ה-PlanDayDTO מכיל אותם ואתה רוצה שיהיו זמינים כאן.
        // אם לא, אל תכלול אותם, כדי לשמור על DTO רזה.
        public int TrainingPlanId { get; set; }
        public DateTime CreationDate { get; set; }
        public int? ParentProgramId { get; set; }
        public bool IsHistoricalProgram { get; set; }
    }
}
