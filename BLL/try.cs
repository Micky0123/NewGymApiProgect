//using System;
//using System.Collections.Generic;
//using System.Linq;

//// מתאר מתאמן
//public class Trainee
//{
//    public int Id { get; set; }
//    public string Name { get; set; }
//}
//// דוגמת תרגיל/מכשיר בגרף
//public class ExerciseNode
//{
//    public int Id { get; set; }
//    public int MuscleGroup { get; set; } // מספר השריר
//    public List<ExerciseNode> Neighbors { get; set; } = new List<ExerciseNode>();
//}

//// תא טבלה: מייצג מעבר מתרגיל לתרגיל בשעת סלוט מסוימת
//public class TableCell
//{
//    public int LegalityValue { get; set; } // לדוג' 2^שריר, -1, 0
//    public Queue<Trainee> Queue { get; set; } = new Queue<Trainee>();
//}

//// מחלקת שירות האלגוריתם
//public class PathFinderService
//{
//    // טבלת תלת-ממד: [תרגיל יעד, תרגיל מקור, סלוט זמן]
//    private TableCell[,,] Table3D;
//    private int exerciseCount;
//    private int timeSlotsCount;

//    // יצירת טבלה תלת-ממדית
//    public void InitTable(List<ExerciseNode> exercises, int slotsCount)
//    {
//        exerciseCount = exercises.Count;
//        timeSlotsCount = slotsCount;
//        Table3D = new TableCell[exerciseCount, exerciseCount, timeSlotsCount];

//        for (int to = 0; to < exerciseCount; to++)
//            for (int from = 0; from < exerciseCount; from++)
//                for (int slot = 0; slot < timeSlotsCount; slot++)
//                {
//                    Table3D[to, from, slot] = new TableCell();

//                    // קביעת ערך החוקיות
//                    if (to == from)
//                        Table3D[to, from, slot].LegalityValue = 0;
//                    else if (exercises[from].Neighbors.Contains(exercises[to]))
//                        Table3D[to, from, slot].LegalityValue = (int)Math.Pow(2, exercises[to].MuscleGroup);
//                    else
//                        Table3D[to, from, slot].LegalityValue = -1;
//                }
//    }

//    // פונקציית עזר: המרת DateTime לאינדקס סלוט
//    private int TimeToSlot(DateTime startTime, DateTime slotTime, int slotMinutes)
//    {
//        return (int)((slotTime - startTime).TotalMinutes / slotMinutes);
//    }

//    // בדיקת זמינות
//    private bool IsAvailable(int to, int from, int slot, int maxInQueue = 1)
//    {
//        return Table3D[to, from, slot].Queue.Count < maxInQueue;
//    }

//    // שיבוץ מתאמן
//    private void AddTrainee(int to, int from, int slot, Trainee trainee)
//    {
//        Table3D[to, from, slot].Queue.Enqueue(trainee);
//    }

//    // הסרת מתאמן (Backtrack)
//    private void RemoveTrainee(int to, int from, int slot)
//    {
//        if (Table3D[to, from, slot].Queue.Count > 0)
//            Table3D[to, from, slot].Queue.Dequeue();
//    }

//    // דוגמה: מציאת מסלול למתאמן בתוכנית תרגילים
//    public bool FindPathForTrainee(
//        List<ExerciseNode> exercises,
//        List<int> plan,                  // רשימת מזהי תרגילים (לפי סדר)
//        Trainee trainee,
//        DateTime startTime,
//        int slotMinutes = 5,
//        int maxInQueue = 1
//    )
//    {
//        // נניח שכל תרגיל לוקח סלוט אחד (אפשר להרחיב בקלות)
//        return Backtrack(0, -1, startTime, plan, exercises, trainee, slotMinutes, maxInQueue, startTime);
//    }

//    // Backtracking רקורסיבי
//    private bool Backtrack(
//        int planIndex,
//        int prevExerciseId,
//        DateTime currentTime,
//        List<int> plan,
//        List<ExerciseNode> exercises,
//        Trainee trainee,
//        int slotMinutes,
//        int maxInQueue,
//        DateTime globalStartTime
//    )
//    {
//        if (planIndex == plan.Count)
//            return true; // סיימנו את כל התרגילים

//        int to = plan[planIndex];
//        int from = prevExerciseId == -1 ? to : prevExerciseId;

//        int slot = TimeToSlot(globalStartTime, currentTime, slotMinutes);

