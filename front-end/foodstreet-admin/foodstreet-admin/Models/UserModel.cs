namespace foodstreet_admin.Models;

public class UserModel
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Role { get; set; } = "Seller"; // Seller | Admin
    public string Status { get; set; } = "Active"; // Active | Disabled | Pending
    public string AvatarUrl { get; set; } = "";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class LoginRequest
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public bool RememberMe { get; set; }
}

public class RegisterRequest
{
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Password { get; set; } = "";
    public string ConfirmPassword { get; set; } = "";
    public string Role { get; set; } = "OWNER";
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string Token { get; set; } = "";
    public UserModel? User { get; set; }
}

/// <summary>Response từ POST /api/auth/login của back-end</summary>
public class LoginResponse
{
    public string Token { get; set; } = "";
    public string Role { get; set; } = "";
    public int UserId { get; set; }
}
