using Api.Infraestructure.Data;
using Domain.Entities;

namespace Api.Infrastructure.Data
{
    public static class ApiDbInitializer
    {
        public static void Initialize(ApiDBContext context)
        {
            // Verifica si ya hay eventos en la base de datos
            if (context.Events.Any())
            {
                return;
            }

            var evento = new Event
            {
                Name = "ACDC en Argentina",
                EventDate = DateTime.Now.AddMonths(2), // El evento será en 2 meses
                Venue = "Estadio River Plate",
                Status = "Active"
            };
            context.Events.Add(evento);
            context.SaveChanges();

            var sectorVip = new Sector { EventId = evento.Id, Name = "VIP", Price = 150000.00m, Capacity = 50 };
            var sectorGeneral = new Sector { EventId = evento.Id, Name = "General", Price = 90000.00m, Capacity = 50 };

            context.Sectors.AddRange(sectorVip, sectorGeneral);
            context.SaveChanges(); 


            for (int i = 1; i <= 50; i++)
            {
                // Butacas VIP (Ejemplo de identificador de fila: "V-1", "V-2"...)
                context.Seats.Add(new Seat
                {
                    Id = Guid.NewGuid(),
                    SectorId = sectorVip.Id,
                    RowIdentifier = "V",
                    SeatNumber = i,
                    Status = "Available",
                    Version = 1
                });

                // Butacas General (Ejemplo de identificador de fila: "G-1", "G-2"...)
                context.Seats.Add(new Seat
                {
                    Id = Guid.NewGuid(),
                    SectorId = sectorGeneral.Id,
                    RowIdentifier = "G",
                    SeatNumber = i,
                    Status = "Available",
                    Version = 1
                });
            }

            context.SaveChanges();
        }
    }
}