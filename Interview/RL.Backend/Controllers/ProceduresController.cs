using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.EntityFrameworkCore;
using RL.Backend.DTO;
using RL.Data;
using RL.Data.DataModels;

namespace RL.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class ProceduresController : ControllerBase
{
    private readonly ILogger<ProceduresController> _logger;
    private readonly RLContext _context;

    public ProceduresController(ILogger<ProceduresController> logger, RLContext context)
    {
        _logger = logger;
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    [HttpGet]
    [EnableQuery]
    public IEnumerable<Procedure> Get()
    {
        return _context.Procedures;
    }

    [HttpGet("Users")]
    public ActionResult GetProcedureUsers([FromQuery] int procedureId)
    {
        var users = _context.ProcedureUsers
            .Include(pu => pu.User)
            .Where(pu => pu.ProcedureId == procedureId && !pu.IsDeleted)
            .Select(pu => new
            {
                pu.User.UserId,
                pu.User.Name,
                pu.ProcedureId,
                pu.ProcedureUserId
            })
            .ToList();

        return Ok(users);
    }

    /// <summary>
    /// Adds a user to a procedure. Returns 500 on any error.
    /// </summary>
    /// <param name="dto">DTO containing ProcedureId and UserId.</param>
    /// <returns>200 OK with the created ProcedureUser, or 500 with error details.</returns>
    [HttpPost("Users")]
    public async Task<IActionResult> AddProcedureUser([FromBody] ProcedureUserDTO dto)
    {
        try
        {
            if (!_context.Procedures.Any(p => p.ProcedureId == dto.ProcedureId))
                throw new Exception($"ProcedureId {dto.ProcedureId} not found.");
            if (!_context.Users.Any(u => u.UserId == dto.UserId))
                throw new Exception($"UserId {dto.UserId} not found.");

            var exists = _context.ProcedureUsers.Any(pu => pu.ProcedureId == dto.ProcedureId && pu.UserId == dto.UserId && !pu.IsDeleted);
            if (exists)
                throw new Exception("User already assigned to this procedure.");

            var now = DateTime.UtcNow;
            var procedureUser = new ProcedureUser
            {
                ProcedureId = dto.ProcedureId,
                UserId = dto.UserId,
                IsDeleted = false,
                CreateDate = now,
                UpdateDate = now
            };

            _context.ProcedureUsers.Add(procedureUser);
            await _context.SaveChangesAsync();

            return Ok(procedureUser);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Soft deletes a user from a procedure by ProcedureUserId. Returns 500 on any error.
    /// </summary>
    /// <param name="procedureUserId">The ID of the ProcedureUser to delete.</param>
    /// <returns>200 OK with a message on success, or 500 with error details.</returns>
    [HttpDelete("User")]    
    public async Task<IActionResult> DeleteProcedureUser(int procedureUserId)
    {
        try
        {
            var procedureUser = await _context.ProcedureUsers.FindAsync(procedureUserId);
            if (procedureUser == null || procedureUser.IsDeleted)
                throw new Exception("ProcedureUser not found or already deleted.");

            procedureUser.IsDeleted = true;
            procedureUser.UpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted from procedure." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Soft deletes all users from a procedure by ProcedureId. Returns 500 on any error.
    /// </summary>
    /// <param name="procedureId">The ID of the procedure whose users will be deleted.</param>
    /// <returns>200 OK with a message on success, or 500 with error details.</returns>
    [HttpDelete("Users/ByProcedure")]
    public async Task<IActionResult> DeleteAllUsersInProcedure(int procedureId)
    {
        try
        {
            var procedureUsers = _context.ProcedureUsers
                .Where(pu => pu.ProcedureId == procedureId && !pu.IsDeleted)
                .ToList();

            if (!procedureUsers.Any())
                throw new Exception("No users found for the specified procedure.");

            var now = DateTime.UtcNow;
            foreach (var pu in procedureUsers)
            {
                pu.IsDeleted = true;
                pu.UpdateDate = now;
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "All users deleted for the specified procedure." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
