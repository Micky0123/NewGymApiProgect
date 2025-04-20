using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface ISubMuscleBLL
    {
        Task AddSubMuscleAsync(SubMuscleDTO subMuscle);
        Task<List<SubMuscleDTO>> GetAllSubMusclesAsync();
        Task<SubMuscleDTO> GetSubMuscleByIdAsync(int id);
        Task<SubMuscleDTO> GetSubMuscleByNameAsync(string name);
        Task<List<SubMuscleDTO>> GetAllMuscleByMuscleIdAsync(int id);
        Task UpdateSubMuscleAsync(SubMuscleDTO subMuscle, int id);
        Task DeleteSubMuscleAsync(int id);
    }
}
