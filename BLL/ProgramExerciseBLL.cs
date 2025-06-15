using AutoMapper;
using DBEntities.Models;
using DTO;
using IBLL;
using IDAL;
using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using static BLL.TrainingParams;
using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;

namespace BLL
{

   //מיכלוש שימי לב שצריך לקבל את השמות ושהם לא יהיו קבועים//****************************

   public class DayEntry
   {
       //קטגוריה
       public string Key { get; set; }
       //מספר התרגילים לשריר 
       public int Values { get; set; }
       //שם השריר
       public string Name { get; set; }
   }

   public class ExerciseWithMuscleInfo
   {
       public ExerciseDTO Exercise { get; set; }
       public string MuscleName { get; set; }
       public string SubMuscleName { get; set; }
       public int categoryId { get; set; }
       public int JointCount { get; set; } // Add JointCount property
   }
   public class TrainingParams
   {
       public List<List<DayEntry>> DayLists { get; set; } // רשימה של רשימות אובייקטים מסוג DayEntry
       public Dictionary<string, List<object>> MuscleSizeData { get; set; } // מילון דינאמי לגודל שרירים
       public int MinRep { get; set; }
       public int MaxRep { get; set; }
       public Dictionary<(string Category, string MuscleSize), int> TimeCategories { get; set; }
       public List<string> TypMuscle { get; set; }//רשימה של סוגי שרירים
       public List<Dictionary<int, List<string>>> TypeMuscleData { get; set; }//רשימה של סוגי השרירים לאימון מסודרת לפי חשיבות
       public List<string> equipment { get; set; }//רשימה של ציוד למתאמן
       public List<string> NeedSubMuscleList { get; set; }//רשימה של סוגי השרירים שצריכים תת שריר
       public List<string> subMuscleOfMuscleList { get; set; }//רשימה של כל השרירים שיש להם תת שריר
       public Dictionary<int, string> OrderList { get; set; }//דיקשינרי של סדר התרגילים בתוכנית
       public List<string> musclePriorityOrder { get; set; }//רשימה של כל השרירים שיש להם תת שריר
       public List<string> subMusclePriorityOrder { get; set; }//רשימה של כל השרירים שיש להם תת שריר
   }

   public class ProgramExerciseBLL //: //IProgramExerciseBLL
   {
       private readonly IMuscleTypeDAL muscleTypeDAL;
       private readonly IMuscleDAL muscleDAL;
       private readonly IEquipmentDAL equipmentDAL;
       private readonly IExerciseDAL exerciseDAL;
       private readonly IProgramExerciseDAL programExerciseDAL;
       private readonly ILogger<ProgramExerciseBLL> logger;
       private readonly IMapper mapper;
       private readonly List<string> equipmentList;
       private readonly ITrainingDurationDAL trainingDurationDAL;
       private readonly ICategoryDAL categoryDAL;
       private readonly ISubMuscleDAL subMuscleDAL;

       // Sheet names
       //**********מיכלוש שימי לב שצריך לקבל את השמות ושהם לא יהיו קבועים//****************************
       private const string DaysInWeekSheet = "List1";
       private const string MuscleSheet = "MuscleSize";
       private const string SmallMuscleSheet = "SmallMuscle";
       private const string RepitByGoalSheet = "Repeat";
       private const string CategorySheet = "TimeForCategory";
       private const string SumOfLargeByTimeSheet = "Amount";
       private const string SumOfSmallByTimeSheet = "Amount";
       private const string TypMuscleSheet = "MuscleType";
       private const string EquipmentSheet = "Equipment";
       private const string NeedSubMuscleSheet = "NeedSubMuscle";
       private const string subMusclePriorityOrder = "OrderList";
       private const string musclePriorityOrder = "OrderList";

       private const string OrderListSheet = "OrderList";

       public ProgramExerciseBLL(IMuscleDAL muscleDAL, ILogger<ProgramExerciseBLL> logger, ISubMuscleDAL subMuscleDAL, ICategoryDAL categoryDAL, IMuscleTypeDAL muscleTypeDAL, IProgramExerciseDAL programExerciseDAL, ITrainingDurationDAL trainingDurationDAL, IEquipmentDAL equipmentDAL, IExerciseDAL exerciseDAL)
       {
           this.muscleDAL = muscleDAL;
           this.equipmentDAL = equipmentDAL;
           this.muscleTypeDAL = muscleTypeDAL;
           this.exerciseDAL = exerciseDAL;
           this.trainingDurationDAL = trainingDurationDAL;
           this.categoryDAL = categoryDAL;
           this.logger = logger;
           this.equipmentList = new List<string>();
           this.programExerciseDAL = programExerciseDAL;
           this.subMuscleDAL = subMuscleDAL;


           var configTaskConverter = new MapperConfiguration(cfg =>
           {
               cfg.CreateMap<Muscle, MuscleDTO>().ReverseMap();
           });
           mapper = new Mapper(configTaskConverter);
           this.exerciseDAL = exerciseDAL;
       }

       public async Task addProgramExerciseAsync1(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time1, int traineeID)
       {
           //**********מיכלוש שימי לב שצריך לקבל את השמות ושהם לא יהיו קבועים/***************************
           string filePath1 = @"C:\Users\user\Pictures\תכנות\שנה ב\פוריקט שנתי\C#\פרויקט חדש 20.04\Gym_Api\BLL\new.xlsx";
           var equi = await equipmentDAL.GetAllEquipmentsAsync();
           foreach (var e in equi)
           {
               this.equipmentList.Add(e.EquipmentName);
           }
           var trainingParams = new TrainingParams
           {
               DayLists = new List<List<DayEntry>>(),
               MuscleSizeData = new Dictionary<string, List<object>>(),
               MinRep = 0,
               MaxRep = 0,
               TimeCategories = new Dictionary<(string Category, string MuscleSize), int>(),
               TypMuscle = new List<string>(),
               TypeMuscleData = new List<Dictionary<int, List<string>>>(),
               equipment = new List<string>(),
               NeedSubMuscleList = new List<string>(),
           };

           var time = await trainingDurationDAL.GetTrainingDurationIDByValue(time1);
           // שליפת כל הפרמטרים מהקובץ
           trainingParams = await GetAllParams(filePath1, daysInWeek, goal, level, time1);

           if (trainingParams == null)
           {
               throw new Exception("Failed to retrieve training parameters.");
           }

           trainingParams.DayLists = trainingParams.DayLists.Select(list => list.Where(de => de.Values != 0).ToList()).ToList();
           for (int i = 0; i < trainingParams.DayLists.Count; i++)
           {
               // הדפסת הכותרת של DayList
               logger.LogInformation($"Day List {i + 1}:");

               // מעבר על כל Entry בתוך DayList הנוכחי
               foreach (var entry in trainingParams.DayLists[i])
               {
                   // הדפסת ה-Key וה-Values
                   logger.LogInformation($"Key: {entry.Key}, Values: {entry.Values}");
               }
           }
           // הדפסת מספר החזרות
           logger.LogInformation($"Repetitions: {trainingParams.MinRep}-{trainingParams.MaxRep}");

           // הדפסת הקטגוריות והכמות לכל שריר
           logger.LogInformation("Time Categories: " + string.Join(", ", trainingParams.TimeCategories.Select(kvp => $"[{kvp.Key.Category} - {kvp.Key.MuscleSize}: {kvp.Value}]")));

           //logger.LogInformation("List of MuscleType: " + string.Join(", ", trainingParams.TypMuscle));
           foreach (var dict in trainingParams.TypeMuscleData)
           {
               foreach (var pair in dict)
               {
                   logger.LogInformation($"Muscle Type: {pair.Key} - Values: {string.Join(", ", pair.Value)}");
               }
           }
           logger.LogInformation("List of Need SubMuscle: " + string.Join(", ", trainingParams.NeedSubMuscleList));
           logger.LogInformation("List of equipment: " + string.Join(", ", trainingParams.equipment));
           //logger.LogInformation("List of subMuscleOfMuscleList: " + string.Join(", ", trainingParams.subMuscleOfMuscleList));
           foreach (var dict in trainingParams.subMuscleOfMuscleList)
           {
               logger.LogInformation($"List of subMuscleOfMuscleList: {dict}");
           }


           var ListOfProgram = await GenerateOptimizedExercisePlanAsync(trainingParams);
           await SaveProgramDefaultAsync(trainingParams, ListOfProgram, traineeID, programName: "Default Training Program", daysInWeek, goal, level, time);
       }

       public async Task<TrainingParams> GetAllParams(string filePath, int daysInWeek, int goal, int level, int time)
       {
           try
           {
               using (var workbook = new XLWorkbook(filePath))
               {
                   // שליפת מידע על הימים
                   var daysWorksheet = GetWorksheet(workbook, DaysInWeekSheet);
                   //var dayLists = ExtractDayLists(daysWorksheet, daysInWeek);
                   var dayListsNew = ExtractDayListsNew(daysWorksheet, daysInWeek, time);

                   // קריאת מידע על גדלי השרירים
                   var muscleSizeWorksheet = GetWorksheet(workbook, MuscleSheet);
                   var muscleSizeData = ExtractMuscleSizeData(muscleSizeWorksheet);
                   // מספר החזרות
                   var repWorksheet = GetWorksheet(workbook, RepitByGoalSheet);
                   int colRep = FindColumnByValue(repWorksheet, 1, goal, "Column with the specified goal not found.");
                   int minRep = GetValueFromWorksheet(repWorksheet, "min", colRep, "Min value not found.");
                   int maxRep = GetValueFromWorksheet(repWorksheet, "max", colRep, "Max value not found.");

                   // זמן לקטגוריה
                   var timeCategoryWorksheet = GetWorksheet(workbook, CategorySheet);
                   var timeCategoryList = ExtractTimeCategoryList(timeCategoryWorksheet, goal, time);

                   // הכמות לשרירים
                   var countToMuscleWorksheet = GetWorksheet(workbook, SumOfLargeByTimeSheet);
                   var categoryMuscleSizeData = ExtractCategoryMuscleSizeData(countToMuscleWorksheet, time, daysInWeek);

                   //סוג השרירים
                   var typMuscleWorksheet = GetWorksheet(workbook, TypMuscleSheet);
                   var typMuscleData = ExtractTypMuscleData(typMuscleWorksheet, daysInWeek);

                   //ציוד
                   var equipmentWorksheet = GetWorksheet(workbook, EquipmentSheet);
                   var equipmentData = ExtractEquipmentData(equipmentWorksheet, level);

                   var muscleTypeWorksheet = GetWorksheet(workbook, NeedSubMuscleSheet);
                   var muscleTypeData = ExtractMuscleTypeData(muscleTypeWorksheet);

                   var subMuscleOfMuscleListData = await muscleDAL.GetMusclesOfSubMuscle();

                   var orderListSheet = GetWorksheet(workbook, OrderListSheet);
                   var orderListData = ExtractOrderListData(orderListSheet);

                   var MorderListSheet = GetWorksheet(workbook, OrderListSheet);
                   var subMuscleData = ExtractSubMuscleOrderListData(MorderListSheet);
                   var MuscleData = ExtractMuscleOrderListData(MorderListSheet);
                   // החזרת כל הנתונים
                   return new TrainingParams
                   {
                       DayLists = dayListsNew,
                       MuscleSizeData = muscleSizeData,
                       MinRep = minRep,
                       MaxRep = maxRep,
                       TimeCategories = categoryMuscleSizeData,
                       TypeMuscleData = typMuscleData,
                       equipment = equipmentData,
                       NeedSubMuscleList = muscleTypeData,
                       subMuscleOfMuscleList = subMuscleOfMuscleListData,
                       OrderList = orderListData,
                       subMusclePriorityOrder = subMuscleData,
                       musclePriorityOrder = MuscleData,
                   };
               }
           }
           catch (Exception ex)
           {
               logger.LogError(ex, "Error while processing training parameters.");
               throw;
           }
       }

       private Dictionary<string, List<object>> ExtractMuscleSizeData(IXLWorksheet worksheet)
       {
           var muscleSizeData = new Dictionary<string, List<object>>();

           // מעבר על כל העמודות בשורה הראשונה
           foreach (var col in worksheet.ColumnsUsed())
           {
               string muscleSize = col.FirstCell().GetValue<string>(); // גודל השריר בשורה הראשונה
               var muscles = col.CellsUsed().Skip(1).Select(c => (object)c.Value).ToList(); // רשימת השרירים תחת גודל זה
               muscleSizeData[muscleSize] = muscles;
           }

           return muscleSizeData;
       }

       private List<string> ExtractMuscleTypeData(IXLWorksheet worksheet)
       {
           var muscleTypeListData = new List<string>();

           // מעבר על כל העמודות בשורה הראשונה
           foreach (var row in worksheet.RowsUsed())
           {
               muscleTypeListData.Add(row.Cell(1).Value.ToString());
           }

           return muscleTypeListData;
       }
       private Dictionary<int, string> ExtractOrderListData(IXLWorksheet worksheet)
       {
           var orderListData = new Dictionary<int, string>();

           // מעבר על כל העמודות בשורה הראשונה
           foreach (var row in worksheet.RowsUsed())
           {
               orderListData.Add(row.Cell(1).GetValue<int>(), row.Cell(2).Value.ToString());
           }
           return orderListData;
       }
       private List<string> ExtractMuscleOrderListData(IXLWorksheet worksheet)
       {
           var MuscleOrderListData = new List<string>();

           // מעבר על כל העמודות בשורה הראשונה
           foreach (var row in worksheet.RowsUsed().Skip(1))
           {
               MuscleOrderListData.Add(row.Cell(2).Value.ToString());
           }
           return MuscleOrderListData;
       }
       private List<string> ExtractSubMuscleOrderListData(IXLWorksheet worksheet)
       {
           var SubMuscleOrderListData = new List<string>();

           // מעבר על כל העמודות בשורה הראשונה
           foreach (var row in worksheet.RowsUsed().Skip(1))
           {
               SubMuscleOrderListData.Add(row.Cell(3).Value.ToString());
           }
           return SubMuscleOrderListData;
       }
       private IXLWorksheet GetWorksheet(XLWorkbook workbook, string sheetName)
       {
           var worksheet = workbook.Worksheet(sheetName);
           if (worksheet == null)
           {
               logger.LogWarning($"Worksheet '{sheetName}' not found.");
               throw new Exception($"Worksheet '{sheetName}' does not exist.");
           }
           return worksheet;
       }
       private int FindColumnByValue(IXLWorksheet worksheet, int headerRow, int value, string errorMessage)
       {
           foreach (var col in worksheet.ColumnsUsed())
           {
               if (col.Cell(headerRow).GetValue<int>() == value)
               {
                   return col.ColumnNumber();
               }
           }
           logger.LogWarning(errorMessage);
           throw new Exception(errorMessage);
       }
       private int GetValueFromWorksheet(IXLWorksheet worksheet, string rowText, int column, string errorMessage)
       {
           foreach (var row in worksheet.RowsUsed())
           {
               if (row.Cell(1).GetValue<string>() == rowText)
               {
                   return row.Cell(column).GetValue<int>();
               }
           }
           logger.LogWarning(errorMessage);
           throw new Exception(errorMessage);
       }

       private List<string> ExtractEquipmentData(IXLWorksheet worksheet, int level)
       {
           var listEquipment = new List<string>();
           foreach (var row in worksheet.RowsUsed().Skip(1)) // מעבר על כל השורות
           {
               if (row.Cell(1).GetValue<int>() == level)
               {
                   var value = row.Cell(2).GetValue<string>(); // הערך מהעמודה שנמצאה
                   listEquipment.Add(value);
               }
           }
           return listEquipment;
       }

       private List<Dictionary<int, List<string>>> ExtractTypMuscleData(IXLWorksheet worksheet, int daysInWeek)
       {
           var OrderListOfTypeMuscle = new List<Dictionary<int, List<string>>>();

           foreach (var row in worksheet.RowsUsed().Skip(1))
           {
               if (row.Cell(1).GetValue<int>() == daysInWeek)
               {
                   var dict = new Dictionary<int, List<string>>();
                   for (int col = 3; col <= row.LastCellUsed().Address.ColumnNumber; col++)
                   {
                       if (dict.ContainsKey(row.Cell(2).GetValue<int>()))
                       {
                           dict[row.Cell(2).GetValue<int>()].Add(row.Cell(col).GetValue<string>());
                       }
                       else
                       {
                           dict.Add(row.Cell(2).GetValue<int>(), new List<string> { row.Cell(col).GetValue<string>() });
                       }
                   }
                   OrderListOfTypeMuscle.Add(dict);
               }
           }

           return OrderListOfTypeMuscle;
       }

