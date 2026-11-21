using Kid_Guard_part1.Db_context;
using Kid_Guard_part1.DTO;
using Kid_Guard_part1.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Kid_Guard_part1.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class GrowthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GrowthController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        [HttpPost("{childId}/add-measurement")]
        public async Task<IActionResult> AddMeasurement(int childId, [FromBody] GrowthCheckDto dto)
        {
            
            if (!await IsChildOwnedByParent(childId))
            {
                return Forbid(); 
            }

            var child = await _context.Children.FindAsync(childId);
            if (child == null) return NotFound("الطفل غير موجود.");

       
            string gender = NormalizeGender(child.Gender);
            int ageInMonths = CalculateAgeInMonths(child.DateOfBirth);

            if (ageInMonths > 60) return BadRequest("النظام يدعم المتابعة حتى سن 5 سنوات فقط.");

            
            var weightRow = gender == "male"
                ? await _context.boy_grow_check.FirstOrDefaultAsync(x => x.Month == ageInMonths)
                : await _context.girls_grow_check.FirstOrDefaultAsync(x => x.Month == ageInMonths);

            GrowthStandard heightRow;
            if (ageInMonths < 27)
            {
                heightRow = gender == "male"
                    ? await _context.boy_height_check.FirstOrDefaultAsync(x => x.Month == ageInMonths)
                    : await _context.girls_height_check.FirstOrDefaultAsync(x => x.Month == ageInMonths);
            }
            else
            {
                heightRow = gender == "male"
                    ? await _context.boy_height_after_2.FirstOrDefaultAsync(x => x.Month == ageInMonths)
                    : await _context.girls_height_after_2.FirstOrDefaultAsync(x => x.Month == ageInMonths);
            }

            if (weightRow == null || heightRow == null)
                return BadRequest("المعايير الطبية لهذا السن غير متوفرة.");

            
            string wStatus = Analyze(dto.Weight, weightRow, "وزن");
            string hStatus = Analyze(dto.Height, heightRow, "طول");

            
            var measurement = new GrowthMeasurement
            {
                ChildId = childId,
                Weight = dto.Weight,
                Height = dto.Height,
                AgeInMonths = ageInMonths,
                WeightStatus = wStatus,
                HeightStatus = hStatus,
                CreatedAt = DateTime.Now
            };

            _context.GrowthMeasurements.Add(measurement);
            await _context.SaveChangesAsync();

            return Ok(new GrowthResponseDto
            {
                Id = measurement.Id,
                ChildName = child.FullName,
                AgeInMonths = ageInMonths,
                Weight = measurement.Weight,
                Height = measurement.Height,
                WeightStatus = wStatus,
                HeightStatus = hStatus,
                CreatedAt = measurement.CreatedAt.ToString("yyyy-MM-dd HH:mm")
            });
        }

       
        [HttpGet("{childId}/history")]
        public async Task<IActionResult> GetGrowthHistory(int childId)
        {
            if (!await IsChildOwnedByParent(childId))
            {
                return Forbid();
            }

            var history = await _context.GrowthMeasurements
                .Where(m => m.ChildId == childId)
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new GrowthResponseDto
                {
                    Id = m.Id,
                    AgeInMonths = m.AgeInMonths,
                    Weight = m.Weight,
                    Height = m.Height,
                    WeightStatus = m.WeightStatus,
                    HeightStatus = m.HeightStatus,
                    CreatedAt = m.CreatedAt.ToString("yyyy-MM-dd HH:mm")
                })
                .ToListAsync();

            if (!history.Any()) return NotFound("لا توجد سجلات سابقة.");

            return Ok(history);
        }

       
        private async Task<bool> IsChildOwnedByParent(int childId)
        {
          
            var parentIdFromToken = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(parentIdFromToken)) return false;

          
            return await _context.Children.AnyAsync(c => c.Id == childId && c.ParentId.ToString() == parentIdFromToken);
        }

      
        private string NormalizeGender(string gender)
        {
            if (string.IsNullOrEmpty(gender)) return "male";
            gender = gender.ToLower().Trim();
            if (gender == "ذكر" || gender == "ولد" || gender == "male") return "male";
            if (gender == "أنثى" || gender == "بنت" || gender == "female") return "female";
            return "male";
        }

        private int CalculateAgeInMonths(DateTime birthDate)
        {
            var today = DateTime.Today;
            int months = (today.Year - birthDate.Year) * 12 + today.Month - birthDate.Month;
            if (today.Day < birthDate.Day) months--;
            return months;
        }

        private string Analyze(double val, GrowthStandard s, string type)
        {
            if (val < s.SD2neg) return type == "وزن" ? "نحافة ملحوظة" : "قصر قامة ملحوظ";
            if (val > s.SD2) return type == "وزن" ? "زيادة وزن ملحوظة" : "طول زائد";
            return "طبيعي ومستقر";
        }
    }
}