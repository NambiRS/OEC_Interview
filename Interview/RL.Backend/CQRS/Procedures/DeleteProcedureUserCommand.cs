using MediatR;

namespace RL.Backend.CQRS.Procedures
{
    public class DeleteProcedureUserCommand : IRequest
    {
        public int ProcedureUserId { get; set; }
    }
}