       private Dictionary<(string Category, string MuscleSize), int> ExtractCategoryMuscleSizeData(IXLWorksheet worksheet, int time, int daysInWeek)
       {
           var categoryMuscleSizeData = new Dictionary<(string Category, string MuscleSize), int>();

           // סינון שורות שבהן עמודה A (משך הזמן) ועמודה D (מספר ימים) תואמות לפרמטרים שנתנו
           var matchingRows = worksheet.RowsUsed().Skip(1) // דילוג על השורה הראשונה (שורת הכותרות)
               .Where(row => row.Cell(1).GetValue<int>() == time && row.Cell(4).GetValue<int>() == daysInWeek)
               .ToList();

           if (!matchingRows.Any())
           {
               logger.LogWarning($"No rows found with time {time} and daysInWeek {daysInWeek} in the sheet.");
               throw new Exception($"No rows found with time {time} and daysInWeek {daysInWeek} in the sheet.");
           }

           // מעבר על כל השורות שזוהו והתאמת הנתונים למבנה ה-Dictionary
           foreach (var row in matchingRows)
           {
               string category = row.Cell(2).GetValue<string>(); // קטגוריה בעמודה B
               string muscleSize = row.Cell(3).GetValue<string>(); // גודל שריר בעמודה C
               int quantity = row.Cell(5).GetValue<int>(); // כמות בעמודה E

               // מפתח ה-Dictionary הוא שילוב של הקטגוריה וגודל השריר
               var key = (Category: category, MuscleSize: muscleSize);

               // הוספת הנתונים ל-Dictionary
               if (!categoryMuscleSizeData.ContainsKey(key))
               {
                   categoryMuscleSizeData[key] = quantity;
               }
               else
               {
                   logger.LogWarning($"Duplicate entry found for category '{category}' and muscle size '{muscleSize}'. Only the first occurrence is retained.");
               }
           }

           return categoryMuscleSizeData;
       }
       private Dictionary<string, int> ExtractTimeCategoryList(IXLWorksheet worksheet, int goal, int time)
       {
           var timeCategoryList = new Dictionary<string, int>();

           // סינון שורות שבהן עמודה A (משך זמן) ועמודה B (מטרה) תואמות לפרמטרים שהוזנו
           var matchingRows = worksheet.RowsUsed()
               .Where(row => row.Cell(1).GetValue<int>() == time && row.Cell(2).GetValue<int>() == goal)
               .ToList();

           if (!matchingRows.Any())
           {
               logger.LogWarning($"No rows found with time {time} and goal {goal} in the Time sheet.");
               throw new Exception($"No rows found with time {time} and goal {goal} in the Time sheet.");
           }

           // מעבר על כל השורות שזוהו והתאמת הנתונים למבנה ה-Dictionary
           foreach (var row in matchingRows)
           {
               // יצירת מפתח (קטגוריה) משילוב של הערכים בעמודות C, D
               string category = $"{row.Cell(3).GetValue<string>()}, {row.Cell(4).GetValue<string>()}";

               // הוספת קטגוריה ל-Dictionary עם ערך דיפולטיבי (למשל 1)
               if (!timeCategoryList.ContainsKey(category))
               {
                   timeCategoryList[category] = 1; // ערך דיפולטיבי
               }
               else
               {
                   logger.LogWarning($"Duplicate entry found for category '{category}'. Only the first occurrence is retained.");
               }
           }

           return timeCategoryList;
       }

       public async Task<List<ExerciseDTO>> GetExercisesForMuscleAsync(string muscleName, int count)
       {
           // שליפת תרגילים מה-DAL
           var exercises = await muscleDAL.GetExercisesForMuscleAsync(muscleName);

           // המרת התרגילים ל-DTO
           var exerciseDTOs = exercises.Select(e => new ExerciseDTO
           {
               ExerciseId = e.ExerciseId,
               ExerciseName = e.ExerciseName
           }).ToList();

           // בחר מספר תרגילים באופן אקראי
           //return exerciseDTOs.OrderBy(e => Guid.NewGuid()).Take(count).ToList();
           // אני מנסה שהוא יחזיר את כל התרגילים ואח"כ הוא יבחר ברשימת התרגילים הסופית
           return exerciseDTOs;
       }
       public async Task<List<ExerciseDTO>> GetExercisesForMuscleAndTypeAsync(string MuscleName, string TypeMuscle, int count, List<int> allowedEquipment)
       {
           try
           {
               var exercises = await muscleDAL.GetExercisesForMuscleAndTypeAsync(MuscleName, TypeMuscle, allowedEquipment);

               var exerciseDTOs = exercises.Select(e => new ExerciseDTO
               {
                   ExerciseId = e.ExerciseId,
                   ExerciseName = e.ExerciseName
               }).ToList();

               //return exerciseDTOs.OrderBy(e => Guid.NewGuid()).Take(count).ToList();
               return exerciseDTOs;
           }
           catch (Exception ex)
           {
               logger.LogError(ex, $"Error while fetching exercises for sub-muscle: {MuscleName}");
               throw;
           }
       }

       public async Task<List<ExerciseDTO>> GetExercisesForSubMuscleAsync(string subMuscleName, List<string> allowedEquipment)
       {
           try
           {
               // שליפת התרגילים שמתאימים לתת-השריר מה-DAL
               var exercises = await muscleDAL.GetExercisesForSubMuscleAsync(subMuscleName);

               // סינון לפי רשימת הציוד המותר
               //var filteredExercises = exercises.Where(e =>
               //    e.Equipment.Any(eq => allowedEquipment.Contains(eq.EquipmentName))
               //).ToList();

               // המרת התרגילים ל-DTO
               var exerciseDTOs = exercises.Select(e => new ExerciseDTO
               {
                   ExerciseId = e.ExerciseId,
                   ExerciseName = e.ExerciseName
               }).ToList();

               // החזרת מספר התרגילים הנדרש (נבחרים באופן אקראי)
               return exerciseDTOs;
           }
           catch (Exception ex)
           {
               logger.LogError(ex, $"Error while fetching exercises for sub-muscle: {subMuscleName}");
               throw;
           }
       }

       public List<List<DayEntry>> ExtractDayListsNew(IXLWorksheet worksheet, int daysInWeek, int trainingDuration)
       {
           var dayLists = new List<List<DayEntry>>(); // רשימה של רשימות DayEntry

           // מציאת העמודות בשורה השניה (Header) עם הערך daysInWeek
           var matchingColumns = worksheet.Row(2).CellsUsed()
               .Where(cell => cell.GetValue<int>() == daysInWeek)
               .Select(cell => cell.Address.ColumnNumber)
               .ToList();

           // עבור כל עמודה שמתאימה לערך daysInWeek
           foreach (var col in matchingColumns)
           {
               var dayEntries = new List<DayEntry>(); // רשימה עבור עמודה נוכחית

               // מעבר על כל השורות מתחת לכותרת
               foreach (var row in worksheet.RowsUsed().Skip(2))
               {
                   var duration = row.Cell(1).GetValue<int>(); // קבלת הערך בעמודה הראשונה (Training Duration)

                   // בדיקה אם משך הזמן מתאים
                   if (duration == trainingDuration)
                   {
                       var key = row.Cell(2).GetValue<string>(); // קבלת הערך בעמודה השניה (Key)
                       var value = row.Cell(col).GetValue<int>(); // קבלת הערך בעמודה הנוכחית (Value)
                       var name = row.Cell(3).GetValue<string>(); // קבלת הערך בעמודה השלישית (Name)
                                                                  // יצירת אובייקט DayEntry והוספתו לרשימה
                       dayEntries.Add(new DayEntry
                       {
                           Key = key,
                           Values = value,
                           Name = name
                       });
                   }
               }

               // הוספת הרשימה של העמודה הנוכחית לרשימה הכללית
               dayLists.Add(dayEntries);
           }

           return dayLists;
       }
       public Task SaveDefaultProgramAsync(IBLL.TrainingParams trainingParams, int traineeId, string programName)
       {
           throw new NotImplementedException();
       }


       public async Task<List<List<ExerciseWithMuscleInfo>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
       {
           var exercisePlan = new List<List<ExerciseWithMuscleInfo>>(); // תוכנית האימונים לכל הימים
           var usedExercisesOverall = new HashSet<int>(); // רשימת מזהי תרגילים שכבר נבחרו בכל התוכנית
           var usedExercisesBySubMuscle = new Dictionary<string, int>(); // מיפוי של תרגילים שנבחרו עבור תתי-שרירים
           var typeMuscleList = new List<string>();

           // יצירת רשימת סוגי השרירים
           foreach (var dict in trainingParams.TypeMuscleData)
           {
               foreach (var pair in dict)
               {
                   foreach (var TypeValue in pair.Value)
                   {
                       typeMuscleList.Add(TypeValue);
                   }
               }
               break;
           }

           foreach (var dayList in trainingParams.DayLists) // מעבר על כל יום
           {
               var dayExercises = new List<ExerciseWithMuscleInfo>();
               var usedExercisesForDay = new HashSet<int>(); // רשימת מזהי תרגילים שכבר נבחרו עבור היום הנוכחי

               foreach (var muscleEntry in dayList) // מעבר על כל שריר ברשימה
               {
                   string muscleName = muscleEntry.Key;
                   int exerciseCount = muscleEntry.Values;
                   string typeMuscle = muscleEntry.Name;
                   var category = await categoryDAL.GetCategoryByNameAsync(typeMuscle);
                   var categoryID = category.CategoryId;
                   // בדיקת תת-שרירים אם נדרש
                   if (trainingParams.subMuscleOfMuscleList.Contains(muscleName))
                   {
                       var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscleName);

                       foreach (var subMuscle in subMuscles)
                       {
                           if (exerciseCount <= 0) break;

                           // מציאת תרגילים שעובדים על תת-שריר זה
                           var exercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, trainingParams.equipment);

                           // סינון התרגילים כך שלא יחזרו על עצמם בתתי-שרירים אחרים
                           var filteredExercises = exercises
                               .Where(e => !usedExercisesForDay.Contains(e.ExerciseId) &&
                                           (!usedExercisesBySubMuscle.ContainsKey(subMuscle.SubMuscleName) ||
                                            usedExercisesBySubMuscle[subMuscle.SubMuscleName] == e.ExerciseId))
                               .OrderBy(e => Guid.NewGuid()) // רנדומליות בבחירת התרגילים
                               .ToList();

                           // בחירת תרגיל מתאים
                           //foreach (var exercise in filteredExercises)
                           //{
                           //    dayExercises.Add(new ExerciseWithMuscleInfo
                           //    {
                           //        Exercise = exercise,
                           //        MuscleName = muscleName,
                           //        SubMuscleName = subMuscle.SubMuscleName
                           //    });

                           //    usedExercisesForDay.Add(exercise.ExerciseId);
                           //    usedExercisesOverall.Add(exercise.ExerciseId);
                           //    usedExercisesBySubMuscle[subMuscle.SubMuscleName] = exercise.ExerciseId;
                           //    exerciseCount--;

                           //    if (exerciseCount <= 0) break;
                           //}
                           foreach (var exercise in filteredExercises)
                           {
                               dayExercises.Add(new ExerciseWithMuscleInfo
                               {
                                   Exercise = exercise,
                                   MuscleName = muscleName,
                                   SubMuscleName = subMuscle.SubMuscleName,
                                   JointCount = await exerciseDAL.GetJointCount(exercise.ExerciseId),
                                   categoryId = categoryID
                               });

                               usedExercisesForDay.Add(exercise.ExerciseId);
                               usedExercisesOverall.Add(exercise.ExerciseId);
                               usedExercisesBySubMuscle[subMuscle.SubMuscleName] = exercise.ExerciseId;
                               exerciseCount--;

                               break;
                           }

                       }
                   }

                   // אם עדיין חסרים תרגילים (לשריר הראשי), חפש תרגילים רגילים
                   if (exerciseCount > 0)
                   {
                       var allEquipmentExercises = await GetExercisesForMuscleAsync(muscleName, exerciseCount);
                       var filteredExercises = allEquipmentExercises
                           .Where(e => !usedExercisesForDay.Contains(e.ExerciseId))
                           .OrderBy(e => Guid.NewGuid()) // רנדומליות בבחירת התרגילים
                           .ToList();

                       foreach (var exercise in filteredExercises)
                       {
                           dayExercises.Add(new ExerciseWithMuscleInfo
                           {
                               Exercise = exercise,
                               MuscleName = muscleName,
                               SubMuscleName = null, // אין תת-שריר במקרה הזה
                               categoryId = categoryID,
                               JointCount = await exerciseDAL.GetJointCount(exercise.ExerciseId)
                           });

                           usedExercisesForDay.Add(exercise.ExerciseId);
                           usedExercisesOverall.Add(exercise.ExerciseId);
                           exerciseCount--;

                           if (exerciseCount <= 0) break;
                       }
                   }
               }

               exercisePlan.Add(dayExercises); // הוספת תרגילי היום לתוכנית
           }

           // מיון התרגילים לפי סדר חשיבות של השרירים ותתי-השרירים
           var musclePriorityOrder = trainingParams.musclePriorityOrder;
           var subMusclePriorityOrder = trainingParams.subMusclePriorityOrder;

           exercisePlan = await SortExercisesByPriorityAsync(exercisePlan, musclePriorityOrder, subMusclePriorityOrder);
           //var sortedExercises = SortExercisesByMuscleGroupAndJoints(exercisePlan);
           // הדפסת התוכנית ללוג
           for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
           {
               logger.LogInformation($"Day {dayIndex + 1} Exercises:");
               foreach (var exerciseInfo in exercisePlan[dayIndex])
               {
                   logger.LogInformation($"  - {exerciseInfo.Exercise.ExerciseName} (ID: {exerciseInfo.Exercise.ExerciseId}, Muscle: {exerciseInfo.MuscleName}, SubMuscle: {exerciseInfo.SubMuscleName})");
               }
           }

           return exercisePlan;
       }

       public async Task<List<List<ExerciseWithMuscleInfo>>> SortExercisesByPriorityAsync(
   List<List<ExerciseWithMuscleInfo>> exercisePlan,
   List<string> musclePriorityOrder,
   List<string> subMusclePriorityOrder)
       {
           // מעבר על כל יום בתוכנית האימונים
           for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
           {
               var dayExercises = exercisePlan[dayIndex];

               // יצירת קבוצות לפי muscleIndex
               var groupedExercises = dayExercises
                   .Select(detail => new
                   {
                       Detail = detail,
                       MuscleIndex = musclePriorityOrder.IndexOf(detail.MuscleName ?? string.Empty),
                       SubMuscleIndex = subMusclePriorityOrder.IndexOf(detail.SubMuscleName ?? string.Empty)
                   })
                   .GroupBy(item => item.MuscleIndex)
                   .OrderBy(group => group.Key == -1 ? int.MaxValue : group.Key); // סדר לפי muscleIndex

               var sortedExercises = new List<ExerciseWithMuscleInfo>();

               // מעבר על כל קבוצה ומיון לפי תתי-שרירים ואז לפי JointCount
               foreach (var group in groupedExercises)
               {
                   var sortedGroup = group
                       .OrderBy(item => item.MuscleIndex == -1 ? int.MaxValue : item.MuscleIndex) // סדר לפי subMuscleIndex
                                                                                                  // .OrderBy(item => item.SubMuscleIndex == -1 ? int.MaxValue : item.SubMuscleIndex) // סדר לפי subMuscleIndex
                       .ThenByDescending(item => item.Detail.JointCount) // סדר יורד לפי JointCount
                       .Select(item => item.Detail)
                       .ToList();

                   sortedExercises.AddRange(sortedGroup);
               }

               // עדכון רשימת התרגילים עבור היום
               exercisePlan[dayIndex] = sortedExercises;
           }

           return exercisePlan;
       }

       public async Task SaveProgramDefaultAsync(TrainingParams trainingParams, List<List<ExerciseWithMuscleInfo>> listOfProgram, int traineeId, string programName, int daysInWeek, int goal, int level, int time)
       {
           try
           {
               // יצירת תוכנית חדשה
               var trainingProgram = new TrainingProgram
               {
                   TraineeId = traineeId,
                   ProgramName = programName,
                   CreationDate = DateTime.Now,
                   TrainingDateTime = DateTime.Now,
                   LastUpdateDate = DateTime.Now,
                   IsDefaultProgram = true, // מציין שזו תוכנית דיפולטיבית
                   ParentProgramId = null,
                   IsHistoricalProgram = false,
               };


               // שמירת התוכנית ב-DAL וקבלת ProgramID
               int programId = await programExerciseDAL.SaveTrainingProgramAsync(trainingProgram);

               var defaultPrograms = new DefaultProgram
               {
                   GoalId = goal,
                   TrainingDays = daysInWeek,
                   TrainingDurationId = time,
                   FitnessLevelId = level,
                   ProgramId = programId,
               };
               int defaultProgramID = await programExerciseDAL.SaveDefaultProgramsAsync(defaultPrograms);
               // הכנת רשימת התרגילים לשמירה
               var programExercises = new List<ProgramExercise>();
               int y = 0;
               foreach (var (dayIndex, dayExercises) in listOfProgram.Select((day, index) => (index, day)))
               {
                   int i = 0;
                   foreach (var exerciseEntry in dayExercises)
                   {
                       var exerciseId = exerciseEntry.Exercise.ExerciseId;
                       var muscleId = await muscleDAL.GetIdOfMuscleByNameAsync(exerciseEntry.MuscleName);
                       int? subMuscleId = null; // שימוש ב-Nullable<int> (int?) כדי לאפשר ערכי NULL
                       if (!string.IsNullOrEmpty(exerciseEntry.SubMuscleName))
                       {
                           subMuscleId = await subMuscleDAL.GetIdOfSubMuscleByNameAsync(exerciseEntry.SubMuscleName);
                       }

                       var programExercise = new ProgramExercise
                       {
                           ProgramId = programId,
                           ExerciseId = exerciseId, // מזהה התרגיל
                           ProgramSets = 3, // מספר הסטים
                           ProgramRepetitionsMin = trainingParams.MinRep,
                           ProgramRepetitionsMax = trainingParams.MaxRep,
                           ProgramWeight = 10,
                           ExerciseOrder = i + 1, // סדר היום בתוכנית
                           DayOrder = y + 1,
                           CategoryId = exerciseEntry.categoryId,
                           TimesMin = 5,
                           TimesMax = 10,
                           MuscleId = muscleId,
                           SubMuscleId = subMuscleId,
                       };

                       programExercises.Add(programExercise);
                       i++;
                   }
                   y++;
               }

               // שמירת התרגילים ב-DAL
               await programExerciseDAL.SaveProgramExercisesAsync(programExercises);

               logger.LogInformation("Default training program and exercises saved successfully.");
           }
           catch (Exception ex)
           {
               logger.LogError(ex, "Error occurred while saving the default training program.");
               throw;
           }
       }

       public Task UpdateProgramExerciseAsync(ProgramExerciseDTO programExercise, int id)
       {
           throw new NotImplementedException();
       }

       public Task DeleteProgramExerciseAsync(int id)
       {
           throw new NotImplementedException();
       }

       public Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise)
       {
           throw new NotImplementedException();
       }

       public Task ReadDataFromExcelAsync(string filePath)
       {
           throw new NotImplementedException();
       }
       public Task<List<ProgramExerciseDTO>> GetAllProgramExercisesAsync()
       {
           throw new NotImplementedException();
       }

       public Task<ProgramExerciseDTO> GetProgramExerciseByIdAsync(int id)
       {
           throw new NotImplementedException();
       }

       public Task<ProgramExerciseDTO> GetProgramExerciseByNameAsync(string name)
       {
           throw new NotImplementedException();
       }






   }
}


