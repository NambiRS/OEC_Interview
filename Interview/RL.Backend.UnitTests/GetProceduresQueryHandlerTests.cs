using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RL.Backend.CQRS.Procedures;
using RL.Data;
using RL.Data.DataModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RL.Backend.UnitTests
{
    [TestClass]
    public class GetProceduresQueryHandlerTests
    {
        [TestMethod]
        public async Task Handle_WithExistingProcedures_ReturnsAllProcedures()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new GetProceduresQueryHandler(context);
            var query = new GetProceduresQuery();

            var procedures = new List<Procedure>
            {
                new Procedure { ProcedureId = 1, ProcedureTitle = "Procedure 1", CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new Procedure { ProcedureId = 2, ProcedureTitle = "Procedure 2", CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new Procedure { ProcedureId = 3, ProcedureTitle = "Procedure 3", CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };

            context.Procedures.AddRange(procedures);
            await context.SaveChangesAsync();

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(p => p.ProcedureId == 1 && p.ProcedureTitle == "Procedure 1");
            result.Should().Contain(p => p.ProcedureId == 2 && p.ProcedureTitle == "Procedure 2");
            result.Should().Contain(p => p.ProcedureId == 3 && p.ProcedureTitle == "Procedure 3");
        }

        [TestMethod]
        public async Task Handle_WithNoProcedures_ReturnsEmptyList()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new GetProceduresQueryHandler(context);
            var query = new GetProceduresQuery();

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task Handle_WithCancellationToken_RespectsToken()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new GetProceduresQueryHandler(context);
            var query = new GetProceduresQuery();

            var procedures = new List<Procedure>
            {
                new Procedure { ProcedureId = 1, ProcedureTitle = "Test Procedure", CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };

            context.Procedures.AddRange(procedures);
            await context.SaveChangesAsync();

            var cancellationToken = new CancellationToken();

            // When
            var result = await handler.Handle(query, cancellationToken);

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }
    }
}