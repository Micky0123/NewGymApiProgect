using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IActiveWorkoutManager
    {
        // מפעיל את האלגוריתם ויוצר את תוכנית האימון הראשונית
        PathResult RunAlgorithmAndInitializeWorkout(RunAlgorithmRequest request);

        // מחזיר את תוכנית האימון המעודכנת עבור מתאמן ספציפי
        PathResult GetUpdatedWorkoutPlan(int traineeId);

        // מחזיר את התרגיל הבא באימון
        NextExerciseResponse GetNextExerciseInWorkout(int traineeId);

        // מסמן שתרגיל הושלם
        bool CompleteExercise(int traineeId, int exerciseId);

        TraineeDTO GetTraineeById(int traineeId);
        List<ExercisePlanDTO> GetExercisePlansForPlanDay(int planDayId);
        ExerciseDTO GetExerciseDetails(int exerciseId);

    }
}
