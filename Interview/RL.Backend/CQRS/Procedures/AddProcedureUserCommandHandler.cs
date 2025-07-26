using MediatR;
using RL.Data;
using RL.Data.DataModels;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RL.Backend.CQRS.Procedures
{
    public class AddProcedureUserCommandHandler : IRequestHandler<AddProcedureUserCommand, ProcedureUser>
    {
        private readonly RLContext _context;

        public AddProcedureUserCommandHandler(RLContext context)
        {
            _context = context;
        }

        public async Task<ProcedureUser> Handle(AddProcedureUserCommand request, CancellationToken cancellationToken)
        {
            if (!_context.Procedures.Any(p => p.ProcedureId == request.ProcedureId))
                throw new Exception($"ProcedureId {request.ProcedureId} not found.");
            if (!_context.Users.Any(u => u.UserId == request.UserId))
                throw new Exception($"UserId {request.UserId} not found.");
            if (!_context.Plans.Any(p => p.PlanId == request.PlanId))
                throw new Exception($"PlanId {request.PlanId} not found.");

            var exists = _context.ProcedureUsers.Any(pu => pu.PlanId == request.PlanId && pu.ProcedureId == request.ProcedureId && pu.UserId == request.UserId && !pu.IsDeleted);
            if (exists)
                throw new Exception("User already assigned to this procedure.");

            var now = DateTime.UtcNow;
            var procedureUser = new ProcedureUser
            {
                ProcedureId = request.ProcedureId,
                UserId = request.UserId,
                PlanId = request.PlanId,
                IsDeleted = false,
                CreateDate = now,
                UpdateDate = now
            };

            _context.ProcedureUsers.Add(procedureUser);
            await _context.SaveChangesAsync(cancellationToken);

            return procedureUser;
        }
    }
}
