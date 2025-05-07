using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IMuscleTypeDAL
    {
        Task<MuscleType> GetMuscleTypeByIdAsync(int id);
        Task<List<MuscleType>> GetAllMuscleTypesAsync();
        Task AddMuscleTypeAsync(MuscleType muscleType);
        Task UpdateMuscleTypeAsync(MuscleType muscleType);
        Task DeleteMuscleTypeAsync(int id);
        //Task<MuscleType> GetMuscleTypeByIdAsync(int id);
    }
}
