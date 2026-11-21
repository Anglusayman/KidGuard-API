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
    public class ChildrenController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ChildrenController(ApplicationDbContext context)
        {
            _context = context;
        }

        private int GetParentId()
        {
            var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return string.IsNullOrEmpty(claim) ? 0 : int.Parse(claim);
        }

        // --- 1. جلب أطفال الأب المسجل ---
        [HttpGet("my-children")]
        public async Task<IActionResult> GetMyChildren()
        {
            int parentId = GetParentId();
            var children = await _context.Children
                .Where(c => c.ParentId == parentId)
                .Select(c => new { c.Id, c.FullName, c.DateOfBirth, c.Gender })
                .ToListAsync();

            return Ok(children);
        }

        // --- 2. إضافة طفل جديد مع جدول تطعيماته ---
        [HttpPost("add")]
        public async Task<IActionResult> AddChild(CreateChildDto dto)
        {
            int parentId = GetParentId();
            if (parentId == 0) return Unauthorized("جلسة العمل انتهت، يرجى تسجيل الدخول.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var child = new Child
                {
                    FullName = dto.FullName,
                    DateOfBirth = dto.DateOfBirth,
                    Gender = dto.Gender,
                    ParentId = parentId
                };

                _context.Children.Add(child);
                await _context.SaveChangesAsync();

                var vaccines = await _context.Vaccines.ToListAsync();
                if (vaccines.Any())
                {
                    var childVaccines = vaccines.Select(v => new ChildVaccine
                    {
                        ChildId = child.Id,
                        VaccineId = v.Id,
                        IsUsed = false,
                        IsOverdue = false
                    }).ToList();

                    _context.ChildVaccines.AddRange(childVaccines);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return Ok(new { Message = "تمت إضافة الطفل وتجهيز جدول تطعيماته بنجاح.", ChildId = child.Id });
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return BadRequest("حدث خطأ أثناء حفظ البيانات.");
            }
        }

        // --- 3. تحديث بيانات الطفل ---
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateChild(int id, CreateChildDto dto)
        {
            int parentId = GetParentId();
            var child = await _context.Children
                .FirstOrDefaultAsync(c => c.Id == id && c.ParentId == parentId);

            if (child == null) return NotFound("الطفل غير موجود أو لا تملك صلاحية تعديله.");

            child.FullName = dto.FullName;
            child.DateOfBirth = dto.DateOfBirth;
            child.Gender = dto.Gender;

            await _context.SaveChangesAsync();
            return Ok("تم تحديث بيانات الطفل بنجاح.");
        }

        [HttpGet("{childId}/medications")]
        public async Task<IActionResult> GetChildMedications(int childId)
        {
            int parentId = GetParentId();

            var isOwner = await _context.Children
                .AnyAsync(c => c.Id == childId && c.ParentId == parentId);

            if (!isOwner) return Unauthorized("لا تملك صلاحية الوصول لبيانات هذا الطفل.");

            // 1. نجلب البيانات الأساسية من الداتابيز أولاً
            var medicationsData = await _context.Medications
                .Where(m => m.ChildId == childId)
                .Include(m => m.Schedules)
                .ToListAsync();

            // 2. نقوم بعملية الـ Select والفلترة في الـ Memory (Client-side)
            var result = medicationsData.Select(m => new {
                m.Id,
                m.MedicationName,
                m.Dosage,
                m.Frequency,
                m.Description,
                m.StartDate,
                m.ChildId,

                // هنا الفلترة هتشتغل صح لأنها بقت على لستة في الرام مش في الـ SQL
                ActiveDays = m.Schedules.Select(s => new List<string> {
            s.IsMonday ? "Monday" : null,
            s.IsTuesday ? "Tuesday" : null,
            s.IsWednesday ? "Wednesday" : null,
            s.IsThursday ? "Thursday" : null,
            s.IsFriday ? "Friday" : null,
            s.IsSaturday ? "Saturday" : null,
            s.IsSunday ? "Sunday" : null
        }.Where(day => day != null).ToList()).FirstOrDefault(),

                Schedules = m.Schedules.Select(s => new {
                    s.Id,
                    s.ScheduledTime,
                    s.IsActive
                }).ToList()
            }).ToList();

            return Ok(result);
        }

        // --- 5. جلب جدول تطعيمات الطفل ---
        [HttpGet("{childId}/vaccinations")]
        public async Task<IActionResult> GetChildVaccinations(int childId)
        {
            int parentId = GetParentId();

            var child = await _context.Children
                .FirstOrDefaultAsync(c => c.Id == childId && c.ParentId == parentId);

            if (child == null) return Unauthorized("لا تملك صلاحية الوصول لجدول تطعيمات هذا الطفل.");

            var currentAgeInMonths = ((DateTime.Now.Year - child.DateOfBirth.Year) * 12) + DateTime.Now.Month - child.DateOfBirth.Month;

            var vaccinations = await _context.ChildVaccines
                .Where(cv => cv.ChildId == childId)
                .Include(cv => cv.Vaccine)
                .Select(cv => new
                {
                    cv.Id,
                    VaccineName = cv.Vaccine.VaccineNameAr,
                    VaccineNameEn = cv.Vaccine.VaccineNameEn,
                    AgeMonths = cv.Vaccine.AgeInMonths,
                    Description = cv.Vaccine.AgeDescription,
                    cv.IsUsed,
                    IsOverdue = !cv.IsUsed && currentAgeInMonths > cv.Vaccine.AgeInMonths
                })
                .ToListAsync();

            return Ok(vaccinations);
        }

        // --- 6. حذف الطفل ---
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteChild(int id)
        {
            int parentId = GetParentId();
            var child = await _context.Children
                .FirstOrDefaultAsync(c => c.Id == id && c.ParentId == parentId);

            if (child == null)
                return NotFound("لم يتم العثور على الطفل، أو ليس لديك صلاحية لحذفه.");

            _context.Children.Remove(child);
            await _context.SaveChangesAsync();

            return Ok("تم حذف الطفل وجميع سجلاته بنجاح.");
        }
    }
}