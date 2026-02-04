namespace PoiApi.DTOs.Admin.Requests
{
    public class CreateOwnerDto
    {
        public string Email { get; set; } = String.Empty;
        public string Password { get; set; } = String.Empty;
        public string ShopName { get; set; } = String.Empty;
    }
}
