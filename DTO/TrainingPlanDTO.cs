using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class TrainingPlanDTO
    {
        public int TrainingPlanId { get; set; }

        public int TraineeId { get; set; }

        public int GoalId { get; set; }

        public int TrainingDays { get; set; }

        public int TrainingDurationId { get; set; }

        public int FitnessLevelId { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; }

    }
}
