using AutoMapper;
using BLL;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;
using IBLL;
using IDAL;
using Microsoft.Extensions.Logging;
using static BLL.CreateTrainingPlan;
using DBEntities.Models;
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using Microsoft.Kiota.Abstractions;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using DocumentFormat.OpenXml.Spreadsheet;

namespace BLL
{


    // מחלקה ראשית ליצירת תוכניות אימונים מותאמות אישית
    public class CreateTrainingPlan
    {
        #region Fields - שדות

        // גישה לבסיס הנתונים
        private readonly IMuscleTypeDAL muscleTypeDAL;
        private readonly IMuscleDAL muscleDAL;
        private readonly IEquipmentDAL equipmentDAL;
        private readonly IExerciseDAL exerciseDAL;
        private readonly ITrainingDurationDAL trainingDurationDAL;
        private readonly ICategoryDAL categoryDAL;
        private readonly ISubMuscleDAL subMuscleDAL;
        private readonly IExercisePlanDAL exercisePlanDAL;
        private readonly ITrainingPlanDAL trainingPlanDAL;
        private readonly IPlanDayDAL planDayDAL;

        // כלים נוספים
        private readonly ILogger<TrainingPlanBLL> logger;
        private readonly IMapper mapper;
        private readonly List<string> exerciseList;
        private readonly TrainingConfig config;

        #endregion

        #region Constructor - בנאי
        // בנאי המחלקה - מאתחל את כל התלויות הנדרשות
        public CreateTrainingPlan(
            IMuscleDAL muscleDAL,
            ILogger<TrainingPlanBLL> logger,
            ISubMuscleDAL subMuscleDAL,
            ICategoryDAL categoryDAL,
            IMuscleTypeDAL muscleTypeDAL,
            ITrainingDurationDAL trainingDurationDAL,
            IEquipmentDAL equipmentDAL,
            IExerciseDAL exerciseDAL,
            IExercisePlanDAL exercisePlanDAL,
            ITrainingPlanDAL trainingPlanDAL,
            IPlanDayDAL planDayDAL)
        {
            // אתחול כל השדות
            this.muscleDAL = muscleDAL;
            this.equipmentDAL = equipmentDAL;
            this.muscleTypeDAL = muscleTypeDAL;
            this.exerciseDAL = exerciseDAL;
            this.trainingDurationDAL = trainingDurationDAL;
            this.categoryDAL = categoryDAL;
            this.subMuscleDAL = subMuscleDAL;
            this.trainingPlanDAL = trainingPlanDAL;
            this.exercisePlanDAL = exercisePlanDAL;
            this.planDayDAL = planDayDAL;
            this.logger = logger;
            this.exerciseList = new List<string>();
            // קריאת קובץ הקונפיגורציה
            this.config = TrainingConfig.Load("config.json");

            // הגדרת AutoMapper להמרת אובייקטים
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Muscle, MuscleDTO>().ReverseMap();
                cfg.CreateMap<TrainingPlan, TrainingPlanDTO>().ReverseMap();
                cfg.CreateMap<PlanDay, PlanDayDTO>().ReverseMap();
                cfg.CreateMap<ExercisePlanDTO, ExercisePlan>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);

        }

        #endregion
        
        #region מתודות עיקריות

        // מתודה ראשית ליצירת תוכנית אימונים מותאמת אישית
        public async Task addProgramExerciseAsync(int daysInWeek, int goal, int level, int time, int traineeID)
        {
            try
            {

                logger.LogInformation($"Starting training plan creation for trainee: {traineeID}");

                // שימוש בנתיב לקובץ האקסל מתוך הקונפיגורציה
                string filePath1 = config.ExcelFilePath;

                // טעינת רשימת הציוד הזמין
                await LoadExerciseListAsync();

                // אתחול פרמטרי האימון
                var trainingParams = InitializeTrainingParams();

                // שליפת מזהה זמן האימון
                var time1 = await trainingDurationDAL.GetTrainingDurationByIdAsync(time);

                // שליפת כל הפרמטרים מהקובץ
                trainingParams = await GetAllParams(filePath1, daysInWeek, goal, level, time1.TimeTrainingDuration);

                if (trainingParams == null)
                {
                    throw new Exception("Failed to retrieve training parameters.");
                }

                // ניקוי רשימות מערכים עם ערך 0
                CleanDayLists(trainingParams);

                // רישום לוג של הפרמטרים שנטענו
                LogTrainingParameters(trainingParams);

                // יצירת תוכנית האימון המותאמת
                var ListOfProgram = await GenerateOptimizedExercisePlanAsync(trainingParams);

                // שמירת התוכנית במערכת
                await SaveProgramDefaultAsync(
                    trainingParams,
                    ListOfProgram,
                    traineeID,
                    programName: "Default Training Program",
                    daysInWeek,
                    goal,
                    level,
                    time);

                logger.LogInformation($"Training plan created successfully for trainee: {traineeID}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error creating training plan for trainee: {traineeID}");
                throw;
            }
        }

