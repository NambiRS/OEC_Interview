using MediatR;

namespace RL.Backend.CQRS.Procedures
{
    public class DeleteProcedureUserByIdsCommand : IRequest
    {
        public int ProcedureId { get; set; }
        public int UserId { get; set; }
        public int PlanId { get; set; }
    }
}
