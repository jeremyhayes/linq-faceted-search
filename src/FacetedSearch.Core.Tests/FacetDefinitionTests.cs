using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;

using Xunit;

namespace FacetedSearch.Core.Tests;

public class FacetDefinitionTests
{
    [Fact]
    public void ShouldEvaluatePredicate()
    {
        var target = new TestFacetDefinition();
        target._propertyExpression = x => x.Title;

        var a = new[]
        {
            new TestModel{ Id = 1, Title = "foo" },
            new TestModel{ Id = 2, Title = "bar" },
            new TestModel{ Id = 3, Title = "qux" },
        };
        var result = a.AsQueryable().Where(target.GetPredicate("qux")).ToList();

        Assert.NotNull(result);
        var resultItem = Assert.Single(result);
        Assert.Equal(3, resultItem.Id);
        Assert.Equal("qux", resultItem.Title);
    }

    [Fact]
    public void ShouldEvaluateGroupBy()
    {
        var target = new TestFacetDefinition();
        target._groupByExpression = x => new NameValue<string>(x.Title, x.Title);

        var a = new[]
        {
            new TestModel{ Id = 1, Title = "foo" },
            new TestModel{ Id = 2, Title = "bar" },
            new TestModel{ Id = 3, Title = "qux" },
            new TestModel{ Id = 4, Title = "foo" },
            new TestModel{ Id = 5, Title = "bar" },
            new TestModel{ Id = 6, Title = "qux" },
            new TestModel{ Id = 7, Title = "foo" },
            new TestModel{ Id = 8, Title = "bar" },
            new TestModel{ Id = 9, Title = "qux" },
        };
        var result = target.GetFacet(a.AsQueryable());

        Assert.NotNull(result);
        Assert.NotNull(result.Values);
        Assert.Equal(3, result.Values!.Count());
        var fooItem = Assert.Single(result.Values, x => x.Value == "foo");
        Assert.Equal(3, fooItem.Count);
        var barItem = Assert.Single(result.Values, x => x.Value == "bar");
        Assert.Equal(3, barItem.Count);
        var quxItem = Assert.Single(result.Values, x => x.Value == "qux");
        Assert.Equal(3, quxItem.Count);
    }

    class TestModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
    }

    class TestFacetDefinition : FacetDefinition<TestModel, string>
    {
        public string _qualifier;
        public override string Qualifier => _qualifier;

        public string _name;
        public override string Name => _name;

        public Expression<Func<TestModel, string>> _propertyExpression;
        public override Expression<Func<TestModel, string>> PropertyExpression => _propertyExpression;

        public Expression<Func<TestModel, NameValue<string>>> _groupByExpression;
        public override Expression<Func<TestModel, NameValue<string>>> GroupByExpression => _groupByExpression;
    }
}
