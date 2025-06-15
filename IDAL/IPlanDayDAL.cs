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
        //Task AddPlanDayAsync(PlanDay planDay);
        Task <int> AddPlanDayAsync(PlanDay planDay);

        Task<List<PlanDay>> GetAllPlanDaysAsync();
        Task<PlanDay> GetPlanDayByIdAsync(int id);
       // Task<PlanDay> GetPlanDayByNameAsync(string name);
        Task UpdatePlanDayAsync(PlanDay planDay, int id);
        Task DeletePlanDayAsync(int id);
        Task<List<PlanDay>> GetPlanDaysByTrainingPlanIdAndNotHistorical(int trainingPlanId);
        Task<List<PlanDay>> GetPlanDaysByTrainingPlanIdAndHistorical(int trainingPlanId);
    }
}
