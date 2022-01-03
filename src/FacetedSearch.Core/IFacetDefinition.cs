namespace FacetedSearch.Core;

public interface IFacetDefinition<T>
{
    string Qualifier { get; }
    string Name { get; }
    Predicate<T> GetPredicate(string value);
    Facet GetFacet(IQueryable<T> source);
}
