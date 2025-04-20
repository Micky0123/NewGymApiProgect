using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IExerciseBLL
    {
        Task AddExerciseAsync(ExerciseDTO exercise);
        Task<List<ExerciseDTO>> GetAllExercisesAsync();
        Task<ExerciseDTO> GetExerciseByIdAsync(int id);
        Task<ExerciseDTO> GetExerciseByNameAsync(string name);

        Task UpdateExerciseAsync(ExerciseDTO exercise, int id);
        Task DeleteExerciseAsync(int id);
    }
}