        // שליפת כל הפרמטרים הנדרשים מקובץ Excel
        public async Task<TrainingParams> GetAllParams(string filePath, int daysInWeek, int goal, int level, int time)
        {
            try
            {
                using (var workbook = new XLWorkbook(filePath))
                {
                    logger.LogInformation("Loading training parameters from Excel file");

                    // שליפת מידע על התוכנית היומית
                    var dayListsNew = ExtractDayPlanning(workbook, daysInWeek, time);

                    // שליפת מידע על גדלי השרירים
                    var muscleSizeData = ExtractMuscleSizeData(workbook);

                    // שליפת מספר החזרות לפי מטרה
                    var (minRep, maxRep) = ExtractRepetitionData(workbook, goal);

                    // שליפת זמן אימון לקטגוריות
                    var timeCategoryList = ExtractTimeCategoryData(workbook, goal, time);

                    // שליפת כמות התרגילים לכל סוג שריר
                    var categoryMuscleSizeData = ExtractExerciseCountData(workbook, time, daysInWeek);

                    // שליפת סוגי השרירים
                    var typMuscleData = ExtractMuscleTypeData(workbook, daysInWeek);

                    // שליפת ציוד זמין לפי רמה
                    var equipmentData = ExtractEquipmentData(workbook, level);

                    // שליפת שרירים לתת-שריר
                    var muscleTypeData = ExtractSubMuscleRequirements(workbook);

                    // שליפת רשימת שרירים עם תת-שרירים
                    var subMuscleOfMuscleListData = await muscleDAL.GetMusclesOfSubMuscle();

                    // שליפת סדר התרגילים
                    var orderListData = ExtractOrderData(workbook);

                    // שליפת סדר עדיפויות השרירים
                    var (subMuscleData, muscleData) = ExtractPriorityOrderData(workbook);

                    // החזרת כל הנתונים במבנה מאוחד
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
                        musclePriorityOrder = muscleData,
                    };
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while processing training parameters from Excel file");
                throw;
            }
        }
        
