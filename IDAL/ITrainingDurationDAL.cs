using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface ITrainingDurationDAL
    {
        Task AddTrainingDurationAsync(TrainingDuration trainingDuration);
        Task<List<TrainingDuration>> GetAllTrainingDurationAsync();
        Task<TrainingDuration> GetTrainingDurationByIdAsync(int id);
        Task UpdateTrainingDurationAsync(TrainingDuration trainingDuration, int id);
        Task DeleteTrainingDurationAsync(int id);
    }
}
