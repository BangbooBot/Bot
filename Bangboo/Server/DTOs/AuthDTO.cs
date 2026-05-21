using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Bangboo.Server.DTOs;

public class AuthErrorResponse
{
    public int Code { get; set; }
    
    public String Error { get; set; }
    
    public String Message { get; set; }
}

public class AuthorizeResponse
{
    public String Url { get; set; }
}

public class LoginHeaders
{
    [Required]
    public String UserAgent { get; set; }
    [Required]
    public String Platform { get; set; }
    [Required]
    public String Language { get; set; }
}

public class LoginBody
{
    [Required]
    public String Code { get; set; }
}

public class JsonAuthToken
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; }

    [JsonPropertyName("expires_in")]
    public uint ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; }
}

public class LoginResponse
{
    public required String Username { get; set; }
    public String? Avatar { get; set; }
    public required DateOnly ExpiresIn { get; set; }
}