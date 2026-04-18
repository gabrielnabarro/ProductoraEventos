using Infraestructure.Persistence;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApiDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registro de Repositorios
builder.Services.AddScoped<Application.Interfaces.IEventRepository, Infraestructure.Repositories.EventRepository>();
builder.Services.AddScoped<Application.Interfaces.IReservationRepository, Infraestructure.Repositories.ReservationRepository>();

// Registro de Handlers (Events)
builder.Services.AddScoped<Application.UseCases.Events.Handlers.GetEventsHandler>();
builder.Services.AddScoped<Application.UseCases.Events.Handlers.GetSectorsByEventHandler>();
builder.Services.AddScoped<Application.UseCases.Events.Handlers.GetSeatsBySectorHandler>();

// Registro de Handlers (Reservations)
builder.Services.AddScoped<Application.UseCases.Reservations.Handlers.CreateReservationHandler>();

var app = builder.Build();

//Inicio Precarga de datos

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApiDBContext>();
        // context.Database.EnsureCreated(); 

        ApiDbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Ocurrió un error al poblar la base de datos.");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
