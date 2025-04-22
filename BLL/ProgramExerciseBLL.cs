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
    public class TrainingParams
    {
        public List<List<object>> DayLists { get; set; }
        public List<object> LargeMuscleList { get; set; }
        public List<object> SmallMuscleList { get; set; }
        public int MinRep { get; set; }
        public int MaxRep { get; set; }
        public int LargeMuscleCount { get; set; }
        public Dictionary<int, int> TimeCategoryList { get; set; }
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

        
        public async Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time)
        {
            //string filePath1 = "WorkoutData.xlsx";
            string filePath1 = @"C:\Users\user\Pictures\תכנות\שנה ב\פוריקט שנתי\C#\פרויקט חדש 20.04\Gym_Api\BLL\WorkoutData.xlsx";
            var trainingParams = await GetAllParams(filePath1, daysInWeek, goal, level, time);

            if (trainingParams == null)
            {
                throw new Exception("Failed to retrieve training parameters.");
            }

            // Log retrieved parameters
            logger.LogInformation("Day Lists: " + string.Join(", ", trainingParams.DayLists));
            logger.LogInformation("Large Muscles: " + string.Join(", ", trainingParams.LargeMuscleList));
            logger.LogInformation("Small Muscles: " + string.Join(", ", trainingParams.SmallMuscleList));
            logger.LogInformation($"Repetitions: {trainingParams.MinRep}-{trainingParams.MaxRep}");
            logger.LogInformation($"Large Muscle Count: {trainingParams.LargeMuscleCount}");
            logger.LogInformation("Time Categories: " + string.Join(", ", trainingParams.TimeCategoryList));
        }

        public async Task<TrainingParams> GetAllParams(string filePath, int daysInWeek, int goal, int level, int time)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    var daysWorksheet = GetWorksheet(workbook, DaysInWeekSheet);
                    var dayLists = ExtractDayLists(daysWorksheet, daysInWeek);

                    //var dayLists = new List<List<object>>();
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
                    int rowCount = FindRowByValue(countToMuscleWorksheet, 1, goal, "Row with the specified goal not found.");
                    int largeMuscleCount = countToMuscleWorksheet.Cell(rowCount, colCount).GetValue<int>();

                    var categoryWorksheet = GetWorksheet(workbook, CategorySheet);
                    var timeCategoryWorksheet = GetWorksheet(workbook, TimeCategorySheet);
                    var timeCategoryList = ExtractTimeCategoryList(timeCategoryWorksheet, daysInWeek, categoryWorksheet.RowsUsed().Count());

                    return new TrainingParams
                    {
                        DayLists = dayLists,
                        LargeMuscleList = largeMuscleList,
                        SmallMuscleList = smallMuscleList,
                        MinRep = minRep,
                        MaxRep = maxRep,
                        LargeMuscleCount = largeMuscleCount,
                        TimeCategoryList = timeCategoryList
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

        //private List<List<object>> ExtractDayLists(IXLWorksheet worksheet, int daysInWeek)
        //{
        //    //var dayLists = new List<List<object>>();
        //    //foreach (var col in worksheet.ColumnsUsed())
        //    //{
        //    //    if (col.Cell(1).GetValue<int>() == daysInWeek)
        //    //    {
        //    //        //  dayLists.Add(col.CellsUsed().Skip(1).Select(c => c.Value).ToList());
        //    //        dayLists.Add(col.CellsUsed().Skip(1).Select(c => (object)c.Value).ToList());
        //    //    }
        //    //}
        //    //return dayLists;
        //    var dayLists = new List<List<object>>();

        //    // מציאת העמודה המתאימה לפי הערך בשורה הראשונה
        //    foreach (var col in worksheet.ColumnsUsed())
        //    {
        //        if (col.FirstCell().GetValue<int>() == daysInWeek)  // בדוק את הערך בשורה הראשונה של העמודה
        //        {
        //            // הוסף את הערכים מהעמודה (מתחיל משורה שנייה)
        //            dayLists.Add(col.CellsUsed().Skip(1).Select(c => (object)c.Value).ToList());
        //        }
        //    }
        //    return dayLists;
        //}
        //private List<List<object>> ExtractDayLists(IXLWorksheet worksheet, int daysInWeek)
        //{
        //    var dayLists = new List<List<object>>();

        //    foreach (var col in worksheet.ColumnsUsed())
        //    {
        //        // בדוק אם הערך בשורה הראשונה תואם ל-daysInWeek
        //        if (col.FirstCell().GetValue<int>() == daysInWeek)
        //        {
        //            // הוסף את הערכים מהעמודה (מתחיל משורה שנייה)
        //            dayLists.Add(col.CellsUsed().Skip(1).Select(c => (object)c.Value).ToList());
        //        }
        //    }

        //    return dayLists;
        //}
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
        //private int FindColumnByValueInFirstRow(IXLWorksheet worksheet, int value, string errorMessage)
        //{
        //    foreach (var col in worksheet.ColumnsUsed())
        //    {
        //        // בדוק את הערך בשורה הראשונה של העמודה (תא ראשון בכל עמודה)
        //        if (col.FirstCell().GetValue<int>() == value)
        //        {
        //            return col.ColumnNumber(); // החזר את מספר העמודה
        //        }
        //    }

        //    // אם הערך לא נמצא, רשום ביומן והשלך חריגה
        //    logger.LogWarning(errorMessage);
        //    throw new Exception(errorMessage);
        //}
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

        //private Dictionary<int, int> ExtractTimeCategoryList(IXLWorksheet worksheet, int daysInWeek, int categoryCount)
        //{
        //    int rowTimeCategory = FindRowByValue(worksheet, 1, daysInWeek, "Row with the specified days not found.");
        //    var timeCategoryList = new Dictionary<int, int>();
        //    for (int colT = 2; colT <= categoryCount; colT++)
        //    {
        //        timeCategoryList.Add(colT, worksheet.Cell(rowTimeCategory, colT).GetValue<int>());
        //    }
        //    return timeCategoryList;
        //}
        private Dictionary<int, int> ExtractTimeCategoryList(IXLWorksheet worksheet, int daysInWeek, int categoryCount)
        {
            // מצא את מספר העמודה שבו נמצא daysInWeek בשורה הראשונה
            int colTimeCategory = FindColumnByValueInFirstRow(worksheet, daysInWeek, "Column with the specified days not found.");
            var timeCategoryList = new Dictionary<int, int>();

            // עבור על כל הערכים בעמודה שנמצאת לאחר העמודה הראשונה
            for (int row = 2; row <= categoryCount; row++)
            {
                timeCategoryList.Add(row, worksheet.Cell(row, colTimeCategory).GetValue<int>());
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
    }
}







//using AutoMapper;
//using DBEntities.Models;
//using DTO;
//using IBLL;
//using IDAL;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using OfficeOpenXml;
//using System.IO;
//using System.Threading.Tasks;
//using Microsoft.Extensions.Logging;

//namespace BLL
//{
//    public class TrainingParams
//    {
//        public List<List<object>> DayLists { get; set; }
//        public List<object> LargeMuscleList { get; set; }
//        public List<object> SmallMuscleList { get; set; }
//        public int MinRep { get; set; }
//        public int MaxRep { get; set; }
//        public int LargeMuscleCount { get; set; }
//        public Dictionary<int, int> TimeCategoryList { get; set; }
//    }

//    public class ProgramExerciseBLL : IProgramExerciseBLL
//    {
//        private readonly IMuscleDAL muscleDAL;
//        private readonly ILogger<ProgramExerciseBLL> logger;
//        private readonly IMapper mapper;

//        //לשנות קבועים אלא טוען מתוך האקסל
//        private const string DaysInWeekSheet = "DaysInWeek";
//        private const string LargeMuscleSheet = "LargeMuscle";
//        private const string SmallMuscleSheet = "SmallMuscle";
//        private const string RepitByGoalSheet = "RepitByGoal";
//        private const string SumOfLargeByTimeSheet = "SumOfLargeByTime";
//        private const string CategorySheet = "Category";
//        private const string TimeCategorySheet = "TimeCategory";

//        public ProgramExerciseBLL(IMuscleDAL muscleDAL, ILogger<ProgramExerciseBLL> logger)
//        {
//            this.muscleDAL = muscleDAL;
//            this.logger = logger;

//            var configTaskConverter = new MapperConfiguration(cfg =>
//            {
//                cfg.CreateMap<Muscle, MuscleDTO>().ReverseMap();
//            });
//            mapper = new Mapper(configTaskConverter);
//        }

//        public async Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise, string filePath, int daysInWeek, int goal, int level, int time)
//        {
//            var trainingParams = await GetAllParams(filePath, daysInWeek, goal, level, time);

//            if (trainingParams == null)
//            {
//                throw new Exception("Failed to retrieve training parameters.");
//            }

//            // שימוש בפרמטרים שהתקבלו
//            logger.LogInformation("Day Lists: " + string.Join(", ", trainingParams.DayLists));
//            logger.LogInformation("Large Muscles: " + string.Join(", ", trainingParams.LargeMuscleList));
//            logger.LogInformation("Small Muscles: " + string.Join(", ", trainingParams.SmallMuscleList));
//            logger.LogInformation($"Repetitions: {trainingParams.MinRep}-{trainingParams.MaxRep}");
//            logger.LogInformation($"Large Muscle Count: {trainingParams.LargeMuscleCount}");
//            logger.LogInformation("Time Categories: " + string.Join(", ", trainingParams.TimeCategoryList));
//        }

//        public async Task<TrainingParams> GetAllParams(string filePath, int daysInWeek, int goal, int level, int time)
//        {
//            try
//            {
//                using (var package = new ExcelPackage(new FileInfo(filePath)))
//                {
//                    var daysWorksheet = GetWorksheet(package, DaysInWeekSheet);
//                    var dayLists = ExtractDayLists(daysWorksheet, daysInWeek);

//                    var largeMuscleWorksheet = GetWorksheet(package, LargeMuscleSheet);
//                    var largeMuscleList = ExtractColumnValues(largeMuscleWorksheet, 1);

//                    var smallMuscleWorksheet = GetWorksheet(package, SmallMuscleSheet);
//                    var smallMuscleList = ExtractColumnValues(smallMuscleWorksheet, 1);

//                    var repWorksheet = GetWorksheet(package, RepitByGoalSheet);
//                    int colRep = FindColumnByValue(repWorksheet, 1, goal, "Column with the specified goal not found.");
//                    int minRep = GetValueFromWorksheet(repWorksheet, "min", colRep, "Min value not found.");
//                    int maxRep = GetValueFromWorksheet(repWorksheet, "max", colRep, "Max value not found.");

//                    var countToMuscleWorksheet = GetWorksheet(package, SumOfLargeByTimeSheet);
//                    int colCount = FindColumnByValue(countToMuscleWorksheet, 1, time, "Column with the specified time not found.");
//                    int rowCount = FindRowByValue(countToMuscleWorksheet, 1, goal, "Row with the specified goal not found.");
//                    int largeMuscleCount = countToMuscleWorksheet.Cells[rowCount, colCount].GetValue<int>();

//                    var categoryWorksheet = GetWorksheet(package, CategorySheet);
//                    var timeCategoryWorksheet = GetWorksheet(package, TimeCategorySheet);
//                    var timeCategoryList = ExtractTimeCategoryList(timeCategoryWorksheet, daysInWeek, categoryWorksheet.Dimension.End.Row);

//                    return new TrainingParams
//                    {
//                        DayLists = dayLists,
//                        LargeMuscleList = largeMuscleList,
//                        SmallMuscleList = smallMuscleList,
//                        MinRep = minRep,
//                        MaxRep = maxRep,
//                        LargeMuscleCount = largeMuscleCount,
//                        TimeCategoryList = timeCategoryList
//                    };
//                }
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "Error while processing training parameters.");
//                throw;
//            }
//        }

