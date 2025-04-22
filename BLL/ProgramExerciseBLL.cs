using AutoMapper;
using DBEntities.Models;
using DTO;
using IBLL;
using IDAL;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BLL
{
    public class TrainingParams : IBLL.TrainingParams
    {
        public List<List<object>> DayLists { get; set; }
        public List<object> LargeMuscleList { get; set; }
        public List<object> SmallMuscleList { get; set; }
        public int MinRep { get; set; }
        public int MaxRep { get; set; }
        public int LargeMuscleCount { get; set; }
        public int SmallMuscleCount { get; set; } 
        public Dictionary<int, int> TimeCategoryList { get; set; }
        public List<object> OrderOfMuscle { get; set; }
    }
    public class ProgramExerciseBLL : IProgramExerciseBLL
    {
        private readonly IMuscleDAL muscleDAL;
        private readonly ILogger<ProgramExerciseBLL> logger;
        private readonly IMapper mapper;

        // Sheet names
        private const string DaysInWeekSheet = "DaysInWeek";
        private const string LargeMuscleSheet = "LargeMuscle";
        private const string SmallMuscleSheet = "SmallMuscle";
        private const string RepitByGoalSheet = "RepitByGoal";
        private const string SumOfLargeByTimeSheet = "SumOfLargeByTime";
        private const string SumOfSmallByTimeSheet = "SumOfSmallByTime"; // הוסף את השם של הגיליון החדש
        private const string CategorySheet = "Category";
        private const string TimeCategorySheet = "TimeCategory";

        public ProgramExerciseBLL(IMuscleDAL muscleDAL, ILogger<ProgramExerciseBLL> logger)
        {
            this.muscleDAL = muscleDAL;
            this.logger = logger;

            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Muscle, MuscleDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }


        //public async Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time)
        //{
        //    //string filePath1 = "WorkoutData.xlsx";
        //    string filePath1 = @"C:\Users\user\Pictures\תכנות\שנה ב\פוריקט שנתי\C#\פרויקט חדש 20.04\Gym_Api\BLL\WorkoutData.xlsx";
        //    var trainingParams = await GetAllParams(filePath1, daysInWeek, goal, level, time);

        //    if (trainingParams == null)
        //    {
        //        throw new Exception("Failed to retrieve training parameters.");
        //    }

        //    // Log retrieved parameters
        //    //logger.LogInformation("Day Lists: " + string.Join(", ", trainingParams.DayLists));
        //    // Log Day Lists
        //    for (int i = 0; i < trainingParams.DayLists.Count; i++)
        //    {
        //        logger.LogInformation($"Day List {i + 1}: " + string.Join(", ", trainingParams.DayLists[i]));
        //    }

        //    logger.LogInformation("Large Muscles: " + string.Join(", ", trainingParams.LargeMuscleList));
        //    logger.LogInformation("Small Muscles: " + string.Join(", ", trainingParams.SmallMuscleList));
        //    logger.LogInformation($"Repetitions: {trainingParams.MinRep}-{trainingParams.MaxRep}");
        //    logger.LogInformation($"Large Muscle Count: {trainingParams.LargeMuscleCount}");
        //    logger.LogInformation("Time Categories: " + string.Join(", ", trainingParams.TimeCategoryList));
        //}
        public async Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time)
        {
            string filePath1 = @"C:\Users\user\Pictures\תכנות\שנה ב\פוריקט שנתי\C#\פרויקט חדש 20.04\Gym_Api\BLL\WorkoutData.xlsx";
            var trainingParams = await GetAllParams(filePath1, daysInWeek, goal, level, time);

            if (trainingParams == null)
            {
                throw new Exception("Failed to retrieve training parameters.");
            }

            // Log retrieved parameters
            for (int i = 0; i < trainingParams.DayLists.Count; i++)
            {
                logger.LogInformation($"Day List {i + 1}: " + string.Join(", ", trainingParams.DayLists[i]));
            }

            logger.LogInformation("Large Muscles: " + string.Join(", ", trainingParams.LargeMuscleList));
            logger.LogInformation("Small Muscles: " + string.Join(", ", trainingParams.SmallMuscleList));
            logger.LogInformation($"Repetitions: {trainingParams.MinRep}-{trainingParams.MaxRep}");
            logger.LogInformation($"Large Muscle Count: {trainingParams.LargeMuscleCount}");
            logger.LogInformation($"Small Muscle Count: {trainingParams.SmallMuscleCount}");
            logger.LogInformation("Time Categories: " + string.Join(", ", trainingParams.TimeCategoryList.Select(kvp => $"[{kvp.Key}: {kvp.Value}]")));
            logger.LogInformation("Order of Muscles: " + string.Join(", ", trainingParams.OrderOfMuscle)); // לוג לסדר השרירים

            var exercisePlan = await GenerateExercisePlanAsync(trainingParams);

            // הדפסת התוכנית
            foreach (var day in exercisePlan)
            {
                Console.WriteLine("Day Exercises:");
                foreach (var exercise in day)
                {
                    Console.WriteLine($"- {exercise.ExerciseName}");
                }
            }
        }

        //public async Task<TrainingParams> GetAllParams(string filePath, int daysInWeek, int goal, int level, int time)
        //{
        //    try
        //    {
        //        using (var workbook = new XLWorkbook(filePath))
        //        {
        //            var daysWorksheet = GetWorksheet(workbook, DaysInWeekSheet);
        //            var dayLists = ExtractDayLists(daysWorksheet, daysInWeek);

        //            var largeMuscleWorksheet = GetWorksheet(workbook, LargeMuscleSheet);
        //            var largeMuscleList = ExtractColumnValues(largeMuscleWorksheet, 1);

        //            var smallMuscleWorksheet = GetWorksheet(workbook, SmallMuscleSheet);
        //            var smallMuscleList = ExtractColumnValues(smallMuscleWorksheet, 1);

        //            var repWorksheet = GetWorksheet(workbook, RepitByGoalSheet);
        //            int colRep = FindColumnByValue(repWorksheet, 1, goal, "Column with the specified goal not found.");
        //            int minRep = GetValueFromWorksheet(repWorksheet, "min", colRep, "Min value not found.");
        //            int maxRep = GetValueFromWorksheet(repWorksheet, "max", colRep, "Max value not found.");

        //            var countToMuscleWorksheet = GetWorksheet(workbook, SumOfLargeByTimeSheet);
        //            int colCount = FindColumnByValue(countToMuscleWorksheet, 1, time, "Column with the specified time not found.");
        //            int rowCount = FindRowByValue(countToMuscleWorksheet, 1, goal, "Row with the specified goal not found.");
        //            int largeMuscleCount = countToMuscleWorksheet.Cell(rowCount, colCount).GetValue<int>();

        //            var categoryWorksheet = GetWorksheet(workbook, CategorySheet);
        //            var timeCategoryWorksheet = GetWorksheet(workbook, TimeCategorySheet);
        //            var timeCategoryList = ExtractTimeCategoryList(timeCategoryWorksheet, time);

        //            return new TrainingParams
        //            {
        //                DayLists = dayLists,
        //                LargeMuscleList = largeMuscleList,
        //                SmallMuscleList = smallMuscleList,
        //                MinRep = minRep,
        //                MaxRep = maxRep,
        //                LargeMuscleCount = largeMuscleCount,
        //                TimeCategoryList = timeCategoryList
        //            };


        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error while processing training parameters.");
        //        throw;
        //    }
        //}

        public async Task<TrainingParams> GetAllParams(string filePath, int daysInWeek, int goal, int level, int time)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var daysWorksheet = GetWorksheet(workbook, DaysInWeekSheet);
                    var dayLists = ExtractDayLists(daysWorksheet, daysInWeek);

                    var largeMuscleWorksheet = GetWorksheet(workbook, LargeMuscleSheet);
                    var largeMuscleList = ExtractColumnValues(largeMuscleWorksheet, 1);

                    var smallMuscleWorksheet = GetWorksheet(workbook, SmallMuscleSheet);
                    var smallMuscleList = ExtractColumnValues(smallMuscleWorksheet, 1);

                    var repWorksheet = GetWorksheet(workbook, RepitByGoalSheet);
                    int colRep = FindColumnByValue(repWorksheet, 1, goal, "Column with the specified goal not found.");
                    int minRep = GetValueFromWorksheet(repWorksheet, "min", colRep, "Min value not found.");
                    int maxRep = GetValueFromWorksheet(repWorksheet, "max", colRep, "Max value not found.");

                    var countToMuscleWorksheet = GetWorksheet(workbook, SumOfLargeByTimeSheet);
                    int colCount = FindColumnByValue(countToMuscleWorksheet, 1, time, "Column with the specified time not found.");
                    int rowCount = FindRowByValue(countToMuscleWorksheet, 1, daysInWeek, "Row with the specified goal not found.");
                    int largeMuscleCount = countToMuscleWorksheet.Cell(rowCount, colCount).GetValue<int>();

                    var countToSmallMuscleWorksheet = GetWorksheet(workbook, SumOfSmallByTimeSheet);
                    int colSmallCount = FindColumnByValue(countToSmallMuscleWorksheet, 1, time, "Column with the specified time not found.");
                    int rowSmallCount = FindRowByValue(countToSmallMuscleWorksheet, 1, daysInWeek, "Row with the specified goal not found.");
                    int smallMuscleCount = countToSmallMuscleWorksheet.Cell(rowSmallCount, colSmallCount).GetValue<int>();

                    var categoryWorksheet = GetWorksheet(workbook, CategorySheet);
                    var timeCategoryWorksheet = GetWorksheet(workbook, TimeCategorySheet);
                    var timeCategoryList = ExtractTimeCategoryList(timeCategoryWorksheet, time);

                    // שליפת סדר השרירים מתוך הגיליון OrderOfMuscle
                    var orderOfMuscleWorksheet = GetWorksheet(workbook, "OrderOfMuscle");
                    var orderOfMuscle = ExtractColumnValues(orderOfMuscleWorksheet, 2);

                    return new TrainingParams
                    {
                        DayLists = dayLists,
                        LargeMuscleList = largeMuscleList,
                        SmallMuscleList = smallMuscleList,
                        MinRep = minRep,
                        MaxRep = maxRep,
                        LargeMuscleCount = largeMuscleCount,
                        SmallMuscleCount = smallMuscleCount,
                        TimeCategoryList = timeCategoryList,
                        OrderOfMuscle = orderOfMuscle // הוספת סדר השרירים
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while processing training parameters.");
                throw;
            }
        }
        private IXLWorksheet GetWorksheet(XLWorkbook workbook, string sheetName)
        {
            var worksheet = workbook.Worksheet(sheetName);
            if (worksheet == null)
            {
                logger.LogWarning($"Worksheet '{sheetName}' not found.");
                throw new Exception($"Worksheet '{sheetName}' does not exist.");
            }
            return worksheet;
        }

        private List<List<object>> ExtractDayLists(IXLWorksheet worksheet, int daysInWeek)
        {
            var dayLists = new List<List<object>>();

            foreach (var col in worksheet.ColumnsUsed())
            {
                // בדוק אם הערך בשורה הראשונה תואם ל-daysInWeek
                if (col.FirstCell().GetValue<int>() == daysInWeek)
                {
                    // הוסף את הערכים מהעמודה (מתחיל משורה שנייה)
                    dayLists.Add(col.CellsUsed().Skip(1).Select(c => (object)c.Value).ToList());
                }
            }

            return dayLists;
        }

        private List<object> ExtractColumnValues(IXLWorksheet worksheet, int column)
        {
            return worksheet.Column(column).CellsUsed().Skip(1).Select(c => (object)c.Value).ToList();
            //return worksheet.Column(column).CellsUsed().Skip(1).Select(c => c.Value).ToList();
        }

        private int FindColumnByValue(IXLWorksheet worksheet, int headerRow, int value, string errorMessage)
        {
            foreach (var col in worksheet.ColumnsUsed())
            {
                if (col.Cell(headerRow).GetValue<int>() == value)
                {
                    return col.ColumnNumber();
                }
            }
            logger.LogWarning(errorMessage);
            throw new Exception(errorMessage);
        }
        private int FindColumnByValueInFirstRow(IXLWorksheet worksheet, int value, string errorMessage)
        {
            foreach (var col in worksheet.ColumnsUsed())
            {
                // בדוק את הערך בשורה הראשונה של העמודה
                if (col.FirstCell().GetValue<int>() == value)
                {
                    return col.ColumnNumber(); // החזר את מספר העמודה
                }
            }

            // אם הערך לא נמצא, רשום ביומן והשלך חריגה
            logger.LogWarning(errorMessage);
            throw new Exception(errorMessage);
        }

        private int FindRowByValue(IXLWorksheet worksheet, int column, int value, string errorMessage)
        {
            foreach (var row in worksheet.RowsUsed())
            {
                if (row.Cell(column).GetValue<int>() == value)
                {
                    return row.RowNumber();
                }
            }
            logger.LogWarning(errorMessage);
            throw new Exception(errorMessage);
        }

        private int GetValueFromWorksheet(IXLWorksheet worksheet, string rowText, int column, string errorMessage)
        {
            foreach (var row in worksheet.RowsUsed())
            {
                if (row.Cell(1).GetValue<string>() == rowText)
                {
                    return row.Cell(column).GetValue<int>();
                }
            }
            logger.LogWarning(errorMessage);
            throw new Exception(errorMessage);
        }

        private Dictionary<int, int> ExtractTimeCategoryList(IXLWorksheet worksheet, int time)
        {
            var timeCategoryList = new Dictionary<int, int>();

            // מצא את השורה בעמודה הראשונה שבה הערך שווה ל-time
            int targetRow = FindRowByValue(worksheet, 1, time, $"Row with the specified time ({time}) not found.");

            // מצא את מספר העמודה המתאימה ל-TIME
            int timeColumn = 1; // העמודה הראשונה שבה מחפשים את הערך TIME

            // עבור על כל העמודות בשורה שנמצאה
            foreach (var col in worksheet.ColumnsUsed())
            {
                // אם העמודה היא העמודה שבה נמצא הערך time, דלג עליה
                if (col.ColumnNumber() == timeColumn)
                {
                    continue;
                }

                var keyCell = col.Cell(1); // התא בשורה הראשונה (כותרת)
                var valueCell = col.Cell(targetRow); // התא בעמודה ובשורה שנמצאה

                // ודא שהתאים אינם ריקים ושהערכים שלהם חוקיים
                if (!keyCell.IsEmpty() && !valueCell.IsEmpty() &&
                    int.TryParse(keyCell.GetValue<string>(), out int key) &&
                    int.TryParse(valueCell.GetValue<string>(), out int value))
                {
                    timeCategoryList[key] = value; // הוסף למילון
                }
                else
                {
                    logger.LogWarning($"Skipping invalid or empty cells at {keyCell.Address} or {valueCell.Address}");
                }
            }

            return timeCategoryList;
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

        public async Task<List<List<ExerciseDTO>>> GenerateExercisePlanAsync(IBLL.TrainingParams trainingParams)
        {
            // רשימה שתכיל את כל רשימות התרגילים לכל הימים
            var exercisePlan = new List<List<ExerciseDTO>>();

            // עבור כל רשימה ב-DayLists (כל יום)
            foreach (var dayList in trainingParams.DayLists)
            {
                var dayExercises = new List<ExerciseDTO>(); // רשימה לתרגילים של יום זה

                // עבור על כל שריר ברשימה של היום
                foreach (var muscle in dayList)
                {
                    // בדוק אם השריר נמצא ב-LargeMuscleList
                    if (trainingParams.LargeMuscleList.Contains(muscle))
                    {
                        // שלוף תרגילים לשריר זה לפי LargeMuscleCount
                        var exercises = await GetExercisesForMuscleAsync(muscle.ToString(), trainingParams.LargeMuscleCount);
                        dayExercises.AddRange(exercises);
                    }
                    // בדוק אם השריר נמצא ב-SmallMuscleList
                    else if (trainingParams.SmallMuscleList.Contains(muscle))
                    {
                        // שלוף תרגילים לשריר זה לפי SmallMuscleCount
                        var exercises = await GetExercisesForMuscleAsync(muscle.ToString(), trainingParams.SmallMuscleCount);
                        dayExercises.AddRange(exercises);
                    }
                }

                // הוסף את רשימת התרגילים של היום לרשימה הכללית
                exercisePlan.Add(dayExercises);
            }

            return exercisePlan;
        }

        public async Task<List<ExerciseDTO>> GetExercisesForMuscleAsync(string muscleName, int count)
        {
            // שליפת תרגילים מה-DAL
            var exercises = await muscleDAL.GetExercisesForMuscleAsync(muscleName);

            // המרת התרגילים ל-DTO
            var exerciseDTOs = exercises.Select(e => new ExerciseDTO
            {
                ExerciseId = e.ExerciseId,
                ExerciseName = e.ExerciseName
            }).ToList();

            // בחר מספר תרגילים באופן אקראי
            return exerciseDTOs.OrderBy(e => Guid.NewGuid()).Take(count).ToList();
        }

        
    }
}
