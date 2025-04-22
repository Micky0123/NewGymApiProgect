using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
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
    }
}
