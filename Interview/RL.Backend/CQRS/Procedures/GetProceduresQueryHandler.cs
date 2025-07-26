using MediatR;
using RL.Data;
using RL.Data.DataModels;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RL.Backend.CQRS.Procedures
{
    public class GetProceduresQueryHandler : IRequestHandler<GetProceduresQuery, IEnumerable<Procedure>>
    {
        private readonly RLContext _context;

        public GetProceduresQueryHandler(RLContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Procedure>> Handle(GetProceduresQuery request, CancellationToken cancellationToken)
        {
            return await _context.Procedures.ToListAsync(cancellationToken);
        }
    }
}
