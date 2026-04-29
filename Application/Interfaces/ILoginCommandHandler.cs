using Application.DTOs;
using Application.UseCases.Auth.Commands.Login;

namespace Application.Interfaces;

public interface ILoginCommandHandler
{
    Task<AuthResponseDto> Handle(LoginCommand command, CancellationToken cancellationToken = default);
}
