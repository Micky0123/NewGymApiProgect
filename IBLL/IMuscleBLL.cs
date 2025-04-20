using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IMuscleBLL
    {
        Task AddMuscleAsync(MuscleDTO muscle);
        Task<List<MuscleDTO>> GetAllMusclesAsync();
        Task<MuscleDTO> GetMuscleByIdAsync(int id);
        Task<MuscleDTO> GetMuscleByNameAsync(string name);
        Task UpdateMuscleAsync(MuscleDTO muscle, int id);
        Task DeleteMuscleAsync(int id);
    }
}
