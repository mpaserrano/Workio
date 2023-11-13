using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Workio.Attributes;

namespace Workio.Models.Filters
{
    [NotMapped]
    public class TeamsFilterViewModel
    {
        public Guid currentUserId { get; set; }

        public string Name { get; set; } = "";

        // create a list of filters
        public List<Filter<Team>> filters { get; set; }

        

        public TeamsFilterViewModel()
        {
            Func<string, string, bool> containsIgnoringCaseAndAccentuation = (s1, s2) =>
                CultureInfo.InvariantCulture.CompareInfo.IndexOf(s1, s2, CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) >= 0;

            // define the common condition as a lambda expression
            Func<Team, bool> nameCondition = p =>
                containsIgnoringCaseAndAccentuation(p.TeamName, Name) ||
                p.Skills.Any(s => containsIgnoringCaseAndAccentuation(s.TagName, Name)) ||
                p.Positions.Any(p => containsIgnoringCaseAndAccentuation(p.Name, Name));

            filters = new List<Filter<Team>>()
            {
                new Filter<Team> { Index = 0, Name = "Open", FilterType = FilterType.Checkbox, Condition = p => p.Status == TeamStatus.Open && nameCondition(p)},
                new Filter<Team> { Index = 1, Name = "Closed", FilterType = FilterType.Checkbox, Condition = p => p.Status == TeamStatus.Closed && nameCondition(p)},
                new Filter<Team> { Index = 2, Name = "Finish", FilterType = FilterType.Checkbox, Condition = p => p.Status == TeamStatus.Finish && nameCondition(p)},
                new Filter<Team> { Index = 3, Name = "Owned", FilterType = FilterType.Checkbox, Condition = p => p.OwnerId == currentUserId && nameCondition(p)},
                new Filter<Team> { Index = 4, Name = "Name", FilterType = FilterType.Input, Condition = nameCondition },
            };
        }

        public int[] selectedFilters { get; set; } = new int[0];

        public void Sort()
        {
            filters.Sort((a, b) => a.Index.CompareTo(b.Index));
        }
    }


    [NotMapped]
    public class Filter<T>
    {
        public int Index { get; set; }
        public string Name { get; set; }
        public FilterType FilterType { get; set; }
        public Func<T, bool> Condition { get; set; }
    }

    public enum FilterType
    {
        Checkbox, Input
    }
}
