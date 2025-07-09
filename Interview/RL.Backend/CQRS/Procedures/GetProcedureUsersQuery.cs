using MediatR;
using System.Collections.Generic;

public class GetProcedureUsersQuery : IRequest<IEnumerable<object>>
{
    public int ProcedureId { get; set; }
}