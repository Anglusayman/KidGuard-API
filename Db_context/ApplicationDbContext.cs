using Kid_Guard_part1.Model;
using Microsoft.EntityFrameworkCore;

namespace Kid_Guard_part1.Db_context
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // جداول البيانات الأساسية
        public DbSet<Parent> Parents { get; set; }
        public DbSet<Child> Children { get; set; }
        public DbSet<Vaccine> Vaccines { get; set; }
        public DbSet<ChildVaccine> ChildVaccines { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<MedicationSchedule> MedicationSchedules { get; set; }
        public DbSet<GrowthMeasurement> GrowthMeasurements { get; set; }
        public DbSet<MedicalHistory> MedicalHistories { get; set; }

        // جداول معايير النمو (تستخدم نفس الـ Model: GrowthStandard)
        public DbSet<GrowthStandard> boy_grow_check { get; set; }
        public DbSet<GrowthStandard> girls_grow_check { get; set; }
        public DbSet<GrowthStandard> boy_height_check { get; set; }
        public DbSet<GrowthStandard> girls_height_check { get; set; }
        public DbSet<GrowthStandard> boy_height_after_2 { get; set; }
        public DbSet<GrowthStandard> girls_height_after_2 { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. خريطة الأسماء للجداول الأساسية
            modelBuilder.Entity<Parent>().ToTable("Parents");
            modelBuilder.Entity<Child>().ToTable("Children");
            modelBuilder.Entity<Vaccine>().ToTable("Vaccines");
            modelBuilder.Entity<ChildVaccine>().ToTable("ChildVaccines");
            modelBuilder.Entity<Medication>().ToTable("Medications");
            modelBuilder.Entity<MedicationSchedule>().ToTable("MedicationSchedules");
            modelBuilder.Entity<GrowthMeasurement>().ToTable("GrowthMeasurements");

            // 2. معالجة جداول النمو (لضمان فصل البيانات رغم استخدام نفس الـ Model)
            modelBuilder.Entity<GrowthStandard>(entity => {
                entity.HasKey(e => e.Month);
            });

            // ربط كل DbSet بجدوله الفعلي في قاعدة البيانات
            modelBuilder.Entity<GrowthStandard>().ToTable("boy_grow_check");

            // 3. تعريف العلاقات و الـ Cascade Delete لضمان سلامة البيانات

            // علاقة الأب بالأطفال
            modelBuilder.Entity<Child>()
                .HasOne(c => c.Parent)
                .WithMany(p => p.Children)
                .HasForeignKey(c => c.ParentId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة الطفل بالأدوية
            modelBuilder.Entity<Medication>()
                .HasOne(m => m.Child)
                .WithMany(c => c.Medications)
                .HasForeignKey(m => m.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة الدواء بالمواعيد (مهم جداً للـ IsActive)
            // حذف الدواء سيحذف مواعيده، وتحديث حالة IsActive سيتم التعامل معه برمجياً في الكنترولر
            modelBuilder.Entity<MedicationSchedule>()
                .HasOne(ms => ms.Medication)
                .WithMany(m => m.Schedules)
                .HasForeignKey(ms => ms.MedicationId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة الطفل بسجلات النمو
            modelBuilder.Entity<GrowthMeasurement>()
                .HasOne(gm => gm.Child)
                .WithMany(c => c.GrowthMeasurements)
                .HasForeignKey(gm => gm.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة الطفل بالتطعيمات
            modelBuilder.Entity<ChildVaccine>()
                .HasOne(cv => cv.Child)
                .WithMany(c => c.ChildVaccines)
                .HasForeignKey(cv => cv.ChildId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة التطعيم بسجل تطعيم الطفل
            modelBuilder.Entity<ChildVaccine>()
                .HasOne(cv => cv.Vaccine)
                .WithMany()
                .HasForeignKey(cv => cv.VaccineId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}