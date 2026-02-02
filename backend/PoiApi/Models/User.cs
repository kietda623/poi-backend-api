using System.Data;

namespace PoiApi.Models
{
    public class User
    {
        public int Id { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }

        public int? PoiId { get; set; }
        public POI? Poi { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }


    }
}
    