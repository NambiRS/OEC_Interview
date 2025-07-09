using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RL.Backend.Controllers;
using RL.Backend.DTO;
using RL.Data.DataModels;

namespace RL.Backend.UnitTests;

[TestClass]
public class ProceduresControllerTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private Mock<ILogger<ProceduresController>> _loggerMock = null!;
    private ProceduresController _sut = null!;

    [TestInitialize]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<ProceduresController>>();
        _sut = new ProceduresController(_loggerMock.Object, _mediatorMock.Object);
    }

    [TestMethod]
    public async Task Get_ReturnsAllProcedures()
    {
        // Given
        var procedures = new List<Procedure>
        {
            new Procedure { ProcedureId = 1, ProcedureTitle = "P1" },
            new Procedure { ProcedureId = 2, ProcedureTitle = "P2" }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProceduresQuery>(), default))
            .ReturnsAsync(procedures);

        // When
        var result = await _sut.Get();

        // Then
        result.Should().BeEquivalentTo(procedures);
        _mediatorMock.Verify(m => m.Send(It.IsAny<GetProceduresQuery>(), default), Times.Once);
    }

    [TestMethod]
    public async Task GetProcedureUsers_ReturnsUsers()
    {
        // Given
        var users = new List<object>
        {
            new { UserId = 1, Name = "User1", ProcedureId = 1, ProcedureUserId = 1 }
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetProcedureUsersQuery>(), default))
            .ReturnsAsync(users);

        // When
        var result = await _sut.GetProcedureUsers(1) as OkObjectResult;

        // Then
        result.Should().NotBeNull();
        result!.Value.Should().BeEquivalentTo(users);
        _mediatorMock.Verify(m => m.Send(It.Is<GetProcedureUsersQuery>(q => q.ProcedureId == 1), default), Times.Once);
    }

    [TestMethod]
    public async Task AddProcedureUser_Success_ReturnsOk()
    {
        // Given
        var dto = new ProcedureUserDTO { ProcedureId = 1, UserId = 1 };
        var procedureUser = new ProcedureUser { ProcedureUserId = 1, ProcedureId = 1, UserId = 1 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<AddProcedureUserCommand>(), default))
            .ReturnsAsync(procedureUser);

        // When
        var result = await _sut.AddProcedureUser(dto);

        // Then
        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.Should().BeEquivalentTo(procedureUser);
        _mediatorMock.Verify(m => m.Send(It.Is<AddProcedureUserCommand>(c => c.ProcedureId == 1 && c.UserId == 1), default), Times.Once);
    }

    [TestMethod]
    public async Task AddProcedureUser_Failure_Returns500()
    {
        // Given
        var dto = new ProcedureUserDTO { ProcedureId = 1, UserId = 1 };
        _mediatorMock.Setup(m => m.Send(It.IsAny<AddProcedureUserCommand>(), default))
            .ThrowsAsync(new Exception("error"));

        // When
        var result = await _sut.AddProcedureUser(dto);

        // Then
        result.Should().BeOfType<ObjectResult>();
        var obj = result as ObjectResult;
        obj!.StatusCode.Should().Be(500);
        obj.Value.Should().BeEquivalentTo(new { error = "error" });
    }

    [TestMethod]
    public async Task DeleteProcedureUser_Success_ReturnsOk()
    {
        // Given
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProcedureUserCommand>(), default))
            .ReturnsAsync(Unit.Value);

        // When
        var result = await _sut.DeleteProcedureUser(1);

        // Then
        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.ToString().Should().Contain("User deleted");
        _mediatorMock.Verify(m => m.Send(It.Is<DeleteProcedureUserCommand>(c => c.ProcedureUserId == 1), default), Times.Once);
    }

    [TestMethod]
    public async Task DeleteProcedureUser_Failure_Returns500()
    {
        // Given
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteProcedureUserCommand>(), default))
            .ThrowsAsync(new Exception("error"));

        // When
        var result = await _sut.DeleteProcedureUser(1);

        // Then
        result.Should().BeOfType<ObjectResult>();
        var obj = result as ObjectResult;
        obj!.StatusCode.Should().Be(500);
        obj.Value.Should().BeEquivalentTo(new { error = "error" });
    }

    [TestMethod]
    public async Task DeleteAllUsersInProcedure_Success_ReturnsOk()
    {
        // Given
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAllUsersInProcedureCommand>(), default))
            .ReturnsAsync(Unit.Value);

        // When
        var result = await _sut.DeleteAllUsersInProcedure(1);

        // Then
        result.Should().BeOfType<OkObjectResult>();
        var ok = result as OkObjectResult;
        ok!.Value.ToString().Should().Contain("All users deleted");
        _mediatorMock.Verify(m => m.Send(It.Is<DeleteAllUsersInProcedureCommand>(c => c.ProcedureId == 1), default), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAllUsersInProcedure_Failure_Returns500()
    {
        // Given
        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteAllUsersInProcedureCommand>(), default))
            .ThrowsAsync(new Exception("error"));

        // When
        var result = await _sut.DeleteAllUsersInProcedure(1);

        // Then
        result.Should().BeOfType<ObjectResult>();
        var obj = result as ObjectResult;
        obj!.StatusCode.Should().Be(500);
        obj.Value.Should().BeEquivalentTo(new { error = "error" });
    }
}