using MediatR;
using RL.Data.DataModels;
using System.Collections.Generic;

public class GetProceduresQuery : IRequest<IEnumerable<Procedure>>
{
}