using AutoMapper;
using DBEntities.Models;
using DTO;
using IBLL;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OfficeOpenXml;
using System.IO;
using System.Threading.Tasks;

using System.Collections.Generic;


namespace BLL
{
    //    public enum goal
    //    {
    //      1	ירידה במשקל
    //      2	עליה במשקל
    //      3	חיטוב
    //      4	העלאת מסת שריר
    //      5	שיפור כללי
    //    }
    //    public enum muscle
    //    {
    //      1	רגליים
    //      2	חזה
    //      3	גב
    //      4	כתפיים
    //      5	יד קדמית
    //      6	יד אחורית
    //      7	בטן
    //      8	זוקפי גו
    //     }

    //new
    public class ProgramExerciseBLL : IProgramExerciseBLL
    {
        private readonly IMuscleDAL muscleDAL;
        private readonly IGoalDAL goalDAL;
        private readonly IMapper mapper;
        public ProgramExerciseBLL(IMuscleDAL muscleDAL)
        {
            this.muscleDAL = muscleDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Muscle, MuscleDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise, string filePath, int daysInWeek, int goal, int level, int time)
        {
            using (var package = new ExcelPackage(new FileInfo(filePath)))
            {
                // פונקציית עזר למציאת גיליון
                ExcelWorksheet GetWorksheet(string sheetName)
                {
                    var worksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == sheetName);
                    if (worksheet == null)
                    {
                        Console.WriteLine($"הגיליון בשם '{sheetName}' לא נמצא.");
                    }
                    return worksheet;
                }

                // פונקציית עזר לקריאת ערכים מעמודה
                List<object> GetColumnValues(ExcelWorksheet worksheet, int column)
                {
                    var values = new List<object>();
                    for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                    {
                        values.Add(worksheet.Cells[row, column].Value);
                    }
                    return values;
                }

                // יצירת רשימות של שרירים לכל יום מימי האימון
                var daysWorksheet = GetWorksheet("DaysInWeek");
                if (daysWorksheet == null) return;

                var dayLists = new List<List<object>>();
                for (int col = 1; col <= daysWorksheet.Dimension.End.Column; col++)
                {
                    if (int.Parse(daysWorksheet.Cells[1, col].Text) == daysInWeek)
                    {
                        dayLists.Add(GetColumnValues(daysWorksheet, col));
                    }
                }

                // יצירת רשימה של שרירים גדולים
                var largeMuscleWorksheet = GetWorksheet("LargeMuscle");
                if (largeMuscleWorksheet == null) return;

                var largeMuscleList = GetColumnValues(largeMuscleWorksheet, 1);

                // יצירת רשימה של שרירים קטנים
                var smallMuscleWorksheet = GetWorksheet("SmallMuscle");
                if (smallMuscleWorksheet == null) return;

                var smallMuscleList = GetColumnValues(smallMuscleWorksheet, 1);

                // מספר החזרות לפי המטרה
                var repWorksheet = GetWorksheet("RepitByGoal");
                if (repWorksheet == null) return;

                int colRep = FindColumnByValue(repWorksheet, 1, goal, "לא נמצא עמודה עם הערך GOAL בשורה 1.");
                if (colRep == -1) return;

                int minRep = GetValueFromWorksheet(repWorksheet, "min", colRep, "לא נמצא ערך 'min' בעמודה.");
                int maxRep = GetValueFromWorksheet(repWorksheet, "max", colRep, "לא נמצא ערך 'max' בעמודה.");

                // כמות השרירים
                var countToMuscleWorksheet = GetWorksheet("SumOfLargeByTime");
                if (countToMuscleWorksheet == null) return;

                int colCount = FindColumnByValue(countToMuscleWorksheet, 1, time, "לא נמצאה עמודה עם הזמן.");
                if (colCount == -1) return;

                int rowCount = FindRowByValue(countToMuscleWorksheet, 1, goal, "לא נמצא שורה עם הערך GOAL.");
                if (rowCount == -1) return;

                int largeMuscleCount = (short)(countToMuscleWorksheet.Cells[rowCount, colCount].Value);

                // דיקשינרי של קטגוריות וזמן אימון
                var categoryWorksheet = GetWorksheet("Category");
                if (categoryWorksheet == null) return;

                int categoryCount = categoryWorksheet.Dimension.End.Row;

                var timeCategoryWorksheet = GetWorksheet("TimeCategory");
                if (timeCategoryWorksheet == null) return;

                var timeCategoryList = new Dictionary<int, int>();
                int rowTimeCategory = FindRowByValue(timeCategoryWorksheet, 1, daysInWeek, "לא נמצא שורה עם ערך הימים.");
                if (rowTimeCategory == -1) return;

                for (int colT = 2; colT <= categoryCount; colT++)
                {
                    timeCategoryList.Add(colT, (short)(timeCategoryWorksheet.Cells[rowTimeCategory, colT].Value));
                }
            }
        }

        // פונקציית עזר למציאת עמודה לפי ערך
        private int FindColumnByValue(ExcelWorksheet worksheet, int headerRow, int value, string errorMessage)
        {
            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
            {
                if (worksheet.Cells[headerRow, col].GetValue<int>() == value) return col;
            }
            Console.WriteLine(errorMessage);
            return -1;
        }

