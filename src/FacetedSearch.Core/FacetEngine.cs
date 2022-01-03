namespace FacetedSearch.Core;

public abstract class FacetEngine<T>
{
    private readonly List<IFacetDefinition<T>> _facetDefinitions = new();

    protected void Add(IFacetDefinition<T> facetDefinition)
    {
        _facetDefinitions.Add(facetDefinition);
    }

    public FacetedEnumerable<T> Evaluate(IQueryable<T> source, IDictionary<string, string> filters)
    {
        // building facet using the original unfiltered source
        // TODO apply facets to fully-filtered set to help user avoid filtering to nothing
        var facets = _facetDefinitions
            .Select(x => x.GetFacet(source));

        var items = source;
        foreach (var facet in _facetDefinitions)
        {
            if (!filters.TryGetValue(facet.Qualifier, out var filterValue))
                continue;

            var predicate = facet.GetPredicate(filterValue);
            // TODO this _probably_ won't work in linq2sql/EF
            items = items.Where(x => predicate(x));
        }

        return new()
        {
            Items = items,
            Facets = facets,
        };
    }
}
