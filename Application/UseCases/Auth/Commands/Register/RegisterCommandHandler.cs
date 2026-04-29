using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Exceptions;

namespace Application.UseCases.Auth.Commands.Register;

public sealed class RegisterCommandHandler : IRegisterCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterCommandHandler(IUserRepository userRepository, IPasswordHasher passwordHasher, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponseDto> Handle(RegisterCommand command, CancellationToken cancellationToken = default)
    {
        var name = NormalizeName(command.Name);
        var email = NormalizeEmail(command.Email);
        var password = command.Password?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ValidationException("El nombre es obligatorio.");
        }

        if (name.Length > 100)
        {
            throw new ValidationException("El nombre no puede superar los 100 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new ValidationException("El email es obligatorio.");
        }

        if (email.Length > 150)
        {
            throw new ValidationException("El email no puede superar los 150 caracteres.");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ValidationException("La contrasena es obligatoria.");
        }

        if (password.Length < 4)
        {
            throw new ValidationException("La contrasena debe tener al menos 4 caracteres.");
        }

        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
        {
            throw new ConflictException("Ya existe un usuario con ese email.");
        }

        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = _passwordHasher.Hash(password)
        };

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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

    private static string NormalizeName(string? name)
    {
        return (name ?? string.Empty).Trim();
    }
}
