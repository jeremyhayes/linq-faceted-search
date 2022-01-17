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

        // using EF built-in support for many-many type mapping without an explicit join entity
        // made most sense to load the join seed data here
        using var stream2 = File.OpenRead(@"Data/Seed/spells.json");
        var joinData = JsonSerializer.Deserialize<List<SpellJson>>(stream2, options)
            .SelectMany(x => x.classes, (x, y) => new
            {
                SpellListKey = x._key,
                ClassListKey = y._key
            })
            .ToList();

        builder.HasMany(x => x.ClassList)
            .WithMany(x => x.SpellList)
            .UsingEntity(j => j.HasData(joinData));
    }

    class SpellJson
    {
        public string _key { get; set; }
        public ICollection<ClassJson> classes { get; set; }
    }
    class ClassJson
    {
        public string _key { get; set; }
    }
}