//        // בדוק חוקיות מעבר
//        if (Table3D[to, from, slot].LegalityValue == -1)
//            return false;

//        // בדוק זמינות בתור
//        if (!IsAvailable(to, from, slot, maxInQueue))
//            return false;

//        // שיבוץ מתאמן בתור
//        AddTrainee(to, from, slot, trainee);

//        // המשך שלב הבא
//        bool found = Backtrack(planIndex + 1, to, currentTime.AddMinutes(slotMinutes), plan, exercises, trainee, slotMinutes, maxInQueue, globalStartTime);

//        // Backtrack: הסר מהתור
//        RemoveTrainee(to, from, slot);

//        return found;
//    }
//}

////////////////// דוגמת שימוש //////////////////

//public class Program
//{
//    public static void Main()
//    {
//        // יצירת מכשירים/תרגילים
//        var ex0 = new ExerciseNode { Id = 0, MuscleGroup = 0 };
//        var ex1 = new ExerciseNode { Id = 1, MuscleGroup = 1 };
//        var ex2 = new ExerciseNode { Id = 2, MuscleGroup = 2 };

//        // קביעת שכנים (מעברים חוקיים)
//        ex0.Neighbors.Add(ex1); // ex0 -> ex1 מותר
//        ex1.Neighbors.Add(ex2); // ex1 -> ex2 מותר
//        ex2.Neighbors.Add(ex0); // ex2 -> ex0 מותר

//        var exercises = new List<ExerciseNode> { ex0, ex1, ex2 };

//        // אתחול טבלה תלת-ממדית
//        PathFinderService pathFinder = new PathFinderService();
//        int slotsCount = 10; // נניח 10 סלוטים
//        pathFinder.InitTable(exercises, slotsCount);

//        // יצירת מתאמן
//        var trainee = new Trainee { Id = 1, Name = "יוסי" };

//        // תוכנית תרגילים: ex0 -> ex1 -> ex2
//        var plan = new List<int> { 0, 1, 2 };

//        DateTime startTime = new DateTime(2025, 5, 21, 8, 0, 0);

//        // חיפוש מסלול
//        bool found = pathFinder.FindPathForTrainee(
//            exercises,
//            plan,
//            trainee,
//            startTime,
//            slotMinutes: 5,
//            maxInQueue: 1
//        );

//        Console.WriteLine(found
//            ? "נמצא מסלול חוקי למתאמן!"
//            : "לא נמצא מסלול חוקי.");
//    }
//}

////using System;
////using System.Collections.Generic;
////using System.Linq;

////// מחלקה שמייצגת מתאמן
////public class Trainee
////{
////    public int Id { get; set; }
////    public string Name { get; set; }
////}

////// תא בטבלה התלת־ממדית: מכיל ערך חוקיות ותור מתאמנים
////public class TableCell
////{
////    public int LegalityValue { get; set; } // -1 = מעבר אסור, 0 = אותו תרגיל, 2^x = מעבר חוקי
////    public Queue<Trainee> Queue { get; set; } = new Queue<Trainee>();
////}

////// תרגיל/מכשיר
////public class ExerciseNode
////{
////    public int Id { get; set; }
////    public int MuscleGroup { get; set; }
////}

////// שירות שמחשב מסלולים
////public class PathFinderService
////{
////    private TableCell[,,] Table3D; // טבלה: [תרגיל יעד, תרגיל מקור, סלוט זמן]
////    private int exerciseCount;
////    private int timeSlotsCount;

////    // אתחול טבלה תלת־ממדית
////    public void InitTable(
////        List<ExerciseNode> exercises,
////        int slotsCount,
////        Func<int, int, int> legalityRule // פונקציה שמחזירה ערך חוקיות בין תרגילים
////    )
////    {
////        exerciseCount = exercises.Count;
////        timeSlotsCount = slotsCount;
////        Table3D = new TableCell[exerciseCount, exerciseCount, timeSlotsCount];

////        for (int to = 0; to < exerciseCount; to++)
////            for (int from = 0; from < exerciseCount; from++)
////                for (int slot = 0; slot < timeSlotsCount; slot++)
////                {
////                    Table3D[to, from, slot] = new TableCell
////                    {
////                        LegalityValue = legalityRule(from, to) // לפי פונקציית מעבר
////                    };
////                }
////    }

////    // פונקציה להמרת זמן לאינדקס סלוט
////    private int TimeToSlot(DateTime start, DateTime current, int slotMinutes)
////    {
////        return (int)((current - start).TotalMinutes / slotMinutes);
////    }

