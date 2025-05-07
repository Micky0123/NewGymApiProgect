using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class FitnessLevelDTO
    {
        public int FitnessLevelId { get; set; }

        public string FitnessLevelName { get; set; } = null!;

        public string Description { get; set; }

    }
}
