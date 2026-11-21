
using Kid_Guard_part1.Db_context;
using Kid_Guard_part1.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

 namespace Kid_Guard_part1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class Vaccines_Controller : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public Vaccines_Controller(ApplicationDbContext context)
        {
            _context = context;
        }

        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vaccine>>> GetAll()
        {
            return await _context.Vaccines.ToListAsync();
        }

        
        [HttpGet("age/{months}")]
        public async Task<ActionResult<IEnumerable<Vaccine>>> GetByAge(int months)
        {
            var results = await _context.Vaccines
                .Where(v => v.AgeInMonths == months)
                .ToListAsync();

            if (!results.Any())
                return NotFound(new { message = "لا توجد تطعيمات لهذا العمر." });

            return Ok(results);
        }

        
        [HttpPost]
        public async Task<ActionResult<Vaccine>> Create(Vaccine vaccine)
        {
            _context.Vaccines.Add(vaccine);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = vaccine.Id }, vaccine);
        }

       
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Vaccine updatedVaccine)
        {
            if (id != updatedVaccine.Id)
            {
                return BadRequest(new { message = "ID المبعوث غير مطابق." });
            }

            _context.Entry(updatedVaccine).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Vaccines.Any(v => v.Id == id))
                    return NotFound();
                else
                    throw;
            }

            return Ok(new { message = "تم التحديث بنجاح." });
        }

       
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var vaccine = await _context.Vaccines.FindAsync(id);
            if (vaccine == null) return NotFound();

            _context.Vaccines.Remove(vaccine);
            await _context.SaveChangesAsync();

            return Ok(new { message = "تم الحذف بنجاح." });
        }
    }
}