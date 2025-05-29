using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    // מכיל מידע על תרגיל ושריר קשור
    public class ExerciseWithMuscleInfo
    {
        public ExerciseDTO Exercise { get; set; }// פרטי התרגיל
        public string MuscleName { get; set; }// שם השריר הראשי
        public string SubMuscleName { get; set; }// שם תת-השריר
        public int categoryId { get; set; }// מזהה הקטגוריה
        public int JointCount { get; set; }// מספר המפרקים המעורבים בתרגיל
        public int Time { get; set; } = 5;//❤
    }
}
