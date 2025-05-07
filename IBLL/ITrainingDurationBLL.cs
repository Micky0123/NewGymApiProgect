using DBEntities.Models;
using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface ITrainingDurationBLL
    {
        Task AddTrainingDurationAsync(TrainingDurationDTO trainingDuration);
        Task<List<TrainingDurationDTO>> GetAllTrainingDurationAsync();
        Task<TrainingDurationDTO> GetTrainingDurationByIdAsync(int id);
        Task UpdateTrainingDurationAsync(TrainingDurationDTO trainingDuration, int id);
        Task DeleteTrainingDurationAsync(int id);
    }
}
