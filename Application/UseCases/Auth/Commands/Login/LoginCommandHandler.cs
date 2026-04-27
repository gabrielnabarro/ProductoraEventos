using Application.DTOs;
using Application.Interfaces;
using Domain.Exceptions;

namespace Application.UseCases.Auth.Commands.Login;

public sealed class LoginCommandHandler : ILoginCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand command, CancellationToken cancellationToken = default)
    {
        var email = NormalizeEmail(command.Email);
        var password = command.Password?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException("El email es obligatorio.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ValidationException("La contrasena es obligatoria.");
        }

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(password, user.PasswordHash))
        {
            throw new UnauthorizedException("Email o contrasena incorrectos.");
        }

        return new AuthResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email
        };
    }

    private static string NormalizeEmail(string? email)
    {
        return (email ?? string.Empty).Trim().ToLowerInvariant();
    }
}