        // יצירת תוכנית אימון
        public async Task<List<List<ExerciseWithMuscleInfo>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
        {
            logger.LogInformation("Generating optimized exercise plan...");
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

        //סידור תוכנית האימון
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

        #endregion

        #region מתודות שליפת תרגילים

        // שליפת תרגילים לשריר מסוים
        public async Task<List<ExerciseDTO>> GetExercisesForMuscleAsync(string muscleName, int count)
        {
            try
            {
                logger.LogInformation($"Fetching exercises for muscle: {muscleName}");

                // שליפת תרגילים מבסיס הנתונים
                var exercises = await muscleDAL.GetExercisesForMuscleAsync(muscleName);

                // המרת התרגילים ל-DTO
                var exerciseDTOs = exercises.Select(e => new ExerciseDTO
                {
                    ExerciseId = e.ExerciseId,
                    ExerciseName = e.ExerciseName
                }).ToList();

                logger.LogInformation($"Found {exerciseDTOs.Count} exercises for muscle: {muscleName}");

                // החזרת כל התרגילים - הבחירה הסופית תתבצע בשלב מאוחר יותר
                return exerciseDTOs;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error fetching exercises for muscle: {muscleName}");
                throw;
            }
        }

        // שליפת תרגילים לתת-שריר מסוים
        public async Task<List<ExerciseDTO>> GetExercisesForSubMuscleAsync(
            string subMuscleName,
            List<string> allowedEquipment)
        {
            try
            {
                logger.LogInformation($"Fetching exercises for sub-muscle: {subMuscleName}");

                // שליפת התרגילים המתאימים לתת-השריר
                var exercises = await muscleDAL.GetExercisesForSubMuscleAsync(subMuscleName);

                // המרת התרגילים ל-DTO
                var exerciseDTOs = exercises.Select(e => new ExerciseDTO
                {
                    ExerciseId = e.ExerciseId,
                    ExerciseName = e.ExerciseName
                }).ToList();

                logger.LogInformation($"Found {exerciseDTOs.Count} exercises for sub-muscle: {subMuscleName}");
                return exerciseDTOs;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error while fetching exercises for sub-muscle: {subMuscleName}");
                throw;
            }
        }

        #endregion

        #region שמירת הנתונים למסד נתונים 

        // שמירת התוכנית במערכת
        private async Task SaveProgramDefaultAsync(
            TrainingParams trainingParams,
            List<List<ExerciseWithMuscleInfo>> listOfProgram,
            int traineeID,
            string programName,
            int daysInWeek,
            int goal,
            int level,
            object time)
        {
            logger.LogInformation($"Starting to save training program: {programName} for trainee: {traineeID}");

            try
            {
                // שלב 1: יצירת רשומת תוכנית אימון ראשית
                var trainingPlan = await CreateMainTrainingPlan(traineeID, daysInWeek, goal, level, time);

                logger.LogInformation($"Created main training plan with ID: {trainingPlan.TrainingPlanId}");

                // שלב 2: שמירת ימי האימון
                var planDays = await SaveTrainingDays(trainingPlan.TrainingPlanId, listOfProgram, trainingParams);

                logger.LogInformation($"Saved {planDays.Count} training days");

                // שלב 3: שמירת התרגילים לכל יום
                await SaveExercisesForEachDay(planDays, listOfProgram, trainingParams);

                logger.LogInformation("All exercises saved successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error saving training program for trainee: {traineeID}");
                throw;
            }
        }

        // יצירת רשומת תוכנית אימון ראשית
        private async Task<TrainingPlanDTO> CreateMainTrainingPlan(
            int traineeID,
            int daysInWeek,
            int goal,
            int level,
            object time)
        {
            try
            {

                var trainingPlan = new TrainingPlanDTO
                {
                    TraineeId = traineeID,
                    TrainingDays = daysInWeek,
                    GoalId = goal,
                    FitnessLevelId = level,
                    TrainingDurationId = 1,
                    StartDate = DateTime.Now,
                    EndDate= DateTime.Now.AddMonths(3),
                    IsActive = true,
                };

                // שמירת התוכנית במסד הנתונים
                TrainingPlan trainingP = mapper.Map<TrainingPlanDTO, TrainingPlan>(trainingPlan);
                var id= await trainingPlanDAL.AddTrainingPlanAsync(trainingP);
                logger.LogDebug($"Main training plan created");

                TrainingPlan savedPlan = await trainingPlanDAL.GetTrainingPlanByIdAsync(id);
                return mapper.Map<TrainingPlanDTO>(savedPlan);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error creating main training plan");
                throw;
            }
        }

        // שמירת ימי האימון
        private async Task<List<PlanDayDTO>> SaveTrainingDays(
            int planId,
            List<List<ExerciseWithMuscleInfo>> listOfProgram,
            TrainingParams trainingParams)
        {
            var planDays = new List<PlanDayDTO>();

            try
            {
                for (int dayIndex = 0; dayIndex < listOfProgram.Count; dayIndex++)
                {
                    var dayExercises = listOfProgram[dayIndex];

                    // יצירת תיאור היום בהתבסס על השרירים המאומנים
                    string dayDescription = CreateDayDescription(dayExercises, dayIndex + 1);

                    var planDay = new PlanDayDTO
                    {
                        TrainingPlanId = planId,
                        DayOrder = dayIndex + 1,
                        ProgramName = dayDescription,
                        CreationDate = DateTime.Now,
                        IsDefaultProgram = true,
                        ParentProgramId = null,
                        IsHistoricalProgram = false
                    };
                    PlanDay planD = mapper.Map<PlanDayDTO, PlanDay>(planDay);
                    var savedDay = await planDayDAL.AddPlanDayAsync(planD);
                    var planById = await planDayDAL.GetPlanDayByIdAsync(savedDay);

                    planDays.Add(mapper.Map<PlanDayDTO>(planById));
                    logger.LogDebug($"Saved training day {dayIndex + 1}");
                }
                return planDays;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving training days");
                throw;
            }
        }

        // שמירת התרגילים לכל יום
        private async Task SaveExercisesForEachDay(
            List<PlanDayDTO> planDays,
            List<List<ExerciseWithMuscleInfo>> listOfProgram,
            TrainingParams trainingParams)
        {
            try
            {
                for (int dayIndex = 0; dayIndex < planDays.Count; dayIndex++)
                {
                    var planDay = planDays[dayIndex];
                    var dayExercises = listOfProgram[dayIndex];

                    for (int exerciseIndex = 0; exerciseIndex < dayExercises.Count; exerciseIndex++)
                    {
                        var exerciseInfo = dayExercises[exerciseIndex];
                        var subId = 0;
                        if (exerciseInfo.SubMuscleName!=null)
                        {
                           subId = await subMuscleDAL.GetIdOfSubMuscleByNameAsync(exerciseInfo.SubMuscleName);
                        }

                        // יצירת רשומת תרגיל בתוכנית
                        var exercisePlan = new ExercisePlanDTO
                        {
                            PlanDayId = planDay.PlanDayId,
                            ExerciseId = exerciseInfo.Exercise.ExerciseId,
                            IndexOrder = exerciseIndex + 1,
                            PlanSets =config.set,
                            PlanRepetitionsMin = trainingParams.MinRep,
                            PlanRepetitionsMax = trainingParams.MaxRep,
                            CategoryId = exerciseInfo.categoryId,
                            TimesMin= config.TimesMin,
                            TimesMax =config.TimesMax,
                            PlanWeight =config.Weight,
                            SubMuscleId = subId == 0 ? (int?)null : subId,
                            TrainingDateTime=DateTime.Now,
                        };
                        ExercisePlan exerciseP = mapper.Map<ExercisePlanDTO, ExercisePlan>(exercisePlan);
                        await exercisePlanDAL.AddExercisePlanAsync(exerciseP);

                        logger.LogDebug($"Main ExercisePlan created");

                    }

                    logger.LogDebug($"Saved {dayExercises.Count} exercises for day {dayIndex + 1}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error saving exercises for days");
                throw;
            }
        }


        #endregion

        #region Excel Data Extraction Methods - מתודות שליפת נתונים מ-Excel

        // שליפת תכנון יומי מגיליון Excel
        private List<List<DayEntry>> ExtractDayPlanning(XLWorkbook workbook, int daysInWeek, int time)
        {
            var daysWorksheet = GetWorksheet(workbook, config.DaysInWeekSheet);
            return ExtractDayListsNew(daysWorksheet, daysInWeek, time);
        }

        // שליפת מידע על גדלי השרירים
        private Dictionary<string, List<object>> ExtractMuscleSizeData(XLWorkbook workbook)
        {
            var muscleSizeWorksheet = GetWorksheet(workbook, config.MuscleSheet);
            return ExtractMuscleSizeData(muscleSizeWorksheet);
        }

        // שליפת מידע על מספר החזרות
        private (int minRep, int maxRep) ExtractRepetitionData(XLWorkbook workbook, int goal)
        {
            var repWorksheet = GetWorksheet(workbook, config.RepitByGoalSheet);
            int colRep = FindColumnByValue(repWorksheet, 1, goal, "Column with the specified goal not found.");
            int minRep = GetValueFromWorksheet(repWorksheet, "min", colRep, "Min value not found.");
            int maxRep = GetValueFromWorksheet(repWorksheet, "max", colRep, "Max value not found.");
            return (minRep, maxRep);
        }

        // שליפת מידע על זמן לקטגוריות
        private Dictionary<string, int> ExtractTimeCategoryData(XLWorkbook workbook, int goal, int time)
        {
            var timeCategoryWorksheet = GetWorksheet(workbook, config.CategorySheet);
            return ExtractTimeCategoryList(timeCategoryWorksheet, goal, time);
        }

        // שליפת מידע על כמות התרגילים
        private Dictionary<(string Category, string MuscleSize), int> ExtractExerciseCountData(
            XLWorkbook workbook, int time, int daysInWeek)
        {
            var countToMuscleWorksheet = GetWorksheet(workbook, config.SumOfLargeByTimeSheet);
            return ExtractCategoryMuscleSizeData(countToMuscleWorksheet, time, daysInWeek);
        }

        // שליפת מידע על סוגי השרירים
        private List<Dictionary<int, List<string>>> ExtractMuscleTypeData(XLWorkbook workbook, int daysInWeek)
        {
            var typMuscleWorksheet = GetWorksheet(workbook, config.TypMuscleSheet);
            return ExtractTypMuscleData(typMuscleWorksheet, daysInWeek);
        }

        // שליפת מידע על ציוד זמין
        private List<string> ExtractEquipmentData(XLWorkbook workbook, int level)
        {
            var equipmentWorksheet = GetWorksheet(workbook, config.EquipmentSheet);
            return ExtractEquipmentData(equipmentWorksheet, level);
        }

        // שליפת דרישות תת-שרירים
        private List<string> ExtractSubMuscleRequirements(XLWorkbook workbook)
        {
            var muscleTypeWorksheet = GetWorksheet(workbook, config.NeedSubMuscleSheet);
            return ExtractMuscleTypeData(muscleTypeWorksheet);
        }

        // שליפת מידע על סדר התרגילים
        private Dictionary<int, string> ExtractOrderData(XLWorkbook workbook)
        {
            var orderListSheet = GetWorksheet(workbook, config.OrderListSheet);
            return ExtractOrderListData(orderListSheet);
        }

        // שליפת סדר עדיפויות השרירים
        private (List<string> subMuscleData, List<string> muscleData) ExtractPriorityOrderData(XLWorkbook workbook)
        {
            var orderListSheet = GetWorksheet(workbook, config.OrderListSheet);
            var subMuscleData = ExtractSubMuscleOrderListData(orderListSheet);
            var muscleData = ExtractMuscleOrderListData(orderListSheet);
            return (subMuscleData, muscleData);
        }

        // שליפת מידע על גדלי השרירים מגיליון
        private Dictionary<string, List<object>> ExtractMuscleSizeData(IXLWorksheet worksheet)
        {
            var muscleSizeData = new Dictionary<string, List<object>>();

            // מעבר על כל העמודות בשורה הראשונה
            foreach (var col in worksheet.ColumnsUsed())
            {
                string muscleSize = col.FirstCell().GetValue<string>(); // גודל השריר
                var muscles = col.CellsUsed().Skip(1).Select(c => (object)c.Value).ToList(); // רשימת השרירים
                muscleSizeData[muscleSize] = muscles;
            }

            return muscleSizeData;
        }

        // שליפת רשימת סוגי השרירים הזקוקים לתת-שריר
        private List<string> ExtractMuscleTypeData(IXLWorksheet worksheet)
        {
            var muscleTypeListData = new List<string>();

            // מעבר על כל השורות וקריאת הערך מהעמודה הראשונה
            foreach (var row in worksheet.RowsUsed())
            {
                muscleTypeListData.Add(row.Cell(1).Value.ToString());
            }

            return muscleTypeListData;
        }

        // שליפת מידע על סדר התרגילים
        private Dictionary<int, string> ExtractOrderListData(IXLWorksheet worksheet)
        {
            var orderListData = new Dictionary<int, string>();

            // מעבר על כל השורות וקריאת המפתח והערך
            foreach (var row in worksheet.RowsUsed())
            {
                int key = row.Cell(1).GetValue<int>(); // מספר סדר
                string value = row.Cell(2).Value.ToString(); // תיאור
                orderListData.Add(key, value);
            }

            return orderListData;
        }

        // שליפת סדר עדיפות השרירים הראשיים
        private List<string> ExtractMuscleOrderListData(IXLWorksheet worksheet)
        {
            var muscleOrderListData = new List<string>();

            // מעבר על כל השורות (פרט לראשונה) וקריאת השרירים הראשיים
            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                muscleOrderListData.Add(row.Cell(2).Value.ToString());
            }

            return muscleOrderListData;
        }

        // שליפת סדר עדיפות תת-השרירים
        private List<string> ExtractSubMuscleOrderListData(IXLWorksheet worksheet)
        {
            var subMuscleOrderListData = new List<string>();

            // מעבר על כל השורות (פרט לראשונה) וקריאת תת-השרירים
            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                subMuscleOrderListData.Add(row.Cell(3).Value.ToString());
            }

            return subMuscleOrderListData;
        }

