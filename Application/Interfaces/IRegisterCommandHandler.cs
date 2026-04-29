using Application.DTOs;
using Application.UseCases.Auth.Commands.Register;

namespace Application.Interfaces;

public interface IRegisterCommandHandler
{
    Task<AuthResponseDto> Handle(RegisterCommand command, CancellationToken cancellationToken = default);
}
