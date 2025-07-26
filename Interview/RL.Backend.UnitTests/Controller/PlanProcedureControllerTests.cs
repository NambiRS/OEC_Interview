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
    public class PlanProcedureControllerTests
    {
        private Mock<ILogger<PlanProcedureController>> _loggerMock;
        private RLContext _context;
        private PlanProcedureController _controller;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<PlanProcedureController>>();
            _context = DbContextHelper.CreateContext();
            _controller = new PlanProcedureController(_loggerMock.Object, _context);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
        }

        [TestMethod]
        public void Get_WithExistingPlanProcedures_ReturnsAllPlanProcedures()
        {
            // Given
            var planProcedures = new List<PlanProcedure>
            {
                new PlanProcedure { PlanId = 1, ProcedureId = 1, CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new PlanProcedure { PlanId = 1, ProcedureId = 2, CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new PlanProcedure { PlanId = 2, ProcedureId = 1, CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };

            _context.PlanProcedures.AddRange(planProcedures);
            _context.SaveChanges();

            // When
            var result = _controller.Get();

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(pp => pp.PlanId == 1 && pp.ProcedureId == 1);
            result.Should().Contain(pp => pp.PlanId == 1 && pp.ProcedureId == 2);
            result.Should().Contain(pp => pp.PlanId == 2 && pp.ProcedureId == 1);
        }

        [TestMethod]
        public void Get_WithNoPlanProcedures_ReturnsEmptyCollection()
        {
            // Given
            // No plan procedures in context

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
            var planProcedures = new List<PlanProcedure>
            {
                new PlanProcedure { PlanId = 1, ProcedureId = 1, CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new PlanProcedure { PlanId = 2, ProcedureId = 1, CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };

            _context.PlanProcedures.AddRange(planProcedures);
            _context.SaveChanges();

            // When
            var result = _controller.Get();

            // Then
            result.Should().BeAssignableTo<IQueryable<PlanProcedure>>();
            result.Should().HaveCount(2);
        }

        [TestMethod]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            // Given, When & Then
            Assert.ThrowsException<ArgumentNullException>(() =>
                new PlanProcedureController(_loggerMock.Object, null));
        }

        [TestMethod]
        public void Get_WithMultiplePlans_ReturnsAllAssociations()
        {
            // Given
            var planProcedures = new List<PlanProcedure>
            {
                new PlanProcedure {  PlanId = 1, ProcedureId = 1, CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new PlanProcedure {  PlanId = 1, ProcedureId = 2, CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new PlanProcedure {  PlanId = 2, ProcedureId = 1, CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new PlanProcedure {  PlanId = 2, ProcedureId = 3, CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };

            _context.PlanProcedures.AddRange(planProcedures);
            _context.SaveChanges();

            // When
            var result = _controller.Get().ToList();

            // Then
            result.Should().HaveCount(4);
            result.Where(pp => pp.PlanId == 1).Should().HaveCount(2);
            result.Where(pp => pp.PlanId == 2).Should().HaveCount(2);
            result.Where(pp => pp.ProcedureId == 1).Should().HaveCount(2);
        }
    }
}