        // פונקציית עזר למציאת שורה לפי ערך
        private int FindRowByValue(ExcelWorksheet worksheet, int column, int value, string errorMessage)
        {
            for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
            {
                if (worksheet.Cells[row, column].GetValue<int>() == value) return row;
            }
            Console.WriteLine(errorMessage);
            return -1;
        }

        // פונקציית עזר לקריאת ערך לפי שורה וטקסט
        private int GetValueFromWorksheet(ExcelWorksheet worksheet, string rowText, int column, string errorMessage)
        {
            for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
            {
                if (worksheet.Cells[row, 1].GetValue<string>() == rowText)
                {
                    return worksheet.Cells[row, column].GetValue<int>();
                }
            }
            Console.WriteLine(errorMessage);
            return -1;
        }

        public Task<List<ProgramExerciseDTO>> GetAllProgramExercisesAsync()
        {

            throw new NotImplementedException();
        }

        public Task<ProgramExerciseDTO> GetProgramExerciseByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<ProgramExerciseDTO> GetProgramExerciseByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task UpdateProgramExerciseAsync(ProgramExerciseDTO programExercise, int id)
        {
            throw new NotImplementedException();
        }

        public Task DeleteProgramExerciseAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise)
        {
            throw new NotImplementedException();
        }

        public Task ReadDataFromExcelAsync(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}



//public async Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time)
//{
//    // this parameter is the number of days in a week that the user wants to train
//    int SmallMuscle;//הכמות של תרגילים לשרירים קטנים
//    int LargeMuscle;//הכמות של תרגילים לשרירים גדולים
//    int minRep = 0;//הכמות של חזרות מינימליות
//    int maxRep = 0;//הכמות של חזרות המקסימליות
//    int warm;//זמן לחימום
//    int power;//זמן לאימון כוח
//    int Cardio;//זמן לאירובי
//    List<string> muscleGroups1 = new List<string>();
//    List<string> muscleGroups2 = new List<string>();
//    List<string> muscleGroups3 = new List<string>();
//    List<string> muscleGroups4 = new List<string>();
//    bool isAerobic = false;




//    // Set the number of repetitions based on the goal
//    switch (goal)
//    {
//        case 1:
//            //load the num of repititions from the excel file in num of days is 1

//            minRep = 8;
//            maxRep = 12;
//            break;
//        case 2:
//            minRep = 8;
//            maxRep = 12;
//            break;
//        case 3:
//            minRep = 6;
//            maxRep = 8;
//            break;
//        case 4:
//            minRep = 6;
//            maxRep = 12;
//            break;
//        case 5:
//            minRep = 5;
//            maxRep = 10;
//            break;
//        default:
//            throw new ArgumentException("Invalid goal");
//    }

//    //list of muscles to 1-3 days

//    if (daysInWeek >= 1 & daysInWeek <= 3)
//    {
//        muscleGroups1.Add("רגליים");
//        muscleGroups1.Add("חזה");
//        muscleGroups1.Add("גב");
//        muscleGroups1.Add("כתפיים");
//        muscleGroups1.Add("יד קדמית");
//        muscleGroups1.Add("יד אחורית");
//        muscleGroups1.Add("בטן");
//        muscleGroups1.Add("זוקפי גו");

//        //במידה וזמן האימון הוא 40 דקות לכל שריר גדול 2 תרגילים 
//        //לשרירים קטנים תרגיל אחד
//        if (time == 40)
//        {
//            SmallMuscle = 1;
//            LargeMuscle = 2;

//            warm = 5;
//            power = 20;
//            Cardio = 20;
//            if (await goalDAL.GetIdOfGoalByNameAsync("עליה במשקל") == goal)
//            {
//                Cardio = 15;
//            }
//        }
//        //במידה וזמן האימון הוא 60 דקות לכל שריר גדול 3 תרגילים
//        //לשרירים קטנים 2 תרגילים
//        else if (time == 60)
//        {
//            SmallMuscle = 2;
//            LargeMuscle = 3;

//            warm = 10;
//            power = 40;
//            Cardio = 40;
//        }
//        else
//        {
//            throw new ArgumentException("Invalid time");
//        }
//    }
//    else if (daysInWeek == 4)
//    {
//        // Add logic for 4 days

//        isAerobic = true;
//        muscleGroups1.Add("רגליים");
//        muscleGroups3.Add("חזה");
//        muscleGroups2.Add("גב");
//        muscleGroups2.Add("כתפיים");
//        muscleGroups3.Add("יד קדמית");
//        muscleGroups3.Add("יד אחורית");
//        muscleGroups1.Add("בטן");
//        muscleGroups1.Add("זוקפי גו");

