namespace FacetedSearch.Core;

public abstract class FacetEngine<T>
{
    private readonly List<IFacetDefinition> _facetDefinitions = new();

    protected void Add(IFacetDefinition facetDefinition)
    {
        _facetDefinitions.Add(facetDefinition);
    }

    public FacetedEnumerable<T> Evaluate(IQueryable<T> source)
    {
        return new()
        {
            Items = source,
            Facets = _facetDefinitions
                .Select(x => new Facet
                {
                    Qualifier = x.Qualifier,
                    Name = x.Name,
                })
        };
    }
}
