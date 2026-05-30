using Bangboo.Discord.Services;
using Microsoft.EntityFrameworkCore;
using NetCord;
using NetCord.Gateway;
using NetCord.Hosting.Gateway;
using NetCord.Rest;

namespace Bangboo.Discord.Events;

public class GuildMemberRemove : IGuildUserRemoveGatewayHandler
{
    private readonly RestClient _client;
    private readonly GlobalService _globalService;
    
    public GuildMemberRemove(RestClient client, DatabaseService databaseService, IServiceScopeFactory scopeFactory)
    {
        var scope = scopeFactory.CreateScope();
        _client = client;
        _globalService = scope.ServiceProvider.GetRequiredService<GlobalService>();
    }


    public async ValueTask HandleAsync(GuildUserRemoveEventArgs arg)
    {
        if (arg.User.IsBot) return;
        
        var guildBan = await _client.GetGuildBanAsync(arg.GuildId, arg.User.Id);
        if (guildBan is null) return;
        
        var dbCtx = _globalService.dbContext;
        var memberEvents = await dbCtx.MemberEvents.Where(e => e.FkGuildId == arg.GuildId).FirstOrDefaultAsync();
        if (memberEvents is null) return;
        if (!memberEvents.OnJoin) return;
        if (!memberEvents.SystemChannelId.HasValue) return;

        var channel = (TextChannel)await _client.GetChannelAsync(memberEvents.SystemChannelId.Value);
        var card = await _globalService.GlobalMessage(arg.User, EMemberEvent.Leave);
        var stream = card.AsStream();
        var propriedadesMensagem = new MessageProperties()
        {
            Attachments = new[] { new AttachmentProperties("card.png", stream) }
        };
        await channel.SendMessageAsync(propriedadesMensagem);
    }
}