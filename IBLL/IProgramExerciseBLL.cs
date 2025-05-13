using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public class DayEntry
    {
        //קטגוריה
        public string Key { get; set; }
        //מספר התרגילים לשריר 
        public int Values { get; set; }
        //שם השריר
        public string Name { get; set; }
    }

    public class ExerciseWithMuscleInfo
    {
        public ExerciseDTO Exercise { get; set; }
        public string MuscleName { get; set; }
        public string SubMuscleName { get; set; }
        public int JointCount { get; set; } // Add JointCount property
    }
    public class TrainingParams
    {
        public List<List<DayEntry>> DayLists { get; set; } // רשימה של רשימות אובייקטים מסוג DayEntry
        public Dictionary<string, List<object>> MuscleSizeData { get; set; } // מילון דינאמי לגודל שרירים
        public int MinRep { get; set; }
        public int MaxRep { get; set; }
        public Dictionary<(string Category, string MuscleSize), int> TimeCategories { get; set; }
        public List<string> TypMuscle { get; set; }//רשימה של סוגי שרירים
        public List<Dictionary<int, List<string>>> TypeMuscleData { get; set; }//רשימה של סוגי השרירים לאימון מסודרת לפי חשיבות
        public List<string> equipment { get; set; }//רשימה של ציוד למתאמן
        public List<string> NeedSubMuscleList { get; set; }//רשימה של סוגי השרירים שצריכים תת שריר
        public List<string> subMuscleOfMuscleList { get; set; }//רשימה של כל השרירים שיש להם תת שריר
        public Dictionary<int, string> OrderList { get; set; }//דיקשינרי של סדר התרגילים בתוכנית
        public List<string> musclePriorityOrder { get; set; }//רשימה של כל השרירים שיש להם תת שריר
        public List<string> subMusclePriorityOrder { get; set; }//רשימה של כל השרירים שיש להם תת שריר
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
       // Task<List<List<ExerciseDTO>>> AddProgramExerciseAsync(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time);
        Task addProgramExerciseAsync1(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time, int traineeID);
        Task<List<ExerciseDTO>> GetExercisesForMuscleAsync(string muscleName, int count);
        // Task<List<List<ExerciseDTO>>> GenerateExercisePlanAsync(TrainingParams trainingParams);

        //    Task<List<List<List<ExerciseDTO>>>> GenerateExercisePlanAsync(TrainingParams trainingParams);
        Task SaveDefaultProgramAsync(TrainingParams trainingParams, int traineeId, string programName);
    }
}
