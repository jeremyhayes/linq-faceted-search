using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacetedSearch.Examples.WebFiltering.Data;

public class SpellEntityConfiguration : IEntityTypeConfiguration<Spell>
{
    public void Configure(EntityTypeBuilder<Spell> builder)
    {
        builder.HasKey(x => x.Key);

        builder.HasData(
            new Spell { Key = "spells:foo", Name = "foo" },
            new Spell { Key = "spells:bar", Name = "bar" },
            new Spell { Key = "spells:baz", Name = "baz" }
        );
    }
}