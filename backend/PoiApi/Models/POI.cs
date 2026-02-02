using System.Collections.Generic;

namespace PoiApi.Models
{
    public class POI
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public string? Location { get; set; }

        public ICollection<POITranslation> Translations { get; set; }
            = new List<POITranslation>();
        public ICollection<Menu> Menus { get; set; }
        = new List<Menu>();
    }
}
