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
    public class GetProcedureUsersQueryHandlerTests
    {
        [TestMethod]
        public async Task Handle_WithExistingProcedureUsers_ReturnsFilteredUsers()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new GetProcedureUsersQueryHandler(context);

            // Setup required entities
            var procedure = new Procedure
            {
                ProcedureId = 1,
                ProcedureTitle = "Test Procedure",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var user1 = new User
            {
                UserId = 1,
                Name = "User One",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var user2 = new User
            {
                UserId = 2,
                Name = "User Two",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var plan = new Plan
            {
                PlanId = 1,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

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
                }
            };

            context.Procedures.Add(procedure);
            context.Users.AddRange(user1, user2);
            context.Plans.Add(plan);
            context.ProcedureUsers.AddRange(procedureUsers);
            await context.SaveChangesAsync();

            var query = new GetProcedureUsersQuery
            {
                ProcedureId = 1,
                PlanId = 1
            };

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            var resultList = result.ToList();
            var firstUser = resultList[0];
            var secondUser = resultList[1];

            // Verify the anonymous object properties using dynamic typing
            ((dynamic)firstUser).ProcedureUserId.Should().BeOneOf(1, 2);
            ((dynamic)firstUser).ProcedureId.Should().Be(1);
            ((dynamic)firstUser).PlanId.Should().Be(1);
            ((dynamic)firstUser).UserName.Should().BeOneOf("User One", "User Two");
            ((dynamic)firstUser).ProcedureTitle.Should().Be("Test Procedure");

            ((dynamic)secondUser).ProcedureUserId.Should().BeOneOf(1, 2);
            ((dynamic)secondUser).ProcedureId.Should().Be(1);
            ((dynamic)secondUser).PlanId.Should().Be(1);
            ((dynamic)secondUser).UserName.Should().BeOneOf("User One", "User Two");
            ((dynamic)secondUser).ProcedureTitle.Should().Be("Test Procedure");
        }

        [TestMethod]
        public async Task Handle_WithNoProcedureUsers_ReturnsEmptyList()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new GetProcedureUsersQueryHandler(context);

            var query = new GetProcedureUsersQuery
            {
                ProcedureId = 999,
                PlanId = 999
            };

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task Handle_WithDeletedProcedureUsers_ExcludesDeletedUsers()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new GetProcedureUsersQueryHandler(context);

            // Setup required entities
            var procedure = new Procedure
            {
                ProcedureId = 1,
                ProcedureTitle = "Test Procedure",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var user1 = new User
            {
                UserId = 1,
                Name = "Active User",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var user2 = new User
            {
                UserId = 2,
                Name = "Deleted User",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var plan = new Plan
            {
                PlanId = 1,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var procedureUsers = new[]
            {
                new ProcedureUser
                {
                    ProcedureUserId = 1,
                    ProcedureId = 1,
                    UserId = 1,
                    PlanId = 1,
                    IsDeleted = false, // Active user
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                },
                new ProcedureUser
                {
                    ProcedureUserId = 2,
                    ProcedureId = 1,
                    UserId = 2,
                    PlanId = 1,
                    IsDeleted = true, // Deleted user - should be excluded
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                }
            };

            context.Procedures.Add(procedure);
            context.Users.AddRange(user1, user2);
            context.Plans.Add(plan);
            context.ProcedureUsers.AddRange(procedureUsers);
            await context.SaveChangesAsync();

            var query = new GetProcedureUsersQuery
            {
                ProcedureId = 1,
                PlanId = 1
            };

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(1);

            var user = result.Single();
            ((dynamic)user).UserName.Should().Be("Active User");
            ((dynamic)user).UserId.Should().Be(1);
        }

        [TestMethod]
        public async Task Handle_WithDifferentProcedureId_FiltersCorrectly()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new GetProcedureUsersQueryHandler(context);

            // Setup required entities
            var procedure1 = new Procedure
            {
                ProcedureId = 1,
                ProcedureTitle = "Procedure One",
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var procedure2 = new Procedure
            {
                ProcedureId = 2,
                ProcedureTitle = "Procedure Two",
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

            context.Procedures.AddRange(procedure1, procedure2);
            context.Users.Add(user);
            context.Plans.Add(plan);
            context.ProcedureUsers.AddRange(procedureUsers);
            await context.SaveChangesAsync();

            var query = new GetProcedureUsersQuery
            {
                ProcedureId = 1, // Only procedure 1
                PlanId = 1
            };

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(1);

            var user1 = result.Single();
            ((dynamic)user1).ProcedureId.Should().Be(1);
            ((dynamic)user1).ProcedureTitle.Should().Be("Procedure One");
        }

        [TestMethod]
        public async Task Handle_WithDifferentPlanId_FiltersCorrectly()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new GetProcedureUsersQueryHandler(context);

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

            var plan1 = new Plan
            {
                PlanId = 1,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

            var plan2 = new Plan
            {
                PlanId = 2,
                CreateDate = DateTime.UtcNow,
                UpdateDate = DateTime.UtcNow
            };

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
                    UserId = 1,
                    PlanId = 2, // Different plan
                    IsDeleted = false,
                    CreateDate = DateTime.UtcNow,
                    UpdateDate = DateTime.UtcNow
                }
            };

            context.Procedures.Add(procedure);
            context.Users.Add(user);
            context.Plans.AddRange(plan1, plan2);
            context.ProcedureUsers.AddRange(procedureUsers);
            await context.SaveChangesAsync();

            var query = new GetProcedureUsersQuery
            {
                ProcedureId = 1,
                PlanId = 1 // Only plan 1
            };

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(1);

            var user1 = result.Single();
            ((dynamic)user1).PlanId.Should().Be(1);
        }

        [TestMethod]
        public async Task Handle_WithCancellationToken_RespectsToken()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new GetProcedureUsersQueryHandler(context);

            // Setup minimal required entities
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

            context.Procedures.Add(procedure);
            context.Users.Add(user);
            context.Plans.Add(plan);
            context.ProcedureUsers.Add(procedureUser);
            await context.SaveChangesAsync();

            var query = new GetProcedureUsersQuery
            {
                ProcedureId = 1,
                PlanId = 1
            };

            var cancellationToken = new CancellationToken();

            // When
            var result = await handler.Handle(query, cancellationToken);

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [TestMethod]
        public async Task Handle_VerifiesAnonymousObjectStructure()
        {
            // Given
            var context = DbContextHelper.CreateContext();
            var handler = new GetProcedureUsersQueryHandler(context);

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

            var createDate = DateTime.UtcNow.AddDays(-1);
            var updateDate = DateTime.UtcNow;

            var procedureUser = new ProcedureUser
            {
                ProcedureUserId = 1,
                ProcedureId = 1,
                UserId = 1,
                PlanId = 1,
                IsDeleted = false,
                CreateDate = createDate,
                UpdateDate = updateDate
            };

            context.Procedures.Add(procedure);
            context.Users.Add(user);
            context.Plans.Add(plan);
            context.ProcedureUsers.Add(procedureUser);
            await context.SaveChangesAsync();

            var query = new GetProcedureUsersQuery
            {
                ProcedureId = 1,
                PlanId = 1
            };

            // When
            var result = await handler.Handle(query, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(1);

            var user1 = result.Single();

            // Verify all expected properties exist and have correct values
            ((dynamic)user1).ProcedureUserId.Should().Be(1);
            ((dynamic)user1).ProcedureId.Should().Be(1);
            ((dynamic)user1).UserId.Should().Be(1);
            ((dynamic)user1).PlanId.Should().Be(1);
            ((dynamic)user1).UserName.Should().Be("Test User");
            ((dynamic)user1).ProcedureTitle.Should().Be("Test Procedure");
            ((dynamic)user1).CreateDate.Should().Be(createDate);
            ((dynamic)user1).UpdateDate.Should().Be(updateDate);
        }
    }
}