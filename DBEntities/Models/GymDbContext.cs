using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DBEntities.Models;

public partial class GymDbContext : DbContext
{
    public GymDbContext()
    {
    }

    public GymDbContext(DbContextOptions<GymDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<Goal> Goals { get; set; }

    public virtual DbSet<Joint> Joints { get; set; }

    public virtual DbSet<Muscle> Muscles { get; set; }

    public virtual DbSet<ProgramExercise> ProgramExercises { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<SubMuscle> SubMuscles { get; set; }

    public virtual DbSet<Trainee> Trainees { get; set; }

    public virtual DbSet<TrainingDay> TrainingDays { get; set; }

    public virtual DbSet<TrainingProgram> TrainingPrograms { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=מיכל\\SQLEXPRESS;Initial Catalog=GymDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B039B50AE");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EquipmentId).HasName("PK__Equipmen__34474599FC86338E");

            entity.Property(e => e.EquipmentId).HasColumnName("EquipmentID");
            entity.Property(e => e.EquipmentName).HasMaxLength(100);
        });

        modelBuilder.Entity<Exercise>(entity =>
        {
            entity.HasKey(e => e.ExerciseId).HasName("PK__Exercise__A074AD0F72E5FA53");

            entity.Property(e => e.ExerciseId).HasColumnName("ExerciseID");
            entity.Property(e => e.ExerciseName).HasMaxLength(100);

            entity.HasMany(d => d.Categories).WithMany(p => p.Exercises)
                .UsingEntity<Dictionary<string, object>>(
                    "ExerciseCategory",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ExerciseCategories_Categories"),
                    l => l.HasOne<Exercise>().WithMany()
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ExerciseCategories_Exercises"),
                    j =>
                    {
                        j.HasKey("ExerciseId", "CategoryId").HasName("PK__Exercise__01E43EAD5F95A763");
                        j.ToTable("ExerciseCategories");
                        j.IndexerProperty<int>("ExerciseId").HasColumnName("ExerciseID");
                        j.IndexerProperty<int>("CategoryId").HasColumnName("CategoryID");
                    });

            entity.HasMany(d => d.Equipment).WithMany(p => p.Exercises)
                .UsingEntity<Dictionary<string, object>>(
                    "ExerciseEquipment",
                    r => r.HasOne<Equipment>().WithMany()
                        .HasForeignKey("EquipmentId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ExerciseEquipment_Equipment"),
                    l => l.HasOne<Exercise>().WithMany()
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ExerciseEquipment_Exercises"),
                    j =>
                    {
                        j.HasKey("ExerciseId", "EquipmentId").HasName("PK__Exercise__2330D956676F16DE");
                        j.ToTable("ExerciseEquipment");
                        j.IndexerProperty<int>("ExerciseId").HasColumnName("ExerciseID");
                        j.IndexerProperty<int>("EquipmentId").HasColumnName("EquipmentID");
                    });

            entity.HasMany(d => d.Joints).WithMany(p => p.Exercises)
                .UsingEntity<Dictionary<string, object>>(
                    "ExerciseJoint",
                    r => r.HasOne<Joint>().WithMany()
                        .HasForeignKey("JointId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ExerciseJoints_Joints"),
                    l => l.HasOne<Exercise>().WithMany()
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ExerciseJoints_Exercises"),
                    j =>
                    {
                        j.HasKey("ExerciseId", "JointId").HasName("PK__Exercise__2EE5238FA67C4484");
                        j.ToTable("ExerciseJoints");
                        j.IndexerProperty<int>("ExerciseId").HasColumnName("ExerciseID");
                        j.IndexerProperty<int>("JointId").HasColumnName("JointID");
                    });

            entity.HasMany(d => d.MuscleGroups).WithMany(p => p.Exercises)
                .UsingEntity<Dictionary<string, object>>(
                    "ExercisesSize",
                    r => r.HasOne<Size>().WithMany()
                        .HasForeignKey("MuscleGroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ExercisesSize_Size"),
                    l => l.HasOne<Exercise>().WithMany()
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ExercisesSize_Exercises"),
                    j =>
                    {
                        j.HasKey("ExerciseId", "MuscleGroupId").HasName("PK__Exercise__D0E3038F7312BA46");
                        j.ToTable("ExercisesSize");
                        j.IndexerProperty<int>("ExerciseId").HasColumnName("ExerciseID");
                        j.IndexerProperty<int>("MuscleGroupId").HasColumnName("MuscleGroupID");
                    });

            entity.HasMany(d => d.Muscles).WithMany(p => p.Exercises)
                .UsingEntity<Dictionary<string, object>>(
                    "ExerciseMuscle",
                    r => r.HasOne<Muscle>().WithMany()
                        .HasForeignKey("MuscleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ExerciseMuscles_Muscles"),
                    l => l.HasOne<Exercise>().WithMany()
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ExerciseMuscles_Exercises"),
                    j =>
                    {
                        j.HasKey("ExerciseId", "MuscleId").HasName("PK__Exercise__5A017D7A69B0D6EC");
                        j.ToTable("ExerciseMuscles");
                        j.IndexerProperty<int>("ExerciseId").HasColumnName("ExerciseID");
                        j.IndexerProperty<int>("MuscleId").HasColumnName("MuscleID");
                    });

            entity.HasMany(d => d.SubMuscles).WithMany(p => p.Exercises)
                .UsingEntity<Dictionary<string, object>>(
                    "ExerciseSubMuscle",
                    r => r.HasOne<SubMuscle>().WithMany()
                        .HasForeignKey("SubMuscleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ExerciseSubMuscles_SubMuscles"),
                    l => l.HasOne<Exercise>().WithMany()
                        .HasForeignKey("ExerciseId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ExerciseSubMuscles_Exercises"),
                    j =>
                    {
                        j.HasKey("ExerciseId", "SubMuscleId").HasName("PK__Exercise__9B3B694E407114F8");
                        j.ToTable("ExerciseSubMuscles");
                        j.IndexerProperty<int>("ExerciseId").HasColumnName("ExerciseID");
                        j.IndexerProperty<int>("SubMuscleId").HasColumnName("SubMuscleID");
                    });
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.GoalId).HasName("PK__Goal__8A4FFF3167EB44C3");

            entity.ToTable("Goal");

            entity.Property(e => e.GoalId).HasColumnName("GoalID");
            entity.Property(e => e.GoalName).HasMaxLength(50);
        });

