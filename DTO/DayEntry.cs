using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    // מייצג יום אימון בתוכנית
    public class DayEntry
    {
        public string Key { get; set; }// קטגוריה - סוג השריר או קבוצת השרירים
        public int Values { get; set; }// מספר התרגילים הנדרש לשריר זה
        public string Name { get; set; }// שם השריר
    }
}
