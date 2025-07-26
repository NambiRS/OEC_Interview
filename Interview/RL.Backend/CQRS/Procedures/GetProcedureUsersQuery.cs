using MediatR;
using System.Collections.Generic;

namespace RL.Backend.CQRS.Procedures
{
    public class GetProcedureUsersQuery : IRequest<IEnumerable<object>>
    {
        public int ProcedureId { get; set; }
        public int PlanId { get; set; }
    }
}
