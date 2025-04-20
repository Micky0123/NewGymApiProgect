using AutoMapper;
using DBEntities.Models;
using DTO;
using IBLL;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    //    public enum goal
    //    {
    //      1	ירידה במשקל
    //      2	עליה במשקל
    //      3	חיטוב
    //      4	העלאת מסת שריר
    //      5	שיפור כללי
    //    }
    //    public enum muscle
    //    {
    //      1	רגליים
    //      2	חזה
    //      3	גב
    //      4	כתפיים
    //      5	יד קדמית
    //      6	יד אחורית
    //      7	בטן
    //      8	זוקפי גו
    //     }
    public class ProgramExerciseBLL:IProgramExerciseBLL
    {
        private readonly IMuscleDAL muscleDAL;
        private readonly IGoalDAL goalDAL;
        private readonly IMapper mapper;
        public ProgramExerciseBLL(IMuscleDAL muscleDAL)
        {
            this.muscleDAL = muscleDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Muscle, MuscleDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddProgramExerciseAsync(ProgramExerciseDTO programExercise, int daysInWeek, int goal, int level, int time)
        {
            // this parameter is the number of days in a week that the user wants to train
            int SmallMuscle;//הכמות של תרגילים לשרירים קטנים
            int LargeMuscle;//הכמות של תרגילים לשרירים גדולים
            int minRep = 0;//הכמות של חזרות מינימליות
            int maxRep = 0;//הכמות של חזרות המקסימליות
            int warm;//זמן לחימום
            int power;//זמן לאימון כוח
            int Cardio;//זמן לאירובי
            List<string> muscleGroups1 = new List<string>();
            List<string> muscleGroups2 = new List<string>();
            List<string> muscleGroups3 = new List<string>();
            List<string> muscleGroups4 = new List<string>();
            bool isAerobic=false;

            // Set the number of repetitions based on the goal
            switch (goal)
            {
                case 1:
                    minRep = 8;
                    maxRep = 12;
                    break;
                case 2:
                    minRep = 8;
                    maxRep = 12;
                    break;
                case 3:
                    minRep = 6;
                    maxRep = 8;
                    break;
                case 4:
                    minRep = 6;
                    maxRep = 12;
                    break;
                case 5:
                    minRep = 5;
                    maxRep = 10;
                    break;
                default:
                    throw new ArgumentException("Invalid goal");
            }

            //list of muscles to 1-3 days
           
            if (daysInWeek >= 1 & daysInWeek<=3)
            {
                muscleGroups1.Add("רגליים");
                muscleGroups1.Add("חזה");
                muscleGroups1.Add("גב");
                muscleGroups1.Add("כתפיים");
                muscleGroups1.Add("יד קדמית");
                muscleGroups1.Add("יד אחורית");
                muscleGroups1.Add("בטן");
                muscleGroups1.Add("זוקפי גו");

                //במידה וזמן האימון הוא 40 דקות לכל שריר גדול 2 תרגילים 
                //לשרירים קטנים תרגיל אחד
                if (time == 40)
                {
                    SmallMuscle = 1;
                    LargeMuscle = 2;

                    warm = 5;
                    power = 20;
                    Cardio = 20;
                    if (await goalDAL.GetIdOfGoalByNameAsync("עליה במשקל") == goal)
                    {
                        Cardio = 15;
                    }
                }
                //במידה וזמן האימון הוא 60 דקות לכל שריר גדול 3 תרגילים
                //לשרירים קטנים 2 תרגילים
                else if (time == 60)
                {
                    SmallMuscle = 2;
                    LargeMuscle = 3;

                    warm = 10;
                    power = 40;
                    Cardio = 40;
                }
                else
                {
                    throw new ArgumentException("Invalid time");
                }
            }
            else if (daysInWeek == 4)
            {
                // Add logic for 4 days
                
                isAerobic=true;
                muscleGroups1.Add("רגליים");
                muscleGroups3.Add("חזה");
                muscleGroups2.Add("גב");
                muscleGroups2.Add("כתפיים");
                muscleGroups3.Add("יד קדמית");
                muscleGroups3.Add("יד אחורית");
                muscleGroups1.Add("בטן");
                muscleGroups1.Add("זוקפי גו");

                //במידה וזמן האימון הוא 40 דקות לכל שריר גדול 6 תרגילים
                //לשרירים קטנים 2 תרגילים
                if (time == 40)
                {
                    SmallMuscle = 2;
                    LargeMuscle = 6;
                }
                //(לשים לב ליום 1 של אימון זה...)
                //במידה וזמן האימון הוא 60 דקות לכל שריר גדול 8 תרגילים
                //לשרירים קטנים 4 תרגילים
                else if (time == 60)
                {
                    SmallMuscle = 4;
                    LargeMuscle = 8;
                }
                else
                {
                    throw new ArgumentException("Invalid time");
                }
            }
            else if (daysInWeek == 5)
            {
                // Add logic for 5 days 
                isAerobic = true;
                muscleGroups1.Add("רגליים");
                muscleGroups3.Add("חזה");
                muscleGroups2.Add("גב");
                muscleGroups4.Add("כתפיים");
                muscleGroups3.Add("יד קדמית");
                muscleGroups4.Add("יד אחורית");
                muscleGroups1.Add("בטן");
                muscleGroups2.Add("זוקפי גו");

                //במידה וזמן האימון הוא 40 דקות לכל שריר גדול 6 תרגילים
                //לשרירים קטנים 2 תרגילים
                if (time == 40)
                {
                    SmallMuscle = 2;
                    LargeMuscle = 6;
                }
                //(לשים לב ליום 1 של אימון זה...)
                //במידה וזמן האימון הוא 60 דקות לכל שריר גדול 8 תרגילים
                //לשרירים קטנים 4 תרגילים
                else if (time == 60)
                {
                    SmallMuscle = 4;
                    LargeMuscle = 8;
                }
                else
                {
                    throw new ArgumentException("Invalid time");
                }
            }

            //עכשיו צריך לבחור את התרגילים ולסדר אותם לתוכנית אימון
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
    }
}





// This class is responsible for managing the relationship between programs and exercises.
// It will handle adding, updating, deleting, and retrieving exercises associated with a program.
// It will also handle the logic for assigning exercises to specific categories within a program.

// Add methods to manage exercises in a program
// For example:
// - AddExerciseToProgram
// - RemoveExerciseFromProgram
// - GetExercisesByProgramId

//public Task List<List<string>> DivideMuscleGroups(int daysInWeek)
//{
//    // This method divides the muscle groups into a list of lists based on the number of days in a week.
//    List<List<int>> muscleGroups = new List<List<int>>();

//    for (int i = 0; i < daysInWeek; i++)
//    {
//        List<string> muscleGroup = new List<string>();
//        muscleGroups.Add(muscleGroup);
//    }

//    muscleGroups[0].Add( "רגליים");
//    muscleGroups[1].Add("חזה");
//    muscleGroups[2].Add("גב");
//    muscleGroups[3].Add("כתפיים");
//    muscleGroups[4].Add("יד קדמית");
//    muscleGroups[5].Add("יד אחורית");
//    muscleGroups[6].Add("בטן");
//    muscleGroups[7].Add("זוקפי גו");

//    return muscleGroups;
//}



//public static string[][] DivideMuscleGroups(int daysInWeek)
//{


//    return muscleGroups;
//}

// This method divides the muscle groups into a list of lists based on the number of days in a week.
//    List<List<int>> muscleGroups = new List<List<int>>();

//    for (int i = 0; i < daysInWeek; i++)
//    {
//        List<string> muscleGroup = new List<string>();
//        muscleGroups.Add(muscleGroup);
//    }

//    muscleGroups[0].Add(muscleDAL.GetIdOfMuscleByNameAsync("רגליים"));
//    muscleGroups[1].Add(muscleDAL.GetIdOfMuscleByNameAsync("חזה"));
//    muscleGroups[2].Add(muscleDAL.GetIdOfMuscleByNameAsync("גב"));
//    muscleGroups[3].Add(muscleDAL.GetIdOfMuscleByNameAsync("כתפיים"));
//    muscleGroups[4].Add(muscleDAL.GetIdOfMuscleByNameAsync("יד קדמית"));
//    muscleGroups[5].Add(muscleDAL.GetIdOfMuscleByNameAsync("יד אחורית"));
//    muscleGroups[6].Add(muscleDAL.GetIdOfMuscleByNameAsync("בטן"));
//    muscleGroups[7].Add(muscleDAL.GetIdOfMuscleByNameAsync("זוקפי גו"));



//    return muscleGroups;
//}


//int[][] muscleGroups = new int[daysInWeek][];

//muscleGroups[0][0] = muscleDAL.GetIdOfMuscleByNameAsync("רגליים");
//muscleGroups[1][0] = muscleDAL.GetIdOfMuscleByNameAsync("חזה");
//muscleGroups[2][0] = muscleDAL.GetIdOfMuscleByNameAsync("גב");
//muscleGroups[3][0] = muscleDAL.GetIdOfMuscleByNameAsync("כתפיים");
//muscleGroups[4][0] = muscleDAL.GetIdOfMuscleByNameAsync("יד קדמית");
//muscleGroups[5][0] = muscleDAL.GetIdOfMuscleByNameAsync("יד אחורית");
//muscleGroups[6][0] = muscleDAL.GetIdOfMuscleByNameAsync("בטן");
//muscleGroups[7][0] = muscleDAL.GetIdOfMuscleByNameAsync("זוקפי גו");
