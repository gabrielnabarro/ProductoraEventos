using Application.DTOs;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Application.Interfaces;
using Application.UseCases.Events.Queries.GetAllEvents;
using Application.UseCases.Events.Queries.GetEventById;
using Application.UseCases.Events.Queries.GetEventSeatMap;
using Application.UseCases.Reservations.Commands.CreateReservation;
using EventsApi.Middlewares;
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

var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(defaultConnection));

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<ISeatRepository, SeatRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IGetAllEventsQueryHandler, GetAllEventsQueryHandler>();
builder.Services.AddScoped<IGetEventByIdQueryHandler, GetEventByIdQueryHandler>();
builder.Services.AddScoped<IGetEventSeatMapQueryHandler, GetEventSeatMapQueryHandler>();
builder.Services.AddScoped<ICreateReservationCommandHandler, CreateReservationCommandHandler>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<AppDbContext>();
        context.Database.Migrate();
        AppDbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "No se pudo inicializar la base de datos.");
    }
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
