﻿using AutoMapper;
using DAL;
using DBEntities.Models;
using DocumentFormat.OpenXml.Office2010.Excel;
using DTO;
using IBLL;
using IDAL;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{

    public class ActiveWorkoutManager
    {
        private BacktrackingScheduler scheduler;
        public bool IsInitialized => scheduler != null;

        private Dictionary<int, TraineeExerciseStatus> activeTrainees;
       // private SchedulerManager scheduler1;
        private readonly IPlanDayDAL planDayDAL;
        private readonly IExercisePlanDAL exercisePlanDAL;
        private readonly IMapper mapper;
        private readonly SemaphoreSlim _startWorkoutLock = new SemaphoreSlim(1, 1);

        private readonly ITraineeBLL _traineeBLL;
        private readonly IPlanDayDAL _planDayDAL;
        private readonly IExercisePlanDAL _exercisePlanDAL;
        //private readonly IMapper _mapper;
        //private BacktrackingScheduler _scheduler;
        private readonly IMemoryCache _cache;
      

        public ActiveWorkoutManager(
            IMemoryCache cache,
            ITraineeBLL traineeBLL,
            IPlanDayDAL planDayDAL,
            IExercisePlanDAL exercisePlanDAL)
        {
            _cache = cache;
            this._traineeBLL = traineeBLL;
            this.planDayDAL = planDayDAL;
            this.exercisePlanDAL = exercisePlanDAL;
           // this.mapper = mapper;

            activeTrainees = new Dictionary<int, TraineeExerciseStatus>();

            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<PlanDay, PlanDayDTO>().ReverseMap();
                cfg.CreateMap<ExercisePlan, ExercisePlanDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);

        }
        //public ActiveWorkoutManager(
        //    ITraineeBLL traineeBLL,
        //    List<ExerciseDTO> exerciseList,
        //    List<GraphEdgeDTO> exerciseEdges,
        //    List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
        //    List<MuscleEdgeDTO> muscleEdges,
        //    Dictionary<int, int> equipmentCountByExercise,
        //    int slotMinutes,
        //    int slotCount,
        //    DateTime firstSlotStart,
        //    IPlanDayDAL planDayDAL,
        //    IExercisePlanDAL exercisePlanDAL)
        //{
        //    activeTrainees = new Dictionary<int, TraineeExerciseStatus>();

        //    scheduler = new BacktrackingScheduler(traineeBLL);

        //    this.planDayDAL = planDayDAL;
        //    this.exercisePlanDAL = exercisePlanDAL;
        //    var configTaskConverter = new MapperConfiguration(cfg =>
        //    {
        //        cfg.CreateMap<PlanDay, PlanDayDTO>().ReverseMap();
        //        cfg.CreateMap<ExercisePlan, ExercisePlanDTO>().ReverseMap();
        //    });
        //    mapper = new Mapper(configTaskConverter);

        //    scheduler.Initialize(
        //        exerciseList: exerciseList,
        //        exerciseEdges: exerciseEdges,
        //        exerciseToMuscleEdges: exerciseToMuscleEdges,
        //        muscleEdges: muscleEdges,
        //        equipmentCountByExercise: equipmentCountByExercise,
        //        firstSlotStart: firstSlotStart,
        //        slotMinutes: slotMinutes,
        //        slotCount: slotCount
        //    );
        //}
        //public void Initialize(
        //    List<ExerciseDTO> exerciseList,
        //    List<GraphEdgeDTO> exerciseEdges,
        //    List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
        //    List<MuscleEdgeDTO> muscleEdges,
        //    Dictionary<int, int> equipmentCountByExercise,
        //    DateTime firstSlotStart,
        //    int slotMinutes,
        //    int slotCount)
        //{
        //    scheduler = new BacktrackingScheduler(_traineeBLL);
        //    scheduler.Initialize(
        //        exerciseList, exerciseEdges, exerciseToMuscleEdges, muscleEdges,
        //        equipmentCountByExercise, firstSlotStart, slotMinutes, slotCount
        //    );
        //}

        //public void Initialize(
        //      List<ExerciseDTO> exerciseList,
        //      List<GraphEdgeDTO> exerciseEdges,
        //      List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
        //      List<MuscleEdgeDTO> muscleEdges,
        //      Dictionary<int, int> equipmentCountByExercise,
        //      DateTime firstSlotStart,
        //      int slotMinutes,
        //      int slotCount)
        //{
        //    scheduler.Initialize(
        //        exerciseList: exerciseList,
        //        exerciseEdges: exerciseEdges,
        //        exerciseToMuscleEdges: exerciseToMuscleEdges,
        //        muscleEdges: muscleEdges,
        //        equipmentCountByExercise: equipmentCountByExercise,
        //        firstSlotStart: firstSlotStart,
        //        slotMinutes: slotMinutes,
        //        slotCount: slotCount
        //    );
        //}

        //public void Initialize(
        //   List<ExerciseDTO> exerciseList,
        //   List<GraphEdgeDTO> exerciseEdges,
        //   List<DeviceMuscleEdgeDTO> exerciseToMuscleEdges,
        //   List<MuscleEdgeDTO> muscleEdges,
        //   Dictionary<int, int> equipmentCountByExercise,
        //   DateTime firstSlotStart,
        //   int slotMinutes,
        //   int slotCount)
        //{
        //    if (IsInitialized)
        //        throw new Exception("Scheduler already initialized!");

        //    scheduler = new BacktrackingScheduler(_traineeBLL);
        //    scheduler.Initialize(
        //        exerciseList, exerciseEdges, exerciseToMuscleEdges, muscleEdges,
        //        equipmentCountByExercise, firstSlotStart, slotMinutes, slotCount
        //    );
        //}

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

            var scheduler = new BacktrackingScheduler(_traineeBLL);
            scheduler.Initialize(
                exerciseList, exerciseEdges, exerciseToMuscleEdges, muscleEdges,
                equipmentCountByExercise, firstSlotStart, slotMinutes, slotCount
            );
            _cache.Set("Scheduler", scheduler);
        }

        // הפונקציה החדשה:
        private BacktrackingScheduler GetScheduler()
        {
            if (!_cache.TryGetValue("Scheduler", out BacktrackingScheduler scheduler) || scheduler == null)
                throw new Exception("Scheduler is not initialized!");
            return scheduler;
        }


        public void ResetScheduler()
        {
            scheduler = null;
        }

        // פונקציה להדפסת מטריצת המעבר של ה-BacktrackingScheduler
        public void PrintSchedulerMatrix()
        {
            var scheduler = GetScheduler();
            scheduler.PrintTransitionMatrixToConsole();
        }

        // קריאה לאלגוריתם והתחלת אימון
        public async Task StartWorkoutAsync(TraineeDTO trainee, List<ExercisePlanDTO> exerciseOrder, DateTime startTime, int planDayId)
        {
            var scheduler = GetScheduler();
            // ננסה לקבל מיד את המנעול, ואם לא מצליחים נדפיס הודעה ונחכה
            if (!await _startWorkoutLock.WaitAsync(0))
            {
                Console.WriteLine("המערכת מחשבת נתונים");
                await _startWorkoutLock.WaitAsync(); // מחכים עד שהמנעול ישתחרר
            }
           // await _startWorkoutLock.WaitAsync();
            try
            {
                var pathResult = await scheduler.FindOptimalPath(trainee, exerciseOrder, startTime);

                if (pathResult == null)
                    throw new Exception("לא נמצא מסלול מתאים עבור מתאמן זה.");

                // בניית סטטוס תרגילים מה-PathResult
                var exercisesStatus = pathResult.ExerciseIdsInPath
                    .OrderBy(pair => pair.Value.OrderInList)
                    .Select(pair => new ExerciseStatusEntry
                    {
                        OriginalExercise = pair.Key,
                        ExerciseId = pair.Value.ExerciseId,
                        OrderInList = pair.Value.OrderInList,
                        IsDone = false,
                        PerformedAt = null,
                        StartedAt = null
                    }).ToList();

                //activeTrainees[trainee.TraineeId] = new TraineeExerciseStatus
                //{
                //    Trainee = trainee,
                //    Exercises = exercisesStatus,
                //    planDayId = planDayId
                //};
                _cache.Set($"Trainee_{trainee.TraineeId}", new TraineeExerciseStatus
                {
                    Trainee = trainee,
                    Exercises = exercisesStatus,
                    planDayId = planDayId
                });

            }
            finally
            {
                _startWorkoutLock.Release();
            }
        }

        // קריאה להתחלת תרגיל עבור מתאמן
        public bool StartExercise(int traineeId, int exerciseId, DateTime startTime)
        {
            //if (!activeTrainees.TryGetValue(traineeId, out var traineeStatus))
            //    throw new Exception("Trainee not found");
            if (!_cache.TryGetValue($"Trainee_{traineeId}", out TraineeExerciseStatus traineeStatus) || traineeStatus == null)
                throw new Exception("Trainee not found");
            var scheduler = GetScheduler();

            var exercise = traineeStatus.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
            //var exercise = traineeStatus.Exercises
            //    .Where(e => !e.IsDone)
            //    .OrderBy(e => e.OrderInList)
            //    .FirstOrDefault();
            if (exercise == null)
                throw new Exception("Exercise not found for this trainee");

            exercise.StartedAt = startTime;
            return true;
        }

        // קריאה להתחלת תרגיל עבור מתאמן
        public bool CompleteExercise(int traineeId, int exerciseId, DateTime endTime)
        {
            //if (!activeTrainees.TryGetValue(traineeId, out var traineeStatus))
            //    throw new Exception("Trainee not found");
            if (!_cache.TryGetValue($"Trainee_{traineeId}", out TraineeExerciseStatus traineeStatus) || traineeStatus == null)
                throw new Exception("Trainee not found");
            var scheduler = GetScheduler();

            var exercise = traineeStatus.Exercises.FirstOrDefault(e => e.ExerciseId == exerciseId);
            if (exercise == null)
                throw new Exception("Exercise not found for this trainee");

            exercise.IsDone = true;
            exercise.PerformedAt = endTime;

            if (traineeStatus.Exercises.All(e => e.IsDone))
            {
                SaveWorkoutToDatabase(traineeStatus);
                // ניתן למחוק מכאן את המתאמן כעת
                activeTrainees.Remove(traineeId);
                _cache.Remove($"Trainee_{traineeId}");
            }
            return true;
        }

        // לוגיקה למיפוי ושמירה למסד הנתונים שלך
        private async Task SaveWorkoutToDatabase(TraineeExerciseStatus status)
        {
            var scheduler = GetScheduler();

            PlanDay planDay = await planDayDAL.GetPlanDayByIdAsync(status.planDayId);
            var planDayDto = mapper.Map<PlanDayDTO>(planDay);

            // דוגמה: המרה ל-PlanDay, ExercisePlan ושמירה ב-DB
            var NewplanDay = new PlanDayDTO()
            {
                //PlanDayId = 0, // או ID חדש שיתקבל מהמסד
                TrainingPlanId = planDayDto.TrainingPlanId, // או ID של תוכנית האימון המתאימה
                ProgramName = "Workout Plan",
                DayOrder = planDayDto.DayOrder, // או סדר היום המתאים
                CreationDate = DateTime.Now,
                IsDefaultProgram = false,
                ParentProgramId = planDayDto.PlanDayId,
                IsHistoricalProgram = false
            };
            // שמירת ה-PlanDay
            var savedPlanDay = await planDayDAL.AddPlanDayAsync(mapper.Map<PlanDay>(NewplanDay));

            foreach (var exercise in status.Exercises)
            {
                var OrigenExercisePlan = await exercisePlanDAL.GetExercisePlanByIdAsync(exercise.OriginalExercise);
                // שמירת ה-ExercisePlan
                var exercisePlan = new ExercisePlanDTO()
                {
                    //ExercisePlanId = 0, // או ID חדש שיתקבל מהמסד
                    ExerciseId = exercise.ExerciseId,
                    PlanDayId = status.planDayId,
                    TimesMax = OrigenExercisePlan.TimesMax,
                    TimesMin = OrigenExercisePlan.TimesMin,
                    PlanRepetitionsMax = OrigenExercisePlan.TimesMax,
                    PlanRepetitionsMin = OrigenExercisePlan.TimesMin,
                    PlanSets = OrigenExercisePlan.PlanSets,
                    PlanWeight = OrigenExercisePlan.PlanWeight,
                    CategoryId = OrigenExercisePlan.CategoryId,
                    SubMuscleId = OrigenExercisePlan.SubMuscleId,
                    TrainingDateTime = DateTime.Now,
                    IndexOrder = exercise.OrderInList,
                };

                await exercisePlanDAL.AddExercisePlanAsync(mapper.Map<ExercisePlan>(exercisePlan));
            }


        }

    }

}
