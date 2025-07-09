using MediatR;
using RL.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class DeleteAllUsersInProcedureCommandHandler : IRequestHandler<DeleteAllUsersInProcedureCommand>
{
    private readonly RLContext _context;

    public DeleteAllUsersInProcedureCommandHandler(RLContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteAllUsersInProcedureCommand request, CancellationToken cancellationToken)
    {
        var procedureUsers = _context.ProcedureUsers
            .Where(pu => pu.ProcedureId == request.ProcedureId && !pu.IsDeleted)
            .ToList();

        if (!procedureUsers.Any())
            throw new Exception("No users found for the specified procedure.");

        var now = DateTime.UtcNow;
        foreach (var pu in procedureUsers)
        {
            pu.IsDeleted = true;
            pu.UpdateDate = now;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}   