﻿using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IExerciseDAL
    {
        Task AddExerciseAsync(Exercise exercise);
        Task<List<Exercise>> GetAllExercisesAsync();
        Task<Exercise> GetExerciseByIdAsync(int id);
        Task<Exercise> GetExerciseByNameAsync(string name);
        Task UpdateExerciseAsync(Exercise exercise, int id);
        Task DeleteExerciseAsync(int id);
        Task AddExerciseToCategoryAsync(Exercise exercise, int categoryId);
       // Task<int> GetCategoryOfExercise(int exerciseId);
        Task<List<int>> GetCategoryIdsOfExercise(int exerciseId);
        Task<List<Equipment>> GetEquipmentByExercisesIdAsync(List<int> exerciseIds);
        Task<string> GetSubMuscleByExerciseAsync(int exerciseId);
        Task<string> GetMuscleByExerciseAsync(int exerciseId);
        Task<Muscle> GetAllMusclesByExerciseAsync(int exerciseId);
        Task<int> GetJointCount(int exerciseId);

    }
}
