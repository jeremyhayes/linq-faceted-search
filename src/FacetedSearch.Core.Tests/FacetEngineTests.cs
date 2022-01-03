using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

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
        Assert.Contains(result.Facets, x => x.Qualifier == "make");
        Assert.Contains(result.Facets, x => x.Qualifier == "year");
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
        Assert.Equal(2, result.Facets?.Count());
        Assert.Contains(result.Facets, x => x.Qualifier == "make");
        Assert.Contains(result.Facets, x => x.Qualifier == "year");
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
        Assert.Equal(2, result.Facets?.Count());
        Assert.Contains(result.Facets, x => x.Qualifier == "make");
        Assert.Contains(result.Facets, x => x.Qualifier == "year");
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
        Assert.Contains(result.Facets, x => x.Qualifier == "make");
        Assert.Contains(result.Facets, x => x.Qualifier == "year");
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
        Assert.Equal(2, result.Facets?.Count());
        Assert.Contains(result.Facets, x => x.Qualifier == "make");
        Assert.Contains(result.Facets, x => x.Qualifier == "year");
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

    public Predicate<Car> GetPredicate(string value)
    {
        // TODO clean string parsing; strong value typing
        int.TryParse(value, out var valueInt);
        return x => x.Year == valueInt;
    }
}

class MakeFacetDefinition : IFacetDefinition<Car>
{
    public string Qualifier => "make";
    public string Name => "Make";

    public Predicate<Car> GetPredicate(string value)
    {
        return x => x.Make == value;
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