//עם לוגים לבדיקה איזה תת שריר
//public async Task<List<List<ExerciseDTO>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>(); // תוכנית האימונים לכל הימים
//    var usedExercisesOverall = new HashSet<int>(); // רשימת מזהי תרגילים שכבר נבחרו בכל התוכנית
//    var usedExercisesBySubMuscle = new Dictionary<string, int>(); // מיפוי של תרגילים שנבחרו עבור תתי-שרירים

//    foreach (var dayList in trainingParams.DayLists) // מעבר על כל יום
//    {
//        var dayExercises = new List<ExerciseDTO>();
//        var usedExercisesForDay = new HashSet<int>(); // רשימת מזהי תרגילים שכבר נבחרו עבור היום הנוכחי

//        foreach (var muscleEntry in dayList) // מעבר על כל שריר ברשימה
//        {
//            string muscleName = muscleEntry.Key;
//            int exerciseCount = muscleEntry.Values;
//            logger.LogInformation($"Processing muscle: {muscleName} with {exerciseCount} exercises needed.");

//            בדיקת תתי-שרירים
//            if (trainingParams.subMuscleOfMuscleList.Contains(muscleName))
//            {
//                var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscleName);
//                logger.LogInformation($"Found {subMuscles.Count} sub-muscles for muscle '{muscleName}'.");

//                foreach (var subMuscle in subMuscles)
//                {
//                    if (exerciseCount <= 0) break;

//                    logger.LogInformation($"Processing sub-muscle: {subMuscle.SubMuscleName}.");

//                    שליפת תרגילים לתת - שריר
//                    var exercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, trainingParams.equipment);
//                    logger.LogInformation($"Found {exercises.Count} exercises for sub-muscle '{subMuscle.SubMuscleName}'.");

//                    סינון תרגילים
//                    var filteredExercises = exercises
//                        .Where(e => !usedExercisesForDay.Contains(e.ExerciseId) &&
//                                    (!usedExercisesBySubMuscle.ContainsKey(subMuscle.SubMuscleName) ||
//                                     usedExercisesBySubMuscle[subMuscle.SubMuscleName] == e.ExerciseId))
//                        .ToList();

//                    logger.LogInformation($"After filtering, {filteredExercises.Count} exercises remain for sub-muscle '{subMuscle.SubMuscleName}'.");

//                    בחירת תרגיל
//                    if (filteredExercises.Count > 0)
//                    {
//                        var exercise = filteredExercises.First(); // בחר תרגיל ראשון מתוך המסוננים
//                        dayExercises.Add(new ExerciseDTO { ExerciseId = exercise.ExerciseId, ExerciseName = exercise.ExerciseName });
//                        usedExercisesForDay.Add(exercise.ExerciseId);
//                        usedExercisesOverall.Add(exercise.ExerciseId);
//                        usedExercisesBySubMuscle[subMuscle.SubMuscleName] = exercise.ExerciseId;
//                        exerciseCount--;
//                        logger.LogInformation($"Selected exercise '{exercise.ExerciseName}' (ID: {exercise.ExerciseId}) for sub-muscle '{subMuscle.SubMuscleName}'.");
//                    }
//                    else
//                    {
//                        logger.LogWarning($"No valid exercises found for sub-muscle '{subMuscle.SubMuscleName}'.");
//                    }
//                }
//            }

//            אם לא נשארו מספיק תרגילים
//            if (exerciseCount > 0)
//            {
//                logger.LogWarning($"Not enough exercises found for muscle '{muscleName}'. {exerciseCount} exercises still needed.");
//            }
//        }

//        exercisePlan.Add(dayExercises); // הוספת תרגילי היום לתוכנית
//        logger.LogInformation($"Day {exercisePlan.Count} completed with {dayExercises.Count} exercises.");
//    }

//    הדפסת תוכנית האימון ללוג
//    for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
//    {
//        logger.LogInformation($"Day {dayIndex + 1} Exercises:");
//        foreach (var exercise in exercisePlan[dayIndex])
//        {
//            logger.LogInformation($"  - {exercise.ExerciseName} (ID: {exercise.ExerciseId})");
//        }
//    }

//    return exercisePlan;
//}

//public async Task<List<List<ExerciseDTO>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<(ExerciseDTO Exercise, string TargetMuscle, string Category)>>(); // תוכנית האימונים לכל הימים
//    var usedExercises = new HashSet<int>(); // שמירה על מזהי תרגילים שכבר נבחרו
//    var typeMuscleList = new List<string>();

//    foreach (var dict in trainingParams.TypeMuscleData)
//    {
//        foreach (var pair in dict)
//        {
//            foreach (var typeValue in pair.Value)
//            {
//                typeMuscleList.Add(typeValue);
//            }
//        }
//        break;
//    }

//    foreach (var dayList in trainingParams.DayLists) // מעבר על כל יום
//    {
//        var dayExercises = new List<(ExerciseDTO Exercise, string TargetMuscle, string Category)>();

//        foreach (var muscleEntry in dayList) // מעבר על כל שריר ברשימה
//        {
//            string muscleName = muscleEntry.Key;
//            int exerciseCount = muscleEntry.Values;
//            string categoryName = muscleEntry.Name; // הקטגוריה של השריר הספציפי

//            // בדיקת תת-שרירים אם נדרש
//            foreach (var typeMuscle in typeMuscleList)
//            {
//                if (trainingParams.NeedSubMuscleList.Contains(typeMuscle) && trainingParams.subMuscleOfMuscleList.Contains(muscleName))
//                {
//                    var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscleName);
//                    if (subMuscles.Count > 0) // רק אם יש תת-שרירים
//                    {
//                        foreach (var subMuscle in subMuscles)
//                        {
//                            if (exerciseCount <= 0) break;

//                            var exercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, 1, trainingParams.equipment);
//                            var filteredExercises = exercises.Where(e => !usedExercises.Contains(e.ExerciseId)).ToList();

//                            foreach (var exercise in filteredExercises)
//                            {
//                                dayExercises.Add((exercise, muscleName, categoryName));
//                                usedExercises.Add(exercise.ExerciseId);
//                                exerciseCount--;

//                                if (exerciseCount <= 0) break;
//                            }
//                        }
//                    }
//                }
//            }

//            // אם נשארו עוד תרגילים שצריך, בדוק סוגי שרירים אחרים
//            if (exerciseCount > 0)
//            {
//                //var equipment1 = trainingParams.equipment.Select(async name => (await equipmentDAL.GetEquipmentByNameAsync(name)).EquipmentId).ToList();
//                var equipment = new List<int>();
//                foreach (var equipmentName in trainingParams.equipment)
//                {
//                    var e = await equipmentDAL.GetEquipmentByNameAsync(equipmentName);
//                    equipment.Add(e.EquipmentId);
//                }

//                foreach (var muscleType in trainingParams.TypeMuscleData.SelectMany(d => d))
//                {
//                    foreach (var muscleTypeName in muscleType.Value)
//                    {
//                        var exercises = await GetExercisesForMuscleAndTypeAsync(muscleName, muscleTypeName, exerciseCount, equipment);
//                        var filteredExercises = exercises.Where(e => !usedExercises.Contains(e.ExerciseId)).ToList();

//                        foreach (var exercise in filteredExercises)
//                        {
//                            ///****
//                            dayExercises.Add((exercise, muscleName, categoryName));
//                            usedExercises.Add(exercise.ExerciseId);
//                            exerciseCount--;

//                            if (exerciseCount <= 0) break;
//                        }
//                    }
//                    if (exerciseCount <= 0) break;
//                }
//            }

//            // אם עדיין חסרים תרגילים, חפש בכל הציוד
//            if (exerciseCount > 0)
//            {
//                var allEquipmentExercises = await GetExercisesForMuscleAsync(muscleName, exerciseCount);
//                foreach (var exercise in allEquipmentExercises)
//                {
//                    if (usedExercises.Contains(exercise.ExerciseId)) continue;

//                    dayExercises.Add((exercise, muscleName, categoryName));
//                    usedExercises.Add(exercise.ExerciseId);
//                    exerciseCount--;

//                    if (exerciseCount <= 0) break;
//                }
//            }
//        }

//        exercisePlan.Add(dayExercises); // הוספת תרגילי היום לתוכנית
//    }

//    //var mappedExercises = exercisePlan(dayExercises =>SortExercisesByOrderListAndCategory(
//    //         dayExercises,
//    //         trainingParams.OrderList,
//    //         new List<string> { "חימום", "כוח", "אירובי" } // סדר הקטגוריות
//    //     )
//    // ).ToList();
//    // var sortedPlan = SortExercisesByOrderListAndCategory(mappedExercises, trainingParams.OrderList, new List<string> { "חימום", "כוח", "אירובי" });

//    // //// סידור התרגילים לפי סדר השרירים וסדר הקטגוריות
//    // //var sortedPlan = exercisePlan.Select(dayExercises =>
//    // //    SortExercisesByOrderListAndCategory(
//    // //        dayExercises,
//    // //        trainingParams.OrderList,
//    // //        //************************************
//    // //        new List<string> { "חימום", "כוח", "אירובי" } // סדר הקטגוריות
//    // //    )
//    // //).ToList();

//    // // הדפסת התוכנית ללוג
//    // for (int dayIndex = 0; dayIndex < sortedPlan.Count; dayIndex++)
//    // {
//    //     logger.LogInformation($"Day {dayIndex + 1} Exercises:");
//    //     foreach (var exercise in sortedPlan[dayIndex])
//    //     {
//    //         logger.LogInformation($"  - {exercise.Exercise.ExerciseName} (ID: {exercise.Exercise.ExerciseId}), Target Muscle: {exercise.TargetMuscle}, Category: {exercise.Category}");
//    //     }
//    // }

//    // // המרה לרשימת ExerciseDTO בלבד
//    // return sortedPlan.Select(day => day.Select(e => e.Exercise).ToList()).ToList();

//    var mappedExercises = exercisePlan.Select(dayExercises =>
//        SortExercisesByOrderListAndCategory(
//            dayExercises,
//            trainingParams.OrderList,
//            new List<string> { "חימום", "כוח", "אירובי" } // סדר הקטגוריות
//        )
//    ).ToList();
//    var sortedPlan = mappedExercises.SelectMany(x => x).ToList();

//    // הדפסת התוכנית ללוג
//    for (int dayIndex = 0; dayIndex < sortedPlan.Count; dayIndex++)
//    {
//        logger.LogInformation($"Day {dayIndex + 1} Exercises:");
//        foreach (var exercise in sortedPlan[dayIndex])
//        {
//            logger.LogInformation($"  - {exercise.Exercise.ExerciseName} (ID: {exercise.Exercise.ExerciseId}), Target Muscle: {exercise.TargetMuscle}, Category: {exercise.Category}");
//        }
//    }

//    // המרה לרשימת ExerciseDTO בלבד
//    return sortedPlan.Select(exercise => new List<ExerciseDTO> { exercise.Exercise }).ToList();
//}

//לא עובדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדדד
// סידור התרגילים לפי OrderList
//foreach (var dayExercises in exercisePlan)
//{
//    dayExercises.Sort((exercise1, exercise2) =>
//    {
//        // חיפוש המיקום של שם התרגיל במילון OrderList
//        int order1 = trainingParams.OrderList.FirstOrDefault(kvp => kvp.Value == exercise1.ExerciseName).Key;
//        int order2 = trainingParams.OrderList.FirstOrDefault(kvp => kvp.Value == exercise2.ExerciseName).Key;

//        // אם לא נמצא במילון, מיקום מקסימלי (בסוף הרשימה)
//        if (order1 == 0) order1 = int.MaxValue;
//        if (order2 == 0) order2 = int.MaxValue;

//        return order1.CompareTo(order2);
//    });
//}



//10/05
//public List<Exercise> SortExercisesByOrderListAndCategory(List<(Exercise Exercise, string TargetMuscle, string Category)> exercisesWithDetails,
//                                                            Dictionary<int, string> orderList, List<string> categoryOrder)
//{
//    // מיפוי תרגילים למספרים ב-OrderList ובקטגוריה
//    var exerciseOrderMapping = exercisesWithDetails.Select(tuple =>
//    {
//        var exercise = tuple.Exercise;
//        var targetMuscle = tuple.TargetMuscle;
//        var category = tuple.Category;

//        // חיפוש המיקום של השריר המיועד ב-OrderList
//        var muscleOrder = orderList.FirstOrDefault(kvp => kvp.Value == targetMuscle).Key;

//        // חיפוש המיקום של הקטגוריה בסדר הקטגוריות
//        var categoryOrderIndex = categoryOrder.IndexOf(category);
//        if (categoryOrderIndex == -1) categoryOrderIndex = int.MaxValue; // אם הקטגוריה לא בסדר מוגדר, למקם בסוף

//        return new
//        {
//            Exercise = exercise,
//            MuscleOrder = muscleOrder > 0 ? muscleOrder : int.MaxValue, // תרגילים ללא התאמה ייכנסו בסוף
//            CategoryOrder = categoryOrderIndex
//        };
//    }).ToList();

//    // סידור התרגילים לפי סדר השרירים ואז לפי סדר הקטגוריות
//    var sortedExercises = exerciseOrderMapping
//        .OrderBy(mapping => mapping.MuscleOrder) // סידור לפי סדר השרירים
//        .ThenBy(mapping => mapping.CategoryOrder) // סידור לפי סדר הקטגוריות
//        .Select(mapping => mapping.Exercise) // שליפת האובייקטים Exercise בלבד
//        .ToList();

//    return sortedExercises;
//}

//סידור רשימת התרגילים
//public async Task<List<List<ExerciseDTO>>> SortExercisesByPriorityAsync(
//        List<List<ExerciseDTO>> exercisePlan,
//        List<string> musclePriorityOrder,
//        List<string> subMusclePriorityOrder)
//{
//    // מיון רשימת התרגילים עבור כל יום
//    for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
//    {
//        var dayExercises = exercisePlan[dayIndex];

//        // מיון התרגילים לפי סדר השרירים ותתי-השרירים
//        dayExercises = dayExercises
//            .OrderBy(e =>
//            {
//                // קבל את השריר הראשי ותת-השריר עבור כל תרגיל
//                string muscleName = await exerciseDAL.GetMuscleByExerciseAsync(e.ExerciseId);
//                string subMuscleName = await exerciseDAL.GetSubMuscleByExerciseAsync(e.ExerciseId);

//                // מצא את האינדקס של השריר ותת-השריר בסדרי העדיפויות
//                int muscleIndex = musclePriorityOrder.IndexOf(muscleName);
//                int subMuscleIndex = subMusclePriorityOrder.IndexOf(subMuscleName);

//                // אם לא נמצא ברשימה, נשתמש בערך גבוה כדי לשים אותו בסוף
//                if (muscleIndex == -1) muscleIndex = int.MaxValue;
//                if (subMuscleIndex == -1) subMuscleIndex = int.MaxValue;

//                return (muscleIndex, subMuscleIndex);
//            })
//            .ToList();

//        // עדכון התרגילים של היום
//        exercisePlan[dayIndex] = dayExercises;
//    }

//    return exercisePlan;
//}



//public async Task<List<List<ExerciseDTO>>> SortExercisesByPriorityAsync(List<List<ExerciseDTO>> exercisePlan,List<string> musclePriorityOrder,List<string> subMusclePriorityOrder)
//{
//    // מיון רשימת התרגילים עבור כל יום
//    for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
//    {
//        var dayExercises = exercisePlan[dayIndex];

//        // שליפת המידע האסינכרוני מראש
//        var exerciseDetails = new List<(ExerciseDTO Exercise, string MuscleName, string SubMuscleName)>();
//        foreach (var exercise in dayExercises)
//        {
//            string muscleName = exercise.;
//            string subMuscleName = await exerciseDAL.GetSubMuscleByExerciseAsync(exercise.ExerciseId);
//            //string muscleName = await exerciseDAL.GetMuscleByExerciseAsync(exercise.ExerciseId);
//            //string subMuscleName = await exerciseDAL.GetSubMuscleByExerciseAsync(exercise.ExerciseId);
//            exerciseDetails.Add((exercise, muscleName, subMuscleName));
//        }

//        // מיון התרגילים לפי סדר השרירים ותתי-השרירים
//        var sortedExercises = exerciseDetails
//            .OrderBy(detail =>
//            {
//                // מצא את האינדקס של השריר ותת-השריר בסדרי העדיפויות
//                int muscleIndex = musclePriorityOrder.IndexOf(detail.MuscleName);
//                int subMuscleIndex = subMusclePriorityOrder.IndexOf(detail.SubMuscleName);

//                // אם לא נמצא ברשימה, נשתמש בערך גבוה כדי לשים אותו בסוף
//                if (muscleIndex == -1) muscleIndex = int.MaxValue;
//                if (subMuscleIndex == -1) subMuscleIndex = int.MaxValue;

//                return (muscleIndex, subMuscleIndex);
//            })
//            .Select(detail => detail.Exercise) // החזרת האובייקטים של התרגילים
//            .ToList();

//        // עדכון התרגילים של היום
//        exercisePlan[dayIndex] = sortedExercises;
//    }

