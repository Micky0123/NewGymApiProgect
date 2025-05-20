using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PlanDayDTO
    {
        public int PlanDayId { get; set; }

        public int TrainingPlanId { get; set; }

        public string ProgramName { get; set; } = null!;

        public int DayOrder { get; set; }

        public DateTime CreationDate { get; set; }

        public bool IsDefaultProgram { get; set; }

        public int? ParentProgramId { get; set; }

        public bool IsHistoricalProgram { get; set; }
    }
}
