//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BLL;
//using DTO;

//namespace BLL
//{
//    public class Backtracking
//    {
//        private TraineeDTO currentTrainee; // המתאמן הנוכחי

//        // מילון לתכנון דינאמי - שומר תוצאות של מצבים שכבר חושבו(עבור memoization)
//        private Dictionary<(int mask, int lastNodeId, long timeMinutes), (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)> memo = new();

//        // אלגוריתם Backtracking מתוקן עם המבנה החדש
//        public (bool found, List<int> bestPath, DateTime endTime) BacktrackWithPriority(
//            List<int> exerciseOrder, //רשימת התרגילים
//            int mask,  //יצוג בינארי אלו תרגילים נעשו
//            List<int> currentPath, //מסלול
//            int currentAlternatives, //מספר החלופות 
//            DateTime currentTime,
//            out DateTime endTime)
//        {
//            int lastNodeId = currentPath.Count > 0 ? currentPath.Last() : -1;
//            var currentSlot = currentTime;//צריך סוג של המרה
//            // מפתח למטמון
//            var memoKey = (mask, lastNodeId, currentTime.Ticks / TimeSpan.TicksPerMinute);
//            // בדיקה במטמון
//            if (memo.TryGetValue(memoKey, out var cachedResult))
//            {
//                endTime = cachedResult.endTime;
//                return (cachedResult.found, cachedResult.bestPath, cachedResult.endTime);
//            }
//            // בדיקת תנאי סיום: כל התרגילים בוצעו
//            if (mask == (1 << exerciseOrder.Count) - 1)
//            {
//                endTime = currentTime;
//                return (true, new List<int>(currentPath), currentTime);
//            }
//            bool foundAny = false;
//            List<int> bestPath = null;
//            DateTime bestEndTime = DateTime.MinValue;
//            int minAlternatives = int.MaxValue;

//            // אסטרטגיה 1: נסה לבצע תרגילים לפי סדר רגיל
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                // אם התרגיל כבר בוצע, דלג
//                if (IsExerciseDone(mask, i))
//                    continue;
//                int nodeId = exerciseOrder[i];

//                //במידה והפונקציה IsLegalTransition תקינה ומחזירה האם ניתן להגיע מהתרגיל הקודם אליו
//                if (lastNodeId != -1 && !TransitionMatrix.IsLegalTransition(lastNodeId, nodeId, currentSlot))
//                    continue;

//                //בדיקה האם ניתן להגיע לשאר התרגילים מהתרגיל הנוכחי
//                if (!TransitionMatrix.CanRichToALlEx)//*****************************************
//                    continue;

//                // בדוק זמינות בסלוט הזמן הנוכחי
//                if (TransitionMatrix.IsSlotAvailable(nodeId, lastNodeId, currentSlot))//##שליחה לפונקציה שבודקת זמינות תרגיל
//                {
//                    // "תפוס" את הסלוט זמנית
//                    AddTraineeToSlot(nodeId, lastNodeId, currentSlot, currentTrainee);//##פונקצית הוספת מתאמן לתור

//                    // נסה להמשיך את המסלול
//                    currentPath.Add(nodeId);
//                    var nextTime = currentTime.AddMinutes();// כאן צריך לשלוח את הזמן הבא אחרי הפוסה

//                    var result = BacktrackWithPriority(exerciseOrder, MarkExerciseDone(mask, i), currentPath, currentAlternatives,
//                        nextTime, out DateTime candidateEndTime
//                    );

//                    if (result.found)
//                    {
//                        foundAny = true;
//                        bestPath = result.bestPath;
//                        bestEndTime = candidateEndTime;
//                        minAlternatives = currentAlternatives;
//                        // שמור את התוצאה ב-cache
//                        memo[memoKey] = (true, minAlternatives, bestPath, bestEndTime);

//                        endTime = bestEndTime;
//                        return (true, bestPath, bestEndTime);
//                    }
//                    memo[memoKey] = (false, minAlternatives, bestPath, bestEndTime);
//                    // שחרר את הסלוט (ניסיון לא צלח)
//                    RemoveTraineeFromSlot(nodeId, lastNodeId, currentSlot);//##פונקציה שחרור הסלוט 
//                    currentPath.RemoveAt(currentPath.Count - 1);
//                }
//            }