        // קבלת גיליון עבודה לפי שם עם בדיקת קיום
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

        // חיפוש עמודה לפי ערך בשורה מסוימת
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

        // קבלת ערך מגיליון לפי טקסט בשורה ומספר עמודה
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

        // שליפת רשימת ציוד לפי רמת המתאמן
        private List<string> ExtractEquipmentData(IXLWorksheet worksheet, int level)
        {
            var listEquipment = new List<string>();

            // מעבר על כל השורות (פרט לראשונה) וחיפוש רמה מתאימה
            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                if (row.Cell(1).GetValue<int>() == level)
                {
                    var equipmentName = row.Cell(2).GetValue<string>();
                    listEquipment.Add(equipmentName);
                }
            }

            logger.LogInformation($"Found {listEquipment.Count} equipment items for level {level}");
            return listEquipment;
        }

        // שליפת מידע על סוגי השרירים לפי מספר ימי האימון
        private List<Dictionary<int, List<string>>> ExtractTypMuscleData(IXLWorksheet worksheet, int daysInWeek)
        {
            var orderListOfTypeMuscle = new List<Dictionary<int, List<string>>>();

            // מעבר על כל השורות (פרט לראשונה)
            foreach (var row in worksheet.RowsUsed().Skip(1))
            {
                // בדיקה אם השורה מתאימה למספר ימי האימון
                if (row.Cell(1).GetValue<int>() == daysInWeek)
                {
                    var dict = new Dictionary<int, List<string>>();
                    int priority = row.Cell(2).GetValue<int>(); // רמת עדיפות

                    // מעבר על כל העמודות החל מהשלישית
                    for (int col = 3; col <= row.LastCellUsed().Address.ColumnNumber; col++)
                    {
                        string muscleType = row.Cell(col).GetValue<string>();

                        if (dict.ContainsKey(priority))
                        {
                            dict[priority].Add(muscleType);
                        }
                        else
                        {
                            dict.Add(priority, new List<string> { muscleType });
                        }
                    }
                    orderListOfTypeMuscle.Add(dict);
                }
            }

            return orderListOfTypeMuscle;
        }

        // שליפת מידע על כמות התרגילים לפי קטגוריה וגודל שריר
        private Dictionary<(string Category, string MuscleSize), int> ExtractCategoryMuscleSizeData(
            IXLWorksheet worksheet, int time, int daysInWeek)
        {
            var categoryMuscleSizeData = new Dictionary<(string Category, string MuscleSize), int>();

            // סינון שורות המתאימות לזמן ומספר ימים
            var matchingRows = worksheet.RowsUsed().Skip(1) // דילוג על שורת הכותרות
                .Where(row => row.Cell(1).GetValue<int>() == time && row.Cell(4).GetValue<int>() == daysInWeek)
                .ToList();

            if (!matchingRows.Any())
            {
                logger.LogWarning($"No matching rows found for time: {time}, daysInWeek: {daysInWeek}");
                throw new Exception($"No rows found with time {time} and daysInWeek {daysInWeek} in the sheet.");
            }

            // מעבר על השורות המתאימות ויצירת המילון
            foreach (var row in matchingRows)
            {
                string category = row.Cell(2).GetValue<string>();      // קטגוריה
                string muscleSize = row.Cell(3).GetValue<string>();    // גודל שריר
                int quantity = row.Cell(5).GetValue<int>();            // כמות

                var key = (Category: category, MuscleSize: muscleSize);

                if (!categoryMuscleSizeData.ContainsKey(key))
                {
                    categoryMuscleSizeData[key] = quantity;
                    logger.LogDebug($"Added: {category} - {muscleSize} = {quantity}");
                }
                else
                {
                    logger.LogWarning($"Duplicate entry for category '{category}' and muscle size '{muscleSize}'");
                }
            }

            return categoryMuscleSizeData;
        }

        // שליפת רשימת קטגוריות זמן לפי מטרה וזמן אימון
        private Dictionary<string, int> ExtractTimeCategoryList(IXLWorksheet worksheet, int goal, int time)
        {
            var timeCategoryList = new Dictionary<string, int>();

            // סינון שורות המתאימות לזמן ומטרה
            var matchingRows = worksheet.RowsUsed()
                .Where(row => row.Cell(1).GetValue<int>() == time && row.Cell(2).GetValue<int>() == goal)
                .ToList();

            if (!matchingRows.Any())
            {
                logger.LogWarning($"No matching rows found for time: {time}, goal: {goal}");
                throw new Exception($"No rows found with time {time} and goal {goal} in the Time sheet.");
            }

            // מעבר על השורות המתאימות ויצירת קטגוריות
            foreach (var row in matchingRows)
            {
                // יצירת מפתח קטגוריה משילוב העמודות
                string category = $"{row.Cell(3).GetValue<string>()}, {row.Cell(4).GetValue<string>()}";

                if (!timeCategoryList.ContainsKey(category))
                {
                    timeCategoryList[category] = 1; // ערך ברירת מחדל
                    logger.LogDebug($"Added time category: {category}");
                }
                else
                {
                    logger.LogWarning($"Duplicate time category: {category}");
                }
            }

            return timeCategoryList;
        }

        /// שליפת רשימת ימים חדשה מהגיליון*****************
        private List<List<DayEntry>> ExtractDayListsNew(IXLWorksheet worksheet, int daysInWeek, int time)
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
                    if (duration == time)
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
        // יצירת תיאור היום בהתבסס על השרירים ❤
        private string CreateDayDescription(List<ExerciseWithMuscleInfo> dayExercises, int dayNumber)
        {
            //var muscles = dayExercises.Select(ex => ex.MuscleName).Distinct().ToList();
            //string musclesList = string.Join(", ", muscles);
            return $"יום {dayNumber} ";
        }

        #endregion

        #region  מתודות עזר פרטיות

        // טעינת רשימת הציוד הזמין במכון
        private async Task LoadExerciseListAsync()
        {
            try
            {
                var exercise = await exerciseDAL.GetAllExercisesAsync();
                foreach (var e in exercise)
                {
                    this.exerciseList.Add(e.ExerciseName);
                }
                logger.LogInformation($"Loaded {exerciseList.Count} exercise items");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error loading exercise list");
                throw;
            }
        }

        // אתחול פרמטרי האימון
        private TrainingParams InitializeTrainingParams()
        {
            return new TrainingParams
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
        }

        // ניקוי רשימות יומיות מערכים עם ערך 0
        private void CleanDayLists(TrainingParams trainingParams)
        {
            trainingParams.DayLists = trainingParams.DayLists
                .Select(list => list.Where(de => de.Values != 0).ToList())
                .ToList();
        }

        // רישום פרמטרי האימון בלוג
        private void LogTrainingParameters(TrainingParams trainingParams)
        {
            // רישום רשימות יומיות
            for (int i = 0; i < trainingParams.DayLists.Count; i++)
            {
                logger.LogInformation($"Day List {i + 1}:");
                foreach (var entry in trainingParams.DayLists[i])
                {
                    logger.LogInformation($"Key: {entry.Key}, Values: {entry.Values}");
                }
            }

            // רישום מספר החזרות
            logger.LogInformation($"Repetitions: {trainingParams.MinRep}-{trainingParams.MaxRep}");

            // רישום קטגוריות זמן
            logger.LogInformation("Time Categories: " +
                string.Join(", ", trainingParams.TimeCategories.Select(kvp =>
                    $"[{kvp.Key.Category} - {kvp.Key.MuscleSize}: {kvp.Value}]")));

            // רישום סוגי שרירים
            foreach (var dict in trainingParams.TypeMuscleData)
            {
                foreach (var pair in dict)
                {
                    logger.LogInformation($"Muscle Type: {pair.Key} - Values: {string.Join(", ", pair.Value)}");
                }
            }

            // רישום רשימות נוספות
            logger.LogInformation("List of Need SubMuscle: " + string.Join(", ", trainingParams.NeedSubMuscleList));
            logger.LogInformation("List of equipment: " + string.Join(", ", trainingParams.equipment));

            foreach (var muscle in trainingParams.subMuscleOfMuscleList)
            {
                logger.LogInformation($"List of subMuscleOfMuscleList: {muscle}");
            }
        }

        #endregion


    }
}



















