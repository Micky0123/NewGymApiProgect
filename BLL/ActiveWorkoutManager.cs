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

    public class ActiveWorkoutManager 

    {
        private BacktrackingScheduler scheduler;
        public bool IsInitialized => _cache.TryGetValue("Scheduler", out BacktrackingScheduler existing) && existing != null; // עדכון לשימוש ב-cache

        private readonly ITraineeBLL _traineeBLL;
        private readonly IPlanDayDAL _planDayDAL; 
        private readonly IExercisePlanDAL _exercisePlanDAL; 
        private readonly IMapper _mapper;
        private readonly IMemoryCache _cache;
        private readonly ITrainingPlanDAL _trainingPlan;
        

        private readonly SemaphoreSlim _startWorkoutLock = new SemaphoreSlim(1, 1);

        // זה מאפשר גישה מהירה לכל האימונים הפעילים.
        private readonly ConcurrentDictionary<int, TraineeExerciseStatus> _activeWorkoutsInternal = new ConcurrentDictionary<int, TraineeExerciseStatus>();

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
            _planDayDAL = planDayDAL; 
            _exercisePlanDAL = exercisePlanDAL;
            _trainingPlan = trainingPlanDAL;
            _mapper = mapper; 
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

            this.scheduler = new BacktrackingScheduler(_traineeBLL); 
            this.scheduler.Initialize( 
                exerciseList, exerciseEdges, exerciseToMuscleEdges, muscleEdges,
                equipmentCountByExercise, firstSlotStart, slotMinutes, slotCount
            );
            _cache.Set("Scheduler", this.scheduler); 
        }

        private BacktrackingScheduler GetScheduler()
        {
            if (!_cache.TryGetValue("Scheduler", out BacktrackingScheduler scheduler) || scheduler == null)
                throw new Exception("Scheduler is not initialized!");
            return scheduler;
        }

        public void ResetScheduler()
        {
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
            var scheduler = GetScheduler(); 
            bool enteredLock = false; 

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
                var pathResult = await scheduler.FindOptimalPath(trainee, exerciseOrder, startTime);

                if (pathResult == null)
                    throw new Exception("לא נמצא מסלול מתאים עבור מתאמן זה.");

                // בניית TraineeExerciseStatus מה-PathResult
                var exercisesStatus = new List<ExerciseStatusEntry>();
                foreach (var pair in pathResult.ExerciseIdsInPath.OrderBy(p => p.Value.OrderInList))
                {
                    var exerciseEntryFromScheduler = pair.Value;
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
                _activeWorkoutsInternal.TryAdd(trainee.TraineeId, traineeWorkoutStatus); 

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
            _cache.Set($"Trainee_{traineeId}", traineeStatus);
            _activeWorkoutsInternal.AddOrUpdate(traineeId, traineeStatus, (key, oldValue) => traineeStatus); // מעדכן גם במילון הפנימי
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
                _activeWorkoutsInternal.TryRemove(traineeId, out _); // הסר ממילון האימונים הפעילים
                Console.WriteLine($"Workout for Trainee {traineeId} completed and saved.");
            }
            // אם האימון לא הושלם, ה-TraineeExerciseStatus נשאר במטמון עם הסטטוס המעודכן
            else
            {
                // אם האימון לא הושלם, עדכן את מצבו במטמון ובמילון הפנימי
                _cache.Set($"Trainee_{traineeId}", traineeStatus);
                _activeWorkoutsInternal.AddOrUpdate(traineeId, traineeStatus, (key, oldValue) => traineeStatus);
            }
            return true;
        }


        public PathResult GetUpdatedWorkoutPlan(int traineeId)
        {
            if (_cache.TryGetValue($"Trainee_{traineeId}", out TraineeExerciseStatus traineeStatus) && traineeStatus != null)
            {
                return MapTraineeStatusToPathResultDTO(traineeStatus);
            }
            return null; // או זרוק Exception אם המתאמן לא נמצא
        }

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
            var NextExercise = _mapper.Map<ExerciseEntry>(nextExerciseEntry); 
            return new NextExerciseResponse
            {
                TraineeId = traineeId,
                NextExercise = _mapper.Map<ExerciseEntry>(nextExerciseEntry), 
                IsWorkoutComplete = false,
                RemainingExercisesCount = traineeStatus.Exercises.Count(e => !e.IsDone) - 1, // כמה נותרו (לא כולל הנוכחי)
            };
        }

        // --- לוגיקה למיפוי ושמירה למסד הנתונים ---
        private async Task SaveWorkoutToDatabase(TraineeExerciseStatus status)
        {
            PlanDay planDay = await _planDayDAL.GetPlanDayByIdAsync(status.planDayId);
            if (planDay == null)
            {
                Console.WriteLine($"Error: Original PlanDay with ID {status.planDayId} not found for historical save.");
                planDay = new PlanDay { PlanDayId = 0, ProgramName = "Unknown Original Plan", CreationDate = DateTime.Now }; 
            }

            var planDayDto = _mapper.Map<PlanDayDTO>(planDay);

            var newPlanDay = new PlanDayDTO()
            {
                TrainingPlanId = planDayDto.TrainingPlanId,
                ProgramName = "Workout History: ",
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
                    TrainingDateTime = (DateTime)(exerciseStatusEntry.PerformedAt ?? exerciseStatusEntry.StartedAt), 
                    IndexOrder = exerciseStatusEntry.OrderInList,
                    Exercise=exerciseStatusEntry.Plan.Exercise,
                };
                await _exercisePlanDAL.AddExercisePlanAsync(_mapper.Map<ExercisePlan>(exercisePlan));
            }
        }

        // --- פונקציות עזר: המרה מ-TraineeExerciseStatus ל-PathResultDTO ---
        private PathResult MapTraineeStatusToPathResultDTO(TraineeExerciseStatus status)
        {
            return _mapper.Map<PathResult>(status);
        }


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
                        var lastCompletedChild = await _planDayDAL.GetLastCompletedHistoricalPlanDayForParentAndTrainingPlanAsync(
                            planDayEntity.PlanDayId, activePlanEntity.TrainingPlanId);

                        if ((lastCompletedChild != null && lastCompletedChild.CreationDate <= startOfWeek)||(lastCompletedChild == null))
                        {
                            // הוסף רק ימי אימון שעדיין לא הושלמו השבוע
                            planDaysForFrontend.Add(new PlanDayResponseForFrontend
                            {
                                PlanDayId = planDayEntity.PlanDayId,
                                ProgramName = planDayEntity.ProgramName,
                                DayOrder = planDayEntity.DayOrder,
                                IsDefaultProgram = planDayEntity.IsDefaultProgram,
                                IsCompletedThisWeek = false, // תמיד יהיה false כי סיננו החוצה את אלו שהושלמו
                                TrainingPlanId = planDayEntity.TrainingPlanId,
                                CreationDate = planDayEntity.CreationDate,
                                ParentProgramId = planDayEntity.ParentProgramId,
                                IsHistoricalProgram = planDayEntity.IsHistoricalProgram
                            });
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


        public List<PathResult> GetAllActiveWorkouts()
        {
            var activeWorkoutsList = new List<PathResult>();
            foreach (var workoutStatus in _activeWorkoutsInternal.Values)
            {
                activeWorkoutsList.Add(MapTraineeStatusToPathResultDTO(workoutStatus));
            }
            return activeWorkoutsList;
        }
        public  async Task<List<TraineeDTO>> GetAllActiveTraineesId()
        {
            var activeTrainee =new List<TraineeDTO>();
            var trainee = await _traineeBLL.GetAllTraineesAsync();
            foreach (var item in trainee)
            {
                if (_cache.TryGetValue($"Trainee_{item.TraineeId}", out TraineeExerciseStatus traineeStatus) && traineeStatus != null)
                {
                    activeTrainee.Add(item);
                }
            }
            return activeTrainee;
        }


    }
}


