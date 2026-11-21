using Kid_Guard_part1.Db_context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kid_Guard_part1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MedicalHistoryController : ControllerBase
    {
        private readonly ApplicationDbContext Db_context;

        public MedicalHistoryController(ApplicationDbContext context)
        {
            Db_context = context;
        }

        // دالة مساعدة لجلب الـ ParentId من التوكن
        private int GetParentId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // --- عرض سجل الأرشيف للطفل ---
        [HttpGet("child-archive/{childId}")]
        public async Task<IActionResult> GetChildArchive(int childId)
        {
            int parentId = GetParentId();

            // التأكد إن الطفل ده يخص الأب اللي باعت الطلب
            var isChildOwnedByParent = await Db_context.Children
                .AnyAsync(c => c.Id == childId && c.ParentId == parentId);

            if (!isChildOwnedByParent)
                return Unauthorized("لا تملك صلاحية الوصول لبيانات هذا الطفل.");

            // جلب الأرشيف (مرتب من الأحدث للقديم)
            var archive = await Db_context.MedicalHistories
                .Where(h => h.ChildId == childId)
                .OrderByDescending(h => h.EndDate)
                .Select(h => new {
                    h.MedicationName,
                    h.Dosage,
                    h.Frequency,
                    h.Description,
                    // تنسيق التاريخ لشكل بسيط يسهل قراءته في الموبايل
                    StartDate = h.StartDate.ToString("yyyy-MM-dd"),
                    EndDate = h.EndDate.ToString("yyyy-MM-dd"),
                    h.ActualUsageDays,
                    h.DaysPattern,
                    h.TimesPattern,
                    h.ActionTaken // عشان الأب يعرف السجل ده اتعمل ليه (حذف ولا تعديل)
                })
                .ToListAsync();

            if (archive == null || !archive.Any())
                return NotFound("لا يوجد سجل أدوية سابق لهذا الطفل.");

            return Ok(archive);
        }
    }
}