//    return exercisePlan;
//}
//    public async Task<List<List<ExerciseWithMuscleInfo>>> SortExercisesByPriorityAsync(
//List<List<ExerciseWithMuscleInfo>> exercisePlan,
//List<string> musclePriorityOrder,
//List<string> subMusclePriorityOrder)
//    {
//        // מיון רשימת התרגילים עבור כל יום
//        for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
//        {
//            var dayExercises = exercisePlan[dayIndex];

//            // מיון התרגילים לפי סדר השרירים ותתי-השרירים
//            var sortedExercises = dayExercises
//                .OrderBy(detail =>
//                {
//                    // מצא את האינדקס של השריר ותת-השריר בסדרי העדיפויות
//                    int muscleIndex = musclePriorityOrder.IndexOf(detail.MuscleName);
//                    int subMuscleIndex = subMusclePriorityOrder.IndexOf(detail.SubMuscleName);

//                    // אם לא נמצא ברשימה, נשתמש בערך גבוה כדי לשים אותו בסוף
//                    if (muscleIndex == -1) muscleIndex = int.MaxValue;
//                    if (subMuscleIndex == -1) subMuscleIndex = int.MaxValue;

//                    return (muscleIndex, subMuscleIndex);
//                })
//                .ToList();
//            // עדכון התרגילים של היום
//            exercisePlan[dayIndex] = sortedExercises;
//        }

//        return exercisePlan;
//    }

//    public async Task<List<List<ExerciseWithMuscleInfo>>> SortExercisesByPriorityAsync(
//List<List<ExerciseWithMuscleInfo>> exercisePlan,
//List<string> musclePriorityOrder,
//List<string> subMusclePriorityOrder)
//    {
//        // מיון רשימת התרגילים עבור כל יום
//        for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
//        {
//            var dayExercises = exercisePlan[dayIndex];

//            // מיון התרגילים לפי סדר השרירים ותתי-השרירים
//            var sortedExercises = dayExercises
//                .OrderBy(detail =>
//                {
//                    // מצא את האינדקס של השריר ותת-השריר בסדרי העדיפויות
//                    int muscleIndex = musclePriorityOrder.IndexOf(detail.MuscleName);
//                    int subMuscleIndex = subMusclePriorityOrder.IndexOf(detail.SubMuscleName);

//                    // אם לא נמצא ברשימה, נשתמש בערך גבוה כדי לשים אותו בסוף
//                    if (muscleIndex == -1) muscleIndex = int.MaxValue;
//                    if (subMuscleIndex == -1) subMuscleIndex = int.MaxValue;

//                    return (muscleIndex, subMuscleIndex);
//                })
//                // מיון פנימי לפי כמות המפרקים בתוך כל קבוצת שרירים
//                .ThenByDescending(detail => detail.JointCount) // הנחה שיש שדה JointCount
//                .ToList();
//            //var sortedExercises = dayExercises
//            //    .OrderByDescending(detail => detail.JointCount) // מיון ראשוני לפי JointCount בסדר יורד
//            //    .ThenBy(detail =>
//            //    {
//            //        int muscleIndex = musclePriorityOrder.IndexOf(detail.MuscleName);
//            //        int subMuscleIndex = subMusclePriorityOrder.IndexOf(detail.SubMuscleName);

//            //        if (muscleIndex == -1) muscleIndex = int.MaxValue;
//            //        if (subMuscleIndex == -1) subMuscleIndex = int.MaxValue;

//            //        return (muscleIndex, subMuscleIndex);
//            //    })
//            //    .ToList();
//            // עדכון התרגילים של היום
//            exercisePlan[dayIndex] = sortedExercises;
//        }

//        return exercisePlan;
//    }

//    public async Task<List<List<ExerciseWithMuscleInfo>>> SortExercisesByPriorityAsync(
//List<List<ExerciseWithMuscleInfo>> exercisePlan,
//List<string> musclePriorityOrder,
//List<string> subMusclePriorityOrder)
//    {
//        // מעבר על כל יום בתוכנית האימונים
//        for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
//        {
//            var dayExercises = exercisePlan[dayIndex];

//            // מיון התרגילים עבור היום הנוכחי
//            var sortedExercises = dayExercises
//                // מיון ראשוני לפי סדר השרירים ותתי השרירים
//                .OrderBy(detail =>
//                {
//                    // מציאת האינדקסים של השריר ותת-השריר ברשימות העדיפויות
//                    int muscleIndex = musclePriorityOrder.IndexOf(detail.MuscleName);
//                    int subMuscleIndex = subMusclePriorityOrder.IndexOf(detail.SubMuscleName);

//                    // אם לא נמצא ברשימה, נשתמש ב-int.MaxValue כדי לשים אותו בסוף
//                    if (muscleIndex == -1) muscleIndex = int.MaxValue;
//                    if (subMuscleIndex == -1) subMuscleIndex = int.MaxValue;

//                    return (muscleIndex, subMuscleIndex);
//                })
//                // מיון נוסף בתוך קבוצות השרירים לפי כמות המפרקים בסדר יורד
//                .ThenByDescending(detail => detail.JointCount)
//                .ToList();

//            // עדכון רשימת התרגילים עבור היום
//            exercisePlan[dayIndex] = sortedExercises;
//        }

//        return exercisePlan;
//    }


//public async Task<List<List<ExerciseDTO>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>(); // תוכנית האימונים לכל הימים
//    var usedExercises = new HashSet<int>(); // שמירה על מזהי תרגילים שכבר נבחרו
//    var subMuscleOfMuscleList = new List<Muscle>();
//    var typeMuscleList = new List<string>();
//    foreach (var dict in trainingParams.TypeMuscleData)
//    {
//        foreach (var pair in dict)
//        {
//            foreach (var TypeValue in pair.Value)
//            {
//                typeMuscleList.Add(TypeValue);
//            }

//        }
//        break;
//    }

//    foreach (var dayList in trainingParams.DayLists) // מעבר על כל יום
//    {
//        var dayExercises = new List<ExerciseDTO>();

//        foreach (var muscleEntry in dayList) // מעבר על כל שריר ברשימה
//        {
//            string muscleName = muscleEntry.Key;
//            int exerciseCount = muscleEntry.Values;
//            string categoryName = muscleEntry.Name;//השריר הספציפי

//            // בדיקת תת-שרירים אם נדרש
//            foreach (var typeMuscle in typeMuscleList)
//            {
//                if (trainingParams.NeedSubMuscleList.Contains(typeMuscle))///לשנות לבדיקה על הסוג שריר
//                {
//                    if (trainingParams.subMuscleOfMuscleList.Contains(muscleName))
//                    {
//                        var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscleName);
//                        if (subMuscles.Count > 0)//רק במידה שיש תת שריר לשריר הספציפי
//                        {
//                            //מעבר על כל תת שריר
//                            foreach (var subMuscle in subMuscles)
//                            {
//                                //מתחיל לפי הסדר ובמידה ונגמר כמות התרגילים הוא מסיים לקחת
//                                if (exerciseCount <= 0) break;

//                                //שליחה לפונקציה שמוצאת את כל התרגילים שעובדים על השריר 
//                                var exercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, 1, trainingParams.equipment);
//                                //מכניס רק את התרגילים שלא משתמשים ביום הזה כבר
//                                if (exercises == null)
//                                {
//                                    var exercises1 = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, 1, this.equipmentList);
//                                }
//                                var filteredExercises = exercises
//                                    .Where(e => !usedExercises.Contains(e.ExerciseId))
//                                    .ToList();
//                                //מעבר על כל רשימת התרגילים 

//                                foreach (var exercise in filteredExercises)
//                                {
//                                    dayExercises.Add(exercise);
//                                    usedExercises.Add(exercise.ExerciseId);
//                                    exerciseCount--;

//                                    if (exerciseCount <= 0) break;
//                                    //לא בטוח
//                                    break;
//                                }


//                            }
//                        }
//                    }
//                }
//            }

//            // אם נשארו עוד תרגילים שצריך, בדוק סוגי שרירים אחרים
//            if (exerciseCount > 0)
//            {
//                var equipment = new List<int>();
//                foreach (var equipmentName in trainingParams.equipment)
//                {
//                    var e = await equipmentDAL.GetEquipmentByNameAsync(equipmentName);
//                    equipment.Add(e.EquipmentId);
//                }
//                //מעבר על כל הסוגי שרירים השימושיים לפי סדר
//                foreach (var muscleType in trainingParams.TypeMuscleData.SelectMany(d => d))
//                {
//                    //קבלת כל התרגילים שעובדים על שריר מסוים וסוג שריר ובציוד האפשרי
//                    foreach (var nuscleTypeName in muscleType.Value)
//                    {
//                        var exercises = await GetExercisesForMuscleAndTypeAsync(
//                       muscleName,
//                       nuscleTypeName,
//                       exerciseCount,
//                       equipment);
//                        //סינון רק התרגילים שלא משתמשים בהם כבר בתוכנית
//                        var filteredExercises = exercises
//                            .Where(e => !usedExercises.Contains(e.ExerciseId))
//                            .ToList();
//                        //מעבר על רשימת התרגילים האפשרית
//                        foreach (var exercise in filteredExercises)
//                        {
//                            dayExercises.Add(exercise);
//                            usedExercises.Add(exercise.ExerciseId);
//                            exerciseCount--;

//                            if (exerciseCount <= 0) break;
//                        }
//                    }
//                    if (exerciseCount <= 0) break;

//                }
//            }

//            // אם עדיין חסרים תרגילים, חפש בכל הציוד
//            if (exerciseCount > 0)
//            {
//                //בוחר מספר אקראי של תרגילים כמספר החסר
//                var allEquipmentExercises = await GetExercisesForMuscleAsync(muscleName, exerciseCount);
//                foreach (var exercise in allEquipmentExercises)
//                {
//                    if (usedExercises.Contains(exercise.ExerciseId)) continue;

//                    dayExercises.Add(exercise);
//                    usedExercises.Add(exercise.ExerciseId);
//                    exerciseCount--;

//                    if (exerciseCount <= 0) break;
//                }
//            }
//        }

//        exercisePlan.Add(dayExercises); // הוספת תרגילי היום לתוכנית
//    }


//    // הדפסת התוכנית ללוג
//    for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
//    {
//        logger.LogInformation($"Day {dayIndex + 1} Exercises:");
//        foreach (var exercise in exercisePlan[dayIndex])
//        {
//            logger.LogInformation($"  - {exercise.ExerciseName} (ID: {exercise.ExerciseId})");
//        }
//    }

//    return exercisePlan;
//}

//public async Task<List<List<ExerciseDTO>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>(); // תוכנית האימונים לכל הימים
//    var usedExercises = new HashSet<int>(); // שמירה על מזהי תרגילים שכבר נבחרו
//    var typeMuscleList = new List<string>();

//    // יצירת רשימת סוגי השרירים
//    foreach (var dict in trainingParams.TypeMuscleData)
//    {
//        foreach (var pair in dict)
//        {
//            foreach (var TypeValue in pair.Value)
//            {
//                typeMuscleList.Add(TypeValue);
//            }
//        }
//        break;
//    }

//    foreach (var dayList in trainingParams.DayLists) // מעבר על כל יום
//    {
//        var dayExercises = new List<ExerciseDTO>();


//        foreach (var muscleEntry in dayList) // מעבר על כל שריר ברשימה
//        {
//            string muscleName = muscleEntry.Key;
//            int exerciseCount = muscleEntry.Values;
//            string categoryName = muscleEntry.Name; // השריר הספציפי
//            bool exerciseFound = false; // דגל לבדיקת מציאת תרגיל לשריר

//            // בדיקת תת-שרירים אם נדרש
//            foreach (var typeMuscle in typeMuscleList)
//            {
//                if (trainingParams.NeedSubMuscleList.Contains(typeMuscle))
//                {
//                    if (trainingParams.subMuscleOfMuscleList.Contains(muscleName))
//                    {
//                        var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscleName);
//                        if (subMuscles.Count > 0) // רק אם יש תת שריר
//                        {
//                            foreach (var subMuscle in subMuscles)
//                            {
//                                if (exerciseCount <= 0) break;

//                                var exercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, 1, trainingParams.equipment);
//                                var filteredExercises = exercises.Where(e => !usedExercises.Contains(e.ExerciseId)).ToList();

//                                foreach (var exercise in filteredExercises)
//                                {
//                                    dayExercises.Add(exercise);
//                                    usedExercises.Add(exercise.ExerciseId);
//                                    exerciseCount--;
//                                    exerciseFound = true;

//                                    if (exerciseCount <= 0) break;
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            // אם נשארו עוד תרגילים שצריך, בדוק סוגי שרירים אחרים
//            if (exerciseCount > 0)
//            {
//                var equipment = new List<int>();
//                foreach (var equipmentName in trainingParams.equipment)
//                {
//                    var e = await equipmentDAL.GetEquipmentByNameAsync(equipmentName);
//                    equipment.Add(e.EquipmentId);
//                }

//                foreach (var muscleType in trainingParams.TypeMuscleData.SelectMany(d => d))
//                {
//                    foreach (var muscleTypeName in muscleType.Value)
//                    {
//                        var exercises = await GetExercisesForMuscleAndTypeAsync(muscleName, muscleTypeName, exerciseCount, equipment);
//                        var filteredExercises = exercises.Where(e => !usedExercises.Contains(e.ExerciseId)).ToList();

//                        foreach (var exercise in filteredExercises)
//                        {
//                            dayExercises.Add(exercise);
//                            usedExercises.Add(exercise.ExerciseId);
//                            exerciseCount--;
//                            exerciseFound = true;

//                            if (exerciseCount <= 0) break;
//                        }
//                    }
//                    if (exerciseCount <= 0) break;
//                }
//            }

//            // אם עדיין חסרים תרגילים, חפש בכל הציוד
//            if (exerciseCount > 0)
//            {
//                var allEquipmentExercises = await GetExercisesForMuscleAsync(muscleName, exerciseCount);
//                foreach (var exercise in allEquipmentExercises)
//                {
//                    if (usedExercises.Contains(exercise.ExerciseId)) continue;

//                    dayExercises.Add(exercise);
//                    usedExercises.Add(exercise.ExerciseId);
//                    exerciseCount--;
//                    exerciseFound = true;

//                    if (exerciseCount <= 0) break;
//                }
//            }

//            // אם לא נמצא תרגיל מתאים לשריר
//            if (!exerciseFound)
//            {
//                logger.LogWarning($"No exercise found for muscle: {muscleName}");
//            }
//        }

//        exercisePlan.Add(dayExercises); // הוספת תרגילי היום לתוכנית
//    }

//    // הדפסת התוכנית ללוג
//    for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
//    {
//        logger.LogInformation($"Day {dayIndex + 1} Exercises:");
//        foreach (var exercise in exercisePlan[dayIndex])
//        {
//            logger.LogInformation($"  - {exercise.ExerciseName} (ID: {exercise.ExerciseId})");
//        }
//    }

//    return exercisePlan;
//}


//public async Task<List<List<ExerciseDTO>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>(); // תוכנית האימונים לכל הימים
//    var typeMuscleList = new List<string>();

//    // יצירת רשימת סוגי השרירים
//    foreach (var dict in trainingParams.TypeMuscleData)
//    {
//        foreach (var pair in dict)
//        {
//            foreach (var TypeValue in pair.Value)
//            {
//                typeMuscleList.Add(TypeValue);
//            }
//        }
//        break;
//    }

//    foreach (var dayList in trainingParams.DayLists) // מעבר על כל יום
//    {
//        var dayExercises = new List<ExerciseDTO>();
//        var usedExercisesForDay = new HashSet<int>(); // רשימת מזהי תרגילים שכבר נבחרו עבור היום הנוכחי

//        foreach (var muscleEntry in dayList) // מעבר על כל שריר ברשימה
//        {
//            string muscleName = muscleEntry.Key;
//            int exerciseCount = muscleEntry.Values;
//            string categoryName = muscleEntry.Name; // השריר הספציפי
//            bool exerciseFound = false; // דגל לבדיקת מציאת תרגיל לשריר

//            // בדיקת תת-שרירים אם נדרש
//            foreach (var typeMuscle in typeMuscleList)
//            {
//                if (trainingParams.NeedSubMuscleList.Contains(typeMuscle))
//                {
//                    if (trainingParams.subMuscleOfMuscleList.Contains(muscleName))
//                    {
//                        var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscleName);
//                        if (subMuscles.Count > 0) // רק אם יש תת שריר
//                        {
//                            foreach (var subMuscle in subMuscles)
//                            {
//                                if (exerciseCount <= 0) break;

//                                var exercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, 1, trainingParams.equipment);
//                                var filteredExercises = exercises.Where(e => !usedExercisesForDay.Contains(e.ExerciseId)).ToList();

//                                foreach (var exercise in filteredExercises)
//                                {
//                                    dayExercises.Add(exercise);
//                                    usedExercisesForDay.Add(exercise.ExerciseId);
//                                    exerciseCount--;
//                                    exerciseFound = true;

//                                    if (exerciseCount <= 0) break;
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            // אם נשארו עוד תרגילים שצריך, בדוק סוגי שרירים אחרים
//            if (exerciseCount > 0)
//            {
//                var equipment = new List<int>();
//                foreach (var equipmentName in trainingParams.equipment)
//                {
//                    var e = await equipmentDAL.GetEquipmentByNameAsync(equipmentName);
//                    equipment.Add(e.EquipmentId);
//                }

//                foreach (var muscleType in trainingParams.TypeMuscleData.SelectMany(d => d))
//                {
//                    foreach (var muscleTypeName in muscleType.Value)
//                    {
//                        var exercises = await GetExercisesForMuscleAndTypeAsync(muscleName, muscleTypeName, exerciseCount, equipment);
//                        var filteredExercises = exercises.Where(e => !usedExercisesForDay.Contains(e.ExerciseId)).ToList();

