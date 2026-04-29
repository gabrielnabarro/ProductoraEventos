using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.Auth.Commands.Login;
using Application.UseCases.Auth.Commands.Register;
using Microsoft.AspNetCore.Mvc;

namespace EventsApi.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IRegisterCommandHandler _registerCommandHandler;
    private readonly ILoginCommandHandler _loginCommandHandler;

    public AuthController(IRegisterCommandHandler registerCommandHandler, ILoginCommandHandler loginCommandHandler)
    {
        _registerCommandHandler = registerCommandHandler;
        _loginCommandHandler = loginCommandHandler;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterCommand command, CancellationToken cancellationToken)
    {
        var response = await _registerCommandHandler.Handle(command, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, response);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginCommand command, CancellationToken cancellationToken)
    {
        var response = await _loginCommandHandler.Handle(command, cancellationToken);
        return Ok(response);
    }
}
