using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.Audits.Queries.GetAllAudits;
using Application.UseCases.Auth.Commands.Login;
using Application.UseCases.Auth.Commands.Register;
using Application.UseCases.Events.Queries.GetAllEvents;
using Application.UseCases.Events.Queries.GetEventById;
using Application.UseCases.Events.Queries.GetEventSeatMap;
using Application.UseCases.Payments.Commands.ConfirmPayment;
using Application.UseCases.Reservations.Commands.CreateReservation;
using Application.UseCases.Reservations.Commands.ExpirePendingReservations;
using Application.UseCases.Reservations.Queries.GetUserReservations;
using EventsApi.BackgroundJobs;
using EventsApi.Middlewares;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Values
                .SelectMany(modelState => modelState.Errors)
                .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "La solicitud es invalida." : error.ErrorMessage)
                .ToArray();

            var response = new ErrorResponseDto
            {
                Message = errors.Length == 0 ? "La solicitud es invalida." : string.Join(" ", errors),
                StatusCode = StatusCodes.Status400BadRequest,
                Timestamp = DateTime.UtcNow
            };

            return new BadRequestObjectResult(response);
        };
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.Configure<ReservationExpirationJobOptions>(
    builder.Configuration.GetSection(ReservationExpirationJobOptions.SectionName));

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(defaultConnection));

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPasswordHasher, Sha256PasswordHasher>();

builder.Services.AddScoped<IGetAllEventsQueryHandler, GetAllEventsQueryHandler>();
builder.Services.AddScoped<IGetAllAuditsQueryHandler, GetAllAuditsQueryHandler>();
builder.Services.AddScoped<IGetEventByIdQueryHandler, GetEventByIdQueryHandler>();
builder.Services.AddScoped<IGetEventSeatMapQueryHandler, GetEventSeatMapQueryHandler>();
builder.Services.AddScoped<ReservationSelectionPolicy>();
builder.Services.AddScoped<ReservationAuditLogFactory>();
builder.Services.AddScoped<ReservationExpirationAuditLogFactory>();
builder.Services.AddScoped<ReservationMessageFactory>();
builder.Services.AddScoped<ReservationResponseFactory>();
builder.Services.AddScoped<ICreateReservationCommandHandler, CreateReservationCommandHandler>();
builder.Services.AddScoped<IExpirePendingReservationsCommandHandler, ExpirePendingReservationsCommandHandler>();
builder.Services.AddScoped<IGetUserReservationsQueryHandler, GetUserReservationsQueryHandler>();
builder.Services.AddScoped<IRegisterCommandHandler, RegisterCommandHandler>();
builder.Services.AddScoped<ILoginCommandHandler, LoginCommandHandler>();
builder.Services.AddHostedService<ExpirePendingReservationsHostedService>();
builder.Services.AddScoped<IConfirmPaymentCommandHandler, ConfirmPaymentCommandHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        context.Database.Migrate();
        AppDbInitializer.Initialize(context, passwordHasher);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "No se pudo inicializar la base de datos.");
    }
}

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseSwagger();
app.UseSwaggerUI();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
