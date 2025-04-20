using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ProgramExerciseDTO
    {
        public int ProgramExerciseId { get; set; }

        public int ProgramId { get; set; }

        public int ExerciseId { get; set; }

        public int ProgramSets { get; set; }

        public int? ProgramRepetitionsMin { get; set; }

        public int? ProgramRepetitionsMax { get; set; }

        public decimal ProgramWeight { get; set; }
            
        public int ExerciseOrder { get; set; }
    }
}
