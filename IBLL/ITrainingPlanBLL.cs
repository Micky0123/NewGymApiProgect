using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface ITrainingPlanBLL
    {
        Task <int> AddTrainingPlanAsync(TrainingPlanDTO trainingPlan);
        //Task <int> AddTrainingPlanAsync(TrainingPlanDTO trainingPlan);
        Task<List<TrainingPlanDTO>> GetAllTrainingPlansAsync();
        Task<TrainingPlanDTO> GetTrainingPlanByIdAsync(int id);
        //Task<TrainingPlanDTO> GetTrainingPlanByNameAsync(string name);
        Task UpdateTrainingPlanAsync(TrainingPlanDTO trainingPlan, int id);
        Task DeleteTrainingPlanAsync(int id);
    }
}