///// קישור התוכנית לפרופיל המתאמן
//private async Task LinkProgramToTrainee(int traineeID, int planId)
//{
//    try
//    {
//        // עדכון הסטטוס של תוכניות קודמות ללא פעילות
//        await trainingPlanDAL.DeactivateOldPlansAsync(traineeID);

//        // הפעלת התוכנית החדשה
//        await trainingPlanDAL.SetActivePlanAsync(traineeID, planId);

//        logger.LogDebug($"Linked plan {planId} to trainee {traineeID}");
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, $"Error linking program to trainee {traineeID}");
//        throw;
//    }
//}

///// שמירת מטא-דאטה נוספת
//private async Task SaveAdditionalMetadata(int planId, TrainingParams trainingParams)
//{
//    try
//    {
//        // שמירת הפרמטרים שבהם נעשה שימוש ליצירת התוכנית
//        var metadata = new PlanMetadataEntity
//        {
//            PlanId = planId,
//            EquipmentUsed = string.Join(",", trainingParams.equipment),
//            MuscleDistribution = CalculateMuscleDistribution(trainingParams),
//            TrainingPhilosophy = DetermineTrainingPhilosophy(trainingParams),
//            CreatedBy = "System",
//            CreationParameters = SerializeTrainingParams(trainingParams)
//        };

