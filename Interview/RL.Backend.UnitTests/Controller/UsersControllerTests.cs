using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RL.Backend.Controllers;
using RL.Data;
using RL.Data.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RL.Backend.UnitTests.Controller
{
    [TestClass]
    public class UsersControllerTests
    {
        private Mock<ILogger<UsersController>> _loggerMock;
        private RLContext _context;
        private UsersController _controller;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<UsersController>>();
            _context = DbContextHelper.CreateContext();
            _controller = new UsersController(_loggerMock.Object, _context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
        }

        [TestMethod]
        public void Get_WithExistingUsers_ReturnsAllUsers()
        {
            // Given
            var users = new List<User>
            {
                new User { UserId = 1, Name = "User One", CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new User { UserId = 2, Name = "User Two", CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new User { UserId = 3, Name = "User Three", CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };

            _context.Users.AddRange(users);
            _context.SaveChanges();

            // When
            var result = _controller.Get();

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(u => u.UserId == 1 && u.Name == "User One");
            result.Should().Contain(u => u.UserId == 2 && u.Name == "User Two");
            result.Should().Contain(u => u.UserId == 3 && u.Name == "User Three");
        }

        [TestMethod]
        public void Get_WithNoUsers_ReturnsEmptyCollection()
        {
            // Given
            // No users in context

            // When
            var result = _controller.Get();

            // Then
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public void Get_ReturnsQueryableResult()
        {
            // Given
            var users = new List<User>
            {
                new User { UserId = 1, Name = "Test User 1", CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new User { UserId = 2, Name = "Test User 2", CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };

            _context.Users.AddRange(users);
            _context.SaveChanges();

            // When
            var result = _controller.Get();

            // Then
            result.Should().BeAssignableTo<IQueryable<User>>();
            result.Should().HaveCount(2);
        }

        [TestMethod]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            // Given, When & Then
            Assert.ThrowsException<ArgumentNullException>(() =>
                new UsersController(_loggerMock.Object, null));
        }

        [TestMethod]
        public void Get_WithDifferentUserNames_ReturnsCorrectUsers()
        {
            // Given
            var users = new List<User>
            {
                new User { UserId = 1, Name = "Alice Smith", CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new User { UserId = 2, Name = "Bob Johnson", CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new User { UserId = 3, Name = "Charlie Brown", CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };

            _context.Users.AddRange(users);
            _context.SaveChanges();

            // When
            var result = _controller.Get().ToList();

            // Then
            result.Should().HaveCount(3);
            result.Should().Contain(u => u.Name == "Alice Smith");
            result.Should().Contain(u => u.Name == "Bob Johnson");
            result.Should().Contain(u => u.Name == "Charlie Brown");
        }

        [TestMethod]
        public void Get_PreservesUserData_ReturnsCompleteUserObjects()
        {
            // Given
            var createDate = DateTime.UtcNow.AddDays(-1);
            var updateDate = DateTime.UtcNow;
            var user = new User 
            { 
                UserId = 1, 
                Name = "Test User", 
                CreateDate = createDate, 
                UpdateDate = updateDate 
            };

            _context.Users.Add(user);
            _context.SaveChanges();

            // When
            var result = _controller.Get().Single();

            // Then
            result.UserId.Should().Be(1);
            result.Name.Should().Be("Test User");
            result.CreateDate.Should().Be(createDate);
            result.UpdateDate.Should().Be(updateDate);
        }
    }
}