using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IGoalBLL
    {
        Task AddGoalAsync(GoalDTO goal);
        Task<List<GoalDTO>> GetAllGoalsAsync();
        Task<GoalDTO> GetGoalByIdAsync(int id);
        Task<GoalDTO> GetGoalByNameAsync(string name);

        Task UpdateGoalAsync(GoalDTO goal, int id);
        Task DeleteGoalAsync(int id);
    }
}
