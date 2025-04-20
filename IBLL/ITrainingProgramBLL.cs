using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface ITrainingProgramBLL
    {
        Task AddTrainingProgramAsync(TrainingProgramDTO trainingProgram);
        Task<List<TrainingProgramDTO>> GetAllTrainingProgramsAsync();
        Task<TrainingProgramDTO> GetTrainingProgramByIdAsync(int id);
        Task<TrainingProgramDTO> GetTrainingProgramByNameAsync(string name);

        Task UpdateTrainingProgramAsync(TrainingProgramDTO trainingProgram, int id);
        Task DeleteTrainingProgramAsync(int id);
    }
}
