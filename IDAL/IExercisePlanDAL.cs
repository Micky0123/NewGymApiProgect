using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IExercisePlanDAL
    {
        Task AddExercisePlanAsync(ExercisePlan exercisePlan);
        Task<List<ExercisePlan>> GetAllExercisePlansAsync();
        Task<ExercisePlan> GetExercisePlanByIdAsync(int id);
        // Task<ExercisePlan> GetExercisePlanByNameAsync(string name);
        Task<List<ExercisePlan>> GetExercisesByPlanDayIdAsync(int planDayId);
        Task UpdateExercisePlanAsync(ExercisePlan exercisePlan, int id);
        Task DeleteExercisePlanAsync(int id);
    }
}
