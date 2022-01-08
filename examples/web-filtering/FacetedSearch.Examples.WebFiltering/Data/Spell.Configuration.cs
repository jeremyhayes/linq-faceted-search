using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacetedSearch.Examples.WebFiltering.Data;

public class SpellEntityConfiguration : IEntityTypeConfiguration<Spell>
{
    public void Configure(EntityTypeBuilder<Spell> builder)
    {
        builder.HasKey(x => x.Key);
        ConfigureSeedData(builder);
    }

    private void ConfigureSeedData(EntityTypeBuilder<Spell> builder)
    {
        using var stream = File.OpenRead(@"Data/Seed/spells.json");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var spells = JsonSerializer.Deserialize<List<Spell>>(stream, options);
        builder.HasData(spells);
    }
}