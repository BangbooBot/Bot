using Bangboo.Data;
using Bangboo.Modules.Services;
using Microsoft.Extensions.Options;
using NetCord;
using SkiaSharp;

namespace Bangboo.Discord.Services;

public enum EMemberEvent
{
    Join,
    Leave,
    Mod
}

public class GlobalService : DiscordServiceModule
{
    private readonly SKBitmap _dafaultAvatar;
    private readonly SKBitmap _newBackground;
    private readonly SKBitmap _backBackground;
    private readonly SKBitmap _modBackground;
    private readonly SKBitmap _leftBackground;
    private readonly SKFont _fredokaFont;
    private readonly SKFont _robotoFont;

    public GlobalService(AppDbContext dbContext, IOptions<Env> options) : base(dbContext, options)
    {
        _dafaultAvatar = SKBitmap.Decode("./Assets/member/default-avatar.png");
        _newBackground = SKBitmap.Decode("./Assets/cards/card-new.png");
        _backBackground = SKBitmap.Decode("./Assets/cards/card-back.png");
        _modBackground = SKBitmap.Decode("./Assets/cards/card-mod.png");
        _leftBackground = SKBitmap.Decode("./Assets/cards/card-left.png");
        _fredokaFont = new SKFont(SKTypeface.FromStream(File.OpenRead("./Assets/fonts/Fredoka-Medium.ttf")), 200);
        _robotoFont = new SKFont(SKTypeface.FromStream(File.OpenRead("./Assets/fonts/Roboto-Medium.ttf")), 96);
    }

    public async Task<SKData> GlobalMessage(User user, EMemberEvent eventType = EMemberEvent.Join)
    {
        var avatarUrl = user.GetAvatarUrl(ImageFormat.Png);
        SKBitmap avatar;
        if (avatarUrl is not null)
            try
            {
                var client = new HttpClient();
                var url = avatarUrl.ToString();
                var uri = new Uri($"{url}?size=512");
                var stream = await client.GetStreamAsync(uri);
                var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;
                avatar = SKBitmap.Decode(memoryStream);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Failed to get avatar: {e.Message}");
                avatar = _dafaultAvatar;
            }
        else
            avatar = _dafaultAvatar;

        var imageInfo = new SKImageInfo(400, 400);
        var sampling = new SKSamplingOptions(SKCubicResampler.Mitchell);
        avatar = avatar.Resize(imageInfo, sampling);

        var bitmap = new SKBitmap(2800, 560);
        var canvas = new SKCanvas(bitmap);

        switch (eventType)
        {
            case EMemberEvent.Join:
                canvas.DrawBitmap(_newBackground, 0, 0);
                break;
            case EMemberEvent.Leave:
                canvas.DrawBitmap(_leftBackground, 0, 0);
                break;
            case EMemberEvent.Mod:
                canvas.DrawBitmap(_modBackground, 0, 0);
                break;
            default:
                canvas.DrawBitmap(_modBackground, 0, 0);
                break;
        }

        canvas.Save();

// Draw avatar
        float x = 0;
        float y = 160;
        float tamanho = 400;

        var raio = tamanho / 2f;
        var centroX = x + raio;
        var centroY = y + raio;

        var pathCirculo = new SKPath();
        pathCirculo.AddCircle(centroX, centroY, raio);
        canvas.ClipPath(pathCirculo, antialias: true);

        var rectDestino = new SKRect(x, y, x + tamanho, y + tamanho);
        var rectOrigem = new SKRect(0, 0, avatar.Width, avatar.Height);

        var paintImagem = new SKPaint { IsAntialias = true };
        canvas.DrawBitmap(avatar, rectOrigem, rectDestino, paintImagem);

        canvas.Restore();

        var globalName = user.GlobalName ?? user.Username;
        var username = user.Username;

        var textPaint = new SKPaint { IsAntialias = true, Color = SKColors.White };

        var namePoint = new SKPoint { X = 530, Y = 297 };
        var usernamePoint = new SKPoint { X = 530, Y = 452 };

// Draw global name
        canvas.DrawText(username, namePoint, _fredokaFont, textPaint);

// Draw username
        canvas.DrawText($"@{globalName}", usernamePoint, _robotoFont, textPaint);


        var image = SKImage.FromBitmap(bitmap);
        var data = image.Encode(SKEncodedImageFormat.Png, 90);

        return data;
    }
}