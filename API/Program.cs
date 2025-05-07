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
    using OfficeOpenXml;
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
            builder.Services.AddScoped<IProgramExerciseBLL, ProgramExerciseBLL>();
            builder.Services.AddScoped<IProgramExerciseDAL, ProgramExerciseDAL>();
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


            // Add services to the container.

            builder.Services.AddControllers();

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
       // הגדרת רישיון EPPlus
            //OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
            //ExcelPackage.License = LicenseContext.NonCommercial;
            //ExcelPackage.SetLicenseContext(LicenseContext.NonCommercial);
            //OfficeOpenXml.ExcelPackage.SetLicenseContext(OfficeOpenXml.LicenseContext.NonCommercial);
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            //ExcelPackage.License = LicenseContext.NonCommercial;
            // ExcelPackage.SetLicense(LicenseContext.NonCommercial);
            // קריאת הרישיון מתוך appsettings.json
            //var licenseContext = builder.Configuration["EPPlus:LicenseContext"];
            //if (Enum.TryParse(licenseContext, out LicenseContext license))
            //{
            //    ExcelPackage.SetLicense(license);
            //}
            // קריאת הרישיון מתוך appsettings.json
            //var licenseContext = builder.Configuration["EPPlus:LicenseContext"];
            //if (Enum.TryParse(licenseContext, out LicenseContext license))
            //{
            //    // הגדרת הרישיון
            //    ExcelPackage.LicenseContext = license;
            //}