//                        foreach (var exercise in filteredExercises)
//                        {
//                            dayExercises.Add(exercise);
//                            usedExercisesForDay.Add(exercise.ExerciseId);
//                            exerciseCount--;
//                            exerciseFound = true;

//                            if (exerciseCount <= 0) break;
//                        }
//                    }
//                    if (exerciseCount <= 0) break;
//                }
//            }

//            // אם עדיין חסרים תרגילים, חפש בכל הציוד
//            if (exerciseCount > 0)
//            {
//                var allEquipmentExercises = await GetExercisesForMuscleAsync(muscleName, exerciseCount);
//                foreach (var exercise in allEquipmentExercises)
//                {
//                    if (usedExercisesForDay.Contains(exercise.ExerciseId)) continue;

//                    dayExercises.Add(exercise);
//                    usedExercisesForDay.Add(exercise.ExerciseId);
//                    exerciseCount--;
//                    exerciseFound = true;

//                    if (exerciseCount <= 0) break;
//                }
//            }

//            // אם לא נמצא תרגיל מתאים לשריר
//            if (!exerciseFound)
//            {
//                logger.LogWarning($"No exercise found for muscle: {muscleName}");
//            }
//        }

//        exercisePlan.Add(dayExercises); // הוספת תרגילי היום לתוכנית
//    }

//    // הדפסת התוכנית ללוג
//    for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
//    {
//        logger.LogInformation($"Day {dayIndex + 1} Exercises:");
//        foreach (var exercise in exercisePlan[dayIndex])
//        {
//            logger.LogInformation($"  - {exercise.ExerciseName} (ID: {exercise.ExerciseId})");
//        }
//    }

//    return exercisePlan;
//}


//public async Task<List<List<ExerciseDTO>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>(); // תוכנית האימונים לכל הימים
//    var usedExercisesOverall = new HashSet<int>(); // רשימת מזהי תרגילים שכבר נבחרו בכל התוכנית
//    var typeMuscleList = new List<string>();

//    // יצירת רשימת סוגי השרירים
//    foreach (var dict in trainingParams.TypeMuscleData)
//    {
//        foreach (var pair in dict)
//        {
//            foreach (var TypeValue in pair.Value)
//            {
//                typeMuscleList.Add(TypeValue);
//            }
//        }
//        break;
//    }

//    foreach (var dayList in trainingParams.DayLists) // מעבר על כל יום
//    {
//        var dayExercises = new List<ExerciseDTO>();
//        var usedExercisesForDay = new HashSet<int>(); // רשימת מזהי תרגילים שכבר נבחרו עבור היום הנוכחי

//        foreach (var muscleEntry in dayList) // מעבר על כל שריר ברשימה
//        {
//            string muscleName = muscleEntry.Key;
//            int exerciseCount = muscleEntry.Values;
//            string categoryName = muscleEntry.Name; // השריר הספציפי
//            bool exerciseFound = false; // דגל לבדיקת מציאת תרגיל לשריר

//            // בדיקת תת-שרירים אם נדרש
//            foreach (var typeMuscle in typeMuscleList)
//            {
//                if (trainingParams.NeedSubMuscleList.Contains(typeMuscle))
//                {
//                    if (trainingParams.subMuscleOfMuscleList.Contains(muscleName))
//                    {
//                        var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscleName);
//                        if (subMuscles.Count > 0) // רק אם יש תת שריר
//                        {
//                            foreach (var subMuscle in subMuscles)
//                            {
//                                if (exerciseCount <= 0) break;

//                                var exercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, 1, trainingParams.equipment);
//                                var filteredExercises = exercises
//                                    .Where(e => !usedExercisesOverall.Contains(e.ExerciseId) && !usedExercisesForDay.Contains(e.ExerciseId))
//                                    .OrderBy(e => Guid.NewGuid()) // רנדומליות בבחירת התרגילים
//                                    .ToList();

//                                foreach (var exercise in filteredExercises)
//                                {
//                                    dayExercises.Add(exercise);
//                                    usedExercisesForDay.Add(exercise.ExerciseId);
//                                    usedExercisesOverall.Add(exercise.ExerciseId);
//                                    exerciseCount--;
//                                    exerciseFound = true;

//                                    if (exerciseCount <= 0) break;
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            // אם נשארו עוד תרגילים שצריך, בדוק סוגי שרירים אחרים
//            if (exerciseCount > 0)
//            {
//                var equipment = new List<int>();
//                foreach (var equipmentName in trainingParams.equipment)
//                {
//                    var e = await equipmentDAL.GetEquipmentByNameAsync(equipmentName);
//                    equipment.Add(e.EquipmentId);
//                }

//                foreach (var muscleType in trainingParams.TypeMuscleData.SelectMany(d => d))
//                {
//                    foreach (var muscleTypeName in muscleType.Value)
//                    {
//                        var exercises = await GetExercisesForMuscleAndTypeAsync(muscleName, muscleTypeName, exerciseCount, equipment);
//                        var filteredExercises = exercises
//                            .Where(e => !usedExercisesOverall.Contains(e.ExerciseId) && !usedExercisesForDay.Contains(e.ExerciseId))
//                            .OrderBy(e => Guid.NewGuid()) // רנדומליות בבחירת התרגילים
//                            .ToList();

//                        foreach (var exercise in filteredExercises)
//                        {
//                            dayExercises.Add(exercise);
//                            usedExercisesForDay.Add(exercise.ExerciseId);
//                            usedExercisesOverall.Add(exercise.ExerciseId);
//                            exerciseCount--;
//                            exerciseFound = true;

//                            if (exerciseCount <= 0) break;
//                        }
//                    }
//                    if (exerciseCount <= 0) break;
//                }
//            }

//            // אם עדיין חסרים תרגילים, חפש תרגילים שכבר השתמשו בהם בתוכנית
//            if (exerciseCount > 0)
//            {
//                var allEquipmentExercises = await GetExercisesForMuscleAsync(muscleName, exerciseCount);
//                var filteredExercises = allEquipmentExercises
//                    .Where(e => !usedExercisesForDay.Contains(e.ExerciseId)) // תרגילים שכבר נבחרו בתוכנית אך לא ביום הנוכחי
//                    .OrderBy(e => Guid.NewGuid()) // רנדומליות בבחירת התרגילים
//                    .ToList();

//                foreach (var exercise in filteredExercises)
//                {
//                    dayExercises.Add(exercise);
//                    usedExercisesForDay.Add(exercise.ExerciseId);
//                    exerciseCount--;
//                    exerciseFound = true;

//                    if (exerciseCount <= 0) break;
//                }
//            }

//            // אם לא נמצא תרגיל מתאים לשריר
//            if (!exerciseFound)
//            {
//                logger.LogWarning($"No exercise found for muscle: {muscleName}");
//            }
//        }

//        exercisePlan.Add(dayExercises); // הוספת תרגילי היום לתוכנית
//    }

//    // הדפסת התוכנית ללוג
//    for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
//    {
//        logger.LogInformation($"Day {dayIndex + 1} Exercises:");
//        foreach (var exercise in exercisePlan[dayIndex])
//        {
//            logger.LogInformation($"  - {exercise.ExerciseName} (ID: {exercise.ExerciseId})");
//        }
//    }

//    return exercisePlan;
//}

//public async Task<List<List<ExerciseDTO>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>(); // תוכנית האימונים לכל הימים
//    var usedExercisesOverall = new HashSet<int>(); // רשימת מזהי תרגילים שכבר נבחרו בכל התוכנית
//    var usedExercisesByMuscle = new Dictionary<string, HashSet<int>>(); // מעקב אחר תרגילים לכל שריר במהלך השבוע
//    var typeMuscleList = new List<string>();

//    // יצירת רשימת סוגי השרירים
//    foreach (var dict in trainingParams.TypeMuscleData)
//    {
//        foreach (var pair in dict)
//        {
//            foreach (var TypeValue in pair.Value)
//            {
//                typeMuscleList.Add(TypeValue);
//            }
//        }
//        break;
//    }

//    foreach (var dayList in trainingParams.DayLists) // מעבר על כל יום
//    {
//        var dayExercises = new List<ExerciseDTO>();
//        var usedExercisesForDay = new HashSet<int>(); // רשימת מזהי תרגילים שכבר נבחרו עבור היום הנוכחי

//        foreach (var muscleEntry in dayList) // מעבר על כל שריר ברשימה
//        {
//            string muscleName = muscleEntry.Key;
//            int exerciseCount = muscleEntry.Values;
//            string categoryName = muscleEntry.Name; // השריר הספציפי
//            bool exerciseFound = false; // דגל לבדיקת מציאת תרגיל לשריר

//            if (!usedExercisesByMuscle.ContainsKey(muscleName))
//            {
//                usedExercisesByMuscle[muscleName] = new HashSet<int>();
//            }

//            // בדיקת תת-שרירים אם נדרש
//            foreach (var typeMuscle in typeMuscleList)
//            {
//                if (trainingParams.NeedSubMuscleList.Contains(typeMuscle))
//                {
//                    if (trainingParams.subMuscleOfMuscleList.Contains(muscleName))
//                    {
//                        var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscleName);
//                        if (subMuscles.Count > 0) // רק אם יש תת שריר
//                        {
//                            foreach (var subMuscle in subMuscles)
//                            {
//                                if (exerciseCount <= 0) break;

//                                var exercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, 1, trainingParams.equipment);
//                                var filteredExercises = exercises
//                                    .Where(e => !usedExercisesOverall.Contains(e.ExerciseId) && !usedExercisesForDay.Contains(e.ExerciseId))
//                                    .OrderBy(e => Guid.NewGuid()) // רנדומליות בבחירת התרגילים
//                                    .ToList();

//                                foreach (var exercise in filteredExercises)
//                                {
//                                    dayExercises.Add(exercise);
//                                    usedExercisesForDay.Add(exercise.ExerciseId);
//                                    usedExercisesOverall.Add(exercise.ExerciseId);
//                                    usedExercisesByMuscle[muscleName].Add(exercise.ExerciseId);
//                                    exerciseCount--;
//                                    exerciseFound = true;

//                                    if (exerciseCount <= 0) break;
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            // אם נשארו עוד תרגילים שצריך, בדוק סוגי שרירים אחרים
//            if (exerciseCount > 0)
//            {
//                var equipment = new List<int>();
//                foreach (var equipmentName in trainingParams.equipment)
//                {
//                    var e = await equipmentDAL.GetEquipmentByNameAsync(equipmentName);
//                    equipment.Add(e.EquipmentId);
//                }

//                foreach (var muscleType in trainingParams.TypeMuscleData.SelectMany(d => d))
//                {
//                    foreach (var muscleTypeName in muscleType.Value)
//                    {
//                        var exercises = await GetExercisesForMuscleAndTypeAsync(muscleName, muscleTypeName, exerciseCount, equipment);
//                        var filteredExercises = exercises
//                            .Where(e => !usedExercisesOverall.Contains(e.ExerciseId) && !usedExercisesForDay.Contains(e.ExerciseId))
//                            .OrderBy(e => Guid.NewGuid()) // רנדומליות בבחירת התרגילים
//                            .ToList();

//                        foreach (var exercise in filteredExercises)
//                        {
//                            dayExercises.Add(exercise);
//                            usedExercisesForDay.Add(exercise.ExerciseId);
//                            usedExercisesOverall.Add(exercise.ExerciseId);
//                            usedExercisesByMuscle[muscleName].Add(exercise.ExerciseId);
//                            exerciseCount--;
//                            exerciseFound = true;

//                            if (exerciseCount <= 0) break;
//                        }
//                    }
//                    if (exerciseCount <= 0) break;
//                }
//            }
//            var allEquipmentExercises = await GetExercisesForMuscleAsync(muscleName, exerciseCount);

//            // אם עדיין חסרים תרגילים, חפש תרגילים שכבר השתמשו בהם בתוכנית
//            if (exerciseCount > 0)
//            {
//                var filteredExercises = allEquipmentExercises
//                    .Where(e => !usedExercisesForDay.Contains(e.ExerciseId) && !usedExercisesByMuscle[muscleName].Contains(e.ExerciseId))
//                    .OrderBy(e => Guid.NewGuid()) // רנדומליות בבחירת התרגילים
//                    .ToList();

//                foreach (var exercise in filteredExercises)
//                {
//                    dayExercises.Add(exercise);
//                    usedExercisesForDay.Add(exercise.ExerciseId);
//                    usedExercisesByMuscle[muscleName].Add(exercise.ExerciseId);
//                    exerciseCount--;
//                    exerciseFound = true;

//                    if (exerciseCount <= 0) break;
//                }
//            }

//            // אם עדיין אין תרגילים, בחר תרגיל שכבר השתמשו בו לשריר הזה
//            if (exerciseCount > 0 && !exerciseFound)
//            {
//                var repeatedExercises = allEquipmentExercises
//                    .Where(e => !usedExercisesForDay.Contains(e.ExerciseId)) // תרגילים שכבר נבחרו לשריר זה
//                    .OrderBy(e => Guid.NewGuid())
//                    .ToList();

//                foreach (var exercise in repeatedExercises)
//                {
//                    dayExercises.Add(exercise);
//                    usedExercisesForDay.Add(exercise.ExerciseId);
//                    exerciseCount--;

//                    logger.LogWarning($"Reusing exercise '{exercise.ExerciseName}' for muscle '{muscleName}' due to lack of alternatives.");

//                    if (exerciseCount <= 0) break;
//                }
//            }

//            // אם לא נמצא תרגיל מתאים לשריר
//            if (!exerciseFound)
//            {
//                logger.LogWarning($"No exercise found for muscle: {muscleName}");
//            }
//        }

//        exercisePlan.Add(dayExercises); // הוספת תרגילי היום לתוכנית
//    }

//    // הדפסת התוכנית ללוג
//    for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
//    {
//        logger.LogInformation($"Day {dayIndex + 1} Exercises:");
//        foreach (var exercise in exercisePlan[dayIndex])
//        {
//            logger.LogInformation($"  - {exercise.ExerciseName} (ID: {exercise.ExerciseId})");
//        }
//    }

//    return exercisePlan;
//}

//בדיקה עבור תת שריר 
//public async Task<List<List<ExerciseDTO>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>(); // תוכנית האימונים לכל הימים
//    var usedExercisesOverall = new HashSet<int>(); // רשימת מזהי תרגילים שכבר נבחרו בכל התוכנית
//    var usedExercisesBySubMuscle = new Dictionary<string, int>(); // מיפוי של תרגילים שנבחרו עבור תתי-שרירים
//    var typeMuscleList = new List<string>();

//    // יצירת רשימת סוגי השרירים
//    foreach (var dict in trainingParams.TypeMuscleData)
//    {
//        foreach (var pair in dict)
//        {
//            foreach (var TypeValue in pair.Value)
//            {
//                typeMuscleList.Add(TypeValue);
//            }
//        }
//        break;
//    }

//    foreach (var dayList in trainingParams.DayLists) // מעבר על כל יום
//    {
//        var dayExercises = new List<ExerciseDTO>();
//        var usedExercisesForDay = new HashSet<int>(); // רשימת מזהי תרגילים שכבר נבחרו עבור היום הנוכחי

//        foreach (var muscleEntry in dayList) // מעבר על כל שריר ברשימה
//        {
//            string muscleName = muscleEntry.Key;
//            int exerciseCount = muscleEntry.Values;
//            string categoryName = muscleEntry.Name; // השריר הספציפי
//            bool exerciseFound = false; // דגל לבדיקת מציאת תרגיל לשריר

//            // בדיקת תת-שרירים אם נדרש
//            if (trainingParams.subMuscleOfMuscleList.Contains(muscleName))
//            {
//                var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscleName);
//                var subMuscleToExerciseMap = new Dictionary<string, ExerciseDTO>(); // מיפוי בין תתי-שרירים לתרגילים

//                foreach (var subMuscle in subMuscles)
//                {
//                    if (exerciseCount <= 0) break;

//                    // מציאת תרגילים שעובדים על תת-שריר זה
//                    var exercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, trainingParams.equipment);

//                    // סינון התרגילים כך שלא יחזרו על עצמם בתתי-שרירים אחרים
//                    var filteredExercises = exercises
//                        .Where(e => !usedExercisesForDay.Contains(e.ExerciseId) &&
//                                    (!usedExercisesBySubMuscle.ContainsKey(subMuscle.SubMuscleName) ||
//                                     usedExercisesBySubMuscle[subMuscle.SubMuscleName] == e.ExerciseId))
//                        .OrderBy(e => Guid.NewGuid()) // רנדומליות בבחירת התרגילים
//                        .ToList();

//                    // אם אין תרגיל מתאים, בודקים אם יש תרגיל יחיד שעובד רק על תת-שריר זה
//                    if (filteredExercises.Count == 0)
//                    {
//                        var exclusiveExercises = exercises
//                            .Where(e => !usedExercisesForDay.Contains(e.ExerciseId))
//                            .OrderBy(e => Guid.NewGuid()) // רנדומליות
//                            .ToList();

//                        if (exclusiveExercises.Count > 0)
//                        {
//                            var exercise = exclusiveExercises.First();
//                            subMuscleToExerciseMap[subMuscle.SubMuscleName] = exercise;
//                            usedExercisesForDay.Add(exercise.ExerciseId);
//                            usedExercisesOverall.Add(exercise.ExerciseId);
//                            usedExercisesBySubMuscle[subMuscle.SubMuscleName] = exercise.ExerciseId;
//                            dayExercises.Add(exercise);
//                            exerciseCount--;
//                            exerciseFound = true;
//                        }
//                    }
//                    else
//                    {
//                        // בחירת תרגיל מתאים
//                        foreach (var exercise in filteredExercises)
//                        {
//                            subMuscleToExerciseMap[subMuscle.SubMuscleName] = exercise;
//                            usedExercisesForDay.Add(exercise.ExerciseId);
//                            usedExercisesOverall.Add(exercise.ExerciseId);
//                            usedExercisesBySubMuscle[subMuscle.SubMuscleName] = exercise.ExerciseId;
//                            dayExercises.Add(exercise);
//                            exerciseCount--;
//                            exerciseFound = true;

