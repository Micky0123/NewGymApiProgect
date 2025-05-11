using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IProgramExerciseDAL
    {
        Task AddProgramExerciseAsync(ProgramExercise programExercise);
        Task<List<ProgramExercise>> GetAllProgramExercisesAsync();
        Task<ProgramExercise> GetProgramExerciseByIdAsync(int id);
       // Task<ProgramExercise> GetProgramExerciseByNameAsync(string name);
        Task UpdateProgramExerciseAsync(ProgramExercise programExercise, int id);
        Task DeleteProgramExerciseAsync(int id);
        Task<List<Exercise>> GetExercisesForMuscleAsync(string muscleName);
        Task<int> SaveTrainingProgramAsync(TrainingProgram trainingProgram);
        Task SaveProgramExercisesAsync(List<ProgramExercise> programExercises);

    }
}
