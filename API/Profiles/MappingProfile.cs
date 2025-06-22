using AutoMapper;
using DBEntities.Models;
using DTO;

namespace API.Profiles
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Muscle, MuscleDTO>().ReverseMap();
            CreateMap<TrainingPlan, TrainingPlanDTO>().ReverseMap();

            CreateMap<TraineeDTO, Trainee>()
                .ForMember(dest => dest.TraineeId, opt => opt.Ignore()); // מתעלם מהמיפוי של TraineeId
            CreateMap<DeviceMuscleEdge, DeviceMuscleEdgeDTO>();

            CreateMap< ExerciseEntry,ExerciseStatusEntry > ();
            CreateMap<PlanDay, PlanDayDTO>(); 
            CreateMap<PlanDayDTO, PlanDay>();
            //CreateMap<ExercisePlan, ExercisePlanDTO>();
            //CreateMap<ExercisePlanDTO, ExercisePlan>();
            // מיפוי מ-ExercisePlan (מודל DB) ל-ExercisePlanDTO
            CreateMap<ExercisePlan, ExercisePlanDTO>()
                .ForMember(dest => dest.Exercise, opt => opt.MapFrom(src => src.Exercise)); // AutoMapper ישתמש במיפוי Exercise -> ExerciseDTO שהוגדר למעלה

            // מיפוי מ-ExercisePlanDTO ל-ExercisePlan (מודל DB)
            CreateMap<ExercisePlanDTO, ExercisePlan>()
                .ForMember(dest => dest.Exercise, opt => opt.Ignore()); // במיפוי מ-DTO ל-DB, לרוב לא נרצה ליצור או לעדכן את אובייקט ה-Exercise המלא.
                                                                        // אם תצטרך לשייך את ה-Exercise על בסיס ה-ExerciseId בלבד, AutoMapper יעשה זאת אוטומטית אם שמות המאפיינים תואמים.
                                                                        // אם אתה רוצה למפות את ה-ExerciseId ספציפית:
                                                                        // .ForMember(dest => dest.ExerciseId, opt => opt.MapFrom(src => src.Exercise.ExerciseId));

            // מיפויים של ExerciseEntry ו-ExerciseStatusEntry
            //CreateMap<ExerciseStatusEntry, ExerciseEntry>()
            //    .ForMember(dest => dest.ExerciseId, opt => opt.MapFrom(src => src.ExerciseId))
            //    .ForMember(dest => dest.OrderInList, opt => opt.MapFrom(src => src.OrderInList))
            //    .ForMember(dest => dest.Slots, opt => opt.MapFrom(src => src.Slots))
            //    .ForMember(dest => dest.OriginalExercise, opt => opt.MapFrom(src => src.OriginalExercise))
            //    .ForMember(dest => dest.IsDone, opt => opt.MapFrom(src => src.IsDone))
            //    .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartedAt))
            //    .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.PerformedAt))
            //    .ForMember(dest => dest.ExerciseDetails, opt => opt.MapFrom(src => src.Plan)); // מיפוי עבור ExercisePlanDTO



            // *** זהו המיפוי החסר שגרם לשגיאה ***
            // מיפוי TraineeExerciseStatus ל-PathResult
            CreateMap<TraineeExerciseStatus, PathResult>()
                .ForMember(
                    dest => dest.ExerciseIdsInPath,
                    opt => opt.MapFrom(src =>
                        src.Exercises.OrderBy(e => e.OrderInList)
                                     .Select((entry, index) => new { entry, index })
                                     .ToDictionary(x => x.index, x => x.entry)
                    )
                )
                .ForMember(dest => dest.Trainee, opt => opt.MapFrom(src => src.Trainee)) // מפה את TraineeDTO
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.WorkoutStartTime))
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.WorkoutEndTime))
                .ForMember(dest => dest.AlternativesUsed, opt => opt.Ignore()) // או הגדר ערך ברירת מחדל אם הוא לא קיים ב-source
                .ForMember(dest => dest.CurrentExerciseOrderIndex, opt => opt.MapFrom(src => src.CurrentExerciseOrderIndex))
                .ForMember(dest => dest.IsWorkoutComplete, opt => opt.MapFrom(src => src.Exercises.All(e => e.IsDone)));
            // אם Trainee הוא TraineeDTO ב-TraineeExerciseStatus, אז תצטרכי גם מיפוי של TraineeDTO ל-TraineeDTO (שזה לא עושה כלום אבל מונע שגיאות אם שדה ה-Trainee הוא אובייקט מסוג שונה).
            CreateMap<TraineeDTO, TraineeDTO>(); // מניח ש-Trainee הוא כבר DTO בפנים

            // ודא שיש לך מיפוי מ-ExerciseStatusEntry ל-ExerciseEntry
            // מיפוי מ-ExerciseStatusEntry (מקור) ל-ExerciseEntry (יעד - DTO)
            CreateMap<ExerciseStatusEntry, ExerciseEntry>()
                .ForMember(dest => dest.ExerciseId, opt => opt.MapFrom(src => src.ExerciseId))
                .ForMember(dest => dest.OrderInList, opt => opt.MapFrom(src => src.OrderInList))
                .ForMember(dest => dest.Slots, opt => opt.MapFrom(src => src.Slots))
                .ForMember(dest => dest.OriginalExercise, opt => opt.MapFrom(src => src.OriginalExercise))
                .ForMember(dest => dest.IsDone, opt => opt.MapFrom(src => src.IsDone))

                // --- טיפול בשדות התאריך השונים ---
                // StartedAt ב-ExerciseStatusEntry ממפה ל-StartTime ב-ExerciseEntry
                .ForMember(dest => dest.StartTime, opt => opt.MapFrom(src => src.StartedAt))

                // PerformedAt ב-ExerciseStatusEntry ממפה ל-EndTime ב-ExerciseEntry
                .ForMember(dest => dest.EndTime, opt => opt.MapFrom(src => src.PerformedAt))
                // **** מיפוי השדות החדשים ****
                // נניח ש-ExerciseStatusEntry מכיל את ExercisePlanDTO
                // אם הוא לא מכיל, תצטרך לטעון את זה ב-BeforeMap או ב-ResolveUsing
                //.ForMember(dest => dest.ExercisePlanDetails, opt => opt.MapFrom(src => src.ExercisePlanDetails)) // אם ExerciseStatusEntry מכיל כבר את ה-DTO הזה
                .ForMember(dest => dest.ExerciseDetails, opt => opt.MapFrom(src => src.Plan)) // אם ExerciseStatusEntry מכיל כבר את ה-DTO הזה
                ;
            // אם יש לך ExerciseDetails ב-ExerciseEntry ואתה צריך למפות אותו, ודא שיש מיפוי נפרד לזה.
            // לדוגמה, אם ExerciseEntry מכיל ExerciseDetailsDTO
            // .ForMember(dest => dest.ExerciseDetails, opt => opt.MapFrom(src => src.ExerciseDetails))
            CreateMap< ExerciseEntry, ExerciseStatusEntry>()
             .ForMember(dest => dest.ExerciseId, opt => opt.MapFrom(src => src.ExerciseId))
             .ForMember(dest => dest.OrderInList, opt => opt.MapFrom(src => src.OrderInList))
             .ForMember(dest => dest.Slots, opt => opt.MapFrom(src => src.Slots))
             .ForMember(dest => dest.OriginalExercise, opt => opt.MapFrom(src => src.OriginalExercise))
             .ForMember(dest => dest.IsDone, opt => opt.MapFrom(src => src.IsDone))

             // --- טיפול בשדות התאריך השונים ---
             // StartedAt ב-ExerciseStatusEntry ממפה ל-StartTime ב-ExerciseEntry
             .ForMember(dest => dest.StartedAt, opt => opt.MapFrom(src => src.StartTime))

             // PerformedAt ב-ExerciseStatusEntry ממפה ל-EndTime ב-ExerciseEntry
             .ForMember(dest => dest.PerformedAt, opt => opt.MapFrom(src => src.EndTime))
             // **** מיפוי השדות החדשים ****
             // נניח ש-ExerciseStatusEntry מכיל את ExercisePlanDTO
             // אם הוא לא מכיל, תצטרך לטעון את זה ב-BeforeMap או ב-ResolveUsing
             //.ForMember(dest => dest.ExercisePlanDetails, opt => opt.MapFrom(src => src.ExercisePlanDetails)) // אם ExerciseStatusEntry מכיל כבר את ה-DTO הזה
             .ForMember(dest => dest.Plan, opt => opt.MapFrom(src => src.ExerciseDetails)) // אם ExerciseStatusEntry מכיל כבר את ה-DTO הזה
             ;

            CreateMap<RegisterRequest, TraineeDTO>()
           .ForMember(dest => dest.TraineeId, opt => opt.Ignore()) // תן ל-DB להקצות ID
           .ForMember(dest => dest.LoginDateTime, opt => opt.Ignore()) // נעדכן ידנית
           .ForMember(dest => dest.IsAdmin, opt => opt.Ignore()) // נעדכן ידנית
                                                                 // עבור Gender: AutoMapper אמור למפות אוטומטית int ל-Enum אם הערכים תואמים.
                                                                 // אם לא, אפשר להוסיף:
           .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => (TraineeDTO.EGender)src.Gender));


            // מיפוי מ-Exercise (מודל DB) ל-ExerciseDTO
            CreateMap<Exercise, ExerciseDTO>()
                .ForMember(dest => dest.ExerciseId, opt => opt.MapFrom(src => src.ExerciseId))
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.ExerciseName))
                .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active))
                .ForMember(dest => dest.MuscleId, opt => opt.MapFrom(src => src.MuscleId))
                .ForMember(dest => dest.MuscleTypeId, opt => opt.MapFrom(src => src.MuscleTypeId))
                .ForMember(dest => dest.MuscleGroupId, opt => opt.MapFrom(src => src.MuscleGroupId))
                .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count));
            // אם יש לך שדות נוספים ב-ExerciseDTO, הוסף אותם כאן (לדוגמה, Duration אם קיים)

            // מיפוי מ-ExerciseDTO ל-Exercise (מודל DB)
            CreateMap<ExerciseDTO, Exercise>()
                .ForMember(dest => dest.ExerciseId, opt => opt.MapFrom(src => src.ExerciseId))
                .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.ExerciseName))
                .ForMember(dest => dest.Active, opt => opt.MapFrom(src => src.Active))
                .ForMember(dest => dest.MuscleId, opt => opt.MapFrom(src => src.MuscleId))
                .ForMember(dest => dest.MuscleTypeId, opt => opt.MapFrom(src => src.MuscleTypeId))
                .ForMember(dest => dest.MuscleGroupId, opt => opt.MapFrom(src => src.MuscleGroupId))
                .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.Count))
                // ישויות ניווט (Navigation Properties) – לרוב נתעלם מהן במיפוי חזרה ל-DB.
                // AutoMapper לא אמור ליצור ישויות מקושרות חדשות כברירת מחדל מ-DTO.
                .ForMember(dest => dest.Muscle, opt => opt.Ignore())
                .ForMember(dest => dest.MuscleGroup, opt => opt.Ignore())
                .ForMember(dest => dest.MuscleType, opt => opt.Ignore())
                .ForMember(dest => dest.DeviceMuscleEdges, opt => opt.Ignore())
                .ForMember(dest => dest.ExercisePlans, opt => opt.Ignore())
                .ForMember(dest => dest.GraphEdgeDevice1s, opt => opt.Ignore())
                .ForMember(dest => dest.GraphEdgeDevice2s, opt => opt.Ignore())
                .ForMember(dest => dest.Categories, opt => opt.Ignore())
                .ForMember(dest => dest.Equipment, opt => opt.Ignore())
                .ForMember(dest => dest.Joints, opt => opt.Ignore())
                .ForMember(dest => dest.SubMuscles, opt => opt.Ignore());

        }

    }
}
