using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.Events.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Events.Handlers
{
    public class GetSeatsBySectorHandler
    {
        private readonly IEventRepository _repository;
        public GetSeatsBySectorHandler(IEventRepository repository) => _repository = repository;

        public async Task<IEnumerable<SeatDto>> Handle(GetSeatsBySectorQuery query)
        {
            var seats = await _repository.GetSeatsBySectorAsync(query.SectorId);
            return seats.Select(s => new SeatDto
            {
                Id = s.Id,
                RowIdentifier = s.RowIdentifier,
                SeatNumber = s.SeatNumber,
                Status = s.Status
            }).ToList();
        }
    }
}
