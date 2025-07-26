using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RL.Backend.Commands;
using RL.Backend.Controllers;
using RL.Backend.Models;
using RL.Data;
using RL.Data.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RL.Backend.UnitTests.Controller
{
    [TestClass]
    public class PlanControllerTests
    {
        private Mock<ILogger<PlanController>> _loggerMock;
        private Mock<IMediator> _mediatorMock;
        private RLContext _context;
        private PlanController _controller;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<PlanController>>();
            _mediatorMock = new Mock<IMediator>();
            _context = DbContextHelper.CreateContext();
            _controller = new PlanController(_loggerMock.Object, _context, _mediatorMock.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _context?.Dispose();
        }

        [TestMethod]
        public void Get_WithExistingPlans_ReturnsAllPlans()
        {
            // Given
            var plans = new List<Plan>
            {
                new Plan { PlanId = 1, CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new Plan { PlanId = 2, CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow },
                new Plan { PlanId = 3, CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow }
            };

            _context.Plans.AddRange(plans);
            _context.SaveChanges();

            // When
            var result = _controller.Get();

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
            result.Should().Contain(p => p.PlanId == 1);
            result.Should().Contain(p => p.PlanId == 2);
            result.Should().Contain(p => p.PlanId == 3);
        }

        [TestMethod]
        public void Get_WithNoPlans_ReturnsEmptyCollection()
        {
            // Given
            // No plans in context

            // When
            var result = _controller.Get();

            // Then
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [TestMethod]
        public async Task PostPlan_WithValidCommand_CallsMediatorAndReturnsActionResult()
        {
            // Given
            var command = new CreatePlanCommand();
            var expectedResponse = ApiResponse<Plan>.Succeed(new Plan { PlanId = 1 });
            
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedResponse);

            // When
            var result = await _controller.PostPlan(command, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddProcedureToPlan_WithValidCommand_CallsMediatorAndReturnsActionResult()
        {
            // Given
            var command = new AddProcedureToPlanCommand { PlanId = 1, ProcedureId = 1 };
            var expectedResponse = ApiResponse<Unit>.Succeed(new Unit());
            
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedResponse);

            // When
            var result = await _controller.AddProcedureToPlan(command, CancellationToken.None);

            // Then
            result.Should().NotBeNull();
            _mediatorMock.Verify(m => m.Send(command, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            // Given, When & Then
            Assert.ThrowsException<ArgumentNullException>(() =>
                new PlanController(_loggerMock.Object, null, _mediatorMock.Object));
        }

        [TestMethod]
        public void Constructor_WithNullMediator_ThrowsArgumentNullException()
        {
            // Given, When & Then
            Assert.ThrowsException<ArgumentNullException>(() =>
                new PlanController(_loggerMock.Object, _context, null));
        }

        [TestMethod]
        public async Task PostPlan_WithCancellationToken_PassesTokenToMediator()
        {
            // Given
            var command = new CreatePlanCommand();
            var cancellationToken = new CancellationToken();
            var expectedResponse = ApiResponse<Plan>.Succeed(new Plan { PlanId = 1 });
            
            _mediatorMock.Setup(m => m.Send(command, cancellationToken))
                        .ReturnsAsync(expectedResponse);

            // When
            await _controller.PostPlan(command, cancellationToken);

            // Then
            _mediatorMock.Verify(m => m.Send(command, cancellationToken), Times.Once);
        }

        [TestMethod]
        public async Task AddProcedureToPlan_WithCancellationToken_PassesTokenToMediator()
        {
            // Given
            var command = new AddProcedureToPlanCommand { PlanId = 1, ProcedureId = 1 };
            var cancellationToken = new CancellationToken();
            var expectedResponse = ApiResponse<Unit>.Succeed(new Unit());
            
            _mediatorMock.Setup(m => m.Send(command, cancellationToken))
                        .ReturnsAsync(expectedResponse);

            // When
            await _controller.AddProcedureToPlan(command, cancellationToken);

            // Then
            _mediatorMock.Verify(m => m.Send(command, cancellationToken), Times.Once);
        }

        [TestMethod]
        public async Task PostPlan_WithSuccessfulResponse_ReturnsCorrectActionResult()
        {
            // Given
            var command = new CreatePlanCommand();
            var plan = new Plan { PlanId = 1, CreateDate = DateTime.UtcNow, UpdateDate = DateTime.UtcNow };
            var expectedResponse = ApiResponse<Plan>.Succeed(plan);
            
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedResponse);

            // When
            var result = await _controller.PostPlan(command, CancellationToken.None);

            // Then
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be(plan);
        }

        [TestMethod]
        public async Task PostPlan_WithFailedResponse_ReturnsBadRequest()
        {
            // Given
            var command = new CreatePlanCommand();
            var exception = new Exception("Test exception");
            var expectedResponse = ApiResponse<Plan>.Fail(exception);
            
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedResponse);

            // When
            var result = await _controller.PostPlan(command, CancellationToken.None);

            // Then
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be(exception);
        }

        [TestMethod]
        public async Task AddProcedureToPlan_WithSuccessfulResponse_ReturnsOkResult()
        {
            // Given
            var command = new AddProcedureToPlanCommand { PlanId = 1, ProcedureId = 1 };
            var expectedResponse = ApiResponse<Unit>.Succeed(new Unit());
            
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedResponse);

            // When
            var result = await _controller.AddProcedureToPlan(command, CancellationToken.None);

            // Then
            result.Should().BeOfType<OkResult>();
        }

        [TestMethod]
        public async Task AddProcedureToPlan_WithFailedResponse_ReturnsBadRequest()
        {
            // Given
            var command = new AddProcedureToPlanCommand { PlanId = 1, ProcedureId = 1 };
            var exception = new Exception("Test exception");
            var expectedResponse = ApiResponse<Unit>.Fail(exception);
            
            _mediatorMock.Setup(m => m.Send(command, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedResponse);

            // When
            var result = await _controller.AddProcedureToPlan(command, CancellationToken.None);

            // Then
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be(exception);
        }
    }
}