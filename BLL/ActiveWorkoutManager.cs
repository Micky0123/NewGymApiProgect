using AutoMapper;
using DAL;
using DBEntities.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using DTO;
using IBLL;
using IDAL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{

    public class ActiveWorkoutManager //: IActiveWorkoutManager
    //{
    //    private BacktrackingScheduler scheduler;
    //    public bool IsInitialized => scheduler != null;

    //    private Dictionary<int, TraineeExerciseStatus> activeTrainees;
    //    private readonly IPlanDayDAL planDayDAL;
    //    private readonly IExercisePlanDAL exercisePlanDAL;
    //    private readonly IMapper mapper;
    //    private readonly SemaphoreSlim _startWorkoutLock = new SemaphoreSlim(1, 1);

    //    private readonly ITraineeBLL _traineeBLL;
    //    private readonly IPlanDayDAL _planDayDAL;
    //    private readonly IExercisePlanDAL _exercisePlanDAL;
    //    private readonly IMemoryCache _cache;


    //    public ActiveWorkoutManager(
    //        IMemoryCache cache,
    //        ITraineeBLL traineeBLL,
    //        IPlanDayDAL planDayDAL,
    //        IExercisePlanDAL exercisePlanDAL)
    //    {
    //        _cache = cache;
    //        this._traineeBLL = traineeBLL;
    //        this.planDayDAL = planDayDAL;
    //        this.exercisePlanDAL = exercisePlanDAL;

    //        activeTrainees = new Dictionary<int, TraineeExerciseStatus>();

    //        var configTaskConverter = new MapperConfiguration(cfg =>
    //        {
    //            cfg.CreateMap<PlanDay, PlanDayDTO>().ReverseMap();
    //            cfg.CreateMap<ExercisePlan, ExercisePlanDTO>().ReverseMap();
    //        });
    //        mapper = new Mapper(configTaskConverter);
    //    }


    //    public void Initialize(
    //        List<ExerciseDTO> exerciseList,
    //        List<GraphEdgeDTO> exerciseEdges,
    //        List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
    //        List<MuscleEdgeDTO> muscleEdges,
    //        Dictionary<int, int> equipmentCountByExercise,
    //        DateTime firstSlotStart,
    //        int slotMinutes,
    //        int slotCount)
    //    {
    //        if (_cache.TryGetValue("Scheduler", out BacktrackingScheduler existing) && existing != null)
    //            throw new Exception("Scheduler already initialized!");

    //        var scheduler = new BacktrackingScheduler(_traineeBLL);
    //        scheduler.Initialize(
    //            exerciseList, exerciseEdges, exerciseToMuscleEdges, muscleEdges,
    //            equipmentCountByExercise, firstSlotStart, slotMinutes, slotCount
    //        );
    //        _cache.Set("Scheduler", scheduler);
    //    }

    //    // שמירה בזיכרון cache
    //    private BacktrackingScheduler GetScheduler()
    //    {
    //        if (!_cache.TryGetValue("Scheduler", out BacktrackingScheduler scheduler) || scheduler == null)
    //            throw new Exception("Scheduler is not initialized!");
    //        return scheduler;
    //    }


    //    public void ResetScheduler()
    //    {
    //        scheduler = null;
    //    }

    //    // פונקציה להדפסת מטריצת המעבר של ה-BacktrackingScheduler
    //    public void PrintSchedulerMatrix()
    //    {
    //        var scheduler = GetScheduler();
    //        scheduler.PrintTransitionMatrixToConsole();
    //    }

    //    // קריאה לאלגוריתם והתחלת אימון
    //    public async Task StartWorkoutAsync(TraineeDTO trainee, List<ExercisePlanDTO> exerciseOrder, DateTime startTime, int planDayId)
    //    {
    //        var scheduler = GetScheduler();
    //        // ננסה לקבל מיד את המנעול, ואם לא מצליחים נדפיס הודעה ונחכה
    //        if (!await _startWorkoutLock.WaitAsync(0))
    //        {
    //            Console.WriteLine("המערכת מחשבת נתונים");
    //            await _startWorkoutLock.WaitAsync(); // מחכים עד שהמנעול ישתחרר
    //        }
    //        try
    //        {
    //            var pathResult = await scheduler.FindOptimalPath(trainee, exerciseOrder, startTime);

    //            if (pathResult == null)
    //                throw new Exception("לא נמצא מסלול מתאים עבור מתאמן זה.");

    //            // בניית סטטוס תרגילים מה-PathResult
    //            var exercisesStatus = pathResult.ExerciseIdsInPath
    //                .OrderBy(pair => pair.Value.OrderInList)
    //                .Select(pair => new ExerciseStatusEntry
    //                {
    //                    OriginalExercise = pair.Key,
    //                    ExerciseId = pair.Value.ExerciseId,
    //                    OrderInList = pair.Value.OrderInList,
    //                    IsDone = false,
    //                    PerformedAt = null,
    //                    StartedAt = null
    //                }).ToList();
    //            _cache.Set($"Trainee_{trainee.TraineeId}", new TraineeExerciseStatus
    //            {
    //                Trainee = trainee,
    //                Exercises = exercisesStatus,
    //                planDayId = planDayId
    //            });

    //        }
    //        finally
    //        {
    //            _startWorkoutLock.Release();
    //        }
    //    }

    //    // קריאה להתחלת תרגיל עבור מתאמן
    //    public bool StartExercise(int traineeId, int exerciseId, DateTime startTime)
    //    {
    //        if (!_cache.TryGetValue($"Trainee_{traineeId}", out TraineeExerciseStatus traineeStatus) || traineeStatus == null)
    //            throw new Exception("Trainee not found");
    //        var scheduler = GetScheduler();

    //        var exercise = traineeStatus.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
    //        if (exercise == null)
    //            throw new Exception("Exercise not found for this trainee");

    //        exercise.StartedAt = startTime;
    //        return true;
    //    }

    //    // קריאה לסיום תרגיל עבור מתאמן
    //    public bool CompleteExercise(int traineeId, int exerciseId, DateTime endTime)
    //    {
    //        if (!_cache.TryGetValue($"Trainee_{traineeId}", out TraineeExerciseStatus traineeStatus) || traineeStatus == null)
    //            throw new Exception("Trainee not found");
    //        var scheduler = GetScheduler();

    //        var exercise = traineeStatus.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
    //        if (exercise == null)
    //            throw new Exception("Exercise not found for this trainee");

    //        exercise.IsDone = true;
    //        exercise.PerformedAt = endTime;

    //        if (traineeStatus.Exercises.All(e => e.IsDone))
    //        {
    //            SaveWorkoutToDatabase(traineeStatus);
    //            // ניתן למחוק מכאן את המתאמן כעת
    //            activeTrainees.Remove(traineeId);
    //            _cache.Remove($"Trainee_{traineeId}");
    //        }
    //        return true;
    //    }

    //    // לוגיקה למיפוי ושמירה למסד הנתונים 
    //    private async Task SaveWorkoutToDatabase(TraineeExerciseStatus status)
    //    {
    //        var scheduler = GetScheduler();

    //        PlanDay planDay = await planDayDAL.GetPlanDayByIdAsync(status.planDayId);
    //        var planDayDto = mapper.Map<PlanDayDTO>(planDay);

    //        // דוגמה: המרה ל-PlanDay, ExercisePlan ושמירה ב-DB
    //        var NewplanDay = new PlanDayDTO()
    //        {
    //            //PlanDayId = 0, // או ID חדש שיתקבל מהמסד
    //            TrainingPlanId = planDayDto.TrainingPlanId, // או ID של תוכנית האימון המתאימה
    //            ProgramName = "Workout Plan",
    //            DayOrder = planDayDto.DayOrder, // או סדר היום המתאים
    //            CreationDate = DateTime.Now,
    //            IsDefaultProgram = false,
    //            ParentProgramId = planDayDto.PlanDayId,
    //            IsHistoricalProgram = true
    //        };
    //        // שמירת ה-PlanDay
    //        var savedPlanDay = await planDayDAL.AddPlanDayAsync(mapper.Map<PlanDay>(NewplanDay));
    //        foreach (var exercise in status.Exercises)
    //        {
    //            var OrigenExercisePlan = await exercisePlanDAL.GetExercisePlanByIdAsync(exercise.OriginalExercise);
    //            // שמירת ה-ExercisePlan
    //            var exercisePlan = new ExercisePlanDTO()
    //            {
    //                ExerciseId = exercise.ExerciseId,
    //                PlanDayId = status.planDayId,
    //                TimesMax = OrigenExercisePlan.TimesMax,
    //                TimesMin = OrigenExercisePlan.TimesMin,
    //                PlanRepetitionsMax = OrigenExercisePlan.TimesMax,
    //                PlanRepetitionsMin = OrigenExercisePlan.TimesMin,
    //                PlanSets = OrigenExercisePlan.PlanSets,
    //                PlanWeight = OrigenExercisePlan.PlanWeight,
    //                CategoryId = OrigenExercisePlan.CategoryId,
    //                SubMuscleId = OrigenExercisePlan.SubMuscleId,
    //                TrainingDateTime = DateTime.Now,
    //                IndexOrder = exercise.OrderInList,
    //            };
    //            await exercisePlanDAL.AddExercisePlanAsync(mapper.Map<ExercisePlan>(exercisePlan));
    //        }
    //    }



    //    //public async Task<List<PathResult>> GetUpdatedWorkoutPlan(int traineeId)
    //    //{

    //    //}




    //    //// שומרים את תוכניות האימון הפעילות בזיכרון.
    //    //// מפתח: TraineeId, ערך: PathResultDTO (תוכנית האימון הפעילה)
    //    //private static readonly ConcurrentDictionary<int, PathResultDTO> _activeWorkoutPlans = new ConcurrentDictionary<int, PathResultDTO>();

    //    //// נתוני דוגמה (בפרויקט אמיתי זה יגיע ממסד נתונים או שירות אחר)
    //    //private static readonly List<TraineeDTO> _mockTrainees = new List<TraineeDTO>
    //    //{
    //    //    new TraineeDTO { TraineeId = 1, Name = "ישראל ישראלי", CurrentPlanDayId = 1 },
    //    //    new TraineeDTO { TraineeId = 2, Name = "שרה כהן", CurrentPlanDayId = 2 }
    //    //};

    //    //private static readonly List<ExerciseDTO> _mockExercises = new List<ExerciseDTO>
    //    //{
    //    //    new ExerciseDTO { ExerciseId = 1, ExerciseName = "לחיצת חזה במוט", Description = "תרגיל לחיזוק שרירי החזה", MuscleIds = new List<int> { 1 } },
    //    //    new ExerciseDTO { ExerciseId = 2, ExerciseName = "חתירה בפולי תחתון", Description = "תרגיל לחיזוק שרירי הגב", MuscleIds = new List<int> { 2 } },
    //    //    new ExerciseDTO { ExerciseId = 3, ExerciseName = "לחיצת כתפיים בדאמבלים", Description = "תרגיל לחיזוק שרירי הכתפיים", MuscleIds = new List<int> { 3 } },
    //    //    new ExerciseDTO { ExerciseId = 4, ExerciseName = "סקוואט", Description = "תרגיל רגליים", MuscleIds = new List<int> { 4, 5 } },
    //    //    new ExerciseDTO { ExerciseId = 5, ExerciseName = "בייספס יד-יד", Description = "תרגיל לידיים", MuscleIds = new List<int> { 6 } }
    //    //};

    //    //private static readonly List<ExercisePlanDTO> _mockExercisePlans = new List<ExercisePlanDTO>
    //    //{
    //    //    // PlanDayId 1
    //    //    new ExercisePlanDTO { PlanDayId = 1, ExerciseId = 1, Sets = 3, Reps = 10, RestTime = 60, TimesMax = 20, ExerciseDetails = _mockExercises.FirstOrDefault(e => e.ExerciseId == 1) },
    //    //    new ExercisePlanDTO { PlanDayId = 1, ExerciseId = 2, Sets = 4, Reps = 8, RestTime = 90, TimesMax = 25, ExerciseDetails = _mockExercises.FirstOrDefault(e => e.ExerciseId == 2) },
    //    //    new ExercisePlanDTO { PlanDayId = 1, ExerciseId = 4, Sets = 3, Reps = 12, RestTime = 60, TimesMax = 20, ExerciseDetails = _mockExercises.FirstOrDefault(e => e.ExerciseId == 4) },
    //    //    // PlanDayId 2
    //    //    new ExercisePlanDTO { PlanDayId = 2, ExerciseId = 3, Sets = 3, Reps = 10, RestTime = 60, TimesMax = 15, ExerciseDetails = _mockExercises.FirstOrDefault(e => e.ExerciseId == 3) },
    //    //    new ExercisePlanDTO { PlanDayId = 2, ExerciseId = 5, Sets = 3, Reps = 12, RestTime = 45, TimesMax = 10, ExerciseDetails = _mockExercises.FirstOrDefault(e => e.ExerciseId == 5) }
    //    //};


    //    // --- פונקציה שמפעילה את האלגוריתם (לצורך הדוגמה, יצירת מסלול פשוט) ---
    //    public PathResult RunAlgorithmAndInitializeWorkout(RunAlgorithmRequest request)
    //    {
    //        // הסרה של אימון קודם אם קיים
    //        _activeWorkoutPlans.TryRemove(request.Trainee, out _);

    //        // 1. קבל את פרטי המתאמן והתרגילים ליום התוכנית
    //        TraineeDTO trainee = GetTraineeById(request.Trainee);
    //        if (trainee == null)
    //        {
    //            throw new ArgumentException("Trainee not found.");
    //        }

    //        List<ExercisePlanDTO> exerciseOrder = GetExercisePlansForPlanDay(request.planday);
    //        if (exerciseOrder == null || !exerciseOrder.Any())
    //        {
    //            throw new ArgumentException("No exercises found for the specified plan day.");
    //        }

    //        // 2. "הפעלת האלגוריתם" (לצורך הדוגמה, סדר תרגילים רנדומלי או לפי ID)
    //        // בפרויקט אמיתי, כאן היית מפעיל לוגיקה מורכבת של תזמון
    //        var bestPathIds = exerciseOrder.Select(ep => ep.ExerciseId).ToList(); // לדוגמה, לפי סדר ה-ID

    //        // 3. בנה את ה-PathResultDTO הסופי עם זמנים ופרטים
    //        var currentTime = request.StartTime;
    //        var exerciseEntries = new Dictionary<int, ExerciseEntry>();
    //        int orderInList = 0;

    //        foreach (var exerciseId in bestPathIds)
    //        {
    //            var planDetails = exerciseOrder.FirstOrDefault(ep => ep.ExerciseId == exerciseId);
    //            if (planDetails == null) continue;

    //            var duration = TimeSpan.FromMinutes(planDetails.TimesMax); // משך תרגיל מהתוכנית
    //            var exerciseDetails = GetExerciseDetails(exerciseId);

    //            // קביעת זמני התרגיל - פשוט לצרכי הדוגמה
    //            var exerciseStartTime = currentTime;
    //            var exerciseEndTime = currentTime.Add(duration);

    //            var entry = new ExerciseEntry
    //            {
    //                ExerciseId = exerciseId,
    //                OrderInList = orderInList,
    //                StartTime = exerciseStartTime,
    //                EndTime = exerciseEndTime,
    //                //ExerciseDetails = exerciseDetails,
    //                //Sets = planDetails.Sets,
    //                //Reps = planDetails.Reps,
    //                //RestTime = planDetails.RestTime
    //            };
    //            exerciseEntries.Add(exerciseId, entry);

    //            currentTime = exerciseEndTime;//.AddSeconds(planDetails.RestTime); // הוסף זמן מנוחה
    //            orderInList++;
    //        }

    //        var pathResultDto = new PathResult
    //        {
    //            Trainee = trainee,
    //            ExerciseIdsInPath = exerciseEntries,
    //            StartTime = request.StartTime,
    //            EndTime = currentTime, // סוף האימון
    //            AlternativesUsed = 0, // או כפי שהאלגוריתם מחזיר
    //            //CurrentExerciseIndex = 0 // מתחיל מהתרגיל הראשון
    //        };

    //        // שמור את תוכנית האימון הפעילה בזיכרון
    //        _activeWorkoutPlans[trainee.TraineeId] = pathResultDto;

    //        return pathResultDto;
    //    }

    //    // --- פונקציה לקבלת תוכנית האימון המעודכנת (כל התוכנית) ---
    //    public PathResult GetUpdatedWorkoutPlan(int traineeId)
    //    {
    //        _activeWorkoutPlans.TryGetValue(traineeId, out var plan);
    //        return plan; // מחזיר את ה-DTO ישירות מהזיכרון
    //    }

    //    // --- פונקציה לקבלת התרגיל הבא ---
    //    public NextExerciseResponse GetNextExerciseInWorkout(int traineeId)
    //    {
    //        if (!_activeWorkoutPlans.TryGetValue(traineeId, out var currentPlan))
    //        {
    //            return new NextExerciseResponse { IsWorkoutComplete = true, Message = "לא נמצא אימון פעיל למתאמן." };
    //        }

    //        var sortedEntries = currentPlan.ExerciseEntries.Values.OrderBy(e => e.OrderInList).ToList();

    //        if (currentPlan.CurrentExerciseIndex >= sortedEntries.Count)
    //        {
    //            // האימון הסתיים
    //            return new NextExerciseResponse { IsWorkoutComplete = true, Message = "האימון הושלם בהצלחה!" };
    //        }

    //        var nextExerciseEntry = sortedEntries[currentPlan.CurrentExerciseIndex];

    //        // עדכן את האינדקס לתרגיל הבא לפעם הבאה
    //        // (זה חשוב אם ה-frontend לא תמיד קורא ל-CompleteExercise)
    //        // currentPlan.CurrentExerciseIndex++; // נעדכן רק ב-CompleteExercise

    //        return new NextExerciseResponse
    //        {
    //            NextExercise = nextExerciseEntry,
    //            IsWorkoutComplete = false,
    //            RemainingExercisesCount = sortedEntries.Count - (currentPlan.CurrentExerciseIndex + 1),
    //            Message = "תרגיל הבא."
    //        };
    //    }

    //    // --- פונקציה לסימון תרגיל כהושלם (מעדכן את מצב האימון בזיכרון) ---
    //    public bool CompleteExercise(int traineeId, int exerciseId)
    //    {
    //        if (!_activeWorkoutPlans.TryGetValue(traineeId, out var currentPlan))
    //        {
    //            return false; // אין אימון פעיל
    //        }

    //        var currentEntries = currentPlan.ExerciseEntries.Values.OrderBy(e => e.OrderInList).ToList();
    //        var exerciseToComplete = currentEntries.FirstOrDefault(e => e.ExerciseId == exerciseId && e.OrderInList == currentPlan.CurrentExerciseIndex);

    //        if (exerciseToComplete == null)
    //        {
    //            // התרגיל לא תואם לתרגיל הנוכחי הצפוי
    //            return false;
    //        }

    //        // עדכן את האינדקס לתרגיל הבא
    //        currentPlan.CurrentExerciseIndex++;

    //        // אם נשאר מקום (לדוגמה, מכונה תפוסה) ניתן לעדכן את סדר התרגילים כאן
    //        // לדוגמה: אם התרגיל הבא לא זמין, ניתן לדלג עליו ולמצוא את הבא בתור
    //        // for (int i = currentPlan.CurrentExerciseIndex; i < currentEntries.Count; i++) { ... }

    //        return true;
    //    }

    //    // --- פונקציות עזר (במקום DAL/DB) ---
    //    public TraineeDTO GetTraineeById(int traineeId)
    //    {
    //        return _mockTrainees.FirstOrDefault(t => t.TraineeId == traineeId);
    //    }

    //    public List<ExercisePlanDTO> GetExercisePlansForPlanDay(int planDayId)
    //    {
    //        return _mockExercisePlans.Where(ep => ep.PlanDayId == planDayId).ToList();
    //    }

    //    public ExerciseDTO GetExerciseDetails(int exerciseId)
    //    {
    //        return _mockExercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
    //    }
    // }
    // public class ActiveWorkoutManager : IActiveWorkoutManager
    {
        private BacktrackingScheduler scheduler; // נשנה את זה ל-GetScheduler()
        public bool IsInitialized => _cache.TryGetValue("Scheduler", out BacktrackingScheduler existing) && existing != null; // עדכון לשימוש ב-cache

        // private Dictionary<int, TraineeExerciseStatus> activeTrainees; // זה לא נחוץ יותר, הכל ב-cache
        private readonly ITraineeBLL _traineeBLL;
        private readonly IPlanDayDAL _planDayDAL; // השתמש בזה ולא בשני שדות נפרדים
        private readonly IExercisePlanDAL _exercisePlanDAL; // השתמש בזה ולא בשני שדות נפרדים
        private readonly IMapper _mapper; // השתמש בזה ולא בשני שדות נפרדים
        private readonly IMemoryCache _cache;
        private readonly ITrainingPlanDAL _trainingPlan;

        private readonly SemaphoreSlim _startWorkoutLock = new SemaphoreSlim(1, 1);

        public ActiveWorkoutManager(
            IMemoryCache cache,
            ITraineeBLL traineeBLL,
            IPlanDayDAL planDayDAL,
            IExercisePlanDAL exercisePlanDAL,
            ITrainingPlanDAL trainingPlanDAL,
            IMapper mapper)
        {
            _cache = cache;
            _traineeBLL = traineeBLL;
            _planDayDAL = planDayDAL; // שימוש ב-planDayDAL שהוזרק
            _exercisePlanDAL = exercisePlanDAL; // שימוש ב-exercisePlanDAL שהוזרק
            _trainingPlan = trainingPlanDAL;
            _mapper = mapper; // <--- שמור את המופע המוזרק
            // activeTrainees = new Dictionary<int, TraineeExerciseStatus>(); // אין צורך בזה

            // הגדרת AutoMapper - יש לשים לב למפות החדשות
            //var configTaskConverter = new MapperConfiguration(cfg =>
            //{
            //    cfg.CreateMap<PlanDay, PlanDayDTO>().ReverseMap();
            //    cfg.CreateMap<ExercisePlan, ExercisePlanDTO>().ReverseMap();
            //    // הוסף מפות למודלים הפנימיים ול-DTOs המתאימים
            //});
            //_mapper = new Mapper(configTaskConverter);
        }

        public void Initialize(
            List<ExerciseDTO> exerciseList,
            List<GraphEdgeDTO> exerciseEdges,
            List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
            List<MuscleEdgeDTO> muscleEdges,
            Dictionary<int, int> equipmentCountByExercise,
            DateTime firstSlotStart,
            int slotMinutes,
            int slotCount)
        {
            if (_cache.TryGetValue("Scheduler", out BacktrackingScheduler existing) && existing != null)
                throw new Exception("Scheduler already initialized!");

            // ודא שאתה יוצר מופע חדש של BacktrackingScheduler
            this.scheduler = new BacktrackingScheduler(_traineeBLL); // הקוד שלך כבר עושה את זה
            this.scheduler.Initialize( // הקוד שלך כבר עושה את זה
                exerciseList, exerciseEdges, exerciseToMuscleEdges, muscleEdges,
                equipmentCountByExercise, firstSlotStart, slotMinutes, slotCount
            );
            _cache.Set("Scheduler", this.scheduler); // ודא שאתה שומר את המופע שיצרת זה עתה
        }

        private BacktrackingScheduler GetScheduler()
        {
            if (!_cache.TryGetValue("Scheduler", out BacktrackingScheduler scheduler) || scheduler == null)
                throw new Exception("Scheduler is not initialized!");
            return scheduler;
        }

        public void ResetScheduler()
        {
            // scheduler = null; // אין צורך, מספיק למחוק מהקאש
            _cache.Remove("Scheduler");
        }

        public void PrintSchedulerMatrix()
        {
            var scheduler = GetScheduler();
            scheduler.PrintTransitionMatrixToConsole();
        }

        // --- קריאה לאלגוריתם והתחלת אימון ---
        public async Task<PathResult> StartWorkoutAsync(TraineeDTO trainee, List<ExercisePlanDTO> exerciseOrder, DateTime startTime, int planDayId)
        {
            var scheduler = GetScheduler(); // המתודה הקיימת שלך
            bool enteredLock = false; // אתחל כאן כדי להיות בטוחים

            try // בלוק try שמקיף את הניסיון להיכנס למנעול
            {
                // מחכים עד שהמנעול ישתחרר ב-2 דקות (120 שניות)
                enteredLock = await _startWorkoutLock.WaitAsync(TimeSpan.FromSeconds(120));

                if (!enteredLock)
                {
                    // אם המנעול לא נכנס בתוך 120 שניות, זרוק חריגה מותאמת אישית
                    throw new ServerBusyException("השרת עמוס כרגע בעיבוד בקשות אימון. אנא המתן מעט ונסה שוב.");
                }

                // אם הגענו לכאן, נכנסנו למנעול בהצלחה
                // ... (הקוד הקיים שלך בתוך ה-try הפנימי)
                // קבל את ה-PathResult מה-scheduler
                var pathResult = await scheduler.FindOptimalPath(trainee, exerciseOrder, startTime);

                if (pathResult == null)
                    throw new Exception("לא נמצא מסלול מתאים עבור מתאמן זה.");

                // בניית TraineeExerciseStatus מה-PathResult
                var exercisesStatus = new List<ExerciseStatusEntry>();
                foreach (var pair in pathResult.ExerciseIdsInPath.OrderBy(p => p.Value.OrderInList))
                {
                    var exerciseEntryFromScheduler = pair.Value; // PathResultExerciseEntry
                    var originalExercisePlan = exerciseOrder.FirstOrDefault(ep => ep.ExercisePlanId == exerciseEntryFromScheduler.OriginalExercise); // השג את פרטי התוכנית המקוריים

                    exercisesStatus.Add(new ExerciseStatusEntry
                    {
                        OriginalExercise = exerciseEntryFromScheduler.OriginalExercise == 0 ? exerciseEntryFromScheduler.ExerciseId : exerciseEntryFromScheduler.OriginalExercise,
                        ExerciseId = exerciseEntryFromScheduler.ExerciseId,
                        OrderInList = exerciseEntryFromScheduler.OrderInList,
                        IsDone = false,
                        PerformedAt = null,
                        StartedAt = null,
                        Plan=exerciseEntryFromScheduler.ExerciseDetails,
                    });
                }

                var traineeWorkoutStatus = new TraineeExerciseStatus
                {
                    Trainee = trainee,
                    Exercises = exercisesStatus,
                    planDayId = planDayId,
                    WorkoutStartTime = startTime,
                    WorkoutEndTime = pathResult.EndTime, // זמן סיום מהאלגוריתם
                    CurrentExerciseOrderIndex = 0 // התחל מהתרגיל הראשון
                };

                _cache.Set($"Trainee_{trainee.TraineeId}", traineeWorkoutStatus);

                return _mapper.Map<PathResult>(traineeWorkoutStatus);
            }
            finally // בלוק finally שיוודא שהמנעול משתחרר
            {
                // חשוב: שחרר את המנעול רק אם נכנסנו אליו
                if (enteredLock)
                {
                    _startWorkoutLock.Release();
                }
            }
        }

        //// --- קריאה לאלגוריתם והתחלת אימון ---
        //public async Task<PathResult> StartWorkoutAsync(TraineeDTO trainee, List<ExercisePlanDTO> exerciseOrder, DateTime startTime, int planDayId)
        //{
        //    var scheduler = GetScheduler();
        //    bool enteredLock = await _startWorkoutLock.WaitAsync(TimeSpan.FromSeconds(120)); // מחכים עד שהמנעול ישתחרר ב-2 דקות


        //    // אם הגענו לכאן, נכנסו למנעול בהצלחה
        //    //await _startWorkoutLock.WaitAsync(); // מחכים עד שהמנעול ישתחרר
        //    try
        //    {
        //        // קבל את ה-PathResult מה-scheduler
        //        var pathResult = await scheduler.FindOptimalPath(trainee, exerciseOrder, startTime);

        //        if (pathResult == null)
        //            throw new Exception("לא נמצא מסלול מתאים עבור מתאמן זה.");

        //        // בניית TraineeExerciseStatus מה-PathResult
        //        var exercisesStatus = new List<ExerciseStatusEntry>();
        //        foreach (var pair in pathResult.ExerciseIdsInPath.OrderBy(p => p.Value.OrderInList))
        //        {
        //            var exerciseEntryFromScheduler = pair.Value; // PathResultExerciseEntry
        //            var originalExercisePlan = exerciseOrder.FirstOrDefault(ep => ep.ExercisePlanId == exerciseEntryFromScheduler.OriginalExercise); // השג את פרטי התוכנית המקוריים
        //            //if (originalExercisePlan == null)
        //            //{
        //            //    // אם לא נמצא, זהו מצב חריג, טפל בו
        //            //    //Console.WriteLine($"Warning: Original ExercisePlan not found for ID: {exerciseEntryFromScheduler.OriginalExercise}");
        //            //    //continue;
        //            //  //אני רוצה שהוא ישים בתרגיל המקורי את התרגיל הנוכחי
        //            //    ;originalExercisePlan = new ExercisePlanDTO
        //            //    {
        //            //        ExerciseId = exerciseEntryFromScheduler.ExerciseId,
        //            //        PlanDayId = planDayId,
        //            //        TimesMax = 0, // או ערך ברירת מחדל אחר
        //            //        TimesMin = 0, // או ערך ברירת מחדל אחר
        //            //        PlanRepetitionsMax = 0, // או ערך ברירת מחדל אחר
        //            //        PlanRepetitionsMin = 0, // או ערך ברירת מחדל אחר
        //            //        PlanSets = 0, // או ערך ברירת מחדל אחר
        //            //        PlanWeight = 0, // או ערך ברירת מחדל אחר
        //            //        CategoryId = 0, // או ערך ברירת מחדל אחר
        //            //        SubMuscleId = 0, // או ערך ברירת מחדל אחר
        //            //        TrainingDateTime = startTime,
        //            //        IndexOrder = exerciseEntryFromScheduler.OrderInList
        //            //    };  
        //            //}

        //            exercisesStatus.Add(new ExerciseStatusEntry
        //            {
        //                OriginalExercise = exerciseEntryFromScheduler.OriginalExercise == 0 ? exerciseEntryFromScheduler.ExerciseId : exerciseEntryFromScheduler.OriginalExercise,
        //                ExerciseId = exerciseEntryFromScheduler.ExerciseId,
        //                OrderInList = exerciseEntryFromScheduler.OrderInList,
        //                IsDone = false,
        //                PerformedAt = null,
        //                StartedAt = null,
        //                // העתק פרטים מ-ExercisePlanDTO
        //                //Sets = originalExercisePlan.Sets,
        //                //Reps = originalExercisePlan.Reps,
        //                //RestTime = originalExercisePlan.RestTime,
        //                //TimesMax = originalExercisePlan.TimesMax,
        //                //TimesMin = originalExercisePlan.TimesMin,
        //                //ExerciseDetails = originalExercisePlan.ExerciseDetails // כבר DTO
        //            });
        //        }

        //        var traineeWorkoutStatus = new TraineeExerciseStatus
        //        {
        //            Trainee = trainee,
        //            Exercises = exercisesStatus,
        //            planDayId = planDayId,
        //            WorkoutStartTime = startTime,
        //            WorkoutEndTime = pathResult.EndTime, // זמן סיום מהאלגוריתם
        //            CurrentExerciseOrderIndex = 0 // התחל מהתרגיל הראשון
        //        };

        //        _cache.Set($"Trainee_{trainee.TraineeId}", traineeWorkoutStatus);

        //        // המר את TraineeExerciseStatus ל-PathResultDTO להחזרה ל-frontend
        //        //return MapTraineeStatusToPathResultDTO(traineeWorkoutStatus);
        //        // *** המר את TraineeExerciseStatus ל-PathResultDTO ישירות כאן ***
        //        return _mapper.Map<PathResult>(traineeWorkoutStatus); // <--- שינוי כאן!
        //    }
        //    finally
        //    {
        //        _startWorkoutLock.Release();
        //    }
        //}

        // --- קריאה להתחלת תרגיל עבור מתאמן ---
        public bool StartExercise(int traineeId, int exerciseId, DateTime startTime)
        {
            if (!_cache.TryGetValue($"Trainee_{traineeId}", out TraineeExerciseStatus traineeStatus) || traineeStatus == null)
                throw new Exception("Trainee not found or workout not active.");

            var currentExercise = traineeStatus.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId && e.OrderInList == (traineeStatus.CurrentExerciseOrderIndex + 1));
            if (currentExercise == null)
            {
                // זה יכול לקרות אם ה-frontend מנסה להתחיל תרגיל לא נכון או שהסדר השתנה
                throw new Exception($"Exercise {exerciseId} not the current expected exercise for this trainee or not found.");
            }

            currentExercise.StartedAt = startTime;
            // *** הוסף את השורה הקריטית הזו!!! ***
            _cache.Set($"Trainee_{traineeId}", traineeStatus);
            return true;
        }

        // --- קריאה לסיום תרגיל עבור מתאמן ---
        public async Task<bool> CompleteExercise(int traineeId, int exerciseId, DateTime endTime)
        {
            if (!_cache.TryGetValue($"Trainee_{traineeId}", out TraineeExerciseStatus traineeStatus) || traineeStatus == null)
                throw new Exception("Trainee not found or workout not active.");

            var exerciseToComplete = traineeStatus.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId && !e.IsDone && e.OrderInList == (traineeStatus.CurrentExerciseOrderIndex + 1));
            if (exerciseToComplete == null)
            {
                throw new Exception($"Exercise {exerciseId} is not the current active exercise or already completed.");
            }

            exerciseToComplete.IsDone = true;
            exerciseToComplete.PerformedAt = endTime;

            // קידום האינדקס לתרגיל הבא
            traineeStatus.CurrentExerciseOrderIndex++;

            // בדוק אם האימון הושלם
            if (traineeStatus.Exercises.All(e => e.IsDone))
            {
                // שמור ל-DB רק אם כל האימון הסתיים
                await SaveWorkoutToDatabase(traineeStatus);
                _cache.Remove($"Trainee_{traineeId}"); // הסר מהזיכרון cache
                Console.WriteLine($"Workout for Trainee {traineeId} completed and saved.");
            }
            // אם האימון לא הושלם, ה-TraineeExerciseStatus נשאר במטמון עם הסטטוס המעודכן
            return true;
        }


        // --- הוספת פונקציה: GetUpdatedWorkoutPlan ---
        public PathResult GetUpdatedWorkoutPlan(int traineeId)
        {
            if (_cache.TryGetValue($"Trainee_{traineeId}", out TraineeExerciseStatus traineeStatus) && traineeStatus != null)
            {
                return MapTraineeStatusToPathResultDTO(traineeStatus);
            }
            return null; // או זרוק Exception אם המתאמן לא נמצא
        }

        // --- הוספת פונקציה: GetNextExerciseInWorkout ---
        public NextExerciseResponse GetNextExerciseInWorkout(int traineeId)
        {
            if (!_cache.TryGetValue($"Trainee_{traineeId}", out TraineeExerciseStatus traineeStatus) || traineeStatus == null)
            {
                return new NextExerciseResponse { TraineeId = traineeId, IsWorkoutComplete = true, Message = "לא נמצא אימון פעיל למתאמן." };
            }

            var nextExerciseEntry = traineeStatus.Exercises.FirstOrDefault(e => e.OrderInList == (traineeStatus.CurrentExerciseOrderIndex + 1));

            if (nextExerciseEntry == null)
            {
                // האימון הסתיים, או שיש בעיה כלשהי
                bool allDone = traineeStatus.Exercises.All(e => e.IsDone);
                return new NextExerciseResponse
                {
                    TraineeId = traineeId,
                    IsWorkoutComplete = allDone, // אם כל התרגילים בוצעו
                    Message = allDone ? "האימון הושלם בהצלחה!" : "לא נמצא תרגיל הבא (ייתכן שהיה דילוג או תקלה).",
                    RemainingExercisesCount = 0
                };
            }
            var NextExercise = _mapper.Map<ExerciseEntry>(nextExerciseEntry); // המרה ל-DTO
           // NextExercise.ExerciseDetails=
            return new NextExerciseResponse
            {
                TraineeId = traineeId,
                NextExercise = _mapper.Map<ExerciseEntry>(nextExerciseEntry), // המרה ל-DTO
                IsWorkoutComplete = false,
                RemainingExercisesCount = traineeStatus.Exercises.Count(e => !e.IsDone) - 1, // כמה נותרו (לא כולל הנוכחי)
                                                                                             //  RecommendedRestTimeSeconds = nextExerciseEntry.RestTime // זמן מנוחה
            };
        }

        // --- לוגיקה למיפוי ושמירה למסד הנתונים ---
        private async Task SaveWorkoutToDatabase(TraineeExerciseStatus status)
        {
            // var scheduler = GetScheduler(); // לא נחוץ לשמירה ל-DB

            PlanDay planDay = await _planDayDAL.GetPlanDayByIdAsync(status.planDayId);
            if (planDay == null)
            {
                Console.WriteLine($"Error: Original PlanDay with ID {status.planDayId} not found for historical save.");
                // ניתן לזרוק אקשן או ליצור PlanDay חדש
                planDay = new PlanDay { PlanDayId = 0, ProgramName = "Unknown Original Plan", CreationDate = DateTime.Now }; // fallback
            }

            var planDayDto = _mapper.Map<PlanDayDTO>(planDay);

            var newPlanDay = new PlanDayDTO()
            {
                //PlanDayId = 0, // ID חדש ייווצר ב-DAL
                TrainingPlanId = planDayDto.TrainingPlanId,
                ProgramName = "Workout History: ",
                //ProgramName = "Workout History: " + status.Trainee.TraineeName + " - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"),
                DayOrder = planDayDto.DayOrder,
                CreationDate = DateTime.Now,
                IsDefaultProgram = false,
                ParentProgramId = status.planDayId, // הפניה לתוכנית המקורית
                IsHistoricalProgram = true
            };
            // שמירת ה-PlanDay (מודל)
            var savedPlanDay = await _planDayDAL.AddPlanDayAsync(_mapper.Map<PlanDay>(newPlanDay));

            foreach (var exerciseStatusEntry in status.Exercises)
            {
                var originalExercisePlan = await _exercisePlanDAL.GetExercisePlanByIdAsync(exerciseStatusEntry.OriginalExercise);
                if (originalExercisePlan == null)
                {
                    Console.WriteLine($"Warning: Original ExercisePlan with ID {exerciseStatusEntry.OriginalExercise} not found for historical exercise {exerciseStatusEntry.ExerciseId}. Skipping.");
                    continue;
                }

                // שמירת ה-ExercisePlan (מודל)
                var exercisePlan = new ExercisePlanDTO()
                {
                    ExerciseId = exerciseStatusEntry.ExerciseId,
                    PlanDayId = savedPlanDay, // קשר ל-PlanDay החדש ההיסטורי
                    TimesMax = originalExercisePlan.TimesMax,
                    TimesMin = originalExercisePlan.TimesMin,
                    PlanRepetitionsMax = originalExercisePlan.PlanRepetitionsMax,
                    PlanRepetitionsMin = originalExercisePlan.PlanRepetitionsMin,
                    PlanSets = originalExercisePlan.PlanSets,
                    PlanWeight = originalExercisePlan.PlanWeight,
                    CategoryId = originalExercisePlan.CategoryId,
                    SubMuscleId = originalExercisePlan.SubMuscleId,
                    TrainingDateTime = (DateTime)(exerciseStatusEntry.PerformedAt ?? exerciseStatusEntry.StartedAt), // השתמש בזמן הביצוע בפועל
                    IndexOrder = exerciseStatusEntry.OrderInList,
                    Exercise=exerciseStatusEntry.Plan.Exercise,
                    // ייתכן שתרצה להוסיף כאן גם את StartedAt, PerformedAt, IsDone כ-Custom Properties אם ה-DB תומך
                };
                await _exercisePlanDAL.AddExercisePlanAsync(_mapper.Map<ExercisePlan>(exercisePlan));
            }
        }

        // --- פונקציות עזר: המרה מ-TraineeExerciseStatus ל-PathResultDTO ---
        private PathResult MapTraineeStatusToPathResultDTO(TraineeExerciseStatus status)
        {
            // *** פשוט קראי ל-AutoMapper כאן ***
            return _mapper.Map<PathResult>(status);

            // var exerciseEntries = new List<ExerciseEntry>();
            //var exerciseEntries = new Dictionary<int ,ExerciseEntry>();
            //int i = 0;
            //foreach (var entry in status.Exercises.OrderBy(e => e.OrderInList))
            //{
            //    // המרה מ-ExerciseStatusEntry ל-ExerciseEntryDTO באמצעות AutoMapper
            //    exerciseEntries.Add( i,_mapper.Map<ExerciseEntry>(entry));
            //    i++;
            //}

            //return new PathResult
            //{
            //    Trainee = status.Trainee,
            //    ExerciseIdsInPath = exerciseEntries,
            //    StartTime = status.WorkoutStartTime,
            //    EndTime = status.WorkoutEndTime, // זה יהיה זמן סיום משוער מהאלגוריתם
            //    AlternativesUsed = 0, // צריך להוסיף שדה זה ל-TraineeExerciseStatus אם רוצים לעקוב
            //    CurrentExerciseOrderIndex = status.CurrentExerciseOrderIndex,
            //    IsWorkoutComplete = status.Exercises.All(e => e.IsDone)
            //};

        }

        //public async Task<ActiveTrainingPlanResponse?> GetActiveTrainingPlanForTrainee(int traineeId)
        //{
        //    // Fetch the active training plan for the trainee, including related PlanDays and Trainee details.
        //    // Using your DAL interfaces, you would call appropriate methods.
        //    // This assumes your DAL has methods to fetch TrainingPlan with its related PlanDays and Trainee.
        //    // If IDAL/DAL do not provide direct Include, you might need to fetch them separately
        //    // or adapt your DAL to provide such composite data.

        //    // Example assuming your DAL methods exist and return DB Entities:
        //    // First, get the active TrainingPlan.
        //    var activeTrainingPlan = await _planDayDAL.GetActiveTrainingPlanByTraineeIdAsync(traineeId); // Assuming you add this method to IPlanDayDAL/PlanDayDAL

        //    if (activeTrainingPlan == null)
        //    {
        //        return null; // No active plan for this trainee
        //    }

        //    // Get all default PlanDays for this active training plan
        //    var defaultPlanDays = await _planDayDAL.GetDefaultPlanDaysByTrainingPlanIdAsync(activeTrainingPlan.TrainingPlanId); // Assuming this method exists

        //    // Get trainee details for TraineeName
        //    var trainee = await _traineeBLL.GetTraineeByIdAsync(traineeId); // Assuming GetTraineeByIdAsync returns Trainee entity or DTO

        //    // Calculate the start of the current week (Sunday at midnight)
        //    DateTime today = DateTime.Today;
        //    DayOfWeek currentDayOfWeek = today.DayOfWeek;
        //    int daysSinceSunday = (int)currentDayOfWeek - (int)DayOfWeek.Sunday;
        //    if (daysSinceSunday < 0)
        //    {
        //        daysSinceSunday += 7; // Adjust for systems where Sunday is not 0 (e.g., ISO-8601 where Monday is 1)
        //    }
        //    DateTime startOfCurrentWeek = today.AddDays(-daysSinceSunday);
        //    startOfCurrentWeek = new DateTime(startOfCurrentWeek.Year, startOfCurrentWeek.Month, startOfCurrentWeek.Day, 0, 0, 0, DateTimeKind.Local); // Or Utc, depending on your DB storing convention

        //    var planDaysForFrontend = new List<PlanDayResponseForFrontend>();

        //    foreach (var defaultPlanDay in defaultPlanDays)
        //    {
        //        // Find the latest completed historical "child" PlanDay for this default PlanDay and the current trainee's active training plan.
        //        // This assumes PlanDay entity has ParentProgramId, IsHistoricalProgram, CreationDate, and TrainingPlanId.
        //        var lastCompletedChild = await _planDayDAL.GetLastCompletedHistoricalPlanDayForParentAndTrainingPlanAsync(
        //                                    defaultPlanDay.PlanDayId,
        //                                    activeTrainingPlan.TrainingPlanId); // Assuming this method exists in IPlanDayDAL/PlanDayDAL

        //        bool isCompletedThisWeek = false;
        //        if (lastCompletedChild != null && lastCompletedChild.CreationDate >= startOfCurrentWeek)
        //        {
        //            isCompletedThisWeek = true;
        //        }

        //        planDaysForFrontend.Add(new PlanDayResponseForFrontend
        //        {
        //            PlanDayId = defaultPlanDay.PlanDayId,
        //            ProgramName = defaultPlanDay.ProgramName,
        //            DayOrder = defaultPlanDay.DayOrder,
        //            IsDefaultProgram = defaultPlanDay.IsDefaultProgram,
        //            IsCompletedThisWeek = isCompletedThisWeek
        //        });
        //    }

        //    var response = new ActiveTrainingPlanResponse
        //    {
        //        TrainingPlanId = activeTrainingPlan.TrainingPlanId,
        //        TraineeId = traineeId, // Use the traineeId from the parameter
        //        TraineeName = trainee?.TraineeName ?? "Unknown Trainee", // Safely access trainee name
        //        PlanDays = planDaysForFrontend
        //    };

        //    return response;
        //}

        public async Task<ActiveTrainingPlanResponse?> GetActiveTrainingPlanForTrainee(int traineeId)
        {
            var activePlanEntity = await _trainingPlan.GetActiveTrainingPlanWithDetails(traineeId);

            if (activePlanEntity == null)
            {
                return null;
            }

            // חישוב תחילת השבוע (ראשון ב-00:00)
            var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);

            var planDaysForFrontend = new List<PlanDayResponseForFrontend>();

            if (activePlanEntity.PlanDays != null)
            {
                foreach (var planDayEntity in activePlanEntity.PlanDays.OrderBy(pd => pd.DayOrder))
                {
                    if (planDayEntity.IsDefaultProgram)
                    {
                        // נבדוק האם יום האימון הושלם השבוע
                        // אם LastCompletionDate הוא null, או שהוא מוקדם מראשון השבוע - הוא לא הושלם השבוע
                        //bool isCompletedThisWeek = planDayEntity.CreationDate.HasValue &&
                        //                           planDayEntity.CreationDate.Value >= startOfWeek;

                        var lastCompletedChild = await _planDayDAL.GetLastCompletedHistoricalPlanDayForParentAndTrainingPlanAsync(
                            planDayEntity.PlanDayId, activePlanEntity.TrainingPlanId);

                        if ((lastCompletedChild != null && lastCompletedChild.CreationDate <= startOfWeek)||(lastCompletedChild == null))
                        {
                            // bool isCompletedThisWeek = lastCompletedChild.CreationDate >= startOfWeek;
                            // הוסף רק ימי אימון שעדיין לא הושלמו השבוע
                            //if (!isCompletedThisWeek)
                            //{
                            planDaysForFrontend.Add(new PlanDayResponseForFrontend
                            {
                                PlanDayId = planDayEntity.PlanDayId,
                                ProgramName = planDayEntity.ProgramName,
                                DayOrder = planDayEntity.DayOrder,
                                IsDefaultProgram = planDayEntity.IsDefaultProgram,
                                IsCompletedThisWeek = false, // תמיד יהיה false כי סיננו החוצה את אלו שהושלמו
                                                             // העבר שדות נוספים מה-Entity ל-DTO
                                TrainingPlanId = planDayEntity.TrainingPlanId,
                                CreationDate = planDayEntity.CreationDate,
                                ParentProgramId = planDayEntity.ParentProgramId,
                                IsHistoricalProgram = planDayEntity.IsHistoricalProgram
                            });
                            //}
                        }
                        else
                        {
                            planDaysForFrontend.Add(new PlanDayResponseForFrontend
                            {
                                PlanDayId = planDayEntity.PlanDayId,
                                ProgramName = planDayEntity.ProgramName,
                                DayOrder = planDayEntity.DayOrder,
                                IsDefaultProgram = planDayEntity.IsDefaultProgram,
                                IsCompletedThisWeek = true,
                                TrainingPlanId = planDayEntity.TrainingPlanId,
                                CreationDate = planDayEntity.CreationDate,
                                ParentProgramId = planDayEntity.ParentProgramId,
                                IsHistoricalProgram = planDayEntity.IsHistoricalProgram
                            });
                        }


                        //bool isCompletedThisWeek = planDayEntity.CreationDate >= startOfWeek;
                        //// הוסף רק ימי אימון שעדיין לא הושלמו השבוע
                        //if (!isCompletedThisWeek)
                        //{
                        //    planDaysForFrontend.Add(new PlanDayResponseForFrontend
                        //    {
                        //        PlanDayId = planDayEntity.PlanDayId,
                        //        ProgramName = planDayEntity.ProgramName,
                        //        DayOrder = planDayEntity.DayOrder,
                        //        IsDefaultProgram = planDayEntity.IsDefaultProgram,
                        //        IsCompletedThisWeek = false, // תמיד יהיה false כי סיננו החוצה את אלו שהושלמו
                        //                                     // העבר שדות נוספים מה-Entity ל-DTO
                        //        TrainingPlanId = planDayEntity.TrainingPlanId,
                        //        CreationDate = planDayEntity.CreationDate,
                        //        ParentProgramId = planDayEntity.ParentProgramId,
                        //        IsHistoricalProgram = planDayEntity.IsHistoricalProgram
                        //    });
                        //}
                    }

                }
            }

            var response = new ActiveTrainingPlanResponse
            {
                TrainingPlanId = activePlanEntity.TrainingPlanId,
                TraineeId = activePlanEntity.TraineeId,
                TraineeName = activePlanEntity.Trainee?.TraineeName ?? "מתאמן",
                GoalId = activePlanEntity.GoalId,
                TrainingDays = activePlanEntity.TrainingDays,
                TrainingDurationId = activePlanEntity.TrainingDurationId,
                FitnessLevelId = activePlanEntity.FitnessLevelId,
                StartDate = activePlanEntity.StartDate,
                EndDate = activePlanEntity.EndDate,
                IsActive = activePlanEntity.IsActive,
                PlanDays = planDaysForFrontend
            };

            return response;
        }


        //public async Task<ActiveTrainingPlanResponse?> GetActiveTrainingPlanForTrainee(int traineeId)
        //{
        //    // 1. קבל את התוכנית הפעילה ופרטיה מה-DAL
        //    var activePlanEntity = await .GetActiveTrainingPlanWithDetails(traineeId);

        //    if (activePlanEntity == null)
        //    {
        //        return null; // אין תוכנית פעילה
        //    }

        //    // 2. קבל את היסטוריית האימונים שהושלמו השבוע מה-DAL
        //    var startOfWeek = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek);
        //    var completedPlanDayIdsThisWeek = await _trainingPlanRepository.GetCompletedPlanDayIdsThisWeek(traineeId, startOfWeek);

        //    // 3. בנה את רשימת ימי האימון עבור ה-Frontend, וסנן החוצה את אלו שהושלמו
        //    var planDaysForFrontend = new List<PlanDayResponseForFrontend>();

        //    // וודא ש-activePlanEntity.PlanDays אינו null לפני הלולאה
        //    if (activePlanEntity.PlanDays != null)
        //    {
        //        foreach (var planDayEntity in activePlanEntity.PlanDays.OrderBy(pd => pd.DayOrder))
        //        {
        //            bool isCompletedThisWeek = completedPlanDayIdsThisWeek.Contains(planDayEntity.PlanDayId);

        //            // אתה רוצה רק ימי אימון שעדיין לא הושלמו השבוע
        //            if (!isCompletedThisWeek)
        //            {
        //                planDaysForFrontend.Add(new PlanDayResponseForFrontend
        //                {
        //                    PlanDayId = planDayEntity.PlanDayId,
        //                    ProgramName = planDayEntity.ProgramName,
        //                    DayOrder = planDayEntity.DayOrder,
        //                    IsDefaultProgram = planDayEntity.IsDefaultProgram,
        //                    IsCompletedThisWeek = false, // יהיה false כי סיננו החוצה את אלו שהושלמו
        //                    // הוסף את השדות הנוספים שתרצה ב-Frontend DTO
        //                    TrainingPlanId = planDayEntity.TrainingPlanId,
        //                    CreationDate = planDayEntity.CreationDate,
        //                    ParentProgramId = planDayEntity.ParentProgramId,
        //                    IsHistoricalProgram = planDayEntity.IsHistoricalProgram
        //                });
        //            }
        //        }
        //    }

        //    // 4. בנה את אובייקט התגובה הסופי עבור ה-Frontend
        //    var response = new ActiveTrainingPlanResponse
        //    {
        //        TrainingPlanId = activePlanEntity.TrainingPlanId,
        //        TraineeId = activePlanEntity.TraineeId,
        //        TraineeName = activePlanEntity.Trainee?.TraineeName ?? "מתאמן",
        //        GoalId = activePlanEntity.GoalId,
        //        TrainingDays = activePlanEntity.TrainingDays,
        //        TrainingDurationId = activePlanEntity.TrainingDurationId,
        //        FitnessLevelId = activePlanEntity.FitnessLevelId,
        //        StartDate = activePlanEntity.StartDate,
        //        EndDate = activePlanEntity.EndDate,
        //        IsActive = activePlanEntity.IsActive,
        //        PlanDays = planDaysForFrontend // רשימת ימי האימון המסוננת
        //    };

        //    return response;
        //}
    }
}