//                            if (exerciseCount <= 0) break;
//                        }
//                    }
//                }
//            }

//            // אם עדיין חסרים תרגילים (לשריר הראשי), חפש תרגילים רגילים
//            if (exerciseCount > 0)
//            {
//                var allEquipmentExercises = await GetExercisesForMuscleAsync(muscleName, exerciseCount);
//                var filteredExercises = allEquipmentExercises
//                    .Where(e => !usedExercisesForDay.Contains(e.ExerciseId))
//                    .OrderBy(e => Guid.NewGuid()) // רנדומליות בבחירת התרגילים
//                    .ToList();

//                foreach (var exercise in filteredExercises)
//                {
//                    dayExercises.Add(exercise);
//                    usedExercisesForDay.Add(exercise.ExerciseId);
//                    usedExercisesOverall.Add(exercise.ExerciseId);
//                    exerciseCount--;
//                    exerciseFound = true;

//                    if (exerciseCount <= 0) break;
//                }
//            }

//            // אם לא נמצא תרגיל מתאים
//            if (!exerciseFound)
//            {
//                logger.LogWarning($"No exercise found for muscle: {muscleName}");
//            }
//        }

//        exercisePlan.Add(dayExercises); // הוספת תרגילי היום לתוכנית
//    }

//    var musclePriorityOrder= trainingParams.musclePriorityOrder;

//    var subMusclePriorityOrder = trainingParams.subMusclePriorityOrder;

//    //קריאה לפונקצית סידור התרגילים
//    exercisePlan = await SortExercisesByPriorityAsync(exercisePlan, musclePriorityOrder, subMusclePriorityOrder);

//    // הדפסת התוכנית ללוג
//    for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
//    {
//        logger.LogInformation($"Day {dayIndex + 1} Exercises:");
//        foreach (var exercise in exercisePlan[dayIndex])
//        {
//            logger.LogInformation($"  - {exercise.ExerciseName} (ID: {exercise.ExerciseId})");
//        }
//    }

//    return exercisePlan;
//}


//private List<List<DayEntry>> ExtractDayLists(IXLWorksheet worksheet, int daysInWeek)
//{
//    var dayLists = new List<List<DayEntry>>(); // רשימה של רשימות DayEntry

//    // מציאת העמודות בשורה הראשונה (Header) עם הערך daysInWeek
//    var matchingColumns = worksheet.Row(1).CellsUsed()
//        .Where(cell => cell.GetValue<int>() == daysInWeek)
//        .Select(cell => cell.Address.ColumnNumber)
//        .ToList();

//    // עבור כל עמודה שמתאימה לערך daysInWeek
//    foreach (var col in matchingColumns)
//    {
//        var dayEntries = new List<DayEntry>(); // רשימה עבור עמודה נוכחית

//        // מעבר על כל השורות מתחת לכותרת
//        foreach (var row in worksheet.RowsUsed().Skip(1))
//        {
//            var key = row.Cell(1).GetValue<string>(); // קבלת הערך בעמודה הראשונה (Key)
//            var value = row.Cell(col).GetValue<int>(); // קבלת הערך בעמודה הנוכחית (Value)

//            // יצירת אובייקט DayEntry והוספתו לרשימה
//            dayEntries.Add(new DayEntry
//            {
//                Key = key,
//                Values = value
//            });
//        }

//        // הוספת הרשימה של העמודה הנוכחית לרשימה הכללית
//        dayLists.Add(dayEntries);
//    }

//    return dayLists;
//}
//private List<object> ExtractColumnValues(IXLWorksheet worksheet, int column)
//{
//    return worksheet.Column(column).CellsUsed().Skip(1).Select(c => (object)c.Value).ToList();
//}
//private int FindColumnByValueInFirstRow(IXLWorksheet worksheet, int value, string errorMessage)
//{
//    foreach (var col in worksheet.ColumnsUsed())
//    {
//        // בדוק את הערך בשורה הראשונה של העמודה
//        if (col.FirstCell().GetValue<int>() == value)
//        {
//            return col.ColumnNumber(); // החזר את מספר העמודה
//        }
//    }

//    // אם הערך לא נמצא, רשום ביומן והשלך חריגה
//    logger.LogWarning(errorMessage);
//    throw new Exception(errorMessage);
//}

//private int FindRowByValue(IXLWorksheet worksheet, int column, int value, string errorMessage)
//{
//    foreach (var row in worksheet.RowsUsed())
//    {
//        if (row.Cell(column).GetValue<int>() == value)
//        {
//            return row.RowNumber();
//        }
//    }
//    logger.LogWarning(errorMessage);
//    throw new Exception(errorMessage);
//}


//private List<string> ExtractTypMuscleData(IXLWorksheet worksheet, int daysInWeek)
//{
//    var listTypeMuscle = new List<string>();
//    var col = FindColumnByValue(worksheet, 1, daysInWeek, "Column not found");
//    foreach (var row in worksheet.RowsUsed().Skip(1)) // מעבר על כל השורות
//    {

//        var value = row.Cell(col).GetValue<string>(); // הערך מהעמודה שנמצאה
//        listTypeMuscle.Add(value);
//    }
//    return listTypeMuscle;
//}

//private List<Dictionary<int, string>> ExtractTypMuscleData(IXLWorksheet worksheet, int daysInWeek)
//{
//    var OrderListOfTypeMuscle = new List<Dictionary<int, string>>();

//    foreach (var row in worksheet.RowsUsed().Skip(1))
//    {
//        if (row.Cell(1).GetValue<int>() == daysInWeek)
//        {
//            var dict = new Dictionary<int, string>();
//            for (int col = 3; col <= row.LastCellUsed().Address.ColumnNumber; col++)
//            {
//                dict.Add(row.Cell(2).GetValue<int>(), row.Cell(col).GetValue<string>());
//            }
//            OrderListOfTypeMuscle.Add(dict);
//        }
//    }
//    //מחזיר רשימה של דיקשינרי של הסוגי שרירים שמסודרים לפי סדר לפי המפתח בסדר עולה (1 הכי חשוב וכו) 0
//    return OrderListOfTypeMuscle;
//}

//public async Task<List<List<ExerciseDTO>>> GenerateExercisePlanAsync(IBLL.TrainingParams trainingParams)
//{
//    // רשימה שתכיל את כל רשימות התרגילים לכל הימים
//    var exercisePlan = new List<List<ExerciseDTO>>();

//    // עבור כל רשימה ב-DayLists (כל יום)
//    foreach (var dayList in trainingParams.DayLists)
//    {
//        var dayExercises = new List<ExerciseDTO>(); // רשימה לתרגילים של יום זה

//        // עבור על כל שריר ברשימה של היום
//        foreach (var muscle in dayList)
//        {
//            // בדוק אם השריר נמצא ב-LargeMuscleList
//            if (trainingParams.LargeMuscleList.Contains(muscle))
//            {
//                // שלוף תרגילים לשריר זה לפי LargeMuscleCount
//                var exercises = await GetExercisesForMuscleAsync(muscle.ToString(), trainingParams.LargeMuscleCount);
//                dayExercises.AddRange(exercises);
//            }
//            // בדוק אם השריר נמצא ב-SmallMuscleList
//            else if (trainingParams.SmallMuscleList.Contains(muscle))
//            {
//                // שלוף תרגילים לשריר זה לפי SmallMuscleCount
//                var exercises = await GetExercisesForMuscleAsync(muscle.ToString(), trainingParams.SmallMuscleCount);
//                dayExercises.AddRange(exercises);
//            }
//        }

//        // הוסף את רשימת התרגילים של היום לרשימה הכללית
//        exercisePlan.Add(dayExercises);
//    }

//    return exercisePlan;
//}


//public async Task<List<List<ExerciseDTO>>> GenerateExercisePlanWithEquipmentAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>(); // רשימה של רשימות תרגילים (כל יום הוא רשימה)

//    // מעבר על כל יום ברשימת הימים
//    foreach (var dayList in trainingParams.DayLists)
//    {
//        var dayExercises = new List<ExerciseDTO>(); // רשימה לתרגילים עבור היום הנוכחי
//        var usedExercises = new HashSet<int>(); // שמירה על תרגילים שכבר נבחרו כדי למנוע כפילויות

//        // מעבר על כל DayEntry ברשימת היום
//        foreach (var muscle in dayList)
//        {
//            var muscleKey = muscle.Key; // שם השריר
//            var exerciseCount = muscle.Values; // כמות התרגילים הנדרשת
//            var category = muscle.Name; // קטגוריה

//            // בדיקת תת-שרירים אם הערך "מבודדים" קיים ב-TypeMuscle
//            if (trainingParams.TypMuscle.Contains("מבודד"))
//            {
//                var muscles = await muscleDAL.GetMuscleByNameAsync(muscleKey);
//                // שליפת תת-שרירים מהמסד נתונים
//                var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscles);

//                //מעבר על כל תת שריר ובחירת תרגיל מתאים בשבילו
//                foreach (var subMuscle in subMuscles)
//                {
//                    if (exerciseCount <= 0) break;

//                    if (trainingParams.equipment == null || !trainingParams.equipment.Any())
//                    {
//                        throw new Exception("Equipment list is empty or not defined.");
//                    }
//                    //שליפה של כל התרגילים שעובדים על תת שריר בציוד האפשרי
//                    var exercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, 1, trainingParams.equipment);
//                    //מכניס רק את התרגילים שלא משתמשים ביום הזה כבר
//                    var filteredExercises = exercises.Where(e => !usedExercises.Contains(e.ExerciseId)).ToList();
//                    //מעבר על כל רשימת התרגילים 
//                    foreach (var exercise in filteredExercises)
//                    {
//                        if (exerciseCount <= 0) break;

//                        dayExercises.Add(exercise);
//                        usedExercises.Add(exercise.ExerciseId);
//                        exerciseCount--;
//                        //לא בטוח
//                        break;
//                    }
//                }
//            }

//            // השלמת תרגילים משרירים מורכבים אם נדרש ואם הערך "מורכבים" קיים ב-TypeMuscle
//            if (exerciseCount > 0 && trainingParams.TypMuscle.Contains("מורכב"))
//            {

//                if (trainingParams.equipment == null || !trainingParams.equipment.Any())
//                {
//                    throw new Exception("Equipment list is empty or not defined.");
//                }

//                var equipmentIds = new List<int>();

//                foreach (var equipmentName in trainingParams.equipment)
//                {
//                    var equipment = await equipmentDAL.GetEquipmentByNameAsync(equipmentName);
//                    equipmentIds.Add(equipment.EquipmentId);
//                }
//                var complexExercises = await GetExercisesForMuscleAndTypeAsync(muscleKey, "מורכב", exerciseCount, equipmentIds);//***********************

//                var filteredComplexExercises = complexExercises.Where(e => !usedExercises.Contains(e.ExerciseId)).ToList();

//                //var filteredComplexExercises = complexExercises
//                //    .Where(e => trainingParams.equipment.Contains(e.Equipment) && !usedExercises.Contains(e.ExerciseId))
//                //    .ToList();

//                foreach (var exercise in filteredComplexExercises)
//                {
//                    if (exerciseCount <= 0) break;

//                    dayExercises.Add(exercise);
//                    usedExercises.Add(exercise.ExerciseId);
//                    exerciseCount--;
//                }
//            }
//        }

//        // הוספת רשימת התרגילים של היום לרשימה הכללית
//        exercisePlan.Add(dayExercises);
//    }

//    // הדפסת התוכנית ללוג
//    foreach (var (day, dayIndex) in exercisePlan.Select((value, index) => (value, index)))
//    {
//        logger.LogInformation($"Day {dayIndex + 1} Exercises:");

//        foreach (var exercise in day)
//        {
//            logger.LogInformation($"  - {exercise.ExerciseName}");
//        }
//    }

//    return exercisePlan;
//}


//public async Task<List<Exercise>> GetExercisesWithEquipmentFilter(string muscleName, string muscleType, List<int> equipmentIds)
//{
//    var exercises = await muscleDAL.GetExercisesForMuscleAndTypeAsync(muscleName, muscleType, equipmentIds);

//    var filteredExercises = exercises.Where(e => e.Equipment.Any(eq => equipmentIds.Contains(eq.EquipmentId))).ToList();
//    return filteredExercises;
//}

//public async Task<List<List<ExerciseDTO>>> GenerateExercisePlanAndLogAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>(); // רשימה של רשימות תרגילים (כל יום הוא רשימה)

//    // מעבר על כל יום ברשימת הימים
//    foreach (var dayList in trainingParams.DayLists)
//    {
//        var dayExercises = new List<ExerciseDTO>(); // רשימה לתרגילים עבור היום הנוכחי

//        // מעבר על כל DayEntry ברשימת היום
//        foreach (var muscle in dayList)
//        {
//            var muscleKey = muscle.Key; // ה-Key מייצג את שם השריר
//            var exerciseCount = muscle.Values; // ה-Value מייצג את כמות התרגילים הנדרשת
//            var caregory = muscle.Name;
//            if (exerciseCount > 0)
//            {
//                // שליפת תרגילים מהמסד נתונים לפי שם השריר וכמות התרגילים
//                var exercises = await GetExercisesForMuscleByCategoryAsync(muscleKey, caregory, exerciseCount);

//                if (!exercises.Any())
//                {
//                    logger.LogWarning($"No exercises found for Muscle '{muscleKey}' with requested count '{exerciseCount}' in category '{caregory}'.");
//                    continue; // אם אין תרגילים לשריר, דלג עליו
//                }

//                // הוספת התרגילים לרשימה של היום
//                dayExercises.AddRange(exercises);
//            }
//        }

//        // הוספת רשימת התרגילים של היום לרשימה הכללית
//        exercisePlan.Add(dayExercises);
//    }

//    // הדפסת התוכנית ללוג
//    foreach (var (day, dayIndex) in exercisePlan.Select((value, index) => (value, index)))
//    {
//        logger.LogInformation($"Day {dayIndex + 1} Exercises:");

//        foreach (var exercise in day)
//        {
//            logger.LogInformation($"  - {exercise.ExerciseName} ");
//        }
//    }

//    // החזרת התוכנית
//    return exercisePlan;
//}


//private async Task<List<ExerciseDTO>> GetExercisesForMuscleAndCategoryAsync(string muscleName, string categoryName, int count)
//{
//    // שליפת תרגילים עבור שריר וקטגוריה
//    var exercises = await muscleDAL.GetExercisesForMuscleAndCategoryAsync(muscleName, categoryName);

//    // המרת התרגילים ל-DTO
//    var exerciseDTOs = exercises.Select(e => new ExerciseDTO
//    {
//        ExerciseId = e.ExerciseId,
//        ExerciseName = e.ExerciseName
//    }).ToList();

//    // בחר את מספר התרגילים הנדרש
//    return exerciseDTOs.OrderBy(e => Guid.NewGuid()).Take(count).ToList();
//}


//public async Task<List<List<ExerciseDTO>>> GenerateOptimizedExercisePlanAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>(); // תוכנית האימונים לכל הימים
//    var usedExercises = new HashSet<int>(); // לשמירה על תרגילים שכבר נבחרו

//    foreach (var dayList in trainingParams.DayLists) // מעבר על כל יום
//    {
//        var dayExercises = new List<ExerciseDTO>();
//        foreach (var typeMuscleData in trainingParams.TypeMuscleData) // מעבר על סוגי השרירים לפי תעדוף
//        {
//            foreach (var muscleType in typeMuscleData.OrderBy(d => d.Key)) // סדר עדיפות
//            {
//                foreach (var muscleName in muscleType.Value) // מעבר על כל שריר בסוג
//                {
//                    var exerciseCount = dayList.FirstOrDefault(d => d.Key == muscleName)?.Values ?? 0;
//                    if (exerciseCount <= 0) continue;

//                    // שליפת תרגילים לפי סוג שריר
//                    var exercises = await GetExercisesForMuscleAndTypeAsync(muscleName, muscleType.Value[1], exerciseCount, trainingParams.equipment.Select(e => int.Parse(e)).ToList());
//                    var filteredExercises = exercises.Where(e => !usedExercises.Contains(e.ExerciseId)).ToList();

//                    foreach (var exercise in filteredExercises)
//                    {
//                        dayExercises.Add(exercise);
//                        usedExercises.Add(exercise.ExerciseId);
//                        if (--exerciseCount <= 0) break;
//                    }

//                    if (exerciseCount > 0)
//                    {
//                        // נסיון להוסיף תרגילים מתת-שרירים
//                        var subMuscles = await muscleDAL.GetSubMusclesOfMuscaleAsync(muscleName);
//                        foreach (var subMuscle in subMuscles)
//                        {
//                            var subExercises = await GetExercisesForSubMuscleAsync(subMuscle.SubMuscleName, exerciseCount, trainingParams.equipment);
//                            foreach (var subExercise in subExercises)
//                            {
//                                if (usedExercises.Contains(subExercise.ExerciseId)) continue;
//                                dayExercises.Add(subExercise);
//                                usedExercises.Add(subExercise.ExerciseId);
//                                if (--exerciseCount <= 0) break;
//                            }
//                        }
//                    }
//                }
//            }
//        }

//        exercisePlan.Add(dayExercises); // הוספת תרגילי היום הנוכחי לתוכנית
//    }
//    // הדפסת התוכנית ללוג
//    for (int dayIndex = 0; dayIndex < exercisePlan.Count; dayIndex++)
//    {
//        var dayExercises = exercisePlan[dayIndex];
//        logger.LogInformation($"Day {dayIndex + 1} Exercises:");
//        foreach (var exercise in dayExercises)
//        {
//            logger.LogInformation($"  - {exercise.ExerciseName} (ID: {exercise.ExerciseId})");
//        }
//    }

