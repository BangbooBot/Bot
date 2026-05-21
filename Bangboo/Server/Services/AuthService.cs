using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Bangboo.Data;
using Bangboo.Models;
using Bangboo.Modules.Services;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Bangboo.Server.Services;

public class AuthService : ServerServicesModule
{
    public AuthService(IHost host, AppDbContext dbContext, IOptions<Env> options) : base(host, dbContext, options)
    {
    }

    public async Task<(bool result, SessionsModel? sessionModel)> ValidateSession(HttpRequest request)
    {
        var sessionId = 0UL;
        if (!ulong.TryParse(request.Cookies["session-id"] ?? "0", out ulong result))
        {
            return (false, null);
        }
        sessionId = result;
        
        var session = await _dbContext.Sessions.Where(s => s.Id == sessionId).FirstOrDefaultAsync();
        if (session is null) return (false, null);
        
        //var now = new DateOnly(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
        //if (session.ExpiresIn <= now) return (false, null);
        
        var headers = request.Headers;
        var userAgent = headers.UserAgent.ToString();
        var language = headers.AcceptLanguage.ToString();
        if (session.UserAgent != userAgent || session.Language != language) return (false, null);
        
        return (true, session);
    }

    public Task<HttpResponseMessage> Authorize()
    {
        var clientId = _env.ClientId;
        var redirectUri = _env.RedirectUrl;

        var query = new Dictionary<string, string?>
        {
            ["client_id"] = $"{clientId}",
            ["response_type"] = "code",
            ["redirect_uri"] = redirectUri,
            ["scope"] = "identify guilds email"
        };
        
        var uri = QueryHelpers.AddQueryString(
            $"{_env.DiscordApiUrl}/oauth2/authorize",
            query
        );

        var client = new HttpClient();

        return client.GetAsync(uri);
    }

    public Task<HttpResponseMessage> TokenExchange(String code)
    {
        var clientId = _env.ClientId;
        var clientSecret = _env.ClientSecret;
        var redirectUri = _env.RedirectUrl;

        var body = new Dictionary<string, string>
        {
            ["client_id"] = $"{clientId}",
            ["client_secret"] = clientSecret,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = redirectUri,
            ["Content-Type"] = "application/x-www-form-urlencoded"
        };

        var content = new FormUrlEncodedContent(body);

        var client = new HttpClient();

        return client.PostAsync(
            $"{_env.DiscordApiUrl}/oauth2/token",
            content
        );
    }

    public Task<HttpResponseMessage> RefreshToken(String refreshToken)
    {
        var clientId = _env.ClientId;
        var clientSecret = _env.ClientSecret;
        var redirectUri = _env.RedirectUrl;

        var body = new Dictionary<string, string>
        {
            ["client_id"] = $"{clientId}",
            ["client_secret"] = clientSecret,
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = refreshToken,
            ["redirect_uri"] = redirectUri
        };
        
        var content = new FormUrlEncodedContent(body);

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Content-Type", "application/x-www-form-urlencoded");

        return client.PostAsync(
            $"{_env.DiscordApiUrl}/oauth2/token",
            content
        );
    }
    
    public Task<HttpResponseMessage> GetUserInfo(String token)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return client.GetAsync(
            $"{_env.DiscordApiUrl}/users/@me"
        );
    }
}