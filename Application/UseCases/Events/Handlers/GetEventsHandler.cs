using Application.DTOs;
using Application.Interfaces;
using Application.UseCases.Events.Queries;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases.Events.Handlers
{
    public class GetEventsHandler
    {

        private readonly IEventRepository _repository;

        public GetEventsHandler(IEventRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<EventDto>> Handle(GetEventsQuery query)
        {

            var events = await _repository.GetActiveEventsAsync();

            return events.Select(e => new EventDto
            {
                Id = e.Id,
                Name = e.Name,
                EventDate = e.EventDate,
                Venue = e.Venue
            }).ToList();
        }
    }
}
