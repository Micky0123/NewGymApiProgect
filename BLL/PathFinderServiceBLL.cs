using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBLL;

namespace BLL
{
    public class PathFinderServiceBLL  /*IPathFinderService*/
    {
        //מסלול של תרגילים ומשך הזמן של כל תרגיל
        public class PathResult
        {
            public List<int> ExerciseIdsInPath { get; set; } = new();
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
        }

        public PathResult FindValidPath(SubGraph subGraph,//הגרף של המתאמן 
                                        List<int> exerciseOrder,//רשימת התרגילים מהתוכנית הדיפולטיבית
                                        DateTime startTime)//תאריך התחלה
        {
            var visited = new HashSet<int>();
            var path = new List<int>();
            var currentTime = startTime;

            //שליחה לאלגוריתם שמוצא מסלול חוקי
            var success = Backtrack(
                subGraph,
                exerciseOrder,
                new Dictionary<int, List<int>>(), // מכשירים חילופיים **********************************************
                visited,
                path,
                currentTime,
                out var finalTime
            );

            if (!success)
                throw new Exception("לא נמצא מסלול חוקי לפי ההגבלות.");
            // אם לא נמצא מסלול חוקי, נכניס אופציה של תרגיל חילופי
            // איך נדע איזה תרגיל להחליף
            // עדכון תורי זמינות
            foreach (var machineId in path)
            {
                var machineNode = subGraph.Nodes[machineId];
                machineNode.AddToQueue(currentTime); // מוסיף את המתאמן לתור בשעה הזו
                currentTime = currentTime.AddMinutes(machineNode.UsageDurationMinutes);
            }

            return new PathResult
            {
                ExerciseIdsInPath = path,
                StartTime = startTime,
                EndTime = finalTime
            };
        }

        /// האלגוריתם: Backtracking + Memoization
        //private bool Backtrack(
        //    SubGraph graph, // הגרף של המתאמן
        //    List<int> exerciseOrder,//רשימת התרגילים מהתוכנית הדיפולטיבית
        //    HashSet<int> visited, //התרגילים שביקרנו
        //    List<int> path, //מסלול
        //    DateTime currentTime,//תאריך התחלה
        //    out DateTime finishTime //תאריך סיום
        //)
        //{
        //    // אם כל התרגילים ביקרנו בהם, נעדכן את זמן הסיום
        //    if (visited.Count == exerciseOrder.Count)
        //    {
        //        finishTime = currentTime;
        //        return true;
        //    }

        //    // ננסה לפי הסדר, אבל נבחר רק צמתים שלא ביקרנו ושהם זמינים בזמן הזה
        //    foreach (var ExerciseId in exerciseOrder)
        //    {
        //        if (visited.Contains(ExerciseId))
        //            continue;

        //        // נוודא שהתרגיל קיים בגרף והוא זמין בזמן הזה
        //        var exercise = graph.Nodes.FirstOrDefault(n => n.Id == ExerciseId);
        //        if (exercise == null || !exercise.IsAvailableAt(currentTime))
        //            continue;

        //        //אם התרגיל קיים וזמין, נוסיף אותו למסלול
        //        visited.Add(ExerciseId);
        //        path.Add(ExerciseId);

        //        //צריך לשהות לפי משך הזמן של התרגיל********************************************
        //        var nextTime = currentTime.AddMinutes(exercise.UsageDurationMinutes);

        //        //נחפש את המסלול הבא
        //        if (Backtrack(graph, exerciseOrder, visited, path, nextTime, out finishTime))
        //            return true;
        //        //במידה ולא נמצא מסלול חוקי, נחזור לתרגיל הקודם

        //        visited.Remove(ExerciseId);
        //        path.RemoveAt(path.Count - 1);
        //    }

        //    finishTime = currentTime;
        //    return false;
        //}

        //        private bool Backtrack(
        //            SubGraph graph, // הגרף של המתאמן
        //            List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
        //            Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
        //            HashSet<int> visited, // התרגילים שביקרנו
        //            List<int> path, // מסלול
        //            DateTime currentTime, // תאריך התחלה
        //            out DateTime finishTime // תאריך סיום
        //)
        //        {
        //            // אם כל התרגילים בוצעו, נעדכן את זמן הסיום
        //            if (visited.Count == exerciseOrder.Count)
        //            {
        //                finishTime = currentTime;
        //                return true;
        //            }

