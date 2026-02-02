using PoiApi.Models;
using System.Text.Json.Serialization;

public class Menu
{
    public int Id { get; set; }

    public int PoiId { get; set; }

    [JsonIgnore] // 🔥
    public POI Poi { get; set; } = null!;

    public string Name { get; set; } = null!;

    public List<MenuItem> MenuItems { get; set; } = new();
}
