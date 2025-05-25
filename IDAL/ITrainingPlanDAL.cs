using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface ITrainingPlanDAL
    {
       // Task AddTrainingPlanAsync(TrainingPlan trainingPlan);
        Task <int> AddTrainingPlanAsync(TrainingPlan trainingPlan);
        Task<List<TrainingPlan>> GetAllTrainingPlansAsync();
        Task<TrainingPlan> GetTrainingPlanByIdAsync(int id);
       // Task<TrainingPlan> GetTrainingPlanByNameAsync(string name);
        Task UpdateTrainingPlanAsync(TrainingPlan trainingPlan, int id);
        Task DeleteTrainingPlanAsync(int id);
    }
}