//            // אסטרטגיה 2: נסה למצוא תרגילים חלופיים
//            for (int i = 0; i < exerciseOrder.Count; i++)
//            {
//                // אם התרגיל כבר בוצע, דלג
//                if (IsExerciseDone(mask, i))
//                    continue;

//                int nodeId = exerciseOrder[i];
//                // בדוק האם המעבר חוקי
//                //במידה והפונקציה IsLegalTransition תקינה ומחזירה האם ניתן להגיע מהתרגיל הקודם אליו
//                if (lastNodeId != -1 && !TransitionMatrix.IsLegalTransition(lastNodeId, nodeId, currentSlot))
//                    continue;

//                //בדיקה האם ניתן להגיע לשאר התרגילים מהתרגיל הנוכחי
//                if (!TransitionMatrix.CanRichToALlEx)//*****************************************
//                    continue;

//                // בדוק זמינות בסלוט הזמן הנוכחי
//                //if (TransitionMatrix.IsSlotAvailable(nodeId, lastNodeId, currentSlot))//##שליחה לפונקציה שבודקת זמינות תרגיל
//                //{
//                //    // "תפוס" את הסלוט זמנית
//                //    AddTraineeToSlot(nodeId, lastNodeId, currentSlot, currentTrainee);//##פונקצית הוספת מתאמן לתור

//                //    // נסה להמשיך את המסלול
//                //    currentPath.Add(nodeId);
//                //    var nextTime = currentTime.AddMinutes();// כאן צריך לשלוח את הזמן הבא אחרי הפוסה
//                //    var result = BacktrackWithPriority(exerciseOrder, MarkExerciseDone(mask, i), currentPath,
//                //        currentAlternatives, nextTime, out DateTime candidateEndTime
//                //    );
//                //    //if (result.found)
//                //    //{
//                //    //    foundAny = true;
//                //    //    bestPath = result.bestPath;
//                //    //    bestEndTime = candidateEndTime;
//                //    //    minAlternatives = currentAlternatives;

//                //    //    memo[memoKey] = (true, minAlternatives, bestPath, bestEndTime);

//                //    //    endTime = bestEndTime;
//                //    //    return (true, bestPath, bestEndTime);
//                //    //}
//                //    //memo[memoKey] = (false, minAlternatives, bestPath, bestEndTime);
//                //    //// שחרר את הסלוט (ניסיון לא צלח)
//                //    //RemoveTraineeFromSlot(nodeId, lastNodeId, currentSlot);//##פונקציה שחרור הסלוט 
//                //    //currentPath.RemoveAt(currentPath.Count - 1);
//                //    if (result.found)
//                //    {
//                //        if (!foundAny || currentAlternatives < minAlternatives)
//                //        {
//                //            foundAny = true;
//                //            minAlternatives = currentAlternatives;
//                //            bestPath = result.bestPath;
//                //            bestEndTime = candidateEndTime;
//                //        }
//                //    }
//                //    // שמור את התוצאה ב-cache
//                //    memo[memoKey] = (foundAny, minAlternatives, bestPath, bestEndTime);
//                //    // שחרר את הסלוט החלופי
//                //    RemoveTraineeFromSlot(alternativeNodeId, lastNodeId, currentSlot);
//                //    currentPath.RemoveAt(currentPath.Count - 1);
//                //}
//                //else
//                //{
//                    //פונקציה שמחזירה רשימה של כל התרגילים החילופיים האפשריים שפנויים 
//                    List<int> alternative = Func();///*********************************
//                    //בדיקה האם הוא תפוס
//                    bool flag = false;
//                    //בעצם הוא עושה את הבדיקה על התרגילים החילופיים או החלפה של מתאמנים עד שהוא מוצא מישהו שאפשר להחליף איתו 
//                    //במקרה הטוב יכול להיות פעם אחת במקרה הגרוע מעבר על כל התרגילים החילופיים
//                    for (int j = 0; j < alternative.Count; j++)
//                    {
//                        if (TransitionMatrix.IsSlotAvailable(nodeId, i, currentSlot))
//                        {
//                            AddTraineeToSlot(j, lastNodeId, currentSlot, currentTrainee);//##פונקצית הוספת מתאמן לתור
//                            // נסה להמשיך את המסלול
//                            currentPath.Add(j);
//                            var nextTime = currentTime.AddMinutes();// כאן צריך לשלוח את הזמן הבא אחרי הפוסה
//                                                                    // שים לב: הגדלנו את מספר החילופים ב-1
//                            var result = BacktrackWithPriority(
//                                exerciseOrder,
//                                MarkExerciseDone(mask, i), currentPath, currentAlternatives + 1,
//                                nextTime, out DateTime candidateEndTime
//                            );

