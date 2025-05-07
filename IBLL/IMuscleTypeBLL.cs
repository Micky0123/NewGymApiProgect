using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IMuscleTypeBLL
    {
        Task AddMuscleTypeAsync(MuscleTypeDTO muscleType);
        Task<List<MuscleTypeDTO>> GetAllMusclesTypeAsync();
        Task<MuscleTypeDTO> GetMuscleTypeByIdAsync(int id);
        Task<MuscleTypeDTO> GetMuscleTypeByNameAsync(string name);
        Task UpdateMuscleTypeAsync(MuscleTypeDTO muscleType, int id);
        Task DeleteMuscleTypeAsync(int id);
    }
}
