using System.Linq.Expressions;

namespace FacetedSearch.Core;

public interface IFacetDefinition<T>
{
    string Qualifier { get; }
    string Name { get; }
    Expression<Func<T, bool>> GetPredicate(string value);
    Facet GetFacet(IQueryable<T> source);
}
