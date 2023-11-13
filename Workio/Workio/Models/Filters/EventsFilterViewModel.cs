using System;
using System.ComponentModel.DataAnnotations.Schema;
using Workio.Models.Events;

namespace Workio.Models.Filters
{
    [NotMapped]
    public class EventsFilterViewModel
    {
        // create a list of filters
        public List<Filter<Event>> filters { get; set; } = new List<Filter<Event>>()
        {
            new Filter<Event> { Index = 0, Name = "Open", Condition = p => p.State == EventState.Open},
            new Filter<Event> { Index = 1, Name = "On Going", Condition = p => p.State == EventState.OnGoing},
            new Filter<Event> { Index = 2, Name = "Finish", Condition = p => p.State == EventState.Finish }
        };
        public int[] selectedFilters { get; set; } = new int[0];

        public void Sort()
        {
            filters.Sort((a, b) => a.Index.CompareTo(b.Index));
        }
    }
}
