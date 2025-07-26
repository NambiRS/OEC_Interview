using MediatR;
using Microsoft.EntityFrameworkCore;
using RL.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RL.Backend.CQRS.Procedures
{
    public class GetProcedureUsersQueryHandler : IRequestHandler<GetProcedureUsersQuery, IEnumerable<object>>
    {
        private readonly RLContext _context;

        public GetProcedureUsersQueryHandler(RLContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<object>> Handle(GetProcedureUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _context.ProcedureUsers
                .Where(pu => pu.ProcedureId == request.ProcedureId
                          && pu.PlanId == request.PlanId
                          && !pu.IsDeleted)
                .Include(pu => pu.User)
                .Include(pu => pu.Procedure)
                .Include(pu => pu.Plan)
                .Select(pu => new
                {
                    ProcedureUserId = pu.ProcedureUserId,
                    ProcedureId = pu.ProcedureId,
                    UserId = pu.UserId,
                    PlanId = pu.PlanId,
                    UserName = pu.User.Name, // Assuming User has UserName property
                    ProcedureTitle = pu.Procedure.ProcedureTitle,
                    CreateDate = pu.CreateDate,
                    UpdateDate = pu.UpdateDate
                })
                .ToListAsync(cancellationToken);

            return users;
        }
    }
}
