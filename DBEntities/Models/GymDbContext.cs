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

    public virtual DbSet<DeviceMuscleEdge> DeviceMuscleEdges { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<Exercise> Exercises { get; set; }

    public virtual DbSet<ExercisePlan> ExercisePlans { get; set; }

    public virtual DbSet<FitnessLevel> FitnessLevels { get; set; }

    public virtual DbSet<Goal> Goals { get; set; }

    public virtual DbSet<GraphEdge> GraphEdges { get; set; }

    public virtual DbSet<Joint> Joints { get; set; }

    public virtual DbSet<Muscle> Muscles { get; set; }

    public virtual DbSet<MuscleEdge> MuscleEdges { get; set; }

    public virtual DbSet<MuscleType> MuscleTypes { get; set; }

    public virtual DbSet<PlanDay> PlanDays { get; set; }

    public virtual DbSet<Size> Sizes { get; set; }

    public virtual DbSet<SubMuscle> SubMuscles { get; set; }

    public virtual DbSet<Trainee> Trainees { get; set; }

    public virtual DbSet<TrainingDay> TrainingDays { get; set; }

    public virtual DbSet<TrainingDuration> TrainingDurations { get; set; }

    public virtual DbSet<TrainingPlan> TrainingPlans { get; set; }

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

        modelBuilder.Entity<DeviceMuscleEdge>(entity =>
        {
            entity.HasKey(e => e.EdgeId).HasName("PK__DeviceMu__DD62104605501A82");

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceMuscleEdges)
                .HasForeignKey(d => d.DeviceId)
                .HasConstraintName("FK_Device");

            entity.HasOne(d => d.Muscle).WithMany(p => p.DeviceMuscleEdges)
                .HasForeignKey(d => d.MuscleId)
                .HasConstraintName("FK_Muscle");
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
            entity.Property(e => e.MuscleGroupId).HasColumnName("MuscleGroupID");
            entity.Property(e => e.MuscleId).HasColumnName("MuscleID");

            entity.HasOne(d => d.MuscleGroup).WithMany(p => p.Exercises)
                .HasForeignKey(d => d.MuscleGroupId)
                .HasConstraintName("FK_Exercises_Size");

            entity.HasOne(d => d.Muscle).WithMany(p => p.Exercises)
                .HasForeignKey(d => d.MuscleId)
                .HasConstraintName("FK_Exercises_Muscles");

            entity.HasOne(d => d.MuscleType).WithMany(p => p.Exercises)
                .HasForeignKey(d => d.MuscleTypeId)
                .HasConstraintName("FK_Exercises_MuscleTypes");

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

        modelBuilder.Entity<ExercisePlan>(entity =>
        {
            entity.HasKey(e => e.ExercisePlanId).HasName("PK__Exercise__8A3C5C39108344DA");

            entity.ToTable("ExercisePlan");

            entity.Property(e => e.ExercisePlanId).HasColumnName("ExercisePlanID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.ExerciseId).HasColumnName("ExerciseID");
            entity.Property(e => e.PlanDayId).HasColumnName("PlanDayID");
            entity.Property(e => e.PlanWeight).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.SubMuscleId).HasColumnName("SubMuscleID");
            entity.Property(e => e.TrainingDateTime).HasColumnType("datetime");

            entity.HasOne(d => d.Exercise).WithMany(p => p.ExercisePlans)
                .HasForeignKey(d => d.ExerciseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExerciseID");

            entity.HasOne(d => d.PlanDay).WithMany(p => p.ExercisePlans)
                .HasForeignKey(d => d.PlanDayId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PlanDayID");
        });

        modelBuilder.Entity<FitnessLevel>(entity =>
        {
            entity.HasKey(e => e.FitnessLevelId).HasName("PK__FitnessL__A30B549EDFB241BA");

            entity.ToTable("FitnessLevel");

            entity.Property(e => e.FitnessLevelId).HasColumnName("FitnessLevelID");
            entity.Property(e => e.Description).HasMaxLength(50);
            entity.Property(e => e.FitnessLevelName).HasMaxLength(10);
        });

        modelBuilder.Entity<Goal>(entity =>
        {
            entity.HasKey(e => e.GoalId).HasName("PK__Goal__8A4FFF3167EB44C3");

            entity.ToTable("Goal");

            entity.Property(e => e.GoalId).HasColumnName("GoalID");
            entity.Property(e => e.GoalName).HasMaxLength(50);
        });

        modelBuilder.Entity<GraphEdge>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__GraphEdg__3214EC277687C829");

            entity.Property(e => e.Id).HasColumnName("ID");

            entity.HasOne(d => d.Device1).WithMany(p => p.GraphEdgeDevice1s)
                .HasForeignKey(d => d.Device1Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExerciseID1");

            entity.HasOne(d => d.Device2).WithMany(p => p.GraphEdgeDevice2s)
                .HasForeignKey(d => d.Device2Id)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ExerciseID2");
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

        modelBuilder.Entity<MuscleEdge>(entity =>
        {
            entity.HasKey(e => e.MuscleEdgeId).HasName("PK__MuscleEd__367B88E60B6DE6BF");

            entity.HasOne(d => d.MuscleId1Navigation).WithMany(p => p.MuscleEdgeMuscleId1Navigations)
                .HasForeignKey(d => d.MuscleId1)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MuscleEdges1");

            entity.HasOne(d => d.MuscleId2Navigation).WithMany(p => p.MuscleEdgeMuscleId2Navigations)
                .HasForeignKey(d => d.MuscleId2)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_MuscleEdges2");
        });

        modelBuilder.Entity<MuscleType>(entity =>
        {
            entity.HasKey(e => e.MuscleTypeId).HasName("PK__MuscleTy__5FE4726626BD5844");

            entity.Property(e => e.MuscleTypeName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<PlanDay>(entity =>
        {
            entity.HasKey(e => e.PlanDayId).HasName("PK__PlanDay__BA415CF8997F3AB5");

            entity.ToTable("PlanDay");

            entity.Property(e => e.PlanDayId).HasColumnName("PlanDayID");
            entity.Property(e => e.CreationDate).HasColumnType("datetime");
            entity.Property(e => e.ProgramName).HasMaxLength(20);
            entity.Property(e => e.TrainingPlanId).HasColumnName("TrainingPlanID");

            entity.HasOne(d => d.TrainingPlan).WithMany(p => p.PlanDays)
                .HasForeignKey(d => d.TrainingPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DefaultProgramID");
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
            entity.Property(e => e.Gender).HasMaxLength(6);
            entity.Property(e => e.Idnumber)
                .HasMaxLength(9)
                .HasColumnName("IDNumber");
            entity.Property(e => e.LoginDateTime).HasColumnType("datetime");
            entity.Property(e => e.Password).HasMaxLength(255);
            entity.Property(e => e.Phone).HasMaxLength(10);
            entity.Property(e => e.TraineeHeight).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TraineeName).HasMaxLength(100);
            entity.Property(e => e.TraineeWeight).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<TrainingDay>(entity =>
        {
            entity.HasKey(e => e.TrainingDaysId).HasName("PK__Training__3991020463F29237");

            entity.Property(e => e.TrainingDaysId).HasColumnName("TrainingDaysID");
        });

        modelBuilder.Entity<TrainingDuration>(entity =>
        {
            entity.HasKey(e => e.TrainingDurationId).HasName("PK__Training__EC863E0E8599230F");

            entity.ToTable("TrainingDuration");

            entity.Property(e => e.TrainingDurationId).HasColumnName("TrainingDurationID");
        });

        modelBuilder.Entity<TrainingPlan>(entity =>
        {
            entity.HasKey(e => e.TrainingPlanId).HasName("PK__Training__D6224AE96CEF725D");

            entity.ToTable("TrainingPlan");

            entity.Property(e => e.TrainingPlanId).HasColumnName("TrainingPlanID");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.FitnessLevelId).HasColumnName("FitnessLevelID");
            entity.Property(e => e.GoalId).HasColumnName("GoalID");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.TraineeId).HasColumnName("TraineeID");
            entity.Property(e => e.TrainingDurationId).HasColumnName("TrainingDurationID");

            entity.HasOne(d => d.FitnessLevel).WithMany(p => p.TrainingPlans)
                .HasForeignKey(d => d.FitnessLevelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_FitnessLevelID");

            entity.HasOne(d => d.Goal).WithMany(p => p.TrainingPlans)
                .HasForeignKey(d => d.GoalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_GoalID");

            entity.HasOne(d => d.Trainee).WithMany(p => p.TrainingPlans)
                .HasForeignKey(d => d.TraineeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TraineeID");

            entity.HasOne(d => d.TrainingDuration).WithMany(p => p.TrainingPlans)
                .HasForeignKey(d => d.TrainingDurationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TrainingDurationID");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
