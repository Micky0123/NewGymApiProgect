using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IFitnessLevelBLL
    {
        Task AddFitnessLevelAsync(FitnessLevelDTO fitnessLevel);
        Task<List<FitnessLevelDTO>> GetAllFitnessLevelAsync();
        Task<FitnessLevelDTO> GetFitnessLevelByIdAsync(int id);
        Task UpdateFitnessLevelAsync(FitnessLevelDTO fitnessLevel, int id);
        Task DeleteFitnessLevelAsync(int id);
    }
}
