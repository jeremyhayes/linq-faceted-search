using System.Linq.Expressions;

namespace FacetedSearch.Core;

public interface IFacetDefinition<T>
{
    string Qualifier { get; }
    string Name { get; }
    Expression<Func<T, bool>> GetPredicate(string value);
    Facet GetFacet(IQueryable<T> source);
    AppliedFilter GetAppliedFilter(IQueryable<T> source, string value);
}

public abstract class FacetDefinition<T, TProp> : IFacetDefinition<T>
{
    public abstract string Qualifier { get; }
    public abstract string Name { get; }
    public abstract Expression<Func<T, TProp>> PropertyExpression { get; }
    public abstract Expression<Func<T, NameValue<TProp>>> GroupByExpression { get; }

    public Expression<Func<T, bool>> GetPredicate(string value)
    {
        var selector = this.PropertyExpression;
        var parameter = Expression.Parameter(typeof(T));
        var expressionParameter = Expression.Property(parameter, GetParameterName(selector));

        var body = Expression.Equal(
            expressionParameter,
            Expression.Constant(value, typeof(string)));
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private static string GetParameterName<TData, TKey>(Expression<Func<TData, TKey>> expression)
    {
        // TODO extend this to support more complex expression types
        // Currently requires a direct property access (e.g. x => x.Description)
        if (expression.Body is MemberExpression memberExpression)
            return memberExpression.ToString().Substring(2);

        throw new NotSupportedException($"Unsupported {expression.Body.NodeType} expression type.");
    }

    public Facet GetFacet(IQueryable<T> source)
    {
        return new()
        {
            Qualifier = this.Qualifier,
            Name = this.Name,
            Values = source
                .GroupBy(this.GroupByExpression)
                .Select(x => new FacetValue
                {
                    Name = x.Key.Name,
                    Value = x.Key.Value.ToString(), // TODO casting
                    Count = x.Count(),
                })
        };
    }

    public AppliedFilter GetAppliedFilter(IQueryable<T> source, string value)
    {
        return new()
        {
            Qualifier = this.Qualifier,
            Name = this.Name,
            Values = source
                .Where(this.GetPredicate(value))
                .GroupBy(this.GroupByExpression)
                .Select(x => new AppliedFilterValue
                {
                    Name = x.Key.Name,
                    Value = x.Key.Value.ToString(), // TODO casting
                })
        };
    }
}

public record NameValue<TValue>(string Name, TValue Value);