        //            // ננסה לפי הסדר, אבל נבחר רק צמתים שלא ביקרנו ושהם זמינים בזמן הזה
        //            foreach (var exerciseId in exerciseOrder)
        //            {
        //                if (visited.Contains(exerciseId))
        //                    continue;

        //                // נוודא שהתרגיל קיים בגרף והוא זמין בזמן הזה
        //                var exercise = graph.Nodes.FirstOrDefault(n => n.Id == exerciseId);
        //                if (exercise != null && exercise.IsAvailableAt(currentTime))
        //                {
        //                    // אם התרגיל זמין, נוסיף אותו למסלול
        //                    visited.Add(exerciseId);
        //                    path.Add(exerciseId);

        //                    // עדכון הזמן לתרגיל הבא
        //                    var nextTime = currentTime.AddMinutes(exercise.UsageDurationMinutes);

        //                    // חפש את המסלול הבא
        //                    if (Backtrack(graph, exerciseOrder, alternativeDevices, visited, path, nextTime, out finishTime))
        //                        return true;

        //                    // אם לא הצלחנו, נחזור אחורה
        //                    visited.Remove(exerciseId);
        //                    path.RemoveAt(path.Count - 1);
        //                }
        //                else
        //                {
        //                    // אם התרגיל לא זמין, ננסה מכשירים חילופיים
        //                    if (alternativeDevices.ContainsKey(exerciseId))
        //                    {
        //                        foreach (var alternativeId in alternativeDevices[exerciseId])
        //                        {
        //                            var alternativeExercise = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
        //                            if (alternativeExercise != null && !visited.Contains(alternativeId) && alternativeExercise.IsAvailableAt(currentTime))
        //                            {
        //                                // נסה תרגיל חילופי
        //                                visited.Add(alternativeId);
        //                                path.Add(alternativeId);

        //                                // עדכון הזמן לתרגיל הבא
        //                                var nextTime = currentTime.AddMinutes(alternativeExercise.UsageDurationMinutes);

        //                                // חפש את המסלול הבא
        //                                if (Backtrack(graph, exerciseOrder, alternativeDevices, visited, path, nextTime, out finishTime))
        //                                    return true;

        //                                // אם התרגיל החילופי לא הצליח, נחזור אחורה
        //                                visited.Remove(alternativeId);
        //                                path.RemoveAt(path.Count - 1);
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            finishTime = currentTime;
        //            return false;
        //        }


        //private bool Backtrack(
        //    SubGraph graph, // הגרף של המתאמן
        //    List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
        //    Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
        //    HashSet<int> visited, // התרגילים שביקרנו
        //    List<int> path, // מסלול
        //    DateTime currentTime, // תאריך התחלה
        //    out DateTime finishTime) // תאריך סיום
        //{
        //    // אם כל התרגילים בוצעו, נעדכן את זמן הסיום
        //    if (visited.Count == exerciseOrder.Count)
        //    {
        //        finishTime = currentTime;
        //        return true;
        //    }

        //    // ננסה כל תרגיל שנשאר ברשימה
        //    for (int i = 0; i < exerciseOrder.Count; i++)
        //    {
        //        var exerciseId = exerciseOrder[i];

        //        if (visited.Contains(exerciseId))
        //            continue;

        //        // נוודא שהתרגיל קיים בגרף
        //        var exercise = graph.Nodes.FirstOrDefault(n => n.Id == exerciseId);
        //        if (exercise != null && exercise.IsAvailableAt(currentTime))
        //        {
        //            // אם התרגיל זמין, נוסיף אותו למסלול
        //            visited.Add(exerciseId);
        //            path.Add(exerciseId);

        //            // עדכון הזמן לתרגיל הבא
        //            var nextTime = currentTime.AddMinutes(exercise.UsageDurationMinutes);

        //            // חפש את המסלול הבא
        //            if (Backtrack(graph, exerciseOrder.Where(e => e != exerciseId).ToList(), alternativeDevices, visited, path, nextTime, out finishTime))
        //                return true;

        //            // אם לא הצלחנו, נחזור אחורה
        //            visited.Remove(exerciseId);
        //            path.RemoveAt(path.Count - 1);
        //        }
        //        else
        //        {
        //            // אם התרגיל לא זמין, ננסה מכשירים חילופיים
        //            if (alternativeDevices.ContainsKey(exerciseId))
        //            {
        //                foreach (var alternativeId in alternativeDevices[exerciseId])
        //                {
        //                    var alternativeExercise = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
        //                    if (alternativeExercise != null && !visited.Contains(alternativeId) && alternativeExercise.IsAvailableAt(currentTime))
        //                    {
        //                        // נסה תרגיל חילופי
        //                        visited.Add(alternativeId);
        //                        path.Add(alternativeId);

