using MediatR;
using RL.Data.DataModels;

public class AddProcedureUserCommand : IRequest<ProcedureUser>
{
    public int ProcedureId { get; set; }
    public int UserId { get; set; }
}