using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class TraineeDTO
    {
        public enum EGender
        {
            זכר=1,
            נקבה
        }
        //public enum EFitnessLevel 
        //{
        //    מתחיל = 1,
        //    מתקדם
        //}


        public int TraineeId { get; set; }

        public string Idnumber { get; set; }

        public string TraineeName { get; set; }

        public int Age { get; set; }

        public decimal TraineeWeight { get; set; }

        public decimal TraineeHeight { get; set; }

        public EGender Gender { get; set; }

        public string Phone { get; set; }

        public string Email { get; set; }

        public bool IsAdmin { get; set; }

        public string Password { get; set; } = null!;

        public int TrainingDays { get; set; }

        public int TrainingDuration { get; set; }

        public int GoalId { get; set; }

        public DateTime LoginDateTime { get; set; }
        public int FitnessLevelId { get; set; }

    }
}
