using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RL.Backend.CQRS.Procedures;
using RL.Data;
using RL.Data.DataModels;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RL.Backend.UnitTests.Handlers
{
    [TestClass]
    public class DeleteAllUsersInProcedureCommandHandlerTests
    {
        [TestMethod]
        public async Task Handle_WithValidIds_SoftDeletesAllProcedureUsers()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new DeleteAllUsersInProcedureCommandHandler(context);

            var procedureUsers = new[]
            {
                new ProcedureUser
                {
                    ProcedureUserId = 1,
                    ProcedureId = 1,
                    UserId = 1,
                    PlanId = 1,
                    IsDeleted = false,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                },
                new ProcedureUser
                {
                    ProcedureUserId = 2,
                    ProcedureId = 1,
                    UserId = 2,
                    PlanId = 1,
                    IsDeleted = false,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                },
                new ProcedureUser
                {
                    ProcedureUserId = 3,
                    ProcedureId = 1,
                    UserId = 3,
                    PlanId = 1,
                    IsDeleted = false,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                }
            };

            context.ProcedureUsers.AddRange(procedureUsers);
            await context.SaveChangesAsync();

            var command = new DeleteAllUsersInProcedureCommand
            {
                ProcedureId = 1,
                PlanId = 1
            };

            // When
            var result = await handler.Handle(command, CancellationToken.None);

            // Then
            var updatedUsers = context.ProcedureUsers
                .Where(pu => pu.ProcedureId == 1 && pu.PlanId == 1)
                .ToList();

            updatedUsers.Should().HaveCount(3);
            updatedUsers.Should().OnlyContain(pu => pu.IsDeleted == true);
            updatedUsers.Should().OnlyContain(pu => pu.UpdateDate <= DateTime.UtcNow && pu.UpdateDate >= DateTime.UtcNow.AddSeconds(-5));
        }

        [TestMethod]
        public async Task Handle_WithNoProcedureUsers_ThrowsException()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new DeleteAllUsersInProcedureCommandHandler(context);

            var command = new DeleteAllUsersInProcedureCommand
            {
                ProcedureId = 999,
                PlanId = 999
            };

            // When & Then
            var exception = await Assert.ThrowsExceptionAsync<Exception>(
                () => handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("No users found for the specified procedure and plan.");
        }

        [TestMethod]
        public async Task Handle_WithAllUsersAlreadyDeleted_ThrowsException()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new DeleteAllUsersInProcedureCommandHandler(context);

            var procedureUsers = new[]
            {
                new ProcedureUser
                {
                    ProcedureUserId = 1,
                    ProcedureId = 1,
                    UserId = 1,
                    PlanId = 1,
                    IsDeleted = true,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                },
                new ProcedureUser
                {
                    ProcedureUserId = 2,
                    ProcedureId = 1,
                    UserId = 2,
                    PlanId = 1,
                    IsDeleted = true,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                }
            };

            context.ProcedureUsers.AddRange(procedureUsers);
            await context.SaveChangesAsync();

            var command = new DeleteAllUsersInProcedureCommand
            {
                ProcedureId = 1,
                PlanId = 1
            };

            // When & Then
            var exception = await Assert.ThrowsExceptionAsync<Exception>(
                () => handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("No users found for the specified procedure and plan.");
        }

        [TestMethod]
        public async Task Handle_WithMixedDeletedAndActiveUsers_DeletesOnlyActiveUsers()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new DeleteAllUsersInProcedureCommandHandler(context);

            var procedureUsers = new[]
            {
                new ProcedureUser
                {
                    ProcedureUserId = 1,
                    ProcedureId = 1,
                    UserId = 1,
                    PlanId = 1,
                    IsDeleted = false,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                },
                new ProcedureUser
                {
                    ProcedureUserId = 2,
                    ProcedureId = 1,
                    UserId = 2,
                    PlanId = 1,
                    IsDeleted = true, // Already deleted
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                },
                new ProcedureUser
                {
                    ProcedureUserId = 3,
                    ProcedureId = 1,
                    UserId = 3,
                    PlanId = 1,
                    IsDeleted = false,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                }
            };

            context.ProcedureUsers.AddRange(procedureUsers);
            await context.SaveChangesAsync();

            var command = new DeleteAllUsersInProcedureCommand
            {
                ProcedureId = 1,
                PlanId = 1
            };

            // When
            var result = await handler.Handle(command, CancellationToken.None);

            // Then
            var allUsers = context.ProcedureUsers
                .Where(pu => pu.ProcedureId == 1 && pu.PlanId == 1)
                .ToList();

            allUsers.Should().HaveCount(3);
            allUsers.Should().OnlyContain(pu => pu.IsDeleted == true);
        }

        [TestMethod]
        public async Task Handle_WithDifferentProcedureId_DoesNotAffectOtherProcedures()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new DeleteAllUsersInProcedureCommandHandler(context);

            var procedureUsers = new[]
            {
                new ProcedureUser
                {
                    ProcedureUserId = 1,
                    ProcedureId = 1,
                    UserId = 1,
                    PlanId = 1,
                    IsDeleted = false,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                },
                new ProcedureUser
                {
                    ProcedureUserId = 2,
                    ProcedureId = 2, // Different procedure
                    UserId = 1,
                    PlanId = 1,
                    IsDeleted = false,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                }
            };

            context.ProcedureUsers.AddRange(procedureUsers);
            await context.SaveChangesAsync();

            var command = new DeleteAllUsersInProcedureCommand
            {
                ProcedureId = 1,
                PlanId = 1
            };

            // When
            var result = await handler.Handle(command, CancellationToken.None);

            // Then
            var procedure1Users = context.ProcedureUsers
                .Where(pu => pu.ProcedureId == 1 && pu.PlanId == 1)
                .ToList();
            var procedure2Users = context.ProcedureUsers
                .Where(pu => pu.ProcedureId == 2 && pu.PlanId == 1)
                .ToList();

            procedure1Users.Should().HaveCount(1);
            procedure1Users.Should().OnlyContain(pu => pu.IsDeleted == true);

            procedure2Users.Should().HaveCount(1);
            procedure2Users.Should().OnlyContain(pu => pu.IsDeleted == false);
        }
    }
}