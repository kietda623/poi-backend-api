using PoiApi.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

public class Menu
{
    public int Id { get; set; }

    [Column("poi_id")]
    public int PoiId { get; set; }

    [JsonIgnore] // 🔥
    public POI Poi { get; set; } = null!;

    public string Name { get; set; } = null!;

    public List<MenuItem> MenuItems { get; set; } = new();
}
