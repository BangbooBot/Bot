using System.Security.Cryptography;
using Bangboo.Models;
using Bangboo.Server.DTOs;
using Bangboo.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NetCord.JsonModels;

namespace Bangboo.Server.Controllers;

[ApiController]
[Route("/auth")]
public class AuthController : ControllerBase
{
    private readonly DatabaseService _databaseService;
    private readonly AuthService _authService;

    public AuthController(DatabaseService databaseService, AuthService authService)
    {
        _databaseService = databaseService;
        _authService = authService;
    }

    /// <summary>
    /// Login user
    /// </summary>
    /// <remarks>
    /// Get user info from discord
    /// </remarks>
    [HttpGet("authorize")]
    [ProducesResponseType(typeof(AuthorizeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAuthorize()
    {
        var res = await _authService.Authorize();
        res.EnsureSuccessStatusCode();

        var redirectUri = res.RequestMessage?.RequestUri?.ToString();
        if (redirectUri == null)
        {
            var error = new AuthErrorResponse
            {
                Code = 500,
                Error = "Failed to authorize",
                Message = "Redirect url is null"
            };

            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }

        return Ok(new AuthorizeResponse
        {
            Url = redirectUri
        });
    }

    /// <summary>
    /// Login user
    /// </summary>
    /// <remarks>
    /// Get user info from discord
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostLogin([FromBody] LoginBody body)
    {
        var tokenExchangeRes = await _authService.TokenExchange(body.Code);
        //tokenExchangeRes.EnsureSuccessStatusCode();

        var tokenExchangeData = await tokenExchangeRes.Content.ReadFromJsonAsync<JsonAuthToken>();
        if (tokenExchangeData == null)
        {
            var error = new AuthErrorResponse
            {
                Code = 500,
                Error = "Failed to exchange token",
                Message = "Token exchange response is null"
            };

            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }

        var expiresIn = DateOnly.FromDateTime(
            DateTime.UtcNow.AddSeconds(tokenExchangeData.ExpiresIn)
        );

        var userRes = await _authService.GetUserInfo(tokenExchangeData.AccessToken);
        var user = await userRes.Content.ReadFromJsonAsync<JsonUser>();

        if (user == null)
        {
            var error = new AuthErrorResponse
            {
                Code = 500,
                Error = "Failed to get user info",
                Message = "User info response is null"
            };

            return StatusCode(StatusCodes.Status500InternalServerError, error);
        }

        var dbCtx = _databaseService.GetDbContext();
        var userModel = await dbCtx.Users.Where(u => u.Id == user.Id).FirstOrDefaultAsync();
        if (userModel is null)
        {
            userModel = new UsersModel
            {
                Id = user.Id,
                Username = user.Username,
                Avatar = user.AvatarHash
            };

            await dbCtx.Users.AddAsync(userModel);
        }
        else
        {
            userModel.Id = user.Id;
            userModel.Username = user.Username;
            userModel.Avatar = user.AvatarHash;
        }
        
        await dbCtx.SaveChangesAsync();
        
        var authModel = await dbCtx.Auths.Where(a => a.FkUserId == user.Id).FirstOrDefaultAsync();
        if (authModel is null)
        {
            authModel = new AuthsModel
            {
                AccessToken = tokenExchangeData.AccessToken,
                RefreshToken = tokenExchangeData.RefreshToken,
                TokenType = tokenExchangeData.TokenType,
                ExpiresIn = expiresIn,
                Scope = tokenExchangeData.Scope,
                FkUserId = user.Id
            };
            
            authModel = (await dbCtx.Auths.AddAsync(authModel)).Entity;
        }
        else
        {
            authModel.AccessToken = tokenExchangeData.AccessToken;
            authModel.RefreshToken = tokenExchangeData.RefreshToken;
            authModel.TokenType = tokenExchangeData.TokenType;
            authModel.ExpiresIn = expiresIn;
            authModel.Scope = tokenExchangeData.Scope;
            authModel.FkUserId = user.Id;
        }
        
        await dbCtx.SaveChangesAsync();
        
        var headers = HttpContext.Request.Headers;
        var userAgent = headers.UserAgent.ToString();
        var language = headers.AcceptLanguage.ToString();
        
        byte[] buffer = new byte[8];
        RandomNumberGenerator.Fill(buffer);
        ulong sessionId = BitConverter.ToUInt64(buffer, 0);
        var sessionModel = await dbCtx.Sessions.Where(s => s.FkAuthId == authModel.Id)
            .FirstOrDefaultAsync(s => s.Id == sessionId);
        if (sessionModel is null)
        {
            sessionModel = new SessionsModel
            {
                Id = sessionId,
                UserAgent = userAgent,
                Language = language,
                FkAuthId = authModel.Id
            };
            sessionModel = (await dbCtx.Sessions.AddAsync(sessionModel)).Entity;
        }
        else
        {
            sessionModel.UserAgent = userAgent;
            sessionModel.Language = language;
            sessionModel.FkAuthId = authModel.Id;
        }

        await dbCtx.SaveChangesAsync();

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddSeconds(tokenExchangeData.ExpiresIn)
        };
        HttpContext.Response.Cookies.Append("session-id", sessionId.ToString(), cookieOptions);

        return Ok(new LoginResponse
        {
            Username = user.Username,
            Avatar = $"https://cdn.discordapp.com/avatars/{user.Id}/{user.AvatarHash}.png?size=64",
            ExpiresIn = expiresIn
        });
    }

    /// <summary>
    /// Logout user
    /// </summary>
    /// <remarks>
    /// Remove user session from database
    /// </remarks>
    [HttpDelete("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteLogout()
    {
        var (result, session) = await _authService.ValidateSession(HttpContext.Request);
        if (!result)
        {
            return NotFound();
        }
        
        var dbCtx = _databaseService.GetDbContext();
        var _ = await dbCtx.Sessions.Where(s => s.Id == session.Id)
            .ExecuteDeleteAsync();

        await dbCtx.SaveChangesAsync();

        HttpContext.Response.Cookies.Delete("session-id");
        return NoContent();
    }
}