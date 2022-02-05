using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using FacetedSearch.Core;
using FacetedSearch.Examples.WebFiltering.Data;

namespace FacetedSearch.Examples.WebFiltering.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly SpellsContext _context;

    public List<Spell> Spells { get; set; }
    public List<Facet> Facets { get; set; }
    public List<AppliedFilter> AppliedFilters { get; set; }

    public IndexModel(ILogger<IndexModel> logger, SpellsContext context)
    {
        _logger = logger;
        _context = context;
    }

    public void OnGet()
    {
        var query = _context.Spell
            .Include(x => x.School)
            .Include(x => x.ClassList)
            .ToList()
            .AsQueryable();
        // TODO IEnumerable<KeyValuePair<string, StringValues>> is not contravariant to IDictionary<string, IEnumerable<string>>
        var filters = Request.Query.ToDictionary(x => x.Key, x => string.Join(",", x.Value));

        var result = new SpellFacetEngine()
            .Evaluate(query, filters);

        Spells = result.Items.ToList();
        Facets = result.Facets.ToList();
        AppliedFilters = result.AppliedFilters.ToList();
    }
}

class SpellFacetEngine : FacetEngine<Spell>
{
    public SpellFacetEngine()
    {
        Add(new LevelFacet());
        Add(new SchoolFacet());
        Add(new ClassFacet());
    }

    class LevelFacet : FacetDefinition<Spell, int>
    {
        public override string Qualifier => "level";
        public override string Name => "Level";

        public override Expression<Func<Spell, int>> PropertyExpression
            => s => s.Level;

        public override Expression<Func<Spell, NameValue<int>>> GroupByExpression
            => s => new NameValue<int>(s.Level == 0 ? "Cantrips" : $"Level {s.Level}", s.Level);
    }

    class SchoolFacet : FacetDefinition<Spell, string>
    {
        public override string Qualifier => "school";
        public override string Name => "School";

        public override Expression<Func<Spell, string>> PropertyExpression
            => s => s.SchoolKey;

        public override Expression<Func<Spell, NameValue<string>>> GroupByExpression
            => s => new(s.School.Name, s.School.Key);
    }

    class ClassFacet : IFacetDefinition<Spell>
    {
        public string Qualifier => "class";
        public string Name => "Class";

        public Expression<Func<Spell, bool>> GetPredicate(string value)
        {
            return x => x.ClassList.Any(y => y.Key == value);
        }

        public Facet GetFacet(IQueryable<Spell> source)
        {
            return new()
            {
                Qualifier = Qualifier,
                Name = Name,
                Values = source
                    .SelectMany(x => x.ClassList)
                    .GroupBy(x => x.Key)
                    .Select(x => new FacetValue
                    {
                        Name = x.First().Name,
                        Value = x.Key,
                        Count = x.Count(),
                    })
            };
        }

        public AppliedFilter GetAppliedFilter(IQueryable<Spell> source, string value)
        {
            return new()
            {
                Qualifier = Qualifier,
                Name = Name,
                Values = source
                    .SelectMany(x => x.ClassList)
                    .Where(x => x.Key == value)
                    .GroupBy(x => x.Key)
                    .Select(x => new AppliedFilterValue
                    {
                        Name = x.First().Name,
                        Value = x.Key,
                    })
            };
        }
    }
}