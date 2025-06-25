using BLL;
using DAL;
using IBLL;
using IDAL;
using OfficeOpenXml;
using System.ComponentModel;
using ClosedXML.Excel;
using AutoMapper;
using DTO;

namespace API
{
    using API.Profiles;
    using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
    using OfficeOpenXml;
    using System.Text.Json.Serialization;

    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // הוספת CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAllOrigins", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
            builder.Services.AddAutoMapper(typeof(MappingProfile));


            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<ICategoryBLL, CategoryBLL>();
            builder.Services.AddScoped<ICategoryDAL, CategoryDAL>();
            builder.Services.AddScoped<IEquipmentDAL, EquipmentDAL>();
            builder.Services.AddScoped<IExerciseBLL, ExerciseBLL>();
            builder.Services.AddScoped<IExerciseDAL, ExerciseDAL>();
            builder.Services.AddScoped<IGoalBLL, GoalBLL>();
            builder.Services.AddScoped<IGoalDAL, GoalDAL>();
            builder.Services.AddScoped<IJointBLL, JointBLL>();
            builder.Services.AddScoped<IJointDAL, JointDAL>();
            builder.Services.AddScoped<IMuscleBLL, MuscleBLL>();
            builder.Services.AddScoped<IMuscleDAL, MuscleDAL>();
            builder.Services.AddScoped<ISizeBLL, SizeBLL>();
            builder.Services.AddScoped<ISizeDAL, SizeDAL>();
            builder.Services.AddScoped<ISubMuscleBLL, SubMuscleBLL>();
            builder.Services.AddScoped<ISubMuscleDAL, SubMuscleDAL>();
            builder.Services.AddScoped<ITraineeBLL, TraineeBLL>();
            builder.Services.AddScoped<ITraineeDAL, TraineeDAL>();
            builder.Services.AddScoped<ITrainingDayBLL, TrainingDayBLL>();
            builder.Services.AddScoped<ITrainingDayDAL, TrainingDayDAL>();
            builder.Services.AddScoped<IFitnessLevelBLL, FitnessLevelBLL>();
            builder.Services.AddScoped<IFitnessLevelDAL, FitnessLevelDAL>();
            builder.Services.AddScoped<ITrainingDurationBLL, TrainingDurationBLL>();
            builder.Services.AddScoped<ITrainingDurationDAL, TrainingDurationDAL>();
            builder.Services.AddScoped<IEquipmentBLL, EquipmentBLL>();
            builder.Services.AddScoped<IEquipmentDAL, EquipmentDAL>();
            builder.Services.AddScoped<IMuscleTypeBLL, MuscleTypeBLL>();
            builder.Services.AddScoped<IMuscleTypeDAL, MuscleTypeDAL>();
            builder.Services.AddScoped<IGraphEdgeBLL, GraphEdgeBLL>();
            builder.Services.AddScoped<IGraphEdgeDAL, GraphEdgeDAL>();
            builder.Services.AddScoped<IDeviceMuscleEdgeBLL, DeviceMuscleEdgeBLL>();
            builder.Services.AddScoped<IDeviceMuscleEdgeDAL, DeviceMuscleEdgeDAL>();
            builder.Services.AddScoped<IMuscleEdgeBLL, MuscleEdgeBLL>();
            builder.Services.AddScoped<IMuscleEdgeDAL, MuscleEdgeDAL>();
            builder.Services.AddScoped<IExercisePlanBLL, ExercisePlanBLL>();
            builder.Services.AddScoped<IExercisePlanDAL, ExercisePlanDAL>();
            builder.Services.AddScoped<IPlanDayDAL, PlanDayDAL>();
            builder.Services.AddScoped<IPlanDayBLL, PlanDayBLL>();
            builder.Services.AddScoped<ITrainingPlanDAL, TrainingPlanDAL>();
            builder.Services.AddScoped<ITrainingPlanBLL, TrainingPlanBLL>();

            builder.Services.AddScoped<CreateTrainingPlan>();
            builder.Services.AddScoped<ActiveWorkoutManager>();


            builder.Services.AddMemoryCache();
            // builder.Services.AddSingleton<ActiveWorkoutManager>();

            builder.Services.AddControllers()
             .AddJsonOptions(options =>
              {
                  // זה מה שמונע את לולאת ההתייחסות ב-JSON Serialization
                  options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                  // אפשר להוסיף כאן גם הגדרות אחרות ל-JSON אם יש צורך
                  options.JsonSerializerOptions.WriteIndented = true; // אופציונלי: יגרום לפלט ה-JSON להיות עם הזחות לקריאה נוחה יותר
              });

            var app = builder.Build();

            // Configure the HTTP request pipeline.


            // שימוש ב-CORS
            app.UseCors("AllowAllOrigins");


            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }



            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
