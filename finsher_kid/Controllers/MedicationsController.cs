using Kid_Guard_part1.Db_context;
using Kid_Guard_part1.DTO;
using Kid_Guard_part1.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kid_Guard_part1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicationsController : ControllerBase
    {
        private readonly ApplicationDbContext Db_context;

        public MedicationsController(ApplicationDbContext context)
        {
            Db_context = context;
        }

        private int GetParentId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // --- 1. إضافة دواء جديد ---
        [HttpPost("add")]
        public async Task<IActionResult> AddMedication(CreateMedicationDto dto)
        {
            int parentId = GetParentId();
            var childExists = await Db_context.Children.AnyAsync(c => c.Id == dto.ChildId && c.ParentId == parentId);
            if (!childExists) return Unauthorized("Unauthorized access.");

            using var transaction = await Db_context.Database.BeginTransactionAsync();
            try
            {
                var medication = new Medication
                {
                    MedicationName = dto.MedicationName,
                    Dosage = dto.Dosage,
                    Frequency = dto.Frequency,
                    Description = dto.Description,
                    ChildId = dto.ChildId,
                    IsActive = true,
                    StartDate = DateTime.Now // بداية عداد الدواء
                };
                Db_context.Medications.Add(medication);
                await Db_context.SaveChangesAsync();

                if (dto.Times != null)
                {
                    var schedules = dto.Times.Select(t => new MedicationSchedule
                    {
                        MedicationId = medication.Id,
                        ScheduledTime = t,
                        IsActive = true,
                        IsMonday = dto.IsMonday,
                        IsTuesday = dto.IsTuesday,
                        IsWednesday = dto.IsWednesday,
                        IsThursday = dto.IsThursday,
                        IsFriday = dto.IsFriday,
                        IsSaturday = dto.IsSaturday,
                        IsSunday = dto.IsSunday
                    }).ToList();
                    Db_context.MedicationSchedules.AddRange(schedules);
                }
                await Db_context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { Message = "Added Successfully" });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        // --- 2. تحديث الدواء (مع أرشفة تلقائية للنسخة القديمة) ---
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateMedication(int id, CreateMedicationDto dto)
        {
            int parentId = GetParentId();
            var medication = await Db_context.Medications
                .Include(m => m.Schedules)
                .FirstOrDefaultAsync(m => m.Id == id && m.Child.ParentId == parentId);

            if (medication == null) return NotFound("الدواء غير موجود.");

            // فحص التغيير الجوهري: هل تغير الاسم أو الجرعة أو التكرار؟
            bool isCoreChange = medication.MedicationName != dto.MedicationName ||
                               medication.Dosage != dto.Dosage ||
                               medication.Frequency != dto.Frequency;

            using var transaction = await Db_context.Database.BeginTransactionAsync();
            try
            {
                // الأرشفة التلقائية: إذا مر 24 ساعة وكان التعديل جوهرياً
                if ((DateTime.Now - medication.StartDate).TotalHours >= 24 && isCoreChange)
                {
                    await ArchiveToHistory(medication, "تحديث بيانات (نسخة آلية)");
                    medication.StartDate = DateTime.Now; // إعادة ضبط العداد للجرعة الجديدة
                }

                medication.MedicationName = dto.MedicationName;
                medication.Dosage = dto.Dosage;
                medication.Frequency = dto.Frequency;
                medication.Description = dto.Description;
                medication.IsActive = dto.IsActive;

                Db_context.MedicationSchedules.RemoveRange(medication.Schedules);
                if (dto.Times != null && dto.Times.Any())
                {
                    var newSchedules = dto.Times.Select(t => new MedicationSchedule
                    {
                        MedicationId = id,
                        ScheduledTime = t,
                        IsActive = dto.IsActive,
                        IsMonday = dto.IsMonday,
                        IsTuesday = dto.IsTuesday,
                        IsWednesday = dto.IsWednesday,
                        IsThursday = dto.IsThursday,
                        IsFriday = dto.IsFriday,
                        IsSaturday = dto.IsSaturday,
                        IsSunday = dto.IsSunday
                    }).ToList();
                    Db_context.MedicationSchedules.AddRange(newSchedules);
                }

                await Db_context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(new { Message = "تم التحديث والأرشفة التلقائية بنجاح." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        // --- 3. حذف الدواء (أرشفة تلقائية قبل المسح النهائي) ---
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMedication(int id)
        {
            int parentId = GetParentId();
            var medication = await Db_context.Medications
                .Include(m => m.Schedules)
                .FirstOrDefaultAsync(m => m.Id == id && m.Child.ParentId == parentId);

            if (medication == null) return NotFound("Medication not found.");

            try
            {
                // أرشفة تلقائية إذا كان الدواء استمر لأكثر من يوم
                if ((DateTime.Now - medication.StartDate).TotalHours >= 24)
                {
                    await ArchiveToHistory(medication, "حذف الدواء (نسخة مؤرشفة)");
                }

                Db_context.Medications.Remove(medication);
                await Db_context.SaveChangesAsync();
                return Ok(new { Message = "Deleted and Archived successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // --- محرك الأرشفة التلقائي (هذا الجزء لا يراه اليوزر) ---
        private async Task ArchiveToHistory(Medication med, string action)
        {
            var firstSchedule = med.Schedules.FirstOrDefault();
            var history = new MedicalHistory
            {
                ChildId = med.ChildId,
                MedicationName = med.MedicationName,
                Dosage = med.Dosage,
                Frequency = med.Frequency,
                Description = med.Description,
                StartDate = med.StartDate,
                EndDate = DateTime.Now,
                ActionTaken = action,
                DaysPattern = GetDaysText(firstSchedule),
                TimesPattern = string.Join(", ", med.Schedules.Select(s => s.ScheduledTime.ToString(@"hh\:mm"))),
                ActualUsageDays = CalculateActualDays(med.StartDate, DateTime.Now, firstSchedule)
            };
            Db_context.MedicalHistories.Add(history);
            // سيتم الحفظ النهائي مع SaveChanges الموجودة في الـ Endpoints
        }

        private int CalculateActualDays(DateTime start, DateTime end, MedicationSchedule s)
        {
            if (s == null) return 0;
            int actualDays = 0;
            for (var date = start.Date; date <= end.Date; date = date.AddDays(1))
            {
                bool isPlanned = date.DayOfWeek switch
                {
                    DayOfWeek.Monday => s.IsMonday,
                    DayOfWeek.Tuesday => s.IsTuesday,
                    DayOfWeek.Wednesday => s.IsWednesday,
                    DayOfWeek.Thursday => s.IsThursday,
                    DayOfWeek.Friday => s.IsFriday,
                    DayOfWeek.Saturday => s.IsSaturday,
                    DayOfWeek.Sunday => s.IsSunday,
                    _ => false
                };
                if (isPlanned) actualDays++;
            }
            return actualDays;
        }

        private string GetDaysText(MedicationSchedule s)
        {
            if (s == null) return "";
            var days = new List<string>();
            if (s.IsMonday) days.Add("Mon"); if (s.IsTuesday) days.Add("Tue");
            if (s.IsWednesday) days.Add("Wed"); if (s.IsThursday) days.Add("Thu");
            if (s.IsFriday) days.Add("Fri"); if (s.IsSaturday) days.Add("Sat");
            if (s.IsSunday) days.Add("Sun");
            return string.Join(", ", days);
        }
    }
}