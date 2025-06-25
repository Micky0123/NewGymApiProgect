using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    // זהו ה-DTO שיחזיק את כלל המידע על תוכנית האימון הפעילה עבור המתאמן,
    // כולל רשימת ימי האימון בפורמט שה-Frontend צריך.
    public class ActiveTrainingPlanResponse
    {
        public int TrainingPlanId { get; set; }
        public int TraineeId { get; set; } // נוסיף את זה לנוחות, למרות שזה לא היה במבנה הקודם
        public string TraineeName { get; set; } = null!; // שם המתאמן, כדי להציג אותו ב-Frontend
        public int GoalId { get; set; }
        public int TrainingDays { get; set; }
        public int TrainingDurationId { get; set; }
        public int FitnessLevelId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }

        // ***** כאן נשתמש ברשימה של DTOs חדשים לפרונטאנד *****
        public List<PlanDayResponseForFrontend> PlanDays { get; set; } = new List<PlanDayResponseForFrontend>();
    }
}