//        //במידה וזמן האימון הוא 40 דקות לכל שריר גדול 6 תרגילים
//        //לשרירים קטנים 2 תרגילים
//        if (time == 40)
//        {
//            SmallMuscle = 2;
//            LargeMuscle = 6;
//        }
//        //(לשים לב ליום 1 של אימון זה...)
//        //במידה וזמן האימון הוא 60 דקות לכל שריר גדול 8 תרגילים
//        //לשרירים קטנים 4 תרגילים
//        else if (time == 60)
//        {
//            SmallMuscle = 4;
//            LargeMuscle = 8;
//        }
//        else
//        {
//            throw new ArgumentException("Invalid time");
//        }
//    }
//    else if (daysInWeek == 5)
//    {
//        // Add logic for 5 days 
//        isAerobic = true;
//        muscleGroups1.Add("רגליים");
//        muscleGroups3.Add("חזה");
//        muscleGroups2.Add("גב");
//        muscleGroups4.Add("כתפיים");
//        muscleGroups3.Add("יד קדמית");
//        muscleGroups4.Add("יד אחורית");
//        muscleGroups1.Add("בטן");
//        muscleGroups2.Add("זוקפי גו");

//        //במידה וזמן האימון הוא 40 דקות לכל שריר גדול 6 תרגילים
//        //לשרירים קטנים 2 תרגילים
//        if (time == 40)
//        {
//            SmallMuscle = 2;
//            LargeMuscle = 6;
//        }
//        //(לשים לב ליום 1 של אימון זה...)
//        //במידה וזמן האימון הוא 60 דקות לכל שריר גדול 8 תרגילים
//        //לשרירים קטנים 4 תרגילים
//        else if (time == 60)
//        {
//            SmallMuscle = 4;
//            LargeMuscle = 8;
//        }
//        else
//        {
//            throw new ArgumentException("Invalid time");
//        }
//    }

//    //עכשיו צריך לבחור את התרגילים ולסדר אותם לתוכנית אימון

//}

// This class is responsible for managing the relationship between programs and exercises.
// It will handle adding, updating, deleting, and retrieving exercises associated with a program.
// It will also handle the logic for assigning exercises to specific categories within a program.

// Add methods to manage exercises in a program
// For example:
// - AddExerciseToProgram
// - RemoveExerciseFromProgram
// - GetExercisesByProgramId

//public Task List<List<string>> DivideMuscleGroups(int daysInWeek)
//{
//    // This method divides the muscle groups into a list of lists based on the number of days in a week.
//    List<List<int>> muscleGroups = new List<List<int>>();

//    for (int i = 0; i < daysInWeek; i++)
//    {
//        List<string> muscleGroup = new List<string>();
//        muscleGroups.Add(muscleGroup);
//    }

//    muscleGroups[0].Add( "רגליים");
//    muscleGroups[1].Add("חזה");
//    muscleGroups[2].Add("גב");
//    muscleGroups[3].Add("כתפיים");
//    muscleGroups[4].Add("יד קדמית");
//    muscleGroups[5].Add("יד אחורית");
//    muscleGroups[6].Add("בטן");
//    muscleGroups[7].Add("זוקפי גו");

//    return muscleGroups;
//}



//public static string[][] DivideMuscleGroups(int daysInWeek)
//{


//    return muscleGroups;
//}

// This method divides the muscle groups into a list of lists based on the number of days in a week.
//    List<List<int>> muscleGroups = new List<List<int>>();

//    for (int i = 0; i < daysInWeek; i++)
//    {
//        List<string> muscleGroup = new List<string>();
//        muscleGroups.Add(muscleGroup);
//    }

//    muscleGroups[0].Add(muscleDAL.GetIdOfMuscleByNameAsync("רגליים"));
//    muscleGroups[1].Add(muscleDAL.GetIdOfMuscleByNameAsync("חזה"));
//    muscleGroups[2].Add(muscleDAL.GetIdOfMuscleByNameAsync("גב"));
//    muscleGroups[3].Add(muscleDAL.GetIdOfMuscleByNameAsync("כתפיים"));
//    muscleGroups[4].Add(muscleDAL.GetIdOfMuscleByNameAsync("יד קדמית"));
//    muscleGroups[5].Add(muscleDAL.GetIdOfMuscleByNameAsync("יד אחורית"));
//    muscleGroups[6].Add(muscleDAL.GetIdOfMuscleByNameAsync("בטן"));
//    muscleGroups[7].Add(muscleDAL.GetIdOfMuscleByNameAsync("זוקפי גו"));



//    return muscleGroups;
//}


//int[][] muscleGroups = new int[daysInWeek][];

//muscleGroups[0][0] = muscleDAL.GetIdOfMuscleByNameAsync("רגליים");
//muscleGroups[1][0] = muscleDAL.GetIdOfMuscleByNameAsync("חזה");
//muscleGroups[2][0] = muscleDAL.GetIdOfMuscleByNameAsync("גב");
//muscleGroups[3][0] = muscleDAL.GetIdOfMuscleByNameAsync("כתפיים");
//muscleGroups[4][0] = muscleDAL.GetIdOfMuscleByNameAsync("יד קדמית");
//muscleGroups[5][0] = muscleDAL.GetIdOfMuscleByNameAsync("יד אחורית");
//muscleGroups[6][0] = muscleDAL.GetIdOfMuscleByNameAsync("בטן");
//muscleGroups[7][0] = muscleDAL.GetIdOfMuscleByNameAsync("זוקפי גו");
