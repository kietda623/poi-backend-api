namespace foodstreet_admin.Services;

/// <summary>
/// Lưu JWT token trong memory (scoped per Blazor Server circuit).
/// </summary>
public class TokenService
{
    private string? _token;

    public string? Token => _token;

    public void SetToken(string token) => _token = token;

    public void ClearToken() => _token = null;

    public bool HasToken => !string.IsNullOrEmpty(_token);
}