//                            if (result.found)
//                            {
//                                if (!foundAny || currentAlternatives + 1 < minAlternatives)
//                                {
//                                    foundAny = true;
//                                    minAlternatives = currentAlternatives + 1;
//                                    bestPath = result.bestPath;
//                                    bestEndTime = candidateEndTime;
//                                }
//                                flag = true;
//                            }
//                            // שמור את התוצאה ב-cache
//                            memo[memoKey] = (foundAny, minAlternatives, bestPath, bestEndTime);
//                            // שחרר את הסלוט החלופי
//                            RemoveTraineeFromSlot(alternativeNodeId, lastNodeId, currentSlot);
//                            currentPath.RemoveAt(currentPath.Count - 1);
//                        }
//                        else
//                        {
//                            // אסטרטגיה 3: נסה לשנות את הסדר של מתאמנים אחרים
//                            var occupyingTrainee = GetOccupyingTraineeInSlot(nodeId, lastNodeId, currentSlot);
//                            if (occupyingTrainee != null && occupyingTrainee != currentTrainee)
//                            {
//                                if (TryRescheduleOtherTrainee(occupyingTrainee, nodeId, currentTime))
//                                {
//                                    // אם הצלחנו להזיז את המתאמן האחר, נסה שוב
//                                    AddTraineeToSlot(nodeId, lastNodeId, currentSlot, currentTrainee);
//                                    currentPath.Add(nodeId);

//                                    var result = BacktrackWithPriority(
//                                        exerciseOrder,
//                                        MarkExerciseDone(mask, i), currentPath,
//                                        currentAlternatives + 2, // עלות גבוהה יותר להזזת מתאמן אחר
//                                        currentTime.AddMinutes(node.UsageDurationMinutes),
//                                        out DateTime candidateEndTime
//                                    );

//                                    if (result.found)
//                                    {
//                                        if (!foundAny || currentAlternatives + 2 < minAlternatives)
//                                        {
//                                            foundAny = true;
//                                            minAlternatives = currentAlternatives + 2;
//                                            bestPath = result.bestPath;
//                                            bestEndTime = candidateEndTime;
//                                        }
//                                        flag = true;
//                                    }
//                                    RemoveTraineeFromSlot(nodeId, lastNodeId, currentSlot);
//                                    currentPath.RemoveAt(currentPath.Count - 1);
//                                }
//                            }
//                        }
//                        if (flag)
//                        {
//                            continue;
//                        }
//                    }
//                //}
//            }
//            // שמירת התוצאה ב-cache
//            endTime = foundAny ? bestEndTime : DateTime.MinValue;
//            memo[memoKey] = (foundAny, foundAny ? minAlternatives : int.MaxValue, bestPath, endTime);

//            return (foundAny, bestPath, endTime);


//        }
//    }
//}








////public class Backtracking
////{
////    // private int exerciseCount;
////    //private int timeSlotsCount;
////    //private List<ExerciseNode> exercises; // שמירת רשימת התרגילים
////    private Trainee currentTrainee; // המתאמן הנוכחי
////    /// אולי להוסיף כמות מכל תרגיל


////    //?????????
////    // מילון לתכנון דינאמי - שומר תוצאות של מצבים שכבר חושבו(עבור memoization)
////    private Dictionary<(int mask, int lastNodeId, long timeMinutes), (bool found, int numAlternatives, List<int> bestPath, DateTime endTime)> memo = new();


