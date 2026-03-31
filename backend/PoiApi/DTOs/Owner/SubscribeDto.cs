namespace PoiApi.DTOs.Owner
{
    public class SubscribeDto
    {
        public int PackageId { get; set; }
        /// <summary>Monthly | Yearly</summary>
        public string BillingCycle { get; set; } = "Monthly";
    }
}
