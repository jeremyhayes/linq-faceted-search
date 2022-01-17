using System.Text.Json.Serialization;

namespace FacetedSearch.Examples.WebFiltering.Data;

public class Class
{
    [JsonPropertyName("_key")]
    public string Key { get; set; }
    public string Name { get; set; }

    public virtual ICollection<Spell> SpellList { get; set; }
}