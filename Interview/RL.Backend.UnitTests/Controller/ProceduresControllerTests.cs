using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using RL.Backend.Controllers;
using RL.Backend.CQRS.Procedures;
using RL.Backend.DTO;
using RL.Data.DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RL.Backend.UnitTests.Controller
{
    [TestClass]
    public class ProceduresControllerTests
    {
        private Mock<ILogger<ProceduresController>> _loggerMock;
        private Mock<IMediator> _mediatorMock;
        private ProceduresController _controller;

        [TestInitialize]
        public void Setup()
        {
            _loggerMock = new Mock<ILogger<ProceduresController>>();
            _mediatorMock = new Mock<IMediator>();
            _controller = new ProceduresController(_loggerMock.Object, _mediatorMock.Object);
        }

        [TestMethod]
        public async Task Get_CallsMediatorWithGetProceduresQuery()
        {
            // Given
            var expectedProcedures = new List<Procedure>
            {
                new Procedure { ProcedureId = 1, ProcedureTitle = "Test Procedure" }
            };

            _mediatorMock.Setup(m => m.Send(It.IsAny<GetProceduresQuery>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedProcedures);

            // When
            var result = await _controller.Get();

            // Then
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetProceduresQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetProcedureUsers_WithValidParameters_ReturnsOkResult()
        {
            // Given
            var procedureId = 1;
            var planId = 1;
            var expectedUsers = new List<object> { new { UserId = 1, UserName = "Test User" } };

            _mediatorMock.Setup(m => m.Send(It.Is<GetProcedureUsersQuery>(q => q.ProcedureId == procedureId && q.PlanId == planId), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedUsers);

            // When
            var result = await _controller.GetProcedureUsers(procedureId, planId);

            // Then
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be(expectedUsers);
            _mediatorMock.Verify(m => m.Send(It.IsAny<GetProcedureUsersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task GetProcedureUsers_WithInvalidProcedureId_ReturnsBadRequest()
        {
            // Given
            var procedureId = 0;
            var planId = 1;

            // When
            var result = await _controller.GetProcedureUsers(procedureId, planId);

            // Then
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("ProcedureId must be greater than 0.");
        }

        [TestMethod]
        public async Task GetProcedureUsers_WithInvalidPlanId_ReturnsBadRequest()
        {
            // Given
            var procedureId = 1;
            var planId = 0;

            // When
            var result = await _controller.GetProcedureUsers(procedureId, planId);

            // Then
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("PlanId must be greater than 0.");
        }

        [TestMethod]
        public async Task AddProcedureUser_WithValidDTO_ReturnsOkResult()
        {
            // Given
            var dto = new RL.Backend.DTO.ProcedureUserDTO { ProcedureId = 1, UserId = 1, PlanId = 1 };
            var expectedResult = new ProcedureUser { ProcedureUserId = 1, ProcedureId = 1, UserId = 1, PlanId = 1 };

            _mediatorMock.Setup(m => m.Send(It.IsAny<AddProcedureUserCommand>(), It.IsAny<CancellationToken>()))
                        .ReturnsAsync(expectedResult);

            // When
            var result = await _controller.AddProcedureUser(dto);

            // Then
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            okResult.Value.Should().Be(expectedResult);
            _mediatorMock.Verify(m => m.Send(It.IsAny<AddProcedureUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task AddProcedureUser_WithException_ReturnsInternalServerError()
        {
            // Given
            var dto = new RL.Backend.DTO.ProcedureUserDTO { ProcedureId = 1, UserId = 1, PlanId = 1 };
            var exceptionMessage = "Test exception";

            _mediatorMock.Setup(m => m.Send(It.IsAny<AddProcedureUserCommand>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception(exceptionMessage));

            // When
            var result = await _controller.AddProcedureUser(dto);

            // Then
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task DeleteProcedureUser_WithValidId_ReturnsOkResult()
        {
            // Given
            var procedureUserId = 1;

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProcedureUserCommand>(), It.IsAny<CancellationToken>()))
                        .Returns((Task<Unit>)Task.CompletedTask);

            // When
            var result = await _controller.DeleteProcedureUser(procedureUserId);

            // Then
            result.Should().BeOfType<OkObjectResult>();
            var okResult = result as OkObjectResult;
            _mediatorMock.Verify(m => m.Send(It.IsAny<DeleteProcedureUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task DeleteProcedureUser_WithInvalidId_ReturnsBadRequest()
        {
            // Given
            var procedureUserId = 0;

            // When
            var result = await _controller.DeleteProcedureUser(procedureUserId);

            // Then
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("ProcedureUserId must be greater than 0.");
        }

        [TestMethod]
        public async Task DeleteProcedureUser_WithException_ReturnsInternalServerError()
        {
            // Given
            var procedureUserId = 1;
            var exceptionMessage = "Test exception";

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProcedureUserCommand>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception(exceptionMessage));

            // When
            var result = await _controller.DeleteProcedureUser(procedureUserId);

            // Then
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task DeleteProcedureUserByIds_WithValidDTO_ReturnsOkResult()
        {
            // Given
            var dto = new RL.Backend.DTO.DeleteProcedureUserDTO { ProcedureId = 1, UserId = 1, PlanId = 1 };

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProcedureUserByIdsCommand>(), It.IsAny<CancellationToken>()))
                        .Returns((Task<Unit>)Task.CompletedTask);

            // When
            var result = await _controller.DeleteProcedureUserByIds(dto);

            // Then
            result.Should().BeOfType<OkObjectResult>();
            _mediatorMock.Verify(m => m.Send(It.IsAny<DeleteProcedureUserByIdsCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task DeleteProcedureUserByIds_WithException_ReturnsInternalServerError()
        {
            // Given
            var dto = new RL.Backend.DTO.DeleteProcedureUserDTO { ProcedureId = 1, UserId = 1, PlanId = 1 };
            var exceptionMessage = "Test exception";

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProcedureUserByIdsCommand>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception(exceptionMessage));

            // When
            var result = await _controller.DeleteProcedureUserByIds(dto);

            // Then
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public async Task DeleteAllUsersInProcedure_WithValidParameters_ReturnsOkResult()
        {
            // Given
            var procedureId = 1;
            var planId = 1;

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAllUsersInProcedureCommand>(), It.IsAny<CancellationToken>()))
                        .Returns((Task<Unit>)Task.CompletedTask);

            // When
            var result = await _controller.DeleteAllUsersInProcedure(procedureId, planId);

            // Then
            result.Should().BeOfType<OkObjectResult>();
            _mediatorMock.Verify(m => m.Send(It.IsAny<DeleteAllUsersInProcedureCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestMethod]
        public async Task DeleteAllUsersInProcedure_WithInvalidProcedureId_ReturnsBadRequest()
        {
            // Given
            var procedureId = 0;
            var planId = 1;

            // When
            var result = await _controller.DeleteAllUsersInProcedure(procedureId, planId);

            // Then
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("ProcedureId must be greater than 0.");
        }

        [TestMethod]
        public async Task DeleteAllUsersInProcedure_WithInvalidPlanId_ReturnsBadRequest()
        {
            // Given
            var procedureId = 1;
            var planId = 0;

            // When
            var result = await _controller.DeleteAllUsersInProcedure(procedureId, planId);

            // Then
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Value.Should().Be("PlanId must be greater than 0.");
        }

        [TestMethod]
        public async Task DeleteAllUsersInProcedure_WithException_ReturnsInternalServerError()
        {
            // Given
            var procedureId = 1;
            var planId = 1;
            var exceptionMessage = "Test exception";

            _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAllUsersInProcedureCommand>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new Exception(exceptionMessage));

            // When
            var result = await _controller.DeleteAllUsersInProcedure(procedureId, planId);

            // Then
            result.Should().BeOfType<ObjectResult>();
            var objectResult = result as ObjectResult;
            objectResult.StatusCode.Should().Be(500);
        }

        [TestMethod]
        public void Constructor_WithNullMediator_ThrowsArgumentNullException()
        {
            // Given, When & Then
            Assert.ThrowsException<ArgumentNullException>(() =>
                new ProceduresController(_loggerMock.Object, null));
        }
    }
}