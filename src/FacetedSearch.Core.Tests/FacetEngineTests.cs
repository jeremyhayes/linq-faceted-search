using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

using Xunit;

namespace FacetedSearch.Core.Tests;

public class FacetEngineTests
{
    [Fact]
    public void ShouldReturnItemsAndFacets()
    {
        // Given
        var queryable = GetTestData();

        // When
        var result = new CarSearchEngine()
            .Evaluate(queryable, ImmutableDictionary<string, string>.Empty);

        // Then
        Assert.NotNull(result);

        Assert.NotNull(result.Items);
        Assert.Equal(6, result.Items?.Count());

        Assert.NotNull(result.Facets);
        Assert.Equal(2, result.Facets?.Count());
        var makeFacet = Assert.Single<Facet>(result.Facets, x => x.Qualifier == "make");
        Assert.Equal(3, makeFacet.Values?.Count());
        Assert.Single(makeFacet.Values, x => x.Value == "Ford");
        Assert.Single(makeFacet.Values, x => x.Value == "Honda");
        Assert.Single(makeFacet.Values, x => x.Value == "Toyota");
        var yearFacet = Assert.Single<Facet>(result.Facets, x => x.Qualifier == "year");
        Assert.Equal(6, yearFacet.Values?.Count());
        Assert.Single(yearFacet.Values, x => x.Value == "2000");
        Assert.Single(yearFacet.Values, x => x.Value == "2001");
        Assert.Single(yearFacet.Values, x => x.Value == "2002");
        Assert.Single(yearFacet.Values, x => x.Value == "2003");
        Assert.Single(yearFacet.Values, x => x.Value == "2004");
        Assert.Single(yearFacet.Values, x => x.Value == "2005");
    }

    [Fact]
    public void ShouldApplySingleFacet()
    {
        // Given
        var queryable = GetTestData();

        // When
        var filters = new Dictionary<string, string>
        {
            ["make"] = "Honda"
        };
        var result = new CarSearchEngine()
            .Evaluate(queryable, filters);

        // Then
        Assert.NotNull(result);

        Assert.NotNull(result.Items);
        Assert.Equal(2, result.Items?.Count());
        Assert.All(result.Items, x => Assert.Equal("Honda", x.Make));

        Assert.NotNull(result.Facets);
        Assert.Equal(1, result.Facets?.Count());
        var yearFacet = Assert.Single<Facet>(result.Facets, x => x.Qualifier == "year");
        Assert.Equal(2, yearFacet.Values?.Count());
        Assert.Single(yearFacet.Values, x => x.Value == "2002");
        Assert.Single(yearFacet.Values, x => x.Value == "2003");
    }

    [Fact]
    public void ShouldApplyMultipleFacets()
    {
        // Given
        var queryable = GetTestData();

        // When
        var filters = new Dictionary<string, string>
        {
            ["make"] = "Honda",
            ["year"] = "2003",
        };
        var result = new CarSearchEngine()
            .Evaluate(queryable, filters);

        // Then
        Assert.NotNull(result);

        Assert.NotNull(result.Items);
        Assert.Equal(1, result.Items?.Count());
        var resultItem = Assert.Single<Car>(result.Items);
        Assert.Equal(2003, resultItem.Year);
        Assert.Equal("Honda", resultItem.Make);
        Assert.Equal("Civic", resultItem.Model);

        Assert.NotNull(result.Facets);
        Assert.Empty(result.Facets);
    }

    [Fact]
    public void ShouldIgnoreInvalidFilters()
    {
        // Given
        var queryable = GetTestData();

        // When
        var filters = new Dictionary<string, string>
        {
            ["fake"] = "not real",
        };
        var result = new CarSearchEngine()
            .Evaluate(queryable, filters);

        // Then
        Assert.NotNull(result);

        Assert.NotNull(result.Items);
        Assert.Equal(6, result.Items?.Count());

        Assert.NotNull(result.Facets);
        Assert.Equal(2, result.Facets?.Count());
        var makeFacet = Assert.Single<Facet>(result.Facets, x => x.Qualifier == "make");
        Assert.Equal(3, makeFacet.Values?.Count());
        Assert.Single(makeFacet.Values, x => x.Value == "Ford");
        Assert.Single(makeFacet.Values, x => x.Value == "Honda");
        Assert.Single(makeFacet.Values, x => x.Value == "Toyota");
        var yearFacet = Assert.Single<Facet>(result.Facets, x => x.Qualifier == "year");
        Assert.Equal(6, yearFacet.Values?.Count());
        Assert.Single(yearFacet.Values, x => x.Value == "2000");
        Assert.Single(yearFacet.Values, x => x.Value == "2001");
        Assert.Single(yearFacet.Values, x => x.Value == "2002");
        Assert.Single(yearFacet.Values, x => x.Value == "2003");
        Assert.Single(yearFacet.Values, x => x.Value == "2004");
        Assert.Single(yearFacet.Values, x => x.Value == "2005");
    }

    [Fact]
    public void ShouldFilterToZeroResults()
    {
        // Given
        var queryable = GetTestData();

        // When
        var filters = new Dictionary<string, string>
        {
            ["make"] = "Honda",
            ["year"] = "1800",
        };
        var result = new CarSearchEngine()
            .Evaluate(queryable, filters);

        // Then
        Assert.NotNull(result);
        Assert.NotNull(result.Items);
        Assert.Empty(result.Items);
        Assert.NotNull(result.Facets);
        Assert.Empty(result.Facets);
    }

    private static IQueryable<Car> GetTestData()
        => new[]
        {
            new Car { Year = 2000, Make = "Ford", Model = "Fiesta" },
            new Car { Year = 2001, Make = "Ford", Model = "Focus" },
            new Car { Year = 2002, Make = "Honda", Model = "Accord" },
            new Car { Year = 2003, Make = "Honda", Model = "Civic" },
            new Car { Year = 2004, Make = "Toyota", Model = "Camry" },
            new Car { Year = 2005, Make = "Toyota", Model = "Corrola" },
        }.AsQueryable();
}

class Car
{
    public int Year { get; init; }
    public string? Make { get; init; }
    public string? Model { get; init; }
}

class YearFacetDefinition : IFacetDefinition<Car>
{
    public string Qualifier => "year";
    public string Name => "Year";

    public Expression<Func<Car, bool>> GetPredicate(string value)
    {
        // TODO clean string parsing; strong value typing
        int.TryParse(value, out var valueInt);
        return x => x.Year == valueInt;
    }

    public Facet GetFacet(IQueryable<Car> source)
    {
        return new Facet
        {
            Qualifier = Qualifier,
            Name = Name,
            Values = source
                .GroupBy(x => x.Year)
                .Select(x => new FacetValue
                {
                    Name = x.Key.ToString(),
                    Value = x.Key.ToString(),
                })
        };
    }
}

class MakeFacetDefinition : IFacetDefinition<Car>
{
    public string Qualifier => "make";
    public string Name => "Make";

    public Expression<Func<Car, bool>> GetPredicate(string value)
    {
        return x => x.Make == value;
    }

    public Facet GetFacet(IQueryable<Car> source)
    {
        return new Facet
        {
            Qualifier = Qualifier,
            Name = Name,
            Values = source
                .GroupBy(x => x.Make)
                .Select(x => new FacetValue
                {
                    Name = x.Key,
                    Value = x.Key,
                })
        };
    }
}

class CarSearchEngine : FacetEngine<Car>
{
    public CarSearchEngine()
    {
        Add(new MakeFacetDefinition());
        Add(new YearFacetDefinition());
    }
}