        modelBuilder.Entity<Joint>(entity =>
        {
            entity.HasKey(e => e.JointId).HasName("PK__Joints__E918E8095BA0336E");

            entity.Property(e => e.JointId).HasColumnName("JointID");
            entity.Property(e => e.JointName).HasMaxLength(100);
        });

        modelBuilder.Entity<Muscle>(entity =>
        {
            entity.HasKey(e => e.MuscleId).HasName("PK__Muscles__A75D075EAE5420E4");

            entity.Property(e => e.MuscleId).HasColumnName("MuscleID");
            entity.Property(e => e.MuscleName).HasMaxLength(100);
        });

        modelBuilder.Entity<ProgramExercise>(entity =>
        {
            entity.HasKey(e => e.ProgramExerciseId).HasName("PK__ProgramE__217C745EB399FA4A");

            entity.Property(e => e.ProgramExerciseId).HasColumnName("ProgramExerciseID");
            entity.Property(e => e.ExerciseId).HasColumnName("ExerciseID");
            entity.Property(e => e.ProgramId).HasColumnName("ProgramID");
            entity.Property(e => e.ProgramWeight).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Exercise).WithMany(p => p.ProgramExercises)
                .HasForeignKey(d => d.ExerciseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProgramExercises_Exercises");

            entity.HasOne(d => d.Program).WithMany(p => p.ProgramExercises)
                .HasForeignKey(d => d.ProgramId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProgramExercises_TrainingPrograms");
        });

        modelBuilder.Entity<Size>(entity =>
        {
            entity.HasKey(e => e.MuscleGroupId).HasName("PK__Size__097AE806BF25F629");

            entity.ToTable("Size");

            entity.Property(e => e.MuscleGroupId).HasColumnName("MuscleGroupID");
            entity.Property(e => e.MuscleGroupName).HasMaxLength(100);
        });

        modelBuilder.Entity<SubMuscle>(entity =>
        {
            entity.HasKey(e => e.SubMuscleId).HasName("PK__SubMuscl__B4FC441213FDD210");

            entity.Property(e => e.SubMuscleId).HasColumnName("SubMuscleID");
            entity.Property(e => e.MuscleId).HasColumnName("MuscleID");
            entity.Property(e => e.SubMuscleName).HasMaxLength(100);

            entity.HasOne(d => d.Muscle).WithMany(p => p.SubMuscles)
                .HasForeignKey(d => d.MuscleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SubMuscle__Muscl__0C85DE4D");
        });

        modelBuilder.Entity<Trainee>(entity =>
        {
            entity.HasKey(e => e.TraineeId).HasName("PK__Trainees__3BA911AAE032474F");

            entity.Property(e => e.TraineeId).HasColumnName("TraineeID");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FitnessLevel).HasMaxLength(10);
            entity.Property(e => e.Gender).HasMaxLength(6);
            entity.Property(e => e.GoalId).HasColumnName("GoalID");
            entity.Property(e => e.Idnumber)
                .HasMaxLength(9)
                .HasColumnName("IDNumber");
            entity.Property(e => e.LoginDateTime).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(10);
            entity.Property(e => e.TraineeHeight).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TraineeName).HasMaxLength(100);
            entity.Property(e => e.TraineeWeight).HasColumnType("decimal(5, 2)");

            entity.HasMany(d => d.Goals).WithMany(p => p.Trainees)
                .UsingEntity<Dictionary<string, object>>(
                    "TraineeGoal",
                    r => r.HasOne<Goal>().WithMany()
                        .HasForeignKey("GoalId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TraineeGoal_Goal"),
                    l => l.HasOne<Trainee>().WithMany()
                        .HasForeignKey("TraineeId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_TraineeGoal_Trainees"),
                    j =>
                    {
                        j.HasKey("TraineeId", "GoalId").HasName("PK__TraineeG__330DEE59BAF6BB03");
                        j.ToTable("TraineeGoal");
                        j.IndexerProperty<int>("TraineeId").HasColumnName("TraineeID");
                        j.IndexerProperty<int>("GoalId").HasColumnName("GoalID");
                    });
        });

        modelBuilder.Entity<TrainingDay>(entity =>
        {
            entity.HasKey(e => e.TrainingDaysId).HasName("PK__Training__3991020463F29237");

            entity.Property(e => e.TrainingDaysId).HasColumnName("TrainingDaysID");
        });

        modelBuilder.Entity<TrainingProgram>(entity =>
        {
            entity.HasKey(e => e.ProgramId).HasName("PK__Training__75256038C36759E1");

            entity.Property(e => e.ProgramId).HasColumnName("ProgramID");
            entity.Property(e => e.CreationDate).HasColumnType("datetime");
            entity.Property(e => e.ProgramName).HasMaxLength(100);
            entity.Property(e => e.TraineeId).HasColumnName("TraineeID");
            entity.Property(e => e.TrainingDateTime).HasColumnType("datetime");

            entity.HasOne(d => d.Trainee).WithMany(p => p.TrainingPrograms)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrainingPrograms_Trainees");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