////    // בדיקת זמינות בתור
////    private bool IsAvailable(int to, int from, int slot, int maxInQueue = 1)
////    {
////        return Table3D[to, from, slot].Queue.Count < maxInQueue;
////    }

////    // שיבוץ מתאמן
////    private void AddTrainee(int to, int from, int slot, Trainee trainee)
////    {
////        Table3D[to, from, slot].Queue.Enqueue(trainee);
////    }

////    // הסרת מתאמן (ב־Backtrack)
////    private void RemoveTrainee(int to, int from, int slot)
////    {
////        if (Table3D[to, from, slot].Queue.Count > 0)
////            Table3D[to, from, slot].Queue.Dequeue();
////    }

////    // מציאת מסלול: מחזיר true אם אפשר למסלול את כל התרגילים
////    public bool FindPathForTrainee(
////        List<ExerciseNode> exercises,
////        List<int> plan, // מזהי תרגילים לפי סדר אימון
////        Trainee trainee,
////        DateTime startTime,
////        int slotMinutes = 5,
////        int maxInQueue = 1
////    )
////    {
////        return Backtrack(
////            0, -1, startTime, plan, exercises, trainee, slotMinutes, maxInQueue, startTime
////        );
////    }

////    // Backtracking רקורסיבי
////    private bool Backtrack(
////        int planIndex,
////        int prevExerciseId,
////        DateTime currentTime,
////        List<int> plan,
////        List<ExerciseNode> exercises,
////        Trainee trainee,
////        int slotMinutes,
////        int maxInQueue,
////        DateTime globalStartTime
////    )
////    {
////        if (planIndex == plan.Count)
////            return true; // סיימנו את כל התרגילים

////        int to = plan[planIndex];
////        int from = prevExerciseId == -1 ? to : prevExerciseId;
////        int slot = TimeToSlot(globalStartTime, currentTime, slotMinutes);

////        // בדיקת חוקיות מעבר
////        if (Table3D[to, from, slot].LegalityValue == -1)
////            return false;

////        // בדיקת זמינות בתור
////        if (!IsAvailable(to, from, slot, maxInQueue))
////            return false;

////        // שיבוץ מתאמן בתור
////        AddTrainee(to, from, slot, trainee);

////        // מעבר לתרגיל הבא
////        bool found = Backtrack(
////            planIndex + 1, to, currentTime.AddMinutes(slotMinutes),
////            plan, exercises, trainee, slotMinutes, maxInQueue, globalStartTime
////        );

////        // מחיקה מהתור (Backtrack)
////        RemoveTrainee(to, from, slot);

////        return found;
////    }
////}

//////////////////////// דוגמת שימוש ////////////////////

////public class Program
////{
////    public static void Main()
////    {
////        // הגדרת תרגילים:
////        var ex0 = new ExerciseNode { Id = 0, MuscleGroup = 0 };
////        var ex1 = new ExerciseNode { Id = 1, MuscleGroup = 1 };
////        var ex2 = new ExerciseNode { Id = 2, MuscleGroup = 2 };
////        var exercises = new List<ExerciseNode> { ex0, ex1, ex2 };

////        // פונקציית חוקיות: מעבר חוקי אם לאותו שריר או מעבר סביר (כאן לצורך הדוגמה)
////        int legalityRule(int from, int to)
////        {
////            if (from == to) return 0; // תרגיל זהה
////            if (Math.Abs(exercises[to].MuscleGroup - exercises[from].MuscleGroup) == 1)
////                return 1 << exercises[to].MuscleGroup; // 2^x
////            return -1; // מעבר אסור
////        }

////        // אתחול טבלה
////        PathFinderService pathFinder = new PathFinderService();
////        int slotsCount = 10;
////        pathFinder.InitTable(exercises, slotsCount, legalityRule);

////        // יצירת מתאמן
////        var trainee = new Trainee { Id = 1, Name = "יוסי" };

////        // תוכנית: ex0 -> ex1 -> ex2
////        var plan = new List<int> { 0, 1, 2 };
////        DateTime startTime = new DateTime(2025, 5, 21, 8, 0, 0);

////        // חיפוש מסלול
////        bool found = pathFinder.FindPathForTrainee(
////            exercises, plan, trainee, startTime, slotMinutes: 5, maxInQueue: 1
////        );

////        Console.WriteLine(found
////            ? "נמצא מסלול חוקי למתאמן!"
////            : "לא נמצא מסלול חוקי.");
////    }
////}
