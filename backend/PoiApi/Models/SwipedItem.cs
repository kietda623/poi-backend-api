using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PoiApi.Models
{
    /// <summary>Tinder-style swipe history: tracks which shops a user liked or disliked</summary>
    public class SwipedItem
    {
        [Key]
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string? DeviceId { get; set; }

        [Required]
        public int ShopId { get; set; }

        /// <summary>true = swipe right (liked), false = swipe left (disliked)</summary>
        public bool IsLiked { get; set; }

        public DateTime SwipedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [ForeignKey("ShopId")]
        public Shop Shop { get; set; } = null!;
    }
}
