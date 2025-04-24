using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class ProgramChangeDTO
    {
        public int ProgramChangeId { get; set; }

        public int ProgramId { get; set; }

        public string ChangeDescription { get; set; } = null!;

        public DateTime ChangeDateTime { get; set; }

        public virtual TrainingProgram Program { get; set; } = null!;
    }
}