////    // אלגוריתם Backtracking מתוקן עם המבנה החדש
////    public (bool found, List<int> bestPath, DateTime endTime) BacktrackWithPriority(
////        //SubGraph graph, //גרף 
////        //Dictionary<int, Node> nodeDict, //מילון צמתים לגישה מהירה
////        //Dictionary<int, HashSet<int>> reachableFromNode, //מטריצת נגישות
////        List<int> exerciseOrder, //רשימת התרגילים
////                                 // Dictionary<int, List<int>> alternativeDevices, //מילון של התרגילים החילופיים
////        int mask,  //יצוג בינארי אלו תרגילים נעשו
////        List<int> currentPath, //מסלול
////        int currentAlternatives, //מספר החלופות 
////        DateTime currentTime,
////        out DateTime endTime
////        )
////    {
////        int lastNodeId = currentPath.Count > 0 ? currentPath.Last() : -1;
////        var currentSlot = currentTime;//צריך סוג של המרה

////        // מפתח למטמון
////        var memoKey = (mask, lastNodeId, currentTime.Ticks / TimeSpan.TicksPerMinute);

////        // בדיקה במטמון
////        if (memo.TryGetValue(memoKey, out var cachedResult))
////        {
////            endTime = cachedResult.endTime;
////            return (cachedResult.found, cachedResult.bestPath, cachedResult.endTime);
////        }

////        // בדיקת תנאי סיום: כל התרגילים בוצעו
////        if (mask == (1 << exerciseOrder.Count) - 1)
////        {
////            endTime = currentTime;
////            return (true, new List<int>(currentPath), currentTime);
////        }

////        bool foundAny = false;
////        List<int> bestPath = null;
////        DateTime bestEndTime = DateTime.MinValue;
////        int minAlternatives = int.MaxValue;

////        // אסטרטגיה 1: נסה לבצע תרגילים לפי סדר רגיל
////        for (int i = 0; i < exerciseOrder.Count; i++)
////        {
////            // אם התרגיל כבר בוצע, דלג
////            if (IsExerciseDone(mask, i))
////                continue;

////            //שימי ❤ לא יודעת אם צריך את זה 
////            int nodeId = exerciseOrder[i];
////            //if (!nodeDict.TryGetValue(nodeId, out var node))
////            //    continue;

////            // בדוק האם המעבר חוקי
////            //במידה והפונקציה IsLegalTransition תקינה ומחזירה האם ניתן להגיע מהתרגיל הקודם אליו
////            if (lastNodeId != -1 && !TransitionMatrix.IsLegalTransition(lastNodeId, nodeId, currentSlot))
////                continue;

////            //בדיקה האם ניתן להגיע לשאר התרגילים מהתרגיל הנוכחי
////            if (!TransitionMatrix.CanRichToALlEx)//*****************************************
////                continue;

////            // בדוק זמינות בסלוט הזמן הנוכחי
////            if (TransitionMatrix.IsSlotAvailable(nodeId, lastNodeId, currentSlot))//##שליחה לפונקציה שבודקת זמינות תרגיל
////            {
////                // "תפוס" את הסלוט זמנית
////                AddTraineeToSlot(nodeId, lastNodeId, currentSlot, currentTrainee);//##פונקצית הוספת מתאמן לתור

////                // נסה להמשיך את המסלול
////                currentPath.Add(nodeId);
////                var nextTime = currentTime.AddMinutes();// כאן צריך לשלוח את הזמן הבא אחרי הפוסה

////                var result = BacktrackWithPriority(
////                    exerciseOrder,
////                    MarkExerciseDone(mask, i), currentPath,
////                    currentAlternatives, nextTime,
////                    out DateTime candidateEndTime
////                );

////                if (result.found)
////                {
////                    foundAny = true;
////                    bestPath = result.bestPath;
////                    bestEndTime = candidateEndTime;
////                    minAlternatives = currentAlternatives;

////                    //שימי ❤ לא יודעת אם צריך לשחרר הרי אני רוצה לתפוס
////                    // שחרר את הסלוט
////                    // RemoveTraineeFromSlot(nodeId, lastNodeId, currentSlot);

////                    // שמור את התוצאה ב-cache
////                    memo[memoKey] = (true, minAlternatives, bestPath, bestEndTime);

////                    endTime = bestEndTime;
////                    return (true, bestPath, bestEndTime);
////                }

