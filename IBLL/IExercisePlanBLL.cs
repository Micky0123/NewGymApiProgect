using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IExercisePlanBLL
    {
        Task AddExercisePlanAsync(ExercisePlanDTO exercisePlan);
        Task<List<ExercisePlanDTO>> GetAllExercisePlansAsync();
        Task<ExercisePlanDTO> GetExercisePlanByIdAsync(int id);
        //Task<ExercisePlanDTO> GetExercisePlanByNameAsync(string name);
        Task UpdateExercisePlanAsync(ExercisePlanDTO exercisePlan, int id);
        Task DeleteExercisePlanAsync(int id);
    }
}
