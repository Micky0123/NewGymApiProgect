using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{

    public interface TrainingParams
    {
        List<List<object>> DayLists { get; set; }
        List<object> LargeMuscleList { get; set; }
        List<object> SmallMuscleList { get; set; }
        int MinRep { get; set; }
        int MaxRep { get; set; }
        int LargeMuscleCount { get; set; }
        int SmallMuscleCount { get; set; }
        Dictionary<int, int> TimeCategoryList { get; set; }
        List<object> OrderOfMuscle { get; set; }
    }
    public interface IProgramExerciseBLL
    {
        
        Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise);
        Task<List<ProgramExerciseDTO>> GetAllProgramExercisesAsync();
        Task<ProgramExerciseDTO> GetProgramExerciseByIdAsync(int id);
        Task<ProgramExerciseDTO> GetProgramExerciseByNameAsync(string name);

        Task UpdateProgramExerciseAsync(ProgramExerciseDTO programExercise, int id);
        Task DeleteProgramExerciseAsync(int id);

        Task ReadDataFromExcelAsync(string filePath);
        Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time);
        Task<List<ExerciseDTO>> GetExercisesForMuscleAsync(string muscleName, int count);
        Task<List<List<ExerciseDTO>>> GenerateExercisePlanAsync(TrainingParams trainingParams);
    }
}