        //                        // עדכון הזמן לתרגיל הבא
        //                        var nextTime = currentTime.AddMinutes(alternativeExercise.UsageDurationMinutes);

        //                        // חפש את המסלול הבא
        //                        if (Backtrack(graph, exerciseOrder.Where(e => e != exerciseId).ToList(), alternativeDevices, visited, path, nextTime, out finishTime))
        //                            return true;

        //                        // אם תרגיל חילופי לא הצליח, נחזור אחורה
        //                        visited.Remove(alternativeId);
        //                        path.RemoveAt(path.Count - 1);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    finishTime = currentTime;
        //    return false;
        //}



        //מעולה רק בלי ENUM של סוגי הצמתים
        //private bool Backtrack(
        //    SubGraph graph, // הגרף של המתאמן
        //    List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
        //    Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
        //    HashSet<int> visited, // התרגילים שביקרנו
        //    List<int> path, // מסלול
        //    DateTime currentTime, // תאריך התחלה
        //    out DateTime finishTime // תאריך סיום
        //)
        //{
        //    // אם כל התרגילים בוצעו, נעדכן את זמן הסיום
        //    if (visited.Count == exerciseOrder.Count)
        //    {
        //        finishTime = currentTime;
        //        return true;
        //    }

        //    // ננסה כל תרגיל שנשאר ברשימה
        //    for (int i = 0; i < exerciseOrder.Count; i++)
        //    {
        //        var exerciseId = exerciseOrder[i];

        //        if (visited.Contains(exerciseId))
        //            continue;

        //        // נוודא שהתרגיל קיים בגרף
        //        var exercise = graph.Nodes.FirstOrDefault(n => n.Id == exerciseId);
        //        if (exercise != null && exercise.IsAvailableAt(currentTime))
        //        {
        //            // אם התרגיל זמין, נוסיף אותו למסלול
        //            visited.Add(exerciseId);
        //            path.Add(exerciseId);

        //            // עדכון הזמן לתרגיל הבא
        //            var nextTime = currentTime.AddMinutes(exercise.UsageDurationMinutes);

        //            // חפש את המסלול הבא
        //            if (Backtrack(graph, exerciseOrder.Where(e => e != exerciseId).ToList(), alternativeDevices, visited, path, nextTime, out finishTime))
        //                return true;

        //            // אם לא הצלחנו, נחזור אחורה
        //            visited.Remove(exerciseId);
        //            path.RemoveAt(path.Count - 1);
        //        }
        //        else
        //        {
        //            // בדוק אם ניתן להגיע לתרגיל הזה מאוחר יותר
        //            if (!CanReachNode(graph, exerciseId, exerciseOrder.Where(e => !visited.Contains(e) && e != exerciseId).ToList()))
        //            {
        //                // אם אי אפשר להגיע אליו ואין מכשיר חילופי, החזר FALSE
        //                if (!alternativeDevices.ContainsKey(exerciseId) || alternativeDevices[exerciseId].Count == 0)
        //                {
        //                    finishTime = currentTime;
        //                    return false;
        //                }
        //                // אם אי אפשר להגיע אליו, ננסה מכשיר חילופי
        //                foreach (var alternativeId in alternativeDevices[exerciseId])
        //                {
        //                    var alternativeExercise = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
        //                    if (alternativeExercise != null && !visited.Contains(alternativeId) && alternativeExercise.IsAvailableAt(currentTime))
        //                    {
        //                        // נסה תרגיל חילופי
        //                        visited.Add(alternativeId);
        //                        path.Add(alternativeId);

        //                        // עדכון הזמן לתרגיל הבא
        //                        var nextTime = currentTime.AddMinutes(alternativeExercise.UsageDurationMinutes);

        //                        // חפש את המסלול הבא
        //                        if (Backtrack(graph, exerciseOrder.Where(e => e != exerciseId).ToList(), alternativeDevices, visited, path, nextTime, out finishTime))
        //                            return true;

        //                        // אם תרגיל חילופי לא הצליח, נחזור אחורה
        //                        visited.Remove(alternativeId);
        //                        path.RemoveAt(path.Count - 1);
        //                    }
        //                }

