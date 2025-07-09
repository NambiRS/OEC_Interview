using MediatR;

public class DeleteProcedureUserCommand : IRequest
{
    public int ProcedureUserId { get; set; }
}