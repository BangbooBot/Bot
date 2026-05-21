using NetCord.Services.ApplicationCommands;

namespace Bangboo.Discord.Commands;

public class PingModule : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("ping", "Responde with pong!")]
    public String Ping()
    {
        return $"🏓 pong!";
    }
}