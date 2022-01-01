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
            .Evaluate(queryable);

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
    public void ShouldApplyFacets() { }

    private static IQueryable<Car> GetTestData()
        => new[]
        {
            new Car { Year = 1999, Make = "Ford", Model = "Fiesta" },
            new Car { Year = 1999, Make = "Ford", Model = "Focus" },
            new Car { Year = 1999, Make = "Honda", Model = "Accord" },
            new Car { Year = 1999, Make = "Honda", Model = "Civic" },
            new Car { Year = 1999, Make = "Toyota", Model = "Camry" },
            new Car { Year = 1999, Make = "Toyota", Model = "Corrola" },
        }.AsQueryable();
}

class Car
{
    public int Year { get; init; }
    public string? Make { get; init; }
    public string? Model { get; init; }
}

class YearFacetDefinition : IFacetDefinition
{
    public string Qualifier => "year";
    public string Name => "Year";
}
class MakeFacetDefinition : IFacetDefinition
{
    public string Qualifier => "make";
    public string Name => "Make";
}

class CarSearchEngine : FacetEngine<Car>
{
    public CarSearchEngine()
    {
        Add(new MakeFacetDefinition());
        Add(new YearFacetDefinition());
    }
}