        //                //// אם אין תרגיל חילופי זמין, החזר FALSE
        //                //finishTime = currentTime;
        //                //return false;
        //            }
        //            else
        //            {
        //                // אם אפשר להגיע אליו, נסה לשנות את הסדר ולהמשיך
        //                var remainingExercises = exerciseOrder.Where(e => !visited.Contains(e)).ToList();
        //                if (Backtrack(graph, remainingExercises, alternativeDevices, visited, path, currentTime, out finishTime))
        //                    return true;
        //            }
        //        }
        //    }

        //    finishTime = currentTime;
        //    return false;
        //}


        //private bool Backtrack(
        //    SubGraph graph, // הגרף של המתאמן
        //    List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
        //    Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
        //    HashSet<int> visitedExercises, // התרגילים שביקרנו (רק מסוג Exercise)
        //    List<int> path, // מסלול
        //    DateTime currentTime, // תאריך התחלה
        //    out DateTime finishTime // תאריך סיום
        //)
        //{
        //    // אם כל התרגילים מסוג Exercise בוצעו, נעדכן את זמן הסיום
        //    if (visitedExercises.Count == exerciseOrder.Count)
        //    {
        //        finishTime = currentTime;
        //        return true;
        //    }

        //    // ננסה כל תרגיל שנשאר ברשימה
        //    for (int i = 0; i < exerciseOrder.Count; i++)
        //    {
        //        var nodeId = exerciseOrder[i];

        //        var node = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
        //        if (node == null)
        //            continue;

        //        // אם זה צומת מסוג Exercise וביקרנו בו כבר, נדלג עליו
        //        if (node.Type == NodeType.Exsercise && visitedExercises.Contains(nodeId))
        //            continue;

        //        // אם הצומת מסוג Exercise, נוודא שהוא זמין
        //        if (node.Type == NodeType.Exsercise && !node.IsAvailableAt(currentTime))
        //        {
        //            // בדוק אם ניתן להגיע אליו מאוחר יותר
        //            if (!CanReachNode(graph, nodeId, exerciseOrder.Where(e => !visitedExercises.Contains(e) && e != nodeId).ToList()))
        //            {
        //                // אם אי אפשר להגיע אליו ואין מכשיר חילופי, החזר FALSE
        //                if (!alternativeDevices.ContainsKey(nodeId) || alternativeDevices[nodeId].Count == 0)
        //                {
        //                    finishTime = currentTime;
        //                    return false;
        //                }

        //                // נסה מכשיר חילופי
        //                foreach (var alternativeId in alternativeDevices[nodeId])
        //                {
        //                    var alternativeNode = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
        //                    if (alternativeNode != null && !visitedExercises.Contains(alternativeId) && alternativeNode.IsAvailableAt(currentTime))
        //                    {
        //                        // נסה תרגיל חילופי
        //                        visitedExercises.Add(alternativeId);
        //                        path.Add(alternativeId);

        //                        var nextTime = currentTime.AddMinutes(alternativeNode.UsageDurationMinutes);

        //                        if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
        //                            return true;

        //                        // אם תרגיל חילופי לא הצליח, נחזור אחורה
        //                        visitedExercises.Remove(alternativeId);
        //                        path.RemoveAt(path.Count - 1);
        //                    }
        //                }

        //                // אם אין תרגיל חילופי זמין, החזר FALSE
        //                finishTime = currentTime;
        //                return false;
        //            }
        //            else
        //            {
        //                // אם אפשר להגיע אליו, נסה לשנות את הסדר ולהמשיך
        //                var remainingNodes = exerciseOrder.Where(e => !visitedExercises.Contains(e)).ToList();
        //                if (Backtrack(graph, remainingNodes, alternativeDevices, visitedExercises, path, currentTime, out finishTime))
        //                    return true;
        //            }
        //        }
        //        else
        //        {
        //            // אם הצומת מסוג MuscleGroup, אין צורך לבדוק זמינות ואין צורך להכניס לסט visited
        //            if (node.Type == NodeType.MuscleGroup || (node.Type == NodeType.Exsercise && node.IsAvailableAt(currentTime)))
        //            {
        //                if (node.Type == NodeType.Exsercise)
        //                    visitedExercises.Add(nodeId); // הוסף רק צמתים מסוג Exercise לסט visited

