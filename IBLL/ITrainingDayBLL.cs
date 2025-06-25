using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface ITrainingDayBLL
    {
        Task AddTrainingDayAsync(TrainingDayDTO trainingDay);
        Task<List<TrainingDayDTO>> GetAllTrainingDaysAsync();
        Task<TrainingDayDTO> GetTrainingDayByIdAsync(int id);
        Task UpdateTrainingDayAsync(TrainingDayDTO trainingDay, int id);
        Task DeleteTrainingDayAsync(int id);
    }
}