//        private ExcelWorksheet GetWorksheet(ExcelPackage package, string sheetName)
//        {
//            var worksheet = package.Workbook.Worksheets.FirstOrDefault(ws => ws.Name == sheetName);
//            if (worksheet == null)
//            {
//                logger.LogWarning($"Worksheet '{sheetName}' not found.");
//                throw new Exception($"Worksheet '{sheetName}' does not exist.");
//            }
//            return worksheet;
//        }

//        private List<List<object>> ExtractDayLists(ExcelWorksheet worksheet, int daysInWeek)
//        {
//            var dayLists = new List<List<object>>();
//            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
//            {
//                if (int.Parse(worksheet.Cells[1, col].Text) == daysInWeek)
//                {
//                    dayLists.Add(ExtractColumnValues(worksheet, col));
//                }
//            }
//            return dayLists;
//        }

//        private List<object> ExtractColumnValues(ExcelWorksheet worksheet, int column)
//        {
//            var values = new List<object>();
//            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
//            {
//                values.Add(worksheet.Cells[row, column].Value);
//            }
//            return values;
//        }

//        private int FindColumnByValue(ExcelWorksheet worksheet, int headerRow, int value, string errorMessage)
//        {
//            for (int col = 1; col <= worksheet.Dimension.End.Column; col++)
//            {
//                if (worksheet.Cells[headerRow, col].GetValue<int>() == value) return col;
//            }
//            logger.LogWarning(errorMessage);
//            throw new Exception(errorMessage);
//        }

