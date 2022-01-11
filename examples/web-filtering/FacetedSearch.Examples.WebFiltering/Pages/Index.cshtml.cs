using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        var query = _context.Spell.AsQueryable();

        var result = new SpellFacetEngine()
            .Evaluate(query, new Dictionary<string, string>());

        Spells = result.Items.ToList();
        Facets = result.Facets.ToList();
    }
}

class SpellFacetEngine : FacetEngine<Spell>
{
    public SpellFacetEngine()
    {
        Add(new SchoolFacet());
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