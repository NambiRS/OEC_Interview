using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RL.Backend.CQRS.Procedures;
using RL.Data;
using RL.Data.DataModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RL.Backend.UnitTests.Handlers
{
    [TestClass]
    public class DeleteProcedureUserByIdsCommandHandlerTests
    {
        [TestMethod]
        public async Task Handle_WithValidIds_SoftDeletesProcedureUser()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new DeleteProcedureUserByIdsCommandHandler(context);

            var procedureUser = new ProcedureUser
            {
                ProcedureUserId = 1,
                ProcedureId = 1,
                UserId = 1,
                PlanId = 1,
                IsDeleted = false,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            context.ProcedureUsers.Add(procedureUser);
            await context.SaveChangesAsync();

            var command = new DeleteProcedureUserByIdsCommand
            {
                ProcedureId = 1,
                UserId = 1,
                PlanId = 1
            };

            // When
            var result = await handler.Handle(command, CancellationToken.None);

            // Then
            var updatedProcedureUser = await context.ProcedureUsers.FindAsync(1);
            updatedProcedureUser.Should().NotBeNull();
            updatedProcedureUser.IsDeleted.Should().BeTrue();
            updatedProcedureUser.UpdateDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        }

        [TestMethod]
        public async Task Handle_WithNonExistentProcedureUser_ThrowsException()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new DeleteProcedureUserByIdsCommandHandler(context);

            var command = new DeleteProcedureUserByIdsCommand
            {
                ProcedureId = 999,
                UserId = 999,
                PlanId = 999
            };

            // When & Then
            var exception = await Assert.ThrowsExceptionAsync<Exception>(
                () => handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("ProcedureUser not found or already deleted.");
        }

        [TestMethod]
        public async Task Handle_WithAlreadyDeletedProcedureUser_ThrowsException()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new DeleteProcedureUserByIdsCommandHandler(context);

            var procedureUser = new ProcedureUser
            {
                ProcedureUserId = 1,
                ProcedureId = 1,
                UserId = 1,
                PlanId = 1,
                IsDeleted = true,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            context.ProcedureUsers.Add(procedureUser);
            await context.SaveChangesAsync();

            var command = new DeleteProcedureUserByIdsCommand
            {
                ProcedureId = 1,
                UserId = 1,
                PlanId = 1
            };

            // When & Then
            var exception = await Assert.ThrowsExceptionAsync<Exception>(
                () => handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("ProcedureUser not found or already deleted.");
        }

        [TestMethod]
        public async Task Handle_WithPartialMatchingIds_ThrowsException()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new DeleteProcedureUserByIdsCommandHandler(context);

            var procedureUser = new ProcedureUser
            {
                ProcedureUserId = 1,
                ProcedureId = 1,
                UserId = 1,
                PlanId = 1,
                IsDeleted = false,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            context.ProcedureUsers.Add(procedureUser);
            await context.SaveChangesAsync();

            var command = new DeleteProcedureUserByIdsCommand
            {
                ProcedureId = 1,
                UserId = 2, // Different UserId
                PlanId = 1
            };

            // When & Then
            var exception = await Assert.ThrowsExceptionAsync<Exception>(
                () => handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("ProcedureUser not found or already deleted.");
        }
    }
}