//        private int FindRowByValue(ExcelWorksheet worksheet, int column, int value, string errorMessage)
//        {
//            for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
//            {
//                if (worksheet.Cells[row, column].GetValue<int>() == value) return row;
//            }
//            logger.LogWarning(errorMessage);
//            throw new Exception(errorMessage);
//        }

//        private int GetValueFromWorksheet(ExcelWorksheet worksheet, string rowText, int column, string errorMessage)
//        {
//            for (int row = 1; row <= worksheet.Dimension.End.Row; row++)
//            {
//                if (worksheet.Cells[row, 1].GetValue<string>() == rowText)
//                {
//                    return worksheet.Cells[row, column].GetValue<int>();
//                }
//            }
//            logger.LogWarning(errorMessage);
//            throw new Exception(errorMessage);
//        }

//        private Dictionary<int, int> ExtractTimeCategoryList(ExcelWorksheet worksheet, int daysInWeek, int categoryCount)
//        {
//            int rowTimeCategory = FindRowByValue(worksheet, 1, daysInWeek, "Row with the specified days not found.");
//            var timeCategoryList = new Dictionary<int, int>();
//            for (int colT = 2; colT <= categoryCount; colT++)
//            {
//                timeCategoryList.Add(colT, worksheet.Cells[rowTimeCategory, colT].GetValue<int>());
//            }
//            return timeCategoryList;
//        }

//        public Task<List<ProgramExerciseDTO>> GetAllProgramExercisesAsync()
//        {
//            throw new NotImplementedException();
//        }

//        public Task<ProgramExerciseDTO> GetProgramExerciseByIdAsync(int id)
//        {
//            throw new NotImplementedException();
//        }

//        public Task<ProgramExerciseDTO> GetProgramExerciseByNameAsync(string name)
//        {
//            throw new NotImplementedException();
//        }

//        public Task UpdateProgramExerciseAsync(ProgramExerciseDTO programExercise, int id)
//        {
//            throw new NotImplementedException();
//        }

//        public Task DeleteProgramExerciseAsync(int id)
//        {
//            throw new NotImplementedException();
//        }

//        public Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise)
//        {
//            throw new NotImplementedException();
//        }

//        public Task ReadDataFromExcelAsync(string filePath)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
