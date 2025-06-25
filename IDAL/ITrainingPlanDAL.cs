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
        Task <int> AddTrainingPlanAsync(TrainingPlan trainingPlan);
        Task<List<TrainingPlan>> GetAllTrainingPlansAsync();
        Task<TrainingPlan> GetTrainingPlanByIdAsync(int id);
        Task UpdateTrainingPlanAsync(TrainingPlan trainingPlan, int id);
        Task DeleteTrainingPlanAsync(int id);
        Task<TrainingPlan> GetActiveTrainingPlanWithDaysOfTrainee(int traineeId);
        Task<List<TrainingPlan>> GetAllHistoryTrainingPlansWithDaysOfTrainee(int traineeId);
        Task<TrainingPlan?> GetActiveTrainingPlanWithDetails(int traineeId);
    }
}
