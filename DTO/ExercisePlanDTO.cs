using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ExercisePlanDTO
    {
        public int ExercisePlanId { get; set; }

        public int PlanDayId { get; set; }

        public int ExerciseId { get; set; }

        public int PlanSets { get; set; }

        public int PlanRepetitionsMin { get; set; }

        public decimal PlanWeight { get; set; }

        public int PlanRepetitionsMax { get; set; }

        public int CategoryId { get; set; }

        public int TimesMin { get; set; }

        public int TimesMax { get; set; }

        public int? SubMuscleId { get; set; }

        public int IndexOrder { get; set; }

        public DateTime TrainingDateTime { get; set; }

    }
}
