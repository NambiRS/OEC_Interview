using MediatR;
using RL.Data.DataModels;
using System.Collections.Generic;

namespace RL.Backend.CQRS.Procedures
{
    public class GetProceduresQuery : IRequest<IEnumerable<Procedure>>
    {
    }
}
