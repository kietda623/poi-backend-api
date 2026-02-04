namespace PoiApi.DTOs.Admin.Requests
{
    public class CreateOwnerDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ShopName { get; set; } = string.Empty;
    }
}
