using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IMuscleDAL
    {
        Task AddMuscleAsync(Muscle muscle);
        Task<List<Muscle>> GetAllMusclesAsync();
        Task<Muscle> GetMuscleByIdAsync(int id);
        Task<Muscle> GetMuscleByNameAsync(string name);
        int GetIdOfMuscleByNameAsync(string name);
        Task UpdateMuscleAsync(Muscle muscle, int id);
        Task DeleteMuscleAsync(int id);
        Task<List<Exercise>> GetExercisesForMuscleAsync(string muscleName);
        Task<List<Exercise>> GetExercisesForSubMuscleAsync(string subMuscleName);
        Task<List<Exercise>> GetExercisesForMuscleByCategoryAsync(string muscleName, string categoryName);
        Task<List<Exercise>> GetExercisesForMuscleAndCategoryAsync(string muscleName, string categoryName);
        // Task<List<Exercise>> GetExercisesForMuscleAndTypeAsync(string muscleName, string typeMuscle);
        Task <List<SubMuscle>> GetSubMusclesOfMuscaleAsync(Muscle muscle);
        Task<List<Exercise>> GetExercisesForMuscleAndTypeAsync(string muscleName, string TypeMuscle);

    }
}