        //                path.Add(nodeId);

        //                // עדכון הזמן לתרגיל הבא
        //                var nextTime = currentTime.AddMinutes(node.UsageDurationMinutes);

        //                // חפש את המסלול הבא
        //                if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
        //                    return true;

        //                // אם לא הצלחנו, נחזור אחורה
        //                if (node.Type == NodeType.Exsercise)
        //                    visitedExercises.Remove(nodeId); // הסר רק צמתים מסוג Exercise מ-visited

        //                path.RemoveAt(path.Count - 1);
        //            }
        //        }
        //    }

        //    finishTime = currentTime;
        //    return false;
        //}


        ////הוספה של הצמתים השרירים למסלול
        //private bool Backtrack(
        //    SubGraph graph, // הגרף של המתאמן
        //    List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
        //    Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
        //    HashSet<int> visitedExercises, // התרגילים שביקרנו (רק מסוג Exercise)
        //    List<int> path, // מסלול
        //    DateTime currentTime, // תאריך התחלה
        //    out DateTime finishTime // תאריך סיום
        //)
        //{
        //    // אם כל התרגילים מסוג Exercise בוצעו, נעדכן את זמן הסיום
        //    if (visitedExercises.Count == exerciseOrder.Count)
        //    {
        //        finishTime = currentTime;
        //        return true;
        //    }

        //    // ננסה כל תרגיל שנשאר ברשימה
        //    for (int i = 0; i < exerciseOrder.Count; i++)
        //    {
        //        var nodeId = exerciseOrder[i];

        //        var node = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
        //        if (node == null)
        //            continue;

        //        // אם זה צומת מסוג Exercise וביקרנו בו כבר, נדלג עליו
        //        if (node.Type == NodeType.Exsercise && visitedExercises.Contains(nodeId))
        //            continue;

        //        // אם הצומת מסוג Exercise, נוודא שהוא זמין
        //        if (node.Type == NodeType.Exsercise && !node.IsAvailableAt(currentTime))
        //        {
        //            // בדוק אם ניתן להגיע אליו מאוחר יותר
        //            if (!CanReachNode(graph, nodeId, exerciseOrder.Where(e => !visitedExercises.Contains(e) && e != nodeId).ToList()))
        //            {
        //                // אם אי אפשר להגיע אליו ואין מכשיר חילופי, החזר FALSE
        //                if (!alternativeDevices.ContainsKey(nodeId) || alternativeDevices[nodeId].Count == 0)
        //                {
        //                    finishTime = currentTime;
        //                    return false;
        //                }

        //                // נסה מכשיר חילופי
        //                foreach (var alternativeId in alternativeDevices[nodeId])
        //                {
        //                    var alternativeNode = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
        //                    if (alternativeNode != null && !visitedExercises.Contains(alternativeId) && alternativeNode.IsAvailableAt(currentTime))
        //                    {
        //                        // נסה תרגיל חילופי
        //                        visitedExercises.Add(alternativeId);
        //                        path.Add(alternativeId);

        //                        var nextTime = currentTime.AddMinutes(alternativeNode.UsageDurationMinutes);

        //                        if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
        //                            return true;

        //                        // אם תרגיל חילופי לא הצליח, נחזור אחורה
        //                        visitedExercises.Remove(alternativeId);
        //                        path.RemoveAt(path.Count - 1);
        //                    }
        //                }

        //                // אם אין תרגיל חילופי זמין, החזר FALSE
        //                finishTime = currentTime;
        //                return false;
        //            }
        //            else
        //            {
        //                // אם אפשר להגיע אליו, נסה לשנות את הסדר ולהמשיך
        //                var remainingNodes = exerciseOrder.Where(e => !visitedExercises.Contains(e)).ToList();
        //                if (Backtrack(graph, remainingNodes, alternativeDevices, visitedExercises, path, currentTime, out finishTime))
        //                    return true;
        //            }
        //        }
        //        else
        //        {
        //            // אם הצומת מסוג MuscleGroup, אין צורך לבדוק זמינות ואין צורך להכניס לסט visited
        //            if (node.Type == NodeType.MuscleGroup || (node.Type == NodeType.Exsercise && node.IsAvailableAt(currentTime)))
        //            {
        //                if (node.Type == NodeType.Exsercise)
        //                    visitedExercises.Add(nodeId); // הוסף רק צמתים מסוג Exercise לסט visited

