using MediatR;
using RL.Data.DataModels;

namespace RL.Backend.CQRS.Procedures
{
    public class AddProcedureUserCommand : IRequest<ProcedureUser>
    {
        public int ProcedureId { get; set; }
        public int UserId { get; set; }
        public int PlanId { get; set; }
    }
}
