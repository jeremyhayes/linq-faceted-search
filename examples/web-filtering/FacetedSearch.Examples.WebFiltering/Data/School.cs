using System.Text.Json.Serialization;

using Microsoft.EntityFrameworkCore;

namespace FacetedSearch.Examples.WebFiltering.Data;

public class School
{
    [JsonPropertyName("_key")]
    public string Key { get; set; }
    public string Name { get; set; }

    public virtual ICollection<Spell> SpellList { get; set; }
}