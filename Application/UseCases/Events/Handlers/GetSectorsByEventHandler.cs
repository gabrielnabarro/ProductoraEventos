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
    public class GetSectorsByEventHandler
    {
        private readonly IEventRepository _repository;
        public GetSectorsByEventHandler(IEventRepository repository) => _repository = repository;

        public async Task<IEnumerable<SectorDto>> Handle(GetSectorsByEventQuery query)
        {
            var sectors = await _repository.GetSectorsByEventAsync(query.EventId);
            return sectors.Select(s => new SectorDto
            {
                Id = s.Id,
                Name = s.Name,
                Price = s.Price,
                Capacity = s.Capacity
            }).ToList();
        }
    }
}
