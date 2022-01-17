using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacetedSearch.Examples.WebFiltering.Data;

public class SchoolEntityConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.HasKey(x => x.Key);
        ConfigureSeedData(builder);
    }

    private void ConfigureSeedData(EntityTypeBuilder<School> builder)
    {
        using var stream = File.OpenRead(@"Data/Seed/schools.json");
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        var schools = JsonSerializer.Deserialize<List<School>>(stream, options);
        builder.HasData(schools);
    }
}