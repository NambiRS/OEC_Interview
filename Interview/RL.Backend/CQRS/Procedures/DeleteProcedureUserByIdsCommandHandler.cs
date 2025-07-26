using MediatR;
using RL.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RL.Backend.CQRS.Procedures
{
    public class DeleteProcedureUserByIdsCommandHandler : IRequestHandler<DeleteProcedureUserByIdsCommand>
    {
        private readonly RLContext _context;

        public DeleteProcedureUserByIdsCommandHandler(RLContext context)
        {
            _context = context;
        }

        public async Task<Unit> Handle(DeleteProcedureUserByIdsCommand request, CancellationToken cancellationToken)
        {
            var procedureUser = _context.ProcedureUsers
                .FirstOrDefault(pu => pu.ProcedureId == request.ProcedureId
                                   && pu.UserId == request.UserId
                                   && pu.PlanId == request.PlanId
                                   && !pu.IsDeleted);

            if (procedureUser == null)
                throw new Exception("ProcedureUser not found or already deleted.");

            procedureUser.IsDeleted = true;
            procedureUser.UpdateDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