//        await trainingPlanDAL.SaveMetadataAsync(metadata);

//        logger.LogDebug("Additional metadata saved successfully");
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "Error saving additional metadata");
//        throw;
//    }
//}


#region Helper Methods for Save Operations

///// יצירת תיאור היום בהתבסס על השרירים
//private string CreateDayDescription(List<ExerciseWithMuscleInfo> dayExercises, int dayNumber)
//{
//    var muscles = dayExercises.Select(ex => ex.MuscleName).Distinct().ToList();
//    string musclesList = string.Join(", ", muscles);
//    return $"יום {dayNumber} - {musclesList}";
//}

///// חישוב זמן משוער ליום אימון
////private int CalculateEstimatedDuration(List<ExerciseWithMuscleInfo> dayExercises)
////{
////    // הערכה של 4 דקות לתרגיל (כולל מנוחה)
////    return dayExercises.Count * 4;
////}

///// קבלת קבוצות השרירים ליום
////private string GetMuscleGroupsForDay(List<ExerciseWithMuscleInfo> dayExercises)
////{
////    var muscleGroups = dayExercises.Select(ex => ex.MuscleName).Distinct().ToList();
////    return string.Join(",", muscleGroups);
////}

///// חישוב מספר הסטים
//private int CalculateSets(ExerciseWithMuscleInfo exerciseInfo, TrainingParams trainingParams)
//{
//    // לוגיקה לחישוב מספר סטים בהתבסס על סוג השריר ורמת הקושי
//    if (exerciseInfo.JointCount > 2)
//        return 4; // תרגילים מורכבים
//    else if (exerciseInfo.JointCount == 2)
//        return 3; // תרגילים בינוניים
//    else
//        return 3; // תרגילים פשוטים
//}

