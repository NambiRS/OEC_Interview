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
    public class AddProcedureUserCommandHandlerTests
    {
        [TestMethod]
        public async Task Handle_WithValidData_CreatesProcedureUser()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new AddProcedureUserCommandHandler(context);

            // Setup required entities
            var procedure = new Procedure 
            { 
                ProcedureId = 1, 
                ProcedureTitle = "Test Procedure",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            var user = new User 
            { 
                UserId = 1, 
                Name = "Test User",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            var plan = new Plan 
            { 
                PlanId = 1,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            context.Procedures.Add(procedure);
            context.Users.Add(user);
            context.Plans.Add(plan);
            await context.SaveChangesAsync();

            var command = new AddProcedureUserCommand
            {
                ProcedureId = 1,
                UserId = 1,
                PlanId = 1
            };

            // When
            var result = await handler.Handle(command, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.ProcedureId.Should().Be(1);
            result.UserId.Should().Be(1);
            result.PlanId.Should().Be(1);
            result.IsDeleted.Should().BeFalse();
            result.CreateDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            result.UpdateDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

            var savedEntity = await context.ProcedureUsers.FindAsync(result.ProcedureUserId);
            savedEntity.Should().NotBeNull();
            savedEntity.Should().BeEquivalentTo(result);
        }

        [TestMethod]
        public async Task Handle_WithNonExistentProcedure_ThrowsException()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new AddProcedureUserCommandHandler(context);

            // Setup required entities except procedure
            var user = new User 
            { 
                UserId = 1, 
                Name = "Test User",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            var plan = new Plan 
            { 
                PlanId = 1,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            context.Users.Add(user);
            context.Plans.Add(plan);
            await context.SaveChangesAsync();

            var command = new AddProcedureUserCommand
            {
                ProcedureId = 999, // Non-existent
                UserId = 1,
                PlanId = 1
            };

            // When & Then
            var exception = await Assert.ThrowsExceptionAsync<Exception>(
                () => handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("ProcedureId 999 not found.");
        }

        [TestMethod]
        public async Task Handle_WithNonExistentUser_ThrowsException()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new AddProcedureUserCommandHandler(context);

            // Setup required entities except user
            var procedure = new Procedure 
            { 
                ProcedureId = 1, 
                ProcedureTitle = "Test Procedure",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            var plan = new Plan 
            { 
                PlanId = 1,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            context.Procedures.Add(procedure);
            context.Plans.Add(plan);
            await context.SaveChangesAsync();

            var command = new AddProcedureUserCommand
            {
                ProcedureId = 1,
                UserId = 999, // Non-existent
                PlanId = 1
            };

            // When & Then
            var exception = await Assert.ThrowsExceptionAsync<Exception>(
                () => handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("UserId 999 not found.");
        }

        [TestMethod]
        public async Task Handle_WithNonExistentPlan_ThrowsException()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new AddProcedureUserCommandHandler(context);

            // Setup required entities except plan
            var procedure = new Procedure 
            { 
                ProcedureId = 1, 
                ProcedureTitle = "Test Procedure",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            var user = new User 
            { 
                UserId = 1, 
                Name = "Test User",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            context.Procedures.Add(procedure);
            context.Users.Add(user);
            await context.SaveChangesAsync();

            var command = new AddProcedureUserCommand
            {
                ProcedureId = 1,
                UserId = 1,
                PlanId = 999 // Non-existent
            };

            // When & Then
            var exception = await Assert.ThrowsExceptionAsync<Exception>(
                () => handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("PlanId 999 not found.");
        }

        [TestMethod]
        public async Task Handle_WithExistingAssignment_ThrowsException()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new AddProcedureUserCommandHandler(context);

            // Setup required entities
            var procedure = new Procedure 
            { 
                ProcedureId = 1, 
                ProcedureTitle = "Test Procedure",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            var user = new User 
            { 
                UserId = 1, 
                Name = "Test User",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            var plan = new Plan 
            { 
                PlanId = 1,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            context.Procedures.Add(procedure);
            context.Users.Add(user);
            context.Plans.Add(plan);

            // Add existing assignment
            var existingAssignment = new ProcedureUser
            {
                ProcedureId = 1,
                UserId = 1,
                PlanId = 1,
                IsDeleted = false,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.ProcedureUsers.Add(existingAssignment);
            await context.SaveChangesAsync();

            var command = new AddProcedureUserCommand
            {
                ProcedureId = 1,
                UserId = 1,
                PlanId = 1
            };

            // When & Then
            var exception = await Assert.ThrowsExceptionAsync<Exception>(
                () => handler.Handle(command, CancellationToken.None));

            exception.Message.Should().Be("User already assigned to this procedure.");
        }

        [TestMethod]
        public async Task Handle_WithDeletedExistingAssignment_CreatesNewAssignment()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new AddProcedureUserCommandHandler(context);

            // Setup required entities
            var procedure = new Procedure 
            { 
                ProcedureId = 1, 
                ProcedureTitle = "Test Procedure",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            var user = new User 
            { 
                UserId = 1, 
                Name = "Test User",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            var plan = new Plan 
            { 
                PlanId = 1,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            context.Procedures.Add(procedure);
            context.Users.Add(user);
            context.Plans.Add(plan);

            // Add deleted assignment
            var deletedAssignment = new ProcedureUser
            {
                ProcedureUserId = 1,
                ProcedureId = 1,
                UserId = 1,
                PlanId = 1,
                IsDeleted = true,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            context.ProcedureUsers.Add(deletedAssignment);
            await context.SaveChangesAsync();

            var command = new AddProcedureUserCommand
            {
                ProcedureId = 1,
                UserId = 1,
                PlanId = 1
            };

            // When
            var result = await handler.Handle(command, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.ProcedureId.Should().Be(1);
            result.UserId.Should().Be(1);
            result.PlanId.Should().Be(1);
            result.IsDeleted.Should().BeFalse();

            // Should have 2 records: one deleted, one active
            var allAssignments = context.ProcedureUsers
                .Where(pu => pu.ProcedureId == 1 && pu.UserId == 1 && pu.PlanId == 1)
                .ToList();

            allAssignments.Should().HaveCount(2);
            allAssignments.Should().Contain(pu => pu.IsDeleted == true);
            allAssignments.Should().Contain(pu => pu.IsDeleted == false);
        }

        [TestMethod]
        public async Task Handle_WithValidData_SetsCorrectTimestamps()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new AddProcedureUserCommandHandler(context);

            // Setup required entities
            var procedure = new Procedure 
            { 
                ProcedureId = 1, 
                ProcedureTitle = "Test Procedure",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            var user = new User 
            { 
                UserId = 1, 
                Name = "Test User",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };
            var plan = new Plan 
            { 
                PlanId = 1,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            context.Procedures.Add(procedure);
            context.Users.Add(user);
            context.Plans.Add(plan);
            await context.SaveChangesAsync();

            var beforeExecution = DateTime.UtcNow;

            var command = new AddProcedureUserCommand
            {
                ProcedureId = 1,
                UserId = 1,
                PlanId = 1
            };

            // When
            var result = await handler.Handle(command, CancellationToken.None);

            // Then
            var afterExecution = DateTime.UtcNow;

            result.CreateDate.Should().BeOnOrAfter(beforeExecution);
            result.CreateDate.Should().BeOnOrBefore(afterExecution);
            result.UpdateDate.Should().BeOnOrAfter(beforeExecution);
            result.UpdateDate.Should().BeOnOrBefore(afterExecution);
            result.CreateDate.Should().Be(result.UpdateDate);
        }
    }
}