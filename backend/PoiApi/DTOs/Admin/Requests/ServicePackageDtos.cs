namespace PoiApi.DTOs.Admin.Requests
{
    public class CreateServicePackageDto
    {
        public string Name { get; set; } = string.Empty;
        public string Tier { get; set; } = "Basic";
        public decimal MonthlyPrice { get; set; }
        public decimal YearlyPrice { get; set; }
        public string Description { get; set; } = string.Empty;
        /// <summary>Danh sách tính năng, phân cách bằng |</summary>
        public string Features { get; set; } = string.Empty;
        public int MaxStores { get; set; } = 1;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateServicePackageDto : CreateServicePackageDto { }
}
