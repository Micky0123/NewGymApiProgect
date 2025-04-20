using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IGoalDAL
    {
        Task AddGoalAsync(Goal goal);
        Task<List<Goal>> GetAllGoalsAsync();
        Task<Goal> GetGoalByIdAsync(int id);
        Task<Goal> GetGoalByNameAsync(string name);
        Task UpdateGoalAsync(Goal goal, int id);
        Task DeleteGoalAsync(int id);
    }
}
