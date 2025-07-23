using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpendSmart_Backend.Data;
using SpendSmart_Backend.DTOs;
using SpendSmart_Backend.Models;

namespace SpendSmart_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SavingRecordsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SavingRecordsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SavingRecords
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SavingRecordDto>>> GetSavingRecords()
        {
            var savingRecords = await _context.SavingRecords
                .Select(sr => new SavingRecordDto
                {
                    Id = sr.Id,
                    Amount = sr.Amount,
                    Date = sr.Date,
                    Time = sr.Time,
                    Description = sr.Description,
                    GoalId = sr.GoalId,
                    UserId = sr.UserId,
                    CreatedAt = DateTime.UtcNow, // You might want to add these fields to your model
                    UpdatedAt = DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(savingRecords);
        }

        // GET: api/SavingRecords/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SavingRecordDto>> GetSavingRecord(int id)
        {
            var savingRecord = await _context.SavingRecords.FindAsync(id);

            if (savingRecord == null)
            {
                return NotFound();
            }

            var dto = new SavingRecordDto
            {
                Id = savingRecord.Id,
                Amount = savingRecord.Amount,
                Date = savingRecord.Date,
                Time = savingRecord.Time,
                Description = savingRecord.Description,
                GoalId = savingRecord.GoalId,
                UserId = savingRecord.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return Ok(dto);
        }

        // GET: api/SavingRecords/goal/5
        [HttpGet("goal/{goalId}")]
        public async Task<ActionResult<IEnumerable<SavingRecordDto>>> GetSavingRecordsByGoalId(int goalId)
        {
            var savingRecords = await _context.SavingRecords
                .Where(sr => sr.GoalId == goalId)
                .OrderByDescending(sr => sr.Date)
                .ThenByDescending(sr => sr.Time)
                .Select(sr => new SavingRecordDto
                {
                    Id = sr.Id,
                    Amount = sr.Amount,
                    Date = sr.Date,
                    Time = sr.Time,
                    Description = sr.Description,
                    GoalId = sr.GoalId,
                    UserId = sr.UserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                })
                .ToListAsync();

            return Ok(savingRecords);
        }

        // POST: api/SavingRecords
        [HttpPost]
        public async Task<ActionResult<SavingRecordDto>> CreateSavingRecord(CreateSavingRecordDto createDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if the goal exists
                var goal = await _context.Goals.FindAsync(createDto.GoalId);
                if (goal == null)
                {
                    return BadRequest("Goal not found");
                }

                // Check if user exists (optional since UserId is nullable)
                if (createDto.UserId.HasValue)
                {
                    var user = await _context.Users.FindAsync(createDto.UserId.Value);
                    if (user == null)
                    {
                        return BadRequest("User not found");
                    }
                }

                // Parse the date and time from the frontend
                var dateTime = createDto.Date;
                var dateOnly = dateTime.Date;
                var timeOnly = dateTime.TimeOfDay;

                var savingRecord = new SavingRecord
                {
                    Amount = createDto.Amount,
                    Date = dateOnly,
                    Time = timeOnly,
                    Description = createDto.Description ?? "",
                    GoalId = createDto.GoalId,
                    UserId = createDto.UserId ?? 1 // Default to user 1 for now
                };

                _context.SavingRecords.Add(savingRecord);

                // Update the goal's current amount
                goal.CurrentAmount += createDto.Amount;
                _context.Goals.Update(goal);

                await _context.SaveChangesAsync();

                var dto = new SavingRecordDto
                {
                    Id = savingRecord.Id,
                    Amount = savingRecord.Amount,
                    Date = savingRecord.Date,
                    Time = savingRecord.Time,
                    Description = savingRecord.Description,
                    GoalId = savingRecord.GoalId,
                    UserId = savingRecord.UserId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                return CreatedAtAction(nameof(GetSavingRecord), new { id = savingRecord.Id }, dto);
            }
            catch (Exception ex)
            {
                // Log the error details for debugging
                Console.WriteLine($"Error creating saving record: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/SavingRecords/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSavingRecord(int id, CreateSavingRecordDto updateDto)
        {
            var savingRecord = await _context.SavingRecords.FindAsync(id);
            if (savingRecord == null)
            {
                return NotFound();
            }

            // Get the old amount to calculate the difference
            var oldAmount = savingRecord.Amount;

            // Parse the date and time from the frontend
            var dateTime = updateDto.Date;
            var dateOnly = dateTime.Date;
            var timeOnly = dateTime.TimeOfDay;

            savingRecord.Amount = updateDto.Amount;
            savingRecord.Date = dateOnly;
            savingRecord.Time = timeOnly;
            savingRecord.Description = updateDto.Description ?? "";

            // Update the goal's current amount
            var goal = await _context.Goals.FindAsync(savingRecord.GoalId);
            if (goal != null)
            {
                goal.CurrentAmount = goal.CurrentAmount - oldAmount + updateDto.Amount;
                _context.Goals.Update(goal);
            }

            _context.Entry(savingRecord).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SavingRecordExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/SavingRecords/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSavingRecord(int id)
        {
            var savingRecord = await _context.SavingRecords.FindAsync(id);
            if (savingRecord == null)
            {
                return NotFound();
            }

            // Update the goal's current amount
            var goal = await _context.Goals.FindAsync(savingRecord.GoalId);
            if (goal != null)
            {
                goal.CurrentAmount -= savingRecord.Amount;
                _context.Goals.Update(goal);
            }

            _context.SavingRecords.Remove(savingRecord);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool SavingRecordExists(int id)
        {
            return _context.SavingRecords.Any(e => e.Id == id);
        }
    }
}