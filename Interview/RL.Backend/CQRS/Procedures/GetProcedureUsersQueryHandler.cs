using MediatR;
using RL.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class GetProcedureUsersQueryHandler : IRequestHandler<GetProcedureUsersQuery, IEnumerable<object>>
{
    private readonly RLContext _context;

    public GetProcedureUsersQueryHandler(RLContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<object>> Handle(GetProcedureUsersQuery request, CancellationToken cancellationToken)
    {
        return await _context.ProcedureUsers
            .Include(pu => pu.User)
            .Where(pu => pu.ProcedureId == request.ProcedureId && !pu.IsDeleted)
            .Select(pu => new
            {
                pu.User.UserId,
                pu.User.Name,
                pu.ProcedureId,
                pu.ProcedureUserId
            })
            .ToListAsync(cancellationToken);
    }
}