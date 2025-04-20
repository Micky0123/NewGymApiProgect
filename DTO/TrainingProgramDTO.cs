using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class TrainingProgramDTO
    {
        public int ProgramId { get; set; }

        public int TraineeId { get; set; }

        public string ProgramName { get; set; } = null!;

        public DateTime CreationDate { get; set; }

        public DateTime TrainingDateTime { get; set; }
    }
}
