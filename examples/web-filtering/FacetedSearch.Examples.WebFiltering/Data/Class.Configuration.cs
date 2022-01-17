using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacetedSearch.Examples.WebFiltering.Data;

public class ClassEntityConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.HasKey(x => x.Key);
        builder.HasMany(x => x.SpellList)
            .WithMany(x => x.ClassList);

        ConfigureSeedData(builder);
    }

    private void ConfigureSeedData(EntityTypeBuilder<Class> builder)
    {
        using var stream = File.OpenRead(@"Data/Seed/classes.json");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var data = JsonSerializer.Deserialize<List<Class>>(stream, options);
        builder.HasData(data);
    }
}