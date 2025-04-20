using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface ITrainingProgramDAL
    {
        Task AddTrainingProgramAsync(TrainingProgram trainingProgram);
        Task<List<TrainingProgram>> GetAllTrainingProgramsAsync();
        Task<TrainingProgram> GetTrainingProgramByIdAsync(int id);
        Task<TrainingProgram> GetTrainingProgramByNameAsync(string name);
        Task UpdateTrainingProgramAsync(TrainingProgram trainingProgram, int id);
        Task DeleteTrainingProgramAsync(int id);
    }
}
