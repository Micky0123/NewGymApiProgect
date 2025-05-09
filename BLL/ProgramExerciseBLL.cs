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
using DocumentFormat.OpenXml.Spreadsheet;
using static BLL.TrainingParams;
using DAL;

namespace BLL
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

    }

    public class ProgramExerciseBLL : IProgramExerciseBLL
    {
        private readonly IMuscleTypeDAL muscleTypeDAL;
        private readonly IMuscleDAL muscleDAL;
        private readonly IEquipmentDAL equipmentDAL;
        private readonly IExerciseDAL exerciseDAL;
        private readonly ILogger<ProgramExerciseBLL> logger;
        private readonly IMapper mapper;

        // Sheet names
        //**********מיכלוש שימי לב שצריך לקבל את השמות ושהם לא יהיו קבועים//****************************
        private const string DaysInWeekSheet = "List1";
        private const string MuscleSheet = "MuscleSize";
        private const string SmallMuscleSheet = "SmallMuscle";
        private const string RepitByGoalSheet = "Repeat";
        private const string CategorySheet = "TimeForCategory";
        private const string SumOfLargeByTimeSheet = "Amount";
        private const string SumOfSmallByTimeSheet = "Amount";
        private const string TypMuscleSheet = "MuscleType";
        private const string EquipmentSheet = "Equipment";
        private const string NeedSubMuscleSheet = "NeedSubMuscle";
        public ProgramExerciseBLL(IMuscleDAL muscleDAL, ILogger<ProgramExerciseBLL> logger, IMuscleTypeDAL muscleTypeDAL, IEquipmentDAL equipmentDAL, IExerciseDAL exerciseDAL)
        {
            this.muscleDAL = muscleDAL;
            this.equipmentDAL = equipmentDAL;
            this.muscleTypeDAL = muscleTypeDAL;
            this.exerciseDAL = exerciseDAL;
            this.logger = logger;

            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Muscle, MuscleDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
            this.exerciseDAL = exerciseDAL;
        }

        public async Task addProgramExerciseAsync1(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time)
        {
            //**********מיכלוש שימי לב שצריך לקבל את השמות ושהם לא יהיו קבועים//****************************
            string filePath1 = @"C:\Users\user\Pictures\תכנות\שנה ב\פוריקט שנתי\C#\פרויקט חדש 20.04\Gym_Api\BLL\new.xlsx";

            var trainingParams = new TrainingParams
            {
                DayLists = new List<List<DayEntry>>(),
                MuscleSizeData = new Dictionary<string, List<object>>(),
                MinRep = 0,
                MaxRep = 0,
                TimeCategories = new Dictionary<(string Category, string MuscleSize), int>(),
                TypMuscle = new List<string>(),
                TypeMuscleData = new List<Dictionary<int, List<string>>>(),
                equipment = new List<string>(),
                NeedSubMuscleList = new List<string>(),
            };
            // שליפת כל הפרמטרים מהקובץ
            trainingParams = await GetAllParams(filePath1, daysInWeek, goal, level, time);

            if (trainingParams == null)
            {
                throw new Exception("Failed to retrieve training parameters.");
            }

            trainingParams.DayLists = trainingParams.DayLists.Select(list => list.Where(de => de.Values != 0).ToList()).ToList();
            for (int i = 0; i < trainingParams.DayLists.Count; i++)
            {
                // הדפסת הכותרת של DayList
                logger.LogInformation($"Day List {i + 1}:");

                // מעבר על כל Entry בתוך DayList הנוכחי
                foreach (var entry in trainingParams.DayLists[i])
                {
                    // הדפסת ה-Key וה-Values
                    logger.LogInformation($"Key: {entry.Key}, Values: {entry.Values}");
                }
            }
            // הדפסת מספר החזרות
            logger.LogInformation($"Repetitions: {trainingParams.MinRep}-{trainingParams.MaxRep}");

            // הדפסת הקטגוריות והכמות לכל שריר
            logger.LogInformation("Time Categories: " + string.Join(", ", trainingParams.TimeCategories.Select(kvp => $"[{kvp.Key.Category} - {kvp.Key.MuscleSize}: {kvp.Value}]")));

            //logger.LogInformation("List of MuscleType: " + string.Join(", ", trainingParams.TypMuscle));
            foreach (var dict in trainingParams.TypeMuscleData)
            {
                foreach (var pair in dict)
                {
                    logger.LogInformation($"Muscle Type: {pair.Key} - Values: {string.Join(", ", pair.Value)}");
                }
            }
            logger.LogInformation("List of Need SubMuscle: " + string.Join(", ", trainingParams.NeedSubMuscleList));
            logger.LogInformation("List of equipment: " + string.Join(", ", trainingParams.equipment));
            // var exercisePlan = await GenerateExercisePlanAsync(trainingParams);
            ///// await GenerateExercisePlanAndLogAsync(trainingParams);
            //   await GenerateExercisePlanWithEquipmentAsync(trainingParams);
            await GenerateOptimizedExercisePlanAsync(trainingParams);
        }

        public async Task<TrainingParams> GetAllParams(string filePath, int daysInWeek, int goal, int level, int time)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    // שליפת מידע על הימים
                    var daysWorksheet = GetWorksheet(workbook, DaysInWeekSheet);
                    //var dayLists = ExtractDayLists(daysWorksheet, daysInWeek);
                    var dayListsNew = ExtractDayListsNew(daysWorksheet, daysInWeek, time);

                    // קריאת מידע על גדלי השרירים
                    var muscleSizeWorksheet = GetWorksheet(workbook, MuscleSheet);
                    var muscleSizeData = ExtractMuscleSizeData(muscleSizeWorksheet);
                    // מספר החזרות
                    var repWorksheet = GetWorksheet(workbook, RepitByGoalSheet);
                    int colRep = FindColumnByValue(repWorksheet, 1, goal, "Column with the specified goal not found.");
                    int minRep = GetValueFromWorksheet(repWorksheet, "min", colRep, "Min value not found.");
                    int maxRep = GetValueFromWorksheet(repWorksheet, "max", colRep, "Max value not found.");

                    // זמן לקטגוריה
                    var timeCategoryWorksheet = GetWorksheet(workbook, CategorySheet);
                    var timeCategoryList = ExtractTimeCategoryList(timeCategoryWorksheet, goal, time);

                    // הכמות לשרירים
                    var countToMuscleWorksheet = GetWorksheet(workbook, SumOfLargeByTimeSheet);
                    var categoryMuscleSizeData = ExtractCategoryMuscleSizeData(countToMuscleWorksheet, time, daysInWeek);

                    //סוג השרירים
                    var typMuscleWorksheet = GetWorksheet(workbook, TypMuscleSheet);
                    var typMuscleData = ExtractTypMuscleData(typMuscleWorksheet, daysInWeek);

                    //ציוד
                    var equipmentWorksheet = GetWorksheet(workbook, EquipmentSheet);
                    var equipmentData = ExtractEquipmentData(equipmentWorksheet, level);

                    var muscleTypeWorksheet = GetWorksheet(workbook, NeedSubMuscleSheet);
                    var muscleTypeData = ExtractMuscleTypeData(muscleTypeWorksheet);

                    // החזרת כל הנתונים
                    return new TrainingParams
                    {
                        DayLists = dayListsNew,
                        MuscleSizeData = muscleSizeData,
                        MinRep = minRep,
                        MaxRep = maxRep,
                        TimeCategories = categoryMuscleSizeData,
                        TypeMuscleData = typMuscleData,
                        equipment = equipmentData,
                        NeedSubMuscleList = muscleTypeData,

                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while processing training parameters.");
                throw;
            }
        }

        private Dictionary<string, List<object>> ExtractMuscleSizeData(IXLWorksheet worksheet)
        {
            var muscleSizeData = new Dictionary<string, List<object>>();

            // מעבר על כל העמודות בשורה הראשונה
            foreach (var col in worksheet.ColumnsUsed())
            {
                string muscleSize = col.FirstCell().GetValue<string>(); // גודל השריר בשורה הראשונה
                var muscles = col.CellsUsed().Skip(1).Select(c => (object)c.Value).ToList(); // רשימת השרירים תחת גודל זה
                muscleSizeData[muscleSize] = muscles;
            }

            return muscleSizeData;
        }

        private List<string> ExtractMuscleTypeData(IXLWorksheet worksheet)
        {
            var muscleTypeListData = new List<string>();

            // מעבר על כל העמודות בשורה הראשונה
            foreach (var row in worksheet.RowsUsed())
            {
                muscleTypeListData.Add(row.Cell(1).Value.ToString());
            }

            return muscleTypeListData;
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

        private List<string> ExtractEquipmentData(IXLWorksheet worksheet, int level)
        {
            var listEquipment = new List<string>();
            foreach (var row in worksheet.RowsUsed().Skip(1)) // מעבר על כל השורות
            {
                if (row.Cell(1).GetValue<int>() == level)
                {
                    var value = row.Cell(2).GetValue<string>(); // הערך מהעמודה שנמצאה
                    listEquipment.Add(value);
                }
            }
            return listEquipment;
        }

        private List<Dictionary<int, List<string>>> ExtractTypMuscleData(IXLWorksheet worksheet, int daysInWeek)
        {
            var OrderListOfTypeMuscle = new List<Dictionary<int, List<string>>>();

            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                if (row.Cell(1).GetValue<int>() == daysInWeek)
                {
                    var dict = new Dictionary<int, List<string>>();
                    for (int col = 3; col <= row.LastCellUsed().Address.ColumnNumber; col++)
                    {
                        if (dict.ContainsKey(row.Cell(2).GetValue<int>()))
                        {
                            dict[row.Cell(2).GetValue<int>()].Add(row.Cell(col).GetValue<string>());
                        }
                        else
                        {
                            dict.Add(row.Cell(2).GetValue<int>(), new List<string> { row.Cell(col).GetValue<string>() });
                        }
                    }
                    OrderListOfTypeMuscle.Add(dict);
                }
            }

            return OrderListOfTypeMuscle;
        }

        private Dictionary<(string Category, string MuscleSize), int> ExtractCategoryMuscleSizeData(IXLWorksheet worksheet, int time, int daysInWeek)
        {
            var categoryMuscleSizeData = new Dictionary<(string Category, string MuscleSize), int>();

            // סינון שורות שבהן עמודה A (משך הזמן) ועמודה D (מספר ימים) תואמות לפרמטרים שנתנו
            var matchingRows = worksheet.RowsUsed().Skip(1) // דילוג על השורה הראשונה (שורת הכותרות)
                .Where(row => row.Cell(1).GetValue<int>() == time && row.Cell(4).GetValue<int>() == daysInWeek)
                .ToList();

            if (!matchingRows.Any())
            {
                logger.LogWarning($"No rows found with time {time} and daysInWeek {daysInWeek} in the sheet.");
                throw new Exception($"No rows found with time {time} and daysInWeek {daysInWeek} in the sheet.");
            }

            // מעבר על כל השורות שזוהו והתאמת הנתונים למבנה ה-Dictionary
            foreach (var row in matchingRows)
            {
                string category = row.Cell(2).GetValue<string>(); // קטגוריה בעמודה B
                string muscleSize = row.Cell(3).GetValue<string>(); // גודל שריר בעמודה C
                int quantity = row.Cell(5).GetValue<int>(); // כמות בעמודה E

                // מפתח ה-Dictionary הוא שילוב של הקטגוריה וגודל השריר
                var key = (Category: category, MuscleSize: muscleSize);

                // הוספת הנתונים ל-Dictionary
                if (!categoryMuscleSizeData.ContainsKey(key))
                {
                    categoryMuscleSizeData[key] = quantity;
                }
                else
                {
                    logger.LogWarning($"Duplicate entry found for category '{category}' and muscle size '{muscleSize}'. Only the first occurrence is retained.");
                }
            }

            return categoryMuscleSizeData;
        }
        private Dictionary<string, int> ExtractTimeCategoryList(IXLWorksheet worksheet, int goal, int time)
        {
            var timeCategoryList = new Dictionary<string, int>();

            // סינון שורות שבהן עמודה A (משך זמן) ועמודה B (מטרה) תואמות לפרמטרים שהוזנו
            var matchingRows = worksheet.RowsUsed()
                .Where(row => row.Cell(1).GetValue<int>() == time && row.Cell(2).GetValue<int>() == goal)
                .ToList();

            if (!matchingRows.Any())
            {
                logger.LogWarning($"No rows found with time {time} and goal {goal} in the Time sheet.");
                throw new Exception($"No rows found with time {time} and goal {goal} in the Time sheet.");
            }

            // מעבר על כל השורות שזוהו והתאמת הנתונים למבנה ה-Dictionary
            foreach (var row in matchingRows)
            {
                // יצירת מפתח (קטגוריה) משילוב של הערכים בעמודות C, D
                string category = $"{row.Cell(3).GetValue<string>()}, {row.Cell(4).GetValue<string>()}";

                // הוספת קטגוריה ל-Dictionary עם ערך דיפולטיבי (למשל 1)
                if (!timeCategoryList.ContainsKey(category))
                {
                    timeCategoryList[category] = 1; // ערך דיפולטיבי
                }
                else
                {
                    logger.LogWarning($"Duplicate entry found for category '{category}'. Only the first occurrence is retained.");
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
        public async Task<List<ExerciseDTO>> GetExercisesForMuscleAndTypeAsync(string MuscleName, string TypeMuscle, int count, List<int> allowedEquipment)
        {
            try
            {
                var exercises = await muscleDAL.GetExercisesForMuscleAndTypeAsync(MuscleName, TypeMuscle, allowedEquipment);

                var exerciseDTOs = exercises.Select(e => new ExerciseDTO
                {
                    ExerciseId = e.ExerciseId,
                    ExerciseName = e.ExerciseName
                }).ToList();

                return exerciseDTOs.OrderBy(e => Guid.NewGuid()).Take(count).ToList();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error while fetching exercises for sub-muscle: {MuscleName}");
                throw;
            }
        }

        public async Task<List<ExerciseDTO>> GetExercisesForSubMuscleAsync(string subMuscleName, int v, List<string> allowedEquipment)
        {
            try
            {
                // שליפת התרגילים שמתאימים לתת-השריר מה-DAL
                var exercises = await muscleDAL.GetExercisesForSubMuscleAsync(subMuscleName);

                // סינון לפי רשימת הציוד המותר
                var filteredExercises = exercises.Where(e =>
                    e.Equipment.Any(eq => allowedEquipment.Contains(eq.EquipmentName))
                ).ToList();

                // המרת התרגילים ל-DTO
                var exerciseDTOs = filteredExercises.Select(e => new ExerciseDTO
                {
                    ExerciseId = e.ExerciseId,
                    ExerciseName = e.ExerciseName
                }).ToList();

                // החזרת מספר התרגילים הנדרש (נבחרים באופן אקראי)
                return exerciseDTOs;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error while fetching exercises for sub-muscle: {subMuscleName}");
                throw;
            }
        }

        public List<List<DayEntry>> ExtractDayListsNew(IXLWorksheet worksheet, int daysInWeek, int trainingDuration)
        {
            var dayLists = new List<List<DayEntry>>(); // רשימה של רשימות DayEntry

            // מציאת העמודות בשורה השניה (Header) עם הערך daysInWeek
            var matchingColumns = worksheet.Row(2).CellsUsed()
                .Where(cell => cell.GetValue<int>() == daysInWeek)
                .Select(cell => cell.Address.ColumnNumber)
                .ToList();

            // עבור כל עמודה שמתאימה לערך daysInWeek
            foreach (var col in matchingColumns)
            {
                var dayEntries = new List<DayEntry>(); // רשימה עבור עמודה נוכחית

                // מעבר על כל השורות מתחת לכותרת
                foreach (var row in worksheet.RowsUsed().Skip(2))
                {
                    var duration = row.Cell(1).GetValue<int>(); // קבלת הערך בעמודה הראשונה (Training Duration)

                    // בדיקה אם משך הזמן מתאים
                    if (duration == trainingDuration)
                    {
                        var key = row.Cell(2).GetValue<string>(); // קבלת הערך בעמודה השניה (Key)
                        var value = row.Cell(col).GetValue<int>(); // קבלת הערך בעמודה הנוכחית (Value)
                        var name = row.Cell(3).GetValue<string>(); // קבלת הערך בעמודה השלישית (Name)
                                                                   // יצירת אובייקט DayEntry והוספתו לרשימה
                        dayEntries.Add(new DayEntry
                        {
                            Key = key,
                            Values = value,
                            Name = name
                        });
                    }
                }

                // הוספת הרשימה של העמודה הנוכחית לרשימה הכללית
                dayLists.Add(dayEntries);
            }

            return dayLists;
        }
        public async Task<List<List<ExerciseDTO>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
        {
            var exercisePlan = new List<List<ExerciseDTO>>(); // תוכנית האימונים לכל הימים
            var usedExercises = new HashSet<int>(); // שמירה על מזהי תרגילים שכבר נבחרו

            var typeMuscleList = new List<string>();

            foreach (var dict in trainingParams.TypeMuscleData)
            {
                foreach (var pair in dict)
                {
                    foreach (var TypeValue in pair.Value)
                    {
                        typeMuscleList.Add(TypeValue);
                    }

                }
                break;
            }

            foreach (var dayList in trainingParams.DayLists) // מעבר על כל יום
            {
                var dayExercises = new List<ExerciseDTO>();

                foreach (var muscleEntry in dayList) // מעבר על כל שריר ברשימה
                {
                    string muscleName = muscleEntry.Key;
                    int exerciseCount = muscleEntry.Values;
                    string categoryName = muscleEntry.Name;//השריר הספציפי

                    // בדיקת תת-שרירים אם נדרש
                    foreach (var typeMuscle in typeMuscleList)
                    {
                        if (trainingParams.NeedSubMuscleList.Contains(typeMuscle))///לשנות לבדיקה על הסוג שריר
                        {
                            var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscleName);
                            if (subMuscles.Count > 0)//רק במידה שיש תת שריר לשריר הספציפי
                            {
                                //מעבר על כל תת שריר
                                foreach (var subMuscle in subMuscles)
                                {
                                    //מתחיל לפי הסדר ובמידה ונגמר כמות התרגילים הוא מסיים לקחת
                                    if (exerciseCount <= 0) break;

                                    //שליחה לפונקציה שמוצאת את כל התרגילים שעובדים על השריר 
                                    var exercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, 1, trainingParams.equipment);
                                    //מכניס רק את התרגילים שלא משתמשים ביום הזה כבר
                                    var filteredExercises = exercises
                                        .Where(e => !usedExercises.Contains(e.ExerciseId))
                                        .ToList();
                                    //מעבר על כל רשימת התרגילים 
                                    foreach (var exercise in filteredExercises)
                                    {
                                        dayExercises.Add(exercise);
                                        usedExercises.Add(exercise.ExerciseId);
                                        exerciseCount--;

                                        if (exerciseCount <= 0) break;
                                        //לא בטוח
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    // אם נשארו עוד תרגילים שצריך, בדוק סוגי שרירים אחרים
                    if (exerciseCount > 0)
                    {
                        var equipment = new List<int>();
                        foreach (var equipmentName in trainingParams.equipment)
                        {
                            var e = await equipmentDAL.GetEquipmentByNameAsync(equipmentName);
                            equipment.Add(e.EquipmentId);
                        }
                        //מעבר על כל הסוגי שרירים השימושיים לפי סדר
                        foreach (var muscleType in trainingParams.TypeMuscleData.SelectMany(d => d))
                        {

                            var a = 0;
                            //קבלת כל התרגילים שעובדים על שריר מסוים וסוג שריר ובציוד האפשרי
                            foreach (var nuscleTypeName in muscleType.Value)
                            {
                                var exercises = await GetExercisesForMuscleAndTypeAsync(
                               muscleName,
                               nuscleTypeName,
                               exerciseCount,
                               equipment);
                                //סינון רק התרגילים שלא משתמשים בהם כבר בתוכנית
                                var filteredExercises = exercises
                                    .Where(e => !usedExercises.Contains(e.ExerciseId))
                                    .ToList();
                                //מעבר על רשימת התרגילים האפשרית
                                foreach (var exercise in filteredExercises)
                                {
                                    dayExercises.Add(exercise);
                                    usedExercises.Add(exercise.ExerciseId);
                                    exerciseCount--;

                                    if (exerciseCount <= 0) break;
                                }
                            }
                            if (exerciseCount <= 0) break;

                        }
                    }

                    // אם עדיין חסרים תרגילים, חפש בכל הציוד
                    if (exerciseCount > 0)
                    {
                        //בוחר מספר אקראי של תרגילים כמספר החסר
                        var allEquipmentExercises = await GetExercisesForMuscleAsync(muscleName, exerciseCount);
                        foreach (var exercise in allEquipmentExercises)
                        {
                            if (usedExercises.Contains(exercise.ExerciseId)) continue;

                            dayExercises.Add(exercise);
                            usedExercises.Add(exercise.ExerciseId);
                            exerciseCount--;

                            if (exerciseCount <= 0) break;
                        }
                    }
                }

                exercisePlan.Add(dayExercises); // הוספת תרגילי היום לתוכנית
            }

            // הדפסת התוכנית ללוג
            for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
            {
                logger.LogInformation($"Day {dayIndex + 1} Exercises:");
                foreach (var exercise in exercisePlan[dayIndex])
                {
                    logger.LogInformation($"  - {exercise.ExerciseName} (ID: {exercise.ExerciseId})");
                }
            }

            return exercisePlan;
        }

        //private List<List<DayEntry>> ExtractDayLists(IXLWorksheet worksheet, int daysInWeek)
        //{
        //    var dayLists = new List<List<DayEntry>>(); // רשימה של רשימות DayEntry

        //    // מציאת העמודות בשורה הראשונה (Header) עם הערך daysInWeek
        //    var matchingColumns = worksheet.Row(1).CellsUsed()
        //        .Where(cell => cell.GetValue<int>() == daysInWeek)
        //        .Select(cell => cell.Address.ColumnNumber)
        //        .ToList();

        //    // עבור כל עמודה שמתאימה לערך daysInWeek
        //    foreach (var col in matchingColumns)
        //    {
        //        var dayEntries = new List<DayEntry>(); // רשימה עבור עמודה נוכחית

        //        // מעבר על כל השורות מתחת לכותרת
        //        foreach (var row in worksheet.RowsUsed().Skip(1))
        //        {
        //            var key = row.Cell(1).GetValue<string>(); // קבלת הערך בעמודה הראשונה (Key)
        //            var value = row.Cell(col).GetValue<int>(); // קבלת הערך בעמודה הנוכחית (Value)

        //            // יצירת אובייקט DayEntry והוספתו לרשימה
        //            dayEntries.Add(new DayEntry
        //            {
        //                Key = key,
        //                Values = value
        //            });
        //        }

        //        // הוספת הרשימה של העמודה הנוכחית לרשימה הכללית
        //        dayLists.Add(dayEntries);
        //    }

        //    return dayLists;
        //}
        //private List<object> ExtractColumnValues(IXLWorksheet worksheet, int column)
        //{
        //    return worksheet.Column(column).CellsUsed().Skip(1).Select(c => (object)c.Value).ToList();
        //}
        //private int FindColumnByValueInFirstRow(IXLWorksheet worksheet, int value, string errorMessage)
        //{
        //    foreach (var col in worksheet.ColumnsUsed())
        //    {
        //        // בדוק את הערך בשורה הראשונה של העמודה
        //        if (col.FirstCell().GetValue<int>() == value)
        //        {
        //            return col.ColumnNumber(); // החזר את מספר העמודה
        //        }
        //    }

        //    // אם הערך לא נמצא, רשום ביומן והשלך חריגה
        //    logger.LogWarning(errorMessage);
        //    throw new Exception(errorMessage);
        //}

        //private int FindRowByValue(IXLWorksheet worksheet, int column, int value, string errorMessage)
        //{
        //    foreach (var row in worksheet.RowsUsed())
        //    {
        //        if (row.Cell(column).GetValue<int>() == value)
        //        {
        //            return row.RowNumber();
        //        }
        //    }
        //    logger.LogWarning(errorMessage);
        //    throw new Exception(errorMessage);
        //}


        //private List<string> ExtractTypMuscleData(IXLWorksheet worksheet, int daysInWeek)
        //{
        //    var listTypeMuscle = new List<string>();
        //    var col = FindColumnByValue(worksheet, 1, daysInWeek, "Column not found");
        //    foreach (var row in worksheet.RowsUsed().Skip(1)) // מעבר על כל השורות
        //    {

        //        var value = row.Cell(col).GetValue<string>(); // הערך מהעמודה שנמצאה
        //        listTypeMuscle.Add(value);
        //    }
        //    return listTypeMuscle;
        //}

        //private List<Dictionary<int, string>> ExtractTypMuscleData(IXLWorksheet worksheet, int daysInWeek)
        //{
        //    var OrderListOfTypeMuscle = new List<Dictionary<int, string>>();

        //    foreach (var row in worksheet.RowsUsed().Skip(1))
        //    {
        //        if (row.Cell(1).GetValue<int>() == daysInWeek)
        //        {
        //            var dict = new Dictionary<int, string>();
        //            for (int col = 3; col <= row.LastCellUsed().Address.ColumnNumber; col++)
        //            {
        //                dict.Add(row.Cell(2).GetValue<int>(), row.Cell(col).GetValue<string>());
        //            }
        //            OrderListOfTypeMuscle.Add(dict);
        //        }
        //    }
        //    //מחזיר רשימה של דיקשינרי של הסוגי שרירים שמסודרים לפי סדר לפי המפתח בסדר עולה (1 הכי חשוב וכו) 0
        //    return OrderListOfTypeMuscle;
        //}

        //public async Task<List<List<ExerciseDTO>>> GenerateExercisePlanAsync(IBLL.TrainingParams trainingParams)
        //{
        //    // רשימה שתכיל את כל רשימות התרגילים לכל הימים
        //    var exercisePlan = new List<List<ExerciseDTO>>();

        //    // עבור כל רשימה ב-DayLists (כל יום)
        //    foreach (var dayList in trainingParams.DayLists)
        //    {
        //        var dayExercises = new List<ExerciseDTO>(); // רשימה לתרגילים של יום זה

        //        // עבור על כל שריר ברשימה של היום
        //        foreach (var muscle in dayList)
        //        {
        //            // בדוק אם השריר נמצא ב-LargeMuscleList
        //            if (trainingParams.LargeMuscleList.Contains(muscle))
        //            {
        //                // שלוף תרגילים לשריר זה לפי LargeMuscleCount
        //                var exercises = await GetExercisesForMuscleAsync(muscle.ToString(), trainingParams.LargeMuscleCount);
        //                dayExercises.AddRange(exercises);
        //            }
        //            // בדוק אם השריר נמצא ב-SmallMuscleList
        //            else if (trainingParams.SmallMuscleList.Contains(muscle))
        //            {
        //                // שלוף תרגילים לשריר זה לפי SmallMuscleCount
        //                var exercises = await GetExercisesForMuscleAsync(muscle.ToString(), trainingParams.SmallMuscleCount);
        //                dayExercises.AddRange(exercises);
        //            }
        //        }

        //        // הוסף את רשימת התרגילים של היום לרשימה הכללית
        //        exercisePlan.Add(dayExercises);
        //    }

        //    return exercisePlan;
        //}


        //public async Task<List<List<ExerciseDTO>>> GenerateExercisePlanWithEquipmentAsync(TrainingParams trainingParams)
        //{
        //    var exercisePlan = new List<List<ExerciseDTO>>(); // רשימה של רשימות תרגילים (כל יום הוא רשימה)

        //    // מעבר על כל יום ברשימת הימים
        //    foreach (var dayList in trainingParams.DayLists)
        //    {
        //        var dayExercises = new List<ExerciseDTO>(); // רשימה לתרגילים עבור היום הנוכחי
        //        var usedExercises = new HashSet<int>(); // שמירה על תרגילים שכבר נבחרו כדי למנוע כפילויות

        //        // מעבר על כל DayEntry ברשימת היום
        //        foreach (var muscle in dayList)
        //        {
        //            var muscleKey = muscle.Key; // שם השריר
        //            var exerciseCount = muscle.Values; // כמות התרגילים הנדרשת
        //            var category = muscle.Name; // קטגוריה

        //            // בדיקת תת-שרירים אם הערך "מבודדים" קיים ב-TypeMuscle
        //            if (trainingParams.TypMuscle.Contains("מבודד"))
        //            {
        //                var muscles = await muscleDAL.GetMuscleByNameAsync(muscleKey);
        //                // שליפת תת-שרירים מהמסד נתונים
        //                var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscles);

        //                //מעבר על כל תת שריר ובחירת תרגיל מתאים בשבילו
        //                foreach (var subMuscle in subMuscles)
        //                {
        //                    if (exerciseCount <= 0) break;

        //                    if (trainingParams.equipment == null || !trainingParams.equipment.Any())
        //                    {
        //                        throw new Exception("Equipment list is empty or not defined.");
        //                    }
        //                    //שליפה של כל התרגילים שעובדים על תת שריר בציוד האפשרי
        //                    var exercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, 1, trainingParams.equipment);
        //                    //מכניס רק את התרגילים שלא משתמשים ביום הזה כבר
        //                    var filteredExercises = exercises.Where(e => !usedExercises.Contains(e.ExerciseId)).ToList();
        //                    //מעבר על כל רשימת התרגילים 
        //                    foreach (var exercise in filteredExercises)
        //                    {
        //                        if (exerciseCount <= 0) break;

        //                        dayExercises.Add(exercise);
        //                        usedExercises.Add(exercise.ExerciseId);
        //                        exerciseCount--;
        //                        //לא בטוח
        //                        break;
        //                    }
        //                }
        //            }

        //            // השלמת תרגילים משרירים מורכבים אם נדרש ואם הערך "מורכבים" קיים ב-TypeMuscle
        //            if (exerciseCount > 0 && trainingParams.TypMuscle.Contains("מורכב"))
        //            {

        //                if (trainingParams.equipment == null || !trainingParams.equipment.Any())
        //                {
        //                    throw new Exception("Equipment list is empty or not defined.");
        //                }

        //                var equipmentIds = new List<int>();

        //                foreach (var equipmentName in trainingParams.equipment)
        //                {
        //                    var equipment = await equipmentDAL.GetEquipmentByNameAsync(equipmentName);
        //                    equipmentIds.Add(equipment.EquipmentId);
        //                }
        //                var complexExercises = await GetExercisesForMuscleAndTypeAsync(muscleKey, "מורכב", exerciseCount, equipmentIds);//***********************

        //                var filteredComplexExercises = complexExercises.Where(e => !usedExercises.Contains(e.ExerciseId)).ToList();

        //                //var filteredComplexExercises = complexExercises
        //                //    .Where(e => trainingParams.equipment.Contains(e.Equipment) && !usedExercises.Contains(e.ExerciseId))
        //                //    .ToList();

        //                foreach (var exercise in filteredComplexExercises)
        //                {
        //                    if (exerciseCount <= 0) break;

        //                    dayExercises.Add(exercise);
        //                    usedExercises.Add(exercise.ExerciseId);
        //                    exerciseCount--;
        //                }
        //            }
        //        }

        //        // הוספת רשימת התרגילים של היום לרשימה הכללית
        //        exercisePlan.Add(dayExercises);
        //    }

        //    // הדפסת התוכנית ללוג
        //    foreach (var (day, dayIndex) in exercisePlan.Select((value, index) => (value, index)))
        //    {
        //        logger.LogInformation($"Day {dayIndex + 1} Exercises:");

        //        foreach (var exercise in day)
        //        {
        //            logger.LogInformation($"  - {exercise.ExerciseName}");
        //        }
        //    }

        //    return exercisePlan;
        //}


        //public async Task<List<Exercise>> GetExercisesWithEquipmentFilter(string muscleName, string muscleType, List<int> equipmentIds)
        //{
        //    var exercises = await muscleDAL.GetExercisesForMuscleAndTypeAsync(muscleName, muscleType, equipmentIds);

        //    var filteredExercises = exercises.Where(e => e.Equipment.Any(eq => equipmentIds.Contains(eq.EquipmentId))).ToList();
        //    return filteredExercises;
        //}

        //public async Task<List<List<ExerciseDTO>>> GenerateExercisePlanAndLogAsync(TrainingParams trainingParams)
        //{
        //    var exercisePlan = new List<List<ExerciseDTO>>(); // רשימה של רשימות תרגילים (כל יום הוא רשימה)

        //    // מעבר על כל יום ברשימת הימים
        //    foreach (var dayList in trainingParams.DayLists)
        //    {
        //        var dayExercises = new List<ExerciseDTO>(); // רשימה לתרגילים עבור היום הנוכחי

        //        // מעבר על כל DayEntry ברשימת היום
        //        foreach (var muscle in dayList)
        //        {
        //            var muscleKey = muscle.Key; // ה-Key מייצג את שם השריר
        //            var exerciseCount = muscle.Values; // ה-Value מייצג את כמות התרגילים הנדרשת
        //            var caregory = muscle.Name;
        //            if (exerciseCount > 0)
        //            {
        //                // שליפת תרגילים מהמסד נתונים לפי שם השריר וכמות התרגילים
        //                var exercises = await GetExercisesForMuscleByCategoryAsync(muscleKey, caregory, exerciseCount);

        //                if (!exercises.Any())
        //                {
        //                    logger.LogWarning($"No exercises found for Muscle '{muscleKey}' with requested count '{exerciseCount}' in category '{caregory}'.");
        //                    continue; // אם אין תרגילים לשריר, דלג עליו
        //                }

        //                // הוספת התרגילים לרשימה של היום
        //                dayExercises.AddRange(exercises);
        //            }
        //        }

        //        // הוספת רשימת התרגילים של היום לרשימה הכללית
        //        exercisePlan.Add(dayExercises);
        //    }

        //    // הדפסת התוכנית ללוג
        //    foreach (var (day, dayIndex) in exercisePlan.Select((value, index) => (value, index)))
        //    {
        //        logger.LogInformation($"Day {dayIndex + 1} Exercises:");

        //        foreach (var exercise in day)
        //        {
        //            logger.LogInformation($"  - {exercise.ExerciseName} ");
        //        }
        //    }

        //    // החזרת התוכנית
        //    return exercisePlan;
        //}


        //private async Task<List<ExerciseDTO>> GetExercisesForMuscleAndCategoryAsync(string muscleName, string categoryName, int count)
        //{
        //    // שליפת תרגילים עבור שריר וקטגוריה
        //    var exercises = await muscleDAL.GetExercisesForMuscleAndCategoryAsync(muscleName, categoryName);

        //    // המרת התרגילים ל-DTO
        //    var exerciseDTOs = exercises.Select(e => new ExerciseDTO
        //    {
        //        ExerciseId = e.ExerciseId,
        //        ExerciseName = e.ExerciseName
        //    }).ToList();

        //    // בחר את מספר התרגילים הנדרש
        //    return exerciseDTOs.OrderBy(e => Guid.NewGuid()).Take(count).ToList();
        //}


        //public async Task<List<List<ExerciseDTO>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
        //{
        //    var exercisePlan = new List<List<ExerciseDTO>>(); // תוכנית האימונים לכל הימים
        //    var usedExercises = new HashSet<int>(); // לשמירה על תרגילים שכבר נבחרו

        //    foreach (var dayList in trainingParams.DayLists) // מעבר על כל יום
        //    {
        //        var dayExercises = new List<ExerciseDTO>();
        //        foreach (var typeMuscleData in trainingParams.TypeMuscleData) // מעבר על סוגי השרירים לפי תעדוף
        //        {
        //            foreach (var muscleType in typeMuscleData.OrderBy(d => d.Key)) // סדר עדיפות
        //            {
        //                foreach (var muscleName in muscleType.Value) // מעבר על כל שריר בסוג
        //                {
        //                    var exerciseCount = dayList.FirstOrDefault(d => d.Key == muscleName)?.Values ?? 0;
        //                    if (exerciseCount <= 0) continue;

        //                    // שליפת תרגילים לפי סוג שריר
        //                    var exercises = await GetExercisesForMuscleAndTypeAsync(muscleName, muscleType.Value[1], exerciseCount, trainingParams.equipment.Select(e => int.Parse(e)).ToList());
        //                    var filteredExercises = exercises.Where(e => !usedExercises.Contains(e.ExerciseId)).ToList();

        //                    foreach (var exercise in filteredExercises)
        //                    {
        //                        dayExercises.Add(exercise);
        //                        usedExercises.Add(exercise.ExerciseId);
        //                        if (--exerciseCount <= 0) break;
        //                    }

        //                    if (exerciseCount > 0)
        //                    {
        //                        // נסיון להוסיף תרגילים מתת-שרירים
        //                        var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscleName);
        //                        foreach (var subMuscle in subMuscles)
        //                        {
        //                            var subExercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, exerciseCount, trainingParams.equipment);
        //                            foreach (var subExercise in subExercises)
        //                            {
        //                                if (usedExercises.Contains(subExercise.ExerciseId)) continue;
        //                                dayExercises.Add(subExercise);
        //                                usedExercises.Add(subExercise.ExerciseId);
        //                                if (--exerciseCount <= 0) break;
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //        }

        //        exercisePlan.Add(dayExercises); // הוספת תרגילי היום הנוכחי לתוכנית
        //    }
        //    // הדפסת התוכנית ללוג
        //    for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
        //    {
        //        var dayExercises = exercisePlan[dayIndex];
        //        logger.LogInformation($"Day {dayIndex + 1} Exercises:");
        //        foreach (var exercise in dayExercises)
        //        {
        //            logger.LogInformation($"  - {exercise.ExerciseName} (ID: {exercise.ExerciseId})");
        //        }
        //    }

        //    return exercisePlan;
        //}


        /****************************************/
        //public async Task<List<ExerciseDTO>> GetExercisePlanAsync(TrainingParams trainingParams)
        //{
        //    // קבל את הנתונים של השרירים
        //    var muscles = trainingParams.NeedSubMuscleList;

        //    // קבל את הנתונים של הציוד
        //    var equipment = trainingParams.equipment;

        //    // קבל את הנתונים של התרגילים
        //    var exercises = await _exerciseBLL.GetAllExercisesAsync();

        //    // בנה תוכנית אימון
        //    var exercisePlan = new List<ExerciseDTO>();

        //    foreach (var muscle in muscles)
        //    {
        //        // בדוק אם יש תרגילים שעובדים על השריר הזה
        //        var exercisesForMuscle = exercises.Where(e => e.MuscleId == muscle.Id).ToList();

        //        // בדוק אם יש תרגילים שעובדים על הציוד הזה
        //        var exercisesForEquipment = exercisesForMuscle.Where(e => e.EquipmentId == equipment.Id).ToList();

        //        // בדוק אם יש תרגילים שעובדים על הסוג שריר הזה
        //        var exercisesForMuscleType = exercisesForEquipment.Where(e => e.MuscleTypeId == muscle.MuscleTypeId).ToList();

        //        // הוסף את התרגילים לתוכנית האימון
        //        exercisePlan.AddRange(exercisesForMuscleType);
        //    }

        //    // בדוק אם יש עוד תרגילים שצריך להוסיף
        //    if (exercisePlan.Count < trainingParams.NumExercises)
        //    {
        //        // הוסף את התרגילים הנוספים
        //        var additionalExercises = exercises.Where(e => !exercisePlan.Contains(e)).Take(trainingParams.NumExercises - exercisePlan.Count).ToList();
        //        exercisePlan.AddRange(additionalExercises);
        //    }

        //    return exercisePlan;
        //}


        //public class TrainingParams
        //{
        //    public List<DayEntry> DayLists { get; set; } // רשימה של אובייקטים מסוג DayEntry
        //    public Dictionary<string, List<object>> MuscleSizeData { get; set; } // מילון דינאמי לגודל שרירים
        //    public int MinRep { get; set; }
        //    public int MaxRep { get; set; }
        //    public Dictionary<(string Category, string MuscleSize), int> TimeCategories { get; set; }

        //}

        //public async Task addProgramExerciseAsync1(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time)
        //{
        //    // לא קבוע
        //    string filePath1 = @"C:\Users\user\Pictures\תכנות\שנה ב\פוריקט שנתי\C#\פרויקט חדש 20.04\Gym_Api\BLL\new.xlsx";

        //    // שליפת כל הפרמטרים מהקובץ
        //    var trainingParams = await GetAllParams(filePath1, daysInWeek, goal, level, time);

        //    if (trainingParams == null)
        //    {
        //        throw new Exception("Failed to retrieve training parameters.");
        //    }

        //    // הדפסת רשימות השרירים לכל יום
        //    for (int i = 0; i < trainingParams.DayLists.Count; i++)
        //    {
        //        logger.LogInformation($"Day List {i + 1}: " + string.Join(", ", trainingParams.DayLists[i]));
        //    }

        //    // הדפסת כל גדלי השרירים ורשימות השרירים שלהם
        //    foreach (var muscleSize in trainingParams.MuscleSizeData)
        //    {
        //        logger.LogInformation($"Muscle Size: {muscleSize.Key}");
        //        logger.LogInformation("Muscles: " + string.Join(", ", muscleSize.Value));
        //    }

        //    // הדפסת מספר החזרות
        //    logger.LogInformation($"Repetitions: {trainingParams.MinRep}-{trainingParams.MaxRep}");

        //    // הדפסת הקטגוריות והכמות לכל שריר
        //    logger.LogInformation("Time Categories: " + string.Join(", ", trainingParams.TimeCategories.Select(kvp => $"[{kvp.Key.Category} - {kvp.Key.MuscleSize}: {kvp.Value}]")));

        //    // כאן ניתן להוסיף קריאה ל-GenerateExercisePlanAsync אם יש צורך
        //    // var exercisePlan = await GenerateExercisePlanAsync(trainingParams);

        //    // הדפסת התוכנית
        //    // if (exercisePlan != null)
        //    // {
        //    //     foreach (var day in exercisePlan)
        //    //     {
        //    //         logger.LogInformation("Day Exercises:");
        //    //         foreach (var exercise in day)
        //    //         {
        //    //             logger.LogInformation($"- {exercise.ExerciseName}");
        //    //         }
        //    //     }
        //    // },

        //    foreach (var day in trainingParams.DayLists)
        //    {
        //        logger.LogInformation($"Key (Day): {day.Key}, Values: {string.Join(", ", day.Values)}");
        //    }

        //    await GenerateExercisePlanAndLogAsync(trainingParams);
        //}




        //public async Task<TrainingParams> GetAllParams(string filePath, int daysInWeek, int goal, int level, int time)
        //{
        //    try
        //    {
        //        using (var workbook = new XLWorkbook(filePath))
        //        {
        //            // רשימה של השרירים לכל יום
        //            //var daysWorksheet = GetWorksheet(workbook, DaysInWeekSheet);
        //            //var dayLists = ExtractDayLists(daysWorksheet, daysInWeek);
        //            var daysWorksheet = GetWorksheet(workbook, DaysInWeekSheet);
        //            var dayLists = ExtractDayLists(daysWorksheet, daysInWeek);

        //            // קריאת מידע של גדלי השרירים בצורה דינאמית
        //            var muscleSizeWorksheet = GetWorksheet(workbook, MuscleSheet);
        //            var muscleSizeData = ExtractMuscleSizeData(muscleSizeWorksheet);
        //            ;
        //            // מספר החזרות
        //            var repWorksheet = GetWorksheet(workbook, RepitByGoalSheet);
        //            int colRep = FindColumnByValue(repWorksheet, 1, goal, "Column with the specified goal not found.");
        //            int minRep = GetValueFromWorksheet(repWorksheet, "min", colRep, "Min value not found.");
        //            int maxRep = GetValueFromWorksheet(repWorksheet, "max", colRep, "Max value not found.");

        //            // זמן לקטגוריה
        //            var timeCategoryWorksheet = GetWorksheet(workbook, CategorySheet);
        //            var timeCategoryList = ExtractTimeCategoryList(timeCategoryWorksheet, goal, time);

        //            // הכמות לשרירים dictinary
        //            var countToMuscleWorksheet = GetWorksheet(workbook, SumOfLargeByTimeSheet);
        //            var categoryMuscleSizeData = ExtractCategoryMuscleSizeData(countToMuscleWorksheet, time, daysInWeek);

        //            // החזרת כל הנתונים בצורה דינאמית
        //            return new TrainingParams
        //            {
        //                DayLists = dayLists,
        //                MuscleSizeData = muscleSizeData, // במקום LargeMuscleList ו-SmallMuscleList
        //                MinRep = minRep,
        //                MaxRep = maxRep,
        //                TimeCategories = categoryMuscleSizeData,
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error while processing training parameters.");
        //        throw;
        //    }
        //}

        //private List<DayEntry> ExtractDayLists(IXLWorksheet worksheet, int daysInWeek)
        //{
        //    var dayEntries = new List<DayEntry>();

        //    // שליפת מספר העמודה המתאימה לערך "DAYINWEEK" בשורה השנייה
        //    var headerRow = worksheet.Row(1); // שורה שנייה
        //                                      // var dayInWeekColumn = headerRow.CellsUsed()
        //                                      //   .FirstOrDefault(cell => cell.GetValue<string>().Equals("DAYINWEEK", StringComparison.OrdinalIgnoreCase));
        //    var dayInWeekColumn = headerRow.CellsUsed()
        //        .FirstOrDefault(cell => cell.GetValue<int>() == daysInWeek);
        //    if (dayInWeekColumn == null)
        //    {
        //        throw new Exception("Column with header 'DAYINWEEK' not found in the sheet.");
        //    }

        //    int dayInWeekColumnNumber = dayInWeekColumn.Address.ColumnNumber;

        //    //**********************************************************************
        //    // מעבר על השורות מתחת לשורת הכותרת (למשל, משורה שלישית ואילך)
        //    foreach (var row in worksheet.RowsUsed().Skip(1)) // דילוג על שורות 1 ו-2 (כותרת ושורת DAYINWEEK)
        //    {
        //        var key = row.Cell(1).GetValue<string>(); // המפתח הוא הערך בעמודה הראשונה
        //        var value = row.Cell(dayInWeekColumnNumber).GetValue<string>(); // הערך מהעמודה שנמצאה

        //        // יצירת אובייקט חדש מסוג DayEntry והוספתו לרשימה
        //        dayEntries.Add(new DayEntry
        //        {
        //            Key = key,
        //            Values = value
        //        });
        //    }

        //    return dayEntries;
        //}



        //public async Task GenerateExercisePlanAndLogAsync(TrainingParams trainingParams)
        //{
        //    var exercisePlan = new List<List<List<ExerciseDTO>>>(); // רשימה כללית של תרגילים לכל הימים

        //    // מעבר על כל יום ברשימת הימים
        //    foreach (var dayList in trainingParams.DayLists)
        //    {
        //        var dayExercises = new List<List<ExerciseDTO>>(); // רשימה לתרגילים עבור היום הנוכחי

        //        // מעבר על כל DayEntry ברשימת היום
        //        foreach (var dayEntry in dayList)
        //        {
        //            var muscleGroupExercises = new List<ExerciseDTO>(); // רשימה לתרגילים עבור קבוצת השרירים

        //            // קבלת ה-Key וה-Values מתוך DayEntry
        //            var dayKey = dayEntry.Key; // ה-Key מייצג את היום
        //            var muscles = dayEntry.Values.Split(',').Select(m => m.Trim()); // פיצול רשימת השרירים

        //            // מעבר על כל שריר בקבוצה
        //            foreach (var muscle in muscles)
        //            {
        //                var matchingMuscleSize = trainingParams.MuscleSizeData.FirstOrDefault(ms => ms.Value
        //                    .Any(m => string.Equals(m.ToString().Trim(), muscle.Trim(), StringComparison.OrdinalIgnoreCase)));

        //                if (matchingMuscleSize.Key == null)
        //                {
        //                    logger.LogWarning($"Muscle '{muscle}' not found in MuscleSizeData. Available muscles: {string.Join(", ", trainingParams.MuscleSizeData.SelectMany(ms => ms.Value))}");
        //                    continue; // דלג על השריר אם הוא לא נמצא
        //                }

        //                // מצא את הקטגוריה והכמות מהדיקשנרי TimeCategories
        //                var matchingCategory = trainingParams.TimeCategories
        //                    .FirstOrDefault(tc => tc.Key.MuscleSize == matchingMuscleSize.Key);

        //                if (matchingCategory.Key.Category == null)
        //                {
        //                    logger.LogWarning($"No matching category found for MuscleSize '{matchingMuscleSize.Key}' and Muscle '{muscle}'.");
        //                    continue; // דלג אם אין קטגוריה מתאימה
        //                }

        //                int exerciseCount = matchingCategory.Value;

        //                // שלוף תרגילים מהמסד נתונים
        //                var exercises = await GetExercisesForMuscleAndCategoryAsync(muscle, matchingCategory.Key.Category, exerciseCount);

        //                if (!exercises.Any())
        //                {
        //                    logger.LogWarning($"No exercises found for Muscle '{muscle}' and Category '{matchingCategory.Key.Category}'.");
        //                }

        //                muscleGroupExercises.AddRange(exercises); // הוספת התרגילים לרשימת קבוצת השרירים
        //            }

        //            dayExercises.Add(muscleGroupExercises); // הוספת קבוצת השרירים ליום הנוכחי
        //        }

        //        exercisePlan.Add(dayExercises); // הוספת היום לרשימה הכללית
        //    }

        //public async Task GenerateExercisePlanAndLogAsync(TrainingParams trainingParams)
        //{
        //    var exercisePlan = new List<List<ExerciseDTO>>(); // רשימה של רשימות תרגילים (כל יום הוא רשימה)

        //    // מעבר על כל יום ברשימת הימים
        //    foreach (var dayList in trainingParams.DayLists)
        //    {
        //        var dayExercises = new List<ExerciseDTO>(); // רשימה לתרגילים עבור היום הנוכחי

        //        // מעבר על כל DayEntry ברשימת היום
        //        foreach (var dayEntry in dayList)
        //        {
        //            // קבלת ה-Key וה-Values מתוך DayEntry
        //            var dayKey = dayEntry.Key; // ה-Key מייצג את היום
        //            var muscles = dayEntry.Values.Split(',').Select(m => m.Trim()); // פיצול רשימת השרירים

        //            // מעבר על כל שריר בקבוצה
        //            foreach (var muscle in muscles)
        //            {
        //                // מציאת התאמה לשריר במידע MuscleSizeData
        //                var matchingMuscleSize = trainingParams.MuscleSizeData.FirstOrDefault(ms => ms.Value
        //                    .Any(m => string.Equals(m.ToString().Trim(), muscle.Trim(), StringComparison.OrdinalIgnoreCase)));

        //                if (matchingMuscleSize.Key == null)
        //                {
        //                    logger.LogWarning($"Muscle '{muscle}' not found in MuscleSizeData. Available muscles: {string.Join(", ", trainingParams.MuscleSizeData.SelectMany(ms => ms.Value))}");
        //                    continue; // דלג על השריר אם הוא לא נמצא
        //                }

        //                // מציאת הקטגוריה והכמות מתוך TimeCategories
        //                var matchingCategory = trainingParams.TimeCategories
        //                    .FirstOrDefault(tc => tc.Key.MuscleSize == matchingMuscleSize.Key);

        //                if (matchingCategory.Key.Category == null)
        //                {
        //                    logger.LogWarning($"No matching category found for MuscleSize '{matchingMuscleSize.Key}' and Muscle '{muscle}'.");
        //                    continue; // דלג אם אין קטגוריה מתאימה
        //                }

        //                int exerciseCount = matchingCategory.Value;

        //                // שליפת תרגילים מהמסד נתונים
        //                var exercises = await GetExercisesForMuscleAndCategoryAsync(muscle, matchingCategory.Key.Category, exerciseCount);

        //                if (!exercises.Any())
        //                {
        //                    logger.LogWarning($"No exercises found for Muscle '{muscle}' and Category '{matchingCategory.Key.Category}'.");
        //                }

        //                dayExercises.AddRange(exercises); // הוספת התרגילים לרשימה של היום
        //            }
        //        }

        //        exercisePlan.Add(dayExercises); // הוספת תרגילי היום לרשימה הכללית
        //    }

        //    // הדפסת התוכנית ללוג
        //    foreach (var (day, dayIndex) in exercisePlan.Select((value, index) => (value, index)))
        //    {
        //        logger.LogInformation($"Day {dayIndex + 1} Exercises:");

        //        foreach (var exercise in day)
        //        {
        //            logger.LogInformation($"  - {exercise.ExerciseName}");
        //        }
        //    }

        //}


        // פונקציה לשליפת כל הערכים בעמודה מסוימת
        //public List<object> ExtractColumnValues(IXLWorksheet worksheet, int column)
        //{
        //    return worksheet.Column(column).CellsUsed().Skip(1).Select(c => (object)c.Value).ToList();
        //}
        //public async Task GenerateExercisePlanAndLogAsync(TrainingParams trainingParams)
        //{
        //    var exercisePlan = new List<List<ExerciseDTO>>();

        //    // מעבר על כל יום ברשימת הימים
        //    foreach (var dayEntry in trainingParams.DayLists)
        //    {
        //        var dayKey = dayEntry.Key; // ה-Key מייצג את היום
        //        var muscle = dayEntry.Values; // ה-Values מייצגים את רשימת השרירים עבור היום

        //        var dayExercises = new List<ExerciseDTO>();

        //        //// מעבר על כל שריר ברשימה של אותו יום
        //        ////string cleanedMuscleName = muscle.ToString().Trim().ToLower();

        //        //// מצא את גודל השריר מתוך MuscleSizeData
        //        ////var matchingMuscleSize = trainingParams.MuscleSizeData
        //        ////    .FirstOrDefault(ms => ms.Value
        //        ////        .Select(m => m.ToString().Trim().ToLower())
        //        ////        .Contains(cleanedMuscleName));
        //        //var matchingMuscleSize = trainingParams.MuscleSizeData
        //        //    .FirstOrDefault(ms => ms.Value.Contains(muscle));


        //        //if (matchingMuscleSize.Key == null)
        //        //{
        //        //    logger.LogWarning($"Muscle '{muscle}' not found in MuscleSizeData. Available muscles: {string.Join(", ", trainingParams.MuscleSizeData.SelectMany(ms => ms.Value))}");
        //        //    continue; // דלג על השריר אם הוא לא נמצא
        //        //}

        //        var matchingMuscleSize = trainingParams.MuscleSizeData.FirstOrDefault(ms => ms.Value
        //              .Any(m => string.Equals(m.ToString().Trim(), muscle.Trim(), StringComparison.OrdinalIgnoreCase)));

        //        if (matchingMuscleSize.Key == null)
        //        {
        //            logger.LogWarning($"Muscle '{muscle}' not found in MuscleSizeData. Available muscles: {string.Join(", ", trainingParams.MuscleSizeData.SelectMany(ms => ms.Value))}");
        //            continue; // דלג על השריר אם הוא לא נמצא
        //        }



        //        // מצא את הקטגוריה והכמות מהדיקשנרי TimeCategories
        //        var matchingCategory = trainingParams.TimeCategories
        //            .FirstOrDefault(tc => tc.Key.MuscleSize == matchingMuscleSize.Key);

        //        if (matchingCategory.Key.Category == null)
        //        {
        //            logger.LogWarning($"No matching category found for MuscleSize '{matchingMuscleSize.Key}' and Muscle '{muscle}'.");
        //            continue; // דלג אם אין קטגוריה מתאימה
        //        }

        //        int exerciseCount = matchingCategory.Value;

        //        // כאן הערך של הקטגוריה מוחלף ב-dayKey
        //       // logger.LogInformation($"Retrieving exercises for Muscle: '{muscle}', Category: '{dayKey}', Count: {exerciseCount}");

        //        // שלוף תרגילים מהמסד נתונים
        //        var exercises = await GetExercisesForMuscleAndCategoryAsync(muscle, dayKey, exerciseCount);

        //        if (!exercises.Any())
        //        {
        //            logger.LogWarning($"No exercises found for Muscle '{muscle}' and Category '{dayKey}'.");
        //        }

        //        dayExercises.AddRange(exercises);


        //        // הוסף את רשימת התרגילים של היום לתוכנית
        //        exercisePlan.Add(dayExercises);
        //    }

        //    // הדפסת התוכנית ללוג
        //    foreach (var (day, index) in exercisePlan.Select((value, index) => (value, index)))
        //    {
        //        logger.LogInformation($"Exercises {index + 1}:");
        //        foreach (var exercise in day)
        //        {
        //            logger.LogInformation($"- {exercise.ExerciseName}");
        //        }
        //    }
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
        //private Dictionary<object, List<object>> ExtractDayLists(IXLWorksheet worksheet, int daysInWeek)
        //{
        //    var dayLists = new Dictionary<object, List<object>>();

        //    foreach (var row in worksheet.RowsUsed().Skip(1)) // דילוג על שורת הכותרת
        //    {
        //        var key = row.Cell(1).Value; // המפתח הוא הערך בעמודה הראשונה
        //        var values = row.Cells(2, row.LastCellUsed().Address.ColumnNumber)
        //                        .Select(c => (object)c.Value)
        //                        .ToList(); // הערכים הם שאר התאים בשורה

        //        if (!dayLists.ContainsKey(key))
        //        {
        //            dayLists[key] = values;
        //        }
        //        else
        //        {
        //            throw new Exception($"Duplicate key '{key}' found in the sheet.");
        //        }
        //    }

        //    return dayLists;
        //}

        // הפונקציה המעודכנת לטעינת הנתונים לאובייקט DayEntry
        //private List<DayEntry> ExtractDayLists(IXLWorksheet worksheet, int daysInWeek)
        //{
        //    var dayEntries = new List<DayEntry>();

        //    foreach (var row in worksheet.RowsUsed().Skip(1)) // דילוג על שורת הכותרת
        //    {
        //       // var key = row.Cell(1).GetValue<string>(); // המפתח הוא הערך בעמודה הראשונה כ-String
        //        //var values = row.Cells(2, row.LastCellUsed().Address.ColumnNumber)
        //        //                .Select(c => c.GetValue<string>())
        //        //                .ToList(); // הערכים הם שאר התאים בשורה כ-Strings
        //        var values2 = row.Cells(2,
        //        // יצירת אובייקט חדש מסוג DayEntry והוספתו לרשימה
        //        dayEntries.Add(new DayEntry
        //        {
        //            Key = key,
        //            Values = values
        //        });
        //    }

        //    return dayEntries;
        //}


        //private Dictionary<int, int> ExtractTimeCategoryList(IXLWorksheet worksheet, int time)
        //{
        //    var timeCategoryList = new Dictionary<int, int>();

        //    // מצא את השורה בעמודה הראשונה שבה הערך שווה ל-time
        //    int targetRow = FindRowByValue(worksheet, 1, time, $"Row with the specified time ({time}) not found.");

        //    // מצא את מספר העמודה המתאימה ל-TIME
        //    int timeColumn = 1; // העמודה הראשונה שבה מחפשים את הערך TIME

        //    // עבור על כל העמודות בשורה שנמצאה
        //    foreach (var col in worksheet.ColumnsUsed())
        //    {
        //        // אם העמודה היא העמודה שבה נמצא הערך time, דלג עליה
        //        if (col.ColumnNumber() == timeColumn)
        //        {
        //            continue;
        //        }

        //        var keyCell = col.Cell(1); // התא בשורה הראשונה (כותרת)
        //        var valueCell = col.Cell(targetRow); // התא בעמודה ובשורה שנמצאה

        //        // ודא שהתאים אינם ריקים ושהערכים שלהם חוקיים
        //        if (!keyCell.IsEmpty() && !valueCell.IsEmpty() &&
        //            int.TryParse(keyCell.GetValue<string>(), out int key) &&
        //            int.TryParse(valueCell.GetValue<string>(), out int value))
        //        {
        //            timeCategoryList[key] = value; // הוסף למילון
        //        }
        //        else
        //        {
        //            logger.LogWarning($"Skipping invalid or empty cells at {keyCell.Address} or {valueCell.Address}");
        //        }
        //    }

        //    return timeCategoryList;
        //}
        //private Dictionary<int, int> ExtractTimeCategoryList(IXLWorksheet worksheet, int goal, int time)
        //{
        //    var timeCategoryList = new Dictionary<int, int>();

        //    // Filter rows where column 2 matches the provided goal
        //    var matchingRows = worksheet.RowsUsed()
        //        .Where(row => row.Cell(2).GetValue<int>() == goal)
        //        .ToList();

        //    if (!matchingRows.Any())
        //    {
        //        logger.LogWarning($"No rows found with goal {goal} in the Time sheet.");
        //        throw new Exception($"No rows found with goal {goal} in the Time sheet.");
        //    }

        //    // Find the column where the first row matches the provided time
        //    int timeColumn = worksheet.ColumnsUsed()
        //        .FirstOrDefault(col => col.FirstCell().GetValue<int>() == time)?
        //        .ColumnNumber() ?? -1;

        //    if (timeColumn == -1)
        //    {
        //        logger.LogWarning($"Column with time {time} not found in the Time sheet.");
        //        throw new Exception($"Column with time {time} not found in the Time sheet.");
        //    }

        //    // Populate the dictionary with values from the matching rows
        //    foreach (var row in matchingRows)
        //    {
        //        int categoryId = row.Cell(2).GetValue<int>(); // Column 2 contains category ID
        //        int duration = row.Cell(timeColumn).GetValue<int>(); // Value in the time column
        //        timeCategoryList[categoryId] = duration;
        //    }

        //    return timeCategoryList;
        //}
        //private Dictionary<string, int> ExtractTimeCategoryList(IXLWorksheet worksheet, int goal, int time)
        //{
        //    var timeCategoryList = new Dictionary<string, int>();

        //    // סינון שורות שבהן עמודה A (משך זמן) ועמודה B (מטרה) תואמות לפרמטרים שהוזנו
        //    var matchingRows = worksheet.RowsUsed()
        //        .Where(row => row.Cell(1).GetValue<int>() == time && row.Cell(2).GetValue<int>() == goal)
        //        .ToList();

        //    if (!matchingRows.Any())
        //    {
        //        logger.LogWarning($"No rows found with time {time} and goal {goal} in the Time sheet.");
        //        throw new Exception($"No rows found with time {time} and goal {goal} in the Time sheet.");
        //    }

        //    // מעבר על כל השורות שזוהו והתאמת הנתונים למבנה ה-Dictionary
        //    foreach (var row in matchingRows)
        //    {
        //        string category = $"{row.Cell(3).GetValue<string>()}, {row.Cell(4).GetValue<string>()}, {row.Cell(5).GetValue<string>()}"; // שילוב של קטגוריות
        //        int quantity = row.Cell(5).GetValue<int>(); // כמות מעמודה E

        //        // הוספת הנתונים ל-Dictionary
        //        if (!timeCategoryList.ContainsKey(category))
        //        {
        //            timeCategoryList[category] = quantity;
        //        }
        //        else
        //        {
        //            logger.LogWarning($"Duplicate entry found for category '{category}'. Only the first occurrence is retained.");
        //        }
        //    }

        //    return timeCategoryList;
        //}



        //public async Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time)
        //{
        //    //לא קבוע
        //    string filePath1 = @"C:\Users\user\Pictures\תכנות\שנה ב\פוריקט שנתי\C#\פרויקט חדש 20.04\Gym_Api\BLL\new.xlsx";
        //   // string filePath1 = @"C:\Users\user\הורדות\אקסל מסודר.xlsx";
        //    var trainingParams = await GetAllParams(filePath1, daysInWeek, goal, level, time);

        //    if (trainingParams == null)
        //    {
        //        throw new Exception("Failed to retrieve training parameters.");
        //    }

        //    // Log retrieved parameters
        //    for (int i = 0; i < trainingParams.DayLists.Count; i++)
        //    {
        //        logger.LogInformation($"Day List {i + 1}: " + string.Join(", ", trainingParams.DayLists[i]));
        //    }

        //    logger.LogInformation("Large Muscles: " + string.Join(", ", trainingParams.LargeMuscleList));
        //    logger.LogInformation("Small Muscles: " + string.Join(", ", trainingParams.SmallMuscleList));
        //    logger.LogInformation($"Repetitions: {trainingParams.MinRep}-{trainingParams.MaxRep}");
        //    logger.LogInformation("Time Categories: " + string.Join(", ", trainingParams.TimeCategories.Select(kvp => $"[{kvp.Key.Category} - {kvp.Key.MuscleSize}: {kvp.Value}]")));
        //    //var exercisePlan = await GenerateExercisePlanAsync(trainingParams);

        //    // הדפסת התוכנית
        //    //foreach (var day in exercisePlan)
        //    //{
        //    //    Console.WriteLine("Day Exercises:");
        //    //    foreach (var exercise in day)
        //    //    {
        //    //        Console.WriteLine($"- {exercise.ExerciseName}");
        //    //    }
        //    //}
        //    //return exercisePlan;
        //}

        //public async Task<TrainingParams> GetAllParams(string filePath, int daysInWeek, int goal, int level, int time)
        //{
        //    try
        //    {
        //        using (var workbook = new XLWorkbook(filePath))
        //        {
        //            //רשימה של השרירים
        //            var daysWorksheet = GetWorksheet(workbook, DaysInWeekSheet);
        //            var dayLists = ExtractDayLists(daysWorksheet, daysInWeek);
        //            // רשימה של השרירים הגדולים
        //            var largeMuscleWorksheet = GetWorksheet(workbook, MuscleSheet);
        //            var largeMuscleList = largeMuscleWorksheet.Column(1).CellsUsed().Skip(1).Select(c => (object)c.Value).ToList();
        //            // רשימה של השרירים הקטנים
        //            var SmallMuscleWorksheet = GetWorksheet(workbook, MuscleSheet);
        //            var SmallMuscleList = SmallMuscleWorksheet.Column(2).CellsUsed().Skip(1).Select(c => (object)c.Value).ToList();
        //            //מספר החזרות
        //            var repWorksheet = GetWorksheet(workbook, RepitByGoalSheet);
        //            int colRep = FindColumnByValue(repWorksheet, 1, goal, "Column with the specified goal not found.");
        //            int minRep = GetValueFromWorksheet(repWorksheet, "min", colRep, "Min value not found.");
        //            int maxRep = GetValueFromWorksheet(repWorksheet, "max", colRep, "Max value not found.");
        //            //זמן לקטגוריה
        //            var timeCategoryWorksheet = GetWorksheet(workbook, CategorySheet);
        //            var timeCategoryList = ExtractTimeCategoryList(timeCategoryWorksheet, goal, time);
        //            //הכמות לשרירים dictinery 
        //            var countToMuscleWorksheet = GetWorksheet(workbook, SumOfLargeByTimeSheet);
        //            var categoryMuscleSizeData = ExtractCategoryMuscleSizeData(countToMuscleWorksheet, time, daysInWeek);

        //            return new TrainingParams
        //            {
        //                DayLists = dayLists,
        //                LargeMuscleList = largeMuscleList,
        //                SmallMuscleList = SmallMuscleList,
        //                MinRep = minRep,
        //                MaxRep = maxRep,
        //                TimeCategories= categoryMuscleSizeData,
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error while processing training parameters.");
        //        throw;
        //    }
        //}


        //public async Task GenerateExercisePlanAndLogAsync(TrainingParams trainingParams)
        //{
        //    var exercisePlan = new List<List<ExerciseDTO>>();

        //    // מעבר על כל יום ברשימת הימים
        //    foreach (var dayList in trainingParams.DayLists)
        //    {
        //        var dayExercises = new List<ExerciseDTO>();

        //        // מעבר על כל שריר ברשימה של היום
        //        foreach (var muscle in dayList)
        //        {
        //            string cleanedMuscleName = muscle.ToString().Trim().ToLower();

        //            // מצא את גודל השריר מתוך MuscleSizeData
        //            var matchingMuscleSize = trainingParams.MuscleSizeData
        //                .FirstOrDefault(ms => ms.Value
        //                    .Select(m => m.ToString().Trim().ToLower())
        //                    .Contains(cleanedMuscleName));

        //            if (matchingMuscleSize.Key == null)
        //            {
        //                logger.LogWarning($"Muscle '{cleanedMuscleName}' not found in MuscleSizeData. Available muscles: {string.Join(", ", trainingParams.MuscleSizeData.SelectMany(ms => ms.Value))}");
        //                continue; // דלג על השריר אם הוא לא נמצא
        //            }

        //            // מצא את הקטגוריה והכמות מהדיקשנרי TimeCategories
        //            var matchingCategory = trainingParams.TimeCategories
        //                .FirstOrDefault(tc => tc.Key.MuscleSize == matchingMuscleSize.Key);

        //            if (matchingCategory.Key.Category == null)
        //            {
        //                logger.LogWarning($"No matching category found for MuscleSize '{matchingMuscleSize.Key}' and Muscle '{cleanedMuscleName}'.");
        //                continue; // דלג אם אין קטגוריה מתאימה
        //            }

        //            int exerciseCount = matchingCategory.Value;
        //            logger.LogInformation($"Retrieving exercises for Muscle: '{cleanedMuscleName}', Category: '{matchingCategory.Key.Category.Trim()}', Count: {exerciseCount}");

        //            // שלוף תרגילים מהמסד נתונים
        //            var exercises = await GetExercisesForMuscleAndCategoryAsync(cleanedMuscleName, matchingCategory.Key.Category.Trim(), exerciseCount);

        //            if (!exercises.Any())
        //            {
        //                logger.LogWarning($"No exercises found for Muscle '{cleanedMuscleName}' and Category '{matchingCategory.Key.Category}'.");
        //            }

        //            dayExercises.AddRange(exercises);
        //        }

        //        // הוסף את רשימת התרגילים של היום לתוכנית
        //        exercisePlan.Add(dayExercises);
        //    }

        //    // הדפסת התוכנית ללוג
        //    foreach (var (day, index) in exercisePlan.Select((value, index) => (value, index)))
        //    {
        //        logger.LogInformation($"Day {index + 1} Exercises:");
        //        foreach (var exercise in day)
        //        {
        //            logger.LogInformation($"- {exercise.ExerciseName}");
        //        }
        //    }
        //}

        //public async Task GenerateExercisePlanAndLogAsync(TrainingParams trainingParams)
        //{
        //    var exercisePlan = new List<List<ExerciseDTO>>();

        //    // מעבר על כל יום במילון הימים
        //    foreach (var dayEntry in trainingParams.DayLists)
        //    {
        //        var dayKey = dayEntry.Key; // ה-Key מייצג את היום
        //        var dayList = dayEntry.Value; // ה-Value מייצג את רשימת השרירים עבור היום

        //        var dayExercises = new List<ExerciseDTO>();

        //        // מעבר על כל שריר ברשימה של אותו יום
        //        foreach (var muscle in dayList)
        //        {
        //            string cleanedMuscleName = muscle.ToString().Trim().ToLower();

        //            // מצא את גודל השריר מתוך MuscleSizeData
        //            var matchingMuscleSize = trainingParams.MuscleSizeData
        //                .FirstOrDefault(ms => ms.Value
        //                    .Select(m => m.ToString().Trim().ToLower())
        //                    .Contains(cleanedMuscleName));

        //            if (matchingMuscleSize.Key == null)
        //            {
        //                logger.LogWarning($"Muscle '{cleanedMuscleName}' not found in MuscleSizeData. Available muscles: {string.Join(", ", trainingParams.MuscleSizeData.SelectMany(ms => ms.Value))}");
        //                continue; // דלג על השריר אם הוא לא נמצא
        //            }

        //            // מצא את הקטגוריה והכמות מהדיקשנרי TimeCategories
        //            var matchingCategory = trainingParams.TimeCategories
        //                .FirstOrDefault(tc => tc.Key.MuscleSize == matchingMuscleSize.Key);

        //            if (matchingCategory.Key.Category == null)
        //            {
        //                logger.LogWarning($"No matching category found for MuscleSize '{matchingMuscleSize.Key}' and Muscle '{cleanedMuscleName}'.");
        //                continue; // דלג אם אין קטגוריה מתאימה
        //            }

        //            int exerciseCount = matchingCategory.Value;

        //            logger.LogInformation($"Retrieving exercises for Muscle: '{cleanedMuscleName}', Category: '{dayKey}', Count: {exerciseCount}");

        //            // שלוף תרגילים מהמסד נתונים
        //            var exercises = await GetExercisesForMuscleAndCategoryAsync(cleanedMuscleName, matchingCategory.Key.Category.Trim(), exerciseCount);

        //            if (!exercises.Any())
        //            {
        //                logger.LogWarning($"No exercises found for Muscle '{cleanedMuscleName}' and Category '{matchingCategory.Key.Category}'.");
        //            }

        //            dayExercises.AddRange(exercises);
        //        }

        //        // הוסף את רשימת התרגילים של היום לתוכנית
        //        exercisePlan.Add(dayExercises);
        //    }

        //    // הדפסת התוכנית ללוג
        //    foreach (var (day, index) in exercisePlan.Select((value, index) => (value, index)))
        //    {
        //        logger.LogInformation($"Day {index + 1} Exercises:");
        //        foreach (var exercise in day)
        //        {
        //            logger.LogInformation($"- {exercise.ExerciseName}");
        //        }
        //    }
        //}

        //public async Task<List<ExerciseDTO>> GetExercisesForMuscleByCategoryAsync(string muscleName, string categoryName, int count)
        //{
        //    // שליפת תרגילים מה-DAL
        //    var exercises = await muscleDAL.GetExercisesForMuscleByCategoryAsync(muscleName, categoryName);

        //    // המרת התרגילים ל-DTO
        //    var exerciseDTOs = exercises.Select(e => new ExerciseDTO
        //    {
        //        ExerciseId = e.ExerciseId,
        //        ExerciseName = e.ExerciseName
        //    }).ToList();

        //    // בחר מספר תרגילים באופן אקראי
        //    return exerciseDTOs.OrderBy(e => Guid.NewGuid()).Take(count).ToList();
        //}


    }
}

