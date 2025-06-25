using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IPlanDayDAL
    {
        Task <int> AddPlanDayAsync(PlanDay planDay);

        Task<List<PlanDay>> GetAllPlanDaysAsync();
        Task<PlanDay> GetPlanDayByIdAsync(int id);
        Task UpdatePlanDayAsync(PlanDay planDay, int id);
        Task DeletePlanDayAsync(int id);
        Task<List<PlanDay>> GetPlanDaysByTrainingPlanIdAndNotHistorical(int trainingPlanId);
        Task<List<PlanDay>> GetPlanDaysByTrainingPlanIdAndHistorical(int trainingPlanId);
        //
        Task<TrainingPlan?> GetActiveTrainingPlanByTraineeIdAsync(int traineeId);
        Task<List<PlanDay>> GetDefaultPlanDaysByTrainingPlanIdAsync(int trainingPlanId);
        Task<PlanDay?> GetLastCompletedHistoricalPlanDayForParentAndTrainingPlanAsync(int parentPlanDayId, int trainingPlanId);

    }
}