///// חישוב זמן מנוחה
//private int CalculateRestTime(ExerciseWithMuscleInfo exerciseInfo)
//{
//    // זמן מנוחה בשניות
//    return exerciseInfo.JointCount > 2 ? 120 : 90;
//}

///// יצירת הערות לתרגיל
//private string GenerateExerciseNotes(ExerciseWithMuscleInfo exerciseInfo)
//{
//    var notes = new List<string>();

//    if (!string.IsNullOrEmpty(exerciseInfo.SubMuscleName))
//    {
//        notes.Add($"מתמקד ב-{exerciseInfo.SubMuscleName}");
//    }

//    if (exerciseInfo.JointCount > 2)
//    {
//        notes.Add("תרגיל מורכב - שמור על טכניקה נכונה");
//    }

//    return string.Join(". ", notes);
//}

///// חישוב התפלגות השרירים
//private string CalculateMuscleDistribution(TrainingParams trainingParams)
//{
//    var distribution = new Dictionary<string, int>();

//    foreach (var dayList in trainingParams.DayLists)
//    {
//        foreach (var entry in dayList)
//        {
//            if (distribution.ContainsKey(entry.Name))
//                distribution[entry.Name] += entry.Values;
//            else
//                distribution[entry.Name] = entry.Values;
//        }
//    }

//    return string.Join(",", distribution.Select(kvp => $"{kvp.Key}:{kvp.Value}"));
//}

///// קביעת פילוסופיית האימון
//private string DetermineTrainingPhilosophy(TrainingParams trainingParams)
//{
//    if (trainingParams.MaxRep <= 6)
//        return "כוח - מיקוד בעומסים כבדים";
//    else if (trainingParams.MaxRep <= 12)
//        return "היפרטרופיה - בניית מסת שריר";
//    else
//        return "סיבולת - מיקוד בנפח גבוה";
//}

///// סיריאליזציה של פרמטרי האימון
//private string SerializeTrainingParams(TrainingParams trainingParams)
//{
//    try
//    {
//        // יצירת אובייקט פשוט לשמירה
//        var simplifiedParams = new
//        {
//            MinRep = trainingParams.MinRep,
//            MaxRep = trainingParams.MaxRep,
//            Equipment = trainingParams.equipment,
//            DayCount = trainingParams.DayLists.Count,
//            MuscleTypes = trainingParams.TypMuscle
//        };

//        return System.Text.Json.JsonSerializer.Serialize(simplifiedParams);
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "Error serializing training parameters");
//        return "{}";
//    }
//}

#endregion

#region Additional Entity Classes - מחלקות ישות נוספות

/// <summary>
/// ישות תוכנית אימון
/// </summary>
//public class TrainingPlanEntity
//{
//    public int PlanId { get; set; }
//    public int TraineeId { get; set; }
//    public string PlanName { get; set; }
//    public int DaysPerWeek { get; set; }
//    public int Goal { get; set; }
//    public int Level { get; set; }
//    public int Duration { get; set; }
//    public DateTime CreatedDate { get; set; }
//    public bool IsActive { get; set; }
//    public string Description { get; set; }
//}

/// <summary>
/// ישות יום אימון
/// </summary>
//public class PlanDayEntity
//{
//    public int DayId { get; set; }
//    public int PlanId { get; set; }
//    public int DayNumber { get; set; }
//    public string DayName { get; set; }
//    public string Description { get; set; }
//    public int EstimatedDuration { get; set; }
//    public string MuscleGroups { get; set; }
//    public bool IsRestDay { get; set; }
//}

/// <summary>
/// ישות תרגיל בתוכנית
/// </summary>
//public class ExercisePlanEntity
//{
//    public int ExercisePlanId { get; set; }
//    public int PlanDayId { get; set; }
//    public int ExerciseId { get; set; }
//    public int OrderInDay { get; set; }
//    public int Sets { get; set; }
//    public int MinReps { get; set; }
//    public int MaxReps { get; set; }
//    public int RestTime { get; set; }
//    public decimal Weight { get; set; }
//    public string Notes { get; set; }
//    public string TargetMuscle { get; set; }
//    public string SubMuscle { get; set; }
//}

/// <summary>
/// ישות מטא-דאטה של תוכנית
/// </summary>
//public class PlanMetadataEntity
//{
//    public int MetadataId { get; set; }
//    public int PlanId { get; set; }
//    public string EquipmentUsed { get; set; }
//    public string MuscleDistribution { get; set; }
//    public string TrainingPhilosophy { get; set; }
//    public string CreatedBy { get; set; }
//    public string CreationParameters { get; set; }
//    public DateTime CreatedAt { get; set; } = DateTime.Now;
//}

#endregion

/// שליפת רשימת ימים חדשה מהגיליון
//private List<List<DayEntry>> ExtractDayListsNew(IXLWorksheet worksheet, int daysInWeek, int time)
//{
//    var dayLists = new List<List<DayEntry>>();

//    logger.LogInformation($"Extracting day lists for {daysInWeek} days, {time} minutes");