        //                path.Add(nodeId);

        //                // **מעבר בצמתים מסוג MuscleGroup**
        //                // נבדוק את כל השכנים של הצומת הנוכחי
        //                foreach (var neighbor in node.Neighbors)
        //                {
        //                    if (neighbor.Type == NodeType.MuscleGroup && !path.Contains(neighbor.Id))
        //                    {
        //                        path.Add(neighbor.Id); // הוסף את צומת השריר למסלול
        //                    }
        //                }

        //                // עדכון הזמן לתרגיל הבא
        //                var nextTime = currentTime.AddMinutes(node.UsageDurationMinutes);

        //                // חפש את המסלול הבא
        //                if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
        //                    return true;

        //                // אם לא הצלחנו, נחזור אחורה
        //                if (node.Type == NodeType.Exsercise)
        //                    visitedExercises.Remove(nodeId); // הסר רק צמתים מסוג Exercise מ-visited

        //                path.RemoveAt(path.Count - 1);
        //            }
        //        }
        //    }

        //    finishTime = currentTime;
        //    return false;
        //}

        //שילוב של בדיקה אם לעבור לצומת של השריר
        private bool Backtrack(
            SubGraph graph, // הגרף של המתאמן
            List<int> exerciseOrder, // רשימת התרגילים מהתוכנית הדיפולטיבית
            Dictionary<int, List<int>> alternativeDevices, // מכשירים חילופיים לכל תרגיל
            HashSet<int> visitedExercises, // התרגילים שביקרנו (רק מסוג Exercise)
            List<int> path, // מסלול
            DateTime currentTime, // תאריך התחלה
            out DateTime finishTime // תאריך סיום
        )
        {
            // אם כל התרגילים מסוג Exercise בוצעו, נעדכן את זמן הסיום
            if (visitedExercises.Count == exerciseOrder.Count)
            {
                finishTime = currentTime;
                return true;
            }

            // ננסה כל תרגיל שנשאר ברשימה
            for (int i = 0; i < exerciseOrder.Count; i++)
            {
                var nodeId = exerciseOrder[i];
                var node = graph.Nodes.FirstOrDefault(n => n.Id == nodeId);
                if (node == null)
                    continue;

                // אם זה צומת מסוג Exercise וביקרנו בו כבר, נדלג עליו
                if (node.Type == NodeType.Exsercise && visitedExercises.Contains(nodeId))
                    continue;

                // אם הצומת מסוג Exercise, נוודא שהוא זמין
                if (node.Type == NodeType.Exsercise && !node.IsAvailableAt(currentTime))
                {
                    // בדוק אם ניתן להגיע אליו מאוחר יותר
                    if (!CanReachNode(graph, nodeId, exerciseOrder.Where(e => !visitedExercises.Contains(e) && e != nodeId).ToList()))
                    {
                        // אם אי אפשר להגיע אליו ואין מכשיר חילופי, החזר FALSE
                        if (!alternativeDevices.ContainsKey(nodeId) || alternativeDevices[nodeId].Count == 0)
                        {
                            finishTime = currentTime;
                            return false;
                        }

                        // נסה מכשיר חילופי
                        foreach (var alternativeId in alternativeDevices[nodeId])
                        {
                            var alternativeNode = graph.Nodes.FirstOrDefault(n => n.Id == alternativeId);
                            if (alternativeNode != null && !visitedExercises.Contains(alternativeId) && alternativeNode.IsAvailableAt(currentTime))
                            {
                                // נסה תרגיל חילופי
                                visitedExercises.Add(alternativeId);
                                path.Add(alternativeId);

                                var nextTime = currentTime.AddMinutes(alternativeNode.UsageDurationMinutes);

                                if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
                                    return true;

                                // אם תרגיל חילופי לא הצליח, נחזור אחורה
                                visitedExercises.Remove(alternativeId);
                                path.RemoveAt(path.Count - 1);
                            }
                        }

                        // אם אין תרגיל חילופי זמין, החזר FALSE
                        finishTime = currentTime;
                        return false;
                    }
                    else
                    {
                        // אם אפשר להגיע אליו, נסה לשנות את הסדר ולהמשיך
                        var remainingNodes = exerciseOrder.Where(e => !visitedExercises.Contains(e)).ToList();
                        if (Backtrack(graph, remainingNodes, alternativeDevices, visitedExercises, path, currentTime, out finishTime))
                            return true;
                    }
                }
                else
                {
                    // אם הצומת מסוג MuscleGroup, אין צורך לבדוק זמינות ואין צורך להכניס לסט visited
                    if (node.Type == NodeType.MuscleGroup || (node.Type == NodeType.Exsercise && node.IsAvailableAt(currentTime)))
                    {
                        if (node.Type == NodeType.Exsercise)
                            visitedExercises.Add(nodeId); // הוסף רק צמתים מסוג Exercise לסט visited

                        path.Add(nodeId);

                        // **מעבר ישיר בין תרגילים**
                        // בדוק אם ניתן לעבור ישירות לתרגיל הבא ברשימה
                        if (i + 1 < exerciseOrder.Count)
                        {
                            var nextExerciseId = exerciseOrder[i + 1];
                            var nextExerciseNode = graph.Nodes.FirstOrDefault(n => n.Id == nextExerciseId);
                            if (nextExerciseNode != null && node.Neighbors.Contains(nextExerciseNode))
                            {
                                // אם יש מעבר ישיר, המשך לתרגיל הבא
                                continue;
                            }
                        }

                        ///******************
                        // **מעבר בצמתים מסוג MuscleGroup**
                        // נבדוק את כל השכנים של הצומת הנוכחי
                        foreach (var neighbor in node.Neighbors)
                        {
                            if (neighbor.Type == NodeType.MuscleGroup && !path.Contains(neighbor.Id))
                            {
                                path.Add(neighbor.Id); // הוסף את צומת השריר למסלול
                            }
                        }
                       
                        // עדכון הזמן לתרגיל הבא
                        var nextTime = currentTime.AddMinutes(node.UsageDurationMinutes);

                        // חפש את המסלול הבא
                        if (Backtrack(graph, exerciseOrder.Where(e => e != nodeId).ToList(), alternativeDevices, visitedExercises, path, nextTime, out finishTime))
                            return true;

                        // אם לא הצלחנו, נחזור אחורה
                        if (node.Type == NodeType.Exsercise)
                            visitedExercises.Remove(nodeId); // הסר רק צמתים מסוג Exercise מ-visited

                        path.RemoveAt(path.Count - 1);
                    }
                }
            }

            finishTime = currentTime;
            return false;
        }
        private bool CanReachNode(SubGraph graph, int targetNodeId, List<int> remainingNodes)
        {
            // רשימת צמתים לביקור
            var toVisit = new Queue<int>(remainingNodes);
            var visited = new HashSet<int>();

            while (toVisit.Count > 0)
            {
                var currentNodeId = toVisit.Dequeue();

                // אם הגענו לצומת היעד
                if (currentNodeId == targetNodeId)
                    return true;

                // הוסף את הצומת הנוכחי לרשימת ביקור
                visited.Add(currentNodeId);

                // הוסף את כל השכנים של הצומת הנוכחי לתור, אם הם לא ביקרו בהם
                var currentNode = graph.Nodes.FirstOrDefault(n => n.Id == currentNodeId);
                if (currentNode != null)
                {
                    foreach (var neighbor in currentNode.Neighbors)
                    {
                        if (!visited.Contains(neighbor.Id) && remainingNodes.Contains(neighbor.Id))
                        {
                            toVisit.Enqueue(neighbor.Id);
                        }
                    }
                }
            }

            // אם לא מצאנו דרך לצומת היעד
            return false;
        }
    }

}




//private bool Backtrack(
//    SubGraph graph,
//    List<int> exerciseOrder,
//    HashSet<int> visited,
//    List<int> path,
//    DateTime currentTime,
//    out DateTime finishTime
//)
//{
//    if (visited.Count == exerciseOrder.Count)
//    {
//        finishTime = currentTime;
//        return true;
//    }

//    foreach (var machineId in exerciseOrder)
//    {
//        if (visited.Contains(machineId))
//            continue;

//        var machine = graph.Nodes[machineId];

//        if (!machine.IsAvailableAt(currentTime))
//            continue;

//        visited.Add(machineId);
//        path.Add(machineId);
//        var newTime = currentTime.AddMinutes(machine.UsageDurationMinutes);

//        if (Backtrack(graph, exerciseOrder, visited, path, newTime, out finishTime))
//            return true;

//        // Backtrack
//        visited.Remove(machineId);
//        path.RemoveAt(path.Count - 1);
//    }

//    finishTime = currentTime;
//    return false;
//}