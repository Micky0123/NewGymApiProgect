//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using BLL;
//using DTO;

//namespace BLL
//{
//    public class PathFinderService
//    {
//        // מתודת העטיפה הראשית - מוצאת מסלול תרגילים תקף למתאמן
//        public PathResult FindValidPath(
//            //SubGraph subGraph, //שימי ❤ נראה לי שצריך את כל הגרף
//            ExerciseTransition exerciseTransition,//המטריצה
//            List<int> exerciseOrder,//רשימת התרגילים למתאמן
//            DateTime startTime,//זמן תחילת אימון
//            TraineeDTO trainee,//מתאמן
//            Dictionary<int, List<int>> alternativeDevices = null//מילון תרגילים חילופיים???????????
//            )
//        {
//            //if (subGraph == null)
//            //    throw new ArgumentNullException(nameof(subGraph));
//            if (exerciseOrder == null || exerciseOrder.Count == 0)
//                throw new ArgumentException("רשימת התרגילים ריקה", nameof(exerciseOrder));

//           var currentTrainee = trainee ?? throw new ArgumentNullException(nameof(trainee));

//            //??????????????
//            if (alternativeDevices == null)
//                alternativeDevices = new Dictionary<int, List<int>>();

//            // שלב 1: לבנות מילון צמתים לגישה מהירה
//           //במקום זה יש טבלה
//           //var nodeDict = subGraph.Nodes.ToDictionary(n => n.Id);

//            // שלב 2: לבנות מטריצת נגישות
//           // var reachableFromNode = BuildReachabilityMatrix(nodeDict.Values);

//            int mask = 0; // ייצוג בינארי של אילו תרגילים כבר בוצעו
//            var path = new List<int>(); // המסלול הנבחר

//            // מחיקת המילון הקודם לפני חיפוש חדש
//            //לא בטוח ----
//            memo.Clear();

//            // שלב 3: קריאה לאלגוריתם הרקורסיבי
//            var (found, bestPath, finalTime) =Backtracking.BacktrackWithPriority(
//                subGraph,
//                nodeDict,
//                reachableFromNode,
//                exerciseOrder,
//                alternativeDevices,
//                mask,
//                path,
//                0,
//                startTime,
//                out DateTime endTime
//            );

//            // אם לא נמצא מסלול, נזרוק חריגה
//            if (!found || bestPath == null)
//                throw new InvalidOperationException("לא נמצא מסלול חוקי לפי ההגבלות.");

//            // בניית תוצאת המסלול המלא
//            return new PathResult
//            {
//                Trainee = trainee,
//                ExerciseIdsInPath = bestPath,
//                StartTime = startTime,
//                EndTime = finalTime,
//                AlternativesUsed = CalculateAlternativesUsed(bestPath, exerciseOrder)
//            };
//        }

//        // שימי ❤ צריך לשנות את הפונקציה
//        // פונקציית עזר לחישוב כמה חילופים נעשו
//        private int CalculateAlternativesUsed(List<int> actualPath, List<int> originalOrder)
//        {
//            int alternatives = 0;
//            for (int i = 0; i < Math.Min(actualPath.Count, originalOrder.Count); i++)
//            {
//                if (actualPath[i] != originalOrder[i])
//                    alternatives++;
//            }
//            return alternatives;
//        }


//    }
//}
