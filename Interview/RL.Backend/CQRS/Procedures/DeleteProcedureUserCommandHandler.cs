using MediatR;
using RL.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

public class DeleteProcedureUserCommandHandler : IRequestHandler<DeleteProcedureUserCommand>
{
    private readonly RLContext _context;

    public DeleteProcedureUserCommandHandler(RLContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(DeleteProcedureUserCommand request, CancellationToken cancellationToken)
    {
        var procedureUser = await _context.ProcedureUsers.FindAsync(new object[] { request.ProcedureUserId }, cancellationToken);
        if (procedureUser == null || procedureUser.IsDeleted)
            throw new Exception("ProcedureUser not found or already deleted.");

        procedureUser.IsDeleted = true;
        procedureUser.UpdateDate = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}