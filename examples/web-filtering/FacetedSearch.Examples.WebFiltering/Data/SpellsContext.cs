using Microsoft.EntityFrameworkCore;

using System.ComponentModel.DataAnnotations;

namespace FacetedSearch.Examples.WebFiltering.Data;

public class SpellsContext : DbContext
{
    public SpellsContext(DbContextOptions<SpellsContext> options)
        : base(options)
    {
    }

    public DbSet<Spell> Spell { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(SpellsContext).Assembly);
    }
}
