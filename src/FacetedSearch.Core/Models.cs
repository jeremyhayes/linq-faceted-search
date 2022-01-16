namespace FacetedSearch.Core;

public class FacetedEnumerable<T>
{
    public IEnumerable<T>? Items { get; init; }
    public IEnumerable<Facet>? Facets { get; init; }
}

public class Facet
{
    public string? Qualifier { get; init; }
    public string? Name { get; init; }
    public IEnumerable<FacetValue>? Values { get; init; }
}

public class FacetValue
{
    public string Name { get; init; }
    public string Value { get; init; }
}
