namespace PoiApi.DTOs.Admin.Requests
{
    public class CreateServicePackageDto
    {
        public string Name { get; set; } = string.Empty;
        public string Tier { get; set; } = "Basic";
        public string Audience { get; set; } = PoiApi.Models.RoleConstants.Owner;
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Features { get; set; } = string.Empty;
        public int MaxStores { get; set; } = 1;
        public bool AllowAudioAccess { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateServicePackageDto : CreateServicePackageDto { }
}