//    try
//    {
//        // מעבר על כל השורות בגיליון
//        foreach (var row in worksheet.RowsUsed().Skip(1)) // דילוג על שורת הכותרות
//        {
//            // בדיקה אם השורה מתאימה למספר ימים ולזמן
//            int rowDays = row.Cell(1).GetValue<int>();
//            int rowTime = row.Cell(2).GetValue<int>();

//            if (rowDays == daysInWeek && rowTime == time)
//            {
//                // יצירת רשימה חדשה ליום אימון
//                var dayList = new List<DayEntry>();

//                // מעבר על כל העמודות בשורה החל מהעמודה השלישית
//                for (int col = 3; col <= row.LastCellUsed().Address.ColumnNumber; col += 3)
//                {
//                    // בדיקה שיש מספיק עמודות
//                    if (col + 2 <= row.LastCellUsed().Address.ColumnNumber)
//                    {
//                        var key = row.Cell(col).GetValue<string>();      // קטגוריה
//                        var values = row.Cell(col + 1).GetValue<int>();  // מספר תרגילים
//                        var name = row.Cell(col + 2).GetValue<string>(); // שם השריר

//                        // הוספת הערך רק אם הוא תקין
//                        if (!string.IsNullOrEmpty(key) && values > 0 && !string.IsNullOrEmpty(name))
//                        {
//                            dayList.Add(new DayEntry
//                            {
//                                Key = key,
//                                Values = values,
//                                Name = name
//                            });
//                        }
//                    }
//                }

//                // הוספת הרשימה היומית אם היא לא ריקה
//                if (dayList.Any())
//                {
//                    dayLists.Add(dayList);
//                    logger.LogDebug($"Added day list with {dayList.Count} entries");
//                }
//            }
//        }

//        logger.LogInformation($"Extracted {dayLists.Count} day lists successfully");
//        return dayLists;
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "Error extracting day lists from worksheet");
//    }
//    throw;
//}

///// <summary>
///// יצירת תוכנית אימונים מותאמת ומותאמת אישית
///// </summary>
///// <param name="trainingParams">פרמטרי האימון</param>
///// <returns>רשימת תוכניות אימון</returns>**********************
////private async Task<object> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
////{
////    // TODO: מימוש הלוגיקה ליצירת תוכנית האימונים
////    logger.LogInformation("Generating optimized exercise plan...");

////    // כאן יש לממש את האלגוריתם ליצירת התוכנית:
////    // 1. בחירת שרירים לפי סדר עדיפות
////    // 2. התאמת תרגילים לציוד הזמין
////    // 3. חלוקה לימי אימון
////    // 4. אופטימיזציה לפי זמן האימון

////    return new object(); // החזרה זמנית
////}

///// <summary>
///// שמירת התוכנית במערכת
///// </summary>**************
//private async Task SaveProgramDefaultAsync(
//    TrainingParams trainingParams,
//    object listOfProgram,
//    int traineeID,
//    string programName,
//    int daysInWeek,
//    int goal,
//    int level,
//    object time)
//{
//    // TODO: מימוש שמירת התוכנית בבסיס הנתונים
//    logger.LogInformation($"Saving training program: {programName} for trainee: {traineeID}");

//    // כאן יש לממש:
//    // 1. יצירת רשומת תוכנית אימון
//    // 2. שמירת ימי האימון
//    // 3. שמירת התרגילים לכל יום
//    // 4. קישור לפרופיל המתאמן
//}

#region DTOs and Interfaces - אובייקטי העברת נתונים וממשקים

// אובייקט העברת נתונים לתרגיל
//public class ExerciseDTO
//{
//    public int ExerciseId { get; set; }
//    public string ExerciseName { get; set; }
//}

/// אובייקט שריר למיפוי
//public class Muscle
//{
//    public int MuscleId { get; set; }
//    public string MuscleName { get; set; }
//}

/// אובייקט העברת נתונים לשריר
//public class MuscleDTO
//{
//    public int MuscleId { get; set; }
//    public string MuscleName { get; set; }
//}

// ממשקים לגישה לבסיס הנתונים (DAL)
//public interface IMuscleDAL
//{
//    Task<List<object>> GetExercisesForMuscleAsync(string muscleName);
//    Task<List<object>> GetExercisesForMuscleAndTypeAsync(string muscleName, string typeMuscle, List<int> allowedEquipment);
//    Task<List<object>> GetExercisesForSubMuscleAsync(string subMuscleName);
//    Task<List<string>> GetMusclesOfSubMuscle();
//}

//public interface IMuscleTypeDAL { }
//public interface IEquipmentDAL
//{
//    Task<List<dynamic>> GetAllEquipmentsAsync();
//}
//public interface IExerciseDAL { }
//public interface ITrainingDurationDAL
//{
//    Task<object> GetTrainingDurationIDByValue(int time);
//}
//public interface ICategoryDAL { }
//public interface ISubMuscleDAL { }
//public interface IExercisePlanDAL { }
//public interface ITrainingPlanDAL { }
//public interface IPlanDayDAL { }

// ממשק ללוגים
//public interface ILogger<T>
//{
//    void LogInformation(string message);
//    void LogWarning(string message);
//    void LogError(Exception ex, string message);
//    void LogDebug(string message);
//}

//// ממשק למיפוי אובייקטים
//public interface IMapper
//{
//    TDestination Map<TDestination>(object source);
//}

#endregion
