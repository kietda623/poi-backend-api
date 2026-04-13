using System.ComponentModel.DataAnnotations;

namespace PoiApi.DTOs.App;

/// <summary>Request to generate AI Tour Plan from liked shops</summary>
public class AiTourPlanRequestDto
{
    /// <summary>List of ShopIds the user swiped right (liked) in Tinder</summary>
    [Required]
    public List<int> LikedShopIds { get; set; } = new();

    /// <summary>Optional extra preferences (e.g. "Tôi thích đồ cay", "Budget 200K")</summary>
    public string? Preferences { get; set; }
}

/// <summary>Request to chat with the "Tho Dia" chatbot</summary>
public class AiChatbotRequestDto
{
    [Required]
    public string Message { get; set; } = string.Empty;

    /// <summary>Previous conversation history for multi-turn context</summary>
    public List<ChatMessageDto>? History { get; set; }
}

public class ChatMessageDto
{
    /// <summary>"user" or "model"</summary>
    public string Role { get; set; } = "user";
    public string Message { get; set; } = string.Empty;
}

/// <summary>Request to swipe a shop card (Tinder feature)</summary>
public class TinderSwipeRequestDto
{
    [Required]
    public int ShopId { get; set; }

    /// <summary>true = like (swipe right), false = dislike (swipe left)</summary>
    public bool IsLiked { get; set; }
}