////                memo[memoKey] = (false, minAlternatives, bestPath, bestEndTime);
////                // שחרר את הסלוט (ניסיון לא צלח)
////                RemoveTraineeFromSlot(nodeId, lastNodeId, currentSlot);//##פונקציה שחרור הסלוט 
////                currentPath.RemoveAt(currentPath.Count - 1);
////            }
////        }


////        //שימי ❤ זה ממש לא נכון
////        // אסטרטגיה 2: נסה למצוא תרגילים חלופיים
////        for (int i = 0; i < exerciseOrder.Count; i++)
////        {
////            // אם התרגיל כבר בוצע, דלג
////            if (IsExerciseDone(mask, i))
////                continue;

////            //שימי ❤ לא יודעת אם צריך את זה 
////            int nodeId = exerciseOrder[i];
////            // בדוק האם המעבר חוקי
////            //במידה והפונקציה IsLegalTransition תקינה ומחזירה האם ניתן להגיע מהתרגיל הקודם אליו
////            if (lastNodeId != -1 && !TransitionMatrix.IsLegalTransition(lastNodeId, nodeId, currentSlot))
////                continue;

////            //בדיקה האם ניתן להגיע לשאר התרגילים מהתרגיל הנוכחי
////            if (!TransitionMatrix.CanRichToALlEx)//*****************************************
////                continue;

////            // בדוק זמינות בסלוט הזמן הנוכחי
////            if (TransitionMatrix.IsSlotAvailable(nodeId, lastNodeId, currentSlot))//##שליחה לפונקציה שבודקת זמינות תרגיל
////            {
////                // "תפוס" את הסלוט זמנית
////                AddTraineeToSlot(nodeId, lastNodeId, currentSlot, currentTrainee);//##פונקצית הוספת מתאמן לתור

////                // נסה להמשיך את המסלול
////                currentPath.Add(nodeId);
////                var nextTime = currentTime.AddMinutes();// כאן צריך לשלוח את הזמן הבא אחרי הפוסה
////                var result = BacktrackWithPriority(
////                    exerciseOrder,
////                    MarkExerciseDone(mask, i), currentPath,
////                    currentAlternatives, nextTime,
////                    out DateTime candidateEndTime
////                );

////                if (result.found)
////                {
////                    foundAny = true;
////                    bestPath = result.bestPath;
////                    bestEndTime = candidateEndTime;
////                    minAlternatives = currentAlternatives;

////                    //שימי ❤ לא יודעת אם צריך לשחרר הרי אני רוצה לתפוס
////                    // שחרר את הסלוט
////                    // RemoveTraineeFromSlot(nodeId, lastNodeId, currentSlot);

////                    // שמור את התוצאה ב-cache
////                    memo[memoKey] = (true, minAlternatives, bestPath, bestEndTime);

////                    endTime = bestEndTime;
////                    return (true, bestPath, bestEndTime);
////                }

////                memo[memoKey] = (false, minAlternatives, bestPath, bestEndTime);
////                // שחרר את הסלוט (ניסיון לא צלח)
////                RemoveTraineeFromSlot(nodeId, lastNodeId, currentSlot);//##פונקציה שחרור הסלוט 
////                currentPath.RemoveAt(currentPath.Count - 1);
////            }
////            else
////            {
////                int originalNodeId = exerciseOrder[i];
////                // אם יש מכשירים חלופיים לתרגיל זה
////                //פונקציה שמחזירה רשימה של כל התרגילים החילופיים האפשריים שפנויים 
////                List<int> alternative = Func();///*********************************
////                // "תפוס" את הסלוט זמנית
////                //בדיקה האם הוא תפוס
////                for (int j = 0; j < alternative.Count; j++)
////                {
////                    if (TransitionMatrix.IsSlotAvailable(nodeId, i, currentSlot))
////                    {
////                        AddTraineeToSlot(alternative, lastNodeId, currentSlot, currentTrainee);//##פונקצית הוספת מתאמן לתור

////                        // נסה להמשיך את המסלול
////                        currentPath.Add(alternative);
////                        var nextTime = currentTime.AddMinutes();// כאן צריך לשלוח את הזמן הבא אחרי הפוסה

////                        // שים לב: הגדלנו את מספר החילופים ב-1
////                        var result = BacktrackWithPriority(
////                            exerciseOrder,
////                            MarkExerciseDone(mask, i), currentPath, currentAlternatives + 1,
////                            nextTime, out DateTime candidateEndTime
////                        );

