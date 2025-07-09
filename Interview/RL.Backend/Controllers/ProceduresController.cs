using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using RL.Backend.DTO;
using RL.Data.DataModels;
using System.ComponentModel.DataAnnotations;

namespace RL.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class ProceduresController : ControllerBase
{
    private readonly ILogger<ProceduresController> _logger;
    private readonly IMediator _mediator;

    public ProceduresController(ILogger<ProceduresController> logger, IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    [HttpGet]
    [EnableQuery]
    public async Task<IEnumerable<Procedure>> Get()
    {
        return await _mediator.Send(new GetProceduresQuery());
    }

    [HttpGet("Users")]
    public async Task<ActionResult> GetProcedureUsers([FromQuery, Range(1, int.MaxValue)] int procedureId)
    {
        if (procedureId <= 0)
            return BadRequest("ProcedureId must be greater than 0.");

        var users = await _mediator.Send(new GetProcedureUsersQuery { ProcedureId = procedureId });
        return Ok(users);
    }

    /// <summary>
    /// Adds a user to a procedure. Returns 500 on any error.
    /// </summary>
    [HttpPost("Users")]
    public async Task<IActionResult> AddProcedureUser([FromBody] ProcedureUserDTO dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var result = await _mediator.Send(new AddProcedureUserCommand { ProcedureId = dto.ProcedureId, UserId = dto.UserId });
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Soft deletes a user from a procedure by ProcedureUserId. Returns 500 on any error.
    /// </summary>
    [HttpDelete("User")]
    public async Task<IActionResult> DeleteProcedureUser([FromQuery, Range(1, int.MaxValue)] int procedureUserId)
    {
        if (procedureUserId <= 0)
            return BadRequest("ProcedureUserId must be greater than 0.");

        try
        {
            await _mediator.Send(new DeleteProcedureUserCommand { ProcedureUserId = procedureUserId });
            return Ok(new { message = "User deleted from procedure." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Soft deletes all users from a procedure by ProcedureId. Returns 500 on any error.s
    /// </summary>
    [HttpDelete("Users/ByProcedure")]
    public async Task<IActionResult> DeleteAllUsersInProcedure([FromQuery, Range(1, int.MaxValue)] int procedureId)
    {
        if (procedureId <= 0)
            return BadRequest("ProcedureId must be greater than 0.");

        try
        {
            await _mediator.Send(new DeleteAllUsersInProcedureCommand { ProcedureId = procedureId });
            return Ok(new { message = "All users deleted for the specified procedure." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
