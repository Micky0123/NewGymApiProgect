using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class RegisterRequest
    {
        public string IdNumber { get; set; }
        public string TraineeName { get; set; }
        public int Age { get; set; }
        public double TraineeWeight { get; set; }
        public double TraineeHeight { get; set; } 
        public int Gender { get; set; } 
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }

        public int TrainingDays { get; set; }
        public int TrainingDuration { get; set; }
        public int GoalId { get; set; }
        public int FitnessLevelId { get; set; }
    }
}