////                        if (result.found)
////                        {
////                            if (!foundAny || currentAlternatives + 1 < minAlternatives)
////                            {
////                                foundAny = true;
////                                minAlternatives = currentAlternatives + 1;
////                                bestPath = result.bestPath;
////                                bestEndTime = candidateEndTime;
////                            }
////                        }


////                        // שמור את התוצאה ב-cache
////                        memo[memoKey] = (foundAny, minAlternatives, bestPath, bestEndTime);
////                        // שחרר את הסלוט החלופי
////                        RemoveTraineeFromSlot(alternativeNodeId, lastNodeId, currentSlot);
////                        currentPath.RemoveAt(currentPath.Count - 1);

////                    }
////                    else
////                    {
////                        // אסטרטגיה 3: נסה לשנות את הסדר של מתאמנים אחרים
////                        // בדוק אם יש מתאמן אחר שתופס את הסלוט
////                        //if (currentSlot < timeSlotsCount && !IsSlotAvailable(nodeId, lastNodeId, currentSlot))
////                        //{
////                        // נסה להזיז את המתאמן האחר
////                        var occupyingTrainee = GetOccupyingTraineeInSlot(nodeId, lastNodeId, currentSlot);
////                        if (occupyingTrainee != null && occupyingTrainee != currentTrainee)
////                        {
////                            if (TryRescheduleOtherTrainee(occupyingTrainee, nodeId, currentTime))
////                            {
////                                // אם הצלחנו להזיז את המתאמן האחר, נסה שוב
////                                AddTraineeToSlot(nodeId, lastNodeId, currentSlot, currentTrainee);
////                                currentPath.Add(nodeId);

////                                var result = BacktrackWithPriority(
////                                    exerciseOrder,
////                                    MarkExerciseDone(mask, i), currentPath,
////                                    currentAlternatives + 2, // עלות גבוהה יותר להזזת מתאמן אחר
////                                    currentTime.AddMinutes(node.UsageDurationMinutes),
////                                    out DateTime candidateEndTime
////                                );

////                                if (result.found)
////                                {
////                                    if (!foundAny || currentAlternatives + 2 < minAlternatives)
////                                    {
////                                        foundAny = true;
////                                        minAlternatives = currentAlternatives + 2;
////                                        bestPath = result.bestPath;
////                                        bestEndTime = candidateEndTime;
////                                    }
////                                }

////                                RemoveTraineeFromSlot(nodeId, lastNodeId, currentSlot);
////                                currentPath.RemoveAt(currentPath.Count - 1);
////                            }
////                            //}

////                        }
////                    }

////                }
////            }



////            // שמירת התוצאה ב-cache
////            endTime = foundAny ? bestEndTime : DateTime.MinValue;
////            memo[memoKey] = (foundAny, foundAny ? minAlternatives : int.MaxValue, bestPath, endTime);

////            return (foundAny, bestPath, endTime);
////        }

////    }

////    //if (!foundAny)
////    //{
////    //    for (int i = 0; i < exerciseOrder.Count; i++)
////    //    {
////    //        if (IsExerciseDone(mask, i))
////    //            continue;

////    //        int nodeId = exerciseOrder[i];
////    //        //if (!nodeDict.TryGetValue(nodeId, out var node))
////    //        //    continue;

////    //        // בדוק אם יש מתאמן אחר שתופס את הסלוט
////    //        if (currentSlot < timeSlotsCount && !IsSlotAvailable(nodeId, lastNodeId, currentSlot))
////    //        {
////    //            // נסה להזיז את המתאמן האחר
////    //            var occupyingTrainee = GetOccupyingTraineeInSlot(nodeId, lastNodeId, currentSlot);
////    //            if (occupyingTrainee != null && occupyingTrainee != currentTrainee)
////    //            {
////    //                if (TryRescheduleOtherTrainee(occupyingTrainee, nodeId, currentTime))
////    //                {
////    //                    // אם הצלחנו להזיז את המתאמן האחר, נסה שוב
////    //                    AddTraineeToSlot(nodeId, lastNodeId, currentSlot, currentTrainee);
////    //                    currentPath.Add(nodeId);