//cfg.CreateMap<ExerciseStatusEntry, ExerciseEntry>()
//   .ForMember(dest => dest.OriginalExercisePlanId, opt => opt.MapFrom(src => src.OriginalExercisePlanId))
//   .ForMember(dest => dest.ExerciseId, opt => opt.MapFrom(src => src.ExerciseId))
//   .ForMember(dest => dest.OrderInList, opt => opt.MapFrom(src => src.OrderInList))
//   .ForMember(dest => dest.IsDone, opt => opt.MapFrom(src => src.IsDone))
//   .ForMember(dest => dest.StartedAt, opt => opt.MapFrom(src => src.StartedAt))
//   .ForMember(dest => dest.PerformedAt, opt => opt.MapFrom(src => src.PerformedAt))
//   //.ForMember(dest => dest.Sets, opt => opt.MapFrom(src => src.Sets))
//   //.ForMember(dest => dest.Reps, opt => opt.MapFrom(src => src.Reps))
//   //.ForMember(dest => dest.RestTime, opt => opt.MapFrom(src => src.RestTime))
//   .ForMember(dest => dest.ExerciseDetails, opt => opt.MapFrom(src => src.ExerciseDetails)); // מיפוי ישיר של ה-DTO

// מפה מה-PathResultExerciseEntry של ה-Scheduler ל-ExerciseStatusEntry
//cfg.CreateMap<PathResultExerciseEntry, ExerciseStatusEntry>()
//   .ForMember(dest => dest.OriginalExercisePlanId, opt => opt.MapFrom(src => src.OriginalExercisePlanId))
//   .ForMember(dest => dest.ExerciseId, opt => opt.MapFrom(src => src.ExerciseId))
//   .ForMember(dest => dest.OrderInList, opt => opt.MapFrom(src => src.OrderInList))
//   .ForMember(dest => dest.IsDone, opt => opt.Ignore()) // אלו יוגדרו בלוגיקה
//   .ForMember(dest => dest.StartedAt, opt => opt.Ignore())
//   .ForMember(dest => dest.PerformedAt, opt => opt.Ignore())
//   .ForMember(dest => dest.Sets, opt => opt.Ignore()) // יוגדרו בלוגיקה מאוחר יותר
//   .ForMember(dest => dest.Reps, opt => opt.Ignore())
//   .ForMember(dest => dest.RestTime, opt => opt.Ignore())
//   .ForMember(dest => dest.TimesMax, opt => opt.Ignore())
//   .ForMember(dest => dest.TimesMin, opt => opt.Ignore())
//   .ForMember(dest => dest.ExerciseDetails, opt => opt.Ignore());