//    return exercisePlan;
//}


//***************************************
//public async Task<List<ExerciseDTO>> GetExercisePlanAsync(TrainingParams trainingParams)
//{
//    // קבל את הנתונים של השרירים
//    var muscles = trainingParams.NeedSubMuscleList;

//    // קבל את הנתונים של הציוד
//    var equipment = trainingParams.equipment;

//    // קבל את הנתונים של התרגילים
//    var exercises = await _exerciseBLL.GetAllExercisesAsync();

//    // בנה תוכנית אימון
//    var exercisePlan = new List<ExerciseDTO>();

//    foreach (var muscle in muscles)
//    {
//        // בדוק אם יש תרגילים שעובדים על השריר הזה
//        var exercisesForMuscle = exercises.Where(e => e.MuscleId == muscle.Id).ToList();

//        // בדוק אם יש תרגילים שעובדים על הציוד הזה
//        var exercisesForEquipment = exercisesForMuscle.Where(e => e.EquipmentId == equipment.Id).ToList();

//        // בדוק אם יש תרגילים שעובדים על הסוג שריר הזה
//        var exercisesForMuscleType = exercisesForEquipment.Where(e => e.MuscleTypeId == muscle.MuscleTypeId).ToList();

//        // הוסף את התרגילים לתוכנית האימון
//        exercisePlan.AddRange(exercisesForMuscleType);
//    }

//    // בדוק אם יש עוד תרגילים שצריך להוסיף
//    if (exercisePlan.Count < trainingParams.NumExercises)
//    {
//        // הוסף את התרגילים הנוספים
//        var additionalExercises = exercises.Where(e => !exercisePlan.Contains(e)).Take(trainingParams.NumExercises - exercisePlan.Count).ToList();
//        exercisePlan.AddRange(additionalExercises);
//    }

//    return exercisePlan;
//}


//public class TrainingParams
//{
//    public List<DayEntry> DayLists { get; set; } // רשימה של אובייקטים מסוג DayEntry
//    public Dictionary<string, List<object>> MuscleSizeData { get; set; } // מילון דינאמי לגודל שרירים
//    public int MinRep { get; set; }
//    public int MaxRep { get; set; }
//    public Dictionary<(string Category, string MuscleSize), int> TimeCategories { get; set; }

//}

//public async Task addProgramExerciseAsync1(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time)
//{
//    // לא קבוע
//    string filePath1 = @"C:\Users\user\Pictures\תכנות\שנה ב\פוריקט שנתי\C#\פרויקט חדש 20.04\Gym_Api\BLL\new.xlsx";

//    // שליפת כל הפרמטרים מהקובץ
//    var trainingParams = await GetAllParams(filePath1, daysInWeek, goal, level, time);

//    if (trainingParams == null)
//    {
//        throw new Exception("Failed to retrieve training parameters.");
//    }

//    // הדפסת רשימות השרירים לכל יום
//    for (int i = 0; i < trainingParams.DayLists.Count; i++)
//    {
//        logger.LogInformation($"Day List {i + 1}: " + string.Join(", ", trainingParams.DayLists[i]));
//    }

//    // הדפסת כל גדלי השרירים ורשימות השרירים שלהם
//    foreach (var muscleSize in trainingParams.MuscleSizeData)
//    {
//        logger.LogInformation($"Muscle Size: {muscleSize.Key}");
//        logger.LogInformation("Muscles: " + string.Join(", ", muscleSize.Value));
//    }

//    // הדפסת מספר החזרות
//    logger.LogInformation($"Repetitions: {trainingParams.MinRep}-{trainingParams.MaxRep}");

//    // הדפסת הקטגוריות והכמות לכל שריר
//    logger.LogInformation("Time Categories: " + string.Join(", ", trainingParams.TimeCategories.Select(kvp => $"[{kvp.Key.Category} - {kvp.Key.MuscleSize}: {kvp.Value}]")));

//    // כאן ניתן להוסיף קריאה ל-GenerateExercisePlanAsync אם יש צורך
//    // var exercisePlan = await GenerateExercisePlanAsync(trainingParams);

//    // הדפסת התוכנית
//    // if (exercisePlan != null)
//    // {
//    //     foreach (var day in exercisePlan)
//    //     {
//    //         logger.LogInformation("Day Exercises:");
//    //         foreach (var exercise in day)
//    //         {
//    //             logger.LogInformation($"- {exercise.ExerciseName}");
//    //         }
//    //     }
//    // },

//    foreach (var day in trainingParams.DayLists)
//    {
//        logger.LogInformation($"Key (Day): {day.Key}, Values: {string.Join(", ", day.Values)}");
//    }

//    await GenerateExercisePlanAndLogAsync(trainingParams);
//}




//public async Task<TrainingParams> GetAllParams(string filePath, int daysInWeek, int goal, int level, int time)
//{
//    try
//    {
//        using (var workbook = new XLWorkbook(filePath))
//        {
//            // רשימה של השרירים לכל יום
//            //var daysWorksheet = GetWorksheet(workbook, DaysInWeekSheet);
//            //var dayLists = ExtractDayLists(daysWorksheet, daysInWeek);
//            var daysWorksheet = GetWorksheet(workbook, DaysInWeekSheet);
//            var dayLists = ExtractDayLists(daysWorksheet, daysInWeek);

//            // קריאת מידע של גדלי השרירים בצורה דינאמית
//            var muscleSizeWorksheet = GetWorksheet(workbook, MuscleSheet);
//            var muscleSizeData = ExtractMuscleSizeData(muscleSizeWorksheet);
//            ;
//            // מספר החזרות
//            var repWorksheet = GetWorksheet(workbook, RepitByGoalSheet);
//            int colRep = FindColumnByValue(repWorksheet, 1, goal, "Column with the specified goal not found.");
//            int minRep = GetValueFromWorksheet(repWorksheet, "min", colRep, "Min value not found.");
//            int maxRep = GetValueFromWorksheet(repWorksheet, "max", colRep, "Max value not found.");

//            // זמן לקטגוריה
//            var timeCategoryWorksheet = GetWorksheet(workbook, CategorySheet);
//            var timeCategoryList = ExtractTimeCategoryList(timeCategoryWorksheet, goal, time);

//            // הכמות לשרירים dictinary
//            var countToMuscleWorksheet = GetWorksheet(workbook, SumOfLargeByTimeSheet);
//            var categoryMuscleSizeData = ExtractCategoryMuscleSizeData(countToMuscleWorksheet, time, daysInWeek);

//            // החזרת כל הנתונים בצורה דינאמית
//            return new TrainingParams
//            {
//                DayLists = dayLists,
//                MuscleSizeData = muscleSizeData, // במקום LargeMuscleList ו-SmallMuscleList
//                MinRep = minRep,
//                MaxRep = maxRep,
//                TimeCategories = categoryMuscleSizeData,
//            };
//        }
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "Error while processing training parameters.");
//        throw;
//    }
//}

//private List<DayEntry> ExtractDayLists(IXLWorksheet worksheet, int daysInWeek)
//{
//    var dayEntries = new List<DayEntry>();

//    // שליפת מספר העמודה המתאימה לערך "DAYINWEEK" בשורה השנייה
//    var headerRow = worksheet.Row(1); // שורה שנייה
//                                      // var dayInWeekColumn = headerRow.CellsUsed()
//                                      //   .FirstOrDefault(cell => cell.GetValue<string>().Equals("DAYINWEEK", StringComparison.OrdinalIgnoreCase));
//    var dayInWeekColumn = headerRow.CellsUsed()
//        .FirstOrDefault(cell => cell.GetValue<int>() == daysInWeek);
//    if (dayInWeekColumn == null)
//    {
//        throw new Exception("Column with header 'DAYINWEEK' not found in the sheet.");
//    }

//    int dayInWeekColumnNumber = dayInWeekColumn.Address.ColumnNumber;

//    //**********************************************************************
//    // מעבר על השורות מתחת לשורת הכותרת (למשל, משורה שלישית ואילך)
//    foreach (var row in worksheet.RowsUsed().Skip(1)) // דילוג על שורות 1 ו-2 (כותרת ושורת DAYINWEEK)
//    {
//        var key = row.Cell(1).GetValue<string>(); // המפתח הוא הערך בעמודה הראשונה
//        var value = row.Cell(dayInWeekColumnNumber).GetValue<string>(); // הערך מהעמודה שנמצאה

//        // יצירת אובייקט חדש מסוג DayEntry והוספתו לרשימה
//        dayEntries.Add(new DayEntry
//        {
//            Key = key,
//            Values = value
//        });
//    }

//    return dayEntries;
//}



//public async Task GenerateExercisePlanAndLogAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<List<ExerciseDTO>>>(); // רשימה כללית של תרגילים לכל הימים

//    // מעבר על כל יום ברשימת הימים
//    foreach (var dayList in trainingParams.DayLists)
//    {
//        var dayExercises = new List<List<ExerciseDTO>>(); // רשימה לתרגילים עבור היום הנוכחי

//        // מעבר על כל DayEntry ברשימת היום
//        foreach (var dayEntry in dayList)
//        {
//            var muscleGroupExercises = new List<ExerciseDTO>(); // רשימה לתרגילים עבור קבוצת השרירים

//            // קבלת ה-Key וה-Values מתוך DayEntry
//            var dayKey = dayEntry.Key; // ה-Key מייצג את היום
//            var muscles = dayEntry.Values.Split(',').Select(m => m.Trim()); // פיצול רשימת השרירים

//            // מעבר על כל שריר בקבוצה
//            foreach (var muscle in muscles)
//            {
//                var matchingMuscleSize = trainingParams.MuscleSizeData.FirstOrDefault(ms => ms.Value
//                    .Any(m => string.Equals(m.ToString().Trim(), muscle.Trim(), StringComparison.OrdinalIgnoreCase)));

//                if (matchingMuscleSize.Key == null)
//                {
//                    logger.LogWarning($"Muscle '{muscle}' not found in MuscleSizeData. Available muscles: {string.Join(", ", trainingParams.MuscleSizeData.SelectMany(ms => ms.Value))}");
//                    continue; // דלג על השריר אם הוא לא נמצא
//                }

//                // מצא את הקטגוריה והכמות מהדיקשנרי TimeCategories
//                var matchingCategory = trainingParams.TimeCategories
//                    .FirstOrDefault(tc => tc.Key.MuscleSize == matchingMuscleSize.Key);

//                if (matchingCategory.Key.Category == null)
//                {
//                    logger.LogWarning($"No matching category found for MuscleSize '{matchingMuscleSize.Key}' and Muscle '{muscle}'.");
//                    continue; // דלג אם אין קטגוריה מתאימה
//                }

//                int exerciseCount = matchingCategory.Value;

//                // שלוף תרגילים מהמסד נתונים
//                var exercises = await GetExercisesForMuscleAndCategoryAsync(muscle, matchingCategory.Key.Category, exerciseCount);

//                if (!exercises.Any())
//                {
//                    logger.LogWarning($"No exercises found for Muscle '{muscle}' and Category '{matchingCategory.Key.Category}'.");
//                }

//                muscleGroupExercises.AddRange(exercises); // הוספת התרגילים לרשימת קבוצת השרירים
//            }

//            dayExercises.Add(muscleGroupExercises); // הוספת קבוצת השרירים ליום הנוכחי
//        }

//        exercisePlan.Add(dayExercises); // הוספת היום לרשימה הכללית
//    }

//public async Task GenerateExercisePlanAndLogAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>(); // רשימה של רשימות תרגילים (כל יום הוא רשימה)

//    // מעבר על כל יום ברשימת הימים
//    foreach (var dayList in trainingParams.DayLists)
//    {
//        var dayExercises = new List<ExerciseDTO>(); // רשימה לתרגילים עבור היום הנוכחי

//        // מעבר על כל DayEntry ברשימת היום
//        foreach (var dayEntry in dayList)
//        {
//            // קבלת ה-Key וה-Values מתוך DayEntry
//            var dayKey = dayEntry.Key; // ה-Key מייצג את היום
//            var muscles = dayEntry.Values.Split(',').Select(m => m.Trim()); // פיצול רשימת השרירים

//            // מעבר על כל שריר בקבוצה
//            foreach (var muscle in muscles)
//            {
//                // מציאת התאמה לשריר במידע MuscleSizeData
//                var matchingMuscleSize = trainingParams.MuscleSizeData.FirstOrDefault(ms => ms.Value
//                    .Any(m => string.Equals(m.ToString().Trim(), muscle.Trim(), StringComparison.OrdinalIgnoreCase)));

//                if (matchingMuscleSize.Key == null)
//                {
//                    logger.LogWarning($"Muscle '{muscle}' not found in MuscleSizeData. Available muscles: {string.Join(", ", trainingParams.MuscleSizeData.SelectMany(ms => ms.Value))}");
//                    continue; // דלג על השריר אם הוא לא נמצא
//                }

//                // מציאת הקטגוריה והכמות מתוך TimeCategories
//                var matchingCategory = trainingParams.TimeCategories
//                    .FirstOrDefault(tc => tc.Key.MuscleSize == matchingMuscleSize.Key);

//                if (matchingCategory.Key.Category == null)
//                {
//                    logger.LogWarning($"No matching category found for MuscleSize '{matchingMuscleSize.Key}' and Muscle '{muscle}'.");
//                    continue; // דלג אם אין קטגוריה מתאימה
//                }

//                int exerciseCount = matchingCategory.Value;

//                // שליפת תרגילים מהמסד נתונים
//                var exercises = await GetExercisesForMuscleAndCategoryAsync(muscle, matchingCategory.Key.Category, exerciseCount);

//                if (!exercises.Any())
//                {
//                    logger.LogWarning($"No exercises found for Muscle '{muscle}' and Category '{matchingCategory.Key.Category}'.");
//                }

//                dayExercises.AddRange(exercises); // הוספת התרגילים לרשימה של היום
//            }
//        }

//        exercisePlan.Add(dayExercises); // הוספת תרגילי היום לרשימה הכללית
//    }

//    // הדפסת התוכנית ללוג
//    foreach (var (day, dayIndex) in exercisePlan.Select((value, index) => (value, index)))
//    {
//        logger.LogInformation($"Day {dayIndex + 1} Exercises:");

//        foreach (var exercise in day)
//        {
//            logger.LogInformation($"  - {exercise.ExerciseName}");
//        }
//    }

//}


// פונקציה לשליפת כל הערכים בעמודה מסוימת
//public List<object> ExtractColumnValues(IXLWorksheet worksheet, int column)
//{
//    return worksheet.Column(column).CellsUsed().Skip(1).Select(c => (object)c.Value).ToList();
//}
//public async Task GenerateExercisePlanAndLogAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>();

//    // מעבר על כל יום ברשימת הימים
//    foreach (var dayEntry in trainingParams.DayLists)
//    {
//        var dayKey = dayEntry.Key; // ה-Key מייצג את היום
//        var muscle = dayEntry.Values; // ה-Values מייצגים את רשימת השרירים עבור היום

//        var dayExercises = new List<ExerciseDTO>();

//        //// מעבר על כל שריר ברשימה של אותו יום
//        ////string cleanedMuscleName = muscle.ToString().Trim().ToLower();

//        //// מצא את גודל השריר מתוך MuscleSizeData
//        ////var matchingMuscleSize = trainingParams.MuscleSizeData
//        ////    .FirstOrDefault(ms => ms.Value
//        ////        .Select(m => m.ToString().Trim().ToLower())
//        ////        .Contains(cleanedMuscleName));
//        //var matchingMuscleSize = trainingParams.MuscleSizeData
//        //    .FirstOrDefault(ms => ms.Value.Contains(muscle));


//        //if (matchingMuscleSize.Key == null)
//        //{
//        //    logger.LogWarning($"Muscle '{muscle}' not found in MuscleSizeData. Available muscles: {string.Join(", ", trainingParams.MuscleSizeData.SelectMany(ms => ms.Value))}");
//        //    continue; // דלג על השריר אם הוא לא נמצא
//        //}

//        var matchingMuscleSize = trainingParams.MuscleSizeData.FirstOrDefault(ms => ms.Value
//              .Any(m => string.Equals(m.ToString().Trim(), muscle.Trim(), StringComparison.OrdinalIgnoreCase)));

//        if (matchingMuscleSize.Key == null)
//        {
//            logger.LogWarning($"Muscle '{muscle}' not found in MuscleSizeData. Available muscles: {string.Join(", ", trainingParams.MuscleSizeData.SelectMany(ms => ms.Value))}");
//            continue; // דלג על השריר אם הוא לא נמצא
//        }



//        // מצא את הקטגוריה והכמות מהדיקשנרי TimeCategories
//        var matchingCategory = trainingParams.TimeCategories
//            .FirstOrDefault(tc => tc.Key.MuscleSize == matchingMuscleSize.Key);

//        if (matchingCategory.Key.Category == null)
//        {
//            logger.LogWarning($"No matching category found for MuscleSize '{matchingMuscleSize.Key}' and Muscle '{muscle}'.");
//            continue; // דלג אם אין קטגוריה מתאימה
//        }

//        int exerciseCount = matchingCategory.Value;

//        // כאן הערך של הקטגוריה מוחלף ב-dayKey
//       // logger.LogInformation($"Retrieving exercises for Muscle: '{muscle}', Category: '{dayKey}', Count: {exerciseCount}");

//        // שלוף תרגילים מהמסד נתונים
//        var exercises = await GetExercisesForMuscleAndCategoryAsync(muscle, dayKey, exerciseCount);

//        if (!exercises.Any())
//        {
//            logger.LogWarning($"No exercises found for Muscle '{muscle}' and Category '{dayKey}'.");
//        }

//        dayExercises.AddRange(exercises);


