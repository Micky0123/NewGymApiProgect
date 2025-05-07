using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class DayEntry1
    {
        public string Key { get; set; }
        public string Values { get; set; }
    }
    public class TryProgram
    {
        // פונקציה לשליפת רשימות של ימים
        public List<List<DayEntry1>> ExtractDayListsNew(IXLWorksheet worksheet, int daysInWeek, int trainingDuration)
        {
            var dayLists = new List<List<DayEntry1>>(); // רשימה של רשימות DayEntry

            // מציאת העמודות בשורה הראשונה (Header) עם הערך daysInWeek
            var matchingColumns = worksheet.Row(1).CellsUsed()
                .Where(cell => cell.GetValue<int>() == daysInWeek)
                .Select(cell => cell.Address.ColumnNumber)
                .ToList();

            // עבור כל עמודה שמתאימה לערך daysInWeek
            foreach (var col in matchingColumns)
            {
                var dayEntries = new List<DayEntry1>(); // רשימה עבור עמודה נוכחית

                // מעבר על כל השורות מתחת לכותרת
                foreach (var row in worksheet.RowsUsed().Skip(1))
                {
                    var duration = row.Cell(1).GetValue<int>(); // קבלת הערך בעמודה הראשונה (Training Duration)

                    // בדיקה אם משך הזמן מתאים
                    if (duration == trainingDuration)
                    {
                        var key = row.Cell(1).GetValue<string>(); // קבלת הערך בעמודה הראשונה (Key)
                        var value = row.Cell(col).GetValue<string>(); // קבלת הערך בעמודה הנוכחית (Value)

                        // יצירת אובייקט DayEntry והוספתו לרשימה
                        dayEntries.Add(new DayEntry1
                        {
                            Key = key,
                            Values = value
                        });
                    }
                }

                // הוספת הרשימה של העמודה הנוכחית לרשימה הכללית
                dayLists.Add(dayEntries);
            }

            return dayLists;
        }

        // פונקציה לשליפת כל הערכים בעמודה מסוימת
        public List<object> ExtractColumnValues(IXLWorksheet worksheet, int column)
        {
            return worksheet.Column(column).CellsUsed().Skip(1).Select(c => (object)c.Value).ToList();
        }
    }



}
