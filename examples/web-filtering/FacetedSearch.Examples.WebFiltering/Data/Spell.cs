using Microsoft.EntityFrameworkCore;

namespace FacetedSearch.Examples.WebFiltering.Data;

public class Spell
{
    public string Key { get; set; }
    public int Level { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string School { get; set; }
    public bool Ritual { get; set; }
    public string CastingTime { get; set; }
    public string Range { get; set; }
    // public ICollection<SpellComponent> Components { get; set; }
    public string Duration { get; set; }
    // public ICollection<Class> Classes { get; set; }
}