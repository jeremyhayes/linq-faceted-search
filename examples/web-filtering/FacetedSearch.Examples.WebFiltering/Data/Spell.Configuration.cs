using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacetedSearch.Examples.WebFiltering.Data;

public class SpellEntityConfiguration : IEntityTypeConfiguration<Spell>
{
    public void Configure(EntityTypeBuilder<Spell> builder)
    {
        builder.HasKey(x => x.Key);
        builder.HasOne(x => x.School)
            .WithMany(x => x.SpellList)
            .HasForeignKey(x => x.SchoolKey);
        builder.HasMany(x => x.ClassList)
            .WithMany(x => x.SpellList);

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
        spells.ForEach(x =>
        {
            // fix foreign key references on import
            x.SchoolKey = x.School.Key;
            x.School = null;
        });
        builder.HasData(spells);
    }
}