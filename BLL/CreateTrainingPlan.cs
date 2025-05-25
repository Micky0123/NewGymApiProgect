using AutoMapper;
using ClosedXML.Excel;
using DAL;
using DBEntities.Models;
using DTO;
using IDAL;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    //מיכלוש שימי לב שצריך לקבל את השמות ושהם לא יהיו קבועים//****************************
    public class DayEntry
    {
        public string Key { get; set; }//קטגוריה
        public int Values { get; set; }//מספר התרגילים לשריר 
        public string Name { get; set; }//שם השריר
    }

    public class ExerciseWithMuscleInfo
    {
        public ExerciseDTO Exercise { get; set; }
        public string MuscleName { get; set; }
        public string SubMuscleName { get; set; }
        public int categoryId { get; set; }
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

    public class CreateTrainingPlan
    {
        private readonly IMuscleTypeDAL muscleTypeDAL;
        private readonly IMuscleDAL muscleDAL;
        private readonly IEquipmentDAL equipmentDAL;
        private readonly IExerciseDAL exerciseDAL;
        //private readonly IProgramExerciseDAL programExerciseDAL;
        private readonly ILogger<TrainingPlanBLL> logger;
        private readonly IMapper mapper;
        private readonly List<string> equipmentList;
        private readonly ITrainingDurationDAL trainingDurationDAL;
        private readonly ICategoryDAL categoryDAL;
        private readonly ISubMuscleDAL subMuscleDAL;

        private readonly IExercisePlanDAL exercisePlanDAL;
        private readonly ITrainingPlanDAL trainingPlanDAL;
        private readonly IPlanDayDAL planDayDAL;

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
        private const string subMusclePriorityOrder = "OrderList";
        private const string musclePriorityOrder = "OrderList";

        private const string OrderListSheet = "OrderList";

        public CreateTrainingPlan(IMuscleDAL muscleDAL, ILogger<TrainingPlanBLL> logger, ISubMuscleDAL subMuscleDAL, ICategoryDAL categoryDAL, IMuscleTypeDAL muscleTypeDAL, ITrainingDurationDAL trainingDurationDAL, IEquipmentDAL equipmentDAL, IExerciseDAL exerciseDAL, IExercisePlanDAL exercisePlanDAL, ITrainingPlanDAL trainingPlanDAL, IPlanDayDAL planDayDAL)
        {
            this.muscleDAL = muscleDAL;
            this.equipmentDAL = equipmentDAL;
            this.muscleTypeDAL = muscleTypeDAL;
            this.exerciseDAL = exerciseDAL;
            this.trainingDurationDAL = trainingDurationDAL;
            this.categoryDAL = categoryDAL;
          //  this.logger = logger;
            this.equipmentList = new List<string>();
           // this.programExerciseDAL = programExerciseDAL;
            this.subMuscleDAL = subMuscleDAL;

            this.trainingPlanDAL = trainingPlanDAL;
            this.exercisePlanDAL = exercisePlanDAL;
            this.planDayDAL = planDayDAL;

            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Muscle, MuscleDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
            this.exerciseDAL = exerciseDAL;
            this.exercisePlanDAL = exercisePlanDAL;
            this.trainingPlanDAL = trainingPlanDAL;
            this.planDayDAL = planDayDAL;
        }

        public async Task addProgramExerciseAsync1( int daysInWeek, int goal, int level, int time1, int traineeID)
        {
            //**********מיכלוש שימי לב שצריך לקבל את השמות ושהם לא יהיו קבועים/***************************
            string filePath1 = @"C:\Users\user\Pictures\תכנות\שנה ב\פוריקט שנתי\C#\פרויקט חדש 20.04\Gym_Api\BLL\new.xlsx";
            var equi = await equipmentDAL.GetAllEquipmentsAsync();
            foreach (var e in equi)
            {
                this.equipmentList.Add(e.EquipmentName);
            }
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

            var time = await trainingDurationDAL.GetTrainingDurationIDByValue(time1);
            // שליפת כל הפרמטרים מהקובץ
            trainingParams = await GetAllParams(filePath1, daysInWeek, goal, level, time1);

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
            //logger.LogInformation("List of subMuscleOfMuscleList: " + string.Join(", ", trainingParams.subMuscleOfMuscleList));
            foreach (var dict in trainingParams.subMuscleOfMuscleList)
            {
                logger.LogInformation($"List of subMuscleOfMuscleList: {dict}");
            }


            var ListOfProgram = await GenerateOptimizedExercisePlanAsync(trainingParams);
            await SaveProgramDefaultAsync(trainingParams, ListOfProgram, traineeID, programName: "Default Training Program", daysInWeek, goal, level, time);
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

                    var subMuscleOfMuscleListData = await muscleDAL.GetMusclesOfSubMuscle();

                    var orderListSheet = GetWorksheet(workbook, OrderListSheet);
                    var orderListData = ExtractOrderListData(orderListSheet);

                    var MorderListSheet = GetWorksheet(workbook, OrderListSheet);
                    var subMuscleData = ExtractSubMuscleOrderListData(MorderListSheet);
                    var MuscleData = ExtractMuscleOrderListData(MorderListSheet);
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
                        subMuscleOfMuscleList = subMuscleOfMuscleListData,
                        OrderList = orderListData,
                        subMusclePriorityOrder = subMuscleData,
                        musclePriorityOrder = MuscleData,
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
        private Dictionary<int, string> ExtractOrderListData(IXLWorksheet worksheet)
        {
            var orderListData = new Dictionary<int, string>();

            // מעבר על כל העמודות בשורה הראשונה
            foreach (var row in worksheet.RowsUsed())
            {
                orderListData.Add(row.Cell(1).GetValue<int>(), row.Cell(2).Value.ToString());
            }
            return orderListData;
        }
        private List<string> ExtractMuscleOrderListData(IXLWorksheet worksheet)
        {
            var MuscleOrderListData = new List<string>();

            // מעבר על כל העמודות בשורה הראשונה
            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                MuscleOrderListData.Add(row.Cell(2).Value.ToString());
            }
            return MuscleOrderListData;
        }
        private List<string> ExtractSubMuscleOrderListData(IXLWorksheet worksheet)
        {
            var SubMuscleOrderListData = new List<string>();

            // מעבר על כל העמודות בשורה הראשונה
            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                SubMuscleOrderListData.Add(row.Cell(3).Value.ToString());
            }
            return SubMuscleOrderListData;
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
            //return exerciseDTOs.OrderBy(e => Guid.NewGuid()).Take(count).ToList();
            // אני מנסה שהוא יחזיר את כל התרגילים ואח"כ הוא יבחר ברשימת התרגילים הסופית
            return exerciseDTOs;
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

                //return exerciseDTOs.OrderBy(e => Guid.NewGuid()).Take(count).ToList();
                return exerciseDTOs;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error while fetching exercises for sub-muscle: {MuscleName}");
                throw;
            }
        }

        public async Task<List<ExerciseDTO>> GetExercisesForSubMuscleAsync(string subMuscleName, List<string> allowedEquipment)
        {
            try
            {
                // שליפת התרגילים שמתאימים לתת-השריר מה-DAL
                var exercises = await muscleDAL.GetExercisesForSubMuscleAsync(subMuscleName);

                // סינון לפי רשימת הציוד המותר
                //var filteredExercises = exercises.Where(e =>
                //    e.Equipment.Any(eq => allowedEquipment.Contains(eq.EquipmentName))
                //).ToList();

                // המרת התרגילים ל-DTO
                var exerciseDTOs = exercises.Select(e => new ExerciseDTO
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
        public Task SaveDefaultProgramAsync( int traineeId, string programName)
        {
            throw new NotImplementedException();
        }


        public async Task<List<List<ExerciseWithMuscleInfo>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
        {
            var exercisePlan = new List<List<ExerciseWithMuscleInfo>>(); // תוכנית האימונים לכל הימים
            var usedExercisesOverall = new HashSet<int>(); // רשימת מזהי תרגילים שכבר נבחרו בכל התוכנית
            var usedExercisesBySubMuscle = new Dictionary<string, int>(); // מיפוי של תרגילים שנבחרו עבור תתי-שרירים
            var typeMuscleList = new List<string>();

            // יצירת רשימת סוגי השרירים
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
                var dayExercises = new List<ExerciseWithMuscleInfo>();
                var usedExercisesForDay = new HashSet<int>(); // רשימת מזהי תרגילים שכבר נבחרו עבור היום הנוכחי

                foreach (var muscleEntry in dayList) // מעבר על כל שריר ברשימה
                {
                    string muscleName = muscleEntry.Key;
                    int exerciseCount = muscleEntry.Values;
                    string typeMuscle = muscleEntry.Name;
                    var category = await categoryDAL.GetCategoryByNameAsync(typeMuscle);
                    var categoryID = category.CategoryId;
                    // בדיקת תת-שרירים אם נדרש
                    if (trainingParams.subMuscleOfMuscleList.Contains(muscleName))
                    {
                        var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscleName);

                        foreach (var subMuscle in subMuscles)
                        {
                            if (exerciseCount <= 0) break;

                            // מציאת תרגילים שעובדים על תת-שריר זה
                            var exercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, trainingParams.equipment);

                            // סינון התרגילים כך שלא יחזרו על עצמם בתתי-שרירים אחרים
                            var filteredExercises = exercises
                                .Where(e => !usedExercisesForDay.Contains(e.ExerciseId) &&
                                            (!usedExercisesBySubMuscle.ContainsKey(subMuscle.SubMuscleName) ||
                                             usedExercisesBySubMuscle[subMuscle.SubMuscleName] == e.ExerciseId))
                                .OrderBy(e => Guid.NewGuid()) // רנדומליות בבחירת התרגילים
                                .ToList();

                            // בחירת תרגיל מתאים
                            //foreach (var exercise in filteredExercises)
                            //{
                            //    dayExercises.Add(new ExerciseWithMuscleInfo
                            //    {
                            //        Exercise = exercise,
                            //        MuscleName = muscleName,
                            //        SubMuscleName = subMuscle.SubMuscleName
                            //    });

                            //    usedExercisesForDay.Add(exercise.ExerciseId);
                            //    usedExercisesOverall.Add(exercise.ExerciseId);
                            //    usedExercisesBySubMuscle[subMuscle.SubMuscleName] = exercise.ExerciseId;
                            //    exerciseCount--;

                            //    if (exerciseCount <= 0) break;
                            //}
                            foreach (var exercise in filteredExercises)
                            {
                                dayExercises.Add(new ExerciseWithMuscleInfo
                                {
                                    Exercise = exercise,
                                    MuscleName = muscleName,
                                    SubMuscleName = subMuscle.SubMuscleName,
                                    JointCount = await exerciseDAL.GetJointCount(exercise.ExerciseId),
                                    categoryId = categoryID
                                });

                                usedExercisesForDay.Add(exercise.ExerciseId);
                                usedExercisesOverall.Add(exercise.ExerciseId);
                                usedExercisesBySubMuscle[subMuscle.SubMuscleName] = exercise.ExerciseId;
                                exerciseCount--;

                                break;
                            }

                        }
                    }

                    // אם עדיין חסרים תרגילים (לשריר הראשי), חפש תרגילים רגילים
                    if (exerciseCount > 0)
                    {
                        var allEquipmentExercises = await GetExercisesForMuscleAsync(muscleName, exerciseCount);
                        var filteredExercises = allEquipmentExercises
                            .Where(e => !usedExercisesForDay.Contains(e.ExerciseId))
                            .OrderBy(e => Guid.NewGuid()) // רנדומליות בבחירת התרגילים
                            .ToList();

                        foreach (var exercise in filteredExercises)
                        {
                            dayExercises.Add(new ExerciseWithMuscleInfo
                            {
                                Exercise = exercise,
                                MuscleName = muscleName,
                                SubMuscleName = null, // אין תת-שריר במקרה הזה
                                categoryId = categoryID,
                                JointCount = await exerciseDAL.GetJointCount(exercise.ExerciseId)
                            });

                            usedExercisesForDay.Add(exercise.ExerciseId);
                            usedExercisesOverall.Add(exercise.ExerciseId);
                            exerciseCount--;

                            if (exerciseCount <= 0) break;
                        }
                    }
                }

                exercisePlan.Add(dayExercises); // הוספת תרגילי היום לתוכנית
            }

            // מיון התרגילים לפי סדר חשיבות של השרירים ותתי-השרירים
            var musclePriorityOrder = trainingParams.musclePriorityOrder;
            var subMusclePriorityOrder = trainingParams.subMusclePriorityOrder;

            exercisePlan = await SortExercisesByPriorityAsync(exercisePlan, musclePriorityOrder, subMusclePriorityOrder);
            //var sortedExercises = SortExercisesByMuscleGroupAndJoints(exercisePlan);
            // הדפסת התוכנית ללוג
            for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
            {
                logger.LogInformation($"Day {dayIndex + 1} Exercises:");
                foreach (var exerciseInfo in exercisePlan[dayIndex])
                {
                    logger.LogInformation($"  - {exerciseInfo.Exercise.ExerciseName} (ID: {exerciseInfo.Exercise.ExerciseId}, Muscle: {exerciseInfo.MuscleName}, SubMuscle: {exerciseInfo.SubMuscleName})");
                }
            }

            return exercisePlan;
        }

        public async Task<List<List<ExerciseWithMuscleInfo>>> SortExercisesByPriorityAsync(
    List<List<ExerciseWithMuscleInfo>> exercisePlan,
    List<string> musclePriorityOrder,
    List<string> subMusclePriorityOrder)
        {
            // מעבר על כל יום בתוכנית האימונים
            for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
            {
                var dayExercises = exercisePlan[dayIndex];

                // יצירת קבוצות לפי muscleIndex
                var groupedExercises = dayExercises
                    .Select(detail => new
                    {
                        Detail = detail,
                        MuscleIndex = musclePriorityOrder.IndexOf(detail.MuscleName ?? string.Empty),
                        SubMuscleIndex = subMusclePriorityOrder.IndexOf(detail.SubMuscleName ?? string.Empty)
                    })
                    .GroupBy(item => item.MuscleIndex)
                    .OrderBy(group => group.Key == -1 ? int.MaxValue : group.Key); // סדר לפי muscleIndex

                var sortedExercises = new List<ExerciseWithMuscleInfo>();

                // מעבר על כל קבוצה ומיון לפי תתי-שרירים ואז לפי JointCount
                foreach (var group in groupedExercises)
                {
                    var sortedGroup = group
                        .OrderBy(item => item.MuscleIndex == -1 ? int.MaxValue : item.MuscleIndex) // סדר לפי subMuscleIndex
                                                                                                   // .OrderBy(item => item.SubMuscleIndex == -1 ? int.MaxValue : item.SubMuscleIndex) // סדר לפי subMuscleIndex
                        .ThenByDescending(item => item.Detail.JointCount) // סדר יורד לפי JointCount
                        .Select(item => item.Detail)
                        .ToList();

                    sortedExercises.AddRange(sortedGroup);
                }

                // עדכון רשימת התרגילים עבור היום
                exercisePlan[dayIndex] = sortedExercises;
            }

            return exercisePlan;
        }

        //public async Task SaveProgramDefaultAsync(TrainingParams trainingParams, List<List<ExerciseWithMuscleInfo>> listOfProgram, int traineeId, string programName, int daysInWeek, int goal, int level, int time)
        //{
        //    try
        //    {
        //        // יצירת תוכנית חדשה
        //        var trainingProgram = new TrainingProgram
        //        {
        //            TraineeId = traineeId,
        //            ProgramName = programName,
        //            CreationDate = DateTime.Now,
        //            TrainingDateTime = DateTime.Now,
        //            LastUpdateDate = DateTime.Now,
        //            IsDefaultProgram = true, // מציין שזו תוכנית דיפולטיבית
        //            ParentProgramId = null,
        //            IsHistoricalProgram = false,
        //        };


                // שמירת התוכנית ב-DAL וקבלת ProgramID
                //        int programId = await programExerciseDAL.SaveTrainingProgramAsync(trainingProgram);

        //        var defaultPrograms = new DefaultProgram
        //        {
        //            GoalId = goal,
        //            TrainingDays = daysInWeek,
        //            TrainingDurationId = time,
        //            FitnessLevelId = level,
        //            ProgramId = programId,
        //        };
        //        int defaultProgramID = await programExerciseDAL.SaveDefaultProgramsAsync(defaultPrograms);
        //        // הכנת רשימת התרגילים לשמירה
        //        var programExercises = new List<ProgramExercise>();
        //        int y = 0;
        //        foreach (var (dayIndex, dayExercises) in listOfProgram.Select((day, index) => (index, day)))
        //        {
        //            int i = 0;
        //            foreach (var exerciseEntry in dayExercises)
        //            {
        //                var exerciseId = exerciseEntry.Exercise.ExerciseId;
        //                var muscleId = await muscleDAL.GetIdOfMuscleByNameAsync(exerciseEntry.MuscleName);
        //                int? subMuscleId = null; // שימוש ב-Nullable<int> (int?) כדי לאפשר ערכי NULL
        //                if (!string.IsNullOrEmpty(exerciseEntry.SubMuscleName))
        //                {
        //                    subMuscleId = await subMuscleDAL.GetIdOfSubMuscleByNameAsync(exerciseEntry.SubMuscleName);
        //                }

        //                var programExercise = new ProgramExercise
        //                {
        //                    ProgramId = programId,
        //                    ExerciseId = exerciseId, // מזהה התרגיל
        //                    ProgramSets = 3, // מספר הסטים
        //                    ProgramRepetitionsMin = trainingParams.MinRep,
        //                    ProgramRepetitionsMax = trainingParams.MaxRep,
        //                    ProgramWeight = 10,
        //                    ExerciseOrder = i + 1, // סדר היום בתוכנית
        //                    DayOrder = y + 1,
        //                    CategoryId = exerciseEntry.categoryId,
        //                    TimesMin = 5,
        //                    TimesMax = 10,
        //                    MuscleId = muscleId,
        //                    SubMuscleId = subMuscleId,
        //                };

        //                programExercises.Add(programExercise);
        //                i++;
        //            }
        //            y++;
        //        }

        //        // שמירת התרגילים ב-DAL
        //        await programExerciseDAL.SaveProgramExercisesAsync(programExercises);

        //        logger.LogInformation("Default training program and exercises saved successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Error occurred while saving the default training program.");
        //        throw;
        //    }
        //}

        public async Task SaveProgramDefaultAsync(
            TrainingParams trainingParams,
            List<List<ExerciseWithMuscleInfo>> listOfProgram,
            int traineeId,
            string programName,
            int daysInWeek,
            int goal,
            int level,
            int time)
            {
                try
                {
                    // יצירת TrainingPlan חדש
                    var trainingPlan = new TrainingPlan
                    {
                        TraineeId = traineeId,
                        GoalId = goal,
                        TrainingDays = daysInWeek,
                        TrainingDurationId = time,
                        FitnessLevelId = level,
                        StartDate = DateTime.Now,
                        EndDate = DateTime.Now.AddMonths(3), // לפי הצורך
                        IsActive = true
                    };

                    // הוספת התוכנית למסד הנתונים (DAL)
                    var trainingPlanId = await trainingPlanDAL.AddTrainingPlanAsync(trainingPlan);

                    // מעבר על כל יום בתוכנית
                    for (int dayIdx = 0; dayIdx < listOfProgram.Count; dayIdx++)
                    {
                        var planDay = new PlanDay
                        {
                            TrainingPlanId = trainingPlanId,
                            ProgramName = programName,
                            DayOrder = dayIdx + 1,
                            CreationDate = DateTime.Now,
                            IsDefaultProgram = true,
                            ParentProgramId = null,
                            IsHistoricalProgram = false
                        };
                        var planDayId  = await planDayDAL.AddPlanDayAsync(planDay);
                        // מעבר על כל תרגיל ביום
                        for (int exIdx = 0; exIdx < listOfProgram[dayIdx].Count; exIdx++)
                        {
                            var exerciseInfo = listOfProgram[dayIdx][exIdx];
                            var exercisePlan = new ExercisePlan
                            {
                                PlanDayId = planDayId,
                                ExerciseId = exerciseInfo.Exercise.ExerciseId,
                                PlanSets = 3,
                                PlanRepetitionsMin = trainingParams.MinRep,
                                PlanRepetitionsMax = trainingParams.MaxRep,
                                PlanWeight = 10,
                                IndexOrder = exIdx + 1,
                                CategoryId = exerciseInfo.categoryId,
                                TimesMin = 5,
                                TimesMax = 10,
                                SubMuscleId = !string.IsNullOrEmpty(exerciseInfo.SubMuscleName)
                                    ? await subMuscleDAL.GetIdOfSubMuscleByNameAsync(exerciseInfo.SubMuscleName)
                                    : null,
                                TrainingDateTime = DateTime.Now
                            };
                            await exercisePlanDAL.AddExercisePlanAsync(exercisePlan);
                        }
                    }

                    logger.LogInformation("TrainingPlan, PlanDays, and ExercisePlans saved successfully!");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while saving the TrainingPlan.");
                    throw;
                }
            }

            //public Task UpdateProgramExerciseAsync(ProgramExerciseDTO programExercise, int id)
            //{
            //    throw new NotImplementedException();
            //}
        }
    }