////    //                    var result = BacktrackWithPriority(
////    //                        graph, nodeDict, reachableFromNode, exerciseOrder, alternativeDevices,
////    //                        MarkExerciseDone(mask, i), currentPath,
////    //                        currentAlternatives + 2, // עלות גבוהה יותר להזזת מתאמן אחר
////    //                        currentTime.AddMinutes(node.UsageDurationMinutes),
////    //                        out DateTime candidateEndTime
////    //                    );

////    //                    if (result.found)
////    //                    {
////    //                        if (!foundAny || currentAlternatives + 2 < minAlternatives)
////    //                        {
////    //                            foundAny = true;
////    //                            minAlternatives = currentAlternatives + 2;
////    //                            bestPath = result.bestPath;
////    //                            bestEndTime = candidateEndTime;
////    //                        }
////    //                    }

////    //                    RemoveTraineeFromSlot(nodeId, lastNodeId, currentSlot);
////    //                    currentPath.RemoveAt(currentPath.Count - 1);
////    //                }
////    //            }
////    //        }
////    //    }
////    //}

////    private bool IsExerciseDone(int mask, int exerciseIndex)
////    {
////        return (mask & (1 << exerciseIndex)) != 0;
////    }







////    // הוספת תרגיל לרשימת התרגילים שבוצעו
////    private int MarkExerciseDone(int mask, int exerciseIndex)
////    {
////        return mask | (1 << exerciseIndex);
////    }

////    // פונקציה לאיפוס המטמון (שימושי לביצועים)
////    public void ClearCache()
////    {
////        memo.Clear();
////        reachabilityCache = null;
////    }

////    // פונקציה לקבלת סטטיסטיקות על הביצועים
////    public (int CacheHits, int TotalSearches, double HitRatio) GetPerformanceStats()
////    {
////        int totalSearches = memo.Count;
////        // כאן ניתן להוסיף מעקב על cache hits אם נרצה
////        return (0, totalSearches, 0.0);
////    }

////    /// קבלת אינדקס התרגיל הבא שטרם בוצע
////    private int GetNextUncompletedExercise(int mask, List<int> exerciseOrder)
////    {
////        for (int i = 0; i < exerciseOrder.Count; i++)
////        {
////            if (!IsExerciseDone(mask, i))
////                return i;
////        }
////        return -1;
////    }

////    // ספירת מספר התרגילים שכבר בוצעו
////    private int CountCompletedExercises(int mask)
////    {
////        int count = 0;
////        while (mask > 0)
////        {
////            count += mask & 1;
////            mask >>= 1;
////        }
////        return count;
////    }

////    // ספירת ביטים דלוקים ב-mask (כמה תרגילים כבר בוצעו)
////    private int CountBits(int mask)
////    {
////        int count = 0;
////        while (mask > 0)
////        {
////            count += mask & 1;
////            mask >>= 1;
////        }
////        return count;
////    }

////    private bool TryRescheduleOtherTrainee(
////    Trainee otherTrainee,
////    int exerciseToReschedule,
////    DateTime desiredTime)
////    {
////        // מימוש בסיסי - יש לפתח לפי הצורך
////        try
////        {
////            // בדוק אם המתאמן כבר התחיל את האימון
////            if (otherTrainee.HasStartedTraining)
////                return false; // אם כבר התחיל, לא מזיזים אותו

////            // נסה למצוא מסלול חוקי חדש עבור המתאמן השני
////            return TryFindAlternativePathForTrainee(otherTrainee, exerciseToReschedule, desiredTime);
////        }
////        catch
////        {
////            return false; // במקרה של שגיאה, לא מזיזים
////        }
////    }

////    // פונקציית עזר להזזת מתאמן אחר
////    private bool TryFindAlternativePathForTrainee(Trainee trainee, int conflictExercise, DateTime conflictTime)
////    {
////        // כאן צריך לממש את הלוגיקה שמנסה למצוא מסלול חלופי למתאמן
////        // זה יכול להיות רקורסיה לאלגוריתם הראשי או אלגוריתם פשוט יותר

////        // מימוש בסיסי - מחזיר true בהסתברות של 30% (לדוגמה)
////        Random rand = new Random();
////        return rand.NextDouble() < 0.3;
////    }


////}