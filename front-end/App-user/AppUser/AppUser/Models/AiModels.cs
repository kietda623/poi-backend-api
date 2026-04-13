namespace AppUser.Models;

public class AiTourPlanRequestDto
{
    public List<int>? LikedShopIds { get; set; }
    public string? Preferences { get; set; }
}

public class AiTourPlanResponseDto
{
    public bool Success { get; set; }
    public string? TourPlan { get; set; }
    public int LikedShopCount { get; set; }
    public int SuggestedShopCount { get; set; }
}

public class AiChatbotRequestDto
{
    public string Message { get; set; } = string.Empty;
    public List<AiChatTurnDto>? History { get; set; }
}

public class AiChatTurnDto
{
    public string Role { get; set; } = "user"; // "user" or "model"
    public string Message { get; set; } = string.Empty;
}

public class AiChatResponseDto
{
    public bool Success { get; set; }
    public string? Reply { get; set; }
}

public class AiSubscriptionInfoDto
{
    public bool HasSubscription { get; set; }
    public string? Tier { get; set; }
    public string? PackageName { get; set; }
    public DateTime? EndDate { get; set; }
    public bool AllowAudio { get; set; }
    public bool AllowTinder { get; set; }
    public bool AllowAiPlan { get; set; }
    public bool AllowChatbot { get; set; }
}

public class TinderCardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int ListenCount { get; set; }
    public List<TinderMenuItemDto> TopItems { get; set; } = new();
}

public class TinderMenuItemDto
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
}

public class TinderCardsResponseDto
{
    public bool Success { get; set; }
    public List<TinderCardDto> Cards { get; set; } = new();
    public int RemainingCount { get; set; }
}

public class LikedShopsResponseDto
{
    public bool Success { get; set; }
    public int Count { get; set; }
    public List<LikedShopDto> Shops { get; set; } = new();
}

public class LikedShopDto
{
    public int ShopId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public DateTime SwipedAt { get; set; }
}

public class TourPlanGeneratedMessage(string result)
{
    public string Result { get; } = result;
}
