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

    public IndexModel(ILogger<IndexModel> logger, SpellsContext context)
    {
        _logger = logger;
        _context = context;
    }

    public void OnGet()
    {
        var query = _context.Spell
            .Include(x => x.School)
            .AsQueryable();
        // TODO IEnumerable<KeyValuePair<string, StringValues>> is not contravariant to IDictionary<string, IEnumerable<string>>
        var filters = Request.Query.ToDictionary(x => x.Key, x => string.Join(",", x.Value));

        var result = new SpellFacetEngine()
            .Evaluate(query, filters);

        Spells = result.Items.ToList();
        Facets = result.Facets.ToList();
    }
}

class SpellFacetEngine : FacetEngine<Spell>
{
    public SpellFacetEngine()
    {
        Add(new LevelFacet());
        Add(new SchoolFacet());
    }

    class LevelFacet : IFacetDefinition<Spell>
    {
        public string Qualifier => "level";
        public string Name => "Level";

        public Expression<Func<Spell, bool>> GetPredicate(string value)
        {
            var intValue = int.Parse(value);
            return x => x.Level == intValue;
        }

        public Facet GetFacet(IQueryable<Spell> source)
        {
            return new()
            {
                Qualifier = Qualifier,
                Name = Name,
                Values = source
                    .GroupBy(x => x.Level)
                    .Select(x => new FacetValue
                    {
                        Name = x.Key == 0 ? "Cantrips" : $"Level {x.Key}",
                        Value = x.Key.ToString(),
                    })
            };
        }
    }

    class SchoolFacet : IFacetDefinition<Spell>
    {
        public string Qualifier => "school";
        public string Name => "School";

        public Expression<Func<Spell, bool>> GetPredicate(string value)
        {
            return x => x.School.Key == value;
        }

        public Facet GetFacet(IQueryable<Spell> source)
        {
            return new()
            {
                Qualifier = Qualifier,
                Name = Name,
                Values = source
                    .GroupBy(x => x.School.Key)
                    .Select(x => new FacetValue
                    {
                        Name = x.First().School.Name,
                        Value = x.Key
                    })
            };
        }
    }
}