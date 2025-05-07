using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IFitnessLevelDAL
    {
        Task AddFitnessLevelAsync(FitnessLevel fitnessLevel);
        Task<List<FitnessLevel>> GetAllFitnessLevelAsync();
        Task<FitnessLevel> GetFitnessLevelByIdAsync(int id);
        Task UpdateFitnessLevelAsync(FitnessLevel fitnessLevel, int id);
        Task DeleteFitnessLevelAsync(int id);
    }
}
