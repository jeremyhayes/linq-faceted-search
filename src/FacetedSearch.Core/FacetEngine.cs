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
        var items = source;
        var appliedFilters = new List<AppliedFilter>();

        foreach (var facet in _facetDefinitions)
        {
            if (!filters.TryGetValue(facet.Qualifier, out var filterValue))
                continue;

            var predicate = facet.GetPredicate(filterValue);
            items = items.Where(predicate);

            var appliedFilter = facet.GetAppliedFilter(source, filterValue);
            appliedFilters.Add(appliedFilter);
        }

        // build facets after filtering so each includes only available values
        var facets = _facetDefinitions
            .Select(x => x.GetFacet(items))
            .Where(x => x.Values.Count() > 1);

        return new()
        {
            Items = items,
            Facets = facets,
            AppliedFilters = appliedFilters,
        };
    }
}
