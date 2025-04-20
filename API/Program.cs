using BLL;
using DAL;
using IBLL;
using IDAL;

namespace API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddScoped<ICategoryBLL, CategoryBLL>();
            builder.Services.AddScoped<ICategoryDAL, CategoryDAL>();
            //add all BLL and DAL classes here
            // builder.Services.AddScoped<IEquipmentBLL, EquipmentBLL>();
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

            // Add services to the container.

            builder.Services.AddControllers();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

          



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