//        // הוסף את רשימת התרגילים של היום לתוכנית
//        exercisePlan.Add(dayExercises);
//    }

//    // הדפסת התוכנית ללוג
//    foreach (var (day, index) in exercisePlan.Select((value, index) => (value, index)))
//    {
//        logger.LogInformation($"Exercises {index + 1}:");
//        foreach (var exercise in day)
//        {
//            logger.LogInformation($"- {exercise.ExerciseName}");
//        }
//    }
//}


//private List<List<object>> ExtractDayLists(IXLWorksheet worksheet, int daysInWeek)
//{
//    var dayLists = new List<List<object>>();

//    foreach (var col in worksheet.ColumnsUsed())
//    {
//        // בדוק אם הערך בשורה הראשונה תואם ל-daysInWeek
//        if (col.FirstCell().GetValue<int>() == daysInWeek)
//        {
//            // הוסף את הערכים מהעמודה (מתחיל משורה שנייה)
//            dayLists.Add(col.CellsUsed().Skip(1).Select(c => (object)c.Value).ToList());
//        }
//    }

//    return dayLists;
//}
//private Dictionary<object, List<object>> ExtractDayLists(IXLWorksheet worksheet, int daysInWeek)
//{
//    var dayLists = new Dictionary<object, List<object>>();

//    foreach (var row in worksheet.RowsUsed().Skip(1)) // דילוג על שורת הכותרת
//    {
//        var key = row.Cell(1).Value; // המפתח הוא הערך בעמודה הראשונה
//        var values = row.Cells(2, row.LastCellUsed().Address.ColumnNumber)
//                        .Select(c => (object)c.Value)
//                        .ToList(); // הערכים הם שאר התאים בשורה

//        if (!dayLists.ContainsKey(key))
//        {
//            dayLists[key] = values;
//        }
//        else
//        {
//            throw new Exception($"Duplicate key '{key}' found in the sheet.");
//        }
//    }

//    return dayLists;
//}

// הפונקציה המעודכנת לטעינת הנתונים לאובייקט DayEntry
//private List<DayEntry> ExtractDayLists(IXLWorksheet worksheet, int daysInWeek)
//{
//    var dayEntries = new List<DayEntry>();

//    foreach (var row in worksheet.RowsUsed().Skip(1)) // דילוג על שורת הכותרת
//    {
//       // var key = row.Cell(1).GetValue<string>(); // המפתח הוא הערך בעמודה הראשונה כ-String
//        //var values = row.Cells(2, row.LastCellUsed().Address.ColumnNumber)
//        //                .Select(c => c.GetValue<string>())
//        //                .ToList(); // הערכים הם שאר התאים בשורה כ-Strings
//        var values2 = row.Cells(2,
//        // יצירת אובייקט חדש מסוג DayEntry והוספתו לרשימה
//        dayEntries.Add(new DayEntry
//        {
//            Key = key,
//            Values = values
//        });
//    }

//    return dayEntries;
//}


//private Dictionary<int, int> ExtractTimeCategoryList(IXLWorksheet worksheet, int time)
//{
//    var timeCategoryList = new Dictionary<int, int>();

//    // מצא את השורה בעמודה הראשונה שבה הערך שווה ל-time
//    int targetRow = FindRowByValue(worksheet, 1, time, $"Row with the specified time ({time}) not found.");

//    // מצא את מספר העמודה המתאימה ל-TIME
//    int timeColumn = 1; // העמודה הראשונה שבה מחפשים את הערך TIME

//    // עבור על כל העמודות בשורה שנמצאה
//    foreach (var col in worksheet.ColumnsUsed())
//    {
//        // אם העמודה היא העמודה שבה נמצא הערך time, דלג עליה
//        if (col.ColumnNumber() == timeColumn)
//        {
//            continue;
//        }

//        var keyCell = col.Cell(1); // התא בשורה הראשונה (כותרת)
//        var valueCell = col.Cell(targetRow); // התא בעמודה ובשורה שנמצאה

//        // ודא שהתאים אינם ריקים ושהערכים שלהם חוקיים
//        if (!keyCell.IsEmpty() && !valueCell.IsEmpty() &&
//            int.TryParse(keyCell.GetValue<string>(), out int key) &&
//            int.TryParse(valueCell.GetValue<string>(), out int value))
//        {
//            timeCategoryList[key] = value; // הוסף למילון
//        }
//        else
//        {
//            logger.LogWarning($"Skipping invalid or empty cells at {keyCell.Address} or {valueCell.Address}");
//        }
//    }

//    return timeCategoryList;
//}
//private Dictionary<int, int> ExtractTimeCategoryList(IXLWorksheet worksheet, int goal, int time)
//{
//    var timeCategoryList = new Dictionary<int, int>();

//    // Filter rows where column 2 matches the provided goal
//    var matchingRows = worksheet.RowsUsed()
//        .Where(row => row.Cell(2).GetValue<int>() == goal)
//        .ToList();

//    if (!matchingRows.Any())
//    {
//        logger.LogWarning($"No rows found with goal {goal} in the Time sheet.");
//        throw new Exception($"No rows found with goal {goal} in the Time sheet.");
//    }

//    // Find the column where the first row matches the provided time
//    int timeColumn = worksheet.ColumnsUsed()
//        .FirstOrDefault(col => col.FirstCell().GetValue<int>() == time)?
//        .ColumnNumber() ?? -1;

//    if (timeColumn == -1)
//    {
//        logger.LogWarning($"Column with time {time} not found in the Time sheet.");
//        throw new Exception($"Column with time {time} not found in the Time sheet.");
//    }

//    // Populate the dictionary with values from the matching rows
//    foreach (var row in matchingRows)
//    {
//        int categoryId = row.Cell(2).GetValue<int>(); // Column 2 contains category ID
//        int duration = row.Cell(timeColumn).GetValue<int>(); // Value in the time column
//        timeCategoryList[categoryId] = duration;
//    }

//    return timeCategoryList;
//}
//private Dictionary<string, int> ExtractTimeCategoryList(IXLWorksheet worksheet, int goal, int time)
//{
//    var timeCategoryList = new Dictionary<string, int>();

//    // סינון שורות שבהן עמודה A (משך זמן) ועמודה B (מטרה) תואמות לפרמטרים שהוזנו
//    var matchingRows = worksheet.RowsUsed()
//        .Where(row => row.Cell(1).GetValue<int>() == time && row.Cell(2).GetValue<int>() == goal)
//        .ToList();

//    if (!matchingRows.Any())
//    {
//        logger.LogWarning($"No rows found with time {time} and goal {goal} in the Time sheet.");
//        throw new Exception($"No rows found with time {time} and goal {goal} in the Time sheet.");
//    }

//    // מעבר על כל השורות שזוהו והתאמת הנתונים למבנה ה-Dictionary
//    foreach (var row in matchingRows)
//    {
//        string category = $"{row.Cell(3).GetValue<string>()}, {row.Cell(4).GetValue<string>()}, {row.Cell(5).GetValue<string>()}"; // שילוב של קטגוריות
//        int quantity = row.Cell(5).GetValue<int>(); // כמות מעמודה E

//        // הוספת הנתונים ל-Dictionary
//        if (!timeCategoryList.ContainsKey(category))
//        {
//            timeCategoryList[category] = quantity;
//        }
//        else
//        {
//            logger.LogWarning($"Duplicate entry found for category '{category}'. Only the first occurrence is retained.");
//        }
//    }

//    return timeCategoryList;
//}



//public async Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time)
//{
//    //לא קבוע
//    string filePath1 = @"C:\Users\user\Pictures\תכנות\שנה ב\פוריקט שנתי\C#\פרויקט חדש 20.04\Gym_Api\BLL\new.xlsx";
//   // string filePath1 = @"C:\Users\user\הורדות\אקסל מסודר.xlsx";
//    var trainingParams = await GetAllParams(filePath1, daysInWeek, goal, level, time);

//    if (trainingParams == null)
//    {
//        throw new Exception("Failed to retrieve training parameters.");
//    }

//    // Log retrieved parameters
//    for (int i = 0; i < trainingParams.DayLists.Count; i++)
//    {
//        logger.LogInformation($"Day List {i + 1}: " + string.Join(", ", trainingParams.DayLists[i]));
//    }

//    logger.LogInformation("Large Muscles: " + string.Join(", ", trainingParams.LargeMuscleList));
//    logger.LogInformation("Small Muscles: " + string.Join(", ", trainingParams.SmallMuscleList));
//    logger.LogInformation($"Repetitions: {trainingParams.MinRep}-{trainingParams.MaxRep}");
//    logger.LogInformation("Time Categories: " + string.Join(", ", trainingParams.TimeCategories.Select(kvp => $"[{kvp.Key.Category} - {kvp.Key.MuscleSize}: {kvp.Value}]")));
//    //var exercisePlan = await GenerateExercisePlanAsync(trainingParams);

//    // הדפסת התוכנית
//    //foreach (var day in exercisePlan)
//    //{
//    //    Console.WriteLine("Day Exercises:");
//    //    foreach (var exercise in day)
//    //    {
//    //        Console.WriteLine($"- {exercise.ExerciseName}");
//    //    }
//    //}
//    //return exercisePlan;
//}

//public async Task<TrainingParams> GetAllParams(string filePath, int daysInWeek, int goal, int level, int time)
//{
//    try
//    {
//        using (var workbook = new XLWorkbook(filePath))
//        {
//            //רשימה של השרירים
//            var daysWorksheet = GetWorksheet(workbook, DaysInWeekSheet);
//            var dayLists = ExtractDayLists(daysWorksheet, daysInWeek);
//            // רשימה של השרירים הגדולים
//            var largeMuscleWorksheet = GetWorksheet(workbook, MuscleSheet);
//            var largeMuscleList = largeMuscleWorksheet.Column(1).CellsUsed().Skip(1).Select(c => (object)c.Value).ToList();
//            // רשימה של השרירים הקטנים
//            var SmallMuscleWorksheet = GetWorksheet(workbook, MuscleSheet);
//            var SmallMuscleList = SmallMuscleWorksheet.Column(2).CellsUsed().Skip(1).Select(c => (object)c.Value).ToList();
//            //מספר החזרות
//            var repWorksheet = GetWorksheet(workbook, RepitByGoalSheet);
//            int colRep = FindColumnByValue(repWorksheet, 1, goal, "Column with the specified goal not found.");
//            int minRep = GetValueFromWorksheet(repWorksheet, "min", colRep, "Min value not found.");
//            int maxRep = GetValueFromWorksheet(repWorksheet, "max", colRep, "Max value not found.");
//            //זמן לקטגוריה
//            var timeCategoryWorksheet = GetWorksheet(workbook, CategorySheet);
//            var timeCategoryList = ExtractTimeCategoryList(timeCategoryWorksheet, goal, time);
//            //הכמות לשרירים dictinery 
//            var countToMuscleWorksheet = GetWorksheet(workbook, SumOfLargeByTimeSheet);
//            var categoryMuscleSizeData = ExtractCategoryMuscleSizeData(countToMuscleWorksheet, time, daysInWeek);

//            return new TrainingParams
//            {
//                DayLists = dayLists,
//                LargeMuscleList = largeMuscleList,
//                SmallMuscleList = SmallMuscleList,
//                MinRep = minRep,
//                MaxRep = maxRep,
//                TimeCategories= categoryMuscleSizeData,
//            };
//        }
//    }
//    catch (Exception ex)
//    {
//        logger.LogError(ex, "Error while processing training parameters.");
//        throw;
//    }
//}


//public async Task GenerateExercisePlanAndLogAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>();

//    // מעבר על כל יום ברשימת הימים
//    foreach (var dayList in trainingParams.DayLists)
//    {
//        var dayExercises = new List<ExerciseDTO>();

//        // מעבר על כל שריר ברשימה של היום
//        foreach (var muscle in dayList)
//        {
//            string cleanedMuscleName = muscle.ToString().Trim().ToLower();

//            // מצא את גודל השריר מתוך MuscleSizeData
//            var matchingMuscleSize = trainingParams.MuscleSizeData
//                .FirstOrDefault(ms => ms.Value
//                    .Select(m => m.ToString().Trim().ToLower())
//                    .Contains(cleanedMuscleName));

//            if (matchingMuscleSize.Key == null)
//            {
//                logger.LogWarning($"Muscle '{cleanedMuscleName}' not found in MuscleSizeData. Available muscles: {string.Join(", ", trainingParams.MuscleSizeData.SelectMany(ms => ms.Value))}");
//                continue; // דלג על השריר אם הוא לא נמצא
//            }

//            // מצא את הקטגוריה והכמות מהדיקשנרי TimeCategories
//            var matchingCategory = trainingParams.TimeCategories
//                .FirstOrDefault(tc => tc.Key.MuscleSize == matchingMuscleSize.Key);

//            if (matchingCategory.Key.Category == null)
//            {
//                logger.LogWarning($"No matching category found for MuscleSize '{matchingMuscleSize.Key}' and Muscle '{cleanedMuscleName}'.");
//                continue; // דלג אם אין קטגוריה מתאימה
//            }

//            int exerciseCount = matchingCategory.Value;
//            logger.LogInformation($"Retrieving exercises for Muscle: '{cleanedMuscleName}', Category: '{matchingCategory.Key.Category.Trim()}', Count: {exerciseCount}");

//            // שלוף תרגילים מהמסד נתונים
//            var exercises = await GetExercisesForMuscleAndCategoryAsync(cleanedMuscleName, matchingCategory.Key.Category.Trim(), exerciseCount);

//            if (!exercises.Any())
//            {
//                logger.LogWarning($"No exercises found for Muscle '{cleanedMuscleName}' and Category '{matchingCategory.Key.Category}'.");
//            }

//            dayExercises.AddRange(exercises);
//        }

//        // הוסף את רשימת התרגילים של היום לתוכנית
//        exercisePlan.Add(dayExercises);
//    }

//    // הדפסת התוכנית ללוג
//    foreach (var (day, index) in exercisePlan.Select((value, index) => (value, index)))
//    {
//        logger.LogInformation($"Day {index + 1} Exercises:");
//        foreach (var exercise in day)
//        {
//            logger.LogInformation($"- {exercise.ExerciseName}");
//        }
//    }
//}

//public async Task GenerateExercisePlanAndLogAsync(TrainingParams trainingParams)
//{
//    var exercisePlan = new List<List<ExerciseDTO>>();

//    // מעבר על כל יום במילון הימים
//    foreach (var dayEntry in trainingParams.DayLists)
//    {
//        var dayKey = dayEntry.Key; // ה-Key מייצג את היום
//        var dayList = dayEntry.Value; // ה-Value מייצג את רשימת השרירים עבור היום

//        var dayExercises = new List<ExerciseDTO>();

//        // מעבר על כל שריר ברשימה של אותו יום
//        foreach (var muscle in dayList)
//        {
//            string cleanedMuscleName = muscle.ToString().Trim().ToLower();

//            // מצא את גודל השריר מתוך MuscleSizeData
//            var matchingMuscleSize = trainingParams.MuscleSizeData
//                .FirstOrDefault(ms => ms.Value
//                    .Select(m => m.ToString().Trim().ToLower())
//                    .Contains(cleanedMuscleName));

//            if (matchingMuscleSize.Key == null)
//            {
//                logger.LogWarning($"Muscle '{cleanedMuscleName}' not found in MuscleSizeData. Available muscles: {string.Join(", ", trainingParams.MuscleSizeData.SelectMany(ms => ms.Value))}");
//                continue; // דלג על השריר אם הוא לא נמצא
//            }

//            // מצא את הקטגוריה והכמות מהדיקשנרי TimeCategories
//            var matchingCategory = trainingParams.TimeCategories
//                .FirstOrDefault(tc => tc.Key.MuscleSize == matchingMuscleSize.Key);

//            if (matchingCategory.Key.Category == null)
//            {
//                logger.LogWarning($"No matching category found for MuscleSize '{matchingMuscleSize.Key}' and Muscle '{cleanedMuscleName}'.");
//                continue; // דלג אם אין קטגוריה מתאימה
//            }

//            int exerciseCount = matchingCategory.Value;

//            logger.LogInformation($"Retrieving exercises for Muscle: '{cleanedMuscleName}', Category: '{dayKey}', Count: {exerciseCount}");

//            // שלוף תרגילים מהמסד נתונים
//            var exercises = await GetExercisesForMuscleAndCategoryAsync(cleanedMuscleName, matchingCategory.Key.Category.Trim(), exerciseCount);

//            if (!exercises.Any())
//            {
//                logger.LogWarning($"No exercises found for Muscle '{cleanedMuscleName}' and Category '{matchingCategory.Key.Category}'.");
//            }

//            dayExercises.AddRange(exercises);
//        }

//        // הוסף את רשימת התרגילים של היום לתוכנית
//        exercisePlan.Add(dayExercises);
//    }

//    // הדפסת התוכנית ללוג
//    foreach (var (day, index) in exercisePlan.Select((value, index) => (value, index)))
//    {
//        logger.LogInformation($"Day {index + 1} Exercises:");
//        foreach (var exercise in day)
//        {
//            logger.LogInformation($"- {exercise.ExerciseName}");
//        }
//    }
//}

//public async Task<List<ExerciseDTO>> GetExercisesForMuscleByCategoryAsync(string muscleName, string categoryName, int count)
//{
//    // שליפת תרגילים מה-DAL
//    var exercises = await muscleDAL.GetExercisesForMuscleByCategoryAsync(muscleName, categoryName);

//    // המרת התרגילים ל-DTO
//    var exerciseDTOs = exercises.Select(e => new ExerciseDTO
//    {
//        ExerciseId = e.ExerciseId,
//        ExerciseName = e.ExerciseName
//    }).ToList();

//    // בחר מספר תרגילים באופן אקראי
//    return exerciseDTOs.OrderBy(e => Guid.NewGuid()).Take(count).ToList();
//}
