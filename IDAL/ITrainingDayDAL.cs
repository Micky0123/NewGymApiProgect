using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface ITrainingDayDAL
    {
        Task AddTrainingDayAsync(TrainingDay trainingDay);
        Task<List<TrainingDay>> GetAllTrainingDaysAsync();
        Task<TrainingDay> GetTrainingDayByIdAsync(int id);
       // Task<TrainingDay> GetTrainingDayByNameAsync(string name);
        Task UpdateTrainingDayAsync(TrainingDay trainingDay, int id);
        Task DeleteTrainingDayAsync(int id);
    }
}
