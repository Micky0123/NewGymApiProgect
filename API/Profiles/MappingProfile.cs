using AutoMapper;
using DBEntities.Models;
using DTO;

namespace API.Profiles
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TraineeDTO, Trainee>()
                .ForMember(dest => dest.TraineeId, opt => opt.Ignore()); // מתעלם מהמיפוי של TraineeId
            CreateMap<DeviceMuscleEdge, DeviceMuscleEdgeDTO>();

           // CreateMap<ExerciseStatusEntry, ExerciseEntry>();
            CreateMap< ExerciseEntry,ExerciseStatusEntry > ();
            CreateMap<PlanDay, PlanDayDTO>(); 
            CreateMap<PlanDayDTO, PlanDay>();
            CreateMap<ExercisePlan, ExercisePlanDTO>();
            CreateMap<ExercisePlanDTO, ExercisePlan>();

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

                // אם יש לך ExerciseDetails ב-ExerciseEntry ואתה צריך למפות אותו, ודא שיש מיפוי נפרד לזה.
                // לדוגמה, אם ExerciseEntry מכיל ExerciseDetailsDTO
                // .ForMember(dest => dest.ExerciseDetails, opt => opt.MapFrom(src => src.ExerciseDetails))
                ;

            CreateMap<RegisterRequest, TraineeDTO>()
           .ForMember(dest => dest.TraineeId, opt => opt.Ignore()) // תן ל-DB להקצות ID
           .ForMember(dest => dest.LoginDateTime, opt => opt.Ignore()) // נעדכן ידנית
           .ForMember(dest => dest.IsAdmin, opt => opt.Ignore()) // נעדכן ידנית
                                                                 // עבור Gender: AutoMapper אמור למפות אוטומטית int ל-Enum אם הערכים תואמים.
                                                                 // אם לא, אפשר להוסיף:
           .ForMember(dest => dest.Gender, opt => opt.MapFrom(src => (TraineeDTO.EGender)src.Gender));

        }

    }
}
