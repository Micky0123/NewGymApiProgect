using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    /// מכיל את כל הפרמטרים הנדרשים ליצירת תוכנית אימונים
    public class TrainingParams
    {
        public List<List<DayEntry>> DayLists { get; set; }// רשימה של רשימות יומיות - כל רשימה פנימית מייצגת יום אימון
        public Dictionary<string, List<object>> MuscleSizeData { get; set; }// מילון המכיל מידע על גדלי השרירים (גדול/בינוני/קטן)
        public int MinRep { get; set; }// מספר החזרות המינימלי בתרגיל
        public int MaxRep { get; set; }// מספר החזרות המקסימלי בתרגיל
        public Dictionary<(string Category, string MuscleSize), int> TimeCategories { get; set; }// מילון המגדיר זמן אימון לפי קטגוריה וגודל שריר
        public List<string> TypMuscle { get; set; }// רשימה של סוגי השרירים השונים
        public List<Dictionary<int, List<string>>> TypeMuscleData { get; set; }// רשימה של סוגי השרירים לאימון מסודרת לפי חשיבות
        public List<string> equipment { get; set; }// רשימת הציוד הזמין למתאמן
        public List<string> NeedSubMuscleList { get; set; }// רשימת השרירים הדורשים תת-שריר
        public List<string> subMuscleOfMuscleList { get; set; }// רשימת כל השרירים שיש להם תת-שריר
        public Dictionary<int, string> OrderList { get; set; }// מילון המגדיר את סדר התרגילים בתוכנית
        public List<string> musclePriorityOrder { get; set; }// רשימת עדיפויות השרירים הראשיים
        public List<string> subMusclePriorityOrder { get; set; }// רשימת עדיפויות תת-השרירים
    }
}
