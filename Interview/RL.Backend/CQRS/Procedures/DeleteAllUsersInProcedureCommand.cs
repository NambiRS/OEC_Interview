using MediatR;

public class DeleteAllUsersInProcedureCommand : IRequest
{
    public int ProcedureId { get; set; }
}   